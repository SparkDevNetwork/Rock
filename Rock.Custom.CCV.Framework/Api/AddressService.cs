using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace Rock.Custom.CCV.Api
{
    [AspNetCompatibilityRequirements( RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed )]
    public class AddressService : IAddressService
    {
        [WebGet( UriTemplate = "{address}" )]
        public string Geocode( string address )
        {
            return "we geocoded this address: " + address;
        }
    }
}
