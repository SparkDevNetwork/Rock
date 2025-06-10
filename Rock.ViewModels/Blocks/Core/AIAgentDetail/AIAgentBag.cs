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

using System.Collections.Generic;

using Rock.Enums.Core.AI.Agent;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.AIAgentDetail
{
    /// <summary>
    /// The item details for the AI Agent Detail block.
    /// </summary>
    public class AIAgentBag : EntityBagBase
    {
        /// <summary>
        /// A collection containing the Rock.Model.AIAgentSkill entities
        /// that represent the skills attached to this agent.
        /// </summary>
        public List<ListItemBag> Skills { get; set; }

        /// <summary>
        /// The binary file that contains the image to use as the avatar to
        /// represent the agent. This will be used in the administrative UI and
        /// the chat UI to represent the agent visually.
        /// </summary>
        public ListItemBag AvatarBinaryFile { get; set; }

        /// <summary>
        /// The description of the agent, which provides additional context or
        /// information about its intended purpose and functionality.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The friendly name of the agent that will be used to identify it in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The persona of the agent, which is a string that describes how the
        /// agent should behavor or respond. This can include tone, style, and
        /// special instructions it should follow when interacting with people.
        /// </summary>
        public string Persona { get; set; }

        /// <summary>
        /// The role that this agent uses when determining which AI model to use.
        /// </summary>
        public ModelServiceRole Role { get; set; }
    }
}
