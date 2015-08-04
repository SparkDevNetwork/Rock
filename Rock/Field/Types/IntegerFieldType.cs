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
using System.Linq.Expressions;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a numeric value
    /// </summary>
    [Serializable]
    public class IntegerFieldType : FieldType
    {

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            int? intValue = (int?)value.AsDecimalOrNull();
            return base.FormatValue( parentControl, intValue.ToString(), configurationValues, condensed );
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
            return new NumberBox { ID = id };
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="required"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                int result;
                if ( !Int32.TryParse( value, out result ) )
                {
                    message = "The input provided is not a valid integer.";
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
        /// Geta a filter expression for an attribute value.
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
                ConstantExpression constantExpression = Expression.Constant( filterValues[0].AsDecimal(), typeof( decimal ) );
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
            }

            return null;
        }

        /// <summary>
        /// Gets the name of the attribute value field that should be bound to (Value, ValueAsDateTime, or ValueAsNumeric)
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