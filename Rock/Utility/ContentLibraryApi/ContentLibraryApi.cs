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

using RestSharp;
using System;
using System.Configuration;
using System.Net;

using Rock.Attribute;
using Rock.Logging;

namespace Rock.Utility.ContentLibraryApi
{
    /// <summary>
    /// API Calls to Content Library server. 
    /// </summary>
    [RockInternal( "1.16" )]
    public class ContentLibraryApi : IContentLibraryApi
    {
        /// <inheritdoc/>
        public ContentLibraryApiResult<ContentLibraryApiMetadataBox> GetMetadata()
        {
            var client = GetRestClient();
            var request = new RestRequest( "metadata", Method.GET );

            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK )
            {
                return SuccessResult<ContentLibraryApiMetadataBox>( response.Content );
            }

            // Log the error response.
            RockLogger.Log.Error( RockLogDomains.Cms, "Received {Error} and {StatusCode} when getting metadata from Content Library.", response.Content, response.StatusCode );

            if ( response.StatusCode == HttpStatusCode.BadRequest )
            {
                // If a bad request was sent,
                // then we want to return the error.
                return FailureResult<ContentLibraryApiMetadataBox>( response.Content );
            }

            // Do not expose the error in the response unless it was a 400 (bad request).
            return FailureResult<ContentLibraryApiMetadataBox>();
        }

        /// <inheritdoc/>
        public ContentLibraryApiResult<ContentLibraryApiItemDetailBag> GetItem( Guid contentLibraryItemGuid )
        {
            var client = GetRestClient();
            var request = new RestRequest( $"items/{contentLibraryItemGuid}", Method.GET );

            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK )
            {
                return SuccessResult<ContentLibraryApiItemDetailBag>( response.Content );
            }

            // Log the error response.
            RockLogger.Log.Error( RockLogDomains.Cms, "Received {Error} and {StatusCode} when getting {ContentLibraryItemGuid} from Content Library.", response.Content, response.StatusCode, contentLibraryItemGuid );

            if ( response.StatusCode == HttpStatusCode.BadRequest )
            {
                // If a bad request was sent,
                // then we want to return the error.
                return FailureResult<ContentLibraryApiItemDetailBag>( response.Content );
            }

            // Do not expose the error in the response unless it was a 400 (bad request).
            return FailureResult<ContentLibraryApiItemDetailBag>();
        }

        /// <inheritdoc/>
        public ContentLibraryApiResult<ContentLibraryApiItemDetailBag> DownloadItem( Guid contentLibraryItemGuid, ContentLibraryApiItemDownloadBag contentLibraryItemDownloadBag )
        {
            var client = GetRestClient();
            var request = new RestRequest( $"items/{contentLibraryItemGuid}/downloads", Method.POST )
            {
                RequestFormat = DataFormat.Json
            };
            request.AddBody( contentLibraryItemDownloadBag );

            // Download the item.
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK )
            {
                return SuccessResult<ContentLibraryApiItemDetailBag>( response.Content );
            }

            // Log the error response.
            RockLogger.Log.Error( RockLogDomains.Cms, "Received {Error} and {StatusCode} when downloading {ContentLibraryItemGuid} from Content Library.", response.Content, response.StatusCode, contentLibraryItemGuid );

            if ( response.StatusCode == HttpStatusCode.BadRequest )
            {
                // If a bad request was sent,
                // then we want to return the error.
                return FailureResult<ContentLibraryApiItemDetailBag>( response.Content );
            }

            // Do not expose the error in the response unless it was a 400 (bad request).
            return FailureResult<ContentLibraryApiItemDetailBag>();
        }

        /// <inheritdoc/>
        public ContentLibraryApiResult<ContentLibraryApiItemDetailBag> UploadItem( ContentLibraryApiItemUploadBag contentLibraryItemUploadBag )
        {
            var client = GetRestClient();
            var request = new RestRequest( "items", Method.POST )
            {
                RequestFormat = DataFormat.Json
            };
            request.AddBody( contentLibraryItemUploadBag );

            // Upload the item.
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK )
            {
                return SuccessResult<ContentLibraryApiItemDetailBag>( response.Content );
            }

            // Log the error response.
            RockLogger.Log.Error( RockLogDomains.Cms, "Received {Error} and {StatusCode} when uploading {ContentChannelItemGuid} to Content Library.", response.Content, response.StatusCode, contentLibraryItemUploadBag.SourceIdentifier );

            if ( response.StatusCode == HttpStatusCode.BadRequest )
            {
                // If a bad request was sent,
                // then we want to return the errors.
                return FailureResult<ContentLibraryApiItemDetailBag>( response.Content );
            }

            // Do not expose the error in the response unless it was a 400 (bad request).
            return FailureResult<ContentLibraryApiItemDetailBag>();
        }

        #region Private Methods

        /// <summary>
        /// Gets a rest client for the Content Library API.
        /// </summary>
        private RestClient GetRestClient()
        {
            return new RestClient( $"{ ConfigurationManager.AppSettings["SparkApiUrl"].EnsureTrailingForwardslash() }api/org.sparkdevnetwork/ContentLibrary" );
        }

        /// <summary>
        /// Creates a failure result.
        /// </summary>
        /// <typeparam name="T">The type of result.</typeparam>
        /// <param name="error">The error.</param>
        private ContentLibraryApiResult<T> FailureResult<T>( string error = null )
        {
            return new ContentLibraryApiResult<T>
            {
                Error = error
            };
        }

        /// <summary>
        /// Creates a success result.
        /// </summary>
        /// <typeparam name="T">The type of result.</typeparam>
        /// <param name="rawData">The raw data.</param>
        private ContentLibraryApiResult<T> SuccessResult<T>( string rawData = null )
        {
            return new ContentLibraryApiResult<T>
            {
                Data = rawData == null ? default( T ) : rawData.FromJsonOrNull<T>(),
                RawData = rawData,
                IsSuccess = true
            };
        }

        #endregion
    }
}
