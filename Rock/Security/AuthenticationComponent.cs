//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock.Model;
using Rock.Extension;

namespace Rock.Security
{
    /// <summary>
    /// Base class for components that perform authentication based on a username and password entered by the user
    /// </summary>
    public abstract class AuthenticationComponent : Component
    {
        /// <summary>
        /// Determines if user is directed to another site (i.e. Facebook, Gmail, Twitter, etc) to confirm approval of using
        /// that site's credentials for authentication.
        /// </summary>
        /// <value>
        /// The requires remote authentication.
        /// </value>
        public abstract Boolean RequiresRemoteAuthentication { get; }

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
    }
}