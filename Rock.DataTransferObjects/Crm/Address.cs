//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CRM.DTO
{
    /// <summary>
    /// Address Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class Address : Rock.DTO<Address>
    {
		/// <summary>
		/// Gets or sets the Raw.
		/// </summary>
		/// <value>
		/// Raw.
		/// </value>
		public string Raw { get; set; }

		/// <summary>
		/// Gets or sets the Street 1.
		/// </summary>
		/// <value>
		/// Street 1.
		/// </value>
		public string Street1 { get; set; }

		/// <summary>
		/// Gets or sets the Street 2.
		/// </summary>
		/// <value>
		/// Street 2.
		/// </value>
		public string Street2 { get; set; }

		/// <summary>
		/// Gets or sets the City.
		/// </summary>
		/// <value>
		/// City.
		/// </value>
		public string City { get; set; }

		/// <summary>
		/// Gets or sets the State.
		/// </summary>
		/// <value>
		/// State.
		/// </value>
		public string State { get; set; }

		/// <summary>
		/// Gets or sets the Country.
		/// </summary>
		/// <value>
		/// Country.
		/// </value>
		public string Country { get; set; }

		/// <summary>
		/// Gets or sets the Zip.
		/// </summary>
		/// <value>
		/// Zip.
		/// </value>
		public string Zip { get; set; }

		/// <summary>
		/// Gets or sets the Latitude.
		/// </summary>
		/// <value>
		/// Latitude.
		/// </value>
		public double Latitude { get; set; }

		/// <summary>
		/// Gets or sets the Longitude.
		/// </summary>
		/// <value>
		/// Longitude.
		/// </value>
		public double Longitude { get; set; }

		/// <summary>
		/// Gets or sets the Standardize Attempt.
		/// </summary>
		/// <value>
		/// Standardize Attempt.
		/// </value>
		public DateTime? StandardizeAttempt { get; set; }

		/// <summary>
		/// Gets or sets the Standardize Service.
		/// </summary>
		/// <value>
		/// Standardize Service.
		/// </value>
		public string StandardizeService { get; set; }

		/// <summary>
		/// Gets or sets the Standardize Result.
		/// </summary>
		/// <value>
		/// Standardize Result.
		/// </value>
		public string StandardizeResult { get; set; }

		/// <summary>
		/// Gets or sets the Standardize Date.
		/// </summary>
		/// <value>
		/// Standardize Date.
		/// </value>
		public DateTime? StandardizeDate { get; set; }

		/// <summary>
		/// Gets or sets the Geocode Attempt.
		/// </summary>
		/// <value>
		/// Geocode Attempt.
		/// </value>
		public DateTime? GeocodeAttempt { get; set; }

		/// <summary>
		/// Gets or sets the Geocode Service.
		/// </summary>
		/// <value>
		/// Geocode Service.
		/// </value>
		public string GeocodeService { get; set; }

		/// <summary>
		/// Gets or sets the Geocode Result.
		/// </summary>
		/// <value>
		/// Geocode Result.
		/// </value>
		public string GeocodeResult { get; set; }

		/// <summary>
		/// Gets or sets the Geocode Date.
		/// </summary>
		/// <value>
		/// Geocode Date.
		/// </value>
		public DateTime? GeocodeDate { get; set; }
	}
}
