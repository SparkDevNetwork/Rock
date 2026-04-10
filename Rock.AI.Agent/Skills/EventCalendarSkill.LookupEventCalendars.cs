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
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class EventCalendarSkill
{
    #region Tool(s)

    /// <summary>
    /// Retrieves all configured calendars in Rock.
    /// </summary>
    [Description( "Retrieves all configured calendars in Rock." )]
    [AgentPurpose( "Retrieves all configured calendars in Rock." )]
    [AgentToolGuid( "dbc1ad8a-f41c-4bb7-89de-f9d795f017de" )]
    public AgentToolResult LookupEventCalendars()
    {
        var calendarResults = GetConfiguredCalendars()
            .Select( c => new KeyNameResult( c.Id, c.Name ) )
            .OrderBy( kn => kn.Name )
            .ToList();

        var result = Success( calendarResults );

        if ( calendarResults.Count > 50 )
        {
            result = result.WithoutHistoryContent();
        }

        return result;
    }

    #endregion
}
