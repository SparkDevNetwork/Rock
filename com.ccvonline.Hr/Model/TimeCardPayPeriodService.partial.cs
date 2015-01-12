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
    }
}
