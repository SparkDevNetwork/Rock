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
using System.Reflection;

using Rock.Data;
using Rock.Reporting;
using Rock.Utility;
using Rock.ViewModels.Reporting;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Special implementation of <see cref="FieldFilterExpressionBuilder"/>
    /// that adds support for the additional property types used in check-in
    /// labels that can be filtered on.
    /// </summary>
    internal class CheckInFieldFilterBuilder : FieldFilterExpressionBuilder
    {
        /// <summary>
        /// The MethodInfo that describes the Enumerable.Any method taking
        /// a <see cref="string"/> as a generic type.
        /// </summary>
        private static readonly Lazy<MethodInfo> _anyStringMethod = new Lazy<MethodInfo>( () => typeof( Enumerable )
            .GetMethods()
            .Where( m => m.Name == nameof( Enumerable.Any )
                && m.GetParameters().Length == 2
                && m.GetParameters()[1].ParameterType.IsGenericType
                && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof( Func<,> ) )
            .FirstOrDefault()
            .MakeGenericMethod( typeof( string ) ) );

        /// <inheritdoc/>
        protected override Expression GetRulePropertyExpression( Expression instanceExpression, FieldFilterRuleBag rule, RockContext rockContext )
        {
            // If the instance is an entity, then we don't need to do
            // anything special. We should never be called with a RockContext
            // but bail out just in case.
            if ( typeof( Data.IEntity ).IsAssignableFrom( instanceExpression.Type ) || rockContext != null )
            {
                return base.GetRulePropertyExpression( instanceExpression, rule, rockContext );
            }

            var property = instanceExpression.Type.GetProperty( rule.PropertyName );

            // If property was not found, return an expression that
            // never matches.
            if ( property == null )
            {
                return Expression.Constant( false );
            }

            if ( typeof( ICollection<string> ).IsAssignableFrom( property.PropertyType ) )
            {
                // If the property is a collection of strings, then we want
                // to run the normal text expression on all strings in the
                // collection and return true if any of them match. We also
                // need to convert everything to lower case so so that the
                // comparisons happen case-insensitive.
                var filterValues = new List<string>
                    {
                        rule.ComparisonType.ConvertToString( false ),
                        rule.Value?.ToLower()
                    };

                var propertyExpression = Expression.Property( instanceExpression, property );

                // Create an expression for prop.Any( (s) => s.ToLower() == value ) and then pass
                // that value to the property filter expression.
                var innerParameterExpression = Expression.Parameter( typeof( string ), "s" );
                var lowerStringExpression = Expression.Call( innerParameterExpression, nameof( string.ToLower ), Type.EmptyTypes );
                var propertyFilterExpression = ExpressionHelper.PropertyFilterExpression( filterValues, lowerStringExpression );
                var propertyFilterFunc = Expression.Lambda<Func<string, bool>>( propertyFilterExpression, innerParameterExpression );

                return Expression.Call( _anyStringMethod.Value, propertyExpression, propertyFilterFunc );
            }

            return base.GetRulePropertyExpression( instanceExpression, rule, rockContext );
        }
    }
}
