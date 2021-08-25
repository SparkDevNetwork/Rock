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
using System.Net;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Allows the user to authenticate.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Login" )]
    [Category( "Obsidian > Security" )]
    [Description( "Allows the user to authenticate." )]
    [IconCssClass( "fa fa-user-lock" )]

    #region Block Attributes

    [LinkedPage( "Registration Page",
        Description = "The page that will be used to register the user.",
        IsRequired = false,
        Key = AttributeKey.RegistrationPage,
        Order = 0 )]

    [UrlLinkField( "Forgot Password URL",
        Description = "The URL to link the user to when they have forgotten their password.",
        IsRequired = false,
        Key = AttributeKey.ForgotPasswordUrl,
        Order = 1 )]

    [CodeEditorField( "Locked Out Caption",
        Description = "The text (HTML) to display when a user's account has been locked.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue =
@"{%- assign phone = Global' | Attribute:'OrganizationPhone' | Trim -%}
{%- assign email = Global' | Attribute:'OrganizationEmail' | Trim -%}
Sorry, your account has been locked.  Please
{% if phone != '' %}
    contact our office at {{ phone }} or email
{% else %}
    email us at
{% endif %}
<a href='mailto:{{ email }}'>{{ email }}</a>
for help. Thank you.",
        Order = 2,
        Key = AttributeKey.LockedOutCaption )]

    [CodeEditorField( "Confirm Caption",
        Description = "The text (HTML) to display when a user's account needs to be confirmed.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "Thank you for logging in, however, we need to confirm the email associated with this account belongs to you. We’ve sent you an email that contains a link for confirming.  Please click the link in your email to continue.",
        Order = 3,
        Key = AttributeKey.ConfirmCaption )]

    [LinkedPage( "Help Page",
        Description = "Page to navigate to when user selects 'Help' option (if blank will use 'ForgotUserName' page route)",
        IsRequired = false,
        DefaultValue = "",
        Order = 4,
        Key = AttributeKey.HelpPage )]

    #endregion

    public class Login : RockObsidianBlockType
    {
        #region Keys

        /// <summary>
        /// The block setting attribute keys for the block.
        /// </summary>
        public static class AttributeKey
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
            /// The locked out caption
            /// </summary>
            public const string LockedOutCaption = "LockedOutCaption";

            /// <summary>
            /// The confirm caption
            /// </summary>
            public const string ConfirmCaption = "ConfirmCaption";

            /// <summary>
            /// The help page
            /// </summary>
            public const string HelpPage = "HelpPage";
        }

        #endregion Keys

        #region IRockObsidianBlockType Implementation

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            return new
            {
                RegistrationPageGuid = GetAttributeValue( AttributeKey.RegistrationPage ).AsGuidOrNull(),
                ForgotPasswordUrl = GetAttributeValue( AttributeKey.ForgotPasswordUrl )
            };
        }

        #endregion

        #region Actions

        /// <summary>
        /// Handles the login action
        /// </summary>
        /// <param name="password"></param>
        /// <param name="rememberMe"></param>
        /// <param name="username"></param>
        [BlockAction]
        public BlockActionResult DoLogin( string username, string password, bool rememberMe )
        {
            if ( username.IsNullOrWhiteSpace() || password.IsNullOrWhiteSpace() )
            {
                return new BlockActionResult( HttpStatusCode.BadRequest )
                {
                    Error = "Username and password are required."
                };
            }

            var userLogin = GetAuthenticatedUserLogin( username, password );

            if ( userLogin == null )
            {
                return new BlockActionResult( HttpStatusCode.Unauthorized )
                {
                    Error = "We couldn’t find an account with that username and password combination."
                };
            }

            if ( userLogin.IsConfirmed != true )
            {
                // TODO - Send the confirmation email

                return new BlockActionResult( HttpStatusCode.Unauthorized )
                {
                    Error = GetUnconfirmedMarkup()
                };
            }

            if ( userLogin.IsLockedOut == true )
            {
                return new BlockActionResult( HttpStatusCode.Unauthorized )
                {
                    Error = GetLockedOutMarkup()
                };
            }

            var authCookie = Rock.Security.Authorization.GetSimpleAuthCookie( username, rememberMe, false );

            if ( authCookie == null )
            {
                return new BlockActionResult( HttpStatusCode.InternalServerError )
                {
                    Error = "An error occurred creating the cookie."
                };
            }

            return new BlockActionResult( HttpStatusCode.Created, new LoginResponse
            {
                AuthCookie = authCookie
            } );
        }

        #endregion Actions

        #region Attribute Helpers

        /// <summary>
        /// Gets the locked out markup.
        /// </summary>
        /// <returns></returns>
        private string GetLockedOutMarkup()
        {
            var mergeFields = LavaHelper.GetCommonMergeFields( null, GetCurrentPerson() );
            return GetAttributeValue( AttributeKey.LockedOutCaption ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the locked out markup.
        /// </summary>
        /// <returns></returns>
        private string GetUnconfirmedMarkup()
        {
            var mergeFields = LavaHelper.GetCommonMergeFields( null, GetCurrentPerson() );
            return GetAttributeValue( AttributeKey.ConfirmCaption ).ResolveMergeFields( mergeFields );
        }

        #endregion Attribute Helpers

        #region Auth Helpers

        /// <summary>
        /// Gets the authenticated user login.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private UserLogin GetAuthenticatedUserLogin( string username, string password )
        {
            var rockContext = new RockContext();
            var userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.GetByUserName( username );

            if ( userLogin?.EntityType == null )
            {
                return null;
            }

            var component = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );

            if ( component?.IsActive != true || !component.Authenticate( userLogin, password ) )
            {
                return null;
            }

            return userLogin;
        }

        #endregion Auth Helpers

        #region Responses

        /// <summary>
        /// A login result object
        /// </summary>
        public class LoginResponse
        {
            /// <summary>
            /// Gets or sets the cookie.
            /// </summary>
            /// <value>
            /// The cookie.
            /// </value>
            public SimpleCookie AuthCookie { get; set; }
        }

        #endregion Responses
    }
}
