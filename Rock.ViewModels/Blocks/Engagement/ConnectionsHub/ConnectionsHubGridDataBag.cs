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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the response for the Connections Hub grid data request, containing the grid
    /// row data along with grouping metadata for the connectors present in the result set.
    /// </summary>
    public class ConnectionsHubGridDataBag
    {
        /// <summary>
        /// Gets or sets the grid row data for the connection request list.
        /// </summary>
        public GridDataBag GridData { get; set; }

        /// <summary>
        /// Gets or sets the grouping metadata for the distinct connectors present in the grid
        /// data. Used by the client to label connector groups whose connector is not part of
        /// the block-load available groupings list (e.g. a connector who is not a member of
        /// any connector group).
        /// </summary>
        public List<GroupingFieldBag> ConnectorGroupings { get; set; }
    }
}
