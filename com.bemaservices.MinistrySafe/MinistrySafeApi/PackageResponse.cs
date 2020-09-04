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
using Newtonsoft.Json;

namespace com.bemaservices.MinistrySafe.MinistrySafeApi
{
    /// <summary>
    /// JSON return structure for the create candidate API call's response.
    /// </summary>
    internal class PackageResponse
    {
        /// <summary>
        /// Gets or sets the candidate ID.
        /// </summary>
        /// <value>
        /// The candidate ID.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        [JsonProperty( "name" )]
        public string Name { get; set; }

        [JsonProperty( "code" )]
        public string Code { get; set; }

        [JsonProperty( "price" )]
        public decimal Price { get; set; }
   
        public int? Level { get; set; }
    }
}