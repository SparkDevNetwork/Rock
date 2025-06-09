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
using Rock.ViewModels.Core.Automation;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.AutomationTriggerDetail
{
    /// <summary>
    /// The item details for the Automation Trigger Detail block.
    /// </summary>
    public class AutomationTriggerBag : EntityBagBase
    {
        /// <summary>
        /// The description of the trigger. This is used to provide additional
        /// details about when the trigger will execute the events and describe
        /// the purpose the trigger serves.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates if this trigger is active. If this is set to false
        /// then the trigger will not be initialized and no events will execute.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The name of the trigger. This is used to identify the trigger in the
        /// user interface and logs. It should be short, but descriptive.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of trigger component that will be used to process this
        /// trigger.
        /// </summary>
        public ListItemBag TriggerType { get; set; }

        /// <summary>
        /// The definition of the initial component to render based on the
        /// current selection. This is read-only.
        /// </summary>
        public DynamicComponentDefinitionBag ComponentDefinition { get; set; }

        /// <summary>
        /// The configuration settings for the trigger component.
        /// </summary>
        public Dictionary<string, string> ComponentConfiguration { get; set; }

        /// <summary>
        /// The configuration details that will be displayed in the UI when
        /// in view mode. These provide additional context about how the component
        /// is configured. This is read-only.
        /// </summary>
        public List<ListItemBag> ConfigurationDetails { get; set; }

        /// <summary>
        /// The events that have been created and attached to this trigger.
        /// </summary>
        public List<AutomationEventBag> Events { get; set; }

        /// <summary>
        /// The definitions for what values are available on events executed by
        /// this trigger. This is only valid in view mode and is read-only.
        /// </summary>
        public List<AutomationValueDefinitionBag> ValueDefinitions { get; set; }
    }
}

