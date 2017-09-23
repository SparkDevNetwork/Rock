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
using System.Data.Entity;
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process communications
    /// </summary>
    [GroupTypeField( "Group Type", "The Group type to send attendance reminders for.", true, Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP, "", 0 )]
    [SystemEmailField( "System Email", "The system email to use when sending reminder.", true, Rock.SystemGuid.SystemEmail.GROUP_ATTENDANCE_REMINDER, "", 1 )]
    [TextField( "Send Reminders", "Comma delimited list of days after a group meets to send an additional reminder. For example, a value of '2,4' would result in an additional reminder getting sent two and four days after group meets if attendance was not entered.", false, "", "", 2 )]
    [DisallowConcurrentExecution]
    public class SendAttendanceReminder : IJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public SendAttendanceReminder()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var groupType = GroupTypeCache.Read( dataMap.GetString( "GroupType" ).AsGuid() );
            int attendanceRemindersSent = 0;
            if ( groupType.TakesAttendance && groupType.SendAttendanceReminder )
            {

                // Get the occurrence dates that apply
                var dates = new List<DateTime>();
                dates.Add( RockDateTime.Today );
                try
                {
                    string[] reminderDays = dataMap.GetString( "SendReminders" ).Split( ',' );
                    foreach ( string reminderDay in reminderDays )
                    {
                        if ( reminderDay.Trim() != string.Empty )
                        {
                            var reminderDate = RockDateTime.Today.AddDays( 0 - Convert.ToInt32( reminderDay ) );
                            if ( !dates.Contains( reminderDate ) )
                            {
                                dates.Add( reminderDate );
                            }
                        }
                    }
                }
                catch { }

                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var scheduleService = new ScheduleService( rockContext );
                var attendanceService = new AttendanceService( rockContext );

                var startDate = dates.Min();
                var endDate = dates.Max().AddDays( 1 );

                // Find all 'occurrences' for the groups that occur on the affected dates
                var occurrences = new Dictionary<int, List<DateTime>>();
                foreach ( var group in groupService
                    .Queryable( "Schedule" ).AsNoTracking()
                    .Where( g =>
                        g.GroupTypeId == groupType.Id &&
                        g.IsActive &&
                        g.Schedule != null &&
                        g.Members.Any( m =>
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.GroupRole.IsLeader &&
                            m.Person.Email != null &&
                            m.Person.Email != "" ) ) )
                {
                    // Add the group 
                    occurrences.Add( group.Id, new List<DateTime>() );

                    // Check for a iCal schedule
                    if ( !string.IsNullOrWhiteSpace( group.Schedule.iCalendarContent ) )
                    {
                        // If schedule has an iCal schedule, get occurrences between first and last dates
                        foreach ( var occurrence in group.Schedule.GetOccurrences( startDate, endDate ) )
                        {
                            var startTime = occurrence.Period.StartTime.Value;
                            if ( dates.Contains( startTime.Date ) )
                            {
                                occurrences[group.Id].Add( startTime );
                            }
                        }
                    }
                    else
                    {
                        // if schedule does not have an iCal, then check for weekly schedule and calculate occurrences starting with first attendance or current week
                        if ( group.Schedule.WeeklyDayOfWeek.HasValue )
                        {
                            foreach ( var date in dates )
                            {
                                if ( date.DayOfWeek == group.Schedule.WeeklyDayOfWeek.Value )
                                {
                                    var startTime = date;
                                    if ( group.Schedule.WeeklyTimeOfDay.HasValue )
                                    {
                                        startTime = startTime.Add( group.Schedule.WeeklyTimeOfDay.Value );
                                    }
                                    occurrences[group.Id].Add( startTime );
                                }
                            }
                        }
                    }
                }

                // Remove any occurrences during group type exclusion date ranges
                foreach ( var exclusion in groupType.GroupScheduleExclusions )
                {
                    if ( exclusion.Start.HasValue && exclusion.End.HasValue )
                    {
                        foreach ( var keyVal in occurrences )
                        {
                            foreach ( var occurrenceDate in keyVal.Value.ToList() )
                            {
                                if ( occurrenceDate >= exclusion.Start.Value &&
                                    occurrenceDate < exclusion.End.Value.AddDays( 1 ) )
                                {
                                    keyVal.Value.Remove( occurrenceDate );
                                }
                            }
                        }
                    }
                }

                // Remove any 'occurrenes' that already have attendance data entered
                foreach ( var occurrence in attendanceService
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.StartDateTime >= startDate &&
                        a.StartDateTime < endDate &&
                        occurrences.Keys.Contains( a.GroupId.Value ) &&
                        a.ScheduleId.HasValue )
                    .Select( a => new
                    {
                        GroupId = a.GroupId.Value,
                        a.StartDateTime
                    } )
                    .Distinct()
                    .ToList() )
                {
                    occurrences[occurrence.GroupId].RemoveAll( d => d.Date == occurrence.StartDateTime.Date );
                }

                // Get the groups that have occurrences
                var groupIds = occurrences.Where( o => o.Value.Any() ).Select( o => o.Key ).ToList();

                // Get the leaders of those groups
                var leaders = groupMemberService
                    .Queryable( "Group,Person" ).AsNoTracking()
                    .Where( m =>
                        groupIds.Contains( m.GroupId ) &&
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        m.GroupRole.IsLeader &&
                        m.Person.Email != null &&
                        m.Person.Email != "" )
                    .ToList();

                // Loop through the leaders
                foreach ( var leader in leaders )
                {
                    foreach ( var group in occurrences.Where( o => o.Key == leader.GroupId ) )
                    {
                        var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields(  null, leader.Person );
                        mergeObjects.Add( "Person", leader.Person );
                        mergeObjects.Add( "Group", leader.Group );
                        mergeObjects.Add( "Occurrence", group.Value.Max() );

                        var recipients = new List<RecipientData>();
                        recipients.Add( new RecipientData( leader.Person.Email, mergeObjects ) );

                        var emailMessage = new RockEmailMessage( dataMap.GetString( "SystemEmail" ).AsGuid() );
                        emailMessage.SetRecipients( recipients );
                        emailMessage.Send();

                        attendanceRemindersSent++;
                    }
                }
            }

            context.Result = string.Format( "{0} attendance reminders sent", attendanceRemindersSent );
        }
    }
}