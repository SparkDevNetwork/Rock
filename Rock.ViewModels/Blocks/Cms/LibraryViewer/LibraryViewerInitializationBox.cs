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

namespace Rock.ViewModels.Blocks.Cms.LibraryViewer
{
    /// <summary>
    /// A box that contains the information required to render the Obsidian Library Viewer block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class LibraryViewerInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the content channel identifier key.
        /// </summary>
        public string ContentChannelIdKey { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<LibraryViewerItemBag> Items { get; set; }

        /// <summary>
        /// Gets or sets the validation results.
        /// </summary>
        /// <value>
        /// The validation results.
        /// </value>
        public LibraryViewerValidationResultsBag ValidationResults { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether download date is shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if download date is shown; otherwise, <c>false</c>.
        /// </value>
        public bool IsDownloadDateShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether download date is shown as a date range.
        /// </summary>
        /// <value>
        ///   <c>true</c> if download date is shown as a date range; otherwise, <c>false</c>.
        /// </value>
        public bool IsDownloadDateShownAsDateRange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether download date is shown with time.
        /// </summary>
        /// <value>
        ///   <c>true</c> if download date is shown with time; otherwise, <c>false</c>.
        /// </value>
        public bool IsDownloadDateShownWithTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether download status is shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if download status is shown; otherwise, <c>false</c>.
        /// </value>
        public bool IsDownloadStatusShown { get; set; }
    }
}
