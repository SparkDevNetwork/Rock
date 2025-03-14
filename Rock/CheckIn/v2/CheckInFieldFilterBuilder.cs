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
using Rock.Web.Cache;

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

        /// <summary>
        /// The MethodInfo that describes the Enumerable.Any method taking
        /// a <see cref="int"/> as a generic type.
        /// </summary>
        private static readonly Lazy<MethodInfo> _anyIntegerMethod = new Lazy<MethodInfo>( () => typeof( Enumerable )
            .GetMethods()
            .Where( m => m.Name == nameof( Enumerable.Any )
                && m.GetParameters().Length == 2
                && m.GetParameters()[1].ParameterType.IsGenericType
                && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof( Func<,> ) )
            .FirstOrDefault()
            .MakeGenericMethod( typeof( int ) ) );

        /// <inheritdoc/>
        protected override Expression GetRulePropertyExpression( Expression instanceExpression, FieldFilterRuleBag rule, RockContext rockContext )
        {
            // If the instance is an entity, then we don't need to do
            // anything special. We should never be called with a RockContext
            // but bail out just in case.
            if ( typeof( Data.IEntity ).IsAssignableFrom( instanceExpression.Type ) || rockContext != null )
            {
                // Special handling of certain properties.
                if ( instanceExpression.Type == typeof( Model.Person ) )
                {
                    if ( rule.PropertyName == nameof( Model.Person.AgePrecise ) )
                    {
                        return GetAgePreciseExpression( instanceExpression, rule, rockContext );
                    }
                    else if ( rule.PropertyName == nameof( Model.Person.GradeOffset ) )
                    {
                        return GetGradeOffsetExpression( instanceExpression, rule, rockContext );
                    }
                }

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
                // to run the normal text expression on all values in the
                // collection and return true if any of them match. We also
                // need to convert everything to lower case so so that the
                // comparisons happen case-insensitive.
                var filterValues = new List<string>
                {
                    rule.ComparisonType.ConvertToString( false ),
                    rule.Value?.ToLower()
                };

                var propertyExpression = Expression.Property( instanceExpression, property );

                // Create an expression patterned like property.Any( (s) => s.ToLower() == value )
                // and then pass that value to the property filter expression.
                // The actual conditional expression will be provided by the
                // PropertyFilterExpression method.
                var innerParameterExpression = Expression.Parameter( typeof( string ), "s" );
                var lowerStringExpression = Expression.Call( innerParameterExpression, nameof( string.ToLower ), Type.EmptyTypes );
                var propertyFilterExpression = ExpressionHelper.PropertyFilterExpression( filterValues, lowerStringExpression );
                var propertyFilterFunc = Expression.Lambda<Func<string, bool>>( propertyFilterExpression, innerParameterExpression );

                return Expression.Call( _anyStringMethod.Value, propertyExpression, propertyFilterFunc );
            }
            else if ( typeof( ICollection<int> ).IsAssignableFrom( property.PropertyType ) )
            {
                // If the property is a collection of integers, then we want
                // to run the normal integer expression on all values in the
                // collection and return true if any of them match.
                var filterValues = new List<string>
                {
                    rule.ComparisonType.ConvertToString( false ),
                    rule.Value
                };

                var propertyExpression = Expression.Property( instanceExpression, property );

                // Create an expression patterned like property.Any( (v) => v == value )
                // and then pass that value to the property filter expression.
                // The actual conditional expression will be provided by the
                // PropertyFilterExpression method.
                var innerParameterExpression = Expression.Parameter( typeof( int ), "v" );
                var propertyFilterExpression = ExpressionHelper.PropertyFilterExpression( filterValues, innerParameterExpression );
                var propertyFilterFunc = Expression.Lambda<Func<int, bool>>( propertyFilterExpression, innerParameterExpression );

                return Expression.Call( _anyIntegerMethod.Value, propertyExpression, propertyFilterFunc );
            }


            return base.GetRulePropertyExpression( instanceExpression, rule, rockContext );
        }

        /// <summary>
        /// Gets the expression that will be used to evaluate a rule for the
        /// <see cref="Model.Person.GradeOffset"/> property.
        /// </summary>
        /// <param name="instanceExpression">The expression that will identify the object instance to be evaluated. This will already point to the object specified by the rule path.</param>
        /// <param name="rule">The object that contains the filter rule information.</param>
        /// <param name="rockContext">If <c>null</c> then an in-memory expression will be created; otherwise this specifies the database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="instanceExpression"/>.</returns>
        private Expression GetGradeOffsetExpression( Expression instanceExpression, FieldFilterRuleBag rule, RockContext rockContext )
        {
            var gradeOffsetProperty = typeof( Model.Person ).GetProperty( nameof( Model.Person.GradeOffset ) );
            var entityField = new EntityField( gradeOffsetProperty.Name, FieldKind.Property, gradeOffsetProperty )
            {
                FieldType = FieldTypeCache.Get( SystemGuid.FieldType.INTEGER.AsGuid(), rockContext )
            };

            return GetEntityFieldRulePropertyExpression( entityField, instanceExpression, rule, rockContext );
        }

        /// <summary>
        /// Gets the expression that will be used to evaluate a rule for the
        /// <see cref="Model.Person.AgePrecise"/> property.
        /// </summary>
        /// <param name="instanceExpression">The expression that will identify the object instance to be evaluated. This will already point to the object specified by the rule path.</param>
        /// <param name="rule">The object that contains the filter rule information.</param>
        /// <param name="rockContext">If <c>null</c> then an in-memory expression will be created; otherwise this specifies the database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="instanceExpression"/>.</returns>
        private Expression GetAgePreciseExpression( Expression instanceExpression, FieldFilterRuleBag rule, RockContext rockContext )
        {
            var agePreciseProperty = typeof( Model.Person ).GetProperty( nameof( Model.Person.AgePrecise ) );
            var entityField = new EntityField( agePreciseProperty.Name, FieldKind.Property, agePreciseProperty )
            {
                FieldType = FieldTypeCache.Get( SystemGuid.FieldType.DECIMAL.AsGuid(), rockContext )
            };

            return GetEntityFieldRulePropertyExpression( entityField, instanceExpression, rule, rockContext );
        }
    }
}
