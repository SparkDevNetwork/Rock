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

using System.Collections.Generic;

namespace Rock.Cms
{
    /// <summary>
    /// The schedule data for <see cref="Model.PageShortLink"/>. This contains
    /// all the information required to make a short link dynamic based on a
    /// schedule.
    /// </summary>
    internal class PageShortLinkScheduleData
    {
        /// <summary>
        /// The ordered schedules for a <see cref="Model.PageShortLink"/>. The
        /// first schedule that is active and matches the current date and time
        /// will be used.
        /// </summary>
        public List<PageShortLinkSchedule> Schedules { get; set; } = new List<PageShortLinkSchedule>();
    }
}
