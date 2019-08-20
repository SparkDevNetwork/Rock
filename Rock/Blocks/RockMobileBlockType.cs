using Rock.Attribute;
using Rock.Web;

namespace Rock.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="Rock.Blocks.IRockMobileBlockType" />
    public abstract class RockMobileBlockType : RockBlockType, IRockMobileBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the required mobile application binary interface version.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version.
        /// </value>
        public abstract int RequiredMobileAbiVersion { get; }

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public abstract string MobileBlockType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public abstract object GetMobileConfigurationValues();

        #endregion

        #region Custom Settings

        /// <summary>
        /// Defines the control that will provide the Basic Settings tab content
        /// for all RockMobileBockType blocks.
        /// </summary>
        /// <seealso cref="Rock.Web.RockCustomSettingsUserControlProvider" />
        [TargetType( typeof( RockMobileBlockType ) )]
        public class RockMobileBlockTypeCustomAdvancedSettingsProvider : RockCustomSettingsUserControlProvider
        {
            /// <summary>
            /// Gets the path to the user control file.
            /// </summary>
            /// <value>
            /// The path to the user control file.
            /// </value>
            protected override string UserControlPath => "~/Blocks/Mobile/MobileCustomAdvancedSettings.ascx";

            /// <summary>
            /// Gets the custom settings title. Used when displaying tabs or links to these settings.
            /// </summary>
            /// <value>
            /// The custom settings title.
            /// </value>
            public override string CustomSettingsTitle => "Advanced Settings";
        }

        #endregion
    }
}
