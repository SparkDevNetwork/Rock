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
using System.Net;

using RestSharp;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Login;
using Rock.Common.Mobile.Security.Authentication;
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
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Log In" )]
    [Category( "Mobile > Cms" )]
    [Description( "Allows the user to log in on a mobile application." )]
    [IconCssClass( "fa fa-user-lock" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

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

    [CodeEditorField( "Header Content",
        Key = AttributeKeys.HeaderContent,
        Description = "The content to display for the header. This only works if the block isn't in a ScrollView.",
        IsRequired = false,
        DefaultValue = "",
        Order = 6 )]

    [CodeEditorField( "Footer Content",
        Key = AttributeKeys.FooterContent,
        Description = "The content to display for the footer. This only works if the block isn't in a ScrollView. Disappears when the keyboard is shown.",
        IsRequired = false,
        DefaultValue = "",
        Order = 7 )]

    [BooleanField(
        "Enable Auth0 Login",
        Key = AttributeKeys.EnableAuth0Login,
        Description = "Whether or not to enable Auth0 as an authentication provider. This must be configured in the application settings beforehand.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 8 )]

    [BooleanField( "Enable Microsoft Entra Login",
        Key = AttributeKeys.EnableEntraLogin,
        Description = "Whether or not to enable Entra as an authentication provider. This must be configured in the application settings beforehand.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 9 )]

    [BooleanField( "Enable Database Login",
        Key = AttributeKeys.EnableDatabaseLogin,
        Description = "Whether or not to enable `Database` as an authentication provider.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 10 )]

    [TextField( "Auth0 Login Button Text",
        Key = AttributeKeys.Auth0LoginButtonText,
        Description = "The text of the Auth0 login button.",
        IsRequired = false,
        DefaultValue = "Login With Auth0",
        Order = 11 )]

    [TextField( "Entra Login Button Text",
        Key = AttributeKeys.EntraLoginButtonText,
        Description = "The text of the Entra login button.",
        IsRequired = false,
        DefaultValue = "Login With Entra",
        Order = 12 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_LOGIN_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5" )]

    public class Login : RockBlockType
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

            /// <summary>
            /// The enable auth0 login key.
            /// </summary>
            public const string EnableAuth0Login = "EnableAuth0Login";

            /// <summary>
            /// The enable entra key.
            /// </summary>
            public const string EnableEntraLogin = "EnableEntraLogin";

            /// <summary>
            /// The enable database login key.
            /// </summary>
            public const string EnableDatabaseLogin = "EnableDatabaseLogin";

            /// <summary>
            /// The auth0 login button text.
            /// </summary>
            public const string Auth0LoginButtonText = "Auth0LoginButtonText";

            /// <summary>
            /// The entra login button text.
            /// </summary>
            public const string EntraLoginButtonText = "EntraLoginButtonText";

            /// <summary>
            /// The header content key.
            /// </summary>
            public const string HeaderContent = "HeaderContent";

            /// <summary>
            /// The footer content key.
            /// </summary>
            public const string FooterContent = "FooterContent";
        }

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Login.Configuration
            {
                // todo update this to use the mobile configuration
                RegistrationPageGuid = GetAttributeValue( AttributeKeys.RegistrationPage ).AsGuidOrNull(),
                ForgotPasswordUrl = GetAttributeValue( AttributeKeys.ForgotPasswordUrl ),
                ReturnPageGuid = GetAttributeValue( AttributeKeys.ReturnPage ).AsGuidOrNull(),
                CancelPageGuid = GetAttributeValue( AttributeKeys.CancelPage ).AsGuidOrNull(),
                EnableAuth0Login = GetAttributeValue( AttributeKeys.EnableAuth0Login ).AsBoolean(),
                EnableDatabaseLogin = GetAttributeValue( AttributeKeys.EnableDatabaseLogin ).AsBoolean(),
                EnableEntraLogin = GetAttributeValue( AttributeKeys.EnableEntraLogin ).AsBoolean(),
                EntraLoginButtonText = GetAttributeValue( AttributeKeys.EntraLoginButtonText ),
                Auth0LoginButtonText = GetAttributeValue( AttributeKeys.Auth0LoginButtonText ),
                HeaderContent = GetAttributeValue( AttributeKeys.HeaderContent ),
                FooterContent = GetAttributeValue( AttributeKeys.FooterContent )
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

            return ActionOk( new MobileLoginResult
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
            var confirmationPageGuid = confirmationWebPage.Split( ',' )[ 0 ].AsGuidOrNull();
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

        /// <summary>
        /// Gets or creates a person from information returned by external authentication.
        /// </summary>
        /// <param name="personInfo">The externally authenticated user to either create or get.</param>
        /// <param name="userLoginInfo">The username of the person to look for or create. This is usually dependent on the
        /// authentication provider. For instance, an Auth0 related username is "AUTH0_{FOREIGN_KEY}. It is up to the person
        /// implementing this method into an external authentication provider to make sure the username is formatted correctly."</param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        [RockInternal( "1.15.1" )]
        internal static UserLogin GetOrCreatePersonFromExternalAuthenticationUserInfo( ExternalAuthenticationUserInfoBag personInfo, ExternalAuthUserLoginBag userLoginInfo, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            UserLogin user = null;
            Person person = null;

            // Query for an existing user from the external authentication user name.
            var userLoginService = new UserLoginService( rockContext );
            user = userLoginService.GetByUserName( userLoginInfo.Username );

            // If no user was found, see if we can find a match in the person table.
            if ( user == null )
            {
                var personRecordTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                var personStatusPending = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

                //
                // Build the person match query based off of the data the external authentication returned.
                //
                var firstName = personInfo.FirstName?.Trim()?.FixCase();
                var lastName = personInfo.LastName?.Trim()?.FixCase();
                var email = personInfo.Email;
                var phoneNumber = personInfo.PhoneNumber;

                // In order to match or create a person, we need a valid
                // email or phone, and first and last name,
                var hasValidPhoneOrEmail = email.IsNotNullOrWhiteSpace() || phoneNumber.IsNotNullOrWhiteSpace();
                var hasValidFirstAndLastName = firstName.IsNotNullOrWhiteSpace() && lastName.IsNotNullOrWhiteSpace();

                if ( !hasValidPhoneOrEmail || !hasValidFirstAndLastName )
                {
                    return null;
                }

                // Match the person.
                var personMatchQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, phoneNumber );
                var personService = new PersonService( rockContext );

                person = personService.FindPerson( personMatchQuery, true );

                rockContext.WrapTransaction( () =>
                {
                    // If we couldn't match a person, we should create a new one.
                    if ( person == null )
                    {
                        person = new Person();
                        person.IsSystem = false;
                        person.RecordTypeValueId = personRecordTypeId;
                        person.RecordStatusValueId = personStatusPending;
                        person.FirstName = firstName;
                        person.LastName = lastName;
                        person.Email = email;
                        person.IsEmailActive = true;
                        person.EmailPreference = EmailPreference.EmailAllowed;

                        person.NickName = personInfo.NickName?.Trim()?.FixCase();
                        person.Gender = personInfo.Gender.ToNative();

                        if ( personInfo.BirthDate.HasValue )
                        {
                            person.SetBirthDate( personInfo.BirthDate.Value.DateTime );
                        }

                        if ( phoneNumber.IsNotNullOrWhiteSpace() )
                        {
                            var mobilePhoneDefinedValueCache = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                            person.UpdatePhoneNumber( mobilePhoneDefinedValueCache.Id, null, Rock.Model.PhoneNumber.CleanNumber( phoneNumber ), null, null, rockContext );
                        }

                        PersonService.SaveNewPerson( person, rockContext, null, false );
                        rockContext.SaveChanges();
                    }

                    user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, userLoginInfo.ProviderEntityTypeId, userLoginInfo.Username, userLoginInfo.ExternalPass, true );
                    user.ForeignKey = personInfo.ForeignKey;
                } );
            }

            // If a UserLogin entry already exists for this username.
            if ( user != null )
            {
                // If there is an associated Person with this user.
                if ( user.PersonId.HasValue )
                {
                    // Query that person.
                    var personService = new PersonService( rockContext );
                    var userPerson = personService.Get( user.PersonId.Value );
                    if ( userPerson != null )
                    {
                        if ( personInfo.PhoneNumber.IsNotNullOrWhiteSpace() )
                        {
                            var mobilePhoneDefinedValueCache = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                            userPerson.UpdatePhoneNumber( mobilePhoneDefinedValueCache.Id, null, Rock.Model.PhoneNumber.CleanNumber( personInfo.PhoneNumber ), null, null, rockContext );
                            rockContext.SaveChanges();
                        }

                        // If person does not have a photo, try to get the photo return with auth0.
                        if ( !userPerson.PhotoId.HasValue && !string.IsNullOrWhiteSpace( personInfo.Picture ) )
                        {
                            // Download the photo from the url provided.
                            var restClient = new RestClient( personInfo.Picture );
                            var restRequest = new RestRequest( Method.GET );
                            var restResponse = restClient.Execute( restRequest );
                            if ( restResponse.StatusCode == HttpStatusCode.OK )
                            {
                                var bytes = restResponse.RawBytes;

                                // Create and save the image.
                                BinaryFileType fileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );
                                if ( fileType != null )
                                {
                                    var binaryFileService = new BinaryFileService( rockContext );
                                    var binaryFile = new BinaryFile();
                                    binaryFileService.Add( binaryFile );
                                    binaryFile.IsTemporary = false;
                                    binaryFile.BinaryFileType = fileType;
                                    binaryFile.MimeType = restResponse.ContentType;
                                    binaryFile.FileName = user.Person.NickName + user.Person.LastName + ".jpg";
                                    binaryFile.FileSize = bytes.Length;
                                    binaryFile.ContentStream = new System.IO.MemoryStream( bytes );

                                    rockContext.SaveChanges();
                                    userPerson.PhotoId = binaryFile.Id;
                                    rockContext.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

            return user;
        }

        /// <summary>
        /// Gets the necessary user login information for the supported authentication provider.
        /// </summary>
        /// <param name="supportedMobileProvider"></param>
        /// <param name="usernameValue"></param>
        /// <returns></returns>
        private ExternalAuthUserLoginBag GetExternalAuthUserLoginInfo( Common.Mobile.Enums.SupportedAuthenticationProvider supportedMobileProvider, string usernameValue )
        {
            string usernamePrefix;
            int? providerEntityTypeId;
            string externalPass;

            //
            // For the authentication providers that are supported,
            // we need to structure the UserLogin accordingly.
            //
            switch ( supportedMobileProvider )
            {
                //
                // AUTH0_<value>
                //
                case Common.Mobile.Enums.SupportedAuthenticationProvider.Auth0:
                    usernamePrefix = "AUTH0_";
                    externalPass = "auth0";
                    providerEntityTypeId = EntityTypeCache.Get( "9D2EDAC7-1051-40A1-BE28-32C0ABD1B28F" )?.Id;
                    break;

                //
                // ENTRA_<value> or AzureAD_<value> or Office365_<value>
                //
                case Common.Mobile.Enums.SupportedAuthenticationProvider.Entra:
                    var entraLoginInfo = GetEntraComponentInfo();
                    usernamePrefix = entraLoginInfo.UsernamePrefix;
                    providerEntityTypeId = entraLoginInfo.ProviderEntityTypeId;
                    externalPass = "entra";
                    break;

                //
                // Unsupported
                //
                default:
                    return null;
            }

            if ( !providerEntityTypeId.HasValue )
            {
                return null;
            }

            return new ExternalAuthUserLoginBag
            {
                Username = usernamePrefix + usernameValue,
                ExternalPass = externalPass,
                ProviderEntityTypeId = providerEntityTypeId.Value
            };
        }

        /// <summary>
        /// Gets the corresponding information for the configured Entra component.
        /// </summary>
        /// <returns></returns>
        private (string UsernamePrefix, int? ProviderEntityTypeId) GetEntraComponentInfo()
        {
            var additionalSettings = this.PageCache.Layout.Site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

            if ( additionalSettings == null || additionalSettings.EntraAuthenticationComponent == null )
            {
                return (null, null);
            }

            var provider = EntityTypeCache.Get( additionalSettings.EntraAuthenticationComponent.Value );

            if ( provider == null )
            {
                return (null, null);
            }

            string prefix = "ENTRA_";

            //
            // The majority of entra logins are provided by the Triumph or BEMA
            // plugin.
            //
            // These plugins handle the naming a little differently, and for the sake
            // We're just going to do a quick check to see if
            // we can make the UserLogin records match the plugin style.
            //
            // Otherwise, use the standard ENTRA_<email> format.

            // Triumph 
            if ( provider.AssemblyName.Contains( "tech.triumph" ) )
            {
                prefix = "AzureAD_";
            }
            // BEMA
            else if ( provider.AssemblyName.Contains( "com.bemaservices" ) )
            {
                prefix = "Office365_";
            }

            return (prefix, provider.Id);
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

        /// <summary>
        /// Processes an external login.
        /// On the shell, use whatever authentication
        /// component to create a <see cref="ExternalAuthenticationUserInfoBag" />. 
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier.</param>
        /// <param name="rememberMe">if set to <c>true</c> [remember me].</param>
        /// <param name="provider">The supported authentication provider.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult ProcessExternalLogin( ExternalAuthenticationUserInfoBag userInfo, Guid? personalDeviceGuid, bool rememberMe, Rock.Common.Mobile.Enums.SupportedAuthenticationProvider provider )
        {
            using ( var rockContext = new RockContext() )
            {
                //
                // For the authentication providers that are supported,
                // we need to structure the UserLogin accordingly.
                //

                string usernameValue = string.Empty;

                switch ( provider )
                {
                    case Common.Mobile.Enums.SupportedAuthenticationProvider.Auth0:
                        usernameValue = userInfo.ForeignKey;
                        break;
                    case Common.Mobile.Enums.SupportedAuthenticationProvider.Entra:
                        usernameValue = userInfo.Email;
                        break;
                }

                if ( usernameValue.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( "There was no corresponding username provided for the authentication provider." );
                }

                var providerInfo = GetExternalAuthUserLoginInfo( provider, usernameValue );

                if ( providerInfo == null )
                {
                    return ActionBadRequest( "There was no entity found for that authentication provider." );
                }

                // Create or retrieve a Person using the information provided in the external authentication info bag.
                var userLogin = GetOrCreatePersonFromExternalAuthenticationUserInfo( userInfo, providerInfo, rockContext );

                // Something went wrong or we didn't receive enough information to create a Person.
                if ( userLogin == null )
                {
                    return ActionBadRequest( "There was an error when authenticating your request. Please ensure your external authentication provider is configured correctly." );
                }

                // Make sure the login is confirmed, otherwise login is not allowed.
                if ( userLogin.IsConfirmed != true )
                {
                    SendConfirmation( userLogin );
                    return ActionBadRequest( GetUnconfirmedMessage() );
                }

                // Make sure the login is not locked out.
                if ( userLogin.IsLockedOut == true )
                {
                    return ActionBadRequest( GetLockedOutMessage() );
                }

                UpdateLastLoginDetails( userLogin, personalDeviceGuid, rockContext );

                var authCookie = Rock.Security.Authorization.GetSimpleAuthCookie( userLogin.UserName, rememberMe, false );
                var mobilePerson = MobileHelper.GetMobilePerson( userLogin.Person, PageCache.Layout.Site );
                mobilePerson.AuthToken = authCookie.Value;

                return ActionOk( new
                {
                    Person = mobilePerson
                } );
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A bag containing the information needed to create a new UserLogin
        /// from external authentication.
        /// </summary>
        internal class ExternalAuthUserLoginBag
        {
            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            /// Gets or sets the external password.
            /// </summary>
            public string ExternalPass { get; set; }

            /// <summary>
            /// Gets or sets the entity type of the authentication provider.
            /// </summary>
            public int ProviderEntityTypeId { get; set; }
        }

        #endregion
    }
}
