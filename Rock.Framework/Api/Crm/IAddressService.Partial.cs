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
        Rock.DataTransferObjects.Crm.Address Geocode( Rock.DataTransferObjects.Crm.Address address );

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        Rock.DataTransferObjects.Crm.Address Standardize( Rock.DataTransferObjects.Crm.Address address );
    }
}
