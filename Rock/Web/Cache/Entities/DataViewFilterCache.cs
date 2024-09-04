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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;

namespace Rock.Web.Cache
{
    /// <summary>
    /// A cache representation of a DataViewFilter.
    /// </summary>
    [Serializable]
    [DataContract]
    public class DataViewFilterCache : ModelCache<DataViewFilterCache, DataViewFilter>, IDataViewFilterDefinition
    {
        #region Fields

        /// <summary>
        /// Tracks the cached "all item ids" lists per data view.
        /// </summary>
        private static readonly AlternateIdListCache<DataViewFilterCache, int> _byParentIdCache = new AlternateIdListCache<DataViewFilterCache, int>( "parent" );

        #endregion

        #region Properties

        /// <inheritdoc cref="Rock.Model.DataViewFilter.ExpressionType" />
        [DataMember]
        public FilterExpressionType ExpressionType { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataViewFilter.ParentId" />
        [DataMember]
        public int? ParentId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataViewFilter.EntityTypeId" />
        [DataMember]
        public int? EntityTypeId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataViewFilter.Selection" />
        [DataMember]
        public string Selection { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataViewFilter.DataViewId" />
        [DataMember]
        public int? DataViewId { get; private set; }

        /// <inheritdoc cref="Rock.Model.DataViewFilter.RelatedDataViewId" />
        [DataMember]
        public int? RelatedDataViewId { get; private set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets the child filters of this filter.
        /// </summary>
        /// <value>
        /// The child filters.
        /// </value>
        public List<DataViewFilterCache> ChildFilters => AllForParentId( Id );

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all cache objects that are children of the specified parent
        /// identifier.
        /// </summary>
        /// <param name="parentId">The parent filter identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A list of <see cref="DataViewFilterCache"/> objects.</returns>
        public static List<DataViewFilterCache> AllForParentId( int parentId, RockContext rockContext = null )
        {
            var keys = _byParentIdCache.GetOrAddKeys( parentId, pId =>
            {
                if ( rockContext != null )
                {
                    return QueryDbForParentId( pId, rockContext );
                }
                else
                {
                    using ( var newRockContext = new RockContext() )
                    {
                        return QueryDbForParentId( pId, newRockContext );
                    }
                }
            } );

            return GetMany( keys.AsIntegerList(), rockContext ).ToList();
        }

        /// <summary>
        /// Queries the database for all data view filter keys for the
        /// given parent identifier.
        /// </summary>
        /// <param name="parentId">The parent filter identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A collection of data view filter keys.</returns>
        private static List<string> QueryDbForParentId( int parentId, RockContext rockContext )
        {
            var service = new DataViewFilterService( rockContext );

            return service.Queryable()
                .AsNoTracking()
                .Where( dvf => dvf.ParentId == parentId )
                .Select( i => i.Id )
                .ToList()
                .ConvertAll( i => i.ToString() );
        }

        /// <summary>
        /// Sets the cached objects properties from the model/entity properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is DataViewFilter dataViewFilter ) )
            {
                return;
            }

            ExpressionType = dataViewFilter.ExpressionType;
            ParentId = dataViewFilter.ParentId;
            EntityTypeId = dataViewFilter.EntityTypeId;
            Selection = dataViewFilter.Selection;
            DataViewId = dataViewFilter.DataViewId;
            RelatedDataViewId = dataViewFilter.RelatedDataViewId;
        }

        /// <summary>
        /// Removes or invalidates the CachedItem based on EntityState
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entityState">State of the entity. If unknown, use <see cref="EntityState.Detached" /></param>
        public static new void UpdateCachedEntity( int entityId, EntityState entityState )
        {
            throw new NotSupportedException( "Do not call UpdateCachedEntity on DataViewFilterCache with an entity identifier." );
        }

        /// <summary>
        /// Removes or invalidates the DataViewFilterCache based on <paramref name="entityState"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityState">State of the entity. If unknown, use <see cref="EntityState.Detached" /></param>
        public static void UpdateCachedEntity( DataViewFilter entity, EntityState entityState )
        {
            if ( entityState == EntityState.Deleted )
            {
                Remove( entity );
            }
            else if ( entityState == EntityState.Added )
            {
                // add this entity to All Ids, but don't fetch it into cache until somebody asks for it
                AddToAllIds( entity );
            }
            else
            {
                FlushItem( entity.Id );
            }
        }

        /// <summary>
        /// This method is not supported on DataViewFilterCache, call the method
        /// that takes a <see cref="GroupLocation"/> parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        public static new void Remove( int key )
        {
            throw new NotSupportedException( "Do not call Remove on DataViewFilterCache with an entity identifier." );
        }

        /// <summary>
        /// This method is not supported on DataViewFilterCache, call the method
        /// that takes a <see cref="GroupLocation"/> parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        public static new void Remove( string key )
        {
            throw new NotSupportedException( "Do not call Remove on DataViewFilterCache with a cache key." );
        }

        /// <summary>
        /// Removes the related cache for the entity.
        /// </summary>
        /// <param name="entity">The entity whose cache data should be removed.</param>
        public static void Remove( DataViewFilter entity )
        {
            var key = entity.Id.ToString();

            ItemCache<DataViewFilterCache>.Remove( key );

            if ( entity.ParentId.HasValue )
            {
                _byParentIdCache.Remove( key, entity.ParentId.Value );
            }
        }

        /// <summary>
        /// Adds a new entity to the "all ids" lists.
        /// </summary>
        /// <param name="entity">The entity whose cache data should be added.</param>
        internal static void AddToAllIds( DataViewFilter entity )
        {
            var key = entity.Id.ToString();

            ItemCache<DataViewFilterCache>.AddToAllIds( key );

            if ( entity.ParentId.HasValue )
            {
                _byParentIdCache.Add( key, entity.ParentId.Value );
            }
        }

        /// <summary>
        /// Removes all items of this type from cache.
        /// </summary>
        public static new void Clear()
        {
            ItemCache<DataViewFilterCache>.Clear();
            _byParentIdCache.Clear();
        }

        /// <summary>
        /// Clears the by location cached lookup table for the specified
        /// location. This should be called whenever a DataViewFilterCache
        /// is added, removed, or modified in a way that would change
        /// the list of DataViewFilterCache child identifiers associated with
        /// the DataViewFilterCache.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        public static void ClearByParentId( int parentId )
        {
            _byParentIdCache.Clear( parentId );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"[{ExpressionType}] DataViewId={DataViewId}, Selection={Selection}";
        }

        /// <summary>
        /// Determines whether the specified action is authorized but instead of traversing child 
        /// filters (an expensive query), a list of all filters can be passed in and this will be 
        /// checked instead ( See DataViewPicker.LoadDropDownItems() for example of use ).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="allEntityFilters">All entity filters.</param>
        /// <returns></returns>
        public bool IsAuthorized( string action, Person person, List<DataViewFilterCache> allEntityFilters )
        {
            // First check if user is authorized for model
            bool authorized = base.IsAuthorized( action, person );

            if ( !authorized )
            {
                return false;
            }

            // If viewing, make sure user is authorized to view the component that filter is using
            // and all the child models/components
            if ( string.Compare( action, Rock.Security.Authorization.VIEW, true ) == 0 )
            {
                if ( EntityTypeId.HasValue )
                {
                    var filterComponent = Rock.Reporting.DataFilterContainer.GetComponent( EntityTypeCache.Get( this.EntityTypeId.Value )?.Name );
                    if ( filterComponent != null )
                    {
                        authorized = filterComponent.IsAuthorized( action, person );
                    }
                }

                if ( !authorized )
                {
                    return false;
                }

                // If there are no filters to evaluate return the current authorized value.
                if ( allEntityFilters.Count == 0 )
                {
                    return authorized;
                }

                foreach ( var childFilter in allEntityFilters.Where( f => f.ParentId == Id ) )
                {
                    if ( !childFilter.IsAuthorized( action, person, allEntityFilters ) )
                    {
                        return false;
                    }
                }
            }

            return authorized;
        }

        #endregion

        #region IDataViewFilterDefinition implementation

        /// <inheritdoc/>
        Guid? IDataViewFilterDefinition.Guid => this.Guid;

        /// <inheritdoc/>
        IDataViewDefinition IDataViewFilterDefinition.DataView => DataViewCache.Get( this.DataViewId.GetValueOrDefault() );

        /// <inheritdoc/>
        ICollection<IDataViewFilterDefinition> IDataViewFilterDefinition.ChildFilters => ChildFilters.Cast<IDataViewFilterDefinition>().ToList();

        #endregion
    }
}
