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
using System.Linq.Expressions;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Defines the information required to build a compound LINQ expression from
    /// multiple filters.
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.FilterExpression" />
    public class CompoundFilterExpression : FilterExpression
    {
        #region Properties

        /// <summary>
        /// Gets or sets the type of the expression grouping.
        /// </summary>
        /// <value>
        /// The type of the expression grouping.
        /// </value>
        public Model.FilterExpressionType ExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the child expressions.
        /// </summary>
        /// <value>
        /// The child expressions.
        /// </value>
        public List<FilterExpression> Filters { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundFilterExpression"/> class.
        /// </summary>
        public CompoundFilterExpression()
        {
            ExpressionType = Model.FilterExpressionType.GroupAny;
            Filters = new List<FilterExpression>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundFilterExpression"/> class.
        /// </summary>
        /// <param name="jobject">The JSON object that contains the data.</param>
        public CompoundFilterExpression( Newtonsoft.Json.Linq.JObject jobject )
            : this()
        {
            ExpressionType = ( Model.FilterExpressionType ) jobject.Value<int>( "ExpressionType" );
            foreach ( Newtonsoft.Json.Linq.JObject filter in jobject.Value<Newtonsoft.Json.Linq.JArray>( "Filters" ) )
            {
                Filters.Add( FilterExpression.FromJObject( filter ) );
            }

        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the expression that will be used to evaluate this comparison.
        /// </summary>
        /// <param name="property">The property that contains the value to be compared.</param>
        /// <returns>A LINQ Expression that will evaluate this comparison.</returns>
        public override Expression GetExpression( MemberExpression property )
        {
            Expression expression = null;

            if ( Filters.Count == 0 )
            {
                return Expression.Constant( true );
            }

            switch ( ExpressionType )
            {
                case Model.FilterExpressionType.GroupAll:
                case Model.FilterExpressionType.GroupAnyFalse:
                    foreach ( var comparison in Filters )
                    {
                        var expr = comparison.GetExpression( property );

                        if ( expression == null )
                        {
                            expression = expr;
                        }
                        else
                        {
                            expression = Expression.AndAlso( expression, expr );
                        }
                    }

                    if ( ExpressionType == Model.FilterExpressionType.GroupAnyFalse )
                    {
                        //
                        // If only one of the conditions must be false, invert the expression so
                        // that it becomes the logical equivalent of "NOT ALL".
                        //
                        expression = Expression.Not( expression );
                    }

                    return expression;

                case Model.FilterExpressionType.GroupAny:
                case Model.FilterExpressionType.GroupAllFalse:
                    foreach ( var comparison in Filters )
                    {
                        var expr = comparison.GetExpression( property );

                        if ( expression == null )
                        {
                            expression = expr;
                        }
                        else
                        {
                            expression = Expression.OrElse( expression, expr );
                        }
                    }

                    if ( ExpressionType == Model.FilterExpressionType.GroupAllFalse )
                    {
                        //
                        // If all of the conditions must be false, invert the expression so
                        // that it becomes the logical equivalent of "NOT ANY".
                        //
                        expression = Expression.Not( expression );
                    }

                    return expression;

                default:
                    throw new Exception( $"Unknown expression type { ExpressionType }" );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string operationWord;
            string prefixWord = string.Empty;

            switch ( ExpressionType )
            {
                case Model.FilterExpressionType.GroupAll:
                    operationWord = " And ";
                    break;

                case Model.FilterExpressionType.GroupAny:
                    operationWord = " Or ";
                    break;

                default:
                    operationWord = " ?? ";
                    break;
            }

            var text = string.Join( operationWord, Filters.Select( f => f.ToString() ) );

            if ( ExpressionType == Model.FilterExpressionType.GroupAllFalse || ExpressionType == Model.FilterExpressionType.GroupAnyFalse )
            {
                text = $"Not ({ text })";
            }

            return text;
        }

        #endregion
    }
}
