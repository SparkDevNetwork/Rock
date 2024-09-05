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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.PageShortLinkDetail
{
    /// <summary>
    /// The details of a single scheduled redirect for a short link when editing
    /// a page short link item.
    /// </summary>
    public class ScheduledRedirectBag
    {
        /// <summary>
        /// The selected named schedule. If not set then a custom schedule is
        /// in use.
        /// </summary>
        public ListItemBag NamedSchedule { get; set; }

        /// <summary>
        /// The iCalendar content that describes the custom schedule.
        /// </summary>
        public string CustomCalendarContent { get; set; }

        /// <summary>
        /// A short description of the schedule. For a named schedule this will
        /// be the name.
        /// </summary>
        public string ScheduleText { get; set; }

        /// <summary>
        /// The text that describes the date range of this scheduled redirect.
        /// </summary>
        public string ScheduleRangeText { get; set; }

        /// <summary>
        /// The URL to redirect the individual to during this scheduled period.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// An optional purpose key that will be used to categorize interactions.
        /// </summary>
        public string PurposeKey { get; set; }

        /// <summary>
        /// The UTM settings to append to the URL when this schedule is used.
        /// </summary>
        public UtmSettingsBag UtmSettings { get; set; }
    }
}
