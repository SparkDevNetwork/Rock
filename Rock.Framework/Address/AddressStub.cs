using System.ComponentModel.Composition;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using Rock.Cms.Security;

namespace Rock.Address
{
    /// <summary>
    /// Used to pass and return address information to the standardization and geocoding WCF
    /// services
    /// </summary>
    public class AddressStub
    {
        /// <summary>
        /// Gets or sets the street1.
        /// </summary>
        /// <value>
        /// The street1.
        /// </value>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the street2.
        /// </summary>
        /// <value>
        /// The street2.
        /// </value>
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>
        /// The zip.
        /// </value>
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double Latitude { get; set; }
        
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="AddressStub"/> is standardized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if standardized; otherwise, <c>false</c>.
        /// </value>
        public bool Standardized { get; set; }

        /// <summary>
        /// Gets or sets the standardize service.
        /// </summary>
        /// <value>
        /// The standardize service.
        /// </value>
        public string StandardizeService { get; set; }

        /// <summary>
        /// Gets or sets the standardize result.
        /// </summary>
        /// <value>
        /// The standardize result.
        /// </value>
        public string StandardizeResult { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AddressStub"/> is geocoded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if geocoded; otherwise, <c>false</c>.
        /// </value>
        public bool Geocoded { get; set; }

        /// <summary>
        /// Gets or sets the geocode service.
        /// </summary>
        /// <value>
        /// The geocode service.
        /// </value>
        public string GeocodeService { get; set; }

        /// <summary>
        /// Gets or sets the geocode result.
        /// </summary>
        /// <value>
        /// The geocode result.
        /// </value>
        public string GeocodeResult { get; set; }

        /// <summary>
        /// Gets the raw address (the Street1, Street2, City, State, and Zip concatenated together)
        /// </summary>
        public string Raw
        {
            get
            {
                return string.Format( "{0} {1} {2}, {3} {4}",
                    this.Street1, this.Street2, this.City, this.State, this.Zip );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressStub"/> class.
        /// </summary>
        public AddressStub()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressStub"/> class.
        /// </summary>
        /// <param name="addressModel">a <see cref="Rock.Models.Crm.Address"/> model.</param>
        public AddressStub( Rock.Models.Crm.Address addressModel )
        {
            this.Street1 = addressModel.Street1;
            this.Street2 = addressModel.Street2;
            this.City = addressModel.City;
            this.State = addressModel.State;
            this.Zip = addressModel.Zip;
            this.Latitude = addressModel.Latitude;
            this.Longitude = addressModel.Longitude;
            this.Standardized = addressModel.StandardizeDate.HasValue;
            this.StandardizeService = addressModel.StandardizeService;
            this.StandardizeResult = addressModel.StandardizeResult;
            this.Geocoded = addressModel.GeocodeDate.HasValue;
            this.GeocodeService = addressModel.GeocodeService;
            this.GeocodeResult = addressModel.GeocodeResult;
        }
    }

}