//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CMS.DTO
{
    /// <summary>
    /// File Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class File : Rock.DTO<File>
    {
		/// <summary>
		/// Gets or sets the Temporary.
		/// </summary>
		/// <value>
		/// Temporary.
		/// </value>
		public bool IsTemporary { get; set; }

		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Data.
		/// </summary>
		/// <value>
		/// Data.
		/// </value>
		public byte[] Data { get; set; }

		/// <summary>
		/// Gets or sets the Url.
		/// </summary>
		/// <value>
		/// Url.
		/// </value>
		public string Url { get; set; }

		/// <summary>
		/// Gets or sets the File Name.
		/// </summary>
		/// <value>
		/// File Name.
		/// </value>
		public string FileName { get; set; }

		/// <summary>
		/// Gets or sets the Mime Type.
		/// </summary>
		/// <value>
		/// Mime Type.
		/// </value>
		public string MimeType { get; set; }

		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		public string Description { get; set; }
	}
}
