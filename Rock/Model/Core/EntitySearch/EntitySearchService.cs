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
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

using Rock.Core;
using Rock.Data;
using Rock.Security;
using Rock.Utility;
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
                    OrderByExpression = entitySearch.OrderByExpression,
                    MaximumResultsPerQuery = entitySearch.MaximumResultsPerQuery,
                    IsEntitySecurityEnforced = entitySearch.IsEntitySecurityEnforced,
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
                OrderByExpression = entitySearch.OrderByExpression,
                MaximumResultsPerQuery = entitySearch.MaximumResultsPerQuery,
                IsEntitySecurityEnforced = entitySearch.IsEntitySecurityEnforced,
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
        /// <typeparam name="TEntity">The type of objects in the queryable.</typeparam>
        /// <param name="queryable">The queryable to search.</param>
        /// <param name="systemQuery">The system query definition.</param>
        /// <param name="userQuery">The additional user query details.</param>
        /// <returns>A list of dynamic objects that represents the results.</returns>
        internal static EntitySearchResultsBag GetSearchResultsInternal<TEntity>( IQueryable<TEntity> queryable, EntitySearchSystemQuery systemQuery, EntitySearchQueryBag userQuery )
            where TEntity : IEntity
        {
            var config = _parsingConfig;
            var takeCount = systemQuery?.MaximumResultsPerQuery;
            var isRefinementAllowed = systemQuery?.IsRefinementAllowed != false;
            IQueryable resultQry;

            // Perform the system query elements.
            if ( systemQuery != null )
            {
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
                    resultQry = resultQry.Where( config, userQuery.Where );
                }

                if ( isRefinementAllowed && userQuery.GroupBy.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.GroupBy( config, userQuery.GroupBy );
                }

                if ( isRefinementAllowed && userQuery.Select.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.Select( config, userQuery.Select );
                }

                if ( isRefinementAllowed && userQuery.OrderBy.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.OrderBy( config, userQuery.OrderBy );
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
                .GetMethod( nameof( GetSearchResultsInternal ), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic );

            var genericMethod = method.MakeGenericMethod( entityType );

            try
            {
                return ( EntitySearchResultsBag ) genericMethod.Invoke( null, new object[] { queryable, systemQuery, userQuery } );
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
            var queryable = Reflection.GetQueryableForEntityType( typeof( TEntity ), rockContext )
                ?? throw new Exception( $"Entity type '{typeof( TEntity ).FullName}' does not support entity search." );

            return GetSearchResultsInternal( queryable, systemQuery, userQuery );
        }

        #endregion

        /// <summary>
        /// Expression visitor that handles IdKey requests for Entity Searches.
        /// </summary>
        internal class IdKeyExpressionVisitor : ExpressionVisitor
        {
            /// <summary>
            /// The prefix for IdKey property accessors.
            /// </summary>
            private readonly string _idKeyPrefix;

            /// <summary>
            /// Initializes a new instance of the <see cref="IdKeyExpressionVisitor"/> class.
            /// </summary>
            /// <param name="idKeyPrefix">The IdKey prefix to use when rewriting select statements.</param>
            public IdKeyExpressionVisitor( string idKeyPrefix )
            {
                _idKeyPrefix = idKeyPrefix;
            }

            /// <inheritdoc/>
            protected override MemberAssignment VisitMemberAssignment( MemberAssignment node )
            {
                if ( IsIdKeyMember( node.Expression, out var idKeyExpression ) )
                {
                    // At this point in the code, we are processing an expression
                    // designed to assign some member the value of IEntity.IdKey.
                    // But that doesn't actually exist in the database so we have
                    // to fake it. We are going to do that by effectively doing:
                    // x = _idKeyPrefix + Id
                    //
                    // The first thing we need to do is convert the Id column to
                    // a string value. Then we need to combine the prefix text
                    // with the converted Id value. Later, when instantiating the
                    // results, another piece of code will handle translating these
                    // values into hashed IdKey values.

                    // Convert the Id column into a string.
                    var idExpression = Expression.Property( idKeyExpression.Expression, nameof( IEntity.Id ) );
                    var idStringExpression = Expression.Call( idExpression, typeof( object ).GetMethod( "ToString" ) );

                    // Get the prefix value. This seems excessive since we could
                    // just do Expression.Constant( _idKeyPrefix ) and get the
                    // result data - but it is actually much slower to do it that
                    // way. On the order of an extra 20ms. Don't ask me why, I
                    // don't understand the difference. But doing it this way is
                    // also the way C# LINQ would build the expression so we
                    // will follow suite.
                    var prefixMember = Expression.MakeMemberAccess( Expression.Constant( this ), this.GetType().GetField( nameof( _idKeyPrefix ), BindingFlags.NonPublic | BindingFlags.Instance ) );

                    // Create a new string that concatenates the prefix and the Id.
                    var strConcatMethod = typeof( string ).GetMethod( "Concat", new[] { typeof( string ), typeof( string ) } );
                    var idEncodedExpression = Expression.Add( prefixMember, idStringExpression, strConcatMethod );

                    return node.Update( idEncodedExpression );
                }

                return base.VisitMemberAssignment( node );
            }

            /// <inheritdoc/>
            protected override Expression VisitBinary( BinaryExpression node )
            {
                // IdKey is only valid for equals and not equals comparisons.
                if ( node.NodeType != ExpressionType.Equal && node.NodeType != ExpressionType.NotEqual )
                {
                    return base.VisitBinary( node );
                }

                if ( IsIdKeyMember( node.Left, out var idKeyExpression ) )
                {
                    // Make sure we are doing a comparison we can handle.
                    if ( !( node.Right is ConstantExpression constantExpression ) )
                    {
                        return base.VisitBinary( node );
                    }

                    // Create an expression that compares the de-hashed value against Id.
                    var memberExpression = Expression.Property( idKeyExpression.Expression, nameof( IEntity.Id ) );
                    var id = IdHasher.Instance.GetId( constantExpression.Value.ToString() ) ?? 0;

                    return node.NodeType == ExpressionType.Equal
                        ? Expression.Equal( memberExpression, Expression.Constant( id ) )
                        : Expression.NotEqual( memberExpression, Expression.Constant( id ) );
                }

                if ( IsIdKeyMember( node.Right, out idKeyExpression ) )
                {
                    // Make sure we are doing a comparison we can handle.
                    if ( !( node.Left is ConstantExpression constantExpression ) )
                    {
                        return base.VisitBinary( node );
                    }

                    // Create an expression that compares the de-hashed value against Id.
                    var memberExpression = Expression.Property( idKeyExpression.Expression, nameof( IEntity.Id ) );
                    var id = IdHasher.Instance.GetId( constantExpression.Value.ToString() ) ?? 0;

                    return node.NodeType == ExpressionType.Equal
                        ? Expression.Equal( Expression.Constant( id ), memberExpression )
                        : Expression.NotEqual( Expression.Constant( id ), memberExpression );
                }

                return base.VisitBinary( node );
            }

            /// <summary>
            /// Determines if the expression is one that accesses the IdKey property.
            /// </summary>
            /// <param name="expression">The expression to be checked.</param>
            /// <param name="memberExpression">Contains the expression cast as a member expression if return value is <c>true</c>.</param>
            /// <returns><c>true</c> if the expresion matches the expected pattern; <c>false</c> otherwise.</returns>
            private static bool IsIdKeyMember( Expression expression, out MemberExpression memberExpression )
            {
                memberExpression = expression as MemberExpression;

                return memberExpression != null
                    && typeof( IEntity ).IsAssignableFrom( memberExpression.Member.ReflectedType )
                    && memberExpression.Member.Name == nameof( IEntity.IdKey );
            }
        }
    }
}
