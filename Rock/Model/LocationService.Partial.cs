// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.Location"/> entities.
    /// </summary>
    public partial class LocationService
    {
        /// <summary>
        /// Returns the first
        /// <see cref="Rock.Model.Location" /> where the address matches the provided address.  If no address is found with the provided values,
        /// the address will be standardized. If there is still not a match, the address will be saved as a new location.
        /// </summary>
        /// <param name="street1">A <see cref="System.String" /> representing the Address Line 1 to search by.</param>
        /// <param name="street2">A <see cref="System.String" /> representing the Address Line 2 to search by.</param>
        /// <param name="city">A <see cref="System.String" /> representing the City to search by.</param>
        /// <param name="state">A <see cref="System.String" /> representing the State to search by.</param>
        /// <param name="postalCode">A <see cref="System.String" /> representing the Zip/Postal code to search by</param>
        /// <param name="country">The country.</param>
        /// <param name="verifyLocation">if set to <c>true</c> [verify location].</param>
        /// <returns>
        /// The first <see cref="Rock.Model.Location" /> where an address match is found, if no match is found a new <see cref="Rock.Model.Location" /> is created and returned.
        /// </returns>
        public Location Get( string street1, string street2, string city, string state, string postalCode, string country, bool verifyLocation = true )
        {
            // Make sure it's not an empty address
            if ( string.IsNullOrWhiteSpace( street1 ) )
            {
                return null;
            }

            // First check if a location exists with the entered values
            Location existingLocation = Queryable().FirstOrDefault( t =>
                ( t.Street1 == street1 || ( ( street1 == null || street1 == "" ) && ( t.Street1 == null || t.Street1 == "" ) ) ) &&
                ( t.Street2 == street2 || ( ( street2 == null || street2 == "" ) && ( t.Street2 == null || t.Street2 == "" ) ) ) &&
                ( t.City == city || ( ( city == null || city == "" ) && ( t.City == null || t.City == "" ) ) ) &&
                ( t.State == state || ( ( state == null || state == "" ) && ( t.State == null || t.State == "" ) ) ) &&
                ( t.PostalCode == postalCode || ( ( postalCode == null || postalCode == "" ) && ( t.PostalCode == null || t.PostalCode == "" ) ) ) &&
                ( t.Country == country || ( ( country == null || country == "" ) && ( t.Country == null || t.Country == "" ) ) ) );
            if ( existingLocation != null )
            {
                return existingLocation;
            }

            // If existing location wasn't found with entered values, try standardizing the values, and
            // search for an existing value again
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

            existingLocation = Queryable().FirstOrDefault( t =>
                ( t.Street1 == newLocation.Street1 || ( ( newLocation.Street1 == null || newLocation.Street1 == "" ) && ( t.Street1 == null || t.Street1 == "" ) ) ) &&
                ( t.Street2 == newLocation.Street2 || ( ( newLocation.Street2 == null || newLocation.Street2 == "" ) && ( t.Street2 == null || t.Street2 == "" ) ) ) &&
                ( t.City == newLocation.City || ( ( newLocation.City == null || newLocation.City == "" ) && ( t.City == null || t.City == "" ) ) ) &&
                ( t.State == newLocation.State || ( ( newLocation.State == null || newLocation.State == "" ) && ( t.State == null || t.State == "" ) ) ) &&
                ( t.PostalCode == newLocation.PostalCode || ( ( newLocation.PostalCode == null || newLocation.PostalCode == "" ) && ( t.PostalCode == null || t.PostalCode == "" ) ) ) &&
                ( t.Country == newLocation.Country || ( ( newLocation.Country == null || newLocation.Country == "" ) && ( t.Country == null || t.Country == "" ) ) ) );

            if ( existingLocation != null )
            {
                return existingLocation;
            }

            // Create a new context/service so that save does not affect calling method's context
            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );
            locationService.Add( newLocation );
            rockContext.SaveChanges();

            // refetch it from the database to make sure we get a valid .Id
            return Get( newLocation.Guid );
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

                    // If location has been succesfully standardized and geocoded, break to get out, otherwise next service will be attempted
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
        /// Returns an enumerable collection of <see cref="Rock.Model.Location">Locations</see> that are descendants of a <see cref="Rock.Model.Location"/>
        /// </summary>
        /// <param name="parentLocationId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/></param>
        /// <returns>A collection of <see cref="Rock.Model.Location"/> entities that are descendants of the provided parent <see cref="Rock.Model.Location"/>.</returns>
        public IEnumerable<Location> GetAllDescendents( int parentLocationId )
        {
            return ExecuteQuery( string.Format(
                @"
                WITH CTE AS (
                    SELECT * FROM [Location] WHERE [ParentLocationId]={0}
                    UNION ALL
                    SELECT [a].* FROM [Location] [a]
                    INNER JOIN  CTE pcte ON pcte.Id = [a].[ParentLocationId]
                )
                SELECT * FROM CTE
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
                    SELECT [a].[Id], [a].[ParentLocationId], [Name] FROM [Location] [a]
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
        public int? GetCampusIdForLocation( int? locationId)
        {
            if ( !locationId.HasValue )
            {
                return null;
            }

            // If location is not a campus, check the location's parent locations to see if any of them are a campus
            var location = this.Get( locationId.Value );
            int? campusId = location.CampusId;
            if ( !campusId.HasValue )
            {
                var campusLocations = new Dictionary<int, int>();
                Rock.Web.Cache.CampusCache.All()
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
                        break;
                    }
                }
            }

            return campusId;
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
        /// Gets the locations associated to a device and optionally any child locaitons
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="includeChildLocations">if set to <c>true</c> [include child locations].</param>
        /// <returns></returns>
        public IEnumerable<Location> GetByDevice( int deviceId, bool includeChildLocations = true )
        {
            string childQuery = includeChildLocations ? @"
                    UNION ALL
                    SELECT [a].*
                        FROM [Location] [a]
                            INNER JOIN  CTE pcte ON pcte.Id = [a].[ParentLocationId]
" : "";

            return ExecuteQuery( string.Format(
                @"
            WITH CTE AS (
                SELECT L.*
                    FROM [DeviceLocation] D
                        INNER JOIN [Location] L ON L.[Id] = D.[LocationId]
                WHERE D.[DeviceId] = {0}
                {1}
            )

            SELECT * FROM CTE
            ", deviceId, childQuery ) );
        }
    }
}