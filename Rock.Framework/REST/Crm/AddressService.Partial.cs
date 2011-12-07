using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Rock.REST.Crm
{
    public partial class AddressService 
    {
        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [WebInvoke( Method = "PUT", UriTemplate = "Geocode" )]
        public Rock.DataTransferObjects.Crm.Address Geocode( Rock.DataTransferObjects.Crm.Address address )
        {
            var currentUser = System.Web.Security.Membership.GetUser();
            if ( currentUser == null )
                throw new WebFaultException<string>("Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                if ( address != null )
                {
                    Rock.Services.Crm.AddressService addressService = new Services.Crm.AddressService();
                    Rock.Models.Crm.Address addressModel = addressService.Geocode( address, currentUser.PersonId() );
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
        public Rock.DataTransferObjects.Crm.Address ApiGeocode( string apiKey, Rock.DataTransferObjects.Crm.Address address )
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                Rock.Services.Cms.UserService userService = new Rock.Services.Cms.UserService();
                Rock.Models.Cms.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

                if ( user != null )
                {
                    if ( address != null )
                    {
                        Rock.Services.Crm.AddressService addressService = new Services.Crm.AddressService();
                        Rock.Models.Crm.Address addressModel = addressService.Geocode( address, user.PersonId );
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
        public Rock.DataTransferObjects.Crm.Address Standardize( Rock.DataTransferObjects.Crm.Address address )
        {
            var currentUser = System.Web.Security.Membership.GetUser();
            if ( currentUser == null )
                throw new WebFaultException<string>( "Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                if ( address != null )
                {
                    Rock.Services.Crm.AddressService addressService = new Services.Crm.AddressService();
                    Rock.Models.Crm.Address addressModel = addressService.Standardize( address, currentUser.PersonId() );
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
        public Rock.DataTransferObjects.Crm.Address ApiStandardize( string apiKey, Rock.DataTransferObjects.Crm.Address address )
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                Rock.Services.Cms.UserService userService = new Rock.Services.Cms.UserService();
                Rock.Models.Cms.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

                if ( user != null )
                {
                    if ( address != null )
                    {
                        Rock.Services.Crm.AddressService addressService = new Services.Crm.AddressService();
                        Rock.Models.Crm.Address addressModel = addressService.Standardize( address, user.PersonId );
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
