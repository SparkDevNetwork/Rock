using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using church.ccv.Hr.Data;
using church.ccv.Hr.Model;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Hr
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Time Card Detail" )]
    [Category( "CCV > Time Card" )]
    [Description( "Displays the details of a time card." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve all timecards, regardless of department." )]

    [SystemEmailField( "Submitted Email", "The email to send when a time card is submitted. If not specified, an email will not be sent.", false )]
    [SystemEmailField( "Approved Email", "The email to send when a time card is approved. If not specified, an email will not be sent.", false )]
    public partial class TimeCardDetail : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            RockPage.AddCSSLink( ResolveRockUrl( "~/Plugins/church_ccv/Hr/Styles/hr.css" ) );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
        }

        #endregion

        #region repeater helpers

        /// <summary>
        /// Formats the time card time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="zeroAsDash">if set to <c>true</c> [zero as dash].</param>
        /// <returns></returns>
        public string FormatTimeCardTime( DateTime? dateTime, bool zeroAsDash = false )
        {
            if ( dateTime.HasValue )
            {
                if ( dateTime.Value.TimeOfDay == TimeSpan.Zero && zeroAsDash )
                {
                    return "-";
                }
                else
                {
                    return dateTime.Value.ToString( "hh:mmtt" );
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Formats the time card hours.
        /// </summary>
        /// <param name="hours">The hours.</param>
        /// <returns></returns>
        public string FormatTimeCardHours( decimal? hours )
        {
            if ( hours.HasValue && hours != 0 )
            {
                TimeSpan timeSpan = TimeSpan.FromHours( Convert.ToDouble( hours ) );
                return timeSpan.TotalHours.ToString( "0.##" );
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();
        }

        private List<HoursPerTimeCardDay> _workedRegularHoursCache = null;
        private List<HoursPerTimeCardDay> _workedOvertimeHoursCache = null;
        private List<DateTime> _holidayDatesCache = null;

        public List<DateTime> GetHolidayDates( TimeCard timeCard )
        {
            Rock.Model.Schedule timeCardHolidaySchedule = new Rock.Model.ScheduleService( new Rock.Data.RockContext() ).Get( church.ccv.Hr.SystemGuid.Schedule.TIMECARD_HOLIDAY_SCHEDULE.AsGuid() );

            if ( timeCardHolidaySchedule != null )
            {
                DDay.iCal.Event calEvent = timeCardHolidaySchedule.GetCalenderEvent();
                if ( calEvent != null )
                {
                    return calEvent.GetOccurrences( timeCard.TimeCardPayPeriod.StartDate, timeCard.TimeCardPayPeriod.EndDate ).Select( a => a.Period.StartTime.Date ).ToList();
                }
            }

            return new List<DateTime>();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptTimeCardDay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptTimeCardDay_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var timeCardDay = e.Item.DataItem as TimeCardDay;
            if ( timeCardDay != null )
            {
                var repeaterItem = e.Item;

                bool isHoliday = _holidayDatesCache.Any( a => a == timeCardDay.StartDateTime.Date );
                bool isEndOfWeek = timeCardDay.StartDateTime.DayOfWeek == DayOfWeek.Sunday;

                Panel pnlTimeCardRow = repeaterItem.FindControl( "pnlTimeCardRow" ) as Panel;
                Panel pnlTimeCardSummaryRow = repeaterItem.FindControl( "pnlTimeCardSummaryRow" ) as Panel;

                pnlTimeCardSummaryRow.Visible = isEndOfWeek;

                // Display Only
                Literal lTimeCardDayName = repeaterItem.FindControl( "lTimeCardDayName" ) as Literal;
                lTimeCardDayName.Text = timeCardDay.StartDateTime.ToString( "ddd" );

                Badge lTimeCardDate = repeaterItem.FindControl( "lTimeCardDate" ) as Badge;
                lTimeCardDate.BadgeType = isHoliday ? "Info" : string.Empty;
                lTimeCardDate.ToolTip = isHoliday ? "Holiday" : string.Empty;
                lTimeCardDate.Text = timeCardDay.StartDateTime.ToString( "MM/dd" );

                Literal lStartDateTime = repeaterItem.FindControl( "lStartDateTime" ) as Literal;
                lStartDateTime.Text = FormatTimeCardTime( timeCardDay.StartDateTime, !timeCardDay.TotalWorkedDuration.HasValue );

                Literal lLunchStartDateTime = repeaterItem.FindControl( "lLunchStartDateTime" ) as Literal;
                lLunchStartDateTime.Text = FormatTimeCardTime( timeCardDay.LunchStartDateTime );

                Literal lLunchEndDateTime = repeaterItem.FindControl( "lLunchEndDateTime" ) as Literal;
                lLunchEndDateTime.Text = FormatTimeCardTime( timeCardDay.LunchEndDateTime );

                Literal lEndDateTime = repeaterItem.FindControl( "lEndDateTime" ) as Literal;
                lEndDateTime.Text = FormatTimeCardTime( timeCardDay.EndDateTime );

                Literal lWorkedRegularHours = repeaterItem.FindControl( "lWorkedRegularHours" ) as Literal;
                var regularHoursForDay = _workedRegularHoursCache.Where( a => a.TimeCardDay == timeCardDay ).FirstOrDefault();
                lWorkedRegularHours.Text = FormatTimeCardHours( regularHoursForDay != null ? regularHoursForDay.Hours : 0 );

                Badge lWorkedOvertimeHours = repeaterItem.FindControl( "lWorkedOvertimeHours" ) as Badge;
                var workedOvertimeHoursForDay = _workedOvertimeHoursCache.Where( a => a.TimeCardDay == timeCardDay ).FirstOrDefault();
                lWorkedOvertimeHours.Text = FormatTimeCardHours( workedOvertimeHoursForDay != null ? workedOvertimeHoursForDay.Hours : 0 );

                Badge lPaidVacationHours = repeaterItem.FindControl( "lPaidVacationHours" ) as Badge;
                lPaidVacationHours.Text = FormatTimeCardHours( timeCardDay.PaidVacationHours );

                Badge lPaidHolidayHours = repeaterItem.FindControl( "lPaidHolidayHours" ) as Badge;
                decimal earnedHolidayHours = timeCardDay.EarnedHolidayHours ?? 0;
                if ( earnedHolidayHours != 0 )
                {
                    lPaidHolidayHours.Text = FormatTimeCardHours( ( timeCardDay.PaidHolidayHours ?? 0 ) + earnedHolidayHours );
                    if ( ( timeCardDay.PaidHolidayHours ?? 0 ) != 0 )
                    {
                        lPaidHolidayHours.ToolTip = string.Format( "{0} Paid Holiday Hours + {1}", timeCardDay.PaidHolidayHours, earnedHolidayHours );
                    }
                    else
                    {
                        lPaidHolidayHours.ToolTip = string.Format( "+ {1} Paid Holiday Hours", timeCardDay.PaidHolidayHours, earnedHolidayHours );
                    }
                }
                else
                {
                    lPaidHolidayHours.Text = FormatTimeCardHours( timeCardDay.PaidHolidayHours );
                }

                Badge lPaidSickHours = repeaterItem.FindControl( "lPaidSickHours" ) as Badge;
                lPaidSickHours.Text = FormatTimeCardHours( timeCardDay.PaidSickHours );

                decimal totalOtherHours = ( timeCardDay.PaidVacationHours ?? 0 ) + ( timeCardDay.TotalHolidayHours ?? 0 ) + ( timeCardDay.PaidSickHours ?? 0 );
                Literal lTotalHours = repeaterItem.FindControl( "lTotalHours" ) as Literal;
                lTotalHours.Text = FormatTimeCardHours( timeCardDay.TotalWorkedDuration + totalOtherHours );

                Literal lNotes = repeaterItem.FindControl( "lNotes" ) as Literal;
                lNotes.Text = timeCardDay.Notes;

                if ( isEndOfWeek )
                {
                    Literal lWorkedRegularHoursSummary = repeaterItem.FindControl( "lWorkedRegularHoursSummary" ) as Literal;
                    Literal lWorkedOvertimeHoursSummary = repeaterItem.FindControl( "lWorkedOvertimeHoursSummary" ) as Literal;
                    Literal lOtherHoursSummary = repeaterItem.FindControl( "lOtherHoursSummary" ) as Literal;
                    Literal lTotalHoursSummary = repeaterItem.FindControl( "lTotalHoursSummary" ) as Literal;

                    var workedRegularSummaryHours = timeCardDay.TimeCard.GetRegularHours()
                        .Where( a => a.TimeCardDay.StartDateTime.Date <= timeCardDay.StartDateTime.Date
                            && a.TimeCardDay.StartDateTime.Date >= timeCardDay.StartDateTime.Date.AddDays( -7 ) )
                            .Sum( a => a.Hours );
                    var workedOvertimeSummaryHours = timeCardDay.TimeCard.GetOvertimeHours()
                        .Where( a => a.TimeCardDay.StartDateTime.Date <= timeCardDay.StartDateTime.Date
                            && a.TimeCardDay.StartDateTime.Date >= timeCardDay.StartDateTime.Date.AddDays( -7 ) )
                            .Sum( a => a.Hours );
                    var otherSummaryHours = timeCardDay.TimeCard.TimeCardDays
                        .Where( a => a.StartDateTime.Date <= timeCardDay.StartDateTime.Date
                            && a.StartDateTime.Date >= timeCardDay.StartDateTime.Date.AddDays( -7 ) )
                            .Sum( a => ( a.PaidHolidayHours ?? 0 ) + ( a.PaidSickHours ?? 0 ) + ( a.PaidVacationHours ?? 0 ) + ( a.EarnedHolidayHours ?? 0 ) );

                    string subtotalItemFormat = "{0}";
                    lWorkedRegularHoursSummary.Text = string.Format( subtotalItemFormat, FormatTimeCardHours( workedRegularSummaryHours ) );
                    lWorkedOvertimeHoursSummary.Text = string.Format( subtotalItemFormat, FormatTimeCardHours( workedOvertimeSummaryHours ) );
                    lOtherHoursSummary.Text = string.Format( subtotalItemFormat, FormatTimeCardHours( otherSummaryHours ) );
                    lTotalHoursSummary.Text = string.Format( subtotalItemFormat, FormatTimeCardHours( workedRegularSummaryHours + workedOvertimeSummaryHours + otherSummaryHours ) );
                }

                Literal lTimeCardDayNameEdit = repeaterItem.FindControl( "lTimeCardDayNameEdit" ) as Literal;
                lTimeCardDayNameEdit.Text = lTimeCardDayName.Text;

                Badge lTimeCardDateEdit = repeaterItem.FindControl( "lTimeCardDateEdit" ) as Badge;
                lTimeCardDateEdit.BadgeType = isHoliday ? "Info" : string.Empty;
                lTimeCardDateEdit.ToolTip = isHoliday ? "Holiday" : string.Empty;
                lTimeCardDateEdit.Text = timeCardDay.StartDateTime.ToString( "MM/dd" );

                // Edit Controls
                TimePicker tpTimeIn = repeaterItem.FindControl( "tpTimeIn" ) as TimePicker;
                if ( timeCardDay.StartDateTime.TimeOfDay != TimeSpan.Zero || ( timeCardDay.TotalWorkedDuration ?? 0 ) > 0 )
                {
                    tpTimeIn.SelectedTime = timeCardDay.StartDateTime.TimeOfDay;
                }
                else
                {
                    tpTimeIn.SelectedTime = null;
                }

                TimePicker tpLunchOut = repeaterItem.FindControl( "tpLunchOut" ) as TimePicker;
                tpLunchOut.SelectedTime = timeCardDay.LunchStartDateTime.HasValue ? timeCardDay.LunchStartDateTime.Value.TimeOfDay : (TimeSpan?)null;

                TimePicker tpLunchIn = repeaterItem.FindControl( "tpLunchIn" ) as TimePicker;
                tpLunchIn.SelectedTime = timeCardDay.LunchEndDateTime.HasValue ? timeCardDay.LunchEndDateTime.Value.TimeOfDay : (TimeSpan?)null;

                TimePicker tpTimeOut = repeaterItem.FindControl( "tpTimeOut" ) as TimePicker;
                tpTimeOut.SelectedTime = timeCardDay.EndDateTime.HasValue ? timeCardDay.EndDateTime.Value.TimeOfDay : (TimeSpan?)null;

                RockDropDownList ddlVacationHours = repeaterItem.FindControl( "ddlVacationHours" ) as RockDropDownList;
                RockDropDownList ddlHolidayHours = repeaterItem.FindControl( "ddlHolidayHours" ) as RockDropDownList;
                RockDropDownList ddlSickHours = repeaterItem.FindControl( "ddlSickHours" ) as RockDropDownList;

                ddlVacationHours.Items.Clear();
                ddlVacationHours.Items.Add( string.Empty );
                ddlHolidayHours.Items.Clear();
                ddlHolidayHours.Items.Add( string.Empty );
                ddlSickHours.Items.Clear();
                ddlSickHours.Items.Add( string.Empty );

                for ( double hour = 0.25; hour <= 8; hour += 0.25 )
                {
                    ddlVacationHours.Items.Add( hour.ToString( "0.00" ) );
                    ddlHolidayHours.Items.Add( hour.ToString( "0.00" ) );
                    ddlSickHours.Items.Add( hour.ToString( "0.00" ) );
                }

                ddlVacationHours.SetValue( timeCardDay.PaidVacationHours.ToNearestQtrHour().ToString() );
                ddlHolidayHours.SetValue( timeCardDay.PaidHolidayHours.ToNearestQtrHour().ToString() );
                Label lEarnedHolidayHours = repeaterItem.FindControl( "lEarnedHolidayHours" ) as Label;
                lEarnedHolidayHours.Attributes["data-is-holiday"] = isHoliday ? "1" : string.Empty;
                if ( !isHoliday )
                {
                    lEarnedHolidayHours.Style[HtmlTextWriterStyle.Display] = "none";
                }

                if ( timeCardDay.EarnedHolidayHours.HasValue && timeCardDay.EarnedHolidayHours > 0 )
                {
                    // if they have earned hours, show it, even if it isn't a holiday
                    lEarnedHolidayHours.Style[HtmlTextWriterStyle.Display] = "block";
                    lEarnedHolidayHours.Text = string.Format( "+ {0}", timeCardDay.EarnedHolidayHours );
                }

                ddlSickHours.SetValue( timeCardDay.PaidSickHours.ToNearestQtrHour().ToString() );

                RockTextBox tbNotes = repeaterItem.FindControl( "tbNotes" ) as RockTextBox;
                tbNotes.Text = timeCardDay.Notes;

                // Action Controls
                LinkButton lbSave = repeaterItem.FindControl( "lbSave" ) as LinkButton;
                lbSave.CommandArgument = timeCardDay.Id.ToString();

                // only show the Save button if in edit mode (this current person can edit this time card)
                lbSave.Visible = hfEditMode.Value.AsBooleanOrNull() ?? false;

                // only enable the EditRow controls if in edit mode (this current person can edit this time card)
                Panel pnlEditRow = repeaterItem.FindControl( "pnlEditRow" ) as Panel;
                pnlEditRow.Enabled = hfEditMode.Value.AsBooleanOrNull() ?? false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int timeCardId = PageParameter( "TimeCardId" ).AsInteger();
            hfTimeCardId.Value = timeCardId.ToString();

            nbMessage.Visible = false;

            var hrContext = new HrContext();
            var timeCardDayService = new TimeCardDayService( hrContext );
            var timeCardService = new TimeCardService( hrContext );
            var timeCard = timeCardService.Queryable( "TimeCardDays" ).FirstOrDefault( a => a.Id == timeCardId );
            if ( timeCard == null )
            {
                nbMessage.Text = "A valid TimeCardId must be specified";
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Visible = true;
                pnlDetails.Visible = false;
                return;
            }

            if ( !TimeCardPayPeriodService.ValidateApproverAttributeExists( hrContext ) )
            {
                nbMessage.Text = "WARNING: A GroupMember Attribute with a key of 'CanApproveTimeCards' is required before time cards can be submitted.";
                nbMessage.NotificationBoxType = NotificationBoxType.Warning;
                nbMessage.Visible = true;
            }

            // make sure the current person is the timecard.person or is an approver of the timecard.person
            List<Person> approvers = TimeCardPayPeriodService.GetApproversForStaffPerson( hrContext, this.CurrentPersonId ?? 0 );

            bool editMode = false;

            pnlDetails.Visible = true;
            pnlApproverActions.Visible = false;
            int? timeCardPersonId = timeCard.PersonAlias != null ? timeCard.PersonAlias.PersonId : (int?)null;
            if ( timeCardPersonId == this.CurrentPersonId )
            {
                // only allow the timecard to be edited by the timeCard.Person, and only if the status is InProgress or Submitted
                if ( timeCard.TimeCardStatus == TimeCardStatus.InProgress || timeCard.TimeCardStatus == TimeCardStatus.Submitted )
                {
                    editMode = true;
                }
                else
                {
                    // timecard will be readonly, and not show the approve button
                }
            }
            else
            {
                if ( this.IsUserAuthorized( Authorization.APPROVE ) || approvers.Any( a => a.Id == this.CurrentPersonId ) )
                {
                    // if the current person a global Approver or an approver of the timecard.person, enable the Approve button if is has been submitted.
                    pnlApproverActions.Visible = timeCard.TimeCardStatus == TimeCardStatus.Submitted;
                }
                else
                {
                    // if the currentPersonId is neither the TimeCard person or an approver of the timecard person, don't let them see it
                    nbMessage.Visible = true;
                    nbMessage.Text = "You are only allowed to view your timecards or timecards of person that report to you.";
                    nbMessage.NotificationBoxType = NotificationBoxType.Warning;
                    pnlDetails.Visible = false;
                    return;
                }
            }

            hfEditMode.Value = editMode.ToTrueFalse();

            // only show the Submit panel if in edit mode
            pnlPersonActions.Visible = editMode;

            lTimeCardPersonName.Text = string.Format( "{0}", timeCard.PersonAlias );
            lTitle.Text = "Pay Period: " + timeCard.TimeCardPayPeriod.ToString();
            hlblSubTitle.Text = timeCard.GetStatusText();

            switch ( timeCard.TimeCardStatus )
            {
                case TimeCardStatus.Approved:
                    hlblSubTitle.LabelType = LabelType.Success;
                    break;
                case TimeCardStatus.Submitted:
                    hlblSubTitle.LabelType = LabelType.Warning;
                    break;
                case TimeCardStatus.Exported:
                    hlblSubTitle.LabelType = LabelType.Default;
                    break;
                default:
                    hlblSubTitle.LabelType = LabelType.Info;
                    break;
            }

            var qry = timeCardDayService.Queryable().Where( a => a.TimeCardId == timeCardId ).OrderBy( a => a.StartDateTime );
            var qryList = qry.ToList();

            // ensure 14 days
            if ( qryList.Count() < 14 )
            {
                var missingDays = new List<TimeCardDay>();
                var startDateTime = timeCard.TimeCardPayPeriod.StartDate;
                while ( startDateTime < timeCard.TimeCardPayPeriod.EndDate )
                {
                    if ( !qryList.Any( a => a.StartDateTime.Date == startDateTime.Date ) )
                    {
                        var timeCardDay = new TimeCardDay();
                        timeCardDay.TimeCardId = timeCardId;
                        timeCardDay.StartDateTime = startDateTime;
                        missingDays.Add( timeCardDay );
                    }

                    startDateTime = startDateTime.AddDays( 1 );
                }

                timeCardDayService.AddRange( missingDays );
                hrContext.SaveChanges();
            }

            var timeCardDayList = qry.ToList();

            // cache some stuff for rptTimeCardDay_ItemDataBound
            _workedRegularHoursCache = timeCard.GetRegularHours();
            _workedOvertimeHoursCache = timeCard.GetOvertimeHours();
            _holidayDatesCache = GetHolidayDates( timeCard );

            // bind time card day repeater 
            rptTimeCardDay.DataSource = timeCardDayList;
            rptTimeCardDay.DataBind();

            // Actions/Submit
            ddlSubmitTo.Items.Clear();
            ddlSubmitTo.Items.Add( new ListItem() );
            ddlSubmitTo.Items.AddRange( approvers.Select( a => new ListItem( a.ToString(), a.Id.ToString() ) ).ToArray() );

            // Totals
            lTotalRegularWorked.Text = timeCard.GetRegularHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lTotalOvertimeWorked.Text = timeCard.GetOvertimeHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lTotalVacationPaid.Text = timeCard.PaidVacationHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lTotalHolidayPaid.Text = timeCard.PaidHolidayHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lTotalSickPaid.Text = timeCard.PaidSickHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );

            var totalHours = timeCard.GetTotalWorkedHoursPerDay().Sum( a => a.Hours ?? 0 )
                + timeCard.PaidVacationHours().Sum( a => a.Hours ?? 0 )
                + timeCard.PaidHolidayHours().Sum( a => a.Hours ?? 0 )
                + timeCard.PaidSickHours().Sum( a => a.Hours ?? 0 );

            lTotalHours.Text = totalHours.ToString( "0.##" );

            // if this timeCard doesn't have any history records yet, assume they are entering the TimeCard for the first time
            var timeCardHistoryService = new TimeCardHistoryService( hrContext );
            if ( !timeCardHistoryService.Queryable().Any( a => a.TimeCardId == timeCard.Id ) )
            {
                TimeCardHistory timeCardHistory = new TimeCardHistory();
                timeCardHistory.TimeCardId = timeCard.Id;
                timeCardHistory.HistoryDateTime = RockDateTime.Now;
                timeCardHistory.TimeCardStatus = TimeCardStatus.InProgress;
                timeCardHistory.Notes = string.Format( "{0} created new time card", this.CurrentPersonAlias );
                timeCardHistory.StatusPersonAliasId = this.CurrentPersonAliasId;

                timeCardHistoryService.Add( timeCardHistory );
                hrContext.SaveChanges();

                // get after saving to flesh out the virtual fields
                timeCardHistory = timeCardHistoryService.Get( timeCardHistory.Guid );
            }

            // TimeCard History grid
            var historyQry = timeCardHistoryService.Queryable().Where( a => a.TimeCardId == timeCard.Id ).OrderByDescending( a => a.HistoryDateTime );
            gHistory.DataSource = historyQry.ToList();
            gHistory.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var hrContext = new HrContext();
            var repeaterItem = ( sender as Control ).BindingContainer as RepeaterItem;

            var timeCardDayId = ( sender as LinkButton ).CommandArgument;
            var timeCardDayService = new TimeCardDayService( hrContext );
            var timeCardDay = timeCardDayService.Get( timeCardDayId.AsInteger() );

            TimePicker tpTimeIn = repeaterItem.FindControl( "tpTimeIn" ) as TimePicker;

            var timeCardDate = timeCardDay.StartDateTime.Date;
            timeCardDay.StartDateTime = timeCardDate + ( tpTimeIn.SelectedTime ?? TimeSpan.Zero ).ToNearestQtrHour();

            TimePicker tpLunchOut = repeaterItem.FindControl( "tpLunchOut" ) as TimePicker;
            timeCardDay.LunchStartDateTime = GetTimeCardTimeValue( repeaterItem, timeCardDay, tpLunchOut );

            TimePicker tpLunchIn = repeaterItem.FindControl( "tpLunchIn" ) as TimePicker;
            timeCardDay.LunchEndDateTime = GetTimeCardTimeValue( repeaterItem, timeCardDay, tpLunchIn );

            TimePicker tpTimeOut = repeaterItem.FindControl( "tpTimeOut" ) as TimePicker;
            timeCardDay.EndDateTime = GetTimeCardTimeValue( repeaterItem, timeCardDay, tpTimeOut );

            RockDropDownList ddlVacationHours = repeaterItem.FindControl( "ddlVacationHours" ) as RockDropDownList;
            timeCardDay.PaidVacationHours = ddlVacationHours.SelectedValue.AsDecimalOrNull();

            RockDropDownList ddlHolidayHours = repeaterItem.FindControl( "ddlHolidayHours" ) as RockDropDownList;
            timeCardDay.PaidHolidayHours = ddlHolidayHours.SelectedValue.AsDecimalOrNull();

            _holidayDatesCache = GetHolidayDates( timeCardDay.TimeCard );
            bool isHoliday = _holidayDatesCache.Any( a => a == timeCardDay.StartDateTime.Date );
            timeCardDay.EarnedHolidayHours = timeCardDay.GetEarnedHolidayHours( isHoliday );

            RockDropDownList ddlSickHours = repeaterItem.FindControl( "ddlSickHours" ) as RockDropDownList;
            timeCardDay.PaidSickHours = ddlSickHours.SelectedValue.AsDecimalOrNull();

            RockTextBox tbNotes = repeaterItem.FindControl( "tbNotes" ) as RockTextBox;
            timeCardDay.Notes = tbNotes.Text;

            // log changes to the TimeCardDays if changes are made after getting submitted (status is not TimeCardStatus.InProgress)
            List<string> sbTimeCardDayHistory = new List<string>();
            if ( timeCardDay.TimeCard.TimeCardStatus != TimeCardStatus.InProgress )
            {
                var properties = timeCardDay.GetType().GetProperties().Where( a => !a.GetGetMethod().IsVirtual );

                foreach ( PropertyInfo propInfo in properties )
                {
                    var timeCardDayEntry = hrContext.Entry( timeCardDay );

                    // If entire entity was added or deleted or this property was modified
                    var dbPropertyEntry = timeCardDayEntry.Property( propInfo.Name );
                    if ( dbPropertyEntry != null && dbPropertyEntry.IsModified )
                    {
                        if ( dbPropertyEntry.CurrentValue != dbPropertyEntry.OriginalValue )
                        {
                            sbTimeCardDayHistory.Add( string.Format( "changed {0} from '{1}' to '{2}'", propInfo.Name.SplitCase(), dbPropertyEntry.OriginalValue, dbPropertyEntry.CurrentValue ) );
                        }
                    }
                }
            }

            if ( sbTimeCardDayHistory.Any() )
            {
                var timeCardHistoryService = new TimeCardHistoryService( hrContext );
                var timeCardHistory = new TimeCardHistory();
                timeCardHistory.TimeCardId = timeCardDay.TimeCardId;
                timeCardHistory.TimeCardStatus = timeCardDay.TimeCard.TimeCardStatus;
                timeCardHistory.StatusPersonAliasId = this.CurrentPersonAliasId;
                timeCardHistory.HistoryDateTime = RockDateTime.Now;
                timeCardHistory.Notes = sbTimeCardDayHistory.AsDelimited( "<br/>" );
                timeCardHistoryService.Add( timeCardHistory );
            }

            hrContext.SaveChanges();

            ShowDetail();
        }

        /// <summary>
        /// Sets the time card time value.
        /// </summary>
        /// <param name="repeaterItem">The repeater item.</param>
        /// <param name="timeCardDay">The time card day.</param>
        /// <param name="tpTimePicker">The tp time picker.</param>
        private DateTime? GetTimeCardTimeValue( RepeaterItem repeaterItem, TimeCardDay timeCardDay, TimePicker tpTimePicker )
        {
            if ( tpTimePicker.SelectedTime.HasValue )
            {
                var timeCardDate = timeCardDay.StartDateTime.Date;

                // round to the nearest 15 minute
                tpTimePicker.SelectedTime = tpTimePicker.SelectedTime.ToNearestQtrHour();

                if ( tpTimePicker.SelectedTime < timeCardDay.StartDateTime.TimeOfDay )
                {
                    // they picked a time that is earlier than the StartDateTime, which means it is the next day
                    return timeCardDate.AddDays( 1 ) + tpTimePicker.SelectedTime.Value;
                }
                else
                {
                    return timeCardDate + tpTimePicker.SelectedTime.Value;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSubmit_Click( object sender, EventArgs e )
        {
            var hrContext = new HrContext();

            int timeCardId = hfTimeCardId.Value.AsInteger();
            var timeCardService = new TimeCardService( hrContext );
            var timeCard = timeCardService.Get( timeCardId );
            if ( timeCard == null )
            {
                return;
            }

            var submitToPersonAlias = new PersonAliasService( hrContext ).Get( ddlSubmitTo.SelectedValue.AsInteger() );
            if ( submitToPersonAlias == null )
            {
                return;
            }

            timeCard.TimeCardStatus = TimeCardStatus.Submitted;
            timeCard.SubmittedToPersonAliasId = submitToPersonAlias.Id;
            var timeCardHistoryService = new TimeCardHistoryService( hrContext );
            var timeCardHistory = new TimeCardHistory();
            timeCardHistory.TimeCardId = timeCard.Id;
            timeCardHistory.TimeCardStatus = timeCard.TimeCardStatus;
            timeCardHistory.StatusPersonAliasId = this.CurrentPersonAliasId;
            timeCardHistory.HistoryDateTime = RockDateTime.Now;

            // NOTE: if status was already Submitted, still log it as history
            timeCardHistory.Notes = string.Format( "Submitted to {0}", submitToPersonAlias );
            timeCardHistoryService.Add( timeCardHistory );

            hrContext.SaveChanges();

            // Send an email (if specified) after timecard is marked submitted
            Guid? submittedEmailTemplateGuid = GetAttributeValue( "SubmittedEmail" ).AsGuidOrNull();

            if ( submittedEmailTemplateGuid.HasValue )
            {
                var mergeObjects = GlobalAttributesCache.GetMergeFields( null );
                mergeObjects.Add( "TimeCardPayPeriod", timeCard.TimeCardPayPeriod.ToString() );
                mergeObjects.Add( "TimeCard", timeCard );
                mergeObjects.Add( "Person", this.CurrentPerson );
                mergeObjects.Add( "SubmitToPerson", submitToPersonAlias.Person );

                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( submitToPersonAlias.Person.Email, mergeObjects ) );
                Email.Send( submittedEmailTemplateGuid.Value, recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
            }

            nbSubmittedSuccessMessage.Text = string.Format( "Successfully submitted to {0}", submitToPersonAlias );
            nbSubmittedSuccessMessage.Visible = true;
            ShowDetail();
        }

        /// <summary>
        /// Handles the Click event of the btnApprove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApprove_Click( object sender, EventArgs e )
        {
            int timeCardId = hfTimeCardId.Value.AsInteger();

            // Send an email (if specified) after timecard is marked approved
            Guid? approvedEmailTemplateGuid = GetAttributeValue( "ApprovedEmail" ).AsGuidOrNull();

            var timeCardService = new TimeCardService( new HrContext() );

            if ( timeCardService.ApproveTimeCard( timeCardId, this.RockPage, approvedEmailTemplateGuid ) )
            {
                nbApprovedSuccessMessage.Text = string.Format( "Successfully approved by {0}", this.CurrentPersonAlias );
                nbApprovedSuccessMessage.NotificationBoxType = NotificationBoxType.Success;
                nbApprovedSuccessMessage.Visible = true;
            }
            else
            {
                // shouldn't happen, but just in case
                nbApprovedSuccessMessage.Text = string.Format( "Error approving timecard" );
                nbApprovedSuccessMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbApprovedSuccessMessage.Visible = true;
            }

            ShowDetail();
        }
    }
}