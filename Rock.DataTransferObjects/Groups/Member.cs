//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Groups.DTO
{
    /// <summary>
    /// Member Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class Member : Rock.DTO<Member>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Group Id.
		/// </summary>
		/// <value>
		/// Group Id.
		/// </value>
		public int GroupId { get; set; }

		/// <summary>
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id.
		/// </value>
		public int PersonId { get; set; }

		/// <summary>
		/// Gets or sets the Group Role Id.
		/// </summary>
		/// <value>
		/// Group Role Id.
		/// </value>
		public int GroupRoleId { get; set; }
	}
}
