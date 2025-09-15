using Rock.AI.Agent.Classes.Common;
using Rock.Model;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Represents the result of a reminder type operation.
    /// </summary>
    /// <remarks>
    /// This class contains details about a reminder type, including its name, description, associated entity type, and notification settings.
    /// </remarks>
    public class ReminderTypeResult : EntityResultBase 
    {
        /// <summary>
        /// Gets or sets the name of the reminder type.
        /// </summary>
        /// <value>
        /// The name of the reminder type.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the reminder type.
        /// </summary>
        /// <value>
        /// The description of the reminder type.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the entity type associated with the reminder type.
        /// </summary>
        /// <value>
        /// The entity type associated with the reminder type.
        /// </value>
        public KeyNameResult EntityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reminder type is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the reminder type is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the order of the reminder type.
        /// </summary>
        /// <value>
        /// The order of the reminder type.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the notification type for the reminder type.
        /// </summary>
        /// <value>
        /// The notification type for the reminder type.
        /// </value>
        public ReminderNotificationType NotificationType { get; set; }
    }
}
