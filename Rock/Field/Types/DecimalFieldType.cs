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

using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a decimal numeric value
    /// </summary>
    [Serializable]
    public class DecimalFieldType : FieldType
    {

        #region Formatting 

        /// <summary>
        /// Formats the value.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            decimal? decimalValue = value.AsDecimalOrNull();
            if ( decimalValue.HasValue )
            {
                // from http://stackoverflow.com/a/216705/1755417 (to trim trailing zeros)
                return base.FormatValue( parentControl, decimalValue.Value.ToString("G29"), configurationValues, condensed );
            }
            else
            {
                return base.FormatValue( parentControl, value, configurationValues, condensed );
            }
        }

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object ValueAsFieldType( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value.AsDecimalOrNull();
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // return ValueAsFieldType which returns the value as a Decimal
            return this.ValueAsFieldType( parentControl, value, configurationValues );
        }

        /// <summary>
        /// Gets the align value that should be used when displaying value
        /// </summary>
        public override System.Web.UI.WebControls.HorizontalAlign AlignValue
        {
            get
            {
                return System.Web.UI.WebControls.HorizontalAlign.Right;
            }
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var numberBox = new NumberBox { ID = id }; 
            numberBox.NumberType = System.Web.UI.WebControls.ValidationDataType.Double;
            return numberBox;
        }
        
        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                decimal result;
                if ( !decimal.TryParse( value, out result ) )
                {
                    message = "The input provided is not a valid decimal value.";
                    return true;
                }
            }

            return base.IsValid( value, required, out message );
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get { return ComparisonHelper.NumericFilterComparisonTypes; }
        }

        /// <summary>
        /// Gets a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            if ( filterValues.Count == 1 )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "ValueAsNumeric" );
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, AttributeConstantExpression( filterValues[0] ) );
            }

            return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
        }

        /// <summary>
        /// Determines whether the filter's comparison type and filter compare value(s) evaluates to true for the specified value
        /// </summary>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is compared to value] [the specified filter values]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsComparedToValue( List<string> filterValues, string value )
        {
            if ( filterValues == null || filterValues.Count < 2 )
            {
                return false;
            }

            ComparisonType? filterComparisonType = filterValues[0].ConvertToEnumOrNull<ComparisonType>();
            ComparisonType? equalToCompareValue = GetEqualToCompareValue().ConvertToEnumOrNull<ComparisonType>();
            var filterValueAsDecimal = filterValues[1].AsDecimalOrNull();
            var valueAsDecimal = value.AsDecimalOrNull();

            return ComparisonHelper.CompareNumericValues( filterComparisonType.Value, valueAsDecimal, filterValueAsDecimal, null );
        }

        /// <summary>
        /// Gets the name of the attribute value field that should be bound to (Value, ValueAsDateTime, ValueAsBoolean, or ValueAsNumeric)
        /// </summary>
        /// <value>
        /// The name of the attribute value field.
        /// </value>
        public override string AttributeValueFieldName
        {
            get
            {
                return "ValueAsNumeric";
            }
        }

        /// <summary>
        /// Attributes the constant expression.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override ConstantExpression AttributeConstantExpression( string value )
        {
            return Expression.Constant( value.AsDecimal(), typeof( decimal ) );
        }

        /// <summary>
        /// Gets the type of the attribute value field.
        /// </summary>
        /// <value>
        /// The type of the attribute value field.
        /// </value>
        public override Type AttributeValueFieldType
        {
            get
            {
                return typeof( decimal? );
            }
        }

        #endregion

    }
}