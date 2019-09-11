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
using System.Linq.Expressions;
using System.Reflection;

using Rock.Data;
using Rock.Reporting.DataFilter;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// DataView Service and Data access class
    /// </summary>
    public partial class DataViewService
    {
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
            if ( dataView != null && dataView.EntityTypeId.HasValue )
            {
                var cachedEntityType = EntityTypeCache.Get( dataView.EntityTypeId.Value );
                if ( cachedEntityType != null && cachedEntityType.AssemblyName != null )
                {
                    Type entityType = cachedEntityType.GetEntityType();

                    if ( entityType != null )
                    {
                        System.Data.Entity.DbContext reportDbContext = Reflection.GetDbContextForEntityType( entityType );
                        if ( reportDbContext != null )
                        {
                            reportDbContext.Database.CommandTimeout = 180;
                            IService serviceInstance = Reflection.GetServiceForEntityType( entityType, reportDbContext );
                            if ( serviceInstance != null )
                            {
                                var errorMessages = new List<string>();
                                ParameterExpression paramExpression = serviceInstance.ParameterExpression;
                                Expression whereExpression = dataView.GetExpression( serviceInstance, paramExpression, out errorMessages );

                                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ) } );
                                if ( getMethod != null )
                                {
                                    var getResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression } );
                                    var qry = getResult as IQueryable<IEntity>;

                                    return qry.Select( t => t.Id ).ToList();
                                }
                            }
                        }
                    }
                }
            }

            return null;
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
            var dataViewFilterEntityId = new EntityTypeService( (RockContext)this.Context ).Get( typeof( OtherDataViewFilter ), false, null ).Id;

            return IsViewInFilter( dataViewId, filter, dataViewFilterEntityId );
        }

        private bool IsViewInFilter( int dataViewId, DataViewFilter filter, int dataViewFilterEntityId )
        {
            if ( filter.EntityTypeId == dataViewFilterEntityId )
            {
                var filterDataViewId = filter.Selection.AsIntegerOrNull();
                if ( filterDataViewId == dataViewId )
                {
                    return true;
                }
            }

            return filter.ChildFilters.Any( childFilter => IsViewInFilter( dataViewId, childFilter, dataViewFilterEntityId ) );
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
            var newItem = (DataView)( item.Clone( true ) );

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
        /// Reset all of the identifiers on a DataViewFilter that uniquely identify it in the permanent store.
        /// </summary>
        /// <param name="filter">The data view filter.</param>
        private void ResetPermanentStoreIdentifiers( DataViewFilter filter )
        {
            if ( filter == null )
                return;

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

    }
}
