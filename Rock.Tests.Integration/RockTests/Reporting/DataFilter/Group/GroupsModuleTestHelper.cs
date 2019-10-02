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
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Reporting.DataFilter.Group
{
    /// <summary>
    /// Helper functions for testing core functions of Rock.
    /// </summary>
    [TestClass]
    public partial class GroupsModuleTestHelper
    {
        /// <summary>
        /// Instructions for Groups feature testing.
        /// </summary>
        [TestMethod]
        [TestProperty( "Feature", TestFeatures.Groups )]
        public void _ReadMe()
        {
            /*
             * Integration tests for the Groups feature require that the test data set exists in the current database.
             * To set the target database connection for integration testing, modify the "app.ConnectionStrings.config" file in this project.
             * 
             * Test data can be added to the current database by running the tests in the "Data Setup" Feature:
             * - GroupsModule_AddTestData: adds the complete set of test data.
             *
             * Test data can be removed or maintained for the current database by running the tests in the "Data Maintenance" Feature:
             * - GroupsModule_RemoveTestData: removes the complete set of test data.
             * - GroupsModule_AddDataViews: adds the test Data Views associated with this feature to the current database.
             */
        }

        #region Test Settings

        // Set this value to the foreign key used to identify the test data added to the target database.
        private readonly string _SampleDataForeignKey = "Groups Sample Data";

        #endregion

        public static class Constants
        {
            // A DataView that returns all of the locations outside the state of Arizona.
            public static Guid DataViewLocationsOutsideArizonaGuid = new Guid( "14B1854D-4F45-4F4D-AFFF-C0A1E06353DF" );
            public static Guid DataViewLocationsInsideArizonaGuid = new Guid( "C39B353E-3E44-42C0-9D85-2107FB5E8C04" );

            public static Guid CategoryGroupsGuid = new Guid( "5CF5224C-F01D-4904-91D0-E58B723F0D2A" );
            public static Guid CategoryLocationsGuid = new Guid( "1D45C0A7-3DE8-428C-94A8-14E5ED5E2E36" );

        }

        #region Test Data Setup

        /// <summary>
        /// Remove all Groups test data from the current database.
        /// </summary>
        [TestMethod]
        [TestCategory( TestCategories.RemoveData )]
        [TestProperty( "Feature", TestFeatures.DataMaintenance )]
        public void GroupsModule_RemoveTestData()
        {
            var dataContext = new RockContext();

            // Remove Data Views
            var reportingHelper = new ReportingModuleTestHelper( _SampleDataForeignKey );

            reportingHelper.DeleteDataViewsByRecordTag( dataContext );

            // Remove Categories
            var coreHelper = new CoreModuleTestHelper( _SampleDataForeignKey );

            coreHelper.DeleteCategoriesByRecordTag( dataContext );
        }

        /// <summary>
        /// Add a complete set of Steps test data to the current database.
        /// Existing Steps test data will be removed.
        /// </summary>
        [TestMethod]
        [TestCategory( TestCategories.AddData )]
        [TestProperty( "Feature", TestFeatures.DataSetup )]
        public void GroupsModule_AddTestData()
        {
            GroupsModule_RemoveTestData();

            GroupsModule_AddDataViews();
        }

        [TestMethod]
        [TestCategory( TestCategories.DeveloperSetup )]
        [TestProperty( "Feature", TestFeatures.DataMaintenance )]
        public void GroupsModule_AddDataViews()
        {
            var dataContext = new RockContext();

            // Add Data View Category "Groups".
            const string categoryDataViewName = "Groups";

            Debug.Print( $"Adding Data View Category \"{ categoryDataViewName }\"..." );

            var entityTypeId = EntityTypeCache.Get( typeof( global::Rock.Model.DataView ) ).Id;

            var coreHelper = new CoreModuleTestHelper( _SampleDataForeignKey );

            var locationsCategory = coreHelper.CreateCategory( categoryDataViewName, Constants.CategoryLocationsGuid, entityTypeId );

            coreHelper.AddOrUpdateCategory( dataContext, locationsCategory );

            dataContext.SaveChanges();

            // Get Data View service.
            var service = new DataViewService( dataContext );

            int categoryId = CategoryCache.GetId( Constants.CategoryLocationsGuid ) ?? 0;

            DataViewFilter rootFilter;

            // Create Data View: Locations Inside Arizona
            const string dataViewLocationsInsideArizona = "Locations in the state of Arizona";

            Debug.Print( $"Adding Data View \"{ dataViewLocationsInsideArizona }\"..." );

            var dataViewInside = new DataView();

            dataViewInside.IsSystem = false;

            dataViewInside.Name = dataViewLocationsInsideArizona;
            dataViewInside.Description = "Locations that are within the state of Arizona.";
            dataViewInside.EntityTypeId = EntityTypeCache.GetId( typeof( global::Rock.Model.Location ) );
            dataViewInside.CategoryId = categoryId;
            dataViewInside.Guid = Constants.DataViewLocationsInsideArizonaGuid;
            dataViewInside.ForeignKey = _SampleDataForeignKey;

            rootFilter = new DataViewFilter();

            rootFilter.ExpressionType = FilterExpressionType.GroupAll;

            dataViewInside.DataViewFilter = rootFilter;

            var inStateFilter = new TextPropertyFilterSettings { PropertyName = "State", Comparison = ComparisonType.EqualTo, Value = "AZ" };

            rootFilter.ChildFilters.Add( inStateFilter.GetFilter() );

            service.Add( dataViewInside );

            dataContext.SaveChanges();

            // Create Data View: Locations Outside Arizona
            const string dataViewLocationsOutsideArizona = "Locations outside Arizona";

            Debug.Print( $"Adding Data View \"{ dataViewLocationsOutsideArizona }\"..." );

            var dataViewOutside = new DataView();

            dataViewOutside.IsSystem = false;

            dataViewOutside.Name = dataViewLocationsOutsideArizona;
            dataViewOutside.Description = "Locations that are not within the state of Arizona.";
            dataViewOutside.EntityTypeId = EntityTypeCache.GetId( typeof( global::Rock.Model.Location ) );
            dataViewOutside.CategoryId = categoryId;
            dataViewOutside.Guid = Constants.DataViewLocationsOutsideArizonaGuid;
            dataViewOutside.ForeignKey = _SampleDataForeignKey;

            rootFilter = new DataViewFilter();

            rootFilter.ExpressionType = FilterExpressionType.GroupAll;

            dataViewOutside.DataViewFilter = rootFilter;

            var notInStateFilter = new TextPropertyFilterSettings { PropertyName = "State", Comparison = ComparisonType.NotEqualTo, Value = "AZ" };

            rootFilter.ChildFilters.Add( notInStateFilter.GetFilter() );

            service.Add( dataViewOutside );

            dataContext.SaveChanges();
        }

        #endregion
    }
}
