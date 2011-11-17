using System.ComponentModel.Composition;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using Rock.Cms.Security;

namespace Rock.Api.Crm.Address
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

        public string Service { get; set; }
        public string ResultCode { get; set; }
    }
}