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
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class CommunicationSkill
    {
        #region Tool(s)

        /// <summary>
        /// Drafts a communication (email/SMS/push) for a specified recipient.
        /// </summary>
        /// <param name="recipientIdKey">The idKey of the recipient.</param>
        /// <param name="communicationType">SMS, Push or Email.</param>
        /// <param name="subjectHint">The hint of the subject.</param>
        /// <param name="referenceData">The relevant data for crafting the communication.</param>
        /// <param name="draftGuidance">Guidance for when composing the draft.</param>
        /// <param name="tone">The tone of the message.</param>
        /// <param name="existingDraftIdKey">The draft to update in place.</param>
        /// <returns></returns>
        [AgentPurpose( "Creates a new draft (email/SMS/push) for the specified recipient, or updates an existing draft if one is provided. Drafts are saved as communications and can later be sent." )]
        [AgentUsage( "The recipient is always provided by IdKey only. Never ask the user for email addresses or phone numbers." )]
        [AgentUsage( "The function automatically resolves the recipient's actual contact details from the IdKey." )]
        [AgentToolPrerequisite( "If a corresponding draft already exists and has not been sent, pass existingDraftIdKey to update it instead of creating a new draft." )]
        [AgentToolGuid( "4EEF6200-AA05-4F26-AB4D-19C73DEB3BDD" )]
        public async Task<RockToolResult> DraftCommunicationToPerson(
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
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "The current person is not available. Ensure the agent is properly initialized." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var personService = new PersonService( rockContext );
                var communicationService = new CommunicationService( rockContext );

                var recipient = personService.Get( recipientIdKey, false );
                if ( recipient == null )
                {
                    return RockToolResult.Error( $"No valid recipient found for the provided recipientIdKey: {recipientIdKey}." )
                        .WithInstructions( "Verify the recipientIdKey and try again." );
                }

                int? fromNumberId = null;

                if ( communicationType == AgentCommunicationType.Sms )
                {
                    if ( fromNumberIdKey.IsNotNullOrWhiteSpace() )
                    {
                        var fromNumber = SystemPhoneNumberCache.Get( fromNumberIdKey, false );
                        if ( fromNumber == null || !fromNumber.IsActive || !fromNumber.IsSmsEnabled )
                        {
                            return RockToolResult.Error( "The provided fromNumberIdKey does not correspond to a valid active SMS-enabled system phone number." );
                        }

                        fromNumberId = fromNumber.Id;
                    }
                    else
                    {
                        fromNumberId = GetDefaultSmsPhoneNumber()?.Id;

                        if ( !fromNumberId.HasValue )
                        {
                            return RockToolResult.Error( "No valid default SMS 'from' number could be determined for the current person. Please provide a fromNumberIdKey." )
                                .WithInstructions( "Call the LookupSystemPhoneNumbers function, and prompt the user to pick from the list." );
                        }
                    }
                }

                var medium = TryGetCommunicationMedium( communicationType, rockContext, fromNumberId );
                if ( medium == null )
                {
                    return RockToolResult.Error( $"The communication type '{communicationType}' is not supported." );
                }

                Rock.Model.Communication draftCommunication = null;
                if ( existingDraftIdKey.IsNotNullOrWhiteSpace() )
                {
                    draftCommunication = communicationService.Get( existingDraftIdKey );
                    if ( draftCommunication == null )
                    {
                        return RockToolResult.Error( $"No valid draft communication found for the provided existingDraftIdKey: {existingDraftIdKey}." )
                            .WithInstructions( "Ask the user if they would like you to generate a new one." );
                    }
                    else if ( draftCommunication.Status != CommunicationStatus.Transient )
                    {
                        return RockToolResult.Error( "This draft is not in a transient state. It has likely already been sent." )
                            .WithInstructions( "Ask the user if they would prefer you create a new draft." );
                    }
                }

                var recipients = new List<Rock.Model.Person> { recipient };
                var recipientValidation = medium.ValidateRecipients( recipients );
                if ( recipientValidation.Count > 0 )
                {
                    return RockToolResult.Error( recipientValidation );
                }

                string emailSignature = string.Empty;
                if ( communicationType == AgentCommunicationType.Email )
                {
                    var prefs = AgentRequestContext.RockRequestContext.GetGlobalPersonPreferences();
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
                    return RockToolResult.Error( "Failed to draft the communication. Check the logs for details." );
                }

                if ( draftResult == null )
                {
                    return RockToolResult.Error( "The draft content is null. Ensure the medium's DraftAsync method is implemented correctly." );
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
                        return RockToolResult.Error( "Failed to build the communication object." );
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
                    return RockToolResult.Error( "Failed to save the communication. Check the logs for details." );
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

                return RockToolResult.Success( draftResult )
                    .WithInstructions( returnInstructions )
                    .WithHistoryContent( historyContent, draftCommunication.IdKey )
                    .WithReferenceRoute( AgentRequestContext.RockRequestContext, "Draft Communication", $"/Communication/{draftCommunication.Id}", false );
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns the specified medium based on the communication type.
        /// </summary>
        /// <param name="communicationType"></param>
        /// <returns></returns>
        private IAgentCommunicationMedium TryGetCommunicationMedium( AgentCommunicationType communicationType, RockContext rockContext, int? fromNumberId = null )
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

        #endregion
    }
}
