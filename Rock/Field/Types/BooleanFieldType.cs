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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to save a boolean value. Stored as "True" or "False"
    /// </summary>
    public class BooleanFieldType : FieldType
    {
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
            bool boolValue = value.AsBooleanOrNull() ?? false;

            if ( boolValue )
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
            bool boolValue = false;
            if ( !bool.TryParse( value, out boolValue ) )
            {
                message = "Invalid boolean value";
                return false;
            }

            return base.IsValid( value, required, out message );
        }

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
            var editControl = new RockCheckBox { ID = id };

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( "truetext" ) )
                {
                    editControl.Text = configurationValues["truetext"].Value;
                }
                else
                {
                    editControl.Text = "Yes";
                }
            }

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
            if ( control != null && control is CheckBox )
            {
                return ( (CheckBox)control ).Checked.ToString();
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
                if ( control != null && control is CheckBox )
                {
                    ( (CheckBox)control ).Checked = value.AsBooleanOrNull() ?? false;
                }
            }
        }

        /// <summary>
        /// Gets information about how to configure a filter UI for this type of field. Used primarily for dataviews
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public override Reporting.EntityField GetFilterConfig( Rock.Web.Cache.AttributeCache attribute )
        {
            var filterConfig = base.GetFilterConfig( attribute );
            filterConfig.ControlCount = 1;
            filterConfig.FilterFieldType = SystemGuid.FieldType.SINGLE_SELECT;
            return filterConfig;
        }
    }
}