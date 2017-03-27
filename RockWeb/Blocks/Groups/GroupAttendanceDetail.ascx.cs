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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Attendance Detail" )]
    [Category( "Groups" )]
    [Description( "Lists the group members for a specific occurrence datetime and allows selecting if they attended or not." )]

    [BooleanField( "Allow Add", "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?", true, "", 0 )]
    [BooleanField( "Allow Adding Person", "Should block support adding new attendee ( Requires that person has rights to search for new person )?", false, "", 1 )]
    [BooleanField( "Allow Campus Filter", "Should block add an option to allow filtering people and attendance counts by campus?", false, "", 2 )]
    [WorkflowTypeField( "Workflow", "An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.", false, false, "", "", 3 )]
    [MergeTemplateField( "Attendance Roster Template", "", false, "", "", 4 )]
    [CodeEditorField( "Lava Template", "An optional lava template to appear next to each person in the list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, "", "", 5 )]

    public partial class GroupAttendanceDetail : RockBlock
    {
        #region Private Variables

        private RockContext _rockContext = null;
        private Group _group = null;
        private bool _canEdit = false;
        private bool _allowAdd = false;
        private bool _allowCampusFilter = false;
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
                lHeading.Text = _group.Name + " Attendance";
                _canEdit = true;
            }

            _allowAdd = GetAttributeValue( "AllowAdd" ).AsBoolean();

            _allowCampusFilter = GetAttributeValue( "AllowCampusFilter" ).AsBoolean();
            bddlCampus.Visible = _allowCampusFilter;
            if ( _allowCampusFilter )
            {
                bddlCampus.DataSource = CampusCache.All();
                bddlCampus.DataBind();
                bddlCampus.Items.Insert( 0, new ListItem( "All Campuses", "0" ) );
            }
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
                    if ( _allowCampusFilter )
                    {
                        var campus = CampusCache.Read( GetBlockUserPreference( "Campus" ).AsInteger() );
                        if ( campus != null )
                        {
                            bddlCampus.Title = campus.Name;
                            bddlCampus.SetValue( campus.Id );
                        }
                    }

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

                bool dateAdjusted = false;

                DateTime startDate = _occurrence.Date;

                // If this is a manuall entered occurrence, check to see if date was changed
                if ( !_occurrence.ScheduleId.HasValue )
                {
                    DateTime? originalDate = PageParameter( "Date" ).AsDateTime();
                    if ( originalDate.HasValue && originalDate.Value.Date != startDate.Date )
                    {
                        startDate = originalDate.Value.Date;
                        dateAdjusted = true;
                    }
                }
                DateTime endDate = startDate.AddDays( 1 );

                var existingAttendees = attendanceService
                    .Queryable( "PersonAlias" )
                    .Where( a =>
                        a.GroupId == _group.Id &&
                        a.LocationId == _occurrence.LocationId &&
                        a.ScheduleId == _occurrence.ScheduleId &&
                        a.StartDateTime >= startDate &&
                        a.StartDateTime < endDate );

                if ( dateAdjusted )
                {
                    foreach ( var attendee in existingAttendees )
                    {
                        attendee.StartDateTime = _occurrence.Date;
                    }
                }

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
                    int? campusId = locationService.GetCampusIdForLocation( _occurrence.LocationId );
                    if ( !campusId.HasValue )
                    {
                        campusId = _group.CampusId;
                    }
                    if ( !campusId.HasValue && _allowCampusFilter )
                    {
                        var campus = CampusCache.Read( bddlCampus.SelectedValueAsInt() ?? 0 );
                        if ( campus != null )
                        {
                            campusId = campus.Id;
                        }
                    }

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
                                attendance.StartDateTime = _occurrence.Date.Date.Add( _occurrence.StartTime );
                                attendance.LocationId = _occurrence.LocationId;
                                attendance.CampusId = campusId;
                                attendance.ScheduleId = _occurrence.ScheduleId;

                                // check that the attendance record is valid
                                cvAttendance.IsValid = attendance.IsValid;
                                if ( !cvAttendance.IsValid )
                                {
                                    cvAttendance.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                                    return;
                                }

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

                if ( _occurrence.LocationId.HasValue )
                {
                    Rock.CheckIn.KioskLocationAttendance.Flush( _occurrence.LocationId.Value );
                }

                rockContext.SaveChanges();

                WorkflowType workflowType = null;
                Guid? workflowTypeGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
                if ( workflowTypeGuid.HasValue )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
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

                var qryParams = new Dictionary<string, string> { { "GroupId", _group.Id.ToString() } };

                var groupTypeIds = PageParameter( "GroupTypeIds" );
                if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
                {
                    qryParams.Add( "GroupTypeIds", groupTypeIds );
                }

                NavigateToParentPage( qryParams );
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
                var qryParams = new Dictionary<string, string> { { "GroupId", _group.Id.ToString() } };

                var groupTypeIds = PageParameter( "GroupTypeIds" );
                if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
                {
                    qryParams.Add( "GroupTypeIds", groupTypeIds );
                }

                NavigateToParentPage( qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbPrintAttendanceRoster control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPrintAttendanceRoster_Click( object sender, EventArgs e )
        {
            // NOTE: lbPrintAttendanceRoster is a full postback since we are returning a download of the roster

            nbPrintRosterWarning.Visible = false;
            var rockContext = new RockContext();

            Dictionary<int, object> mergeObjectsDictionary = new Dictionary<int, object>();
            if ( _attendees != null )
            {
                var personIdList = _attendees.Select( a => a.PersonId ).ToList();
                var personList = new PersonService( rockContext ).GetByIds( personIdList );
                foreach ( var person in personList.OrderBy( a => a.LastName ).ThenBy( a => a.NickName ) )
                {
                    mergeObjectsDictionary.AddOrIgnore( person.Id, person );
                }
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Group", this._group );

            var mergeTemplate = new MergeTemplateService( rockContext ).Get( this.GetAttributeValue( "AttendanceRosterTemplate" ).AsGuid() );

            if ( mergeTemplate == null )
            {
                this.LogException( new Exception( "No Merge Template specified in block settings" ) );
                nbPrintRosterWarning.Visible = true;
                nbPrintRosterWarning.Text = "Unable to print Attendance Roster";
                return;
            }

            MergeTemplateType mergeTemplateType = mergeTemplate.GetMergeTemplateType();
            if ( mergeTemplateType == null )
            {
                this.LogException( new Exception( "Unable to determine Merge Template Type" ) );
                nbPrintRosterWarning.Visible = true;
                nbPrintRosterWarning.Text = "Error printing Attendance Roster";
                return;
            }

            BinaryFile outputBinaryFileDoc = null;

            var mergeObjectList = mergeObjectsDictionary.Select( a => a.Value ).ToList();

            outputBinaryFileDoc = mergeTemplateType.CreateDocument( mergeTemplate, mergeObjectList, mergeFields );

            // set the name of the output doc
            outputBinaryFileDoc = new BinaryFileService( rockContext ).Get( outputBinaryFileDoc.Id );
            outputBinaryFileDoc.FileName = _group.Name + " Attendance Roster" + Path.GetExtension( outputBinaryFileDoc.FileName ?? "" ) ?? ".docx";
            rockContext.SaveChanges();

            if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
            {
                if ( mergeTemplateType.Exceptions.Count == 1 )
                {
                    this.LogException( mergeTemplateType.Exceptions[0] );
                }
                else if ( mergeTemplateType.Exceptions.Count > 50 )
                {
                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions for top 50.", mergeTemplate.Name ), mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
                }
                else
                {
                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions", mergeTemplate.Name ), mergeTemplateType.Exceptions.ToList() ) );
                }
            }

            var uri = new UriBuilder( outputBinaryFileDoc.Url );
            var qry = System.Web.HttpUtility.ParseQueryString( uri.Query );
            qry["attachment"] = true.ToTrueFalse();
            uri.Query = qry.ToString();
            Response.Redirect( uri.ToString(), false );
            Context.ApplicationInstance.CompleteRequest();
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

        /// <summary>
        /// Handles the SelectionChanged event of the bddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlCampus_SelectionChanged( object sender, EventArgs e )
        {
            SetBlockUserPreference( "Campus", bddlCampus.SelectedValue );
            var campus = CampusCache.Read( bddlCampus.SelectedValueAsInt() ?? 0 );
            bddlCampus.Title = campus != null ? campus.Name : "All Campuses";
            BindAttendees();
        }

        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {

            string template = GetAttributeValue( "LavaTemplate" );

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
                        attendee.CampusIds = Person.GetCampusIds();

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Person", Person );
                        mergeFields.Add( "Attended", true );
                        attendee.MergedTemplate = template.ResolveMergeFields( mergeFields );
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
            if ( !occurrenceDate.HasValue )
            {
                occurrenceDate = PageParameter( "Occurrence" ).AsDateTime();
            }

            List<int> locationIds = new List<int>();
            int? locationId = PageParameter( "LocationId" ).AsIntegerOrNull();
            locationIds.Add( locationId ?? 0 );

            List<int> scheduleIds = new List<int>();
            int? scheduleId = PageParameter( "ScheduleId" ).AsIntegerOrNull();
            scheduleIds.Add( scheduleId ?? 0 );

            if ( Page.IsPostBack && _allowAdd )
            {
                if ( dpOccurrenceDate.Visible && dpOccurrenceDate.SelectedDate.HasValue )
                {
                    occurrenceDate = dpOccurrenceDate.SelectedDate;
                }

                if ( !locationIds.Any( l => l != 0 ) && ddlLocation.SelectedValueAsInt().HasValue )
                {
                    locationId = ddlLocation.SelectedValueAsInt().Value;
                    locationIds = new List<int> { locationId.Value };
                }

                if ( !scheduleIds.Any( s => s != 0 ) && ddlSchedule.SelectedValueAsInt().HasValue )
                {
                    scheduleId = ddlSchedule.SelectedValueAsInt().Value;
                    scheduleIds = new List<int> { scheduleId.Value };
                }
            }

            if ( occurrenceDate.HasValue )
            {
                // Try to find the selected occurrence based on group's schedule
                if ( _group != null )
                {
                    // Get all the occurrences for this group, and load the attendance so we can show Attendance Count
                    var occurrence = new ScheduleService( _rockContext )
                        .GetGroupOccurrences( _group, occurrenceDate.Value.Date, occurrenceDate.Value.AddDays( 1 ),
                            locationIds, scheduleIds, true )
                        .OrderBy( o => o.Date )
                        .FirstOrDefault();

                    if ( occurrence != null )
                    {
                        if ( occurrenceDate.Value.Date != occurrence.Date.Date )
                        {
                            occurrence.ScheduleId = null;
                            occurrence.ScheduleName = string.Empty;
                            occurrence.Date = occurrenceDate.Value;
                        }
                        return occurrence;
                    }
                }

                // If an occurrence date was included, but no occurrence was found with that date, and new 
                // occurrences can be added, create a new one
                if ( _allowAdd )
                {
                    Schedule schedule = null;
                    if ( scheduleId.HasValue )
                    {
                        schedule = new ScheduleService( _rockContext ).Get( scheduleId.Value );
                    }
                    return new ScheduleOccurrence( occurrenceDate.Value.Date, ( schedule != null ? schedule.StartTimeOfDay : occurrenceDate.Value.TimeOfDay ), scheduleId, string.Empty, locationId );
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

                    if ( !locations.ContainsKey( location.Id ) )
                    {
                        locations.Add( location.Id, new List<string> { parentLocationPath, location.Name }.AsDelimited( " > " ) );
                    }
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
                    lOccurrenceDate.Visible = _occurrence.ScheduleId.HasValue;
                    lOccurrenceDate.Text = _occurrence.Date.ToShortDateString();

                    dpOccurrenceDate.Visible = !_occurrence.ScheduleId.HasValue;
                    dpOccurrenceDate.SelectedDate = _occurrence.Date;

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
                }
                else
                {
                    lOccurrenceDate.Visible = false;
                    dpOccurrenceDate.Visible = true;
                    dpOccurrenceDate.SelectedDate = RockDateTime.Today;

                    int? locationId = PageParameter( "LocationId" ).AsIntegerOrNull();
                    if ( locationId.HasValue )
                    {
                        lLocation.Visible = true;
                        lLocation.Text = new LocationService( _rockContext ).GetPath( locationId.Value );
                        ddlLocation.Visible = false;

                        Schedule schedule = null;
                        int? scheduleId = PageParameter( "ScheduleId" ).AsIntegerOrNull();
                        if ( scheduleId.HasValue )
                        {
                            schedule = new ScheduleService( _rockContext ).Get( scheduleId.Value );
                        }

                        if ( schedule != null )
                        {
                            lSchedule.Visible = true;
                            lSchedule.Text = schedule.Name;
                            ddlSchedule.Visible = false;
                        }
                        else
                        {
                            BindSchedules( locationId.Value );
                            lSchedule.Visible = false;
                            ddlSchedule.Visible = ddlSchedule.Items.Count > 1;
                        }
                    }
                    else
                    {
                        lLocation.Visible = false;
                        ddlLocation.Visible = ddlLocation.Items.Count > 1;

                        lSchedule.Visible = false;
                        ddlSchedule.Visible = ddlSchedule.Items.Count > 1;
                    }
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

                string template = GetAttributeValue( "LavaTemplate" );
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                // Bind the attendance roster
                _attendees = new PersonService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( p => attendedIds.Contains( p.Id ) || unattendedIds.Contains( p.Id ) )
                    .ToList()
                    .Select( p => new GroupAttendanceAttendee()
                    {
                        PersonId = p.Id,
                        NickName = p.NickName,
                        LastName = p.LastName,
                        Attended = attendedIds.Contains( p.Id ),
                        CampusIds = p.GetCampusIds(),
                        MergedTemplate = template.ResolveMergeFields( mergeFields.Union( new Dictionary<string, object>() { { "Person", p } } ).ToDictionary( x => x.Key, x => x.Value ) )
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
            var campusAttendees = _attendees;
            if ( _allowCampusFilter )
            {
                var campus = CampusCache.Read( bddlCampus.SelectedValueAsInt() ?? 0 );
                if ( campus != null )
                {
                    campusAttendees = _attendees.Where( a => a.CampusIds.Contains( campus.Id ) ).ToList();
                }
            }

            int attendanceCount = campusAttendees.Where( a => a.Attended ).Count();
            lDidAttendCount.Visible = attendanceCount > 0;
            lDidAttendCount.Text = attendanceCount.ToString( "N0" );

            lvMembers.DataSource = campusAttendees.OrderBy( a => a.LastName ).ThenBy( a => a.NickName ).ToList();
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

            /// <summary>
            /// Gets or sets the campus ids that a person's families belong to.
            /// </summary>
            /// <value>
            /// The campus ids.
            /// </value>
            public List<int> CampusIds { get; set; }

            public string MergedTemplate { get; set; }
        }

        #endregion

    }
}