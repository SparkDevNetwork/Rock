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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;

using Rock.Web.Cache;

namespace Rock.Mobile
{
    /// <summary>
    /// Builds the <c>campuses</c> array the church sends to the church directory for the
    /// shared mobile application, and the hash that decides whether it needs sending again.
    /// </summary>
    /// <remarks>
    /// The hash is taken over exactly the serialized array that is sent and nothing else, so
    /// a change to a campus field the directory never sees (a street line, a phone number)
    /// does not trigger a post, and every change to a field it does see does. Enable, Update
    /// and the daily campus job all use this one class, so the hashed bytes and the sent
    /// bytes cannot drift apart.
    /// </remarks>
    internal static class PlatformMobileAppCampusPayload
    {
        /// <summary>
        /// The serializer settings for the array. Used for both the request body and the hash.
        /// </summary>
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None
        };

        /// <summary>
        /// Gets the active campuses in the shape the church directory expects, in a fixed order:
        /// by the campus's order, then by its identifier. The identifier is never sent; it only
        /// breaks ties so two runs over the same data always produce the same bytes.
        /// </summary>
        /// <returns>The campus entries.</returns>
        public static List<PlatformMobileAppCampus> GetCampuses()
        {
            return CampusCache.All()
                .Where( c => c.IsActive ?? true )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Id )
                .Select( ToPayloadCampus )
                .ToList();
        }

        /// <summary>
        /// Serializes the campus entries exactly as they are sent.
        /// </summary>
        /// <param name="campuses">The campus entries.</param>
        /// <returns>The JSON array.</returns>
        public static string Serialize( List<PlatformMobileAppCampus> campuses )
        {
            return JsonConvert.SerializeObject( campuses, SerializerSettings );
        }

        /// <summary>
        /// Computes the hash of the campus entries: SHA-256 over the serialized array, as a
        /// lowercase hex string. Compared with the value stored under
        /// <see cref="SystemKey.MetadataKey.PlatformMobileAppCampusHash"/>.
        /// </summary>
        /// <param name="campuses">The campus entries.</param>
        /// <returns>The hash.</returns>
        public static string ComputeHash( List<PlatformMobileAppCampus> campuses )
        {
            using ( var sha256 = SHA256.Create() )
            {
                var bytes = sha256.ComputeHash( Encoding.UTF8.GetBytes( Serialize( campuses ) ) );

                return string.Concat( bytes.Select( b => b.ToString( "x2" ) ) );
            }
        }

        /// <summary>
        /// Maps a campus to its directory entry. Coordinates are sent only as a pair, and a
        /// campus that has not been geocoded yet is still included, without them.
        /// </summary>
        /// <param name="campus">The campus.</param>
        /// <returns>The directory entry.</returns>
        private static PlatformMobileAppCampus ToPayloadCampus( CampusCache campus )
        {
            var location = campus.Location;
            var hasCoordinates = location?.Latitude != null && location.Longitude != null;

            return new PlatformMobileAppCampus
            {
                Name = campus.Name,
                City = location?.City.IsNotNullOrWhiteSpace() == true ? location.City : null,
                State = location?.State.IsNotNullOrWhiteSpace() == true ? location.State : null,
                Latitude = hasCoordinates ? location.Latitude : null,
                Longitude = hasCoordinates ? location.Longitude : null
            };
        }
    }

    /// <summary>
    /// One campus as the church directory stores it for the shared mobile application.
    /// </summary>
    internal class PlatformMobileAppCampus
    {
        /// <summary>
        /// Gets or sets the campus name.
        /// </summary>
        [JsonProperty( "name", Order = 1 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the campus city, if known.
        /// </summary>
        [JsonProperty( "city", Order = 2 )]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the campus state, if known.
        /// </summary>
        [JsonProperty( "state", Order = 3 )]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the campus latitude. Omitted unless the longitude is also known.
        /// </summary>
        [JsonProperty( "lat", Order = 4 )]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the campus longitude. Omitted unless the latitude is also known.
        /// </summary>
        [JsonProperty( "long", Order = 5 )]
        public double? Longitude { get; set; }
    }
}
