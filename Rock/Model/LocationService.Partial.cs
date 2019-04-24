// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.Location"/> entities.
    /// </summary>
    public partial class LocationService
    {
        /// <summary>
        /// Returns the first <see cref="Rock.Model.Location" /> where the address matches the provided address, otherwise the address will be saved as a new location.
        /// Note: if <paramref name="street1"/> is blank, null will be returned.
        /// </summary>
        /// <param name="street1">A <see cref="string" /> representing the Address Line 1 to search by.</param>
        /// <param name="street2">A <see cref="string" /> representing the Address Line 2 to search by.</param>
        /// <param name="city">A <see cref="string" /> representing the City to search by.</param>
        /// <param name="state">A <see cref="string" /> representing the State to search by.</param>
        /// <param name="postalCode">A <see cref="string" /> representing the Zip/Postal code to search by</param>
        /// <param name="country">A <see cref="string" /> representing the Country to search by</param>
        /// <param name="verifyLocation">if set to <c>true</c> [verify location].</param>
        /// <returns>
        /// The first <see cref="Rock.Model.Location" /> where an address match is found, if no match is found a new <see cref="Rock.Model.Location" /> is created and returned.
        /// </returns>
        public Location Get( string street1, string street2, string city, string state, string postalCode, string country, bool verifyLocation = true )
        {
            return Get( street1, street2, city, state, postalCode, country, null, verifyLocation );
        }

        /// <summary>
        /// Returns the first <see cref="Rock.Model.Location" /> where the address matches the provided address, otherwise the address will be saved as a new location.
        /// Note: if <paramref name="street1"/> is blank, null will be returned.
        /// </summary>
        /// <param name="street1">A <see cref="string" /> representing the Address Line 1 to search by.</param>
        /// <param name="street2">A <see cref="string" /> representing the Address Line 2 to search by.</param>
        /// <param name="city">A <see cref="string" /> representing the City to search by.</param>
        /// <param name="state">A <see cref="string" /> representing the State to search by.</param>
        /// <param name="postalCode">A <see cref="string" /> representing the Zip/Postal code to search by</param>
        /// <param name="country">A <see cref="string" /> representing the Country to search by</param>
        /// <param name="group">The <see cref="Group"/> (usually a Family) that should be searched first</param>
        /// <param name="verifyLocation">if set to <c>true</c> [verify location].</param>
        /// <param name="createNewLocation">if set to <c>true</c> a new location will be created if it does not exists.</param>
        /// <returns>
        /// The first <see cref="Rock.Model.Location" /> where an address match is found, if no match is found a new <see cref="Rock.Model.Location" /> is created and returned.
        /// </returns>
        public Location Get( string street1, string street2, string city, string state, string postalCode, string country, Group group, bool verifyLocation = true, bool createNewLocation = true )
        {
            // Make sure it's not an empty address
            if ( string.IsNullOrWhiteSpace( street1 ) )
            {
                return null;
            }

            // Try to find a location that matches the values, this is not a case sensitive match
            var foundLocation = Search( new Location { Street1 = street1, Street2 = street2, City = city, State = state, PostalCode = postalCode, Country = country }, group );
            if ( foundLocation != null )
            {
                // Check for casing 
                if ( !String.Equals( street1, foundLocation.Street1 ) || !String.Equals( street2, foundLocation.Street2 ) || !String.Equals( city, foundLocation.City ) || !String.Equals( state, foundLocation.State ) || !String.Equals( postalCode, foundLocation.PostalCode ) || !String.Equals( country, foundLocation.Country ) )
                {
                    var context = new RockContext();
                    var location = new LocationService( context ).Get( foundLocation.Id );
                    location.Street1 = street1;
                    location.Street2 = street2;
                    location.City = city;
                    location.State = state;
                    location.PostalCode = postalCode;
                    location.Country = country;
                    context.SaveChanges();
                    return Get( location.Guid );
                }
                return foundLocation;
            }

            // If existing location wasn't found with entered values, try standardizing the values, and search for an existing value again
            var newLocation = new Location
            {
                Street1 = street1.FixCase(),
                Street2 = street2.FixCase(),
                City = city.FixCase(),
                State = state,
                PostalCode = postalCode,
                Country = country
            };

            if ( verifyLocation )
            {
                Verify( newLocation, false );
            }

            foundLocation = Search( newLocation, group );
            if ( foundLocation != null )
            {
                return foundLocation;
            }

            if ( createNewLocation )
            {
                // Create a new context/service so that save does not affect calling method's context
                var rockContext = new RockContext();
                var locationService = new LocationService( rockContext );
                locationService.Add( newLocation );
                rockContext.SaveChanges();
            }

            // refetch it from the database to make sure we get a valid .Id
            return Get( newLocation.Guid );
        }

        /// <summary>
        /// Searches a group's locations for a match, if one is not found then searches all locations for a match.
        /// </summary>
        /// <param name="location">The <see cref="Location"/> to search for</param>
        /// <param name="group">Search this <see cref="Group"/> first</param>
        /// <returns>The first <see cref="Rock.Model.Location" /> where an address match is found</returns>
        /// <remarks>Keep this private as it is part of the public Get method</remarks>
        private Location Search( Location location, Group group )
        {
            Location existingLocation;

            // If the location's target group is known, first attempt to find this location on that group using the values as they were entered
            if ( group != null && group.GroupLocations.Any() )
            {
                existingLocation = group.GroupLocations.Select( x => x.Location ).FirstOrDefault( t =>
                    ( t.Street1 ?? "" ) == ( location.Street1 ?? "" ) &&
                    ( t.Street2 ?? "" ) == ( location.Street2 ?? "" ) &&
                    ( t.City ?? "" ) == ( location.City ?? "" ) &&
                    ( t.State ?? "" ) == ( location.State ?? "" ) &&
                    ( t.PostalCode ?? "" ) == ( location.PostalCode ?? "" ) &&
                    ( t.Country ?? "" ) == ( location.Country ?? "" ) );

                if ( existingLocation != null )
                {
                    return existingLocation;
                }
            }

            // If this location was not found on the specified group, or no group was specified, search for any instance of this location
            existingLocation = Queryable().FirstOrDefault( t =>
                ( t.Street1 ?? "" ) == ( location.Street1 ?? "" ) &&
                ( t.Street2 ?? "" ) == ( location.Street2 ?? "" ) &&
                ( t.City ?? "" ) == ( location.City ?? "" ) &&
                ( t.State ?? "" ) == ( location.State ?? "" ) &&
                ( t.PostalCode ?? "" ) == ( location.PostalCode ?? "" ) &&
                ( t.Country ?? "" ) == ( location.Country ?? "" ) );

            if ( existingLocation != null )
            {
                return existingLocation;
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Location"/> by GeoPoint. If a match is not found,
        /// a new Location will be added based on the Geopoint.
        /// </summary>
        /// <param name="point">A <see cref="System.Data.Entity.Spatial.DbGeography"/> object
        ///     representing the Geopoint for the location.</param>
        /// <returns>The first <see cref="Rock.Model.Location"/> that matches the specified GeoPoint.</returns>
        public Location GetByGeoPoint( DbGeography point )
        {
            // get the first address that has a GeoPoint the value
            // use the 'Where Max(ID)' trick instead of TOP 1 to optimize SQL performance
            var qryWhere = Queryable()
                .Where( a =>
                    a.GeoPoint != null &&
                    a.GeoPoint.SpatialEquals( point ) );

            var result = Queryable().Where( a => a.Id == qryWhere.Max( b => b.Id ) ).FirstOrDefault();

            if ( result == null )
            {
                // if the Location can't be found, save the new location to the database
                Location newLocation = new Location
                {
                    GeoPoint = point,
                    Guid = Guid.NewGuid()
                };

                // Create a new context/service so that save does not affect calling method's context
                var rockContext = new RockContext();
                var locationService = new LocationService( rockContext );
                locationService.Add( newLocation );
                rockContext.SaveChanges();

                // refetch it from the database to make sure we get a valid .Id
                return Get( newLocation.Guid );
            }

            return result;
        }

        /// <summary>
        /// Returns the first <see cref="Rock.Model.Location"/> with a GeoFence that matches
        /// the specified GeoFence.
        /// </summary>
        /// <param name="fence">A <see cref="System.Data.Entity.Spatial.DbGeography"/> object that
        ///  represents the GeoFence of the location to retrieve.</param>
        /// <returns>The <see cref="Rock.Model.Location"/> for the specified GeoFence. </returns>
        public Location GetByGeoFence( DbGeography fence )
        {
            // get the first address that has the GeoFence value
            // use the 'Where Max(ID)' trick instead of TOP 1 to optimize SQL performance
            var qryWhere = Queryable()
                .Where( a =>
                    a.GeoFence != null &&
                    a.GeoFence.SpatialEquals( fence ) );

            var result = Queryable().Where( a => a.Id == qryWhere.Max( b => b.Id ) ).FirstOrDefault();

            if ( result == null )
            {
                // if the Location can't be found, save the new location to the database
                Location newLocation = new Location
                {
                    GeoFence = fence,
                    Guid = Guid.NewGuid()
                };

                // Create a new context/service so that save does not affect calling method's context
                var rockContext = new RockContext();
                var locationService = new LocationService( rockContext );
                locationService.Add( newLocation );
                rockContext.SaveChanges();

                // refetch it from the database to make sure we get a valid .Id
                return Get( newLocation.Guid );
            }

            return result;
        }

        /// <summary>
        /// Performs Address Verification on the provided <see cref="Rock.Model.Location" />.
        /// </summary>
        /// <param name="location">A <see cref="Rock.Model.Location" /> to verify.</param>
        /// <param name="reVerify">if set to <c>true</c> [re verify].</param>
        public bool Verify( Location location, bool reVerify )
        {
            bool success = false;

            // Do not reverify any locked locations
            if ( location == null || ( location.IsGeoPointLocked.HasValue && location.IsGeoPointLocked.Value ) )
            {
                return false;
            }

            string inputLocation = location.ToString();

            // Create new context to save service log without affecting calling method's context
            var rockContext = new RockContext();
            Model.ServiceLogService logService = new Model.ServiceLogService( rockContext );

            bool standardized = location.StandardizeAttemptedDateTime.HasValue && !reVerify;
            bool geocoded = location.GeocodeAttemptedDateTime.HasValue && !reVerify;
            bool anyActiveStandardizationService = false;
            bool anyActiveGeocodingService = false;

            // Save current values for situation when first service may successfully standardize or geocode, but not both
            // In this scenario the first service's values should be preserved
            string street1 = location.Street1;
            string street2 = location.Street2;
            string city = location.City;
            string county = location.County;
            string state = location.State;
            string country = location.Country;
            string postalCode = location.PostalCode;
            string barcode = location.Barcode;
            DbGeography geoPoint = location.GeoPoint;

            // Try each of the verification services that were found through MEF
            foreach ( var service in Rock.Address.VerificationContainer.Instance.Components )
            {
                var component = service.Value.Value;
                if ( component != null &&
                    component.IsActive && (
                    ( !standardized && component.SupportsStandardization ) ||
                    ( !geocoded && component.SupportsGeocoding ) ) )
                {
                    string resultMsg = string.Empty;
                    var result = component.Verify( location, out resultMsg );

                    if ( !standardized && component.SupportsStandardization )
                    {
                        anyActiveStandardizationService = true;

                        // Log the service and result
                        location.StandardizeAttemptedServiceType = service.Value.Metadata.ComponentName;
                        location.StandardizeAttemptedResult = resultMsg;

                        // As long as there wasn't a connection error, update the attempted datetime
                        if ( ( result & Address.VerificationResult.ConnectionError ) != Address.VerificationResult.ConnectionError )
                        {
                            location.StandardizeAttemptedDateTime = RockDateTime.Now;
                        }

                        // If location was successfully geocoded, update the timestamp
                        if ( ( result & Address.VerificationResult.Standardized ) == Address.VerificationResult.Standardized )
                        {
                            location.StandardizedDateTime = RockDateTime.Now;
                            standardized = true;

                            // Save standardized address in case another service is called for geocoding
                            street1 = location.Street1;
                            street2 = location.Street2;
                            city = location.City;
                            county = location.County;
                            state = location.State;
                            country = location.Country;
                            postalCode = location.PostalCode;
                            barcode = location.Barcode;
                        }
                    }
                    else
                    {
                        // Reset the address back to what it was originally or after previous service successfully standardized it
                        location.Street1 = street1;
                        location.Street2 = street2;
                        location.City = city;
                        location.County = county;
                        location.State = state;
                        location.Country = country;
                        location.PostalCode = postalCode;
                        location.Barcode = barcode;
                    }

                    if ( !geocoded && component.SupportsGeocoding )
                    {
                        anyActiveGeocodingService = true;

                        // Log the service and result
                        location.GeocodeAttemptedServiceType = service.Value.Metadata.ComponentName;
                        location.GeocodeAttemptedResult = resultMsg;

                        // As long as there wasn't a connection error, update the attempted datetime
                        if ( ( result & Address.VerificationResult.ConnectionError ) != Address.VerificationResult.ConnectionError )
                        {
                            location.GeocodeAttemptedDateTime = RockDateTime.Now;
                        }

                        // If location was successfully geocoded, update the timestamp
                        if ( ( result & Address.VerificationResult.Geocoded ) == Address.VerificationResult.Geocoded )
                        {
                            location.GeocodedDateTime = RockDateTime.Now;
                            geocoded = true;

                            // Save the lat/long in case another service is called for standardization
                            geoPoint = location.GeoPoint;
                        }
                    }
                    else
                    {
                        // Reset the lat/long back to what it was originally or after previous service successfully geocoded it
                        location.GeoPoint = geoPoint;
                    }

                    // Log the results of the service
                    if ( !string.IsNullOrWhiteSpace( resultMsg ) )
                    {
                        Model.ServiceLog log = new Model.ServiceLog();
                        log.LogDateTime = RockDateTime.Now;
                        log.Type = "Location Verify";
                        log.Name = service.Value.Metadata.ComponentName;
                        log.Input = inputLocation;
                        log.Result = resultMsg.Left( 200 );
                        log.Success = success;
                        logService.Add( log );
                    }

                    // If location has been successfully standardized and geocoded, break to get out, otherwise next service will be attempted
                    if ( standardized && geocoded )
                    {
                        break;
                    }
                }
            }

            // If there is only one type of active service (standardization/geocoding) the other type's attempted datetime
            // needs to be updated so that the verification job will continue to process additional locations vs just getting
            // stuck on the first batch and doing them over and over again because the other service type's attempted date is
            // never updated.
            if ( anyActiveStandardizationService && !anyActiveGeocodingService )
            {
                location.GeocodeAttemptedDateTime = RockDateTime.Now;
            }
            if ( anyActiveGeocodingService && !anyActiveStandardizationService )
            {
                location.StandardizeAttemptedDateTime = RockDateTime.Now;
            }

            rockContext.SaveChanges();

            return standardized || geocoded;
        }

        /// <summary>
        /// Gets the mapCoordinate from postalcode.
        /// </summary>
        /// <param name="postalCode">The postalcode.</param>
        /// <returns></returns>
        public MapCoordinate GetMapCoordinateFromPostalCode( string postalCode )
        {
            Address.SmartyStreets smartyStreets = new Address.SmartyStreets();
            string resultMsg = string.Empty;
            var coordinate =  smartyStreets.GetLocationFromPostalCode( postalCode, out resultMsg );
            // Log the results of the service
            if ( !string.IsNullOrWhiteSpace( resultMsg ) )
            {
                var rockContext = new RockContext();
                Model.ServiceLogService logService = new Model.ServiceLogService( rockContext );
                Model.ServiceLog log = new Model.ServiceLog();
                log.LogDateTime = RockDateTime.Now;
                log.Type = "Mapcoordinate from postalcode";
                log.Name = smartyStreets.TypeName;
                log.Input = postalCode;
                log.Result = resultMsg.Left( 200 );
                log.Success = coordinate != null;
                logService.Add( log );
                rockContext.SaveChanges();
            }
            return coordinate;
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Location">Locations</see> that are descendants of a <see cref="Rock.Model.Location"/>
        /// </summary>
        /// <param name="parentLocationId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/></param>
        /// <returns>A collection of <see cref="Rock.Model.Location"/> entities that are descendants of the provided parent <see cref="Rock.Model.Location"/>.</returns>
        public IEnumerable<Location> GetAllDescendents( int parentLocationId )
        {
            return ExecuteQuery( string.Format(
                @"
                WITH CTE AS (
                    SELECT Id FROM [Location] WHERE [ParentLocationId]={0}
                    UNION ALL
                    SELECT [a].Id FROM [Location] [a]
                    INNER JOIN CTE pcte ON pcte.Id = [a].[ParentLocationId]
                )
                SELECT L.* FROM CTE
                INNER JOIN [Location] L ON L.[Id] = CTE.[Id]
                ", parentLocationId ) );
        }

        /// <summary>
        /// Gets all descendent ids.
        /// </summary>
        /// <param name="parentLocationId">The parent location identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> GetAllDescendentIds( int parentLocationId )
        {
            return this.Context.Database.SqlQuery<int>( string.Format(
                @"
                WITH CTE AS (
                    SELECT [Id], [ParentLocationId] FROM [Location] WHERE [ParentLocationId]={0}
                    UNION ALL
                    SELECT [a].[Id], [a].[ParentLocationId] FROM [Location] [a]
                    INNER JOIN  CTE pcte ON pcte.[Id] = [a].[ParentLocationId]
                )
                SELECT [Id] FROM CTE
                ", parentLocationId ) );
        }

        /// <summary>
        /// Gets all ancestors.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public IEnumerable<Location> GetAllAncestors( int locationId )
        {
            return ExecuteQuery( string.Format(
                @"
                WITH CTE AS (
                    SELECT * FROM [Location] WHERE [Id]={0}
                    UNION ALL
                    SELECT [a].* FROM [Location] [a]
                    INNER JOIN CTE ON CTE.[ParentLocationId] = [a].[Id]
                )
                SELECT * FROM CTE
                WHERE [Name] IS NOT NULL
                AND [Name] <> ''
                ", locationId ) );
        }

        /// <summary>
        /// Gets all ancestor ids.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> GetAllAncestorIds( int locationId )
        {
            return this.Context.Database.SqlQuery<int>( string.Format(
                @"
                WITH CTE AS (
                    SELECT [Id], [ParentLocationId], [Name] FROM [Location] WHERE [Id]={0}
                    UNION ALL
                    SELECT [a].[Id], [a].[ParentLocationId], [a].[Name] FROM [Location] [a]
                    INNER JOIN CTE ON CTE.[ParentLocationId] = [a].[Id]
                )
                SELECT [Id]
                FROM CTE
                WHERE [Name] IS NOT NULL
                AND [Name] <> ''
                ", locationId ) );
        }

        /// <summary>
        /// Gets the CampusID associated with the Location from the location or from the location's parent path
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public int? GetCampusIdForLocation( int? locationId )
        {
            if ( !locationId.HasValue )
            {
                return null;
            }

            var location = this.Get( locationId.Value );
            int? campusId = location.CampusId;
            if ( campusId.HasValue )
            {
                return campusId;
            }

            // If location is not a campus, check the location's parent locations to see if any of them are a campus
            var campusLocations = new Dictionary<int, int>();
            CampusCache.All()
                .Where( c => c.LocationId.HasValue )
                .Select( c => new
                {
                    CampusId = c.Id,
                    LocationId = c.LocationId.Value
                } )
                .ToList()
                .ForEach( c => campusLocations.Add( c.CampusId, c.LocationId ) );

            foreach ( var parentLocationId in this.GetAllAncestorIds( locationId.Value ) )
            {
                campusId = campusLocations
                    .Where( c => c.Value == parentLocationId )
                    .Select( c => c.Key )
                    .FirstOrDefault();

                if ( campusId != 0 )
                {
                    return campusId;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public string GetPath( int locationId )
        {
            var locations = GetAllAncestors( locationId );
            if ( locations.Any() )
            {
                var locationNames = locations.Select( l => l.Name ).ToList();
                locationNames.Reverse();
                return locationNames.AsDelimited( " > " );
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the locations associated to a device and optionally any child locations
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="includeChildLocations">if set to <c>true</c> [include child locations].</param>
        /// <returns></returns>
        public IEnumerable<Location> GetByDevice( int deviceId, bool includeChildLocations = true )
        {
            string childQuery = includeChildLocations ? @"

        UNION ALL

        SELECT [a].Id
	FROM [Location] [a]
        INNER JOIN  CTE pcte ON pcte.Id = [a].[ParentLocationId]
        WHERE [a].[ParentLocationId] IS NOT NULL
" : "";

            return ExecuteQuery( string.Format(
                @"
    WITH CTE AS (
        SELECT L.Id
        FROM [DeviceLocation] D
        INNER JOIN [Location] L ON L.[Id] = D.[LocationId]
        WHERE D.[DeviceId] = {0}
{1}
    )

    SELECT L.* FROM CTE
    INNER JOIN [Location] L ON L.[Id] = CTE.[Id]
            ", deviceId, childQuery ) );
        }
    }
}
