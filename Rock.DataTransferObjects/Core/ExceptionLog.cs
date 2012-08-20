//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Core.DTO
{
    /// <summary>
    /// Exception Log Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class ExceptionLog : Rock.DTO<ExceptionLog>
    {
		/// <summary>
		/// Gets or sets the Parent Id.
		/// </summary>
		/// <value>
		/// Parent Id.
		/// </value>
		public int? ParentId { get; set; }

		/// <summary>
		/// Gets or sets the Site Id.
		/// </summary>
		/// <value>
		/// Site Id.
		/// </value>
		public int? SiteId { get; set; }

		/// <summary>
		/// Gets or sets the Page Id.
		/// </summary>
		/// <value>
		/// Page Id.
		/// </value>
		public int? PageId { get; set; }

		/// <summary>
		/// Gets or sets the Exception Date.
		/// </summary>
		/// <value>
		/// Exception Date.
		/// </value>
		public DateTime ExceptionDate { get; set; }

		/// <summary>
		/// Gets or sets the Has Inner Exception.
		/// </summary>
		/// <value>
		/// Has Inner Exception.
		/// </value>
		public bool? HasInnerException { get; set; }

		/// <summary>
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id.
		/// </value>
		public int? PersonId { get; set; }

		/// <summary>
		/// Gets or sets the Status Code.
		/// </summary>
		/// <value>
		/// Status Code.
		/// </value>
		public string StatusCode { get; set; }

		/// <summary>
		/// Gets or sets the Exception Type.
		/// </summary>
		/// <value>
		/// Exception Type.
		/// </value>
		public string ExceptionType { get; set; }

		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the Source.
		/// </summary>
		/// <value>
		/// Source.
		/// </value>
		public string Source { get; set; }

		/// <summary>
		/// Gets or sets the Stack Trace.
		/// </summary>
		/// <value>
		/// Stack Trace.
		/// </value>
		public string StackTrace { get; set; }

		/// <summary>
		/// Gets or sets the Page Url.
		/// </summary>
		/// <value>
		/// Page Url.
		/// </value>
		public string PageUrl { get; set; }

		/// <summary>
		/// Gets or sets the Server Variables.
		/// </summary>
		/// <value>
		/// Server Variables.
		/// </value>
		public string ServerVariables { get; set; }

		/// <summary>
		/// Gets or sets the Query String.
		/// </summary>
		/// <value>
		/// Query String.
		/// </value>
		public string QueryString { get; set; }

		/// <summary>
		/// Gets or sets the Form.
		/// </summary>
		/// <value>
		/// Form.
		/// </value>
		public string Form { get; set; }

		/// <summary>
		/// Gets or sets the Cookies.
		/// </summary>
		/// <value>
		/// Cookies.
		/// </value>
		public string Cookies { get; set; }
	}
}
