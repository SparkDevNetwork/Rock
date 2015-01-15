using System;
using System.Globalization;
using System.Linq;
using Rock;

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
        /// <param name="payrollStartDay">The payroll start day.</param>
        /// <returns></returns>
        public TimeCardPayPeriod EnsureCurrentPayPeriod( DayOfWeek payrollStartDay )
        {
            TimeCardPayPeriod currentPayPeriod = GetCurrentPayPeriod();
            if ( currentPayPeriod == null )
            {
                // assume 14 PayPeriods starting on first Saturday of Year
                DateTime currentDate = RockDateTime.Today;
                DateTime jan1 = new DateTime( currentDate.Year, 1, 1 );
                int daysOffset = payrollStartDay - jan1.DayOfWeek;
                DateTime firstSaturday = jan1.AddDays( daysOffset );
                System.Globalization.Calendar cal = new GregorianCalendar( GregorianCalendarTypes.USEnglish );
                var firstWeek = cal.GetWeekOfYear( firstSaturday, CalendarWeekRule.FirstFullWeek, payrollStartDay );
                int weekNum = 1;
                if ( firstWeek <= 1 )
                {
                    weekNum--;
                }

                firstSaturday = firstSaturday.AddDays( weekNum * 7 );
                var payPeriodEnd = firstSaturday.AddDays( -14 );
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
        /// Gets a list of Person Ids for people that are Staff that report to specified leaderPersonId.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="leaderPersonId">The leader person identifier.</param>
        /// <returns></returns>
        public static IQueryable<int> GetStaffThatReportToPerson( Rock.Data.RockContext rockContext, int leaderPersonId )
        {
            // TODO use Rock SystemGuids for these after next merge from core
            string GROUPROLE_ORGANIZATION_UNIT_LEADER = "8438D6C5-DB92-4C99-947B-60E9100F223D";
            string GROUPROLE_ORGANIZATION_UNIT_STAFF = "17E516FC-76A4-4BF4-9B6F-0F859B13F563";

            Guid orgUnitGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT.AsGuid();
            Guid groupLeaderGuid = GROUPROLE_ORGANIZATION_UNIT_LEADER.AsGuid();
            Guid groupStaffGuid = GROUPROLE_ORGANIZATION_UNIT_STAFF.AsGuid();

            // figure out what department the person is a leader in (hopefully at most one department, but we'll deal with multiple just in case)
            var groupMemberService = new Rock.Model.GroupMemberService( rockContext );
            var qryPersonDeptLeaderGroup = groupMemberService.Queryable().Where( a => a.PersonId == leaderPersonId ).Where( a => a.Group.GroupType.Guid == orgUnitGroupTypeGuid && a.GroupRole.Guid == groupLeaderGuid ).Select( a => a.Group );

            var staffPersonIds = groupMemberService.Queryable()
                .Where( a => qryPersonDeptLeaderGroup.Any( x => x.Id == a.GroupId ) )
                .Where( a => a.GroupRole.Guid == groupStaffGuid )
                .Select( a => a.PersonId );
            
            return staffPersonIds;
        }
    }
}
