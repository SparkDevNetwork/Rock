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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionTypeDetail
{
    /// <summary>
    /// Minimal bag representation of a Connection Status Automation.
    /// </summary>
    public class ConnectionStatusAutomationBag
    {
        /// <summary>
        /// Gets or sets the Guid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the name of the automation.
        /// </summary>
        public string AutomationName { get; set; }

        /// <summary>
        /// Gets or sets the destination status Guid.
        /// </summary>
        public Guid? DestinationStatusGuid { get; set; }

        /// <summary>
        /// Gets or sets the data view as a <see cref="ListItemBag"/>.
        /// </summary>
        public ListItemBag DataView { get; set; }

        /// <summary>
        /// Gets or sets the group requirements filter.
        /// </summary>
        public GroupRequirementsFilter GroupRequirementsFilter { get; set; }
    }
}
