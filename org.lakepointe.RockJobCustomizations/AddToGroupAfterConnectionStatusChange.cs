using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Data.Entity;


namespace org.lakepointe.RockJobCustomizations
{

    [GroupField("Group To Add People To",
            Key = AttributeKey.NewsletterGroupKey,
            Description = "Newsletter group to add people to.",
            DefaultValue = "",
            Order = 0)]
    [CampusesField(name: "Campuses",
            description: "Select campuses that this newsletter applies to.",
            required: false,
            defaultCampusGuids: "",
            category: "",
            order: 1,
            key: AttributeKey.CampusesKey)]
    [DefinedValueField("Connection Statuses to add to the group. ",
            Key = AttributeKey.ConnectionStatusesKey,
            AllowMultiple = true,
            DefaultValue = "",
            DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
            Order = 2)]
    [IntegerField("Maximum Days Back",
            Key = AttributeKey.MaxDaysBackKey,
            Description = "The maximum number of days to look back for connection status changes.",
            DefaultIntegerValue = 7,
            Order = 4)]

    [DisallowConcurrentExecution]
    public class AddPersonToGroupAfterConnectionStatusChange : IJob
    {
        private static class AttributeKey
        {
            public const string CampusesKey = "Campuses";
            public const string ConnectionStatusesKey = "ConnectionStatuses";
            public const string NewsletterGroupKey = "NewsletterGroup";
            public const string LastRunDateKey = "LastRunDate";
            public const string MaxDaysBackKey = "MaxDaysBack";
        }

        #region Fields
        private Group _newsletterGroup;
        private List<DefinedValueCache> _connectionStatuses;
        private List<CampusCache> _campuses;

        private RockContext _rockContext;

        #endregion

        #region Properties
        private Group NewsletterGroup
        {
            get
            {
                return _newsletterGroup;
            }
            set
            {
                _newsletterGroup = value;
            }
        }

        private List<DefinedValueCache> ConnectionStatuses
        {
            get
            {
                return _connectionStatuses;
            }
            set
            {
                _connectionStatuses = value;
            }
        }

        private List<CampusCache> Campuses
        {
            get
            {
                return _campuses;
            }
            set
            {
                _campuses = value;
            }
        }


        private DateTime? LastRunTime { get; set; }
        private int MaxDaysBack { get; set; }

        #endregion
        public void Execute(IJobExecutionContext context)
        {
            var datamap = context.JobDetail.JobDataMap;
            _rockContext = new RockContext();

            LoadJobAttributeValues(datamap);
            GetLastRunTime(context.GetJobId());
            var people = GetPeopleWithStatusChanges();

            var defaultRoleId = GroupTypeCache.Get(NewsletterGroup.GroupTypeId, _rockContext).DefaultGroupRoleId ?? 0;
            int counter = 0;
            foreach (var person in people)
            {
                using (var gmContext = new RockContext())
                {
                    var groupMemberService = new GroupMemberService(gmContext);
                    var group = new GroupService(gmContext).Get(NewsletterGroup.Id);
                    var gm = groupMemberService.GetByGroupIdAndPersonId(NewsletterGroup.Id, person.Id).FirstOrDefault();

                    if (gm == null || gm.Id <= 0)
                    {
                        counter++;
                        groupMemberService.AddOrRestoreGroupMember(group, person.Id, defaultRoleId);
                        gmContext.SaveChanges();
                    }
                }
            }

            var statusMessage = string.Format("{0} {1} added to the {2} Group", counter, "person".PluralizeIf(counter != 1), NewsletterGroup.Name);
            context.UpdateLastStatusMessage(statusMessage);

        }


        private List<Person> GetPeopleWithStatusChanges()
        {
            var personEntityType = EntityTypeCache.Get(Rock.SystemGuid.EntityType.PERSON.AsGuid(), _rockContext);

            var historyService = new HistoryService(_rockContext);
            var connectionStatusIdsStr = ConnectionStatuses.Select(c => c.Id.ToString()).ToList();

            // get the Id of the people who had a connection status change within the window
            // of who should be add to the group
            var historyPeopleIds = historyService.Queryable().AsNoTracking()
                .Where(h => h.EntityTypeId == personEntityType.Id)
                .Where(h => h.CreatedDateTime > LastRunTime)
                .Where(h => h.ChangeType == "Property")
                .Where(h => h.ValueName == "Connection Status")
                .Where(h => connectionStatusIdsStr.Contains(h.NewRawValue))
                .Select(h => h.EntityId)
                .Distinct()
                .ToList();


            var inactiveRecordStatus = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid(), _rockContext);
            var connectionStatusIds = ConnectionStatuses.Select(c => c.Id).ToList();

            var campusId = Campuses.Select(c => c.Id).ToList();

            //get the people with the status change who's connection statuses are still
            // eligible to be added to the group automatically
            if (historyPeopleIds.Count > 0)
            {
                var people = new PersonService(_rockContext).Queryable().AsNoTracking()
                    .Where(p => historyPeopleIds.Contains(p.Id))
                    .Where(p => connectionStatusIds.Contains(p.ConnectionStatusValueId ?? 0))
                    .Where(p => campusId.Contains(p.PrimaryCampusId ?? 0))
                    .Where(p => p.RecordStatusValueId != inactiveRecordStatus.Id)
                    .ToList();
                return people;
            }

            return new List<Person>();

        }

        /// <summary>
        /// Loads the JOb attribute values
        /// </summary>
        /// <param name="datamap"></param>
        private void LoadJobAttributeValues(JobDataMap datamap)
        {
            NewsletterGroup = new GroupService(_rockContext).Get(datamap.GetString(AttributeKey.NewsletterGroupKey).AsGuid());

            var connectionStatusGuids = datamap.GetString(AttributeKey.ConnectionStatusesKey).SplitDelimitedValues()
                .Select(v => v.AsGuid())
                .Where(v => !v.Equals(Guid.Empty))
                .ToList();

            ConnectionStatuses = DefinedTypeCache.Get(Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid(), _rockContext)
                 .DefinedValues.Where(v => connectionStatusGuids.Contains(v.Guid))
                 .ToList();

            var campusGuids = datamap.GetString(AttributeKey.CampusesKey).SplitDelimitedValues()
                .Select(v => v.AsGuid())
                .Where(v => !v.Equals(Guid.Empty))
                .ToList();

            Campuses = CampusCache.All().Where(c => campusGuids.Contains(c.Guid)).ToList();
            MaxDaysBack = datamap.GetString(AttributeKey.MaxDaysBackKey).AsInteger();

        }

        /// <summary>
        /// Gets the last time that the job ran successfully. If it hasn't run in the maximum days back, it will
        /// default to that value.
        /// </summary>
        /// <param name="jobId">The job id</param>
        private void GetLastRunTime(int jobId)
        {

            var jobInstance = new ServiceJobService(_rockContext).Get(jobId);

            if (jobInstance != null)
            {
                LastRunTime = jobInstance.LastSuccessfulRunDateTime;
            }

            // if it hasn't run within the maximum days back set to the limit
            if (!LastRunTime.HasValue || LastRunTime < RockDateTime.Today.AddDays(-MaxDaysBack))
            {
                LastRunTime = RockDateTime.Today.AddDays(-MaxDaysBack);
            }

        }

    }
}

