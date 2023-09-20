using System.Collections.Generic;
using System.Linq;
using System;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace rocks.pillars.Jobs
{
    [DataViewField("Data View", "The data view that contains the list of people that will enter into the selected group.", false)]
    [BooleanField("Current Sunday", "Yes", "No", "The attendance occurrence will be saved as the Current sunday date, otherwise it will be saved the date of the job run")]
    [GroupField("Group", "The group that the attendance will be added to")]
    [IntegerField("Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL or DataView queries to complete. Leave blank to use the default (30 seconds).", false, 180, "", 4, "CommandTimeout")]
    public class AddAttendanceToFamilyCustom : IJob
    {
        public AddAttendanceToFamilyCustom()
        {
        }

        public virtual void Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;

            using(var rockContext = new RockContext())
            {
                int? commandTimeout = dataMap.GetString("CommandTimeout").AsIntegerOrNull();
                var dataViewGuid = dataMap.GetString("DataView").AsGuidOrNull();
                var groupGuid = dataMap.GetString("Group").AsGuid();
                var useCurrentSunday = dataMap.GetString("CurrentSunday").AsBoolean();
                var counter = 0;

                var dateOccurrence = RockDateTime.Today;

                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(null);
                List<Person> people = new List<Person>();
                var groupService = new GroupService(rockContext);
                var group = groupService.GetByGuid(groupGuid);

                if (group != null)
                {
                    if(dataViewGuid.HasValue)
                    {
                        people.AddRange(GetPeopleFromDataView(dataViewGuid.Value, commandTimeout, rockContext));
                    }

                    //Set the Sunday Date
                    if(useCurrentSunday)
                    {
                        dateOccurrence = dateOccurrence.DayOfWeek == DayOfWeek.Monday ?
                                RockDateTime.GetSundayDate(dateOccurrence).AddDays(-7) :
                                RockDateTime.GetSundayDate(dateOccurrence);
                    }

                    var attendaceService = new AttendanceService(rockContext);
                    foreach(var per in people)
                    {
                        attendaceService.AddOrUpdate(per.PrimaryAliasId.Value, dateOccurrence, group.Id, null, null, null);
                        counter++;
                    }

                    if (counter > 0)
                    {
                        context.Result = $"Saved {counter} attendance records on {dateOccurrence.ToShortDateString()}";
                        rockContext.SaveChanges();
                    }
                }
                else
                {
                    context.Result = "There was no group entered to save the attendance to";
                }
            }
        }

        private List<Person> GetPeopleFromDataView(Guid dataViewGuid, int? commandTimeout, RockContext rockContext)
        {
            var dataViewService = new DataViewService(rockContext);
            var dataView = dataViewService.Get(dataViewGuid);
            if (dataView != null)
            {
                var dvArgs = new DataViewGetQueryArgs
                {
                    DbContext = rockContext,
                    DatabaseTimeoutSeconds = commandTimeout
                };
                return dataView.GetQuery( dvArgs ).OfType<Person>().ToList();
            }

            return new List<Person>();
        }
    }
}
