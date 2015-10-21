// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace Rock.Reporting
{
    /// <summary>
    /// Helper class than can extract the first inner SELECT from an IQueryable. Useful for building DataSelect expressions for Reporting
    /// </summary>
    public static class SelectExpressionExtractor
    {
        /// <summary>
        /// Helps rewrite the expression by replacing the parameter expression in the qry with another parameterExpression
        /// </summary>
        private class PropertyParameterExpressionVisitor : ExpressionVisitor
        {
            private MemberExpression _propertyExpression;
            private string _parameterName;

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyParameterExpressionVisitor" /> class.
            /// </summary>
            /// <param name="propertyExpression">The property expression.</param>
            /// <param name="parameterName">Name of the parameter.</param>
            public PropertyParameterExpressionVisitor( MemberExpression propertyExpression, string parameterName )
            {
                this._propertyExpression = propertyExpression;
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
                    p = _propertyExpression.Expression as ParameterExpression;
                }

                return base.VisitParameter( p );
            }
        }

        /// <summary>
        /// Extracts the first inner SELECT from an IQueryable. Useful for building DataSelect expressions for Reporting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="qry">The qry.</param>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="parameterName">Name of the parameter (forexample: 'p') from the qry to replace with the parameterExpression.</param>
        /// <returns></returns>
        public static Expression Extract( IQueryable qry, MemberExpression propertyExpression, string parameterName )
        {
            MethodCallExpression methodCallExpression = qry.Expression as MethodCallExpression;
            Expression<Func<LambdaExpression>> executionLambda = Expression.Lambda<Func<LambdaExpression>>( methodCallExpression.Arguments[1] );
            Expression extractedExpression = ( executionLambda.Compile().Invoke()).Body;
            var propertyParameterExpressionVisitor = new PropertyParameterExpressionVisitor( propertyExpression, parameterName );

            return propertyParameterExpressionVisitor.Visit( extractedExpression );
        }

        [Obsolete("The Type Parameter <T> has no effect.")]
        public static Expression Extract<T>( IQueryable qry, MemberExpression propertyExpression, string parameterName )
        {
            return Extract( qry, propertyExpression, parameterName );
        }
    }
}
