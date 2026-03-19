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

namespace Rock.ViewModels.Blocks.Connection.ConnectionOpportunityNavigation
{
    /// <summary>
    /// A bag that contains information about a connection opportunity summary for the Connection Opportunity Navigation block.
    /// </summary>
    public class ConnectionOpportunitySummaryBag : ITranslateIdKey
    {
        /// <inheritdoc />
        public int? Id { get; set; }

        /// <inheritdoc />
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class for this connection opportunity.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the name for this connection opportunity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the summary for this connection opportunity.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the order for this connection opportunity.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the count of active connection requests for this connection opportunity.
        /// </summary>
        public int ActiveRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the count of "due soon" connection requests for this connection opportunity.
        /// </summary>
        public int DueSoonRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the count of overdue connection requests for this connection opportunity.
        /// </summary>
        public int OverdueRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the count of unassigned connection requests for this connection opportunity.
        /// </summary>
        public int UnassignedRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the count of connection requests assigned to the current person for this connection opportunity.
        /// </summary>
        public int AssignedToYouRequestCount { get; set; }
    }
}
