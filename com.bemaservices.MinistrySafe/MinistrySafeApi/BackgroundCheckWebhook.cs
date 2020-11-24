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
using Newtonsoft.Json;

namespace com.bemaservices.MinistrySafe.MinistrySafeApi
{
    /// <summary>
    /// Invitation webhook
    /// </summary>
    /// <seealso cref="com.bemaservices.MinistrySafe.MinistrySafeApi.BackgroundCheckWebhook" />
    internal class BackgroundCheckWebhook
    {
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

        [JsonProperty( "external_id" )]
        public int? ExternalId { get; set; }

        [JsonProperty( "level" )]
        public int? Level { get; set; }

        [JsonProperty( "custom_background_check_package_code" )]
        public string CustomBackgroundCheckPackageCode { get; set; }
    }
}
