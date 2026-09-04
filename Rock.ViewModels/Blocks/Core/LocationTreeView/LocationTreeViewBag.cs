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

using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.LocationTreeView
{
    /// <summary>
    /// The runtime data the Location Tree View block ships to its Obsidian component.
    /// </summary>
    public class LocationTreeViewBag
    {
        /// <summary>
        /// Gets or sets the locations selected on load, resolved from the page parameter for deep-linking.
        /// </summary>
        public List<Guid> SelectedLocationGuids { get; set; }

        /// <summary>
        /// Gets or sets the locations to expand on load.
        /// </summary>
        public List<Guid> ExpandedLocationGuids { get; set; }

        /// <summary>
        /// Gets or sets whether the add-location chrome is shown; false when the person lacks EDIT on the block.
        /// </summary>
        public bool IsAddLocationVisible { get; set; }

        /// <summary>
        /// Gets or sets whether the Add Top-Level action is enabled.
        /// </summary>
        public bool IsAddRootEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether the Add Child To Selected action is enabled for the current selection.
        /// </summary>
        public bool IsAddChildEnabled { get; set; }

        /// <summary>
        /// Gets or sets a URL the client should navigate to immediately when no location is selected
        /// and a first top-level named location is available. Null when no redirect is needed.
        /// </summary>
        public string AutoSelectUrl { get; set; }

        /// <summary>
        /// Gets or sets an error message to display without replacing the tree.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
