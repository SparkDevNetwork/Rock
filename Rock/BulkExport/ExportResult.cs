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
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Rock.BulkExport
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ExportResult
    {
        /// <summary>
        /// Gets or sets the page (1 based) that is included in this export
        /// </summary>
        /// <value>
        /// The page.
        /// </value>
        [JsonProperty( Order = -4 )]
        [DataMember]
        public int Page { get; set; }

        /// <summary>
        /// The PageSize that was specified
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        [JsonProperty( Order = -3 )]
        [DataMember]
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of records (all pages)
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        [JsonProperty( Order = -2 )]
        [DataMember]
        public int TotalCount { get; set; }
    }
}
