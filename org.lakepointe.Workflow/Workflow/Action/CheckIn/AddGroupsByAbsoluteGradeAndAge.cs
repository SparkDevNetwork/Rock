// <copyright>
// Copyright by the Spark Development Network
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
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.lakepointe.Workflow.Action.CheckIn
{
    /// <summary>
    /// Add back groups for each selected family member that are in the absolute range if they don't have any group to select.
    /// </summary>
    [ActionCategory( "org_lakepointe: Check-In" )]
    [Description( " Add back groups for each selected family member that are in the absolute range if they don't have any group to select." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Add Groups By Absolute Grade and Age" )]

    public class AddGroupsByAbsoluteGradeAndAge : CheckInActionComponent
    {
        private List<int> _activeScheduleIds = new List<int>();

        /// <summary>
        /// Check if there are any available locations.
        /// </summary>
        /// <param name="checkInState">State of the check in.</param>
        /// <param name="person">The person.</param>
        /// <param name="haveGroup">if person have a group to check-in, even if it is full.</param>
        /// <returns></returns>
        private bool AvailableLocation( CheckInState checkInState, CheckInPerson person, int scheduleId, out bool haveGroup )
        {
            haveGroup = false;
            foreach ( CheckInGroupType groupType in person.GroupTypes )
            {
                foreach ( CheckInGroup group in groupType.Groups.Where( g => !g.ExcludedByFilter ) )
                {
                    haveGroup = true;

                    foreach ( CheckInLocation location in group.Locations.Where( l => !l.ExcludedByFilter ) )
                    {
                        KioskGroupType kioskGroupType = checkInState.Kiosk.ActiveGroupTypes( checkInState.ConfiguredGroupTypes )
                            .Where( g => g.GroupType.Id == groupType.GroupType.Id )
                            .FirstOrDefault();
                        if (kioskGroupType == null)
                        {
                            continue;
                        }

                        KioskGroup kioskGroup = kioskGroupType.KioskGroups
                            .Where( g => g.Group.Id == group.Group.Id && g.IsCheckInActive )
                            .FirstOrDefault();
                        if (kioskGroup == null)
                        {
                            continue;
                        }

                        KioskLocation kioskLocation = kioskGroup.KioskLocations
                            .Where( l => l.Location.Id == location.Location.Id && l.IsCheckInActive && l.KioskSchedules.Any( s => s.Schedule.Id == scheduleId ) )
                            .FirstOrDefault();
                        if (kioskLocation == null)
                        {
                            continue;
                        }

                        if (kioskLocation.Location.SoftRoomThreshold.HasValue )
                        {
                            var locAttendance = KioskLocationAttendance.Get( kioskLocation.Location.Id );
                            if ( locAttendance != null &&
                                !locAttendance.DistinctPersonIds.Contains( person.Person.Id ) &&
                                kioskLocation.Location.SoftRoomThreshold.Value <= locAttendance.CurrentCount )
                            {
                                continue;
                            }

                            return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Loads the active schedules.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="campusId">The campus identifier.</param>
        private void LoadActiveSchedules( RockContext rockContext, int? campusId )
        {
            // Find all the schedules that are used for check-in
            var checkinSchedules = new ScheduleService( rockContext )
                .Queryable().AsNoTracking()
                .Where( s => s.CheckInStartOffsetMinutes.HasValue )
                .ToList();

            // Find the active schedules for this location (campus)
            var locationDateTime = RockDateTime.Now;
            if ( campusId.HasValue )
            {
                locationDateTime = CampusCache.Get( campusId.Value )?.CurrentDateTime ?? RockDateTime.Now;
            }

            foreach ( var schedule in checkinSchedules )
            {
                if ( schedule.WasScheduleOrCheckInActive( locationDateTime ) )
                {
                    _activeScheduleIds.Add( schedule.Id );
                }
            }
        }

        /// <summary>
        /// Adds groups that are in the absolute age range.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="checkInState">State of the check in.</param>
        /// <param name="person">The person.</param>
        /// <param name="loadedGroupTypes">The loaded group types.</param>
        /// <returns></returns>
        private Dictionary<int, List<CheckInGroup>> AddGroupsByAbsoluteAge( RockContext rockContext, CheckInState checkInState, Person person, Dictionary<int, List<CheckInGroup>> loadedGroupTypes )
        {
            Dictionary<int, List<CheckInGroup>> groupTypes = new Dictionary<int, List<CheckInGroup>>();

            bool ageRequired = checkInState.CheckInType == null || checkInState.CheckInType.AgeRequired;

            var ageAsDouble = person.AgePrecise;
            decimal? age = ageAsDouble.HasValue ? Convert.ToDecimal( ageAsDouble.Value ) : ( decimal? ) null;

            DateTime? birthdate = person.BirthDate;

            if ( ( age == null || birthdate == null ) && !ageRequired )
            {
                foreach ( var loadedGroupType in loadedGroupTypes )
                {
                    List<CheckInGroup> groups = new List<CheckInGroup>();
                    groupTypes[loadedGroupType.Key] = groups;
                    foreach ( var group in loadedGroupType.Value )
                    {
                        var absoluteAgeRange = group.Group.GetAttributeValue( "AbsoluteAgeRange" ) ?? string.Empty;
                        var absoluteBirthdateRange = group.Group.GetAttributeValue( "AbsoluteBirthdateRange" ) ?? string.Empty;

                        if ( absoluteAgeRange.IsNotNullOrWhiteSpace() && absoluteBirthdateRange.IsNotNullOrWhiteSpace() )
                        {
                            groups.Add( group );
                        }
                    }
                }

                return groupTypes;
            }

            foreach ( var loadedGroupType in loadedGroupTypes )
            {
                List<CheckInGroup> groups = new List<CheckInGroup>();
                groupTypes[loadedGroupType.Key] = groups;
                foreach ( var group in loadedGroupType.Value )
                {
                    // First check to see if age matches
                    var absoluteAgeRange = group.Group.GetAttributeValue( "AbsoluteAgeRange" ) ?? string.Empty;

                    if ( absoluteAgeRange.IsNotNullOrWhiteSpace() )
                    {
                        var absoluteAgeRangePair = absoluteAgeRange.Split( new char[] { ',' }, StringSplitOptions.None );
                        string minAbsoluteAgeValue = null;
                        string maxAbsoluteAgeValue = null;
                        if ( absoluteAgeRangePair.Length == 2 )
                        {
                            minAbsoluteAgeValue = absoluteAgeRangePair[0];
                            maxAbsoluteAgeValue = absoluteAgeRangePair[1];
                        }

                        decimal? minAbsoluteAge = minAbsoluteAgeValue.AsDecimalOrNull();
                        decimal? maxAbsoluteAge = maxAbsoluteAgeValue.AsDecimalOrNull();

                        if ( age.HasValue )
                        {
                            // Add if age is more than absolute min.
                            if ( minAbsoluteAge.HasValue && !maxAbsoluteAge.HasValue )
                            {
                                int groupMinAgePrecision = minAbsoluteAge.Value.GetDecimalPrecision();
                                decimal? personAgePrecise = age.Floor( groupMinAgePrecision );
                                if ( personAgePrecise >= minAbsoluteAge )
                                {
                                    groups.Add( group );
                                    continue;
                                }
                            }
                            // Add if age is less than absolute max.
                            else if ( maxAbsoluteAge.HasValue && !minAbsoluteAge.HasValue )
                            {
                                int groupMaxAgePrecision = maxAbsoluteAge.Value.GetDecimalPrecision();
                                decimal? personAgePrecise = age.Floor( groupMaxAgePrecision );
                                if ( personAgePrecise <= maxAbsoluteAge )
                                {
                                    groups.Add( group );
                                    continue;
                                }
                            }
                            // Add if age is between absolute max and absolute max.
                            else if ( minAbsoluteAge.HasValue && maxAbsoluteAge.HasValue )
                            {
                                int groupMinAgePrecision = minAbsoluteAge.Value.GetDecimalPrecision();
                                decimal? personAgePrecise = age.Floor( groupMinAgePrecision );
                                if ( personAgePrecise >= minAbsoluteAge && personAgePrecise <= maxAbsoluteAge )
                                {
                                    groups.Add( group );
                                    continue;
                                }
                            }

                        }
                    }

                    var absoluteBirthdateRange = group.Group.GetAttributeValue( "AbsoluteBirthdateRange" ) ?? string.Empty;

                    if ( !string.IsNullOrWhiteSpace( absoluteBirthdateRange ) )
                    {
                        var absoluteBirthdateRangePair = absoluteBirthdateRange.Split( new char[] { ',' }, StringSplitOptions.None );
                        string minAbsoluteBirthdateValue = null;
                        string maxAbsoluteBirthdateValue = null;

                        if ( absoluteBirthdateRangePair.Length == 2 )
                        {
                            minAbsoluteBirthdateValue = absoluteBirthdateRangePair[0];
                            maxAbsoluteBirthdateValue = absoluteBirthdateRangePair[1];
                        }

                        DateTime? minAbsoluteBirthdate = minAbsoluteBirthdateValue.AsDateTime();
                        DateTime? maxAbsoluteBirthdate = maxAbsoluteBirthdateValue.AsDateTime();

                        if ( birthdate.HasValue )
                        {
                            if ( minAbsoluteBirthdate.HasValue && !maxAbsoluteBirthdate.HasValue )
                            {
                                if ( birthdate.Value >= minAbsoluteBirthdate.Value )
                                {
                                    groups.Add( group );
                                    continue;
                                }
                            }
                            else if ( maxAbsoluteBirthdate.HasValue && !minAbsoluteBirthdate.HasValue )
                            {
                                if ( birthdate.Value <= maxAbsoluteBirthdate.Value )
                                {
                                    groups.Add( group );
                                    continue;
                                }
                            }
                            else if ( minAbsoluteBirthdate.HasValue && maxAbsoluteBirthdate.HasValue )
                            {
                                if ( birthdate.Value >= minAbsoluteBirthdate.Value && birthdate.Value <= maxAbsoluteBirthdate.Value )
                                {
                                    groups.Add( group );
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            return groupTypes;
        }

        /// <summary>
        /// Adds groups that are in the absolute grade range.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="checkInState">State of the check in.</param>
        /// <param name="person">The person.</param>
        /// <param name="loadedGroupTypes">The loaded group types.</param>
        /// <returns></returns>
        private Dictionary<int, List<CheckInGroup>> AddGroupsByAbsoluteGrade( RockContext rockContext, CheckInState checkInState, Person person, Dictionary<int, List<CheckInGroup>> loadedGroupTypes )
        {
            Dictionary<int, List<CheckInGroup>> groupTypes = new Dictionary<int, List<CheckInGroup>>();
            bool gradeRequired = checkInState.CheckInType == null || checkInState.CheckInType.GradeRequired;

            int? personsGradeOffset = person.GradeOffset;
            if ( personsGradeOffset == null && !gradeRequired )
            {
                foreach ( var loadedGroupType in loadedGroupTypes )
                {
                    List<CheckInGroup> groups = new List<CheckInGroup>();
                    groupTypes[loadedGroupType.Key] = groups;
                    foreach ( var group in loadedGroupType.Value )
                    {
                        string absoluteGradeOffsetRange = group.Group.GetAttributeValue( "AbsoluteGradeRange" ) ?? string.Empty;
                        if ( absoluteGradeOffsetRange.IsNotNullOrWhiteSpace() )
                        {
                            groups.Add( group );
                        }
                    }
                }

                return groupTypes;
            }

            foreach ( var loadedGroupType in loadedGroupTypes )
            {
                List<CheckInGroup> groups = new List<CheckInGroup>();
                groupTypes[loadedGroupType.Key] = groups;
                foreach ( var group in loadedGroupType.Value )
                {
                    string absoluteGradeOffsetRange = group.Group.GetAttributeValue( "AbsoluteGradeRange" ) ?? string.Empty;
                    var absoluteGradeOffsetRangePair = absoluteGradeOffsetRange.Split( new char[] { ',' }, StringSplitOptions.None ).AsGuidOrNullList().ToArray();
                    DefinedValueCache minAbsoluteGradeDefinedValue = null;
                    DefinedValueCache maxAbsoluteGradeDefinedValue = null;
                    if ( absoluteGradeOffsetRangePair.Length == 2 )
                    {
                        minAbsoluteGradeDefinedValue = absoluteGradeOffsetRangePair[0].HasValue ? DefinedValueCache.Get( absoluteGradeOffsetRangePair[0].Value ) : null;
                        maxAbsoluteGradeDefinedValue = absoluteGradeOffsetRangePair[1].HasValue ? DefinedValueCache.Get( absoluteGradeOffsetRangePair[1].Value ) : null;
                    }

                    /*
                     * example (assuming defined values are the stock values):
                     * minGrade,maxGrade of between 4th and 6th grade
                     * 4th grade is 8 years until graduation
                     * 6th grade is 6 years until graduation
                     * GradeOffsetRange would be 8 and 6
                     * if person is in:
                     *      7th grade or older (gradeOffset 5 or smaller), they would be NOT included
                     *      6th grade (gradeOffset 6), they would be included
                     *      5th grade (gradeOffset 7), they would be included
                     *      4th grade (gradeOffset 8), they would be included
                     *      3th grade or younger (gradeOffset 9 or bigger), they would be NOT included
                     *      NULL grade, not included
                     */

                    // if the group type specifies a min grade (max gradeOffset) and min absolute grade (max absoluteGradeOffset)...
                    if ( maxAbsoluteGradeDefinedValue != null && minAbsoluteGradeDefinedValue == null )
                    {
                        // NOTE: minGradeOffset is actually based on the MAX Grade since GradeOffset's are Years Until Graduation
                        int? minAbsoluteGradeOffset = maxAbsoluteGradeDefinedValue.Value.AsIntegerOrNull();
                        if ( minAbsoluteGradeOffset.HasValue )
                        {
                            // Add back in if a persons grade offset is more than the max offset and less equal to the absolute max offset
                            // example person is in 5rd grade (offset 7) and range is 4th to 6th (offset 6 to 8)
                            if ( personsGradeOffset.HasValue && personsGradeOffset >= minAbsoluteGradeOffset.Value )
                            {
                                groups.Add( group );
                                continue;
                            }
                        }
                    }
                    else if ( minAbsoluteGradeDefinedValue != null && maxAbsoluteGradeDefinedValue == null )
                    {
                        // NOTE: maxGradeOffset is actually based on the MIN Grade since GradeOffset's are Years Until Graduation
                        int? maxAbsoluteGradeOffset = minAbsoluteGradeDefinedValue.Value.AsIntegerOrNull();
                        if ( maxAbsoluteGradeOffset.HasValue )
                        {
                            // Add back in if a persons grade offset is less than the min offset and more equal to the absolute min offset
                            // example person is in 5rd grade (offset 7) and range is 4th to 6th (offset 6 to 8)
                            if ( !personsGradeOffset.HasValue && personsGradeOffset <= maxAbsoluteGradeOffset.Value )
                            {
                                groups.Add( group );
                                continue;
                            }
                        }
                    }
                    // if the group type specifies a max absolute grade (min absoluteGradeOffset) and min absolute grade (max absoluteGradeOffset)...
                    else if ( maxAbsoluteGradeDefinedValue != null && minAbsoluteGradeDefinedValue != null )
                    {
                        // NOTE: minGradeOffset is actually based on the MAX Grade since GradeOffset's are Years Until Graduation
                        int? maxAbsoluteGradeOffset = minAbsoluteGradeDefinedValue.Value.AsIntegerOrNull();
                        int? minAbsoluteGradeOffset = maxAbsoluteGradeDefinedValue.Value.AsIntegerOrNull();
                        if ( maxAbsoluteGradeOffset.HasValue && minAbsoluteGradeOffset.HasValue )
                        {
                            // Add back in if a persons grade offset is less equal than the absolute max offset and more equal to the absolute min offset
                            // example person is in 5rd grade (offset 7) and range is 4th to 6th (offset 6 to 8)
                            if ( personsGradeOffset.HasValue && personsGradeOffset <= maxAbsoluteGradeOffset.Value && personsGradeOffset >= minAbsoluteGradeOffset.Value )
                            {
                                groups.Add( group );
                                continue;
                            }
                        }
                    }
                }
            }

            return groupTypes;
        }

        /// <summary>
        /// Groups with no absolute age range.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="loadedGroupTypes">The loaded group types.</param>
        /// <returns></returns>
        private Dictionary<int, List<CheckInGroup>> GroupsNoAbsoluteAge( RockContext rockContext, Dictionary<int, List<CheckInGroup>> loadedGroupTypes )
        {
            Dictionary<int, List<CheckInGroup>> groupTypes = new Dictionary<int, List<CheckInGroup>>();

            foreach ( var loadedGroupType in loadedGroupTypes )
            {
                List<CheckInGroup> groups = new List<CheckInGroup>();
                groupTypes[loadedGroupType.Key] = groups;
                foreach ( var group in loadedGroupType.Value )
                {
                    var absoluteAgeRange = group.Group.GetAttributeValue( "AbsoluteAgeRange" ) ?? string.Empty;
                    var absoluteBirthdateRange = group.Group.GetAttributeValue( "AbsoluteBirthdateRange" ) ?? string.Empty;

                    if ( absoluteAgeRange.IsNullOrWhiteSpace() && absoluteBirthdateRange.IsNullOrWhiteSpace() )
                    {
                        groups.Add( group );
                    }
                }
            }

            return groupTypes;
        }

        /// <summary>
        /// Groups with no absolute grade.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="loadedGroupTypes">The loaded group types.</param>
        /// <returns></returns>
        private Dictionary<int, List<CheckInGroup>> GroupsNoAbsoluteGrade( RockContext rockContext, Dictionary<int, List<CheckInGroup>> loadedGroupTypes )
        {
            Dictionary<int, List<CheckInGroup>> groupTypes = new Dictionary<int, List<CheckInGroup>>();

            foreach ( var loadedGroupType in loadedGroupTypes )
            {
                List<CheckInGroup> groups = new List<CheckInGroup>();
                groupTypes[loadedGroupType.Key] = groups;
                foreach ( var group in loadedGroupType.Value )
                {
                    string absoluteGradeOffsetRange = group.Group.GetAttributeValue( "AbsoluteGradeRange" ) ?? string.Empty;

                    if ( absoluteGradeOffsetRange.IsNullOrWhiteSpace() )
                    {
                        groups.Add( group );
                    }
                }
            }

            return groupTypes;
        }

        /// <summary>
        /// Filters the groups by gender.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="loadedGroupTypes">The loaded group types.</param>
        /// <returns></returns>
        public Dictionary<int, List<CheckInGroup>> FilterByGender( Person person, Dictionary<int, List<CheckInGroup>> loadedGroupTypes )
        {
            Dictionary<int, List<CheckInGroup>> groupTypes = new Dictionary<int, List<CheckInGroup>>();
            foreach ( var loadedGroupType in loadedGroupTypes )
            {
                List<CheckInGroup> groups = new List<CheckInGroup>();
                groupTypes[loadedGroupType.Key] = groups;
                foreach ( var group in loadedGroupType.Value )
                {
                    var groupGender = group.Group.GetAttributeValue( "Gender" ).ConvertToEnumOrNull<Gender>();
                    if ( !groupGender.HasValue || groupGender.Value == person.Gender )
                    {
                        groups.Add( group );
                    }
                }
            }

            return groupTypes;
        }

        /// <summary>
        /// Loads the locations that are in the threshold.
        /// </summary>
        /// <param name="checkInState">State of the check in.</param>
        /// <param name="person">The person.</param>
        /// <param name="loadedGroupTypes">The loaded group types.</param>
        private void LoadLocationsByThreshold( CheckInState checkInState, Person person, Dictionary<int, List<CheckInGroup>> loadedGroupTypes )
        {
            foreach ( var loadedGroupType in loadedGroupTypes )
            {
                var kioskGroupType = checkInState.Kiosk.ActiveGroupTypes( checkInState.ConfiguredGroupTypes )
                    .Where( g => g.GroupType.Id == loadedGroupType.Key )
                    .FirstOrDefault();

                if ( kioskGroupType != null )
                {
                    foreach ( var group in loadedGroupType.Value )
                    {
                        foreach ( var kioskGroup in kioskGroupType.KioskGroups
                            .Where( g => g.Group.Id == group.Group.Id && g.IsCheckInActive )
                            .ToList() )
                        {
                            foreach ( var kioskLocation in kioskGroup.KioskLocations.Where( l => l.IsCheckInActive && l.IsActiveAndNotFull ) )
                            {
                                if ( !group.Locations.Any( l => l.Location.Id == kioskLocation.Location.Id ) )
                                {
                                    if ( kioskLocation.Location.SoftRoomThreshold.HasValue )
                                    {
                                        var locAttendance = KioskLocationAttendance.Get( kioskLocation.Location.Id );
                                        if ( locAttendance != null &&
                                            !locAttendance.DistinctPersonIds.Contains( person.Id ) &&
                                            kioskLocation.Location.SoftRoomThreshold.Value <= locAttendance.CurrentCount )
                                        {
                                            continue;
                                        }
                                    }

                                    var checkInLocation = new CheckInLocation();
                                    checkInLocation.Location = kioskLocation.Location.Clone( false );
                                    checkInLocation.Location.CopyAttributesFrom( kioskLocation.Location );
                                    checkInLocation.CampusId = kioskLocation.CampusId;
                                    checkInLocation.Order = kioskLocation.Order;
                                    group.Locations.Add( checkInLocation );
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads all the groups selected by the kiosk device.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="checkInState">State of the check in.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        private Dictionary<int, List<CheckInGroup>> LoadGroups( RockContext rockContext, CheckInState checkInState, CheckInPerson person, int scheduleId )
        {
            Dictionary<int, List<CheckInGroup>> checkInGroups = new Dictionary<int, List<CheckInGroup>>();
            foreach ( CheckInGroupType groupType in person.GroupTypes )
            {
                var kioskGroupType = checkInState.Kiosk.ActiveGroupTypes( checkInState.ConfiguredGroupTypes )
                    .Where( g => g.GroupType.Id == groupType.GroupType.Id )
                    .FirstOrDefault();

                if ( kioskGroupType != null )
                {
                    if ( !checkInGroups.ContainsKey( groupType.GroupType.Id ) )
                    {
                        checkInGroups[groupType.GroupType.Id] = new List<CheckInGroup>();
                    }

                    foreach ( var kioskGroup in kioskGroupType.KioskGroups
                        .Where( g => g.IsCheckInActive                          // check-in-active groups
                            && ( g.KioskLocations                               // that have locations
                                .Where( l => l.KioskSchedules                   // with schedules
                                    .Any( s => s.Schedule.Id == scheduleId)     // where the schedule is this one
                                ).Any() )                                       // and there are any such locations
                        ) )
                    {
                        bool validGroup = true;
                        if ( groupType.GroupType.AttendanceRule == AttendanceRule.AlreadyBelongs )
                        {
                            validGroup = new GroupMemberService( rockContext ).Queryable()
                                .Any( m =>
                                    m.GroupId == kioskGroup.Group.Id &&
                                    m.GroupMemberStatus == GroupMemberStatus.Active &&
                                    m.PersonId == person.Person.Id );
                        }

                        if ( validGroup && !checkInGroups[groupType.GroupType.Id].Any( g => g.Group.Id == kioskGroup.Group.Id ) )
                        {
                            var checkInGroup = new CheckInGroup();
                            checkInGroup.Group = kioskGroup.Group.Clone( false );
                            checkInGroup.Group.CopyAttributesFrom( kioskGroup.Group );
                            checkInGroups[groupType.GroupType.Id].Add( checkInGroup );
                        }
                    }
                }
            }

            return checkInGroups;
        }

        /// <summary>
        /// Adds the only the first available absolute group/group-location back to the person's CheckInGroupTypes.
        /// </summary>
        /// <param name="person">The person before adding the absolute ranges.</param>
        /// <param name="addGroupTypes">All the possible absolute limit CheckInGroupTypes.</param>
        private void AddFirstAbsoluteGroupWithLocation( CheckInPerson person, Dictionary<int, List<CheckInGroup>> addGroupTypes )
        {
            foreach ( CheckInGroupType existingGroupType in person.GroupTypes.OrderBy( gt => gt.GroupType.Order ).ThenBy( gt => gt.GroupType.Name ) )
            {
                if ( !addGroupTypes.ContainsKey( existingGroupType.GroupType.Id ) )
                {
                    continue;
                }

                var addGroups = addGroupTypes[existingGroupType.GroupType.Id];
                foreach ( var anAddGroup in addGroups.OrderBy( g => g.Group.Order ).ThenBy( g => g.Group.Name ) )
                {
                    if ( anAddGroup.Locations.Count != 0 )
                    {
                        var existingGroup = existingGroupType.Groups.FirstOrDefault( g => g.Group.Id == anAddGroup.Group.Id );
                        if ( existingGroup != null )
                        {
                            // Don't add the group in if there are not locations in it
                            existingGroup.ExcludedByFilter = false;
                            existingGroup.Notes = "Using Absolute Limits.";
                            existingGroup.Locations.Add( anAddGroup.Locations.OrderBy( l => l.Order ).ThenBy( l => l.Location.Name ).FirstOrDefault() );

                            return;
                        }
                        else
                        {
                            anAddGroup.Notes = "Using Absolute Limits.";
                            // Remove all but the first location.
                            anAddGroup.Locations = new List<CheckInLocation>() { anAddGroup.Locations.OrderBy( l => l.Order ).ThenBy( l => l.Location.Name ).FirstOrDefault() };
                            existingGroupType.Groups.Add( anAddGroup );

                            return;
                        }
                    }
                }
            }
        }

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
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                int? campusId = null;
                var locations = family.People.SelectMany( p => p.GroupTypes ).SelectMany( gt => gt.Groups ).SelectMany( g => g.Locations );
                campusId = locations.FirstOrDefault( l => l.CampusId.HasValue )?.CampusId;
                LoadActiveSchedules( rockContext, campusId );
                var scheduleIds = checkInState.Kiosk.KioskGroupTypes.SelectMany( gt => gt.KioskGroups ).SelectMany( g => g.KioskLocations ).SelectMany( l => l.KioskSchedules ).Select( s => s.Schedule.Id ).Distinct().ToList();

                foreach (var person in family.People)
                {
                    foreach ( var scheduleId in scheduleIds )
                    {
                        // If a room is available, do not add extra rooms
                        if (AvailableLocation( checkInState, person, scheduleId, out bool haveGroup ))
                        {
                            continue;
                        }

                        // Check-in location not applicable to person
                        if (!haveGroup)
                        {
                            continue;
                        }

                        // No room available. Search for other rooms using absolute ranges.
                        var loadedGroupTypes = LoadGroups( rockContext, checkInState, person, scheduleId );
                        Dictionary<int, List<CheckInGroup>> addGroupTypes = new Dictionary<int, List<CheckInGroup>>();
                        Dictionary<int, List<CheckInGroup>> addGroupTypesGrade = AddGroupsByAbsoluteGrade( rockContext, checkInState, person.Person, loadedGroupTypes );
                        Dictionary<int, List<CheckInGroup>> addGroupTypesAge = AddGroupsByAbsoluteAge( rockContext, checkInState, person.Person, loadedGroupTypes );
                        Dictionary<int, List<CheckInGroup>> groupTypesNoAbsoluteGrade = GroupsNoAbsoluteGrade( rockContext, loadedGroupTypes );
                        Dictionary<int, List<CheckInGroup>> groupTypesNoAbsoluteAge = GroupsNoAbsoluteAge( rockContext, loadedGroupTypes );

                        foreach ( var checkInGroupKey in addGroupTypesAge.Keys )
                        {
                            List<CheckInGroup> addGroups = new List<CheckInGroup>();
                            if ( addGroupTypesGrade.ContainsKey( checkInGroupKey ) )
                            {
                                List<CheckInGroup> addGroupsGrade = addGroupTypesGrade[checkInGroupKey];
                                addGroups.AddRange( addGroupTypesAge[checkInGroupKey].Where( a => addGroupsGrade.Contains( a ) ) ); // Add where both absolute age and absolute grade criteria are met.
                            }
                            
                            if ( groupTypesNoAbsoluteGrade.ContainsKey( checkInGroupKey ) )
                            {
                                List<CheckInGroup> groupsNoAbsoluteGrade = groupTypesNoAbsoluteGrade[checkInGroupKey];
                                addGroups.AddRange( addGroupTypesAge[checkInGroupKey].Where( a => groupsNoAbsoluteGrade.Contains( a ) ) );  // Add where only absolute age range is specified and criteria is met.
                            }

                            if ( addGroups.Count != 0 )
                            {
                                addGroupTypes[checkInGroupKey] = addGroups;
                            }
                        }

                        foreach ( var checkInGroupKey in addGroupTypesGrade.Keys )
                        {
                            List<CheckInGroup> addGroups = new List<CheckInGroup>();

                            if (groupTypesNoAbsoluteAge.ContainsKey( checkInGroupKey ))
                            {
                                List<CheckInGroup> groupsNoAbsoluteAge = groupTypesNoAbsoluteAge[checkInGroupKey];
                                addGroups.AddRange( addGroupTypesGrade[checkInGroupKey].Where( a => groupsNoAbsoluteAge.Contains( a ) ) ); // Add where only absolute grade range is specified and criteria is met.
                            }

                            if (addGroups.Count != 0)
                            {
                                if (addGroupTypes.ContainsKey( checkInGroupKey ))
                                {
                                    addGroupTypes[checkInGroupKey].AddRange( addGroups );
                                }
                                else
                                {
                                    addGroupTypes[checkInGroupKey] = addGroups;
                                }
                            }
                        }

                        addGroupTypes = FilterByGender( person.Person, addGroupTypes ); // Filter by gender

                        LoadLocationsByThreshold( checkInState, person.Person, addGroupTypes ); // Filter out rooms that are already full

                        AddFirstAbsoluteGroupWithLocation( person, addGroupTypes );
                    }
                }
            }

            return true;
        }
    }
}