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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.EnRoute
{
    /// <summary>
    /// The response wrapper returned by the GetGridData block action.
    /// Contains the grid data along with any error or warning state.
    /// </summary>
    public class EnRouteGridDataBag
    {
        /// <summary>
        /// Gets or sets the grid data containing the attendee rows.
        /// </summary>
        public GridDataBag GridData { get; set; }

        /// <summary>
        /// Gets or sets a warning message to display instead of the grid
        /// (e.g. when no campus is selected).
        /// </summary>
        public string WarningMessage { get; set; }
    }
}
