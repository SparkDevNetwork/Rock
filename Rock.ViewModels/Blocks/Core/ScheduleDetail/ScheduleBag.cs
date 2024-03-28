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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.ScheduleDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class ScheduleBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the shortened name of the attribute.
        /// If null or whitespace then the full name is returned.
        /// </summary>
        public string AbbreviatedName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto inactivate when complete].
        /// </summary>
        public bool AutoInactivateWhenComplete { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Category that this Schedule belongs to.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes following schedule start that Check-in should be active. 0 represents that Check-in will only be available
        /// until the Schedule's start time.
        /// </summary>
        public int? CheckInEndOffsetMinutes { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes prior to the Schedule's start time  that Check-in should be active. 0 represents that Check-in 
        /// will not be available to the beginning of the event.
        /// </summary>
        public int? CheckInStartOffsetMinutes { get; set; }

        /// <summary>
        /// Gets or sets a user defined Description of the Schedule.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets that date that this Schedule expires and becomes inactive. This value is inclusive and the schedule will be inactive after this date.
        /// </summary>
        public DateTimeOffset? EffectiveEndDate { get; set; }

        /// <summary>
        /// Gets or sets the Date that the Schedule becomes effective/active. This property is inclusive, and the schedule will be inactive before this date. 
        /// </summary>
        public DateTimeOffset? EffectiveStartDate { get; set; }

        /// <summary>
        /// Gets the friendly schedule text.
        /// </summary>
        public string FriendlyScheduleText { get; set; }

        /// <summary>
        /// Gets or sets the content lines of the iCalendar
        /// </summary>
        public string iCalendarContent { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active schedule. This value is required.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this Schedule is public.
        /// </summary>
        public bool? IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Schedule. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the next occurrence date to be displayed on the remote device.
        /// </summary>
        public string NextOccurrence { get; set; }
    }
}
