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

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.AI.AIAgentDetail
{
    /// <summary>
    /// Represents the response data for retrieving a component definition. This
    /// contains the data required to display a custom Obsidian component in the
    /// UI when adding a skill to an AI agent.
    /// </summary>
    public class GetComponentDefinitionResponseBag
    {
        /// <summary>
        /// The list of tools defined on the skill that can be enabled.
        /// </summary>
        public List<ListItemBag> AvailableTools { get; set; }

        /// <summary>
        /// The current configuration values for the skill.
        /// </summary>
        public Dictionary<string, string> ConfigurationValues { get; set; }

        /// <summary>
        /// The component definition that will be used to render the UI
        /// for the skill.
        /// </summary>
        public DynamicComponentDefinitionBag ComponentDefinition { get; set; }
    }
}
