//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ServiceModel;

using Rock.Extension;
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
        Rock.CRM.DTO.Address Geocode( Rock.CRM.DTO.Address address );

        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        Rock.CRM.DTO.Address ApiGeocode( string apiKey, Rock.CRM.DTO.Address address );

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        Rock.CRM.DTO.Address Standardize( Rock.CRM.DTO.Address address );

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [OperationContract]
        Rock.CRM.DTO.Address ApiStandardize( string apiKey, Rock.CRM.DTO.Address address );
    }
}
