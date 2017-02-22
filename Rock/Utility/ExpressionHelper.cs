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
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Reporting;

namespace Rock.Utility
{
    /// <summary>
    /// Expresssion Helper Methods
    /// </summary>
    public static class ExpressionHelper
    {

        /// <summary>
        /// Gets a filter expression for an entity property value.
        /// </summary>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public static Expression PropertyFilterExpression( List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            if ( filterValues.Count >= 2 )
            {
                string comparisonValue = filterValues[0];
                if ( comparisonValue != "0" )
                {
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );

                    var type = propertyType;
                    bool isNullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> );
                    if ( isNullableType )
                    {
                        type = Nullable.GetUnderlyingType( type );
                    }

                    object value = ConvertValueToPropertyType( filterValues[1], type, isNullableType );
                    ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );

                    bool valueNotNeeded = (ComparisonType.IsBlank | ComparisonType.IsNotBlank).HasFlag( comparisonType );

                    if ( value != null || valueNotNeeded )
                    {
                        ConstantExpression constantExpression = value != null ? Expression.Constant( value, type ) : null;
                        return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Converts the type of the value to property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="isNullableType">if set to <c>true</c> [is nullable type].</param>
        /// <returns></returns>
        public static object ConvertValueToPropertyType( string value, Type propertyType, bool isNullableType )
        {
            if ( propertyType == typeof( string ) )
            {
                return value;
            }

            if ( propertyType == typeof( Guid ) )
            {
                return value.AsGuid();
            }

            if ( string.IsNullOrWhiteSpace( value ) && isNullableType )
            {
                return null;
            }

            if ( propertyType.IsEnum )
            {
                return Enum.Parse( propertyType, value );
            }

            return Convert.ChangeType( value, propertyType );
        }

        /// <summary>
        /// Builds an expression for an attribute field
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="entityField">The property.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static Expression GetAttributeExpression( IService serviceInstance, ParameterExpression parameterExpression, EntityField entityField, List<string> values )
        {
            var service = new AttributeValueService( (RockContext)serviceInstance.Context );

            var attributeValues = service.Queryable().Where( v =>
                v.EntityId.HasValue &&
                v.Value != string.Empty );

            if ( entityField.AttributeGuid.HasValue )
            {
                attributeValues = attributeValues.Where( v => v.Attribute.Guid == entityField.AttributeGuid );
            }
            else
            {
                attributeValues = attributeValues.Where( v => v.Attribute.Key == entityField.Name && v.Attribute.FieldTypeId == entityField.FieldType.Id );
            }

            ParameterExpression attributeValueParameterExpression = Expression.Parameter( typeof( AttributeValue ), "v" );

            // Determine the appropriate comparison type to use for this Expression.
            // Attribute Value records only exist for Entities that have a value specified for the Attribute.
            // Therefore, if the specified comparison works by excluding certain values we must invert our filter logic:
            // first we find the Attribute Values that match those values and then we exclude the associated Entities from the result set.
            var comparisonType = ComparisonType.EqualTo;
            ComparisonType evaluatedComparisonType = comparisonType;

            if ( values.Count >= 2 )
            {
                string comparisonValue = values[0];
                if ( comparisonValue != "0" )
                {
                    comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                }

                switch ( comparisonType )
                {
                    case ComparisonType.DoesNotContain:
                        evaluatedComparisonType = ComparisonType.Contains;
                        break;
                    case ComparisonType.IsBlank:
                        evaluatedComparisonType = ComparisonType.IsNotBlank;
                        break;
                    case ComparisonType.LessThan:
                        evaluatedComparisonType = ComparisonType.GreaterThanOrEqualTo;
                        break;
                    case ComparisonType.LessThanOrEqualTo:
                        evaluatedComparisonType = ComparisonType.GreaterThan;
                        break;
                    case ComparisonType.NotEqualTo:
                        evaluatedComparisonType = ComparisonType.EqualTo;
                        break;
                    default:
                        evaluatedComparisonType = comparisonType;
                        break;
                }

                values[0] = evaluatedComparisonType.ToString();
            }

            var filterExpression = entityField.FieldType.Field.AttributeFilterExpression( entityField.FieldConfig, values, attributeValueParameterExpression );
            if ( filterExpression != null )
            {
                attributeValues = attributeValues.Where( attributeValueParameterExpression, filterExpression, null );
            }

            IQueryable<int> ids = attributeValues.Select( v => v.EntityId.Value );

            MemberExpression propertyExpression = Expression.Property( parameterExpression, "Id" );
            ConstantExpression idsExpression = Expression.Constant( ids.AsQueryable(), typeof( IQueryable<int> ) );
            Expression expression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, idsExpression, propertyExpression );

            // If we have used an inverted comparison type for the evaluation, invert the Expression so that it excludes the matching Entities.
            if ( comparisonType != evaluatedComparisonType )
            {
                return Expression.Not( expression );
            }
            else
            {
                return expression;
            }
        }
    }
}
