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
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a date/time value
    /// </summary>
    [Serializable]
    [FieldTypeUsage( FieldTypeUsage.Common )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms | Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><g><path d=""M7.91,13.69h-6a.44.44,0,0,1-.44-.44V5.38H12.38V4.06a1.32,1.32,0,0,0-1.32-1.31H9.75V1.44A.44.44,0,0,0,9.31,1H8.44A.44.44,0,0,0,8,1.44V2.75H4.5V1.44A.44.44,0,0,0,4.06,1H3.19a.44.44,0,0,0-.44.44V2.75H1.44A1.32,1.32,0,0,0,.12,4.06v9.63A1.32,1.32,0,0,0,1.44,15H9.18A4.66,4.66,0,0,1,7.91,13.69Z""/><path d=""M11.94,7.12a3.94,3.94,0,1,0,3.94,3.94A3.94,3.94,0,0,0,11.94,7.12Zm0,7.14a3.2,3.2,0,1,1,3.2-3.2A3.2,3.2,0,0,1,11.94,14.26Zm1.57-2.72-1.2-.69V9a.38.38,0,0,0-.37-.37.37.37,0,0,0-.37.37v2.09a.37.37,0,0,0,.18.32l1.39.8a.53.53,0,0,0,.18.05.36.36,0,0,0,.32-.18A.38.38,0,0,0,13.51,11.54Z""/></g></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.DATE_TIME )]
    public class DateTimeFieldType : DateFieldType
    {
        #region Configuration

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            return GetTextOrCondensedValue( value, configurationValues, false );
        }

        /// <inheritdoc/>
        public override string GetCondensedTextValue( string value, Dictionary<string, string> configurationValues )
        {
            return GetTextOrCondensedValue( value, configurationValues, true );
        }

        /// <summary>
        /// Formats the value for display as a date.
        /// </summary>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        private string FormatValue( string value, Dictionary<string, string> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues )
                : GetCondensedTextValue( value, configurationValues );
        }

        private static string GetTextOrCondensedValue( string value, Dictionary<string, string> configurationValues, bool condensed )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }

            if ( value.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
            {
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
                        !configurationValues["format"].IsNullOrWhiteSpace() )
                    {
                        try
                        {
                            formattedValue = dateValue.Value.ToString( configurationValues["format"] );
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
                            bool displayDiff = configurationValues["displayDiff"].AsBooleanOrNull() ?? false;
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

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Try to ensure the value is the proper format.
            if ( DateTime.TryParse( publicValue, out var dateTimeValue ) )
            {
                return dateTimeValue.ToString( "o" );
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
        }

        #endregion

        #region Filter Control

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

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override System.Collections.Generic.List<System.Web.UI.Control> ConfigurationControls()
        {
            // DateFieldType takes care of creating the ConfigurationControls, and
            return base.ConfigurationControls();
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
            return FormatValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ), condensed );
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
        /// Gets the filter value.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode )
        {
            var values = new List<string>();

            if ( filterControl != null )
            {
                try
                {
                    string compare = GetFilterCompareValue( filterControl.Controls[0].Controls[0], filterMode );
                    if ( compare != "0" )
                    {
                        values.Add( compare );
                    }

                    ComparisonType? comparisonType = compare.ConvertToEnumOrNull<ComparisonType>();
                    if ( comparisonType.HasValue )
                    {
                        if ( ( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType.Value ) )
                        {
                            // if using IsBlank or IsNotBlank, we don't care about the value, so don't try to grab it from the UI
                            values.Add( string.Empty );
                        }
                        else
                        {
                            string value = GetFilterValueValue( filterControl.Controls[1].Controls[0], configurationValues );
                            var filterValues = value.Split( new string[] { "\t" }, StringSplitOptions.None );
                            if ( filterValues.All( a => a.IsNullOrWhiteSpace() ) )
                            {
                                return new List<string>();
                            }

                            values.Add( value );
                        }
                    }

                }
                catch
                {
                    // intentionally ignore error
                }
            }

            return values;
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
                var selectedDateRange = slidingDateRangePicker.SelectedDateRange;
                if ( selectedDateRange != null && ( selectedDateRange.Start.HasValue || selectedDateRange.End.HasValue ) )
                {
                    slidingDateRangePickerValue = slidingDateRangePicker.DelimitedValues;
                }
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

#endif
        #endregion
    }
}