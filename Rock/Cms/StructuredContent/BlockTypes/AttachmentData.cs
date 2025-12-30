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
    /// The data used by the <see cref="AttachmentRenderer"/> block.
    /// </summary>
    [DataContract]
    public class AttachmentData
    {
        /// <summary>
        /// The title to display for the attachment.
        /// </summary>
        [DataMember( Name = "title" )]
        public string Title { get; set; }

        /// <summary>
        /// The details about the file.
        /// </summary>
        [DataMember( Name = "file" )]
        public AttachmentDataFile File { get; set; }
    }
}
