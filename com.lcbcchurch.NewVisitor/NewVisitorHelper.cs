using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.lcbcchurch.NewVisitor
{
    public class NewVisitorHelper
    {
        /// <summary>
        /// Uses the Rock GLOBAL start day of week to calculate the starting date for "this" week of the given date.
        /// </summary>
        private static DateTime StartOfWeek( DateTime aDate )
        {
            DayOfWeek startDayOfWeek = DayOfWeek.Saturday;
            int diff = ( 7 + ( aDate.DayOfWeek - startDayOfWeek ) ) % 7;
            return aDate.AddDays( -1 * diff ).Date;
        }

        /// <summary>
        /// Uses the Rock GLOBAL start day of week to calculate the starting date for "last" week of the given date.
        /// </summary>
        public static DateTime StartOfLastWeek( DateTime aDate )
        {
            return StartOfWeek( aDate ).AddDays( -7 );
        }
    }
}
