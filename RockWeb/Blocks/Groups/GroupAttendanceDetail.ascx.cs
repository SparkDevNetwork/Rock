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
    [WorkflowTypeField( "Workflow", "An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.", false, false, "", "", 1 )]
    public partial class GroupAttendanceDetail : RockBlock
    {
        #region Private Variables

        private RockContext _rockContext = null;
        private Group _group = null;
        private bool _canEdit = false;
        private bool _allowAdd = false;
        private ScheduleOccurrence _occurrence = null;

        #endregion

        #region Control Methods

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

                var existingAttendees = attendanceService.Queryable()
                        .Where( a =>
                            a.GroupId == _group.Id &&
                            a.ScheduleId == _group.ScheduleId &&
                            a.StartDateTime >= _occurrence.StartDateTime &&
                            a.StartDateTime < _occurrence.EndDateTime )
                        .ToList();

                // If did not meet was selected and this was a manually entered occurrence (not based on a schedule)
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

                    foreach ( var item in lvMembers.Items )
                    {
                        var hfMember = item.FindControl( "hfMember" ) as HiddenField;
                        var cbMember = item.FindControl( "cbMember" ) as CheckBox;

                        if ( hfMember != null && cbMember != null )
                        {
                            int personId = hfMember.ValueAsInt();

                            var attendance = existingAttendees
                                .Where( a => a.PersonAlias.PersonId == personId )
                                .FirstOrDefault();

                            if ( attendance == null )
                            {
                                int? personAliasId = personAliasService.GetPrimaryAliasId( personId );
                                if ( personAliasId.HasValue )
                                {
                                    attendance = new Attendance();
                                    attendance.GroupId = _group.Id;
                                    attendance.ScheduleId = _group.ScheduleId;
                                    attendance.PersonAliasId = personAliasId;
                                    attendance.StartDateTime = _occurrence.StartDateTime;
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
                                    attendance.DidAttend = cbMember.Checked;
                                    attendance.DidNotOccur = null;
                                }
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
                            var workflowService = new WorkflowService( rockContext );
                            var workflow = Workflow.Activate( workflowType, _group.Name );

                            workflow.SetAttributeValue( "StartDateTime", _occurrence.StartDateTime.ToString( "o" ) );
                            workflow.SetAttributeValue( "Schedule", _group.Schedule.Guid.ToString() );

                            List<string> workflowErrors;
                            if ( workflow.Process( rockContext, _group, out workflowErrors ) )
                            {
                                if ( workflow.IsPersisted || workflow.IsPersisted )
                                {
                                    if ( workflow.Id == 0 )
                                    {
                                        workflowService.Add( workflow );
                                    }

                                    rockContext.WrapTransaction( () =>
                                    {
                                        rockContext.SaveChanges();
                                        workflow.SaveAttributeValues( _rockContext );
                                        foreach ( var activity in workflow.Activities )
                                        {
                                            activity.SaveAttributeValues( rockContext );
                                        }
                                    } );
                                }
                            }

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
            DateTime? occurrenceDate = PageParameter( "Occurrence" ).AsDateTime();

            // If an occurrence date was not specified in query string, use the selected date
            if ( !occurrenceDate.HasValue && 
                Page.IsPostBack &&
                _allowAdd &&
                dpOccurrenceDate.SelectedDate.HasValue )
            {
                occurrenceDate = dpOccurrenceDate.SelectedDate;
            }

            if ( occurrenceDate.HasValue )
            {
                // Try to find the selected occurrence based on group's schedule
                if ( _group != null )
                {
                    lHeading.Text = _group.Name + " Attendance";

                    // Get all the occurrences for this group ( without loading attendance yet )
                    var occurrences = new ScheduleService( _rockContext )
                        .GetGroupOccurrences( _group, false )
                        .OrderBy( o => o.StartDateTime )
                        .ToList();

                    // If occurrences were found, loop through them looking for the selected occurrence
                    var occurrence = occurrences
                        .Where( o =>
                            occurrenceDate.Value >= o.StartDateTime &&
                            occurrenceDate.Value < o.EndDateTime )
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
                    return new ScheduleOccurrence( occurrenceDate.Value, occurrenceDate.Value.Date.AddDays(1) );
                }
            }

            return null;
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
                    lOccurrenceDate.Visible = existingOccurrence;
                    lOccurrenceDate.Text = _occurrence.StartDateTime.ToShortDateString();
                    dpOccurrenceDate.Visible = false;
                }
                else
                {
                    lOccurrenceDate.Visible = false;
                    dpOccurrenceDate.Visible = true;
                    dpOccurrenceDate.SelectedDate = RockDateTime.Today;
                }

                lMembers.Text = _group.GroupType.GroupMemberTerm.Pluralize();
                lPendingMembers.Text = "Pending " + lMembers.Text;

                List<int> attendedIds = new List<int>();

                // Load the attendance for the selected occurrence
                if ( existingOccurrence )
                {
                    new ScheduleService( _rockContext ).LoadAttendanceData( _group, _occurrence );

                    cbDidNotMeet.Checked = _occurrence.DidNotOccur;

                    // Get the list of people who attended
                    attendedIds = _occurrence.Attendance
                        .Where( a => a.DidAttend.HasValue && a.DidAttend.Value )
                        .Select( a => a.PersonAlias.PersonId )
                        .ToList();
                }

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
                lvMembers.DataSource = new PersonService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( p => attendedIds.Contains( p.Id ) || unattendedIds.Contains( p.Id ) )
                    .OrderBy( p => p.LastName )
                    .ThenBy( p => p.NickName )
                    .Select( p => new
                    {
                        Id = p.Id,
                        Attended = attendedIds.Contains( p.Id ),
                        FullName = p.NickName + " " + p.LastName
                    } )
                    .ToList();
                lvMembers.DataBind();

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

    }

}