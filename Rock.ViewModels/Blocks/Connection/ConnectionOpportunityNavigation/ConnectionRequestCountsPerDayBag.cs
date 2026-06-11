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

namespace Rock.ViewModels.Blocks.Connection.ConnectionOpportunityNavigation
{
    /// <summary>
    /// A bag that contains information about connection request counts per day for the Connection Opportunity Navigation block.
    /// </summary>
    public class ConnectionRequestCountsPerDayBag
    {
        /// <summary>
        /// Gets or sets the start date for the connection request counts.
        /// </summary>
        /// <remarks>
        /// This date will correspond with the first entry in the <see cref="NewRequestCounts"/> and
        /// <see cref="CompletedRequestCounts"/> lists. The counts in those lists will be for each day
        /// starting with this date.
        /// </remarks>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the list of new connection request counts for each day starting with the <see cref="StartDate"/>.
        /// </summary>
        public List<int> NewRequestCounts { get; set; }

        /// <summary>
        /// Gets or sets the list of completed connection request counts for each day starting with the <see cref="StartDate"/>.
        /// </summary>
        public List<int> CompletedRequestCounts { get; set; }
    }
}
