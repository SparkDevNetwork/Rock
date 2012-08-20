//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Core.DTO
{
    /// <summary>
    /// Attribute Value Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class AttributeValue : Rock.DTO<AttributeValue>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Attribute Id.
		/// </summary>
		/// <value>
		/// Attribute Id.
		/// </value>
		public int AttributeId { get; set; }

		/// <summary>
		/// Gets or sets the Entity Id.
		/// </summary>
		/// <value>
		/// Entity Id.
		/// </value>
		public int? EntityId { get; set; }

		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		public int? Order { get; set; }

		/// <summary>
		/// Gets or sets the Value.
		/// </summary>
		/// <value>
		/// Value.
		/// </value>
		public string Value { get; set; }
	}
}
