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

namespace Rock.Media
{
    /// <summary>
    /// The quality of the media file.
    /// </summary>
    /// <remarks>
    /// <para>When deciding on which file to play by default the order is as follows:</para>
    /// <para>HLS, UltraHD, HD, SD, Embed, Other.</para>
    /// <para>In each quality, the results will be further sorted by <see cref="MediaElementFileData.Width" />
    /// in descending order.</para>
    /// </remarks>
    public enum MediaElementQuality
    {
        /// <summary>
        /// Any other file type not already defined.
        /// </summary>
        Other,

        /// <summary>
        /// An audio file that contains no video.
        /// </summary>
        Audio,

        /// <summary>
        /// The URL is actually an i-frame embed URL that will be
        /// used to play the video.
        /// </summary>
        Embed,

        /// <summary>
        /// Standard Definition video is defined as below 720p.
        /// </summary>
        [Description( "Standard Definition" )]
        SD,

        /// <summary>
        /// High Definition video is defined as 720p or higher.
        /// </summary>
        [Description( "High Definition" )]
        HD,

        /// <summary>
        /// Ultra High Definition video is defined as 4K or higher.
        /// </summary>
        [Description( "Ultra High Definition" )]
        UltraHD,

        /// <summary>
        /// HTTP Live Streaming quality, this is for m3u8 streams that adapt
        /// to the user's available bandwidth to show the best quality
        /// available.
        /// </summary>
        HLS
    }
}
