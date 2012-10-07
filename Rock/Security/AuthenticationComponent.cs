//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock.Cms;
using Rock.Extension;

namespace Rock.Security
{
	/// <summary>
	/// Base class for components that perform authentication based on a username and password entered by the user
	/// </summary>
    public abstract class AuthenticationComponent : Component
    {
		/// <summary>
		/// Authenticates the specified user name and password
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		public abstract Boolean Authenticate( User user, string password );

		/// <summary>
		/// Encodes the password.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="newPassword">The new password.</param>
		/// <returns></returns>
		public abstract String EncodePassword( User user, string password );

	}
}