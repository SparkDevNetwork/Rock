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
        /// <summary>
        /// Standardizes the specified <see cref="AddressDTO"/>
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public Rock.Models.Crm.Address Standardize( Rock.DataTransferObjects.Crm.Address address, int? personId )
        {
            Rock.Models.Crm.Address addressModel = GetByAddressDTO( address, personId );

            Standardize( addressModel, personId );

            return addressModel;
        }

        /// <summary>
        /// Standardizes the specified <see cref="Rock.Models.Crm.Address"/>
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="personId">The person id.</param>
        public void Standardize( Rock.Models.Crm.Address address, int? personId )
        {
            Core.ServiceLogService logService = new Core.ServiceLogService();
            string inputAddress = address.ToString();

            // Try each of the standardization services that were found through MEF
            foreach ( KeyValuePair<int, Lazy<Rock.Address.StandardizeService, Rock.Address.IStandardizeServiceData>> service in Rock.Address.StandardizeContainer.Instance.Services )
                if ( !service.Value.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.Value.AttributeValues["Active"].Value ) )
                {
                    string result;
                    bool success = service.Value.Value.Standardize( address, out result );

                    // Log the results of the service
                    Models.Core.ServiceLog log = new Models.Core.ServiceLog();
                    log.Time = DateTime.Now;
                    log.Type = "Address Standardize";
                    log.Name = service.Value.Metadata.ServiceName;
                    log.Input = inputAddress;
                    log.Result = result;
                    log.Success = success;
                    logService.Add( log, personId );
                    logService.Save( log, personId );

                    // If succesful, set the results and stop processing
                    if ( success )
                    {
                        address.StandardizeService = service.Value.Metadata.ServiceName;
                        address.StandardizeResult = result;
                        address.StandardizeDate = DateTime.Now;
                        break;
                    }
                }

            address.StandardizeAttempt = DateTime.Now;
        }

        /// <summary>
        /// Geocodes the specified <see cref="AddressDTO"/>
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public Rock.Models.Crm.Address Geocode( Rock.DataTransferObjects.Crm.Address address, int? personId )
        {
            Rock.Models.Crm.Address addressModel = GetByAddressDTO( address, personId );

            Geocode( addressModel, personId );

            return addressModel;
        }

        /// <summary>
        /// Geocodes the specified <see cref="Rock.Models.Crm.Address"/>
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="personId">The person id.</param>
        public void Geocode( Rock.Models.Crm.Address address, int? personId )
        {
            Core.ServiceLogService logService = new Core.ServiceLogService();
            string inputAddress = address.ToString();

            // Try each of the geocoding services that were found through MEF
            foreach ( KeyValuePair<int, Lazy<Rock.Address.GeocodeService, Rock.Address.IGeocodeServiceData>> service in Rock.Address.GeocodeContainer.Instance.Services )
                if ( !service.Value.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.Value.AttributeValues["Active"].Value ) )
                {
                    string result;
                    bool success = service.Value.Value.Geocode( address, out result );

                    // Log the results of the service
                    Models.Core.ServiceLog log = new Models.Core.ServiceLog();
                    log.Time = DateTime.Now;
                    log.Type = "Address Geocode";
                    log.Name = service.Value.Metadata.ServiceName;
                    log.Input = inputAddress;
                    log.Result = result;
                    log.Success = success;
                    logService.Add( log, personId );
                    logService.Save( log, personId );

                    // If succesful, set the results and stop processing
                    if ( success  )
                    {
                        address.GeocodeService = service.Value.Metadata.ServiceName;
                        address.GeocodeResult = result;
                        address.GeocodeDate = DateTime.Now;
                        break;
                    }
                }

            address.GeocodeAttempt = DateTime.Now;
        }

        /// <summary>
        /// Looks for an existing address model first by searching for a raw value, and then by the street, 
        /// city, state, and zip of the specified address stub.  If a match is not found, then a new address
        /// block is returned.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        private Rock.Models.Crm.Address GetByAddressDTO( Rock.DataTransferObjects.Crm.Address address, int? personId )
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
            }

            return addressModel;
        }
		

	}
}