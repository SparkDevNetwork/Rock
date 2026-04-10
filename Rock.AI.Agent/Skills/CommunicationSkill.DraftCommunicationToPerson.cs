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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.AI.Agent.Utilities.CommunicationSkill.Mediums;
using Rock.Communication;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class CommunicationSkill
{
    #region Tool(s)

    [Description( "Draft a communication (email/SMS/push) for a specified recipient." )]
    [AgentPurpose( "Creates a new draft (email/SMS/push) for the specified recipient, or updates an existing draft if one is provided. Drafts are saved as communications and can later be sent." )]
    [AgentUsage( "The recipient is always provided by IdKey only. Never ask the user for email addresses or phone numbers." )]
    [AgentUsage( "The function automatically resolves the recipient's actual contact details from the IdKey." )]
    [AgentToolPrerequisite( "If a corresponding draft already exists and has not been sent, pass existingDraftIdKey to update it instead of creating a new draft." )]
    [AgentToolGuid( "4EEF6200-AA05-4F26-AB4D-19C73DEB3BDD" )]
    public async Task<AgentToolResult> DraftCommunicationToPerson(
                [Description("The IdKey of the person to whom the communication will be sent. Used to fetch the contact information for the person.")]
                string recipientIdKey,

                AgentCommunicationType communicationType,
                string subjectHint,

                [Description("The data corresponding to the draft being written. Not the draft itself.")]
                string referenceData,
                string draftGuidance,
                string tone = "warm",


                [Description("Only relevant to SMS. If omitted, the person's default sms phone number will be used.")]
                string fromNumberIdKey = "",

                [Description("An optional parameter to update an existing draft as opposed to saving a new one.")]
                string existingDraftIdKey = "" )
    {
        var currentPerson = AgentRequestContext.CurrentPerson;
        if ( currentPerson == null )
        {
            return Error( "The current person is not available. Ensure the agent is properly initialized." );
        }

        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var personService = new PersonService( rockContext );
        var communicationService = new CommunicationService( rockContext );
        Model.Communication draftCommunication = null;

        var recipient = helper.GetRequiredEntity<Model.Person>( recipientIdKey );
        var fromNumberId = GetFromNumberId( helper, communicationType, fromNumberIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var medium = GetCommunicationMedium( communicationType, rockContext, fromNumberId );

        if ( medium == null )
        {
            helper.AddError( $"The communication type '{communicationType}' is not supported." );
        }

        if ( existingDraftIdKey.IsNotNullOrWhiteSpace() )
        {
            draftCommunication = helper.GetRequiredEntity<Model.Communication>( existingDraftIdKey );

            if ( draftCommunication != null && draftCommunication.Status != CommunicationStatus.Transient )
            {
                helper.AddError( "This draft is not in a transient state. It has likely already been sent." );
                helper.AddInstructions( "Ask the user if they would prefer you create a new draft." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var recipients = new List<Rock.Model.Person> { recipient };
        var recipientValidation = medium.ValidateRecipients( recipients );
        if ( recipientValidation.Count > 0 )
        {
            return Error( recipientValidation );
        }

        string emailSignature = string.Empty;
        if ( communicationType == AgentCommunicationType.Email )
        {
            var prefs = PersonPreferenceCache.GetPersonPreferenceCollection( currentPerson );
            emailSignature = prefs.GetValue( PersonPreferenceKey.EMAIL_CLOSING_PHRASE );
        }

        var draftRequest = new DraftRequest( communicationType, subjectHint, draftGuidance, referenceData, tone, currentPerson, recipients, emailSignature );

        DraftResult draftResult;
        try
        {
            draftResult = await medium.DraftAsync( AgentRequestContext.ChatAgent, draftRequest );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Failed to draft communication." );
            return Error( "Failed to draft the communication. Check the logs for details." );
        }

        if ( draftResult == null )
        {
            return Error( "The draft content is null. Ensure the medium's DraftAsync method is implemented correctly." );
        }

        if ( draftCommunication != null )
        {
            draftCommunication = medium.UpdateCommunication( draftRequest, recipients, draftCommunication, draftResult );
        }
        else
        {
            draftCommunication = medium.BuildCommunication( draftRequest, recipients, draftResult );
            if ( draftCommunication == null )
            {
                return Error( "Failed to build the communication object." );
            }

            communicationService.Add( draftCommunication );
        }

        try
        {
            rockContext.SaveChanges();
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Failed to save communication." );
            return Error( "Failed to save the communication. Check the logs for details." );
        }

        // Update our draft result with the newly saved communication.
        draftResult.CommunicationIdKey = draftCommunication.IdKey;

        var returnInstructions = "Never call SendCommunication directly after this.";

        if ( draftResult.VerificationText.IsNotNullOrWhiteSpace() )
        {
            returnInstructions += "\r\nAsk the user for verification on the following fields: \r\n";
            returnInstructions += draftResult.VerificationText;
        }

        var historyContent = new
        {
            Recipient = new KeyNameResult( recipient.IdKey, recipient.FullName ),
            CommunicationIdKey = draftCommunication.IdKey
        };

        return Success( draftResult )
            .WithInstructions( returnInstructions )
            .WithHistoryContent( historyContent, draftCommunication.IdKey )
            .WithReferenceRoute( AgentRequestContext, "Draft Communication", $"/Communication/{draftCommunication.Id}", false );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Returns the specified medium based on the communication type.
    /// </summary>
    /// <param name="communicationType"></param>
    /// <returns></returns>
    private IAgentCommunicationMedium GetCommunicationMedium( AgentCommunicationType communicationType, RockContext rockContext, int? fromNumberId = null )
    {
        IAgentCommunicationMedium medium;

        if ( communicationType == AgentCommunicationType.Email )
        {
            if ( !MediumContainer.HasActiveEmailTransport() )
            {
                return null;
            }

            medium = new EmailMedium();
        }
        else if ( communicationType == AgentCommunicationType.Sms )
        {
            if ( !fromNumberId.HasValue )
            {
                return null;
            }

            if ( !MediumContainer.HasActiveSmsTransport() )
            {
                return null;
            }

            medium = new SmsMedium( fromNumberId.Value );
        }
        else if ( communicationType == AgentCommunicationType.Push )
        {
            if ( !MediumContainer.HasActivePushTransport() )
            {
                return null;
            }

            medium = new PushNotificationMedium( rockContext );
        }
        else
        {
            return null;
        }

        return medium;
    }

    /// <summary>
    /// Get the system phone number identifier to use when creating an SMS
    /// communication.
    /// </summary>
    /// <param name="helper">The tool helper.</param>
    /// <param name="communicationType">The type of communication being processed.</param>
    /// <param name="fromNumberIdKey">The identifier key of the system phone number that was specified.</param>
    /// <returns>The integer identifier of a system phone number or <c>null</c> if it could not be determined.</returns>
    private int? GetFromNumberId( AgentToolHelper helper, AgentCommunicationType communicationType, string fromNumberIdKey )
    {
        if ( communicationType != AgentCommunicationType.Sms )
        {
            return null;
        }

        if ( fromNumberIdKey.IsNotNullOrWhiteSpace() )
        {
            var fromNumber = SystemPhoneNumberCache.Get( fromNumberIdKey, false );

            if ( fromNumber != null && fromNumber.IsActive && fromNumber.IsSmsEnabled )
            {
                return fromNumber.Id;
            }

            helper.AddError( "The provided fromNumberIdKey does not correspond to a valid active SMS-enabled system phone number." );
        }
        else
        {
            var fromNumberId = GetDefaultSmsPhoneNumber()?.Id;

            if ( fromNumberId.HasValue )
            {
                return fromNumberId.Value;
            }

            helper.AddError( "No valid default SMS 'from' number could be determined for the current person. Please provide a fromNumberIdKey." );
            helper.AddInstructions( "Call the LookupSystemPhoneNumbers function, and prompt the user to pick from the list." );
        }

        return null;
    }

    #endregion
}
