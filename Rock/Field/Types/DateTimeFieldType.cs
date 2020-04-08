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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a date/time value
    /// </summary>
    [Serializable]
    public class DateTimeFieldType : DateFieldType
    {

        #region Configuration

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override System.Collections.Generic.List<System.Web.UI.Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();
            _tbDateFormat.Label = "Date Time Format";
            _tbDateFormat.Help = "The format string to use for date (default is system short date and time).";

            _ddlDatePickerMode.Visible = false;
            _nbFutureYearCount.Visible = false;

            _cbDisplayCurrent.Help = "Include option to specify value as the current time.";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var values = base.ConfigurationValues( controls );
            values["format"].Name = "Date Time Format";
            values["format"].Description = "The format string to use for date (default is system short date and time).";
            values["displayCurrentOption"].Description = "Include option to specify value as the current time.";
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
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }

            if ( value.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
            {
                DateTime currentDate = RockDateTime.Today;

                var valueParts = value.Split( ':' );
                if ( valueParts.Length > 1 )
                {
                    int? days = valueParts[1].AsIntegerOrNull();
                    if ( days.HasValue && days.Value != 0 )
                    {
                        if ( days > 0 )
                        {
                            return string.Format( "Current Time plus {0} minutes", days.Value );
                        }
                        else
                        {
                            return string.Format( "Current Time minus {0} minutes", -days.Value );
                        }
                    }
                }

                return "Current Time";
            }
            else
            {

                string formattedValue = string.Empty;

                DateTime? dateValue = value.AsDateTime();
                if ( dateValue.HasValue )
                {
                    formattedValue = dateValue.Value.ToShortDateString() + " " + dateValue.Value.ToShortTimeString();

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
                            formattedValue = dateValue.Value.ToShortDateString() + " " + dateValue.Value.ToShortTimeString();
                        }
                    }

                    if ( !condensed )
                    {
                        if ( configurationValues != null && configurationValues.ContainsKey( "displayDiff" ) )
                        {
                            bool displayDiff = configurationValues["displayDiff"].Value.AsBooleanOrNull() ?? false;
                            if ( displayDiff )
                            {
                                formattedValue += " (" + dateValue.ToElapsedString( true, true ) + ")";
                            }
                        }
                    }
                }

                return formattedValue;
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
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var dateTimePicker = new DateTimePicker { ID = id };
            dateTimePicker.DisplayCurrentOption = configurationValues != null &&
                configurationValues.ContainsKey( "displayCurrentOption" ) &&
                configurationValues["displayCurrentOption"].Value.AsBoolean();
            return dateTimePicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var dtp = control as DateTimePicker;
            if ( dtp != null )
            {
                if ( dtp.DisplayCurrentOption && dtp.IsCurrentTimeOffset )
                {
                    return string.Format( "CURRENT:{0}", dtp.CurrentTimeOffsetMinutes );
                }
                else if ( dtp.SelectedDateTime.HasValue )
                {
                    return dtp.SelectedDateTime.Value.ToString( "o" );
                }
                else
                {
                    return string.Empty;
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
            var dtp = control as DateTimePicker;
            if ( dtp != null )
            {
                if ( dtp.DisplayCurrentOption && value != null && value.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
                {
                    dtp.IsCurrentTimeOffset = true;
                    var valueParts = value.Split( ':' );
                    if ( valueParts.Length > 1 )
                    {
                        dtp.CurrentTimeOffsetMinutes = valueParts[1].AsInteger();
                    }
                    else
                    {
                        dtp.CurrentTimeOffsetMinutes = 0;
                    }
                }
                else
                {
                    // NullReferenceException will *NOT* be thrown if value is null because the AsDateTime() extension method is null safe.
                    dtp.SelectedDateTime = value.AsDateTime();
                }
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var dateFiltersPanel = new Panel();
            dateFiltersPanel.ID = string.Format( "{0}_dtFilterControls", id );

            var datePickerPanel = new Panel();
            dateFiltersPanel.Controls.Add( datePickerPanel );

            var datePicker = new DateTimePicker();
            datePicker.ID = string.Format( "{0}_dtPicker", id );
            datePicker.DisplayCurrentOption = true;
            datePickerPanel.AddCssClass( "js-filter-control" );
            datePickerPanel.Controls.Add( datePicker );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = string.Format( "{0}_dtSlidingDateRange", id );
            slidingDateRangePicker.AddCssClass( "js-filter-control-between" );
            slidingDateRangePicker.Label = string.Empty;
            slidingDateRangePicker.PreviewLocation = SlidingDateRangePicker.DateRangePreviewLocation.Right;
            dateFiltersPanel.Controls.Add( slidingDateRangePicker );

            return dateFiltersPanel;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var dateFiltersPanel = control as Panel;
            var datePicker = dateFiltersPanel.ControlsOfTypeRecursive<DateTimePicker>().FirstOrDefault();
            var slidingDateRangePicker = dateFiltersPanel.ControlsOfTypeRecursive<SlidingDateRangePicker>().FirstOrDefault();
            string datePickerValue = string.Empty;
            string slidingDateRangePickerValue = string.Empty;
            if ( datePicker != null )
            {
                datePickerValue = this.GetEditValue( datePicker, configurationValues );
            }

            if ( slidingDateRangePicker != null )
            {
                slidingDateRangePickerValue = slidingDateRangePicker.DelimitedValues;
            }

            // use Tab Delimited since slidingDateRangePicker is | delimited
            return string.Format( "{0}\t{1}", datePickerValue, slidingDateRangePickerValue );
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            // uses Tab Delimited since slidingDateRangePicker is | delimited
            var filterValues = value.Split( new string[] { "\t" }, StringSplitOptions.None );

            var dateFiltersPanel = control as Panel;

            var dateTimePicker = dateFiltersPanel.ControlsOfTypeRecursive<DateTimePicker>().FirstOrDefault();
            if ( dateTimePicker != null && filterValues.Length > 0 )
            {
                this.SetEditValue( dateTimePicker, configurationValues, filterValues[0] );
            }

            var slidingDateRangePicker = dateFiltersPanel.ControlsOfTypeRecursive<SlidingDateRangePicker>().FirstOrDefault();
            if ( slidingDateRangePicker != null && filterValues.Length > 1 )
            {
                slidingDateRangePicker.DelimitedValues = filterValues[1];
            }
        }

        /// <summary>
        /// Gets the filter format script.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        /// <remarks>
        /// This script must set a javascript variable named 'result' to a friendly string indicating value of filter controls
        /// a '$selectedContent' should be used to limit script to currently selected filter fields
        /// </remarks>
        public override string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
        {
            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            var format = "return Rock.reporting.formatFilterForDateTimeField('{0}', $selectedContent);";
            return string.Format( format, titleJs );
        }

        /// <summary>
        /// Checks to see if value is for 'current' date and if so, adjusts the date value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string ParseRelativeValue( string value )
        {
            if ( value.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
            {
                DateTime currentTime = RockDateTime.Now;

                var valueParts = value.Split( ':' );

                if ( valueParts.Length > 1 )
                {
                    currentTime = currentTime.AddMinutes( valueParts[1].AsInteger() );
                }

                return currentTime.ToString( "o" );
            }

            return value;
        }

        #endregion

    }
}