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

        #region enums

        /// <summary>
        /// 
        /// </summary>
        public enum DatePickerControlType
        {
            /// <summary>
            /// The date picker
            /// </summary>
            DatePicker,
            
            /// <summary>
            /// The date parts picker
            /// </summary>
            DatePartsPicker
        }

        #endregion

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
            keys.Add( "displayCurrentOption" );
            keys.Add( "datePickerControlType" );
            keys.Add( "futureYearCount" );
            return keys;
        }

        /// <summary>
        /// The DateFormat configuration control
        /// </summary>
        protected RockTextBox _tbDateFormat;

        /// <summary>
        /// The 'Display as Elapsed Time' configuration control
        /// </summary>
        protected RockCheckBox _cbDisplayDiff;
        
        /// <summary>
        /// The Date Picker Control Type configuration control
        /// </summary>
        protected RockDropDownList _ddlDatePickerMode;
        
        /// <summary>
        /// The Display Current configuration control
        /// </summary>
        protected RockCheckBox _cbDisplayCurrent;

        /// <summary>
        /// The future year count (for the date parts picker)
        /// </summary>
        protected NumberBox _nbFutureYearCount;

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override System.Collections.Generic.List<System.Web.UI.Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            _tbDateFormat = new RockTextBox();
            controls.Add( _tbDateFormat );
            _tbDateFormat.Label = "Date Format";
            _tbDateFormat.Help = "The format string to use for date (default is system short date).";

            _cbDisplayDiff = new RockCheckBox();
            controls.Add( _cbDisplayDiff );
            _cbDisplayDiff.Label = "Display as Elapsed Time";
            _cbDisplayDiff.Text = "Yes";
            _cbDisplayDiff.Help = "Display value as an elapsed time.";

            _ddlDatePickerMode = new RockDropDownList();
            controls.Add( _ddlDatePickerMode );
            _ddlDatePickerMode.Items.Clear();
            _ddlDatePickerMode.Items.Add( new ListItem( "Date Picker", DatePickerControlType.DatePicker.ConvertToString() ) );
            _ddlDatePickerMode.Items.Add( new ListItem( "Date Parts Picker", DatePickerControlType.DatePartsPicker.ConvertToString() ) );
            _ddlDatePickerMode.Label = "Control Type";
            _ddlDatePickerMode.Help = "Select 'Date Picker' to use a DatePicker, or 'Date Parts Picker' to select Month, Day and Year individually";
            _ddlDatePickerMode.AutoPostBack = true;
            _ddlDatePickerMode.SelectedIndexChanged += OnQualifierUpdated;

            _cbDisplayCurrent = new RockCheckBox();
            controls.Add( _cbDisplayCurrent );
            _cbDisplayCurrent.AutoPostBack = true;
            _cbDisplayCurrent.CheckedChanged += OnQualifierUpdated;
            _cbDisplayCurrent.Label = "Display Current Option";
            _cbDisplayCurrent.Text = "Yes";
            _cbDisplayCurrent.Help = "Include option to specify value as the current date.";

            _nbFutureYearCount = new NumberBox();
            controls.Add( _nbFutureYearCount );
            _nbFutureYearCount.Label = "Future Years";
            _nbFutureYearCount.Text = "";
            _nbFutureYearCount.Help = "The number of years in the future in include the year picker. Set to 0 to limit to current year. Leaving it blank will default to 50.";

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

            if ( controls != null && controls.Count > 3 )
            {
                var tbDateFormat = controls[0] as RockTextBox;
                var cbDisplayDiff = controls[1] as RockCheckBox;
                var ddlDatePickerMode = controls[2] as RockDropDownList;
                var cbDisplayCurrent = controls[3] as RockCheckBox;
                var nbFutureYearCount = controls[4] as NumberBox;

                if ( configurationValues.ContainsKey( "format" ) && tbDateFormat != null )
                {
                    tbDateFormat.Text = configurationValues["format"].Value ?? string.Empty;
                }

                if ( configurationValues.ContainsKey( "displayDiff" ) && cbDisplayDiff != null )
                {
                    cbDisplayDiff.Checked = configurationValues["displayDiff"].Value.AsBoolean( false );
                }

                DatePickerControlType datePickerControlType = DatePickerControlType.DatePicker;

                if ( configurationValues.ContainsKey( "datePickerControlType" ) && ddlDatePickerMode != null )
                {
                    ddlDatePickerMode.SetValue( configurationValues["datePickerControlType"].Value );
                    datePickerControlType = configurationValues["datePickerControlType"].Value.ConvertToEnumOrNull<DatePickerControlType>() ?? DatePickerControlType.DatePicker;
                }

                if ( configurationValues.ContainsKey( "displayCurrentOption" ) && cbDisplayCurrent != null )
                {
                    cbDisplayCurrent.Checked = configurationValues["displayCurrentOption"].Value.AsBoolean( false );

                    // only support the 'Use Current' option of they are using the DatePicker
                    cbDisplayCurrent.Visible = datePickerControlType == DatePickerControlType.DatePicker;
                }

                if ( configurationValues.ContainsKey( "futureYearCount" ) )
                {
                    nbFutureYearCount.Text = configurationValues["futureYearCount"].Value;
                }

                nbFutureYearCount.Visible = datePickerControlType == DatePickerControlType.DatePartsPicker;
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
            values.Add( "format", new ConfigurationValue( "Date Format", "The format string to use for date (default is system short date).", "" ) );
            values.Add( "displayDiff", new ConfigurationValue( "Display as Elapsed Time", "Display value as an elapsed time.", "False" ) );
            values.Add( "displayCurrentOption", new ConfigurationValue( "Display Current Option", "Include option to specify value as the current date.", "False" ) );
            values.Add( "datePickerControlType", new ConfigurationValue( "Control Type", "Select 'Date' to use a DatePicker, or 'Month,Day,Year' to select Month, Day and Year individually", DatePickerControlType.DatePicker.ConvertToString() ) );
            values.Add( "futureYearCount", new ConfigurationValue( "Future Years", "The number of years in the future in include the year picker. Set to 0 to limit to current year. Leaving it blank will default to 50.", string.Empty ) );

            if ( controls != null && controls.Count > 4 )
            {
                var tbDateFormat = controls[0] as RockTextBox;
                var cbDisplayDiff = controls[1] as RockCheckBox;
                var ddlDatePickerMode = controls[2] as RockDropDownList;
                var cbDisplayCurrent = controls[3] as RockCheckBox;
                var nbFutureYearCount = controls[4] as NumberBox;

                values["format"].Value = tbDateFormat.Text;
                values["displayDiff"].Value = cbDisplayDiff.Checked.ToString();
                values["displayCurrentOption"].Value = cbDisplayCurrent.Checked.ToString();
                values["datePickerControlType"].Value = ddlDatePickerMode.SelectedValue;
                values["futureYearCount"].Value = nbFutureYearCount.Text;
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
                            return string.Format( "Current Date plus {0} days", days.Value );
                        }
                        else
                        {
                            return string.Format( "Current Date minus {0} days", -days.Value );
                        }
                    }
                }

                return "Current Date";
            }
            else
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

                return formattedValue;
            }

        }

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object ValueAsFieldType( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value.AsDateTime();
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
            // return ValueAsFieldType which returns the value as a DateTime
            return this.ValueAsFieldType( parentControl, value, configurationValues );
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
            var datePickerControlType = configurationValues?.GetValueOrNull( "datePickerControlType" ).ConvertToEnumOrNull<DatePickerControlType>() ?? DatePickerControlType.DatePicker;
            switch ( datePickerControlType )
            {
                case DatePickerControlType.DatePartsPicker:
                    var datePartsPicker = new DatePartsPicker { ID = id };
                    datePartsPicker.FutureYearCount = configurationValues?.GetValueOrNull( "futureYearCount" ).AsIntegerOrNull();
                    return datePartsPicker;
                case DatePickerControlType.DatePicker:
                default:
                    var datePicker = new DatePicker { ID = id };
                    datePicker.DisplayCurrentOption = configurationValues?.GetValueOrNull( "displayCurrentOption" )?.AsBooleanOrNull() ?? false;
                    return datePicker;
            }
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var datePicker = control as DatePicker;
            var datePartsPicker = control as DatePartsPicker;
            if ( datePicker != null )
            {
                if ( datePicker.DisplayCurrentOption && datePicker.IsCurrentDateOffset )
                {
                    return string.Format( "CURRENT:{0}", datePicker.CurrentDateOffsetDays );
                }
                else if ( datePicker.SelectedDate.HasValue )
                {
                    return datePicker.SelectedDate.Value.ToString( "o" );
                }
                else
                {
                    return string.Empty;
                }
            }
            else if ( datePartsPicker != null )
            {
                if ( datePartsPicker.SelectedDate.HasValue )
                {
                    return datePartsPicker.SelectedDate.Value.ToString( "o" );
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
            var datePicker = control as DatePicker;
            var datePartsPicker = control as DatePartsPicker;
            if ( datePicker != null )
            {
                if ( datePicker.DisplayCurrentOption && value != null && value.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
                {
                    datePicker.IsCurrentDateOffset = true;
                    var valueParts = value.Split( ':' );
                    if ( valueParts.Length > 1 )
                    {
                        datePicker.CurrentDateOffsetDays = valueParts[1].AsInteger();
                    }
                    else
                    {
                        datePicker.CurrentDateOffsetDays = 0;
                    }
                }
                else
                {
                    datePicker.SelectedDate = value.AsDateTime();
                }
            }
            else if ( datePartsPicker != null )
            {
                datePartsPicker.SelectedDate = value.AsDateTime();
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the filter compare control with the specified FilterMode
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var ddlCompare = base.FilterCompareControl( configurationValues, id, required, filterMode );

            if ( ddlCompare is DropDownList )
            {
                var liBetween = ( ddlCompare as DropDownList ).Items.FindByValue( ComparisonType.Between.ConvertToInt().ToString() );
                if ( liBetween != null )
                {
                    // in the case of a 'between' comparison, change it to say 'range' since we use a sliding date range control to do the 'between' for dates
                    liBetween.Text = "Range";
                }
            }

            return ddlCompare;
        }

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
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var dateFiltersPanel = new Panel();
            dateFiltersPanel.ID = string.Format( "{0}_dtFilterControls", id );

            var datePickerPanel = new Panel();
            dateFiltersPanel.Controls.Add( datePickerPanel );

            var datePickerControlType = configurationValues?.GetValueOrNull( "datePickerControlType" ).ConvertToEnumOrNull<DatePickerControlType>() ?? DatePickerControlType.DatePicker;

            switch ( datePickerControlType )
            {
                case DatePickerControlType.DatePartsPicker:
                    var datePartsPicker = new DatePartsPicker { ID = id };
                    datePartsPicker.ID = string.Format( "{0}_dtPicker", id );
                    datePartsPicker.FutureYearCount = configurationValues?.GetValueOrNull( "futureYearCount" ).AsIntegerOrNull();
                    datePickerPanel.AddCssClass( "js-filter-control" );
                    datePickerPanel.Controls.Add( datePartsPicker );
                    break;
                case DatePickerControlType.DatePicker:
                default:
                    var datePicker = new DatePicker();
                    datePicker.ID = string.Format( "{0}_dtPicker", id );
                    datePicker.DisplayCurrentOption = true;
                    datePickerPanel.AddCssClass( "js-filter-control" );
                    datePickerPanel.Controls.Add( datePicker );
                    break;
            }

            

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = string.Format( "{0}_dtSlidingDateRange", id );
            slidingDateRangePicker.AddCssClass("js-filter-control-between");
            slidingDateRangePicker.Label = string.Empty;
            slidingDateRangePicker.PreviewLocation = SlidingDateRangePicker.DateRangePreviewLocation.Right;
            dateFiltersPanel.Controls.Add( slidingDateRangePicker );
            
            return dateFiltersPanel;
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
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
            var datePicker = dateFiltersPanel.ControlsOfTypeRecursive<DatePicker>().FirstOrDefault();
            var datePartsPicker = dateFiltersPanel.ControlsOfTypeRecursive<DatePartsPicker>().FirstOrDefault();
            var slidingDateRangePicker = dateFiltersPanel.ControlsOfTypeRecursive<SlidingDateRangePicker>().FirstOrDefault();
            string datePickerValue = string.Empty;
            string slidingDateRangePickerValue = string.Empty;
            if ( datePicker != null )
            {
                datePickerValue = this.GetEditValue( datePicker, configurationValues );
            }

            if ( datePartsPicker != null )
            {
                datePickerValue = this.GetEditValue( datePartsPicker, configurationValues );
            }

            if ( slidingDateRangePicker != null)
            {
                slidingDateRangePickerValue = slidingDateRangePicker.DelimitedValues;
            }

            // use Tab Delimited since slidingDateRangePicker is | delimited
            return string.Format( "{0}\t{1}" , datePickerValue, slidingDateRangePickerValue );
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
            if ( dateFiltersPanel != null )
            {
                var datePicker = dateFiltersPanel.ControlsOfTypeRecursive<DatePicker>().FirstOrDefault();
                if ( datePicker != null && filterValues.Length > 0 )
                {
                    this.SetEditValue( datePicker, configurationValues, filterValues[0] );
                }

                var datePartsPicker = dateFiltersPanel.ControlsOfTypeRecursive<DatePartsPicker>().FirstOrDefault();
                if ( datePartsPicker != null && filterValues.Length > 0 )
                {
                    this.SetEditValue( datePartsPicker, configurationValues, filterValues[0] );
                }

                var slidingDateRangePicker = dateFiltersPanel.ControlsOfTypeRecursive<SlidingDateRangePicker>().FirstOrDefault();
                if ( slidingDateRangePicker != null && filterValues.Length > 1 )
                {
                    slidingDateRangePicker.DelimitedValues = filterValues[1];
                }
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
            var format = "return Rock.reporting.formatFilterForDateField('{0}', $selectedContent);";
            return string.Format( format, titleJs );
        }

        /// <summary>
        /// Formats the filter values.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <returns></returns>
        public override string FormatFilterValues( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            if ( filterValues.Count >= 2 )
            {
                // uses Tab Delimited since slidingDateRangePicker is | delimited
                var filterValueValues = filterValues[1].Split( new string[] { "\t" }, StringSplitOptions.None );

                string comparisonValue = filterValues[0];
                if ( comparisonValue != "0" )
                {
                    ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                    if ( comparisonType == ComparisonType.Between && filterValueValues.Length > 1 )
                    {
                        var dateRangeText = SlidingDateRangePicker.FormatDelimitedValues( filterValueValues[1] );
                        return dateRangeText.IsNotNullOrWhiteSpace() ? string.Format( "During '{0}'", dateRangeText ) : null;
                    }
                    else
                    {
                        List<string> filterValuesForFormat = new List<string>();
                        filterValuesForFormat.Add( filterValues[0]);
                        filterValuesForFormat.Add(filterValueValues[0]);
                        return base.FormatFilterValues( configurationValues, filterValuesForFormat );
                    }
                }
            }

            return null;
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
            if ( filterValues.Count >= 2 )
            {
                // uses Tab Delimited since slidingDateRangePicker is | delimited
                var filterValueValues = filterValues[1].Split( new string[] { "\t" }, StringSplitOptions.None );

                // Parse for RelativeValue of DateTime (if specified)
                filterValueValues[0] = ParseRelativeValue( filterValueValues[0] );
            
                string comparisonValue = filterValues[0];
                if ( comparisonValue != "0" && comparisonValue.IsNotNullOrWhiteSpace() )
                {
                    ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );
                    if (comparisonType == ComparisonType.Between && filterValueValues.Length > 1)
                    {
                        var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( filterValueValues[1] );
                        ConstantExpression constantExpressionLower = dateRange.Start.HasValue 
                            ? Expression.Constant( dateRange.Start, typeof( DateTime ) )
                            : null;

                        ConstantExpression constantExpressionUpper = dateRange.End.HasValue
                            ? Expression.Constant( dateRange.End, typeof( DateTime ) )
                            : null;

                        if ( constantExpressionLower == null && constantExpressionUpper == null )
                        {
                            return new NoAttributeFilterExpression();
                        }
                        else
                        {
                            return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpressionLower, constantExpressionUpper );
                        }
                    }
                    else
                    {
                        var dateTime = filterValueValues[0].AsDateTime();
                        if ( dateTime.HasValue )
                        {
                            ConstantExpression constantExpression = Expression.Constant( dateTime, typeof( DateTime ) );
                            return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                        }
                        else
                        {
                            if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                            {
                                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, null );
                            }
                            else
                            {
                                return new NoAttributeFilterExpression();
                            }
                        }
                    }
                }
            }

            return null;
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
            var comparison = PropertyFilterExpression( configurationValues, filterValues, parameterExpression, "ValueAsDateTime", typeof( DateTime? ) );

            if ( comparison == null )
            {
                return new Rock.Data.NoAttributeFilterExpression();
            }

            return comparison;
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

            if (filterComparisonType == null )
            {
                return false;
            }

            ComparisonType? equalToCompareValue = GetEqualToCompareValue().ConvertToEnumOrNull<ComparisonType>();
            DateTime? valueAsDateTime = value.AsDateTime();

            // uses Tab Delimited since slidingDateRangePicker is | delimited
            var filterValueValues = filterValues[1].Split( new string[] { "\t" }, StringSplitOptions.None );

            // Parse for RelativeValue of DateTime (if specified)
            filterValueValues[0] = ParseRelativeValue( filterValueValues[0] );
            DateTime? filterValueAsDateTime1;
            DateTime? filterValueAsDateTime2 = null;
            if ( filterComparisonType == ComparisonType.Between )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( filterValueValues[1] );
                filterValueAsDateTime1 = dateRange.Start;
                filterValueAsDateTime2 = dateRange.End;
            }
            else
            {
                filterValueAsDateTime1 = filterValueValues[0].AsDateTime();
                filterValueAsDateTime2 = null;
            }

            return ComparisonHelper.CompareNumericValues( filterComparisonType.Value, valueAsDateTime?.Ticks, filterValueAsDateTime1?.Ticks, filterValueAsDateTime2?.Ticks );
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
                return "ValueAsDateTime";
            }
        }

        /// <summary>
        /// Attributes the constant expression.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override ConstantExpression AttributeConstantExpression( string value )
        {
            var dateTime = value.AsDateTime() ?? DateTime.MinValue;
            return Expression.Constant( dateTime, typeof( DateTime ) );
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
                return typeof( DateTime? );
            }
        }

        /// <summary>
        /// Checks to see if value is for 'current' date and if so, adjusts the date value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual string ParseRelativeValue( string value )
        {
            if ( value.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
            {
                DateTime currentDate = RockDateTime.Today;

                var valueParts = value.Split( ':' );

                if ( valueParts.Length > 1 )
                {
                    currentDate = currentDate.AddDays( valueParts[1].AsInteger() );
                }

                return currentDate.ToString( "o" );
            }

            return value;
        }

        #endregion

    }
}