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
using System.ComponentModel.Composition;
using System.DirectoryServices.AccountManagement;

using Rock.Attribute;
using Rock.Model;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Authenticates a username using Active Directory
    /// </summary>
    [Description( "Active Directory Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "Active Directory" )]
    [TextField( "Server", "The Active Directory server name", true, "", "Server", 0 )]
    [TextField( "Domain", "The network domain that users belongs to", true, "", "Server", 1 )]
    [BooleanField( "Allow Change Password", "Set to true to allow user to change their Active Directory user password from the Rock system", false, "Server" )]
    [Rock.SystemGuid.EntityTypeGuid( "8057ABAB-6AAC-4872-A11F-AC0D52AB40F6" )]
    public class ActiveDirectory : AuthenticationComponent
    {
        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public override AuthenticationServiceType ServiceType
        {
            get { return AuthenticationServiceType.External; }
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
        /// Authenticates the specified user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public override bool Authenticate( UserLogin user, string password )
        {
            var serverName = GetAttributeValue( "Server" );
            using ( var context = new PrincipalContext( ContextType.Domain, serverName ) )
            {
                /* 09/29/2022 MDP 

                   IMPORTANT!: ValidateCredentials can return a 'false-positive' if the Guest Account is enabled in the domain!
                   See https://stackoverflow.com/questions/290548/validate-a-username-and-password-against-active-directory#comment8867466_499716
                   which says 'if the domain-level Guest account is enabled, ValidateCredentials returns true if you give it a non-existant user'

                   Also, when testing locally, it also returns true if the user exists but password is incorrect.

                   In other words, when Guest Account is Enabled, ValidateCredentials always returns true no matter what!
               */

                // First see if ValidateCredentials returns false. If so, we know for sure that the password is invalid.
                // However, if ValidateCredentials returns true, we need to make sure it isn't a false-positive, so we'll use
                // FindByIdentity to make sure the username and password is actually valid.
                var validateCredentials = context.ValidateCredentials( user.UserName, password );
                if ( !validateCredentials )
                {
                    // Invalid username/password so return false.
                    return false;
                }

                UserPrincipal userPrincipal;

                try
                {
                    /* 09/29/2022 MDP

                     We have to pass the username and password to the new PrincipalContext, otherwise UserPrincipal.FindByIdentity won't work.
                     In the case of an incorrect username/password, new PrincipalContext will still created (without an exception), but then 
                     UserPrincipal.FindByIdentity will throw an 'invaid username/password' exception (see notes below).

                     */

                    using ( var findByIdentityContext = new PrincipalContext( ContextType.Domain, serverName, user.UserName, password ) )
                    {
                        userPrincipal = UserPrincipal.FindByIdentity( findByIdentityContext, user.UserName );
                    }

                }
                catch ( Exception )
                {
                    /* 10/3/2022 MDP

                    In the case of possible False-Positive on ValidateCredentials:
                         UserName/Password is valid: FindByIdentity finds the user/   
                         Username/Password is invalid: FindByIdentity will throw 'Incorrect Username/Password exception'/
                         UserName doesn't exist: FindByIdentity will throw 'Account Directory not available'/
                         UserName/Password is valid, but account is disabled: FindByIdentity will throw 'Account Directory not available'.

                    Also note that the Exception type is just a generic OLE exception with different messages on the problem, so we can't really check for specific exception.
                    
                     */

                    userPrincipal = null;
                }

                // If the userPrincipal is not null then FindByIdentity discovered the user exists and the password is valid, so return true. Otherwise return false.
                return userPrincipal != null;
            }
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override string EncodePassword( UserLogin user, string password )
        {
            return null;
        }

        /// <summary>
        /// Authenticates the user based on a request from a third-party provider.  Will set the username and returnUrl values.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Authenticate( System.Web.HttpRequest request, out string userName, out string returnUrl )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates the log in URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Uri GenerateLoginUrl( System.Web.HttpRequest request )
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
        public override bool IsReturningFromAuthentication( System.Web.HttpRequest request )
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
                return GetAttributeValue( "AllowChangePassword" ).AsBoolean();
            }
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
            warningMessage = null;
            string username = user.UserName;
            if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "Domain" ) ) )
            {
                username = string.Format( @"{0}\{1}", GetAttributeValue( "Domain" ), user.UserName );
            }

            var context = new PrincipalContext( ContextType.Domain, GetAttributeValue( "Server" ) );
            using ( context )
            {
                var userPrincipal = UserPrincipal.FindByIdentity( context, username );
                if ( userPrincipal != null )
                {
                    try
                    {
                        userPrincipal.ChangePassword( oldPassword, newPassword );
                        return true;
                    }
                    catch ( PasswordException pex )
                    {
                        warningMessage = pex.Message;
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void SetPassword( UserLogin user, string password )
        {
            string username = user.UserName;
            if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "Domain" ) ) )
            {
                username = string.Format( @"{0}\{1}", GetAttributeValue( "Domain" ), user.UserName );
            }

            var context = new PrincipalContext( ContextType.Domain, GetAttributeValue( "Server" ) );
            using ( context )
            {
                var userPrincipal = UserPrincipal.FindByIdentity( context, username );
                if ( userPrincipal != null )
                {
                    userPrincipal.SetPassword( password );
                }
            }
        }
    }
}