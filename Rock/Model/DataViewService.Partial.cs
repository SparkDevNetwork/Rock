// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

using Rock.Data;
using Rock.Reporting.DataFilter;

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
        /// Determines whether the specified Data View forms part of a filter.
        /// </summary>
        /// <param name="dataViewId">The unique identifier of a Data View.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        ///   <c>true</c> if the specified Data View forms part of the conditions for the specified filter.
        /// </returns>
        public bool IsViewInFilter( int dataViewId, DataViewFilter filter )
        {
            var dataViewFilterEntityId = new EntityTypeService((RockContext)this.Context).Get(typeof(OtherDataViewFilter), false, null).Id;

            return IsViewInFilter(dataViewId, filter, dataViewFilterEntityId);
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
       
    }
}
