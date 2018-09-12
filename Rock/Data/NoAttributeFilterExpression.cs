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
using System.Linq.Expressions;

namespace Rock.Data
{
    /// <summary>
    /// Indicates that an Attribute Value Filter should not be used
    /// </summary>
    public class NoAttributeFilterExpression : Expression
    {
        /// <summary>
        /// Gets the node type of this <see cref="T:System.Linq.Expressions.Expression" />.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.IsTrue;

        /// <summary>
        /// Reduces the node and then calls the visitor delegate on the reduced expression. The method throws an exception if the node is not reducible.
        /// </summary>
        /// <param name="visitor">An instance of <see cref="T:System.Func`2" />.</param>
        /// <returns>
        /// The expression being visited, or an expression which should replace it in the tree.
        /// </returns>
        protected override Expression VisitChildren( ExpressionVisitor visitor ) => Expression.Constant( true );

        /// <summary>
        /// Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.
        /// </summary>
        public override Type Type => typeof( Boolean );

        /// <summary>
        /// Dispatches to the specific visit method for this node type. For example, <see cref="T:System.Linq.Expressions.MethodCallExpression" /> calls the <see cref="M:System.Linq.Expressions.ExpressionVisitor.VisitMethodCall(System.Linq.Expressions.MethodCallExpression)" />.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>
        /// The result of visiting this node.
        /// </returns>
        protected override Expression Accept( ExpressionVisitor visitor ) => Expression.Constant( true );
    }
}
