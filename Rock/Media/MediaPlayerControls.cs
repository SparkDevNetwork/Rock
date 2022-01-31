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
    /// The user interface controls that are available on the media players.
    /// </summary>
    /// <remarks>
    /// Intentionally made as static instead of const so that we can change
    /// the values later if we need to without causing plugins to need to
    /// recompile.
    /// </remarks>
    public static class MediaPlayerControls
    {
        /// <summary>
        /// Airplay button (currently Safari only).
        /// </summary>
        public static readonly string Airplay = "airplay";

        /// <summary>
        /// Toggle captions on and off.
        /// </summary>
        public static readonly string Captions = "captions";

        /// <summary>
        /// The current time of playback.
        /// </summary>
        public static readonly string CurrentTime = "current-time";

        /// <summary>
        /// Show a download button with a link to the source.
        /// </summary>
        public static readonly string Download = "download";

        /// <summary>
        /// The full duration of the media.
        /// </summary>
        public static readonly string Duration = "duration";

        /// <summary>
        /// Fast forward by the seek time.
        /// </summary>
        public static readonly string FastForward = "fast-forward";

        /// <summary>
        /// Toggle full-screen playback.
        /// </summary>
        public static readonly string Fullscreen = "fullscreen";

        /// <summary>
        /// Toggle mute on and off.
        /// </summary>
        public static readonly string Mute = "mute";

        /// <summary>
        /// Picture-in-picture (currently Safari only).
        /// </summary>
        public static readonly string PictureInPicture = "pip";

        /// <summary>
        /// Play or pause playback.
        /// </summary>
        public static readonly string Play = "play";

        /// <summary>
        /// The large play button in the center.
        /// </summary>
        public static readonly string PlayLarge = "play-large";

        /// <summary>
        /// The progress bar and scrubber for playback and buffering.
        /// </summary>
        public static readonly string Progress = "progress";

        /// <summary>
        /// Restart playback at beginning.
        /// </summary>
        public static readonly string Restart = "restart";

        /// <summary>
        /// Rewind by the seek time.
        /// </summary>
        public static readonly string Rewind = "rewind";

        /// <summary>
        /// Settings menu to adjust playback speed and other options depending on media.
        /// </summary>
        public static readonly string Settings = "settings";

        /// <summary>
        /// Volume control.
        /// </summary>
        public static readonly string Volume = "volume";
    }
}
