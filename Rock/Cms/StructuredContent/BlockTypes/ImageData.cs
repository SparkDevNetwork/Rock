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
using System.Runtime.Serialization;

namespace Rock.Cms.StructuredContent.BlockTypes
{
    /// <summary>
    /// The data used by the <see cref="ImageRenderer"/> block.
    /// </summary>
    [DataContract]
    public class ImageData
    {
        /// <summary>
        /// Gets or sets the caption to be displayed with the image.
        /// </summary>
        /// <value>
        /// The caption to be displayed with the image.
        /// </value>
        [DataMember( Name = "caption" )]
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this image is stretched.
        /// </summary>
        /// <value>
        ///   <c>true</c> if stretched; otherwise, <c>false</c>.
        /// </value>
        [DataMember( Name = "stretched" )]
        public bool Stretched { get; set; }

        /// <summary>
        /// Gets or sets the file data.
        /// </summary>
        /// <value>
        /// The file data.
        /// </value>
        [DataMember( Name = "file" )]
        public ImageDataFile File { get; set; }

        /// <summary>
        /// Gets or sets the legacy URL. If this instance contains a valid <see cref="File"/>
        /// value then it should be used instead.
        /// </summary>
        /// <value>
        /// The legacy URL.
        /// </value>
        [DataMember( Name = "url" )]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a border should be displayed
        /// around the image.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a border should be displayed; otherwise, <c>false</c>.
        /// </value>
        [Obsolete( "This value is not used by Rock." )]
        [RockObsolete("1.13")]
        [DataMember( Name = "withBorder" )]
        public bool WithBorder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a background should be
        /// displayed behind the image.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a background should be displayed; otherwise, <c>false</c>.
        /// </value>
        [Obsolete( "This value is not used by Rock." )]
        [RockObsolete("1.13")]
        [DataMember( Name = "withBackground" )]
        public bool WithBackground { get; set; }
    }

}
