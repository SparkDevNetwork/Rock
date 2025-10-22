using System;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Tasks;

namespace Rock.AI.Agent.Skills
{
    internal partial class CommunicationSkill
    {
        #region Tool(s)

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
            if ( communication.CommunicationType == CommunicationType.SMS )
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

        #endregion
    }
}
