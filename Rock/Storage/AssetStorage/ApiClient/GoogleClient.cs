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
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1;
using static Google.Apis.Storage.v1.ObjectsResource;

namespace Rock.Storage.AssetStorage.ApiClient
{
    /// <summary>
    /// It is necessary to create this custom Google client for the Google Cloud Asset Storage Provider because the earliest NuGet package
    /// "Google.Cloud.Storage.V1" version available (1.0.0) requires Newtonsoft.Json 10. At this time we are not ready to advance our dependency
    /// version from 9 to 10. This client implementation attempts to keep the same or very similar method signatures to the NuGet package so
    /// that replacing this GoogleClient in the future with the Google supported solution should be simple. This type is also marked "internal"
    /// so that it can be easily removed without concern of references in 3rd party plugin code.
    /// </summary>
    internal class GoogleClient : IDisposable
    {
        /// <summary>
        /// The storage service
        /// </summary>
        private StorageService _storageService;

        /// <summary>
        /// The Google credential
        /// </summary>
        private GoogleCredential _googleCredential;

        /// <summary>
        /// The client's scope of control on the API
        /// </summary>
        private static readonly string[] _scopes = new[] { StorageService.Scope.DevstorageFullControl };

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleClient"/> class.
        /// </summary>
        /// <param name="accountKeyJson">The account key JSON.</param>
        public GoogleClient( string accountKeyJson )
        {
            _googleCredential = GoogleCredential.FromJson( accountKeyJson ).CreateScoped( _scopes );
            _storageService = new StorageService();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _storageService.Dispose();
        }

        /// <summary>
        /// Lists objects in the bucket whose name does not contain the delimiter
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="prefix"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public List<Google.Apis.Storage.v1.Data.Object> ListObjects( string bucketName, string prefix, string delimiter )
        {
            var request = _storageService.Objects.List( bucketName );
            request.OauthToken = GetOauthToken();
            request.Delimiter = delimiter;
            request.Prefix = prefix == "/" ? null : prefix;
            var objects = new List<Google.Apis.Storage.v1.Data.Object>();

            do
            {
                var response = request.Execute();

                if ( response.Items != null )
                {
                    objects.AddRange( response.Items );
                }

                request.PageToken = response.NextPageToken;
            } while ( !request.PageToken.IsNullOrWhiteSpace() );

            return objects;
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="objectName">The key.</param>
        /// <returns></returns>
        public Google.Apis.Storage.v1.Data.Object GetObject( string bucketName, string objectName )
        {
            var request = _storageService.Objects.Get( bucketName, objectName );
            request.OauthToken = GetOauthToken();
            var response = request.Execute();
            return response;
        }

        /// <summary>
        /// Downloads the object.
        /// </summary>
        /// <param name="source">The Google object.</param>
        /// <param name="destination">To stream.</param>
        public void DownloadObject( Google.Apis.Storage.v1.Data.Object source, Stream destination )
        {
            var request = _storageService.Objects.Get( source.Bucket, source.Name );
            request.OauthToken = GetOauthToken();
            request.Download( destination );
        }

        /// <summary>
        /// Uploads the object.
        /// </summary>
        /// <param name="googleObject">The Google object.</param>
        /// <param name="source">From stream.</param>
        public void UploadObject( Google.Apis.Storage.v1.Data.Object googleObject, Stream source )
        {
            var body = new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = googleObject.Bucket,
                Name = googleObject.Name,
                ContentType = googleObject.ContentType
            };

            var request = new InsertMediaUpload( _storageService, body, googleObject.Bucket, source, googleObject.ContentType );
            request.OauthToken = GetOauthToken();
            request.Upload();

            var finalProgress = request.GetProgress();

            if ( finalProgress.Exception != null )
            {
                throw finalProgress.Exception;
            }
        }

        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <param name="googleObject">The Google object.</param>
        public void DeleteObject( Google.Apis.Storage.v1.Data.Object googleObject )
        {
            var request = _storageService.Objects.Delete( googleObject.Bucket, googleObject.Name );
            request.Generation = googleObject.Generation;
            request.OauthToken = GetOauthToken();
            request.Execute();
        }

        /// <summary>
        /// Get the Oauth Token using the service credential
        /// </summary>
        /// <returns></returns>
        private string GetOauthToken()
        {
            // This needs to be run synchronously because otherwise this error will occur in the Asset Manager UI block:
            // An asynchronous operation cannot be started at this time. Asynchronous operations may only be started within
            // an asynchronous handler or module or during certain events in the Page lifecycle.
            return Task.Run( () => _googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync() ).Result;
        }
    }
}
