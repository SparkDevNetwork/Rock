//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Rock.REST.CRM
{
    public partial class AddressService 
    {
        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [WebInvoke( Method = "PUT", UriTemplate = "Geocode" )]
        public Rock.CRM.DTO.Address Geocode( Rock.CRM.DTO.Address address )
        {
            var currentUser = Rock.CMS.UserRepository.GetCurrentUser();
            if ( currentUser == null )
                throw new WebFaultException<string>("Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                if ( address != null )
                {
                    Rock.CRM.AddressRepository addressRepository = new Rock.CRM.AddressRepository();
                    Rock.CRM.Address addressModel = addressRepository.Geocode( address, currentUser.PersonId );
                    return addressModel.DataTransferObject;
                }
                else
                    throw new WebFaultException<string>( "Invalid Address", System.Net.HttpStatusCode.BadRequest );
            }
        }

        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [WebInvoke( Method = "PUT", UriTemplate = "Geocode/{apiKey}" )]
        public Rock.CRM.DTO.Address ApiGeocode( string apiKey, Rock.CRM.DTO.Address address )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.CMS.UserRepository userRepository = new Rock.CMS.UserRepository();
                Rock.CMS.User user = userRepository.AsQueryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

                if ( user != null )
                {
                    if ( address != null )
                    {
                        Rock.CRM.AddressRepository addressRepository = new Rock.CRM.AddressRepository();
                        Rock.CRM.Address addressModel = addressRepository.Geocode( address, user.PersonId );
                        return addressModel.DataTransferObject;
                    }
                    else
                        throw new WebFaultException<string>( "Invalid Address", System.Net.HttpStatusCode.BadRequest );
                }
                else
                    throw new WebFaultException<string>( "Invalid API Key", System.Net.HttpStatusCode.Forbidden );
            }
        }

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [WebInvoke( Method = "PUT", UriTemplate = "Standardize" )]
        public Rock.CRM.DTO.Address Standardize( Rock.CRM.DTO.Address address )
        {
            var currentUser = Rock.CMS.UserRepository.GetCurrentUser();
            if ( currentUser == null )
                throw new WebFaultException<string>( "Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                if ( address != null )
                {
                    Rock.CRM.AddressRepository addressRepository = new Rock.CRM.AddressRepository();
                    Rock.CRM.Address addressModel = addressRepository.Standardize( address, currentUser.PersonId );
                    return addressModel.DataTransferObject;
                }
                else
                    throw new WebFaultException<string>( "Invalid Address", System.Net.HttpStatusCode.BadRequest );
            }
        }

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [WebInvoke( Method = "PUT", UriTemplate = "Standardize/{apiKey}" )]
        public Rock.CRM.DTO.Address ApiStandardize( string apiKey, Rock.CRM.DTO.Address address )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.CMS.UserRepository userRepository = new Rock.CMS.UserRepository();
                Rock.CMS.User user = userRepository.AsQueryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

                if ( user != null )
                {
                    if ( address != null )
                    {
                        Rock.CRM.AddressRepository addressRepository = new Rock.CRM.AddressRepository();
                        Rock.CRM.Address addressModel = addressRepository.Standardize( address, user.PersonId );
                        return addressModel.DataTransferObject;
                    }
                    else
                        throw new WebFaultException<string>( "Invalid Address", System.Net.HttpStatusCode.BadRequest );
                }
                else
                    throw new WebFaultException<string>( "Invalid API Key", System.Net.HttpStatusCode.Forbidden );
            }
        }
    }
}
