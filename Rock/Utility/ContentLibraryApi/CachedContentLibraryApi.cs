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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web;

namespace Rock.Utility.ContentLibraryApi
{
    /// <summary>
    /// Wraps cache behavior around a <see cref="IContentLibraryApi"/> instance.
    /// </summary>
    /// <seealso cref="Rock.Utility.ContentLibraryApi.IContentLibraryApi" />
    public class CachedContentLibraryApi : IContentLibraryApi
    {
        private readonly IContentLibraryApi _innerContentLibraryApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedContentLibraryApi"/> class which uses a new instance of <see cref="ContentLibraryApi"/> under the hood.
        /// </summary>
        public CachedContentLibraryApi()
            : this( new ContentLibraryApi() )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedContentLibraryApi"/> class.
        /// </summary>
        /// <param name="innerContentLibraryApi">The inner content library API.</param>
        public CachedContentLibraryApi( IContentLibraryApi innerContentLibraryApi )
        {
            _innerContentLibraryApi = innerContentLibraryApi ?? throw new ArgumentNullException( nameof( innerContentLibraryApi ) );
        }

        /// <summary>
        /// Downloads an item from the Content Library, updates the cached item, and returns it.
        /// <para>The code following usage of this method should save a copy of the Content Library item.</para>
        /// <para>Use <seealso cref="GetItem(Guid)"/> if retrieving item details without the intent to download a copy into the Rock instance.</para>
        /// </summary>
        /// <param name="contentLibraryItemGuid">The content library item unique identifier.</param>
        /// <param name="contentLibraryItemDownloadBag">The details for the content library item to download.</param>
        /// <returns>The downloaded Content Library item.</returns>
        public ContentLibraryApiResult<ContentLibraryApiItemDetailBag> DownloadItem( Guid contentLibraryItemGuid, ContentLibraryApiItemDownloadBag contentLibraryItemDownloadBag )
        {
            var result = _innerContentLibraryApi.DownloadItem( contentLibraryItemGuid, contentLibraryItemDownloadBag );

            if ( result?.IsSuccess == true && result.Data != null )
            {
                UpdateCachedItem( result.Data );
            }

            return result;
        }

        /// <summary>
        /// Gets an item from the Content Library and updates the cached item.
        /// </summary>
        /// <param name="contentLibraryItemGuid">The content library item unique identifier.</param>
        /// <returns>The Content Library item.</returns>
        public ContentLibraryApiResult<ContentLibraryApiItemDetailBag> GetItem( Guid contentLibraryItemGuid )
        {
            var result = _innerContentLibraryApi.GetItem( contentLibraryItemGuid );

            if ( result?.IsSuccess == true && result.Data != null )
            {
                UpdateCachedItem( result.Data );
            }

            return result;
        }

        /// <summary>
        /// Gets the metadata from the cache or, if the cached metadata is null or expired, fetches the metadata from the Content Library, updates it in the cache, and returns it.
        /// </summary>
        /// <returns>The Content Library metadata.</returns>
        public ContentLibraryApiResult<ContentLibraryApiMetadataBox> GetMetadata()
        {
            var metadata = ReadFromCache();

            if ( metadata != null )
            {
                return new ContentLibraryApiResult<ContentLibraryApiMetadataBox>
                {
                    Data = metadata,
                    IsSuccess = true
                };
            }

            // The metadata has not been retrieved yet today,
            // so retrieve it from the Content Library.
            var result = _innerContentLibraryApi.GetMetadata();

            SaveToCache( result.RawData );

            return result;
        }

        /// <summary>
        /// Uploads an item to the Content Library, updates the cached item, and returns it.
        /// </summary>
        /// <param name="contentLibraryItemUploadBag">The details for the content library item to upload.</param>
        /// <returns>The uploaded Content Library item.</returns>
        public ContentLibraryApiResult<ContentLibraryApiItemDetailBag> UploadItem( ContentLibraryApiItemUploadBag contentLibraryItemUploadBag )
        {
            var result = _innerContentLibraryApi.UploadItem( contentLibraryItemUploadBag );

            // Update the uploaded library item in the metadata.
            if ( result?.IsSuccess == true && result.Data != null )
            {
                UpdateCachedItem( result.Data );
            }

            return result;
        }

        #region Private Methods

        /// <summary>
        /// Updates a cached item.
        /// </summary>
        /// <param name="apiItemDetailBag">The API item detail bag.</param>
        private void UpdateCachedItem( ContentLibraryApiItemDetailBag apiItemDetailBag )
        {
            if ( apiItemDetailBag == null )
            {
                return;
            }

            var metadataResult = GetMetadata();

            if ( metadataResult?.IsSuccess == true && metadataResult.Data != null )
            {
                var oldItem = metadataResult.Data.Items.FirstOrDefault( i => i.Guid == apiItemDetailBag.Guid );

                if ( oldItem != null )
                {
                    // Only update if the old item exists in the metadata.
                    // Metadata from the Content Library returns a specific set of items.
                    // If our item is not in the metadata, then we should not add it.
                    metadataResult.Data.Items.Remove( oldItem );
                    metadataResult.Data.Items.Add( apiItemDetailBag.ToSummaryBag() );

                    SaveToCache( metadataResult.Data );

                    // Do not update the LastContentLibraryMetadataLoadedDay here.
                    // That should only be updated after retrieving metadata from the Content Library.
                }
            }
        }

        /// <summary>
        /// Reads the <see cref="ContentLibraryApiMetadataBox"/> from the cache.
        /// </summary>
        private ContentLibraryApiMetadataBox ReadFromCache()
        {
            var attributeData = new AttributeService( new RockContext() )
                .GetSystemSettings()
                .Where( a => a.Key == Rock.SystemKey.SystemSetting.CONTENT_LIBRARY_DATA_JSON )
                .Select( a => new
                {
                    LastUpdatedDateTime = a.ModifiedDateTime ?? a.CreatedDateTime
                } )
                .FirstOrDefault();

            if ( attributeData?.LastUpdatedDateTime.HasValue != true )
            {
                // This should not happen, but if there is no modified or created date,
                // then the Content Library metadata should be refetched from the API.
                return null;
            }

            var timeSinceLastUpdate = RockDateTime.Now - attributeData.LastUpdatedDateTime.Value;

            if ( timeSinceLastUpdate >= TimeSpan.FromHours( 24 ) )
            {
                // If the cache hasn't been updated for 24 hours
                // then the Content Library metadata should be refetched from the API.
                return null;
            }

            return SystemSettings.GetValue( Rock.SystemKey.SystemSetting.CONTENT_LIBRARY_DATA_JSON )
                ?.FromJsonOrNull<ContentLibraryApiMetadataBox>();
        }

        /// <summary>
        /// Saves the <see cref="ContentLibraryApiMetadataBox"/> to the cache.
        /// </summary>
        /// <param name="contentLibraryApiMetadataBox">The content library API metadata box.</param>
        private void SaveToCache( ContentLibraryApiMetadataBox contentLibraryApiMetadataBox )
        {
            SaveToCache( contentLibraryApiMetadataBox.ToJson() );
        }

        /// <summary>
        /// Saves raw JSON data to the cache.
        /// </summary>
        /// <param name="rawJsonData">The raw JSON data.</param>
        private void SaveToCache( string rawJsonData )
        {
            // Save to system settings.
            SystemSettings.SetValue( Rock.SystemKey.SystemSetting.CONTENT_LIBRARY_DATA_JSON, rawJsonData );
        }

        #endregion
    }
}
