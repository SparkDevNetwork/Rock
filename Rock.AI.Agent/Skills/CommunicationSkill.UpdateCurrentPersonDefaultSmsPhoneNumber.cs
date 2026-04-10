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

using System.ComponentModel;

using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class CommunicationSkill
{
    #region Tool(s)

    [Description( "Updates the current person's default SMS phone number preference." )]
    [AgentToolGuid( "56278E81-B81A-46CC-A529-E164DBE35AD3" )]
    public AgentToolResult UpdateCurrentPersonDefaultSmsPhoneNumber( string numberIdKey )
    {
        var currentPerson = AgentRequestContext.CurrentPerson;
        if ( currentPerson == null )
        {
            return Error( "The current person is not available. Ensure the agent is properly initialized." );
        }

        if ( numberIdKey.IsNullOrWhiteSpace() )
        {
            return Error( "A numberIdKey is required to update the default SMS phone number." )
                .WithInstructions( "Ask the user to select one of their available SMS 'from' numbers." );
        }

        var spn = SystemPhoneNumberCache.Get( numberIdKey, false );
        if ( spn == null || !spn.IsActive || !spn.IsSmsEnabled )
        {
            return Error( "The provided numberIdKey does not correspond to a valid active SMS-enabled system phone number." )
                .WithInstructions( "Ask the user to select one of their available SMS 'from' numbers." );
        }

        var prefs = PersonPreferenceCache.GetPersonPreferenceCollection( currentPerson );
        prefs.SetValue( PersonPreferenceKey.DEFAULT_SMS_PHONE_NUMBER, spn.Id.ToString() );
        prefs.Save();

        return Success( $"The default SMS 'from' number has been updated to '{spn.Number}'." );
    }

    #endregion
}
