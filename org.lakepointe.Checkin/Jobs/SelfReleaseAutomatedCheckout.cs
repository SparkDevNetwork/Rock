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

namespace org.lakepointe.Checkin.Jobs
{
    [DataViewField("Self Release DataView", "DataView that contains children who are eligible for self release.", true, Order = 0)]
    [TextField("Self Release Groups", "A comma delimited list of group Ids that are enabled for self release.", true, Order = 1)]
    [BooleanField("Include Child Groups", "Include child/descendant groups. Default is true", true, Order = 2)]
    [IntegerField("Command Timeout", "SQL Command Timeout in seconds. Default is 30.", false, 30, Order = 3)]
    [DisallowConcurrentExecution]
    public class SelfReleaseAutomatedCheckout : IJob
    {

        public void Execute( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;
            var selfReleaseDataViewGuid = dataMap.GetString( "SelfReleaseDataView" ).AsGuid();
            var baseGroupIds = dataMap.GetString( "SelfReleaseGroups" ).SplitDelimitedValues().AsIntegerList();
            var includeChildGroups = dataMap.GetBoolean( "IncludeChildGroups" );
            var commandTimeout = dataMap.GetInt( "CommandTimeout" );

            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = commandTimeout;

            if ( selfReleaseDataViewGuid == Guid.Empty )
            {
                context.UpdateLastStatusMessage( "Job Aborted - Self Release DataView Not Provided." );
                return;
            }

            if ( baseGroupIds.Count == 0 )
            {
                context.UpdateLastStatusMessage( "Job Aborted - No Groups Provided." );
                return;
            }

            var dataView = new DataViewService( rockContext ).Get( selfReleaseDataViewGuid );

            if ( dataView == null )
            {
                context.UpdateLastStatusMessage("Job Aborted - Self Release DataView Not Found.");
                return;
            }

            var groupService = new GroupService( rockContext );
            var selfReleaseGroupIds = new List<int>();

            foreach ( var id in baseGroupIds )
            {
                var group = groupService.Get( id );

                if ( group == null || !group.IsActive )
                {
                    continue;
                }

                selfReleaseGroupIds.Add( id );
                if ( includeChildGroups )
                {
                    selfReleaseGroupIds.AddRange(
                            groupService.GetAllDescendentGroupIds(id, false));
                }
            }

            if ( selfReleaseGroupIds.Count == 0 )
            {
                context.UpdateLastStatusMessage( "No Active Groups Found." );
                return;
            }

            var personService = new PersonService( rockContext );
            var paramExpression = personService.ParameterExpression;
            var whereExpression = dataView.GetExpression( personService, paramExpression );

            var selfReleasePersonIds = personService.Queryable( false, false ).AsNoTracking()
                .Where( paramExpression, whereExpression, null )
                .Select( p => p.Id )
                .ToList();

            var attendanceContext = new RockContext();
            attendanceContext.Database.CommandTimeout = commandTimeout;

            var attendanceSvc = new AttendanceService( attendanceContext );
            var today = RockDateTime.Today;
            var tomorrow = today.AddDays( 1 );
            int counter = 0;
            var attendance = attendanceSvc.Queryable()
                .Where( a => a.DidAttend.HasValue
                         && a.DidAttend.Value
                         && a.StartDateTime >= today
                         && a.StartDateTime < tomorrow
                         && !a.EndDateTime.HasValue
                         && selfReleaseGroupIds.Contains( a.Occurrence.GroupId ?? -1 )
                         && selfReleasePersonIds.Contains( a.PersonAlias.PersonId )
                      ).ToList();

            foreach ( var a in attendance )
            {
                a.EndDateTime = RockDateTime.Now;
                counter++;
            }

            attendanceContext.SaveChanges();

            var statusMessage = string.Format( "{0} {1} checked out.", counter, "person".PluralizeIf( counter != 1 ) );

            context.UpdateLastStatusMessage( statusMessage );
        }
    }
}
