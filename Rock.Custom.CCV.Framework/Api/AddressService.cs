using System.ComponentModel.Composition;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace Rock.Custom.CCV.Api
{
    [Export( typeof( Rock.Api.IService ) )]
    [ExportMetadata( "RouteName", "api/CCV/Address" )]
    [AspNetCompatibilityRequirements( RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed )]
    public class AddressService : IAddressService, Rock.Api.IService
    {
        [WebGet( UriTemplate = "{address}" )]
        public string Geocode( string address )
        {
            return "we geocoded this address: " + address;
        }
    }
}
