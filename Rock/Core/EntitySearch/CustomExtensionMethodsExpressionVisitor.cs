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
using System.Linq.Expressions;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Core.EntitySearch
{
    /// <summary>
    /// Expression visitor that handles translating our custom Entity Search
    /// methods into ones that SQL will be able to parse.
    /// </summary>
    class CustomExtensionMethodsExpressionVisitor : ExpressionVisitor
    {
        #region Fields

        /// <summary>
        /// The rock context to use when we need to access the database to
        /// get additionald ata.
        /// </summary>
        private readonly RockContext _rockContext;

        /// <summary>
        /// The cached data view identifier queryables for this query. This
        /// allows us to re-use the same queryable if it is used in different
        /// parts of the Where or Select clauses.
        /// </summary>
        private readonly Dictionary<(Type, int), IQueryable<int>> _dataViewIdQueryables = new Dictionary<(Type, int), IQueryable<int>>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomExtensionMethodsExpressionVisitor"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        public CustomExtensionMethodsExpressionVisitor( RockContext rockContext )
        {
            _rockContext = rockContext;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Expression VisitMethodCall( MethodCallExpression node )
        {
            if ( node.Method.Name == nameof( DynamicLinqQueryExtensionMethods.IsInDataView ) )
            {
                var entityExpression = node.Arguments[0];
                var dataViewIdExpression = node.Arguments[1];

                if ( !( dataViewIdExpression is ConstantExpression constExpression ) || !( constExpression.Value is int dataViewId ) )
                {
                    throw new Exception( "InDataView() must take a constant integer value." );
                }

                var idQry = GetDataViewIdQueryable( entityExpression.Type, dataViewId );

                // Get the method to be called: idQry.Contains( a.Id )
                var containsMethod = typeof( Queryable ).GetMethods()
                    .Where( m => m.Name == nameof( Queryable.Contains ) && m.GetParameters().Length == 2 )
                    .Single();
                var containsGenericMethod = containsMethod.MakeGenericMethod( typeof( int ) );

                // Define the parameters for the method call.
                var idQryExpression = Expression.Constant( idQry );
                var entityIdExpression = Expression.Property( entityExpression, "Id" );

                return Expression.Call( containsGenericMethod, idQryExpression, entityIdExpression );
            }

            return base.VisitMethodCall( node );
        }

        /// <summary>
        /// Gets the data view identifier queryable. This will use the cache
        /// if possible, otherwise it attempts to hit the database.
        /// </summary>
        /// <param name="entityType">Type of the entity to be checked against the data view.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns>A queryable of the Id values in the data view.</returns>
        private IQueryable<int> GetDataViewIdQueryable( Type entityType, int dataViewId )
        {
            if ( _dataViewIdQueryables.TryGetValue( (entityType, dataViewId), out var idQry ) )
            {
                return idQry;
            }

            var dataView = new DataViewService( _rockContext ).Get( dataViewId );

            if ( dataView == null )
            {
                throw new Exception( $"Data view '{dataViewId}' was not found." );
            }

            if ( !dataView.EntityTypeId.HasValue )
            {
                throw new Exception( $"Data view '{dataViewId}' was expected to be for type '{entityType.Name}' but was not specified by the data view." );
            }

            var dataViewEntityType = EntityTypeCache.Get( dataView.EntityTypeId.Value )?.GetEntityType();

            if ( dataViewEntityType != entityType )
            {
                throw new Exception( $"Data view '{dataViewId}' was expected to be for type '{entityType.Name}' but was actually for type '{dataViewEntityType?.Name}'." );
            }

            var qry = dataView.GetQuery( new DataViewGetQueryArgs
            {
                DbContext = _rockContext
            } );

            idQry = qry.Select( a => a.Id );
            _dataViewIdQueryables.Add( (entityType, dataViewId), idQry );

            return idQry;
        }

        #endregion
    }
}
