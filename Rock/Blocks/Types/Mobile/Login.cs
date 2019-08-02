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
using System.ComponentModel;

using Rock.Attribute;

namespace Rock.Blocks.Types.Mobile
{
    /// <summary>
    /// Allows the user to login on a mobile application.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="Rock.Blocks.IRockMobileBlockType" />

    [DisplayName( "Login" )]
    [Category( "Mobile" )]
    [Description( "Allows the user to login on a mobile application." )]
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

    public class Login : RockBlockType, IRockMobileBlockType
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
