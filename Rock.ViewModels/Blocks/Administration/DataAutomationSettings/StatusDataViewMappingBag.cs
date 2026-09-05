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

namespace Rock.ViewModels.Blocks.Administration.DataAutomationSettings
{
    /// <summary>
    /// Represents a single row that maps a status (a connection status or a
    /// family status defined value) to the data view that determines which
    /// people or families should receive that status.
    /// </summary>
    public class StatusDataViewMappingBag
    {
        /// <summary>
        /// Gets or sets the status defined value being mapped. The value is the
        /// defined value unique identifier and the text is its display name.
        /// This is read-only in the UI and identifies the row on save.
        /// </summary>
        public ListItemBag Status { get; set; }

        /// <summary>
        /// Gets or sets the data view selected for this status, or <c>null</c>
        /// when no data view has been chosen.
        /// </summary>
        public ListItemBag DataView { get; set; }
    }
}
