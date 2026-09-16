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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail
{
    /// <summary>
    /// A sign-up project opportunity: a schedule at a location. Used both as a row of the
    /// opportunities grid and as the payload of the add and edit opportunity modal.
    /// </summary>
    public class SignUpOpportunityBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the group location the opportunity belongs to. Null
        /// when adding an opportunity.
        /// </summary>
        public string GroupLocationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the opportunity's location. Null when adding an
        /// opportunity.
        /// </summary>
        public string LocationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the opportunity's schedule. Null when adding an
        /// opportunity.
        /// </summary>
        public string ScheduleIdKey { get; set; }

        /// <summary>
        /// Gets or sets the optional name of the opportunity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the next start date time of the opportunity or, when there is none, the
        /// last start date time. Used to sort the opportunities.
        /// </summary>
        public DateTimeOffset? NextOrLastStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the friendly text describing the opportunity's schedule.
        /// </summary>
        public string FriendlyDateTime { get; set; }

        /// <summary>
        /// Gets or sets the friendly text describing the opportunity's location.
        /// </summary>
        public string FriendlyLocation { get; set; }

        /// <summary>
        /// Gets or sets the minimum attendee capacity of the opportunity.
        /// </summary>
        public int? SlotsMinimum { get; set; }

        /// <summary>
        /// Gets or sets the desired attendee capacity of the opportunity.
        /// </summary>
        public int? SlotsDesired { get; set; }

        /// <summary>
        /// Gets or sets the maximum attendee capacity of the opportunity.
        /// </summary>
        public int? SlotsMaximum { get; set; }

        /// <summary>
        /// Gets or sets the count of slots currently filled. Deceased individuals are excluded
        /// from the count.
        /// </summary>
        public int SlotsFilled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the opportunity has a future start date time.
        /// </summary>
        public bool IsUpcoming { get; set; }

        /// <summary>
        /// Gets or sets the type of the opportunity's schedule. Only custom and named schedules
        /// can be saved from the modal.
        /// </summary>
        public ScheduleType ScheduleType { get; set; }

        /// <summary>
        /// Gets or sets the iCalendar content of the schedule when <see cref="ScheduleType"/> is
        /// <c>Custom</c>.
        /// </summary>
        public string ICalendarContent { get; set; }

        /// <summary>
        /// Gets or sets the named schedule when <see cref="ScheduleType"/> is <c>Named</c>. The
        /// value is the schedule unique identifier.
        /// </summary>
        public ListItemBag NamedSchedule { get; set; }

        /// <summary>
        /// Gets or sets the mode the location picker used to select <see cref="Location"/>.
        /// </summary>
        public GroupLocationPickerMode LocationPickerMode { get; set; }

        /// <summary>
        /// Gets or sets the location as emitted by the location picker. The runtime shape depends
        /// on <see cref="LocationPickerMode"/>: a <see cref="ListItemBag"/> whose value is the
        /// location unique identifier when Named, a
        /// <see cref="Rock.ViewModels.Controls.AddressControlBag"/> when Address, or a Well-Known
        /// Text string when Point or Polygon.
        /// </summary>
        public object Location { get; set; }

        /// <summary>
        /// Gets or sets the additional details appended to the reminder communication for this
        /// opportunity.
        /// </summary>
        public string ReminderAdditionalDetails { get; set; }

        /// <summary>
        /// Gets or sets the additional details appended to the confirmation communication for
        /// this opportunity.
        /// </summary>
        public string ConfirmationAdditionalDetails { get; set; }
    }
}
