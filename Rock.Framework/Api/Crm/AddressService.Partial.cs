using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using Rock.Address;
using Rock.Models.Crm;

using Rock.Cms.Security;

namespace Rock.Api.Crm
{
    public partial class AddressService 
    {
        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [WebInvoke( Method = "PUT", UriTemplate = "Geocode" )]
        public AddressDTO Geocode( AddressDTO address )
        {
            if ( address != null )
            {
                int? personId = null;
                var currentUser = System.Web.Security.Membership.GetUser();
                if ( currentUser != null )
                    personId = currentUser.PersonId();

                using ( new Rock.Helpers.UnitOfWorkScope() )
                {
                    Rock.Services.Crm.AddressService addressService = new Services.Crm.AddressService();
                    Rock.Models.Crm.Address addressModel = addressService.Geocode( address, personId );
                    return addressModel.DataTransferObject;
                }
            }
            else
                throw new FaultException( "Invalid Address" );
        }

        /// <summary>
        /// Standardizes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [WebInvoke( Method = "PUT", UriTemplate = "Standardize" )]
        public AddressDTO Standardize( AddressDTO address )
        {
            if ( address != null )
            {
                int? personId = null;
                var currentUser = System.Web.Security.Membership.GetUser();
                if ( currentUser != null )
                    personId = currentUser.PersonId();

                using ( new Rock.Helpers.UnitOfWorkScope() )
                {
                    Rock.Services.Crm.AddressService addressService = new Services.Crm.AddressService();
                    Rock.Models.Crm.Address addressModel = addressService.Standardize( address, personId );
                    return addressModel.DataTransferObject;
                }

            }
            else
                throw new FaultException( "Invalid Address" );
        }
    }
}
