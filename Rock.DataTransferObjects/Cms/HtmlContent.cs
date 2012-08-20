//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CMS.DTO
{
    /// <summary>
    /// Html Content Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class HtmlContent : Rock.DTO<HtmlContent>
    {
		/// <summary>
		/// Gets or sets the Block Id.
		/// </summary>
		/// <value>
		/// Block Id.
		/// </value>
		public int BlockId { get; set; }

		/// <summary>
		/// Gets or sets the Entity Value.
		/// </summary>
		/// <value>
		/// Entity Value.
		/// </value>
		public string EntityValue { get; set; }

		/// <summary>
		/// Gets or sets the Version.
		/// </summary>
		/// <value>
		/// Version.
		/// </value>
		public int Version { get; set; }

		/// <summary>
		/// Gets or sets the Content.
		/// </summary>
		/// <value>
		/// Content.
		/// </value>
		public string Content { get; set; }

		/// <summary>
		/// Gets or sets the Approved.
		/// </summary>
		/// <value>
		/// Approved.
		/// </value>
		public bool IsApproved { get; set; }

		/// <summary>
		/// Gets or sets the Approved By Person Id.
		/// </summary>
		/// <value>
		/// Approved By Person Id.
		/// </value>
		public int? ApprovedByPersonId { get; set; }

		/// <summary>
		/// Gets or sets the Approved Date Time.
		/// </summary>
		/// <value>
		/// Approved Date Time.
		/// </value>
		public DateTime? ApprovedDateTime { get; set; }
		/// <summary>
		/// Gets or sets the Start Date Time.
		/// </summary>
		/// <value>
		/// Start Date Time.
		/// </value>
		public DateTime? StartDateTime { get; set; }

		/// <summary>
		/// Gets or sets the Expire Date Time.
		/// </summary>
		/// <value>
		/// Expire Date Time.
		/// </value>
		public DateTime? ExpireDateTime { get; set; }
	}
}
