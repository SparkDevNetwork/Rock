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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    /// <summary>
    /// Tests to verify operations related to Locations.
    /// </summary>
    [TestClass]
    public class LocationTests : DatabaseTestsBase
    {
        #region Setup/Cleanup

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            CreateTestData();
        }

        #endregion

        #region Location Address Requirements Tests

        private static string CountryCodeWithOptionalAddressRequirements = "TST-O";
        private static string CountryCodeWithMandatoryAddressRequirements = "TST-R";
        private static string CountryCodeWithMixedAddressRequirements = "TST-M";

        private static void CreateTestData()
        {
            var dataContext = new RockContext();

            var definedTypeService = new DefinedTypeService( dataContext );

            var attributes1 = new Dictionary<string, object>();

            attributes1.Add( "core_CountryAddressLine1Requirement", DataEntryRequirementLevelSpecifier.Required );
            attributes1.Add( "core_CountryAddressLine2Requirement", DataEntryRequirementLevelSpecifier.Required );
            attributes1.Add( "core_CountryAddressCityRequirement", DataEntryRequirementLevelSpecifier.Required );
            attributes1.Add( "core_CountryAddressStateRequirement", DataEntryRequirementLevelSpecifier.Required );
            attributes1.Add( "core_CountryAddressLocalityRequirement", DataEntryRequirementLevelSpecifier.Required );
            attributes1.Add( "core_CountryAddressPostalCodeRequirement", DataEntryRequirementLevelSpecifier.Required );

            definedTypeService.AddOrUpdateValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, CountryCodeWithMandatoryAddressRequirements, "Test-1", attributes1 );

            var attributes2 = new Dictionary<string, object>();

            attributes2.Add( "core_CountryAddressLine1Requirement", DataEntryRequirementLevelSpecifier.Optional );
            attributes2.Add( "core_CountryAddressLine2Requirement", DataEntryRequirementLevelSpecifier.Optional );
            attributes2.Add( "core_CountryAddressCityRequirement", DataEntryRequirementLevelSpecifier.Optional );
            attributes2.Add( "core_CountryAddressStateRequirement", DataEntryRequirementLevelSpecifier.Optional );
            attributes2.Add( "core_CountryAddressLocalityRequirement", DataEntryRequirementLevelSpecifier.Optional );
            attributes2.Add( "core_CountryAddressPostalCodeRequirement", DataEntryRequirementLevelSpecifier.Optional );

            definedTypeService.AddOrUpdateValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, CountryCodeWithOptionalAddressRequirements, "Test-2", attributes2 );

            var attributes3 = new Dictionary<string, object>();

            attributes3.Add( "core_CountryAddressLine1Requirement", DataEntryRequirementLevelSpecifier.Optional );
            attributes3.Add( "core_CountryAddressLine2Requirement", DataEntryRequirementLevelSpecifier.Optional );
            attributes3.Add( "core_CountryAddressCityRequirement", DataEntryRequirementLevelSpecifier.Required );
            attributes3.Add( "core_CountryAddressStateRequirement", DataEntryRequirementLevelSpecifier.Required );
            attributes3.Add( "core_CountryAddressLocalityRequirement", DataEntryRequirementLevelSpecifier.Optional );
            attributes3.Add( "core_CountryAddressPostalCodeRequirement", DataEntryRequirementLevelSpecifier.Optional );

            definedTypeService.AddOrUpdateValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, CountryCodeWithMixedAddressRequirements, "Test-3", attributes3 );

        }

        private static void RemoveTestData()
        {
            var dataContext = new RockContext();

            // Remove Defined Values for Countries
            var definedTypeService = new DefinedTypeService( dataContext );

            var countryCodeValues = new List<string> { CountryCodeWithMandatoryAddressRequirements, CountryCodeWithOptionalAddressRequirements, CountryCodeWithMixedAddressRequirements };

            definedTypeService.DeleteValues( SystemGuid.DefinedType.LOCATION_COUNTRIES, countryCodeValues );

            // Remove test locations.
            var locationService = new LocationService( dataContext );

            locationService.DeleteRange( locationService.Queryable().Where( x => countryCodeValues.Contains( x.Country ) ) );

            dataContext.SaveChanges();
        }

        [TestMethod]
        public void LocationAddress_WithMissingRequiredFields_FailsValidationCheck()
        {
            var dataContext = new RockContext();

            var locationTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_BUILDING ).Id;

            var location = new Location()
            {
                Country = CountryCodeWithMandatoryAddressRequirements,
                LocationTypeValueId = locationTypeValueId,
                IsActive = true,
            };

            var locationService = new LocationService( dataContext );

            string errorMessage;

            var isValid = LocationService.ValidateLocationAddressRequirements( location, out errorMessage );

            Assert.That.IsFalse( isValid, "Address validation succeeded unexpectedly." );
            Assert.That.AreEqual( $"Incomplete Address. The following fields are required: Address Line 1, Address Line 2, City, County, Province, Postal Code.", errorMessage );
        }

        [TestMethod]
        public void LocationAddress_WithMissingOptionalFields_IsAdded()
        {
            var dataContext = new RockContext();

            var street2Guid = Guid.NewGuid().ToString();

            var location = new Location()
            {
                Street2 = street2Guid,
                Country = CountryCodeWithOptionalAddressRequirements,
                IsActive = true,
            };

            var locationService = new LocationService( dataContext );

            var newLocation = locationService.Get( location.Street1, location.Street2, location.City, location.State, location.County, location.PostalCode, location.Country, group: null, verifyLocation: false, createNewLocation: true );

            Assert.That.AreEqual( street2Guid, newLocation.Street2.ToLower() );
        }

        [TestMethod]
        public void LocationAddress_WithSuppliedRequiredFields_IsAdded()
        {
            var dataContext = new RockContext();

            var street2Guid = Guid.NewGuid().ToString();

            var location = new Location()
            {
                Street1 = "Street 1",
                Street2 = street2Guid,
                County = "County",
                City = "City",
                State = "State",
                PostalCode = "1234",
                Country = CountryCodeWithMandatoryAddressRequirements,
                IsActive = true,
            };

            var locationService = new LocationService( dataContext );

            string errorMessage;

            var isValid = LocationService.ValidateLocationAddressRequirements( location, out errorMessage );

            Assert.That.IsTrue( isValid, "Address validation failed." );

            var newLocation = locationService.Get( location.Street1, location.Street2, location.City, location.State, location.County, location.PostalCode, location.Country, group: null, verifyLocation: false, createNewLocation: true );

            Assert.That.AreEqual( street2Guid, newLocation.Street2.ToLower() );
        }

        [TestMethod]
        public void LocationAddress_WithSuppliedRequiredFieldsAndEmptyOptionalFields_IsAdded()
        {
            var dataContext = new RockContext();

            var cityGuid = Guid.NewGuid().ToString();

            // Country has mixed address requirements. Required fields are supplied, one optional field (Street1) is not.
            var location = new Location()
            {
                City = cityGuid,
                State = "State",
                Country = CountryCodeWithMixedAddressRequirements
            };

            var locationService = new LocationService( dataContext );

            string errorMessage;

            var isValid = LocationService.ValidateLocationAddressRequirements( location, out errorMessage );

            Assert.That.IsTrue( isValid, "Address validation failed." );

            var newLocation = locationService.Get( location.Street1, location.Street2, location.City, location.State, location.County, location.PostalCode, location.Country, group: null, verifyLocation: false, createNewLocation: true );

            Assert.That.AreEqual( cityGuid, newLocation.City.ToLower() );
        }

        [TestMethod]
        public void LocationAddress_WithMissingRequiredFieldsAndPopulatedOptionalFields_ThrowsIncompleteException()
        {
            var dataContext = new RockContext();

            // Country has mixed address requirements. Optional fields are supplied, one required field (State) is not.
            var location = new Location()
            {
                Street1 = "Street 1",
                City = "City",
                Country = CountryCodeWithMixedAddressRequirements
            };

            var locationService = new LocationService( dataContext );

            string errorMessage;

            var isValid = LocationService.ValidateLocationAddressRequirements( location, out errorMessage );

            Assert.That.IsFalse( isValid, "Address validation succeeded unexpectedly." );
            Assert.That.AreEqual( $"Incomplete Address. The following fields are required: Province.", errorMessage );
        }

        [TestMethod]
        public void LocationAddress_WithAllEmptyFields_ThrowsEmptyAddressException()
        {
            var dataContext = new RockContext();

            var location = new Location()
            {
                Country = CountryCodeWithOptionalAddressRequirements,
                IsActive = true,
            };

            var locationService = new LocationService( dataContext );

            string errorMessage;

            var isValid = LocationService.ValidateLocationAddressRequirements( location, out errorMessage );

            Assert.That.IsFalse( isValid, "Address validation succeeded unexpectedly." );
            Assert.That.AreEqual( "Invalid Address. At least one field is required.", errorMessage );
        }

        #endregion
    }
}
