using System.ComponentModel.Composition;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using Rock.Cms.Security;

namespace Rock.Address
{
    public class AddressStub
    {
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string StandardizeService { get; set; }
        public string StandardizeResult { get; set; }

        public string GeocodeService { get; set; }
        public string GeocodeResult { get; set; }

        public string Raw
        {
            get
            {
                return string.Format( "{0} {1} {2}, {3} {4}",
                    this.Street1, this.Street2, this.City, this.State, this.Zip );
            }
        }

        public AddressStub()
        {
        }

        public AddressStub( Rock.Models.Crm.Address addressModel )
        {
            this.Street1 = addressModel.Street1;
            this.Street2 = addressModel.Street2;
            this.City = addressModel.City;
            this.State = addressModel.State;
            this.Zip = addressModel.Zip;
            this.Latitude = addressModel.Latitude;
            this.Longitude = addressModel.Longitude;
            this.StandardizeService = addressModel.StandardizeService;
            this.StandardizeResult = addressModel.StandardizeResult;
            this.GeocodeService = addressModel.GeocodeService;
            this.GeocodeResult = addressModel.GeocodeResult;
        }
    }

}