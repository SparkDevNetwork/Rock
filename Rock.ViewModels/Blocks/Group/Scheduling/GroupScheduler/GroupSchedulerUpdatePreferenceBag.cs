using System;
using Rock.Enums.Blocks.Group.Scheduling;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler
{
    /// <summary>
    /// The information needed to update a group member's scheduling preference for the group scheduler.
    /// </summary>
    public class GroupSchedulerUpdatePreferenceBag
    {
        /// <summary>
        /// Gets or sets the attendance identifier.
        /// </summary>
        /// <value>
        /// The attendance identifier.
        /// </value>
        public int AttendanceId { get; set; }

        /// <summary>
        /// Gets or sets the group member identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        public int GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the preference that should be applied.
        /// </summary>
        /// <value>
        /// The preference that should be applied.
        /// </value>
        public GroupSchedulerPreferenceBag SchedulePreference { get; set; }

        /// <summary>
        /// Gets or sets the mode to be used when applying this update.
        /// </summary>
        /// <value>
        /// The mode to be used when applying this update.
        /// </value>
        public UpdateSchedulePreferenceMode UpdateMode { get; set; }
    }
}
