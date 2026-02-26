using Rock.AI.Agent.Classes.Common;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal partial class CommunicationSkill
    {
        #region Tool(s)

        /// <summary>
        /// Cancels and deletes a draft communication that has not yet been sent.
        /// </summary>
        /// <param name="communicationIdKey"></param>
        /// <returns></returns>
        [AgentToolGuid( "8EC76EA6-83BE-4796-9B91-6B4A34C0C3AD" )]
        public IAgentToolResult CancelDraft( string communicationIdKey )
        {
            if ( communicationIdKey.IsNullOrWhiteSpace() )
            {
                return Error( "CommunicationIdKey is required." );
            }

            using var rockContext = RockApp.Current.CreateRockContext();
            var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
            var communicationService = new CommunicationService( rockContext );

            var draft = helper.GetRequiredEntity<Model.Communication>( communicationIdKey, checkSecurity: false );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( draft.Status != CommunicationStatus.Transient )
            {
                return Error( "You can not cancel a communication that is not in a transient state." );
            }

            if ( !communicationService.CanDelete( draft, out var errorMessage ) )
            {
                return Error( $"Unable to delete communication: {errorMessage}" );
            }

            communicationService.Delete( draft );

            rockContext.SaveChanges();

            return Success( "The communication has been deleted." );
        }

        #endregion
    }
}
