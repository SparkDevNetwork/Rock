//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Location POCO Service class
    /// </summary>
    public partial class LocationService : Service<Location, LocationDto>
    {
        /// <summary>
        /// Gets Location by Full Address
        /// </summary>
        /// <param name="fullAddress">Full Address.</param>
        /// <returns>
        /// Location object.
        /// </returns>
        public Location GetByFullAddress( string fullAddress )
        {
            return Repository.FirstOrDefault( t => ( t.FullAddress == fullAddress || ( fullAddress == null && t.FullAddress == null ) ) );
        }
        
        /// <summary>
        /// Gets Location by Street 1 And Street 2 And City And State And Zip
        /// </summary>
        /// <param name="street1">Street 1.</param>
        /// <param name="street2">Street 2.</param>
        /// <param name="city">City.</param>
        /// <param name="state">State.</param>
        /// <param name="zip">Zip.</param>
        /// <returns>Location object.</returns>
        public Location GetByStreet1AndStreet2AndCityAndStateAndZip( string street1, string street2, string city, string state, string zip )
        {
            return Repository.FirstOrDefault( t => ( t.Street1 == street1 || ( street1 == null && t.Street1 == null ) ) && ( t.Street2 == street2 || ( street2 == null && t.Street2 == null ) ) && ( t.City == city || ( city == null && t.City == null ) ) && ( t.State == state || ( state == null && t.State == null ) ) && ( t.Zip == zip || ( zip == null && t.Zip == null ) ) );
        }

        /// <summary>
        /// Standardizes the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public Location Standardize(LocationDto location, int? personId)
        {
            Location locationModel = GetByLocationDto(location, personId);

            Standardize( locationModel, personId );

            return locationModel;
        }

        /// <summary>
        /// Standardizes the specified <see cref="Location"/>
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="personId">The person id.</param>
        public void Standardize( Location location, int? personId )
        {
            Model.ServiceLogService logService = new Model.ServiceLogService();
            string inputLocation = location.ToString();

            // Try each of the standardization services that were found through MEF
            foreach ( KeyValuePair<int, Lazy<Rock.Address.StandardizeComponent, Rock.Extension.IComponentData>> service in Rock.Address.StandardizeContainer.Instance.Components )
                if ( !service.Value.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.Value.AttributeValues["Active"][0].Value ) )
                {
                    string result;
                    bool success = service.Value.Value.Standardize( location, out result );

                    // Log the results of the service
                    Model.ServiceLog log = new Model.ServiceLog();
                    log.Time = DateTime.Now;
                    log.Type = "Location Standardize";
                    log.Name = service.Value.Metadata.ComponentName;
                    log.Input = inputLocation;
                    log.Result = result;
                    log.Success = success;
                    logService.Add( log, personId );
                    logService.Save( log, personId );

                    // If succesful, set the results and stop processing
                    if ( success )
                    {
                        location.StandardizeAttemptedServiceType = service.Value.Metadata.ComponentName;
                        location.StandardizeAttemptedResult = result;
                        location.StandardizedDateTime = DateTime.Now;
                        break;
                    }
                }

            location.StandardizeAttemptedDateTime = DateTime.Now;
        }

        /// <summary>
        /// Geocodes the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public Location Geocode(LocationDto location, int? personId)
        {
            Location locationModel = GetByLocationDto(location, personId);

            Geocode( locationModel, personId );

            return locationModel;
        }

        /// <summary>
        /// Geocodes the specified <see cref="Location"/>
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="personId">The person id.</param>
        public void Geocode( Location location, int? personId )
        {
            Model.ServiceLogService logService = new Model.ServiceLogService();
            string inputLocation = location.ToString();

            // Try each of the geocoding services that were found through MEF

            foreach ( KeyValuePair<int, Lazy<Rock.Address.GeocodeComponent, Rock.Extension.IComponentData>> service in Rock.Address.GeocodeContainer.Instance.Components )
                if ( !service.Value.Value.AttributeValues.ContainsKey( "Active" ) || bool.Parse( service.Value.Value.AttributeValues["Active"][0].Value ) )
                {
                    string result;
                    bool success = service.Value.Value.Geocode( location, out result );

                    // Log the results of the service
                    Model.ServiceLog log = new Model.ServiceLog();
                    log.Time = DateTime.Now;
                    log.Type = "Location Geocode";
                    log.Name = service.Value.Metadata.ComponentName;
                    log.Input = inputLocation;
                    log.Result = result;
                    log.Success = success;
                    logService.Add( log, personId );
                    logService.Save( log, personId );

                    // If succesful, set the results and stop processing
                    if ( success )
                    {
                        location.GeocodeAttemptedServiceType = service.Value.Metadata.ComponentName;
                        location.GeocodeAttemptedResult = result;
                        location.GeocodedDateTime = DateTime.Now;
                        break;
                    }
                }

            location.GeocodeAttemptedDateTime = DateTime.Now;
        }

        /// <summary>
        /// Looks for an existing location model first by searching for a raw value, and then by the street, 
        /// city, state, and zip of the specified location stub.  If a match is not found, then a new location
        /// block is returned.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        private Location GetByLocationDto(LocationDto location, int? personId)
        {
            string address = location.FullAddress;

            Location locationModel = GetByFullAddress( address );

            if ( locationModel == null )
                locationModel = GetByStreet1AndStreet2AndCityAndStateAndZip(
                    location.Street1, location.Street2, location.City, location.State, location.Zip );

            if ( locationModel == null )
            {
                locationModel = new Model.Location();
                locationModel.FullAddress = address;
                locationModel.Street1 = location.Street1;
                locationModel.Street2 = location.Street2;
                locationModel.City = location.City;
                locationModel.State = location.State;
                locationModel.Zip = location.Zip;
            }

            return locationModel;
        }
    }
}
