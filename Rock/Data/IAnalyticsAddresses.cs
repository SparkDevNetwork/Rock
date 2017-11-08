using System.Data.Entity.Spatial;

/// <summary>
/// 
/// </summary>
namespace Rock.Data
{
    /// <summary>
    /// Interface for Analytics tables that contain Mailing and Mapped Address fields
    /// </summary>
    public interface IAnalyticsAddresses
    {
        /// <summary>
        /// Gets or sets the mailing address city.
        /// </summary>
        /// <value>
        /// The mailing address city.
        /// </value>
        string MailingAddressCity { get; set; }

        /// <summary>
        /// Gets or sets the mailing address country.
        /// </summary>
        /// <value>
        /// The mailing address country.
        /// </value>
        string MailingAddressCountry { get; set; }

        /// <summary>
        /// Gets or sets the mailing address county.
        /// </summary>
        /// <value>
        /// The mailing address county.
        /// </value>
        string MailingAddressCounty { get; set; }

        /// <summary>
        /// Gets or sets the mailing address geo fence.
        /// </summary>
        /// <value>
        /// The mailing address geo fence.
        /// </value>
        DbGeography MailingAddressGeoFence { get; set; }

        /// <summary>
        /// Gets or sets the mailing address geo point.
        /// </summary>
        /// <value>
        /// The mailing address geo point.
        /// </value>
        DbGeography MailingAddressGeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the mailing address latitude.
        /// </summary>
        /// <value>
        /// The mailing address latitude.
        /// </value>
        double? MailingAddressLatitude { get; set; }

        /// <summary>
        /// Gets or sets the mailing address longitude.
        /// </summary>
        /// <value>
        /// The mailing address longitude.
        /// </value>
        double? MailingAddressLongitude { get; set; }

        /// <summary>
        /// Gets or sets the mailing address postal code.
        /// </summary>
        /// <value>
        /// The mailing address postal code.
        /// </value>
        string MailingAddressPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the state of the mailing address.
        /// </summary>
        /// <value>
        /// The state of the mailing address.
        /// </value>
        string MailingAddressState { get; set; }

        /// <summary>
        /// Gets or sets the mailing address street1.
        /// </summary>
        /// <value>
        /// The mailing address street1.
        /// </value>
        string MailingAddressStreet1 { get; set; }

        /// <summary>
        /// Gets or sets the mailing address street2.
        /// </summary>
        /// <value>
        /// The mailing address street2.
        /// </value>
        string MailingAddressStreet2 { get; set; }

        /// <summary>
        /// Gets or sets the mapped address city.
        /// </summary>
        /// <value>
        /// The mapped address city.
        /// </value>
        string MappedAddressCity { get; set; }

        /// <summary>
        /// Gets or sets the mapped address country.
        /// </summary>
        /// <value>
        /// The mapped address country.
        /// </value>
        string MappedAddressCountry { get; set; }

        /// <summary>
        /// Gets or sets the mapped address county.
        /// </summary>
        /// <value>
        /// The mapped address county.
        /// </value>
        string MappedAddressCounty { get; set; }

        /// <summary>
        /// Gets or sets the mapped address geo fence.
        /// </summary>
        /// <value>
        /// The mapped address geo fence.
        /// </value>
        DbGeography MappedAddressGeoFence { get; set; }

        /// <summary>
        /// Gets or sets the mapped address geo point.
        /// </summary>
        /// <value>
        /// The mapped address geo point.
        /// </value>
        DbGeography MappedAddressGeoPoint { get; set; }

        /// <summary>
        /// Gets or sets the mapped address latitude.
        /// </summary>
        /// <value>
        /// The mapped address latitude.
        /// </value>
        double? MappedAddressLatitude { get; set; }

        /// <summary>
        /// Gets or sets the mapped address longitude.
        /// </summary>
        /// <value>
        /// The mapped address longitude.
        /// </value>
        double? MappedAddressLongitude { get; set; }

        /// <summary>
        /// Gets or sets the mapped address postal code.
        /// </summary>
        /// <value>
        /// The mapped address postal code.
        /// </value>
        string MappedAddressPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the state of the mapped address.
        /// </summary>
        /// <value>
        /// The state of the mapped address.
        /// </value>
        string MappedAddressState { get; set; }

        /// <summary>
        /// Gets or sets the mapped address street1.
        /// </summary>
        /// <value>
        /// The mapped address street1.
        /// </value>
        string MappedAddressStreet1 { get; set; }

        /// <summary>
        /// Gets or sets the mapped address street2.
        /// </summary>
        /// <value>
        /// The mapped address street2.
        /// </value>
        string MappedAddressStreet2 { get; set; }
    }
}