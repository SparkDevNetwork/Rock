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

namespace Rock.Utility.ContentLibraryApi
{
    /// <summary>
    /// Extensions for Content Library API.
    /// </summary>
    public static class ContentLibraryApiExtensions
    {
        /// <summary>
        /// Converts a <see cref="ContentLibraryApiItemDetailBag"/> to a <see cref="ContentLibraryApiItemSummaryBag"/>.
        /// </summary>
        /// <param name="apiItemDetailBag">The API item detail bag.</param>
        /// <returns>A converted <see cref="ContentLibraryApiItemSummaryBag"/>.</returns>
        public static ContentLibraryApiItemSummaryBag ToSummaryBag( this ContentLibraryApiItemDetailBag apiItemDetailBag )
        {
            if ( apiItemDetailBag == null )
            {
                return null;
            }

            return new ContentLibraryApiItemSummaryBag
            {
                AuthorName = apiItemDetailBag.AuthorName,
                Downloads = apiItemDetailBag.Downloads,
                ExperienceLevel = apiItemDetailBag.ExperienceLevel,
                Guid = apiItemDetailBag.Guid,
                ImageDownloadUrl = apiItemDetailBag.ImageDownloadUrl,
                IsNew = apiItemDetailBag.IsNew,
                IsPopular = apiItemDetailBag.IsPopular,
                IsTrending = apiItemDetailBag.IsTrending,
                LicenseTypeGuid = apiItemDetailBag.LicenseTypeGuid,
                PublishedDateTime = apiItemDetailBag.PublishedDateTime,
                SourcePublisherName = apiItemDetailBag.SourcePublisherName,
                Summary = apiItemDetailBag.Summary,
                Title = apiItemDetailBag.Title,
                TopicGuid = apiItemDetailBag.TopicGuid,
            };
        }

        /// <summary>
        /// Converts a <see cref="ContentLibraryApiItemSummaryBag"/> to a <see cref="ContentLibraryApiItemDetailBag"/>.
        /// </summary>
        /// <param name="apiItemSummaryBag">The API item summary bag.</param>
        /// <returns>A converted <see cref="ContentLibraryApiItemDetailBag"/>.</returns>
        public static ContentLibraryApiItemDetailBag ToDetailBag( this ContentLibraryApiItemSummaryBag apiItemSummaryBag )
        {
            if ( apiItemSummaryBag == null )
            {
                return null;
            }

            return new ContentLibraryApiItemDetailBag
            {
                AuthorName = apiItemSummaryBag.AuthorName,
                Downloads = apiItemSummaryBag.Downloads,
                ExperienceLevel = apiItemSummaryBag.ExperienceLevel,
                Guid = apiItemSummaryBag.Guid,
                HtmlContent = null,
                ImageDownloadUrl = apiItemSummaryBag.ImageDownloadUrl,
                IsNew = apiItemSummaryBag.IsNew,
                IsPopular = apiItemSummaryBag.IsPopular,
                IsTrending = apiItemSummaryBag.IsTrending,
                LicenseTypeGuid = apiItemSummaryBag.LicenseTypeGuid,
                PublishedDateTime = apiItemSummaryBag.PublishedDateTime,
                SourcePublisherName = apiItemSummaryBag.SourcePublisherName,
                StructuredContent = null,
                Summary = apiItemSummaryBag.Summary,
                Title = apiItemSummaryBag.Title,
                TopicGuid = apiItemSummaryBag.TopicGuid,
            };
        }
    }
}
