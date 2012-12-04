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
    public abstract class AuthenticationComponent : ComponentManaged
    {
        /// <summary>
        /// Authenticates the specified user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public abstract Boolean Authenticate( UserLogin user, string password );

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public abstract String EncodePassword( UserLogin user, string password );

    }
}