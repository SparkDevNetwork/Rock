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
using System.Linq.Dynamic.Core;

using Rock.Core;
using Rock.Data;
using Rock.Security;
using Rock.ViewModels.Core;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.EntitySearch"/> entity objects.
    /// </summary>
    public partial class EntitySearchService 
    {
        #region Fields

        private readonly static ParsingConfig _parsingConfig = new ParsingConfig
        {
            DisableMemberAccessToIndexAccessorFallback = true
        };

        #endregion

        #region Methods

        /// <summary>
        /// Gets the search results for the search specified by the cache object.
        /// </summary>
        /// <param name="entitySearch">The entity search definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        public static List<dynamic> GetSearchResults( EntitySearchCache entitySearch, EntitySearchQueryBag userQuery )
        {
            var entityType = entitySearch.EntityType.GetEntityType()
                ?? throw new Exception( $"Entity type {entitySearch.EntityType.Name} was not found." );

            using ( var rockContext = new RockContext() )
            {
                var systemQuery = new EntitySearchSystemQuery
                {
                    WhereExpression = entitySearch.WhereExpression,
                    GroupByExpression = entitySearch.GroupByExpression,
                    SelectExpression = entitySearch.SelectExpression,
                    OrderByExpression = entitySearch.OrderByExpression,
                    MaximumResultsPerQuery = entitySearch.MaximumResultsPerQuery,
                    IsEntitySecurityEnforced = entitySearch.IsEntitySecurityEnforced
                };

                return GetSearchResults( entityType, systemQuery, userQuery, rockContext );
            }
        }

        /// <summary>
        /// Gets the search results for the search specified by the search object.
        /// </summary>
        /// <param name="entitySearch">The entity search definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        public static List<dynamic> GetSearchResults( EntitySearch entitySearch, EntitySearchQueryBag userQuery )
        {
            var entityType = EntityTypeCache.Get( entitySearch.EntityTypeId )?.GetEntityType()
                ?? throw new Exception( $"Entity type {entitySearch.EntityType.Name} was not found." );

            using ( var rockContext = new RockContext() )
            {
                var systemQuery = new EntitySearchSystemQuery
                {
                    WhereExpression = entitySearch.WhereExpression,
                    GroupByExpression = entitySearch.GroupByExpression,
                    SelectExpression = entitySearch.SelectExpression,
                    OrderByExpression = entitySearch.OrderByExpression,
                    MaximumResultsPerQuery = entitySearch.MaximumResultsPerQuery,
                    IsEntitySecurityEnforced = entitySearch.IsEntitySecurityEnforced
                };

                return GetSearchResults( entityType, systemQuery, userQuery, rockContext );
            }
        }

        /// <summary>
        /// Gets the search results from the specified queryable using the
        /// provided query information.
        /// </summary>
        /// <typeparam name="TEntity">The type of objects in the queryable.</typeparam>
        /// <param name="queryable">The queryable to search.</param>
        /// <param name="systemQuery">The system query definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        internal static List<dynamic> GetSearchResultsInternal<TEntity>( IQueryable<TEntity> queryable, EntitySearchSystemQuery systemQuery, EntitySearchQueryBag userQuery )
            where TEntity : IEntity
        {
            var config = _parsingConfig;
            int? takeCount = systemQuery.MaximumResultsPerQuery;

            // Perform the system query elements.

            if ( systemQuery.WhereExpression.IsNotNullOrWhiteSpace() )
            {
                queryable = queryable.Where( config, systemQuery.WhereExpression );
            }

            if ( systemQuery.IsEntitySecurityEnforced )
            {
                if ( !typeof( ISecured ).IsAssignableFrom( typeof( TEntity ) ) )
                {
                    throw new Exception( $"Entity type '{typeof( TEntity ).FullName}' does not support security but enforcing security was specified." );
                }

                queryable = queryable.ToList()
                    .Where( a => ( ( ISecured ) a ).IsAuthorized( Authorization.VIEW, null ) )
                    .AsQueryable();
            }

            // Switch to a generic IQueryable for dynamic LINQ since we don't
            // know the structure of the .Select() call.
            IQueryable resultQry = queryable;

            if ( systemQuery.GroupByExpression.IsNotNullOrWhiteSpace() )
            {
                resultQry = resultQry.GroupBy( config, systemQuery.GroupByExpression );
            }

            if ( systemQuery.SelectExpression.IsNotNullOrWhiteSpace() )
            {
                resultQry = resultQry.Select( config, systemQuery.SelectExpression );
            }

            if ( systemQuery.OrderByExpression.IsNotNullOrWhiteSpace() )
            {
                resultQry = resultQry.OrderBy( config, systemQuery.OrderByExpression );
            }

            // Perform the user query elements.

            if ( userQuery != null )
            {
                if ( userQuery.Where.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.Where( config, userQuery.Where );
                }

                if ( userQuery.GroupBy.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.GroupBy( config, userQuery.GroupBy );
                }

                if ( userQuery.Select.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.Select( config, userQuery.Select );
                }

                if ( userQuery.OrderBy.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.OrderBy( config, userQuery.OrderBy );
                }

                if ( userQuery.Skip.HasValue )
                {
                    resultQry = resultQry.Skip( userQuery.Skip.Value );
                }

                if ( userQuery.Take.HasValue )
                {
                    takeCount = Math.Min( userQuery.Take.Value, takeCount ?? int.MaxValue );
                }
            }

            if ( takeCount.HasValue )
            {
                resultQry = resultQry.Take( takeCount.Value );
            }

            return resultQry.ToDynamicList();
        }

        /// <summary>
        /// Gets the search results from the specified queryable using the
        /// provided query information.
        /// </summary>
        /// <param name="entityType">The entity types to be searched.</param>
        /// <param name="systemQuery">The system query definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <param name="rockContext">The database context to search in.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        private static List<dynamic> GetSearchResults( Type entityType, EntitySearchSystemQuery systemQuery, EntitySearchQueryBag userQuery, RockContext rockContext )
        {
            if ( !typeof( IEntity ).IsAssignableFrom( entityType ) )
            {
                throw new Exception( $"Type '{entityType.FullName}' must implement IEntity to use entity search." );
            }

            var queryable = Reflection.GetQueryableForEntityType( entityType, rockContext )
                ?? throw new Exception( $"Entity type '{entityType.FullName}' does not support entity search." );

            var method = typeof( EntitySearchService )
                .GetMethod( nameof( GetSearchResultsInternal ), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic );

            var genericMethod = method.MakeGenericMethod( entityType );

            return ( List<object> ) genericMethod.Invoke( null, new object[] { queryable, systemQuery, userQuery } );
        }

        /// <summary>
        /// Gets the search results from the specified queryable using the
        /// provided query information.
        /// </summary>
        /// <typeparam name="TEntity">The type of objects to be searched in the database.</typeparam>
        /// <param name="systemQuery">The system query definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <param name="rockContext">The database context to search in.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        private static List<dynamic> GetSearchResults<TEntity>( EntitySearchSystemQuery systemQuery, EntitySearchQueryBag userQuery, RockContext rockContext )
            where TEntity : IEntity
        {
            var queryable = Reflection.GetQueryableForEntityType( typeof( TEntity ), rockContext )
                ?? throw new Exception( $"Entity type '{typeof( TEntity ).FullName}' does not support entity search." );

            return GetSearchResultsInternal( queryable, systemQuery, userQuery );
        }

        #endregion
    }
}
