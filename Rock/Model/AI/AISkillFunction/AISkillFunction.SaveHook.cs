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

using System.Linq;

using Rock.Data;
using Rock.Web.Cache.Entities;

namespace Rock.Model
{
    public partial class AISkillFunction
    {
        /// <summary>
        /// Save hook implementation for <see cref="AISkillFunction"/>.
        /// </summary>
        internal class SaveHook : EntitySaveHook<AISkillFunction>
        {
            /// <inheritdoc/>
            protected override void PostSave()
            {
                var agentIds = RockContext.Set<AIAgentSkill>()
                    .Where( a => a.AISkillId == Entity.AISkillId )
                    .Select( a => a.AIAgentId )
                    .ToList();

                // Flush the cache for all the agents that this function is linked
                // so that they will rebuild their configuration data.
                foreach ( var agentId in agentIds )
                {
                    AIAgentCache.FlushItem( agentId );
                }
            }
        }
    }
}
