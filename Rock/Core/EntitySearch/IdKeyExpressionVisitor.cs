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
using Rock.Utility;

using System.Linq.Expressions;
using System.Reflection;

namespace Rock.Core.EntitySearch
{
    /// <summary>
    /// Expression visitor that handles IdKey requests for Entity Searches.
    /// </summary>
    class IdKeyExpressionVisitor : ExpressionVisitor
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
            // This method handles binary comparison operators. We need to check
            // for comparisons with IdKey and translate them into Id property
            // comparisons so that they are valid in SQL.

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
