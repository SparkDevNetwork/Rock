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

using Rock.Model;
using System;

namespace Rock.ViewModels.Blocks.Cms.LibraryViewer
{
    /// <summary>
    /// Bag containing information required to download a Content Library item.
    /// </summary>
    public class LibraryViewerDownloadItemBag
    {
        /// <summary>
        /// Gets or sets the Content Library item unique identifier.
        /// </summary>
        public Guid ContentLibraryItemGuid { get; set; }

        /// <summary>
        /// Gets or sets the start date of the downloaded Content Channel Item.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the downloaded Content Channel Item.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the downloaded Content Channel Item status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public ContentChannelItemStatus? Status { get; set; }
    }
}
