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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionOperationalSnapshot
{
    /// <summary>
    /// Performance metrics for an individual connector.
    /// </summary>
    public class ConnectorBag
    {
        /// <summary>
        /// Gets or sets the connector person's unique identifier key.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the connector person.
        /// </summary>
        public PersonFieldBag Person { get; set; }

        /// <summary>
        /// Gets or sets the umber of active requests assigned to the connector.
        /// </summary>
        public int ActiveRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the number of overdue requests assigned to the connector.
        /// </summary>
        public int OverdueRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the number of completed requests handled by the connector.
        /// </summary>
        public int CompletedRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the average number of days required to complete requests.
        /// </summary>
        public decimal AverageCompletionDays { get; set; }
    }
}
