using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PrayerSkill
    {
        #region Tool(s)

        /// <summary>
        /// Deletes the prayer request with the provided idKey.
        /// </summary>
        /// <param name="idKey"></param>
        /// <returns></returns>
        [Description( "Deletes a prayer request." )]
        [AgentToolGuid( "423AFDB5-1095-4D55-8631-4F284FC0AFED" )]
        [AgentGuardrail( "This action will permanently delete the specified prayer request. Ensure that this action is intentional and that you have the correct prayer request identifier before proceeding." )]
        public RockToolResult DeletePrayerRequest( string idKey )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();
            var prayerRequestService = new PrayerRequestService( rockContext );
            var existingPrayerRequest = prayerRequestService.Get( idKey, false );
            if ( existingPrayerRequest == null )
            {
                return RockToolResult.Error( "Invalid prayer request provided." );
            }
            prayerRequestService.Delete( existingPrayerRequest );
            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "An error occurred while deleting a prayer request." );
                return RockToolResult.Error( "An error occurred while deleting the prayer request." );
            }

            return RockToolResult.Success()
                .WithHistoryContent( existingPrayerRequest.IdKey, existingPrayerRequest.IdKey )
                .WithInstructions( "The prayer request has been deleted." );
        }

        #endregion

    }
}
