using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using com.ccvonline.Hr.Data;
using com.ccvonline.Hr.Model;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_ccvonline.Hr
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Time Card Detail" )]
    [Category( "CCV > Time Card" )]
    [Description( "Displays the details of a time card." )]

    [WorkflowTypeField( "Workflow", "The workflow to activate when a TimeCard is submitted.", false, false, order: 2 )]

    // NOTE: This Attributes should also be on TimeCardEmployeeCardList, TimeCardPayPeriodList and TimeCardDetail
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Can Approve Timecard Attribute", "Select the Person Attribute that is used to determine if a person can approve timecards, even if they aren't a leader.", order: 3 )]
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

                // only show the Save button if in edit mode (this current person can edit this time card)
                lbSave.Visible = hfEditMode.Value.AsBooleanOrNull() ?? false;

                // only enable the EditRow controls if in edit mode (this current person can edit this time card)
                Panel pnlEditRow = repeaterItem.FindControl( "pnlEditRow" ) as Panel;
                pnlEditRow.Enabled = hfEditMode.Value.AsBooleanOrNull() ?? false;

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
                nbMessage.Text = "A valid TimeCardId must be specified";
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Visible = true;
                pnlDetails.Visible = false;
                return;
            }

            AttributeCache canApproveTimecardAttribute = AttributeCache.Read( this.GetAttributeValue( "CanApproveTimecardAttribute" ).AsGuid() );

            // make sure the current person is the timecard.person or is a leader of the timecard.person
            List<Person> leaders = TimeCardPayPeriodService.GetApproversForStaffPerson( hrContext, this.CurrentPersonId ?? 0, canApproveTimecardAttribute != null ? canApproveTimecardAttribute.Key : null );

            bool editMode = false;
            nbMessage.Visible = false;
            pnlDetails.Visible = true;
            pnlApproverActions.Visible = false;
            int? timeCardPersonId = timeCard.PersonAlias != null ? timeCard.PersonAlias.PersonId : (int?)null;
            if ( timeCardPersonId == this.CurrentPersonId )
            {
                editMode = true;
            }
            else
            {
                if ( !leaders.Any( a => a.Id == this.CurrentPersonId ) )
                {
                    // if the currentPersonId is neither the TimeCard person or a leader of the timecard person, don't let them see it
                    nbMessage.Visible = true;
                    nbMessage.Text = "You are only allowed to view your timecards or timecards of person that report to you.";
                    nbMessage.NotificationBoxType = NotificationBoxType.Warning;
                    pnlDetails.Visible = false;
                    return;
                }
                else
                {
                    // if the current person is a leader of the timecard.person, enable the Approve button if is has been submitted.
                    pnlApproverActions.Visible = timeCard.TimeCardStatus == TimeCardStatus.Submitted;
                }
            }

            hfEditMode.Value = editMode.ToTrueFalse();

            // only show the Submit panel if in edit mode
            pnlPersonActions.Visible = editMode;

            lTimeCardPersonName.Text = string.Format( "{0}", timeCard.PersonAlias );
            lTitle.Text = "Pay Period: " + timeCard.TimeCardPayPeriod.ToString();
            hlblSubTitle.Text = timeCard.GetStatusText();

            switch (timeCard.TimeCardStatus)
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

            _workedRegularHoursCache = timeCard.GetRegularHours();
            _workedOvertimeHoursCache = timeCard.GetOvertimeHours();
            _workedHolidayHoursCache = timeCard.GetWorkedHolidayHours();

            // bind time card day repeater 
            rptTimeCardDay.DataSource = timeCardDayList;
            rptTimeCardDay.DataBind();

            // Actions/Submit
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
            var hrContext = new HrContext();

            int timeCardId = hfTimeCardId.Value.AsInteger();
            var timeCardService = new TimeCardService( hrContext );
            var timeCard = timeCardService.Get( timeCardId );
            if ( timeCard == null )
            {
                return;
            }

            timeCard.TimeCardStatus = TimeCardStatus.Submitted;
            timeCard.SubmittedToPersonAliasId = ddlSubmitTo.SelectedValue.AsIntegerOrNull();
            var timeCardHistoryService = new TimeCardHistoryService( hrContext );
            var timeCardHistory = new TimeCardHistory();
            timeCardHistory.TimeCardId = timeCard.Id;
            timeCardHistory.TimeCardStatus = timeCard.TimeCardStatus;
            timeCardHistory.StatusPersonAliasId = ddlSubmitTo.SelectedValue.AsIntegerOrNull();
            timeCardHistory.HistoryDateTime = RockDateTime.Now;
            timeCardHistory.Notes = string.Empty;
            timeCardHistoryService.Add( timeCardHistory );

            hrContext.SaveChanges();

            // Launch the Workflow after timecard is marked submitted
            Guid? workflowTypeGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                var workflowTypeService = new WorkflowTypeService( hrContext );
                var workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
                if ( workflowType != null )
                {
                    var workflowName = string.Format( "{0} Time Card for {1}", timeCard.TimeCardPayPeriod, timeCard.PersonAlias.Person );
                    var workflow = Workflow.Activate( workflowType, workflowName );

                    List<string> workflowErrors;
                    if ( workflow.Process( hrContext, timeCard, out workflowErrors ) )
                    {
                        if ( workflow.IsPersisted || workflowType.IsPersisted )
                        {
                            var workflowService = new Rock.Model.WorkflowService( hrContext );
                            workflowService.Add( workflow );
                            hrContext.SaveChanges();
                        }
                    }
                }
            }

            ShowDetail();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gHistory_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            TimeCardHistory timeCardHistory = e.Row.DataItem as TimeCardHistory;
            if ( timeCardHistory != null )
            {
                Literal lStatusText = e.Row.FindControl( "lStatusText" ) as Literal;
                lStatusText.Text = timeCardHistory.GetStatusText();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApprove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApprove_Click( object sender, EventArgs e )
        {
            var hrContext = new HrContext();

            int timeCardId = hfTimeCardId.Value.AsInteger();
            var timeCardService = new TimeCardService( hrContext );
            var timeCard = timeCardService.Get( timeCardId );
            if ( timeCard == null )
            {
                return;
            }

            timeCard.TimeCardStatus = TimeCardStatus.Approved;
            timeCard.ApprovedByPersonAliasId = this.CurrentPersonAliasId;
            var timeCardHistoryService = new TimeCardHistoryService( hrContext );
            var timeCardHistory = new TimeCardHistory();
            timeCardHistory.TimeCardId = timeCard.Id;
            timeCardHistory.TimeCardStatus = timeCard.TimeCardStatus;
            timeCardHistory.StatusPersonAliasId = timeCard.ApprovedByPersonAliasId;
            timeCardHistory.HistoryDateTime = RockDateTime.Now;
            timeCardHistory.Notes = string.Empty;
            timeCardHistoryService.Add( timeCardHistory );

            hrContext.SaveChanges();

            ShowDetail();
        }
    }
}