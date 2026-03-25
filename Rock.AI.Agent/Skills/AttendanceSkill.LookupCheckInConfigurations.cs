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
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class AttendanceSkill
{
    #region Tool(s)

    [Description( "Retrieves all configured check-in configurations in Rock." )]
    [AgentPurpose( "Retrieves a list of all of the check-in configurations." )]
    [AgentPurpose( "Check-in configurations are the high level container for all check-in related areas, groups and locations." )]
    [AgentToolGuid( "c11e7eff-7ef8-4476-b713-74ee267648bb" )]
    public IAgentToolResult LookupCheckInConfigurations()
    {
        var checkInConfigurationPurposeId = DefinedValueCache.Get( DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid(), AgentRequestContext.RockContext ).Id;

        var checkInConfigurations = GroupTypeCache.All( AgentRequestContext.RockContext )
            .Where( gt => gt.GroupTypePurposeValueId == checkInConfigurationPurposeId );

        var checkInConfigurationResults = checkInConfigurations
            .Select( c => new KeyNameResult
            {
                Id = c.Id,
                Name = c.Name,
            } )
            .ToList();

        return Success( checkInConfigurationResults )
            .WithHistoryContent( checkInConfigurationResults );
    }

    #endregion
}
