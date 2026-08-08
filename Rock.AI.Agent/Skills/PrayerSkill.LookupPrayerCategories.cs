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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PrayerSkill
{
    #region Tool(s)

    [Description( "Provides a list of prayer categories." )]
    [AgentToolGuid( "4E4A5AC6-85DC-4773-A03D-9BC1722366FD" )]
    public AgentToolResult LookupPrayerCategories()
    {
        var queryable = GetPrayerCategoriesQueryable( AgentRequestContext.RockContext );

        var prayerCategories = queryable
            .Select( pc => new CategoryResult
            {
                Id = pc.Id,
                Description = pc.Description,
                Name = pc.Name,
            } )
            .ToList();

        // Lose the description for history content.
        var trimmedCategories = prayerCategories.Select( pc => new KeyNameResult
        {
            Id = pc.Id,
            Name = pc.Name,
        } ).ToList();

        return Success( prayerCategories )
            .WithHistoryContent( trimmedCategories );
    }

    #endregion
}
