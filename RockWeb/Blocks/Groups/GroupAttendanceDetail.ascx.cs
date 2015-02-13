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
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
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

    public partial class GroupAttendanceDetail : RockBlock
    {
        #region Private Variables

        private RockContext _rockContext = null;
        private Group _group = null;
        private bool _canEdit = false;

        private GroupOccurrence prevOccurrence = null;
        private GroupOccurrence thisOccurrence = null;
        private GroupOccurrence nextOccurrence = null;

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

        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( _group != null && _group.ScheduleId.HasValue && thisOccurrence != null )
            {
                var rockContext = new RockContext();
                var attendanceService = new AttendanceService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                var existingAttendees = attendanceService.Queryable()
                        .Where( a =>
                            a.GroupId == _group.Id &&
                            a.ScheduleId == _group.ScheduleId &&
                            a.StartDateTime == thisOccurrence.StartDateTime )
                        .ToList();

                if ( cbDidNotMeet.Checked )
                {
                    foreach( var attendance in existingAttendees )
                    {
                        attendance.DidAttend = null;
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
                                attendance.StartDateTime = thisOccurrence.StartDateTime;
                                attendanceService.Add( attendance );
                            }
                        }

                        if ( attendance != null )
                        {
                            if ( cbDidNotMeet.Checked )
                            {
                                attendance.DidAttend = null;
                            }
                            else
                            {
                                attendance.DidAttend = cbMember.Checked;
                            }
                        }
                    }
                }

                rockContext.SaveChanges();
            }
        }

        #endregion

        #region Internal Methods

        private void GetOccurrenceItems()
        {
            DateTime? occurrenceDate = PageParameter( "Occurrence" ).AsDateTime();
            if ( _group != null )
            {
                lGroupName.Text = _group.Name;

                // Get all the occurrences for this group ( without loading attendance yet )
                var occurrences = new GroupService(_rockContext)
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
                                thisOccurrence = occurrences[i];
                                if ( i > 0 )
                                {
                                    prevOccurrence = occurrences[i - 1];
                                }
                                if ( i < occurrences.Count - 1 )
                                {
                                    nextOccurrence = occurrences[i + 1];
                                }
                            }
                        }
                    }

                    // If the selected occurrence was not found or one was not specified, use the last one in list
                    if ( thisOccurrence == null )
                    {
                        thisOccurrence = occurrences.Last();
                        if ( occurrences.Count >= 2 )
                        {
                            prevOccurrence = occurrences[occurrences.Count - 2];
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
            if ( thisOccurrence != null )
            {

                lOccurrenceDate.Text = thisOccurrence.StartDateTime.ToShortDateString();

                // Configure 'Previous' Button
                if ( prevOccurrence != null )
                {
                    var pageReference = CurrentPageReference;
                    pageReference.Parameters.AddOrReplace( "Occurrence", prevOccurrence.StartDateTime.ToString( "yyyy-MM-ddTHH:mm:ss" ) );
                    aPrev.NavigateUrl = pageReference.BuildUrl();
                    aPrev.Visible = true;
                }
                else
                {
                    aPrev.Visible = false;
                }

                // Configure 'Next' Button
                if ( nextOccurrence != null )
                {
                    var pageReference = CurrentPageReference;
                    pageReference.Parameters.AddOrReplace( "Occurrence", nextOccurrence.StartDateTime.ToString( "yyyy-MM-ddTHH:mm:ss" ) );
                    aNext.NavigateUrl = pageReference.BuildUrl();
                    aNext.Visible = true;
                }
                else
                {
                    aNext.Visible = false;
                }


                // Load the attendance for the selected occurrence
                new GroupService( _rockContext ).LoadAttendanceData( _group, thisOccurrence );

                cbDidNotMeet.Checked = thisOccurrence.DidNotMeet;

                // Get the list of people who attended
                var attendedIds = thisOccurrence.Attendance
                    .Where( a => a.DidAttend.HasValue && a.DidAttend.Value )
                    .Select( a => a.PersonAlias.PersonId )
                    .ToList();

                // Add any existing active members not on that list
                var unattendedIds = new GroupMemberService( _rockContext )
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

    }});
", cbDidNotMeet.ClientID );

            ScriptManager.RegisterStartupScript( cbDidNotMeet, cbDidNotMeet.GetType(), "group-attendance-detail", script, true );
        }

        #endregion
    }
    
}