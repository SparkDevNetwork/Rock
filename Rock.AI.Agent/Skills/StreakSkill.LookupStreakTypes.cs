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
using Rock.AI.Agent.Classes.Skills.StreakSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class StreakSkill
    {
        #region Tool(s)

        /// <summary>
        /// Retrieves all configured streak types in Rock.
        /// </summary>
        [Description( "Retrieves all configured streak types in Rock." )]
        [AgentPurpose( "Retrieves all configured streak types in Rock." )]
        [AgentToolGuid( "dbc1ad8a-f41c-4bb7-89de-f9d795f017de" )]
        public IAgentToolResult LookupStreakTypes()
        {
            var streakTypeResults = StreakTypeCache.All( AgentRequestContext.RockContext )
                .Where( st => st.IsActive
                    && st.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
                .Select( st => new StreakTypeResult
                {
                    Id = st.Id,
                    Name = st.Name,
                    Description = st.Description,
                    OccurrenceFrequency = st.OccurrenceFrequency,
                } )
                .OrderBy( kn => kn.Name )
                .ToList();

            var result = Success( streakTypeResults );

            if ( streakTypeResults.Count > 50 )
            {
                result = result.WithoutHistoryContent();
            }

            return result;
        }

        #endregion
    }
}
