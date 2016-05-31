// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Authenticates a username/password using the Rock database
    /// </summary>
    [Description( "Database Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Database" )]
    [CustomDropdownListField( "Encryption Method", "Which hashing algorithm should be used to store passwords in the database", "BCrypt,HMAC-SHA-1", true, "HMAC-SHA-1" )]
    [IntegerField( "BCrypt Cost Factor", "The higher this number, the more secure BCrypt can be. However it also will be slower.", false, 11 )]
    public class Database : AuthenticationComponent
    {
        private enum EncryptionMethod
        {
            HMACSHA1,
            BCrypt
        };

        private static byte[] _encryptionKey;
        private static List<byte[]> _oldEncryptionKeys;

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public override AuthenticationServiceType ServiceType
        {
            get { return AuthenticationServiceType.Internal; }
        }

        /// <summary>
        /// Determines if user is directed to another site (i.e. Facebook, Gmail, Twitter, etc) to confirm approval of using
        /// that site's credentials for authentication.
        /// </summary>
        /// <value>
        /// The requires remote authentication.
        /// </value>
        public override bool RequiresRemoteAuthentication
        {
            get { return false; }
        }

        /// <summary>
        /// Initializes the <see cref="Database" /> class.
        /// </summary>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">Authentication requires a 'PasswordKey' app setting</exception>
        static Database()
        {
            var passwordKey = ConfigurationManager.AppSettings["PasswordKey"];
            if ( String.IsNullOrWhiteSpace( passwordKey ) )
            {
                throw new ConfigurationErrorsException( "Authentication requires a 'PasswordKey' app setting" );
            }

            _encryptionKey = Encryption.HexToByte( passwordKey );

            // Check for any old encryption keys
            _oldEncryptionKeys = new List<byte[]>();
            int i = 0;
            passwordKey = ConfigurationManager.AppSettings["OldPasswordKey" + ( i > 0 ? i.ToString() : "" )];
            while ( !string.IsNullOrWhiteSpace( passwordKey ) )
            {
                _oldEncryptionKeys.Add( Encryption.HexToByte( passwordKey ) );
                i++;
                passwordKey = ConfigurationManager.AppSettings["OldPasswordKey" + ( i > 0 ? i.ToString() : "" )];
            }
        }

        /// <summary>
        /// Authenticates the specified user name.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public override Boolean Authenticate( UserLogin user, string password )
        {
            try
            {
                if ( IsBcryptHash( user.Password ) )
                {
                    return AuthenticateBcrypt( user, password );
                }

                return AuthenticateSha1( user, password );
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override String EncodePassword( UserLogin user, string password )
        {
            return EncodePassword( user, password, _encryptionKey );
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="encryptionKey">The encryption key.</param>
        /// <returns></returns>
        private string EncodePassword( UserLogin user, string password, byte[] encryptionKey )
        {
            switch ( GetEncryptionMethod() )
            {
                case EncryptionMethod.BCrypt:
                    return EncodeBcrypt( password );

                case EncryptionMethod.HMACSHA1:
                    return EncodeSha1( user.Guid, password, encryptionKey );

                default:
                    throw new NotImplementedException( "The encryption method is not implemented" );
            }
        }

        /// <summary>
        /// Authenticates the user based on a request from a third-party provider.  Will set the username and returnUrl values.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Authenticate( HttpRequest request, out string userName, out string returnUrl )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tests the Http Request to determine if authentication should be tested by this
        /// authentication provider.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool IsReturningFromAuthentication( HttpRequest request )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the URL of an image that should be displayed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string ImageUrl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether [supports change password].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports change password]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsChangePassword
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="warningMessage">The warning message.</param>
        /// <returns>
        /// A <see cref="System.Boolean" /> value that indicates if the password change was successful. <c>true</c> if successful; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.Exception">Cannot change password on external service type</exception>
        public override bool ChangePassword( UserLogin user, string oldPassword, string newPassword, out string warningMessage )
        {
            warningMessage = null;

            if ( !Authenticate( user, oldPassword ) )
            {
                return false;
            }

            SetPassword( user, newPassword );
            return true;
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        public override void SetPassword( UserLogin user, string password )
        {
            user.Password = EncodePassword( user, password );
            user.LastPasswordChangedDateTime = RockDateTime.Now;
            user.IsPasswordChangeRequired = false;
        }

        /// <summary>
        /// Generates the username.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="tryCount">The try count.</param>
        /// <returns></returns>
        public static string GenerateUsername( string firstName, string lastName, int tryCount = 0 )
        {
            // create username
            string username = ( firstName.Substring( 0, 1 ) + lastName ).ToLower();

            if ( tryCount != 0 )
            {
                username = username + tryCount.ToString();
            }

            // check if username exists
            UserLoginService userService = new UserLoginService( new RockContext() );
            var loginExists = userService.Queryable().Where( l => l.UserName == username ).Any();
            if ( !loginExists )
            {
                return username;
            }
            else
            {
                return GenerateUsername( firstName, lastName, tryCount + 1 );
            }
        }

        /// <summary>
        /// Gets the bcrypt cost factor setting or the default value
        /// </summary>
        /// <returns></returns>
        private int GetBCryptCostFactor()
        {
            var factor = GetAttributeValue( "BCryptCostFactor" ).AsIntegerOrNull();
            return factor.HasValue ? factor.Value : 11;
        }

        /// <summary>
        /// Gets the encryption method setting or the default value
        /// </summary>
        /// <returns></returns>
        private EncryptionMethod GetEncryptionMethod()
        {
            var method = GetAttributeValue( "EncryptionMethod" );

            switch ( method.ToLower() )
            {
                case "bcrypt":
                    return EncryptionMethod.BCrypt;

                default:
                    return EncryptionMethod.HMACSHA1;
            }
        }

        /// <summary>
        /// Returns true if the password matches the stored SHA1 hash. If the password matches and the encryption method is not set to
        /// SHA1, the password is rehashed according to the chosen encryption method.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool AuthenticateSha1( UserLogin user, string password )
        {
            bool matches = false;
            bool usesLatestKey = false;

            try
            {
                matches = EncodeSha1( user.Guid, password, _encryptionKey ) == user.Password;

                if ( matches )
                {
                    usesLatestKey = true;
                }
            }
            catch { }

            if ( !matches )
            {
                foreach ( var encryptionKey in _oldEncryptionKeys )
                {
                    try
                    {
                        if ( EncodeSha1( user.Guid, password, encryptionKey ) == user.Password )
                        {
                            matches = true;
                        }
                    }
                    catch { }
                }
            }

            if ( matches && ( GetEncryptionMethod() != EncryptionMethod.HMACSHA1 || !usesLatestKey ) )
            {
                SetNewPassword( user, password );
            }

            return matches;
        }

        /// <summary>
        /// Returns true if the password matches the stored Bcrypt hash. If the password matches and the encryption method is not set to
        /// Bcrypt, the password is rehashed according to the chosen encryption method.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool AuthenticateBcrypt( UserLogin user, string password )
        {
            var hash = user.Password;

            if ( IsBcryptHash( hash ) )
            {
                var currentCost = hash.Substring( 4, 2 ).AsInteger();
                var matches = BCrypt.Net.BCrypt.Verify( password, hash );

                if ( matches && ( currentCost != GetBCryptCostFactor() || GetEncryptionMethod() != EncryptionMethod.BCrypt ) )
                {
                    SetNewPassword( user, password );
                }

                return matches;
            }

            return false;
        }

        /// <summary>
        /// Updates the user's database stored password hash according to the chosen encryption method
        /// </summary>
        /// <param name="user"></param>
        /// <param name="rawPassword"></param>
        /// <returns></returns>
        private UserLogin SetNewPassword( UserLogin user, string rawPassword )
        {
            string hash = null;

            switch ( GetEncryptionMethod() )
            {
                case EncryptionMethod.BCrypt:
                    hash = EncodeBcrypt( rawPassword );
                    break;

                case EncryptionMethod.HMACSHA1:
                    hash = EncodeSha1( user.Guid, rawPassword, _encryptionKey );
                    break;
            }

            if ( hash == null )
            {
                throw new NotImplementedException( "The encryption method is not implemented" );
            }

            var context = new RockContext();
            var userService = new UserLoginService( context );
            var contextUser = userService.Get( user.Id );
            contextUser.Password = hash;
            context.SaveChanges();
            return contextUser;
        }

        /// <summary>
        /// Checks a password hash to see if it is a bcrypt hash
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private bool IsBcryptHash( string hash )
        {
            return Regex.IsMatch( hash, @"^\$2a\$\d{2}\$[\/\.a-zA-Z0-9]{53}$" );
        }

        /// <summary>
        /// Transforms a raw password into a new bcrypt hash
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string EncodeBcrypt( string password )
        {
            var workFactor = GetBCryptCostFactor();
            var salt = BCrypt.Net.BCrypt.GenerateSalt( workFactor );
            return BCrypt.Net.BCrypt.HashPassword( password, salt );
        }

        /// <summary>
        /// Transforms a raw password into a new HMACSHA1 hash
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="password"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        private string EncodeSha1( Guid userGuid, string password, byte[] encryptionKey )
        {
            var hash = new HMACSHA1();
            hash.Key = encryptionKey;

            var uniqueHash = new HMACSHA1();
            uniqueHash.Key = Encryption.HexToByte( userGuid.ToString().Replace( "-", "" ) );

            return Convert.ToBase64String( uniqueHash.ComputeHash( hash.ComputeHash( Encoding.Unicode.GetBytes( password ) ) ) );
        }
    }
}