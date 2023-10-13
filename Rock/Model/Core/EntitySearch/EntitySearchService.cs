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
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

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

            var hashPrefix = new Random().Next( ushort.MaxValue ).ToString( "X4" );
            var visitedExpression = new IdKeyExpressionVisitor( hashPrefix ).Visit( resultQry.Expression );

            resultQry = resultQry.Provider.CreateQuery( visitedExpression );

            var results = resultQry.ToDynamicList();

            // Check if we need to translate any IdKey properties.
            if ( results.Any() && results[0] is IDynamicMetaObjectProvider )
            {
                foreach ( var item in results )
                {
                    if ( item.idKey is string firstIdKeyStr && firstIdKeyStr.StartsWith( hashPrefix ) )
                    {
                        item.IdKey = IdHasher.Instance.GetHash( firstIdKeyStr.Substring( hashPrefix.Length ).AsInteger() );
                    }
                    else if ( item.IdKey is string secondIdKeyStr && secondIdKeyStr.StartsWith( hashPrefix ) )
                    {
                        item.IdKey = IdHasher.Instance.GetHash( secondIdKeyStr.Substring( hashPrefix.Length ).AsInteger() );
                    }
                }
            }

            return results;
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

        /// <summary>
        /// Expression visitor that handles IdKey requests for Entity Searches.
        /// </summary>
        internal class IdKeyExpressionVisitor : ExpressionVisitor
        {
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
