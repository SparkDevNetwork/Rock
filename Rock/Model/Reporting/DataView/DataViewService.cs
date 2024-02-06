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
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Data.SqlClient;
using System.Linq;
using EF6.TagWith;
using Microsoft.SqlServer.Types;
using Rock.Data;
using Rock.Logging;
using Rock.Reporting.DataFilter;
using Rock.SystemKey;
using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// DataView Service and Data access class
    /// </summary>
    public partial class DataViewService
    {
        /// <summary>
        /// Gets a boolean of whether a read-only replica of the database context was enabled.
        /// </summary>
        public bool ReadOnlyContextEnabled
        {
            get
            {
                var rockContext = RockInstanceConfig.Database.ConnectionString;
                var readOnlyContext = RockInstanceConfig.Database.ReadOnlyConnectionString;

                if ( rockContext != null && readOnlyContext != null && !rockContext.Equals( readOnlyContext ) )
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.EntityType">EntityTypes</see> that have a DataView associated with them.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="Rock.Model.EntityType">EntityTypes</see> that have a <see cref="Rock.Model.DataView" /> associated with them.</returns>
        public IQueryable<Rock.Model.EntityType> GetAvailableEntityTypes()
        {
            return Queryable()
                .Select( d => d.EntityType )
                .Distinct();
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.DataView">DataViews</see> that are associated with a specified <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.DataView">DataViews</see> that are associated with the specified <see cref="Rock.Model.EntityType"/>.</returns>
        public IQueryable<Rock.Model.DataView> GetByEntityTypeId( int entityTypeId )
        {
            return Queryable()
                .Where( d => d.EntityTypeId == entityTypeId )
                .OrderBy( d => d.Name );
        }

        /// <summary>
        /// Gets the ids.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns></returns>
        public List<int> GetIds( int dataViewId )
        {
            var dataView = Queryable().AsNoTracking().FirstOrDefault( d => d.Id == dataViewId );
            var dataViewGetQueryArgs = new DataViewGetQueryArgs
            {
                DatabaseTimeoutSeconds = 180,
            };

            return dataView.GetQuery( dataViewGetQueryArgs ).Select( a => a.Id ).ToList();
        }

        /// <summary>
        /// Determines whether the specified Data View forms part of a filter.
        /// </summary>
        /// <param name="dataViewId">The unique identifier of a Data View.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        ///   <c>true</c> if the specified Data View forms part of the conditions for the specified filter.
        /// </returns>
        public bool IsViewInFilter( int dataViewId, DataViewFilter filter )
        {
            var dataView = Get( dataViewId );
            return IsViewInFilter( dataView, filter );
        }

        /// <summary>
        /// Determines whether [is view in filter] [the specified data view].
        /// </summary>
        /// <param name="dataView">The data view.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        ///   <c>true</c> if [is view in filter] [the specified data view]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsViewInFilter( DataView dataView, DataViewFilter filter )
        {
            if ( filter.EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( filter.EntityTypeId.Value );
                var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                if ( component is OtherDataViewFilter otherDataViewFilter )
                {
                    var otherDataView = otherDataViewFilter.GetSelectedDataView( filter.Selection );
                    if ( otherDataView == null )
                    {
                        return false;
                    }

                    if ( otherDataView.Id == dataView.Id )
                    {
                        // if we discover that this DataView is also used in one of its child views, we've got infinite recursion
                        return true;
                    }
                    else
                    {
                        // dig down recursively thru the *other* DataView's child filters to see if any of it's child filters is using this dataview
                        return IsViewInFilter( dataView, otherDataView.DataViewFilter );
                    }
                }
            }

            foreach ( var childFilter in filter.ChildFilters )
            {
                // dig down recursively thru *this* DataView's child filters 
                if ( IsViewInFilter( dataView, childFilter ) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Create a new non-persisted Data View using an existing Data View as a template. 
        /// </summary>
        /// <param name="dataViewId">The identifier of a Data View to use as a template for the new Data View.</param>
        /// <returns></returns>
        public DataView GetNewFromTemplate( int dataViewId )
        {
            var item = this.Queryable()
                           .AsNoTracking()
                           .Include( x => x.DataViewFilter )
                           .FirstOrDefault( x => x.Id == dataViewId );

            if ( item == null )
            {
                throw new Exception( string.Format( "GetNewFromTemplate method failed. Template Data View ID \"{0}\" could not be found.", dataViewId ) );
            }

            // Deep-clone the Data View and reset the properties that connect it to the permanent store.
            var newItem = ( DataView ) item.Clone( true );

            newItem.Id = 0;
            newItem.Guid = Guid.NewGuid();
            newItem.ForeignId = null;
            newItem.ForeignGuid = null;
            newItem.ForeignKey = null;

            newItem.DataViewFilterId = 0;

            this.ResetPermanentStoreIdentifiers( newItem.DataViewFilter );

            return newItem;
        }

        /// <summary>
        /// Gets the data views referenced by this data view's filters.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public List<DataView> GetReferencedDataViews( int dataViewId, RockContext context )
        {
            var dataViewFilterService = new DataViewFilterService( context );

            var relatedDataViews = GetDistinctRelatedDataViews( dataViewId, dataViewFilterService )
                .ToDictionary( dvf => dvf.RelatedDataView.Id, dvf => dvf.RelatedDataView );

            var relatedDataViewIds = relatedDataViews.Keys.ToList();
            for ( var i = 0; i < relatedDataViewIds.Count; i++ )
            {
                var key = relatedDataViewIds[i];
                var relatedDataView = relatedDataViews[key];

                var relatedChildDataViews = GetDistinctRelatedDataViews( dataViewId, dataViewFilterService )
                    .Select( dvf => dvf.RelatedDataView )
                    .ToList();

                foreach ( var dv in relatedChildDataViews )
                {
                    if ( relatedDataViewIds.Contains( dv.Id ) )
                    {
                        continue;
                    }

                    relatedDataViewIds.Add( dv.Id );
                    relatedDataViews[dv.Id] = dv;
                }
            }

            return relatedDataViews.Values.ToList();
        }

        private IEnumerable<DataViewFilter> GetDistinctRelatedDataViews( int dataViewId, DataViewFilterService dataViewFilterService )
        {
            return dataViewFilterService
                            .Queryable()
                            .Where( dvf => dvf.DataViewId != null && dvf.DataViewId == dataViewId && dvf.RelatedDataViewId != null )
                            .Include( "RelatedDataView" )
                            .DistinctBy( dvf => dvf.RelatedDataView.Id );
        }

        #region Static Methods

        /// <summary>
        /// Adds AddRunDataViewTransaction to transaction queue
        /// </summary>
        /// <param name="dataViewId">The unique identifier of a Data View.</param>
        /// <param name="timeToRunDurationMilliseconds">The time to run dataview in milliseconds.</param>
        /// <param name="persistedLastRunDurationMilliseconds">The time to persist dataview in milliseconds.</param>
        public static void AddRunDataViewTransaction( int dataViewId, int? timeToRunDurationMilliseconds = null, int? persistedLastRunDurationMilliseconds = null )
        {
            var dataViewInfo = new Rock.Transactions.UpdateDataViewStatisticsTransaction.DataViewInfo()
            {
                DataViewId = dataViewId,
                LastRunDateTime = RockDateTime.Now,
                ShouldIncrementRunCount = true
            };

            if ( timeToRunDurationMilliseconds.HasValue )
            {
                RockLogger.Log.Debug( RockLogDomains.Reporting, "{methodName} dataViewId: {dataViewId} timeToRunDurationMilliseconds: {timeToRunDurationMilliseconds}", nameof( AddRunDataViewTransaction ), dataViewId, timeToRunDurationMilliseconds );
                dataViewInfo.TimeToRunDurationMilliseconds = timeToRunDurationMilliseconds;
                /*
                 * If the run duration is set that means this was called after the expression was
                 * already evaluated, which in turn already counted the run so we don't want to double count it here.
                 */
                dataViewInfo.ShouldIncrementRunCount = false;
            }

            var transaction = new Rock.Transactions.UpdateDataViewStatisticsTransaction( dataViewInfo );
            transaction.Enqueue();
        }

        #endregion Static Methods

        /// <summary>
        /// Reset all of the identifiers on a DataViewFilter that uniquely identify it in the permanent store.
        /// </summary>
        /// <param name="filter">The data view filter.</param>
        private void ResetPermanentStoreIdentifiers( DataViewFilter filter )
        {
            if ( filter == null )
            {
                return;
            }

            filter.Id = 0;
            filter.Guid = Guid.NewGuid();
            filter.ForeignId = null;
            filter.ForeignGuid = null;
            filter.ForeignKey = null;

            // Recursively reset any contained filters.
            foreach ( var childFilter in filter.ChildFilters )
            {
                this.ResetPermanentStoreIdentifiers( childFilter );
            }
        }

        /// <summary>
        /// Gets a List of <see cref="Rock.Model.DataViewPersistedValue"/>
        /// matching the specified list of <paramref name="entityIds"/> and
        /// (optionally) <paramref name="dataViewIds"/>.
        /// </summary>
        /// <param name="entityIds">The list of EntityIds to filter to.</param>
        /// <param name="dataViewIds">The (optional) list of DataViewIds to filter to.</param>
        /// <returns>A List of <see cref="Rock.Model.DataViewPersistedValue"/></returns>
        public List<DataViewPersistedValue> GetDataViewPersistedValuesForIds( IEnumerable<int> entityIds, IEnumerable<int> dataViewIds = null )
        {
            var entityIdsParamName = "@EntityIds";
            var entityIdsParam = entityIds.ConvertToEntityIdListParameter( entityIdsParamName );
            var sql = $@"
                SELECT [DataViewId]
                    , [EntityId]
                FROM [DataViewPersistedValue] [dv]
                JOIN {entityIdsParamName} [e] ON [dv].EntityId = [e].Id";

            if ( dataViewIds != null && dataViewIds.Any() )
            {
                var dataViewIdsParamName = "@DataViewIds";
                // If there's also a filter condition for dataViewIds add the condition and return the result.
                var dataViewIdsParam = dataViewIds.ConvertToEntityIdListParameter( dataViewIdsParamName );
                sql += $@"
                JOIN {dataViewIdsParamName} [d] ON [dv].[DataViewId] = [d].Id";
                return this.Context.Database.SqlQuery<DataViewPersistedValue>( sql, entityIdsParam, dataViewIdsParam ).ToList();
            }

            // Return the list based solely on EntityIds filtering.
            return this.Context.Database.SqlQuery<DataViewPersistedValue>( sql, entityIdsParam ).ToList();
        }

        /// <summary>
        /// Updates the data view's <see cref="DataViewPersistedValue"/>s in the database.
        /// </summary>
        /// <param name="dataViewId">The identifier of the data view whose values should be updated.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout in seconds.</param>
        internal void UpdateDataViewPersistedValues( int dataViewId, int? databaseTimeoutSeconds = null )
        {
            UpdateDataViewPersistedValues( Get( dataViewId ), databaseTimeoutSeconds );
        }

        /// <summary>
        /// Updates the data view's <see cref="DataViewPersistedValue"/>s in the database.
        /// </summary>
        /// <param name="dataView">The data view whose values should be updated.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout in seconds.</param>
        internal void UpdateDataViewPersistedValues( DataView dataView, int? databaseTimeoutSeconds = null )
        {
            if ( dataView == null )
            {
                return;
            }

            // Get the dynamic query to be used to source the entity IDs for this data view. Set an override so that any existing
            // persisted values aren't used when rebuilding the values from the data view query.
            var overrides = new DataViewFilterOverrides { ShouldUpdateStatics = false };
            overrides.IgnoreDataViewPersistedValues.Add( dataView.Id );

            var dataViewObjectQuery = dataView
                .GetQuery( new DataViewGetQueryArgs
                {
                    DbContext = this.Context,
                    DataViewFilterOverrides = overrides,
                    DatabaseTimeoutSeconds = databaseTimeoutSeconds
                } )
                .Select( entity => entity.Id )
                .ToObjectQuery();

            if ( dataViewObjectQuery == null )
            {
                RockLogger.Log.WriteToLog( RockLogLevel.Error, $"{nameof( UpdateDataViewPersistedValues )}: Unable to update persisted values for data view with ID {dataView.Id}." );
                return;
            }

            var tagger = new SqlServerTagger();
            var taggedSql = tagger.GetTaggedSqlQuery( dataViewObjectQuery.ToTraceString(), new TaggingOptions { TagMode = TagMode.Prefix } );

            var tempTableName = $"DataView_{dataView.Id}";

            var sql = $@"
BEGIN TRY
    DROP TABLE IF EXISTS #{tempTableName};

    CREATE TABLE #{tempTableName}
    (
        [EntityId] [int] NOT NULL
    );

    -- Select the new entity IDs that should be added/remain in the persisted set.
    INSERT INTO #{tempTableName}
    {taggedSql};

    -- Delete any old Entity IDs that should no longer be in the persisted set.
    -- We'll delete these in batches to prevent locking the table.
    DECLARE @RowsAffected [int];
    WHILE @RowsAffected IS NULL OR @RowsAffected > 0
    BEGIN
        DELETE TOP (1500) [existing]
        FROM [DataViewPersistedValue] [existing]
        WHERE [existing].[DataViewId] = @DataViewId
            AND ( NOT EXISTS (
                    SELECT [EntityId]
                    FROM #{tempTableName}
                    WHERE [EntityId] = [existing].[EntityId]
                )
            );

        SET @RowsAffected = @@ROWCOUNT;
    END

    -- Add any missing Entity IDs that weren't already in the persisted set.
    INSERT INTO [DataViewPersistedValue]
    (
        [DataViewId]
        , [EntityId]
    )
    SELECT @DataViewId
        , [EntityId]
    FROM #{tempTableName} [new]
    WHERE NOT EXISTS (
        SELECT [EntityId]
        FROM [DataViewPersistedValue]
        WHERE [DataViewId] = @DataViewId
            AND [EntityId] = [new].[EntityId]
    );

    DROP TABLE #{tempTableName};
END TRY
BEGIN CATCH
    DROP TABLE IF EXISTS #{tempTableName};
END CATCH;";

            var parameters = new List<object> {
                new SqlParameter( "@DataViewId", dataView.Id )
            }
            .Concat(
                dataViewObjectQuery.Parameters.Select( objectParameter =>
                {
                    if ( objectParameter.Value is DbGeography geography )
                    {
                        // We need to manually convert DbGeography to SqlGeography since we're sidestepping EF here.
                        // https://stackoverflow.com/a/45099842 (Use SqlGeography instead of DbGeography)
                        // https://stackoverflow.com/a/23187033 (Entity Framework: SqlGeography vs DbGeography)
                        return new SqlParameter
                        {
                            ParameterName = objectParameter.Name,
                            Value = SqlGeography.Parse( geography.AsText() ),
                            SqlDbType = SqlDbType.Udt,
                            UdtTypeName = "geography"
                        };
                    }

                    return new SqlParameter( objectParameter.Name, objectParameter.Value ?? DBNull.Value );
                } )
            )
            .ToArray();

            this.Context.Database.ExecuteSqlCommand( TransactionalBehavior.DoNotEnsureTransaction, sql, parameters );
        }
    }
}
