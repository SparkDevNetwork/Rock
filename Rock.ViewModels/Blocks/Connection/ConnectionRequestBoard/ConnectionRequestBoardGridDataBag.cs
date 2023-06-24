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

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains grid information for the connection request board.
    /// </summary>
    public class ConnectionRequestBoardGridDataBag
    {
        /// <summary>
        /// Gets or sets the data for the grid.
        /// </summary>
        public GridDataBag Data { get; set; }

        /// <summary>
        /// Gets or sets the definition for the grid.
        /// </summary>
        public GridDefinitionBag Definition { get; set; }

        /// <summary>
        /// Gets or sets the filters that were used to source and build the grid information.
        /// </summary>
        public ConnectionRequestBoardFiltersBag Filters { get; set; }
    }
}
