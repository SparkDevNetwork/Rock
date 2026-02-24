using System;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.Configuration;
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
                return Error( "The current person is not available. Ensure the agent is properly initialized." )
                    .WithInstructions( "Make sure the agent has access to the current person context." );
            }

            if ( communicationIdKey.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "A communicationIdKey is required to send a communication." )
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
                .WithReferenceRoute( AgentRequestContext.RockRequestContext, "Communication", $"/Communication/{communication.Id}", false );

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

        #endregion
    }
}
