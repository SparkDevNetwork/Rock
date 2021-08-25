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

namespace Rock.Media
{
    /// <summary>
    /// The type of player to present to the user when displaying a
    /// media player instance.
    /// </summary>
    public enum MediaPlayerInterfaceType
    {
        /// <summary>
        /// Automatically detect based on the media being played.
        /// </summary>
        Automatic = 0,

        /// <summary>
        /// Force display as a video player.
        /// </summary>
        Video = 1,

        /// <summary>
        /// Force display as an audio player.
        /// </summary>
        Audio = 2
    }
}
