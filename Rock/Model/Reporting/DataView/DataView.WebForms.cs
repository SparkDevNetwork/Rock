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
using System.Reflection;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    /// <summary>
    /// Represents a filterable DataView in Rock.
    /// </summary>
    public partial class DataView : Model<DataView>, ICategorized
    {
        /// <summary>
        /// Gets the query using the most appropriate type of dbContext 
        /// </summary>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetQuery( DataViewGetQueryArgs dataViewGetQueryArgs ) instead" )]
        public IQueryable<IEntity> GetQuery( SortProperty sortProperty, int? databaseTimeoutSeconds, out List<string> errorMessages )
        {
            return GetQuery( sortProperty, null, null, databaseTimeoutSeconds, out errorMessages );
        }

        /// <summary>
        /// Gets the query using the specified dbContext
        /// </summary>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetQuery( DataViewGetQueryArgs dataViewGetQueryArgs ) instead" )]
        public IQueryable<IEntity> GetQuery( SortProperty sortProperty, System.Data.Entity.DbContext dbContext, int? databaseTimeoutSeconds, out List<string> errorMessages )
        {
            return GetQuery( sortProperty, dbContext, null, databaseTimeoutSeconds, out errorMessages );
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetQuery( DataViewGetQueryArgs dataViewGetQueryArgs ) instead" )]
        public IQueryable<IEntity> GetQuery( SortProperty sortProperty, System.Data.Entity.DbContext dbContext, DataViewFilterOverrides dataViewFilterOverrides, int? databaseTimeoutSeconds, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            return GetQuery( new DataViewGetQueryArgs
            {
                DbContext = dbContext,
                DataViewFilterOverrides = dataViewFilterOverrides,
                DatabaseTimeoutSeconds = databaseTimeoutSeconds,
                SortProperty = sortProperty
            } );
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="dataViewGetQueryArgs">The data view get query arguments.</param>
        /// <returns></returns>
        /// <exception cref="Rock.Reporting.RockReportingException">
        /// Unable to determine DbContext for {this}
        /// or
        /// Unable to determine ServiceInstance for {this}
        /// or
        /// Unable to determine IService.Get for {this}
        /// </exception>
        public IQueryable<IEntity> GetQuery( DataViewGetQueryArgs dataViewGetQueryArgs )
        {
            var dbContext = dataViewGetQueryArgs.DbContext;
            if ( dbContext == null )
            {
                dbContext = this.GetDbContext();
                if ( dbContext == null )
                {
                    // this could happen if the EntityTypeId id refers to an assembly/type that doesn't exist anymore
                    // we'll just default to new RockContext(), but it'll likely fail when we try to get a ServiceInstance below if the entityType doesn't exist in an assembly
                    dbContext = new RockContext();
                }
            }

            IService serviceInstance = this.GetServiceInstance( dbContext );
            if ( serviceInstance == null )
            {
                var entityTypeCache = EntityTypeCache.Get( this.EntityTypeId ?? 0 );
                throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"Unable to determine ServiceInstance from DataView EntityType {entityTypeCache} for {this}" );
            }

            var databaseTimeoutSeconds = dataViewGetQueryArgs.DatabaseTimeoutSeconds;
            if ( databaseTimeoutSeconds.HasValue )
            {
                dbContext.Database.CommandTimeout = databaseTimeoutSeconds.Value;
            }

            var dataViewFilterOverrides = dataViewGetQueryArgs.DataViewFilterOverrides;
            ParameterExpression paramExpression = serviceInstance.ParameterExpression;
            Expression whereExpression = GetExpression( serviceInstance, paramExpression, dataViewFilterOverrides );

            MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( SortProperty ) } );
            if ( getMethod == null )
            {
                throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"Unable to determine IService.Get for Report: {this}" );
            }

            var sortProperty = dataViewGetQueryArgs.SortProperty;

            if ( sortProperty == null )
            {
                // if no sorting is specified, just sort by Id
                sortProperty = new SortProperty { Direction = SortDirection.Ascending, Property = "Id" };
            }

            var getResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression, sortProperty } );
            var qry = getResult as IQueryable<IEntity>;

            return qry;
        }
    }
}
