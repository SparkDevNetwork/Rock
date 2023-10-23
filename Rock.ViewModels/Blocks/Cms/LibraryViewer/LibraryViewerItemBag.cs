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

using Rock.Enums.Cms;
using Rock.ViewModels.Utility;
using System;

namespace Rock.ViewModels.Blocks.Cms.LibraryViewer
{
    /// <summary>
    /// Bag containing Library Viewer item information.
    /// </summary>
    public class LibraryViewerItemBag
    {
        /// <summary>
        /// Gets or sets the name of the author.
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets the downloads.
        /// </summary>
        public int Downloads { get; set; }

        /// <summary>
        /// Gets or sets the experience level.
        /// </summary>
        public ListItemBag ExperienceLevel { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the HTML content.
        /// <para>Only sent when item details are retrieved or when item is downloaded.</para>
        /// </summary>
        public string HtmlContent { get; set; }

        /// <summary>
        /// Gets or sets the image download URL.
        /// </summary>
        public string ImageDownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is downloaded for a specific channel.
        /// </summary>
        public bool IsDownloaded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is new.
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is popular.
        /// </summary>
        public bool IsPopular { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is trending.
        /// </summary>
        public bool IsTrending { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is uploaded for a specific channel.
        /// </summary>
        public bool IsUploaded { get; set; }

        /// <summary>
        /// Gets or sets the license type.
        /// </summary>
        public ListItemBag LicenseType { get; set; }

        /// <summary>
        /// Gets or sets the published date time.
        /// </summary>
        public DateTime? PublishedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the source publisher.
        /// </summary>
        public string SourcePublisherName { get; set; }

        /// <summary>
        /// Gets or sets the structured content.
        /// <para>Only sent when item is downloaded.</para>
        /// </summary>
        public string StructuredContent { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the topic.
        /// </summary>
        public ListItemBag Topic { get; set; }

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        /// <value>
        /// The warning message.
        /// </value>
        public string WarningMessage { get; set; }
    }
}
