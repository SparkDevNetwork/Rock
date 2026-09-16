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

using Rock.AI.Agent.Annotations;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class SystemUtilitySkill
{
    #region Tool(s)

    [Description( "Gets the current date and time in the organization's configured time zone." )]
    [AgentToolPreamble( "Getting the current date and time." )]
    [AgentPurpose( "Gets the current date and time in the organization's configured time zone." )]
    [AgentUsage( "Use this tool when the request asks for the current date, time, or day of week, or when you need the current moment as a reference point for date math." )]
    [AgentToolGuid( "4CC7570A-3730-4A85-9B8C-21DD1315407C" )]
    public AgentToolResult GetCurrentDateTime()
    {
        var now = RockDateTime.Now;

        return Success( new CurrentDateTimeResult
        {
            DateTime = now.ToString( "s" ),
            DayOfWeek = now.DayOfWeek.ToString(),
            TimeZone = RockDateTime.OrgTimeZoneInfo.DisplayName
        } );
    }

    #endregion

    /// <summary>
    /// The payload returned by <see cref="GetCurrentDateTime"/>.
    /// </summary>
    private class CurrentDateTimeResult
    {
        /// <summary>
        /// The current date and time in ISO 8601 format, in the
        /// organization's configured time zone.
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// The current day of the week.
        /// </summary>
        public string DayOfWeek { get; set; }

        /// <summary>
        /// The display name of the organization's configured time zone.
        /// </summary>
        public string TimeZone { get; set; }
    }
}
