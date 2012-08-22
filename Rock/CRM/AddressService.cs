//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.CRM
{
	/// <summary>
	/// Address POCO Service class
	/// </summary>
    public partial class AddressService : Service<Address, AddressDTO>
    {
		/// <summary>
		/// Gets Address by Raw
		/// </summary>
		/// <param name="raw">Raw.</param>
		/// <returns>Address object.</returns>
	    public Address GetByRaw( string raw )
        {
            return Repository.FirstOrDefault( t => ( t.Raw == raw || ( raw == null && t.Raw == null ) ) );
        }
		
		/// <summary>
		/// Gets Address by Street 1 And Street 2 And City And State And Zip
		/// </summary>
		/// <param name="street1">Street 1.</param>
		/// <param name="street2">Street 2.</param>
		/// <param name="city">City.</param>
		/// <param name="state">State.</param>
		/// <param name="zip">Zip.</param>
		/// <returns>Address object.</returns>
	    public Address GetByStreet1AndStreet2AndCityAndStateAndZip( string street1, string street2, string city, string state, string zip )
        {
            return Repository.FirstOrDefault( t => ( t.Street1 == street1 || ( street1 == null && t.Street1 == null ) ) && ( t.Street2 == street2 || ( street2 == null && t.Street2 == null ) ) && ( t.City == city || ( city == null && t.City == null ) ) && ( t.State == state || ( state == null && t.State == null ) ) && ( t.Zip == zip || ( zip == null && t.Zip == null ) ) );
        }

        /// <summary>
        /// Standardizes the specified <see cref="Rock.CRM.DTO.Address"/>
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public Address Standardize( AddressDTO address, int? personId )
        {
            Address addressModel = GetByAddressDTO( address, personId );

            Standardize( addressModel, personId );

            return addressModel;
        }

        /// <summary>
        /// Standardizes the specified <see cref="Address"/>
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="personId">The person id.</param>
        public void Standardize( Address address, int? personId )
        {
            Core.ServiceLogService logService = new Core.ServiceLogService();
            string inputAddress = address.ToString();

            // Try each of the standardization services that were found through MEF
            foreach ( KeyValuePair<int, Lazy<Rock.Address.StandardizeComponent, Rock.Extension.IComponentData>> service in Rock.Address.StandardizeContainer.Instance.Components )
                if ( !service.Value.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.Value.AttributeValues["Active"].Value[0].Value ) )
                {
                    string result;
                    bool success = service.Value.Value.Standardize( address, out result );

                    // Log the results of the service
                    Core.ServiceLog log = new Core.ServiceLog();
                    log.Time = DateTime.Now;
                    log.Type = "Address Standardize";
                    log.Name = service.Value.Metadata.ComponentName;
                    log.Input = inputAddress;
                    log.Result = result;
                    log.Success = success;
                    logService.Add( log, personId );
                    logService.Save( log, personId );

                    // If succesful, set the results and stop processing
                    if ( success )
                    {
                        address.StandardizeService = service.Value.Metadata.ComponentName;
                        address.StandardizeResult = result;
                        address.StandardizeDate = DateTime.Now;
                        break;
                    }
                }

            address.StandardizeAttempt = DateTime.Now;
        }

        /// <summary>
        /// Geocodes the specified <see cref="Rock.CRM.DTO.Address"/>
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public Address Geocode( AddressDTO address, int? personId )
        {
            Address addressModel = GetByAddressDTO( address, personId );

            Geocode( addressModel, personId );

            return addressModel;
        }

        /// <summary>
        /// Geocodes the specified <see cref="Address"/>
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="personId">The person id.</param>
        public void Geocode( Address address, int? personId )
        {
            Core.ServiceLogService logService = new Core.ServiceLogService();
            string inputAddress = address.ToString();

            // Try each of the geocoding services that were found through MEF

            foreach ( KeyValuePair<int, Lazy<Rock.Address.GeocodeComponent, Rock.Extension.IComponentData>> service in Rock.Address.GeocodeContainer.Instance.Components )
                if ( !service.Value.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.Value.AttributeValues["Active"].Value[0].Value ) )
                {
                    string result;
                    bool success = service.Value.Value.Geocode( address, out result );

                    // Log the results of the service
                    Core.ServiceLog log = new Core.ServiceLog();
                    log.Time = DateTime.Now;
                    log.Type = "Address Geocode";
                    log.Name = service.Value.Metadata.ComponentName;
                    log.Input = inputAddress;
                    log.Result = result;
                    log.Success = success;
                    logService.Add( log, personId );
                    logService.Save( log, personId );

                    // If succesful, set the results and stop processing
                    if ( success )
                    {
                        address.GeocodeService = service.Value.Metadata.ComponentName;
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
        private Address GetByAddressDTO( AddressDTO address, int? personId )
        {
            string raw = address.Raw;

            Address addressModel = GetByRaw( raw );

            if ( addressModel == null )
                addressModel = GetByStreet1AndStreet2AndCityAndStateAndZip(
                    address.Street1, address.Street2, address.City, address.State, address.Zip );

            if ( addressModel == null )
            {
                addressModel = new CRM.Address();
                addressModel.Raw = raw;
                addressModel.Street1 = address.Street1;
                addressModel.Street2 = address.Street2;
                addressModel.City = address.City;
                addressModel.State = address.State;
                addressModel.Zip = address.Zip;
            }

            return addressModel;
        }

        /// <summary>
        /// Creates a new model
        /// </summary>
        /// <returns></returns>
        public override Address CreateNew()
        {
            return new Address();
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<AddressDTO> QueryableDTO()
        {
            return this.Queryable().Select( m => new AddressDTO( m ) );
        }
    }
}
