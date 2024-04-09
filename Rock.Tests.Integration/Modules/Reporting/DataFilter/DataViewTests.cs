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
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Reporting.DataFilter
{
    [TestClass]
    public class DataViewTests : DatabaseTestsBase
    {
        #region Tests

        /// <summary>
        /// Verify that all Data Views can be processed.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Reporting.Tests" )]
        [TestProperty( "Feature", TestFeatures.Reporting )]
        public void DataView_AllDataViews_CanExecute()
        {
            var dataContext = new RockContext();
            var dataViewService = new DataViewService( dataContext );
            var dataViewIdList = dataViewService.Queryable()
                .Select( dv => dv.Id )
                .ToList();

            int dataViewTotal = dataViewIdList.Count;
            int dataViewCount = 0;
            var exceptions = new List<Exception>();

            foreach ( var dataViewId in dataViewIdList )
            {
                dataContext = new RockContext();
                dataViewService = new DataViewService( dataContext );

                var dataView = dataViewService.Get( dataViewId );

                dataViewCount++;
                LogHelper.Log( $"Evaluating Data View \"{dataView.Name}\"... ({dataViewCount} of {dataViewTotal})" );

                try
                {
                    var args = new GetQueryableOptions
                    {
                        DatabaseTimeoutSeconds = 300,
                        DbContext = dataContext
                    };
                    var query = DataViewQueryBuilder.Instance.GetDataViewQuery( dataView, args );

                    var stopwatch = Stopwatch.StartNew();
                    var results = query.Select( x => x.Id ).ToList();
                    stopwatch.Stop();

                    LogHelper.Log( $"Data View processed. [ItemCount={results.Count}, Time={stopwatch.Elapsed.TotalSeconds:0.00}s]" );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( new Exception( $"Data View failed. [Name={dataView.Name}]", ex ) );

                    LogHelper.LogError( $"Data View failed. {ex.Message}" );
                }
            }

            if ( exceptions.Any() )
            {
                throw new AggregateException( "One or more Data Views failed to process.", exceptions );
            }
        }

        /// <summary>
        /// Verify that all Data Views can be processed.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Reporting.Tests" )]
        [TestProperty( "Feature", TestFeatures.Reporting )]
        public void DataViewCache_AllDataViews_CanExecute()
        {
            var dataContext = new RockContext();
            var manager = DataViewQueryBuilder.Instance;

            // Get the complete list of DataView identifiers in the current database.
            var dataViewService = new DataViewService( dataContext );
            var dataViewIdList = dataViewService.Queryable()
                .Select( dv => dv.Id )
                .ToList();

            int dataViewTotal = dataViewIdList.Count;
            int dataViewCount = 0;
            var exceptions = new List<Exception>();

            foreach ( var dataViewId in dataViewIdList )
            {
                dataContext = new RockContext();
                dataViewService = new DataViewService( dataContext );

                var dataView = manager.GetDataViewDefinition( dataViewId );

                dataViewCount++;
                LogHelper.Log( $"Evaluating Data View \"{dataView.Name}\"... ({dataViewCount} of {dataViewTotal})" );

                try
                {
                    var args = new GetQueryableOptions
                    {
                        DatabaseTimeoutSeconds = 300,
                        DbContext = dataContext
                    };
                    var query = manager.GetDataViewQuery( dataView, args );

                    var stopwatch = Stopwatch.StartNew();
                    var results = query.Select( x => x.Id ).ToList();
                    stopwatch.Stop();
                    LogHelper.Log( $"Data View processed. [ItemCount={results.Count}, Time={stopwatch.Elapsed.TotalSeconds:0.00}s]" );
                }
                catch ( Exception ex )
                {
                    exceptions.Add( new Exception( $"Data View failed. [Name={dataView.Name}]", ex ) );
                    LogHelper.LogError( $"Data View failed. {ex.Message}" );
                }
            }
            if ( exceptions.Any() )
            {
                throw new AggregateException( "One or more Data Views failed to process.", exceptions );
            }
        }

        /// <summary>
        /// Verify that various sample Data Views return the expected number of records.
        /// </summary>
        [TestMethod]
        public void DataView_KnownDataViews_ReturnExpectedRowCounts()
        {
            VerifyDataViewResultSetCount( TestGuids.DataViews.AdultMembersAndAttendees, 28 );
            VerifyDataViewResultSetCount( TestGuids.DataViews.AdultMembersAndAttendeesMales, 16 );
            VerifyDataViewResultSetCount( TestGuids.DataViews.AdultMembersAndAttendeesFemales, 11 );
            VerifyDataViewResultSetCount( TestGuids.DataViews.MembersAndAttendees, 34 );

            VerifyDataViewResultSetCount( TestGuids.DataViews.Males, 34 );
            VerifyDataViewResultSetCount( TestGuids.DataViews.Females, 19 );

            // These Data Views require that the RockCleanup Job has been executed in the sample database.
            VerifyDataViewResultSetCount( TestGuids.DataViews.UnderThirtyFive, 14 );
            VerifyDataViewResultSetCount( TestGuids.DataViews.ThirtyFiveAndOlder, 35 );
        }

        [TestMethod]
        public void DataViewCache_WhenDataViewIsModified_CacheIsUpdated()
        {
            // Load a sample Data View into cache and store a property setting.
            var dataViewCacheMales1 = DataViewCache.Get( TestGuids.DataViews.Males );

            // Get the sample Data View definition from the database.
            var dataContext = new RockContext();
            var dataViewService = new DataViewService( dataContext );
            var dataViewMales = dataViewService.Get( TestGuids.DataViews.Males );
            var descriptionOld = dataViewMales.Description;

            // Modify a Data View property and save the changes.
            var descriptionNew = $"Test Description ({Guid.NewGuid()})";
            dataViewMales.Description = descriptionNew;

            dataContext.SaveChanges();

            // Get the Data View from cache and verify that the filter settings have been updated.
            var dataViewCacheMales2 = DataViewCache.Get( TestGuids.DataViews.Males );
            var propertyValueUpdated = dataViewCacheMales2.Description;

            // Restore the original properties of the sample Data View.
            dataViewMales.Description = descriptionOld;
            dataContext.SaveChanges();

            Assert.That.AreEqual( descriptionNew,
                propertyValueUpdated,
                "Cached Data View not updated." );
            Assert.That.IsTrue( dataViewCacheMales2.ModifiedDateTime > ( dataViewCacheMales1.ModifiedDateTime ?? DateTime.MinValue ),
                "Cached Data View Date Modified is invalid." );
        }

        [TestMethod]
        public void DataViewFilterCache_WhenDataViewFilterIsModified_CacheIsUpdated()
        {
            // Load a sample Data View into cache and store the settings for the first filter.
            var dataViewCacheAge1 = DataViewCache.Get( TestGuids.DataViews.ThirtyFiveAndOlder );
            var selectionOld = dataViewCacheAge1.DataViewFilter?.ChildFilters?.FirstOrDefault()?.Selection;

            // Get the sample Data View definition from the database.
            var dataContext = new RockContext();
            var dataViewService = new DataViewService( dataContext );

            var dataViewAge = dataViewService.Get( TestGuids.DataViews.ThirtyFiveAndOlder );
            var dataViewFilterAge = dataViewAge.DataViewFilter.ChildFilters?.FirstOrDefault();

            // Modify the filter settings and save the changes.
            // These settings are invalid, but sufficient for test purposes.
            var selectionNew = $"TestSelection;{Guid.NewGuid()}]";

            dataViewFilterAge.Selection = selectionNew;

            dataContext.SaveChanges();

            // Get the Data View from cache and verify that the filter settings have been updated.
            var dataViewCacheAge2 = DataViewCache.Get( TestGuids.DataViews.ThirtyFiveAndOlder );
            var dataViewCacheAgeFilter = dataViewCacheAge2.DataViewFilter.ChildFilters.FirstOrDefault();
            var selectionUpdated = dataViewCacheAgeFilter.Selection;

            // Restore the original properties of the sample Data View.
            dataViewFilterAge.Selection = selectionOld;
            dataContext.SaveChanges();

            Assert.That.AreEqual( selectionNew, selectionUpdated, "Cached Data View Filter not updated." );
        }

        /// <summary>
        /// Verify that a Data View Filter with no parent Data View can be used as the source of a Rock Entity query.
        /// Filters of this form are used in Rock to select content for some features, such as for viewing subsets of Content Channels
        /// and Registration Instance Group Placements.
        /// </summary>
        [TestMethod]
        public void DataViewFilterCache_DataViewFilterWithNoParentDataView_CanExecute()
        {
            var manager = DataViewQueryBuilder.Instance;

            var filter = manager.GetDataViewFilterDefinition( TestGuids.DataFilters.ContentChannel.AsGuid() );

            VerifyDataFilterResultSetCount( filter.Guid.ToString(), typeof( Rock.Model.ContentChannelItem ), 4 );
        }

        private void VerifyDataViewResultSetCount( string dataViewGuid, int expectedCount )
        {
            var dataContext = new RockContext();
            var manager = DataViewQueryBuilder.Instance;

            var dataView = manager.GetDataViewDefinition( dataViewGuid.AsGuid() );

            LogHelper.Log( $"Evaluating Data View \"{dataView.Name}\"..." );

            var resultCount = 0;

            try
            {
                var args = new GetQueryableOptions
                {
                    DatabaseTimeoutSeconds = 300,
                    DbContext = dataContext,
                    DataViewFilterOverrides = new DataViewFilterOverrides { ShouldUpdateStatics = false }
                };
                var query = manager.GetDataViewQuery( dataView, args );

                var stopwatch = Stopwatch.StartNew();
                var results = query.Select( x => x.Id ).ToList();
                stopwatch.Stop();

                resultCount = results.Count;
                if ( resultCount != expectedCount )
                {
                    throw new Exception( $"Invalid result count [Expected={expectedCount}, Actual={resultCount}]" );
                }

                LogHelper.Log( $"Data View processed. [ItemCount={results.Count}, Time={stopwatch.Elapsed.TotalSeconds:0.00}s]" );
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Data View failed. [Name={dataView.Name}]", ex );
            }
        }

        private void VerifyDataFilterResultSetCount( string dataFilterGuid, Type resultEntityType, int expectedCount )
        {
            var dataContext = new RockContext();
            var manager = DataViewQueryBuilder.Instance;

            var filter = manager.GetDataViewFilterDefinition( dataFilterGuid.AsGuid() );

            LogHelper.Log( $"Evaluating Data Filter \"{filter.Id}\"..." );

            var resultCount = 0;

            try
            {
                var args = new GetQueryableOptions
                {
                    DatabaseTimeoutSeconds = 300,
                    DbContext = dataContext,
                    DataViewFilterOverrides = new DataViewFilterOverrides { ShouldUpdateStatics = false }
                };
                var query = manager.GetDataViewFilterQuery( filter, resultEntityType, args );

                var stopwatch = Stopwatch.StartNew();
                var results = query.Select( x => x.Id ).ToList();
                stopwatch.Stop();

                resultCount = results.Count;
                if ( resultCount != expectedCount )
                {
                    throw new Exception( $"Invalid result count [Expected={expectedCount}, Actual={resultCount}]" );
                }

                LogHelper.Log( $"Data View processed. [ItemCount={results.Count}, Time={stopwatch.Elapsed.TotalSeconds:0.00}s]" );
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Data Filter failed. [Guid={filter.Guid}]", ex );
            }
        }

        #endregion
    }
}
