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
using System.Linq.Expressions;
using System.Reflection;

using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Ensures that the BulkUpdate expression also updates ModifiedDateTime and ModifiedByPersonAliasId
    /// </summary>
    /// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
    internal class RockBulkUpdateExpressionVisitor : ExpressionVisitor
    {
        private DateTime _currentDateTime;
        private PersonAlias _currentPersonAlias;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockBulkUpdateExpressionVisitor"/> class.
        /// </summary>
        /// <param name="currentDateTime">The current date time.</param>
        /// <param name="currentPersonAlias">The current person alias.</param>
        public RockBulkUpdateExpressionVisitor( DateTime currentDateTime, PersonAlias currentPersonAlias )
        {
            _currentDateTime = currentDateTime;
            _currentPersonAlias = currentPersonAlias;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberInitExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitMemberInit( MemberInitExpression node )
        {
            if ( typeof( Rock.Data.IModel ).IsAssignableFrom( node.Type ) )
            {
                var currentBindings = node.Bindings.ToList();
                if ( !currentBindings.Any( a => a.Member.Name == "ModifiedDateTime" ) )
                {
                    MemberInfo modifiedDateTypeMemberInfo = typeof( Rock.Data.IModel ).GetMember( "ModifiedDateTime" ).FirstOrDefault();
                    if ( modifiedDateTypeMemberInfo != null )
                    {
                        currentBindings.Add( Expression.Bind( modifiedDateTypeMemberInfo, Expression.Constant( _currentDateTime ) ) );
                    }

                    MemberInfo modifiedByPersonAliasIdMemberInfo = typeof( Rock.Data.IModel ).GetMember( "ModifiedByPersonAliasId" ).FirstOrDefault();
                    if ( modifiedByPersonAliasIdMemberInfo != null && _currentPersonAlias != null )
                    {
                        currentBindings.Add( Expression.Bind( modifiedByPersonAliasIdMemberInfo, Expression.Constant( _currentPersonAlias.Id ) ) );
                    }

                    node = node.Update( node.NewExpression, currentBindings );
                }
            }

            return base.VisitMemberInit( node );
        }
    }
}
