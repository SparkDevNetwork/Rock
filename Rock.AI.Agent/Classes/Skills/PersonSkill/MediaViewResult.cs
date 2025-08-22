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

using System;
using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Lightweight result model for a person's page-visit aggregate.
    /// </summary>
    public class MediaViewResult
    {
        /// <summary>
        /// The identifier of the media view record.
        /// </summary>
        [JsonIgnore]
        public int MediaId { get; set; }

        /// <summary>
        /// The opaque identifier for the media view record (Id Key).
        /// </summary>
        public string MediaIdKey {
            get
            {
                return this.MediaId.AsIdKey();
            }
        }

        /// <summary>
        /// The date and time when the view data was viewed.
        /// </summary>
        public DateTime ViewDateTime { get; set; }

        /// <summary>
        /// Display digital medium the watch was made on (web,mobile).
        /// </summary>
        public string Medium { get; set; }

        /// <summary>
        /// The percent of he video that was watched.
        /// </summary>
        public int? PercentWatched { get; set; }

        /// <summary>
        /// The length of the media in seconds.
        /// </summary>
        public int? MediaLengthInSeconds { get; set; }

        /// <summary>
        /// The duration of the media that was watched in seconds. This is null if the media was not watched.
        /// </summary>
        public int? DurationWatchedInSeconds { get; set; }

        /// <summary>
        /// The name of the media that was viewed, such as "Sunday Service - 2023-10-01".
        /// </summary>
        public string MediaName { get; set; }

        /// <summary>
        /// The URL where the media can be viewed, such as "https://example.com/media/12345".
        /// </summary>
        public string ViewingLocationUrl { get; set; }
    }
}
