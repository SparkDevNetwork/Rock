//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using DDay.iCal;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ScheduleDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "scheduleId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "scheduleId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Schedule schedule;
            ScheduleService scheduleService = new ScheduleService();

            int scheduleId = int.Parse( hfScheduleId.Value );

            if ( scheduleId == 0 )
            {
                schedule = new Schedule();
                scheduleService.Add( schedule, CurrentPersonId );
            }
            else
            {
                schedule = scheduleService.Get( scheduleId );
            }

            schedule.Name = tbScheduleName.Text;
            schedule.Description = tbScheduleDescription.Text;
            schedule.CheckInStartTime = tpCheckInStartTime.SelectedTime;
            schedule.CheckInEndTime = tpCheckInEndTime.SelectedTime;
            schedule.iCalendarContent = sbSchedule.iCalendarContent;

            // check for duplicates
            if ( scheduleService.Queryable().Count( a => a.Name.Equals( schedule.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( schedule.Id ) ) > 0 )
            {
                nbWarningMessage.Text = WarningMessage.DuplicateFoundMessage( "name", Schedule.FriendlyTypeName );
                return;
            }

            if ( !schedule.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                scheduleService.Save( schedule, CurrentPersonId );
            } );

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the SaveSchedule event of the sbSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbSchedule_SaveSchedule( object sender, EventArgs e )
        {
            UpdateHelpText();
        }

        /// <summary>
        /// Updates the help text.
        /// </summary>
        private void UpdateHelpText()
        {
            hbSchedulePreview.Text = "<div style='white-space: pre' Font-Names='Consolas' Font-Size='9'>" + sbSchedule.iCalendarContent + "</div>";

            iCalendar calendar = iCalendar.LoadFromStream( new StringReader( sbSchedule.iCalendarContent ) ).First() as iCalendar;
            DDay.iCal.Event calendarEvent = calendar.Events[0] as Event;

            List<Occurrence> nextOccurrences = calendar.GetOccurrences( DateTime.Now.Date, DateTime.Now.Date.AddYears( 1 ) ).Take( 26 ).ToList();

            string listHtml = "<hr /><span>Occurrences Preview</span><ul>";
            foreach ( var occurrence in nextOccurrences )
            {
                if ( occurrence.Period.StartTime.Value.Date.Equals( occurrence.Period.EndTime.Value.Date ) )
                {
                    listHtml += string.Format( "<li>{0} - {1} to {2} ( {3} hours) </li>", occurrence.Period.StartTime.Value.Date.ToShortDateString(), occurrence.Period.StartTime.Value.TimeOfDay.ToTimeString(), occurrence.Period.EndTime.Value.TimeOfDay.ToTimeString(), occurrence.Period.Duration.TotalHours.ToString( "#0.00" ) );
                }
                else
                {
                    listHtml += string.Format( "<li>{0} to {1} ( {2} hours) </li>", occurrence.Period.StartTime.Value.ToString( "g" ), occurrence.Period.EndTime.Value.ToString( "g" ), occurrence.Period.Duration.TotalHours.ToString( "#0.00" ) );
                }
            }

            listHtml += string.Format( "<li>{0}</li>", "..." );
            listHtml += "</ul>";

            hbSchedulePreview.Text += listHtml;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            // return if unexpected itemKey 
            if ( itemKey != "scheduleId" )
            {
                return;
            }

            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            Schedule schedule = null;
            if ( !itemKeyValue.Equals( 0 ) )
            {
                schedule = new ScheduleService().Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( Schedule.FriendlyTypeName );
            }
            else
            {
                schedule = new Schedule { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( Schedule.FriendlyTypeName );
            }

            hfScheduleId.Value = schedule.Id.ToString();
            tbScheduleName.Text = schedule.Name;
            tbScheduleDescription.Text = schedule.Description;
            cbIsShared.Checked = schedule.IsShared;

            tpCheckInStartTime.SelectedTime = schedule.CheckInStartTime;
            tpCheckInEndTime.SelectedTime = schedule.CheckInEndTime;
            sbSchedule.iCalendarContent = schedule.iCalendarContent;
            UpdateHelpText();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Schedule.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( Schedule.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbScheduleName.ReadOnly = readOnly;
            btnSave.Visible = !readOnly;
        }

        #endregion
    }
}