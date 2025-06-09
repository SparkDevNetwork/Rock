﻿// <copyright>
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
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rock.Rest.Utility
{
    /// <summary>
    /// Picks the correct <see cref="JsonMediaTypeFormatter"/> depending on
    /// which version of the API is being requested.
    /// </summary>
    /// <seealso cref="System.Net.Http.Formatting.JsonMediaTypeFormatter" />
    public class ApiPickerJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        /// <summary>
        /// Gets the formatter that should be used for v1 endpoints.
        /// </summary>
        /// <value>
        /// The API v2 formatter.
        /// </value>
        internal JsonMediaTypeFormatter ApiV1Formatter { get; }

        /// <summary>
        /// Gets the formatter that should be used for /api/v2 endpoints.
        /// </summary>
        /// <value>
        /// The API v2 formatter.
        /// </value>
        internal JsonMediaTypeFormatter ApiV2Formatter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiPickerJsonMediaTypeFormatter"/> class.
        /// </summary>
        public ApiPickerJsonMediaTypeFormatter()
        {
            // Configure the v1 formatter.
            ApiV1Formatter = new Rock.Utility.RockJsonMediaTypeFormatter();

            // Change DateTimeZoneHandling to Unspecified instead of the default of RoundTripKind since Rock doesn't store dates in a timezone aware format
            // So, since Rock doesn't do TimeZones, we don't want Transmission of DateTimes to specify TimeZone either.
            ApiV1Formatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;

            // Configure the v2 formatter.
            ApiV2Formatter = CreateV2Formatter<ExcludeNavigationPropertiesContractResolver>();
        }

        /// <inheritdoc/>
        public override MediaTypeFormatter GetPerRequestFormatterInstance( Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType )
        {
            if ( request.RequestUri.AbsolutePath.StartsWith( "/api/v2/", StringComparison.OrdinalIgnoreCase ) )
            {
                return ApiV2Formatter.GetPerRequestFormatterInstance( type, request, mediaType );
            }
            else
            {
                return ApiV1Formatter.GetPerRequestFormatterInstance( type, request, mediaType );
            }
        }

        /// <summary>
        /// Creates a new formatter that can be used to format v2 API responses.
        /// </summary>
        /// <returns>A new instance of <see cref="JsonMediaTypeFormatter"/>.</returns>
        internal static JsonMediaTypeFormatter CreateV2Formatter()
        {
            return CreateV2Formatter<ExcludeNavigationPropertiesContractResolver>();
        }

        /// <summary>
        /// Creates a new v2 formatter instance with the specified resolver.
        /// </summary>
        /// <typeparam name="TResolver">The type of the contract resolver.</typeparam>
        /// <returns>A new instance of <see cref="JsonMediaTypeFormatter"/>.</returns>
        private static JsonMediaTypeFormatter CreateV2Formatter<TResolver>()
            where TResolver : DefaultContractResolver, new()
        {
            var formatter = new JsonMediaTypeFormatter();

            formatter.SerializerSettings.ContractResolver = new TResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    // Do not process dictionaries, this messes up attribute keys
                    // and generally with a dictionary they are specifying a specific
                    // key that it should be anyway.
                    ProcessDictionaryKeys = false,
                    OverrideSpecifiedNames = true
                }
            };

            formatter.SerializerSettings.Converters.Add( new RockOrganizationDateTimeJsonConverter() );

            return formatter;
        }
    }
}
