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
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public static class ComparisonHelper
    {
        /// <summary>
        /// Gets the comparison expression.
        /// </summary>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="value2">If doing ComparisonType.Between, value2 is the upper value between expression</param>
        /// <returns></returns>
        public static Expression ComparisonExpression( ComparisonType comparisonType, MemberExpression property, Expression value, Expression value2 = null )
        {
            MemberExpression valueExpression;
            Expression comparisonExpression = null;
            bool isNullableType = property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof( Nullable<> );
            if ( isNullableType )
            {
                // if Nullable Type compare on the .Value of the property (if it HasValue)
                valueExpression = Expression.Property( property, "Value" );
            }
            else
            {
                valueExpression = property;
            }

            if ( comparisonType == ComparisonType.Contains )
            {
                if ( valueExpression.Type == typeof( int ) )
                {
                    comparisonExpression = Expression.Call( value, typeof( List<int> ).GetMethod( "Contains", new Type[] { typeof( int ) } ), valueExpression );
                }
                else
                {
                    comparisonExpression = Expression.Call( valueExpression, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value );
                }
            }
            else if ( comparisonType == ComparisonType.DoesNotContain )
            {
                comparisonExpression = Expression.Not( Expression.Call( valueExpression, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value ) );
            }
            else if ( comparisonType == ComparisonType.EndsWith )
            {
                comparisonExpression = Expression.Call( valueExpression, typeof( string ).GetMethod( "EndsWith", new Type[] { typeof( string ) } ), value );
            }
            else if ( comparisonType == ComparisonType.EqualTo )
            {
                comparisonExpression = Expression.Equal( valueExpression, value );
            }
            else if ( comparisonType == ComparisonType.GreaterThan ||
                comparisonType == ComparisonType.GreaterThanOrEqualTo ||
                comparisonType == ComparisonType.LessThan ||
                comparisonType == ComparisonType.LessThanOrEqualTo ||
                comparisonType == ComparisonType.Between )
            {
                Expression leftExpression = valueExpression;
                Expression rightExpression = value;

                Expression rightExpression2 = value2;

                if ( valueExpression.Type == typeof( string ) )
                {
                    var method = valueExpression.Type.GetMethod( "CompareTo", new[] { typeof( string ) } );
                    leftExpression = Expression.Call( valueExpression, method, value );
                    rightExpression = Expression.Constant( 0 );
                }

                if ( comparisonType == ComparisonType.GreaterThan )
                {
                    comparisonExpression = Expression.GreaterThan( leftExpression, rightExpression );
                }
                else if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
                {
                    comparisonExpression = Expression.GreaterThanOrEqual( leftExpression, rightExpression );
                }
                else if ( comparisonType == ComparisonType.LessThan )
                {
                    comparisonExpression = Expression.LessThan( leftExpression, rightExpression );
                }
                else if ( comparisonType == ComparisonType.LessThanOrEqualTo )
                {
                    comparisonExpression = Expression.LessThanOrEqual( leftExpression, rightExpression );
                }
                else if ( comparisonType == ComparisonType.Between )
                {
                    var lowerComparisonExpression = rightExpression != null ? Expression.GreaterThanOrEqual( leftExpression, rightExpression ) : null;
                    var upperComparisonExpression = rightExpression2 != null ? Expression.LessThanOrEqual( leftExpression, rightExpression2 ) : null;
                    if ( rightExpression != null && rightExpression2 != null )
                    {
                        comparisonExpression = Expression.AndAlso( lowerComparisonExpression, upperComparisonExpression );
                    }
                    else if ( rightExpression != null )
                    {
                        comparisonExpression = lowerComparisonExpression;
                    }
                    else if ( rightExpression2 != null )
                    {
                        comparisonExpression = upperComparisonExpression;
                    }
                    else
                    {
                        return new NoAttributeFilterExpression();
                    }
                }
            }
            else if ( comparisonType == ComparisonType.IsBlank )
            {
                if ( valueExpression.Type == typeof( string ) )
                {
                    Expression trimmed = Expression.Call( valueExpression, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ) );
                    comparisonExpression = Expression.Or( Expression.Equal( trimmed, value ), Expression.Equal( valueExpression, Expression.Constant( null, valueExpression.Type ) ) );
                }
                else
                {
                    if ( isNullableType )
                    {
                        comparisonExpression = Expression.Equal( Expression.Property( property, "HasValue" ), Expression.Constant( false ) );
                    }
                    else
                    {
                        // if not a Nullable type, return false since there aren't any null values
                        comparisonExpression = Expression.Constant( false );
                    }
                }
            }
            else if ( comparisonType == ComparisonType.IsNotBlank )
            {
                if ( valueExpression.Type == typeof( string ) )
                {
                    Expression trimmed = Expression.Call( valueExpression, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ) );
                    Expression emptyString = Expression.Constant( string.Empty );
                    comparisonExpression = Expression.And( Expression.NotEqual( trimmed, emptyString ), Expression.NotEqual( valueExpression, Expression.Constant( null, valueExpression.Type ) ) );
                }
                else
                {
                    if ( isNullableType )
                    {
                        comparisonExpression = Expression.Property( property, "HasValue" );
                    }
                    else
                    {
                        // if not a Nullable type, return true since there aren't any non-null values
                        comparisonExpression = Expression.Constant( true );
                    }
                }
            }
            else if ( comparisonType == ComparisonType.NotEqualTo )
            {
                comparisonExpression = Expression.NotEqual( valueExpression, value );
            }
            else if ( comparisonType == ComparisonType.StartsWith )
            {
                comparisonExpression = Expression.Call( valueExpression, typeof( string ).GetMethod( "StartsWith", new Type[] { typeof( string ) } ), value );
            }

            // unless we are simply checking for Null/NotNull, make sure to check on HasValue for Nullable types
            if ( !( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
            {
                if ( comparisonExpression != null && isNullableType )
                {
                    // if Nullable Type we are comparing on the .Value of the property, so also make sure it HasValue
                    MemberExpression hasValue = Expression.Property( property, "HasValue" );
                    return Expression.AndAlso( hasValue, comparisonExpression );
                }
            }

            return comparisonExpression;
        }

        /// <summary>
        /// Gets a DropDownList of the supported comparison types
        /// </summary>
        /// <param name="supportedComparisonTypes">The supported comparison types.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        public static RockDropDownList ComparisonControl( ComparisonType supportedComparisonTypes, bool required = true )
        {
            var ddlComparisonControl = new RockDropDownList();
            PopulateComparisonControl( ddlComparisonControl, supportedComparisonTypes, required );

            return ddlComparisonControl;
        }

        /// <summary>
        /// Populates the DropDownList with supported comparison types
        /// </summary>
        /// <param name="ddlComparisonControl">The DDL comparison control.</param>
        /// <param name="supportedComparisonTypes">The supported comparison types.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        public static void PopulateComparisonControl( RockDropDownList ddlComparisonControl, ComparisonType supportedComparisonTypes, bool required )
        {
            ddlComparisonControl.Items.Clear();
            if ( !required )
            {
                ddlComparisonControl.Items.Add( new ListItem( string.Empty, "0" ) );
            }
            foreach ( ComparisonType comparisonType in typeof( ComparisonType ).GetOrderedValues<ComparisonType>() )
            {
                if ( ( supportedComparisonTypes & comparisonType ) == comparisonType )
                {
                    ddlComparisonControl.Items.Add( new ListItem( comparisonType.ConvertToString(), comparisonType.ConvertToInt().ToString() ) );
                }
            }
        }

        /// <summary>
        /// Gets the comparison types typically used simple comparisons of: equal, not equal, blank or not blank.
        /// </summary>
        internal const ComparisonType EqualOrBlankFilterComparisonTypes =
                        ComparisonType.EqualTo |
                        ComparisonType.IsBlank |
                        ComparisonType.IsNotBlank |
                        ComparisonType.NotEqualTo;

        /// <summary>
        /// Compares the numeric values.
        /// </summary>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="value">The value.</param>
        /// <param name="compareValue1">The compare value1.</param>
        /// <param name="compareValue2">The compare value2.</param>
        /// <returns></returns>
        public static bool CompareNumericValues( ComparisonType comparisonType, decimal? value, decimal? compareValue1, decimal? compareValue2 = null )
        {
            switch ( comparisonType )
            {
                case ComparisonType.GreaterThan:
                    return value > compareValue1;
                case ComparisonType.GreaterThanOrEqualTo:
                    return value >= compareValue1;
                case ComparisonType.LessThan:
                    return value < compareValue1;
                case ComparisonType.LessThanOrEqualTo:
                    return value <= compareValue1;
                case ComparisonType.EqualTo:
                    return value == compareValue1;
                case ComparisonType.NotEqualTo:
                    return value == compareValue1;
                case ComparisonType.Between:
                    return compareValue2.HasValue && ( value >= compareValue1 && value <= compareValue2 );
            }

            return false;
        }

        /// <summary>
        /// Gets the comparison types typically used for string fields
        /// </summary>
        public const ComparisonType StringFilterComparisonTypes =
                        ComparisonType.Contains |
                        ComparisonType.DoesNotContain |
                        ComparisonType.EqualTo |
                        ComparisonType.IsBlank |
                        ComparisonType.IsNotBlank |
                        ComparisonType.NotEqualTo |
                        ComparisonType.StartsWith |
                        ComparisonType.EndsWith;


        /// <summary>
        /// Gets the comparison types typically used for string fields when a comparison value is required.
        /// </summary>
        public const ComparisonType StringFilterComparisonTypesRequired =
                        ComparisonType.Contains |
                        ComparisonType.DoesNotContain |
                        ComparisonType.EqualTo |
                        ComparisonType.NotEqualTo |
                        ComparisonType.StartsWith |
                        ComparisonType.EndsWith;


        /// <summary>
        /// Gets the comparison types typically used for select or boolean fields
        /// </summary>
        public const ComparisonType BinaryFilterComparisonTypes =
                        ComparisonType.EqualTo |
                        ComparisonType.NotEqualTo;

        /// <summary>
        /// Gets the comparison types typically used for list fields
        /// </summary>
        public const ComparisonType ContainsFilterComparisonTypes =
                        ComparisonType.Contains |
                        ComparisonType.DoesNotContain |
                        ComparisonType.IsBlank;

        /// <summary>
        /// Gets the comparison types typically used for numeric fields
        /// </summary>
        public const ComparisonType NumericFilterComparisonTypes =
                        ComparisonType.EqualTo |
                        ComparisonType.IsBlank |
                        ComparisonType.IsNotBlank |
                        ComparisonType.NotEqualTo |
                        ComparisonType.GreaterThan |
                        ComparisonType.GreaterThanOrEqualTo |
                        ComparisonType.LessThan |
                        ComparisonType.LessThanOrEqualTo;

        /// <summary>
        /// Gets the comparison types typically used for numeric fields when a comparison value is required.
        /// </summary>
        public const ComparisonType NumericFilterComparisonTypesRequired =
                        ComparisonType.EqualTo |
                        ComparisonType.NotEqualTo |
                        ComparisonType.GreaterThan |
                        ComparisonType.GreaterThanOrEqualTo |
                        ComparisonType.LessThan |
                        ComparisonType.LessThanOrEqualTo;

        /// <summary>
        /// Gets the date filter comparison types.
        /// </summary>
        /// <value>
        /// The date filter comparison types.
        /// </value>
        public const ComparisonType DateFilterComparisonTypes =
                   ComparisonType.EqualTo |
                    ComparisonType.IsBlank |
                    ComparisonType.IsNotBlank |
                    ComparisonType.GreaterThan |
                    ComparisonType.GreaterThanOrEqualTo |
                    ComparisonType.LessThan |
                    ComparisonType.LessThanOrEqualTo |
                    ComparisonType.Between;
    }
}
