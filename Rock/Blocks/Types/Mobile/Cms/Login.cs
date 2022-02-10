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
using System;
using System.ComponentModel;

using Rock.Attribute;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// Allows the user to log in on a mobile application.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Log In" )]
    [Category( "Mobile > Cms" )]
    [Description( "Allows the user to log in on a mobile application." )]
    [IconCssClass( "fa fa-user-lock" )]

    #region Block Attributes

    [LinkedPage( "Registration Page",
        Description = "The page that will be used to register the user.",
        IsRequired = false,
        Key = AttributeKeys.RegistrationPage,
        Order = 0 )]

    [UrlLinkField( "Forgot Password URL",
        Description = "The URL to link the user to when they have forgotten their password.",
        IsRequired = false,
        Key = AttributeKeys.ForgotPasswordUrl,
        Order = 1 )]

    [LinkedPage( "Confirmation Page",
        Key = AttributeKeys.ConfirmationWebPage,
        Description = "Web page on a public site for user to confirm their account (if not set then no confirmation e-mail will be sent).",
        IsRequired = false,
        Order = 2 )]

    [SystemCommunicationField( "Confirm Account Template",
        Key = AttributeKeys.ConfirmAccountTemplate,
        Description = "The system communication to use when generating the confirm account e-mail.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT,
        Order = 3 )]

    [LinkedPage( "Return Page",
        Description = "The page to return to after the individual has successfully logged in, defaults to the home page (requires shell v3).",
        IsRequired = false,
        Key = AttributeKeys.ReturnPage,
        Order = 4 )]

    [LinkedPage( "Cancel Page",
        Description = "The page to return to after pressing the cancel button, defaults to the home page (requires shell v3).",
        IsRequired = false,
        Key = AttributeKeys.CancelPage,
        Order = 5 )]

    #endregion

    public class Login : RockMobileBlockType
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

            /// <summary>
            /// The confirmation web page key.
            /// </summary>
            public const string ConfirmationWebPage = "ConfirmationWebPage";

            /// <summary>
            /// The confirm account template key.
            /// </summary>
            public const string ConfirmAccountTemplate = "ConfirmAccountTemplate";

            /// <summary>
            /// The return page key.
            /// </summary>
            public const string ReturnPage = "ReturnPage";

            /// <summary>
            /// The cancel page key.
            /// </summary>
            public const string CancelPage = "CancelPage";
        }

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Login";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                RegistrationPageGuid = GetAttributeValue( AttributeKeys.RegistrationPage ).AsGuidOrNull(),
                ForgotPasswordUrl = GetAttributeValue( AttributeKeys.ForgotPasswordUrl ),
                ReturnPageGuid = GetAttributeValue( AttributeKeys.ReturnPage ).AsGuidOrNull(),
                CancelPageGuid = GetAttributeValue( AttributeKeys.CancelPage ).AsGuidOrNull()
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the last login details and any related login facts.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private void UpdateLastLoginDetails( UserLogin userLogin, Guid? personalDeviceGuid, RockContext rockContext )
        {
            // If we have a personal device, then attempt to update it
            // to point at this person unless it already points there.
            if ( personalDeviceGuid.HasValue )
            {
                var personalDevice = new PersonalDeviceService( rockContext ).Get( personalDeviceGuid.Value );

                if ( personalDevice != null && personalDevice.PersonAliasId != userLogin.Person.PrimaryAliasId )
                {
                    personalDevice.PersonAliasId = userLogin.Person.PrimaryAliasId;
                }
            }

            userLogin.LastLoginDateTime = RockDateTime.Now;

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the response to send for a valid log in on mobile.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <param name="rememberMe">if set to <c>true</c> then the login should persist beyond this session.</param>
        /// <returns>The result of the action.</returns>
        private BlockActionResult GetMobileResponse( UserLogin userLogin, bool rememberMe )
        {
            var site = PageCache.Layout.Site;

            var authCookie = Rock.Security.Authorization.GetSimpleAuthCookie( userLogin.UserName, rememberMe, false );

            var mobilePerson = MobileHelper.GetMobilePerson( userLogin.Person, site );
            mobilePerson.AuthToken = authCookie.Value;

            return ActionOk( new
            {
                Person = mobilePerson
            } );
        }

        /// <summary>
        /// Gets the locked out message.
        /// </summary>
        /// <returns>The message to display.</returns>
        protected virtual string GetLockedOutMessage()
        {
            return "Sorry, your account has been locked. Please contact our office for help.";
        }

        /// <summary>
        /// Gets the unconfirmed message.
        /// </summary>
        /// <returns>The message to display.</returns>
        protected virtual string GetUnconfirmedMessage()
        {
            return "Thank you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming. Please click the link in your email and then try logging in again.";
        }

        /// <summary>
        /// Sends the confirmation e-mail for the login.
        /// </summary>
        /// <param name="userLogin">The user login that should receive the confirmation e-mail.</param>
        /// <returns><c>true</c> if the e-mail was sent; otherwise <c>false</c>.</returns>
        private bool SendConfirmation( UserLogin userLogin )
        {
            var systemEmailGuid = GetAttributeValue( AttributeKeys.ConfirmAccountTemplate ).AsGuidOrNull();
            var confirmationWebPage = GetAttributeValue( AttributeKeys.ConfirmationWebPage );
            var confirmationPageGuid = confirmationWebPage.Split( ',' )[0].AsGuidOrNull();
            var confirmationPage = confirmationPageGuid.HasValue ? PageCache.Get( confirmationPageGuid.Value ) : null;

            // Make sure we have the required information.
            if ( !systemEmailGuid.HasValue || confirmationPage == null )
            {
                return false;
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            UserLoginService.SendConfirmationEmail( userLogin, systemEmailGuid.Value, confirmationPage, null, mergeFields );

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Validates the username and password and returns a response that
        /// can by used by the mobile shell.
        /// </summary>
        /// <param name="username">The username to login with.</param>
        /// <param name="password">The password to login with.</param>
        /// <param name="rememberMe">If <c>true</c> then the cookie will persist across sessions.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier making the request.</param>
        /// <returns>The result of the block action.</returns>
        [BlockAction]
        public BlockActionResult MobileLogin( string username, string password, bool rememberMe, Guid? personalDeviceGuid )
        {
            if ( username.IsNullOrWhiteSpace() || password.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Username and password are required." );
            }

            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var (state, userLogin) = userLoginService.GetAuthenticatedUserLogin( username, password );

                if ( state == UserLoginValidationState.Valid )
                {
                    UpdateLastLoginDetails( userLogin, personalDeviceGuid, rockContext );

                    return GetMobileResponse( userLogin, rememberMe );
                }
                else if ( state == UserLoginValidationState.LockedOut )
                {
                    return ActionUnauthorized( GetLockedOutMessage() );
                }
                else if ( state == UserLoginValidationState.NotConfirmed && SendConfirmation( userLogin ) )
                {
                    return ActionUnauthorized( GetUnconfirmedMessage() );
                }
                else
                {
                    return ActionUnauthorized( "We couldn't find an account with that username and password combination." );
                }
            }
        }

        #endregion
    }
}
