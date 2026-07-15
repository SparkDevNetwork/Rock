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
using System.Collections.Concurrent;
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
        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Cached <see cref="BlobContainerClient"/>s. A <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// is required here because this singleton is shared across all threads: a plain
        /// <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> could corrupt its internal
        /// bucket chain under concurrent Add calls, after which lookups would spin in FindEntry at
        /// 100% CPU (issue #6919).
        /// </summary>
        private readonly ConcurrentDictionary<int, BlobContainerClient> _containerClients = new ConcurrentDictionary<int, BlobContainerClient>();

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
            var hashKey = ( accountName + accountKey + customDomain + containerName ).GetHashCode();

            // GetOrAdd is atomic on the dictionary. The value factory may execute more than once
            // under high contention, but only one BlobContainerClient is retained; the extras are
            // discarded. BlobContainerClient construction is cheap and side-effect-free, so this
            // is acceptable.
            var containerClient = _containerClients.GetOrAdd( hashKey, _ =>
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

                return new BlobContainerClient( connectionString, containerName, clientOptions );
            } );

            return containerClient.GetBlobClient( blobName );
        }

        /// <summary>
        /// Private (singleton) constructor.
        /// </summary>
        private AzureBlobStorageClient()
        {
        }
    }
}