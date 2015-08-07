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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Attendance Detail" )]
    [Category( "Groups" )]
    [Description( "Lists the group members for a specific occurrence datetime and allows selecting if they attended or not." )]

    [BooleanField( "Allow Add", "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?", true, "", 0 )]
    [BooleanField( "Allow Adding Person", "Should block support adding new attendee ( Requires that person has rights to search for new person )?", false, "", 1 )]
    [WorkflowTypeField( "Workflow", "An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.", false, false, "", "", 2 )]
    public partial class GroupAttendanceDetail : RockBlock
    {
        #region Private Variables

        private RockContext _rockContext = null;
        private Group _group = null;
        private bool _canEdit = false;
        private bool _allowAdd = false;
        private ScheduleOccurrence _occurrence = null;
        private List<GroupAttendanceAttendee> _attendees;

        #endregion

        #region Control Methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _attendees = ViewState["Attendees"] as List<GroupAttendanceAttendee>;
        }
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RegisterScript();

            _rockContext = new RockContext();

            int groupId = PageParameter( "GroupId" ).AsInteger();
            _group = new GroupService( _rockContext )
                .Queryable( "GroupType,Schedule" ).AsNoTracking()
                .FirstOrDefault( g => g.Id == groupId );

            if ( _group != null && _group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                _canEdit = true;
            }

            _allowAdd = GetAttributeValue( "AllowAdd" ).AsBoolean();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            _occurrence = GetOccurrence();

            if ( !Page.IsPostBack )
            {
                pnlDetails.Visible = _canEdit;

                if ( _canEdit )
                {
                    BindLocations();
                    ShowDetails();
                }
                else
                {
                    nbNotice.Heading = "Sorry";
                    nbNotice.Text = "<p>You're not authorized to update the attendance for the selected group.</p>";
                    nbNotice.NotificationBoxType = NotificationBoxType.Danger;
                    nbNotice.Visible = true;
                }
            }
            else
            {
                if ( _attendees != null )
                {
                    foreach ( var item in lvMembers.Items )
                    {
                        var hfMember = item.FindControl( "hfMember" ) as HiddenField;
                        var cbMember = item.FindControl( "cbMember" ) as CheckBox;

                        if ( hfMember != null && cbMember != null )
                        {
                            int personId = hfMember.ValueAsInt();

                            var attendance = _attendees.Where( a => a.PersonId == personId ).FirstOrDefault();
                            if ( attendance != null )
                            {
                                attendance.Attended = cbMember.Checked;
                            }
                        }
                    }
                }
            }
        }

        protected override object SaveViewState()
        {
            ViewState["Attendees"] = _attendees;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( _group != null && _occurrence != null )
            {
                var rockContext = new RockContext();
                var attendanceService = new AttendanceService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var locationService = new LocationService( rockContext );

                DateTime startDate = _occurrence.Date;
                DateTime endDate = _occurrence.Date.AddDays( 1 );

                var existingAttendees = attendanceService
                    .Queryable( "PersonAlias" )
                    .Where( a =>
                        a.GroupId == _group.Id &&
                        a.LocationId == _occurrence.LocationId &&
                        a.ScheduleId == _occurrence.ScheduleId &&
                        a.StartDateTime >= startDate &&
                        a.StartDateTime < endDate );

                // If did not meet was selected and this was a manually entered occurrence (not based on a schedule/location)
                // then just delete all the attendance records instead of tracking a 'did not meet' value
                if ( cbDidNotMeet.Checked && !_occurrence.ScheduleId.HasValue )
                {
                    foreach ( var attendance in existingAttendees )
                    {
                        attendanceService.Delete( attendance );
                    }
                }
                else
                {
                    if ( cbDidNotMeet.Checked )
                    {
                        // If the occurrence is based on a schedule, set the did not meet flags
                        foreach ( var attendance in existingAttendees )
                        {
                            attendance.DidAttend = null;
                            attendance.DidNotOccur = true;
                        }
                    }

                    foreach ( var attendee in _attendees )
                    {
                        var attendance = existingAttendees
                            .Where( a => a.PersonAlias.PersonId == attendee.PersonId )
                            .FirstOrDefault();

                        if ( attendance == null )
                        {
                            int? personAliasId = personAliasService.GetPrimaryAliasId( attendee.PersonId );
                            if ( personAliasId.HasValue )
                            {
                                attendance = new Attendance();
                                attendance.GroupId = _group.Id;
                                attendance.ScheduleId = _group.ScheduleId;
                                attendance.PersonAliasId = personAliasId;
                                attendance.StartDateTime = _occurrence.Date;
                                attendance.LocationId = _occurrence.LocationId;
                                attendance.CampusId = locationService.GetCampusIdForLocation( _occurrence.LocationId );
                                attendance.ScheduleId = _occurrence.ScheduleId;
                                attendanceService.Add( attendance );
                            }
                        }

                        if ( attendance != null )
                        {
                            if ( cbDidNotMeet.Checked )
                            {
                                attendance.DidAttend = null;
                                attendance.DidNotOccur = true;
                            }
                            else
                            {
                                attendance.DidAttend = attendee.Attended;
                                attendance.DidNotOccur = null;
                            }
                        }
                    }
                }

                rockContext.SaveChanges();

                WorkflowType workflowType = null;
                Guid? workflowTypeGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
                if ( workflowTypeGuid.HasValue )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
                    if ( workflowType != null )
                    {
                        try
                        {
                            var workflow = Workflow.Activate( workflowType, _group.Name );

                            workflow.SetAttributeValue( "StartDateTime", _occurrence.Date.ToString( "o" ) );
                            workflow.SetAttributeValue( "Schedule", _group.Schedule.Guid.ToString() );

                            List<string> workflowErrors;
                            new WorkflowService( rockContext ).Process( workflow, _group, out workflowErrors );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex, this.Context );
                        }
                    }
                }

                NavigateToParentPage( new Dictionary<string, string> { { "GroupId", _group.Id.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( _group != null )
            {
                NavigateToParentPage( new Dictionary<string, string> { { "GroupId", _group.Id.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindSchedules( ddlLocation.SelectedValueAsInt() );
        }

        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppAddPerson.PersonId.HasValue )
            {
                if ( !_attendees.Any( a => a.PersonId == ppAddPerson.PersonId.Value ) )
                {
                    var Person = new PersonService( new RockContext() ).Get( ppAddPerson.PersonId.Value );
                    if ( Person != null )
                    {
                        var attendee = new GroupAttendanceAttendee();
                        attendee.PersonId = Person.Id;
                        attendee.NickName = Person.NickName;
                        attendee.LastName = Person.LastName;
                        attendee.Attended = true;
                        _attendees.Add( attendee );
                        BindAttendees();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvPendingMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvPendingMembers_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            if ( _group != null && e.CommandName == "Add" )
            {
                int personId = e.CommandArgument.ToString().AsInteger();

                var rockContext = new RockContext();

                foreach ( var groupMember in new GroupMemberService( rockContext )
                    .GetByGroupIdAndPersonId( _group.Id, personId ) )
                {
                    if ( groupMember.GroupMemberStatus == GroupMemberStatus.Pending )
                    {
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    }
                }

                rockContext.SaveChanges();

                ShowDetails();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the occurrence items.
        /// </summary>
        private ScheduleOccurrence GetOccurrence()
        {
            DateTime? occurrenceDate = PageParameter( "Date" ).AsDateTime();

            List<int> locationIds = new List<int>();
            int? locationId = PageParameter( "LocationId" ).AsIntegerOrNull();
            if ( locationId.HasValue )
            {
                locationIds.Add( locationId.Value );
            }

            List<int> scheduleIds = new List<int>();
            int? scheduleId = PageParameter( "ScheduleId" ).AsIntegerOrNull();
            if ( scheduleId.HasValue )
            {
                scheduleIds.Add( scheduleId.Value );
            }

            if ( Page.IsPostBack && _allowAdd )
            {
                if ( !occurrenceDate.HasValue && dpOccurrenceDate.SelectedDate.HasValue )
                {
                    occurrenceDate = dpOccurrenceDate.SelectedDate;
                }

                if ( !locationIds.Any() && ddlLocation.SelectedValueAsInt().HasValue )
                {
                    locationIds.Add( ddlLocation.SelectedValueAsInt().Value );
                }

                if ( !scheduleIds.Any() && ddlSchedule.SelectedValueAsInt().HasValue )
                {
                    scheduleIds.Add( ddlSchedule.SelectedValueAsInt().Value );
                }
            }

            if ( occurrenceDate.HasValue )
            {
                // Try to find the selected occurrence based on group's schedule
                if ( _group != null )
                {
                    lHeading.Text = _group.Name + " Attendance";

                    // Get all the occurrences for this group, and load the attendance so we can show Attendance Count
                    var occurrence = new ScheduleService( _rockContext )
                        .GetGroupOccurrences( _group, occurrenceDate.Value.Date, occurrenceDate.Value.AddDays( 1 ), locationIds, scheduleIds, true )
                        .OrderBy( o => o.Date )
                        .FirstOrDefault();

                    if ( occurrence != null )
                    {
                        return occurrence;
                    }
                }

                // If an occurrence date was included, but no occurrence was found with that date, and new 
                // occurrences can be added, create a new one
                if ( _allowAdd )
                {
                    return new ScheduleOccurrence( occurrenceDate.Value.Date, occurrenceDate.Value.TimeOfDay, scheduleId, string.Empty, locationId );
                }
            }

            return null;
        }

        /// <summary>
        /// Loads the dropdowns.
        /// </summary>
        private void BindLocations()
        {
            var locations = new Dictionary<int, string> { { 0, "" } };

            if ( _group != null )
            {
                var locationPaths = new Dictionary<int, string>();
                var locationService = new LocationService( _rockContext );

                foreach ( var location in _group.GroupLocations
                    .Where( l =>
                        l.Location.Name != null &&
                        l.Location.Name != "" )
                    .Select( l => l.Location ) )
                {
                    // Get location path
                    string parentLocationPath = string.Empty;
                    if ( location.ParentLocationId.HasValue )
                    {
                        var locId = location.ParentLocationId.Value;
                        if ( !locationPaths.ContainsKey( locId ) )
                        {
                            locationPaths.Add( locId, locationService.GetPath( locId ) );
                        }
                        parentLocationPath = locationPaths[locId];
                    }

                    locations.Add( location.Id, new List<string> { parentLocationPath, location.Name }.AsDelimited( " > " ) );
                }
            }

            if ( locations.Any() )
            {
                ddlLocation.DataSource = locations;
                ddlLocation.DataBind();
            }
        }

        private void BindSchedules( int? locationId )
        {
            var schedules = new Dictionary<int, string> { { 0, "" } };

            if ( _group != null && locationId.HasValue )
            {
                _group.GroupLocations
                    .Where( l => l.LocationId == locationId.Value )
                    .SelectMany( l => l.Schedules )
                    .OrderBy( s => s.Name )
                    .ToList()
                    .ForEach( s => schedules.AddOrIgnore( s.Id, s.Name ) );
            }

            if ( schedules.Any() )
            {
                ddlSchedule.DataSource = schedules;
                ddlSchedule.DataBind();
            }

            ddlSchedule.Visible = ddlSchedule.Items.Count > 1;

        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void ShowDetails()
        {
            bool existingOccurrence = _occurrence != null;

            if ( !existingOccurrence && !_allowAdd )
            {
                nbNotice.Heading = "No Occurrences";
                nbNotice.Text = "<p>There are currently not any active occurrences for selected group to take attendance for.</p>";
                nbNotice.NotificationBoxType = NotificationBoxType.Warning;
                nbNotice.Visible = true;

                pnlDetails.Visible = false;
            }
            else
            {
                if ( existingOccurrence )
                {
                    lOccurrenceDate.Visible = true;
                    lOccurrenceDate.Text = _occurrence.Date.ToShortDateString();
                    dpOccurrenceDate.Visible = false;

                    if ( _occurrence.LocationId.HasValue )
                    {
                        lLocation.Visible = true;
                        lLocation.Text = new LocationService( _rockContext ).GetPath( _occurrence.LocationId.Value );
                    }
                    else
                    {
                        lLocation.Visible = false;
                    }
                    ddlLocation.Visible = false;

                    lSchedule.Visible = !string.IsNullOrWhiteSpace( _occurrence.ScheduleName );
                    lSchedule.Text = _occurrence.ScheduleName;
                    ddlSchedule.Visible = false;

                    lDidAttendCount.Visible = true;
                    lDidAttendCount.Text = _occurrence.DidAttendCount.ToString();
                }
                else
                {
                    lOccurrenceDate.Visible = false;
                    dpOccurrenceDate.Visible = true;
                    dpOccurrenceDate.SelectedDate = RockDateTime.Today;

                    lLocation.Visible = false;
                    ddlLocation.Visible = ddlLocation.Items.Count > 1;

                    lSchedule.Visible = false;
                    ddlSchedule.Visible = ddlSchedule.Items.Count > 1;

                    lDidAttendCount.Visible = false;
                }

                lMembers.Text = _group.GroupType.GroupMemberTerm.Pluralize();
                lPendingMembers.Text = "Pending " + lMembers.Text;

                List<int> attendedIds = new List<int>();

                // Load the attendance for the selected occurrence
                if ( existingOccurrence )
                {
                    cbDidNotMeet.Checked = _occurrence.DidNotOccur;

                    // Get the list of people who attended
                    attendedIds = new ScheduleService( _rockContext ).GetAttendance( _group, _occurrence )
                        .Where( a => a.DidAttend.HasValue && a.DidAttend.Value )
                        .Select( a => a.PersonAlias.PersonId )
                        .Distinct()
                        .ToList();
                }

                ppAddPerson.Visible = GetAttributeValue( "AllowAddingPerson" ).AsBoolean(); 

                // Get the group members
                var groupMemberService = new GroupMemberService( _rockContext );

                // Add any existing active members not on that list
                var unattendedIds = groupMemberService
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.GroupId == _group.Id &&
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        !attendedIds.Contains( m.PersonId ) )
                    .Select( m => m.PersonId )
                    .ToList();

                // Bind the attendance roster
                _attendees = new PersonService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( p => attendedIds.Contains( p.Id ) || unattendedIds.Contains( p.Id ) )
                    .Select( p => new GroupAttendanceAttendee()
                    {
                        PersonId = p.Id,
                        NickName = p.NickName,
                        LastName = p.LastName,
                        Attended = attendedIds.Contains( p.Id )
                    } )
                    .ToList();
                BindAttendees();

                // Bind the pending members
                var pendingMembers = groupMemberService
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.GroupId == _group.Id &&
                        m.GroupMemberStatus == GroupMemberStatus.Pending )
                    .OrderBy( m => m.Person.LastName )
                    .ThenBy( m => m.Person.NickName )
                    .Select( m => new
                    {
                        Id = m.PersonId,
                        FullName = m.Person.NickName + " " + m.Person.LastName
                    } )
                    .ToList();

                pnlPendingMembers.Visible = pendingMembers.Any();
                lvPendingMembers.DataSource = pendingMembers;
                lvPendingMembers.DataBind();
            }

        }

        private void BindAttendees()
        {
            lvMembers.DataSource = _attendees.OrderBy( a => a.LastName ).ThenBy( a => a.NickName ).ToList();
            lvMembers.DataBind();

            ppAddPerson.PersonId = Rock.Constants.None.Id;
            ppAddPerson.PersonName = "Add New Attendee";
        }

        protected void RegisterScript()
        {
            string script = string.Format( @"

    Sys.Application.add_load(function () {{

        if ($('#{0}').is(':checked')) {{
            $('div.js-roster').hide();
        }}

        $('#{0}').click(function () {{
            if ($(this).is(':checked')) {{
                $('div.js-roster').hide('fast');
            }} else {{
                $('div.js-roster').show('fast');
            }}
        }});

        $('.js-add-member').click(function ( e ) {{
            e.preventDefault();
            var $a = $(this);
            var memberName = $(this).parent().find('span').html();
            Rock.dialogs.confirm('Add ' + memberName + ' to your group?', function (result) {{
                if (result) {{
                    window.location = $a.prop('href');                    
                }}
            }});
        }});

    }});

", cbDidNotMeet.ClientID );

            ScriptManager.RegisterStartupScript( cbDidNotMeet, cbDidNotMeet.GetType(), "group-attendance-detail", script, true );
        }

        #endregion

        #region Helper Classes

        [Serializable]
        public class GroupAttendanceAttendee
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the name of the nick.
            /// </summary>
            /// <value>
            /// The name of the nick.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName
            {
                get { return NickName + " " + LastName; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="GroupAttendanceAttendee"/> is attended.
            /// </summary>
            /// <value>
            ///   <c>true</c> if attended; otherwise, <c>false</c>.
            /// </value>
            public bool Attended { get; set; }
        }

        #endregion
    }
}