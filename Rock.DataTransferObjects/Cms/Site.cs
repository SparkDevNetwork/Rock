//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CMS.DTO
{
    /// <summary>
    /// Site Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class Site : Rock.DTO<Site>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the Theme.
		/// </summary>
		/// <value>
		/// Theme.
		/// </value>
		public string Theme { get; set; }

		/// <summary>
		/// Gets or sets the Default Page Id.
		/// </summary>
		/// <value>
		/// Default Page Id.
		/// </value>
		public int? DefaultPageId { get; set; }

		/// <summary>
		/// Gets or sets the Favicon Url.
		/// </summary>
		/// <value>
		/// Favicon Url.
		/// </value>
		public string FaviconUrl { get; set; }

		/// <summary>
		/// Gets or sets the Apple Touch Icon Url.
		/// </summary>
		/// <value>
		/// Apple Touch Icon Url.
		/// </value>
		public string AppleTouchIconUrl { get; set; }

		/// <summary>
		/// Gets or sets the Facebook App Id.
		/// </summary>
		/// <value>
		/// Facebook App Id.
		/// </value>
		public string FacebookAppId { get; set; }

		/// <summary>
		/// Gets or sets the Facebook App Secret.
		/// </summary>
		/// <value>
		/// Facebook App Secret.
		/// </value>
		public string FacebookAppSecret { get; set; }

		/// <summary>
		/// Gets or sets the Login Page Reference.
		/// </summary>
		/// <value>
		/// Login Page Reference.
		/// </value>
		public string LoginPageReference { get; set; }

		/// <summary>
		/// Gets or sets the Registration Page Reference.
		/// </summary>
		/// <value>
		/// Registration Page Reference.
		/// </value>
		public string RegistrationPageReference { get; set; }

		/// <summary>
		/// Gets or sets the Error Page.
		/// </summary>
		/// <value>
		/// Error Page.
		/// </value>
		public string ErrorPage { get; set; }
	}
}
