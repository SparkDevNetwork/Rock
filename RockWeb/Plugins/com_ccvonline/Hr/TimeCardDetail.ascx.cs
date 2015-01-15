using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.ccvonline.Hr.Data;
using com.ccvonline.Hr.Model;

using Rock;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_ccvonline.Hr
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Time Card Detail" )]
    [Category( "CCV > Time Card" )]
    [Description( "Displays the details of a time card." )]
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
        private List<HoursPerTimeCardDay> _workedHolidayHoursCache = null;

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

                // Display Only
                Literal lTimeCardDayName = repeaterItem.FindControl( "lTimeCardDayName" ) as Literal;
                lTimeCardDayName.Text = timeCardDay.StartDateTime.ToString( "ddd" );

                Literal lTimeCardDate = repeaterItem.FindControl( "lTimeCardDate" ) as Literal;
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

                Badge lWorkedHolidayHours = repeaterItem.FindControl( "lWorkedHolidayHours" ) as Badge;
                var workedHolidayHoursForDay = _workedHolidayHoursCache.Where( a => a.TimeCardDay == timeCardDay ).FirstOrDefault();
                lWorkedHolidayHours.Text = FormatTimeCardHours( workedHolidayHoursForDay != null ? workedHolidayHoursForDay.Hours : 0 );

                Badge lPaidVacationHours = repeaterItem.FindControl( "lPaidVacationHours" ) as Badge;
                lPaidVacationHours.Text = FormatTimeCardHours( timeCardDay.PaidVacationHours );

                Badge lPaidHolidayHours = repeaterItem.FindControl( "lPaidHolidayHours" ) as Badge;
                lPaidHolidayHours.Text = FormatTimeCardHours( timeCardDay.PaidHolidayHours );

                Badge lPaidSickHours = repeaterItem.FindControl( "lPaidSickHours" ) as Badge;
                lPaidSickHours.Text = FormatTimeCardHours( timeCardDay.PaidSickHours );

                Literal lOtherHours = repeaterItem.FindControl( "lOtherHours" ) as Literal;
                decimal totalOtherHours = timeCardDay.PaidVacationHours ?? 0 + timeCardDay.PaidHolidayHours ?? 0 + timeCardDay.PaidSickHours ?? 0;
                lOtherHours.Text = FormatTimeCardHours( totalOtherHours );

                Literal lTotalHours = repeaterItem.FindControl( "lTotalHours" ) as Literal;
                lTotalHours.Text = FormatTimeCardHours( timeCardDay.TotalWorkedDuration + totalOtherHours );

                Literal lNotes = repeaterItem.FindControl( "lNotes" ) as Literal;
                lNotes.Text = timeCardDay.Notes;

                Literal lTimeCardDayNameEdit = repeaterItem.FindControl( "lTimeCardDayNameEdit" ) as Literal;
                lTimeCardDayNameEdit.Text = lTimeCardDayName.Text;

                Literal lTimeCardDateEdit = repeaterItem.FindControl( "lTimeCardDateEdit" ) as Literal;
                lTimeCardDateEdit.Text = lTimeCardDate.Text;

                // Edit Controls
                TimePicker tpTimeIn = repeaterItem.FindControl( "tpTimeIn" ) as TimePicker;
                tpTimeIn.SelectedTime = timeCardDay.StartDateTime.TimeOfDay;

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

                ddlVacationHours.SetValue( ToNearestQtrHour( timeCardDay.PaidVacationHours ).ToString() );
                ddlHolidayHours.SetValue( ToNearestQtrHour( timeCardDay.PaidHolidayHours ).ToString() );
                ddlSickHours.SetValue( ToNearestQtrHour( timeCardDay.PaidSickHours ).ToString() );

                RockTextBox tbNotes = repeaterItem.FindControl( "tbNotes" ) as RockTextBox;
                tbNotes.Text = timeCardDay.Notes;

                // Action Controls
                LinkButton lbSave = repeaterItem.FindControl( "lbSave" ) as LinkButton;
                lbSave.CommandArgument = timeCardDay.Id.ToString();
            }
        }

        #endregion

        /// <summary>
        /// To the nearest QTR hour.
        /// </summary>
        /// <param name="hours">The hours.</param>
        /// <returns></returns>
        private static decimal? ToNearestQtrHour( decimal? hours )
        {
            return hours.HasValue ? hours - ( hours % 0.25M ) : null;
        }

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int timeCardId = PageParameter( "TimeCardId" ).AsInteger();
            hfTimeCardId.Value = timeCardId.ToString();

            var hrContext = new HrContext();
            var timeCardDayService = new TimeCardDayService( hrContext );
            var timeCardService = new TimeCardService( hrContext );
            var timeCard = timeCardService.Queryable( "TimeCardDays" ).FirstOrDefault( a => a.Id == timeCardId );
            if ( timeCard == null )
            {
                return;
            }

            lTitle.Text = "Pay Period: " + timeCard.TimeCardPayPeriod.ToString();
            hlblSubTitle.Text = timeCard.TimeCardStatus.ConvertToString();

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

            _workedRegularHoursCache = timeCard.GetRegularHours();
            _workedOvertimeHoursCache = timeCard.GetOvertimeHours();
            _workedHolidayHoursCache = timeCard.GetWorkedHolidayHours();

            // bind time card day repeater 
            rptTimeCardDay.DataSource = timeCardDayList;
            rptTimeCardDay.DataBind();

            // Actions/Submit
            List<Person> leaders = TimeCardPayPeriodService.GetLeadersForStaffPerson( hrContext, this.CurrentPersonId ?? 0 );

            ddlSubmitTo.Items.Clear();
            ddlSubmitTo.Items.Add( new ListItem() );
            ddlSubmitTo.Items.AddRange( leaders.Select( a => new ListItem( a.ToString(), a.Id.ToString() ) ).ToArray() );

            // Totals
            lTotalRegularWorked.Text = timeCard.GetRegularHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lTotalOvertimeWorked.Text = timeCard.GetOvertimeHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lTotalHolidayWorked.Text = timeCard.GetWorkedHolidayHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );

            lTotalVacationPaid.Text = timeCard.PaidVacationHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lTotalHolidayPaid.Text = timeCard.PaidHolidayHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lTotalSickPaid.Text = timeCard.PaidSickHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );

            var totalHours = timeCard.GetTotalWorkedHoursPerDay( true, true ).Sum( a => a.Hours ?? 0 )
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
                timeCardHistory.Notes = string.Empty;
                timeCardHistory.StatusPersonAliasId = this.CurrentPersonAliasId;

                timeCardHistoryService.Add( timeCardHistory );
                hrContext.SaveChanges();

                // get after saving to flesh out the virtual fields
                timeCardHistory = timeCardHistoryService.Get( timeCardHistory.Guid );
            }

            // TimeCard History grid
            var historyQry = timeCardHistoryService.Queryable().Where( a => a.TimeCardId == timeCard.Id ).OrderBy( a => a.HistoryDateTime );
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
            if ( !tpTimeIn.SelectedTime.HasValue )
            {
                tpTimeIn.ShowErrorMessage( "Start Time is required" );
                return;
            }

            var timeCardDate = timeCardDay.StartDateTime.Date;
            timeCardDay.StartDateTime = timeCardDate + tpTimeIn.SelectedTime.Value;

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

            RockDropDownList ddlSickHours = repeaterItem.FindControl( "ddlSickHours" ) as RockDropDownList;
            timeCardDay.PaidSickHours = ddlSickHours.SelectedValue.AsDecimalOrNull();

            RockTextBox tbNotes = repeaterItem.FindControl( "tbNotes" ) as RockTextBox;
            timeCardDay.Notes = tbNotes.Text;

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
                tpTimePicker.SelectedTime = TimeSpan.FromMinutes( 15 * Math.Round( tpTimePicker.SelectedTime.Value.TotalMinutes / 15 ) );

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
            // TODO
        }
    }
}