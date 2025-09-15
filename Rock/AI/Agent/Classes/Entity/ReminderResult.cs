using System;

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Represents the result of a reminder operation.
    /// </summary>
    /// <remarks>
    /// This class contains details about a reminder, including its completion status, renewal settings, associated entity, and type.
    /// </remarks>
    public class ReminderResult : EntityResultBase 
    {
        /// <summary>
        /// Gets or sets a value indicating whether the reminder is complete.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the reminder is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the number of days between renewals.
        /// </summary>
        /// <value>
        /// The number of days between renewals, or <c>null</c> if the reminder does not renew.
        /// </value>
        public int? RenewPeriodDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times the reminder can renew.
        /// </summary>
        /// <value>
        /// The maximum number of renewals, or <c>null</c> if there is no limit.
        /// </value>
        public int? RenewMaxCount { get; set; }

        /// <summary>
        /// Gets or sets the current number of times the reminder has renewed.
        /// </summary>
        /// <value>
        /// The current number of renewals, or <c>null</c> if the reminder has not renewed.
        /// </value>
        public int? RenewCurrentCount { get; set; }

        /// <summary>
        /// Gets or sets the date of the reminder.
        /// </summary>
        /// <value>
        /// The date of the reminder.
        /// </value>
        public DateTime ReminderDate { get; set; }

        /// <summary>
        /// Gets or sets the note associated with the reminder.
        /// </summary>
        /// <value>
        /// The note associated with the reminder.
        /// </value>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity associated with the reminder.
        /// </summary>
        /// <value>
        /// The name of the entity associated with the reminder.
        /// </value>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity associated with the reminder.
        /// </summary>
        /// <value>
        /// The type of the entity associated with the reminder.
        /// </value>
        public KeyNameResult EntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the reminder.
        /// </summary>
        /// <value>
        /// The type of the reminder.
        /// </value>
        public KeyNameResult ReminderType { get; set; }
    }
}