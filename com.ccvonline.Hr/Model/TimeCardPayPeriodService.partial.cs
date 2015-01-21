using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rock;
using Rock.Model;

namespace com.ccvonline.Hr.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TimeCardPayPeriodService
    {
        /// <summary>
        /// Ensures the current pay period then returns it
        /// </summary>
        /// <param name="firstPayPeriodStartDate">The first pay period start date</param>
        /// <returns></returns>
        public TimeCardPayPeriod EnsureCurrentPayPeriod( DateTime firstPayPeriodStartDate )
        {
            TimeCardPayPeriod currentPayPeriod = GetCurrentPayPeriod();
            if ( currentPayPeriod == null )
            {
                // assume 14 PayPeriods starting on first Saturday of Year
                DateTime currentDate = RockDateTime.Today;

                var payPeriodEnd = firstPayPeriodStartDate.AddDays( 14 );
                while ( payPeriodEnd < currentDate )
                {
                    payPeriodEnd = payPeriodEnd.AddDays( 14 );
                }

                currentPayPeriod = new TimeCardPayPeriod();
                currentPayPeriod.StartDate = payPeriodEnd.AddDays( -14 );
                currentPayPeriod.EndDate = payPeriodEnd;
                this.Add( currentPayPeriod );
                this.Context.SaveChanges();
            }

            return currentPayPeriod;
        }

        /// <summary>
        /// Gets the current pay period or null if the current pay period doesn't exist yet
        /// </summary>
        /// <returns></returns>
        public TimeCardPayPeriod GetCurrentPayPeriod()
        {
            var currentDate = RockDateTime.Today;
            return this.Queryable().Where( a => currentDate >= a.StartDate && currentDate <= a.EndDate ).FirstOrDefault();
        }

        /// <summary>
        /// Gets a list of Person Ids for people that are Staff that report to specified approverPerson or are co-staff of the approverPerson if the approverPerson CanApproveTimecard
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="approverPerson">The approver person.</param>
        /// <param name="canApproveTimecardAttributeKey">The can approve timecard attribute key.</param>
        /// <returns></returns>
        public static IQueryable<int> GetApproveesForPerson( Rock.Data.RockContext rockContext, Person approverPerson, string canApproveTimecardAttributeKey )
        {
            Guid orgUnitGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT.AsGuid();
            Guid groupLeaderGuid = Rock.SystemGuid.GroupRole.GROUPROLE_ORGANIZATION_UNIT_LEADER.AsGuid();
            Guid groupStaffGuid = Rock.SystemGuid.GroupRole.GROUPROLE_ORGANIZATION_UNIT_STAFF.AsGuid();

            // figure out what department the person is in (hopefully at most one department, but we'll deal with multiple just in case)
            var groupMemberService = new GroupMemberService( rockContext );
            if ( approverPerson.Attributes == null )
            {
                approverPerson.LoadAttributes();
            }

            bool canApproveTimecardIfNotLeader = !string.IsNullOrWhiteSpace( canApproveTimecardAttributeKey ) && ( approverPerson.GetAttributeValue( canApproveTimecardAttributeKey ).AsBooleanOrNull() ?? false );

            // get the dept group(s) that they are a leader of, or are just member of but can approve timecards
            var qryPersonDeptLeaderGroup = groupMemberService.Queryable().Where( a => a.PersonId == approverPerson.Id )
                .Where( a => a.Group.GroupType.Guid == orgUnitGroupTypeGuid && ( canApproveTimecardIfNotLeader || a.GroupRole.Guid == groupLeaderGuid ) )
                .Select( a => a.Group );

            var staffPersonIds = groupMemberService.Queryable()
                .Where( a => qryPersonDeptLeaderGroup.Any( x => x.Id == a.GroupId ) )
                .Where( a => a.GroupRole.Guid == groupStaffGuid )
                .Select( a => a.PersonId );

            return staffPersonIds;
        }

        /// <summary>
        /// Gets the leaders/approvers for staff person starting with the most immediate leader/approvers(s) and ending with the organization leader(s)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="staffPersonId">The staff person identifier.</param>
        /// <param name="canApproveTimecardAttributeKey">The can approve timecard attribute key.</param>
        /// <returns></returns>
        public static List<Person> GetApproversForStaffPerson( Rock.Data.RockContext rockContext, int staffPersonId, string canApproveTimecardAttributeKey )
        {
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            Guid orgUnitGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT.AsGuid();

            // figure out what department the person works in (hopefully at most one department, but we'll deal with multiple just in case)
            var qryPersonDeptGroup = groupMemberService.Queryable().Where( a => a.PersonId == staffPersonId ).Where( a => a.Group.GroupType.Guid == orgUnitGroupTypeGuid ).Select( a => a.Group );

            // get a list of the department(s) and the parent departments so that we can get a list of leaders/approvers that this person could submit the timecard to (starting with most immediate leader/approver)
            List<int> departmentGroupIds = new List<int>();

            foreach ( var deptGroup in qryPersonDeptGroup.ToList() )
            {
                departmentGroupIds.Add( deptGroup.Id );

                // TODO: Use GroupService.GetAncestorIds to do this after next merge from core
                var parentGroup = deptGroup.ParentGroup;
                while ( parentGroup != null )
                {
                    departmentGroupIds.Add( parentGroup.Id );
                    parentGroup = parentGroup.ParentGroup;
                }
            }

            List<Person> leadersApprovers = new List<Person>();
            Guid groupLeaderGuid = Rock.SystemGuid.GroupRole.GROUPROLE_ORGANIZATION_UNIT_LEADER.AsGuid();
            foreach ( var deptGroupId in departmentGroupIds )
            {
                var qryDeptMembers = groupMemberService.Queryable().Where( a => a.GroupId == deptGroupId );

                foreach ( var groupMember in qryDeptMembers )
                {
                    // if they have the leader role, they are automatically an approver
                    if ( groupMember.GroupRole.Guid == groupLeaderGuid )
                    {
                        leadersApprovers.Add( groupMember.Person );
                    }
                    else if ( !string.IsNullOrWhiteSpace( canApproveTimecardAttributeKey ) )
                    {
                        // if they don't have the leader role, figure out if they are approver from the CanApproveTimecardAttributeK
                        groupMember.Person.LoadAttributes();
                        if ( groupMember.Person.GetAttributeValue( canApproveTimecardAttributeKey ).AsBooleanOrNull() ?? false )
                        {
                            leadersApprovers.Add( groupMember.Person );
                        }
                    }
                }
            }
            return leadersApprovers;
        }
    }
}
