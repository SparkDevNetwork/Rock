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

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpOpportunityAttendeeList
{
    /// <summary>
    /// The additional configuration options for the Sign-Up Opportunity Attendee List block.
    /// </summary>
    public class SignUpOpportunityAttendeeListOptionsBag
    {
        /// <summary>
        /// Gets or sets the error message that prevents the block from being displayed.
        /// When set, the summary panel and grid are hidden and only this message is shown.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the display name of this sign-up opportunity. This is the schedule
        /// configuration name when provided, falling back to the project (group) name, with
        /// the schedule name appended when the schedule is a named schedule.
        /// </summary>
        public string OpportunityName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the opportunity's location.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the friendly text describing the opportunity's schedule.
        /// </summary>
        public string FriendlySchedule { get; set; }

        /// <summary>
        /// Gets or sets the minimum attendee capacity configured for this opportunity.
        /// </summary>
        public int? SlotsMinimum { get; set; }

        /// <summary>
        /// Gets or sets the desired attendee capacity configured for this opportunity.
        /// </summary>
        public int? SlotsDesired { get; set; }

        /// <summary>
        /// Gets or sets the maximum attendee capacity configured for this opportunity.
        /// </summary>
        public int? SlotsMaximum { get; set; }

        /// <summary>
        /// Gets or sets the count of slots currently filled for this opportunity. Deceased
        /// individuals are excluded from the count.
        /// </summary>
        public int SlotsFilled { get; set; }

        /// <summary>
        /// Gets or sets the name of the project's group type, displayed as a panel label.
        /// </summary>
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the project's campus, displayed as a panel label.
        /// When null or empty the campus label is hidden.
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the project (group) is inactive, in
        /// which case an "Inactive" label is displayed on the panel.
        /// </summary>
        public bool IsGroupInactive { get; set; }

        /// <summary>
        /// Gets or sets the prefix applied to person preference keys so that grid filters
        /// are remembered per opportunity rather than shared across every opportunity
        /// viewed on this page.
        /// </summary>
        public string PreferenceKeyPrefix { get; set; }

        /// <summary>
        /// Gets or sets the name of the project (group), used as the export file name.
        /// </summary>
        public string ProjectName { get; set; }
    }
}
