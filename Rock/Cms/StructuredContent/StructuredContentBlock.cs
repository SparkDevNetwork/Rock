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

// NOTE: We only use JsonProperty to handle re-encoding so it uses the right
// casing for JavaScript to understand. If we switch away from Newtonsoft
// we will need to handle that in the new system.
//
// The Data property will automatically be encoded in the original casing
// since it was never fully deserialized.

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// A generic type EditorJS block.
    /// </summary>
    public class StructuredContentBlock
    {
        /// <summary>
        /// Gets or sets the unique identifier of the block.
        /// </summary>
        /// <value>
        /// The unique identifier of the block.
        /// </value>
        [Newtonsoft.Json.JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of block.
        /// </summary>
        /// <value>
        /// The type of block.
        /// </value>
        [Newtonsoft.Json.JsonProperty( "type" )]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [Newtonsoft.Json.JsonProperty( "data" )]
        public dynamic Data { get; set; }
    }
}
