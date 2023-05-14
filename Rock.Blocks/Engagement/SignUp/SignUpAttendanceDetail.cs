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
using System.Linq;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Engagement.SignUp.SignUpAttendanceDetail;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Engagement.SignUp
{
    /// <summary>
    /// Lists the group members for a specific sign-up group/project occurrence date time and allows selecting if they attended or not.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Sign-Up Attendance Detail" )]
    [Category( "Obsidian > Engagement > Sign-Up" )]
    [Description( "Lists the group members for a specific sign-up group/project occurrence date time and allows selecting if they attended or not." )]
    [IconCssClass( "fa fa-clipboard-check" )]

    #region Block Attributes

    [CodeEditorField( "Header Lava Template",
        Key = AttributeKey.HeaderLavaTemplate,
        Description = "The Lava template to show at the top of the page.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        DefaultValue = AttributeDefault.HeaderLavaTemplate,
        IsRequired = false,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "747587A0-87E9-437D-A4ED-75431CED55B3" )]
    [Rock.SystemGuid.BlockTypeGuid( "96D160D9-5668-46EF-9941-702BD3A577DB" )]
    public class SignUpAttendanceDetail : RockObsidianBlockType
    {
        #region Keys & Constants

        private static class PageParameterKey
        {
            public const string ProjectId = "ProjectId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
            public const string AttendanceDate = "AttendanceDate";
        }

        private static class AttributeKey
        {
            public const string HeaderLavaTemplate = "HeaderLavaTemplate";
        }

        private static class AttributeDefault
        {
            public const string HeaderLavaTemplate = @"<h3>{{ Group.Name }}</h3>
<div>
    Please enter attendance for the project below.
    <br>Date: {{ AttendanceOccurrenceDate | Date:'dddd, MMM d' }}
    {% if WasScheduleParamProvided %}
        <br>Schedule: {{ ScheduleName }}
    {% endif %}
    {% if WasLocationParamProvided %}
        <br>Location: {{ LocationName }}
    {% endif %}
</div>
<hr>";
        }

        #endregion

        #region Fields

        private AttendanceOccurrenceService _attendanceOccurrenceService;

        #endregion

        #region Properties

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new SignUpAttendanceDetailInitializationBox();

                SetBoxInitialState( box, rockContext );

                return box;
            }
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialState( SignUpAttendanceDetailInitializationBox box, RockContext rockContext )
        {
            var occurrenceData = GetOccurrenceData( rockContext );

            if ( !occurrenceData.CanTakeAttendance )
            {
                box.ErrorMessage = occurrenceData.ErrorMessage ?? "Unable to take attendance for this occurrence.";
                return;
            }

            box.HeaderHtml = GetHeaderHtml( occurrenceData );
            box.Attendees = occurrenceData.Attendees;
        }

        /// <summary>
        /// Gets the occurrence data, built using a combination of page parameter values and existing database records.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="shouldTrackAttendanceRecords">Whether the <see cref="AttendanceOccurrence"/> and <see cref="Attendance"/> records should be tracked by Entity Framework.</param>
        /// <returns>The occurrence data, built using a combination of page parameter values and existing database records.</returns>
        private OccurrenceData GetOccurrenceData( RockContext rockContext, bool shouldTrackAttendanceRecords = false )
        {
            var occurrenceData = new OccurrenceData();

            var projectId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.ProjectId ) );
            if ( !projectId.HasValue )
            {
                occurrenceData.ErrorMessage = "Project ID was not provided.";
                return occurrenceData;
            }

            var group = GetGroup( rockContext, projectId.Value );
            if ( group == null )
            {
                occurrenceData.ErrorMessage = "Project was not found.";
                return occurrenceData;
            }

            occurrenceData.Group = group;

            var currentPerson = this.RequestContext.CurrentPerson;
            if ( !group.IsAuthorized( Authorization.VIEW, currentPerson ) )
            {
                occurrenceData.ErrorMessage = EditModeMessage.NotAuthorizedToView( Rock.Model.Group.FriendlyTypeName );
                return occurrenceData;
            }

            if ( !group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson ) && !group.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                occurrenceData.ErrorMessage = $"You're not authorized to update the attendance for the selected {Rock.Model.Group.FriendlyTypeName}.";
                return occurrenceData;
            }

            var locationId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.LocationId ) );
            var scheduleId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.ScheduleId ) );
            var attendanceOccurrenceDate = PageParameter( PageParameterKey.AttendanceDate ).AsDateTime();

            if ( !TryGetGroupLocationSchedule( occurrenceData, group, locationId, scheduleId, attendanceOccurrenceDate ) )
            {
                // An error message will have been added.
                return occurrenceData;
            }

            if ( !TryGetGroupMembers( rockContext, occurrenceData ) )
            {
                // An error message will have been added.
                return occurrenceData;
            }

            GetExistingOccurrence( rockContext, occurrenceData, shouldTrackAttendanceRecords );

            return occurrenceData;
        }

        /// <summary>
        /// Gets the group for the specified identifier.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <returns>The group for the specified identifier.</returns>
        private Rock.Model.Group GetGroup( RockContext rockContext, int groupId )
        {
            return new GroupService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( g => g.GroupLocations )
                .Include( g => g.GroupLocations.Select( gl => gl.Location ) )
                .Include( g => g.GroupLocations.Select( gl => gl.Schedules ) )
                .FirstOrDefault( g => g.Id == groupId );
        }

        /// <summary>
        /// Tries to get the <see cref="Location"/> and <see cref="Schedule"/> instances for this occurrence, loading them onto the provided <see cref="OccurrenceData"/> instance.
        /// </summary>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <param name="group">The group.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="attendanceOccurrenceDate">The attendance occurrence date.</param>
        /// <returns>Whether <see cref="Location"/> and <see cref="Schedule"/> instances were successfully loaded for this occurrence.</returns>
        private bool TryGetGroupLocationSchedule( OccurrenceData occurrenceData, Rock.Model.Group group, int? locationId, int? scheduleId, DateTime? attendanceOccurrenceDate )
        {
            GroupLocation groupLocation = null;
            if ( locationId.HasValue )
            {
                occurrenceData.WasLocationParamProvided = true;
                groupLocation = group.GroupLocations.FirstOrDefault( gl => gl.LocationId == locationId.Value );
            }
            else if ( group.GroupLocations.Count == 1 )
            {
                groupLocation = group.GroupLocations.First();
            }

            Schedule schedule = null;
            if ( groupLocation != null )
            {
                if ( scheduleId.HasValue )
                {
                    occurrenceData.WasScheduleParamProvided = true;
                    schedule = groupLocation.Schedules.FirstOrDefault( s => s.Id == scheduleId.Value );
                }
                else if ( groupLocation.Schedules.Count == 1 )
                {
                    schedule = groupLocation.Schedules.First();
                }
            }

            if ( groupLocation?.Location == null || schedule == null )
            {
                occurrenceData.ErrorMessage = "The configuration provided does not provide enough information to take attendance. Please provide the schedule and location for this occurrence.";
                return false;
            }

            if ( !attendanceOccurrenceDate.HasValue )
            {
                attendanceOccurrenceDate = RockDateTime.Today;
            }

            // Ensure the specified attendance date matches an occurrence of the selected schedule.
            var date = attendanceOccurrenceDate.Value.Date;
            if ( !schedule.GetScheduledStartTimes( date.StartOfDay(), date.EndOfDay() ).Any() )
            {
                occurrenceData.ErrorMessage = $"The attendance date of {date:dddd, MMM d} does not match the schedule of the project.";
                return false;
            }

            occurrenceData.Location = groupLocation.Location;
            occurrenceData.Schedule = schedule;
            occurrenceData.AttendanceOccurrenceDate = date;

            return true;
        }

        /// <summary>
        /// Tries to get the group members for this occurrence, loading them onto the provided <see cref="OccurrenceData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <returns>Whether group members were successfully loaded for this occurrence.</returns>
        private bool TryGetGroupMembers( RockContext rockContext, OccurrenceData occurrenceData )
        {
            var groupMembers = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( gma => gma.GroupMember )
                .Include( gma => gma.GroupMember.Person )
                .Include( gma => gma.GroupMember.Person.Aliases )
                .Where( gma =>
                    !gma.GroupMember.Person.IsDeceased
                    && gma.GroupMember.GroupId == occurrenceData.Group.Id
                    && gma.LocationId == occurrenceData.Location.Id
                    && gma.ScheduleId == occurrenceData.Schedule.Id
                )
                .OrderBy( gma => gma.GroupMember.Person.LastName )
                .ThenBy( gma => gma.GroupMember.Person.AgeClassification )
                .ThenBy( gma => gma.GroupMember.Person.Gender )
                .Select( gma => gma.GroupMember )
                .ToList();

            if ( !groupMembers.Any() )
            {
                occurrenceData.ErrorMessage = "No attendees found for this occurrence.";
                return false;
            }

            occurrenceData.GroupMembers = groupMembers;

            return true;
        }

        /// <summary>
        /// Gets the existing [Attendance]Occurrence, if one exists, loading it onto the provided <see cref="OccurrenceData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <param name="shouldTrack">Whether the <see cref="AttendanceOccurrence"/> and <see cref="Attendance"/> records should be tracked by Entity Framework.</param>
        private void GetExistingOccurrence( RockContext rockContext, OccurrenceData occurrenceData, bool shouldTrack )
        {
            _attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

            var qry = _attendanceOccurrenceService
                .Queryable()
                .Include( ao => ao.Attendees );

            if ( !shouldTrack )
            {
                qry = qry.AsNoTracking();
            }

            occurrenceData.ExistingOccurrence = qry
                .FirstOrDefault( ao =>
                    ao.GroupId == occurrenceData.Group.Id
                    && ao.LocationId == occurrenceData.Location.Id
                    && ao.ScheduleId == occurrenceData.Schedule.Id
                    && ao.OccurrenceDate == occurrenceData.AttendanceOccurrenceDate
                );
        }

        /// <summary>
        /// Gets the header HTML for this occurrence.
        /// </summary>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <returns>The header HTML.</returns>
        private string GetHeaderHtml( OccurrenceData occurrenceData )
        {
            var lavaTemplate = GetAttributeValue( AttributeKey.HeaderLavaTemplate );
            var mergeFields = this.RequestContext.GetCommonMergeFields();

            mergeFields.Add( "Group", occurrenceData.Group );
            mergeFields.Add( "Location", occurrenceData.Location );
            mergeFields.Add( "Schedule", occurrenceData.Schedule );
            mergeFields.Add( "AttendanceOccurrenceDate", occurrenceData.AttendanceOccurrenceDate );
            mergeFields.Add( "LocationName", occurrenceData.LocationName );
            mergeFields.Add( "WasLocationParamProvided", occurrenceData.WasLocationParamProvided );
            mergeFields.Add( "ScheduleName", occurrenceData.ScheduleName );
            mergeFields.Add( "WasScheduleParamProvided", occurrenceData.WasScheduleParamProvided );

            return lavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Creates or updates <see cref="AttendanceOccurrence"/> and <see cref="Attendance"/> records for this occurrence and the provided attendees.
        /// <para>
        /// If a previous <see cref="Attendance"/> record exists for an attendee who is not represented in the provided attendee list, this <see cref="Attendance"/> record will be deleted from the database.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <param name="attendees">The attendees.</param>
        private void SaveAttendanceRecords( RockContext rockContext, OccurrenceData occurrenceData, List<SignUpAttendeeBag> attendees )
        {
            var occurrence = occurrenceData.ExistingOccurrence;
            if ( occurrence == null )
            {
                occurrence = new AttendanceOccurrence
                {
                    GroupId = occurrenceData.Group.Id,
                    LocationId = occurrenceData.Location.Id,
                    ScheduleId = occurrenceData.Schedule.Id,
                    OccurrenceDate = occurrenceData.AttendanceOccurrenceDate
                };

                _attendanceOccurrenceService.Add( occurrence );
            }

            var existingAttendees = occurrence.Attendees;
            foreach ( var existingAttendee in existingAttendees
                                                  .Where( ea => ea.PersonAliasId.HasValue && !attendees.Any( a => a.PersonAliasId == ea.PersonAliasId.Value ) )
                                                  .ToList() )
            {
                occurrence.Attendees.Remove( existingAttendee );
            }

            var campusId = occurrenceData.Location.CampusId;
            var startDateTime = occurrenceData.Schedule.HasSchedule()
                ? occurrenceData.AttendanceOccurrenceDate.Add( occurrenceData.Schedule.StartTimeOfDay )
                : occurrenceData.AttendanceOccurrenceDate;

            foreach ( var attendee in attendees )
            {
                var attendance = existingAttendees.FirstOrDefault( a => a.PersonAliasId.HasValue && a.PersonAliasId.Value == attendee.PersonAliasId );
                if ( attendance == null )
                {
                    attendance = new Attendance
                    {
                        PersonAliasId = attendee.PersonAliasId,
                        CampusId = campusId,
                        StartDateTime = startDateTime
                    };

                    occurrence.Attendees.Add( attendance );
                }

                attendance.DidAttend = attendee.DidAttend;
            }

            rockContext.SaveChanges();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Creates or updates attendance records for the specified attendees.
        /// </summary>
        /// <param name="bag">The bag that contains the information required to take attendance.</param>
        /// <returns>An empty 200 OK result if attendance records were successfully saved or an error response if the save attempt failed.</returns>
        [BlockAction]
        public BlockActionResult SaveAttendance( SignUpAttendanceBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceData = GetOccurrenceData( rockContext, shouldTrackAttendanceRecords: true );

                if ( !occurrenceData.CanTakeAttendance )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage ?? "Unable to take attendance for this occurrence." );
                }

                SaveAttendanceRecords( rockContext, occurrenceData, bag.Attendees );
            }

            return ActionOk();
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// A runtime object to represent a <see cref="Rock.Model.Group"/>, <see cref="Rock.Model.Location"/> and
        /// <see cref="Rock.Model.Schedule"/> combination, along with it's <see cref="GroupMember"/> collection,
        /// against which an attendance occurrence should be saved.
        /// </summary>
        private class OccurrenceData
        {
            public string ErrorMessage { get; set; }

            public Rock.Model.Group Group { get; set; }

            public Location Location { get; set; }

            public bool WasLocationParamProvided { get; set; }

            public Schedule Schedule { get; set; }

            public bool WasScheduleParamProvided { get; set; }

            public DateTime AttendanceOccurrenceDate { get; set; }

            public List<GroupMember> GroupMembers { get; set; }

            public AttendanceOccurrence ExistingOccurrence { get; set; }

            public bool CanTakeAttendance
            {
                get
                {
                    return string.IsNullOrEmpty( this.ErrorMessage )
                        && this.Group != null
                        && this.Location != null
                        && this.Schedule != null;
                }
            }

            public string LocationName
            {
                get
                {
                    return this.Location?.ToString( true );
                }
            }

            public string ScheduleName
            {
                get
                {
                    return this.Schedule?.ToString();
                }
            }

            public List<SignUpAttendeeBag> Attendees
            {
                get
                {
                    var attendees = new List<SignUpAttendeeBag>();

                    foreach ( var groupMember in GroupMembers )
                    {
                        var person = groupMember.Person;
                        var didAttend = this.ExistingOccurrence != null
                            && this.ExistingOccurrence.Attendees
                                .Any( a => a.DidAttend == true && person.Aliases.Any( pa => pa.Id == a.PersonAliasId ) );

                        attendees.Add( new SignUpAttendeeBag
                        {
                            PersonAliasId = person.PrimaryAliasId.GetValueOrDefault(),
                            Name = person.FullName,
                            DidAttend = didAttend
                        } );
                    }

                    return attendees;
                }
            }
        }

        #endregion
    }
}
