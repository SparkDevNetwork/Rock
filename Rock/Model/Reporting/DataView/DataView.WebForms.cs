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
using EF6.TagWith;
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
        /// <returns></returns>
        public IQueryable<IEntity> GetQuery()
        {
            return GetQuery( null );
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
            dataViewGetQueryArgs = dataViewGetQueryArgs ?? new DataViewGetQueryArgs();

            var dbContext = dataViewGetQueryArgs.DbContext;
            if ( dbContext == null )
            {
                dbContext = this.GetDbContext();
            }

            IService serviceInstance = this.GetServiceInstance( dbContext );
            if ( serviceInstance == null )
            {
                var entityTypeCache = EntityTypeCache.Get( this.EntityTypeId ?? 0 );
                throw new RockDataViewFilterExpressionException( ( IDataViewFilterDefinition ) this.DataViewFilter, $"Unable to determine ServiceInstance from DataView EntityType {entityTypeCache} for {this}" );
            }

            var databaseTimeoutSeconds = dataViewGetQueryArgs.DatabaseTimeoutSeconds;
            if ( databaseTimeoutSeconds.HasValue )
            {
                dbContext.Database.CommandTimeout = databaseTimeoutSeconds.Value;
            }

            var dataViewFilterOverrides = dataViewGetQueryArgs.DataViewFilterOverrides;
            ParameterExpression paramExpression = serviceInstance.ParameterExpression;
            Expression whereExpression = GetExpression( serviceInstance, paramExpression, dataViewFilterOverrides );

            var sortProperty = dataViewGetQueryArgs.SortProperty;

            if ( sortProperty == null )
            {
                // if no sorting is specified, just sort by Id
                sortProperty = new SortProperty { Direction = SortDirection.Ascending, Property = "Id" };
            }

            Type returnType = null;
            IQueryable<IEntity> dataViewQuery;
            var personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();
            if ( this.EntityTypeId.HasValue && this.EntityTypeId.Value == personEntityTypeId && serviceInstance is PersonService personService )
            {
                /* 05/25/2022 MDP

                We have a option in DataViews that are based on Person on whether Deceased individuals should be included. That requires the
                PersonService.Querable( bool includeDeceased ) method, so we'll use that.

                */

                returnType = typeof( Person );
                dataViewQuery = personService.Queryable( this.IncludeDeceased ).Where( paramExpression, whereExpression, sortProperty );
            }
            else
            {
                MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( SortProperty ) } );
                if ( getMethod == null )
                {
                    throw new RockDataViewFilterExpressionException( ( IDataViewFilterDefinition ) this.DataViewFilter, $"Unable to determine IService.Get for Report: {this}" );
                }

                // Get the specific, underlying IEntity type implementation (i.e. Group).
                var genericArgs = getMethod.ReturnType.GetGenericArguments();
                if ( genericArgs.Length > 0 )
                {
                    returnType = genericArgs.First();
                }

                dataViewQuery = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression, sortProperty } ) as IQueryable<IEntity>;
            }

            // Add a comment to the query with the data view id for debugging.
            /*
                6/21/2023 - JPH

                When calling the TagWith() method without explicitly specifying the underlying IEntity implementation type,
                it will add a ParameterExpression of type IEntity to the query, which prevents the successful casting of the
                returned IQueryable<IEntity> to a specific IEntity implementation (i.e. IQueryable<Group>). Many callers of
                this GetQuery() method have a tendency to do exactly that: cast the resulting IQueryable to a specific type.
                Rather than change all of those callers' handling of this return type, we'll use reflection below to ensure
                we're passing the correct underlying type to the TagWith() method, so it doesn't handcuff our usage of the
                returned IQueryable.

                Reason: TagWith() in certain situations is causing query to be null
                (https://app.asana.com/0/474497188512037/1204855716691596/f)
             */
            if ( returnType != null )
            {
                var tagWithMethod = typeof( TagWithExtensions ).GetMethod( "TagWith" );
                if ( tagWithMethod != null )
                {
                    tagWithMethod = tagWithMethod.MakeGenericMethod( returnType );
                    dataViewQuery = tagWithMethod.Invoke( null, new object[]
                    {
                        dataViewQuery,
                        $"Data View Id: {this.Id}" // Here is where we're specifying the "tag" to be added.
                    } ) as IQueryable<IEntity>;
                }
            }

            return dataViewQuery;
        }
    }
}
