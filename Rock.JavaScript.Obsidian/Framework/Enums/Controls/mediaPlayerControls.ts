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

/** The type of the alert box to display. Ex: 'success' will appear green and as if something good happened. */
export const MediaPlayerControls = {
    Airplay: "airplay",
    Captions: "captions",
    CurrentTime: "current-time",
    Download: "download",
    Duration: "duration",
    FastForward: "fast-forward",
    Fullscreen: "fullscreen",
    Mute: "mute",
    PictureInPicture: "pip",
    Play: "play",
    PlayLarge: "play-large",
    Progress: "progress",
    Restart: "restart",
    Rewind: "rewind",
    Settings: "settings",
    Volume: "volume"
} as const;

/** The type of the alert box to display. Ex: 'success' will appear green and as if something good happened. */
export type MediaPlayerControls = typeof MediaPlayerControls[keyof typeof MediaPlayerControls];
