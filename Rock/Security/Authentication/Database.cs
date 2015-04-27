// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Rock.Data;
using Rock.Model;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Authenticates a username/password using the Rock database
    /// </summary>
    [Description( "Database Authentication Provider" )]
    [Export(typeof(AuthenticationComponent))]
    [ExportMetadata("ComponentName", "Database")]
    public class Database : AuthenticationComponent
    {
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
            bool authenticated = false;

            try
            {
                authenticated = EncodePassword( user, password ) == user.Password;
                if ( authenticated || !_oldEncryptionKeys.Any() )
                {
                    return authenticated;
                }
            }
            catch { }

            foreach( var encryptionKey in _oldEncryptionKeys )
            {
                try
                {
                    if ( EncodePassword( user, password, encryptionKey ) == user.Password )
                    {
                        return true;
                    }
                }
                catch { }
            }

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
            HMACSHA1 hash = new HMACSHA1();
            hash.Key = encryptionKey;

            HMACSHA1 uniqueHash = new HMACSHA1();
            uniqueHash.Key = Encryption.HexToByte( user.Guid.ToString().Replace( "-", "" ) );

            return Convert.ToBase64String( uniqueHash.ComputeHash( hash.ComputeHash( Encoding.Unicode.GetBytes( password ) ) ) );
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
            AuthenticationComponent authenticationComponent = AuthenticationContainer.GetComponent( user.EntityType.Name );
            if ( authenticationComponent == null || !authenticationComponent.IsActive )
            {
                throw new Exception( string.Format( "'{0}' service does not exist, or is not active", user.EntityType.FriendlyName ) );
            }

            if ( authenticationComponent.ServiceType == AuthenticationServiceType.External )
            {
                throw new Exception( "Cannot change password on external service type" );
            }

            if ( !authenticationComponent.Authenticate( user, oldPassword ) )
            {
                return false;
            }

            user.Password = authenticationComponent.EncodePassword( user, newPassword );
            user.LastPasswordChangedDateTime = RockDateTime.Now;

            return true;
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
            string username = (firstName.Substring( 0, 1 ) + lastName).ToLower();

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
                return Database.GenerateUsername( firstName, lastName, tryCount + 1 );
            }
        }

    }
}