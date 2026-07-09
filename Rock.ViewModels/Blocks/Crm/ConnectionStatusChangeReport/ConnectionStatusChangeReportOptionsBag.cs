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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.ConnectionStatusChangeReport
{
    /// <summary>
    /// The additional configuration options for the Connection Status Change Report block.
    /// </summary>
    public class ConnectionStatusChangeReportOptionsBag
    {
        /// <summary>
        /// Gets or sets the delimited sliding date range string that the filter panel should
        /// initialize with. An empty value indicates the panel should fall back to its default.
        /// </summary>
        public string DateRange { get; set; }

        /// <summary>
        /// Gets or sets the campus the report is currently filtered to, or <see langword="null"/>
        /// when no campus filter is applied.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the connection status the report is filtered from, or <see langword="null"/>
        /// when no original-status filter is applied.
        /// </summary>
        public ListItemBag FromConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the connection status the report is filtered to, or <see langword="null"/>
        /// when no updated-status filter is applied.
        /// </summary>
        public ListItemBag ToConnectionStatus { get; set; }
    }
}
