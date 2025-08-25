using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.AI.Agent.Utilities.CommunicationSkill.Mediums;
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
    [AgentSkillGuid( "37DF3637-9775-4A89-9A77-BF6744232991" )]
    [EntityTypeGuid( "F67D0B02-B59F-475F-A005-8F2A5CCCA91C" )]
    internal sealed class CommunicationSkill : AgentSkillComponent
    {
        #region Fields

        private readonly ILogger<CommunicationSkill> _logger;
        private readonly RockContext _rockContext;

        #endregion

        #region Constructors

        public CommunicationSkill( RockContext rockContext, ILogger<CommunicationSkill> logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Methods

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
        private IAgentCommunicationMedium GetCommunicationMedium( AgentCommunicationType communicationType )
        {
            if ( communicationType == AgentCommunicationType.Email )
            {
                return new EmailMedium();
            }

            return null;
        }

        #endregion

        #region Kernel Functions

        [KernelFunction]
        [AgentFunctionGuid( "4EEF6200-AA05-4F26-AB4D-19C73DEB3BDD" )]
        [Description( "🎯 Purpose:\r\n" +
                "Always use this tool to draft emails, SMS messages or push notifications. These communications are saved as Transient and can later be sent. To update an existing draft, pass in the existingDraftIdKey." )]
        public async Task<RockToolResult> DraftCommunication(
                    Kernel kernel,

                    // BC TODO: Enum parameter types are broken in MCP.
                    // This is a workaround. This should not make it into final implementation.
                    [Description("Email | Sms | Push")]
                    string communicationType,
                    string subjectHint,
                    string recipientIdKey,

                    [Description("The data corresponding to the draft being written. Not the body itself.")]
                    string referenceData,
                    string draftGuidance,
                    string tone = "warm",

                    [Description("An optional parameter to update an existing draft as opposed to saving a new one.")]
                    string existingDraftIdKey = "" )
        {
            var commType = communicationType.ConvertToEnumOrNull<AgentCommunicationType>();
            if ( commType == null )
            {
                return RockToolResult.Error( $"The communication type '{communicationType}' is not recognized." )
                    .WithInstructions( "Ensure the communication type is one of: Email, Sms, or Push." );
            }

            var requestContext = RockRequestContextAccessor.Current;
            var currentPerson = requestContext?.CurrentPerson;
            if ( currentPerson == null )
            {
                return RockToolResult.Error( "The current person is not available. Ensure the agent is properly initialized." );
            }

            var personService = new PersonService( _rockContext );
            var communicationService = new CommunicationService( _rockContext );

            var recipient = personService.Get( recipientIdKey, false );
            if ( recipient == null )
            {
                return RockToolResult.Error( $"No valid recipient found for the provided recipientIdKey: {recipientIdKey}." )
                    .WithInstructions( "Verify the recipientIdKey and try again." );
            }

            var medium = GetCommunicationMedium( commType.Value );
            if ( medium == null )
            {
                return RockToolResult.Error( $"The communication type '{communicationType}' is not supported." )
                    .WithInstructions( "Tell the user that this is coming soon, Braden's got it." );
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

            var recipientValidation = medium.ValidateRecipient( recipient );
            if ( recipientValidation.Count > 0 )
            {
                return RockToolResult.Error( recipientValidation );
            }

            var draftRequest = new DraftRequest( commType.Value, subjectHint, draftGuidance, referenceData, tone, currentPerson, recipient );

            DraftResult draftResult;
            try
            {
                draftResult = await medium.DraftAsync( kernel, draftRequest, recipient );
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
                medium.UpdateCommunication( draftRequest, recipient, draftCommunication, draftResult );
            }
            else
            {
                draftCommunication = medium.BuildCommunication( draftRequest, recipient, draftResult );
                if ( draftCommunication == null )
                {
                    return RockToolResult.Error( "Failed to build the communication object." );
                }

                communicationService.Add( draftCommunication );
            }

            try
            {
                _rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "Failed to save communication." );
                return RockToolResult.Error( "Failed to save the communication. Check the logs for details." );
            }

            // Update our draft result with the newly saved communication.
            draftResult.CommunicationIdKey = draftCommunication.IdKey;

            var returnInstructions = "Never call SendCommunication directly after this.";

            returnInstructions += "\r\nAsk the user for verification on the following fields: \r\n";
            returnInstructions += draftResult.GetVerificationText( currentPerson, recipient );

            var historyContent = new
            {
                Recipient = new KeyNameResult( recipient.IdKey, recipient.FullName ),
                CommunicationIdKey = draftCommunication.IdKey
            };

            return RockToolResult.Success( draftResult )
                .WithInstructions( returnInstructions )
                .WithHistoryContent( historyContent )
                .WithReferenceRoute( requestContext, "Draft Communication", $"/Communication/{draftCommunication.Id}", false );
        }

        [KernelFunction( "SendCommunication" )]
        [AgentFunctionGuid( "2BB35960-77C6-4EAD-9645-F0ACB0EF132B" )]
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

            var communicationService = new CommunicationService( _rockContext );
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
                _rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "Failed to update communication status." );
                return RockToolResult.Error( "Failed to update the communication status. Check the logs for details." );
            }

            SendCommunication( communication.Id );

            var instructions = "The communication has been queued to be sent.";

            return RockToolResult.Success( new SendCommunicationResult
            {
                CommunicationIdKey = communication.IdKey
            } )
            .WithInstructions( instructions )
            .WithReferenceRoute( requestContext, "Communication", $"/Communication/{communication.Id}", false );
        }

        [KernelFunction]
        [AgentFunctionGuid( "8EC76EA6-83BE-4796-9B91-6B4A34C0C3AD" )]
        public RockToolResult CancelDraft( string communicationIdKey )
        {
            if( communicationIdKey.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "CommunicationIdKey is required." );
            }
            var communicationService = new CommunicationService( _rockContext );
            var draft = communicationService.Get( communicationIdKey, false );
            if ( draft == null )
            {
                return RockToolResult.Error( "No communication record was found for that IdKey." );
            }

            if( draft.Status != CommunicationStatus.Transient )
            {
                return RockToolResult.Error( "You can not cancel a communication that is not in a transient state." );
            }

            if( !communicationService.CanDelete(draft, out var errorMessage ) )
            {
                return RockToolResult.Error( $"Unable to delete communication: {errorMessage}" );
            }

            communicationService.Delete( draft );
            return RockToolResult.Success( "The communication has been deleted" );
        }

        #endregion
    }
}
