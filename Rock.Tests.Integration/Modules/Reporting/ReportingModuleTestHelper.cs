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
using System.Linq;
using System.Transactions;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Reporting.DataFilter;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Reporting
{
    /// <summary>
    /// Functions to assist with testing the reporting module.
    /// </summary>
    public class ReportingModuleTestHelper
    {
        private string _RecordTag = "TestData";

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportingModuleTestHelper()
        {
            //
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="recordTag">A tag that is added to the ForeignKey property of each record created by this helper instance.</param>
        public ReportingModuleTestHelper( string recordTag )
        {
            _RecordTag = recordTag;
        }

        /// <summary>
        /// Add or update a DataView.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="dataView"></param>
        public void AddOrUpdateDataView( RockContext dataContext, DataView dataView )
        {
            var dataViewService = new DataViewService( dataContext );

            var existingDataView = dataViewService.Queryable().FirstOrDefault( x => x.Guid == dataView.Guid );

            if ( existingDataView == null )
            {
                dataViewService.Add( dataView );

                existingDataView = dataView;
            }
            else
            {
                existingDataView.CopyPropertiesFrom( dataView );
            }
        }

        /// <summary>
        /// Create a new DataView
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        /// <param name="appliesToEntityTypeId"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public DataView CreateDataView( string name, Guid guid, int appliesToEntityTypeId, int order = 0 )
        {
            var newDataView = new DataView();

            newDataView.Name = name;
            newDataView.Guid = guid;
            newDataView.IsSystem = true;
            newDataView.EntityTypeId = appliesToEntityTypeId;
            //newDataView.Order = order;

            newDataView.ForeignKey = _RecordTag;

            return newDataView;
        }

        /// <summary>
        /// Remove DataViews flagged with the current test record tag.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public int DeleteDataViewsByRecordTag( RockContext dataContext )
        {
            // Remove DataViews associated with the current test record tag.
            var recordsDeleted = dataContext.Database.ExecuteSqlCommand( $"delete from [DataView] where [ForeignKey] = '{_RecordTag}'" );

            Debug.Print( $"Delete Test Data: {recordsDeleted} DataViews deleted." );

            return recordsDeleted;
        }

        /// <summary>
        /// Remove DataViews flagged with the current test record tag.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public bool DeleteDataView( RockContext dataContext, Guid dataViewGuid )
        {
            using ( var scope = new TransactionScope() )
            {
                var dataViewService = new DataViewService( dataContext );

                var dataView = dataViewService.Get( dataViewGuid );

                if ( dataView == null )
                {
                    return false;
                }

                var dataViewFilterService = new DataViewFilterService( dataContext );

                DeleteDataViewFilter( dataView.DataViewFilter, dataViewFilterService, dataContext );

                var isDeleted = dataViewService.Delete( dataView );

                if (!isDeleted)
                {
                    return false;
                }

                dataContext.SaveChanges();

                scope.Complete();
            }

            return true;
        }

        /// <summary>
        /// Deletes the data view filter.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        /// <param name="service">The service.</param>
        private void DeleteDataViewFilter( DataViewFilter dataViewFilter, DataViewFilterService service, RockContext rockContext )
        {
            if ( dataViewFilter == null )
            {
                return;
            }

            foreach ( var childFilter in dataViewFilter.ChildFilters.ToList() )
            {
                DeleteDataViewFilter( childFilter, service, rockContext );
            }

            dataViewFilter.DataViewId = null;
            dataViewFilter.RelatedDataViewId = null;

            rockContext.SaveChanges();

            service.Delete( dataViewFilter );
        }

        public void AddDataViewsForGroupsModule()
        {
            var dataContext = new RockContext();

            // Remove existing Data Views.
            DeleteDataView( dataContext, TestGuids.DataViews.LocationsInsideArizona.AsGuid() );
            DeleteDataView( dataContext, TestGuids.DataViews.LocationsOutsideArizona.AsGuid() );

            dataContext.SaveChanges();

            // Add Data View Category "Groups".
            const string categoryDataViewName = "Groups";

            Debug.Print( $"Adding Data View Category \"{ categoryDataViewName }\"..." );

            var entityTypeId = EntityTypeCache.Get( typeof( global::Rock.Model.DataView ) ).Id;

            var coreHelper = new CoreModuleDataFactory( _RecordTag );

            var locationsCategory = coreHelper.CreateCategory( categoryDataViewName, TestGuids.Category.DataViewLocations.AsGuid(), entityTypeId );

            coreHelper.AddOrUpdateCategory( dataContext, locationsCategory );

            dataContext.SaveChanges();

            // Get Data View service.
            int categoryId = CategoryCache.GetId( TestGuids.Category.DataViewLocations.AsGuid() ) ?? 0;

            DataViewFilter rootFilter;

            // Create Data View: Locations Inside Arizona
            const string dataViewLocationsInsideArizona = "Locations in the state of Arizona";

            Debug.Print( $"Adding Data View \"{ dataViewLocationsInsideArizona }\"..." );

            var service = new DataViewService( dataContext );

            var dataViewInside = new DataView();
            dataViewInside.IsSystem = false;
            dataViewInside.Name = dataViewLocationsInsideArizona;
            dataViewInside.Description = "Locations that are within the state of Arizona.";
            dataViewInside.EntityTypeId = EntityTypeCache.GetId( typeof( global::Rock.Model.Location ) );
            dataViewInside.CategoryId = categoryId;
            dataViewInside.Guid = TestGuids.DataViews.LocationsInsideArizona.AsGuid();
            dataViewInside.ForeignKey = _RecordTag;

            service.Add( dataViewInside );

            dataContext.SaveChanges();

            rootFilter = new DataViewFilter();
            rootFilter.ExpressionType = FilterExpressionType.GroupAll;

            dataViewInside.DataViewFilter = rootFilter;

            var inStateFilter = new TextPropertyFilterSettings { PropertyName = "State", Comparison = ComparisonType.EqualTo, Value = "AZ" };

            rootFilter.ChildFilters.Add( inStateFilter.GetFilter() );

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
            dataViewOutside.Guid = TestGuids.DataViews.LocationsOutsideArizona.AsGuid();
            dataViewOutside.ForeignKey = _RecordTag;

            service.Add( dataViewOutside );

            dataContext.SaveChanges();

            rootFilter = new DataViewFilter();

            rootFilter.ExpressionType = FilterExpressionType.GroupAll;
            dataViewOutside.DataViewFilter = rootFilter;

            var notInStateFilter = new TextPropertyFilterSettings { PropertyName = "State", Comparison = ComparisonType.NotEqualTo, Value = "AZ" };

            rootFilter.ChildFilters.Add( notInStateFilter.GetFilter() );

            dataContext.SaveChanges();
        }
    }
}
