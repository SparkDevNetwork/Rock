using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using Rock.Address;

using Rock.Cms.Security;

namespace Rock.Api.Crm
{
    public partial class AddressService 
    {
        [WebInvoke( Method = "PUT", UriTemplate = "Geocode" )]
        public AddressStub Geocode( AddressStub address )
        {
            if ( address != null )
            {
                int? personId = null;
                var currentUser = System.Web.Security.Membership.GetUser();
                if ( currentUser != null )
                    personId = currentUser.PersonId();

                Rock.Services.Crm.AddressService addressService = new Services.Crm.AddressService();

                Rock.Models.Crm.Address addressModel = addressService.Geocode( address, personId );

                return new AddressStub( addressModel );
            }
            else
                throw new FaultException( "Invalid Address" );
        }

        [WebInvoke( Method = "PUT", UriTemplate = "Standardize" )]
        public AddressStub Standardize( AddressStub address )
        {
            if ( address != null )
            {
                int? personId = null;
                var currentUser = System.Web.Security.Membership.GetUser();
                if ( currentUser != null )
                    personId = currentUser.PersonId();

                Rock.Services.Crm.AddressService addressService = new Services.Crm.AddressService();

                Rock.Models.Crm.Address addressModel = addressService.Standardize( address, personId );

                return new AddressStub( addressModel );
            }
            else
                throw new FaultException( "Invalid Address" );
        }
    }
}
