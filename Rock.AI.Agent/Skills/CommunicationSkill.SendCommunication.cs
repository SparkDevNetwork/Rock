// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Tasks;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class CommunicationSkill
{
    #region Tool(s)

    [Description( "Sends a previously drafted communication." )]
    [AgentToolGuid( "2BB35960-77C6-4EAD-9645-F0ACB0EF132B" )]
    public AgentToolResult SendCommunication( string communicationIdKey )
    {
        var currentPerson = AgentRequestContext.CurrentPerson;

        if ( currentPerson == null )
        {
            return Error( "The current person is not available. Ensure the agent is properly initialized." )
                .WithInstructions( "Make sure the agent has access to the current person context." );
        }

        if ( communicationIdKey.IsNullOrWhiteSpace() )
        {
            return Error( "A communicationIdKey is required to send a communication." )
                .WithInstructions( "Ask the user if they would like to draft one." );
        }

        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var communicationService = new CommunicationService( rockContext );
        var communication = helper.GetRequiredEntity<Model.Communication>( communicationIdKey );

        if ( communication != null && communication.Status != CommunicationStatus.Transient )
        {
            helper.AddError( "The communication is not in a transient state and cannot be sent." );
            helper.AddInstructions( "Ensure the communication is in a transient state before sending." );
        }
        else if ( communication != null && !IsAuthorizedToEditDraft( communication, currentPerson ) )
        {
            helper.AddError( "You are not authorized to send this communication." );
        }
        else if ( communication != null && HasMultipleRecipients( communication, rockContext ) )
        {
            helper.AddError( "This communication has more than one recipient and cannot be sent by the agent." );
            helper.AddInstructions( "Tell the user to send it from the communication wizard, where it can go through approval, or offer to create a new draft for one person." );
        }
        else if ( communication != null && !IsSmsFromNumberAuthorized( communication, currentPerson ) )
        {
            helper.AddError( "You are not authorized to send from this communication's SMS 'from' number." );
            helper.AddInstructions( "Ask the user to draft the message again from one of their available SMS 'from' numbers." );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        communication.Status = CommunicationStatus.Approved;
        communication.ReviewedDateTime = RockDateTime.Now;
        communication.ReviewerPersonAliasId = currentPerson.PrimaryAliasId;

        try
        {
            rockContext.SaveChanges();
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Failed to update communication status." );
            return Error( "Failed to update the communication status. Check the logs for details." );
        }

        SendCommunication( communication.Id );

        var result = new SendCommunicationResult
        {
            CommunicationIdKey = communicationIdKey,
        };

        var toolResult = Success( result )
            .WithInstructions( "The communication has been queued to be sent. The user can view the details of the communication via the reference url." )
            .WithHistoryKey( communicationIdKey )
            .WithReferenceRoute( AgentRequestContext, "Communication", $"/Communication/{communication.Id}", false );

        // If the communication is SMS and came from a different number than the user's default, prompt the user
        // to see if we should update their default.
        if ( communication.CommunicationType == CommunicationType.SMS )
        {
            // Get the from number of the communication.
            var fromNumberId = communication.SmsFromSystemPhoneNumberId;
            var userDefaultFromNumberId = GetDefaultSmsPhoneNumber()?.Id;

            if ( !userDefaultFromNumberId.HasValue || fromNumberId != userDefaultFromNumberId.Value )
            {
                toolResult.WithInstructions( "Ask the user if they would like to use this number as their default for future messages." );
            }
        }

        return toolResult;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sends a communication.
    /// </summary>
    /// <param name="communicationId"></param>
    private void SendCommunication( int communicationId )
    {
        var transactionMsg = new ProcessSendCommunication.Message()
        {
            CommunicationId = communicationId
        };
        transactionMsg.Send();
    }

    /// <summary>
    /// Determines if the communication would be sent to more than one person.
    /// </summary>
    /// <param name="communication">The communication that will be sent.</param>
    /// <param name="rockContext">The context used to read the recipients.</param>
    /// <returns><c>true</c> if the communication has a communication list, more than one recipient or a recipient without a person; otherwise <c>false</c>.</returns>
    private static bool HasMultipleRecipients( Model.Communication communication, RockContext rockContext )
    {
        /*
            10/25/2026 - MSE

            The agent's draft tool always targets one person, so a transient
            communication that reaches more people (such as one created by a
            grid's Communicate action) must go through the Communication Entry
            Wizard, which applies the Maximum Recipients approval rule the
            agent cannot read. Recipients are counted by distinct person alias
            because re-drafting adds another row for the same person. A row
            without a person alias (such as a push device row) cannot be
            counted by person, so it is refused too. A communication list
            adds its members at send time, so any communication with a list
            is refused.

            Reason: Prevent the agent from approving bulk communications.
        */
        if ( communication.ListGroupId.HasValue )
        {
            return true;
        }

        var recipientPersonAliasIds = new CommunicationRecipientService( rockContext ).Queryable()
            .Where( cr => cr.CommunicationId == communication.Id )
            .Select( cr => cr.PersonAliasId )
            .Distinct()
            .ToList();

        return recipientPersonAliasIds.Count > 1
            || recipientPersonAliasIds.Any( id => !id.HasValue );
    }

    /// <summary>
    /// Determines if the person is allowed to send the communication from its
    /// SMS 'from' number. Communications that do not send SMS, or that have no
    /// 'from' number, are always allowed.
    /// </summary>
    /// <param name="communication">The communication that will be sent.</param>
    /// <param name="person">The person that wants to send the communication.</param>
    /// <returns><c>true</c> if the person can send from the number; otherwise <c>false</c>.</returns>
    private static bool IsSmsFromNumberAuthorized( Model.Communication communication, Model.Person person )
    {
        /*
            10/25/2026 - MSE

            The draft tools only accept 'from' numbers the person can VIEW,
            but a transient communication could have been saved before that
            check existed or created another way. Check the number again at
            send time. RecipientPreference communications can also send SMS,
            so they are checked too.

            Reason: Prevent sending SMS from a system phone number the person cannot view.
        */
        if ( !communication.SmsFromSystemPhoneNumberId.HasValue )
        {
            return true;
        }

        if ( communication.CommunicationType != CommunicationType.SMS
            && communication.CommunicationType != CommunicationType.RecipientPreference )
        {
            return true;
        }

        var fromNumber = SystemPhoneNumberCache.Get( communication.SmsFromSystemPhoneNumberId.Value );

        return fromNumber != null && fromNumber.IsAuthorized( Authorization.VIEW, person );
    }

    #endregion
}
