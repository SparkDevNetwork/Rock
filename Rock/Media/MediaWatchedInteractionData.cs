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
    /// The custom data object to store in the interaction data for a
    /// "media watched" interaction. Marked private for now so we can
    /// move it to a better location later without breaking anything.
    /// </summary>
    public class MediaWatchedInteractionData
    {
        /// <summary>
        /// Gets or sets the watch map.
        /// </summary>
        /// <value>
        /// The watch map.
        /// </value>
        public string WatchMap { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the media that was watched.
        /// </summary>
        /// <value>
        /// The percentage of the media that was watched.
        /// </value>
        public double WatchedPercentage { get; set; }
    }
}
