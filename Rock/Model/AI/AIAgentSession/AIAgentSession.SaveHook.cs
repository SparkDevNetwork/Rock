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

namespace Rock.Model
{
    public partial class AIAgentSession
    {
        /// <summary>
        /// Save hook implementation for <see cref="AIAgentSession"/>.
        /// </summary>
        internal class SaveHook : EntitySaveHook<AIAgentSession>
        {
            /// <inheritdoc/>
            protected override void PreSave()
            {
                if ( PreSaveState == EntityContextState.Added && Entity.StartDateTime == default )
                {
                    // Set the StartDateTime to now if it hasn't been set yet.
                    Entity.StartDateTime = RockDateTime.Now;
                }

                if ( PreSaveState == EntityContextState.Added && Entity.LastMessageDateTime == default )
                {
                    // Set the LastMessageDateTime to now if it hasn't been set yet.
                    Entity.LastMessageDateTime = RockDateTime.Now;
                }
            }
        }
    }
}
