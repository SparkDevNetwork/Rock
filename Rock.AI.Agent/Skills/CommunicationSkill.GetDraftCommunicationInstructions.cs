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

using System.Collections.Generic;
using System.ComponentModel;

using Rock.AI.Agent.Annotations;
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

    [Description( "Request drafting instructions for a communication (email/SMS/push) that will be sent to a specified recipient." )]
    [AgentPurpose( "Returns the medium-specific drafting instructions (recipient details, From Name, closing phrase, tone requirements, length limits) that must be followed when composing a communication draft. This tool does NOT save the draft — after you compose the content, call AddOrUpdateCommunicationDraft to persist it." )]
    [AgentUsage( "Always call this tool first when drafting a new email, SMS, or push notification. The instructions it returns include the current person's From Name and email closing phrase preference, both of which the model does not otherwise have access to and cannot fabricate correctly." )]
    [AgentUsage( "The recipient is always provided by IdKey only. Never ask the user for email addresses or phone numbers." )]
    [AgentUsage( "After receiving the drafting instructions, compose the subject/body yourself and then call AddOrUpdateCommunicationDraft (do NOT call SendCommunication) with the composed content. The Save tool is what actually persists the draft as a transient Communication for the user to approve." )]
    [AgentToolGuid( "4EEF6200-AA05-4F26-AB4D-19C73DEB3BDD" )]
    public AgentToolResult GetDraftCommunicationInstructions(
                [Description( "The IdKey of the person to whom the communication will be sent. Used to fetch the contact information for the person." )]
                string recipientIdKey,

                AgentCommunicationType communicationType,

                [Description( "A short hint describing what the subject line should convey. Used to shape the drafting instructions. Ignored on SMS." )]
                string subjectHint = "",

                [Description( "Background data that should inform the draft (facts about the recipient, event details, etc.)." )]
                string referenceData = "",

                [Description( "Specific guidance for how the draft should be worded (goals, must-include phrases, things to avoid, etc.)." )]
                string draftGuidance = "",

                [Description( "The tone the draft should convey (e.g. warm, formal, urgent). Defaults to 'warm'." )]
                string tone = "warm",

                [Description( "Only relevant to SMS. If omitted, the person's default sms phone number will be used." )]
                string fromNumberIdKey = "",

                [Description( "An optional parameter. Provide the IdKey of an existing transient draft that you intend to update in the follow-up AddOrUpdateCommunicationDraft call so the drafting instructions reflect that intent." )]
                string existingDraftIdKey = "" )
    {
        var currentPerson = AgentRequestContext.CurrentPerson;
        if ( currentPerson == null )
        {
            return Error( "The current person is not available. Ensure the agent is properly initialized." );
        }

        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

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

        // If the caller referenced an existing draft, make sure it is still
        // in a state we can update so the follow-up save call will succeed.
        if ( existingDraftIdKey.IsNotNullOrWhiteSpace() )
        {
            var existingDraft = helper.GetRequiredEntity<Model.Communication>( existingDraftIdKey );

            if ( existingDraft != null && existingDraft.Status != CommunicationStatus.Transient )
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

        var draftRequest = new DraftRequest( communicationType, subjectHint, draftGuidance, referenceData, tone, currentPerson, recipients, emailSignature );

        var draftingInstructions = medium.BuildDraftingInstructions( draftRequest );

        // Build a concrete example of the follow-up AddOrUpdateCommunicationDraft
        // call so the LLM sees the exact parameter shape it needs to produce and
        // any pass-through values (from-number, existing-draft) are pre-filled.
        var subjectExampleValue = communicationType == AgentCommunicationType.Sms
            ? ""
            : "<the subject line you composed>";

        var callbackParameters = new List<string>
        {
            $"  recipientIdKey: \"{recipientIdKey}\"",
            $"  communicationType: \"{communicationType}\"",
            $"  draftedSubject: \"{subjectExampleValue}\"",
            "  draftedBody: \"<the body you composed>\""
        };

        if ( communicationType == AgentCommunicationType.Sms && fromNumberIdKey.IsNotNullOrWhiteSpace() )
        {
            callbackParameters.Add( $"  fromNumberIdKey: \"{fromNumberIdKey}\"" );
        }

        if ( existingDraftIdKey.IsNotNullOrWhiteSpace() )
        {
            callbackParameters.Add( $"  existingDraftIdKey: \"{existingDraftIdKey}\"" );
        }

        var callbackContract =
            "AFTER COMPOSING THE DRAFT: call AddOrUpdateCommunicationDraft to persist it. Do NOT call SendCommunication until the user has explicitly approved the saved draft. The Save call should look like:\r\n\r\n"
            + "AddOrUpdateCommunicationDraft(\r\n"
            + string.Join( ",\r\n", callbackParameters )
            + "\r\n)";

        // No Communication is written here, so no history content is emitted.
        return Success()
            .WithInstructions( callbackContract )
            .WithInstructions( draftingInstructions )
            .WithoutHistoryContent();
    }

    #endregion
}
