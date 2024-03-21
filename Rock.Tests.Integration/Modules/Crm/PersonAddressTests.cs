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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Crm
{
    [TestClass]
    public class PersonAddressTests : DatabaseTestsBase
    {
        private const string _PersonGuidTestPerson1 = "60B92AAD-4053-4DB7-B80F-9D79595B1325";

        /// <summary>
        /// Verify that the Home Location returns an available address, even if it is not set as a mapped location.
        /// </summary>
        /// <remarks>
        /// Verifies a fix for Issue #1859 [https://github.com/SparkDevNetwork/Rock/issues/1859].
        /// Previously, the GetHomeLocation did not return a Home address if it was not also marked as a Mapped Address.
        /// </remarks>
        [TestMethod]
        public void GetHomeLocation_WhereNoMappedLocationsExist_ReturnsMostRecentLocation()
        {
            var rockContext = new RockContext();
            var person = new Person()
            {
                Guid = _PersonGuidTestPerson1.AsGuid(),
                RecordTypeValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ),
                FirstName = "Adam",
                LastName = "@Test",
                Email = "adam@test.com",
                ForeignKey = ""
            };

            var newFamily = CrmModuleTestHelper.AddOrReplacePerson( person, rockContext );

            // Add new addresses, but do not set as mapped locations.
            GroupService.AddNewGroupAddress( rockContext,
                newFamily,
                SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                street1: "1 New St",
                street2: "",
                city: "MyCity",
                state: "MyState",
                postalCode: "123456",
                country: "US",
                moveExistingToPrevious: false,
                modifiedBy: "test",
                isMailingLocation: false,
                isMappedLocation: false );

            GroupService.AddNewGroupAddress( rockContext,
                newFamily,
                SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                street1: "2 New St",
                street2: "",
                city: "MyCity",
                state: "MyState",
                postalCode: "123456",
                country: "US",
                moveExistingToPrevious: false,
                modifiedBy: "test",
                isMailingLocation: false,
                isMappedLocation: false );

            var location = person.GetHomeLocation( rockContext );

            // Verify that the last-added address is returned as the Home Location,
            // even though neither address is set as the Mapped Location.
            Assert.That.AreEqual( "2 New St", location?.Street1, "Expected Home Location not found." );
        }

        /// <summary>
        /// Verify that the Home Location returns a mapped location if possible, even if more recent unmapped addresses exist.
        /// </summary>
        [TestMethod]
        public void GetHomeLocation_WhereMostRecentIsNotMapped_ReturnsEarlierMappedLocation()
        {
            var rockContext = new RockContext();
            var person = new Person()
            {
                Guid = _PersonGuidTestPerson1.AsGuid(),
                RecordTypeValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ),
                FirstName = "Adam",
                LastName = "@Test",
                Email = "adam@test.com"
            };

            var newFamily = CrmModuleTestHelper.AddOrReplacePerson( person, rockContext );

            // Add new Home addresses, but set the first address as the only mapped location.
            GroupService.AddNewGroupAddress( rockContext,
                newFamily,
                SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                street1: "1 New St",
                street2: "",
                city: "MyCity",
                state: "MyState",
                postalCode: "123456",
                country: "US",
                moveExistingToPrevious: false,
                modifiedBy: "test",
                isMailingLocation: false,
                isMappedLocation: true );

            GroupService.AddNewGroupAddress( rockContext,
                newFamily,
                SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                street1: "2 New St",
                street2: "",
                city: "MyCity",
                state: "MyState",
                postalCode: "123456",
                country: "US",
                moveExistingToPrevious: false,
                modifiedBy: "test",
                isMailingLocation: false,
                isMappedLocation: false );

            var location = person.GetHomeLocation( rockContext );

            // Verify that the first address is returned as the Home Location,
            // because it is the only mapped location.
            Assert.That.AreEqual( "1 New St", location?.Street1, "Incorrect Home Location returned." );
        }
    }
}
