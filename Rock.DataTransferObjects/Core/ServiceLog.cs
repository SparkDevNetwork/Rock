//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Core.DTO
{
    /// <summary>
    /// Service Log Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class ServiceLog : Rock.DTO<ServiceLog>
    {
		/// <summary>
		/// Gets or sets the Time.
		/// </summary>
		/// <value>
		/// Time.
		/// </value>
		public DateTime? Time { get; set; }

		/// <summary>
		/// Gets or sets the Input.
		/// </summary>
		/// <value>
		/// Input.
		/// </value>
		public string Input { get; set; }

		/// <summary>
		/// Gets or sets the Type.
		/// </summary>
		/// <value>
		/// Type.
		/// </value>
		public string Type { get; set; }

		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Result.
		/// </summary>
		/// <value>
		/// Result.
		/// </value>
		public string Result { get; set; }

		/// <summary>
		/// Gets or sets the Success.
		/// </summary>
		/// <value>
		/// Success.
		/// </value>
		public bool Success { get; set; }
	}
}
