using System.ServiceModel;

using Rock.Address;

namespace Rock.Api.Crm
{
    public partial interface IAddressService
    {
        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        AddressStub Geocode( AddressStub address );

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        AddressStub Standardize( AddressStub address );
    }
}
