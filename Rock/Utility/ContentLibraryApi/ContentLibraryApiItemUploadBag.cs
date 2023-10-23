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
using Rock.Enums.Cms;

namespace Rock.Utility.ContentLibraryApi
{
    /// <summary>
    /// Bag containing required information to upload a Content Library item.
    /// </summary>
    public class ContentLibraryApiItemUploadBag
    {
        /// <summary>
        /// Gets or sets the name of the author.
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets the HTML content.
        /// </summary>
        public string ContentHtml { get; set; }

        /// <summary>
        /// Gets or sets the structured content.
        /// <para><see cref="IsStructuredContentIncluded"/> must be set to <c>true</c> when uploading/updating structured content.</para>
        /// </summary>
        public string ContentStructured { get; set; }

        /// <summary>
        /// The content topic unique identifier.
        /// </summary>
        public Guid ContentTopicGuid { get; set; }

        /// <summary>
        /// Gets or sets the experience level.
        /// </summary>
        public ContentLibraryItemExperienceLevel ExperienceLevel { get; set; }

        /// <summary>
        /// Gets or sets the image download URL.
        /// </summary>
        public string ImageDownloadUrl { get; set; }

        /// <summary>
        /// Indicates whether structured content data is included in the upload.
        /// </summary>
        public bool IsStructuredContentIncluded { get; set; }

        /// <summary>
        /// Gets or sets the license type value unique identifier.
        /// </summary>
        public Guid LicenseTypeValueGuid { get; set; }

        /// <summary>
        /// The key of the organization uploading the Content Library item.
        /// </summary>
        public string OrganizationKey { get; set; }

        /// <summary>
        /// Gets or sets the ContentChannelItem unique identifier in the source system.
        /// </summary>
        public Guid SourceIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the source publisher.
        /// </summary>
        public string SourcePublisherName { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }
    }
}
