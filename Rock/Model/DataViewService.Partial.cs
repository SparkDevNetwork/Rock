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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

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
        public List<int> GetIds ( int dataViewId )
        {
            var dataView = Queryable().AsNoTracking().FirstOrDefault( d => d.Id == dataViewId );
            if ( dataView != null && dataView.EntityTypeId.HasValue )
            {
                var cachedEntityType = EntityTypeCache.Read( dataView.EntityTypeId.Value );
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
       
    }
}
