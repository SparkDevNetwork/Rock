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
using System.Runtime.Serialization;

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The data used by the <see cref="TableRenderer"/> block type.
    /// </summary>
    [DataContract]
    public class TableData
    {
        /// <summary>
        /// Gets or sets the content as a collection of rows, each with a
        /// collection of column values.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [DataMember( Name = "content" )]
        public List<List<string>> Content { get; set; }

        /// <summary>
        /// Gets or sets the value the specifies if the first row is a heading row.
        /// </summary>
        /// <value>
        /// The value the specifies if the first row is a heading row.
        /// </value>
        [DataMember( Name = "withHeadings" )]
        public bool WithHeadings { get; set; }
    }

}
