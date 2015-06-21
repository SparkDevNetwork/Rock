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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to save a boolean value. Stored as "True" or "False"
    /// </summary>
    public class BooleanFieldType : FieldType
    {

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( "truetext" );
            configKeys.Add( "falsetext" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            RockTextBox tbTrue = new RockTextBox();
            controls.Add( tbTrue );
            tbTrue.AutoPostBack = true;
            tbTrue.TextChanged += OnQualifierUpdated;
            tbTrue.Label = "True Text";
            tbTrue.Text = "Yes";
            tbTrue.Help = "The text to display when value is true.";

            RockTextBox tbFalse = new RockTextBox();
            controls.Add( tbFalse );
            tbFalse.AutoPostBack = true;
            tbFalse.TextChanged += OnQualifierUpdated;
            tbFalse.Label = "False Text";
            tbFalse.Text = "No";
            tbFalse.Help = "The text to display when value is false.";
            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( "truetext", new ConfigurationValue( "True Text",
                "The text to display when value is true (default is 'Yes').", "Yes" ) );
            configurationValues.Add( "falsetext", new ConfigurationValue( "False Text",
                "The text to display when value is false (default is 'No').", "No" ) );

            if ( controls != null && controls.Count == 2 )
            {
                if ( controls[0] != null && controls[0] is TextBox )
                    configurationValues["truetext"].Value = ( (TextBox)controls[0] ).Text;

                if ( controls[1] != null && controls[1] is TextBox )
                    configurationValues["falsetext"].Value = ( (TextBox)controls[1] ).Text;
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 2 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is TextBox && configurationValues.ContainsKey( "truetext" ) )
                    ( (TextBox)controls[0] ).Text = configurationValues["truetext"].Value;

                if ( controls[1] != null && controls[1] is TextBox && configurationValues.ContainsKey( "falsetext" ) )
                    ( (TextBox)controls[1] ).Text = configurationValues["falsetext"].Value;
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;
            bool? boolValue = value.AsBooleanOrNull();

            if ( boolValue.HasValue )
            {
                if ( boolValue.Value )
                {
                    if ( condensed )
                    {
                        formattedValue = "Y";
                    }
                    else
                    {
                        if ( configurationValues.ContainsKey( "truetext" ) )
                        {
                            formattedValue = configurationValues["truetext"].Value;
                        }
                        else
                        {
                            formattedValue = "Yes";
                        }
                    }
                }
                else
                {
                    if ( condensed )
                    {
                        formattedValue = "N";
                    }
                    else
                    {
                        if ( configurationValues.ContainsKey( "falsetext" ) )
                        {
                            formattedValue = configurationValues["falsetext"].Value;
                        }
                        else
                        {
                            formattedValue = "No";
                        }
                    }
                }

                return base.FormatValue( parentControl, formattedValue, null, condensed );
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Renders the controls necessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editControl = new RockDropDownList { ID = id };

            string yesText = "Yes";
            string noText = "No";

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( "truetext" ) )
                {
                    yesText = configurationValues["truetext"].Value;
                }
                if ( configurationValues.ContainsKey( "falsetext" ) )
                {
                    noText = configurationValues["falsetext"].Value;
                }
            }

            editControl.Items.Add( new ListItem() );
            editControl.Items.Add( new ListItem( noText, "False" ) );
            editControl.Items.Add( new ListItem( yesText, "True" ) );

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is RockDropDownList )
            {
                return ( (RockDropDownList)control ).SelectedValue;
            }
            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                if ( control != null && control is RockDropDownList )
                {
                    ( (RockDropDownList)control ).SetValue( value );
                }
            }
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
            bool? boolValue = value.AsBooleanOrNull();
            if ( required && !boolValue.HasValue )
            {
                message = "Invalid boolean value";
                return false;
            }

            return base.IsValid( value, required, out message );
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            var ddl = new RockDropDownList();
            ddl.ID = string.Format( "{0}_ctlCompareValue", id );
            ddl.AddCssClass( "js-filter-control" );

            if ( !required )
            {
                ddl.Items.Add( new ListItem() );
            }
            ddl.Items.Add( new ListItem( "True", "True" ) );
            ddl.Items.Add( new ListItem( "False", "False" ) );
            return ddl;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var ddl = control as DropDownList;
            if (ddl != null )
            {
                // Return the filter value only if a value has been selected.
                return string.IsNullOrEmpty(ddl.SelectedValue) ? null : ddl.SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var ddl = control as DropDownList;
            if ( ddl != null )
            {
                ddl.SetValue( value );
            }
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue(Dictionary<string,ConfigurationValue> configurationValues, string value)
        {
            return value;
        }

        /// <summary>
        /// Gets a filter expression for an entity property value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public override Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            if ( filterValues.Count == 1 )
            {
                // NOTE: this is for backwords compatility for filters that were saved when Boolean DataFilters didn't have a Compare Option
                MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );
                ConstantExpression constantExpression = Expression.Constant( bool.Parse( filterValues[0] ) );
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
            }
            else
            {
                return base.PropertyFilterExpression( configurationValues, filterValues, parameterExpression, propertyName, propertyType );
            }
        }

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonType.EqualTo | ComparisonType.NotEqualTo;
            }
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
                // NOTE: this is for backwords compatility for filters that were saved when Boolean DataFilters didn't have a Compare Option
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
                ConstantExpression constantExpression = Expression.Constant( filterValues[0] );
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
            }
            else
            {
                return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
            }
            
        }

        #endregion

    }
}