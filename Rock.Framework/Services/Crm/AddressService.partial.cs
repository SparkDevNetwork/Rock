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
        public Rock.Models.Crm.Address Standardize( Rock.Address.AddressStub address, int? personId )
        {
            Rock.Models.Crm.Address addressModel = GetByAddressStub( address, personId );

            Standardize( addressModel, personId );

            return addressModel;
        }

        public void Standardize( Rock.Models.Crm.Address address, int? personId )
        {
            Core.ServiceLogService logService = new Core.ServiceLogService();

            foreach ( KeyValuePair<int, Lazy<Rock.Address.StandardizeService, Rock.Address.IStandardizeServiceData>> service in Rock.Address.StandardizeContainer.Instance.Services )
                if ( !service.Value.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.Value.AttributeValues["Active"].Value ) )
                {
                    string result;
                    bool success = service.Value.Value.Standardize( address, out result );

                    Models.Core.ServiceLog log = new Models.Core.ServiceLog();
                    log.Time = DateTime.Now;
                    log.Type = "Address Standardize";
                    log.Name = service.Value.Metadata.ServiceName;
                    log.Input = address.Raw;
                    log.Result = result;
                    log.Success = success;
                    logService.Add( log, personId );
                    logService.Save( log, personId );

                    if ( success )
                    {
                        address.StandardizeService = service.Value.Metadata.ServiceName;
                        address.StandardizeResult = result;
                        address.StandardizeDate = DateTime.Now;
                        break;
                    }
                }

            address.StandardizeAttempt = DateTime.Now;

            Save( address, personId );
        }

        public Rock.Models.Crm.Address Geocode( Rock.Address.AddressStub address, int? personId )
        {
            Rock.Models.Crm.Address addressModel = GetByAddressStub( address, personId );

            Geocode( addressModel, personId );

            return addressModel;
        }

        public void Geocode( Rock.Models.Crm.Address address, int? personId )
        {
            Core.ServiceLogService logService = new Core.ServiceLogService();

            foreach ( KeyValuePair<int, Lazy<Rock.Address.GeocodeService, Rock.Address.IGeocodeServiceData>> service in Rock.Address.GeocodeContainer.Instance.Services )
                if ( !service.Value.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.Value.AttributeValues["Active"].Value ) )
                {
                    string result;
                    bool success = service.Value.Value.Geocode( address, out result );

                    Models.Core.ServiceLog log = new Models.Core.ServiceLog();
                    log.Time = DateTime.Now;
                    log.Type = "Address Geocode";
                    log.Name = service.Value.Metadata.ServiceName;
                    log.Input = address.Raw;
                    log.Result = result;
                    log.Success = success;
                    logService.Add( log, personId );
                    logService.Save( log, personId );

                    if ( success  )
                    {
                        address.GeocodeService = service.Value.Metadata.ServiceName;
                        address.GeocodeResult = result;
                        address.GeocodeDate = DateTime.Now;
                        break;
                    }
                }

            address.GeocodeAttempt = DateTime.Now;

            Save( address, personId );
        }

        private Rock.Models.Crm.Address GetByAddressStub( Rock.Address.AddressStub address, int? personId )
        {
            string raw = address.Raw;

            Rock.Models.Crm.Address addressModel = GetByRaw( raw );

            if ( addressModel == null )
                addressModel = GetByStreet1AndStreet2AndCityAndStateAndZip(
                    address.Street1, address.Street2, address.City, address.State, address.Zip );

            if ( addressModel == null )
            {
                addressModel = new Models.Crm.Address();
                addressModel.Raw = raw;
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