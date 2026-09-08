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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.RoomList
{
    /// <summary>
    /// The response bag returned from the Room List block's GetGridData block
    /// action. Wraps the grid data with the pieces of block-level UI state that
    /// need to be refreshed alongside the grid: panel title, column visibility,
    /// and any warning/redirect surface.
    /// </summary>
    public class RoomListGridDataBag
    {
        /// <summary>
        /// Gets or sets the grid data (rows + definition) for the response.
        /// Null when a warning message or a redirect URL is being returned in
        /// place of grid data.
        /// </summary>
        public GridDataBag GridData { get; set; }

        /// <summary>
        /// Gets or sets a warning message that should be displayed in place of
        /// the grid.
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets a fully-qualified URL that the browser should be
        /// redirected to. Used when the block cannot resolve a check-in area
        /// and an Area Select Page has been configured.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the panel title that should be displayed above the
        /// grid for this response.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Room column should be
        /// shown.
        /// </summary>
        public bool ShowRoomColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Checked-in column
        /// should be shown for this response.
        /// </summary>
        public bool ShowCheckedInCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Checked-out column
        /// should be shown for this response.
        /// </summary>
        public bool ShowCheckedOutCount { get; set; }

        /// <summary>
        /// Gets or sets the header text for the Present column for this
        /// response.
        /// </summary>
        public string PresentColumnHeader { get; set; }
    }
}
