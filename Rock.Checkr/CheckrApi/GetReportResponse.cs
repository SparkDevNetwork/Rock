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
using Newtonsoft.Json;

namespace Rock.Checkr.CheckrApi
{
    /// <summary>
    /// JSON return structure for the Get Report API Call's Response
    /// </summary>
    internal class GetReportResponse
    {
        /// <summary>
        /// Gets or sets the report ID.
        /// </summary>
        /// <value>
        /// The report ID.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the package.
        /// </summary>
        /// <value>
        /// The package.
        /// </value>
        [JsonProperty( "package" )]
        public string Package { get; set; }

        /// <summary>
        /// Gets or sets the candidate identifier.
        /// </summary>
        /// <value>
        /// The candidate identifier.
        /// </value>
        [JsonProperty( "candidate_id" )]
        public string CandidateId { get; set; }

        /// <summary>
        /// Gets or sets the document IDs.
        /// </summary>
        /// <value>
        /// The document IDs.
        /// </value>
        [JsonProperty( "document_ids" )]
        public List<string> DocumentIds { get; set; }
    }
}