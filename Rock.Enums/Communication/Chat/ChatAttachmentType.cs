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
using System.ComponentModel;

namespace Rock.Enums.Communication.Chat
{
    /// <summary>
    /// The type of attachment being sent in a chat message.
    /// </summary>
    [Enums.EnumDomain( "Communication" )]
    public enum ChatAttachmentType
    {
        /// <summary>
        /// Image Attachment.
        /// </summary>
        [Description( "image" )]
        Image = 0,

        /// <summary>
        /// File Attachment.
        /// </summary>
        [Description( "file" )]
        File = 1,

        /// <summary>
        /// Audio Attachment.
        /// </summary>
        [Description( "audio" )]
        Audio = 2,

        /// <summary>
        /// Video Attachment.
        /// </summary>
        [Description( "video" )]
        Video = 3
    }
}
