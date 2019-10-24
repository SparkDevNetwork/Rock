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
using System.Web;
using Rock.Web.Cache;
using Rock.Extension;
using Rock.Model;

namespace Rock.Security
{
    /// <summary>
    /// Base class for components that perform authentication based on a username and password entered by the user
    /// </summary>
    public abstract class AuthenticationComponent : Component
    {
        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public abstract AuthenticationServiceType ServiceType { get; }

        /// <summary>
        /// Determines if user is directed to another site (i.e. Facebook, Gmail, Twitter, etc) to confirm approval of using
        /// that site's credentials for authentication.
        /// </summary>
        /// <value>
        /// The requires remote authentication.
        /// </value>
        public abstract Boolean RequiresRemoteAuthentication { get; }

        /// <summary>
        /// Authenticates the user based on user name and password. If the attempt is a failure,
        /// the the userlogin model is updated and possibly locked according to the global attributes
        /// PasswordAttemptWindow and MaxInvalidPasswordAttempts
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public bool AuthenticateAndTrack( UserLogin user, string password )
        {
            if ( user == null )
            {
                return false;
            }

            if ( user.IsLockedOut.HasValue && user.IsLockedOut.Value )
            {
                return false;
            }

            var isSuccess = false;

            try
            {
                isSuccess = Authenticate( user, password );

                if ( !isSuccess )
                {
                    UserLoginService.UpdateFailureCount( user );
                }
            }
            catch {
                // If an exception occured we want to return the default "false" or the
                // value set by the Authenticate method if that succeeded.  Nothing to do
                // here.
            }

            return isSuccess;
        }

        /// <summary>
        /// Authenticates the user based on user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public abstract Boolean Authenticate( UserLogin user, string password );

        /// <summary>
        /// Authenticates the user based on a request from a third-party provider.  Will set the username and returnUrl values.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        public abstract Boolean Authenticate( HttpRequest request, out string userName, out string returnUrl );

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public abstract String EncodePassword( UserLogin user, string password );

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public abstract Uri GenerateLoginUrl( HttpRequest request );

        /// <summary>
        /// Tests the Http Request to determine if authentication should be tested by this 
        /// authentication provider.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public abstract Boolean IsReturningFromAuthentication( HttpRequest request );

        /// <summary>
        /// Gets the URL of an image that should be displayed.
        /// </summary>
        /// <returns></returns>
        public abstract String ImageUrl();

        /// <summary>
        /// Gets a value indicating whether [supports change password].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports change password]; otherwise, <c>false</c>.
        /// </value>
        public abstract Boolean SupportsChangePassword { get; }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="warningMessage">The warning message.</param>
        /// <returns></returns>
        public abstract bool ChangePassword( UserLogin user, string oldPassword, string newPassword, out string warningMessage );

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        public abstract void SetPassword( UserLogin user, string password );

        /// <summary>
        /// Gets the login button text.
        /// </summary>
        /// <value>
        /// The login button text.
        /// </value>
        public virtual string LoginButtonText
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the login button CSS class.
        /// </summary>
        /// <value>
        /// The login button CSS class.
        /// </value>
        public virtual string LoginButtonCssClass
        {
            get
            {
                return this.GetType().Name.ToLower();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Rock UI should prompt for the username
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prompt for user name]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool PromptForUserName
        {
            get
            {
                return ServiceType == AuthenticationServiceType.Internal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Rock UI should prompt for a password
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prompt for password]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool PromptForPassword
        {
            get
            {
                return ServiceType == AuthenticationServiceType.Internal;
            }
        }
    }
}