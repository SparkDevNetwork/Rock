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
//
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class StreakSkill
    {
        #region Tool(s)

        /// <summary>
        /// Retrieves all configured achievement types in Rock.
        /// </summary>
        [Description( "Retrieves all configured achievement types in Rock." )]
        [AgentPurpose( "Retrieves all configured achievement types in Rock." )]
        [AgentToolGuid( "f6c9300e-a770-4f92-827b-abc01ed21c8b" )]
        public IAgentToolResult LookupAchievementTypes()
        {
            var achievementTypeResults = AchievementTypeCache.All( AgentRequestContext.RockContext )
                .Where( at => at.IsActive )
                .Select( at => new KeyNameResult( at.Id, at.Name ) )
                .OrderBy( at => at.Name )
                .ToList();

            var result = Success( achievementTypeResults );

            if ( achievementTypeResults.Count < 50 )
            {
                result.WithHistoryContent( achievementTypeResults );
            }

            return result;
        }

        #endregion
    }
}
