//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CMS.DTO
{
    /// <summary>
    /// Page Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class Page : Rock.DTO<Page>
    {
		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Title.
		/// </summary>
		/// <value>
		/// Title.
		/// </value>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Parent Page Id.
		/// </summary>
		/// <value>
		/// Parent Page Id.
		/// </value>
		public int? ParentPageId { get; set; }

		/// <summary>
		/// Gets or sets the Site Id.
		/// </summary>
		/// <value>
		/// Site Id.
		/// </value>
		public int? SiteId { get; set; }

		/// <summary>
		/// Gets or sets the Layout.
		/// </summary>
		/// <value>
		/// Layout.
		/// </value>
		public string Layout { get; set; }

		/// <summary>
		/// Gets or sets the Requires Encryption.
		/// </summary>
		/// <value>
		/// Requires Encryption.
		/// </value>
		public bool RequiresEncryption { get; set; }

		/// <summary>
		/// Gets or sets the Enable View State.
		/// </summary>
		/// <value>
		/// Enable View State.
		/// </value>
		public bool EnableViewState { get; set; }

		/// <summary>
		/// Gets or sets the Menu Display Description.
		/// </summary>
		/// <value>
		/// Menu Display Description.
		/// </value>
		public bool MenuDisplayDescription { get; set; }

		/// <summary>
		/// Gets or sets the Menu Display Icon.
		/// </summary>
		/// <value>
		/// Menu Display Icon.
		/// </value>
		public bool MenuDisplayIcon { get; set; }

		/// <summary>
		/// Gets or sets the Menu Display Child Pages.
		/// </summary>
		/// <value>
		/// Menu Display Child Pages.
		/// </value>
		public bool MenuDisplayChildPages { get; set; }

		/// <summary>
		/// Gets or sets the Display In Nav When.
		/// </summary>
		/// <value>
		/// Display In Nav When.
		/// </value>
		public int DisplayInNavWhen { get; set; }

		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		public int Order { get; set; }

		/// <summary>
		/// Gets or sets the Output Cache Duration.
		/// </summary>
		/// <value>
		/// Output Cache Duration.
		/// </value>
		public int OutputCacheDuration { get; set; }

		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the Include Admin Footer.
		/// </summary>
		/// <value>
		/// Include Admin Footer.
		/// </value>
		public bool IncludeAdminFooter { get; set; }

        /// <summary>
		/// Gets or sets the Icon Url.
		/// </summary>
		/// <value>
		/// Icon Url.
		/// </value>
		public string IconUrl { get; set; }
	}
}
