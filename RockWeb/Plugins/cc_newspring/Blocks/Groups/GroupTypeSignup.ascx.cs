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
using RestSharp;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Security;
using Rock.VersionInfo;
using System.Runtime.Caching;
using System.Web.UI.WebControls;
using System.Data.Entity.Core.Objects;

namespace RockWeb.Plugins.cc_newspring.Groups
{
    /// <summary>
    /// Block that syncs selected people to an exchange server.
    /// </summary>
    [DisplayName( "Group Type Signup" )]
    [Category( "NewSpring > Groups" )]
    [Description( "Allows you to register for various groups of a selected type." )]

    [TextField("Block Title", "The title to use for the block header.", true, "Group Signup", order: 0)]
    [GroupTypeField("Group Type", "The group type to select groups from.", true, order: 1)]
    [SlidingDateRangeField("Date Range", "The date range to use for selecting dates from.", true, order: 2)]
    [LocationField("Location", "The location to filter the results by.", true, order: 3)]
    [GroupRoleField("","Group Role", "The role to add people to the group with.", order: 4)]
    [TextField("Description Attribute", "The group member attribute key to add the attendance description (date / schedule) to.", false, "", order: 5)]
    [BooleanField("Group Members Inactive", "Should new group members be created as inactive.", true, order: 6)]
    public partial class GroupTypeSignup : Rock.Web.UI.RockBlock
    {

        #region Fields

        List<ScheduleResult> _schedules = new List<ScheduleResult>();
        List<ScheduleResult> _personalSchedules = new List<ScheduleResult>();
        Location _location = null;
        int _groupRoleId = 0;
        GroupType _groupType = null;
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptScheduleDates.ItemDataBound += rptScheduleDates_ItemDataBound;
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            lBlockTitle.Text = GetAttributeValue( "BlockTitle" );

            base.OnLoad( e );

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "Location" ) ) )
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Warning;
                nbMessages.Text = "No location has been configured for this block.";
                btnSave.Visible = false;
                return;
            }

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "GroupRole" ) ) )
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Warning;
                nbMessages.Text = "No group role has been configured for this block.";
                btnSave.Visible = false;
                return;
            }

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "GroupType" ) ) )
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Warning;
                nbMessages.Text = "No group type has been configured for this block.";
                btnSave.Visible = false;
                return;
            }

            RockContext rockContext = new RockContext();

            _groupType = new GroupTypeService( rockContext).Get( GetAttributeValue( "GroupType" ).AsGuid() );
            _groupRoleId = new GroupTypeRoleService( rockContext ).Get( GetAttributeValue( "GroupRole" ).AsGuid() ).Id;
            _location = new LocationService( rockContext ).Get( GetAttributeValue( "Location" ).AsGuid() );
            LoadRegistration();
            btnSave.Visible = true;
            nbMessages.Text = string.Empty;   
        }

        #endregion

        #region Events

        protected void rptScheduleDates_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Header )
            {
                
            }
            else { 
                var scheduleResult = e.Item.DataItem as DateTime?;

                var placeholder = e.Item.FindControl( "phGroups" ) as PlaceHolder;

                var schedules = _schedules.Select( s => s.Schedule ).Distinct();

                var scheduleSort = new Dictionary<Schedule, DateTime>();

                var startOfDay = scheduleResult.Value.Date;
                var endOfDay = scheduleResult.Value.Date.AddDays( 1 ).AddTicks( -1 );

                foreach ( var schedule in schedules )
                {
                    scheduleSort.Add( schedule, schedule.GetScheduledStartTimes( startOfDay, endOfDay ).FirstOrDefault() );
                }

                foreach ( var schedule in scheduleSort.OrderBy(s => s.Value).Select( s => s.Key) )
                {
                    // get groups that have this schedule
                    var groups = _schedules.Where( s => s.Schedule.Id == schedule.Id && s.Date.Date == scheduleResult.Value )
                                    .Select(s => s.Group).Distinct();

                    var ddlGroups = new DropDownList();
                    ddlGroups.CssClass = "schedule-item form-control";
                    ddlGroups.DataSource = groups;
                    ddlGroups.DataValueField = "Id";
                    ddlGroups.DataTextField = "Name";
                    ddlGroups.Attributes.Add( "schedule-id", schedule.Id.ToString() );
                    ddlGroups.Attributes.Add( "schedule-date", scheduleResult.Value.ToShortDateString() );
                    ddlGroups.DataBind();

                    if ( ddlGroups.Items.Count == 0 )
                    {
                        ddlGroups.Visible = false;
                    }
                    else {
                        ddlGroups.Items.Insert( 0, "" );
                    }

                    // see if the person is marked as RSVP if so select this value
                    var attendingGroupId = _personalSchedules.Where( p =>
                                        p.Date.Date == scheduleResult.Value.Date
                                        && p.Schedule.Id == schedule.Id
                                        && p.Group.GroupTypeId == _groupType.Id )
                                        .Select( p => p.Group.Id )
                                        .FirstOrDefault();

                    ddlGroups.SelectedValue = attendingGroupId.ToString();
                    ddlGroups.Attributes.Add( "original-group-id", attendingGroupId.ToString() );


                    placeholder.Controls.Add( ddlGroups );
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            
        }
        #endregion


        #region Methods

        private void LoadRegistration()
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var attendanceService = new AttendanceService( rockContext );
	    var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

            // get list of groups of this type
            var groups = groupService.Queryable()
                            .Where( g =>
                                 g.GroupTypeId == _groupType.Id
                                 && g.IsActive == true )
                            .ToList();

            var groupIds = groups.Select( g => g.Id ).ToList();

            // get listing of possible attendance for groups of this type
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( "DateRange" ) ?? "-1||" );

            var attendanceOccurrences = attendanceOccurrenceService.Queryable()
                                            .Where( a =>
                                                a.GroupId.HasValue
                                                && groupIds.Contains( a.GroupId.Value )
                                                && a.OccurrenceDate >= dateRange.Start.Value
                                                && a.OccurrenceDate <= dateRange.End.Value
                                                && a.LocationId == _location.Id )
                                            .Distinct()
                                            .ToList();
                                            

            _schedules = attendanceOccurrences.Select( a => new ScheduleResult
                                        {
                                            Group = a.Group,
                                            Schedule = a.Schedule,
                                            Date = a.OccurrenceDate
                                        } )
                                        .Distinct()
                                        .ToList();
            var occurrenceIds = attendanceOccurrences.Select( o => o.Id ).ToList();

            // get personal schedules (things you already signed up for)
            _personalSchedules = attendanceService.Queryable()
                                    .Where( a =>
                                            occurrenceIds.Contains( a.OccurrenceId )
                                            && a.PersonAlias.PersonId == CurrentPersonId
                                            && a.RSVP == RSVP.Yes)
                                        .Select( a => new ScheduleResult
                                        {
                                            Group = a.Occurrence.Group,
                                            Schedule = a.Occurrence.Schedule,
                                            Date = a.StartDateTime
                                        } )
                                        .Distinct()
                                        .ToList();

            rptScheduleDates.DataSource = _schedules.Select( s => s.Date.Date ).Distinct();
            rptScheduleDates.DataBind();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            foreach ( RepeaterItem item in rptScheduleDates.Items )
            {
                var placeholder = item.FindControl( "phGroups" ) as PlaceHolder;

                if (placeholder != null )
                {
                    // get groups
                    foreach( Control control in placeholder.Controls )
                    {
                        if (control is DropDownList )
                        {
                            var ddlGroup = (DropDownList)control;

                            var groupId = ddlGroup.SelectedValue.AsInteger();
                            var scheduleId = ddlGroup.Attributes["schedule-id"].AsInteger();
                            var scheduleDate = ddlGroup.Attributes["schedule-date"].AsDateTime();
                            var originalGroupId = ddlGroup.Attributes["original-group-id"].AsInteger();

                            // check if person is already registered for this schedule
                            var attending = _personalSchedules.Where( p =>
                                                        p.Date.Date == scheduleDate.Value.Date
                                                        && p.Schedule.Id == scheduleId
                                                        && p.Group.Id == groupId )
                                                        .Any();

                            if ( originalGroupId != 0 && groupId == 0 )
                            {
                                RemovePersonRsvp( CurrentPerson, originalGroupId, scheduleId, scheduleDate );
                            }
                            else if (!attending && groupId != 0 )
                            {
                                // remove existing record
                                if ( originalGroupId != 0 )
                                {
                                    RemovePersonRsvp( CurrentPerson, originalGroupId, scheduleId, scheduleDate );
                                }

                                // mark them as coming
                                var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
                                var attendanceOccurrence = attendanceOccurrenceService.Queryable()
                                                            .Where( o =>
                                                                o.GroupId == groupId &&
                                                                o.ScheduleId == scheduleId &&
                                                                o.LocationId == _location.Id
                                                                )
                                                            .FirstOrDefault();

                                // Didn't find an attendance occurence to mark the attendance for. This should not happen, but just in case.
                                if ( attendanceOccurrence == null )
                                {
                                    nbMessages.NotificationBoxType = NotificationBoxType.Warning;
                                    nbMessages.Text = "Could not find a matching attendance occurrence to add the attendance to.";
                                    return;
                                }

                                var attendanceRecord = new Attendance
                                {
                                    PersonAliasId = CurrentPersonAliasId,
                                    OccurrenceId = attendanceOccurrence.Id,
                                    StartDateTime = scheduleDate.Value.Date,
                                    RSVP = RSVP.Yes
                                };

                                attendanceService.Add( attendanceRecord );
                                rockContext.SaveChanges();

                                // add them to the group
                                var groupMember = groupMemberService.Queryable().Where( m => m.PersonId == CurrentPersonId && m.GroupId == groupId ).FirstOrDefault();

                                if ( groupMember == null )
                                {
                                    bool createAsInactive = GetAttributeValue( "GroupMembersInactive" ).AsBoolean();

                                    groupMember = new GroupMember();
                                    groupMember.PersonId = CurrentPersonId.Value;
                                    groupMember.GroupId = groupId;
                                    groupMember.GroupMemberStatus = createAsInactive ? GroupMemberStatus.Inactive : GroupMemberStatus.Active;
                                    groupMember.GroupRoleId = _groupRoleId;
				    
				    groupMemberService.Add( groupMember );
                                }


                                rockContext.SaveChanges();

                                var scheduleName = new ScheduleService( rockContext ).Queryable().Where( s => s.Id == scheduleId ).Select( s => s.Name ).FirstOrDefault();

                                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "DescriptionAttribute" ) ) )
                                {
                                    groupMember.LoadAttributes();
                                    groupMember.SetAttributeValue( GetAttributeValue( "DescriptionAttribute" ), string.Format( "{0} {1}", scheduleDate.Value.ToShortDateString(), scheduleName ) );
                                    groupMember.SaveAttributeValues();
                                }
                                
                            }
                        }
                    }
                }
            }

            nbMessages.NotificationBoxType = NotificationBoxType.Success;
            nbMessages.Text = "Your changes have been saved.";
        }

        /// <summary>
        /// Removes the person RSVP.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="scheduleDate">The schedule date.</param>
        private void RemovePersonRsvp(Person person, int groupId, int scheduleId, DateTime? scheduleDate )
        {
            RockContext rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            // delete the RSVP
            var attendanceRecord = attendanceService.Queryable()
                                    .Where( a =>
                                         a.PersonAlias.PersonId == CurrentPersonId
                                         && a.Occurrence.GroupId == groupId
                                         && a.Occurrence.ScheduleId == scheduleId
                                         && a.RSVP == RSVP.Yes
                                         && DbFunctions.TruncateTime( a.StartDateTime ) == DbFunctions.TruncateTime( scheduleDate.Value ) )
                                    .FirstOrDefault();

            if ( attendanceRecord != null )
            {
                attendanceService.Delete( attendanceRecord );
            }

            // remove them from the group
            var groupMember = groupMemberService.Queryable().Where( m => m.PersonId == CurrentPersonId && m.GroupId == groupId ).FirstOrDefault();

            if ( groupMember != null )
            {
                groupMemberService.Delete( groupMember );
            }

            rockContext.SaveChanges();
        }

        #endregion
    }

    /// <summary>
    /// Schedule return object
    /// </summary>
    public class ScheduleResult
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public Group Group { get; set; }
        
        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        public Schedule Schedule { get; set; }
        
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public DateTime Date { get; set; }
    }

}