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

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Reporting;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Reporting;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    /// Builds LINQ expressions for field filters from a <see cref="FieldFilterGroupBag"/>
    /// or <see cref="FieldFilterRuleBag"/> object. These expression can be
    /// built for either database queries or in-memory object queries.
    /// </summary>
    internal class FieldFilterExpressionBuilder
    {
        /// <summary>
        /// <para>
        /// Creates a new expression and evaluates it against the object to
        /// determine if the object matches the filter.
        /// </para>
        /// <para>
        /// If any attribute value rules exist, it is expected that
        /// LoadAttributes() would have already been called on
        /// <paramref name="instance"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of object to be evaluated.</typeparam>
        /// <param name="instance">The object instance to be evaluated against the rules.</param>
        /// <param name="filter">The object that contains the filter data.</param>
        /// <returns><c>true</c> if <paramref name="instance"/> matches the rules; otherwise <c>false</c>.</returns>
        public bool IsMatch<T>( T instance, FieldFilterGroupBag filter )
            where T : class
        {
            if ( instance == null )
            {
                // If no entity, then no match.
                return false;
            }

            var func = GetIsMatchFunction<T>( filter );

            return func.Invoke( instance );
        }

        /// <summary>
        /// <para>
        /// Creates a new expression that represents the rules in this group
        /// and returns a function that represents the expression. This function
        /// can be called multiple times against different object instances and
        /// will return a value that determines if the object matches the rules.
        /// </para>
        /// <para>
        /// If any attribute value rules exist, it is expected that
        /// LoadAttributes() would have already been called on each instance
        /// passed to the function.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of object to be evaluated.</typeparam>
        /// <param name="filter">The object that contains the filter data.</param>
        /// <returns>A function that takes an instance of <typeparamref name="T"/> and returns <c>true</c> if it matches the rules.</returns>
        public Func<T, bool> GetIsMatchFunction<T>( FieldFilterGroupBag filter )
        {
            var entityType = typeof( T );

            var parameterExpression = Expression.Parameter( entityType, "p" );
            var expression = GetGroupExpression( parameterExpression, filter, null );
            var lambda = Expression.Lambda<Func<T, bool>>( expression, parameterExpression );

            return lambda.Compile();
        }

        /// <summary>
        /// <para>
        /// Creates a new expression that represents the rules in this group
        /// and returns a function that represents the expression. This function
        /// can be called multiple times against different object instances and
        /// will return a value that determines if the object matches the rules.
        /// </para>
        /// <para>
        /// If the object passed to the match function cannot be cast to
        /// <paramref name="entityType"/> then <c>false</c> will be returned by
        /// the function.
        /// </para>
        /// <para>
        /// If any attribute value rules exist, it is expected that
        /// LoadAttributes() would have already been called on each instance
        /// passed to the function.
        /// </para>
        /// </summary>
        /// <param name="filter">The object that contains the filter data.</param>
        /// <param name="entityType">The type of the instance that is expected to be passed to the function.</param>
        /// <returns>A function that takes an instance of type <paramref name="entityType"/> and returns <c>true</c> if it matches the rules.</returns>
        public Func<object, bool> GetIsMatchFunction( FieldFilterGroupBag filter, Type entityType )
        {
            var parameterExpression = Expression.Parameter( typeof( object ), "p" );
            var castedParameterExpression = Expression.TypeAs( parameterExpression, entityType );
            var expression = GetGroupExpression( castedParameterExpression, filter, null );

            // return (p as entitType) == null ? false : expression(p)
            var nullCheckExpression = Expression.Equal( castedParameterExpression, Expression.Constant( null ) );
            var ifNullExpression = Expression.Condition( nullCheckExpression, Expression.Constant( false ), expression );

            var lambda = Expression.Lambda<Func<object, bool>>( ifNullExpression, parameterExpression );

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
        /// <para>
        /// You probably want to call <see cref="GetIsMatchFunction{T}(FieldFilterGroupBag)"/> instead.
        /// </para>
        /// </summary>
        /// <param name="instanceExpression">The expression that will identity the object instance to be evaluated.</param>
        /// <param name="filter">The object that contains the filter data.</param>
        /// <returns>An expression that evaluates instances of <paramref name="instanceExpression"/>.</returns>
        public Expression GetMemoryExpression( Expression instanceExpression, FieldFilterGroupBag filter )
        {
            if ( instanceExpression == null )
            {
                throw new ArgumentNullException( nameof( instanceExpression ) );
            }

            return GetGroupExpression( instanceExpression, filter, null );
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
        /// <param name="instanceExpression">The expression that will identity the object instance to be evaluated.</param>
        /// <param name="filter">The object that contains the filter data.</param>
        /// <param name="rockContext">The database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="instanceExpression"/>.</returns>
        public Expression GetDatabaseExpression( Expression instanceExpression, FieldFilterGroupBag filter, RockContext rockContext )
        {
            if ( instanceExpression == null )
            {
                throw new ArgumentNullException( nameof( instanceExpression ) );
            }

            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            return GetGroupExpression( instanceExpression, filter, rockContext );
        }

        /// <summary>
        /// Gets a LINQ expression that can be used to evaluate objects
        /// to see if they match the defined rules.
        /// </summary>
        /// <param name="instanceExpression">The expression that will identify the object instance to be evaluated.</param>
        /// <param name="filter">The object that contains the filter data.</param>
        /// <param name="rockContext">If <c>null</c> then an in-memory expression will be created; otherwise this specifies the database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="instanceExpression"/>.</returns>
        protected virtual Expression GetGroupExpression( Expression instanceExpression, FieldFilterGroupBag filter, RockContext rockContext )
        {
            var conditionResults = filter.Rules
                .Select( rule => GetRuleExpression( instanceExpression, rule, rockContext ) )
                .ToList();

            if ( !conditionResults.Any() )
            {
                // If there were not any rules, then return a match.
                return Expression.Constant( true );
            }

            var finalExpression = conditionResults.First();

            foreach ( var expression in conditionResults.Skip( 1 ) )
            {
                switch ( filter.ExpressionType )
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

            if ( filter.ExpressionType == FilterExpressionType.GroupAllFalse || filter.ExpressionType == FilterExpressionType.GroupAnyFalse )
            {
                return Expression.Not( finalExpression );
            }

            return finalExpression;
        }

        /// <summary>
        /// Gets a LINQ expression that can be used to evaluate objects
        /// to see if they match the defined rules.
        /// </summary>
        /// <param name="instanceExpression">The expression that will identify the object instance to be evaluated.</param>
        /// <param name="rule">The object that contains the filter rule information.</param>
        /// <param name="rockContext">If <c>null</c> then an in-memory expression will be created; otherwise this specifies the database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="instanceExpression"/>.</returns>
        protected virtual Expression GetRuleExpression( Expression instanceExpression, FieldFilterRuleBag rule, RockContext rockContext )
        {
            instanceExpression = GetExpressionForPath( instanceExpression, rule.Path );

            if ( rule.SourceType == FieldFilterSourceType.Property && rule.PropertyName.IsNotNullOrWhiteSpace() )
            {
                return GetRulePropertyExpression( instanceExpression, rule, rockContext );
            }
            else if ( rule.SourceType == FieldFilterSourceType.Attribute && rule.AttributeGuid.HasValue )
            {
                return GetRuleAttributeExpression( instanceExpression, rule, rockContext );
            }
            else
            {
                // The rule was not fully configured, so don't use it to filter
                // results. This matches the DataView logic.
                // See: Rock.Reporting.DataFilter.PropertyFilter.GetExpression().
                return Expression.Constant( true );
            }
        }

        /// <summary>
        /// Gets the new instance expression after traversing down the properties
        /// specified in <paramref name="path"/>.
        /// </summary>
        /// <param name="instanceExpression">The expression that will identify the initial object instance.</param>
        /// <param name="path">A set of path components separated by periods.</param>
        /// <returns>A new expression that references the property at the path or the original expression if no path was specified.</returns>
        protected virtual Expression GetExpressionForPath( Expression instanceExpression, string path )
        {
            if ( path.IsNotNullOrWhiteSpace() )
            {
                foreach ( var pathComponent in path.Split( '.' ) )
                {
                    instanceExpression = Expression.Property( instanceExpression, pathComponent );
                }
            }

            return instanceExpression;
        }

        /// <summary>
        /// Gets the expression that will be used to evaluate a rule that
        /// applies to a property.
        /// </summary>
        /// <param name="instanceExpression">The expression that will identify the object instance to be evaluated. This will already point to the object specified by the rule path.</param>
        /// <param name="rule">The object that contains the filter rule information.</param>
        /// <param name="rockContext">If <c>null</c> then an in-memory expression will be created; otherwise this specifies the database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="instanceExpression"/>.</returns>
        protected virtual Expression GetRulePropertyExpression( Expression instanceExpression, FieldFilterRuleBag rule, RockContext rockContext )
        {
            var entityField = EntityHelper.GetEntityField( instanceExpression.Type, EntityHelper.MakePropertyNameUnique( rule.PropertyName ), false, true );

            // If the attribute was not found, then throw an exception because
            // filter is no longer valid. This matches the DataView logic.
            // See: Rock.Reporting.DataFilter.PropertyFilter.GetExpression().
            if ( entityField == null )
            {
                throw new RockDataViewFilterExpressionException( $"Rule filter refers to property that doesn't exist: {rule.PropertyName}." );
            }

            var filterValues = new List<string>( 2 );

            // Only add the comparisonTypeValue if it is specified,
            // just like the logic at https://github.com/SparkDevNetwork/Rock/blob/22f64416b2461c8a988faf4b6e556bc3dcb209d3/Rock/Field/FieldType.cs#L558
            var comparisonTypeValue = rule.ComparisonType.ConvertToString( false );
            if ( comparisonTypeValue != null )
            {
                filterValues.Add( comparisonTypeValue );
            }

            // If we are not going to be using this expression with
            // the database then we need to do some special logic for
            // text style filters to make them case-insensitive.
            if ( rockContext == null && entityField.FieldType.Field is Rock.Field.Types.TextFieldType )
            {
                filterValues.Add( rule.Value?.ToLower() );

                Expression propertyExpression = Expression.Property( instanceExpression, entityField.Name );
                propertyExpression = Expression.Call( propertyExpression, nameof( string.ToLower ), Type.EmptyTypes );

                return ExpressionHelper.PropertyFilterExpression( filterValues, propertyExpression );
            }

            filterValues.Add( rule.Value );

            return entityField.FieldType.Field.PropertyFilterExpression( entityField.FieldConfig, filterValues, instanceExpression, entityField.Name, entityField.PropertyType );
        }

        /// <summary>
        /// Gets the expression that will be used to evaluate a rule that
        /// applies to an attribute value.
        /// </summary>
        /// <param name="instanceExpression">The expression that will identify the object instance to be evaluated. This will already point to the object specified by the rule path.</param>
        /// <param name="rule">The object that contains the filter rule information.</param>
        /// <param name="rockContext">If <c>null</c> then an in-memory expression will be created; otherwise this specifies the database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="instanceExpression"/>.</returns>
        protected virtual Expression GetRuleAttributeExpression( Expression instanceExpression, FieldFilterRuleBag rule, RockContext rockContext )
        {
            // If instance type does not support attributes then always
            // return no match.
            if ( !typeof( IHasAttributes ).IsAssignableFrom( instanceExpression.Type ) )
            {
                return Expression.Constant( false );
            }

            var comparedToAttribute = AttributeCache.Get( rule.AttributeGuid.Value );

            // If the attribute was not found, then throw an exception because
            // filter is no longer valid. This matches the DataView logic.
            // See: Rock.Reporting.DataFilter.PropertyFilter.GetExpression().
            if ( comparedToAttribute == null )
            {
                throw new RockDataViewFilterExpressionException( $"Rule filter refers to attribute that doesn't exist: {rule.AttributeGuid.Value}." );
            }

            var filterValues = new List<string>( 2 );

            // Only add the comparisonTypeValue if it is specified,
            // just like the logic at https://github.com/SparkDevNetwork/Rock/blob/22f64416b2461c8a988faf4b6e556bc3dcb209d3/Rock/Field/FieldType.cs#L558
            var comparisonTypeValue = rule.ComparisonType.ConvertToString( false );
            if ( comparisonTypeValue != null )
            {
                filterValues.Add( comparisonTypeValue );
            }

            filterValues.Add( rule.Value );

            if ( rockContext != null )
            {
                var entityField = EntityHelper.GetEntityFieldForAttribute( comparedToAttribute );

                return ExpressionHelper.GetAttributeExpression( rockContext, instanceExpression, entityField, filterValues );
            }
            else
            {
                return ExpressionHelper.GetAttributeMemoryExpression( instanceExpression, comparedToAttribute, filterValues );
            }
        }
    }
}