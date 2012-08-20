//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CMS.DTO
{
    /// <summary>
    /// Site Domain Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class SiteDomain : Rock.DTO<SiteDomain>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Site Id.
		/// </summary>
		/// <value>
		/// Site Id.
		/// </value>
		public int SiteId { get; set; }

		/// <summary>
		/// Gets or sets the Domain.
		/// </summary>
		/// <value>
		/// Domain.
		/// </value>
		public string Domain { get; set; }
	}
}
