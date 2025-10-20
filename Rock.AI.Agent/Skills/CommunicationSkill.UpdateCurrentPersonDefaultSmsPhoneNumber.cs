using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.AI.Agent.Classes.Common;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class CommunicationSkill
    {
        #region Tool(s)

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

        #endregion
    }
}
