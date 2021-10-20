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
    /// The data used by the <see cref="ListData"/> block type.
    /// </summary>
    [DataContract]
    public class ListData
    {
        /// <summary>
        /// The ordered <see cref="Style"/> value.
        /// </summary>
        public static string OrderedStyle = "ordered";

        /// <summary>
        /// The unordered <see cref="Style"/> value.
        /// </summary>
        public static string UnorderedStyle = "unordered";

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        [DataMember( Name = "style" )]
        public string Style { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        /// <remarks>
        /// This is actually a collection of <see cref="ListDataItem"/>, but
        /// we need to use dynamic because of legacy lists that were not
        /// nested.
        /// </remarks>
        [DataMember( Name = "items" )]
        public List<dynamic> Items { get; set; }
    }

}
