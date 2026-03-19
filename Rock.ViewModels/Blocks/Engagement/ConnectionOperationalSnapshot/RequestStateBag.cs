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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionOperationalSnapshot
{
    /// <summary>
    /// Aggregated counts describing the current state of connection requests.
    /// </summary>
    public class RequestStateBag
    {
        /// <summary>
        /// Gets or sets the total number of active requests.
        /// </summary>
        public int TotalActive { get; set; }

        /// <summary>
        /// Gets or sets the total number of active requests without an assigned connector.
        /// </summary>
        public int TotalUnassigned { get; set; }

        /// <summary>
        /// Gets or sets the total number of requests that are due soon.
        /// </summary>
        public int TotalDueSoon { get; set; }

        /// <summary>
        /// Gets or sets the total number of overdue requests.
        /// </summary>
        public int TotalOverdue { get; set; }

        /// <summary>
        /// Gets or sets the total number of requests that are on track.
        /// </summary>
        public int TotalOnTrack { get; set; }

        /// <summary>
        /// Gets or sets the request counts grouped by status.
        /// </summary>
        public List<RequestStatusCountBag> ByStatus { get; set; }
    }
}
