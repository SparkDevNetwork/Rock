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
	/// Base class for components that authenticate user by redirecting to third-party site (i.e. Facebook, Twitter, Google, etc)
	/// </summary>
	public abstract class ExternalAuthenticationComponent : Component
    {
		/// <summary>
		/// Tests the Http Request to determine if authentication should be tested by this 
		/// authentication provider.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public abstract Boolean IsReturningFromAuthentication( HttpRequest request);


		/// <summary>
		/// Gets the external url to redirect user to
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public abstract string ExternalUrl( HttpRequest request );

		/// <summary>
		/// Authenticates the user based on a request from a third-party provider.  Will set the username and returnUrl values.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="userName">Name of the user.</param>
		/// <param name="returnUrl">The return URL.</param>
		/// <returns></returns>
		public abstract Boolean Authenticate( HttpRequest request, out string userName, out string returnUrl );

		/// <summary>
		/// Gets the URL of an image that should be displayed.
		/// </summary>
		/// <returns></returns>
		public abstract String ImageUrl();
    }
}