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
///     Use this checkin workflow action after the Set Available Schedules action in the Schedule Select activity.
///     This action loocs at both group membership and schedules so that is can remove schedules from check-in options when the person
///     is the member of a group for a scheduled time.  This allows the group membership check to be used in a multi-service check-in.
///     For example, if you use criteria based check-in at first service and then have a "staff children only group" for second service,
///     the criteria based can work alongside the member group for the second service.  The only option for second service can be the
///     members group.
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
    [ActionCategory( "BEMA Services > Check-In" )]
    [Description( "Removes (or excludes) groups they are not a member of if they are a member of another group of that type and it's available on the schedule." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Membership and Schedule" )]

    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true )]
    public class FilterGroupsByMembershipAndSchedule : CheckInActionComponent
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
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();


                foreach ( var person in family.People )
                {
                    foreach ( var groupType in person.GroupTypes.Where ( gt => gt.GroupType.AttendanceRule != Rock.Model.AttendanceRule.AlreadyBelongs && !gt.ExcludedByFilter ).ToList () )
                    {
                        // Get a list of all the groups in this parent/top level group type that this person is a member of
                        var memberOfGroups = MemberOfGroups ( rockContext, checkInState.CheckinTypeId.Value, person, TopLevelGroupType ( groupType.GroupType, checkInState.CheckinTypeId.Value ) );

                        if ( memberOfGroups.Any () )
                        {
                            //list of schedule Ids where member groups of this type are available to check-in
                            var scheduleIdsWithMembershipGroups = person.GroupTypes.Where ( gt => gt.GroupType.AttendanceRule != Rock.Model.AttendanceRule.AlreadyBelongs && !gt.ExcludedByFilter )
                                .SelectMany ( gt => gt.Groups )
                                .Where ( g => (!g.ExcludedByFilter) && memberOfGroups.Select (s => s.Id ).Contains ( g.Group.Id ) ) 
                                .SelectMany ( g => g.Locations ).Where ( l => !l.ExcludedByFilter )
                                .SelectMany ( l => l.Schedules ).Where ( s => !s.ExcludedByFilter )
                                .Select ( s => s.Schedule.Id ).ToList ();

                            foreach ( var group in groupType.Groups.Where ( g => !memberOfGroups.Select ( s => s.Id ).Contains( g.Group.Id ) ).ToList () )
                            {
                                foreach ( var location in group.Locations.ToList () )
                                {
                                    foreach ( var schedule in location.Schedules.ToList () )
                                    {
                                        //If group's schedule exists in same schedule with an membership group, exclude or remove it.
                                        if ( scheduleIdsWithMembershipGroups.Any ( sId => sId == schedule.Schedule.Id ) )
                                        {
                                            if ( remove )
                                            {
                                                location.Schedules.Remove ( schedule );
                                            }
                                            else
                                            {
                                                schedule.ExcludedByFilter = true;
                                            }
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

        private List<Group> MemberOfGroups( RockContext rockContext, int checkInTypeId, Rock.CheckIn.CheckInPerson person, GroupTypeCache groupType )
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
                    if ( gt != groupType )
                    {
                        groupTypes.Add ( gt );
                        groupTypes = GroupTypes ( groupTypes, gt );
                    }
                }
            }

            return groupTypes;
        }
        private GroupTypeCache TopLevelGroupType( GroupTypeCache groupType, int checkInTypeId )
        {
            if ( groupType.ParentGroupTypes.Any ( p => p.Id == checkInTypeId ) )
            {
                return groupType;
            }

            return TopLevelGroupType ( groupType.ParentGroupTypes.First ( p => p.TakesAttendance ), checkInTypeId );
        }

    }
}