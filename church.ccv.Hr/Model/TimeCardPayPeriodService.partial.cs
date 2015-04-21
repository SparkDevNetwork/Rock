using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rock;
using Rock.Model;
using Rock.Web.Cache;

namespace church.ccv.Hr.Model
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
                // assume 14-day PayPeriods starting on firstPayPeriodStartDate
                DateTime currentDate = RockDateTime.Today;

                var payPeriodEnd = firstPayPeriodStartDate.AddDays( 14 );
                while ( payPeriodEnd <= currentDate )
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
            return this.Queryable().Where( a => currentDate >= a.StartDate && currentDate < a.EndDate ).FirstOrDefault();
        }

        /// <summary>
        /// Ensures the previous pay period.
        /// </summary>
        /// <param name="firstPayPeriodStartDate">The first pay period start date.</param>
        /// <returns></returns>
        public TimeCardPayPeriod EnsurePreviousPayPeriod( DateTime firstPayPeriodStartDate )
        {
            // ensure that previous pay period exists in case they need to edit it but it hasn't been created yet
            TimeCardPayPeriod previousPayPeriod = GetPreviousPayPeriod();
            if ( previousPayPeriod == null )
            {
                // assume 14-day PayPeriods starting on firstPayPeriodStartDate
                DateTime lastPayPeriodDate = RockDateTime.Today.AddDays( -14 );

                var payPeriodEnd = firstPayPeriodStartDate.AddDays( 14 );
                while ( payPeriodEnd <= lastPayPeriodDate )
                {
                    payPeriodEnd = payPeriodEnd.AddDays( 14 );
                }

                previousPayPeriod = new TimeCardPayPeriod();
                previousPayPeriod.StartDate = payPeriodEnd.AddDays( -14 );
                previousPayPeriod.EndDate = payPeriodEnd;
                this.Add( previousPayPeriod );
                this.Context.SaveChanges();
            }

            return previousPayPeriod;
        }

        /// Gets the previous pay period or null if the previous pay period doesn't exist yet
        public TimeCardPayPeriod GetPreviousPayPeriod()
        {
            var lastPayPeriodDate = RockDateTime.Today.AddDays(-14);
            return this.Queryable().Where( a => lastPayPeriodDate >= a.StartDate && lastPayPeriodDate < a.EndDate ).FirstOrDefault();
        }

        /// <summary>
        /// Gets a list of Person Ids for people that are Staff that report to specified approverPerson or are co-staff of the approverPerson if the approverPerson CanApproveTimecard
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="approverPerson">The approver person.</param>
        /// <returns></returns>
        public static IQueryable<int> GetApproveesForPerson( Rock.Data.RockContext rockContext, Person approverPerson )
        {
            Guid orgUnitGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT.AsGuid();
            Guid groupStaffGuid = Rock.SystemGuid.GroupRole.GROUPROLE_ORGANIZATION_UNIT_STAFF.AsGuid();

            // figure out what department the approver person is in (hopefully at most one department, but we'll deal with multiple just in case)
            var groupMemberService = new GroupMemberService( rockContext );

            // get the dept group(s) that they are a member of where their CanApproveTimeCards group member attribute = 'true'
            var qryPersonDeptCanApproveGroup = groupMemberService.Queryable().Where( a => a.PersonId == approverPerson.Id )
                .Where( a => a.Group.GroupType.Guid == orgUnitGroupTypeGuid )
                .WhereAttributeValue( rockContext, "CanApproveTimeCards", true.ToString() )
                .Select( a => a.Group );

            var staffPersonIds = groupMemberService.Queryable()
                .Where( a => qryPersonDeptCanApproveGroup.Any( x => x.Id == a.GroupId ) )
                .Where( a => a.GroupRole.Guid == groupStaffGuid )
                .Select( a => a.PersonId );

            return staffPersonIds;
        }

        /// <summary>
        /// Validates that the CanApproveTimeCards GroupMember attribute exists.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static bool ValidateApproverAttributeExists( Rock.Data.RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            var groupMemberEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.GroupMember ) ).Id;
            return attributeService.Queryable().Any( a => a.EntityTypeId == groupMemberEntityTypeId && a.Key == "CanApproveTimeCards" );
        }

        /// <summary>
        /// Gets the approvers for staff person starting with the most approvers(s) and ending with the approvers at the top of the org chart
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="staffPersonId">The staff person identifier.</param>
        /// <returns></returns>
        public static List<Person> GetApproversForStaffPerson( Rock.Data.RockContext rockContext, int staffPersonId )
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
                departmentGroupIds.AddRange( groupService.GetAllAncestorIds( deptGroup.Id ) );
            }

            List<Person> approvers = new List<Person>();
            foreach ( var deptGroupId in departmentGroupIds )
            {
                var qryDeptApprovers = groupMemberService.Queryable().Where( a => a.GroupId == deptGroupId ).WhereAttributeValue( rockContext, "CanApproveTimeCards", true.ToString() );
                approvers.AddRange( qryDeptApprovers.Select( a => a.Person ).ToList() );
            }

            return approvers.Distinct().ToList();
        }
    }
}
