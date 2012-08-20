//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CMS.DTO
{
    /// <summary>
    /// Page Route Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class PageRoute : Rock.DTO<PageRoute>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Page Id.
		/// </summary>
		/// <value>
		/// Page Id.
		/// </value>
		public int PageId { get; set; }

		/// <summary>
		/// Gets or sets the Route.
		/// </summary>
		/// <value>
		/// Route.
		/// </value>
		public string Route { get; set; }
	}
}
