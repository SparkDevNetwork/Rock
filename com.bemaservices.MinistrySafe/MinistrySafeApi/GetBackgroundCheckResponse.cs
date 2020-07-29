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
    /// JSON return structure for the create candidate API call's response.
    /// </summary>
    internal class GetBackgroundCheckResponse
    {
        /// <summary>
        /// Gets or sets the candidate ID.
        /// </summary>
        /// <value>
        /// The candidate ID.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        [JsonProperty( "order_date" )]
        public string OrderDate { get; set; }

        [JsonProperty( "status" )]
        public string Status { get; set; }
        
        [JsonProperty( "applicant_interface_url" )]
        public string ApplicantInterfaceUrl { get; set; }

        [JsonProperty( "results_url" )]
        public string ResultsUrl { get; set; }

        [JsonProperty( "user_id" )]
        public int? UserId { get; set; }

        [JsonProperty( "level" )]
        public int? Level { get; set; }
    }
}