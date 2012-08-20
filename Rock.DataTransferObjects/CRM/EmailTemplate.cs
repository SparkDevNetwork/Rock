//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CRM.DTO
{
    /// <summary>
    /// Email Template Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class EmailTemplate : Rock.DTO<EmailTemplate>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id.
		/// </value>
		public int? PersonId { get; set; }

		/// <summary>
		/// Gets or sets the Category.
		/// </summary>
		/// <value>
		/// Category.
		/// </value>
		public string Category { get; set; }

		/// <summary>
		/// Gets or sets the Title.
		/// </summary>
		/// <value>
		/// Title.
		/// </value>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the From.
		/// </summary>
		/// <value>
		/// From.
		/// </value>
		public string From { get; set; }

		/// <summary>
		/// Gets or sets the To.
		/// </summary>
		/// <value>
		/// To.
		/// </value>
		public string To { get; set; }

		/// <summary>
		/// Gets or sets the Cc.
		/// </summary>
		/// <value>
		/// Cc.
		/// </value>
		public string Cc { get; set; }

		/// <summary>
		/// Gets or sets the Bcc.
		/// </summary>
		/// <value>
		/// Bcc.
		/// </value>
		public string Bcc { get; set; }

		/// <summary>
		/// Gets or sets the Subject.
		/// </summary>
		/// <value>
		/// Subject.
		/// </value>
		public string Subject { get; set; }

		/// <summary>
		/// Gets or sets the Body.
		/// </summary>
		/// <value>
		/// Body.
		/// </value>
		public string Body { get; set; }
	}
}
