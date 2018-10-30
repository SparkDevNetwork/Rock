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

namespace Rock.Checkr.CheckrApi
{
    /// <summary>
    /// JSON return structure for the get packages API call's response.
    /// </summary>
    internal class GetPackagesDataResponse
    {
        /// <summary>
        /// Gets or sets the slugified version of the package name.
        /// </summary>
        /// <value>
        /// The slugified package name.
        /// </value>
        [JsonProperty( "slug" )]
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the package name.
        /// </summary>
        /// <value>
        /// The package name.
        /// </value>
        [JsonProperty( "name" )]
        public string Name { get; set; }
    }
}
