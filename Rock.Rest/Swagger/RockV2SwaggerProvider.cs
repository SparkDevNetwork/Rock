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
using System.Collections.Concurrent;

using Rock.Rest.Utility;

using Swashbuckle.Swagger;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// Swagger document provider that supports caching.
    /// </summary>
    internal class RockV2SwaggerProvider : ISwaggerProvider
    {
        /// <summary>
        /// The base provider that will do the real work if a cached document
        /// is not found.
        /// </summary>
        private readonly ISwaggerProvider _baseProvider;

        /// <summary>
        /// The cached API documents.
        /// </summary>
        private static readonly ConcurrentDictionary<string, SwaggerDocument> _cachedDocuments = new ConcurrentDictionary<string, SwaggerDocument>();

        /// <summary>
        /// Creates a new instance of <see cref="RockV2SwaggerProvider"/> that
        /// handles caching of the actual API document.
        /// </summary>
        /// <param name="baseProvider">The base provider that does the real work.</param>
        public RockV2SwaggerProvider( ISwaggerProvider baseProvider )
        {
            _baseProvider = baseProvider;

            var formatter = ApiPickerJsonMediaTypeFormatter.CreateV2Formatter();
            var jsonSerializerSettingsField = baseProvider.GetType().GetField( "_jsonSerializerSettings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );

            jsonSerializerSettingsField?.SetValue( baseProvider, formatter.SerializerSettings );
        }

        /// <summary>
        /// Gets the swagger document that contains the API endpoints for the
        /// given API version.
        /// </summary>
        /// <param name="rootUrl">The root URL path to use for constructing the document.</param>
        /// <param name="apiVersion">The API version to be retrieved.</param>
        /// <returns>The <see cref="SwaggerDocument"/> for this API version.</returns>
        public SwaggerDocument GetSwagger( string rootUrl, string apiVersion )
        {
            var cacheKey = $"{rootUrl}+{apiVersion}";

            return _cachedDocuments.GetOrAdd( cacheKey, _ =>
            {
                var document = _baseProvider.GetSwagger( rootUrl, apiVersion );

                if ( document.basePath == null )
                {
                    document.basePath = "/";
                }

                return document;
            } );
        }
    }
}
