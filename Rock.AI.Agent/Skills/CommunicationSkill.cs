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
//                                                                                              
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.AI.Agent.Utilities.CommunicationSkill.Mediums;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Tasks;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Centralized skill for drafting and sending communications (email and SMS) in Rock.
    /// Provides LLM prompts for drafting messages and tool functions for sending them.
    /// </summary>
    [Description( "This skill helps author and send communications, and track their impact." )]
    [AgentSkillGuid( "37DF3637-9775-4A89-9A77-BF6744232991" )]
    [EntityTypeGuid( "F67D0B02-B59F-475F-A005-8F2A5CCCA91C" )]
    internal sealed class CommunicationSkill : AgentSkillComponent
    {
        #region Fields

        private readonly ILogger<CommunicationSkill> _logger;
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        public CommunicationSkill( IRockContextFactory rockContextFactory, ILogger<CommunicationSkill> logger )
        {
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Helpers

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

        /// <summary>
        /// Gets the system phone numbers, optionally filtering to only SMS-enabled numbers.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="smsEnabled"></param>
        /// <returns></returns>
        private List<SystemPhoneNumberResult> GetSystemPhoneNumbers( RockContext rockContext, bool? smsEnabled = null )
        {
            var spns = SystemPhoneNumberCache.All()
                .Where( spn => spn.IsActive )
                .Where( spn => !smsEnabled.HasValue || spn.IsSmsEnabled == smsEnabled.Value );

            // Filter out based on security.
            spns = spns.Where( spn => spn.IsAuthorized( Authorization.VIEW, AgentRequestContext.RockRequestContext.CurrentPerson ) ).ToList();

            var spnResults = new List<SystemPhoneNumberResult>();
            foreach ( var spn in spns )
            {
                var spnResult = new SystemPhoneNumberResult
                {
                    Id = spn.Id,
                    Name = spn.Name,
                    Description = spn.Description,
                    Number = spn.Number,
                    IsSmsEnabled = spn.IsSmsEnabled,
                };

                if ( spn.AssignedToPersonAliasId.HasValue )
                {
                    var person = new PersonAliasService( rockContext ).GetPerson( spn.AssignedToPersonAliasId.Value );

                    if ( person != null )
                    {
                        spnResult.AssignedToPerson = new PersonResult
                        {
                            FirstName = person.FirstName,
                            LastName = person.LastName,
                            Id = person.Id,
                        };
                    }
                }

                spnResults.Add( spnResult );
            }

            return spnResults;
        }

        /// <summary>
        /// Gets the current person's default SMS phone number, if any.
        /// </summary>
        /// <returns></returns>
        private SystemPhoneNumberCache GetDefaultSmsPhoneNumber()
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return null;
            }

            var prefs = AgentRequestContext.RockRequestContext.GetGlobalPersonPreferences();
            var savedId = prefs.GetValue( PersonPreferenceKey.DEFAULT_SMS_PHONE_NUMBER ).AsIntegerOrNull();

            // If a saved default exists, use it—unless it's gone or inactive, then fall back.
            if ( savedId.HasValue && savedId.Value > 0 )
            {
                var saved = SystemPhoneNumberCache.Get( savedId.Value );
                if ( saved != null && saved.IsActive && saved.IsSmsEnabled )
                {
                    return saved;
                }
            }

            // No valid saved default: pick the first active number assigned to this person.
            var aliasId = currentPerson.PrimaryAliasId;
            if ( !aliasId.HasValue )
            {
                return null;
            }

            var fallback = SystemPhoneNumberCache.All()
                .Where( spn =>
                    spn.IsActive
                    && spn.AssignedToPersonAliasId == aliasId.Value
                    && spn.IsSmsEnabled
                )
                .OrderByDescending( spn => spn.Id )
                .FirstOrDefault();

            return fallback;
        }

        #endregion

        #region Skill Tools

        /// <summary>
        /// Updates the current person's default SMS phone number preference.
        /// </summary>
        /// <param name="numberIdKey"></param>
        /// <returns></returns>
        [AgentToolGuid( "56278E81-B81A-46CC-A529-E164DBE35AD3" )]
        public RockToolResult UpdateCurrentPersonDefaultSmsPhoneNumber( string numberIdKey )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "The current person is not available. Ensure the agent is properly initialized." );
            }

            if ( numberIdKey.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "A numberIdKey is required to update the default SMS phone number." )
                    .WithInstructions( "Ask the user to select one of their available SMS 'from' numbers." );
            }

            var spn = SystemPhoneNumberCache.Get( numberIdKey, false );
            if ( spn == null || !spn.IsActive || !spn.IsSmsEnabled )
            {
                return RockToolResult.Error( "The provided numberIdKey does not correspond to a valid active SMS-enabled system phone number." )
                    .WithInstructions( "Ask the user to select one of their available SMS 'from' numbers." );
            }

            var prefs = AgentRequestContext.RockRequestContext.GetGlobalPersonPreferences();
            prefs.SetValue( PersonPreferenceKey.DEFAULT_SMS_PHONE_NUMBER, spn.Id.ToString() );
            prefs.Save();

            return RockToolResult.Success( $"The default SMS 'from' number has been updated to '{spn.Number}'." );
        }

        /// <summary>
        /// Looks up system phone numbers, optionally filtering to only SMS-enabled numbers.
        /// </summary>
        /// <param name="smsEnabled"></param>
        /// <returns></returns>
        [AgentToolGuid( "FD3F160F-ABCA-4A18-B69F-0E21D61B6874" )]
        public RockToolResult LookupSystemPhoneNumbers( bool? smsEnabled = null )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();

            var spnResults = GetSystemPhoneNumbers( rockContext, smsEnabled );

            // Trim down for history
            var trimmedSpns = spnResults.Select( spn => new KeyNameResult
            {
                Id = spn.Id,
                Name = spn.Name
            } );

            var historyKey = smsEnabled.HasValue ? $"system-phone-numbers-sms-{smsEnabled.Value}" : "system-phone-numbers-all";

            return RockToolResult.Success( spnResults )
                .WithHistoryContent( trimmedSpns, historyKey );
        }

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
                if( communicationType == AgentCommunicationType.Email )
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

        /// <summary>
        /// Sends a previously drafted communication.
        /// </summary>
        /// <param name="communicationIdKey"></param>
        /// <returns></returns>
        [AgentToolGuid( "2BB35960-77C6-4EAD-9645-F0ACB0EF132B" )]
        public RockToolResult SendCommunication( string communicationIdKey )
        {
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;

            if ( currentPerson == null )
            {
                return RockToolResult.Error( "The current person is not available. Ensure the agent is properly initialized." )
                    .WithInstructions( "Make sure the agent has access to the current person context." );
            }

            if ( communicationIdKey.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "A communicationIdKey is required to send a communication." )
                    .WithInstructions( "Ask the user if they would like to draft one." );
            }

            using var rockContext = _rockContextFactory.CreateRockContext();
            var communicationService = new CommunicationService( rockContext );
            var communication = communicationService.Get( communicationIdKey );

            if ( communication == null )
            {
                return RockToolResult.Error( $"No valid communication found for the provided communicationIdKey: {communicationIdKey}." );
            }

            if ( communication.Status != CommunicationStatus.Transient )
            {
                return RockToolResult.Error( "The communication is not in a transient state and cannot be sent." )
                    .WithInstructions( "Ensure the communication is in a transient state before sending." );
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
                return RockToolResult.Error( "Failed to update the communication status. Check the logs for details." );
            }

            SendCommunication( communication.Id );

            var instructions = "The communication has been queued to be sent. The user can view the details of the communication via the reference url.";

            // If the communication is SMS and came from a different number than the user's default, prompt the user
            // to see if we should update their default.
            if( communication.CommunicationType == CommunicationType.SMS )
            {
                // Get the from number of the communication.
                var fromNumberId = communication.SmsFromSystemPhoneNumberId;
                var userDefaultFromNumberId = GetDefaultSmsPhoneNumber()?.Id;

                if ( !userDefaultFromNumberId.HasValue || fromNumberId != userDefaultFromNumberId.Value )
                {
                    instructions += "\r\nAsk the user if they would like to use this number as their default for future messages.";
                }
            }

            return RockToolResult.Success( new SendCommunicationResult
            {
                CommunicationIdKey = communication.IdKey
            } )
            .WithInstructions( instructions )
            .WithHistoryKey( communicationIdKey )
            .WithReferenceRoute( AgentRequestContext.RockRequestContext, "Communication", $"/Communication/{communication.Id}", false );
        }

        /// <summary>
        /// Cancels and deletes a draft communication that has not yet been sent.
        /// </summary>
        /// <param name="communicationIdKey"></param>
        /// <returns></returns>
        [AgentToolGuid( "8EC76EA6-83BE-4796-9B91-6B4A34C0C3AD" )]
        public RockToolResult CancelDraft( string communicationIdKey )
        {
            if ( communicationIdKey.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "CommunicationIdKey is required." );
            }

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var draft = communicationService.Get( communicationIdKey, false );
                if ( draft == null )
                {
                    return RockToolResult.Error( "No communication record was found for that IdKey." );
                }

                if ( draft.Status != CommunicationStatus.Transient )
                {
                    return RockToolResult.Error( "You can not cancel a communication that is not in a transient state." );
                }

                if ( !communicationService.CanDelete( draft, out var errorMessage ) )
                {
                    return RockToolResult.Error( $"Unable to delete communication: {errorMessage}" );
                }

                communicationService.Delete( draft );

                rockContext.SaveChanges();

                return RockToolResult.Success( "The communication has been deleted." );
            }
        }

        #endregion
    }
}
