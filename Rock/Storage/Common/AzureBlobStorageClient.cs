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
using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.Net.Http;

namespace Rock.Storage.Common
{
    /// <summary>
    /// Azure Blob Storage Client Singleton
    /// </summary>
    internal sealed class AzureBlobStorageClient
    {
        /// <summary>
        /// Gets the client singleton instance.
        /// </summary>
        public static AzureBlobStorageClient Instance => _instance;

        /// <summary>
        /// The client singleton instance.
        /// </summary>
        private static readonly AzureBlobStorageClient _instance = new AzureBlobStorageClient();

        /// <summary>
        /// Shared <see cref="HttpClient"/>.  All Azure Clients should use this.
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// Dictionary for cached <see cref="BlobContainerClient"/>s.  Users should have a limited number of containers, so we will keep them in memory as they are used.
        /// </summary>
        private Dictionary<int, BlobContainerClient> _containerClients = new Dictionary<int, BlobContainerClient>();

        /// <summary>
        /// Gets a <see cref="BlobClient"/> for a specific Blob.
        /// </summary>
        /// <param name="accountName">The Azure Storage Account Name</param>
        /// <param name="accountKey">The Azure Storage Account Key</param>
        /// <param name="customDomain">The (optional) custom domain name of the Azure Storage Account.</param>
        /// <param name="containerName">The name of the Azure Blob Container.</param>
        /// <param name="blobName">The name of the Azure Blob.</param>
        /// <returns></returns>
        public BlobClient GetBlobClient( string accountName, string accountKey, string customDomain, string containerName, string blobName )
        {
            _httpClient = _httpClient ?? new HttpClient();

            var hashKey = ( accountName + accountKey + customDomain + containerName ).GetHashCode();
            if ( !_containerClients.ContainsKey( hashKey ) )
            {
                var connectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey}";
                if ( !string.IsNullOrWhiteSpace( customDomain ) )
                {
                    connectionString = $"{connectionString};BlobEndpoint={customDomain}";
                }

                // use shared HttpClient for all container clients.
                var clientOptions = new BlobClientOptions
                {
                    Transport = new Azure.Core.Pipeline.HttpClientTransport( _httpClient )
                };

                var containerClient = new BlobContainerClient( connectionString, containerName, clientOptions );
                _containerClients.Add( hashKey, containerClient );
            }

            return _containerClients[hashKey].GetBlobClient( blobName );
        }

        /// <summary>
        /// Private (singleton) constructor.
        /// </summary>
        private AzureBlobStorageClient()
        {
        }
    }
}