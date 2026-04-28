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
using System.Linq;

namespace Rock.ViewModels.Blocks.Connection.ConnectionOpportunityNavigation
{
    /// <summary>
    /// A bag that contains information about connection opportunity metrics and summaries for the Connection
    /// Opportunity Navigation block.
    /// </summary>
    public class ConnectionOpportunityNavigationDetailsBag
    {
        /// <summary>
        /// Gets or sets the summary information for the connection type to display.
        /// </summary>
        public ConnectionTypeSummaryBag ConnectionTypeSummary { get; set; }

        /// <summary>
        /// Gets or sets the list of filtered connection opportunity summaries to display.
        /// </summary>
        public List<ConnectionOpportunitySummaryBag> ConnectionOpportunitySummaries { get; set; }

        /// <summary>
        /// Gets the total count of active connection requests for all filtered connection opportunities.
        /// </summary>
        public int TotalActiveRequestsCount
        {
            get
            {
                if ( ConnectionOpportunitySummaries?.Any() != true )
                {
                    return 0;
                }

                return ConnectionOpportunitySummaries.Sum( s => s.ActiveRequestCount );
            }
        }

        /// <summary>
        /// Gets the total count of connection requests that are due soon for all filtered connection opportunities.
        /// </summary>
        public int TotalDueSoonRequestsCount
        {
            get
            {
                if ( ConnectionOpportunitySummaries?.Any() != true )
                {
                    return 0;
                }

                return ConnectionOpportunitySummaries.Sum( s => s.DueSoonRequestCount );
            }
        }

        /// <summary>
        /// Gets the total count of connection requests that are overdue for all filtered connection opportunities.
        /// </summary>
        public int TotalOverdueRequestsCount
        {
            get
            {
                if ( ConnectionOpportunitySummaries?.Any() != true )
                {
                    return 0;
                }

                return ConnectionOpportunitySummaries.Sum( s => s.OverdueRequestCount );
            }
        }

        /// <summary>
        /// Gets the total count of connection requests that are on track (not due soon and not overdue) for all filtered connection opportunities.
        /// </summary>
        public int TotalOnTrackRequestsCount => TotalActiveRequestsCount - TotalDueSoonRequestsCount - TotalOverdueRequestsCount;

        /// <summary>
        /// Gets or sets the connection request counts per day for all filtered connection opportunities.
        /// </summary>
        public ConnectionRequestCountsPerDayBag RequestCountsPerDay { get; set; }
    }
}
