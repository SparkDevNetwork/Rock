using System.ComponentModel;

using Rock.Attribute;

namespace Rock.Blocks.Types.Mobile
{
    /// <summary>
    /// Allows the user to login on a mobile application.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="Rock.Blocks.IRockMobileBlockType" />

    [DisplayName( "Mobile Login" )]
    [Category( "Mobile" )]
    [Description( "Allows the user to login on amobile application." )]
    [IconCssClass( "fa fa-user-lock" )]

    #region Block Attributes

    [LinkedPage( "Registration Page",
        Description = "The page that will be used to register the user.",
        IsRequired = true,
        Key = AttributeKeys.RegistrationPage,
        Order = 0 )]

    [UrlLinkField( "Forgot Password Url",
        Description = "The URL to link the user to when they have forgotton their password.",
        IsRequired = true,
        Key = AttributeKeys.ForgotPasswordUrl,
        Order = 1 )]

    #endregion

    public class MobileLogin : RockBlockType, IRockMobileBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the MobileLogin block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The registration page key
            /// </summary>
            public const string RegistrationPage = "RegistrationPage";

            /// <summary>
            /// The forgot password URL key
            /// </summary>
            public const string ForgotPasswordUrl = "ForgotPasswordUrl";
        }

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        int IRockMobileBlockType.RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        string IRockMobileBlockType.MobileBlockType => "Rock.Mobile.Blocks.Login";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        object IRockMobileBlockType.GetMobileConfigurationValues()
        {
            return new
            {
                RegistrationPageGuid = GetAttributeValue( AttributeKeys.RegistrationPage ),
                ForgotPasswordUrl = GetAttributeValue( AttributeKeys.ForgotPasswordUrl )
            };
        }

        #endregion
    }
}
