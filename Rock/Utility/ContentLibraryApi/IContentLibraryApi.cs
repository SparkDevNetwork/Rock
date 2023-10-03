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

namespace Rock.Utility.ContentLibraryApi
{
    /// <summary>
    /// Represents an interface to a Content Library API.
    /// </summary>
    public interface IContentLibraryApi
    {
        /// <summary>
        /// Downloads an item from the Content Library.
        /// <para>Use <seealso cref="GetItem(Guid)"/> if retrieving item details without the intent to download a copy into the Rock instance.</para>
        /// </summary>
        /// <param name="contentLibraryItemGuid">The content library item unique identifier.</param>
        /// <param name="contentLibraryItemDownloadBag">The details for the content library item to download.</param>
        /// <returns>The downloaded Content Library item.</returns>
        ContentLibraryApiResult<ContentLibraryApiItemDetailBag> DownloadItem( Guid contentLibraryItemGuid, ContentLibraryApiItemDownloadBag contentLibraryItemDownloadBag );

        /// <summary>
        /// Gets an item from the Content Library.
        /// </summary>
        /// <param name="contentLibraryItemGuid">The content library item unique identifier.</param>
        /// <returns>The Content Library item.</returns>
        ContentLibraryApiResult<ContentLibraryApiItemDetailBag> GetItem( Guid contentLibraryItemGuid );

        /// <summary>
        /// Gets the metadata from the Content Library.
        /// </summary>
        /// <returns>The Content Library metadata.</returns>
        ContentLibraryApiResult<ContentLibraryApiMetadataBox> GetMetadata();

        /// <summary>
        /// Uploads an item to the Content Library.
        /// </summary>
        /// <param name="contentLibraryItemUploadBag">The details for the content library item to upload.</param>
        /// <returns>The uploaded Content Library item.</returns>
        ContentLibraryApiResult<ContentLibraryApiItemDetailBag> UploadItem( ContentLibraryApiItemUploadBag contentLibraryItemUploadBag );
    }
}