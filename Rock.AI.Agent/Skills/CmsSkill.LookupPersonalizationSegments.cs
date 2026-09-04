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
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    /// <summary>
    /// Looks up the personalization segments configured in Rock.
    /// </summary>
    /// <remarks>
    /// A Lookup because the set is a bounded configuration surface returned whole.
    /// A personalization segment is a named audience used to tailor content.
    /// </remarks>
    [Description( "Looks up the personalization segments configured in Rock. A segment is a named audience (such as 'First-time Visitors') used to personalize content." )]
    [AgentPurpose( "Finds a personalization segment so its configuration can be retrieved." )]
    [AgentToolGuid( "4C291DD0-82C2-4883-AFEF-559B492DC715" )]
    public AgentToolResult LookupPersonalizationSegments()
    {
        var results = PersonalizationSegmentCache.All( AgentRequestContext.RockContext )
            .OrderBy( s => s.Name )
            .Select( s => new PersonalizationSegmentResult
            {
                Id = s.Id,
                Guid = s.Guid,
                Name = s.Name,
                SegmentKey = s.SegmentKey
            } )
            .ToList();

        if ( !results.Any() )
        {
            return NoData()
                .WithInstructions( "No personalization segments are configured." );
        }

        return Success( results )
            .WithHistoryKey( "personalization-segments" );
    }

    #endregion
}
