//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Rock.Models.Crm;
using Rock.Repository.Crm;

namespace Rock.Services.Crm
{
	public partial class AddressService
	{
        public Rock.Models.Crm.Address Geocode( Rock.Address.AddressStub address, int? personId )
        {
            Rock.Models.Crm.Address addressModel = GetByAddressStub( address, personId );

            Geocode( addressModel, personId );

            return addressModel;
        }

        public void Geocode( Rock.Models.Crm.Address address, int? personId )
        {
            address.GeocodeResult = Models.Crm.GeocodeResult.NoMatch;

            foreach ( KeyValuePair<int, Rock.Address.GeocodeService> service in Rock.Address.GeocodeContainer.Instance.Services )
                if ( !service.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.AttributeValues["Active"].Value ) )
                    if ( service.Value.Geocode( address ) )
                        break;

            address.Raw = address.Raw;
            address.GeocodeDate = DateTime.Now;

            Save( address, personId );
        }

        public Rock.Models.Crm.Address Standardize( Rock.Address.AddressStub address, int? personId )
        {
            Rock.Models.Crm.Address addressModel = GetByAddressStub( address, personId );

            Standardize( addressModel, personId );

            return addressModel;
        }

        public void Standardize( Rock.Models.Crm.Address address, int? personId )
        {
            address.StandardizeResult = Models.Crm.StandardizeResult.NoMatch;

            foreach ( KeyValuePair<int, Rock.Address.StandardizeService> service in Rock.Address.StandardizeContainer.Instance.Services )
                if ( !service.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.AttributeValues["Active"].Value ) )
                    if ( service.Value.Standardize( address ) )
                        break;

            address.Raw = address.Raw;
            address.StandardizeDate = DateTime.Now;

            Save( address, personId );
        }

        private Rock.Models.Crm.Address GetByAddressStub( Rock.Address.AddressStub address, int? personId )
        {
            Rock.Models.Crm.Address addressModel = GetByRaw( address.Raw );

            if ( addressModel == null )
                addressModel = GetByStreet1AndStreet2AndCityAndStateAndZip(
                    address.Street1, address.Street2, address.City, address.State, address.Zip );

            if ( addressModel == null )
            {
                addressModel = new Models.Crm.Address();
                addressModel.Street1 = address.Street1;
                addressModel.Street2 = address.Street2;
                addressModel.City = address.City;
                addressModel.State = address.State;
                addressModel.Zip = address.Zip;

                Add( addressModel, personId );
            }

            return addressModel;
        }
		

	}
}