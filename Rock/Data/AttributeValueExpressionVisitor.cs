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
            /*
                11/13/26 - CLAUDE

                Only equality comparisons are rewritten. Two values that are
                equal always have the same checksum, so adding a checksum check
                to an equality comparison narrows the rows without losing any
                matches. Two values that are different can still have the same
                checksum, so the checksum cannot help a not-equal comparison,
                and that comparison matches most rows anyway so an index seek
                would not be used.

                Reason: Rewriting a not-equal comparison returned the wrong rows.
            */
            if ( node.NodeType != ExpressionType.Equal )
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

                return AddChecksumComparison( node, attributeValueExpression, node.Right );
            }

            if ( IsAttributeValueMember( node.Right ) )
            {
                // This should never be the case, but this covers any strange things.
                if ( !( node.Right is MemberExpression attributeValueExpression ) || attributeValueExpression.Expression == null )
                {
                    return base.VisitBinary( node );
                }

                return AddChecksumComparison( node, attributeValueExpression, node.Left );
            }

            return base.VisitBinary( node );
        }

        /// <summary>
        /// Creates an expression that requires both the original comparison and
        /// a comparison of the ValueChecksum column against the checksum of the
        /// other operand to match.
        /// </summary>
        /// <param name="node">The original equality comparison.</param>
        /// <param name="valueExpression">The expression that accesses the Value property.</param>
        /// <param name="otherOperand">The expression the Value property is being compared to.</param>
        /// <returns>The original comparison combined with the checksum comparison.</returns>
        private static Expression AddChecksumComparison( BinaryExpression node, MemberExpression valueExpression, Expression otherOperand )
        {
            var checksumMember = Expression.Property( valueExpression.Expression, "ValueChecksum" );
            Expression checksumCall = Expression.Call( null, _checksumMethod, otherOperand );

            /*
                11/13/26 - CLAUDE

                SqlFunctions.Checksum() returns int?, but ValueChecksum is int on
                AttributeValue and int? on QueryableAttributeValue. Expression.Equal
                throws if the two types do not match. The function call is converted
                rather than the column so the column can still be used by the index.

                Reason: Some comparison orders threw when the query was built.
            */
            if ( checksumCall.Type != checksumMember.Type )
            {
                checksumCall = Expression.Convert( checksumCall, checksumMember.Type );
            }

            return Expression.AndAlso( node, Expression.Equal( checksumMember, checksumCall ) );
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
