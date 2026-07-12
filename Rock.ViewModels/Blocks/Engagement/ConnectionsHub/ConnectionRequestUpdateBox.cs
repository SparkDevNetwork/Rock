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
    /// Represents the combined response when a connection request is updated, containing both the refreshed grid row data and the updated detail panel payload.
    /// </summary>
    public class ConnectionListUpdateBox
    {
        /// <summary>
        /// Gets or sets the updated grid row data for the affected connection request, keyed by column name.
        /// </summary>
        public Dictionary<string, object> GridRow { get; set; }

        /// <summary>
        /// Gets or sets the grouping metadata for the request's currently assigned connector.
        /// Used by the client to label the connector's group when the connector is not part
        /// of the block-load available groupings list. Null when the request is unassigned.
        /// </summary>
        public GroupingFieldBag ConnectorGroupingField { get; set; }

        /// <summary>
        /// Gets or sets the updated detail panel payload for the connection request.
        /// </summary>
        public ConnectionRequestDetailBox DetailBox { get; set; }
    }
}
