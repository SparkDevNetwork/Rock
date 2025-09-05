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
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.AI.Agent.Utilities.CommunicationSkill.Mediums;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.Tasks;

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
        private IAgentCommunicationMedium TryGetCommunicationMedium( AgentCommunicationType communicationType, RockContext rockContext )
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
                if ( !MediumContainer.HasActiveSmsTransport() )
                {
                    return null;
                }

                //medium = new SmsMedium( rockContext );
                medium = new EmailMedium();
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

        #region Skill Tools

        /// <summary>
        /// Drafts a communication (email/SMS/push) for a specified recipient.
        /// </summary>
        /// <param name="kernel">The kernel. Used to invoke an internal prompt to structure the comm.</param>
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

                var medium = TryGetCommunicationMedium( communicationType, rockContext );
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

                var draftRequest = new DraftRequest( communicationType, subjectHint, draftGuidance, referenceData, tone, currentPerson, recipients );

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

        [AgentToolGuid( "2BB35960-77C6-4EAD-9645-F0ACB0EF132B" )]
        public RockToolResult SendCommunication( string communicationIdKey )
        {
            var requestContext = RockRequestContextAccessor.Current;
            var currentPerson = requestContext?.CurrentPerson;

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

            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
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

                return RockToolResult.Success( new SendCommunicationResult
                {
                    CommunicationIdKey = communication.IdKey
                } )
                .WithInstructions( instructions )
                .WithHistoryKey( communicationIdKey )
                .WithReferenceRoute( requestContext, "Communication", $"/Communication/{communication.Id}", false );
            }
        }

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
