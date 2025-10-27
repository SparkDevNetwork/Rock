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
    /// The file data used by the <see cref="AttachmentRenderer"/> block.
    /// </summary>
    [DataContract]
    public class AttachmentDataFile
    {
        /// <summary>
        /// The extension of the file, without the leading period.
        /// </summary>
        [DataMember( Name = "extension" )]
        public string Extension { get; set; }

        /// <summary>
        /// The unique identifier of the uploaded file.
        /// </summary>
        [DataMember( Name = "fileGuid" )]
        public Guid FileGuid { get; set; }

        /// <summary>
        /// The original name of the file.
        /// </summary>
        [DataMember( Name = "name" )]
        public string Name { get; set; }

        /// <summary>
        /// The size of the file, in bytes.
        /// </summary>
        [DataMember( Name = "size" )]
        public long Size { get; set; }

        /// <summary>
        /// The URL that can be used to download the file, relative to the site root.
        /// </summary>
        [DataMember( Name = "url" )]
        public string Url { get; set; }
    }
}
