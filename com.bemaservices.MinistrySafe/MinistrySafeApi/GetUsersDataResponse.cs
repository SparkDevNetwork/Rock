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
using Newtonsoft.Json;

namespace com.bemaservices.MinistrySafe.MinistrySafeApi
{
    /// <summary>
    /// JSON return structure for the get packages API call's response.
    /// </summary>
    internal class GetUsersDataResponse
    {
        /// <summary>
        /// Gets or sets the slugified version of the package name.
        /// </summary>
        /// <value>
        /// The slugified package name.
        /// </value>
        [JsonProperty( "id" )]
        public int? Id { get; set; }

        [JsonProperty( "first_name" )]
        public string FirstName { get; set; }

        [JsonProperty( "last_name" )]
        public string LastName { get; set; }

        [JsonProperty( "external_id" )]
        public string PersonAliasId { get; set; }

        [JsonProperty( "score" )]
        public int? Score { get; set; }

        [JsonProperty( "user_type" )]
        public string UserType { get; set; }

        [JsonProperty( "direct_login_url" )]
        public string DirectLoginUrl { get; set; }

        [JsonProperty( "complete_date" )]
        public string CompletedDateTime { get; set; }
    }
}
