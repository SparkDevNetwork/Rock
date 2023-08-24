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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Blocks.Security.Login;
using Rock.Model;
using Rock.Security;
using Rock.Security.Authentication;
using Rock.Security.Authentication.ExternalRedirectAuthentication;
using Rock.Security.Authentication.OneTimePasscode;
using Rock.ViewModels.Blocks.Security.Login;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Allows the user to authenticate.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Login" )]
    [Category( "Security" )]
    [Description( "Allows the user to authenticate." )]
    [IconCssClass( "fa fa-user-lock" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [TextField(
        "Username Field Label",
        Key = AttributeKey.UsernameFieldLabel,
        Description = "The label to use for the username field.  For example, this allows an organization to customize it to 'Username / Email' in cases where both are supported.",
        IsRequired = false,
        DefaultValue = "Username",
        Category = AttributeCategory.Labels,
        Order = 0 )]

    [LinkedPage(
        "New Account Page",
        Description = "The page to navigate to when individual selects 'Create New Account' or if a matching email / phone number could not be found for passwordless sign in (if blank will use 'NewAccountPage' page route).",
        IsRequired = false,
        Key = AttributeKey.NewAccountPage,
        Category = AttributeCategory.Pages,
        Order = 1 )]

    [LinkedPage(
        "Help Page",
        Key = AttributeKey.HelpPage,
        Description = "Page to navigate to when individual selects 'Forgot Account' option (if blank will use 'ForgotUserName' page route).",
        IsRequired = false,
        Category = AttributeCategory.Pages,
        Order = 2 )]

    [CodeEditorField(
        "Confirm Caption",
        Key = AttributeKey.ConfirmCaption,
        Description = "The text (HTML) to display when a individual's account needs to be confirmed.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "Thank you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.",
        Category = AttributeCategory.Captions,
        Order = 3 )]

    [LinkedPage(
        "Confirmation Page",
        Key = AttributeKey.ConfirmationPage,
        Description = "Page for individual to confirm their account (if blank will use 'ConfirmAccount' page route).",
        IsRequired = false,
        Category = AttributeCategory.Pages,
        Order = 4 )]

    [SystemCommunicationField(
        "Confirm Account Template",
        Key = AttributeKey.ConfirmAccountTemplate,
        Description = "Confirm Account Email Template.",
        IsRequired = false,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT,
        Category = AttributeCategory.CommunicationTemplates,
        Order = 5 )]

    [CodeEditorField(
        "Locked Out Caption",
        Key = AttributeKey.LockedOutCaption,
        Description = "The text (HTML) to display when a individual's account has been locked. <span class='tip tip-lava'></span>.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = LockedOutCaptionDefaultValue,
        Category = AttributeCategory.Captions,
        Order = 6 )]

    [BooleanField(
        "Hide New Account Option",
        Key = AttributeKey.HideNewAccountOption,
        Description = "Should 'New Account' option be hidden? For sites that require individual to be in a role (Internal Rock Site for example), individuals shouldn't be able to create their own account.",
        DefaultBooleanValue = false,
        Order = 7 )]

    [TextField(
        "New Account Text",
        Key = AttributeKey.NewAccountButtonText,
        Description = "The text to show on the New Account button.",
        IsRequired = false,
        DefaultValue = "Register",
        Category = AttributeCategory.Labels,
        Order = 8 )]

    [CodeEditorField(
        "No Account Text",
        Key = AttributeKey.NoAccountText,
        Description = "The text to show when no account exists. <span class='tip tip-lava'></span>.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "We couldn't find an account with that username and password combination. Can we help you recover your <a href='{{HelpPage}}'>account information</a>?",
        Category = AttributeCategory.Captions,
        Order = 9 )]

    [CodeEditorField(
        "Remote Authorization Prompt Message",
        Key = AttributeKey.RemoteAuthorizationPromptMessage,
        Description = "Optional text (HTML) to display above remote authorization options.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "Log in with social account",
        Category = AttributeCategory.Captions,
        Order = 10 )]

    [SecondaryAuthsField(
        AttributeName.SecondaryAuthenticationTypes,
        Key = AttributeKey.SecondaryAuthenticationTypes,
        Description = "The active secondary authorization types that should be displayed as options for authentication.",
        IsRequired = false,
        Order = 11 )]

    [BooleanField(
        AttributeName.ShowInternalDatabaseLogin,
        Key = AttributeKey.ShowInternalLogin,
        Description = "Show the internal database (username & password) login.",
        DefaultBooleanValue = true,
        Order = 12 )]

    [BooleanField(
        "Redirect to Single External Auth Provider",
        Key = AttributeKey.RedirectToSingleExternalAuthProvider,
        Description = "Redirect straight to the external authentication provider if only one is configured and internal database login is disabled.",
        DefaultBooleanValue = false,
        Order = 13 )]

    [CodeEditorField(
        "Prompt Message",
        Key = AttributeKey.PromptMessage,
        Description = "Optional text (HTML) to display above username and password fields.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        Category = AttributeCategory.Captions,
        Order = 14 )]

    [LinkedPage(
        "Redirect Page",
        Key = AttributeKey.RedirectPage,
        Description = "Page to navigate to when upon successful log in. The 'returnurl' query string will always override this setting for database authenticated logins. Redirect Page Setting will override third-party authentication 'returnurl'.",
        IsRequired = false,
        Category = AttributeCategory.Pages,
        Order = 15 )]

    [CodeEditorField(
        "Invalid PersonToken Text",
        Key = AttributeKey.InvalidPersonTokenText,
        Description = "The text to show when an individual is logged out due to an invalid persontoken. <span class='tip tip-lava'></span>.",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<div class='alert alert-warning'>The login token you provided is no longer valid. Please log in below.</div>",
        Category = AttributeCategory.Captions,
        Order = 16 )]

    [CodeEditorField(
        "Content Text",
        Key = AttributeKey.ContentText,
        Description = "Lava template to show below the 'Log in' button. <span class='tip tip-lava'></span>.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<div>By signing in, I agree to {{ 'Global' | Attribute:'OrganizationName' }}'s <a href='/terms'>Terms of Use</a> and <a href='/privacy'>Privacy Policy</a>.</div>",
        Category = AttributeCategory.Captions,
        Order = 17 )]

    [CustomRadioListField(
        AttributeName.DefaultLoginMethod,
        Key = AttributeKey.DefaultLoginMethod,
        Description = "The login method that will be shown when the block is loaded.",
        DefaultValue = "0", // LoginMethod.InternalDatabase
        ListSource = "1^Passwordless,0^Internal Database",
        IsRequired = true,
        Order = 18
        )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "D9482EF9-F774-4E37-AC84-8B340CBCA364" )]
    [Rock.SystemGuid.BlockTypeGuid( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3" )]

    public class Login : RockBlockType
    {
        #region Categories

        private static class AttributeCategory
        {
            public const string Captions = "Captions";

            public const string CommunicationTemplates = "Communication Templates";

            public const string Labels = "Labels";

            public const string Pages = "Pages";
        }

        #endregion

        #region Attribute Names

        private static class AttributeName
        {
            /// <summary>
            /// The default login method attribute name.
            /// </summary>
            public const string DefaultLoginMethod = "Default Login Method";

            /// <summary>
            /// The show internal login attribute name.
            /// </summary>
            public const string ShowInternalDatabaseLogin = "Show Internal Database Login";

            /// <summary>
            /// The secondary authentication types attribute name.
            /// </summary>
            public const string SecondaryAuthenticationTypes = "Secondary Authentication Types";
        }

        #endregion

        #region Attribute Keys

        /// <summary>
        /// The block setting attribute keys for the block.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The username field label key
            /// </summary>
            public const string UsernameFieldLabel = "UsernameFieldLabel";

            /// <summary>
            /// The registration page key
            /// </summary>
            public const string NewAccountPage = "NewAccountPage";

            /// <summary>
            /// The forgot account page key
            /// </summary>
            public const string HelpPage = "HelpPage";

            /// <summary>
            /// The confirm caption key
            /// </summary>
            public const string ConfirmCaption = "ConfirmCaption";

            /// <summary>
            /// The confirmation page key
            /// </summary>
            public const string ConfirmationPage = "ConfirmationPage";

            /// <summary>
            /// The confirm account template key
            /// </summary>
            public const string ConfirmAccountTemplate = "ConfirmAccountTemplate";

            /// <summary>
            /// The locked out caption key
            /// </summary>
            public const string LockedOutCaption = "LockedOutCaption";

            /// <summary>
            /// The hide new account key
            /// </summary>
            public const string HideNewAccountOption = "HideNewAccountOption";

            /// <summary>
            /// The new account button text key
            /// </summary>
            public const string NewAccountButtonText = "NewAccountButtonText";

            /// <summary>
            /// The no account text key
            /// </summary>
            public const string NoAccountText = "NoAccountText";

            /// <summary>
            /// The remote authorization prompt message key
            /// </summary>
            public const string RemoteAuthorizationPromptMessage = "RemoteAuthorizationPromptMessage";

            /// <summary>
            /// The secondary authentication types key
            /// </summary>
            public const string SecondaryAuthenticationTypes = "SecondaryAuthenticationTypes";

            /// <summary>
            /// The show internal login key
            /// </summary>
            public const string ShowInternalLogin = "ShowInternalLogin";

            /// <summary>
            /// The redirect to single external authentication provider key
            /// </summary>
            public const string RedirectToSingleExternalAuthProvider = "RedirectToSingleExternalAuthProvider";

            /// <summary>
            /// The prompt message key
            /// </summary>
            public const string PromptMessage = "PromptMessage";

            /// <summary>
            /// The redirect page key
            /// </summary>
            public const string RedirectPage = "RedirectPage";

            /// <summary>
            /// The invalid person token text key
            /// </summary>
            public const string InvalidPersonTokenText = "InvalidPersonTokenText";

            /// <summary>
            /// The content text key
            /// </summary>
            public const string ContentText = "ContentText";

            /// <summary>
            /// The code confirmation page key
            /// </summary>
            public const string CodeConfirmationPage = "CodeConfirmationPage";

            /// <summary>
            /// The default login method key
            /// </summary>
            public const string DefaultLoginMethod = "DefaultLoginMethod";
        }

        #endregion

        #region Constants

        private const string LockedOutCaptionDefaultValue = "{%- assign phone = 'Global' | Attribute:'OrganizationPhone' | Trim -%} Sorry, your account has been locked. Please {% if phone != '' %}contact our office at {{ 'Global' | Attribute:'OrganizationPhone' }} or email{% else %}email us at{% endif %} <a href='mailto:{{ 'Global' | Attribute:'OrganizationEmail' }}'>{{ 'Global' | Attribute:'OrganizationEmail' }}</a> for help. Thank you.";

        /// <summary>
        /// The time limit that a passwordless login code is valid in minutes.
        /// </summary>
        private const int PasswordlessLoginCodeLifetimeInMinutes = 60;

        private static readonly TimeSpan PasswordlessLoginCodeLifetime = TimeSpan.FromMinutes( PasswordlessLoginCodeLifetimeInMinutes );

        #endregion

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string ReturnUrl = "ReturnUrl";

            public const string State = "State";

            public const string Code = "Code";

            public const string IsPasswordless = "IsPasswordless";
        }

        #endregion Page Parameter Keys

        #region IRockObsidianBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var helpPageUrl = this.GetLinkedPageUrl( AttributeKey.HelpPage );
            if ( helpPageUrl.IsNullOrWhiteSpace() )
            {
                helpPageUrl = "/ForgotUserName";
            }

            var secondaryAuthenticationProviders = GetSelectedSecondaryAuthenticationComponents();
            var externalAuthProviders = FilterActiveExternalAuthenticationProviders( secondaryAuthenticationProviders );

            var box = new LoginInitializationBox
            {
                ContentText = ResolveLavaForAttribute( AttributeKey.ContentText, RequestContext.GetCommonMergeFields() ),
                DefaultLoginMethod = ( LoginMethod ) GetAttributeValue( AttributeKey.DefaultLoginMethod ).AsInteger(),
                ExternalAuthProviderButtons = GetExternalAuthProviderButtons( externalAuthProviders ),
                HideNewAccountOption = GetAttributeValue( AttributeKey.HideNewAccountOption ).AsBoolean( false ),
                HelpPageUrl = helpPageUrl,
                IsInternalDatabaseLoginSupported = GetAttributeValue( AttributeKey.ShowInternalLogin ).AsBoolean( true ),
                IsPasswordlessLoginSupported = secondaryAuthenticationProviders.Any( p => p.Component.EntityType.Guid == SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS.AsGuid() ),
                NewAccountPageUrl = GetNewAccountPageUrl( new Dictionary<string, string>
                {
                    { PageParameterKey.ReturnUrl, GetSafeDecodedUrl( PageParameter( PageParameterKey.ReturnUrl ) ) }
                } ),
                NewAccountButtonText = GetAttributeValue( AttributeKey.NewAccountButtonText ),
                PasswordlessAutoVerifyOptions = GetPasswordlessLoginAutoVerifyOptions(),
                PromptMessage = GetAttributeValue( AttributeKey.PromptMessage ),
                RedirectUrl = GetRedirectUrlAfterLogin(),
                RemoteAuthorizationPromptMessage = GetAttributeValue( AttributeKey.RemoteAuthorizationPromptMessage ),
                UsernameFieldLabel = GetAttributeValue( AttributeKey.UsernameFieldLabel ),
            };

            LogInWithExternalAuthProviderIfNeeded( box, externalAuthProviders );
            RedirectToSingleExternalAuthProviderIfNeeded( box, externalAuthProviders );

            ValidateConfiguration( box );

            return box;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Authenticates using database authentication (username & password).
        /// </summary>
        /// <param name="bag">The login request bag.</param>
        [BlockAction]
        public BlockActionResult CredentialLogin( CredentialLoginRequestBag bag )
        {
            if ( !IsRequestValid( bag ) )
            {
                return ActionOk( new CredentialLoginResponseBag
                {
                    ErrorMessage = GetNoAccountMarkup()
                } );
            }

            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.GetByUserName( bag.Username );

                if ( !CanUserLogInWithInternalAuthentication( userLogin, out var authenticationComponent, out var errorMessage, out var isLockedOut ) )
                {
                    return ActionOk( new CredentialLoginResponseBag
                    {
                        ErrorMessage = errorMessage
                    } );
                }

                var isAuthenticationSuccessful = authenticationComponent.AuthenticateAndTrack( userLogin, bag.Password );
                rockContext.SaveChanges();

                if ( !isAuthenticationSuccessful )
                {
                    return ActionOk( new CredentialLoginResponseBag
                    {
                        ErrorMessage = GetNoAccountMarkup()
                    } );
                }

                // Check lockout and confirmation now that the user has authenticated successfully.
                if ( IsUserLockedOut( userLogin, out errorMessage ) )
                {
                    return ActionOk( new CredentialLoginResponseBag
                    {
                        ErrorMessage = errorMessage,
                        IsLockedOut = true
                    } );
                }

                if ( IsUserConfirmationRequired( userLogin, out errorMessage ) )
                {
                    SendConfirmation( userLogin );

                    return ActionOk( new CredentialLoginResponseBag
                    {
                        ErrorMessage = errorMessage,
                        IsConfirmationRequired = true
                    } );
                }

                UserLoginService.UpdateLastLogin( userLogin.UserName );
                Authorization.SetAuthCookie( userLogin.UserName, bag.RememberMe, isImpersonated: false );

                return ActionOk( new CredentialLoginResponseBag
                {
                    IsAuthenticated = true,
                    RedirectUrl = GetRedirectUrlAfterLogin()
                } );
            }
        }

        /// <summary>
        /// Starts a passwordless authentication session that can be verified by calling the <see cref="PasswordlessLoginVerify(PasswordlessLoginVerifyRequestBag)"/> method.
        /// </summary>
        /// <param name="bag">The passwordless login request bag.</param>
        [BlockAction]
        public BlockActionResult PasswordlessLoginStart( PasswordlessLoginStartRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var passwordlessAuthentication = ( PasswordlessAuthentication ) AuthenticationContainer.GetComponent( SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS );

                var result = passwordlessAuthentication.SendOneTimePasscode(
                    new SendOneTimePasscodeOptions
                    {
                        CommonMergeFields = RequestContext.GetCommonMergeFields(),
                        Email = bag.Email,
                        GetLink = ( queryParams ) => RequestContext.RootUrlPath + this.GetCurrentPageUrl( queryParams ),
                        IpAddress = RequestContext.ClientInformation.IpAddress,
                        OtpLifetime = PasswordlessLoginCodeLifetime,
                        PhoneNumber = bag.PhoneNumber,
                        PostAuthenticationRedirectUrl = GetRedirectUrlAfterLogin(),
                        ShouldSendEmailCode = bag.ShouldSendEmailCode,
                        ShouldSendEmailLink = bag.ShouldSendEmailLink,
                        ShouldSendSmsCode = bag.ShouldSendSmsCode
                    },
                    rockContext );

                return ActionOk( new PasswordlessLoginStartResponseBag
                {
                    ErrorMessage = result.ErrorMessage,
                    IsSuccessful = result.IsSuccessful,
                    State = result.State
                } );
            }
        }

        /// <summary>
        /// Verifies a passwordless authentication session and authenticates the individual if successful.
        /// </summary>
        /// <param name="bag">The passwordless authentication verify request bag.</param>
        [BlockAction]
        public BlockActionResult PasswordlessLoginVerify( PasswordlessLoginVerifyRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var passwordlessAuthentication = ( PasswordlessAuthentication ) AuthenticationContainer.GetComponent( SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS );

                var options = new OneTimePasscodeAuthenticationOptions
                {
                    Code = bag.Code,
                    MatchingPersonValue = bag.MatchingPersonValue,
                    State = bag.State,
                };

                var result = passwordlessAuthentication.Authenticate( options );

                return ActionOk( new PasswordlessLoginVerifyResponseBag
                {
                    ErrorMessage = result.ErrorMessage,
                    IsAuthenticated = result.IsAuthenticated,
                    IsPersonSelectionRequired = result.IsPersonSelectionRequired,
                    IsRegistrationRequired = result.IsRegistrationRequired,
                    RegistrationUrl = result.IsRegistrationRequired
                        ? this.GetNewAccountPageUrl( new Dictionary<string, string>
                        {
                            { "State", result.State },
                            { "ReturnUrl", GetRedirectUrlAfterLogin() }
                        } )
                        : null,
                    MatchingPeople = result.MatchingPeopleResults?.Select( p => new ListItemBag
                    {
                        Value = p.State,
                        Text = p.FullName
                    } ).ToList()
                } );
            }
        }

        /// <summary>
        /// Begins an external provider authentication session that will be verified when the external provider redirects back to Rock.
        /// </summary>
        /// <param name="bag">The remote authentication request bag.</param>
        [BlockAction]
        public BlockActionResult RemoteLoginStart( RemoteLoginStartRequestBag bag )
        {
            if ( IsRequestValid( bag ) )
            {
                return ActionBadRequest();
            }

            var authenticationComponent = AuthenticationContainer.GetComponent( bag.AuthenticationType );
            if ( authenticationComponent?.IsActive != true || !( authenticationComponent is IExternalRedirectAuthentication externalRedirectAuthentication ) )
            {
                // The individual selected a remote authentication provider after it was made inactive.
                // Show an error if this unlikely event occurs.
                return ActionBadRequest( "Please try a different authentication method" );
            }

            var loginUrl = externalRedirectAuthentication.GenerateExternalLoginUrl( GetRedirectUri(), GetRedirectUrlAfterLogin() );

            if ( loginUrl == null )
            {
                return ActionInternalServerError( $"ERROR: {authenticationComponent.GetType().Name} does not have a remote log in URL" );
            }

            return ActionOk( new RemoteLoginStartResponseBag
            {
                RedirectUrl = loginUrl.AbsoluteUri
            } );
        }

        #endregion Actions

        #region Private Methods

        /// <summary>
        /// Performs prechecks to see if the user can log in with credentials.
        /// </summary>
        /// <remarks>
        /// Doesn't attempt to sign the user in yet.
        /// </remarks>
        /// <param name="userLogin">The user login.</param>
        /// <param name="authenticationComponent">The authentication component that is set if the user can log in.</param>
        /// <param name="errorMessage">The invalid error message.</param>
        /// <param name="isLockedOut">if set to <c>true</c> is locked out.</param>
        /// <returns>
        ///   <c>true</c> if this user can log in; otherwise, <c>false</c>.
        /// </returns>
        private bool CanUserLogInWithInternalAuthentication( UserLogin userLogin, out AuthenticationComponent authenticationComponent, out string errorMessage, out bool isLockedOut )
        {
            if ( !IsExistingUser( userLogin ) )
            {
                authenticationComponent = null;
                errorMessage = GetNoAccountMarkup();
                isLockedOut = false;
                return false;
            }

            if ( IsUserLockedOut( userLogin, out errorMessage ) )
            {
                authenticationComponent = null;
                isLockedOut = true;
                return false;
            }

            authenticationComponent = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );
            if ( authenticationComponent?.IsActive != true )
            {
                // The authentication provider associated with this username is inactive.
                errorMessage = GetNoAccountMarkup();
                isLockedOut = false;
                return false;
            }

            if ( authenticationComponent.RequiresRemoteAuthentication )
            {
                // A remote authentication provider is associated with this username.
                errorMessage = GetNoAccountMarkup();
                isLockedOut = false;
                return false;
            }

            errorMessage = null;
            isLockedOut = false;
            return true;
        }

        /// <summary>
        /// Gets the enabled secondary authentication providers for this block instance.
        /// </summary>
        /// <returns>The list of authentication components.</returns>
        private List<NamedComponent<AuthenticationComponent>> FilterActiveExternalAuthenticationProviders( List<NamedComponent<AuthenticationComponent>> authenticationComponents )
        {
            // PIN authentication should already be excluded for new block instances,
            // but we need to remove it for existing blocks.
            var excludedAuthProviderTypes = new List<Guid>
            {
                SystemGuid.EntityType.AUTHENTICATION_PIN.AsGuid()
            };

            return authenticationComponents
                .Where( c => c.Component.IsActive )
                .Where( c => c.Component.RequiresRemoteAuthentication )
                .Where( c => !excludedAuthProviderTypes.Contains( c.Component.EntityType.Guid ) )
                .Where( c => c.Component is IExternalRedirectAuthentication )
                .ToList();
        }

        /// <summary>
        /// Gets the authentication components as an enumerable.
        /// </summary>
        /// <returns>The authentication components as an enumerable.</returns>
        private IEnumerable<NamedComponent<AuthenticationComponent>> GetAuthenticationComponents()
        {
            return AuthenticationContainer.Instance
                .Dictionary
                .Values
                .Select( kvp => new NamedComponent<AuthenticationComponent>
                {
                    Name = kvp.Key,
                    Component = ( AuthenticationComponent ) kvp.Value
                } );
        }

        /// <summary>
        /// Gets the redirect URI that can be used by external authentication components to complete authentication.
        /// </summary>
        private string GetRedirectUri()
        {
            var uri = this.RequestContext.RequestUri;
            return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ).EnsureTrailingForwardslash() + $"page/{PageCache.Id}";
        }

        /// <summary>
        /// Gets button configurations for the supplied external auth providers.
        /// </summary>
        /// <param name="externalAuthProviders">The external authentication components.</param>
        /// <returns></returns>
        private List<ExternalAuthenticationButtonBag> GetExternalAuthProviderButtons( List<NamedComponent<AuthenticationComponent>> externalAuthProviders )
        {
            var externalAuthenticationButtons = new List<ExternalAuthenticationButtonBag>();
            foreach ( var component in externalAuthProviders )
            {
                externalAuthenticationButtons.Add( new ExternalAuthenticationButtonBag
                {
                    Text = $"Sign in with {component.Name}",
                    AuthenticationType = component.Component.TypeGuid.ToString(),
                    ImageUrl = this.RequestContext.ResolveRockUrl( component.Component.ImageUrl() ),
                    CssClass = component.Component.LoginButtonCssClass
                } );
            }

            return externalAuthenticationButtons;
        }

        /// <summary>
        /// Gets the merge fields.
        /// </summary>
        /// <param name="additionalMergeFields">Additional merge fields to add to the result.</param>
        /// <returns>The merge fields.</returns>
        private Dictionary<string, object> GetMergeFields( IDictionary<string, object> additionalMergeFields = null )
        {
            var mergeFields = RequestContext.GetCommonMergeFields();

            if ( additionalMergeFields?.Any() == true )
            {
                foreach ( var mergeField in additionalMergeFields )
                {
                    mergeFields.Add( mergeField.Key, mergeField.Value );
                }
            }

            return mergeFields;
        }

        /// <summary>
        /// Gets the new account page URL.
        /// </summary>
        /// <param name="decodedQueryParams">The decoded query parameters.</param>
        /// <returns>The new account page URL.</returns>
        private string GetNewAccountPageUrl( IDictionary<string, string> decodedQueryParams = null )
        {
            var newAccountPageUrl = this.GetLinkedPageUrl( AttributeKey.NewAccountPage, decodedQueryParams );

            if ( newAccountPageUrl.IsNullOrWhiteSpace() )
            {
                var fallbackUrl = "/NewAccount";
                if ( decodedQueryParams?.Any() == true )
                {
                    return $"{fallbackUrl}?{string.Join( "&", decodedQueryParams.Where( kvp => kvp.Key != null && kvp.Value != null ).Select( kvp => $"{Uri.EscapeDataString( kvp.Key )}={Uri.EscapeDataString( kvp.Value )}" ) )}";
                }

                return fallbackUrl;
            }

            return newAccountPageUrl;
        }

        /// <summary>
        /// Gets the "No Account" markup.
        /// </summary>
        /// <returns>The "No Account" markup.</returns>
        private string GetNoAccountMarkup()
        {
            string helpUrl;
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.HelpPage ) ) )
            {
                helpUrl = this.GetLinkedPageUrl( AttributeKey.HelpPage );
            }
            else
            {
                helpUrl = "/ForgotUserName";
            }

            var mergeFields = GetMergeFields( new Dictionary<string, object>
            {
                { AttributeKey.HelpPage, helpUrl }
            } );
            return ResolveLavaForAttribute( AttributeKey.NoAccountText, mergeFields );
        }

        /// <summary>
        /// Gets the options for auto-verifying passwordless authentication.
        /// </summary>
        /// <returns>The options for auto-verifying passwordless authentication.</returns>
        private PasswordlessLoginAutoVerifyOptionsBag GetPasswordlessLoginAutoVerifyOptions()
        {
            if ( !PageParameter( PageParameterKey.IsPasswordless ).AsBoolean() )
            {
                return null;
            }

            return new PasswordlessLoginAutoVerifyOptionsBag
            {
                Code = PageParameter( PageParameterKey.Code ),
                State = PageParameter( PageParameterKey.State )
            };
        }

        /// <summary>
        /// Gets the redirect URL after login.
        /// </summary>
        /// <returns>The redirect URL after login.</returns>
        private string GetRedirectUrlAfterLogin( string thirdPartyReturnUrl = null )
        {
            var returnUrl = GetSafeDecodedUrl( PageParameter( PageParameterKey.ReturnUrl ) );

            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                return returnUrl;
            }

            returnUrl = this.GetLinkedPageUrl( AttributeKey.RedirectPage );

            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                return returnUrl;
            }

            if ( thirdPartyReturnUrl.IsNotNullOrWhiteSpace() )
            {
                return thirdPartyReturnUrl;
            }

            return "/";
        }

        /// <summary>
        /// Returns the decoded URL if it is safe or <c>null</c> if it is not.
        /// </summary>
        /// <returns>The <paramref name="url"/> if it is safe; otherwise <c>null</c>.</returns>
        private string GetSafeDecodedUrl( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return url;
            }

            var decodedUrl = url.GetFullyUrlDecodedValue();

            // Remove the http and https schemes before checking if URL contains XSS objects.
            if ( decodedUrl.Replace( "https://", string.Empty )
                .Replace( "http://", string.Empty )
                .HasXssObjects() )
            {
                return null;
            }

            return decodedUrl;
        }

        /// <summary>
        /// Gets selected secondary authentication components.
        /// </summary>
        /// <remarks>"secondary" means passwordless or external authentication components.</remarks>
        /// <returns>The secondary authentication components.</returns>
        private List<NamedComponent<AuthenticationComponent>> GetSelectedSecondaryAuthenticationComponents()
        {
            var selectedAuthProviderTypes = GetAttributeValue( AttributeKey.SecondaryAuthenticationTypes )
                .SplitDelimitedValues()
                .AsGuidList();

            return GetAuthenticationComponents()
                .Where( c => selectedAuthProviderTypes.Contains( c.Component.EntityType.Guid ) )
                .ToList();
        }

        /// <summary>
        /// Determines whether the specified user is an existing user.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <returns>
        ///   <c>true</c> if the specified user is an existing user; otherwise, <c>false</c>.
        /// </returns>
        private bool IsExistingUser( UserLogin userLogin )
        {
            return userLogin?.EntityType != null;
        }

        /// <summary>
        /// Determines whether the <paramref name="credentialLoginRequestBag"/> is valid.
        /// </summary>
        /// <param name="credentialLoginRequestBag">The credential login request bag.</param>
        /// <returns>
        ///   <c>true</c> if the specified credential login request bag is valid; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsRequestValid( CredentialLoginRequestBag credentialLoginRequestBag )
        {
            return credentialLoginRequestBag != null ||
                 credentialLoginRequestBag.Username.IsNotNullOrWhiteSpace() ||
                 credentialLoginRequestBag.Password.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines whether the <paramref name="remoteLoginStartRequestBag"/> is valid.
        /// </summary>
        /// <param name="remoteLoginStartRequestBag">The remote login start request bag.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>./</returns>
        private bool IsRequestValid( RemoteLoginStartRequestBag remoteLoginStartRequestBag )
        {
            return remoteLoginStartRequestBag == null ||
                remoteLoginStartRequestBag.AuthenticationType.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines whether the <paramref name="authenticationComponent"/> is a supported secondary authentication component.
        /// </summary>
        /// <param name="authenticationComponent">The authentication component.</param>
        /// <returns>
        ///   <c>true</c> if supported; otherwise, <c>false</c>.
        /// </returns>
        private bool IsSupportedSecondaryAuthenticationComponent( AuthenticationComponent authenticationComponent )
        {
            return authenticationComponent is IExternalRedirectAuthentication || authenticationComponent is IOneTimePasscodeAuthentication;
        }

        /// <summary>
        /// Checks if user confirmation is required and sets the appropriate <paramref name="confirmationRequiredMessage"/> if it is.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <param name="confirmationRequiredMessage">The confirmation required message.</param>
        /// <returns>True if user confirmation is required.</returns>
        private bool IsUserConfirmationRequired( UserLogin userLogin, out string confirmationRequiredMessage )
        {
            if ( userLogin.IsConfirmed == false )
            {
                confirmationRequiredMessage = ResolveLavaForAttribute( AttributeKey.ConfirmCaption, RequestContext.GetCommonMergeFields() );
                return true;
            }

            confirmationRequiredMessage = null;
            return false;
        }

        /// <summary>
        /// Checks to see if the user is locked out and sets the appropriate <paramref name="lockedOutMessage"/> if they are.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <param name="lockedOutMessage">The locked out message.</param>
        /// <returns>True if the user is locked out.</returns>
        private bool IsUserLockedOut( UserLogin userLogin, out string lockedOutMessage )
        {
            if ( userLogin.IsLockedOut == true )
            {
                lockedOutMessage = ResolveLavaForAttribute( AttributeKey.LockedOutCaption, RequestContext.GetCommonMergeFields() );
                return true;
            }

            lockedOutMessage = null;
            return false;
        }

        /// <summary>
        /// Handles external authentication if this page was redirected from a 3rd party provider and sets appropriate values on the initialization box.
        /// </summary>
        /// <param name="box">The initialization box.</param>
        /// <param name="externalAuthProviders">The external authentication providers.</param>
        private void LogInWithExternalAuthProviderIfNeeded( LoginInitializationBox box, List<NamedComponent<AuthenticationComponent>> externalAuthProviders )
        {
            var redirectUrl = GetRedirectUri();

            foreach ( var authProvider in externalAuthProviders.Select( c => c.Component ) )
            {
                if ( !( authProvider is IExternalRedirectAuthentication externalRedirectAuthentication ) )
                {
                    continue;
                }

                if ( !externalRedirectAuthentication.IsReturningFromExternalAuthentication( RequestContext.QueryString.ToSimpleQueryStringDictionary() ) )
                {
                    continue;
                }

                var options = new ExternalRedirectAuthenticationOptions
                {
                    RedirectUrl = redirectUrl,
                    // Always recreate the parameters in case an external auth provider modifies this instance.
                    Parameters = RequestContext.QueryString.ToSimpleQueryStringDictionary()
                };

                var result = externalRedirectAuthentication.Authenticate( options );

                if ( result.IsAuthenticated != true )
                {
                    // Although authentication failed for the current auth provider,
                    // we should continue to try others as the IsReturningFromAuthentication return value may have been a false positive.
                    continue;
                }

                var userLogin = new UserLoginService( new RockContext() ).GetByUserName( result.UserName );

                // Check lockout and confirmation now that the user has authenticated successfully.
                if ( IsUserLockedOut( userLogin, out var errorMessage ) )
                {
                    box.ErrorMessage = errorMessage;
                    return;
                }

                if ( IsUserConfirmationRequired( userLogin, out errorMessage ) )
                {
                    SendConfirmation( userLogin );
                    box.ErrorMessage = errorMessage;
                    return;
                }

                UserLoginService.UpdateLastLogin( userLogin.UserName );
                Authorization.SetAuthCookie( userLogin.UserName, isPersisted: true, isImpersonated: false );

                box.ShouldRedirect = true;
                box.RedirectUrl = GetRedirectUrlAfterLogin( result.ReturnUrl );
                return;
            }

            box.ShouldRedirect = false;
            return;
        }

        /// <summary>
        /// Checks if the client should redirect to a single external auth provider and sets appropriate values on the initialization.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="externalAuthProviders">The external authentication providers.</param>
        private void RedirectToSingleExternalAuthProviderIfNeeded( LoginInitializationBox box, List<NamedComponent<AuthenticationComponent>> externalAuthProviders )
        {
            // Short-circuit if we are already planning to redirect the client somewhere else.
            if ( box.ShouldRedirect )
            {
                return;
            }

            // Check if we need to redirect to an external auth provider.
            if ( externalAuthProviders.Count == 1
                 && !box.IsInternalDatabaseLoginSupported
                 && GetAttributeValue( AttributeKey.RedirectToSingleExternalAuthProvider ).AsBoolean() )
            {
                var singleAuthProvider = externalAuthProviders.First();

                if ( !(singleAuthProvider is IExternalRedirectAuthentication externalRedirectAuthentication) )
                {
                    box.ErrorMessage = "Please try a different authentication method.";
                    return;
                }

                var authLoginUri = externalRedirectAuthentication.GenerateExternalLoginUrl( GetRedirectUri(), GetRedirectUrlAfterLogin() ).AbsoluteUri;

                if ( authLoginUri.IsNotNullOrWhiteSpace() )
                {
                    if ( BlockCache.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) )
                    {
                        box.ErrorMessage = $"If you did not have Administrate permissions on this block, you would have been redirected to the <a href='{authLoginUri}'>{singleAuthProvider.Name}</a> url.";
                    }
                    else
                    {
                        box.ShouldRedirect = true;
                        box.RedirectUrl = authLoginUri;
                    }
                }
            }
        }

        /// <summary>
        /// Resolves lava for an attribute.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns>The resolved lava string.</returns>
        private string ResolveLavaForAttribute( string attributeKey, IDictionary<string, object> mergeFields)
        {
            return GetAttributeValue( attributeKey ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Sends the confirmation.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        private void SendConfirmation( UserLogin userLogin )
        {
            try
            {
                var url = this.GetLinkedPageUrl( AttributeKey.ConfirmationPage );
                if ( url.IsNullOrWhiteSpace() )
                {
                    url = "/ConfirmAccount";
                }

                var mergeFields = GetMergeFields( new Dictionary<string, object>
                {
                    { "ConfirmAccountUrl", RequestContext.RootUrlPath + url.TrimStart( '/' ) },
                    { "Person", userLogin.Person },
                    { "User", userLogin }
                } );

                var message = new RockEmailMessage( GetAttributeValue( AttributeKey.ConfirmAccountTemplate ).AsGuid() );
                message.SetRecipients( new List<RockEmailMessageRecipient>
                {
                    new RockEmailMessageRecipient( userLogin.Person, mergeFields )
                } );
                message.AppRoot = "/";
                message.ThemeRoot = this.RequestContext.ResolveRockUrl( "~~/" );
                message.CreateCommunicationRecord = false;
                message.Send();
            }
            catch (SystemException ex )
            {
                ExceptionLogService.LogException(ex );
            }
        }

        /// <summary>
        /// Validates the block initialization box and adds validation errors to <see cref="LoginInitializationBox.ConfigurationErrors"/>.
        /// </summary>
        /// <param name="box">The box to validate.</param>
        private void ValidateConfiguration( LoginInitializationBox box )
        {
            var configurationErrors = new List<string>();

            if ( box.IsInternalDatabaseLoginSupported || box.IsPasswordlessLoginSupported )
            {
                if ( box.DefaultLoginMethod == LoginMethod.InternalDatabase && !box.IsInternalDatabaseLoginSupported )
                {
                    configurationErrors.Add( $@"{AttributeName.ShowInternalDatabaseLogin} needs to be enabled to use Internal Database as the {AttributeName.DefaultLoginMethod}." );
                }

                if ( box.DefaultLoginMethod == LoginMethod.Passwordless && !box.IsPasswordlessLoginSupported )
                {
                    configurationErrors.Add( $@"Passwordless needs to be enabled in {AttributeName.SecondaryAuthenticationTypes} to use Passwordless as the {AttributeName.DefaultLoginMethod}." );
                }
            }

            if ( box.IsPasswordlessLoginSupported )
            {
                ValidatePasswordlessLoginConfiguration( box, configurationErrors );
            }

            // Inform the admin if there are selected authentication components that are not supported.
            foreach ( var authenticationComponent in GetSelectedSecondaryAuthenticationComponents() )
            {
                if ( !IsSupportedSecondaryAuthenticationComponent( authenticationComponent.Component ) )
                {
                    configurationErrors.Add( $"{authenticationComponent.Name} is not supported by this block." );
                }
            }

            box.ConfigurationErrors = configurationErrors.Any() ? configurationErrors : null;
        }

        /// <summary>
        /// Validates the passwordless authentication configuration and adds configuration errors to <paramref name="configurationErrors"/>.
        /// </summary>
        /// <param name="box">The login initialization box to validate.</param>
        /// <param name="configurationErrors">The configuration errors.</param>
        private void ValidatePasswordlessLoginConfiguration( LoginInitializationBox box, List<string> configurationErrors )
        {
            var securitySettings = new SecuritySettingsService().SecuritySettings;

            var passwordlessSignInSystemCommunication = new SystemCommunicationService( new RockContext() ).Get( securitySettings.PasswordlessConfirmationCommunicationTemplateGuid );

            if ( passwordlessSignInSystemCommunication == null )
            {
                configurationErrors.Add( "A Passwordless Confirmation Communication Template needs to be configured in your security settings to use passwordless login." );
            }
            else
            {
                if ( passwordlessSignInSystemCommunication.IsActive != true )
                {
                    configurationErrors.Add( $@"The {passwordlessSignInSystemCommunication.Title} system communication needs to be active to use passwordless authentication." );

                    // Disable passwordless authentication because the passwordless system communication is inactive.
                    box.IsPasswordlessLoginSupported = false;
                }

                if ( !passwordlessSignInSystemCommunication.SmsFromSystemPhoneNumberId.HasValue )
                {
                    configurationErrors.Add( $@"The { passwordlessSignInSystemCommunication.Title } system communication needs an SMS From value to use passwordless authentication." );

                    // Disable passwordless authentication because the passwordless system communication doesn't have an SMS From value.
                    box.IsPasswordlessLoginSupported = false;
                }
            }
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// <see cref="Extension.Component"/> wrapper that provides the component name.
        /// </summary>
        private class NamedComponent<T> where T : Extension.Component
        {
            /// <summary>
            /// Gets or sets the component name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the component.
            /// </summary>
            public T Component { get; set; }
        }

        #endregion
    }
}
