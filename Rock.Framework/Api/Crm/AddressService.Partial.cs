using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using Rock.Api.Crm.Address;

using Rock.Cms.Security;

namespace Rock.Api.Crm
{
    [Export( typeof( IService ) )]
    [ExportMetadata( "RouteName", "api/Crm/Address" )]
    [AspNetCompatibilityRequirements( RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed )]
    public partial class AddressService : IAddressService, IService
    {
        [WebInvoke( Method = "PUT", UriTemplate = "Geocode" )]
        public AddressStub Geocode( AddressStub address )
        {
            if ( address != null )
            {
                foreach ( IGeocodeService service in GeocodeContainer.Instance.Services )
                    if ( service.Geocode(address) )
                        return address;

                address.ResultCode = "No Match";
                return address;
            }
            else
                throw new FaultException( "Invalid Address" );
        }

        [WebInvoke( Method = "PUT", UriTemplate = "Standardize" )]
        public AddressStub Standardize( AddressStub address )
        {
            if ( address != null )
            {
                foreach ( IStandardizeService service in StandardizeContainer.Instance.Services )
                    if ( service.Standardize( address ) )
                        return address;

                address.ResultCode = "No Match";
                return address;
            }
            else
                throw new FaultException( "Invalid Address" );
        }
    }
}
