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
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Reporting
{
    /// <summary>
    /// A group of filter rules that can be applied to any object type.
    /// </summary>
    [RockInternal( "1.16.6" )]
    internal class FieldFilterGroup
    {
        /// <summary>
        /// The set of rules in this group.
        /// </summary>
        public List<FieldFilterRule> Rules { get; set; } = new List<FieldFilterRule>();

        /// <summary>
        /// The type of the filter expression.
        /// </summary>
        public FilterExpressionType FilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// <para>
        /// Creates a new expression and evaluates it against the object.
        /// </para>
        /// <para>
        /// This is only valid for in-memory objects. If any attribute
        /// value rules exist, it is expected that LoadAttributes() would
        /// have already been called on <paramref name="instance"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of object to be evaluated.</typeparam>
        /// <param name="instance">The object instance to be evaluated against the rules.</param>
        /// <returns><c>true</c> if <paramref name="instance"/> matches the rules; otherwise <c>false</c>.</returns>
        public bool Evaluate<T>( T instance )
            where T : class
        {
            if ( instance == null )
            {
                // If no entity, then no match.
                return false;
            }

            var func = GetEvaluationFunction<T>();

            return func.Invoke( instance );
        }

        /// <summary>
        /// <para>
        /// Creates a new expression that represents the rules in this group
        /// and returns a function that can be called multiple times against
        /// different instances.
        /// </para>
        /// <para>
        /// This is only valid for in-memory objects. If any attribute value
        /// rules exist, it is expected that LoadAttributes() would have
        /// already been called on each instance passed to the function.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of object to be evaluated.</typeparam>
        /// <returns>A function that takes an instance of <typeparamref name="T"/> and returns <c>true</c> if it matches the rules.</returns>
        public Func<T, bool> GetEvaluationFunction<T>()
        {
            var entityType = typeof( T );

            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

            var parameterExpression = Expression.Parameter( entityType, "p" );
            var expression = GetExpression( parameterExpression, null );
            var lambda = Expression.Lambda<Func<T, bool>>( expression, parameterExpression );

            return lambda.Compile();
        }

        /// <summary>
        /// <para>
        /// Gets a LINQ expression that can be used to evaluate objects
        /// to see if they match the defined rules.
        /// </para>
        /// <para>
        /// This method will only return expressions that are valid with
        /// in-memory comparisons of objects. They should not be used with
        /// LINQ to SQL statements.
        /// </para>
        /// </summary>
        /// <param name="parameterExpression">The expression that will identity the object instance to be evaluated.</param>
        /// <returns>An expression that evaluates instances of <paramref name="parameterExpression"/>.</returns>
        public Expression GetMemoryExpression( ParameterExpression parameterExpression )
        {
            if ( parameterExpression == null )
            {
                throw new ArgumentNullException( nameof( parameterExpression ) );
            }

            return GetExpression( parameterExpression, null );
        }

        /// <summary>
        /// <para>
        /// Gets a LINQ expression that can be used to evaluate objects
        /// to see if they match the defined rules.
        /// </para>
        /// <para>
        /// This method will only return expressions that are valid with
        /// database queries as part of <paramref name="rockContext"/>.
        /// </para>
        /// </summary>
        /// <param name="parameterExpression">The expression that will identity the object instance to be evaluated.</param>
        /// <param name="rockContext">The database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="parameterExpression"/>.</returns>
        public Expression GetDatabaseExpression( ParameterExpression parameterExpression, RockContext rockContext )
        {
            if ( parameterExpression == null )
            {
                throw new ArgumentNullException( nameof( parameterExpression ) );
            }

            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            return GetExpression( parameterExpression, rockContext );
        }

        /// <summary>
        /// Gets a LINQ expression that can be used to evaluate objects
        /// to see if they match the defined rules.
        /// </summary>
        /// <param name="parameterExpression">The expression that will identify the object instance to be evaluated.</param>
        /// <param name="rockContext">If <c>null</c> then an in-memory expression will be created; otherwise this specifies the database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="parameterExpression"/>.</returns>
        private Expression GetExpression( ParameterExpression parameterExpression, RockContext rockContext )
        {
            var conditionResults = Rules
                .Select( rule => rule.GetExpression( parameterExpression, rockContext ) )
                .ToList();

            if ( !conditionResults.Any() )
            {
                // If there were not any rules, then return a match.
                return Expression.Constant( true );
            }

            var finalExpression = conditionResults.First();

            foreach ( var expression in conditionResults.Skip( 1 ) )
            {
                switch ( FilterExpressionType )
                {
                    case FilterExpressionType.GroupAll:
                    case FilterExpressionType.GroupAllFalse:
                        finalExpression = Expression.AndAlso( finalExpression, expression );
                        break;

                    case FilterExpressionType.GroupAny:
                    case FilterExpressionType.GroupAnyFalse:
                        finalExpression = Expression.OrElse( finalExpression, expression );
                        break;
                }
            }

            if ( FilterExpressionType == FilterExpressionType.GroupAllFalse || FilterExpressionType == FilterExpressionType.GroupAnyFalse )
            {
                return Expression.Not( finalExpression );
            }

            return finalExpression;
        }
    }
}