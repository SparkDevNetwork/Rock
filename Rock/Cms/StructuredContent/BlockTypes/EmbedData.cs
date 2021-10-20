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

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The data used by the <see cref="Embed"/> block type.
    /// </summary>
    [DataContract]
    public class EmbedData
    {
        /// <summary>
        /// Gets or sets the embed service.
        /// </summary>
        /// <value>
        /// The embed service.
        /// </value>
        [DataMember( Name = "service" )]
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets the source URL.
        /// </summary>
        /// <value>
        /// The source URL.
        /// </value>
        [DataMember( Name = "source" )]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the embed.
        /// </summary>
        /// <value>
        /// The embed.
        /// </value>
        [DataMember( Name = "embed" )]
        public string Embed { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        [DataMember( Name = "width" )]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        [DataMember( Name = "height" )]
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        [DataMember( Name = "caption" )]
        public string Caption { get; set; }
    }
}
