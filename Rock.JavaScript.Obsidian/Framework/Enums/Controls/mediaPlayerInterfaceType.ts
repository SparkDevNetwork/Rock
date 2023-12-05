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
export const MediaPlayerInterfaceType = {
    Automatic: "",
    Video: "video",
    Audio: "audio"
} as const;

/** The type of the alert box to display. Ex: 'success' will appear green and as if something good happened. */
export type MediaPlayerInterfaceType = typeof MediaPlayerInterfaceType[keyof typeof MediaPlayerInterfaceType];
