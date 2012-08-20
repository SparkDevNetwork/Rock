//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Core.DTO
{
    /// <summary>
    /// Attribute Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class Attribute : Rock.DTO<Attribute>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Field Type Id.
		/// </summary>
		/// <value>
		/// Field Type Id.
		/// </value>
		public int FieldTypeId { get; set; }

		/// <summary>
		/// Gets or sets the Entity.
		/// </summary>
		/// <value>
		/// Entity.
		/// </value>
		public string Entity { get; set; }

		/// <summary>
		/// Gets or sets the Entity Qualifier Column.
		/// </summary>
		/// <value>
		/// Entity Qualifier Column.
		/// </value>
		public string EntityQualifierColumn { get; set; }

		/// <summary>
		/// Gets or sets the Entity Qualifier Value.
		/// </summary>
		/// <value>
		/// Entity Qualifier Value.
		/// </value>
		public string EntityQualifierValue { get; set; }

		/// <summary>
		/// Gets or sets the Key.
		/// </summary>
		/// <value>
		/// Key.
		/// </value>
		public string Key { get; set; }

		/// <summary>
		/// Gets or sets the Name.
		/// </summary>
		/// <value>
		/// Name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Category.
		/// </summary>
		/// <value>
		/// Category.
		/// </value>
		public string Category { get; set; }

		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		public int Order { get; set; }

		/// <summary>
		/// Gets or sets the Grid Column.
		/// </summary>
		/// <value>
		/// Grid Column.
		/// </value>
		public bool IsGridColumn { get; set; }

		/// <summary>
		/// Gets or sets the Default Value.
		/// </summary>
		/// <value>
		/// Default Value.
		/// </value>
		public string DefaultValue { get; set; }

		/// <summary>
		/// Gets or sets the Multi Value.
		/// </summary>
		/// <value>
		/// Multi Value.
		/// </value>
		public bool IsMultiValue { get; set; }

		/// <summary>
		/// Gets or sets the Required.
		/// </summary>
		/// <value>
		/// Required.
		/// </value>
		public bool IsRequired { get; set; }
	}
}
