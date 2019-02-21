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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName( "Rapid Attendance Entry" )]
    [Category( "Check-in" )]
    [Description( "Provides a way to manually enter attendance for a large group of people in an efficient manner." )]

    [GroupField( "Parent Group", "Select the parent group whose immediate children will be displayed as options to take attendance for.", required: true, order: 0 )]
    [BooleanField( "Include Parent Group", "If true then the parent group will be included as an option in addition to its children.", false, order: 1 )]
    [BooleanField( "Default Show Current Attendees", "Should the Current Attendees grid be visible by default. When the grid is enabled performance will be reduced.", false, order: 1 )]
    public partial class RapidAttendanceEntry : RockBlock
    {
        #region Base Method Overrides

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
            gAttendees.GridRebind += gAttendees_GridRebind;
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                var parentGroupId = GetAttributeValue( "ParentGroup" ).AsGuid();
                if ( parentGroupId != default( Guid ) )
                {
                    ShowDetails();
                }
                else
                {
                    nbWarning.Text = "Please configure the block's settings with a default group.";
                }
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Show all details. This also clears any existing selections the user may have made.
        /// </summary>
        private void ShowDetails()
        {
            var groups = new List<Group>();
            var rockContext = new RockContext();
            var parentGroup = new GroupService( rockContext ).Get( GetAttributeValue( "ParentGroup" ).AsGuid() );

            //
            // Clear existing values.
            //
            dpAttendanceDate.SelectedDate = null;
            ppAttendee.SetValue( null );

            //
            // Clear group picker.
            //
            pnlGroupPicker.Visible = false;
            ddlGroup.Items.Clear();
            ddlGroup.Items.Add( new ListItem() );

            //
            // Get the list of groups to be displayed in the picker.
            //
            if ( parentGroup != null )
            {
                groups = parentGroup.Groups.Where( g => g.IsActive ).OrderBy( g => g.Order ).ToList();

                if ( GetAttributeValue( "IncludeParentGroup" ).AsBoolean( false ) )
                {
                    groups.Insert( 0, parentGroup );
                }
            }

            //
            // Add all the groups to the drop down list.
            //
            foreach ( var group in groups )
            {
                ddlGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
            }

            //
            // If there is only one group then select it, otherwise show the picker and let the user select.
            //
            if ( groups.Count == 1 )
            {
                ddlGroup.SelectedIndex = 1;
            }
            else
            {
                pnlGroupPicker.Visible = true;
            }

            UpdateLocations();

            //
            // Set default state of the attendees grid.
            //
            pnlCurrentAttendees.Visible = cbShowCurrent.Checked = GetAttributeValue( "DefaultShowCurrentAttendees" ).AsBoolean( false );
            BindAttendees();
        }

        /// <summary>
        /// Update the locations drop down to match the valid locations for the currently
        /// selected group.
        /// </summary>
        private void UpdateLocations()
        {
            //
            // Clear the location picker.
            //
            pnlLocationPicker.Visible = false;
            ddlLocation.Items.Clear();
            ddlLocation.Items.Add( new ListItem() );

            if ( !string.IsNullOrWhiteSpace( ddlGroup.SelectedValue ) )
            {
                var group = new GroupService( new RockContext() ).Get( ddlGroup.SelectedValue.AsInteger() );
                var groupLocations = group.GroupLocations;

                //
                // Add all the locations to the drop down list.
                //
                foreach ( var groupLocation in groupLocations )
                {
                    ddlLocation.Items.Add( new ListItem( groupLocation.Location.Name, groupLocation.Id.ToString() ) );
                }

                //
                // If there is only one location then select it, otherwise show the picker and let the user select.
                //
                if ( groupLocations.Count == 1 )
                {
                    ddlLocation.SelectedIndex = 1;
                }
                else
                {
                    pnlLocationPicker.Visible = true;
                }
            }

            UpdateSchedules();
        }

        /// <summary>
        /// Update the schedules drop down to match the valid schedules for the currently
        /// selected group locations.
        /// </summary>
        private void UpdateSchedules()
        {
            //
            // Clear the schedule picker.
            //
            pnlSchedulePicker.Visible = false;
            ddlSchedule.Items.Clear();
            ddlSchedule.Items.Add( new ListItem() );

            if ( !string.IsNullOrWhiteSpace( ddlLocation.SelectedValue ) )
            {
                var rockContext = new RockContext();
                var group = new GroupService( rockContext ).Get( ddlGroup.SelectedValue.AsInteger() );
                var groupLocation = new GroupLocationService( rockContext ).Get( ddlLocation.SelectedValue.AsInteger() );
                var schedules = groupLocation.Schedules.ToList();

                // TODO: Should keep?
                if ( group.Schedule != null )
                {
                    schedules.Add( group.Schedule );
                }

                //
                // Add all the schedules to the drop down list.
                //
                foreach ( var schedule in schedules )
                {
                    ddlSchedule.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
                }

                //
                // If there is only one schedule then select it, otherwise show the picker and let the user select.
                //
                if ( schedules.Count == 1 )
                {
                    ddlSchedule.SelectedIndex = 1;
                }
                else
                {
                    pnlSchedulePicker.Visible = true;
                }

                BindAttendees();
            }
        }

        /// <summary>
        /// Bind the grid of current attendees. Can be called any time, internally checks if
        /// all user-entered values are correct and binds an empty result set if not.
        /// </summary>
        private void BindAttendees()
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var group = new GroupService( rockContext ).Get( ddlGroup.SelectedValue.AsInteger() );
            var groupLocationId = ddlLocation.SelectedValue.AsIntegerOrNull();
            var scheduleId = ddlSchedule.SelectedValue.AsIntegerOrNull();
            var dateTime = dpAttendanceDate.SelectedDate;
            IEnumerable<Attendance> attendance = new List<Attendance>();

            if ( group != null && groupLocationId != null && scheduleId != null && dateTime != null )
            {
                var groupLocation = new GroupLocationService( rockContext ).Get( groupLocationId.Value );

                //
                // Check for existing attendance records.
                //
                attendance = attendanceService.Queryable()
                    .Where( a => 
                        a.DidAttend == true && 
                        a.Occurrence.GroupId == group.Id && 
                        a.Occurrence.OccurrenceDate == dateTime.Value && 
                        a.Occurrence.LocationId == groupLocation.LocationId && 
                        a.Occurrence.ScheduleId == scheduleId )
                    .OrderBy( a => a.PersonAlias.Person.LastName )
                    .ThenBy( a => a.PersonAlias.Person.FirstName );

                hlCurrentCount.Text = string.Format( "Current Attendance: {0}", attendance.Count() );
            }

            if ( cbShowCurrent.Checked )
            {
                gAttendees.DataSource = attendance.ToList();
            }
            else
            {
                gAttendees.DataSource = null;
            }

            gAttendees.DataBind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var group = new GroupService( rockContext ).Get( ddlGroup.SelectedValue.AsInteger() );
            var person = new PersonService( rockContext ).Get( ppAttendee.PersonId.Value );
            var scheduleId = ddlSchedule.SelectedValue.AsIntegerOrNull();
            var dateTime = dpAttendanceDate.SelectedDate.Value;
            var groupLocation = new GroupLocationService( rockContext ).Get( ddlLocation.SelectedValue.AsInteger() );

            attendanceService.AddOrUpdate( person.PrimaryAliasId.Value, dateTime, group.Id, groupLocation.LocationId, scheduleId, group.CampusId );

            //
            // If the user is using the activate and save button then mark the
            // person as active too.
            //
            if ( sender == btnSaveActivate )
            {
                person.RecordStatusValueId = new DefinedValueService( rockContext ).Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            }

            rockContext.SaveChanges();

            //
            // Flush the attendance cache.
            //
            Rock.CheckIn.KioskLocationAttendance.Remove( groupLocation.LocationId );

            nbAttended.Text = string.Format( "{0} has been marked attended.", person.FullName );

            //
            // Clear the person picker and open it so it's ready to go.
            //
            ppAttendee.SetValue( null );
            btnSaveActivate.Visible = false;
            nbInactivePerson.Visible = false;

            //
            // Autoexpand the person picker for next entry.
            //
            var personPickerStartupScript = @"(function () {
                var runOnce = true;
                Sys.Application.add_load(function () {
                    if ( runOnce ) {
                        var personPicker = $('.js-attendee');
                        var currentPerson = personPicker.find('.picker-selectedperson').html();
                        if (currentPerson != null && currentPerson.length == 0) {
                            $(personPicker).find('a.picker-label').trigger('click');
                        }
                        runOnce = false;
                    }
                })
            })();";
            ScriptManager.RegisterStartupScript( btnSave, this.GetType(), "StartupScript", personPickerStartupScript, true );

            BindAttendees();
        }

        /// <summary>
        /// Handles the user changed the checked state of the cbShowCurrent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbShowCurrent_CheckedChanged( object sender, EventArgs e )
        {
            pnlCurrentAttendees.Visible = cbShowCurrent.Checked;

            if ( cbShowCurrent.Checked )
            {
                BindAttendees();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindAttendees();
        }

        /// <summary>
        /// Handles the user clicking the delete button in the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAttendeesDelete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var attendance = attendanceService.Get( e.RowKeyId );

            attendance.DidAttend = false;
            rockContext.SaveChanges();

            if ( attendance.Occurrence.LocationId != null )
            {
                Rock.CheckIn.KioskLocationAttendance.Remove( attendance.Occurrence.LocationId.Value );
            }

            nbAttended.Text = string.Format( "{0} has been removed from attendance.", attendance.PersonAlias.Person.FullName );

            BindAttendees();
        }

        /// <summary>
        /// Handles the user selecting an item of the ddlGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateLocations();
        }

        /// <summary>
        /// Handles the user selecting an item of the ddlLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateSchedules();
        }

        /// <summary>
        /// Handles the user selecting an item of the ddlSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindAttendees();
        }

        /// <summary>
        /// Handles the user changing the date in the dpAttendanceDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dpAttendanceDate_TextChanged( object sender, EventArgs e )
        {
            BindAttendees();
        }

        /// <summary>
        /// Handles the user changing the selected person in the ppAttendee control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAttendee_SelectPerson( object sender, EventArgs e )
        {
            nbAttended.Text = string.Empty;
            nbInactivePerson.Visible = false;
            btnSaveActivate.Visible = false;

            if ( ppAttendee.PersonId != null )
            {
                var person = new PersonService( new RockContext() ).Get( ppAttendee.PersonId.Value );

                if ( person != null && person.RecordStatusValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() )
                {
                    nbInactivePerson.Visible = true;
                    btnSaveActivate.Visible = true;
                }
            }
        }

        #endregion
    }
}