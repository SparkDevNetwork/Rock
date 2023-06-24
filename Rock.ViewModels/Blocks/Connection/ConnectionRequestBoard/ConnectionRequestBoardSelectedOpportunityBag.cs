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

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains selected connection opportunity information for the connection request board.
    /// </summary>
    public class ConnectionRequestBoardSelectedOpportunityBag
    {
        /// <summary>
        /// Gets or sets the selected connection opportunity.
        /// </summary>
        public ConnectionRequestBoardConnectionOpportunityBag ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the filter options that may be used to filter connection requests.
        /// </summary>
        public ConnectionRequestBoardFilterOptionsBag FilterOptions { get; set; }

        /// <summary>
        /// Gets or sets the selected filters to be used to initialize the connection request board.
        /// </summary>
        public ConnectionRequestBoardFiltersBag Filters { get; set; }

        /// <summary>
        /// Gets or sets whether connection request adding is enabled.
        /// </summary>
        public bool IsRequestAddingEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether connection request security is enabled.
        /// </summary>
        public bool IsRequestSecurityEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether the board is in card view mode.
        /// </summary>
        public bool IsCardViewMode { get; set; } = true;
    }
}
