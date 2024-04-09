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
using System.Linq.Expressions;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a DataView that can be added to a short-term cache.
    /// </summary>
    [Serializable]
    [DataContract]
    public class DataViewCache : ModelCache<DataViewCache, DataView>, IDataViewDefinition
    {
        #region Fields

        /// <summary>
        /// The last time this cache object ran the query from cache rather
        /// than building the expression. We use this to update the last run
        /// value of the data view occasionally.
        /// </summary>
        private DateTime _lastCachedRun = DateTime.MinValue;

        /// <summary>
        /// The persisted entity id values that have been cached for the
        /// DataView. This will be <c>null</c> until the values are loaded
        /// from the database or if the DataView is not persisted.
        /// </summary>
        private IReadOnlyCollection<int> _persistedEntityIds;

        #endregion

        #region Properties

        /// <inheritdoc cref="Rock.Model.DataView.IsSystem" />
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.Name" />
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.Description" />
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.CategoryId" />
        [DataMember]
        public int? CategoryId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.EntityTypeId" />
        [DataMember( IsRequired = true )]
        public int? EntityTypeId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.DataViewFilterId" />
        [DataMember]
        public int? DataViewFilterId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.TransformEntityTypeId" />
        [DataMember]
        public int? TransformEntityTypeId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.PersistedScheduleIntervalMinutes" />
        [DataMember]
        public int? PersistedScheduleIntervalMinutes { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.PersistedLastRefreshDateTime" />
        [DataMember]
        public DateTime? PersistedLastRefreshDateTime { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.IncludeDeceased" />
        [DataMember]
        public bool IncludeDeceased { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.PersistedLastRunDurationMilliseconds" />
        [DataMember]
        public int? PersistedLastRunDurationMilliseconds { get; private set; }

        /// <summary>
        /// <para>
        /// Gets the last run date time. This is only updated when the
        /// data view is actually executed, not when we access the persisted
        /// values directly.
        /// </para>
        /// <para>
        /// This value may be slightly out of sync with the database so it
        /// should not be used for critical logic decisions.
        /// </para>
        /// </summary>
        /// <value>
        /// The last run date time.
        /// </value>
        [DataMember]
        public DateTime? LastRunDateTime { get; private set; }

        /// <summary>
        /// <para>
        /// Gets the run count. This is only updated when the data view
        /// is actually executed, not when we access the persisted values directly.
        /// </para>
        /// <para>
        /// This value may be slightly out of sync with the database so it
        /// should not be used for critical logic decisions.
        /// </para>
        /// </summary>
        /// <value>
        /// The run count.
        /// </value>
        [DataMember]
        public int? RunCount { get; private set; }

        /// <summary>
        /// <para>
        /// Gets the amount of time in milliseconds that it took to run the
        /// <see cref="DataView"/>. This is only updated when the data view
        /// is actually executed, not when we access the persisted values directly.
        /// </para>
        /// <para>
        /// This value may be slightly out of sync with the database so it
        /// should not be used for critical logic decisions.
        /// </para>
        /// </summary>
        /// <value>
        /// The time to run in ms.
        /// </value>
        [DataMember]
        public double? TimeToRunDurationMilliseconds { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.RunCountLastRefreshDateTime" />
        [DataMember]
        public DateTime? RunCountLastRefreshDateTime { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.DisableUseOfReadOnlyContext" />
        [DataMember]
        public bool DisableUseOfReadOnlyContext { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.PersistedScheduleId" />
        [DataMember]
        public int? PersistedScheduleId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.IconCssClass"/>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataView.HighlightColor"/>
        [DataMember]
        public string HighlightColor { get; private set; }

        #endregion

        #region Navigation Properties

        /// <inheritdoc cref="Rock.Model.DataView.Category" />
        public CategoryCache Category => CategoryId.HasValue ? CategoryCache.Get( CategoryId.Value ) : null;

        /// <inheritdoc cref="Rock.Model.DataView.EntityType" />
        public EntityTypeCache EntityType => EntityTypeId.HasValue ? EntityTypeCache.Get( EntityTypeId.Value ) : null;

        /// <inheritdoc cref="Rock.Model.DataView.DataViewFilter" />
        public DataViewFilterCache DataViewFilter => DataViewFilterId.HasValue ? DataViewFilterCache.Get( DataViewFilterId.Value ) : null;

        /// <inheritdoc cref="DataView.PersistedSchedule"/>
        public NamedScheduleCache PersistedSchedule => PersistedScheduleId.HasValue ? NamedScheduleCache.Get( PersistedScheduleId.Value ) : null;

        /// <inheritdoc cref="DataView.TransformEntityType"/>
        public EntityTypeCache TransformEntityType => TransformEntityTypeId.HasValue ? EntityTypeCache.Get( TransformEntityTypeId.Value ) : null;

        #endregion

        #region IDataViewDefinition implementation

        /// <inheritdoc/>
        IDataViewFilterDefinition IDataViewDefinition.DataViewFilter => this.DataViewFilter;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if this DataView is configured to be Persisted.
        /// </summary>
        /// <returns><c>true</c> if this instance is persisted; otherwise, <c>false</c>.</returns>
        public bool IsPersisted()
        {
            return this.PersistedScheduleIntervalMinutes.HasValue || this.PersistedScheduleId.HasValue;
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is DataView dataView ) )
            {
                return;
            }

            CategoryId = dataView.CategoryId;
            DataViewFilterId = dataView.DataViewFilterId;
            Description = dataView.Description;
            DisableUseOfReadOnlyContext = dataView.DisableUseOfReadOnlyContext;
            EntityTypeId = dataView.EntityTypeId;
            HighlightColor = dataView.HighlightColor;
            IconCssClass = dataView.IconCssClass;
            IncludeDeceased = dataView.IncludeDeceased;
            IsSystem = dataView.IsSystem;
            LastRunDateTime = dataView.LastRunDateTime;
            Name = dataView.Name;
            PersistedLastRefreshDateTime = dataView.PersistedLastRefreshDateTime;
            PersistedLastRunDurationMilliseconds = dataView.PersistedLastRunDurationMilliseconds;
            PersistedScheduleId = dataView.PersistedScheduleId;
            PersistedScheduleIntervalMinutes = dataView.PersistedScheduleIntervalMinutes;
            RunCount = dataView.RunCount;
            RunCountLastRefreshDateTime = dataView.RunCountLastRefreshDateTime;
            TimeToRunDurationMilliseconds = dataView.TimeToRunDurationMilliseconds;
            TransformEntityTypeId = dataView.TransformEntityTypeId;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns>A queryable that contains the entities returned by the filters.</returns>
        public IQueryable<IEntity> GetQuery()
        {
            return GetQuery( new GetQueryableOptions() );
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="options">The data view get query arguments.</param>
        /// <returns>A queryable that contains the entities returned by the filters.</returns>
        public IQueryable<IEntity> GetQuery( GetQueryableOptions options )
        {
            options = options ?? new GetQueryableOptions();

            if ( IsPersisted() && PersistedLastRefreshDateTime.HasValue )
            {
                if ( options.DataViewFilterOverrides?.ShouldUpdateStatics != false )
                {
                    MaybeUpdateLastRunDate();
                }

                var entityType = EntityType?.GetEntityType();

                if ( entityType == null )
                {
                    throw new InvalidOperationException( $"Unknown entity type for DataView #{Id}: {Name}." );
                }

                var rockContext = options.DbContext
                    ?? ( DisableUseOfReadOnlyContext ? new RockContext() : new RockContextReadOnly() );

                var entityIdQry = rockContext.Set<DataViewPersistedValue>()
                    .Where( pv => pv.DataViewId == Id )
                    .Select( pv => pv.EntityId );

                var qry = Reflection.GetQueryableForEntityType( entityType, ( DbContext ) rockContext );

                qry = qry.Where( a => entityIdQry.Contains( a.Id ) );
                qry = GetSortedQueryable( qry, options.SortProperty );

                return qry;
            }
            else
            {
                return DataViewQueryBuilder.Instance.GetDataViewQuery( this, options );
            }
        }

        /// <summary>
        /// Gets the expression that is used by the data view to filter results.
        /// </summary>
        /// <param name="serviceInstance">The service instance used to query the database.</param>
        /// <param name="paramExpression">The parameter expression that will be used to identify the entity when building the expression.</param>
        /// <returns>An expression that can be used to filter a queryable.</returns>
        public Expression GetExpression( IService serviceInstance, ParameterExpression paramExpression )
        {
            return this.GetExpression( serviceInstance, paramExpression, null );
        }

        /// <summary>
        /// Gets the expression that is used by the data view to filter results.
        /// </summary>
        /// <param name="serviceInstance">The service instance used to query the database.</param>
        /// <param name="paramExpression">The parameter expression that will be used to identify the entity when building the expression.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <returns>An expression that can be used to filter a queryable.</returns>
        public Expression GetExpression( IService serviceInstance, ParameterExpression paramExpression, DataViewFilterOverrides dataViewFilterOverrides )
        {
            return DataViewQueryBuilder.Instance
                .GetDataViewExpression( this, serviceInstance, paramExpression, dataViewFilterOverrides );
        }

        /// <summary>
        /// Gets the entity identifiers represented by the DataView filters.
        /// This will automatically use the persisted values if they are
        /// configured and available. A new <see cref="RockContext"/> will be
        /// created to query the database if the values are not already cached.
        /// </summary>
        /// <returns>A read-only collection of identifiers.</returns>
        public IReadOnlyCollection<int> GetEntityIds()
        {
            return GetEntityIds( new GetQueryableOptions() );
        }

        /// <summary>
        /// Gets the entity identifiers represented by the DataView filters.
        /// This will automatically use the persisted values if they are
        /// configured and available.
        /// </summary>
        /// <param name="options">The options to use if access to the database is required.</param>
        /// <returns>A read-only collection of identifiers.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public IReadOnlyCollection<int> GetEntityIds( GetQueryableOptions options )
        {
            options = options ?? new GetQueryableOptions();

            if ( IsPersisted() && PersistedLastRefreshDateTime.HasValue )
            {
                if ( options.DataViewFilterOverrides?.ShouldUpdateStatics != false )
                {
                    MaybeUpdateLastRunDate();
                }

                if ( _persistedEntityIds != null )
                {
                    return _persistedEntityIds;
                }

                bool ownsContext = options.DbContext == null;
                var rockContext = options.DbContext ?? new RockContext();

                var idQry = rockContext.Set<DataViewPersistedValue>()
                    .Where( pv => pv.DataViewId == Id )
                    .Select( pv => pv.EntityId );

                _persistedEntityIds = new HashSet<int>( idQry );

                if ( ownsContext )
                {
                    rockContext.Dispose();
                }

                return _persistedEntityIds;
            }
            else
            {
                return DataViewQueryBuilder.Instance.GetDataViewQuery( this, options )
                    .Select( a => a.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the sorted queryable for the specified sort property. This is
        /// used when we are querying the persisted values.
        /// </summary>
        /// <param name="qry">The query to be sorted.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns>A new queryable that has the sorting applied.</returns>
        private IQueryable<IEntity> GetSortedQueryable( IQueryable<IEntity> qry, Rock.Web.UI.Controls.SortProperty sortProperty )
        {
            if ( sortProperty == null )
            {
                // if no sorting is specified, just sort by Id
                sortProperty = new Rock.Web.UI.Controls.SortProperty
                {
                    Direction = System.Web.UI.WebControls.SortDirection.Ascending,
                    Property = "Id"
                };
            }

            var entityType = EntityType.GetEntityType();
            var qryType = typeof( IQueryable<> ).MakeGenericType( entityType );
            var castMethod = typeof( Queryable ).GetMethod( nameof( Queryable.Cast ) ).MakeGenericMethod( entityType );
            var entityQueryable = castMethod.Invoke( null, new object[] { qry } );

            var orderName = sortProperty.Direction == System.Web.UI.WebControls.SortDirection.Ascending
                ? nameof( ExtensionMethods.OrderBy )
                : nameof( ExtensionMethods.OrderByDescending );

            var orderMethod = typeof( ExtensionMethods )
                .GetMethods()
                .Where( m => m.Name == orderName && m.GetParameters().Length == 2 )
                .First();

            return ( IQueryable<IEntity> ) orderMethod
                .MakeGenericMethod( entityType )
                .Invoke( null, new object[] { entityQueryable, sortProperty.Property } );
        }

        /// <summary>
        /// Update the last run date on the data view (via transaction) if it
        /// has not been updated recently.
        /// </summary>
        private void MaybeUpdateLastRunDate()
        {
            // If it has been less than ten minutes since we last updated
            // then don't do anything.
            if ( _lastCachedRun.AddMinutes( 10 ) > RockDateTime.Now )
            {
                return;
            }

            _lastCachedRun = RockDateTime.Now;
            DataViewService.AddRunDataViewTransaction( Id );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
