//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Core.DTO
{
    /// <summary>
    /// Entity Change Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class EntityChange : Rock.DTO<EntityChange>
    {
		/// <summary>
		/// Gets or sets the Change Set.
		/// </summary>
		/// <value>
		/// Change Set.
		/// </value>
		public Guid ChangeSet { get; set; }

		/// <summary>
		/// Gets or sets the Change Type.
		/// </summary>
		/// <value>
		/// Change Type.
		/// </value>
		public string ChangeType { get; set; }

		/// <summary>
		/// Gets or sets the Entity Type.
		/// </summary>
		/// <value>
		/// Entity Type.
		/// </value>
		public string EntityType { get; set; }

		/// <summary>
		/// Gets or sets the Entity Id.
		/// </summary>
		/// <value>
		/// Entity Id.
		/// </value>
		public int EntityId { get; set; }

		/// <summary>
		/// Gets or sets the Property.
		/// </summary>
		/// <value>
		/// Property.
		/// </value>
		public string Property { get; set; }

		/// <summary>
		/// Gets or sets the Original Value.
		/// </summary>
		/// <value>
		/// Original Value.
		/// </value>
		public string OriginalValue { get; set; }

		/// <summary>
		/// Gets or sets the Current Value.
		/// </summary>
		/// <value>
		/// Current Value.
		/// </value>
		public string CurrentValue { get; set; }
	}
}
