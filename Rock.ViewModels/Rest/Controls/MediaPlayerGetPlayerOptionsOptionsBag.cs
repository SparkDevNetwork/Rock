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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetPlayerOptions API action of the MediaPlayer control.
    /// </summary>
    public class MediaPlayerGetPlayerOptionsOptionsBag
    {
        /// <summary>
        /// The initial set of options to pass to the media player
        /// </summary>
        public MediaPlayerOptionsBag PlayerOptions { get; set; }

        /// <summary>
        /// Identifier for the Media Element to play
        /// </summary>
        public Guid MediaElementGuid { get; set; }

        /// <summary>
        /// The number of days back to look for an existing interaction
        /// for the media element. Will be used to find where the user left off
        /// previously and resume from that point.
        /// </summary>
        public int AutoResumeInDays { get; set; }

        /// <summary>
        /// The number of days back to look for an existing
        /// interaction for the media element that should be updated. If one
        /// is not found then a new interaction will be created. Set to
        /// <c>0</c> to always create a new interaction.
        /// </summary>
        public int CombinePlayStatisticsInDays { get; set; }
    }
}
