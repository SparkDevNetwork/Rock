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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// Expression Helper Methods
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
            var propertyExpression = Expression.Property( parameterExpression, propertyName );

            return PropertyFilterExpression( filterValues, propertyExpression );
        }

        /// <summary>
        /// Gets a filter expression for an entity property value.
        /// </summary>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="propertyExpression">The expression used to access the property.</param>
        /// <returns></returns>
        internal static Expression PropertyFilterExpression( List<string> filterValues, Expression propertyExpression )
        {
            /* 2020-08-17 MDP
             * If it isn't fully configured we won't filter. We can detect if the filter isn't configured by..
             * 
             *   1) There are less than 2 filterValues 
             *   2) A comparisonType isn't specified ("0" means not specified) 
             *   3) Except of in the case of IsBlank or IsNotBlank, a "CompareTo null" (filterValues[1]) value means the filter value isn't specified
             *   
             *   If we have any of the above cases, we'll return Expression.Const(true), which means we won't filter on this)
             */

            if ( filterValues.Count < 2 )
            {
                // if PropertyFilter needs at least 2 parameters. If it doesn't, don't filter
                return Expression.Constant( true );
            }

            string comparisonValue = filterValues[0];
            if ( comparisonValue == "0" )
            {
                // if the comparison as a string is "0", that means no comparison type is specified (comparisonType enum starts at 1)
                return Expression.Constant( true );
            }

            var type = propertyExpression.Type;
            bool isNullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> );
            if ( isNullableType )
            {
                type = Nullable.GetUnderlyingType( type );
            }

            object value = ConvertValueToPropertyType( filterValues[1], type, isNullableType );
            ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
            bool valueNotNeeded = ( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType );

            if ( value != null || valueNotNeeded )
            {
                // a valid value is specified or we are doing a NotBlank/IsNotBlank, so build an filter expression
                ConstantExpression constantExpression = value != null ? Expression.Constant( value, type ) : null;

                return ComparisonHelper.ValueComparisonExpression( comparisonType, propertyExpression, constantExpression );
            }
            else
            {
                // if Property Filter isn't fully configured (for example "Birthday(int) greaterThan null"), don't filter the results
                return Expression.Constant( true );
            }
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

            if ( propertyType == typeof( TimeSpan ) )
            {
                return value.AsTimeSpan();
            }

            if ( propertyType == typeof( int ) )
            {
                return value.AsIntegerOrNull();
            }

            return Convert.ChangeType( value, propertyType );
        }

        /// <summary>
        /// Apply the value to the comparison expression and return the result.
        /// </summary>
        /// <param name="attributeValueParameterExpression">The attribute value parameter expression.</param>
        /// <param name="comparisonExpression">The comparison expression.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the comparison expression result in a true result; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsComparedToValue( ParameterExpression attributeValueParameterExpression, Expression comparisonExpression, string value )
        {
            // Creates a dummy attribute value that uses the default value
            AttributeValue attributeValue = AttributeValue.CreateNonPersistedAttributeValue( value );

            // Assign the dummy attribute to the comparison expression
            Expression assignExpr = Expression.Assign( attributeValueParameterExpression, Expression.Constant( attributeValue ) );
            BlockExpression blockExpr = Expression.Block(
                new ParameterExpression[] { attributeValueParameterExpression },
                assignExpr,
                comparisonExpression
                );

            // Execute the comparison expression
            return Expression.Lambda<Func<bool>>( blockExpr ).Compile()();
        }

        /// <summary>
        /// Builds an expression for an attribute field
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="entityField">The entity field.</param>
        /// <param name="values">The filter parameter values: FieldName, <see cref="ComparisonType">Comparison Type</see>, (optional) Comparison Value(s)</param>
        /// <returns></returns>
        public static Expression GetAttributeExpression( IService serviceInstance, ParameterExpression parameterExpression, EntityField entityField, List<string> values )
        {
            return GetAttributeExpression( ( RockContext ) serviceInstance.Context, parameterExpression, entityField, values );
        }

        /// <summary>
        /// Builds an expression for an attribute field
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="entityExpression">The expression to the object instance that contains the attributes.</param>
        /// <param name="entityField">The entity field.</param>
        /// <param name="values">The filter parameter values: FieldName, <see cref="ComparisonType">Comparison Type</see>, (optional) Comparison Value(s)</param>
        /// <returns></returns>
        internal static Expression GetAttributeExpression( RockContext rockContext, Expression entityExpression, EntityField entityField, List<string> values )
        {
            if ( !values.Any() )
            {
                // if no filter parameter values where specified, don't filter
                return new NoAttributeFilterExpression();
            }

            var service = new AttributeValueService( rockContext );

            /*
                13-Sep-2023 - DJL

                Restricting the Attribute Value query to non-null values for EntityId allows the database server
                to take advantage of an index seek rather than requiring a full index scan.

                Reason: Improve Data View performance. (https://github.com/SparkDevNetwork/Rock/issues/5575)
            */
            var attributeValues = service.Queryable().Where( v => v.EntityId.HasValue );

            AttributeCache attributeCache = null;

            if ( entityField.AttributeGuid.HasValue )
            {
                attributeCache = AttributeCache.Get( entityField.AttributeGuid.Value );
                var attributeId = attributeCache != null ? attributeCache.Id : 0;

                attributeValues = attributeValues.Where( v => v.AttributeId == attributeId );
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
            ComparisonType? comparisonType = ComparisonType.EqualTo;
            ComparisonType? evaluatedComparisonType = comparisonType;

            // If Values.Count >= 2, then Values[0] is ComparisonType, and Values[1] is a CompareToValue. Otherwise, Values[0] is a CompareToValue (for example, a SingleSelect attribute)
            if ( values.Count >= 2 )
            {
                comparisonType = values[0].ConvertToEnumOrNull<ComparisonType>();

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
                if ( filterExpression is NoAttributeFilterExpression )
                {
                    // Special Case: If AttributeFilterExpression returns NoAttributeFilterExpression, just return the NoAttributeFilterExpression.
                    // For example, If this is a CampusFieldType and they didn't pick any campus, we don't want to do any filtering for this DataFilter.
                    return filterExpression;
                }
                else
                {
                    attributeValues = attributeValues.Where( attributeValueParameterExpression, filterExpression, null );
                }
            }
            else
            {
                // AttributeFilterExpression returned NULL ( the FieldType didn't specify any additional filter on AttributeValue),
                // ideally the FieldType should have returned a NoAttributeFilterExpression, but just in case, don't filter
                System.Diagnostics.Debug.WriteLine( $"Unexpected NULL result from FieldType.Field.AttributeFilterExpression for { entityField.FieldType }" );
                return new NoAttributeFilterExpression();
            }

            IQueryable<int> ids = attributeValues.Select( v => v.EntityId.Value );

            MemberExpression propertyExpression = Expression.Property( entityExpression, "Id" );
            ConstantExpression idsExpression = Expression.Constant( ids.AsQueryable(), typeof( IQueryable<int> ) );
            Expression expression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, idsExpression, propertyExpression );

            if ( attributeCache != null )
            {
                // Test the default value against the expression filter. If it pass, then we can include all the attribute values with no value.
                var comparedToDefault = IsComparedToValue( attributeValueParameterExpression, filterExpression, attributeCache.DefaultValue );

                if ( comparedToDefault )
                {
                    var allAttributeValueIds = service.Queryable().Where( v => v.Attribute.Id == attributeCache.Id && v.EntityId.HasValue && !string.IsNullOrEmpty( v.Value ) ).Select( a => a.EntityId.Value );

                    ConstantExpression allIdsExpression = Expression.Constant( allAttributeValueIds.AsQueryable(), typeof( IQueryable<int> ) );
                    Expression notContainsExpression = Expression.Not( Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, allIdsExpression, propertyExpression ) );

                    expression = Expression.Or( expression, notContainsExpression );
                }

                // If there is an EntityTypeQualifierColumn/Value on this attribute, also narrow down the entity query to the ones with matching QualifierColumn/Value
                if ( attributeCache.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() && attributeCache.EntityTypeQualifierValue.IsNotNullOrWhiteSpace() )
                {
                    Expression qualifierParameterExpression = null;
                    PropertyInfo qualifierColumnProperty = entityExpression.Type.GetProperty( attributeCache.EntityTypeQualifierColumn );

                    // make sure the QualifierColumn is an actual mapped property on the Entity
                    if ( qualifierColumnProperty != null && qualifierColumnProperty.GetCustomAttribute<NotMappedAttribute>() == null )
                    {
                        qualifierParameterExpression = entityExpression;
                    }
                    else
                    {
                        if ( attributeCache.EntityTypeQualifierColumn == "GroupTypeId" && entityExpression.Type == typeof( Rock.Model.GroupMember ) )
                        {
                            // Special Case for GroupMember with Qualifier of 'GroupTypeId' (which is really Group.GroupTypeId)
                            qualifierParameterExpression = Expression.Property( entityExpression, "Group" );
                        }
                        else if ( attributeCache.EntityTypeQualifierColumn == "RegistrationTemplateId" && entityExpression.Type == typeof( Rock.Model.RegistrationRegistrant ) )
                        {
                            // Special Case for RegistrationRegistrant with Qualifier of 'RegistrationTemplateId' (which is really Registration.RegistrationInstance.RegistrationTemplateId)
                            qualifierParameterExpression = Expression.Property( entityExpression, "Registration" );
                            qualifierParameterExpression = Expression.Property( qualifierParameterExpression, "RegistrationInstance" );
                        }
                        else
                        {
                            // Unable to determine how the EntityTypeQualiferColumn relates to the Entity. Probably will be OK, but spit out a debug message
                            System.Diagnostics.Debug.WriteLine( $"Unable to determine how the EntityTypeQualiferColumn {attributeCache.EntityTypeQualifierColumn} relates to entity {entityExpression.Type} on attribute {attributeCache.Name}:{attributeCache.Guid}" );
                        }
                    }

                    if ( qualifierParameterExpression != null )
                    {
                        // if we figured out the EntityQualifierColumn/Value expression, apply it
                        // This would effectively add something like 'WHERE [GroupTypeId] = 10' to the WHERE clause
                        MemberExpression entityQualiferColumnExpression = Expression.Property( qualifierParameterExpression, attributeCache.EntityTypeQualifierColumn );
                        object entityTypeQualifierValueAsType = Convert.ChangeType( attributeCache.EntityTypeQualifierValue, entityQualiferColumnExpression.Type );
                        Expression entityQualiferColumnEqualExpression = Expression.Equal( entityQualiferColumnExpression, Expression.Constant( entityTypeQualifierValueAsType, entityQualiferColumnExpression.Type ) );

                        // If the qualifier Column is GroupTypeId, we'll have to do an OR clause of all the GroupTypes that inherit from this
                        // This would effectively add something like 'WHERE ([GroupTypeId] = 10) OR ([GroupTypeId] = 12) OR ([GroupTypeId] = 17)' to the WHERE clause
                        if ( attributeCache.EntityTypeQualifierColumn == "GroupTypeId" && attributeCache.EntityTypeQualifierValue.AsIntegerOrNull().HasValue )
                        {
                            var qualifierGroupTypeId = attributeCache.EntityTypeQualifierValue.AsInteger();

                            List<int> inheritedGroupTypeIds = null;
                            using (var groupTypeRockContext = new RockContext() )
                            {
                                var groupType = new GroupTypeService( groupTypeRockContext ).Get( qualifierGroupTypeId );
                                inheritedGroupTypeIds = groupType.GetAllDependentGroupTypeIds( groupTypeRockContext );
                            }

                            if ( inheritedGroupTypeIds != null )
                            {
                                foreach ( var inheritedGroupTypeId in inheritedGroupTypeIds )
                                {
                                    Expression inheritedEntityQualiferColumnEqualExpression = Expression.Equal( entityQualiferColumnExpression, Expression.Constant( inheritedGroupTypeId ) );
                                    entityQualiferColumnEqualExpression = Expression.Or( entityQualiferColumnEqualExpression, inheritedEntityQualiferColumnEqualExpression );
                                }
                            }
                        }

                        expression = Expression.And( entityQualiferColumnEqualExpression, expression );
                    }
                }
            }

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

        /// <summary>
        /// Builds an expression to compare an entities attribute value on an
        /// in-memory object. This expression cannot be used in a LINQ to SQL
        /// statement as it will generate an exception.
        /// </summary>
        /// <param name="entityExpression">The expression that identifies the entity object.</param>
        /// <param name="comparedToAttribute">The attribute that will be used to get the entity value.</param>
        /// <param name="values">The values defined on the filter.</param>
        /// <returns>An expression that compares the attribute value of an entity.</returns>
        internal static Expression GetAttributeMemoryExpression( Expression entityExpression, AttributeCache comparedToAttribute, List<string> values )
        {
            // Determine the appropriate comparison type to use for this Expression.
            // Attribute Value records only exist for Entities that have a value specified for the Attribute.
            // Therefore, if the specified comparison works by excluding certain values we must invert our filter logic:
            // first we find the Attribute Values that match those values and then we exclude the associated Entities from the result set.
            ComparisonType? comparisonType = ComparisonType.EqualTo;
            ComparisonType? evaluatedComparisonType = comparisonType;
            var filterValues = new List<string>( values );

            // If Values.Count >= 2, then Values[0] is ComparisonType, and Values[1] is a CompareToValue. Otherwise, Values[0] is a CompareToValue (for example, a SingleSelect attribute)
            if ( filterValues.Count >= 2 )
            {
                comparisonType = filterValues[0].ConvertToEnumOrNull<ComparisonType>();

                switch ( comparisonType )
                {
                    case ComparisonType.DoesNotContain:
                        evaluatedComparisonType = ComparisonType.Contains;
                        break;
                    case ComparisonType.IsBlank:
                        evaluatedComparisonType = ComparisonType.IsNotBlank;
                        break;
                    // TODO: The following 2 case statements have been removed to
                    // match functionality in develop branch. Since this is new
                    // logic and internal, we are going to make the logic match
                    // now rather than later. Remove when merged to develop.
                    //case ComparisonType.LessThan:
                    //    evaluatedComparisonType = ComparisonType.GreaterThanOrEqualTo;
                    //    break;
                    //case ComparisonType.LessThanOrEqualTo:
                    //    evaluatedComparisonType = ComparisonType.GreaterThan;
                    //    break;
                    case ComparisonType.NotEqualTo:
                        evaluatedComparisonType = ComparisonType.EqualTo;
                        break;
                    default:
                        evaluatedComparisonType = comparisonType;
                        break;
                }

                filterValues[0] = evaluatedComparisonType.ToString();
            }

            // If this is a TextFieldType, In-Memory LINQ is case-sensitive
            // but LinqToSQL is not, so lets compare values using ToLower().
            if ( comparedToAttribute.FieldType.Field is Rock.Field.Types.TextFieldType )
            {
                if ( filterValues.Count >= 2 )
                {
                    filterValues[1] = filterValues[1]?.ToLower();
                }
                else if ( filterValues.Count >= 1 )
                {
                    filterValues[0] = filterValues[0]?.ToLower();
                }
            }

            var attributeValueParameterExpression = Expression.Parameter( typeof( AttributeValue ) );
            var entityCondition = comparedToAttribute.FieldType.Field.AttributeFilterExpression( comparedToAttribute.QualifierValues, filterValues, attributeValueParameterExpression );

            if ( entityCondition is NoAttributeFilterExpression )
            {
                return entityCondition;
            }

            var conditionLambda = Expression.Lambda<Func<AttributeValue, bool>>( entityCondition, attributeValueParameterExpression );
            var filterFunc = conditionLambda.Compile();
            var method = typeof( ExpressionHelper ).GetMethod( nameof( CompareToInMemoryAttribute ), BindingFlags.NonPublic | BindingFlags.Static );

            var expression = Expression.Call( method, entityExpression, Expression.Constant( comparedToAttribute ), Expression.Constant( filterFunc ) );

            // If we have used an inverted comparison type for the evaluation,
            // invert the Expression so that it excludes the matching Entities.
            if ( comparisonType != evaluatedComparisonType )
            {
                return Expression.Not( expression );
            }
            else
            {
                return expression;
            }
        }

        /// <summary>
        /// Performs additional steps required to compare an in-memory object's
        /// attribute value to the filter expression returned by the field type.
        /// </summary>
        /// <param name="entity">The entity to retrieve the value from.</param>
        /// <param name="comparedToAttribute">The attribute that will be used for the source value.</param>
        /// <param name="filterFunc">The function (expression) from the attribute's field type.</param>
        /// <returns><c>true</c> if the attribute value matches the <paramref name="filterFunc"/> filter.</returns>
        private static bool CompareToInMemoryAttribute( IHasAttributes entity, AttributeCache comparedToAttribute, Func<AttributeValue, bool> filterFunc )
        {
            var comparedToAttributeValue = entity.GetAttributeValue( comparedToAttribute.Key );

            // if this is a TextFieldType, In-Memory LINQ is case-sensitive but LinqToSQL is not, so lets compare values using ToLower()
            if ( comparedToAttribute.FieldType.Field is Rock.Field.Types.TextFieldType )
            {
                comparedToAttributeValue = comparedToAttributeValue?.ToLower();
            }

            // create an instance of an AttributeValue to run the expressions against
            var attributeValueToEvaluate = new Rock.Model.AttributeValue
            {
                AttributeId = comparedToAttribute.Id,
                Value = comparedToAttributeValue,
                ValueAsBoolean = comparedToAttributeValue.AsBooleanOrNull(),
                ValueAsNumeric = comparedToAttributeValue.AsDecimalOrNull(),
                ValueAsDateTime = comparedToAttributeValue.AsDateTime()
            };

            return filterFunc.Invoke( attributeValueToEvaluate );
        }

        /// <summary>
        /// Builds an expression using the specified FieldType and control.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public static Expression BuildExpressionFromFieldType<T>( IFieldType fieldType, Control filterControl, AttributeCache attribute, IService serviceInstance, ParameterExpression parameterExpression, FilterMode filterMode ) where T : Entity<T>, new()
        {
            if ( filterControl == null || attribute == null )
            {
                return null;
            }

            // We're going to assume the QualifierValues are the same for all attributes in the array.
            var filterValues = fieldType.GetFilterValues( filterControl, attribute.QualifierValues, filterMode );

            // If the filterValues are all empty then no filtering needs to be done.
            if ( filterValues == null || filterValues.All( v => v.IsNullOrWhiteSpace() ) )
            {
                return null;
            }

            return BuildExpressionFromFieldType<T>( filterValues, attribute, serviceInstance, parameterExpression );
        }

        /// <summary>
        /// Builds an expression using the specified FieldType and control.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public static Expression BuildExpressionFromFieldType<T>( List<string> filterValues, AttributeCache attribute, IService serviceInstance, ParameterExpression parameterExpression ) where T : Entity<T>, new()
        {
            // If the filterValues are all empty then no filtering needs to be done.
            if ( attribute == null || filterValues == null || filterValues.All( v => v.IsNullOrWhiteSpace() ) )
            {
                return null;
            }

            var entityFields = EntityHelper.GetEntityFields( typeof( T ) );

            var entityField = entityFields.Where( a => a.FieldKind == FieldKind.Attribute && a.AttributeGuid == attribute.Guid ).FirstOrDefault();
            if ( entityField == null )
            {
                entityField = EntityHelper.GetEntityFieldForAttribute( attribute, false );
            }

            return GetAttributeExpression( serviceInstance, parameterExpression, entityField, filterValues );
        }
    }
}
