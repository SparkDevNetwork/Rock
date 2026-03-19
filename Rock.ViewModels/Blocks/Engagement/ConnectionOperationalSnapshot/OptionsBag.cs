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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionOperationalSnapshot
{
    /// <summary>
    /// Initialization data for the Connection Operational Snapshot block.
    /// Contains all data required to render the block without additional
    /// server requests.
    /// </summary>
    public class OptionsBag
    {
        /// <summary>
        /// Gets or sets the connection type identifier key.
        /// </summary>
        public string ConnectionTypeIdKey { get; set; }

        /// <summary>
        /// Gets or sets the connection type name.
        /// </summary>
        public string ConnectionTypeName { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunities for the connection type.
        /// </summary>
        public List<IdKeyListItemBag> ConnectionOpportunities { get; set; }

        /// <summary>
        /// Gets or sets the supported filter values and defaults for the block.
        /// </summary>
        public FiltersBag Filters { get; set; }

        /// <summary>
        /// Gets or sets the current snapshot of connection request state.
        /// </summary>
        public RequestStateBag RequestState { get; set; }

        /// <summary>
        /// Gets or sets the time-based workload and upcoming follow-up data.
        /// </summary>
        public RequestTimelineBag RequestTimeline { get; set; }

        /// <summary>
        /// Gets or sets the metrics related to completion quality and efficiency.
        /// </summary>
        public CompletionMetricsBag CompletionMetrics { get; set; }
    }
}
