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
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.WebFarm.WebFarmNodeDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class WebFarmNodeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the current leadership polling interval seconds.
        /// </summary>
        public decimal CurrentLeadershipPollingIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is current job runner.
        /// </summary>
        public bool IsCurrentJobRunner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is leader.
        /// </summary>
        public bool IsLeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is unresponsive.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is unresponsive; otherwise, <c>false</c>.
        /// </value>
        public bool IsUnresponsive { get; set; }

        /// <summary>
        /// Gets or sets the last seen date time.
        /// </summary>
        public DateTime LastSeenDateTime { get; set; }

        /// <summary>
        /// Gets or sets the human readable last seen.
        /// </summary>
        /// <value>
        /// The human readable last seen.
        /// </value>
        public string HumanReadableLastSeen { get; set; }

        /// <summary>
        /// Gets or sets a Node Name.
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// Gets or sets the web farm node metrics.
        /// </summary>
        public List<WebFarmMetricBag> WebFarmNodeMetrics { get; set; }

        /// <summary>
        /// Gets or sets the node detail page URL.
        /// </summary>
        /// <value>
        /// The node detail page URL.
        /// </value>
        public string NodeDetailPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the chart data.
        /// </summary>
        /// <value>
        /// The chart HTML.
        /// </value>
        public string ChartData { get; set; }
    }
}
