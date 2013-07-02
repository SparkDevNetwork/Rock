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
    public partial class LocationService 
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
        /// Gets Location by Street 1 And Street 2 And City And State And Zip, will first standardize the address.  If an existing location 
        /// is not found, a new locaiton will be created and geocoded.
        /// </summary>
        /// <param name="street1">Street 1.</param>
        /// <param name="street2">Street 2.</param>
        /// <param name="city">City.</param>
        /// <param name="state">State.</param>
        /// <param name="zip">Zip.</param>
        /// <returns>Location object.</returns>
        public Location Get( string street1, string street2, string city, string state, string zip )
        {
            // First check if a location exists with the entered values
            Location existingLoction = Repository.FirstOrDefault( t =>
                ( t.Street1 == street1 || ( street1 == null && t.Street1 == null ) ) &&
                ( t.Street2 == street2 || ( street2 == null && t.Street2 == null ) ) &&
                ( t.City == city || ( city == null && t.City == null ) ) &&
                ( t.State == state || ( state == null && t.State == null ) ) &&
                ( t.Zip == zip || ( zip == null && t.Zip == null ) ) );
            if ( existingLoction != null )
            {
                return existingLoction;
            }

            // If existing location wasn't found with entered values, try standardizing the values, and 
            // search for an existing value again
            var newLocation = new Location
            {
                Street1 = street1,
                Street2 = street2,
                City = city,
                State = state,
                Zip = zip
            };

            Standardize( newLocation, null );

            existingLoction = Repository.FirstOrDefault( t =>
                ( t.Street1 == newLocation.Street1 || ( newLocation.Street1 == null && t.Street1 == null ) ) &&
                ( t.Street2 == newLocation.Street2 || ( newLocation.Street2 == null && t.Street2 == null ) ) &&
                ( t.City == newLocation.City || ( newLocation.City == null && t.City == null ) ) &&
                ( t.State == newLocation.State || ( newLocation.State == null && t.State == null ) ) &&
                ( t.Zip == newLocation.Zip || ( newLocation.Zip == null && t.Zip == null ) ) );

            if ( existingLoction != null )
            {
                return existingLoction;
            }

            // If still no existing location, geocode the new location and save it.
            Geocode( newLocation, null );

            Save( newLocation, null );

            return newLocation;
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
                    log.LogDateTime = DateTime.Now;
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
                    log.LogDateTime = DateTime.Now;
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

    }
}
