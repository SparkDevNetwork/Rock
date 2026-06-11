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

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

using Rock;
using Rock.Model;
using Rock.Security;

namespace Rock.Data
{
    /// <summary>
    /// Handles cursor-based pagination for a queryable source. This allows for
    /// efficient paging through large datasets without the performance pitfalls
    /// of offset-based pagination. This is meant to be used at times when you
    /// need to enforce security on the entities being paged, though you can use
    /// a queryable of any type and it will simply ignore the security aspect.
    /// </summary>
    /// <typeparam name="T">The type of object to be returned in the results.</typeparam>
    internal class CursorPaginator<T>
        where T : class
    {
        #region Constants

        /// <summary>
        /// <para>
        /// The maximum number of times to attempt to fill a page when enforcing
        /// security. This is a safeguard to prevent infinite loops in cases where
        /// none of the items in the source are authorized for the current user.
        /// When this limit is reached, the paginator will return whatever items
        /// it has been able to fill so far, even if it's less than the requested
        /// page size.
        /// </para>
        /// <para>
        /// This number was chosen arbitrarily to be a reasonable limit. It can
        /// be adjusted in the future if a better value is determined.
        /// </para>
        /// </summary>
        private const int MaxCursorFillAttempts = 25;

        #endregion

        #region Fields

        /// <summary>
        /// The ordering expression provided by the caller. This is used to
        /// determine how the cursor filters for subsequent pages should be
        /// constructed, and also to apply the same ordering to the
        /// queryable source.
        /// </summary>
        private readonly Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> _orderBy;

        /// <summary>
        /// The compiled version of <see cref="_orderBy"/>. This is used to
        /// speed up retrieval of multiple pages, since compiling the
        /// expression can be relatively expensive.
        /// </summary>
        private readonly Func<IQueryable<T>, IOrderedQueryable<T>> _orderByFn;

        /// <summary>
        /// The person for whom the pagination is being performed. This is
        /// used to enforce entity-level security when retrieving pages.
        /// </summary>
        private readonly Person _person;

        /// <summary>
        /// Additional filter predicates that should be applied to the source
        /// before including them in the page results.
        /// </summary>
        private readonly List<Func<T, bool>> _filterPredicates = new List<Func<T, bool>>();

        #endregion

        #region Properties

        /// <summary>
        /// Determines if security is enforced when retrieving pages. If true,
        /// then only items that are authorized for the current person will be
        /// included in the results. If <typeparamref name="T"/> does not
        /// implement <see cref="ISecured"/>, then this property has no effect
        /// on the results.
        /// </summary>
        public bool EnforceEntitySecurity { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="CursorPaginator{T}"/> class with
        /// security checks disabled.
        /// </summary>
        /// <param name="orderBy">The expression that defines the ordering of the queryable.</param>
        public CursorPaginator( Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBy )
        {
            _orderBy = orderBy;
            _orderByFn = orderBy.Compile();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CursorPaginator{T}"/> class with
        /// security checks enabled.
        /// </summary>
        /// <param name="person">The person to use when checking security on items.</param>
        /// <param name="orderBy">The expression that defines the ordering of the queryable.</param>
        public CursorPaginator( Person person, Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBy )
            : this( orderBy )
        {
            _person = person;

            EnforceEntitySecurity = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a custom predicate that will be used to filter the items in
        /// the source before they are added to the page.
        /// </summary>
        /// <param name="predicate">The custom predicate to use when filtering results.</param>
        public void AddPredicate( Func<T, bool> predicate )
        {
            _filterPredicates.Add( predicate );
        }

        /// <summary>
        /// <para>
        /// Retrieves the next page of results from the specified queryable
        /// source using cursor-based pagination.
        /// </para>
        /// <para>
        /// When <paramref name="enableLookAhead"/> is set to true, the method
        /// retrieves one extra item beyond the specified page size to
        /// determine if additional pages exist.
        /// </para>
        /// <para>
        /// The returned next cursor can be used in subsequent calls to continue
        /// pagination.
        /// </para>
        /// </summary>
        /// <param name="source">The queryable data source from which to retrieve items.</param>
        /// <param name="cursor">A cursor string from a previous call. Pass null to retrieve the first page.</param>
        /// <param name="pageSize">The maximum number of items to include in the returned page.</param>
        /// <param name="enableLookAhead">true to fetch one additional item to determine if more pages are available; otherwise, false.</param>
        /// <returns>A <see cref="CursorPage{T}"/> containing the items and details of the cursor.</returns>
        public CursorPage<T> GetNextPage( IQueryable<T> source, string cursor, int pageSize, bool enableLookAhead )
        {
            // 1. Parse ordering expression
            var orderings = Parse( _orderBy );

            // 2. Decode cursor (if any)
            CursorToken cursorToken = null;
            if ( cursor != null )
            {
                var json = Encryption.DecryptString( cursor );
                cursorToken = json.FromJsonOrNull<CursorToken>();
            }

            // 3. Apply seek predicate
            if ( cursorToken != null )
            {
                source = ApplySeekPredicate( source, orderings, cursorToken );
            }

            // 4. Apply ordering
            var ordered = _orderByFn( source );

            // 5. Fetch page
            var items = FillPage( ordered, pageSize + ( enableLookAhead ? 1 : 0 ) );

            if ( items.Count == 0 )
            {
                return new CursorPage<T>();
            }

            // 6. Build next cursor
            string nextCursor = null;

            if ( enableLookAhead && items.Count > pageSize )
            {
                items.RemoveAt( pageSize );
                nextCursor = BuildCursor( items.Last(), orderings );
            }
            else if ( !enableLookAhead && items.Count == pageSize )
            {
                nextCursor = BuildCursor( items.Last(), orderings );
            }

            return new CursorPage<T>( items, nextCursor );
        }

        /// <summary>
        /// Retrieves a page of items from the specified ordered query,
        /// applying security checks if required.
        /// </summary>
        /// <param name="source">The ordered queryable source from which to retrieve items.</param>
        /// <param name="desiredCount">The maximum number of items to return in the page.</param>
        /// <returns>A list containing up to the specified number of items from the source.</returns>
        private List<T> FillPage( IOrderedQueryable<T> source, int desiredCount )
        {
            var items = new List<T>();
            var offset = 0;

            for ( var attempts = 0; attempts < MaxCursorFillAttempts; attempts++ )
            {
                var takeCount = desiredCount - items.Count;
                var batch = source.Skip( offset ).Take( takeCount ).ToList();
                var batchCount = batch.Count;

                // If we got no more items, then we are confirmed to be done.
                if ( batch.Count == 0 )
                {
                    break;
                }

                var filteredBatch = batch.AsEnumerable();

                if ( typeof( ISecured ).IsAssignableFrom( typeof( T ) ) && EnforceEntitySecurity )
                {
                    filteredBatch = filteredBatch.Where( item => ( ( ISecured ) item ).IsAuthorized( Authorization.VIEW, _person ) );
                }

                foreach ( var filterPredicate in _filterPredicates )
                {
                    filteredBatch = filteredBatch.Where( filterPredicate );
                }

                items.AddRange( filteredBatch );

                if ( items.Count >= desiredCount || batchCount < takeCount )
                {
                    break;
                }

                offset += batchCount;
            }

            return items;
        }

        /// <summary>
        /// Applies a seek-based filtering predicate to the specified queryable
        /// source based on the provided cursor token and ordering information.
        /// </summary>
        /// <param name="source">The source queryable collection to which the seek predicate will be applied.</param>
        /// <param name="orderings">A list of ordering definitions that determine the sort order and fields used for cursor-based pagination.</param>
        /// <param name="cursorToken">The cursor token containing the values that define the seek position within the ordered sequence.</param>
        /// <returns>An <see cref="IQueryable{T}"/> representing the filtered sequence that starts after the position indicated by the cursor token.</returns>
        private IQueryable<T> ApplySeekPredicate( IQueryable<T> source, List<CursorOrderInfo> orderings, CursorToken cursorToken )
        {
            var lambda = CursorExpressions.BuildCursorPredicate<T>( orderings, cursorToken.Values );

            return source.Where( lambda );
        }

        /// <summary>
        /// Builds a Base64-encoded cursor string representing the position
        /// of the specified item according to the provided ordering criteria.
        /// </summary>
        /// <param name="lastItem">The item from which to extract property values for cursor generation.</param>
        /// <param name="orderings">A list of ordering definitions that specify which properties of the item are included in the cursor and in what order.</param>
        /// <returns>A Base64-encoded string that encodes the property values of the item as defined by the ordering criteria.</returns>
        private string BuildCursor( T lastItem, List<CursorOrderInfo> orderings )
        {
            var cursor = new CursorToken();

            foreach ( var ord in orderings )
            {
                var value = ord.KeySelector.Compile().DynamicInvoke( lastItem );
                cursor.Values[ord.PropertyPath] = value;
            }

            // Encode it as base-64.
            var json = cursor.ToCamelCaseJson( false, true );

            // Encrypt the JSON to prevent tampering and viewing cursor values,
            // this will return in base-64.
            return Encryption.EncryptString( json );
        }

        /// <summary>
        /// Parses an expression representing an ordering chain and extracts
        /// the sequence of ordering operations as a list of cursor orders.
        /// </summary>
        /// <param name="orderBy">An expression that defines the ordering to apply to a queryable collection, composed of OrderBy and ThenBy method calls.</param>
        /// <returns>A list of cursor order information objects, each representing a key selector and sort direction extracted from the ordering expression.</returns>
        private static List<CursorOrderInfo> Parse( Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> orderBy )
        {
            var orderings = new List<CursorOrderInfo>();

            // The body of the expression is the full OrderBy/ThenBy chain
            Expression expr = orderBy.Body;

            while ( expr is MethodCallExpression mce && IsOrderingMethod( mce.Method.Name ) )
            {
                // Extract the lambda from the second argument
                var unary = ( UnaryExpression ) mce.Arguments[1];
                var lambda = ( LambdaExpression ) unary.Operand;

                // Extract the property name from the lambda body
                var member = ExtractMemberExpression( lambda.Body );
                orderings.Add( new CursorOrderInfo
                {
                    KeySelector = lambda,
                    Descending = mce.Method.Name.Contains( "Descending" ),
                    PropertyPath = GetPropertyPath( member )
                } );

                // Move to the next method in the chain
                expr = mce.Arguments[0];
            }

            // The chain is built inside-out, so reverse it
            orderings.Reverse();

            return orderings;
        }

        /// <summary>
        /// Determines whether the specified method name corresponds to a
        /// standard LINQ ordering method.
        /// </summary>
        /// <param name="name">The name of the method to evaluate.</param>
        /// <returns><c>true</c> if the specified name matches an expected ordering LINQ method; otherwise <c>false</c>.</returns>
        private static bool IsOrderingMethod( string name )
        {
            return name == nameof( Queryable.OrderBy )
                || name == nameof( Queryable.OrderByDescending )
                || name == nameof( Queryable.ThenBy )
                || name == nameof( Queryable.ThenByDescending );
        }

        /// <summary>
        /// Extracts a MemberExpression from the specified expression, if
        /// possible. This is used to get the property name from a lambda.
        /// </summary>
        /// <param name="expr">The expression from which to extract the MemberExpression.</param>
        /// <returns>The extracted MemberExpression from the provided expression.</returns>
        private static MemberExpression ExtractMemberExpression( Expression expr )
        {
            if ( expr is MemberExpression m )
            {
                return m;
            }
            else if ( expr is UnaryExpression u && u.Operand is MemberExpression m2 )
            {
                return m2;
            }
            else
            {
                throw new InvalidOperationException( $"Unsupported key selector expression: {expr}" );
            }
        }

        /// <summary>
        /// Gets the full property path from a MemberExpression, including nested
        /// properties. For example, for an expression like c => c.LeaderPersonAlias.Person.LastName,
        /// would return "LeaderPersonAlias.Person.LastName".
        /// </summary>
        /// <param name="member">The member expression to analyze.</param>
        /// <returns>The full property path.</returns>
        private static string GetPropertyPath( MemberExpression member )
        {
            var stack = new Stack<string>();
            Expression expr = member;

            while ( expr is MemberExpression m )
            {
                stack.Push( m.Member.Name );
                expr = m.Expression;
            }

            return string.Join( ".", stack );
        }

        #endregion

        #region Support Classes

        private sealed class CursorToken
        {
            public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();
        }

        #endregion
    }
}
