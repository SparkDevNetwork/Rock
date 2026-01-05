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

namespace Rock.ViewModels.Blocks.Core.AutomationTriggerDetail
{
    /// <summary>
    /// The response to the GetComponentDefinition and GetEventComponentDefinition
    /// block actions.
    /// </summary>
    public class GetComponentDefinitionResponseBag
    {
        /// <summary>
        /// The definition of the initial component to render based on the
        /// current selection.
        /// </summary>
        public DynamicComponentDefinitionBag ComponentDefinition { get; set; }

        /// <summary>
        /// The configuration settings for the trigger component.
        /// </summary>
        public Dictionary<string, string> ComponentConfiguration { get; set; }
    }
}

