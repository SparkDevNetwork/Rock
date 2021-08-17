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

namespace Rock.Model
{
    /// <summary>
    /// A little helper to get a list of US Holidays http://www.usa.gov/citizens/holidays.shtml  for a specified Year  
    /// C# from http://stackoverflow.com/a/18790381/1755417
    /// </summary>
    public static class HolidayHelper
    {
        /// <summary>
        /// Holiday Class
        /// </summary>
        public class Holiday
        {
            /// <summary>
            /// Gets or sets the name of the holiday.
            /// </summary>
            /// <value>
            /// The name of the holiday.
            /// </value>
            public string HolidayName { get; set; }

            /// <summary>
            /// Gets or sets the date.
            /// </summary>
            /// <value>
            /// The date.
            /// </value>
            public DateTime Date { get; set; }

            /// <summary>
            /// Gets or sets the week number of year that the Holiday is in (using RockDateTime.FirstDayOfWeek)
            /// </summary>
            /// <value>
            /// The holiday week number of year.
            /// </value>
            public int HolidayWeekNumberOfYear { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Holiday"/> class.
            /// </summary>
            /// <param name="holidayName">Name of the holiday.</param>
            /// <param name="date">The date.</param>
            internal Holiday( string holidayName, DateTime date )
            {
                HolidayName = holidayName;
                Date = date;
            }
        }

        /// <summary>
        /// Gets a list of holidays.
        /// </summary>
        /// <param name="vYear">The v year.</param>
        /// <returns></returns>
        public static List<Holiday> GetHolidayList( int vYear )
        {
            const int FirstWeek = 1;
            const int SecondWeek = 2;
            const int ThirdWeek = 3;
            const int FourthWeek = 4;
            const int LastWeek = 5;

            List<Holiday> holidayList = new List<Holiday>();

            // http://www.usa.gov/citizens/holidays.shtml      
            // http://archive.opm.gov/operating_status_schedules/fedhol/2013.asp

            // New Year's Day            Jan 1
            holidayList.Add( new Holiday( "NewYears", new DateTime( vYear, 1, 1 ) ) );

            // NOTE: DayOfWeek.Monday is used since these Holidays occur on Mondays ( it has nothing to do with RockDateTime.FirstDayOfWeek)

            // Martin Luther King, Jr. third Mon in Jan
            holidayList.Add( new Holiday( "MLK", GetNthDayOfNthWeek( new DateTime( vYear, 1, 1 ), DayOfWeek.Monday, ThirdWeek ) ) );

            // Washington's Birthday third Mon in Feb
            holidayList.Add( new Holiday( "WashingtonsBDay", GetNthDayOfNthWeek( new DateTime( vYear, 2, 1 ), DayOfWeek.Monday, ThirdWeek ) ) );

            // Memorial Day          last Mon in May
            holidayList.Add( new Holiday( "MemorialDay", GetNthDayOfNthWeek( new DateTime( vYear, 5, 1 ), DayOfWeek.Monday, LastWeek ) ) );

            // Independence Day      July 4
            holidayList.Add( new Holiday( "IndependenceDay", new DateTime( vYear, 7, 4 ) ) );

            // Labor Day             first Mon in Sept
            holidayList.Add( new Holiday( "LaborDay", GetNthDayOfNthWeek( new DateTime( vYear, 9, 1 ), DayOfWeek.Monday, FirstWeek ) ) );

            // Columbus Day          second Mon in Oct
            holidayList.Add( new Holiday( "Columbus", GetNthDayOfNthWeek( new DateTime( vYear, 10, 1 ), DayOfWeek.Monday, SecondWeek ) ) );

            // Veterans Day          Nov 11
            holidayList.Add( new Holiday( "Veterans", new DateTime( vYear, 11, 11 ) ) );

            // Thanksgiving Day      fourth Thur in Nov
            holidayList.Add( new Holiday( "Thanksgiving", GetNthDayOfNthWeek( new DateTime( vYear, 11, 1 ), DayOfWeek.Thursday, FourthWeek ) ) );

            // Christmas Day         Dec 25
            holidayList.Add( new Holiday( "Christmas", new DateTime( vYear, 12, 25 ) ) );

            // saturday holidays are moved to Fri; Sun to Mon
            foreach ( var holiday in holidayList )
            {
                if ( holiday.Date.DayOfWeek == DayOfWeek.Saturday )
                {
                    holiday.Date = holiday.Date.AddDays( -1 );
                }

                if ( holiday.Date.DayOfWeek == DayOfWeek.Sunday )
                {
                    holiday.Date = holiday.Date.AddDays( 1 );
                }
            }

            foreach(var holiday in holidayList)
            {
                holiday.HolidayWeekNumberOfYear = holiday.Date.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, RockDateTime.FirstDayOfWeek );
            }

            return holidayList;
        }

        /// <summary>
        /// Gets the NTH day of NTH week.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="dayofWeek">The dayof week.</param>
        /// <param name="whichWeek">The which week.</param>
        /// <returns></returns>
        private static System.DateTime GetNthDayOfNthWeek( DateTime dt, DayOfWeek dayofWeek, int whichWeek )
        {
            // specify which day of which week of a month and this function will get the date
            // this function uses the month and year of the date provided

            // get first day of the given date
            System.DateTime dtFirst = new DateTime( dt.Year, dt.Month, 1 );

            // get first DayOfWeek of the month
            System.DateTime dtRet = dtFirst.AddDays( 6 - (int)dtFirst.AddDays( -1 * ( (int)dayofWeek + 1 ) ).DayOfWeek );

            // get which week
            dtRet = dtRet.AddDays( ( whichWeek - 1 ) * 7 );

            // if day is past end of month then adjust backwards a week
            if ( dtRet >= dtFirst.AddMonths( 1 ) )
            {
                dtRet = dtRet.AddDays( -7 );
            }

            return dtRet;
        }

        /// <summary>
        /// Determines the date of Easter Sunday for a given year
        /// http://stackoverflow.com/a/2510411/1755417
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public static DateTime EasterSunday( int year )
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = ( c - (int)( c / 4 ) - (int)( ( 8 * c + 13 ) / 25 ) + 19 * g + 15 ) % 30;
            int i = h - (int)( h / 28 ) * ( 1 - (int)( h / 28 ) * (int)( 29 / ( h + 1 ) ) * (int)( ( 21 - g ) / 11 ) );

            day = i - ( ( year + (int)( year / 4 ) + i + 2 - c + (int)( c / 4 ) ) % 7 ) + 28;
            month = 3;

            if ( day > 31 )
            {
                month++;
                day -= 31;
            }

            return new DateTime( year, month, day );
        }
    }
}
