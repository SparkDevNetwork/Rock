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
using System.Threading.Tasks;

using RestSharp;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Login;
using Rock.Common.Mobile.Enums;
using Rock.Common.Mobile.Security.Authentication;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Linq;
using Microsoft.Extensions.Logging;

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

    [BooleanField( "Use Embedded Web View For External Authentication",
        Key = AttributeKeys.UseEmbeddedWebViewForExternalAuthentication,
        Description = "When enabled, the application will use an embedded web view for the external authentication process. This must be disabled in cases where you want to offer Fido2/Passkey support.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 13 )]

    [BooleanField( "Enable Enhanced Authentication Security",
        Key = AttributeKeys.EnableEnhancedAuthenticationSecurity,
        Description = "Only applies to external authentication. Whether or not to enable enhanced authentication security. This will be automatically enabled in a future version of Rock, and the setting will be removed.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 14 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_LOGIN_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "6006FE32-DC01-4B1C-A9B8-EE172451F4C5" )]
    public class Login : RockBlockType
    {
        #region Keys

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
            /// Whether or not to use an embedded web view for external authentication.
            /// </summary>
            public const string UseEmbeddedWebViewForExternalAuthentication = "UseEmbeddedWebViewForExternalAuthentication";

            /// <summary>
            /// The header content key.
            /// </summary>
            public const string HeaderContent = "HeaderContent";

            /// <summary>
            /// The footer content key.
            /// </summary>
            public const string FooterContent = "FooterContent";

            /// <summary>
            /// The enable enhanced authentication security key.
            /// </summary>
            public const string EnableEnhancedAuthenticationSecurity = "EnableEnhancedAuthenticationSecurity";
        }

        #endregion

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
                UseEmbeddedWebViewForExternalAuthentication = GetAttributeValue( AttributeKeys.UseEmbeddedWebViewForExternalAuthentication ).AsBoolean(),
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

            new HistoryLogin
            {
                UserName = userLogin.UserName,
                UserLoginId = userLogin.Id,
                PersonAliasId = userLogin.Person?.PrimaryAliasId,
                SourceSiteId = this.PageCache?.SiteId,
                WasLoginSuccessful = true
            }.SaveAfterDelay();
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

        #region External Authentication

        /// <summary>
        /// The keys to utilize when converting data returned from an external authentication provider.
        /// </summary>
        private static class ExternalAuthenticationPayloadKeys
        {
            public readonly static string[] FirstNameEligibleKeys = new[] { "firstname", "first_name", "given_name", "givenName" };
            public readonly static string[] LastNameEligibleKeys = new[] { "lastname", "last_name", "family_name", "surname" };
            public readonly static string[] PhoneNumerEligibleKeys = new[] { "phone", "phonenumber", "phone_number", "mobilePhone" };
            public readonly static string[] PictureEligibleKeys = new[] { "photo", "picture", "profile_image", "avatar" };
            public readonly static string[] DateOfBirthEligibleKeys = new[] { "birthday", "birth_date", "birthdate", "date_of_birth" };
            public readonly static string[] UpdatedAtKeys = new[] { "updated_at" };
            public readonly static string[] NickNameKeys = new[] { "nickname" };
            public readonly static string[] EmailKeys = new[] { "email", "mail" };
            public readonly static string[] SubKeys = new[] { "sub" };
            public readonly static string[] GenderKeys = new[] { "gender" };
            public readonly static string[] EmailVerifiedKeys = new[] { "email_verified" };
            public readonly static string[] PreferredUsernameKeys = new[] { "preferred_username" };
        }

        /// <summary>
        /// Gets whether or not external authentication is enabled.
        /// </summary>
        /// <returns></returns>
        private bool IsExternalAuthenticationEnabled( AdditionalSiteSettings additionalSettings )
        {
            var enableAuth0Login = GetAttributeValue( AttributeKeys.EnableAuth0Login ).AsBoolean();
            var enableEntraLogin = GetAttributeValue( AttributeKeys.EnableEntraLogin ).AsBoolean();

            // If one of the block settings is enabled, then we can assume it's configured.
            if ( enableAuth0Login || enableEntraLogin )
            {
                return true;
            }

            // Otherwise, make sure that we have the necessary settings in the application settings.
            if ( additionalSettings.Auth0ClientId.IsNotNullOrWhiteSpace() || additionalSettings.EntraClientId.IsNotNullOrWhiteSpace() )
            {
                return true;
            }

            return false;
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

        /// <summary>
        /// Gets the user information from the external authentication provider.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="provider">The authentication provider.</param>
        /// <returns>A bag containing user information.</returns>
        private async Task<ExternalAuthenticationUserInfoBag> GetUserInfoAsync( string accessToken, SupportedAuthenticationProvider provider )
        {
            if ( provider == SupportedAuthenticationProvider.Auth0 )
            {
                return await GetAuth0UserInfoAsync( accessToken );
            }
            else if ( provider == SupportedAuthenticationProvider.Entra )
            {
                return await GetEntraUserInfoAsync( accessToken );
            }

            return null;
        }

        /// <summary>
        /// Gets the user information from Auth0.
        /// </summary>
        /// <param name="accessToken">The access token for the request.</param>
        /// <returns></returns>
        private async Task<ExternalAuthenticationUserInfoBag> GetAuth0UserInfoAsync( string accessToken )
        {
            var additionalSettings = this.PageCache.Layout.Site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();
            var domain = additionalSettings.Auth0Domain;

            try
            {
                using ( var httpClient = new HttpClient() )
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", accessToken );

                    var response = await httpClient.GetAsync( $"https://{domain.EnsureTrailingForwardslash()}userinfo" );

                    if ( !response.IsSuccessStatusCode )
                    {
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var payload = JsonConvert.DeserializeObject<Dictionary<string, object>>( json );
                    if( payload == null )
                    {
                        return null;
                    }

                    return GetUserInfoFromData( payload, SupportedAuthenticationProvider.Auth0 );
                }
            }
            catch ( Exception )
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the user information from Entra.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        private async Task<ExternalAuthenticationUserInfoBag> GetEntraUserInfoAsync( string accessToken )
        {
            const string graphMeEndpoint = "https://graph.microsoft.com/v1.0/me";

            try
            {
                using ( var httpClient = new HttpClient() )
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", accessToken );

                    var response = await httpClient.GetAsync( graphMeEndpoint );

                    if ( !response.IsSuccessStatusCode )
                    {
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var payload = JsonConvert.DeserializeObject<Dictionary<string, object>>( json );
                    if ( payload == null )
                    {
                        return null;
                    }

                    return GetUserInfoFromData( payload, SupportedAuthenticationProvider.Entra );
                }
            }
            catch ( Exception )
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a <see cref="ExternalAuthenticationUserInfoBag" /> based on the provided dictionary values.
        /// This is done utilizing the <see cref="ExternalAuthenticationPayloadKeys" /> that are used to represent
        /// all of the different keys we support.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        private ExternalAuthenticationUserInfoBag GetUserInfoFromData( Dictionary<string, object> data, SupportedAuthenticationProvider provider )
        {
            // Helper to grab the first non-empty value from a set of keys
            string GetFirstMatch( params string[] keys ) =>
                keys.Select( key => data.GetValueOrNull( key )?.ToString() )
                    .FirstOrDefault( value => value.IsNotNullOrWhiteSpace() );

            // Core values
            var firstName = GetFirstMatch( ExternalAuthenticationPayloadKeys.FirstNameEligibleKeys );
            var lastName = GetFirstMatch( ExternalAuthenticationPayloadKeys.LastNameEligibleKeys );
            var phoneNumber = GetFirstMatch( ExternalAuthenticationPayloadKeys.PhoneNumerEligibleKeys );
            var picture = GetFirstMatch( ExternalAuthenticationPayloadKeys.PictureEligibleKeys );
            var birthDateStr = GetFirstMatch( ExternalAuthenticationPayloadKeys.DateOfBirthEligibleKeys );
            var updatedAtStr = GetFirstMatch( ExternalAuthenticationPayloadKeys.UpdatedAtKeys );
            var nickname = GetFirstMatch( ExternalAuthenticationPayloadKeys.NickNameKeys );
            var email = GetFirstMatch( ExternalAuthenticationPayloadKeys.EmailKeys );
            var sub = GetFirstMatch( ExternalAuthenticationPayloadKeys.SubKeys );
            var genderStr = GetFirstMatch( ExternalAuthenticationPayloadKeys.GenderKeys );
            var emailVerified = GetFirstMatch( ExternalAuthenticationPayloadKeys.EmailVerifiedKeys );
            var preferredUsername = GetFirstMatch( ExternalAuthenticationPayloadKeys.PreferredUsernameKeys );

            // Parsing
            if( email.IsNullOrWhiteSpace() && preferredUsername.IsNotNullOrWhiteSpace() && preferredUsername.Contains('@' ) )
            {
                email = preferredUsername;
            }

            var updatedAt = updatedAtStr.AsDateTime();
            var birthDate = birthDateStr.AsDateTime()?.Date;
            var gender = genderStr?.ConvertToEnumOrNull<Rock.Common.Mobile.Enums.Gender>() ?? Common.Mobile.Enums.Gender.Unknown;

            return new ExternalAuthenticationUserInfoBag
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                EmailVerified = emailVerified.AsBooleanOrNull() ?? true,
                ForeignKey = sub,
                Gender = gender,
                NickName = nickname,
                PhoneNumber = phoneNumber,
                Picture = picture,
                UpdatedAt = updatedAt ?? RockDateTime.Now,
                BirthDate = birthDate,
                Provider = provider,
            };
        }

        /// <summary>
        /// Gets or creates a person from information returned by external authentication.
        /// </summary>
        /// <param name="personInfo">The externally authenticated user to either create or get.</param>
        /// <param name="userLoginInfo">The username of the person to look for or create. This is usually dependent on the
        /// authentication provider. For instance, an Auth0 related username is "AUTH0_{FOREIGN_KEY}. It is up to the person
        /// implementing this method into an external authentication provider to make sure the username is formatted correctly."</param>
        /// <param name="connectionStatusValueId">The connection status to use when generating a new person.</param>
        /// <param name="recordStatusValueId">The record status to use when generating a </param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        [RockInternal( "1.15.1" )]
        internal static UserLogin GetOrCreatePersonFromExternalAuthenticationUserInfo( ExternalAuthenticationUserInfoBag personInfo, ExternalAuthUserLoginBag userLoginInfo, RockContext rockContext = null, int? connectionStatusValueId = null, int? recordStatusValueId = null )
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

                if ( connectionStatusValueId == null )
                {
                    connectionStatusValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() ).Id;
                }

                if ( recordStatusValueId == null )
                {
                    recordStatusValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;
                }

                // Build the person match query based off of the data the external authentication returned.
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
                        person.RecordStatusValueId = recordStatusValueId;
                        person.ConnectionStatusValueId = connectionStatusValueId;
                        person.FirstName = firstName;
                        person.LastName = lastName;
                        person.Email = email;
                        person.IsEmailActive = true;
                        person.EmailPreference = Rock.Model.EmailPreference.EmailAllowed;

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

            // For the authentication providers that are supported,
            // we need to structure the UserLogin accordingly.
            switch ( supportedMobileProvider )
            {
                // AUTH0_<value>
                case Common.Mobile.Enums.SupportedAuthenticationProvider.Auth0:
                    usernamePrefix = "AUTH0_";
                    externalPass = "auth0";
                    providerEntityTypeId = EntityTypeCache.Get( "9D2EDAC7-1051-40A1-BE28-32C0ABD1B28F" )?.Id;
                    break;

                // ENTRA_<value> or AzureAD_<value> or Office365_<value>
                case Common.Mobile.Enums.SupportedAuthenticationProvider.Entra:
                    var (UsernamePrefix, ProviderEntityTypeId) = GetEntraComponentInfo();
                    usernamePrefix = UsernamePrefix;
                    providerEntityTypeId = ProviderEntityTypeId;
                    externalPass = "entra";
                    break;

                // Unsupported
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

                // We'll supplement and save this as needed below.
                var failedHistoryLogin = new HistoryLogin
                {
                    UserName = username,
                    UserLoginId = userLogin?.Id,
                    PersonAliasId = userLogin?.Person?.PrimaryAliasId,
                    SourceSiteId = this.PageCache?.SiteId,
                    WasLoginSuccessful = false
                };

                if ( state == UserLoginValidationState.Valid )
                {
                    UpdateLastLoginDetails( userLogin, personalDeviceGuid, rockContext );

                    return GetMobileResponse( userLogin, rememberMe );
                }
                else if ( state == UserLoginValidationState.LockedOut )
                {
                    failedHistoryLogin.LoginFailureReason = LoginFailureReason.LockedOut;
                    failedHistoryLogin.SaveAfterDelay();

                    return ActionUnauthorized( GetLockedOutMessage() );
                }
                else if ( state == UserLoginValidationState.NotConfirmed && SendConfirmation( userLogin ) )
                {
                    failedHistoryLogin.LoginFailureReason = LoginFailureReason.UserNotConfirmed;
                    failedHistoryLogin.SaveAfterDelay();

                    return ActionUnauthorized( GetUnconfirmedMessage() );
                }
                else
                {
                    var loginFailureReason = state == UserLoginValidationState.InvalidPassword
                        ? LoginFailureReason.InvalidCredentials
                        : LoginFailureReason.UserNotFound;

                    failedHistoryLogin.LoginFailureReason = loginFailureReason;
                    failedHistoryLogin.SaveAfterDelay();

                    return ActionUnauthorized( "We couldn't find an account with that username and password combination." );
                }
            }
        }

        /// <summary>
        /// Processes the external authentication request.
        /// </summary>
        /// <param name="options">The parameters for external authentication.</param>
        /// <returns></returns>
        [BlockAction]
        public async Task<BlockActionResult> ProcessExternalAuthentication( ExternalAuthenticationRequestBag options )
        {
            var additionalSettings = this.PageCache.Layout.Site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

            if ( !IsExternalAuthenticationEnabled( additionalSettings ) )
            {
                return ActionBadRequest( "External authentication is not enabled." );
            }

            if ( options == null || options.AccessToken.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "There was no token provided for external authentication." );
            }

            // OIDC clients return an access token which is used to retrieve user information.
            var userInfo = await GetUserInfoAsync( options.AccessToken, options.AuthenticationProvider );
            if( userInfo == null )
            {
                Logger.LogError( "There was no user information returned from the external authentication provider." );
                return ActionBadRequest( "There was no user information returned from the external authentication provider." );
            }

            // For the authentication providers that are supported,
            // we need to structure the UserLogin accordingly.
            string usernameValue = string.Empty;

            switch ( options.AuthenticationProvider )
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

            // We'll supplement and save this as needed below.
            var failedHistoryLogin = new HistoryLogin
            {
                UserName = usernameValue,
                SourceSiteId = this.PageCache?.SiteId,
                WasLoginSuccessful = false
            };

            var providerInfo = GetExternalAuthUserLoginInfo( options.AuthenticationProvider, usernameValue );

            if ( providerInfo == null )
            {
                var errorMessage = "There was no entity found for that authentication provider.";

                failedHistoryLogin.LoginFailureReason = LoginFailureReason.Other;
                failedHistoryLogin.LoginFailureMessage = $"{errorMessage} ({options.AuthenticationProvider.ConvertToString()})";
                failedHistoryLogin.SaveAfterDelay();

                return ActionBadRequest( errorMessage );
            }

            var userLogin = GetOrCreatePersonFromExternalAuthenticationUserInfo( userInfo, providerInfo, RockContext, additionalSettings?.Auth0ConnectionStatusValueId, additionalSettings?.Auth0RecordStatusValueId );

            // Something went wrong or we didn't receive enough information to create a Person.
            if ( userLogin == null )
            {
                failedHistoryLogin.LoginFailureReason = LoginFailureReason.UserNotFound;
                failedHistoryLogin.SaveAfterDelay();

                return ActionBadRequest( "There was an error when authenticating your request. Please ensure your external authentication provider is configured correctly." );
            }

            // We now know the IDs of the person attempting to authenticate.
            failedHistoryLogin.UserLoginId = userLogin.Id;
            failedHistoryLogin.PersonAliasId = userLogin.Person?.PrimaryAliasId;

            // Make sure the login is confirmed, otherwise login is not allowed.
            if ( userLogin.IsConfirmed != true )
            {
                failedHistoryLogin.LoginFailureReason = LoginFailureReason.UserNotConfirmed;
                failedHistoryLogin.SaveAfterDelay();

                SendConfirmation( userLogin );
                return ActionBadRequest( GetUnconfirmedMessage() );
            }

            // Make sure the login is not locked out.
            if ( userLogin.IsLockedOut == true )
            {
                failedHistoryLogin.LoginFailureReason = LoginFailureReason.LockedOut;
                failedHistoryLogin.SaveAfterDelay();

                return ActionBadRequest( GetLockedOutMessage() );
            }

            UpdateLastLoginDetails( userLogin, options.PersonalDeviceGuid, RockContext );

            var authCookie = Rock.Security.Authorization.GetSimpleAuthCookie( userLogin.UserName, true, false );
            var mobilePerson = MobileHelper.GetMobilePerson( userLogin.Person, PageCache.Layout.Site );
            mobilePerson.AuthToken = authCookie.Value;

            return ActionOk( new
            {
                Person = mobilePerson
            } );
        }

        #region Legacy Actions

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
        [Obsolete( "This method is obsolete, and will be removed in a near-future release." )]
        [RockObsolete( "17.1" )]
        public BlockActionResult ProcessExternalLogin( ExternalAuthenticationUserInfoBag userInfo, Guid? personalDeviceGuid, bool rememberMe, Rock.Common.Mobile.Enums.SupportedAuthenticationProvider provider )
        {
            var additionalSettings = this.PageCache.Layout.Site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

            if ( !IsExternalAuthenticationEnabled( additionalSettings ) )
            {
                return ActionBadRequest( "External authentication is not enabled." );
            }

            var enforceEnhancedSecurity = GetAttributeValue( AttributeKeys.EnableEnhancedAuthenticationSecurity ).AsBoolean();
            if( enforceEnhancedSecurity )
            {
                Logger.LogError( "Legacy external authentication endpoint was called, but enhanced authentication security is enabled." );
                return ActionBadRequest( "You have enhanced authentication security enabled, but are still utilizing the legacy external authentication endpoint." );
            }

            using ( var rockContext = new RockContext() )
            {
                // For the authentication providers that are supported,
                // we need to structure the UserLogin accordingly.
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

                // We'll supplement and save this as needed below.
                var failedHistoryLogin = new HistoryLogin
                {
                    UserName = usernameValue,
                    SourceSiteId = this.PageCache?.SiteId,
                    WasLoginSuccessful = false
                };

                var providerInfo = GetExternalAuthUserLoginInfo( provider, usernameValue );

                if ( providerInfo == null )
                {
                    var errorMessage = "There was no entity found for that authentication provider.";

                    failedHistoryLogin.LoginFailureReason = LoginFailureReason.Other;
                    failedHistoryLogin.LoginFailureMessage = $"{errorMessage} ({provider.ConvertToString()})";
                    failedHistoryLogin.SaveAfterDelay();

                    return ActionBadRequest( errorMessage );
                }

                // Create or retrieve a Person using the information provided in the external authentication info bag.

                var userLogin = GetOrCreatePersonFromExternalAuthenticationUserInfo( userInfo, providerInfo, rockContext, additionalSettings?.Auth0ConnectionStatusValueId, additionalSettings?.Auth0RecordStatusValueId );

                // Something went wrong or we didn't receive enough information to create a Person.
                if ( userLogin == null )
                {
                    failedHistoryLogin.LoginFailureReason = LoginFailureReason.UserNotFound;
                    failedHistoryLogin.SaveAfterDelay();

                    return ActionBadRequest( "There was an error when authenticating your request. Please ensure your external authentication provider is configured correctly." );
                }

                // We now know the IDs of the person attempting to authenticate.
                failedHistoryLogin.UserLoginId = userLogin.Id;
                failedHistoryLogin.PersonAliasId = userLogin.Person?.PrimaryAliasId;

                // Make sure the login is confirmed, otherwise login is not allowed.
                if ( userLogin.IsConfirmed != true )
                {
                    failedHistoryLogin.LoginFailureReason = LoginFailureReason.UserNotConfirmed;
                    failedHistoryLogin.SaveAfterDelay();

                    SendConfirmation( userLogin );
                    return ActionBadRequest( GetUnconfirmedMessage() );
                }

                // Make sure the login is not locked out.
                if ( userLogin.IsLockedOut == true )
                {
                    failedHistoryLogin.LoginFailureReason = LoginFailureReason.LockedOut;
                    failedHistoryLogin.SaveAfterDelay();

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

        #endregion

        #region Helper Classes

        /// <summary>
        /// A bag representing the information required to process an external authentication request.
        /// </summary>
        public class ExternalAuthenticationRequestBag
        {
            /// <summary>
            /// Gets or sets the access token.
            /// </summary>
            public string AccessToken { get; set; }

            /// <summary>
            /// Gets or sets the identity token.
            /// </summary>
            public string IdentityToken { get; set; }

            /// <summary>
            /// Gets or sets the personal device unique identifier.
            /// </summary>
            public Guid PersonalDeviceGuid { get; set; }

            /// <summary>
            /// Gets or sets the authentication provider.
            /// </summary>
            public SupportedAuthenticationProvider AuthenticationProvider { get; set; }
        }

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
