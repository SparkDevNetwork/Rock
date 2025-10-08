namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Represents the notification details sent to the client when a proximity-based
    /// attendance event occurs.
    /// </summary>
    public class ProximityAttendanceNotificationBag
    {
        /// <summary>
        /// Gets or sets the title of the notification (e.g. the heading presented to the user).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the body/message content of the notification displayed to the user.
        /// </summary>
        public string Message { get; set; }
    }
}
