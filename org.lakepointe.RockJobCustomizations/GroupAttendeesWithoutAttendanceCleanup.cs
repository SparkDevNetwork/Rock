using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.lakepointe.RockJobCustomizations
{
    [GroupTypeField("Group Type", description: "The Group Type of the group to check against", required: true, order: 0, key: "GroupType")]
    [IntegerField("Days Without Attendance", description: "How may days without attendance/activity before removing from group", required: true, defaultValue: 365, order: 2, key: "DaysWithoutAttendance")]
    [BooleanField("Include Inactive Members", description: "Include group mebers in cleanup who are inactive.", defaultValue: true, order: 3, key: "IncludeInactive")]

    [DisallowConcurrentExecution]
    public class GroupAttendeesWithoutAttendanceCleanup : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var datamap = context.JobDetail.JobDataMap;
            var rockContext = new RockContext();

            var groupType = GroupTypeCache.Get(datamap.GetString("GroupType").AsGuid(), rockContext);
            var dayswithoutAttendance = datamap.GetInt("DaysWithoutAttendance");
            var earliestLastActivityDate = DateTime.Now.AddDays(-dayswithoutAttendance).Date;
            var recordsProcessed = 0;


            var includeInactive = datamap.GetBoolean("IncludeInactive");

            var groupMemberIdsToRemove = rockContext.Database.SqlQuery<int>(
                @"
                ;WITH lifegroupids AS(
                        SELECT g.Id
                        FROM[Group] g
                        INNER JOIN GroupMember gm on g.Id = gm.GroupId
                        WHERE GroupTypeId = @GroupTypeId  and g.IsActive = 1 and g.IsArchived = 0
                        GROUP BY g.Id, g.Name
                        HAVING COUNT(gm.Id) > 0
                    ),
                    lgAttendance as
                (
                    SELECT pa.PersonId, ao.GroupId, Max(a.StartDateTime) as LastAttended, count(a.Id) as TimesAttended
                    FROM lifegroupids lg
                    INNER JOIN AttendanceOccurrence ao on lg.Id = ao.GroupId
                    INNER JOIN Attendance a on ao.Id = a.OccurrenceId
                    INNER JOIN PersonAlias pa on a.PersonAliasId = pa.Id
                    WHERE a.DidAttend = 1
                    GROUP BY pa.PersonId, ao.GroupId
                )
                SELECT
                    gm.Id as GroupMemberId
                FROM[Group] g
                INNER JOIN lifegroupids lgi on g.Id = lgi.Id
                INNER JOIN Campus c on g.CampusId = c.Id
                INNER Join GroupMember gm on g.Id = gm.GroupId
                LEFT OUTER JOIN lgAttendance lga on gm.GroupId = lga.GroupId and gm.PersonId = lga.PersonId
                INNER JOIN Person p on gm.PersonId = p.Id
                WHERE((LastAttended is null and gm.CreatedDateTime <= @LastAttendedMinimum) or LastAttended <= @LastAttendedMinimum OR GroupMemberStatus = 0) 
		                and gm.IsArchived = 0",
                new SqlParameter("@GroupTypeId", groupType.Id),
                new SqlParameter("@LastAttendedMinimum", earliestLastActivityDate)
            ).ToList();

            var totalRecords = groupMemberIdsToRemove.Count();

            foreach (var groupMemberId in groupMemberIdsToRemove)
            {
                using (var gmContext = new RockContext())
                {
                    var groupMemberService = new GroupMemberService(gmContext);
                    var groupMember = groupMemberService.Get(groupMemberId);

                    if (groupMember.GroupMemberStatus != GroupMemberStatus.Inactive || includeInactive)
                    {
                        //will archive instead of delete if Group History is turned on.
                        groupMemberService.Delete(groupMember);
                    }

                    gmContext.SaveChanges();

                    recordsProcessed++;

                    if (recordsProcessed % 100 == 0)
                    {
                        UpdateStatusMessage(recordsProcessed, totalRecords, context);
                    }

                }
            }

            UpdateStatusMessage(recordsProcessed, totalRecords, context);
        }

        private void UpdateStatusMessage(int processed, int total, IJobExecutionContext context)
        {
            var message = string.Format("{0} of {1} records processed.", processed, total);
            context.UpdateLastStatusMessage(message);
        }
    }
}
