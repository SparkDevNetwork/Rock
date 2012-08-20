//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Groups.DTO
{
    /// <summary>
    /// Group Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class Group : Rock.DTO<Group>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Parent Group Id.
		/// </summary>
		/// <value>
		/// Parent Group Id.
		/// </value>
		public int? ParentGroupId { get; set; }

		/// <summary>
		/// Gets or sets the Group Type Id.
		/// </summary>
		/// <value>
		/// Group Type Id.
		/// </value>
		public int GroupTypeId { get; set; }

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
		/// Gets or sets the Is Security Role.
		/// </summary>
		/// <value>
		/// Is Security Role.
		/// </value>
		public bool IsSecurityRole { get; set; }
	}
}
