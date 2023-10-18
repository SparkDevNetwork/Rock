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
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

using Rock.Core.EntitySearch;
using Rock.Data;
using Rock.ViewModels.Core;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.EntitySearch"/> entity objects.
    /// </summary>
    public partial class EntitySearchService 
    {
        #region Methods

        /// <summary>
        /// Gets the search results for the search specified by the cache object.
        /// </summary>
        /// <param name="entitySearch">The entity search definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <param name="currentPerson">The person that is requesting execution of the query.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        public static EntitySearchResultsBag GetSearchResults( EntitySearchCache entitySearch, EntitySearchQueryBag userQuery, Person currentPerson )
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
                    SelectManyExpression = entitySearch.SelectManyExpression,
                    SortExpression = entitySearch.SortExpression,
                    MaximumResultsPerQuery = entitySearch.MaximumResultsPerQuery,
                    IsEntitySecurityEnabled = entitySearch.IsEntitySecurityEnabled,
                    IncludePaths = entitySearch.IncludePaths,
                    IsRefinementAllowed = entitySearch.IsRefinementAllowed,
                    CurrentPerson = currentPerson
                };

                return GetSearchResults( entityType, systemQuery, userQuery, rockContext );
            }
        }

        /// <summary>
        /// Gets the search results for the search specified by the search object.
        /// </summary>
        /// <param name="entitySearch">The entity search definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <param name="currentPerson">The person that is requesting execution of the query.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        public static EntitySearchResultsBag GetSearchResults( EntitySearch entitySearch, EntitySearchQueryBag userQuery, Person currentPerson )
        {
            using ( var rockContext = new RockContext() )
            {
                return GetSearchResults( entitySearch, userQuery, currentPerson, rockContext );
            }
        }

        /// <summary>
        /// Gets the search results for the search specified by the search object.
        /// </summary>
        /// <remarks>
        /// This method only exists for the EntitySearchDetail block so it can get
        /// query counts in preview mode.
        /// </remarks>
        /// <param name="entitySearch">The entity search definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <param name="currentPerson">The person that is requesting execution of the query.</param>
        /// <param name="rockContext">The database context.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        internal static EntitySearchResultsBag GetSearchResults( EntitySearch entitySearch, EntitySearchQueryBag userQuery, Person currentPerson, RockContext rockContext )
        {
            var entityType = EntityTypeCache.Get( entitySearch.EntityTypeId )?.GetEntityType()
                ?? throw new Exception( $"Entity type {entitySearch.EntityType.Name} was not found." );

            var systemQuery = new EntitySearchSystemQuery
            {
                WhereExpression = entitySearch.WhereExpression,
                GroupByExpression = entitySearch.GroupByExpression,
                SelectExpression = entitySearch.SelectExpression,
                SelectManyExpression = entitySearch.SelectManyExpression,
                SortExpression = entitySearch.SortExpression,
                MaximumResultsPerQuery = entitySearch.MaximumResultsPerQuery,
                IsEntitySecurityEnabled = entitySearch.IsEntitySecurityEnabled,
                IncludePaths = entitySearch.IncludePaths,
                IsRefinementAllowed = entitySearch.IsRefinementAllowed,
                CurrentPerson = currentPerson
            };

            return GetSearchResults( entityType, systemQuery, userQuery, rockContext );
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
        private static EntitySearchResultsBag GetSearchResults( Type entityType, EntitySearchSystemQuery systemQuery, EntitySearchQueryBag userQuery, RockContext rockContext )
        {
            if ( !typeof( IEntity ).IsAssignableFrom( entityType ) )
            {
                throw new Exception( $"Type '{entityType.FullName}' must implement IEntity to use entity search." );
            }

            var queryable = Reflection.GetQueryableForEntityType( entityType, rockContext )
                ?? throw new Exception( $"Entity type '{entityType.FullName}' does not support entity search." );

            var method = typeof( EntitySearchService )
                .GetMethod( nameof( GetSearchResults ),
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[] { typeof( EntitySearchSystemQuery ), typeof( EntitySearchQueryBag ), typeof( RockContext ) },
                    null );

            var genericMethod = method.MakeGenericMethod( entityType );

            try
            {
                return ( EntitySearchResultsBag ) genericMethod.Invoke( null, new object[] { systemQuery, userQuery, rockContext } );
            }
            catch ( TargetInvocationException ex )
            {
                // Throw the actual exception and preserve the stack trace.
                ExceptionDispatchInfo.Capture( ex.InnerException ).Throw();

                // This is never reached.
                return new EntitySearchResultsBag();
            }
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
        private static EntitySearchResultsBag GetSearchResults<TEntity>( EntitySearchSystemQuery systemQuery, EntitySearchQueryBag userQuery, RockContext rockContext )
            where TEntity : IEntity
        {
            var queryable = ( IQueryable<TEntity> ) Reflection.GetQueryableForEntityType( typeof( TEntity ), rockContext )
                ?? throw new Exception( $"Entity type '{typeof( TEntity ).FullName}' does not support entity search." );

            return EntitySearchHelper.GetSearchResults( queryable, systemQuery, userQuery );
        }

        #endregion
    }
}
