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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class SlidingDateRangePicker : CompositeControl, IRockControl
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        private DropDownList _ddlLastCurrent;
        private NumberBox _nbNumber;
        private DropDownList _ddlTimeUnitTypeSingular;
        private DropDownList _ddlTimeUnitTypePlural;
        private DateRangePicker _drpDateRange;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingDateRangePicker"/> class.
        /// </summary>
        public SlidingDateRangePicker()
            : base()
        {
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            this.Label = "Date Range";

            _ddlLastCurrent = new DropDownList();
            _ddlLastCurrent.CssClass = "form-control input-width-md js-slidingdaterange-select slidingdaterange-select";
            _ddlLastCurrent.ID = "ddlLastCurrent_" + this.ID;
            _ddlLastCurrent.SelectedIndexChanged += ddl_SelectedIndexChanged;

            _nbNumber = new NumberBox();
            _nbNumber.CssClass = "form-control input-width-sm js-number slidingdaterange-number";
            _nbNumber.NumberType = ValidationDataType.Integer;
            _nbNumber.ID = "nbNumber_" + this.ID;
            _nbNumber.Text = "1";

            _ddlTimeUnitTypeSingular = new DropDownList();
            _ddlTimeUnitTypeSingular.CssClass = "form-control input-width-md js-time-units-singular slidingdaterange-timeunits-singular";
            _ddlTimeUnitTypeSingular.ID = "ddlTimeUnitTypeSingular_" + this.ID;
            _ddlTimeUnitTypeSingular.SelectedIndexChanged += ddl_SelectedIndexChanged;

            _ddlTimeUnitTypePlural = new DropDownList();
            _ddlTimeUnitTypePlural.CssClass = "form-control input-width-md js-time-units-plural slidingdaterange-timeunits-plural";
            _ddlTimeUnitTypePlural.ID = "ddlTimeUnitTypePlural_" + this.ID;
            _ddlTimeUnitTypePlural.SelectedIndexChanged += ddl_SelectedIndexChanged;

            _drpDateRange = new DateRangePicker();

            // change the inputsClass on the DateRangePicker to "" instead of "form-control-group";
            _drpDateRange.InputsClass = "";
            _drpDateRange.CssClass = "js-time-units-date-range slidingdaterange-daterange";
            _drpDateRange.ID = "drpDateRange_" + this.ID;

            Controls.Add( _ddlLastCurrent );
            Controls.Add( _nbNumber );
            Controls.Add( _ddlTimeUnitTypeSingular );
            Controls.Add( _ddlTimeUnitTypePlural );
            Controls.Add( _drpDateRange );

            PopulateDropDowns();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the dropdown list controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddl_SelectedIndexChanged( object sender, EventArgs e )
        {
            EnsureChildControls();

            if ( SelectedDateRangeChanged != null )
            {
                SelectedDateRangeChanged( this, e );
            }
        }

        /// <summary>
        /// Occurs when [selected date range changed].
        /// </summary>
        public event EventHandler SelectedDateRangeChanged;

        /// <summary>
        /// Populates the drop downs.
        /// </summary>
        private void PopulateDropDowns()
        {
            EnsureChildControls();
            _ddlLastCurrent.Items.Clear();
            _ddlLastCurrent.Items.Add( new ListItem( string.Empty, SlidingDateRangeType.All.ConvertToInt().ToString() ) );
            _ddlLastCurrent.Items.Add( new ListItem( SlidingDateRangeType.Current.ConvertToString(), SlidingDateRangeType.Current.ConvertToInt().ToString() ) );
            _ddlLastCurrent.Items.Add( new ListItem( SlidingDateRangeType.Previous.ConvertToString(), SlidingDateRangeType.Previous.ConvertToInt().ToString() ) );
            _ddlLastCurrent.Items.Add( new ListItem( SlidingDateRangeType.Last.ConvertToString(), SlidingDateRangeType.Last.ConvertToInt().ToString() ) );
            _ddlLastCurrent.Items.Add( new ListItem( SlidingDateRangeType.DateRange.ConvertToString(), SlidingDateRangeType.DateRange.ConvertToInt().ToString() ) );

            _ddlTimeUnitTypeSingular.Items.Clear();
            _ddlTimeUnitTypePlural.Items.Clear();

            foreach ( var item in Enum.GetValues( typeof( TimeUnitType ) ).OfType<TimeUnitType>() )
            {
                _ddlTimeUnitTypeSingular.Items.Add( new ListItem( item.ConvertToString(), item.ConvertToInt().ToString() ) );
                _ddlTimeUnitTypePlural.Items.Add( new ListItem( item.ConvertToString().Pluralize(), item.ConvertToInt().ToString() ) );
            }

            ddl_SelectedIndexChanged( null, null );
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum SlidingDateRangeType
        {
            /// <summary>
            /// All
            /// </summary>
            All = -1,

            /// <summary>
            /// The last X days,weeks,months, etc (inclusive of current day,week,month,...)
            /// </summary>
            Last = 0,

            /// <summary>
            /// The current day,week,month,year
            /// </summary>
            Current = 1,

            /// <summary>
            /// The date range
            /// </summary>
            DateRange = 2,

            /// <summary>
            /// The previous X days,weeks,months, etc (excludes current day,week,month,...)
            /// </summary>
            Previous = 4
        }

        /// <summary>
        /// 
        /// </summary>
        public enum TimeUnitType
        {
            /// <summary>
            /// The hour
            /// </summary>
            Hour = 0,

            /// <summary>
            /// The day
            /// </summary>
            Day = 1,

            /// <summary>
            /// The week
            /// </summary>
            Week = 2,

            /// <summary>
            /// The month
            /// </summary>
            Month = 3,

            /// <summary>
            /// The year
            /// </summary>
            Year = 4
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer, "slidingdaterange" );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            bool needsAutoPostBack = SelectedDateRangeChanged != null;
            _ddlLastCurrent.AutoPostBack = needsAutoPostBack;
            _ddlTimeUnitTypeSingular.AutoPostBack = needsAutoPostBack;
            _ddlTimeUnitTypePlural.AutoPostBack = needsAutoPostBack;

            writer.WriteLine();
            writer.AddAttribute( "class", "label label-info js-slidingdaterange-info slidingdaterange-info" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _ddlLastCurrent.RenderControl( writer );
            _nbNumber.RenderControl( writer );
            _ddlTimeUnitTypeSingular.RenderControl( writer );
            _ddlTimeUnitTypePlural.RenderControl( writer );
            _drpDateRange.RenderControl( writer );

            writer.RenderEndTag();

            RegisterJavaScript();
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            var script = string.Format( @"Rock.controls.slidingDateRangePicker.initialize({{ id: '{0}' }});", this.ClientID );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "slidingdaterange_picker-" + this.ClientID, script, true );
        }

        /// <summary>
        /// Gets or sets the Last, Current, or Date Range mode
        /// </summary>
        /// <value>
        /// The last or current.
        /// </value>
        public SlidingDateRangeType SlidingDateRangeMode
        {
            get
            {
                EnsureChildControls();
                return _ddlLastCurrent.SelectedValueAsEnum<SlidingDateRangeType>();
            }

            set
            {
                EnsureChildControls();
                _ddlLastCurrent.SelectedValue = value.ConvertToInt().ToString();
                ddl_SelectedIndexChanged( null, null );
            }
        }

        /// <summary>
        /// Gets or sets the number of time units (x Hours, x Days, etc) depending on TimeUnit selection
        /// </summary>
        /// <value>
        /// The number of time units.
        /// </value>
        public int NumberOfTimeUnits
        {
            get
            {
                EnsureChildControls();
                return _nbNumber.Text.AsIntegerOrNull() ?? 1;
            }

            set
            {
                EnsureChildControls();
                _nbNumber.Text = ( value <= 0 ? 1 : value ).ToString();
            }
        }

        /// <summary>
        /// Gets or sets the time unit (Hour, Day, Year, etc)
        /// </summary>
        /// <value>
        /// The time unit.
        /// </value>
        public TimeUnitType TimeUnit
        {
            get
            {
                EnsureChildControls();
                if ( ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous ).HasFlag( this.SlidingDateRangeMode ) )
                {
                    return _ddlTimeUnitTypePlural.SelectedValueAsEnum<TimeUnitType>();
                }
                else
                {
                    return _ddlTimeUnitTypeSingular.SelectedValueAsEnum<TimeUnitType>();
                }
            }

            set
            {
                EnsureChildControls();
                _ddlTimeUnitTypePlural.SelectedValue = value.ConvertToInt().ToString();
                _ddlTimeUnitTypeSingular.SelectedValue = value.ConvertToInt().ToString();
            }
        }

        /// <summary>
        /// Gets the date range mode value.
        /// </summary>
        /// <value>
        /// The date range mode value.
        /// </value>
        public DateTime? DateRangeModeStart
        {
            get
            {
                EnsureChildControls();
                return _drpDateRange.LowerValue;
            }

            set
            {
                EnsureChildControls();
                _drpDateRange.LowerValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the date range mode end.
        /// </summary>
        /// <value>
        /// The date range mode end.
        /// </value>
        public DateTime? DateRangeModeEnd
        {
            get
            {
                EnsureChildControls();
                return _drpDateRange.UpperValue;
            }

            set
            {
                EnsureChildControls();
                _drpDateRange.UpperValue = value;
            }
        }

        /// <summary>
        /// Sets the date range mode value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetDateRangeModeValue( DateRange value )
        {
            EnsureChildControls();
            _drpDateRange.LowerValue = value.Start;
            _drpDateRange.UpperValue = value.End;
        }

        /// <summary>
        /// Gets or sets the SlidingDateRangeMode, NumberOfTimeUnits, and TimeUnit values by specifying pipe-delimited values
        /// </summary>
        /// <value>
        /// The delimited values.
        /// </value>
        public string DelimitedValues
        {
            get
            {
                return string.Format(
                    "{0}|{1}|{2}|{3}|{4}",
                    this.SlidingDateRangeMode,
                    ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous ).HasFlag( this.SlidingDateRangeMode ) ? this.NumberOfTimeUnits : (int?)null,
                    ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Current ).HasFlag( this.SlidingDateRangeMode ) ? this.TimeUnit : (TimeUnitType?)null,
                    this.SlidingDateRangeMode == SlidingDateRangeType.DateRange ? this.DateRangeModeStart : null,
                    this.SlidingDateRangeMode == SlidingDateRangeType.DateRange ? this.DateRangeModeEnd : null );
            }

            set
            {
                string[] splitValues = ( value ?? string.Empty ).Split( '|' );
                if ( splitValues.Length == 5 )
                {
                    this.SlidingDateRangeMode = splitValues[0].ConvertToEnum<SlidingDateRangeType>();
                    this.NumberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                    this.TimeUnit = splitValues[2].ConvertToEnumOrNull<TimeUnitType>() ?? TimeUnitType.Day;
                    this.DateRangeModeStart = splitValues[3].AsDateTime();
                    this.DateRangeModeEnd = splitValues[4].AsDateTime();
                }
            }
        }

        /// <summary>
        /// Formats the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatDelimitedValues( string value )
        {
            string[] splitValues = ( value ?? string.Empty ).Split( '|' );
            string result = string.Empty;
            if ( splitValues.Length == 3 )
            {
                var slidingDateRangeMode = splitValues[0].ConvertToEnum<SlidingDateRangeType>();
                var numberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                var timeUnit = splitValues[2].ConvertToEnumOrNull<TimeUnitType>();
                var start = splitValues[3].AsDateTime();
                var end = splitValues[4].AsDateTime();
                if ( slidingDateRangeMode == SlidingDateRangeType.Current )
                {
                    return string.Format( "{0} {1}", slidingDateRangeMode.ConvertToString(), timeUnit.ConvertToString() );
                }
                else if ( ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous ).HasFlag( slidingDateRangeMode ) )
                {
                    return string.Format( "{0} {1} {2}", slidingDateRangeMode.ConvertToString(), numberOfTimeUnits, timeUnit.ConvertToString() );
                }
                else
                {
                    return string.Format( "{0}: {1} to {2}", slidingDateRangeMode.ConvertToString(), start, end );
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the date range from delimited values in format SlidingDateRangeType|Number|TimeUnitType|StartDate|EndDate
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static DateRange CalculateDateRangeFromDelimitedValues( string value )
        {
            string[] splitValues = ( value ?? "1||4||" ).Split( '|' );
            DateRange result = new DateRange();
            if ( splitValues.Length == 5 )
            {
                var slidingDateRangeMode = splitValues[0].ConvertToEnum<SlidingDateRangeType>();
                var numberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                TimeUnitType? timeUnit = splitValues[2].ConvertToEnumOrNull<TimeUnitType>();
                DateTime currentDateTime = RockDateTime.Now;
                if ( slidingDateRangeMode == SlidingDateRangeType.Current )
                {
                    switch ( timeUnit )
                    {
                        case TimeUnitType.Hour:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 );
                            result.End = result.Start.Value.AddHours( 1 );
                            break;

                        case TimeUnitType.Day:
                            result.Start = currentDateTime.Date;
                            result.End = result.Start.Value.AddDays( 1 );
                            break;

                        case TimeUnitType.Week:

                            // from http://stackoverflow.com/a/38064/1755417
                            int diff = currentDateTime.DayOfWeek - RockDateTime.FirstDayOfWeek;
                            if ( diff < 0 )
                            {
                                diff += 7;
                            }

                            result.Start = currentDateTime.AddDays( -1 * diff ).Date;
                            result.End = result.Start.Value.AddDays( 7 );
                            break;

                        case TimeUnitType.Month:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 );
                            result.End = result.Start.Value.AddMonths( 1 );
                            break;

                        case TimeUnitType.Year:
                            result.Start = new DateTime( currentDateTime.Year, 1, 1 );
                            result.End = new DateTime( currentDateTime.Year + 1, 1, 1 );
                            break;
                    }
                }
                else if ( ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous ).HasFlag( slidingDateRangeMode ) )
                {
                    // Last X Days/Hours. NOTE: addCount is the number of X that it go back (it'll actually subtract)
                    int addCount = numberOfTimeUnits;

                    // if we are getting "Last" round up to inlude the current day/week/month/year
                    int roundUpCount = slidingDateRangeMode == SlidingDateRangeType.Last ? 1 : 0;

                    switch ( timeUnit )
                    {
                        case TimeUnitType.Hour:
                            result.End = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 ).AddHours( roundUpCount );
                            result.Start = result.End.Value.AddHours( -addCount );
                            break;

                        case TimeUnitType.Day:
                            result.End = currentDateTime.Date.AddDays( roundUpCount );
                            result.Start = result.End.Value.AddDays( -addCount );
                            break;

                        case TimeUnitType.Week:
                            // from http://stackoverflow.com/a/38064/1755417
                            int diff = currentDateTime.DayOfWeek - RockDateTime.FirstDayOfWeek;
                            if ( diff < 0 )
                            {
                                diff += 7;
                            }

                            result.End = currentDateTime.AddDays( -1 * diff ).Date.AddDays( 7 * roundUpCount );
                            result.Start = result.End.Value.AddDays( -addCount * 7 );
                            break;

                        case TimeUnitType.Month:
                            result.End = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 ).AddMonths( roundUpCount );
                            result.Start = result.End.Value.AddMonths( -addCount );
                            break;

                        case TimeUnitType.Year:
                            result.End = new DateTime( currentDateTime.Year, 1, 1 ).AddYears( roundUpCount );
                            result.Start = result.End.Value.AddYears( -addCount );
                            break;
                    }
                }
                else if ( slidingDateRangeMode == SlidingDateRangeType.DateRange )
                {
                    result.Start = splitValues[3].AsDateTime();
                    DateTime? endDateTime = splitValues[4].AsDateTime();
                    if ( endDateTime.HasValue )
                    {
                        // add a day to the end since the compare will be "< EndDateTime"
                        result.End = endDateTime.Value.AddDays( 1 );
                    }
                    else
                    {
                        result.End = null;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the help HTML that explains usage of the SlidingDateRange picker with examples
        /// </summary>
        /// <param name="currentDateTime">The current date time.</param>
        /// <returns></returns>
        public static string GetHelpHtml( DateTime currentDateTime )
        {
            SlidingDateRangePicker helperPicker = new SlidingDateRangePicker();
            SlidingDateRangeType[] slidingDateRangeTypesForHelp = new SlidingDateRangeType[] { SlidingDateRangeType.Current, SlidingDateRangeType.Previous, SlidingDateRangeType.Last };

            string helpHtml = @"
    
    <div class='slidingdaterange-help'>

        <p>A date range can either be a specific date range, or a sliding date range based on the current date and time.</p>
        <p>For a sliding date range, you can choose either <strong>current, previous, or last</strong> with a time period of <strong>hour, day, week, month, or year</strong>. Note that a week is Monday thru Sunday.</p>
        <br />
        <ul class=''>
            <li><strong>Current</strong> - the time period that the current date/time is in</li>
            <li><strong>Previous</strong> - the time period(s) prior to the current period (does not include the current time period). For example, to see the most recent weekend, select 'Previous 1 Week'</li>
            <li><strong>Last</strong> - the last X time period(s), including the current period. For example, to see the current week and prior week, select 'Last 2 weeks'</li>
        </ul>

        <h3>Preview of the sliding date ranges</h3>";

            foreach ( var slidingDateRangeType in slidingDateRangeTypesForHelp )
            {
                helperPicker.SlidingDateRangeMode = slidingDateRangeType;
                helperPicker.NumberOfTimeUnits = 2;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat( @"<h4>{0}</h4>", slidingDateRangeType.ConvertToString() );
                sb.AppendLine( "<ul>" );
                foreach ( var timeUnitType in Enum.GetValues( typeof( TimeUnitType ) ).OfType<TimeUnitType>() )
                {
                    helperPicker.TimeUnit = timeUnitType;
                    sb.AppendFormat( @"
                    <li>
                        <span class='slidingdaterange-help-key'>{0} {1}</span>
                        <span class='slidingdaterange-help-value'> - {2}</span>
                    </li>",
                          slidingDateRangeType != SlidingDateRangeType.Current ? helperPicker.NumberOfTimeUnits.ToString() : string.Empty,
                          helperPicker.TimeUnit.ConvertToString().PluralizeIf( slidingDateRangeType != SlidingDateRangeType.Current && helperPicker.NumberOfTimeUnits > 1 ),
                          SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( helperPicker.DelimitedValues ).ToStringAutomatic() );
                }
                sb.AppendLine( "</ul>" );

                helpHtml += sb.ToString();
            }

            helpHtml += @"
    </div>
";

            return helpHtml;
        }
    }
}