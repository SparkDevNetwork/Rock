using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSScriptLibrary;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace rocks.pillars.Jobs.Jobs
{
    /// <summary>
    /// Job that executes CSharp code.
    /// </summary>
    [GroupTypeField("Registration Group Type", "Group Type of the groups that will have this dynamic registration process" )]
    [IntegerField("Close Registration", "How many days before the group meets to close the registration (Make group private)")]
    [IntegerField("Open Registration", "How many days after the group meets to open the registration (Make group public and archive members)")]

    [DisallowConcurrentExecution]
    public class DynamicGroupRegistration : IJob
    {
        private DateTime dateTimeNow;
        private int daysCloseRegistration;
        private int daysOpenRegistration;

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var groupTypeGuid = dataMap.GetString("RegistrationGroupType").AsGuidOrNull();
            var close = dataMap.GetString("CloseRegistration").AsIntegerOrNull();
            var open = dataMap.GetString("OpenRegistration").AsIntegerOrNull();

            if(groupTypeGuid.HasValue && close.HasValue && open.HasValue)
            {
                dateTimeNow = RockDateTime.Now;
                daysCloseRegistration = close.Value;
                daysOpenRegistration = open.Value * -1;  //Since this is days after

                var rockContext = new RockContext();
                var groupService = new GroupService(rockContext);

                var groups = groupService.AsNoFilter().AsQueryable()
                                    .Where(g => g.IsActive && !g.IsArchived && g.GroupType.Guid == groupTypeGuid.Value && g.ScheduleId.HasValue)
                                    .ToList();

                if (groups.Count > 0)
                {
                    CloseRegistrations(groups);

                    OpenRegistrationsAndArchiveMembers(groups);

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Look through Groups of the designated Group Type and if the 
        /// group meets in daysCloseRegistration days it will set the 
        /// group to private 
        /// </summary>
        private void CloseRegistrations(List<Rock.Model.Group> groups)
        {
            foreach(var g in groups)
            {
                var nextDate = g.Schedule.GetNextStartDateTime(dateTimeNow);

                if(nextDate.HasValue && (nextDate.Value - dateTimeNow).Days <= daysCloseRegistration)
                {
                    g.IsPublic = false;
                }
            }
        }

        /// <summary>
        /// Look through Groups of the designated Group Type and if the 
        /// group met daysOpenRegistration days ago it will set the 
        /// group to public 
        /// </summary>
        private void OpenRegistrationsAndArchiveMembers(List<Rock.Model.Group> groups)
        {
            var dateDaysAgoBeg = dateTimeNow.AddDays(daysOpenRegistration).Date;
            var dateDaysAgoEnd = dateDaysAgoBeg.AddHours(24);

            foreach(var g in groups)
            {
                var nextDateTimes = g.Schedule.GetScheduledStartTimes(dateDaysAgoBeg, dateDaysAgoEnd);

                if(nextDateTimes.Count > 0)
                {
                    g.IsPublic = true;

                    var nonLeaders = g.ActiveMembers().Where(gm => !gm.GroupRole.IsLeader && !gm.IsArchived).ToList();
                    nonLeaders.ForEach(gm => { gm.IsArchived = true; });
                }
            }
        }
    }
}
