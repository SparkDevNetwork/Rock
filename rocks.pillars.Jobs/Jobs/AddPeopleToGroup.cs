using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace rocks.pillars.Jobs.Jobs
{
    [DataViewField("Data View", "The data view that contains the list of people that will enter into the selected group.", false, "", "Rock.Model.Person", "", 0)]
    [CodeEditorField("SQL Query", "Optional SQL query to run for list of people to be added it should have column name PersonId", CodeEditorMode.Sql, CodeEditorTheme.Rock, 200, false, "", "", 1, "SQLQuery")]
    [GroupField("Group", "The group that the people will go into.", true, "", "", 2)]
    [IntegerField("Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL or DataView queries to complete. Leave blank to use the default (30 seconds).", false, 180, "", 3, "CommandTimeout")]

    [DisallowConcurrentExecution]
    public class AddPeopleToGroup : IJob
    {

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public AddPeopleToGroup()
        {
        }

        /// <summary>
        /// Job that will run CSharp code.
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute(IJobExecutionContext context)
        {
            int membersAdded = 0;

            // load job settings
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            Guid? dataViewGuid = dataMap.GetString("DataView").AsGuidOrNull();
            string query = dataMap.GetString("SQLQuery");
            Guid? groupGuid = dataMap.GetString("Group").AsGuidOrNull();
            int? commandTimeout = dataMap.GetString("CommandTimeout").AsIntegerOrNull();

            List<int> peopleList = new List<int>();

            if (groupGuid.HasValue)
            {
                var rockContext = new RockContext();

                if (dataViewGuid.HasValue)
                {
                    var dvArgs = new DataViewGetQueryArgs
                    {
                        DbContext = rockContext,
                        DatabaseTimeoutSeconds = commandTimeout
                    };
                    var dataViewQuery = new DataViewService(rockContext).Get(dataViewGuid.Value).GetQuery( dvArgs );

                    peopleList.AddRange(dataViewQuery.Select(q => q.Id).ToList());
                }

                if (query.IsNotNullOrWhiteSpace())
                {
                    DataSet ds = DbService.GetDataSet(query, System.Data.CommandType.Text, null, commandTimeout);
                    if (ds.Tables.Count != 0)
                    {
                        var dt = ds.Tables[0];

                        if (dt.Columns.Contains("PersonId"))
                        {
                            var peopleFromQuery = dt.Rows.Cast<DataRow>().Select(r => r["PersonId"].ToString().AsInteger());

                            peopleList.AddRange(peopleFromQuery);
                        }
                    }

                }

                peopleList = peopleList.Distinct().ToList();

                var personIdsAlreadyAdded = new GroupMemberService(rockContext)
                    .Queryable().AsNoTracking()
                    .Where(g => g.Group.Guid == groupGuid.Value)
                    .Select(g => g.Person.Id)
                    .ToList();

                // add new people to the current survey group
                var peopleToAdd = peopleList.Where(id => !personIdsAlreadyAdded.Contains(id));
                if (peopleToAdd.Any())
                {
                    Group groupToAdd = null;
                    groupToAdd = new GroupService(rockContext).Get(groupGuid.Value);

                    if (groupToAdd != null && groupToAdd.GroupType.DefaultGroupRoleId.HasValue)
                    {
                        using (var groupMemberContext = new RockContext())
                        {
                            var groupMemberService = new GroupMemberService(groupMemberContext);

                            var newGroupMembers = peopleToAdd
                                .Select(id => new GroupMember
                                {
                                    PersonId = id,
                                    GroupId = groupToAdd.Id,
                                    GroupMemberStatus = GroupMemberStatus.Active,
                                    GroupRoleId = groupToAdd.GroupType.DefaultGroupRoleId.Value
                                });

                            membersAdded = newGroupMembers.Count();

                            groupMemberService.AddRange(newGroupMembers);

                            groupMemberContext.SaveChanges();
                        }
                    }
                    else
                    {
                        context.UpdateLastStatusMessage("Could not find group to update.");
                    }
                }

                context.UpdateLastStatusMessage(string.Format("{0} members added to the group.", membersAdded));
            }
            else
            {
                context.UpdateLastStatusMessage("Could not run.  Please check the job settings. Make sure the Group have a value");
            }
        }
    }
}
