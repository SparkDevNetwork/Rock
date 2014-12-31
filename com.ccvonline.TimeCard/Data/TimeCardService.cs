using System;
using System.Globalization;
using System.Linq;
using com.ccvonline.TimeCard.Model;
using Rock;

namespace com.ccvonline.TimeCard.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TimeCardService<T> : Rock.Data.Service<T> where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationService{T}"/> class.
        /// </summary>
        public TimeCardService( TimeCardContext context )
            : base( context )
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public virtual bool CanDelete( T item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Ensures that there is a current pay period based on the current date
        /// and returns the current pay period
        /// </summary>
        /// <param name="payrollStartDay">The payroll start day.</param>
        /// <returns></returns>
        public TimeCardPayPeriod EnsureCurrentPayPeriod( DayOfWeek payrollStartDay )
        {
            var timeCardPayPeriodService = new TimeCardService<com.ccvonline.TimeCard.Model.TimeCardPayPeriod>( this.Context as TimeCardContext );
            var currentDate = RockDateTime.Today;
            var currentPayPeriod = timeCardPayPeriodService.Queryable().Where( a => currentDate >= a.StartDate && currentDate <= a.EndDate ).FirstOrDefault();
            if ( currentPayPeriod == null )
            {
                // assume 14 PayPeriods starting on first Saturday of Year
                DateTime jan1 = new DateTime( currentDate.Year, 1, 1 );
                int daysOffset = payrollStartDay - jan1.DayOfWeek;
                DateTime firstSaturday = jan1.AddDays( daysOffset );
                Calendar cal = new GregorianCalendar( GregorianCalendarTypes.USEnglish );
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
                timeCardPayPeriodService.Add( currentPayPeriod );
                this.Context.SaveChanges();
            }

            return currentPayPeriod;
        }

        


        
    }
}
