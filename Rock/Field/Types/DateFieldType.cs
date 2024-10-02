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
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
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
    [FieldTypeUsage( FieldTypeUsage.Common )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms | Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M6,8.88H4.89a.33.33,0,0,1-.32-.33V7.45a.33.33,0,0,1,.32-.33H6a.33.33,0,0,1,.33.33v1.1A.33.33,0,0,1,6,8.88Zm2.9-.33V7.45a.33.33,0,0,0-.32-.33H7.46a.33.33,0,0,0-.32.33v1.1a.33.33,0,0,0,.32.33H8.54A.33.33,0,0,0,8.86,8.55Zm2.57,0V7.45a.33.33,0,0,0-.32-.33H10a.33.33,0,0,0-.33.33v1.1a.33.33,0,0,0,.33.33h1.07A.33.33,0,0,0,11.43,8.55ZM8.86,11.17V10.08a.33.33,0,0,0-.32-.33H7.46a.33.33,0,0,0-.32.33v1.09a.33.33,0,0,0,.32.33H8.54A.33.33,0,0,0,8.86,11.17Zm-2.57,0V10.08A.33.33,0,0,0,6,9.75H4.89a.33.33,0,0,0-.32.33v1.09a.33.33,0,0,0,.32.33H6A.33.33,0,0,0,6.29,11.17Zm5.14,0V10.08a.33.33,0,0,0-.32-.33H10a.33.33,0,0,0-.33.33v1.09a.33.33,0,0,0,.33.33h1.07A.33.33,0,0,0,11.43,11.17ZM14,4.06v9.63A1.3,1.3,0,0,1,12.71,15H3.29A1.3,1.3,0,0,1,2,13.69V4.06A1.3,1.3,0,0,1,3.29,2.75H4.57V1.33A.33.33,0,0,1,4.89,1H6a.33.33,0,0,1,.33.33V2.75H9.71V1.33A.33.33,0,0,1,10,1h1.07a.33.33,0,0,1,.32.33V2.75h1.28A1.3,1.3,0,0,1,14,4.06Zm-1.29,9.46V5.38H3.29v8.14a.17.17,0,0,0,.16.17h9.1A.17.17,0,0,0,12.71,13.52Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.DATE )]
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

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            return FormatValue( value, configurationValues, false );
        }

        /// <inheritdoc/>
        public override string GetCondensedTextValue( string value, Dictionary<string, string> configurationValues )
        {
            return FormatValue( value, configurationValues, true );
        }

        /// <inheritdoc/>
        public override string GetCondensedHtmlValue( string value, Dictionary<string, string> configurationValues )
        {
            return FormatValue( value, configurationValues, true ).EncodeHtml();
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
                        !String.IsNullOrWhiteSpace( configurationValues["format"] ) )
                    {
                        try
                        {
                            formattedValue = dateValue.Value.ToString( configurationValues["format"] );
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
                            if ( bool.TryParse( configurationValues["displayDiff"], out var displayDiff ) && displayDiff )
                            {
                                formattedValue += " (" + dateValue.ToElapsedString( true, false ) + ")";
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
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
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
                        filterValuesForFormat.Add( filterValues[0] );
                        filterValuesForFormat.Add( filterValueValues[0] );
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
                    if ( comparisonType == ComparisonType.Between && filterValueValues.Length > 1 )
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
                            /*
                             * Convert expressions to int if the property type is an int
                             */
                            if ( propertyType == typeof( int ) || propertyType == typeof( int? ) )
                            {
                                if ( constantExpressionLower != null )
                                {
                                    constantExpressionLower = Expression.Constant( Convert.ToDateTime( constantExpressionLower.Value ).ToString( "yyyyMMdd" ).AsInteger(), typeof( int ) );
                                }
                                if ( constantExpressionUpper != null )
                                {
                                    constantExpressionUpper = Expression.Constant( Convert.ToDateTime( constantExpressionUpper.Value ).ToString( "yyyyMMdd" ).AsInteger(), typeof( int ) );
                                }
                            }

                            return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpressionLower, constantExpressionUpper );
                        }
                    }
                    else
                    {
                        var dateTime = filterValueValues[0].AsDateTime();
                        if ( dateTime.HasValue )
                        {
                            ConstantExpression constantExpression = Expression.Constant( dateTime, typeof( DateTime ) );
                            if ( propertyType == typeof( int ) || propertyType == typeof( int? ) )
                            {
                                constantExpression = Expression.Constant( dateTime?.ToString( "yyyyMMdd" ).AsInteger(), typeof( int ) );
                            }

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

            if ( filterComparisonType == null )
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

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldFormat = oldPrivateConfigurationValues.GetValueOrNull( "format" ) ?? string.Empty;
            var oldDisplayDiff = oldPrivateConfigurationValues.GetValueOrNull( "displayDiff" ) ?? string.Empty;
            var newFormat = newPrivateConfigurationValues.GetValueOrNull( "format" ) ?? string.Empty;
            var newDisplayDiff = newPrivateConfigurationValues.GetValueOrNull( "displayDiff" ) ?? string.Empty;

            if ( oldFormat != newFormat )
            {
                return true;
            }

            if ( oldDisplayDiff != newDisplayDiff )
            {
                return true;
            }

            return false;
        }

        #endregion

        #region WebForms
#if WEBFORMS

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
        /// This event handler is triggerend when the Control Type is modified to show or hide related
        /// options ('Future Years' or 'Display Current Option').
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnControlTypeChanged( object sender, EventArgs e )
        {
            var senderControl = ( sender as Control );
            if ( senderControl == null || senderControl.Parent == null )
            {
                OnQualifierUpdated( sender, e );
                return;
            }

            // Reset the visibility of the nbFutureYearCount control when the Control Type changes.
            var controls = senderControl.Parent.Controls;
            if ( controls != null && controls.Count >= 5 )
            {
                var ddlDatePickerMode = controls[2] as RockDropDownList;
                var cbDisplayCurrent = controls[3] as RockCheckBox;
                var nbFutureYearCount = controls[4] as NumberBox;

                DatePickerControlType datePickerControlType = ddlDatePickerMode.SelectedValue.ConvertToEnumOrNull<DatePickerControlType>() ?? DatePickerControlType.DatePicker;

                // only support the 'Use Current' option of they are using the DatePicker
                cbDisplayCurrent.Visible = datePickerControlType == DatePickerControlType.DatePicker;

                // only support the 'Future Years' option of they are using the DatePartsPicker
                nbFutureYearCount.Visible = datePickerControlType == DatePickerControlType.DatePartsPicker;
            }

            OnQualifierUpdated( sender, e );
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override System.Collections.Generic.List<System.Web.UI.Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var tbDateFormat = new RockTextBox();
            controls.Add( tbDateFormat );
            tbDateFormat.Label = "Date Format";
            tbDateFormat.Help = "The format string to use for date (default is system short date).";

            var cbDisplayDiff = new RockCheckBox();
            controls.Add( cbDisplayDiff );
            cbDisplayDiff.Label = "Display as Elapsed Time";
            cbDisplayDiff.Help = "Display value as an elapsed time.";

            var ddlDatePickerMode = new RockDropDownList();
            controls.Add( ddlDatePickerMode );
            ddlDatePickerMode.Items.Clear();
            ddlDatePickerMode.Items.Add( new ListItem( "Date Picker", DatePickerControlType.DatePicker.ConvertToString() ) );
            ddlDatePickerMode.Items.Add( new ListItem( "Date Parts Picker", DatePickerControlType.DatePartsPicker.ConvertToString() ) );
            ddlDatePickerMode.Label = "Control Type";
            ddlDatePickerMode.Help = "Select 'Date Picker' to use a DatePicker, or 'Date Parts Picker' to select Month, Day and Year individually";
            ddlDatePickerMode.AutoPostBack = true;
            ddlDatePickerMode.SelectedIndexChanged += OnControlTypeChanged;

            var cbDisplayCurrent = new RockCheckBox();
            controls.Add( cbDisplayCurrent );
            cbDisplayCurrent.AutoPostBack = true;
            cbDisplayCurrent.CheckedChanged += OnQualifierUpdated;
            cbDisplayCurrent.Label = "Display Current Option";
            cbDisplayCurrent.Help = "Include option to specify value as the current date.";

            var nbFutureYearCount = new NumberBox();
            controls.Add( nbFutureYearCount );
            nbFutureYearCount.Label = "Future Years";
            nbFutureYearCount.Text = "";
            nbFutureYearCount.Help = "The number of years in the future in include the year picker. Set to 0 to limit to current year. Leaving it blank will default to 50.";

            // if this is the child type of DateTimeFieldType, change the labels and visibility of the controls as needed
            if ( this is DateTimeFieldType )
            {
                tbDateFormat.Label = "Date Time Format";
                tbDateFormat.Help = "The format string to use for date (default is system short date and time).";
                ddlDatePickerMode.Visible = false;
                nbFutureYearCount.Visible = false;
                cbDisplayCurrent.Help = "Include option to specify value as the current time.";
            }

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

            if ( controls != null && controls.Count >= 5 )
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

                // only support the 'Future Years' option of they are using the DatePartsPicker
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
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
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

#endif
        #endregion
    }
}