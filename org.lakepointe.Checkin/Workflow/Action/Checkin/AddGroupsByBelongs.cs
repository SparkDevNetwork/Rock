using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{
    [ActionCategory("LPC Check-In")]
    [Description("Adds back groups for each selected family member that they belong to.")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Add Group By Belongs")]
    [BooleanField("Load All", "By default groups are only loaded for the selected person, group type, and location.  Select this option to load groups for all the loaded people and group types.")]
    public class AddGroupsByBelongs : CheckInActionComponent
    {
        public override bool Execute(RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages)
        {
            var checkInState = GetCheckInState(entity, out errorMessages);

            if (checkInState != null)
            {
                bool loadAll = GetAttributeValue(action, "LoadAll").AsBoolean();

                foreach (var family in checkInState.CheckIn.GetFamilies(true))
                {
                    foreach (var person in family.GetPeople(!loadAll))
                    {
                        foreach (var kioskGroupType in checkInState.Kiosk.ActiveGroupTypes(checkInState.ConfiguredGroupTypes))
                        {
                            var kioskGroupIds = kioskGroupType.KioskGroups.Where(g => g.IsCheckInActive)
                                .Select(g => g.Group.Id)
                                .ToList();

                            var personBelongGroups = new GroupMemberService(rockContext).Queryable().AsNoTracking()
                                .Where(m => m.PersonId == person.Person.Id)
                                .Where(m => m.GroupMemberStatus == GroupMemberStatus.Active)
                                .Where(m => kioskGroupIds.Contains(m.GroupId))
                                .Select(m => m.Group);

                            foreach (var belongsGroup in personBelongGroups)
                            {
                                CheckInGroupType personGroupType = person.GroupTypes.Where(gt => gt.GroupType.Id == belongsGroup.GroupTypeId)
                                    .FirstOrDefault();

                                if (personGroupType != null && personGroupType.GroupType.Id > 0)
                                {
                                    if (personGroupType.ExcludedByFilter)
                                    {
                                        personGroupType.ExcludedByFilter = false;
                                    }
                                }
                                else
                                {
                                    personGroupType = new CheckInGroupType();
                                    personGroupType.GroupType = kioskGroupType.GroupType;
                                    personGroupType.ExcludedByFilter = false;
                                    person.GroupTypes.Add(personGroupType);
                                }

                                CheckInGroup personGroup = personGroupType.Groups.Where(g => g.Group.Id == belongsGroup.Id)
                                    .FirstOrDefault();

                                if (personGroup != null && personGroup.Group.Id > 0)
                                {
                                    if (personGroup.ExcludedByFilter)
                                    {
                                        personGroup.ExcludedByFilter = false;
                                    }
                                }
                                else
                                {
                                    var kioskGroup = kioskGroupType.KioskGroups.Where(kg => kg.Group.Id == belongsGroup.Id).First();
                                    personGroup = new CheckInGroup();
                                    personGroup.Group = kioskGroup.Group.Clone(false);
                                    personGroup.Group.CopyAttributesFrom(kioskGroup.Group);
                                    personGroup.ExcludedByFilter = false;
                                    personGroupType.Groups.Add(personGroup);
                                }
                            }
                        }

                        if (!person.GroupTypes.Where(gt => gt.Groups.Any()).Any())
                        {
                            // If we get here and the person doesn't have any available groups, look to see if we can put them in an unassigned group
                            foreach (var kioskGroupType in checkInState.Kiosk.ActiveGroupTypes(checkInState.ConfiguredGroupTypes))
                            {
                                var kioskGroupIds = kioskGroupType.KioskGroups.Where(g => g.IsCheckInActive)
                                    .Select(g => g.Group.Id)
                                    .ToList();

                                var kioskGroups = new GroupService(rockContext).Queryable().AsNoTracking()
                                    .Where(g => kioskGroupIds.Contains(g.Id));

                                foreach (var kioskGroup in kioskGroups)
                                {
                                    kioskGroup.LoadAttributes(rockContext);
                                    var unassignedGroupGradeRange = kioskGroup.GetAttributeValue("UnassignedGradeRange");
                                    var genderAttribute = kioskGroup.GetAttributeValue("Gender").ConvertToEnumOrNull<Gender>();

                                    if (genderAttribute.HasValue && genderAttribute != person.Person.Gender)
                                    {
                                        continue;
                                    }

                                    if (unassignedGroupGradeRange != null)
                                    {
                                        var unassignedGroupGradeRangeValues = unassignedGroupGradeRange.Split(new char[] { ',' }, StringSplitOptions.None).AsGuidOrNullList().ToArray();

                                        DefinedValueCache minGrade = null;
                                        DefinedValueCache maxGrade = null;
                                        if (unassignedGroupGradeRangeValues.Length == 2)
                                        {
                                            minGrade = unassignedGroupGradeRangeValues[0].HasValue ? DefinedValueCache.Get(unassignedGroupGradeRangeValues[0].Value) : null; // minimum Grade (highest offset)
                                            maxGrade = unassignedGroupGradeRangeValues[1].HasValue ? DefinedValueCache.Get(unassignedGroupGradeRangeValues[1].Value) : null;  //maximum Grade (lowest offset)
                                        }

                                        if (minGrade != null && maxGrade != null)
                                        {
                                            // remember that GradeOffset counts backward, so >= and <= are flipped from what you might expect
                                            if(person.Person.GradeOffset >= maxGrade.Value.AsInteger() && person.Person.GradeOffset <= minGrade.Value.AsInteger() )
                                            {
                                                CheckInGroupType personGroupType = person.GroupTypes
                                                    .Where(gt => gt.GroupType.Id == kioskGroup.GroupTypeId)
                                                    .FirstOrDefault();

                                                if (personGroupType == null)
                                                {
                                                    personGroupType = new CheckInGroupType();
                                                    personGroupType.GroupType = kioskGroupType.GroupType;
                                                    personGroupType.ExcludedByFilter = false;
                                                    person.GroupTypes.Add(personGroupType);
                                                }

                                                var personGroup = new CheckInGroup();
                                                personGroup.Group = kioskGroup.Clone(false);
                                                personGroup.Group.CopyAttributesFrom(kioskGroup);
                                                personGroup.ExcludedByFilter = false;
                                                personGroupType.Groups.Add(personGroup);
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }

            return false;
        }
    }
}
