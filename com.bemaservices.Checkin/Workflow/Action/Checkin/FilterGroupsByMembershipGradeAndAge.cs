// <copyright>
// Copyright by BEMA Information Technologies
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>

///
/// Created By: BEMA Services, Inc.
/// Author: Bob Rufenacht
/// Description:
///     Use this checkin workflow action in place of the default Filter Groups by Age/Grade/Gender to allow groups membership to be assessed
///     and override criteria when someone is a group member.  You can set Overide to DISABLED on those three actions to disable them and
///     use this following where Filter GRoups By Gender in the actions. It should preceded Filter Groups By Exclude Other Criteria Groups.
///     There are action properties to set to point to the Age, Birthdate and No Grade Enforced attributes on the preson/check-in groups.
///

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace com_bemaservices.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member that are not specific to their grade.
    /// </summary>
    [ActionCategory ( "BEMA Services > Check-In" )]
    [Description ( "Removes (or excludes) the groups for each selected family member that are not specific to their grade and age." )]
    [Export ( typeof ( ActionComponent ) )]
    [ExportMetadata ( "ComponentName", "Filter Groups By Membership, Grade and Age" )]

    [BooleanField ( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true )]
    [BooleanField ( "Only Keep Membership Groups", "Select 'Yes' if criteria based groups should be be removed.  Select 'No' if membership and qualified criteria groups should be kept.", true )]
    [AttributeField ( Rock.SystemGuid.EntityType.GROUP, "Group Age Range Attribute", "Select the attribute used to define the age range of the group", true, false,
        Rock.SystemGuid.Attribute.GROUP_AGE_RANGE, order: 2 )]
    [AttributeField ( Rock.SystemGuid.EntityType.GROUP, "Group Birthdate Range Attribute", "Select the attribute used to define the birthdate range of the group", true, false,
        Rock.SystemGuid.Attribute.GROUP_BIRTHDATE_RANGE, order: 3 )]
    [AttributeField ( Rock.SystemGuid.EntityType.GROUP, "No Grade Enforced Attribute", "Select the check-in group attribute used to indicate if blank values for grade should require no grade on the person.", true, false,
         order: 4 )]
    public class FilterGroupsByMembershipGradeAndAge : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState ( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }
            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var remove = GetAttributeValue ( action, "Remove" ).AsBoolean ();
                var removeCriteriaGroups = GetAttributeValue ( action, "OnlyKeepMembershipGroups" ).AsBoolean ();
                var gradeRequired = checkInState.CheckInType == null || checkInState.CheckInType.GradeRequired;
                var ageRequired = checkInState.CheckInType == null || checkInState.CheckInType.AgeRequired;

                // get the Age Range
                var ageRangeAttributeKey = string.Empty;
                var ageRangeAttributeGuid = GetAttributeValue ( action, "GroupAgeRangeAttribute" ).AsGuidOrNull ();
                if ( ageRangeAttributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get ( ageRangeAttributeGuid.Value, rockContext );
                    if ( attribute != null )
                    {
                        ageRangeAttributeKey = attribute.Key;
                    }
                }

                // get the admin-selected attribute key instead of using a hardcoded key
                var birthdateRangeAttributeKey = string.Empty;
                var birthdateRangeAttributeGuid = GetAttributeValue ( action, "GroupBirthdateRangeAttribute" ).AsGuidOrNull ();
                if ( birthdateRangeAttributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get ( birthdateRangeAttributeGuid.Value, rockContext );
                    if ( attribute != null )
                    {
                        birthdateRangeAttributeKey = attribute.Key;
                    }
                }

                // handle enforcing blank grade range attribute
                var noGradeEnforcedAttributeKey = string.Empty;
                var noGradeEnforcedAttributeGuid = GetAttributeValue ( action, "NoGradeEnforcedAttribute" ).AsGuidOrNull ();
                if ( noGradeEnforcedAttributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get ( noGradeEnforcedAttributeGuid.Value, rockContext );
                    if ( attribute != null )
                    {
                        noGradeEnforcedAttributeKey = attribute.Key;
                    }
                }

                foreach ( var person in family.People )
                {
                    var scheduleIds = checkInState.Kiosk.FilteredGroupTypes ( checkInState.ConfiguredGroupTypes ).Where ( t => t.IsCheckInActive ).SelectMany (
                            t =>
                                t.KioskGroups.SelectMany ( kg =>
                                     kg.KioskLocations.Where ( kl => kl.IsCheckInActive )
                                         .SelectMany ( l => l.KioskSchedules.Where ( ks => ks.IsCheckInActive ) )
                                         .Select ( s => s.Schedule.Id ) ) )
                        .Distinct ().ToList ();

                    // Get a list of all the groups in this config that this person is a member of
                    var memberOfGroups = MemberOfGroups ( rockContext, checkInState.CheckinTypeId.Value, person );

                    foreach ( var scheduleId in scheduleIds )
                    {
                        var memberGroupsThisSchedule = memberOfGroups.Where ( m =>
                                    m.GroupLocations.Any ( l =>
                                         l.Schedules.Select ( s => s.Id ).Contains ( scheduleId ) ) )
                                .ToList ();
                        // Check if over threshold

                        // Dont Remove groups NOT in this schedule
                        var allGroupsThisSchedule = checkInState.Kiosk.FilteredGroupTypes ( checkInState.ConfiguredGroupTypes ).Where ( t => t.IsCheckInActive )
                            .SelectMany ( t => t.KioskGroups.Where ( kg =>
                                 kg.KioskLocations.Where ( kl => kl.IsCheckInActive && kl.KioskSchedules.Where ( ks => ks.Schedule.Id == scheduleId ).Count () > 0 )
                                 .Count () > 0 ) )
                            .Select ( kg => kg.Group.Id )
                            .Distinct ().ToList ();

                        if ( memberGroupsThisSchedule.Any () && removeCriteriaGroups )
                        {
                            foreach ( var groupType in person.GroupTypes.Where ( x => x.Groups.Count > 0 ).ToList () )
                            {

                                // Do not remove a group this person is a member of
                                foreach ( var group in groupType.Groups.Where ( g => !memberGroupsThisSchedule.Select ( s => s.Id ).Contains ( g.Group.Id )
                                    && allGroupsThisSchedule.Contains ( g.Group.Id ) ).ToList () )
                                {
                                    if ( remove )
                                    {
                                        groupType.Groups.Remove ( group );
                                    }
                                    else
                                    {
                                        group.ExcludedByFilter = true;
                                    }
                                }
                            }
                        }
                        else // If this person is not a member of any groups for this check-in configuration or we are keeping membership and criteria groups
                        {
                            var gradeOffset = person.Person.GradeOffset;
                            var ageAsDouble = person.Person.AgePrecise;
                            var age = ageAsDouble.HasValue ? Convert.ToDecimal ( ageAsDouble.Value ) : (decimal?) null;
                            var birthdate = person.Person.BirthDate;

                            foreach ( var groupType in person.GroupTypes.Where ( x => x.Groups.Count > 0 ).ToList () )
                            {
                                foreach ( var group in groupType.Groups.ToList () )
                                {
                                    bool? isMatch = null;

                                    // First check for membership
                                    if ( memberOfGroups.Select ( i => i.Id ).Contains ( group.Group.Id ) )
                                    {
                                        isMatch = true;
                                    }
                                    else
                                    {

                                        // Check to see grade range
                                        var gradeOffsetRange = group.Group.GetAttributeValue ( "GradeRange" ) ?? string.Empty;
                                        var gradeOffsetRangePair = gradeOffsetRange
                                            .Split ( new[] { ',' }, StringSplitOptions.None ).AsGuidOrNullList ().ToArray ();
                                        DefinedValueCache minGradeDefinedValue = null;
                                        DefinedValueCache maxGradeDefinedValue = null;
                                        if ( gradeOffsetRangePair.Length == 2 )
                                        {
                                            minGradeDefinedValue = gradeOffsetRangePair[0].HasValue
                                                ? DefinedValueCache.Get ( gradeOffsetRangePair[0].Value )
                                                : null;
                                            maxGradeDefinedValue = gradeOffsetRangePair[1].HasValue
                                                ? DefinedValueCache.Get ( gradeOffsetRangePair[1].Value )
                                                : null;
                                        }
                                        if ( maxGradeDefinedValue != null || minGradeDefinedValue != null && isMatch == false )
                                        {
                                            if ( gradeOffset.HasValue )
                                            {
                                                // if the group type specifies a min grade (max gradeOffset)...
                                                // NOTE: minGradeOffset is actually based on the MAX Grade since GradeOffset's are Years Until Graduation
                                                var minGradeOffset = maxGradeDefinedValue?.Value.AsIntegerOrNull ();
                                                if ( minGradeOffset.HasValue && gradeOffset.Value < minGradeOffset.Value )
                                                {
                                                    isMatch = false;
                                                }

                                                // if the group type specifies a max grade (min gradeOffset)...
                                                // NOTE: maxGradeOffset is actually based on the MIN Grade since GradeOffset's are Years Until Graduation
                                                var maxGradeOffset = minGradeDefinedValue?.Value.AsIntegerOrNull ();
                                                if ( maxGradeOffset.HasValue && gradeOffset.Value > maxGradeOffset.Value )
                                                {
                                                    isMatch = false;
                                                }

                                                // If the person has a grade, and the group has a matching grade range and it wasn't excluded, then assume a match
                                                // and don't bother checking age.
                                                if ( !isMatch.HasValue )
                                                {
                                                    isMatch = true;
                                                }
                                            }
                                            else
                                            {
                                                if ( gradeRequired )
                                                {
                                                    isMatch = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Check for No Grade Enforced
                                            if ( noGradeEnforcedAttributeKey != string.Empty )
                                            {
                                                bool noGradeEnforced = group.Group.GetAttributeValue ( noGradeEnforcedAttributeKey ).AsBoolean ();

                                                if ( noGradeEnforced && gradeOffset.HasValue )
                                                {
                                                    //Since this person has a grade AND this group has No Grade Enforced, remove this group from this person
                                                    isMatch = false;
                                                }
                                            }
                                        }

                                        // If group was not excluded based on grade, then check the age.
                                        if ( !isMatch.HasValue || isMatch == true )
                                        {
                                            bool? ageMatch = null;
                                            bool? birthdayMatch = null;

                                            var ageRange = group.Group.GetAttributeValue ( ageRangeAttributeKey ).ToStringSafe ();

                                            var ageRangePair = ageRange.Split ( new[] { ',' }, StringSplitOptions.None );
                                            decimal? minAge = null;
                                            decimal? maxAge = null;

                                            if ( ageRangePair.Length == 2 )
                                            {
                                                minAge = ageRangePair[0].AsDecimalOrNull ();
                                                maxAge = ageRangePair[1].AsDecimalOrNull ();
                                            }

                                            if ( minAge.HasValue || maxAge.HasValue )
                                            {
                                                if ( age.HasValue )
                                                {
                                                    if ( minAge.HasValue )
                                                    {
                                                        var groupMinAgePrecision = minAge.Value.GetDecimalPrecision ();
                                                        var personAgePrecise = age.Floor ( groupMinAgePrecision );
                                                        if ( personAgePrecise < minAge )
                                                        {
                                                            ageMatch = false;
                                                        }
                                                    }

                                                    if ( maxAge.HasValue )
                                                    {
                                                        var groupMaxAgePrecision = maxAge.Value.GetDecimalPrecision ();
                                                        var personAgePrecise = age.Floor ( groupMaxAgePrecision );
                                                        if ( personAgePrecise > maxAge )
                                                        {
                                                            ageMatch = false;
                                                        }
                                                    }

                                                    if ( !ageMatch.HasValue )
                                                    {
                                                        ageMatch = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if ( ageRequired )
                                                    {
                                                        ageMatch = false;
                                                    }
                                                }
                                            }

                                            // If group was not included or excluded based on grade and age did not match, then check the birthdate.
                                            if ( !ageMatch.HasValue || !ageMatch.Value )
                                            {
                                                var birthdateRange = group.Group.GetAttributeValue ( birthdateRangeAttributeKey )
                                                    .ToStringSafe ();

                                                var birthdateRangePair = birthdateRange.Split ( new[] { ',' },
                                                    StringSplitOptions.None );
                                                DateTime? minBirthdate = null;
                                                DateTime? maxBirthdate = null;

                                                if ( birthdateRangePair.Length == 2 )
                                                {
                                                    minBirthdate = birthdateRangePair[0].AsDateTime ();
                                                    maxBirthdate = birthdateRangePair[1].AsDateTime ();
                                                }

                                                if ( minBirthdate.HasValue || maxBirthdate.HasValue )
                                                {
                                                    if ( birthdate.HasValue )
                                                    {
                                                        if ( minBirthdate.HasValue && birthdate.Value < minBirthdate.Value )
                                                        {
                                                            birthdayMatch = false;
                                                        }

                                                        if ( maxBirthdate.HasValue && birthdate.Value > maxBirthdate.Value )
                                                        {
                                                            birthdayMatch = false;
                                                        }

                                                        if ( !birthdayMatch.HasValue )
                                                        {
                                                            birthdayMatch = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if ( ageRequired )
                                                        {
                                                            birthdayMatch = false;
                                                        }
                                                    }
                                                }
                                            }

                                            if ( ageMatch.HasValue || birthdayMatch.HasValue )
                                            {
                                                if ( !( ( ageMatch ?? false ) || ( birthdayMatch ?? false ) ) )
                                                {
                                                    isMatch = false;
                                                }
                                                else
                                                {
                                                    isMatch = true;
                                                }
                                            }
                                        }

                                        // if age and/or grade matched, check if gender match needed
                                        if ( !isMatch.HasValue || isMatch == true )
                                        {
                                            var groupGender = group.Group.GetAttributeValue ( "Gender" ).ConvertToEnumOrNull<Gender> ();
                                            var personGender = person.Person.Gender;

                                            if ( groupGender.HasValue && groupGender.Value != personGender )
                                            {
                                                isMatch = false;
                                            }

                                        }
                                    }

                                    if ( isMatch.HasValue && !isMatch.Value && allGroupsThisSchedule.Contains ( group.Group.Id ) )
                                    {
                                        if ( remove )
                                        {
                                            groupType.Groups.Remove ( group );
                                        }
                                        else
                                        {
                                            group.ExcludedByFilter = true;
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

        private List<Group> MemberOfGroups( RockContext rockContext, int checkInTypeId, Rock.CheckIn.CheckInPerson person )
        {
            var checkInType = new GroupTypeService ( rockContext ).Get ( checkInTypeId );
            var groupTypes = GroupTypes ( new List<GroupType> { checkInType } );
            var groupTypeIds = groupTypes.Select ( t => t.Id ).ToList ();

            var memberOfGroups = new GroupMemberService ( rockContext ).GetByPersonId ( person.Person.Id )
                .Where ( p => p.GroupMemberStatus == GroupMemberStatus.Active && p.Group.IsActive && groupTypeIds.Contains ( p.Group.GroupTypeId ) )
                .Select ( x => x.Group ).ToList ();

            return memberOfGroups;
        }

        // Recursively create a list of all group types under the types initially passed into the list
        private List<GroupType> GroupTypes( List<GroupType> groupTypes, GroupType groupType = null )
        {
            if ( groupType == null )
            {
                foreach ( var gt in groupTypes.ToList () )
                {
                    groupTypes = GroupTypes ( groupTypes, gt );
                }
            }
            else
            {
                foreach ( var gt in groupType.ChildGroupTypes.ToList () )
                {
                    // Avoid infinite calling if self is group type
                    if (gt != groupType)
                    {
                        groupTypes.Add ( gt );
                        groupTypes = GroupTypes ( groupTypes, gt );
                    }
                }
            }

            return groupTypes;
        }

        private GroupType TopLevelGroupType( GroupType groupType, int checkInTypeId )
        {
            if ( groupType.ParentGroupTypes.Any ( p => p.Id == checkInTypeId ) )
            {
                return groupType;
            }

            return TopLevelGroupType ( groupType.ParentGroupTypes.First ( p => p.TakesAttendance ), checkInTypeId );
        }
    }
}