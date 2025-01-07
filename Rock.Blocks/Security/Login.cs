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
using Rock.IpAddress;
using Rock.Logging;
using Rock.Model;
using Rock.Security;
using Rock.Security.Authentication;
using Rock.Security.Authentication.ExternalRedirectAuthentication;
using Rock.Security.Authentication.OneTimePasscode;
using Rock.Utility.Enums;
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
        Description = "The text (HTML) to display when a individual's account has been locked. <span class='tip tip-lava'></span>",
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
        Description = "The text to show when no account exists. <span class='tip tip-lava'></span>",
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
        Description = "The text to show when an individual is logged out due to an invalid persontoken. <span class='tip tip-lava'></span>",
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
        Description = "Lava template to show below the 'Log in' button. <span class='tip tip-lava'></span>",
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
        Order = 18 )]
    
    [CodeEditorField(
        "Two-Factor Email or Mobile Phone Required",
        Key = AttributeKey.TwoFactorEmailPhoneRequired,
        Description = "Lava template to show when email or mobile phone is required for 2FA. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<div class=\"alert alert-warning\">Your current security access level requires you to complete a Two-Factor Authentication login in order to proceed. This additional layer of security is necessary to ensure the protection of your account and the sensitive data it contains.<br><br>To continue, please provide your email or mobile phone below.</div>",
        Category = AttributeCategory.Captions,
        Order = 19 )]
    
    [CodeEditorField(
        "Two-Factor Email and Mobile Phone Not Available",
        Key = AttributeKey.TwoFactorEmailPhoneNotAvailable,
        Description = "Lava template to show when email or mobile phone is required for 2FA but missing on the account. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<div class=\"alert alert-warning\">Your current security access level requires you to complete a Two-Factor Authentication login in order to proceed. This additional layer of security is necessary to ensure the protection of your account and the sensitive data it contains.<br><br>Your account does not currently have an email address or mobile phone. Please contact us to assist you in configuring this.</div>",
        Category = AttributeCategory.Captions,
        Order = 20 )]
    
    [CodeEditorField(
        "Two-Factor Login Required",
        Key = AttributeKey.TwoFactorLoginRequired,
        Description = "Lava template to show when database login is required for 2FA. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<div class=\"alert alert-warning\">Your current security access level requires you to complete a Two-Factor Authentication login in order to proceed. This additional layer of security is necessary to ensure the protection of your account and the sensitive data it contains.<br><br>To continue, please provide your email or mobile phone below.</div>",
        Category = AttributeCategory.Captions,
        Order = 21 )]
    
    [CodeEditorField(
        "Two-Factor Login Not Available",
        Key = AttributeKey.TwoFactorLoginNotAvailable,
        Description = "Lava template to show when database login is required for 2FA but not available. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<div class=\"alert alert-warning\">Your current security access level requires you to complete a Two-Factor Authentication login in order to proceed. This additional layer of security is necessary to ensure the protection of your account and the sensitive data it contains.<br><br>Your account does not currently have a username or password configured. Please contact us to assist you in configuring this.</div>",
        Category = AttributeCategory.Captions,
        Order = 22 )]
    
    [CodeEditorField(
        "Two-Factor Not Supported by Authorization Component",
        Key = AttributeKey.TwoFactorNotSupportedByAuthenticationMethod,
        Description = "Lava template to show when 2FA is not supported by the authentication method. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<div class=\"alert alert-warning\">Your login attempt requires Two-Factor Authentication for enhanced security. However, the authentication method you used does not support 2FA. Please use a supported method or contact us for assistance.</div>",
        Category = AttributeCategory.Captions,
        Order = 23 )]

    [SystemCommunicationField(
        "Login Confirmation Alert System Communication",
        Key = AttributeKey.LoginConfirmationAlertSystemCommunication,
        Description = "The system communication to use for sending login confirmation alerts when a user successfully logs in using a new browser. Merge fields include: UserAgent, IPAddress, LoginDateTime, Location, etc.<span class='tip tip-lava'></span>",
        DefaultSystemCommunicationGuid = SystemGuid.SystemCommunication.LOGIN_CONFIRMATION_ALERT,
        IsRequired = true,
        Category = AttributeCategory.CommunicationTemplates,
        Order = 24 )]

    [EnumsField(
        "Account Protection Profiles for Login Confirmation Alerts",
        Key = AttributeKey.AccountProtectionProfilesForLoginConfirmationAlerts,
        IsRequired = false,
        EnumSourceType = typeof( AccountProtectionProfile ),
        Category = AttributeCategory.CommunicationTemplates,
        Order = 25 )]

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

            /// <summary>
            /// The two-factor email phone required message
            /// </summary>
            public const string TwoFactorEmailPhoneRequired = "TwoFactorEmailPhoneRequired";

            /// <summary>
            /// The two-factor email phone not available message
            /// </summary>
            public const string TwoFactorEmailPhoneNotAvailable = "TwoFactorEmailPhoneNotAvailable";

            /// <summary>
            /// The two-factor login required message
            /// </summary>
            public const string TwoFactorLoginRequired = "TwoFactorLoginRequired";

            /// <summary>
            /// The two-factor login not available message
            /// </summary>
            public const string TwoFactorLoginNotAvailable = "TwoFactorLoginNotAvailable";

            /// <summary>
            /// The two factor not supported by authentication method message
            /// </summary>
            public const string TwoFactorNotSupportedByAuthenticationMethod = "TwoFactorNotSupportedByAuthenticationMethod";

            /// <summary>
            /// Login Confirmation Alert System Communication
            /// </summary>
            public const string LoginConfirmationAlertSystemCommunication = "LoginConfirmationAlertSystemCommunication";

            /// <summary>
            /// Account Protection Profiles for Login Confirmation Alerts
            /// </summary>
            public const string AccountProtectionProfilesForLoginConfirmationAlerts = "AccountProtectionProfilesForLoginConfirmationAlerts";
        }

        #endregion

        #region Constants

        private const string LockedOutCaptionDefaultValue = "{%- assign phone = 'Global' | Attribute:'OrganizationPhone' | Trim -%} Sorry, your account has been locked. Please {% if phone != '' %}contact our office at {{ 'Global' | Attribute:'OrganizationPhone' }} or email{% else %}email us at{% endif %} <a href='mailto:{{ 'Global' | Attribute:'OrganizationEmail' }}'>{{ 'Global' | Attribute:'OrganizationEmail' }}</a> for help. Thank you.";

        /// <summary>
        /// The time limit that a passwordless login code is valid in minutes.
        /// </summary>
        private const int PasswordlessLoginCodeLifetimeInMinutes = 60;

        private static readonly TimeSpan PasswordlessLoginCodeLifetime = TimeSpan.FromMinutes( PasswordlessLoginCodeLifetimeInMinutes );

        /// <summary>
        /// The number of authentication factors required for two-factor authentication.
        /// </summary>
        private const int TwoFactorAuthenticationFactorCount = 2;

        #endregion

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string ReturnUrl = "ReturnUrl";

            public const string State = "State";

            public const string Code = "Code";

            public const string IsPasswordless = "IsPasswordless";

            public const string AreUsernameAndPasswordRequired = "AreUsernameAndPasswordRequired";
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
            var mergeFields = GetMergeFields();

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
                TwoFactorEmailPhoneRequiredMessage = GetAttributeValue( AttributeKey.TwoFactorEmailPhoneRequired ).ResolveMergeFields( mergeFields ),
                TwoFactorEmailPhoneNotAvailableMessage = GetAttributeValue( AttributeKey.TwoFactorEmailPhoneNotAvailable ).ResolveMergeFields( mergeFields ),
                TwoFactorLoginRequiredMessage = GetAttributeValue( AttributeKey.TwoFactorLoginRequired ).ResolveMergeFields( mergeFields ),
                TwoFactorLoginNotAvailableMessage = GetAttributeValue( AttributeKey.TwoFactorLoginNotAvailable ).ResolveMergeFields( mergeFields ),
                TwoFactorNotSupportedByAuthenticationMethodMessage = GetAttributeValue( AttributeKey.TwoFactorNotSupportedByAuthenticationMethod ).ResolveMergeFields( mergeFields ),
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
                return ActionOk( ResponseHelper.CredentialLogin.Error( GetNoAccountMarkup() ) );
            }

            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.GetByUserName( bag.Username );

                if ( !CanUserLogInWithInternalAuthentication( userLogin, out var authenticationComponent, out var errorMessage, out var isLockedOut ) )
                {
                    if ( isLockedOut )
                    {
                        return ActionOk( ResponseHelper.CredentialLogin.LockedOut( errorMessage ) );
                    }
                    else
                    {
                        return ActionOk( ResponseHelper.CredentialLogin.Error( errorMessage ) );
                    }
                }

                // Check if the credentials are valid (does not authenticate in Rock).
                var isAuthenticationSuccessful = authenticationComponent.AuthenticateAndTrack( userLogin, bag.Password );
                rockContext.SaveChanges();

                if ( !isAuthenticationSuccessful )
                {
                    // If the credentials were invalid then show an error.
                    return ActionOk( ResponseHelper.CredentialLogin.Error( GetNoAccountMarkup() ) );
                }
                else if ( IsUserLockedOut( userLogin, out errorMessage ) )
                {
                    // If the credentials are valid and the user is locked out then show an error.
                    return ActionOk( ResponseHelper.CredentialLogin.LockedOut( errorMessage ) );
                }
                else if ( IsUserConfirmationRequired( userLogin, out errorMessage ) )
                {
                    // If the credentials are valid and the confirmation is required
                    // then send a new confirmation communication and show an error.
                    SendConfirmation( userLogin );
                    return ActionOk( ResponseHelper.CredentialLogin.ConfirmationRequired( errorMessage ) );
                }
                else if ( !IsTwoFactorAuthenticationRequired( userLogin.Person ) )
                {
                    // Authenticate the user in Rock if credentials were valid and 2FA is not required.
                    Authenticate(
                        userLogin,
                        bag.RememberMe,
                        isTwoFactorAuthenticated: false );
                    return ActionOk( ResponseHelper.CredentialLogin.Authenticated( GetRedirectUrlAfterLogin() ) );
                }
                else
                {
                    var passwordlessValidation = ValidatePasswordlessLoginConfiguration();

                    if ( !passwordlessValidation.IsValid )
                    {
                        // 2FA is required but passwordless login is misconfigured.
                        // Authenticate the user in Rock as if 2FA occurred while logging
                        // an error so administrators can fix the issue without inhibiting a successful login.
                        var errors = passwordlessValidation.GetErrorMessages();
                        RockLogger.Log.Error( RockLogDomains.Core, "Two-Factor Authentication is required but Passwordless authentication is not configured {Errors}.", errors.JoinStrings( " " ) );

                        Authenticate(
                            userLogin,
                            bag.RememberMe,
                            isTwoFactorAuthenticated: true );
                        return ActionOk( ResponseHelper.CredentialLogin.Authenticated( GetRedirectUrlAfterLogin() ) );
                    }
                }

                // Otherwise, handle the two-factor authentication.
                
                var personAliasId = userLogin.Person.PrimaryAliasId;
                var mfaTicket = MultiFactorAuthenticationTicket.Decrypt( bag.MfaTicket );

                if ( mfaTicket == null )
                {
                    // Issue a new MFA ticket for the credential-authenticated person.
                    // The settings used for the auth cookie should be based on this authentication factor.
                    var authCookieSettings = new AuthCookieSettings
                    {
                        // Set ExpiresIn = null to use forms authentication expiration.
                        ExpiresIn = null,
                        IsPersisted = bag.RememberMe
                    };
                    mfaTicket = new MultiFactorAuthenticationTicket( personAliasId, TwoFactorAuthenticationFactorCount, authCookieSettings );
                }
                else if ( mfaTicket.PersonAliasId != personAliasId )
                {
                    // The person on the original MFA ticket does not match the person who just authenticated.
                    return ActionOk( ResponseHelper.CredentialLogin.Error( "The credentials entered are for a different account. Please try again with different credentials." ) );
                }
                else if ( mfaTicket.IsExpired )
                {
                    // The MFA ticket is expired.
                    return ActionOk( ResponseHelper.CredentialLogin.Error( "The two-factor authentication session has expired. Please sign in again." ) );
                }

                // Add Database as a successful authentication factor.
                mfaTicket.AddSuccessfulAuthenticationFactor( SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() );

                // Authenticate the user if the ticket has the minimum required factors.
                if ( mfaTicket.HasMinimumRequiredFactors )
                {
                    Authenticate(
                        userLogin,
                        mfaTicket.AuthCookieSettings.IsPersisted,
                        isTwoFactorAuthenticated: true,
                        mfaTicket.AuthCookieSettings.ExpiresIn );
                    return ActionOk( ResponseHelper.CredentialLogin.Authenticated( GetRedirectUrlAfterLogin() ) );
                }

                // Passwordless authentication should be the next authentication factor.
                // Check if the Person associated with the database login has an email or mobile phone.
                var isEmailAndMobilePhoneMissing = userLogin.Person.Email.IsNullOrWhiteSpace()
                    && userLogin.Person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ) == null;

                return ActionOk( ResponseHelper.CredentialLogin.MfaRequired( mfaTicket.Encrypt(), isEmailAndMobilePhoneMissing ) );
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
                
                if ( passwordlessAuthentication == null )
                {
                    return ActionOk( new PasswordlessLoginStartResponseBag
                    {
                        ErrorMessage = "The Passwordless Authentication service needs to be active to use passwordless login."
                    } );
                }

                var result = passwordlessAuthentication.SendOneTimePasscode(
                    new SendOneTimePasscodeOptions
                    {
                        CommonMergeFields = RequestContext.GetCommonMergeFields(),
                        Email = bag.Email,
                        GetLink = ( queryParams ) =>
                        {
                            var parameters = queryParams != null ? new Dictionary<string, string>( queryParams ) : new Dictionary<string, string>();

                            // Add the MFA ticket as a query parameter so that the MFA ticket can be retrieved from the generated link.
                            if ( bag.MfaTicket?.IsNotNullOrWhiteSpace() == true )
                            {
                                parameters.Add( "Mfa", bag.MfaTicket );
                            }

                            return RequestContext.RootUrlPath + this.GetCurrentPageUrl( parameters );
                        },
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
            // Validate that passwordless login CAN BE USED before processing.
            // Passwordless login can only be used when:
            // 1. the Passwordless block setting is enabled
            // or
            // 2. as a second factor for 2FA
            var isPasswordlessLoginEnabled = GetSelectedSecondaryAuthenticationComponents().Any( p => p.Component.EntityType.Guid == SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS.AsGuid() );
            var mfaTicket = MultiFactorAuthenticationTicket.Decrypt( bag?.MfaTicket );
            var isFromMfa = mfaTicket != null;

            if ( !isPasswordlessLoginEnabled && !isFromMfa )
            {
                // A passwordless confirmation link was clicked after Passwordless was disabled in the block.
                return ActionOk( ResponseHelper.PasswordlessLogin.Error( "Passwordless needs to be enabled to use passwordless login." ) );
            }
            else if ( isFromMfa )
            {
                // Validate the multi-factor authentication ticket.
                // The ticket should only exist at this point if passwordless is a secondary authentication factor.
                if ( mfaTicket.IsExpired )
                {
                    return ActionOk( ResponseHelper.PasswordlessLogin.Error( "The two-factor authentication session has expired. Please sign in again." ) );
                }
                else if ( mfaTicket.HasFactor( SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS.AsGuid() ) )
                {
                    // Passwordless login has already been used for this MFA ticket.
                    return ActionOk( ResponseHelper.PasswordlessLogin.Error( "Passwordless login can only be used once for two-factor authentication. Please sign in again." ) );
                }
                else if ( mfaTicket.SuccessfulAuthenticationFactorCount == 0 )
                {
                    // MFA tickets should be issued with at least one successful authentication factor.
                    return ActionOk( ResponseHelper.PasswordlessLogin.Error( "The two-factor authentication session is invalid. Please sign in again." ) );
                }
            }

            var passwordlessAuthentication = ( PasswordlessAuthentication ) AuthenticationContainer.GetComponent( SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS );

            if ( passwordlessAuthentication == null )
            {
                return ActionOk( ResponseHelper.PasswordlessLogin.Error( "The Passwordless Authentication service needs to be active to use passwordless login." ) );
            }

            var options = new OneTimePasscodeAuthenticationOptions
            {
                Code = bag.Code,
                MatchingPersonValue = bag.MatchingPersonValue,
                State = bag.State
            };

            // Authenticates the end-user with the passwordless provider (does not authenticate in Rock yet).
            // This will create a new UserLogin if successful and using passwordless authentication for the first time.
            var passwordlessAuthenticationResult = passwordlessAuthentication.Authenticate( options );

            if ( passwordlessAuthenticationResult.IsPersonSelectionRequired )
            {
                // When the passwordless code is valid but the email or mobile phone matched multiple people,
                // then the matching person must be selected by the end-user.
                var matchingPeople = passwordlessAuthenticationResult.MatchingPeopleResults?.Select( p => new ListItemBag
                {
                    Value = p.State,
                    Text = p.FullName
                } ).ToList();
                return ActionOk( ResponseHelper.PasswordlessLogin.PersonSelectionRequired( matchingPeople ) );
            }
            else if ( passwordlessAuthenticationResult.IsRegistrationRequired )
            {
                if ( isFromMfa )
                {
                    // The passwordless code was valid but the email or phone didn't match an existing person in Rock.
                    // Treat this like credentials for a different account were used.
                    return ActionOk( ResponseHelper.PasswordlessLogin.MfaMismatchedAccounts() );
                }
                else
                {
                    // When the passwordless code is valid but a Person doesn't exist for the email or mobile phone,
                    // then the end-user must create a new account (the created account will have a protection profile of Medium).
                    // If 2FA is required for protection profile Medium,
                    // then registration should also require username and password as they will be used for 2FA.
                    var areUsernameAndPasswordRequiredForRegistration = IsTwoFactorAuthenticationRequired( AccountProtectionProfile.Medium );
                    return ActionOk(
                        ResponseHelper.PasswordlessLogin.RegistrationRequired(
                            GetNewAccountPageUrl( passwordlessAuthenticationResult.State, areUsernameAndPasswordRequiredForRegistration )
                        )
                    );
                }
            }
            else if ( !passwordlessAuthenticationResult.IsAuthenticated || passwordlessAuthenticationResult.AuthenticatedUser == null )
            {
                // If the passwordless provider authentication was unsuccessful then show an error to the end-user.
                return ActionOk( ResponseHelper.PasswordlessLogin.Error( passwordlessAuthenticationResult.ErrorMessage ) );
            }
            else if ( IsUserLockedOut( passwordlessAuthenticationResult.AuthenticatedUser, out var lockedOutMessage ) )
            {
                return ActionOk( ResponseHelper.PasswordlessLogin.Error( lockedOutMessage ) );
            }
            else if ( !IsTwoFactorAuthenticationRequired( passwordlessAuthenticationResult.AuthenticatedUser.Person ) )
            {
                // Passwordless login was successful and 2FA is not required...

                // Ensure the account is confirmed.
                if ( IsUserConfirmationRequired( passwordlessAuthenticationResult.AuthenticatedUser, out var _ ) )
                {
                    ConfirmUserLogin( passwordlessAuthenticationResult.AuthenticatedUser.Id );
                }

                // Authenticate the user in Rock.
                Authenticate(
                    passwordlessAuthenticationResult.AuthenticatedUser,
                    isPersisted: true,
                    isTwoFactorAuthenticated: false,
                    expiresIn: TimeSpan.FromMinutes( new SecuritySettingsService().SecuritySettings.PasswordlessSignInSessionDuration ) );

                return ActionOk( ResponseHelper.PasswordlessLogin.Authenticated() );
            }

            // Otherwise, handle the two-factor authentication.

            var personAliasId = passwordlessAuthenticationResult.AuthenticatedUser.Person.PrimaryAliasId;

            if ( mfaTicket == null )
            {
                // Issue a new MFA ticket for the passwordless-authenticated person.
                // The settings used for the auth cookie should be based on this authentication factor.
                var authCookieSettings = new AuthCookieSettings
                {
                    ExpiresIn = TimeSpan.FromMinutes( new SecuritySettingsService().SecuritySettings.PasswordlessSignInSessionDuration ),
                    IsPersisted = true,
                };
                mfaTicket = new MultiFactorAuthenticationTicket( personAliasId, TwoFactorAuthenticationFactorCount, authCookieSettings );
            }
            else if ( mfaTicket.PersonAliasId != personAliasId )
            {
                // The person on the original MFA ticket does not match the person who just authenticated.
                return ActionOk( ResponseHelper.PasswordlessLogin.MfaMismatchedAccounts() );
            }
            else if ( mfaTicket.IsExpired )
            {
                // The MFA ticket is expired.
                return ActionOk( ResponseHelper.PasswordlessLogin.Error( "The two-factor authentication session has expired. Please sign in again." ) );
            }

            // Ensure the account is confirmed.
            if ( IsUserConfirmationRequired( passwordlessAuthenticationResult.AuthenticatedUser, out var _ ) )
            {
                ConfirmUserLogin( passwordlessAuthenticationResult.AuthenticatedUser.Id );
            }

            // Add Passwordless as a successful authentication factor.
            mfaTicket.AddSuccessfulAuthenticationFactor( SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS.AsGuid() );

            // Authenticate the user if the ticket has the minimum required factors.
            if ( mfaTicket.HasMinimumRequiredFactors )
            {
                Authenticate(
                    passwordlessAuthenticationResult.AuthenticatedUser,
                    mfaTicket.AuthCookieSettings.IsPersisted,
                    isTwoFactorAuthenticated: true,
                    mfaTicket.AuthCookieSettings.ExpiresIn );
                return ActionOk( ResponseHelper.PasswordlessLogin.Authenticated() );
            }

            // Database (username & password) authentication should be the next authentication factor.
            // Check if the Person associated with the passwordless login has a Database login.
            var isUsernameAndPasswordMissing = false;
            var databaseEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() );

            if ( databaseEntityTypeId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var userLoginService = new UserLoginService( rockContext );
                    isUsernameAndPasswordMissing = !userLoginService
                        .GetByPersonId( passwordlessAuthenticationResult.AuthenticatedUser.Person.Id )
                        .Where( userLogin => userLogin.EntityTypeId == databaseEntityTypeId.Value )
                        .Any();
                }
            }

            return ActionOk( ResponseHelper.PasswordlessLogin.MfaRequired( mfaTicket.Encrypt(), isUsernameAndPasswordMissing ) );
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

            // Use the route passed from the client to build the redirect URI.
            // The page route is not currently available within a block action.
            var loginUrl = externalRedirectAuthentication.GenerateExternalLoginUrl( GetRedirectUri( bag.Route ), GetRedirectUrlAfterLogin() );

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
        /// Authenticates a Rock end-user.
        /// </summary>
        /// <param name="userLogin">The <see cref="UserLogin"> associated with the account to authenticate.</param>
        /// <param name="isPersisted">Whether the individual should be authenticated across browsing sessions.</param>
        /// <param name="isTwoFactorAuthenticated">Whether the individual is two-factor authenticated.</param>
        /// <param name="expiresIn">The duration that the authentication is valid.</param>
        private void Authenticate( UserLogin userLogin, bool isPersisted, bool isTwoFactorAuthenticated, TimeSpan? expiresIn = null )
        {
            UserLoginService.UpdateLastLogin( userLogin.UserName );

            if ( expiresIn.HasValue )
            {
                Authorization.SetAuthCookie( userLogin.UserName, isPersisted, isImpersonated: false, isTwoFactorAuthenticated, expiresIn.Value );
            }
            else
            {
                Authorization.SetAuthCookie( userLogin.UserName, isPersisted, isImpersonated: false, isTwoFactorAuthenticated );
            }

            CheckBrowserRecognition( userLogin.Person );
        }

        /// <summary>
        /// Checks to see if the a <see cref="Person"/> who has just authenticated is using a previously recognized browser and sends an alert if appropriate.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> who just authenticated.</param>
        private void CheckBrowserRecognition( Person person )
        {
            var personRecognitionValue = Rock.Utility.GuidHelper.ToShortString( person.Guid );

            if ( !Authorization.IsBrowserRecognized( personRecognitionValue ) )
            {
                var profilesForNotification = GetAttributeValue( AttributeKey.AccountProtectionProfilesForLoginConfirmationAlerts );
                if ( string.IsNullOrEmpty( profilesForNotification ) )
                {
                    return;
                }

                foreach ( var profile in profilesForNotification.SplitDelimitedValues() )
                {
                    if ( !string.IsNullOrWhiteSpace( profile ) )
                    {
                        var profileType = profile.ConvertToEnum<AccountProtectionProfile>();
                        if ( person.AccountProtectionProfile == profileType )
                        {
                            SendLoginConfirmationAlert( person );
                            break;
                        }
                    }
                }    
            }
        }

        /// <summary>
        /// Sends the configured login confirmation alert.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> who just authenticated.</param>
        private void SendLoginConfirmationAlert( Person person )
        {
            var systemCommunicationGuid = GetAttributeValue( AttributeKey.LoginConfirmationAlertSystemCommunication ).AsGuidOrNull();
            if ( !systemCommunicationGuid.HasValue )
            {
                return;
            }

            var systemCommunication = new SystemCommunicationService( new RockContext() ).Get( systemCommunicationGuid.Value );
            if ( systemCommunication == null )
            {
                RockLogger.Log.Error( RockLogDomains.Core, $"Login Confirmation Alert could not be sent for person {person.Id} because the System Communication {systemCommunicationGuid} does not exist. Please check the login block configuration." );
            }

            var location = string.Empty;

            // If an active IP Address Lookup component is configured, try using it to look up the location.
            var ipLookupComponent = IpAddressLookupContainer.Instance.Components.Select( a => a.Value.Value ).Where( x => x.IsActive ).FirstOrDefault();
            if ( ipLookupComponent != null )
            {
                try
                {
                    var lookupResult = ipLookupComponent.Lookup( RequestContext.ClientInformation.IpAddress, out var resultMsg );
                    if ( string.IsNullOrEmpty( resultMsg ) )
                    {
                        location = lookupResult.Location;
                    }
                    else
                    {
                        RockLogger.Log.Error( RockLogDomains.Core, $"Error processing IP Lookup for Login Confirmation Alert: {resultMsg}." );
                    }
                }
                catch ( SystemException ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }

            var mergeFields = GetMergeFields( new Dictionary<string, object>
            {
                { "Person", person },
                { "UserAgent", RequestContext.ClientInformation.UserAgent },
                { "IPAddress", RequestContext.ClientInformation.IpAddress },
                { "LoginDateTime", RockDateTime.Now },
                { "Location", location },
            } );

            // Send the message.
            try
            {

                var message = new RockEmailMessage( systemCommunication )
                {
                    AppRoot = "/",
                    ThemeRoot = this.RequestContext.ResolveRockUrl( "~~/" ),
                    CreateCommunicationRecord = false,
                };

                message.SetRecipients( new List<RockEmailMessageRecipient>
                {
                    new RockEmailMessageRecipient( person, mergeFields )
                } );

                message.Send();
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

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
        /// Confirms the user login.
        /// </summary>
        /// <param name="userLoginId">The user login identifier.</param>
        private void ConfirmUserLogin( int userLoginId )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.Get( userLoginId );

                if ( userLogin != null && userLogin.IsConfirmed != true )
                {
                    userLogin.IsConfirmed = true;
                    rockContext.SaveChanges();
                }
            }
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
        /// <param name="path">The path to use for the redirect URI.</param>
        private string GetRedirectUri( string path )
        {
            // If the path is not valid then default to the current page.
            if ( !IsPageRouteValid( path ) )
            {
                path = this.GetCurrentPageUrl();
            }

            var uriBuilder = new UriBuilder
            {
                Scheme = this.RequestContext.RequestUri.Scheme,
                Host = this.RequestContext.RequestUri.Host,
                Port = this.RequestContext.RequestUri.Port,
                Path = path
            };

            // Build the URI. This will pass a string representation of the URL (including the 443 port) to the Uri constructor.
            var uri = uriBuilder.Uri;

            // Return the original string that was passed to the Uri constructor (including the 443 port).
            return uri.OriginalString;
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
        /// Gets the new account page URL.
        /// </summary>
        /// <param name="decodedQueryParams">The decoded query parameters.</param>
        /// <returns>The new account page URL.</returns>

        private string GetNewAccountPageUrl( string state, bool areUsernameAndPasswordRequired )
        {
            var queryParameters = new Dictionary<string, string>
            {
                { "State", state },
                { "ReturnUrl", GetRedirectUrlAfterLogin() }
            };

            if ( areUsernameAndPasswordRequired )
            {
                queryParameters.Add( PageParameterKey.AreUsernameAndPasswordRequired, true.ToString() );
            }

            return GetNewAccountPageUrl( queryParameters );
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
                .RedirectUrlContainsXss() )
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
        /// Determines whether the specified route is a valid page route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>
        ///   <c>true</c> if the specified route is valid page route; otherwise, <c>false</c>.
        /// </returns>
        private bool IsPageRouteValid( string route )
        {
            // Ensure the supplied path is a valid page route.
            if ( route.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var simplifiedRoute = route.ToLower().RemoveLeadingForwardslash().RemoveTrailingForwardslash();

            if ( this.PageCache.PageRoutes.Any( r => r.Route?.ToLower().RemoveLeadingForwardslash().RemoveTrailingForwardslash() == simplifiedRoute ) )
            {
                return true;
            }

            return simplifiedRoute == $"page/{this.PageCache.Id}";
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
        /// Determines whether two-factor authentication is required for <paramref name="person"/>.
        /// </summary>
        /// <param name="person">The person to check.</param>
        private static bool IsTwoFactorAuthenticationRequired( Person person )
        {
            if ( person == null )
            {
                return false;
            }

            return IsTwoFactorAuthenticationRequired( person.AccountProtectionProfile );
        }

        /// <summary>
        /// Determines whether two-factor authentication is required for <paramref name="protectionProfile"/>.
        /// </summary>
        /// <param name="protectionProfile">The protection profile to check.</param>
        private static bool IsTwoFactorAuthenticationRequired( AccountProtectionProfile protectionProfile )
        {
            var securitySettings = new SecuritySettingsService().SecuritySettings;
            return securitySettings?.RequireTwoFactorAuthenticationForAccountProtectionProfiles?.Contains( protectionProfile ) == true;
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
            var redirectUrl = GetRedirectUri( this.RequestContext.RequestUri.AbsolutePath );

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

                // Authenticate with the external auth provider.
                var result = externalRedirectAuthentication.Authenticate( options );

                if ( result.IsAuthenticated != true )
                {
                    // Although authentication failed for the current auth provider,
                    // we should continue to try others as the IsReturningFromAuthentication return value may have been a false positive.
                    continue;
                }

                var userLogin = new UserLoginService( new RockContext() ).GetByUserName( result.UserName );

                // The old block would stop processing external auth providers if one successfully authenticated the user,
                // but the userLogin doesn't. The block should load as if no login occurred at all.
                if ( userLogin == null )
                {
                    box.ShouldRedirect = false;
                    return;
                }

                // Check lockout.
                if ( IsUserLockedOut( userLogin, out var errorMessage ) )
                {
                    box.ErrorMessage = errorMessage;
                    return;
                }

                // Check confirmation required.
                if ( IsUserConfirmationRequired( userLogin, out errorMessage ) )
                {
                    SendConfirmation( userLogin );
                    box.ErrorMessage = errorMessage;
                    return;
                }

                // If two factor authentication is required but has not been completed by the external auth provider, then show an error.
                // External auth providers handle their own two-factor authentication.
                var isTwoFactorAuthenticated = false;

                if ( IsTwoFactorAuthenticationRequired( userLogin.Person ) )
                {
                    isTwoFactorAuthenticated = authProvider.IsConfiguredForTwoFactorAuthentication();

                    if ( !isTwoFactorAuthenticated )
                    {
                        box.Is2FANotSupportedForAuthenticationFactor = true;
                        return;
                    }
                }

                // Authenticate the end-user in Rock and redirect.
                Authenticate(
                    userLogin,
                    isPersisted: true,
                    isTwoFactorAuthenticated );

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
            if ( box.ShouldRedirect || box.Is2FANotSupportedForAuthenticationFactor == true )
            {
                return;
            }

            // Check if we need to redirect to an external auth provider.
            if ( externalAuthProviders.Count == 1
                 && !box.IsInternalDatabaseLoginSupported
                 && GetAttributeValue( AttributeKey.RedirectToSingleExternalAuthProvider ).AsBoolean() )
            {
                var singleAuthProvider = externalAuthProviders.First();

                if ( !(singleAuthProvider.Component is IExternalRedirectAuthentication externalRedirectAuthentication) )
                {
                    box.ErrorMessage = "Please try a different authentication method.";
                    return;
                }

                var authLoginUri = externalRedirectAuthentication.GenerateExternalLoginUrl( GetRedirectUri( this.RequestContext.RequestUri.AbsolutePath ), GetRedirectUrlAfterLogin() ).AbsoluteUri;

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
                    { "ConfirmAccountUrl", RequestContext.RootUrlPath.EnsureTrailingForwardslash() + url.RemoveLeadingForwardslash() },
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
                    configurationErrors.Add( $"{AttributeName.ShowInternalDatabaseLogin} needs to be enabled to use Internal Database as the {AttributeName.DefaultLoginMethod}." );
                }

                if ( box.DefaultLoginMethod == LoginMethod.Passwordless && !box.IsPasswordlessLoginSupported )
                {
                    configurationErrors.Add( $"Passwordless needs to be enabled in {AttributeName.SecondaryAuthenticationTypes} to use Passwordless as the {AttributeName.DefaultLoginMethod}." );
                }
            }

            var isTwoFactorAuthenticationEnabled = new SecuritySettingsService().SecuritySettings.RequireTwoFactorAuthenticationForAccountProtectionProfiles?.Any() == true;

            if ( box.IsPasswordlessLoginSupported || isTwoFactorAuthenticationEnabled )
            {
                ValidatePasswordlessLoginConfiguration( box, configurationErrors, box.IsPasswordlessLoginSupported, isTwoFactorAuthenticationEnabled );
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
        /// Validates the passwordless authentication configuration.
        /// </summary>
        private PasswordlessLoginConfigurationValidation ValidatePasswordlessLoginConfiguration()
        {
            var validationResults = new PasswordlessLoginConfigurationValidation
            {
                IsPasswordlessLoginInactive = false,
                IsPasswordlessLoginConfirmationCommunicationMissing = false,
                IsPasswordlessLoginConfirmationCommunicationInactive = false,
                IsPasswordlessLoginConfirmationCommunicationSmsMissing = false,
            };

            var securitySettings = new SecuritySettingsService().SecuritySettings;
            
            var passwordlessAuthentication = ( PasswordlessAuthentication ) AuthenticationContainer.GetComponent( SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS );
                
            if ( passwordlessAuthentication == null )
            {
                validationResults.IsPasswordlessLoginInactive = true;
            }

            validationResults.PasswordlessLoginSystemCommunication = new SystemCommunicationService( new RockContext() ).Get( securitySettings.PasswordlessConfirmationCommunicationTemplateGuid );

            if ( validationResults.PasswordlessLoginSystemCommunication == null )
            {
                validationResults.IsPasswordlessLoginConfirmationCommunicationMissing = true;
            }
            else
            {
                if ( validationResults.PasswordlessLoginSystemCommunication.IsActive != true )
                {
                    validationResults.IsPasswordlessLoginConfirmationCommunicationInactive = true;
                }

                if ( !validationResults.PasswordlessLoginSystemCommunication.SmsFromSystemPhoneNumberId.HasValue )
                {
                    validationResults.IsPasswordlessLoginConfirmationCommunicationSmsMissing = true;
                }
            }

            return validationResults;
        }

        /// <summary>
        /// Validates the passwordless authentication configuration and adds configuration errors to <paramref name="configurationErrors"/>.
        /// </summary>
        /// <param name="box">The login initialization box to validate.</param>
        /// <param name="configurationErrors">The configuration errors.</param>
        private void ValidatePasswordlessLoginConfiguration( LoginInitializationBox box, List<string> configurationErrors, bool isPasswordlessLoginEnabled, bool isTwoFactorAuthenticationEnabled )
        {
            var validationResults = ValidatePasswordlessLoginConfiguration();

            var features = new List<string>();

            if ( isPasswordlessLoginEnabled )
            {
                features.Add( "passwordless login" );
            }

            if ( isTwoFactorAuthenticationEnabled )
            {
                features.Add( "two-factor authentication" );
            }

            var errorMessageSuffix = string.Empty;

            if ( features.Any() )
            {
                errorMessageSuffix = $" to use {features.JoinStrings(" or ")}";
            }

            if ( !validationResults.IsValid )
            {
                configurationErrors.AddRange( validationResults.GetErrorMessages( errorMessageSuffix ) );
                box.IsPasswordlessLoginSupported = false;
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

        /// <summary>
        /// The settings used when creating the auth cookie.
        /// </summary>
        private class AuthCookieSettings
        {
            /// <summary>
            /// Gets or sets a value indicating whether the auth cookie will be persisted across browser sessions.
            /// </summary>
            public bool IsPersisted { get; set; }

            /// <summary>
            /// Gets or sets the expiration time for the auth cookie.
            /// </summary>
            /// <remarks>Set to <c>null</c> to use the forms authentication expiration.</remarks>
            public TimeSpan? ExpiresIn { get; set; }
        }

        /// <summary>
        /// Represents a MFA ticket.
        /// </summary>
        private class MultiFactorAuthenticationTicket
        {
            /// <summary>
            /// Gets or sets the auth cookie settings.
            /// </summary>
            /// <value>
            /// The auth cookie settings.
            /// </value>
            public AuthCookieSettings AuthCookieSettings { get; set; }

            /// <summary>
            /// Gets the expiration date and time for the ticket.
            /// </summary>
            /// <value>
            /// The expiration date and time for the ticket.
            /// </value>
            public DateTimeOffset ExpiresOn { get; } = RockDateTime.Now.AddHours( 1 );

            /// <summary>
            /// Unique identifier for this MFA state.
            /// </summary>
            public Guid Guid { get; } = Guid.NewGuid();

            /// <summary>
            /// Gets a value indicating whether this ticket is valid.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this ticket is valid; otherwise, <c>false</c>.
            /// </value>
            public bool IsExpired => RockDateTime.Now >= this.ExpiresOn;

            /// <summary>
            /// The identifier of the person alias authenticating.
            /// </summary>
            public int? PersonAliasId { get; }

            /// <summary>
            /// Gets the minimum required factors.
            /// </summary>
            /// <value>
            /// The minimum required factors.
            /// </value>
            public int MinimumRequiredFactors { get; }

            /// <summary>
            /// Gets or sets the successful authentication factor entity type guids.
            /// </summary>
            /// <value>
            /// The successful authentication factor entity type guids.
            /// </value>
            public List<Guid> SuccessfulAuthenticationFactorEntityTypeGuids { get; } = new List<Guid>();

            /// <summary>
            /// Gets the successful authentication factor count.
            /// </summary>
            /// <value>
            /// The successful authentication factor count.
            /// </value>
            public int SuccessfulAuthenticationFactorCount => SuccessfulAuthenticationFactorEntityTypeGuids?.Distinct().Count() ?? 0;

            /// <summary>
            /// Determines if at least <see cref="MinimumRequiredFactors"/> have been recorded on the ticket.
            /// </summary>
            /// <returns>
            ///   <c>true</c> if at least <see cref="MinimumRequiredFactors"/> have been recorded on the ticket; otherwise, <c>false</c>.
            /// </returns>
            public bool HasMinimumRequiredFactors
            {
                get
                {
                    return this.SuccessfulAuthenticationFactorCount >= this.MinimumRequiredFactors;
                }
            }

            /// <summary>
            /// Creates a new instance of the <see cref="MultiFactorAuthenticationTicket"/> class.
            /// </summary>
            /// <param name="personAliasId">The person for whom the ticket is assigned.</param>
            /// <param name="minimumRequiredFactors">The minimum number of authentication factors this ticket needs.</param>
            /// <param name="authCookieSettings">The settings that will be used to create the auth cookie.</param>
            public MultiFactorAuthenticationTicket( int? personAliasId, int minimumRequiredFactors, AuthCookieSettings authCookieSettings )
            {
                this.PersonAliasId = personAliasId;
                this.MinimumRequiredFactors = minimumRequiredFactors;
                this.AuthCookieSettings = authCookieSettings;
            }

            /// <summary>
            /// Adds the authentication factor unique identifier.
            /// </summary>
            /// <param name="entityTypeGuid">The entity type unique identifier.</param>
            public void AddSuccessfulAuthenticationFactor( Guid entityTypeGuid )
            {
                if ( !HasFactor( entityTypeGuid ) )
                {
                    // Add the authentication factor if it hasn't been added yet.
                    this.SuccessfulAuthenticationFactorEntityTypeGuids.Add( entityTypeGuid );
                }
            }

            /// <summary>
            /// Determines whether the factor has been added to the ticket.
            /// </summary>
            /// <param name="entityTypeGuid">The entity type unique identifier.</param>
            /// <returns>
            ///   <c>true</c> if the factor has been added to the ticket; otherwise, <c>false</c>.
            /// </returns>
            public bool HasFactor( Guid entityTypeGuid )
            {
                return this.SuccessfulAuthenticationFactorEntityTypeGuids?.Contains( entityTypeGuid ) == true;
            }

            /// <summary>
            /// Returns the decrypted <see cref="MultiFactorAuthenticationTicket"/> or null if not valid.
            /// </summary>
            /// <param name="encryptedMfaTicket">The encrypted MFA ticket.</param>
            /// <returns>The decrypted <see cref="MultiFactorAuthenticationTicket"/> or null if not valid.</returns>
            public static MultiFactorAuthenticationTicket Decrypt(string encryptedMfaTicket)
            {
                return Encryption.DecryptString( encryptedMfaTicket )?.FromJsonOrNull<MultiFactorAuthenticationTicket>();
            }

            /// <summary>
            /// Returns this MFA ticket as an encrypted string.
            /// </summary>
            /// <returns>This MFA ticket as an encrypted string.</returns>
            public string Encrypt()
            {
                return Encryption.EncryptString( this.ToJson() );
            }
        }

        private class PasswordlessLoginConfigurationValidation
        {
            public bool IsValid =>
                !this.IsPasswordlessLoginInactive
                && !this.IsPasswordlessLoginConfirmationCommunicationMissing
                && !this.IsPasswordlessLoginConfirmationCommunicationInactive
                && !this.IsPasswordlessLoginConfirmationCommunicationSmsMissing;

            public bool IsPasswordlessLoginInactive { get; internal set; }

            public bool IsPasswordlessLoginConfirmationCommunicationMissing { get; internal set; }

            public bool IsPasswordlessLoginConfirmationCommunicationInactive { get; internal set; }

            public bool IsPasswordlessLoginConfirmationCommunicationSmsMissing { get; internal set; }

            public SystemCommunication PasswordlessLoginSystemCommunication { get; internal set; }

            /// <summary>
            /// Gets the error messages.
            /// </summary>
            /// <param name="errorMessageSuffix">The suffix appended to each error message.</param>
            public List<string> GetErrorMessages( string errorMessageSuffix = null )
            {
                var errorMessages = new List<string>();
             
                if ( this.IsPasswordlessLoginInactive )
                {
                    errorMessages.Add( $"The Passwordless Authentication service needs to be active{errorMessageSuffix}." );
                }

                if ( this.IsPasswordlessLoginConfirmationCommunicationMissing )
                {
                    errorMessages.Add( $"The Passwordless Login Confirmation system communication needs to be configured in your security settings{errorMessageSuffix}." );
                }

                if ( this.PasswordlessLoginSystemCommunication != null )
                {
                    if ( this.IsPasswordlessLoginConfirmationCommunicationInactive )
                    {
                        errorMessages.Add( $"The {this.PasswordlessLoginSystemCommunication.Title} system communication needs to be active{errorMessageSuffix}." );
                    }

                    if ( this.IsPasswordlessLoginConfirmationCommunicationSmsMissing )
                    {
                        errorMessages.Add( $"The { this.PasswordlessLoginSystemCommunication.Title } system communication needs an SMS From value{errorMessageSuffix}." );
                    }
                }

                return errorMessages;
            }
        }

        /// <summary>
        /// A helper class for building different types of responses.
        /// </summary>
        private static class ResponseHelper
        {
            internal static class PasswordlessLogin
            {                
                /// <summary>
                /// Returns an error passwordless login verify response.
                /// </summary>
                /// <param name="errorMessage">The error message.</param>
                /// <returns>An error passwordless login verify response.</returns>
                internal static PasswordlessLoginVerifyResponseBag Error( string errorMessage )
                {
                    return new PasswordlessLoginVerifyResponseBag
                    {
                        ErrorMessage = errorMessage,
                    } ;
                }
        
                /// <summary>
                /// Returns an authenticated passwordless login verify response.
                /// </summary>
                /// <returns>An authenticated passwordless login verify response.</returns>
                internal static PasswordlessLoginVerifyResponseBag Authenticated()
                {
                    return new PasswordlessLoginVerifyResponseBag
                    {
                        IsAuthenticated = true,
                    };
                }
        
                /// <summary>
                /// Returns a person selection required passwordless login verify response.
                /// </summary>
                /// <param name="matchingPeople">The matching people.</param>
                /// <param name="state">The state.</param>
                /// <returns>A person selection required passwordless login verify response.</returns>
                internal static PasswordlessLoginVerifyResponseBag PersonSelectionRequired( List<ListItemBag> matchingPeople )
                {
                    return new PasswordlessLoginVerifyResponseBag
                    {
                        IsPersonSelectionRequired = true,
                        MatchingPeople = matchingPeople
                    };
                }
        
                /// <summary>
                /// Returns a registration required passwordless login verify response.
                /// </summary>
                /// <param name="state">The state.</param>
                /// <param name="registrationUrl">The registration URL.</param>
                /// <returns>A registration required passwordless login verify response.</returns>
                internal static PasswordlessLoginVerifyResponseBag RegistrationRequired( string registrationUrl )
                {
                    return new PasswordlessLoginVerifyResponseBag
                    {
                        IsRegistrationRequired = true,
                        RegistrationUrl = registrationUrl
                    };
                }
        
                /// <summary>
                /// Returns a MFA required passwordless login verify response.
                /// </summary>
                /// <param name="ticket">The MFA ticket.</param>
                /// <param name="isUsernameAndPasswordMissing">Whether the username and password is missing.</param>
                /// <returns>A MFA required passwordless login verify response.</returns>
                internal static PasswordlessLoginVerifyResponseBag MfaRequired( string ticket, bool isUsernameAndPasswordMissing = false )
                {
                    return new PasswordlessLoginVerifyResponseBag
                    {
                        Mfa = new CredentialLoginMfaBag
                        {
                            IsUsernameAndPasswordMissing = isUsernameAndPasswordMissing,
                            Ticket = ticket,
                        }
                    };
                }

                /// <summary>
                /// Returns a MFA mismatched accounts passwordless login verify response.
                /// <para>This occurs when the passwordless email or mobile phone is for a different person than the original MFA session.</para>
                /// </summary>
                /// <returns>A MFA mismatched accounts passwordless login verify response.</returns>
                internal static PasswordlessLoginVerifyResponseBag MfaMismatchedAccounts()
                {
                    return new PasswordlessLoginVerifyResponseBag
                    {
                        ErrorMessage = "The email or mobile phone you provided must be added to your account before it may be used for Two-Factor Authentication. Please contact us to assist you in configuring this."
                    };
                }
            }

            internal static class CredentialLogin
            { 
                /// <summary>
                /// Returns an error credential login response.
                /// </summary>
                /// <param name="errorMessage">The error message.</param>
                /// <returns>An error credential login response.</returns>
                internal static CredentialLoginResponseBag Error( string errorMessage )
                {
                    return new CredentialLoginResponseBag
                    {
                        ErrorMessage = errorMessage
                    };
                }
        
                /// <summary>
                /// Returns a locked out credential login response.
                /// </summary>
                /// <param name="errorMessage">The error message.</param>
                /// <returns>A locked out credential login response.</returns>
                internal static CredentialLoginResponseBag LockedOut( string errorMessage )
                {
                    return new CredentialLoginResponseBag
                    {
                        ErrorMessage = errorMessage,
                        IsLockedOut = true,
                    };
                }
        
                /// <summary>
                /// Returns a confirmation required credential login response.
                /// </summary>
                /// <param name="errorMessage">The error message.</param>
                /// <returns>A confirmation required credential login response.</returns>
                internal static CredentialLoginResponseBag ConfirmationRequired( string errorMessage )
                {
                    return new CredentialLoginResponseBag
                    {
                        ErrorMessage = errorMessage,
                        IsConfirmationRequired = true,
                    };
                }
        
                /// <summary>
                /// Returns a MFA required credential login response.
                /// </summary>
                /// <param name="ticket">The MFA ticket.</param>
                /// <param name="isEmailAndMobilePhoneMissing">Whether the email and mobile phone is missing.</param>
                /// <returns>A MFA required credential login response.</returns>
                internal static CredentialLoginResponseBag MfaRequired( string ticket, bool isEmailAndMobilePhoneMissing = false )
                {
                    return new CredentialLoginResponseBag
                    {
                        Mfa = new PasswordlessLoginMfaBag
                        {
                            Ticket = ticket,
                            IsEmailAndMobilePhoneMissing = isEmailAndMobilePhoneMissing
                        }
                    };
                }
        
                /// <summary>
                /// Returns an authenticated credential login response.
                /// </summary>
                /// <param name="redirectUrl">The redirect URL.</param>
                /// <returns>An authenticated credential login response.</returns>
                internal static CredentialLoginResponseBag Authenticated( string redirectUrl )
                {
                    return new CredentialLoginResponseBag
                    {
                        IsAuthenticated = true,
                        RedirectUrl = redirectUrl,
                    };
                }
            }
        }

        #endregion
    }
}
