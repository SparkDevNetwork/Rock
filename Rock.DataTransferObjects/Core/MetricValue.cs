//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Core.DTO
{
    /// <summary>
    /// MetricValue Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class MetricValue : Rock.DTO<MetricValue>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the MetricIc.
		/// </summary>
		/// <value>
		/// MetricIc.
		/// </value>
		public int MetricId { get; set; }

		/// <summary>
		/// Gets or sets the Value.
		/// </summary>
		/// <value>
		/// Value.
		/// </value>
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the xValue.
		/// </summary>
		/// <value>
		/// xValue.
		/// </value>
		public int xValue { get; set; }

		/// <summary>
		/// Gets or sets the isDateBased flag.
		/// </summary>
		/// <value>
		/// isDateBased.
		/// </value>
		public bool isDateBased { get; set; }

		/// <summary>
		/// Gets or sets the Label.
		/// </summary>
		/// <value>
		/// Label.
		/// </value>
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		public int Order { get; set; }
	}
}
