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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.AutomationTriggerDetail
{
    /// <summary>
    /// The additional configuration options for the Automation Trigger Detail block.
    /// </summary>
    public class AutomationTriggerDetailOptionsBag
    {
        /// <summary>
        /// The list of trigger types that can be used to create a new
        /// automation trigger.
        /// </summary>
        public List<ListItemBag> TriggerTypeItems { get; set; }

        /// <summary>
        /// The list of event types that can be used to create a new
        /// automation event.
        /// </summary>
        public List<ListItemBag> EventTypeItems { get; set; }
    }
}
