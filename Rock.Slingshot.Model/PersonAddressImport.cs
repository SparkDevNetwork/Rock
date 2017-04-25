namespace Rock.Slingshot.Model
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "{Street1}" )]
    public class PersonAddressImport
    {
        /// <summary>
        /// Gets or sets the location type value identifier.
        /// </summary>
        /// <value>
        /// The location type value identifier.
        /// </value>
        public int GroupLocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the referenced by this GroupLocation is the mailing address/location for the group.  
        /// This field is only supported in the UI for family groups
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is the mailing address/location for this group.
        /// </value>
        public bool IsMailingLocation { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is the mappable location for this 
        /// NOTE: Rock requires that exactly one of the Addresses of a Family is the mapped location, so BulkUpdate will do as follows
        /// 1) If exactly one of the AddressImport records is IsMappedLocation=true, that address will be stored in Rock as IsMappedLocation=true
        /// 2) If more than one AddressImport records is IsMappedLocation=true, the "first" IsMappedLocation one be stored in Rock as IsMappedLocation=true
        /// 3) If none of AddressImport records is IsMappedLocation=true, the "first" one be stored in Rock as IsMappedLocation=true
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is location; otherwise, <c>false</c>.
        /// </value>
        public bool IsMappedLocation { get; set; }

        /// <summary>
        /// Gets or sets the first line of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the First line of the Location's Street/Mailing Address. If the Location does not have
        /// a Street/Mailing address, this value is null.
        /// </value>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the second line of the Location's Street/Mailing Address. if this Location does not have 
        /// Street/Mailing Address or if the address does not have a 2nd line, this value is null.
        /// </value>
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the city component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the city component of the Location's Street/Mailing Address. If this Location does not have
        /// a Street/Mailing Address this value will be null.
        /// </value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the State component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the state component of the Location's Street/Mailing Address. If this Location does not have 
        /// a Street/Mailing Address, this value will be null.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the country component of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the country component of the Location's Street/Mailing Address. If this Location does not have a 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the Zip/Postal Code component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Zip/Postal Code component of the Location's Street/Mailing Address. If this Location does not have 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double? Longitude { get; set; }
    }
}
