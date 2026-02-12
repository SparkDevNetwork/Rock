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
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Security.ChangePassword;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Block for user to change their password.
    /// </summary>
    [DisplayName( "Change Password" )]
    [Category( "Security" )]
    [Description( "Block for a user to change their password." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [TextField( "Invalid Password Caption",
        Key = AttributeKey.InvalidPasswordCaption,
        Description = "",
        IsRequired = false,
        DefaultValue = "The password is not valid.",
        Category = "Captions",
        Order = 0 )]

    [TextField( "Success Caption",
        Key = AttributeKey.SuccessCaption,
        Description = "",
        IsRequired = false,
        DefaultValue = "Your password has been changed",
        Category = "Captions",
        Order = 1 )]

    [TextField( "Change Password Not Supported Caption",
        Key = AttributeKey.ChangePasswordNotSupportedCaption,
        Description = "",
        IsRequired = false,
        DefaultValue = "Changing your password is not supported.",
        Category = "Captions",
        Order = 2 )]

    [BooleanField(
        "Disable Captcha Support",
        Key = AttributeKey.DisableCaptchaSupport,
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        DefaultBooleanValue = false,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "0E077CD9-A59F-45E1-A807-52207130EF64" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "95399BFA-E243-4C7B-8C65-C0AC7793CDC3" )]
    [Rock.SystemGuid.BlockTypeGuid( "3C12DE99-2D1B-40F2-A9B8-6FE7C2524B37" )]
    public class ChangePassword : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string InvalidPasswordCaption = "InvalidPasswordCaption";
            public const string SuccessCaption = "SuccessCaption";
            public const string ChangePasswordNotSupportedCaption = "ChangePasswordNotSupportedCaption";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
        }

        private static class AlertType
        {
            public const string Warning = "warning";
            public const string Danger = "danger";
            public const string Success = "success";
            public const string Info = "info";
        }

        private static class PageParameterKey
        {
            public const string ChangeRequired = "ChangeRequired";
            public const string ReturnUrl = "ReturnUrl";
        }

        #endregion Keys

        #region Properties

        private bool DisableCaptchaSupport => Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() );

        private string InvalidPasswordCaption => GetAttributeValue( AttributeKey.InvalidPasswordCaption );

        private string SuccessCaption => GetAttributeValue( AttributeKey.SuccessCaption );

        private string ChangePasswordNotSupportedCaption => GetAttributeValue( AttributeKey.ChangePasswordNotSupportedCaption );

        private string UnableToLoadAccountCaption => "Unable to load your account information.";

        private string MustLoginCaption => "You must log in before changing your password.";

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ChangePasswordBag
            {
                DisableCaptchaSupport = DisableCaptchaSupport,
                IsChangePasswordVisible = true
            };

            if ( RequestContext.CurrentUser == null || !RequestContext.CurrentUser.IsAuthenticated )
            {
                box.IsChangePasswordVisible = false;
                box.AlertMessage = MustLoginCaption;
                box.AlertType = AlertType.Warning;
                return box;
            }

            if ( PageParameter( PageParameterKey.ChangeRequired ).AsBoolean() )
            {
                box.AlertMessage = "Please change your password before continuing.";
                box.AlertType = AlertType.Info;
            }

            var userLogin = new UserLoginService( RockContext ).GetByUserName( RequestContext.CurrentUser.UserName );
            if ( userLogin == null )
            {
                box.IsChangePasswordVisible = false;
                box.AlertMessage = UnableToLoadAccountCaption;
                box.AlertType = AlertType.Danger;
                return box;
            }

            var component = AuthenticationContainer.GetComponent( userLogin.EntityType?.Name );
            if ( component == null || !component.SupportsChangePassword )
            {
                box.IsChangePasswordVisible = false;
                box.AlertMessage = GetChangePasswordNotSupportedMessage( component );
                box.AlertType = AlertType.Warning;
            }

            return box;
        }

        /// <summary>
        /// Gets the configured not-supported message or a provider-specific fallback.
        /// </summary>
        /// <param name="component">The authentication component, if available.</param>
        /// <returns>The message to display.</returns>
        private string GetChangePasswordNotSupportedMessage( AuthenticationComponent component )
        {
            if ( ChangePasswordNotSupportedCaption.IsNotNullOrWhiteSpace() )
            {
                return ChangePasswordNotSupportedCaption;
            }

            var providerFriendlyName = component?.EntityType?.FriendlyName;
            if ( providerFriendlyName.IsNotNullOrWhiteSpace() )
            {
                return $"Changing your password is not supported when logged in using {providerFriendlyName}.";
            }

            return "Changing your password is not supported.";
        }

        /// <summary>
        /// Gets a safe redirect URL from the ReturnUrl page parameter.
        /// </summary>
        /// <returns>A decoded and safe return URL if valid; otherwise, null.</returns>
        private string GetSafeReturnUrl()
        {
            var returnUrl = PageParameter( PageParameterKey.ReturnUrl );
            if ( returnUrl.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var decodedReturnUrl = HttpUtility.UrlDecode( returnUrl );
            if ( decodedReturnUrl.IsNullOrWhiteSpace() || decodedReturnUrl.RedirectUrlContainsXss() )
            {
                return null;
            }

            return decodedReturnUrl;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="bag">The request bag.</param>
        [BlockAction]
        public BlockActionResult Change( ChangePasswordRequestBag bag )
        {
            if ( !DisableCaptchaSupport && !RequestContext.IsCaptchaValid )
            {
                return ActionBadRequest( "Captcha was not valid." );
            }

            if ( RequestContext.CurrentUser == null || !RequestContext.CurrentUser.IsAuthenticated )
            {
                return ActionBadRequest( MustLoginCaption );
            }

            if ( !UserLoginService.IsPasswordValid( bag?.NewPassword ) )
            {
                return ActionBadRequest( UserLoginService.FriendlyPasswordRules() );
            }

            var userLogin = new UserLoginService( RockContext ).GetByUserName( RequestContext.CurrentUser.UserName );
            if ( userLogin == null )
            {
                return ActionBadRequest( UnableToLoadAccountCaption );
            }

            var component = AuthenticationContainer.GetComponent( userLogin.EntityType?.Name );
            if ( component == null || !component.SupportsChangePassword )
            {
                return ActionBadRequest( GetChangePasswordNotSupportedMessage( component ) );
            }

            if ( component.ChangePassword( userLogin, bag.OldPassword, bag.NewPassword, out var warningMessage ) )
            {
                RockContext.SaveChanges();

                return ActionOk( new ChangePasswordResponseBag
                {
                    AlertMessage = SuccessCaption,
                    AlertType = AlertType.Success,
                    RedirectUrl = GetSafeReturnUrl()
                } );
            }

            return ActionBadRequest( warningMessage.IsNullOrWhiteSpace() ? InvalidPasswordCaption : warningMessage );
        }

        #endregion Block Actions
    }
}
