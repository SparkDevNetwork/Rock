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

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class CommunicationSkill
{
    #region Tool(s)

    [Description( "Persist a previously composed communication draft (email/SMS/push) as a transient Communication that the user can approve and send." )]
    [AgentPurpose( "Saves the composed subject/body content as a new transient Communication for the specified recipient, or updates an existing transient Communication when existingDraftIdKey is provided. After this call succeeds, the user is asked to verify the draft before SendCommunication may be called." )]
    [AgentUsage( "The recipient is always provided by IdKey only. Never ask the user for email addresses or phone numbers." )]
    [AgentUsage( "SMS communications do not use a subject line; leave draftedSubject empty for SMS." )]
    [AgentUsage( "After this tool succeeds, ask the user to verify the draft (using the returned verification text) before calling SendCommunication." )]
    [AgentToolPrerequisite( "Call GetDraftCommunicationInstructions first to obtain the medium-specific drafting instructions (recipient, From Name, email closing phrase, tone requirements, and length limits). Compose the subject/body using those instructions and pass them here as draftedSubject/draftedBody. Skipping GetDraftCommunicationInstructions risks producing a draft that omits the sender's name or violates medium-specific formatting rules." )]
    [AgentToolPrerequisite( "If a corresponding transient draft already exists, pass existingDraftIdKey to update it instead of creating a new draft." )]
    [AgentToolGuid( "9986AC39-7362-4909-BD58-7BB93235E1A2" )]
    public AgentToolResult AddOrUpdateCommunicationDraft(
                [Description( "The IdKey of the person to whom the communication will be sent." )]
                string recipientIdKey,

                AgentCommunicationType communicationType,

                [Description( "The composed subject line (email) or notification title (push). This must be the exact text that will be persisted. Leave empty for SMS." )]
                string draftedSubject,

                [Description( "The composed body of the communication. This must be the exact text that will be persisted." )]
                string draftedBody,

                [Description( "Only relevant to SMS. If omitted, the person's default sms phone number will be used." )]
                string fromNumberIdKey = "",

                [Description( "An optional parameter to update an existing transient draft instead of saving a new one." )]
                string existingDraftIdKey = "" )
    {
        var currentPerson = AgentRequestContext.CurrentPerson;
        if ( currentPerson == null )
        {
            return Error( "The current person is not available. Ensure the agent is properly initialized." );
        }

        if ( draftedBody.IsNullOrWhiteSpace() )
        {
            return Error( "draftedBody is required. Compose the body of the communication and pass it in the draftedBody parameter." )
                .WithInstructions( "If you have not yet requested drafting guidance, call GetDraftCommunicationInstructions first so the draft honors the current person's From Name, closing phrase, and tone requirements." );
        }

        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
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

        var emailSignature = string.Empty;
        if ( communicationType == AgentCommunicationType.Email )
        {
            var prefs = PersonPreferenceCache.GetPersonPreferenceCollection( currentPerson );
            emailSignature = prefs.GetValue( PersonPreferenceKey.EMAIL_CLOSING_PHRASE );
        }

        // A DraftRequest is still used to carry the current person / recipients
        // / email signature to the medium so BuildCommunication behaves the
        // same way it did when the tool authored the content internally. The
        // hint / guidance / relevant-data fields are only used for authoring
        // (phase 1) so we do not require them here.
        var draftRequest = new DraftRequest(
            communicationType,
            subjectHint: string.Empty,
            draftGuidance: string.Empty,
            relevantData: string.Empty,
            tone: string.Empty,
            currentPerson,
            recipients,
            emailSignature );

        var draftResult = new DraftResult
        {
            Type = communicationType,
            Subject = draftedSubject,
            Body = draftedBody,
            VerificationText = medium.GetVerificationText( currentPerson, recipients )
        };

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

        // Update the result with the newly saved communication so downstream
        // tool calls (SendCommunication / CancelDraft) can reference it.
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
            .WithReferenceRoute( AgentRequestContext, "Draft Communication", $"/Communication/{draftCommunication.IdKey}", false );
    }

    #endregion
}
