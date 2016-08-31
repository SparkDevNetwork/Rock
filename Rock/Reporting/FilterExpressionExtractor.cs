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
using System.Text;
using System.Threading.Tasks;
using Rock.Model;

namespace Rock.Reporting
{
    /// <summary>
    /// Helper class than can extract the "Where" clause Expression from an IQueryable
    /// </summary>
    public static class FilterExpressionExtractor
    {
        /// <summary>
        /// Helps rewrite the expression by replacing the parameter expression in the qry with another parameterExpression
        /// </summary>
        private class ParameterExpressionVisitor : ExpressionVisitor
        {
            private ParameterExpression _parameterExpression;
            private string _parameterName;

            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterExpressionVisitor"/> class.
            /// </summary>
            /// <param name="parameterExpression">The parameter expression.</param>
            /// <param name="parameterName">Name of the parameter.</param>
            public ParameterExpressionVisitor( ParameterExpression parameterExpression, string parameterName )
            {
                this._parameterExpression = parameterExpression;
                this._parameterName = parameterName;
            }

            /// <summary>
            /// Visits the parameter.
            /// </summary>
            /// <param name="p">The application.</param>
            /// <returns></returns>
            protected override Expression VisitParameter( ParameterExpression p )
            {
                if ( p.Name == _parameterName )
                {
                    p = _parameterExpression;
                }

                return base.VisitParameter( p );
            }
        }

        /// <summary>
        /// Extracts the "Where" clause Expression from an IQueryable
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="parameterExpression">The original parameter expression.</param>
        /// <param name="parameterName">Name of the parameter (forexample: 'p') from the qry to replace with the parameterExpression.</param>
        /// <returns></returns>
        public static Expression Extract<T>( IQueryable qry, ParameterExpression parameterExpression, string parameterName )
        {
            MethodCallExpression methodCallExpression = qry.Expression as MethodCallExpression;
            Expression<Func<LambdaExpression>> executionLambda = Expression.Lambda<Func<LambdaExpression>>( methodCallExpression.Arguments[1] );
            Expression extractedExpression = ( executionLambda.Compile().Invoke() as Expression<Func<T, bool>> ).Body;

            var filterExpressionVisitor = new ParameterExpressionVisitor( parameterExpression, parameterName );

            return filterExpressionVisitor.Visit( extractedExpression );
        }

        /// <summary>
        /// Alters the type of the comparison.
        /// </summary>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="compareEqualExpression">The compare equal expression.</param>
        /// <param name="blankValue">the value to compare equal to for IsBlank/IsNotBlank.</param>
        /// <returns></returns>
        public static BinaryExpression AlterComparisonType( ComparisonType comparisonType, BinaryExpression compareEqualExpression, object blankValue = null )
        {
            BinaryExpression result = compareEqualExpression;

            switch ( comparisonType )
            {
                case ComparisonType.EqualTo:
                    result = Expression.MakeBinary( ExpressionType.Equal, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.GreaterThan:
                    result = Expression.MakeBinary( ExpressionType.GreaterThan, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.GreaterThanOrEqualTo:
                    result = Expression.MakeBinary( ExpressionType.GreaterThanOrEqual, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.IsBlank:
                    result = Expression.MakeBinary( ExpressionType.Equal, compareEqualExpression.Left, Expression.Constant( blankValue, compareEqualExpression.Right.Type ) );
                    break;
                case ComparisonType.IsNotBlank:
                    result = Expression.MakeBinary( ExpressionType.NotEqual, compareEqualExpression.Left, Expression.Constant( blankValue, compareEqualExpression.Right.Type ) );
                    break;
                case ComparisonType.LessThan:
                    result = Expression.MakeBinary( ExpressionType.LessThan, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.LessThanOrEqualTo:
                    result = Expression.MakeBinary( ExpressionType.LessThanOrEqual, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.NotEqualTo:
                    result = Expression.MakeBinary( ExpressionType.NotEqual, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
            }

            return result;
        }
    }
}
