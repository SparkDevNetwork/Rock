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

using Rock.Data;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Core;
using System.Linq.Dynamic.Core;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;

namespace Rock.Core.EntitySearch
{
    /// <summary>
    /// Logic for performing entity searches. This is for internal use. All
    /// requests to perform an entity search should go through
    /// <see cref="Rock.Model.EntitySearchService"/>.
    /// </summary>
    internal static class EntitySearchHelper
    {
        #region Fields

        /// <summary>
        /// The parsing configuration for dynamic LINQ library.
        /// </summary>
        private readonly static ParsingConfig _parsingConfig = new ParsingConfig
        {
            DisableMemberAccessToIndexAccessorFallback = true
        };

        #endregion

        #region Methods

        /// <summary>
        /// Gets the search results from the specified queryable using the
        /// provided query information.
        /// </summary>
        /// <typeparam name="TEntity">The type of objects in the queryable.</typeparam>
        /// <param name="queryable">The queryable to search.</param>
        /// <param name="systemQuery">The system query definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        public static EntitySearchResultsBag GetSearchResults<TEntity>( IQueryable<TEntity> queryable, EntitySearchSystemQuery systemQuery, EntitySearchQueryBag userQuery )
            where TEntity : IEntity
        {
            var config = _parsingConfig;
            var takeCount = systemQuery?.MaximumResultsPerQuery;
            var isRefinementAllowed = systemQuery?.IsRefinementAllowed != false;
            IQueryable resultQry;

            var queryParameters = new Dictionary<string, object>
            {
                ["@CurrentPersonId"] = systemQuery?.CurrentPerson?.Id
            };

            // Perform the system query elements.
            if ( systemQuery != null )
            {
                if ( systemQuery.WhereExpression.IsNotNullOrWhiteSpace() )
                {
                    queryable = queryable.Where( config, systemQuery.WhereExpression, queryParameters );
                }

                if ( systemQuery.IsEntitySecurityEnforced )
                {
                    if ( !typeof( ISecured ).IsAssignableFrom( typeof( TEntity ) ) )
                    {
                        throw new Exception( $"Entity type '{typeof( TEntity ).FullName}' does not support security but enforcing security was specified." );
                    }

                    if ( systemQuery.IncludePaths.IsNotNullOrWhiteSpace() )
                    {
                        var paths = systemQuery.IncludePaths.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                        foreach ( var path in paths )
                        {
                            queryable = queryable.Include( path.Trim() );
                        }
                    }

                    queryable = queryable.ToList()
                        .Where( a => ( ( ISecured ) a ).IsAuthorized( Authorization.VIEW, systemQuery.CurrentPerson ) )
                        .AsQueryable();
                }

                // Switch to a generic IQueryable for dynamic LINQ since we don't
                // know the structure of the .Select() call.
                resultQry = queryable;

                if ( systemQuery.GroupByExpression.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.GroupBy( config, systemQuery.GroupByExpression, queryParameters );
                }

                if ( systemQuery.SelectManyExpression.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.SelectMany( config, systemQuery.SelectManyExpression, queryParameters );
                }
                else if ( systemQuery.SelectExpression.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.Select( config, systemQuery.SelectExpression, queryParameters );
                }

                if ( systemQuery.OrderByExpression.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.OrderBy( config, systemQuery.OrderByExpression, queryParameters );
                }
            }
            else
            {
                // Switch to a generic IQueryable for dynamic LINQ since we don't
                // know the structure of the .Select() call.
                resultQry = queryable;
            }

            // Perform the user query elements.

            if ( userQuery != null )
            {
                if ( isRefinementAllowed && userQuery.Where.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.Where( config, userQuery.Where, queryParameters );
                }

                if ( isRefinementAllowed && userQuery.GroupBy.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.GroupBy( config, userQuery.GroupBy, queryParameters );
                }

                if ( isRefinementAllowed && userQuery.SelectMany.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.SelectMany( config, userQuery.SelectMany, queryParameters );
                }
                else if ( isRefinementAllowed && userQuery.Select.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.Select( config, userQuery.Select, queryParameters );
                }

                if ( isRefinementAllowed && userQuery.OrderBy.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.OrderBy( config, userQuery.OrderBy, queryParameters );
                }

                // Skip and Take are always allowed.
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

            // If they only want the count then just return that.
            if ( userQuery?.IsCountOnly == true )
            {
                return new EntitySearchResultsBag
                {
                    Count = resultQry.Count()
                };
            }

            // The key prefix is used to allow ProcessIdKeyForItem() method to
            // find properties that should be hashed into IdKey values. The '\0'
            // character is to help ensure an early out when doing comparisons.
            // The reason we use a random hex value is to prevent injection attacks
            // aimed and translating an arbitrary integer value into an IdKey value.
            // i.e. Don't let the user do something like '{ "\058" as IdKey }' to
            // translate 58 into the IdKey value. This is not a huge concern but it
            // closes an attack vector at the cost of fractions of a millisecond.
            var idKeyPrefix = $"\0{new Random().Next( ushort.MaxValue ):X4}-";

            // Create a new expression that converts IdKey accessors into simple Id
            // accessors with the prefix so we can decode them later.
            var visitedExpression = new IdKeyExpressionVisitor( idKeyPrefix ).Visit( resultQry.Expression );
            resultQry = resultQry.Provider.CreateQuery( visitedExpression );

            var results = resultQry.ToDynamicList();

            // Do final translation of the results.
            foreach ( var item in results )
            {
                ProcessResultItem( item, idKeyPrefix );
            }

            return new EntitySearchResultsBag
            {
                Count = results.Count,
                Items = results
            };
        }

        /// <summary>
        /// <para>
        /// Performs post-query processing of the result item.
        /// </para>
        /// <para>
        /// Scans for any IdKey values that need to be translated from Id
        /// into IdKey.
        /// </para>
        /// <para>
        /// Scans for IEnumerable arrays that are not ILists.
        /// </para>
        /// </summary>
        /// <param name="item">The object to be scanned and updated.</param>
        /// <param name="idKeyPrefix">The prefix used to designate IdKey values.</param>
        private static void ProcessResultItem( object item, string idKeyPrefix )
        {
            if ( item is DynamicClass dynamicItem )
            {
                var propertyNames = dynamicItem.GetDynamicMemberNames().ToList();

                foreach ( var propertyName in propertyNames )
                {
                    var propertyValue = dynamicItem.GetDynamicPropertyValue( propertyName );

                    // Check if this is an encoded IdKey value.
                    if ( propertyValue is string strValue )
                    {
                        if ( strValue.StartsWith( idKeyPrefix ) )
                        {
                            dynamicItem.SetDynamicPropertyValue( propertyName, IdHasher.Instance.GetHash( strValue.Substring( idKeyPrefix.Length ).AsInteger() ) );
                        }

                        continue;
                    }

                    // Check if value is an enumerable but not a collection. If so then
                    // convert it to a collection. This can happen when entity security
                    // is enforced.
                    if ( propertyValue is IEnumerable && !( propertyValue is ICollection ) )
                    {
                        propertyValue = ConvertEnumerableToList( propertyValue );
                        dynamicItem.SetDynamicPropertyValue( propertyName, propertyValue );
                    }

                    // Otherwise check if it is a child object to be scanned.
                    if ( propertyValue is DynamicClass || propertyValue is ICollection )
                    {
                        ProcessResultItem( propertyValue, idKeyPrefix );
                    }
                }
            }
            else if ( item is ICollection childItems )
            {
                // Scan each child object.
                foreach ( var childItem in childItems )
                {
                    ProcessResultItem( childItem, idKeyPrefix );
                }
            }
        }

        /// <summary>
        /// Converts the enumerable to a list of items. This handles cases where
        /// entity security is enabled but a proper IncludePaths value was not
        /// set. Select statements that touch on a collection navigation property
        /// are lazy loaded and left as an IEnumerable. This causes issues when
        /// the RockContext is disposed as well as getting correct timing and
        /// query counts.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>A new list that represents the source.</returns>
        private static object ConvertEnumerableToList( object source )
        {
            var collectionType = source.GetType()
                .GetInterfaces()
                .FirstOrDefault( i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof( IEnumerable<> ) )
                ?.GetGenericArguments()[0];

            if ( collectionType == null )
            {
                throw new Exception( "Invalid enumerable type found in search results." );
            }

            var miCast = typeof( Enumerable ).GetMethod( nameof( Enumerable.Cast ) );
            var miCastGeneric = miCast.MakeGenericMethod( collectionType );
            var miToList = typeof( Enumerable ).GetMethod( nameof( Enumerable.ToList ) );
            var miToListGeneric = miToList.MakeGenericMethod( collectionType );

            var castValue = miCastGeneric.Invoke( null, new object[] { source } );
            return miToListGeneric.Invoke( null, new object[] { castValue } );
        }

        #endregion
    }
}
