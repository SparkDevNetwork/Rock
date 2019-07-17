using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;

namespace Rock.Blocks.Types.Mobile
{
    [DisplayName( "Mobile Login" )]
    [Category( "Mobile" )]
    [Description( "Allows user to login on mobile applicatoin." )]
    [IconCssClass( "fa fa-user-lock" )]

    #region Block Attributes

    [LinkedPage( "Registration Page",
        "The page that will be used to register the user.",
        true,
        order: 0 )]

    [UrlLinkField( "Forgot Password Url",
        "The URL to link the user to when they have forgotton their password.",
        true,
        order: 1 )]

    #endregion

    public class MobileLogin : RockBlockType, IRockMobileBlockType
    {
        public static class AttributeKeys
        {
            public const string RegistrationPage = "RegistrationPage";

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

        #region Action Methods

        #endregion
    }
}
