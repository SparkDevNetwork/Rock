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

using Rock.Data;
using Rock.Web.Cache.Entities;

namespace Rock.Model
{
    public partial class AIAgentSkill
    {
        /// <summary>
        /// Save hook implementation for <see cref="AIAgentSkill"/>.
        /// </summary>
        internal class SaveHook : EntitySaveHook<AIAgentSkill>
        {
            /// <inheritdoc/>
            protected override void PostSave()
            {
                // Flush the cache for the agent so that it will rebuild it's
                // configuration data.
                AIAgentCache.FlushItem( Entity.AIAgentId );
            }
        }
    }
}
