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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Modules.Reporting.DataFilter.Group
{
    /// <summary>
    /// Test DataFilter: Group/Related Data View/Location Data View.
    /// </summary>
    [TestClass]
    public class LocationDataViewDataFilterTests : DataFilterTestBase
    {
        private const string _TestCategory = "Rock.Crm.Groups.Reporting.LocationDataView.Tests";

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            TestDataHelper.Reporting.AddDataViewsForGroupsModule();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            TestDataHelper.Reporting.DeleteDataViewsByRecordTag( new RockContext(), "TestData" );
        }

        /// <summary>
        /// Verify that the settings can be correctly serialized to a string and deserialized from the same string.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Groups )]
        public void GroupFilters_RelatedDataViewLocation_CanSerializeSettings()
        {
            var settingsSource = new Rock.Reporting.DataFilter.Group.LocationDataViewFilter.FilterSettings();

            settingsSource.DataViewGuid = TestGuids.DataViews.LocationsOutsideArizona.AsGuid();

            var settingsString = settingsSource.ToSelectionString();

            var settingsTarget = new Rock.Reporting.DataFilter.Group.LocationDataViewFilter.FilterSettings( settingsString );

            Assert.That.AreEqual( TestGuids.DataViews.LocationsOutsideArizona.AsGuid(), settingsTarget.DataViewGuid );
        }

        /// <summary>
        /// Verify that filtering by a Location Data View correctly returns Groups having a Location referenced in that Data View.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Groups )]
        public void GroupFilters_RelatedDataViewLocation_ShouldReturnGroupsWithAtLeastOneRelatedLocation()
        {
            var settings = new Rock.Reporting.DataFilter.Group.LocationDataViewFilter.FilterSettings();

            settings.DataViewGuid = TestGuids.DataViews.LocationsInsideArizona.AsGuid();

            var groupQuery = GetGroupQueryWithLocationDataViewFilter( settings );

            var results = groupQuery.ToList();

            Assert.That.IsTrue( results.Count > 0, "The result set must contain at least one group." );

            // Verify all Groups have at least one Location where State = "AZ".
            var countOfGroupsNotInArizona = results.Where( x => !x.GroupLocations.Any( gl => gl.Location.State == "AZ" ) ).Count();

            Assert.That.IsTrue( countOfGroupsNotInArizona == 0, "The result set contains one or more groups that do not match the location filter." );
        }

        /// <summary>
        /// Verify that filtering by a Step Data View does not return a Group who does not have a matching Step.
        /// </summary>
        [TestMethod]
        [TestCategory( _TestCategory )]
        [TestProperty( "Feature", TestFeatures.Groups )]
        public void GroupFilters_RelatedDataViewLocation_ShouldNotReturnGroupWithNoRelatedLocations()
        {
            var settings = new Rock.Reporting.DataFilter.Group.LocationDataViewFilter.FilterSettings();
            settings.DataViewGuid = TestGuids.DataViews.LocationsOutsideArizona.AsGuid();

            var groupQuery = GetGroupQueryWithLocationDataViewFilter( settings );

            var results = groupQuery.ToList();

            Assert.That.IsTrue( results.Count > 0, "The result set must contain at least one group." );

            // Verify that there are no Groups in the result set having any Location where State = "AZ".
            var countOfGroupsInArizona = results.Where( x => x.GroupLocations.Any( gl => gl.Location.State == "AZ" ) ).Count();

            Assert.That.IsTrue( countOfGroupsInArizona == 0, "The result set contains one or more groups that do not match the location filter." );
        }

        /// <summary>
        /// Create a Group Query using the LocationDataViewFilter with the specified settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private IQueryable<Rock.Model.Group> GetGroupQueryWithLocationDataViewFilter( global::Rock.Reporting.DataFilter.Group.LocationDataViewFilter.FilterSettings settings )
        {
            var settingsFilter = new Rock.Reporting.DataFilter.Group.LocationDataViewFilter();

            var dataContext = new RockContext();

            var groupService = new GroupService( dataContext );

            var parameterExpression = groupService.ParameterExpression;

            var predicate = settingsFilter.GetExpression( typeof( Rock.Model.Group ), groupService, parameterExpression, settings.ToSelectionString() );

            var groupQuery = GetFilteredEntityQuery<Rock.Model.Group>( dataContext, predicate, parameterExpression );

            return groupQuery;
        }
    }
}
