using Rock.AI.Agent.Classes.Common;
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
