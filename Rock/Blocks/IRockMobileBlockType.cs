namespace Rock.Blocks
{
    /// <summary>
    /// Defines the properties and methods that all mobile blocks must implement.
    /// </summary>
    /// <seealso cref="Rock.Blocks.IRockBlockType" />
    public interface IRockMobileBlockType : IRockBlockType
    {
        /// <summary>
        /// Gets the required mobile application binary interface version.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version.
        /// </value>
        int RequiredMobileAbiVersion { get; }

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        string MobileBlockType { get; }

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>A collection of string/object pairs.</returns>
        object GetMobileConfigurationValues();
    }
}
