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
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    /// A single filter rule that specifies a comparison to be performed
    /// on some object's property or attribute value.
    /// </summary>
    [RockInternal( "1.16.6" )]
    internal class FieldFilterRule
    {
        /// <summary>
        /// The unique identifier of this rule.
        /// </summary>
        public Guid Guid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The type of the comparison to be performed.
        /// </summary>
        public ComparisonType? ComparisonType { get; set; }

        /// <summary>
        /// The FieldType value (as interpreted by FieldType of the field
        /// that this rule is acting upon ) to be used when doing the
        /// comparison.
        /// </summary>
        public string ComparedToValue { get; set; }

        /// <summary>
        /// The attribute unique identifier that this rule will compare
        /// against or <c>null</c> if it is not an attribute comparison.
        /// </summary>
        public Guid? AttributeGuid { get; set; }

        /// <summary>
        /// The name of the property that this rule will compare against
        /// or <c>null</c> if it is not a property comparison.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets a LINQ expression that can be used to evaluate objects
        /// to see if they match the defined rules.
        /// </summary>
        /// <param name="parameterExpression">The expression that will identify the object instance to be evaluated.</param>
        /// <param name="rockContext">If <c>null</c> then an in-memory expression will be created; otherwise this specifies the database context the expression will be used in.</param>
        /// <returns>An expression that evaluates instances of <paramref name="parameterExpression"/>.</returns>
        internal Expression GetExpression( ParameterExpression parameterExpression, RockContext rockContext )
        {
            var filterValues = new List<string>( 2 );
            var comparedToValue = ComparedToValue;

            // Only add the comparisonTypeValue if it is specified,
            // just like the logic at https://github.com/SparkDevNetwork/Rock/blob/22f64416b2461c8a988faf4b6e556bc3dcb209d3/Rock/Field/FieldType.cs#L558
            var comparisonTypeValue = ComparisonType?.ConvertToString( false );
            if ( comparisonTypeValue != null )
            {
                filterValues.Add( comparisonTypeValue );
            }

            if ( PropertyName.IsNotNullOrWhiteSpace() )
            {
                var entityField = EntityHelper.GetEntityField( parameterExpression.Type, EntityHelper.MakePropertyNameUnique( PropertyName ), false, true );

                // If we are not going to be using this expression with
                // the database then we need to do some special logic for
                // text style filters to make them case-insensitive.
                if ( rockContext == null && entityField.FieldType.Field is Rock.Field.Types.TextFieldType )
                {
                    filterValues.Add( comparedToValue?.ToLower() );

                    Expression propertyExpression = Expression.Property( parameterExpression, entityField.Name );
                    propertyExpression = Expression.Call( propertyExpression, nameof( string.ToLower ), Type.EmptyTypes );

                    return ExpressionHelper.PropertyFilterExpression( filterValues, propertyExpression );
                }

                filterValues.Add( comparedToValue );

                return entityField.FieldType.Field.PropertyFilterExpression( entityField.FieldConfig, filterValues, parameterExpression, entityField.Name, entityField.PropertyType );
            }
            else if ( AttributeGuid.HasValue )
            {
                // If instance type does not support attributes then always
                // return no match.
                if ( !typeof( IHasAttributes ).IsAssignableFrom( parameterExpression.Type ) )
                {
                    return Expression.Constant( false );
                }

                var comparedToAttribute = AttributeCache.Get( AttributeGuid.Value );

                // If the rule is invalid, never match.
                if ( comparedToAttribute == null )
                {
                    return Expression.Constant( false );
                }

                if ( rockContext != null )
                {
                    var entityField = EntityHelper.GetEntityFieldForAttribute( comparedToAttribute );

                    filterValues.Add( comparedToValue );

                    return ExpressionHelper.GetAttributeExpression( rockContext, parameterExpression, entityField, filterValues );
                }
                else
                {
                    filterValues.Add( comparedToValue );

                    return ExpressionHelper.GetAttributeMemoryExpression( parameterExpression, comparedToAttribute, filterValues );
                }
            }
            else
            {
                return Expression.Constant( false );
            }
        }
    }
}