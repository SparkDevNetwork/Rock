using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    [GroupField("Base Level Group", description: "The base/root level group to look at.", true, order: 0, key:"BaseGroup")]
    [GroupTypeField("Group Type", description: "The group type of the groups to update.", required:true, order: 1, key:"GroupType")]
    [BooleanField("Include groups without members", "Include groups that do not contain any group members. Default is false", false, order: 2, key:"IncludeEmptyGroups")]
    [BooleanField("Overwrite Existing Codes", description:"Indicates if existing codes should be overwritten.", defaultValue: false, order: 3, key:"OverwriteCodes")]


    [DisallowConcurrentExecution]
    public class GenerateLGCodes : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var datamap = context.JobDetail.JobDataMap;
            var rockContext = new RockContext();
            var baseGroupGuid = datamap.GetString("BaseGroup").AsGuid();
            var groupType = GroupTypeCache.Get(datamap.GetString("GroupType").AsGuid(), rockContext);
            var includeEmptyGroups = datamap.GetBoolean("IncludeEmptyGroups");
            var overwriteCodes = datamap.GetBoolean("OverwriteCodes");


     
            var groupService = new GroupService(rockContext);
            var attributeService = new AttributeService(rockContext);
            var baseGroup = groupService.Get(baseGroupGuid);

            var descendantGroupIds = groupService.GetAllDescendentGroupIds(baseGroup.Id, false);

            var groupEntityTypeId = EntityTypeCache.Get(Rock.SystemGuid.EntityType.GROUP.AsGuid(), rockContext).Id;

            var groupCodeAttribute = attributeService.Queryable().AsNoTracking()
                .Where(a => a.EntityTypeId == groupEntityTypeId)
                .Where(a => a.EntityTypeQualifierColumn == "GroupTypeId")
                .Where(a => a.EntityTypeQualifierValue == groupType.Id.ToString())
                .Where(a => a.Key == "GroupCode")
                .SingleOrDefault();

            var isHotTopicAttribute = attributeService.Queryable().AsNoTracking()
                .Where(a => a.EntityTypeId == groupEntityTypeId)
                .Where(a => a.EntityTypeQualifierColumn == "GroupTypeId")
                .Where(a => a.EntityTypeQualifierValue == groupType.Id.ToString())
                .Where(a => a.Key == "IsHotTopic")
                .SingleOrDefault();

            if (isHotTopicAttribute == null)
            {
                throw new Exception("Hot Topic Attribute not found.");
            }

            if (groupCodeAttribute == null)
            {
                throw new Exception("Group Code Attribute not found.");
            }

            var attributeValueService = new AttributeValueService(rockContext);

            var groupCodeQry = attributeValueService.Queryable().AsNoTracking()
                .Where(v => v.AttributeId == groupCodeAttribute.Id)
                .Where(v => !(v.Value == null || v.Value.Equals("")));


            var hotTopicAttributeDefault = isHotTopicAttribute.DefaultValue.AsBoolean(false);

            var isHotTopic = attributeValueService.Queryable().AsNoTracking()
                .Where(v => v.AttributeId == isHotTopicAttribute.Id)
                .Select(v => new
                {
                    EntityId = v.EntityId,
                    ValueAsBoolean = v.ValueAsBoolean ?? hotTopicAttributeDefault
                });


            var groupQry = groupService.Queryable().Include("Campus").AsNoTracking()
                .Where(g => descendantGroupIds.Contains(g.Id))
                .Where(g => g.IsActive && !g.IsArchived)
                .GroupJoin(isHotTopic, g => g.Id, ht => ht.EntityId, (g, ht) => new { Group = g, HotTopic = ht })
                .SelectMany(x => x.HotTopic.DefaultIfEmpty(), (g, ht) => new { Group = g.Group, HotTopic = ht })
                .GroupJoin(groupCodeQry, g => g.Group.Id, gc => gc.EntityId, (g, gc) => new { Group = g, GroupCode = gc })
                .SelectMany(x => x.GroupCode.DefaultIfEmpty(), (g, gc) => new { Group = g.Group.Group, HotTopic = g.Group.HotTopic, GroupCode = gc })
                .Select(x => new { Group = x.Group, IsHotTopic = x.HotTopic == null ? hotTopicAttributeDefault : x.HotTopic.ValueAsBoolean, GroupCode = x.GroupCode.Value });

            if (!overwriteCodes)
            {
                groupQry = groupQry.Where(g => g.GroupCode == null || g.GroupCode == "");
            }

            if (!includeEmptyGroups)
            {
                groupQry = groupQry.Where(g => g.Group.Members.Where(m => !m.IsArchived && m.GroupMemberStatus != GroupMemberStatus.Inactive).Count() > 0);
            }

            foreach (var item in groupQry)
            {
                using (var groupContext = new RockContext())
                {
                    var group = new GroupService(groupContext).Get(item.Group.Id);
                    group.LoadAttributes(groupContext);
                    if (item.Group.CampusId.HasValue)
                    {
                        group.SetAttributeValue("GroupCode", GetGroupCode(item.Group.Campus.Guid, item.IsHotTopic));
                        group.SaveAttributeValue("GroupCode", groupContext);
                    }
                }
            }

        }

        private string GetGroupCode(Guid campusGuid, bool isHotTopic)
        {
            var context = new RockContext();
            var definedValueEntityId = EntityTypeCache.Get("53D4BF38-C49E-4A52-8B0E-5E016FB9574E".AsGuid(), context).Id;
            var definedTypeCacheItem = DefinedTypeCache.Get("289869FE-1CA8-45B7-BCE0-770446E35C4B".AsGuid(), context);

            DefinedValue definedValue = null;
            if (!isHotTopic)
            {
                var campusAttribute = new AttributeService(context).Queryable().AsNoTracking()
                    .Where(a => a.EntityTypeId == definedValueEntityId)
                    .Where(a => a.EntityTypeQualifierColumn == "DefinedTypeId")
                    .Where(a => a.EntityTypeQualifierValue == definedTypeCacheItem.Id.ToString())
                    .Where(a => a.Key == "Campus")
                    .SingleOrDefault();

                var campusAttributeValueQry = new AttributeValueService(context).Queryable().AsNoTracking()
                    .Where(av => av.AttributeId == campusAttribute.Id)
                    .Where(av => av.Value != null && av.Value != "");

                definedValue = new DefinedValueService(context).Queryable()
                    .Where(dv => dv.DefinedTypeId == definedTypeCacheItem.Id)
                    .Join(campusAttributeValueQry, dv => dv.Id, ca => ca.EntityId, (dv, ca) => new { DefinedValue = dv, ca })
                    .Where(v => v.ca.Value == campusGuid.ToString())
                    .Select(v => v.DefinedValue)
                    .FirstOrDefault();
            }
            else
            {
                definedValue = new DefinedValueService(context).Queryable()
                    .Where(dv => dv.DefinedTypeId == definedTypeCacheItem.Id)
                    .Where(dv => dv.Value == "Hot Topic")
                    .FirstOrDefault();
            }

            if (definedValue == null)
            {
                return String.Empty;
            }

            definedValue.LoadAttributes(context);
            int codeValue = definedValue.GetAttributeValue("CurrentValue").AsInteger() + 1;
            int maxValue = definedValue.GetAttributeValue("MaxValue").AsInteger();

            if (maxValue > 0 && maxValue < codeValue)
            {
                return string.Empty;
            }

            var groupCode = string.Concat(definedValue.GetAttributeValue("Prefix"), codeValue);

            definedValue.SetAttributeValue("CurrentValue", codeValue);
            definedValue.SaveAttributeValue("CurrentValue", context);

            return groupCode;

        }

    }
}
