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

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Oidc.Client;
using Rock.Web.Cache;

namespace Rock.Security.ExternalAuthentication
{
    /// <summary>
    /// Authenticates a user using the specified OIDC Client.
    /// </summary>
    [Description( "OIDC Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "OidcClient" )]

    [TextField( "App ID",
        Description = "The OIDC Client App ID",
        Key = AttributeKey.ApplicationId,
        Order = 1 )]
    [TextField( "App Secret",
        Description = "The OIDC Client Secret",
        Key = AttributeKey.ApplicationSecret,
        Order = 2 )]
    [TextField( "Authentication Server",
        Description = "The OIDC Server",
        Key = AttributeKey.AuthenticationServer,
        Order = 3 )]
    [TextField( "Redirect URI",
        Description = "The URI that the authentication server should redirect to once authentication is complete.",
        Key = AttributeKey.RedirectUri,
        Order = 4 )]
    [TextField( "Post Logout Redirect URI",
        Description = "The URI that the authentication server should redirect to once the user has been logged out.",
        Key = AttributeKey.PostLogoutRedirectUri,
        Order = 5 )]
    [CustomCheckboxListField(
        "Request Scopes",
        Description = "The scopes you would like to request for the authentication server.",
        ListSource = "email^Email,profile^Profile,phone^Phone,address^Address",
        IsRequired = false,
        Key = AttributeKey.RequestedScopes,
        Order = 6 )]
    public class OidcClient : AuthenticationComponent
    {
        /// <summary>
        /// Attribute Keys
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The application identifier
            /// </summary>
            public const string ApplicationId = "AppId";

            /// <summary>
            /// The application secret
            /// </summary>
            public const string ApplicationSecret = "AppSecret";

            /// <summary>
            /// The authentication server
            /// </summary>
            public const string AuthenticationServer = "AuthServer";

            /// <summary>
            /// The requested claims
            /// </summary>
            public const string RequestedScopes = "RequestedScopes";

            /// <summary>
            /// The redirect URI
            /// </summary>
            public const string RedirectUri = "RedirectUri";

            /// <summary>
            /// The post logout redirect URI
            /// </summary>
            public const string PostLogoutRedirectUri = "PostLogoutRedirectUri";
        }

        /// <summary>
        /// Session Keys
        /// </summary>
        public static class SessionKey
        {
            /// <summary>
            /// The return URL
            /// </summary>
            public const string ReturnUrl = "oidc-returnurl";

            /// <summary>
            /// The state
            /// </summary>
            public const string State = "oidc-state";

            /// <summary>
            /// The nonce
            /// </summary>
            public const string Nonce = "oidc-nonce";
        }

        /// <summary>
        /// Page Parameter Keys
        /// </summary>
        public static class PageParameterKey
        {
            /// <summary>
            /// The return URL
            /// </summary>
            public const string ReturnUrl = "returnurl";

            /// <summary>
            /// The code
            /// </summary>
            public const string Code = "code";

            /// <summary>
            /// The state
            /// </summary>
            public const string State = "state";
        }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public override AuthenticationServiceType ServiceType => AuthenticationServiceType.External;

        /// <summary>
        /// Determines if user is directed to another site (i.e. Facebook, Gmail, Twitter, etc) to confirm approval of using
        /// that site's credentials for authentication.
        /// </summary>
        /// <value>
        /// The requires remote authentication.
        /// </value>
        public override bool RequiresRemoteAuthentication => true;

        /// <summary>
        /// Gets a value indicating whether [supports change password].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports change password]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsChangePassword => false;

        /// <summary>
        /// Authenticates the user based on user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Authenticate( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authenticates the user based on a request from a third-party provider.  Will set the username and returnUrl values.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// State is invalid.
        /// or
        /// </exception>
        public override bool Authenticate( HttpRequest request, out string userName, out string returnUrl )
        {
            userName = string.Empty;
            returnUrl = HttpContext.Current.Session[SessionKey.ReturnUrl].ToStringSafe();
            string redirectUri = GetRedirectUrl( request );
            var code = request.QueryString[PageParameterKey.Code];
            var state = request.QueryString[PageParameterKey.State];
            var validState = HttpContext.Current.Session[SessionKey.State].ToStringSafe();

            if ( validState.IsNullOrWhiteSpace() || !state.Equals( validState ) )
            {
                throw new Exception( "State is invalid." );
            }

            try
            {
                var client = new TokenClient( GetTokenUrl(), GetAttributeValue( AttributeKey.ApplicationId ), GetAttributeValue( AttributeKey.ApplicationSecret ) );
                var response = client.RequestAuthorizationCodeAsync( code, redirectUri ).GetAwaiter().GetResult();

                if ( response.IsError )
                {
                    throw new Exception( response.Error );
                }

                var nonce = HttpContext.Current.Session[SessionKey.Nonce].ToStringSafe();
                var idToken = GetValidatedIdToken( response.IdentityToken, nonce );
                userName = HandleOidcUserAddUpdate( idToken, response.AccessToken );

                HttpContext.Current.Session[SessionKey.Nonce] = string.Empty;
                HttpContext.Current.Session[SessionKey.ReturnUrl] = string.Empty;
                HttpContext.Current.Session[SessionKey.State] = string.Empty;
            }

            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
            }

            return !string.IsNullOrWhiteSpace( userName );
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="warningMessage">The warning message.</param>
        /// <returns></returns>
        public override bool ChangePassword( UserLogin user, string oldPassword, string newPassword, out string warningMessage )
        {
            warningMessage = "not supported";
            return false;
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string EncodePassword( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates the log in URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            string returnUrl = request.QueryString[PageParameterKey.ReturnUrl];
            string redirectUri = GetRedirectUrl( request );

            var nonce = EncodeBcrypt( System.Guid.NewGuid().ToString() );
            var state = EncodeBcrypt( System.Guid.NewGuid().ToString() );

            HttpContext.Current.Session[SessionKey.Nonce] = nonce;
            HttpContext.Current.Session[SessionKey.State] = state;
            HttpContext.Current.Session[SessionKey.ReturnUrl] = returnUrl;

            return new Uri( GetLoginUrl( GetRedirectUrl( request ), nonce, state ) );
        }

        /// <summary>
        /// Gets the URL of an image that should be displayed.
        /// </summary>
        /// <returns></returns>
        public override string ImageUrl()
        {
            return string.Empty;
        }

        /// <summary>
        /// Tests the Http Request to determine if authentication should be tested by this
        /// authentication provider.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override bool IsReturningFromAuthentication( HttpRequest request )
        {
            return !String.IsNullOrWhiteSpace( request.QueryString[PageParameterKey.Code] )
                    && !String.IsNullOrWhiteSpace( request.QueryString[PageParameterKey.State] );
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void SetPassword( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        private string GetRedirectUrl( HttpRequest request )
        {
            return GetAttributeValue( AttributeKey.RedirectUri );
        }

        private string GetLoginUrl( string redirectUrl, string nonce, string state )
        {
            var config = GetOpenIdConnectConfiguration();

            var requestUrl = new RequestUrl( config.AuthorizationEndpoint );

            return requestUrl.CreateAuthorizeUrl( GetAttributeValue( AttributeKey.ApplicationId ),
                OidcConstants.ResponseTypes.Code,
                GetScopes(),
                $"{redirectUrl}",
                state,
                nonce );
        }

        private string GetTokenUrl()
        {
            var config = GetOpenIdConnectConfiguration();
            return config.TokenEndpoint;
        }

        private string GetScopes()
        {
            var scopes = GetAttributeValue( AttributeKey.RequestedScopes );
            if ( scopes.IsNullOrWhiteSpace() )
            {
                return "openid";
            }
            return $"openid {scopes.Replace( ",", " " )}";
        }

        private JwtSecurityToken GetValidatedIdToken( string idToken, string masterNonce )
        {
            var authServer = GetAttributeValue( AttributeKey.AuthenticationServer ).EnsureTrailingForwardslash();
            var config = GetOpenIdConnectConfiguration();

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = GetAttributeValue( AttributeKey.ApplicationId ),
                ValidateIssuer = true,
                ValidIssuer = authServer,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys
            };

            JwtSecurityTokenHandler tokendHandler = new JwtSecurityTokenHandler();

            _ = tokendHandler.ValidateToken( idToken, validationParameters, out var token );

            var validatedToken = token as JwtSecurityToken;

            if ( validatedToken == null || masterNonce.IsNullOrWhiteSpace() || masterNonce != validatedToken.GetClaimValue( JwtClaimTypes.Nonce ) )
            {
                throw new Exception( "Id Token failed to validate." );
            }

            return token as JwtSecurityToken;
        }

        private OpenIdConnectConfiguration GetOpenIdConnectConfiguration()
        {
            var authServer = GetAttributeValue( AttributeKey.AuthenticationServer ).EnsureTrailingForwardslash();
            string stsDiscoveryEndpoint = $"{authServer}{OidcConstants.Discovery.DiscoveryEndpoint}";

            ConfigurationManager<OpenIdConnectConfiguration> configManager =
                 new ConfigurationManager<OpenIdConnectConfiguration>( stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever() );

            return configManager.GetConfigurationAsync().Result;
        }

        /// <summary>
        /// Handles the oidc user add update.
        /// </summary>
        /// <param name="idToken">The identifier token.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public string HandleOidcUserAddUpdate( JwtSecurityToken idToken, string accessToken = "" )
        {
            // accessToken is required
            if ( accessToken.IsNullOrWhiteSpace() )
            {
                //return null;
            }

            string username = string.Empty;
            string oidcId = idToken.GetClaimValue( JwtClaimTypes.Subject );

            string userName = "OIDC_" + oidcId;
            UserLogin user = null;

            using ( var rockContext = new RockContext() )
            {

                // Query for an existing user
                var userLoginService = new UserLoginService( rockContext );
                user = userLoginService.GetByUserName( userName );

                // If no user was found, see if we can find a match in the person table
                if ( user == null )
                {
                    // Get name/email from Facebook login
                    string lastName = idToken.GetClaimValue( JwtClaimTypes.FamilyName ).ToStringSafe();
                    string middleName = idToken.GetClaimValue( JwtClaimTypes.MiddleName ).ToStringSafe();
                    string firstName = idToken.GetClaimValue( JwtClaimTypes.GivenName ).ToStringSafe();
                    string nickName = idToken.GetClaimValue( JwtClaimTypes.NickName ).ToStringSafe();
                    if ( nickName.IsNullOrWhiteSpace() )
                    {
                        nickName = idToken.GetClaimValue( JwtClaimTypes.Name ).ToStringSafe();
                    }
                    if ( nickName.IsNullOrWhiteSpace() )
                    {
                        nickName = idToken.GetClaimValue( JwtClaimTypes.PreferredUserName ).ToStringSafe();
                    }

                    string email = idToken.GetClaimValue( JwtClaimTypes.Email ).ToStringSafe();

                    Person person = null;

                    // If person had an email, get the first person with the same name and email address.
                    if ( !string.IsNullOrWhiteSpace( email ) )
                    {
                        var personService = new PersonService( rockContext );
                        person = personService.FindPerson( firstName, lastName, email, true );
                    }

                    var personRecordTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    var personStatusPending = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

                    rockContext.WrapTransaction( () =>
                    {
                        if ( person == null )
                        {
                            person = new Person();
                            person.IsSystem = false;
                            person.RecordTypeValueId = personRecordTypeId;
                            person.RecordStatusValueId = personStatusPending;
                            person.FirstName = firstName;
                            person.MiddleName = middleName;
                            person.NickName = nickName;
                            person.LastName = lastName;
                            person.Email = email;
                            person.IsEmailActive = true;
                            person.EmailPreference = EmailPreference.EmailAllowed;

                            var gender = idToken.GetClaimValue( JwtClaimTypes.Gender );
                            UpdatePersonGender( person, gender );

                            var birthDate = idToken.GetClaimValue( JwtClaimTypes.BirthDate );
                            UpdatePersonBirthday( person, birthDate );

                            var phone = idToken.GetClaimValue( JwtClaimTypes.PhoneNumber );
                            UpdatePersonPhoneNumber( person, phone );

                            if ( person != null )
                            {
                                PersonService.SaveNewPerson( person, rockContext, null, false );
                            }
                        }

                        if ( person != null )
                        {
                            var typeId = EntityTypeCache.Get( typeof( OidcClient ) ).Id;
                            user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, typeId, userName, "oidc", true );
                        }
                    } );
                }

                if ( user != null )
                {
                    username = user.UserName;

                    if ( user.PersonId.HasValue )
                    {
                        var converter = new ExpandoObjectConverter();

                        var personService = new PersonService( rockContext );
                        var person = personService.Get( user.PersonId.Value );
                        if ( person != null )
                        {
                            var address = idToken.GetClaimValue( JwtClaimTypes.Address );
                            UpdatePersonAddress( person, address, rockContext );

                            var photoUri = idToken.GetClaimValue( JwtClaimTypes.Picture );
                            UpdatePersonPhoto( person, user.UserName, photoUri, rockContext );
                        }
                    }
                }

                return username;
            }
        }

        private void UpdatePersonPhoto( Person person, string userName, string photoUri, RockContext rockContext )
        {
            if ( !person.PhotoId.HasValue && photoUri.IsNotNullOrWhiteSpace() )
            {
                byte[] bytes = null;
                string contentType = string.Empty;
                var photoExtension = string.Empty;
                using ( var client = new HttpClient() )
                {
                    var photoResponse = client.GetAsync( photoUri ).GetAwaiter().GetResult();
                    if ( photoResponse.IsSuccessStatusCode )
                    {
                        bytes = photoResponse.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                        contentType = photoResponse.Content.Headers.ContentType.MediaType;
                        photoExtension = GetValidExtensionFromContentType( contentType );

                        // if content type is empty assume jpeg.
                        if ( contentType.IsNullOrWhiteSpace() )
                        {
                            using ( MemoryStream mem = new MemoryStream( bytes ) )
                            {
                                using ( var yourImage = Image.FromStream( mem ) )
                                {
                                    contentType = "image/jpeg";
                                    photoExtension = "jpg";

                                    using ( MemoryStream tempImage = new MemoryStream() )
                                    {
                                        yourImage.Save( tempImage, ImageFormat.Jpeg );
                                        bytes = tempImage.ToArray();
                                    }
                                }
                            }
                        }
                    }
                }

                if ( bytes != null && contentType.IsNotNullOrWhiteSpace() && photoExtension.IsNotNullOrWhiteSpace() )
                {
                    // Create and save the image
                    var fileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );
                    if ( fileType != null )
                    {
                        var binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = new BinaryFile();
                        binaryFileService.Add( binaryFile );
                        binaryFile.IsTemporary = false;
                        binaryFile.BinaryFileType = fileType;
                        binaryFile.MimeType = contentType;
                        binaryFile.FileName = $"{userName.RemoveSpecialCharacters()}.{photoExtension}";
                        binaryFile.FileSize = bytes.Length;
                        binaryFile.ContentStream = new MemoryStream( bytes );

                        rockContext.SaveChanges();

                        person.PhotoId = binaryFile.Id;
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        private void UpdatePersonAddress( Person person, string address, RockContext rockContext )
        {
            if ( address.IsNullOrWhiteSpace() )
            {
                return;
            }

            var addressObject = JObject.Parse( address );
            var streetAddress = addressObject.TryGetString( "street_address" );
            var city = addressObject.TryGetString( "locality" );
            var state = addressObject.TryGetString( "region" );
            var postalCode = addressObject.TryGetString( "postal_code" );
            var country = addressObject.TryGetString( "country" );

            var familyGroup = person.GetFamily( rockContext );
            var homeAddressDv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
            var hasHomeAddress = familyGroup.GroupLocations.Where( gl => gl.GroupLocationTypeValueId == homeAddressDv.Id ).Any();
            if ( !hasHomeAddress )
            {
                var streetAddresses = streetAddress.ToStringSafe().Split( new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries );
                var homeLocation = new Location
                {
                    Street1 = streetAddresses.Length > 0 ? streetAddresses[0].Trim() : streetAddress,
                    Street2 = streetAddresses.Length > 1 ? streetAddresses[1].Trim() : string.Empty,
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    Country = country
                };
                familyGroup.GroupLocations.Add( new GroupLocation
                {
                    Location = homeLocation,
                    GroupLocationTypeValueId = homeAddressDv.Id
                } );
                rockContext.SaveChanges();
            }
        }

        private void UpdatePersonPhoneNumber( Person person, string phone )
        {
            if ( phone.IsNullOrWhiteSpace() )
            {
                return;
            }

            var countryCode = string.Empty;
            var extension = string.Empty;
            var extStartIndex = phone.IndexOf( ";ext=" );
            var defaultPhoneNumberType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );

            if ( extStartIndex > 0 )
            {
                extension = phone.Substring( extStartIndex );
                phone = phone.Replace( extension, string.Empty );
                extension = PhoneNumber.CleanNumber( extension.Replace( ";ext=", "" ) );
            }

            phone = phone.Trim();

            if ( phone.StartsWith( "+" ) )
            {
                var phoneParts = phone.Split( new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries );
                countryCode = PhoneNumber.CleanNumber( phoneParts[0] );
                phone = string.Join( "", phoneParts, 1, phoneParts.Length - 1 );
            }

            person.PhoneNumbers.Add( new PhoneNumber
            {
                Number = PhoneNumber.CleanNumber( phone ),
                Extension = extension,
                CountryCode = countryCode,
                NumberTypeValueId = defaultPhoneNumberType.Id
            } );
        }

        private void UpdatePersonGender( Person person, string gender )
        {
            gender = gender.ToStringSafe().ToLower();
            if ( gender == "male" || gender == "m" )
            {
                person.Gender = Gender.Male;
            }
            else if ( gender == "female" || gender == "f" )
            {
                person.Gender = Gender.Female;
            }
            else
            {
                person.Gender = Gender.Unknown;
            }
        }

        /// <summary>
        /// Gets the valid image extension. This method will return only gif, jpg, or png. All other content types will return empty string and therefore should not be processed.
        /// </summary>
        /// <param name="contentType">Content type</param>
        /// <returns></returns>
        private string GetValidExtensionFromContentType( string contentType )
        {
            if ( contentType.Equals( "image/gif", StringComparison.InvariantCultureIgnoreCase ) )
            {
                return "gif";
            }

            if ( contentType.Equals( "image/jpeg", StringComparison.InvariantCultureIgnoreCase ) )
            {
                return "jpg";
            }

            if ( contentType.Equals( "image/png", StringComparison.InvariantCultureIgnoreCase ) )
            {
                return "png";
            }
            return string.Empty;
        }

        private string EncodeBcrypt( string input )
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt( 12 );
            return BCrypt.Net.BCrypt.HashPassword( input, salt );
        }

        private void UpdatePersonBirthday( Person person, string birthDate )
        {
            if ( birthDate.IsNullOrWhiteSpace() )
            {
                return;
            }

            if ( birthDate.Length == 4 )
            {
                person.BirthYear = birthDate.AsIntegerOrNull();
                return;
            }

            var dateParts = birthDate.Split( '-' );
            if ( dateParts.Length == 3 )
            {
                if ( !dateParts[0].Equals( "0000" ) )
                {
                    person.BirthYear = dateParts[0].AsIntegerOrNull();
                }
                person.BirthMonth = dateParts[1].AsIntegerOrNull();
                person.BirthDay = dateParts[2].AsIntegerOrNull();
            }
        }
    }
}
