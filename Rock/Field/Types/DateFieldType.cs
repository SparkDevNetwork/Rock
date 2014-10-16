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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a date value
    /// </summary>
    [Serializable]
    public class DateFieldType : FieldType
    {
        /// <summary>
        /// Formats date display
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            if ( value != null && value.Equals( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
            {
                return "Current Date";
            }

            DateTime? dateValue = value.AsDateTime();
            if ( dateValue.HasValue )
            {
                formattedValue = dateValue.Value.ToShortDateString();

                if ( configurationValues != null &&
                    configurationValues.ContainsKey( "format" ) &&
                    !String.IsNullOrWhiteSpace( configurationValues["format"].Value ) )
                {
                    try
                    {
                        formattedValue = dateValue.Value.ToString( configurationValues["format"].Value );
                    }
                    catch
                    {
                        formattedValue = dateValue.Value.ToShortDateString();
                    }
                }

                if ( !condensed )
                {
                    if ( configurationValues != null &&
                        configurationValues.ContainsKey( "displayDiff" ) )
                    {
                        bool displayDiff = false;
                        if ( bool.TryParse( configurationValues["displayDiff"].Value, out displayDiff ) && displayDiff )
                            formattedValue += " (" + dateValue.ToElapsedString( true, false ) + ")";
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
            var keys = base.ConfigurationKeys();
            keys.Add( "format" );
            keys.Add( "displayDiff" );
            keys.Add( "displayCurrentOption" );
            return keys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override System.Collections.Generic.List<System.Web.UI.Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var textbox = new RockTextBox();
            controls.Add( textbox );
            textbox.Label = "Date Format";
            textbox.Help = "The format string to use for date (default is system short date)";

            var cbDisplayDiff = new RockCheckBox();
            controls.Add( cbDisplayDiff );
            cbDisplayDiff.Label = "Display Date Span";
            cbDisplayDiff.Text = "Yes";
            cbDisplayDiff.Help = "Display the number of years between value and current date";

            var cbDisplayCurrent = new RockCheckBox();
            controls.Add( cbDisplayCurrent );
            cbDisplayCurrent.Label = "Allow 'Current' Option";
            cbDisplayCurrent.Text = "Yes";
            cbDisplayCurrent.Help = "Display option for selecting the 'current' date instead of a specific date";
            return controls;

        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is TextBox &&
                    configurationValues.ContainsKey( "format" ) )
                {
                    ( (TextBox)controls[0] ).Text = configurationValues["format"].Value ?? string.Empty;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is CheckBox &&
                    configurationValues.ContainsKey( "displayDiff" ) )
                {
                    ( (CheckBox)controls[1] ).Checked = configurationValues["displayDiff"].Value.AsBoolean( false );
                }

                if ( controls.Count > 2 && controls[2] != null && controls[2] is CheckBox &&
                    configurationValues.ContainsKey( "displayCurrentOption" ) )
                {
                    ( (CheckBox)controls[2] ).Checked = configurationValues["displayCurrentOption"].Value.AsBoolean( false );
                }

            }
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var values = base.ConfigurationValues( controls );
            values.Add( "format", new ConfigurationValue( "Date Format", "The format string to use for date (default is system short date)", "" ) );
            values.Add( "displayDiff", new ConfigurationValue( "Display Date Span", "Display the number of years between value and current date", "False" ) );
            values.Add( "displayCurrentOption", new ConfigurationValue( "Allow 'Current' Option", "Display option for selecting the 'current' date instead of a specific date.", "False" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is TextBox )
                {
                    values["format"].Value = ( (TextBox)controls[0] ).Text;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is CheckBox )
                {
                    values["displayDiff"].Value = ( (CheckBox)controls[1] ).Checked.ToString();
                }

                if ( controls.Count > 2 && controls[2] != null && controls[2] is CheckBox )
                {
                    values["displayCurrentOption"].Value = ( (CheckBox)controls[2] ).Checked.ToString();
                }
            }

            return values;
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var dp = new DatePicker { ID = id }; 
            dp.DisplayCurrentOption = configurationValues.ContainsKey( "displayCurrentOption" )  &&
                configurationValues["displayCurrentOption"].Value.AsBoolean( false );
            return dp;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if (control != null && control is DatePicker)
            {
                var dtp = control as DatePicker;
                if (dtp != null && dtp.SelectedDate.HasValue)
                {
                    if ( dtp.CurrentDate )
                    {
                        return "CURRENT";
                    }

                    // serialize the date using ISO 8601 standard
                    return dtp.SelectedDate.Value.ToString( "o" );
                }
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
            if ( control != null && control is DatePicker ) 
            {
                var dtp = control as DatePicker;
                if ( dtp != null )
                {
                    if ( dtp.DisplayCurrentOption && value != null && value.Equals( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
                    {
                        dtp.CurrentDate = true;
                    }
                    else
                    {
                        var dt = value.AsDateTime();
                        if ( dt.HasValue )
                        {
                            dtp.SelectedDate = dt;
                        }
                    }
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
            filterConfig.FilterFieldType = SystemGuid.FieldType.DATE;
            return filterConfig;
        }
    }
}