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

namespace Rock.Media
{
    /// <summary>
    /// The configuration options that are available for Rock's JavaScript
    /// media player.
    /// </summary>
    [Newtonsoft.Json.JsonObject(
        NamingStrategyType = typeof( Newtonsoft.Json.Serialization.CamelCaseNamingStrategy ),
        ItemNullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore )]
    internal class MediaPlayerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to automatically pause
        /// other media players on the page when this player begins playing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if other players should be paused; otherwise, <c>false</c>.
        /// </value>
        public bool Autopause { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to attempt to automatically
        /// play as soon as it can. This will likely only work if you also
        /// specify the <see cref="Muted"/> parameter as most browsers do not
        /// allow auto play with sound.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the player should attempt to play automatically; otherwise, <c>false</c>.
        /// </value>
        public bool Autoplay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user will be allowed to
        /// click anywhere on the media player to play or pause the playback.
        /// </summary>
        /// <value>
        ///   <c>true</c> if click-to-play should be supported; otherwise, <c>false</c>.
        /// </value>
        public bool ClickToPlay { get; set; }

        /// <summary>
        /// Gets or sets the user interface controls to display on the player.
        /// This should be a comma delimited list of values taken from
        /// <see cref="MediaPlayerControls"/>.
        /// </summary>
        /// <value>
        /// The user interface controls to display on the player.
        /// </value>
        public string Controls { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether debug information should
        /// be written to the JavaScript console.
        /// </summary>
        /// <value>
        ///   <c>true</c> if debug logging is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Debug { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user interface controls
        /// should be automatically hidden after a brief period of inactivity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if user interface controls should be automatically hidden; otherwise, <c>false</c>.
        /// </value>
        public bool HideControls { get; set; }

        /// <summary>
        /// Gets or sets the interaction unique identifier to be updated. If
        /// not set then a new Interaction is created.
        /// </summary>
        /// <value>
        /// The interaction unique identifier to be updated.
        /// </value>
        public Guid? InteractionGuid { get; set; }

        /// <summary>
        /// Gets or sets the previous watch map data. This is used when
        /// calculating the resume position or updating an existing Interaction.
        /// </summary>
        /// <value>
        /// The previous watch map data.
        /// </value>
        public string Map { get; set; }

        /// <summary>
        /// Gets or sets the media element unique identifier. This is used when
        /// writing an Interaction to associate it to the media element.
        /// </summary>
        /// <value>
        /// The media element unique identifier.
        /// </value>
        public Guid? MediaElementGuid { get; set; }

        /// <summary>
        /// Gets or sets the URL to use for playback.
        /// </summary>
        /// <value>
        /// The URL of the media to be played.
        /// </value>
        public string MediaUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the media player will be
        /// initially muted on page load.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initially muted; otherwise, <c>false</c>.
        /// </value>
        public bool Muted { get; set; }

        /// <summary>
        /// Gets or sets the poster image URL to display before the video
        /// starts playing. This only works with HTML5 style videos, it will
        /// not work with embed links such as YouTube uses.
        /// </summary>
        /// <value>
        /// The thumbnail image URL to display before playback starts.
        /// </value>
        public string PosterUrl { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier to store with the
        /// interaction if the session is being tracked.
        /// </summary>
        /// <value>
        /// The related entity identifier for the interaction.
        /// </value>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the related entity type identifier to store with
        /// the interaction if the session is being tracked.
        /// </summary>
        /// <value>
        /// The related entity type identifier for the interaction.
        /// </value>
        public int? RelatedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to resume playback from
        /// the first gap in playback history.
        /// </summary>
        /// <remarks>
        /// This should never be null when encoded.
        /// </remarks>
        /// <value>
        ///   <c>true</c> if playback should be resumed; otherwise, <c>false</c>.
        /// </value>
        public bool? ResumePlaying { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to seek forward or backward
        /// when the fast-forward or rewind controls are clicked.
        /// </summary>
        /// <value>
        /// The number of seconds to fast forward or rewind.
        /// </value>
        public double SeekTime { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the current interaction session.
        /// </summary>
        /// <value>
        /// The unique identifier for the current interaciton session.
        /// </value>
        public Guid? SessionGuid { get; set; }

        /// <summary>
        /// Gets or sets the title to display for the video.
        /// </summary>
        /// <value>
        /// The title to display for the video.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether progress tracking should be
        /// enabled. This is different than <see cref="WriteInteraction"/>. This
        /// value determines if the internal logic for monitoring playback
        /// progress should be used or not. It is required for <see cref="WriteInteraction"/>
        /// to function.
        /// </summary>
        /// <value>
        ///   <c>true</c> if progress tracking should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool TrackProgress { get; set; }

        /// <summary>
        /// Gets or sets the type of media player to display.
        /// </summary>
        /// <value>
        /// The string <c>"video"</c> to force a video interface, <c>"audio"</c>
        /// to force an audio interface. Any other value is treated as detect
        /// automatically.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the initial volume for playback.
        /// </summary>
        /// <value>
        /// The initial volume for playback between 0 and 1.0.
        /// </value>
        public double Volume { get; set; }

        /// <summary>
        /// Determines if the user's session should be tracked and stored as an
        /// <see cref="Rock.Model.Interaction"/> in the system. This is required
        /// to provide play metrics as well as use the resume feature later.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the playback session should be tracked in the Interactions table; otherwise, <c>false</c>.
        /// </value>
        public bool WriteInteraction { get; set; }
    }
}
