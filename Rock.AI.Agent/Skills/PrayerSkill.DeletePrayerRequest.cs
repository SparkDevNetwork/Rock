using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Configuration;
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
        /// <param name="prayerRequestIdKey"></param>
        /// <returns></returns>
        [Description( "Deletes a prayer request." )]
        [AgentToolGuid( "423AFDB5-1095-4D55-8631-4F284FC0AFED" )]
        [AgentGuardrail( "This action will permanently delete the specified prayer request. Ensure that this action is intentional and that you have the correct prayer request identifier before proceeding." )]
        public RockToolResult DeletePrayerRequest( string prayerRequestIdKey )
        {
            using var rockContext = RockApp.Current.CreateRockContext();
            var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
            var prayerRequestService = new PrayerRequestService( rockContext );

            var existingPrayerRequest = helper.GetRequiredEntity<PrayerRequest>( prayerRequestIdKey, checkSecurity: false );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            prayerRequestService.Delete( existingPrayerRequest );

            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "An error occurred while deleting a prayer request." );
                return Error( "An error occurred while deleting the prayer request." );
            }

            return Success( "The prayer request has been deleted." );
        }

        #endregion

    }
}
