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

    public partial class GroupAttendanceDetail : RockBlock
    {
        #region Private Variables

        private RockContext _rockContext = null;
        private Group _group = null;
        private bool _canEdit = false;
        private ScheduleOccurrence _prevOccurrence = null;
        private ScheduleOccurrence _thisOccurrence = null;
        private ScheduleOccurrence _nextOccurrence = null;

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

            _group = new GroupService( _rockContext ).Get( PageParameter( "GroupId" ).AsInteger() );
            if ( _group != null && _group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                _canEdit = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            GetOccurrenceItems();

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
            if ( _group != null && _group.ScheduleId.HasValue && _thisOccurrence != null )
            {
                var rockContext = new RockContext();
                var attendanceService = new AttendanceService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                var existingAttendees = attendanceService.Queryable()
                        .Where( a =>
                            a.GroupId == _group.Id &&
                            a.ScheduleId == _group.ScheduleId &&
                            a.StartDateTime == _thisOccurrence.StartDateTime )
                        .ToList();

                if ( cbDidNotMeet.Checked )
                {
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
                                attendance.StartDateTime = _thisOccurrence.StartDateTime;
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

                rockContext.SaveChanges();

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
        private void GetOccurrenceItems()
        {
            DateTime? occurrenceDate = PageParameter( "Occurrence" ).AsDateTime();
            if ( _group != null )
            {
                lGroupName.Text = _group.Name;

                // Get all the occurrences for this group ( without loading attendance yet )
                var occurrences = new ScheduleService( _rockContext )
                    .GetGroupOccurrences( _group, false )
                    .OrderBy( o => o.StartDateTime )
                    .ToList();

                // If occurrences were found, loop through them looking for the selected occurrence
                if ( occurrences.Any() )
                {
                    // Try to find the selected occurrence
                    if ( occurrenceDate.HasValue )
                    {
                        for ( int i = 0; i < occurrences.Count; i++ )
                        {
                            if ( occurrences[i].StartDateTime == occurrenceDate.Value )
                            {
                                _thisOccurrence = occurrences[i];
                                if ( i > 0 )
                                {
                                    _prevOccurrence = occurrences[i - 1];
                                }
                                if ( i < occurrences.Count - 1 )
                                {
                                    _nextOccurrence = occurrences[i + 1];
                                }
                            }
                        }
                    }

                    // If the selected occurrence was not found or one was not specified, use the last one in list
                    if ( _thisOccurrence == null )
                    {
                        _thisOccurrence = occurrences.Last();
                        if ( occurrences.Count >= 2 )
                        {
                            _prevOccurrence = occurrences[occurrences.Count - 2];
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void ShowDetails()
        {
            if ( _thisOccurrence != null )
            {

                lOccurrenceDate.Text = _thisOccurrence.StartDateTime.ToShortDateString();

                // Configure 'Previous' Button
                if ( _prevOccurrence != null )
                {
                    var pageReference = CurrentPageReference;
                    pageReference.Parameters.AddOrReplace( "Occurrence", _prevOccurrence.StartDateTime.ToString( "yyyy-MM-ddTHH:mm:ss" ) );
                    aPrev.NavigateUrl = pageReference.BuildUrl();
                    aPrev.Visible = true;
                }
                else
                {
                    aPrev.Visible = false;
                }

                // Configure 'Next' Button
                if ( _nextOccurrence != null )
                {
                    var pageReference = CurrentPageReference;
                    pageReference.Parameters.AddOrReplace( "Occurrence", _nextOccurrence.StartDateTime.ToString( "yyyy-MM-ddTHH:mm:ss" ) );
                    aNext.NavigateUrl = pageReference.BuildUrl();
                    aNext.Visible = true;
                }
                else
                {
                    aNext.Visible = false;
                }


                // Load the attendance for the selected occurrence
                new ScheduleService( _rockContext ).LoadAttendanceData( _group, _thisOccurrence );

                cbDidNotMeet.Checked = _thisOccurrence.DidNotOccur;

                // Get the list of people who attended
                var attendedIds = _thisOccurrence.Attendance
                    .Where( a => a.DidAttend.HasValue && a.DidAttend.Value )
                    .Select( a => a.PersonAlias.PersonId )
                    .ToList();

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
            else
            {
                nbNotice.Heading = "No Occurrences";
                nbNotice.Text = "<p>There are currently not any active occurrences for selected group to take attendance for.</p>";
                nbNotice.NotificationBoxType = NotificationBoxType.Warning;
                nbNotice.Visible = true;

                pnlDetails.Visible = false;
            }

        }

        protected void RegisterScript()
        {
            string script = string.Format( @"

    Sys.Application.add_load(function () {{

        if ($('#{0}').is(':checked')) {{
            $('div.group-attendance-roster').hide();
        }}

        $('#{0}').click(function () {{
            if ($(this).is(':checked')) {{
                $('div.group-attendance-roster').hide('fast');
            }} else {{
                $('div.group-attendance-roster').show('fast');
            }}
        }});

        $('.js-add-member').click(function ( e ) {{
            e.preventDefault();
            var $a = $(this);
            var memberName = $(this).parent().find('span').html();
            Rock.dialogs.confirm('Add ' + memberName + ' to your group?', function (result) {{
                if (result) {{
                    eval($a.prop('href'));
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