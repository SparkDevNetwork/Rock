using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using System.Data.Entity;
using Rock.Web.Cache;

namespace org.lakepointe.Checkin.Jobs
{
    [CategoryField( "Tag Categories", "The categories of tags to be checked against when automating group checkouts.", true, "Rock.Model.Tag", Order = 0 )]
    [BooleanField( "Include Child Groups", "Include child/descendant groups. Default is true", true, Order = 1 )]
    [DataViewField( "Optional Self Release DataView", "Use this Data View to optionally restrict the people that can be automatically checked out.", false, Order = 2, Key = "DataView" )]
    [IntegerField( "Command Timeout", "SQL Command Timeout in seconds. Default is 30.", false, 30, Order = 3 )]
    [DisallowConcurrentExecution]
    public class AutomatedCheckout : IJob
    {

        public void Execute( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;

            var commandTimeout = dataMap.GetInt( "CommandTimeout" );
            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = commandTimeout;

            var tagService = new TagService( rockContext );
            var groupEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.GROUP ).Id;
            var categoryGuids = dataMap.GetString( "TagCategories" ).SplitDelimitedValues().AsGuidList();
            var categoryTags = tagService.Queryable().AsNoTracking().Where( t => t.EntityTypeId == groupEntityTypeId && categoryGuids.Contains( t.Category.Guid ) ).ToList();

            if ( !categoryTags.Any() )
            {
                context.UpdateLastStatusMessage( "Job Aborted - No Configured Tag Categories." );
                return;
            }

            // We'll only match down to the minute to prevent accidental wierdness.
            DayOfWeek? scheduledWeekday = null;
            int? scheduledHour = null;
            int? scheduledMinute = null;

            if ( context.ScheduledFireTimeUtc.HasValue )
            {
                var scheduledDateTime = context.ScheduledFireTimeUtc.Value.LocalDateTime;
                scheduledWeekday = scheduledDateTime.DayOfWeek;
                scheduledHour = scheduledDateTime.Hour;
                scheduledMinute = scheduledDateTime.Minute;
            }
            else
            {
                context.UpdateLastStatusMessage( "Job Aborted - Error with Scheduled Fire Time." );
                return;
            }

            var scheduledTags = new List<Tag>();
            foreach ( var categoryTag in categoryTags )
            {
                categoryTag.LoadAttributes();
                DayOfWeek? dayOfWeek = categoryTag.GetAttributeValue( "DayOfWeek" ).ConvertToEnum<DayOfWeek>();
                var time = categoryTag.GetAttributeValue( "Time" ).AsTimeSpan();

                if ( dayOfWeek.HasValue && time.HasValue && dayOfWeek == scheduledWeekday )
                {
                    var hour = time.Value.Hours;
                    var minute = time.Value.Minutes;
                    if ( hour == scheduledHour && minute == scheduledMinute )
                    {
                        scheduledTags.Add( categoryTag );
                    }
                }
            }

            if ( !scheduledTags.Any() )
            {
                context.UpdateLastStatusMessage( "Job Aborted - No Group Tags Scheduled." );
                return;
            }

            var scheduledTagIds = scheduledTags.Select( t => t.Id ).ToList();
            var taggedItemService = new TaggedItemService( rockContext );
            var baseGroupGuids = taggedItemService.Queryable().AsNoTracking().Where( ti => scheduledTagIds.Contains( ti.TagId ) && ti.EntityTypeId == groupEntityTypeId ).Select( ti => ti.EntityGuid ).ToList();

            if ( !baseGroupGuids.Any() )
            {
                context.UpdateLastStatusMessage( "Job Aborted - No Groups Provided." );
                return;
            }

            var groupService = new GroupService( rockContext );
            var automatedCheckoutGroupIds = new List<int>();
            var includeChildGroups = dataMap.GetBoolean( "IncludeChildGroups" );
            foreach ( var guid in baseGroupGuids )
            {
                var group = groupService.Get( guid );

                if ( group != null && group.IsActive )
                {
                    automatedCheckoutGroupIds.Add( group.Id );
                    if ( includeChildGroups )
                    {
                        automatedCheckoutGroupIds.AddRange(
                                groupService.GetAllDescendentGroupIds( group.Id, false ) );
                    }
                }
            }

            if ( automatedCheckoutGroupIds.Count == 0 )
            {
                context.UpdateLastStatusMessage( "Job Aborted - No Active Groups Found." );
                return;
            }

            var selfReleasePersonIds = new List<int>();
            var dataViewGuid = dataMap.GetString( "DataView" ).AsGuid();
            var dataView = new DataViewService( rockContext ).Get( dataViewGuid );
            if ( dataView != null )
            {
                var personService = new PersonService( rockContext );
                var paramExpression = personService.ParameterExpression;
                var whereExpression = dataView.GetExpression( personService, paramExpression );

                selfReleasePersonIds = personService.Queryable( false, false ).AsNoTracking()
                    .Where( paramExpression, whereExpression, null )
                    .Select( p => p.Id )
                    .ToList();
            }

            var attendanceContext = new RockContext();
            attendanceContext.Database.CommandTimeout = commandTimeout;
            var attendanceSvc = new AttendanceService( attendanceContext );
            var today = RockDateTime.Today;
            var tomorrow = today.AddDays( 1 );
            var attendanceQry = attendanceSvc.Queryable()
                .Where( a => a.DidAttend.HasValue
                         && a.DidAttend.Value
                         && a.StartDateTime >= today
                         && a.StartDateTime < tomorrow
                         && !a.EndDateTime.HasValue
                         && automatedCheckoutGroupIds.Contains( a.Occurrence.GroupId ?? -1 ) );
            var attendanceList = attendanceQry.ToList();

            int counter = 0;
            foreach ( var a in attendanceList )
            {
                // logic here is a bit obtuse. Self-release is not applicable to all groups that might be part of this
                // execution of the job, so the self release dataview can't be expected to list everyone who _can_ check
                // out. It has to list the people who _can't_ check out. So if a person was included in the dataview,
                // don't check them out.
                // We could obviously remove these people via the EF query but EF seems to have difficulty translating
                // this "remove people with these ids" filter into efficient SQL. Doing it here instead because the SQL
                // query was timing out.
                if ( !selfReleasePersonIds.Contains( a.PersonAlias.PersonId ) )
                {
                    a.EndDateTime = RockDateTime.Now;
                    counter++;
                }
            }
            attendanceContext.SaveChanges();

            var statusMessage = string.Format( "{0} {1} checked out.", counter, "person".PluralizeIf( counter != 1 ) );
            context.UpdateLastStatusMessage( statusMessage );
        }
    }
}
