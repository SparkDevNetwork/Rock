namespace Rock.Enums.Mobile
{
    /// <summary>
    /// Represents the different levels of location permission that can be granted to an application.
    /// </summary>
    public enum LocationPermissionStatus
    {
        /// <summary>
        /// The Person has not granted permission to use location services.
        /// </summary>
        NotGranted = 0,

        /// <summary>
        /// The Person denied permission to use location services.
        /// </summary>
        Denied = 1,

        /// <summary>
        /// The Person has granted permission to use location services only when the app is in use.
        /// </summary>
        WhenInUse = 2,

        /// <summary>
        /// The Person has granted permission to use location services at all times.
        /// </summary>
        Always = 3
    }
}
