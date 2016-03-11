﻿// <copyright>
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
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
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

        /// <summary>
        /// Gets or sets the date preview location.
        /// </summary>
        /// <value>
        /// The date preview location.
        /// </value>
        public DateRangePreviewLocation PreviewLocation { get; set; }

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
            Label = "Date Range";
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

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

            SlidingDateRangeType[] sortedTypes = new SlidingDateRangeType[] 
                { 
                    SlidingDateRangeType.Current, 
                    SlidingDateRangeType.Previous,
                    SlidingDateRangeType.Last,
                    SlidingDateRangeType.Next,
                    SlidingDateRangeType.Upcoming,
                    SlidingDateRangeType.DateRange
                };

            foreach ( var slidingType in sortedTypes )
            {
                if ( this.EnabledSlidingDateRangeTypes.Contains( slidingType ) )
                {
                    _ddlLastCurrent.Items.Add( new ListItem( slidingType.ConvertToString(), slidingType.ConvertToInt().ToString() ) );
                }
            }

            _ddlTimeUnitTypeSingular.Items.Clear();
            _ddlTimeUnitTypePlural.Items.Clear();

            foreach ( var item in this.EnabledSlidingDateRangeUnits )
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
            /// The last X days,weeks,months, etc (inclusive of current day,week,month,...) but cuts off so it doesn't include future dates
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
            Previous = 4,

            /// <summary>
            /// The next X days,weeks,months, etc (inclusive of current day,week,month,...), but cuts off so it doesn't include past dates
            /// </summary>
            Next = 8,

            /// <summary>
            /// The upcoming X days,weeks,months, etc (excludes current day,week,month,...)
            /// </summary>
            Upcoming = 16
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
        /// Where to put the HTML element of the daterange preview
        /// </summary>
        public enum DateRangePreviewLocation
        {
            /// <summary>
            /// Top
            /// </summary>
            Top,

            /// <summary>
            /// Right
            /// </summary>
            Right,

            /// <summary>
            /// Hide the preview
            /// </summary>
            None
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( "class", "js-slidingdaterange-container " + this.CssClass );
                writer.RenderBeginTag( "div" );

                RockControlHelper.RenderControl( this, writer, "slidingdaterange" );

                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            // set display on render (vs waiting for the javascript to do it after the page is loaded)
            bool isLast = _ddlLastCurrent.SelectedValue == "0";
            bool isCurrent = _ddlLastCurrent.SelectedValue == "1";
            bool isDateRange = _ddlLastCurrent.SelectedValue == "2";
            bool isPrevious = _ddlLastCurrent.SelectedValue == "4";
            _nbNumber.Style[HtmlTextWriterStyle.Display] = ( isLast || isPrevious ) ? "block" : "none";
            _ddlTimeUnitTypeSingular.Style[HtmlTextWriterStyle.Display] = ( isCurrent ) ? "block" : "none";
            _ddlTimeUnitTypePlural.Style[HtmlTextWriterStyle.Display] = ( isLast || isPrevious ) ? "block" : "none";
            _drpDateRange.Style[HtmlTextWriterStyle.Display] = ( isDateRange ) ? "block" : "none";

            bool needsAutoPostBack = SelectedDateRangeChanged != null;
            _ddlLastCurrent.AutoPostBack = needsAutoPostBack;
            _ddlTimeUnitTypeSingular.AutoPostBack = needsAutoPostBack;
            _ddlTimeUnitTypePlural.AutoPostBack = needsAutoPostBack;

            // render a div that will get its text from ~api/Utility/CalculateSlidingDateRange (see slidingDateRangePicker.js)
            Panel dateRangePreviewDiv = new Panel();
            dateRangePreviewDiv.CssClass = "label label-info js-slidingdaterange-info slidingdaterange-info";

            if ( this.PreviewLocation == SlidingDateRangePicker.DateRangePreviewLocation.Top )
            {
                writer.WriteLine();
                dateRangePreviewDiv.RenderControl( writer );
            }

            // render a hidden element that will get its text from ~api/Utility/GetSlidingDateRangeTextValue (see slidingDateRangePicker.js)
            writer.AddAttribute( "type", "hidden" );
            writer.AddAttribute( "class", "js-slidingdaterange-text-value" );
            writer.RenderBeginTag( HtmlTextWriterTag.Input );
            writer.RenderEndTag();

            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _ddlLastCurrent.RenderControl( writer );
            _nbNumber.RenderControl( writer );
            _ddlTimeUnitTypeSingular.RenderControl( writer );
            _ddlTimeUnitTypePlural.RenderControl( writer );
            _drpDateRange.RenderControl( writer );

            if ( this.PreviewLocation == SlidingDateRangePicker.DateRangePreviewLocation.Right )
            {
                writer.WriteLine();
                dateRangePreviewDiv.RenderControl( writer );
            }

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
        /// Gets or sets the enabled sliding date range types.
        /// </summary>
        /// <value>
        /// The enabled sliding date range types.
        /// </value>
        [TypeConverter( typeof( SlidingDateRangeTypeArrayConverter ) )]
        public SlidingDateRangeType[] EnabledSlidingDateRangeTypes
        {
            get
            {
                var result = ViewState["EnabledSlidingDateRangeTypes"] as SlidingDateRangeType[];
                if ( result == null || result.Length == 0 )
                {
                    result = Enum.GetValues( typeof( SlidingDateRangeType ) ).Cast<SlidingDateRangeType>().ToArray();
                }

                return result;
            }

            set
            {
                ViewState["EnabledSlidingDateRangeTypes"] = value;
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
                if ( ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming ).HasFlag( this.SlidingDateRangeMode ) )
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
        /// Gets or sets the enabled sliding date range types.
        /// </summary>
        /// <value>
        /// The enabled sliding date range types.
        /// </value>
        [TypeConverter( typeof( SlidingDateRangeUnitArrayConverter ) )]
        public TimeUnitType[] EnabledSlidingDateRangeUnits
        {
            get
            {
                var result = ViewState["EnabledSlidingDateRangeUnits"] as TimeUnitType[];
                if ( result == null || result.Length == 0 )
                {
                    result = Enum.GetValues( typeof( TimeUnitType ) ).Cast<TimeUnitType>().ToArray();
                }

                return result;
            }

            set
            {
                ViewState["EnabledSlidingDateRangeUnits"] = value;
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
                    ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming ).HasFlag( this.SlidingDateRangeMode ) ? this.NumberOfTimeUnits : (int?)null,
                    ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming | SlidingDateRangeType.Current ).HasFlag( this.SlidingDateRangeMode ) ? this.TimeUnit : (TimeUnitType?)null,
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
        /// Formats the delimited values as a phrase such as "Last 14 Days"
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatDelimitedValues( string value )
        {
            string[] splitValues = ( value ?? string.Empty ).Split( '|' );
            string result = string.Empty;
            if ( splitValues.Length == 5 )
            {
                var slidingDateRangeMode = splitValues[0].ConvertToEnum<SlidingDateRangeType>();
                var numberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                var timeUnitType = splitValues[2].ConvertToEnumOrNull<TimeUnitType>();
                string timeUnitText = timeUnitType != null ? timeUnitType.ConvertToString().PluralizeIf( numberOfTimeUnits != 1 ) : null;
                var start = splitValues[3].AsDateTime();
                var end = splitValues[4].AsDateTime();
                if ( slidingDateRangeMode == SlidingDateRangeType.Current )
                {
                    return string.Format( "{0} {1}", slidingDateRangeMode.ConvertToString(), timeUnitText );
                }
                else if ( ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming ).HasFlag( slidingDateRangeMode ) )
                {
                    return string.Format( "{0} {1} {2}", slidingDateRangeMode.ConvertToString(), numberOfTimeUnits, timeUnitText );
                }
                else
                {
                    // DateRange
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( value );
                    return dateRange.ToStringAutomatic();
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the date range from delimited values in format SlidingDateRangeType|Number|TimeUnitType|StartDate|EndDate
        /// NOTE: The Displayed End Date is one day before the actual end date. 
        /// So, if your date range is displayed as 1/3/2015 to 1/4/2015, this will return 1/5/2015 12:00 AM as the End Date
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

                    // if we are getting "Last" round up to include the current day/week/month/year
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

                    // don't let Last,Previous have any future dates
                    var cutoffDate = RockDateTime.Now.Date.AddDays( 1 );
                    if ( result.End.Value.Date > cutoffDate )
                    {
                        result.End = cutoffDate;
                    }
                }
                else if ( ( SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming ).HasFlag( slidingDateRangeMode ) )
                {
                    // Next X Days,Hours,etc
                    int addCount = numberOfTimeUnits;

                    // if we are getting "Upcoming", round up to exclude the current day/week/month/year
                    int roundUpCount = slidingDateRangeMode == SlidingDateRangeType.Upcoming ? 1 : 0;

                    switch ( timeUnit )
                    {
                        case TimeUnitType.Hour:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 ).AddHours( roundUpCount );
                            result.End = result.Start.Value.AddHours( addCount );
                            break;

                        case TimeUnitType.Day:
                            result.Start = currentDateTime.Date.AddDays( roundUpCount );
                            result.End = result.Start.Value.AddDays( addCount );
                            break;

                        case TimeUnitType.Week:
                            // from http://stackoverflow.com/a/38064/1755417
                            int diff = currentDateTime.DayOfWeek - RockDateTime.FirstDayOfWeek;
                            if ( diff < 0 )
                            {
                                diff += 7;
                            }

                            result.Start = currentDateTime.AddDays( -1 * diff ).Date.AddDays( 7 * roundUpCount );
                            result.End = result.Start.Value.AddDays( addCount * 7 );
                            break;

                        case TimeUnitType.Month:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 ).AddMonths( roundUpCount );
                            result.End = result.Start.Value.AddMonths( addCount );
                            break;

                        case TimeUnitType.Year:
                            result.Start = new DateTime( currentDateTime.Year, 1, 1 ).AddYears( roundUpCount );
                            result.End = result.Start.Value.AddYears( addCount );
                            break;
                    }

                    // don't let Next,Upcoming have any past dates
                    if ( result.Start.Value.Date < RockDateTime.Now.Date )
                    {
                        result.Start = RockDateTime.Now.Date;
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

                // If time unit is days, weeks, months or years subtract a second from time so that end time is with same period
                if ( result.End.HasValue && timeUnit != TimeUnitType.Hour )
                {
                    result.End = result.End.Value.AddSeconds( -1 );
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
            SlidingDateRangeType[] slidingDateRangeTypesForHelp = new SlidingDateRangeType[] { SlidingDateRangeType.Current, SlidingDateRangeType.Previous, SlidingDateRangeType.Last, SlidingDateRangeType.Next, SlidingDateRangeType.Upcoming };

            string helpHtml = @"
    
    <div class='slidingdaterange-help'>

        <p>A date range can either be a specific date range, or a sliding date range based on the current date and time.</p>
        <p>For a sliding date range, you can choose either <strong>current, previous, last, next, upcoming</strong> with a time period of <strong>hour, day, week, month, or year</strong>. Note that a week is Monday thru Sunday.</p>
        <br />
        <ul class=''>
            <li><strong>Current</strong> - the time period that the current date/time is in</li>
            <li><strong>Previous</strong> - the time period(s) prior to the current period (does not include the current time period). For example, to see the most recent weekend, select 'Previous 1 Week'</li>
            <li><strong>Last</strong> - the last X time period(s) including the current until today. For example, to see so far this current week and prior week, select 'Last 2 weeks'</li>
            <li><strong>Next</strong> - the next X time period(s) including the rest of the current period. For example, to see the rest of the current month and the next full month after, select 'Next 2 months'</li>
            <li><strong>Upcoming</strong> - the upcoming X time period(s) not including the current time period.</li>
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

    /// <summary>
    /// 
    /// </summary>
    public class SlidingDateRangeTypeArrayConverter : ArrayConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
        {
            return sourceType == typeof( string );
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that represents the converted value.
        /// </returns>
        public override object ConvertFrom( ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value )
        {
            string[] values = ( value as string ).SplitDelimitedValues();
            return values.Select( a => a.ConvertToEnumOrNull<SlidingDateRangePicker.SlidingDateRangeType>() ).Where( a => a.HasValue ).Select( a => a.Value ).ToArray();
        }

        /// <summary>
        /// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <returns>
        /// true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a common set of values the object supports; otherwise, false.
        /// </returns>
        public override bool GetStandardValuesSupported( ITypeDescriptorContext context )
        {
            return true;
        }

        /// <summary>
        /// Returns whether the collection of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exclusive list of possible values, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <returns>
        /// true if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exhaustive list of possible values; false if other values are possible.
        /// </returns>
        public override bool GetStandardValuesExclusive( ITypeDescriptorContext context )
        {
            return false;
        }

        /// <summary>
        /// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null.</param>
        /// <returns>
        /// A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of valid values, or null if the data type does not support a standard set of values.
        /// </returns>
        public override StandardValuesCollection GetStandardValues( ITypeDescriptorContext context )
        {
            var allTypes = Enum.GetValues( typeof( SlidingDateRangePicker.SlidingDateRangeType ) ).Cast<SlidingDateRangePicker.SlidingDateRangeType>();
            var optionsList = new List<string>();
            optionsList.Add( "Previous, Last, Current, Next, Upcoming, DateRange" );
            optionsList.Add( "Previous, Last, Current, DateRange" );
            optionsList.Add( "Current, Next, Upcoming, DateRange" );
            var result = new StandardValuesCollection( optionsList );
            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SlidingDateRangeUnitArrayConverter : ArrayConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
        {
            return sourceType == typeof( string );
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that represents the converted value.
        /// </returns>
        public override object ConvertFrom( ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value )
        {
            string[] values = ( value as string ).SplitDelimitedValues();
            return values.Select( a => a.ConvertToEnumOrNull<SlidingDateRangePicker.TimeUnitType>() ).Where( a => a.HasValue ).Select( a => a.Value ).ToArray();
        }

        /// <summary>
        /// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <returns>
        /// true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a common set of values the object supports; otherwise, false.
        /// </returns>
        public override bool GetStandardValuesSupported( ITypeDescriptorContext context )
        {
            return true;
        }

        /// <summary>
        /// Returns whether the collection of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exclusive list of possible values, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <returns>
        /// true if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exhaustive list of possible values; false if other values are possible.
        /// </returns>
        public override bool GetStandardValuesExclusive( ITypeDescriptorContext context )
        {
            return false;
        }

        /// <summary>
        /// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null.</param>
        /// <returns>
        /// A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of valid values, or null if the data type does not support a standard set of values.
        /// </returns>
        public override StandardValuesCollection GetStandardValues( ITypeDescriptorContext context )
        {
            var allTypes = Enum.GetValues( typeof( SlidingDateRangePicker.TimeUnitType ) ).Cast<SlidingDateRangePicker.TimeUnitType>();
            var optionsList = new List<string>();
            optionsList.Add( "Hour, Day, Week, Month, Year" );
            optionsList.Add( "Week, Month, Year" );
            var result = new StandardValuesCollection( optionsList );
            return result;
        }

    }

}