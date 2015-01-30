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

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a date value
    /// </summary>
    [Serializable]
    public class DateFieldType : FieldType
    {

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var keys = base.ConfigurationKeys();
            keys.Add( "format" );
            keys.Add( "displayDiff" );
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
            }

            return values;
        }

        #endregion

        #region Formatting

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
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new DatePicker { ID = id }; 
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if (control != null)
            {
                var dtp = control as DatePicker;
                if (dtp != null )
                {
                    if ( dtp.SelectedDate.HasValue )
                    {
                        // serialize the date using ISO 8601 standard
                        return dtp.SelectedDate.Value.ToString( "o" );
                    }
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
            if ( control != null ) 
            {
                var dtp = control as DatePicker;
                if ( dtp != null )
                {
                    var dt = value.AsDateTime();
                    if ( dt.HasValue )
                    {
                        dtp.SelectedDate = dt;
                    }
                }
            }
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
            get { return ComparisonHelper.DateFilterComparisonTypes; }
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var datePicker = new DatePicker();
            datePicker.ID = string.Format( "{0}_dtPicker", id );
            datePicker.AddCssClass( "js-filter-control" );
            datePicker.DisplayCurrentOption = true;
            return datePicker;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var datePicker = control as DatePicker;
            if ( datePicker != null )
            {
                if ( datePicker.IsCurrentDateOffset )
                {
                    return string.Format( "CURRENT:{0}", datePicker.CurrentDateOffsetDays );
                }
                else if ( datePicker.SelectedDate.HasValue )
                {
                    return datePicker.SelectedDate.Value.ToString( "o" );
                }
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
            var datePicker = control as DatePicker;
            if ( datePicker != null )
            {
                if ( datePicker.DisplayCurrentOption && value != null && value.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
                {
                    datePicker.IsCurrentDateOffset = true;
                    var valueParts = value.Split( ':' );
                    if ( valueParts.Length > 1 )
                    {
                        datePicker.CurrentDateOffsetDays = valueParts[1].AsIntegerOrNull() ?? 0;
                    }
                }
                else
                {
                    var dt = value.AsDateTime();
                    if ( dt.HasValue )
                    {
                        datePicker.SelectedDate = dt;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the filters expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <returns></returns>
        public override Expression FilterExpression( IService serviceInstance, ParameterExpression parameterExpression, List<string> filterValues )
        {
            if ( filterValues.Count >= 2 && filterValues[1].StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
            {
                var valueParts = filterValues[1].Split( ':' );
                if ( valueParts.Length > 1 )
                {
                    DateTime currentDate = RockDateTime.Today.AddDays( valueParts[1].AsIntegerOrNull() ?? 0 );
                    filterValues[1] = currentDate.ToString( "o" );
                }
            }

            return base.FilterExpression( serviceInstance, parameterExpression, filterValues );
        }

        #endregion

        /// <summary>
        /// Gets information about how to configure a filter UI for this type of field. Used primarily for dataviews
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public override Reporting.EntityField GetFilterConfig( Rock.Web.Cache.AttributeCache attribute )
        {
            var filterConfig = base.GetFilterConfig( attribute );
            filterConfig.FilterFieldType = SystemGuid.FieldType.FILTER_DATE;
            return filterConfig;
        }
    }
}