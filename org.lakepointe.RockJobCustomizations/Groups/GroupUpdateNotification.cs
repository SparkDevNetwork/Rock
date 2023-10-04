using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;


namespace org.lakepointe.RockJobCustomizations.Groups
{
    [SystemCommunicationField("System Communication",
        Description = "The system communication that is sent that includes all the group updates.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.SystemCommunication)]
    [GroupTypeField("Group Type",
        Description = "The Group Type to report changes for.",
        IsRequired = true,
        DefaultValue = "a4f16049-2525-426e-a6e8-cdfb7b198664",
        Order = 1,
        Key = AttributeKeys.GroupType)]
    [CustomCheckboxListField("Update Types",
        Description = "The updates to be included in the report.",
        ListSource = "1^Inactivted,2^Group Name Change,3^New Groups",
        IsRequired = true,
        Order = 3,
        Key = AttributeKeys.UpdateTypes)]
    [IntegerField("Days Back",
        Description = "The number of days back to check for group updates.",
        IsRequired = false,
        DefaultIntegerValue = 7,
        Order = 3,
        Key = AttributeKeys.DaysBack)]
    [SecurityRoleField("Recepient Security Role",
        Description = "Security Role that contains the recipients of the email.",
        IsRequired = true,
        Order = 5,
        Key = AttributeKeys.SecurityRole)]

    [Description("Sends a scheduled communication listing the group changes for the specified group type for the previous week.")]
    [DisallowConcurrentExecution]
    public class GroupUpdateNotification : IJob
    {
        protected static class AttributeKeys
        {
            public const string SystemCommunication = "SystemCommunication";
            public const string DaysBack = "DaysBack";
            public const string GroupType = "GroupType";
            public const string UpdateTypes = "UpdateTypes";
            public const string SecurityRole = "SecurityRole";

        }


        const int _inactiveGroupHistoryType = 1;
        const int _groupswithNameChanges = 2;
        const int _newGroups = 3;

        public void Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var showInactivatedGroups = false;
            var showNameChanges = false;
            var showNewGroups = false;

            var rockContext = new RockContext();

            var historyTypes = dataMap.GetString(AttributeKeys.UpdateTypes).SplitDelimitedValues()
                .Select(h => h.AsIntegerOrNull())
                .Where(h => h != null);


            if (historyTypes == null || historyTypes.Count() == 0)
            {
                return;
            }

            showInactivatedGroups = historyTypes.Contains(_inactiveGroupHistoryType);
            showNameChanges = historyTypes.Contains(_groupswithNameChanges);
            showNewGroups = historyTypes.Contains(_newGroups);

            var groupTypeGuid = dataMap.GetString(AttributeKeys.GroupType).AsGuidOrNull();

            if (!groupTypeGuid.HasValue)
            {
                return;
            }

            var systemCommunicationGuid = dataMap.GetString(AttributeKeys.SystemCommunication).AsGuidOrNull();

            if (!systemCommunicationGuid.HasValue)
            {
                return;
            }

            var securityRoleGuid = dataMap.GetString(AttributeKeys.SecurityRole).AsGuid();
            var securityRoleMembers = new GroupService(rockContext).Get(securityRoleGuid).ActiveMembers();

            var groupType = GroupTypeCache.Get(groupTypeGuid.Value);
            var groupEntityType = EntityTypeCache.Get(Rock.SystemGuid.EntityType.GROUP.AsGuid());

            var groupService = new GroupService(rockContext);
            var groupQry = groupService.Queryable().AsNoTracking();

            var startDate = RockDateTime.Today.AddDays(- dataMap.GetString(AttributeKeys.DaysBack).AsInteger());
            var endDate = RockDateTime.Today;

            var historyQry = new HistoryService(rockContext)
                .Queryable().AsNoTracking()
                .Where(h => h.EntityTypeId == groupEntityType.Id)
                .Where(h => h.CreatedDateTime >= startDate)
                .Where(h => h.CreatedDateTime < endDate);


            var inactivatedGroups = new List<GroupChangeHistory>();
            if (showInactivatedGroups)
            {
                inactivatedGroups.AddRange(historyQry
                    .Join(groupQry, h => h.EntityId, g => g.Id,
                        (h, g) => new { History = h, Group = g })
                    .Where(g => g.Group.GroupTypeId == groupType.Id)
                    .Where(g => g.Group.IsActive == false || g.Group.IsArchived == true)
                    .Where(h => h.History.Verb == "MODIFY")
                    .Where(h => h.History.ChangeType == "Property")
                    .Where(h => h.History.ValueName == "Active")
                    .Where(h => h.History.NewValue == "False")
                    .Select(h => new GroupChangeHistory
                    {
                        GroupId = h.Group.Id,
                        Name = h.Group.Name,
                        CampusId = h.Group.CampusId,
                        CampusName = h.Group.Campus.Name,
                        ChangeType = "Inactivated",
                        OldValue = h.History.OldValue,
                        NewValue = h.History.NewValue,
                        ChangeDate = h.History.CreatedDateTime,
                        ChangedBy = h.History.CreatedByPersonAlias.Person.NickName + " " + h.History.CreatedByPersonAlias.Person.LastName
                    }).ToList());

                var inactivatedGroupIds = inactivatedGroups.Select(g => g.GroupId).ToList();

                var parentGroupIds = groupService.Queryable().AsNoTracking()
                    .Where(g => inactivatedGroupIds.Contains(g.ParentGroupId ?? -1))
                    .Select(g => g.ParentGroupId)
                    .Distinct()
                    .ToList();

                inactivatedGroups.RemoveAll(g => parentGroupIds.Contains(g.GroupId));

            }

            var groupNameChanges = new List<GroupChangeHistory>();
            if (showNameChanges)
            {
                groupNameChanges.AddRange(historyQry
                    .Join(groupQry, h => h.EntityId, g => g.Id,
                        (h, g) => new { History = h, Group = g })
                    .Where(g => g.Group.GroupTypeId == groupType.Id)
                    .Where(h => h.History.Verb == "MODIFY")
                    .Where(h => h.History.ChangeType == "Property")
                    .Where(h => h.History.ValueName == "Name")
                    .Where(h => h.History.OldValue != null && h.History.OldValue != "")
                    .Select(h => new GroupChangeHistory
                    {
                        GroupId = h.Group.Id,
                        Name = h.Group.Name,
                        CampusId = h.Group.CampusId,
                        CampusName = h.Group.Campus.Name,
                        ChangeType = "Name Change",
                        OldValue = h.History.OldValue,
                        NewValue = h.History.NewValue,
                        ChangeDate = h.History.CreatedDateTime,
                        ChangedBy = h.History.CreatedByPersonAlias.Person.NickName + " " + h.History.CreatedByPersonAlias.Person.LastName
                    }).ToList());

                var inactivatedGroupIds = groupNameChanges.Select(g => g.GroupId).ToList();

                var parentGroupIds = groupService.Queryable().AsNoTracking()
                    .Where(g => inactivatedGroupIds.Contains(g.ParentGroupId ?? -1))
                    .Select(g => g.ParentGroupId)
                    .Distinct()
                    .ToList();

                groupNameChanges.RemoveAll(g => parentGroupIds.Contains(g.GroupId));


            }

            var newGroups = new List<GroupChangeHistory>();
            if (showNewGroups)
            {
                newGroups.AddRange(historyQry
                    .Join(groupQry, h => h.EntityId, g => g.Id,
                    (h, g) => new { History = h, Group = g })
                    .Where(g => g.Group.GroupTypeId == groupType.Id)
                    .Where(g => g.Group.IsPublic)
                    .Where(g => g.History.Verb == "ADD")
                    .Where(g => g.History.ChangeType == "Record")
                    .Where(g => g.History.ValueName == "Group")
                    .Select(h => new GroupChangeHistory
                    {
                        GroupId = h.Group.Id,
                        Name = h.Group.Name,
                        CampusId = h.Group.CampusId,
                        CampusName = h.Group.Campus.Name,
                        ChangeType = "New Group",
                        NewValue = h.History.NewValue,
                        ChangeDate = h.History.CreatedDateTime,
                        ChangedBy = h.History.CreatedByPersonAlias.Person.NickName + " " + h.History.CreatedByPersonAlias.Person.LastName
                    }).ToList());

                var inactivatedGroupIds = newGroups.Select(g => g.GroupId).ToList();

                var parentGroupIds = groupService.Queryable().AsNoTracking()
                    .Where(g => inactivatedGroupIds.Contains(g.ParentGroupId ?? -1))
                    .Select(g => g.ParentGroupId)
                    .Distinct()
                    .ToList();

                newGroups.RemoveAll(g => parentGroupIds.Contains(g.GroupId));


            }


            int counter = 0;
            foreach (var m in securityRoleMembers)
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(null);
                mergeFields.Add("StartDate", startDate);
                mergeFields.Add("EndDate", endDate.AddDays(-1));
                mergeFields.Add("NameChanges", groupNameChanges.OrderBy(g => g.Name));
                mergeFields.Add("Deactivations", inactivatedGroups.OrderBy(g => g.Name));
                mergeFields.Add("NewGroups", newGroups.OrderBy(g => g.Name));
                mergeFields.Add("Person", m.Person);

                var recipients = new List<RockEmailMessageRecipient>();
                var emailMessage = new RockEmailMessage(systemCommunicationGuid.Value);
                recipients.Add(new RockEmailMessageRecipient(m.Person, mergeFields));

                emailMessage.SetRecipients(recipients);

                emailMessage.Send();
                counter++;
            }

            context.Result = string.Format("{0} notifications sent.", counter);

        }

        private class GroupChangeHistory : ILavaDataDictionarySource
        {

            public int GroupId { get; set; }
            public string Name { get; set; }
            public int? CampusId { get; set; }

            public string CampusName { get; set; }

            public string ChangeType { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
            public DateTime? ChangeDate { get; set; }
            public string ChangedBy { get; set; }

            [Rock.Lava.LavaHidden]
            public List<string> AvailableKeys
            {
                get
                {
                    var availableKeys = new List<string> { "GroupId", "Name","CampusId","CampusName", "ChangeType", "OldValue", "NewValue", "ChangeDate", "ChangedBy" };
                    return availableKeys;
                }
            }

            public bool ContainsKey(object key)
            {
                var additionalKeys = new List<string> { "GroupId", "Name", "CampusId", "CampusName", "ChangeType", "OldValue", "NewValue", "ChangeDate", "ChangedBy" };

                return additionalKeys.Contains(key.ToStringSafe());
            }

            public ILavaDataDictionary GetLavaDataDictionary()
            {
                var dictionary = new LavaDataDictionary()
                {
                    { "GroupId", GroupId },
                    { "Name", Name },
                    { "CampusId", CampusId },
                    { "CampusName", CampusName },
                    { "ChangeType", ChangeType },
                    { "OldValue", OldValue },
                    { "NewValue", NewValue },
                    { "ChangeDate", ChangeDate },
                    { "ChangedBy", ChangedBy }
                };
                return dictionary;
            }
        }
    }
}
