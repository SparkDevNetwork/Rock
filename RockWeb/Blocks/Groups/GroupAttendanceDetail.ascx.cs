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


        protected void lvMembers_ItemCommand( object sender, ListViewCommandEventArgs e )
        {

        }
        protected void lvPending_ItemCommand( object sender, ListViewCommandEventArgs e )
        {

        }
        protected void lbSave_Click( object sender, EventArgs e )
        {

        }
        protected void lbAdd_Click( object sender, EventArgs e )
        {

        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void ShowDetails()
        {
            DateTime? occurrenceDate = PageParameter( "Occurrence" ).AsDateTime();
            if ( _group != null )
            {
                GroupOccurrence prevOccurrence = null;
                GroupOccurrence thisOccurrence = null;
                GroupOccurrence nextOccurrence = null;

                var groupService = new GroupService(_rockContext);
                var groupMemberService = new GroupMemberService( _rockContext );

                // Get all the occurrences for this group ( without loading attendance yet )
                var occurrences = groupService
                    .GetGroupOccurrences( _group, false )
                    .OrderBy( o => o.OccurrenceStartDateTime )
                    .ToList();

                // If occurrences were found, loop through them looking for the selected occurrence
                if ( occurrences.Any() )
                {
                    // Try to find the selected occurrence
                    if ( occurrenceDate.HasValue )
                    {
                        for ( int i = 0; i < occurrences.Count; i++ )
                        {
                            if ( occurrences[i].OccurrenceStartDateTime == occurrenceDate.Value )
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

                    // Configure 'Previous' Button
                    if ( prevOccurrence != null )
                    {
                        var pageReference = CurrentPageReference;
                        pageReference.Parameters.AddOrReplace( "Occurrence", prevOccurrence.OccurrenceStartDateTime.ToString("o"));
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
                        pageReference.Parameters.AddOrReplace( "Occurrence", nextOccurrence.OccurrenceStartDateTime.ToString("o"));
                        aNext.NavigateUrl = pageReference.BuildUrl();
                        aNext.Visible = true;
                    }
                    else
                    {
                        aNext.Visible = false;
                    }

                    // Load the attendance for the selected occurrence
                    groupService.LoadAttendanceData( _group, thisOccurrence );

                    // Get the list of people who attended
                    var attendedIds = thisOccurrence.Attendance
                        .Where( a => a.DidAttend )
                        .Select( a => a.PersonAlias.PersonId )
                        .ToList();

                    // Add any existing active members not on that list
                    var unattendedIds = groupMemberService
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.GroupId == _group.Id &&
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            !attendedIds.Contains ( m.PersonId ) )
                        .Select( m => m.PersonId )
                        .ToList();

                    // Bind the attendance roster
                    lvMembers.DataSource = new PersonService( _rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => attendedIds.Contains(p.Id) || unattendedIds.Contains( p.Id ) )
                        .OrderBy( p => p.LastName )
                        .ThenBy( p => p.NickName )
                        .Select( p => new {
                            Id = p.Id,
                            Attended = attendedIds.Contains(p.Id),
                            FullName = p.NickName + " " + p.LastName
                        })
                        .ToList();
                    lvMembers.DataBind();

                    lvPending.DataSource = groupMemberService
                        .Queryable().AsNoTracking()
                        .Where( m => 
                            m.GroupId == _group.Id &&
                            m.GroupMemberStatus == GroupMemberStatus.Pending )
                        .OrderBy( m => m.Person.LastName )
                        .ThenBy( m => m.Person.NickName )
                        .Select( m => new {
                            Id = m.PersonId,
                            FullName = m.Person.NickName + " " + m.Person.LastName
                        })
                        .ToList();
                    lvPending.DataBind();
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

        }

        protected void RegisterScript()
        {
            string script = string.Format( @"

    Sys.Application.add_load(function () {{

        if ($('#{0}').is(':checked')) {{
            $('ul.roster-list').hide();
        }}

        $('#{0}').click(function () {{
            if ($(this).is(':checked')) {{
                $('ul.roster-list').hide('fast');
            }} else {{
                $('ul.roster-list').show('fast');
            }}
        }});

        $('ul.roster-list .js-remove-member').click(function () {{
            var memberName = $(this).prev().find('label').html();
            return confirm('Are you sure you want to remove ' + memberName + ' from your group?');
        }});

        $('ul.pending .js-add-member').click(function () {{
            var memberName = $(this).prev().children('label').html();
            return confirm('Add ' + memberName + ' to your group?');
        }});

    }});
", cbDidNotMeet.ClientID );

            ScriptManager.RegisterStartupScript( cbDidNotMeet, cbDidNotMeet.GetType(), "group-attendance-detail", script, true );
        }

        #endregion
    }
    
}