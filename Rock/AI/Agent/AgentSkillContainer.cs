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

using System;

using Rock.Extension;

namespace Rock.AI.Agent
{
    /// <summary>
    /// A container for AI agent skills. This container is used to register
    /// and get instances of <see cref="AgentSkillComponent"/> components. These skills
    /// provide plug-in functionality for the AI agent.
    /// </summary>
    internal class AgentSkillContainer : LightContainer<AgentSkillComponent>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentSkillContainer"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider that will be used to construct component instances.</param>
        public AgentSkillContainer( IServiceProvider serviceProvider )
            : base( serviceProvider )
        {
        }

        #endregion
    }
}
