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
using Rock.Address;
using Rock.Data;
using Rock.Field.Types;
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
        /// Note: The location search IS NOT constrained by the provided group. Providing the group will cause this method to search that groups locations first, giving a faster result.
        /// </summary>
        /// <param name="street1">A <see cref="string" /> representing the Address Line 1 to search by.</param>
        /// <param name="street2">A <see cref="string" /> representing the Address Line 2 to search by.</param>
        /// <param name="city">A <see cref="string" /> representing the City to search by.</param>
        /// <param name="state">A <see cref="string" /> representing the State to search by.</param>
        /// <param name="postalCode">A <see cref="string" /> representing the Zip/Postal code to search by</param>
        /// <param name="country">A <see cref="string" /> representing the Country to search by</param>
        /// <param name="group">The <see cref="Group"/> (usually a Family) that should be searched first. This is NOT a search constraint.</param>
        /// <param name="verifyLocation">if set to <c>true</c> [verify location].</param>
        /// <param name="createNewLocation">if set to <c>true</c> a new location will be created if it does not exists.</param>
        /// <returns>
        /// The first <see cref="Rock.Model.Location" /> where an address match is found, if no match is found a new <see cref="Rock.Model.Location" /> is created and returned.
        /// </returns>
        public Location Get( string street1, string street2, string city, string state, string postalCode, string country, Group group, bool verifyLocation = true, bool createNewLocation = true )
        {
            return Get( street1, street2, city, state, null, postalCode, country, group, verifyLocation, createNewLocation );
        }

        /// <summary>
        /// Returns the first <see cref="Rock.Model.Location" /> where the address matches the provided address, otherwise the address will be saved as a new location.
        /// Note: The location search IS NOT constrained by the provided group. Providing the group will cause this method to search that groups locations first, giving a faster result.
        /// </summary>
        /// <param name="street1">A <see cref="string" /> representing the Address Line 1 to search by.</param>
        /// <param name="street2">A <see cref="string" /> representing the Address Line 2 to search by.</param>
        /// <param name="city">A <see cref="string" /> representing the City to search by.</param>
        /// <param name="state">A <see cref="string" /> representing the State/Province to search by.</param>
        /// <param name="locality">A <see cref="string" /> representing the Locality/County to search by</param>
        /// <param name="postalCode">A <see cref="string" /> representing the Zip/Postal code to search by</param>
        /// <param name="country">A <see cref="string" /> representing the Country to search by</param>
        /// <param name="group">The <see cref="Group"/> (usually a Family) that should be searched first. This is NOT a search constraint.</param>
        /// <param name="verifyLocation">if set to <c>true</c> [verify location].</param>
        /// <param name="createNewLocation">if set to <c>true</c> a new location will be created if it does not exists.</param>
        /// <returns>
        /// The first <see cref="Rock.Model.Location" /> where an address match is found, if no match is found a new <see cref="Rock.Model.Location" /> is created and returned.
        /// </returns>
        public Location Get( string street1, string street2, string city, string state, string locality, string postalCode, string country, Group group, bool verifyLocation = true, bool createNewLocation = true )
        {
            GetLocationArgs getLocationArgs = new GetLocationArgs
            {
                Group = group,
                VerifyLocation = verifyLocation,
                CreateNewLocation = createNewLocation,
            };

            return Get( street1, street2, city, state, locality, postalCode, country, getLocationArgs );
        }

        /// <summary>
        /// Returns the first <see cref="Rock.Model.Location" /> where the address matches the provided address, otherwise the address will be saved as a new location.
        /// Note: The location search IS NOT constrained by the provided <seealso cref="GetLocationArgs.Group"></seealso>. Providing the group will cause this method to search that groups locations first, giving a faster result.
        /// </summary>
        /// <param name="street1">A <see cref="string" /> representing the Address Line 1 to search by.</param>
        /// <param name="street2">A <see cref="string" /> representing the Address Line 2 to search by.</param>
        /// <param name="city">A <see cref="string" /> representing the City to search by.</param>
        /// <param name="state">A <see cref="string" /> representing the State/Province to search by.</param>
        /// <param name="postalCode">A <see cref="string" /> representing the Zip/Postal code to search by</param>
        /// <param name="country">A <see cref="string" /> representing the Country to search by</param>
        /// <param name="getLocationArgs">The <seealso cref="GetLocationArgs"/></param>
        /// <returns>
        /// The first <see cref="Rock.Model.Location" /> where an address match is found, if no match is found a new <see cref="Rock.Model.Location" /> is created and returned.
        /// </returns>
        /// <exception cref="System.Exception"></exception>
        public Location Get( string street1, string street2, string city, string state, string postalCode, string country, GetLocationArgs getLocationArgs )
        {
            string locality = null;
            return Get( street1, street2, city, state, locality, postalCode, country, getLocationArgs );
        }

        /// <summary>
        /// Returns the first <see cref="Rock.Model.Location" /> where the address matches the provided address, otherwise the address will be saved as a new location.
        /// Note: The location search IS NOT constrained by the provided <seealso cref="GetLocationArgs.Group"></seealso>. Providing the group will cause this method to search that groups locations first, giving a faster result.
        /// </summary>
        /// <param name="street1">A <see cref="string" /> representing the Address Line 1 to search by.</param>
        /// <param name="street2">A <see cref="string" /> representing the Address Line 2 to search by.</param>
        /// <param name="city">A <see cref="string" /> representing the City to search by.</param>
        /// <param name="state">A <see cref="string" /> representing the State/Province to search by.</param>
        /// <param name="locality">A <see cref="string" /> representing the Locality/County to search by</param>
        /// <param name="postalCode">A <see cref="string" /> representing the Zip/Postal code to search by</param>
        /// <param name="country">A <see cref="string" /> representing the Country to search by</param>
        /// <param name="getLocationArgs">The <seealso cref="GetLocationArgs"/></param>
        /// <returns>
        /// The first <see cref="Rock.Model.Location" /> where an address match is found, if no match is found a new <see cref="Rock.Model.Location" /> is created and returned.
        /// </returns>
        /// <exception cref="System.Exception"></exception>
        public Location Get( string street1, string street2, string city, string state, string locality, string postalCode, string country, GetLocationArgs getLocationArgs )
        {
            Group group = getLocationArgs?.Group;
            bool verifyLocation = getLocationArgs?.VerifyLocation ?? true;
            bool createNewLocation = getLocationArgs?.CreateNewLocation ?? true;
            var validateLocation = getLocationArgs?.ValidateLocation ?? true;

            // Remove leading and trailing whitespace.
            street1 = street1.ToStringSafe().Trim();
            street2 = street2.ToStringSafe().Trim();
            city = city.ToStringSafe().Trim();
            state = state.ToStringSafe().Trim();
            locality = locality.ToStringSafe().Trim();
            postalCode = postalCode.ToStringSafe().Trim();
            country = country.ToStringSafe().Trim();

            // Make sure the address has some content.
            if ( string.IsNullOrWhiteSpace( street1 )
                 && string.IsNullOrWhiteSpace( street2 )
                 && string.IsNullOrWhiteSpace( city )
                 && string.IsNullOrWhiteSpace( state )
                 && string.IsNullOrWhiteSpace( locality )
                 && string.IsNullOrWhiteSpace( postalCode ) )
            {
                return null;
            }

            // Try to find a location that matches the values, this is not a case sensitive match
            var foundLocation = Search( new Location { Street1 = street1, Street2 = street2, City = city, State = state, County = locality, PostalCode = postalCode, Country = country }, group );
            if ( foundLocation != null )
            {
                // Check for casing 
                if ( !string.Equals( street1, foundLocation.Street1 ) || !string.Equals( street2, foundLocation.Street2 ) || !string.Equals( city, foundLocation.City )
                    || !string.Equals( state, foundLocation.State ) || !string.Equals( postalCode, foundLocation.PostalCode ) || !string.Equals( country, foundLocation.Country ) )
                {
                    var context = new RockContext();
                    var location = new LocationService( context ).Get( foundLocation.Id );
                    location.Street1 = street1;
                    location.Street2 = street2;
                    location.City = city;
                    location.State = state;
                    location.County = locality;
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
                County = locality,
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
                if ( validateLocation )
                {
                    // Verify that the new location has all of the required fields.
                    string validationError;

                    var isValid = ValidateLocationAddressRequirements( newLocation, out validationError );

                    if ( !isValid )
                    {
                        throw new Exception( validationError );
                    }
                }

                // Create a new context/service so that save does not affect calling method's context
                var rockContext = new RockContext();
                var locationService = new LocationService( rockContext );
                locationService.Add( newLocation );
                rockContext.SaveChanges();
            }

            // Re-fetch it from the database to make sure we get a valid Id.
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
                    ( t.Street1 ?? string.Empty ) == ( location.Street1 ?? string.Empty ) &&
                    ( t.Street2 ?? string.Empty ) == ( location.Street2 ?? string.Empty ) &&
                    ( t.City ?? string.Empty ) == ( location.City ?? string.Empty ) &&
                    ( t.State ?? string.Empty ) == ( location.State ?? string.Empty ) &&
                    ( t.PostalCode ?? string.Empty ) == ( location.PostalCode ?? string.Empty ) &&
                    ( t.Country ?? string.Empty ) == ( location.Country ?? string.Empty ) );

                if ( existingLocation != null )
                {
                    return existingLocation;
                }
            }

            // If this location was not found on the specified group, or no group was specified, search for any instance of this location
            existingLocation = Queryable().FirstOrDefault( t =>
                ( t.Street1 ?? string.Empty ) == ( location.Street1 ?? string.Empty ) &&
                ( t.Street2 ?? string.Empty ) == ( location.Street2 ?? string.Empty ) &&
                ( t.City ?? string.Empty ) == ( location.City ?? string.Empty ) &&
                ( t.State ?? string.Empty ) == ( location.State ?? string.Empty ) &&
                ( t.PostalCode ?? string.Empty ) == ( location.PostalCode ?? string.Empty ) &&
                ( t.Country ?? string.Empty ) == ( location.Country ?? string.Empty ) );

            if ( existingLocation != null )
            {
                return existingLocation;
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.Location"/> by GeoPoint. If a match is not found,
        /// a new Location will be added based on the GeoPoint.
        /// </summary>
        /// <param name="point">A <see cref="System.Data.Entity.Spatial.DbGeography"/> object
        ///     representing the GeoPoint for the location.</param>
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

                // Re-fetch it from the database to make sure we get a valid Id.
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

                // Re-fetch it from the database to make sure we get a valid Id.
                return Get( newLocation.Guid );
            }

            return result;
        }

        /// <summary>
        /// Validate the required parts of the Location Address according to the address requirement rules defined in the Defined Type "Countries".
        /// Replaces the obsolete method <see cref="Rock.Model.LocationService.ValidateAddressRequirements(Location, out string)" /> 
        /// </summary>
        /// <param name="location"></param> 
        /// <param name="errorMessage"></param> Currently it is of type object, can be converted to string once the instance method is replaced.
        /// <returns></returns>
        public static bool ValidateLocationAddressRequirements( Location location, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( location == null
                 || location.IsNamedLocation )
            {
                return true;
            }

            // Get the Defined Value that specifies the address requirements for this country.
            var countryValue = DefinedTypeCache.Get( SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid() )
                .GetDefinedValueFromValue( location.Country );

            // Verify requirements for individual fields.
            var invalidFields = new List<string>();

            if ( countryValue != null )
            {
                var addressLine1Requirement = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLine1Requirement ).ConvertToEnum<DataEntryRequirementLevelSpecifier>( DataEntryRequirementLevelSpecifier.Optional );
                var addressLine2Requirement = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLine2Requirement ).ConvertToEnum<DataEntryRequirementLevelSpecifier>( DataEntryRequirementLevelSpecifier.Optional );
                var cityRequirement = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressCityRequirement ).ConvertToEnum<DataEntryRequirementLevelSpecifier>( DataEntryRequirementLevelSpecifier.Optional );
                var localityRequirement = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLocalityRequirement ).ConvertToEnum<DataEntryRequirementLevelSpecifier>( DataEntryRequirementLevelSpecifier.Optional );
                var stateRequirement = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressStateRequirement ).ConvertToEnum<DataEntryRequirementLevelSpecifier>( DataEntryRequirementLevelSpecifier.Optional );
                var postalCodeRequirement = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressPostalCodeRequirement ).ConvertToEnum<DataEntryRequirementLevelSpecifier>( DataEntryRequirementLevelSpecifier.Optional );

                if ( addressLine1Requirement == DataEntryRequirementLevelSpecifier.Required && string.IsNullOrWhiteSpace( location.Street1 ) )
                {
                    invalidFields.Add( "Address Line 1" );
                }

                if ( addressLine2Requirement == DataEntryRequirementLevelSpecifier.Required && string.IsNullOrWhiteSpace( location.Street2 ) )
                {
                    invalidFields.Add( "Address Line 2" );
                }

                if ( cityRequirement == DataEntryRequirementLevelSpecifier.Required && string.IsNullOrWhiteSpace( location.City ) )
                {
                    invalidFields.Add( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressCityLabel ).IfEmpty( "City" ) );
                }

                if ( localityRequirement == DataEntryRequirementLevelSpecifier.Required && string.IsNullOrWhiteSpace( location.County ) )
                {
                    invalidFields.Add( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLocalityLabel ).IfEmpty( "Locality" ) );
                }

                if ( stateRequirement == DataEntryRequirementLevelSpecifier.Required && string.IsNullOrWhiteSpace( location.State ) )
                {
                    invalidFields.Add( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressStateLabel ).IfEmpty( "State" ) );
                }

                if ( postalCodeRequirement == DataEntryRequirementLevelSpecifier.Required && string.IsNullOrWhiteSpace( location.PostalCode ) )
                {
                    invalidFields.Add( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressPostalCodeLabel ).IfEmpty( "Postal Code" ) );
                }
            }
            else
            {
                // If the Country field contains an undefined value, return an appropriate validation message...
                if ( !string.IsNullOrWhiteSpace( location.Country ) )
                {
                    errorMessage = $"Incomplete Address. Country value \"{location.Country}\" is invalid.";
                    return false;
                }

                // ... otherwise, return the standard message for a required field.
                invalidFields.Add( "Country" );
            }

            if ( invalidFields.Any() )
            {
                errorMessage = $"Incomplete Address. The following fields are required: { invalidFields.AsDelimited( ", " ) }.";
                return false;
            }

            // Verify at least one address field contains a value.
            if ( string.IsNullOrWhiteSpace( location.Street1 )
                 && string.IsNullOrWhiteSpace( location.Street2 )
                 && string.IsNullOrWhiteSpace( location.City )
                 && string.IsNullOrWhiteSpace( location.State )
                 && string.IsNullOrWhiteSpace( location.PostalCode ) )
            {
                errorMessage = "Invalid Address. At least one field is required.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate the required parts of the Location Address according to the address requirement rules defined in the Defined Type "Countries".
        /// </summary>
        /// <param name="location"></param>
        /// <param name="errorMessage">An empty string if the validation is successful, or a message describing the validation failure.</param>
        [Obsolete( "Please use the static method ValidateLocationAddressRequirements( Location location, out string errorMessage )" )]
        [RockObsolete( "1.14" )]
        public bool ValidateAddressRequirements( Location location, out string errorMessage )
        {
            bool isAddressValid = ValidateLocationAddressRequirements( location, out errorMessage );
            return isAddressValid;
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

                        // As long as there wasn't a connection error, update the attempted DateTime.
                        if ( ( result & Address.VerificationResult.ConnectionError ) != Address.VerificationResult.ConnectionError )
                        {
                            location.StandardizeAttemptedDateTime = RockDateTime.Now;
                        }

                        // If location was successfully geocoded, update the time stamp.
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

                        // As long as there wasn't a connection error, update the attempted DateTime.
                        if ( ( result & Address.VerificationResult.ConnectionError ) != Address.VerificationResult.ConnectionError )
                        {
                            location.GeocodeAttemptedDateTime = RockDateTime.Now;
                        }

                        // If location was successfully geocoded, update the time stamp.
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

            // If there is only one type of active service (standardization/geocoding) the other type's attempted DateTime
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
        /// Gets the mapCoordinate from postal code.
        /// </summary>
        /// <param name="postalCode">The postal code.</param>
        /// <returns></returns>
        public MapCoordinate GetMapCoordinateFromPostalCode( string postalCode )
        {
            Address.SmartyStreets smartyStreets = new Address.SmartyStreets();
            string resultMsg = string.Empty;
            var coordinate = smartyStreets.GetLocationFromPostalCode( postalCode, out resultMsg );

            // Log the results of the service.
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
        /// Gets the mapCoordinate from city and state combination.
        /// </summary>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public MapCoordinate GetMapCoordinateFromCityState( string city, string state )
        {
            Address.SmartyStreets smartyStreets = new Address.SmartyStreets();
            string resultMsg = string.Empty;
            var coordinate = smartyStreets.GetLocationFromCityState( city, state, out resultMsg );

            // Log the results of the service.
            if ( !string.IsNullOrWhiteSpace( resultMsg ) )
            {
                var rockContext = new RockContext();
                Model.ServiceLogService logService = new Model.ServiceLogService( rockContext );
                Model.ServiceLog log = new Model.ServiceLog();
                log.LogDateTime = RockDateTime.Now;
                log.Type = "Mapcoordinate from city and state";
                log.Name = smartyStreets.TypeName;
                log.Input = $"{city}, {state}";
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
                ",
                parentLocationId ) );
        }

        /// <summary>
        /// Gets all descendant ids.
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
                ",
                parentLocationId ) );
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
                ",
                locationId ) );
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
                ",
                locationId ) );
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

            return NamedLocationCache.Get( locationId.Value ).GetCampusIdForLocation();
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
        /// Gets all locations associated with a <seealso cref="Rock.Model.Device"/>, optionally including all child locations
        /// </summary>
        /// <param name="includeChildLocations">if set to <c>true</c> [include child locations].</param>
        /// <returns></returns>
        public IEnumerable<Location> GetAllDeviceLocations( bool includeChildLocations )
        {
            return GetByDevice( null, includeChildLocations );
        }

        /// <summary>
        /// Gets device locations for the specified deviceIds.
        /// </summary>
        /// <param name="deviceIds">The device ids to limit which Locations to return. Set to null to return all (which is the same as calling GetAllDeviceLocations) </param>
        /// <param name="includeChildLocations">if set to <c>true</c> [include child locations].</param>
        /// <returns></returns>
        public IEnumerable<Location> GetByDevice( IEnumerable<int> deviceIds, bool includeChildLocations )
        {
            string childQuery = includeChildLocations ? @"

        UNION ALL

        SELECT [a].Id
	FROM [Location] [a]
        INNER JOIN  CTE pcte ON pcte.Id = [a].[ParentLocationId]
        WHERE [a].[ParentLocationId] IS NOT NULL
" : string.Empty;

            string deviceClause;
            if ( deviceIds == null )
            {
                // if NULL is specified for deviceIds, don't restrict by device Id.
                deviceClause = string.Empty;
            }
            else
            {
                if ( !deviceIds.Any() )
                {
                    // if no device id are specified just return an empty list
                    return new List<Location>();
                }
                else if ( deviceIds.Count() == 1 )
                {
                    deviceClause = $"WHERE D.[DeviceId] = {deviceIds.First()}";
                }
                else
                {
                    deviceClause = $"WHERE D.[DeviceId] IN ({deviceIds.ToList().AsDelimited( "," )})";
                }
            }

            string query = $@"
    WITH CTE AS (
        SELECT L.Id
        FROM [DeviceLocation] D
        INNER JOIN [Location] L ON L.[Id] = D.[LocationId]
        {deviceClause}
{childQuery}
    )

    SELECT L.* FROM CTE
    INNER JOIN [Location] L ON L.[Id] = CTE.[Id]";

            return ExecuteQuery( query );
        }

        /// <summary>
        /// Gets the locations associated to a device and optionally any child locations
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="includeChildLocations">if set to <c>true</c> [include child locations].</param>
        /// <returns></returns>
        public IEnumerable<Location> GetByDevice( int deviceId, bool includeChildLocations = true )
        {
            return GetByDevice( new int[] { deviceId }, includeChildLocations );
        }

        /// <summary>
        /// Gets the locations for the Group and Schedule
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        public IQueryable<Location> GetByGroupSchedule( int scheduleId, int groupId )
        {
            var groupLocationQuery = new GroupLocationService( this.Context as RockContext ).Queryable().Where( gl => gl.Schedules.Any( s => s.Id == scheduleId ) && gl.GroupId == groupId );
            return this.Queryable().Where( l => groupLocationQuery.Any( gl => gl.LocationId == l.Id ) );
        }
    }

    /// <summary>
    /// Options for <seealso cref="LocationService.Get(string, string, string, string, string, string, string, GetLocationArgs)"/>
    /// </summary>
    public sealed class GetLocationArgs
    {
        /// PA: Changed all the setters from to public so that they may be accessed by the plugins.

        /// <summary>
        /// The group (usually a Family) that should be searched first. This is NOT a search constraint.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public Group Group { get; set; }

        /// <summary>
        /// Use a <see cref="VerificationComponent.Verify(Location, out string)"> Verification Service</see> to verify and standardize the address.
        /// Default is <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [verify location]; otherwise, <c>false</c>.
        /// </value>
        public bool VerifyLocation { get; set; } = true;

        /// <summary>
        /// Create a new location if a matching location record cannot be found
        /// Default is <c>true</c>..
        /// </summary>
        /// <value>
        ///   <c>true</c> if [create new location]; otherwise, <c>false</c>.
        /// </value>
        public bool CreateNewLocation { get; set; } = true;

        /// <summary>
        /// Validate the required address fields have a value.
        /// Default is <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [validate location]; otherwise, <c>false</c>.
        /// </value>
        public bool ValidateLocation { get; set; } = true;
    }
}
