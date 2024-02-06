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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Tests.Integration;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Performance.Modules.Reporting
{
    /// <summary>
    /// Tests that verify the performance of Data Views.
    /// </summary>
    [TestClass]
    public class DataViewTests : DatabaseTestsBase
    {
        private const string _allPeopleDataViewGuid = "462A34E4-9BE4-4573-8260-FE55726BA2CE";

        /// <summary>
        /// Verifies that attempting to create an entity set from a Data View that returns a large number of results
        /// succeed within a reasonable time.
        /// The test database should contain 100,000+ person records for this test.
        /// </summary>
        [TestMethod]
        [Ignore( "This requires the test database to have over 100,000 person records in the data view. It should be updated to create a Group dataview since those can be created more quickly." )]
        public void DataView_CreateEntitySetForVeryLargeDataView_IsPerformant()
        {
            var rockContext = new RockContext();

            // Create a Data View that returns all Person records.
            var dataViewArgs = new TestDataHelper.Reporting.CreateDataViewArgs
            {
                Name = "All People",
                AppliesToEntityTypeIdentifier = "Rock.Model.Person",
                Guid = _allPeopleDataViewGuid.AsGuid(),
                ExistingItemStrategy = Integration.TestData.CreateExistingItemStrategySpecifier.Replace
            };

            var dataView = TestDataHelper.Reporting.CreateDataView( dataViewArgs );
            var dataViewId = dataView.Id;

            TestHelper.ExecuteWithTimer( "Create EntitySet from DataView", () =>
            {
                var logger = new RockLoggerMemoryBuffer();
                logger.EventLogged += ( s, e ) =>
                {
                    TestHelper.Log( $"<{e.Event.Domain}> {e.Event.Message}" );
                };

                var entitySetService = new EntitySetService( rockContext );

                var entitySetArgs = new EntitySetService.CreateEntitySetFromDataViewActionArgs
                {
                    DataViewId = dataViewId,
                    EntitySetName = $"DataViewId_{dataViewId}",
                    EntitySetNote = "DataViewResultsCache",
                    ExpirationPeriod = new TimeSpan( 1, 0, 0 ),
                    IgnorePersistedValues = true
                };

                var entitySetId = EntitySetService.CreateEntitySetFromDataView( entitySetArgs, rockContext ) ?? 0;
                rockContext.SaveChanges();

                var itemCount = entitySetService.GetEntityQuery( entitySetId ).Count();
                TestHelper.Log( $"EntitySet created with {itemCount} records." );

                // Assert results.
                // NOTE: On the test system, a Data View returning 370,000 records is processed in ~30s.
                Assert.IsTrue( itemCount > 100_000, "The EntitySet does not contain sufficient items to satisfy this test." );
            } );
        }

        /// <summary>
        /// Verifies that attempting to create an entity set from a Data View that returns a large number of results
        /// succeed within a reasonable time.
        /// The test database should contain 100,000+ person records for this test.
        /// </summary>
        [TestMethod]
        [Ignore( "This requires the test database to have over 100,000 person records in the data view. It should be updated to create a Group dataview since those can be created more quickly." )]
        public void DataView_PersistVeryLargeDataView_IsSuccessful()
        {
            var rockContext = new RockContext();

            // Create a Data View that returns all Person records.
            var dataViewArgs = new TestDataHelper.Reporting.CreateDataViewArgs
            {
                Name = "All People",
                AppliesToEntityTypeIdentifier = "Rock.Model.Person",
                Guid = _allPeopleDataViewGuid.AsGuid(),
                ExistingItemStrategy = Integration.TestData.CreateExistingItemStrategySpecifier.Replace
            };

            var dataView = TestDataHelper.Reporting.CreateDataView( dataViewArgs );
            var dataViewId = dataView.Id;
            var startDateTime = RockDateTime.Now;

            TestHelper.ExecuteWithTimer( "Persist Very Large DataView", () =>
            {
                dataView.PersistResult();
            } );

            var itemCount = dataView.GetQuery().Count();
            TestHelper.Log( $"DataView persisted with {itemCount} records." );

            // Assert results.
            // NOTE: On the test system, a Data View returning 350,000 records is persisted in ~30s.
            Assert.IsTrue( itemCount > 100_000, "The Data View does not return sufficient items to satisfy this test." );
            Assert.IsTrue( dataView.PersistedLastRefreshDateTime > startDateTime, "Data View persisted values not updated." );
        }
    }
}
