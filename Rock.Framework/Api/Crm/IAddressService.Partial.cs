using System.ServiceModel;

using Rock.Address;

namespace Rock.Api.Crm
{
    public partial interface IAddressService
    {
        [OperationContract]
        AddressStub Geocode( AddressStub address );

        [OperationContract]
        AddressStub Standardize( AddressStub address );
    }
}
