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
using System.Data.Entity.SqlServer;
using System.Linq.Expressions;
using System.Reflection;

using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Expression visitor that modifies comparisons on attribute values to
    /// also check against the checksum values which are indexed.
    /// </summary>
    internal class AttributeValueExpressionVisitor : ExpressionVisitor
    {
        #region Fields

        /// <summary>
        /// A reference to the SQL Server Checksum() function call that will
        /// be inserted into comparisons.
        /// </summary>
        private static readonly MethodInfo _checksumMethod = typeof( SqlFunctions ).GetMethod( nameof( SqlFunctions.Checksum ), new Type[] { typeof( string ) } );

        #endregion

        /// <inheritdoc/>
        protected override Expression VisitBinary( BinaryExpression node )
        {
            // ValueChecksum is only valid for equals and not equals comparisons.
            if ( node.NodeType != ExpressionType.Equal && node.NodeType != ExpressionType.NotEqual )
            {
                return base.VisitBinary( node );
            }

            if ( IsAttributeValueMember( node.Left ) )
            {
                // This should never be the case, but this covers any strange things.
                if ( !( node.Left is MemberExpression attributeValueExpression ) || attributeValueExpression.Expression == null )
                {
                    return base.VisitBinary( node );
                }

                // Create an expression that compares the specified value against ValueChecksum.
                var memberExpression = Expression.Property( attributeValueExpression.Expression, "ValueChecksum" );
                var callExpression = Expression.Call( null, _checksumMethod, node.Right );
                var checksumExpression = Expression.Equal( memberExpression, Expression.Convert( callExpression, typeof( int ) ) );

                // Return a new expression that says the original check and the
                // ValueChecksum check must match.
                return Expression.AndAlso( node, checksumExpression );
            }

            if ( IsAttributeValueMember( node.Right ) )
            {
                // This should never be the case, but this covers any strange things.
                if ( !( node.Right is MemberExpression attributeValueExpression ) || attributeValueExpression.Expression == null )
                {
                    return base.VisitBinary( node );
                }

                // Create an expression that compares the specified value against ValueChecksum.
                var memberExpression = Expression.Property( attributeValueExpression.Expression, "ValueChecksum" );
                var callExpression = Expression.Call( null, _checksumMethod, node.Left );
                var checksumExpression = Expression.Equal( memberExpression, callExpression );

                // Return a new expression that says the original check and the
                // ValueChecksum check must match.
                return Expression.AndAlso( node, checksumExpression );
            }

            return base.VisitBinary( node );
        }

        /// <summary>
        /// Determines if the expression is one that accesses the Value property
        /// of either AttributeValue or QueryableAttributeValue.
        /// </summary>
        /// <param name="expression">The expression to be checked.</param>
        /// <returns><c>true</c> if the expresion matches the expected pattern; <c>false</c> otherwise.</returns>
        private static bool IsAttributeValueMember( Expression expression )
        {
            var isAttributeValueExpression = expression is MemberExpression memberExpression
                && typeof( AttributeValue ).IsAssignableFrom( memberExpression.Member.ReflectedType )
                && memberExpression.Member.Name == nameof( AttributeValue.Value );

            if ( isAttributeValueExpression )
            {
                return true;
            }

            var isEntityAttributeValueExpression = expression is MemberExpression entityMemberExpression
                && typeof( QueryableAttributeValue ).IsAssignableFrom( entityMemberExpression.Member.ReflectedType )
                && entityMemberExpression.Member.Name == nameof( QueryableAttributeValue.Value );

            return isEntityAttributeValueExpression;
        }
    }
}
