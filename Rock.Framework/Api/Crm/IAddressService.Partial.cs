using System.ServiceModel;

using Rock.Address;
using Rock.Models.Crm;

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
        AddressDTO Geocode( AddressDTO address );

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        AddressDTO Standardize( AddressDTO address );
    }
}
