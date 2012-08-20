//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Core.DTO
{
    /// <summary>
    /// Metric Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class Metric : Rock.DTO<Metric>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Type.
		/// </summary>
		/// <value>
		/// Type.
		/// </value>
		public bool Type { get; set; }

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
		/// Gets or sets the Subtitle.
		/// </summary>
		/// <value>
		/// Subtitle.
		/// </value>
		public string Subtitle { get; set; }

		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the MinValue.
		/// </summary>
		/// <value>
		/// MinValue.
		/// </value>
		public int MinValue { get; set; }

		/// <summary>
		/// Gets or sets the MaxValue.
		/// </summary>
		/// <value>
		/// MaxValue.
		/// </value>
		public int MaxValue { get; set; }

		/// <summary>
		/// Gets or sets the CollectionFrequency.
		/// </summary>
		/// <value>
		/// CollectionFrequency.
		/// </value>
		public int CollectionFrequency { get; set; }

		/// <summary>
		/// Gets or sets the LastCollected date.
		/// </summary>
		/// <value>
		/// LastCollected.
		/// </value>
		public DateTime LastCollected { get; set; }

		/// <summary>
		/// Gets or sets the Source.
		/// </summary>
		/// <value>
		/// Source.
		/// </value>
		public string Source { get; set; }

		/// <summary>
		/// Gets or sets the SourceSQL.
		/// </summary>
		/// <value>
		/// SourceSQL.
		/// </value>
		public string SourceSQL { get; set; }

		/// <summary>
		/// Gets or sets the Order.
		/// </summary>
		/// <value>
		/// Order.
		/// </value>
		public int Order { get; set; }
	}
}
