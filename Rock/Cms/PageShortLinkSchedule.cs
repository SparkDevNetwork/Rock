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

namespace Rock.Cms
{
    /// <summary>
    /// A single schedule configuration for a <see cref="Model.PageShortLink"/>
    /// when it is configured to use schedules.
    /// </summary>
    internal class PageShortLinkSchedule
    {
        /// <summary>
        /// The identifier of the named schedule this short link schedule will
        /// follow. If <c>null</c> then the iCal content from
        /// <see cref="CustomCalendarContent"/> will be used instead.
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// The custom iCal content that defines the schedule for this link.
        /// </summary>
        public string CustomCalendarContent { get; set; }

        /// <summary>
        /// The URL to redirect requests to when this schedule is the best match.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// An optional purpose key that will be used to categorize interactions.
        /// </summary>
        public string PurposeKey { get; set; }

        /// <summary>
        /// The UTM settings to append to the URL when this schedule is the
        /// best match.
        /// </summary>
        public UtmSettings UtmSettings { get; set; }
    }
}
