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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ScheduleBuilder : CompositeControl, IRockControl
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
        /// Gets or sets the CSS Icon text.
        /// </summary>
        /// <value>
        /// The CSS icon class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string IconCssClass
        {
            get { return ViewState["IconCssClass"] as string ?? string.Empty; }
            set { ViewState["IconCssClass"] = value; }
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

        private Panel _scheduleBuilderPanel;
        private LinkButton _btnShowPopup;
        private ModalDialog _modalDialog;
        private ScheduleBuilderPopupContents _scheduleBuilderPopupContents;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleBuilder"/> class.
        /// </summary>
        public ScheduleBuilder()
        {
            RequiredFieldValidator = null;
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RegisterJavaScript();

            var sm = ScriptManager.GetCurrent( this.Page );
            EnsureChildControls();

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnShowPopup );
            }
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            EnsureChildControls();
            var script = string.Format( @"Rock.controls.scheduleBuilder.initialize({{ id: '{0}' }});", this._scheduleBuilderPopupContents.ClientID );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "schedule_builder-init_" + this._scheduleBuilderPopupContents.ClientID, script, true );
        }

        /// <summary>
        /// Occurs when [save schedule].
        /// </summary>
        public event EventHandler SaveSchedule;

        /// <summary>
        /// Gets or sets the content of the attribute calendar.
        /// </summary>
        /// <value>
        /// The content of the attribute calendar.
        /// </value>
        public string iCalendarContent
        {
            get
            {
                EnsureChildControls();
                return _scheduleBuilderPopupContents.iCalendarContent;
            }

            set
            {
                EnsureChildControls();
                _scheduleBuilderPopupContents.iCalendarContent = value;

                if ( ShowScheduleFriendlyTextAsToolTip )
                {
                    this.ToolTip = new Rock.Model.Schedule { iCalendarContent = _scheduleBuilderPopupContents.iCalendarContent }.ToFriendlyScheduleText();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text displayed when the mouse pointer hovers over the Web server control.
        /// </summary>
        /// <returns>The text displayed when the mouse pointer hovers over the Web server control. The default is <see cref="F:System.String.Empty" />.</returns>
        public override string ToolTip
        {
            get
            {
                EnsureChildControls();
                return _btnShowPopup.ToolTip;
            }
            set
            {
                EnsureChildControls();
                _btnShowPopup.ToolTip = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether prompt for duration in the Schedule Builder
        /// defaults to true
        /// Set to false if the schedule only requires start datetime(s)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show duration]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDuration
        {
            get
            {
                EnsureChildControls();
                return _scheduleBuilderPopupContents.ShowDuration;
            }

            set
            {
                EnsureChildControls();
                _scheduleBuilderPopupContents.ShowDuration = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show schedule friendly text as tool tip].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show schedule friendly text as tool tip]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowScheduleFriendlyTextAsToolTip
        {
            get
            {
                return ViewState["ShowScheduleFriendlyTextAsToolTip"] as bool? ?? false;
            }

            set
            {
                ViewState["ShowScheduleFriendlyTextAsToolTip"] = value;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.Controls.Clear();

            _scheduleBuilderPanel = new Panel();
            _scheduleBuilderPanel.ID = "scheduleBuilderPanel_" + this.ClientID;
            _scheduleBuilderPanel.CssClass = "picker";
            _scheduleBuilderPanel.ClientIDMode = ClientIDMode.Static;

            _btnShowPopup = new LinkButton();
            _btnShowPopup.CausesValidation = false;
            _btnShowPopup.ID = "btnShowPopup_" + this.ClientID;
            _btnShowPopup.CssClass = "picker-label";
            _btnShowPopup.Text = "<i class='fa fa-calendar'></i> Edit Schedule";
            _btnShowPopup.ClientIDMode = ClientIDMode.Static;
            _btnShowPopup.Click += _btnShowPopup_Click;

            _modalDialog = new ModalDialog();
            _modalDialog.ID = "modalDialog_" + this.ClientID;
            _modalDialog.ValidationGroup = _modalDialog.ID + "_validationgroup";
            _modalDialog.ClientIDMode = ClientIDMode.Static;
            _modalDialog.Title = "Schedule Builder";
            _modalDialog.SaveButtonText = "OK";
            _modalDialog.SaveClick += btnSaveSchedule_Click;

            _scheduleBuilderPopupContents = new ScheduleBuilderPopupContents();
            _scheduleBuilderPopupContents.ID = "scheduleBuilderPopupContents_" + this.ClientID;
            _scheduleBuilderPopupContents.ValidationGroup = _modalDialog.ValidationGroup;
            _scheduleBuilderPopupContents.ClientIDMode = ClientIDMode.Static;

            this.Controls.Add( _scheduleBuilderPanel );
            _scheduleBuilderPanel.Controls.Add( _btnShowPopup );
            _scheduleBuilderPanel.Controls.Add( _modalDialog );
            _modalDialog.Content.Controls.Add( _scheduleBuilderPopupContents );

            RockControlHelper.CreateChildControls( this, Controls );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// This is where you implment the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            _scheduleBuilderPanel.RenderControl( writer );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveSchedule_Click( object sender, EventArgs e )
        {
            iCalendarContent = _scheduleBuilderPopupContents.GetCalendarContentFromControls();
            _modalDialog.Hide();
            if ( SaveSchedule != null )
            {
                SaveSchedule( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the _btnShowPopup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _btnShowPopup_Click( object sender, EventArgs e )
        {
            _modalDialog.Show();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ScheduleBuilderPopupContents : CompositeControl
    {
        private DateTimePicker _dpStartDateTime;
        private NumberBox _tbDurationHours;
        private NumberBox _tbDurationMinutes;

        private RockRadioButton _radOneTime;
        private RockRadioButton _radRecurring;

        private RockRadioButton _radSpecificDates;
        private RockRadioButton _radDaily;
        private RockRadioButton _radWeekly;
        private RockRadioButton _radMonthly;

        private HiddenFieldWithClass _hfSpecificDateListValues;

        // specific date panel
        private DatePicker _dpSpecificDate;

        // daily recurrence
        private RockRadioButton _radDailyEveryXDays;
        private NumberBox _tbDailyEveryXDays;
        private RockRadioButton _radDailyEveryWeekday;
        private RockRadioButton _radDailyEveryWeekendDay;

        // weekly recurrence
        private NumberBox _tbWeeklyEveryX;
        private RockCheckBox _cbWeeklySunday;
        private RockCheckBox _cbWeeklyMonday;
        private RockCheckBox _cbWeeklyTuesday;
        private RockCheckBox _cbWeeklyWednesday;
        private RockCheckBox _cbWeeklyThursday;
        private RockCheckBox _cbWeeklyFriday;
        private RockCheckBox _cbWeeklySaturday;

        // monthly
        private RockRadioButton _radMonthlyDayX;
        private NumberBox _tbMonthlyDayX;
        private NumberBox _tbMonthlyXMonths;
        private RockRadioButton _radMonthlyNth;
        private RockDropDownList _ddlMonthlyNth;
        private RockDropDownList _ddlMonthlyDayName;

        // end date
        private RockRadioButton _radEndByNone;
        private RockRadioButton _radEndByDate;
        private DatePicker _dpEndBy;
        private RockRadioButton _radEndByOccurrenceCount;
        private NumberBox _tbEndByOccurrenceCount;

        // exclusions
        private HiddenFieldWithClass _hfExclusionDateRangeListValues;
        private DateRangePicker _dpExclusionDateRange;


        private const string iCalendarContentEmptyEvent = @"
BEGIN:VCALENDAR
VERSION:2.0
BEGIN:VEVENT
END:VEVENT
END:VCALENDAR
";

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleBuilder"/> class.
        /// </summary>
        public ScheduleBuilderPopupContents()
        {
            // common
            _dpStartDateTime = new DateTimePicker();

            _tbDurationHours = new NumberBox();
            _tbDurationMinutes = new NumberBox();

            _radOneTime = new RockRadioButton();
            _radRecurring = new RockRadioButton();

            _radSpecificDates = new RockRadioButton();
            _radDaily = new RockRadioButton();
            _radWeekly = new RockRadioButton();
            _radMonthly = new RockRadioButton();

            // specific date
            _hfSpecificDateListValues = new HiddenFieldWithClass();
            _hfSpecificDateListValues.CssClass = "js-specific-datelist-values";

            _dpSpecificDate = new DatePicker();

            // daily
            _radDailyEveryXDays = new RockRadioButton();
            _tbDailyEveryXDays = new NumberBox();
            _radDailyEveryWeekday = new RockRadioButton();
            _radDailyEveryWeekendDay = new RockRadioButton();

            // weekly
            _tbWeeklyEveryX = new NumberBox();
            _cbWeeklySunday = new RockCheckBox();
            _cbWeeklyMonday = new RockCheckBox();
            _cbWeeklyTuesday = new RockCheckBox();
            _cbWeeklyWednesday = new RockCheckBox();
            _cbWeeklyThursday = new RockCheckBox();
            _cbWeeklyFriday = new RockCheckBox();
            _cbWeeklySaturday = new RockCheckBox();

            // monthly
            _radMonthlyDayX = new RockRadioButton();
            _tbMonthlyDayX = new NumberBox();
            _tbMonthlyXMonths = new NumberBox();
            _radMonthlyNth = new RockRadioButton();
            _ddlMonthlyNth = new RockDropDownList();
            _ddlMonthlyDayName = new RockDropDownList();

            // end date
            _radEndByNone = new RockRadioButton();
            _radEndByDate = new RockRadioButton();
            _dpEndBy = new DatePicker();
            _radEndByOccurrenceCount = new RockRadioButton();
            _tbEndByOccurrenceCount = new NumberBox();

            // exclusions
            _hfExclusionDateRangeListValues = new HiddenFieldWithClass();
            _hfExclusionDateRangeListValues.CssClass = "js-exclusion-daterange-list-values";
            _dpExclusionDateRange = new DateRangePicker();
        }

        /// <summary>
        /// The _i calendar content
        /// </summary>
        private string _iCalendarContent
        {
            get
            {
                return ViewState["_iCalendarContent"] as string;
            }

            set
            {
                ViewState["_iCalendarContent"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [_show duration].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [_show duration]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDuration
        {
            get
            {
                return ViewState["ShowDuration"] as bool? ?? true;
            }

            set
            {
                ViewState["ShowDuration"] = value;
            }
        }

        /// <summary>
        /// Texts the box to positive integer.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="minValue">The min value.</param>
        /// <returns></returns>
        private int TextBoxToPositiveInteger( TextBox textBox, int minValue = 1 )
        {
            int result = textBox.Text.AsIntegerOrNull() ?? minValue;
            if ( result < minValue )
            {
                result = minValue;
            }

            textBox.Text = result.ToString();
            return result;
        }

        /// <summary>
        /// Gets the calendar content from controls.
        /// </summary>
        /// <returns></returns>
        internal string GetCalendarContentFromControls()
        {
            EnsureChildControls();

            if ( _dpStartDateTime.SelectedDateTimeIsBlank )
            {
                return iCalendarContentEmptyEvent;
            }

            DDay.iCal.Event calendarEvent = new DDay.iCal.Event();
            calendarEvent.DTStart = new DDay.iCal.iCalDateTime( _dpStartDateTime.SelectedDateTime.Value );
            calendarEvent.DTStart.HasTime = true;

            int durationHours = TextBoxToPositiveInteger( _tbDurationHours, 0 );
            int durationMins = TextBoxToPositiveInteger( _tbDurationMinutes, 0 );

            if ( ( durationHours == 0 && durationMins == 0 ) || this.ShowDuration == false )
            {
                // make a one second duration since a zero duration won't be included in occurrences
                calendarEvent.Duration = new TimeSpan( 0, 0, 1 );
            }
            else
            {
                calendarEvent.Duration = new TimeSpan( durationHours, durationMins, 0 );
            }


            if ( _radRecurring.Checked )
            {
                if ( _radSpecificDates.Checked )
                {
                    #region specific dates
                    PeriodList recurrenceDates = new PeriodList();
                    List<string> dateStringList = _hfSpecificDateListValues.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    foreach ( var dateString in dateStringList )
                    {
                        DateTime newDate;
                        if ( DateTime.TryParse( dateString, out newDate ) )
                        {
                            recurrenceDates.Add( new iCalDateTime( newDate.Date ) );
                        }
                    }

                    calendarEvent.RecurrenceDates.Add( recurrenceDates );
                    #endregion
                }
                else
                {
                    if ( _radDaily.Checked )
                    {
                        #region daily
                        if ( _radDailyEveryXDays.Checked )
                        {
                            RecurrencePattern rruleDaily = new RecurrencePattern( FrequencyType.Daily );

                            rruleDaily.Interval = TextBoxToPositiveInteger( _tbDailyEveryXDays );
                            calendarEvent.RecurrenceRules.Add( rruleDaily );
                        }
                        else
                        {
                            // NOTE:  Daily Every Weekday/Weekend Day is actually Weekly on Day(s)OfWeek in iCal
                            RecurrencePattern rruleWeekly = new RecurrencePattern( FrequencyType.Weekly );
                            if ( _radDailyEveryWeekday.Checked )
                            {
                                rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Monday ) );
                                rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Tuesday ) );
                                rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Wednesday ) );
                                rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Thursday ) );
                                rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Friday ) );
                            }
                            else if ( _radDailyEveryWeekendDay.Checked )
                            {
                                rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Saturday ) );
                                rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Sunday ) );
                            }

                            calendarEvent.RecurrenceRules.Add( rruleWeekly );
                        }
                        #endregion
                    }
                    else if ( _radWeekly.Checked )
                    {
                        #region weekly
                        RecurrencePattern rruleWeekly = new RecurrencePattern( FrequencyType.Weekly );
                        rruleWeekly.Interval = TextBoxToPositiveInteger( _tbWeeklyEveryX );

                        if ( _cbWeeklySunday.Checked )
                        {
                            rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Sunday ) );
                        }

                        if ( _cbWeeklyMonday.Checked )
                        {
                            rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Monday ) );
                        }

                        if ( _cbWeeklyTuesday.Checked )
                        {
                            rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Tuesday ) );
                        }

                        if ( _cbWeeklyWednesday.Checked )
                        {
                            rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Wednesday ) );
                        }

                        if ( _cbWeeklyThursday.Checked )
                        {
                            rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Thursday ) );
                        }

                        if ( _cbWeeklyFriday.Checked )
                        {
                            rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Friday ) );
                        }

                        if ( _cbWeeklySaturday.Checked )
                        {
                            rruleWeekly.ByDay.Add( new WeekDay( DayOfWeek.Saturday ) );
                        }

                        calendarEvent.RecurrenceRules.Add( rruleWeekly );
                        #endregion
                    }
                    else if ( _radMonthly.Checked )
                    {
                        #region monthly
                        RecurrencePattern rruleMonthly = new RecurrencePattern( FrequencyType.Monthly );
                        if ( _radMonthlyDayX.Checked )
                        {
                            rruleMonthly.ByMonthDay.Add( TextBoxToPositiveInteger( _tbMonthlyDayX ) );
                            rruleMonthly.Interval = TextBoxToPositiveInteger( _tbMonthlyXMonths );
                        }
                        else if ( _radMonthlyNth.Checked )
                        {
                            WeekDay monthWeekDay = new WeekDay();
                            monthWeekDay.Offset = _ddlMonthlyNth.SelectedValue.AsIntegerOrNull() ?? 1;
                            monthWeekDay.DayOfWeek = (DayOfWeek)( _ddlMonthlyDayName.SelectedValue.AsIntegerOrNull() ?? 1 );
                            rruleMonthly.ByDay.Add( monthWeekDay );
                        }

                        calendarEvent.RecurrenceRules.Add( rruleMonthly );
                        #endregion
                    }
                }
            }

            if ( calendarEvent.RecurrenceRules.Count > 0 )
            {
                IRecurrencePattern rrule = calendarEvent.RecurrenceRules[0];

                // Continue Until
                if ( _radEndByNone.Checked )
                {
                    // intentionally blank
                }
                else if ( _radEndByDate.Checked )
                {
                    rrule.Until = _dpEndBy.SelectedDate.HasValue ? _dpEndBy.SelectedDate.Value : DateTime.MaxValue;
                }
                else if ( _radEndByOccurrenceCount.Checked )
                {
                    rrule.Count = TextBoxToPositiveInteger( _tbEndByOccurrenceCount, 0 );
                }
            }

            // Exclusions
            List<string> dateRangeStringList = _hfExclusionDateRangeListValues.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            PeriodList exceptionDates = new PeriodList();
            foreach ( string dateRangeString in dateRangeStringList )
            {
                var dateRangeParts = dateRangeString.Split( new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries );
                if ( dateRangeParts.Count() == 2 )
                {
                    DateTime beginDate;
                    DateTime endDate;

                    if ( DateTime.TryParse( dateRangeParts[0], out beginDate ) )
                    {
                        if ( DateTime.TryParse( dateRangeParts[1], out endDate ) )
                        {
                            DateTime dateToAdd = beginDate.Date;
                            while ( dateToAdd <= endDate )
                            {
                                Period periodToAdd = new Period( new iCalDateTime( dateToAdd ) );
                                if ( !exceptionDates.Contains( periodToAdd ) )
                                {
                                    exceptionDates.Add( periodToAdd );
                                }

                                dateToAdd = dateToAdd.AddDays( 1 );
                            }
                        }
                    }
                }
            }

            if ( exceptionDates.Count > 0 )
            {
                calendarEvent.ExceptionDates.Add( exceptionDates );
            }

            DDay.iCal.iCalendar calendar = new iCalendar();
            calendar.Events.Add( calendarEvent );

            iCalendarSerializer s = new iCalendarSerializer( calendar );

            return s.SerializeToString( calendar );
        }

        /// <summary>
        /// Gets or sets the content of the i calendar.
        /// </summary>
        /// <value>
        /// The content of the i calendar. 
        /// </value>
        public string iCalendarContent
        {
            get
            {
                return _iCalendarContent;
            }

            set
            {
                EnsureChildControls();

                //// iCal is stored as a list of Calendar's each with a list of Events, etc.  
                //// We just need one Calendar and one Event

                // set all rad to false to prevent multiple from being true
                foreach ( var radControl in this.Controls.OfType<RockRadioButton>().ToList() )
                {
                    radControl.Checked = false;
                }

                foreach ( var cbControl in this.Controls.OfType<CheckBox>().ToList() )
                {
                    cbControl.Checked = false;
                }

                StringReader stringReader = new StringReader( value ?? iCalendarContentEmptyEvent );
                var calendarList = DDay.iCal.iCalendar.LoadFromStream( stringReader );
                DDay.iCal.Event calendarEvent = null;
                DDay.iCal.iCalendar calendar = null;
                if ( calendarList.Count > 0 )
                {
                    calendar = calendarList[0] as DDay.iCal.iCalendar;
                    if ( calendar == null )
                    {
                        _radOneTime.Checked = true;
                        _iCalendarContent = iCalendarContentEmptyEvent;
                        return;
                    }
                }
                else
                {
                    _radOneTime.Checked = true;
                    _iCalendarContent = iCalendarContentEmptyEvent;
                    return;
                }

                calendarEvent = calendar.Events[0] as DDay.iCal.Event;

                if ( calendarEvent.DTStart != null )
                {
                    _dpStartDateTime.SelectedDateTime = calendarEvent.DTStart.Value;
                    int hours = ( calendarEvent.Duration.Days * 24 ) + calendarEvent.Duration.Hours;
                    _tbDurationHours.Text = hours.ToString();
                    _tbDurationMinutes.Text = calendarEvent.Duration.Minutes.ToString();
                }
                else
                {
                    _dpStartDateTime.SelectedDateTime = null;
                    _tbDurationHours.Text = string.Empty;
                    _tbDurationMinutes.Text = string.Empty;
                }

                if ( calendarEvent.RecurrenceRules.Count == 0 )
                {
                    // One-Time
                    _radRecurring.Checked = false;
                    _radOneTime.Checked = true;
                }
                else
                {
                    // Recurring
                    _radRecurring.Checked = true;
                    _radOneTime.Checked = false;
                }

                if ( _radRecurring.Checked )
                {
                    IRecurrencePattern rrule = calendarEvent.RecurrenceRules[0];
                    switch ( rrule.Frequency )
                    {
                        case FrequencyType.Daily:
                            #region daily

                            _radDaily.Checked = true;
                            _radDailyEveryXDays.Checked = true;
                            _tbDailyEveryXDays.Text = rrule.Interval.ToString();

                            // NOTE:  Daily Every Weekday/Weekend Day is actually Weekly on Day(s)OfWeek in iCal
                            break;

                            #endregion
                        case FrequencyType.Weekly:
                            #region weekly

                            _radWeekly.Checked = true;
                            _tbWeeklyEveryX.Text = rrule.Interval.ToString();

                            foreach ( DayOfWeek dow in rrule.ByDay.Select( a => a.DayOfWeek ) )
                            {
                                switch ( dow )
                                {
                                    case DayOfWeek.Sunday:
                                        _cbWeeklySunday.Checked = true;
                                        break;
                                    case DayOfWeek.Monday:
                                        _cbWeeklyMonday.Checked = true;
                                        break;
                                    case DayOfWeek.Tuesday:
                                        _cbWeeklyTuesday.Checked = true;
                                        break;
                                    case DayOfWeek.Wednesday:
                                        _cbWeeklyWednesday.Checked = true;
                                        break;
                                    case DayOfWeek.Thursday:
                                        _cbWeeklyThursday.Checked = true;
                                        break;
                                    case DayOfWeek.Friday:
                                        _cbWeeklyFriday.Checked = true;
                                        break;
                                    case DayOfWeek.Saturday:
                                        _cbWeeklySaturday.Checked = true;
                                        break;
                                }
                            }

                            break;

                            #endregion
                        case FrequencyType.Monthly:
                            #region monthly

                            _radMonthly.Checked = true;
                            if ( rrule.ByMonthDay.Count > 0 )
                            {
                                // Day X of every X Months
                                _radMonthlyDayX.Checked = true;
                                int monthDay = rrule.ByMonthDay[0];

                                _tbMonthlyDayX.Text = monthDay.ToString();
                                _tbMonthlyXMonths.Text = rrule.Interval.ToString();
                            }
                            else if ( rrule.ByDay.Count > 0 )
                            {
                                // The Nth <DayOfWeekName>
                                _radMonthlyNth.Checked = true;
                                IWeekDay bydate = rrule.ByDay[0];
                                _ddlMonthlyNth.SelectedValue = bydate.Offset.ToString();
                                _ddlMonthlyDayName.SelectedValue = bydate.DayOfWeek.ConvertToInt().ToString();
                            }

                            break;

                            #endregion
                        default:
                            // Unexpected type of recurring, fallback to Specific Dates
                            _radSpecificDates.Checked = true;
                            break;
                    }

                    // Continue Until
                    if ( rrule.Until > DateTime.MinValue )
                    {
                        _radEndByDate.Checked = true;
                        _dpEndBy.SelectedDate = rrule.Until;
                    }
                    else if ( rrule.Count > 0 )
                    {
                        _radEndByOccurrenceCount.Checked = true;
                        _tbEndByOccurrenceCount.Text = rrule.Count.ToString();
                    }
                    else
                    {
                        _radEndByNone.Checked = true;
                    }

                    // Exclusions
                    if ( calendarEvent.ExceptionDates.Count > 0 )
                    {
                        // convert individual ExceptionDates into a list of Date Ranges
                        List<Period> exDateRanges = new List<Period>();
                        List<IDateTime> dates = calendarEvent.ExceptionDates[0].Select( a => a.StartTime ).OrderBy( a => a ).ToList();
                        IDateTime previousDate = dates[0].AddDays( -1 );
                        var dateRange = new Period { StartTime = dates[0] };
                        foreach ( var date in dates )
                        {
                            if ( !date.Equals( previousDate.AddDays( 1 ) ) )
                            {
                                dateRange.EndTime = previousDate;
                                exDateRanges.Add( dateRange );
                                dateRange = new Period { StartTime = date };
                            }

                            previousDate = date;
                        }

                        dateRange.EndTime = dates.Last();
                        exDateRanges.Add( dateRange );

                        _hfExclusionDateRangeListValues.Value = exDateRanges.Select( a => string.Format( "{0} - {1}", a.StartTime, a.EndTime ) ).ToList().AsDelimited( "," );
                    }
                }
                else
                {
                    // Specific Dates is a special case of recurring (RecurrenceDates vs RecurrenceRules)
                    _radSpecificDates.Checked = true;

                    if ( calendarEvent.RecurrenceDates.Count > 0 )
                    {
                        // Specific Dates
                        _radOneTime.Checked = false;
                        _radRecurring.Checked = true;
                        _radEndByNone.Checked = true;
                        _radEndByDate.Checked = false;
                        _radEndByOccurrenceCount.Checked = false;
                        IPeriodList dates = calendarEvent.RecurrenceDates[0];
                        _hfSpecificDateListValues.Value = dates.Select( a => a.StartTime ).ToList().AsDelimited( "," );
                    }

                    _radEndByNone.Checked = true;
                    _hfExclusionDateRangeListValues.Value = string.Empty;
                }

                // rebuild _iCalendarContent from controls in case anything got stripped out from orig value
                _iCalendarContent = GetCalendarContentFromControls();
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup { get; set; }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            string validationGroup = this.ValidationGroup;

            _dpStartDateTime.ClientIDMode = ClientIDMode.Static;
            _dpStartDateTime.ID = "dpStartDateTime_" + this.ClientID;
            _dpStartDateTime.Label = "Start Date / Time";
            _dpStartDateTime.Required = false;
            _dpStartDateTime.ValidationGroup = validationGroup;

            _tbDurationHours.ClientIDMode = ClientIDMode.Static;
            _tbDurationHours.ID = "tbDurationHours_" + this.ClientID;
            _tbDurationHours.CssClass = "input-width-md";
            _tbDurationHours.AppendText = "hrs&nbsp;";
            _tbDurationHours.MinimumValue = "0";
            _tbDurationHours.ValidationGroup = validationGroup;

            _tbDurationMinutes.ClientIDMode = ClientIDMode.Static;
            _tbDurationMinutes.ID = "tbDurationMinutes_" + this.ClientID;
            _tbDurationMinutes.CssClass = "input-width-md";
            _tbDurationMinutes.MinimumValue = "0";
            _tbDurationMinutes.MaximumValue = "59";
            _tbDurationMinutes.AppendText = "mins";
            _tbDurationMinutes.ValidationGroup = validationGroup;

            _radOneTime.ClientIDMode = ClientIDMode.Static;
            _radOneTime.ID = "radOneTime_" + this.ClientID;
            _radOneTime.GroupName = "ScheduleTypeGroup";
            _radOneTime.InputAttributes["class"] = "schedule-type";
            _radOneTime.Text = "One Time";
            _radOneTime.InputAttributes["data-schedule-type"] = "schedule-onetime";

            _radRecurring.ClientIDMode = ClientIDMode.Static;
            _radRecurring.ID = "radRecurring_" + this.ClientID;
            _radRecurring.GroupName = "ScheduleTypeGroup";
            _radRecurring.InputAttributes["class"] = "schedule-type";
            _radRecurring.Text = "Recurring";
            _radRecurring.InputAttributes["data-schedule-type"] = "schedule-Recurring";

            _radSpecificDates.ClientIDMode = ClientIDMode.Static;
            _radSpecificDates.ID = "radSpecificDates_" + this.ClientID;
            _radSpecificDates.GroupName = "recurrence-pattern-radio";
            _radSpecificDates.InputAttributes["class"] = "recurrence-pattern-radio";
            _radSpecificDates.Text = "Specific Dates";
            _radSpecificDates.InputAttributes["data-recurrence-pattern"] = "recurrence-pattern-specific-date";

            _radDaily.ClientIDMode = ClientIDMode.Static;
            _radDaily.ID = "radDaily_" + this.ClientID;
            _radDaily.GroupName = "recurrence-pattern-radio";
            _radDaily.InputAttributes["class"] = "recurrence-pattern-radio";
            _radDaily.Text = "Daily";
            _radDaily.InputAttributes["data-recurrence-pattern"] = "recurrence-pattern-daily";

            _radWeekly.ClientIDMode = ClientIDMode.Static;
            _radWeekly.ID = "radWeekly_" + this.ClientID;
            _radWeekly.GroupName = "recurrence-pattern-radio";
            _radWeekly.InputAttributes["class"] = "recurrence-pattern-radio";
            _radWeekly.Text = "Weekly";
            _radWeekly.InputAttributes["data-recurrence-pattern"] = "recurrence-pattern-weekly";

            _radMonthly.ClientIDMode = ClientIDMode.Static;
            _radMonthly.ID = "radMonthly_" + this.ClientID;
            _radMonthly.GroupName = "recurrence-pattern-radio";
            _radMonthly.InputAttributes["class"] = "recurrence-pattern-radio";
            _radMonthly.Text = "Monthly";
            _radMonthly.InputAttributes["data-recurrence-pattern"] = "recurrence-pattern-monthly";

            _hfSpecificDateListValues.ClientIDMode = ClientIDMode.Static;
            _hfSpecificDateListValues.ID = "hfSpecificDateListValues_" + this.ClientID;

            // specific date
            _dpSpecificDate.ClientIDMode = ClientIDMode.Static;
            _dpSpecificDate.ID = "dpSpecificDate_" + this.ClientID;
            _dpSpecificDate.ValidationGroup = validationGroup;

            // daily recurrence
            _radDailyEveryXDays.ClientIDMode = ClientIDMode.Static;
            _radDailyEveryXDays.ID = "radDailyEveryXDays_" + this.ClientID;
            _radDailyEveryXDays.GroupName = "daily-options";

            _tbDailyEveryXDays.ClientIDMode = ClientIDMode.Static;
            _tbDailyEveryXDays.ID = "tbDailyEveryXDays_" + this.ClientID;
            _tbDailyEveryXDays.CssClass = "input-width-md";
            _tbDailyEveryXDays.AppendText = "days";
            _tbDailyEveryXDays.ValidationGroup = validationGroup;

            _radDailyEveryWeekday.ClientIDMode = ClientIDMode.Static;
            _radDailyEveryWeekday.ID = "radDailyEveryWeekday_" + this.ClientID;
            _radDailyEveryWeekday.GroupName = "daily-options";

            _radDailyEveryWeekendDay.ClientIDMode = ClientIDMode.Static;
            _radDailyEveryWeekendDay.ID = "radDailyEveryWeekendDay_" + this.ClientID;
            _radDailyEveryWeekendDay.GroupName = "daily-options";

            // weekly recurrence
            _tbWeeklyEveryX.ClientIDMode = ClientIDMode.Static;
            _tbWeeklyEveryX.ID = "tbWeeklyEveryX_" + this.ClientID;
            _tbWeeklyEveryX.CssClass = "input-width-md";
            _tbWeeklyEveryX.MinimumValue = "1";
            _tbWeeklyEveryX.MaximumValue = "52";
            _tbWeeklyEveryX.AppendText = "week(s)";
            _tbWeeklyEveryX.ValidationGroup = validationGroup;

            _cbWeeklySunday.ClientIDMode = ClientIDMode.Static;
            _cbWeeklySunday.ID = "cbWeeklySunday_" + this.ClientID;
            _cbWeeklySunday.Text = "Sun";
            _cbWeeklyMonday.ClientIDMode = ClientIDMode.Static;
            _cbWeeklyMonday.ID = "cbWeeklyMonday_" + this.ClientID;
            _cbWeeklyMonday.Text = "Mon";
            _cbWeeklyTuesday.ClientIDMode = ClientIDMode.Static;
            _cbWeeklyTuesday.ID = "cbWeeklyTuesday_" + this.ClientID;
            _cbWeeklyTuesday.Text = "Tue";
            _cbWeeklyWednesday.ClientIDMode = ClientIDMode.Static;
            _cbWeeklyWednesday.ID = "cbWeeklyWednesday_" + this.ClientID;
            _cbWeeklyWednesday.Text = "Wed";
            _cbWeeklyThursday.ClientIDMode = ClientIDMode.Static;
            _cbWeeklyThursday.ID = "cbWeeklyThursday_" + this.ClientID;
            _cbWeeklyThursday.Text = "Thu";
            _cbWeeklyFriday.ClientIDMode = ClientIDMode.Static;
            _cbWeeklyFriday.ID = "cbWeeklyFriday_" + this.ClientID;
            _cbWeeklyFriday.Text = "Fri";
            _cbWeeklySaturday.ClientIDMode = ClientIDMode.Static;
            _cbWeeklySaturday.ID = "cbWeeklySaturday_" + this.ClientID;
            _cbWeeklySaturday.Text = "Sat";

            // monthly
            _radMonthlyDayX.ClientIDMode = ClientIDMode.Static;
            _radMonthlyDayX.ID = "radMonthlyDayX_" + this.ClientID;
            _radMonthlyDayX.GroupName = "monthly-options";

            _tbMonthlyDayX.ClientIDMode = ClientIDMode.Static;
            _tbMonthlyDayX.ID = "tbMonthlyDayX_" + this.ClientID;
            _tbMonthlyDayX.CssClass = "input-width-sm";
            _tbMonthlyDayX.MinimumValue = "1";
            _tbMonthlyDayX.MaximumValue = "31";
            _tbMonthlyDayX.ValidationGroup = validationGroup;

            _tbMonthlyXMonths.ClientIDMode = ClientIDMode.Static;
            _tbMonthlyXMonths.ID = "tbMonthlyXMonths_" + this.ClientID;
            _tbMonthlyXMonths.CssClass = "input-width-sm";
            _tbMonthlyXMonths.MinimumValue = "1";
            _tbMonthlyXMonths.MaximumValue = "12";
            _tbMonthlyXMonths.ValidationGroup = validationGroup;

            _radMonthlyNth.ClientIDMode = ClientIDMode.Static;
            _radMonthlyNth.ID = "radMonthlyNth_" + this.ClientID;
            _radMonthlyNth.GroupName = "monthly-options";

            _ddlMonthlyNth.ClientIDMode = ClientIDMode.Static;
            _ddlMonthlyNth.ID = "ddlMonthlyNth_" + this.ClientID;
            _ddlMonthlyNth.CssClass = "input-width-sm";

            _ddlMonthlyNth.Items.Add( string.Empty );
            foreach ( var nth in Rock.Model.Schedule.NthNames )
            {
                _ddlMonthlyNth.Items.Add( new ListItem( nth.Value, nth.Key.ToString() ) );
            }

            _ddlMonthlyDayName.ClientIDMode = ClientIDMode.Static;
            _ddlMonthlyDayName.ID = "ddlMonthlyDayName_" + this.ClientID;
            _ddlMonthlyDayName.CssClass = "input-width-md";

            DateTimeFormatInfo dateTimeFormatInfo = new DateTimeFormatInfo();
            _ddlMonthlyDayName.Items.Add( string.Empty );
            for ( int dayNum = 0; dayNum < dateTimeFormatInfo.DayNames.Length; dayNum++ )
            {
                _ddlMonthlyDayName.Items.Add( new ListItem( dateTimeFormatInfo.DayNames[dayNum], dayNum.ToString() ) );
            }

            // end date
            _radEndByNone.ClientIDMode = ClientIDMode.Static;
            _radEndByNone.ID = "radEndByNone_" + this.ClientID;
            _radEndByNone.GroupName = "end-by";

            _radEndByDate.ClientIDMode = ClientIDMode.Static;
            _radEndByDate.ID = "radEndByDate_" + this.ClientID;
            _radEndByDate.GroupName = "end-by";

            _dpEndBy.ClientIDMode = ClientIDMode.Static;
            _dpEndBy.ID = "dpEndBy_" + this.ClientID;
            _dpEndBy.ValidationGroup = validationGroup;

            _radEndByOccurrenceCount.ClientIDMode = ClientIDMode.Static;
            _radEndByOccurrenceCount.ID = "radEndByOccurrenceCount_" + this.ClientID;
            _radEndByOccurrenceCount.GroupName = "end-by";

            _tbEndByOccurrenceCount.ClientIDMode = ClientIDMode.Static;
            _tbEndByOccurrenceCount.ID = "tbEndByOccurrenceCount_" + this.ClientID;
            _tbEndByOccurrenceCount.CssClass = "input-width-sm";
            _tbEndByOccurrenceCount.MinimumValue = "1";
            _tbEndByOccurrenceCount.MaximumValue = "999";
            _tbEndByOccurrenceCount.ValidationGroup = validationGroup;

            // exclusions
            _hfExclusionDateRangeListValues.ClientIDMode = ClientIDMode.Static;
            _hfExclusionDateRangeListValues.ID = "hfExclusionDateRangeListValues_" + this.ClientID;

            _dpExclusionDateRange.ClientIDMode = ClientIDMode.Static;
            _dpExclusionDateRange.ID = "dpExclusionDateRange_" + this.ClientID;
            _dpExclusionDateRange.CssClass = "js-exclusion-date-range-picker";
            _dpExclusionDateRange.ValidationGroup = validationGroup;

            Controls.Add( _dpStartDateTime );
            Controls.Add( _tbDurationHours );
            Controls.Add( _tbDurationMinutes );
            Controls.Add( _radOneTime );
            Controls.Add( _radRecurring );

            Controls.Add( _radSpecificDates );
            Controls.Add( _radDaily );
            Controls.Add( _radWeekly );
            Controls.Add( _radMonthly );

            Controls.Add( _hfSpecificDateListValues );
            Controls.Add( _dpSpecificDate );

            // daily recurrence
            Controls.Add( _radDailyEveryXDays );
            Controls.Add( _tbDailyEveryXDays );
            Controls.Add( _radDailyEveryWeekday );
            Controls.Add( _radDailyEveryWeekendDay );

            // weekly recurrence
            Controls.Add( _tbWeeklyEveryX );
            Controls.Add( _cbWeeklySunday );
            Controls.Add( _cbWeeklyMonday );
            Controls.Add( _cbWeeklyTuesday );
            Controls.Add( _cbWeeklyWednesday );
            Controls.Add( _cbWeeklyThursday );
            Controls.Add( _cbWeeklyFriday );
            Controls.Add( _cbWeeklySaturday );

            // monthly
            Controls.Add( _radMonthlyDayX );
            Controls.Add( _tbMonthlyDayX );
            Controls.Add( _tbMonthlyXMonths );
            Controls.Add( _radMonthlyNth );
            Controls.Add( _ddlMonthlyNth );
            Controls.Add( _ddlMonthlyDayName );

            // end date
            Controls.Add( _radEndByNone );
            Controls.Add( _radEndByDate );
            Controls.Add( _dpEndBy );
            Controls.Add( _radEndByOccurrenceCount );
            Controls.Add( _tbEndByOccurrenceCount );

            // exclusions
            Controls.Add( _hfExclusionDateRangeListValues );
            Controls.Add( _dpExclusionDateRange );
        }



        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            if ( string.IsNullOrWhiteSpace( _iCalendarContent ) )
            {
                iCalendarContent = iCalendarContentEmptyEvent;
            }

            writer.AddAttribute( "id", this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Start DateTime
            _dpStartDateTime.RenderControl( writer );

            // Duration
            if ( this.ShowDuration )
            {
                writer.AddAttribute( "class", "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.Write( "<span class='control-label'>Duration</span>" );
                writer.AddAttribute( "class", "form-control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbDurationHours.RenderControl( writer );

                _tbDurationMinutes.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            // One-time/Recurring Radiobuttons
            writer.AddAttribute( "class", "form-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radOneTime.RenderControl( writer );
            _radRecurring.RenderControl( writer );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Recurrence Panel: Start
            if ( _radOneTime.Checked )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

            writer.AddAttribute( "id", "schedule-recurrence-panel_" + this.ClientID );
            writer.AddAttribute( "class", "js-schedule-recurrence-panel" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "legend-small" );
            writer.RenderBeginTag( HtmlTextWriterTag.Legend );
            writer.Write( "Recurrence" );
            writer.RenderEndTag();


            // OccurrencePattern Radiobuttons
            writer.AddAttribute( "class", "form-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "<span class='control-label'>Occurrence Pattern</span>" );
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radSpecificDates.RenderControl( writer );
            _radDaily.RenderControl( writer );
            _radWeekly.RenderControl( writer );
            _radMonthly.RenderControl( writer );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Specific Date Panel
            writer.AddAttribute( "class", "recurrence-pattern-type control-group controls recurrence-pattern-specific-date" );
            writer.AddAttribute( "id", "recurrence-pattern-specific-date_" + this.ClientID );

            if ( !_radSpecificDates.Checked )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _hfSpecificDateListValues.RenderControl( writer );

            writer.Write( @"
                <ul class='lstSpecificDates'>" );

            foreach ( var dateValue in _hfSpecificDateListValues.Value.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                writer.Write( "<li><span>" + dateValue + "</span><a href='#' style='display: none'><i class='fa fa-times'></i></a></li>" );
            }

            writer.Write( @"
                </ul>
                <a class='btn btn-action btn-sm add-specific-date'><i class='fa fa-plus'></i>
                    <span> Add Date</span>
                </a>
" );

            writer.AddAttribute( "id", "add-specific-date-group_" + this.ClientID );
            writer.AddAttribute( "class", "js-add-specific-date-group" );

            writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _dpSpecificDate.AddCssClass( "specific-date" );
            _dpSpecificDate.RenderControl( writer );
            writer.Write( @"
                <div class='actions'>
                    <a class='btn btn-primary btn-xs add-specific-date-ok'>
                        <span>OK</span>
                    </a>
                    <a class='btn btn-link btn-xs add-specific-date-cancel'>
                        <span>Cancel</span>
                    </a>
                </div>
" );

            writer.RenderEndTag();
            writer.RenderEndTag();

            // daily recurrence panel
            writer.AddAttribute( "id", "recurrence-pattern-daily_" + this.ClientID );
            writer.AddAttribute( "class", "recurrence-pattern-type recurrence-pattern-daily" );

            if ( !_radDaily.Checked )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            //writer.AddAttribute( "class", "form-group controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radDailyEveryXDays.Text = "Every";
            _radDailyEveryXDays.RenderControl( writer );
            _tbDailyEveryXDays.AddCssClass( "margin-l-sm" );
            _tbDailyEveryXDays.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radDailyEveryWeekday.Text = "Every weekday";
            _radDailyEveryWeekday.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radDailyEveryWeekendDay.Text = "Every weekend day";
            _radDailyEveryWeekendDay.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();

            // weekly recurrence panel
            writer.AddAttribute( "id", "recurrence-pattern-weekly_" + this.ClientID );
            writer.AddAttribute( "class", "recurrence-pattern-type recurrence-pattern-weekly" );

            if ( !_radWeekly.Checked )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

            writer.AddAttribute( "class", "weekly-x-weeks" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "<span>Every </span>" );
            _tbWeeklyEveryX.RenderControl( writer );
            writer.Write( "<span> on</span>" );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "week-days" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbWeeklySunday.RenderControl( writer );
            _cbWeeklyMonday.RenderControl( writer );
            _cbWeeklyTuesday.RenderControl( writer );
            _cbWeeklyWednesday.RenderControl( writer );
            _cbWeeklyThursday.RenderControl( writer );
            _cbWeeklyFriday.RenderControl( writer );
            _cbWeeklySaturday.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // monthly
            writer.AddAttribute( "id", "recurrence-pattern-monthly_" + this.ClientID );
            writer.AddAttribute( "class", "recurrence-pattern-type recurrence-pattern-monthly" );

            if ( !_radMonthly.Checked )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( "class", "form-group controls" );

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radMonthlyDayX.Text = "Day";
            _radMonthlyDayX.RenderControl( writer );
            _tbMonthlyDayX.AddCssClass( "margin-l-sm" );
            _tbMonthlyDayX.RenderControl( writer );
            writer.Write( "<span> of every </span>" );
            _tbMonthlyXMonths.RenderControl( writer );
            writer.Write( "<span> months</span>" );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radMonthlyNth.Text = "The";
            _radMonthlyNth.RenderControl( writer );
            _ddlMonthlyNth.AddCssClass( "margin-l-sm" );
            _ddlMonthlyNth.RenderControl( writer );
            writer.Write( "<span> </span>" );
            _ddlMonthlyDayName.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();

            // end date
            writer.Write( @"
<div class='controls'><hr /></div>
" );
            writer.AddAttribute( "class", "continue-until js-continue-until" );
            if ( _radSpecificDates.Checked )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "<label class='control-label'>Continue Until</label>" );
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radEndByNone.Text = "No End";
            _radEndByNone.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radEndByDate.Text = "End by ";
            _radEndByDate.RenderControl( writer );
            _dpEndBy.AddCssClass( "margin-l-sm" );
            _dpEndBy.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radEndByOccurrenceCount.Text = "End after ";
            _radEndByOccurrenceCount.RenderControl( writer );
            _tbEndByOccurrenceCount.AddCssClass( "margin-l-sm" );
            _tbEndByOccurrenceCount.RenderControl( writer );
            writer.Write( "<span> occurrences</span>" );

            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.RenderEndTag();

            // exclusions
            writer.Write( @"<hr />" );

            writer.AddAttribute( "class", "exclusions js-exclusion-dates" );
            if ( _radSpecificDates.Checked )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( @"<label class='control-label'>Exclusions</label>" );

            writer.AddAttribute( "id", "recurrence-pattern-exclusions_" + this.ClientID );
            writer.AddAttribute( "class", "form-group controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _hfExclusionDateRangeListValues.RenderControl( writer );

            writer.Write( @"
                <ul class='lstExclusionDateRanges'>" );

            foreach ( var dateRangeValue in _hfExclusionDateRangeListValues.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                writer.Write( "<li><span>" + dateRangeValue + "</span> <a href='#' style='display: none'><i class='fa fa-times'></i></a></li>" );
            }

            writer.Write( @"
                </ul>
                <a class='btn btn-action btn-sm add-exclusion-daterange'><i class='fa fa-plus'></i>
                    <span> Add Date Range</span>
                </a>" );

            writer.AddAttribute( "class", "add-exclusion js-add-exclusion-daterange-group" );
            writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _dpExclusionDateRange.RenderControl( writer );
            writer.Write( @"
                <div class='actions'>
                    <a class='btn btn-primary btn-xs add-exclusion-daterange-ok'>
                        <span>OK</span>
                    </a>
                    <a class='btn btn-link btn-xs add-exclusion-daterange-cancel'>
                        <span>Cancel</span>
                    </a>
                </div>" );

            writer.AddAttribute( "class", "js-last-item clearfix" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.RenderEndTag();

            // Recurrence Panel: End
            writer.RenderEndTag();

            // write out the closing div for <div class='exclusions'>
            writer.RenderEndTag();

            writer.RenderEndTag();
        }
    }
}
