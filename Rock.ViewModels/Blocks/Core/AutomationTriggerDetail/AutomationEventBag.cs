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

namespace Rock.ViewModels.Blocks.Core.AutomationTriggerDetail
{
    /// <summary>
    /// The item details for the Automation Event.
    /// </summary>
    public class AutomationEventBag : EntityBagBase
    {
        /// <summary>
        /// The description of the event. This is calculated by the component
        /// so it is considered read-only.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The component icon class for this event. This is read-only.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Indicates if this event is active. If this is set to false
        /// then the event will not be executed.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The description of the event. This is calculated by the component
        /// so it is considered read-only.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of event component that will be used to process this
        /// event.
        /// </summary>
        public ListItemBag EventType { get; set; }

        /// <summary>
        /// The definition of the initial component to render based on the
        /// current selection. This is read-only.
        /// </summary>
        public DynamicComponentDefinitionBag ComponentDefinition { get; set; }

        /// <summary>
        /// The configuration settings for the event component.
        /// </summary>
        public Dictionary<string, string> ComponentConfiguration { get; set; }
    }
}
