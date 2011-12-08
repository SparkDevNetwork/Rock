using System.ServiceModel;

using Rock.Address;
using Rock.CRM;

namespace Rock.REST.CRM
{
    public partial interface IAddressService
    {
        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        Rock.DataTransferObjects.CRM.Address Geocode( Rock.DataTransferObjects.CRM.Address address );

        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        Rock.DataTransferObjects.CRM.Address ApiGeocode( string apiKey, Rock.DataTransferObjects.CRM.Address address );

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        Rock.DataTransferObjects.CRM.Address Standardize( Rock.DataTransferObjects.CRM.Address address );

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        Rock.DataTransferObjects.CRM.Address ApiStandardize( string apiKey, Rock.DataTransferObjects.CRM.Address address );
    }
}
