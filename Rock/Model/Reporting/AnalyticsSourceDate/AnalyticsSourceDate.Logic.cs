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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

using EntityFramework.Utilities;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsSourceDate Logic
    /// </summary>
    public partial class AnalyticsSourceDate
    {
        #region Constants

        /*
         * 14-DEC-21 DMV
         *
         * Since this SQL script is needed by the Generate method here and the migration
         * the recommendation to me was to create a constant string for it.
         *
         */

        /// <summary>
        /// The Sunday of the year SQL script.
        /// </summary>
        internal const string SundayOfTheYearSql =@"/*
	Assumes the following new columns
	* [WeekOfYear]
	* [WeekCounter]
	* [LeapYearIndicator]
	* [SundayDateYear]
*/

DECLARE @FirstDayOfWeek INT;
SELECT @FirstDayOfWeek = [DefaultValue]
FROM [Attribute]
WHERE [EntityTypeQualifierColumn] = 'SystemSetting' AND [Key] = 'core_StartDayOfWeek';

IF @FirstDayOfWeek IS NULL
BEGIN
    SET @FirstDayOfWeek = 1;
END

-- Rock stores Sunday as 0, DATEFIRST expects Sunday as 7
IF @FirstDayOfWeek = 0
BEGIN
    SET @FirstDayOfWeek = 7;
END

-- IMPORTANT: This value should come from the Starting Day of Week setting here: https://prealpha.rocksolidchurchdemo.com/admin/system/configuration
SET DATEFIRST @FirstDayOfWeek;

-- Update the week of year for Sundays
UPDATE [AnalyticsSourceDate]
SET [WeekOfYear] = DATEPART(wk, [Date])
WHERE [DayOfWeek] = 0;

-- Create temp table of the Sunday dates (makes update below simplier)
DECLARE @SundayLookup TABLE ([LookupDate] date, [LookupWeekOfYear] int );

-- Insert values into lookup
INSERT INTO @SundayLookup
SELECT
	[Date]
	, [WeekOfYear]
FROM [AnalyticsSourceDate]
WHERE [DayOfWeek] = 0;

-- Update WeekOfYear for all non-sundays
UPDATE  a
SET     a.[WeekOfYear] = b.[LookupWeekOfYear]
FROM    [AnalyticsSourceDate] a
        INNER JOIN @SundayLookup b
           ON b.[LookupDate] = a.[SundayDate]
WHERE [DayOfWeek] != 0;

-- Add weekcounter
DECLARE @WeekCounterLookup TABLE ([LookupYear] int, [LookupWeekOfYear] int, [LookupWeekCounter] int )
INSERT INTO @WeekCounterLookup
SELECT
	*
	, ROW_NUMBER() OVER( ORDER BY [Year], [WeekOfYear]) AS [WeekCounter]
FROM (
	SELECT
		YEAR([SundayDate]) AS [Year]
		, [WeekOfYear]
	FROM  [AnalyticsSourceDate]
	GROUP BY YEAR([SundayDate]), [WeekOfYear]
) x;

UPDATE  a
SET     a.[WeekCounter] = b.[LookupWeekCounter]
FROM    [AnalyticsSourceDate] a
        INNER JOIN @WeekCounterLookup b
           ON b.[LookupYear] = YEAR(a.[SundayDate]) AND b.[LookupWeekOfYear] = a.[WeekOfYear];

-- Update Leap Year
UPDATE [AnalyticsSourceDate]
SET [LeapYearIndicator] = CASE WHEN ([CalendarYear] % 4 = 0 AND [CalendarYear] % 100 <> 0) OR [CalendarYear] % 400 = 0 THEN 1 ELSE 0 END;

-- Sunday Date Year
UPDATE [AnalyticsSourceDate]
SET [SundayDateYear] = YEAR([SundayDate]);";

        #endregion

        /// <summary>
        /// Determines the Fiscal Year (the calendar year in which the fiscal year ends) for the specified date and fiscal start month.
        /// </summary>
        /// <param name="fiscalStartMonth">The fiscal start month.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        [Obsolete("Use the new GetFiscalYear method signature below.")]
        private static int GetFiscalYear( int fiscalStartMonth, DateTime date )
        {
            int fiscalYearStart = date.AddMonths( -( fiscalStartMonth - 1 ) ).Year;
            int fiscalYear = fiscalStartMonth == 1 ? fiscalYearStart : fiscalYearStart + 1;
            return fiscalYear;
        }

        /// <summary>
        /// Populates the AnalyticsSourceDate table (and associated Views). It will first empty the AnalyticsSourceDate table if there is already data in it.
        /// </summary>
        /// <param name="fiscalStartMonth">The fiscal start month.</param>
        /// <param name="givingMonthUseSundayDate">if set to <c>true</c> [giving month use Sunday date].</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        public static void GenerateAnalyticsSourceDateData( int fiscalStartMonth, bool givingMonthUseSundayDate, DateTime startDate, DateTime endDate )
        {
            // remove all the rows and rebuild
            using ( var rockContext = new RockContext() )
            {
                try
                {
                    // if TRUNCATE takes more than 5 seconds, it is probably due to a lock. If so, do a DELETE FROM instead
                    rockContext.Database.CommandTimeout = 5;
                    rockContext.Database.ExecuteSqlCommand( string.Format( "TRUNCATE TABLE {0}", typeof( AnalyticsSourceDate ).GetCustomAttribute<TableAttribute>().Name ) );
                }
                catch
                {
                    rockContext.Database.CommandTimeout = null;
                    rockContext.Database.ExecuteSqlCommand( string.Format( "DELETE FROM {0}", typeof( AnalyticsSourceDate ).GetCustomAttribute<TableAttribute>().Name ) );
                }
            }

            List<AnalyticsSourceDate> generatedDates = new List<AnalyticsSourceDate>();

            // NOTE: AnalyticsSourceDate is not an Rock.Model.Entity table and therefore doesn't have a Service<T>, so just use update using rockContext.AnalyticsDimDates
            var generateDate = startDate;
            var currentYear = generateDate.Year;
            var holidayDatesForYear = HolidayHelper.GetHolidayList( currentYear );
            var easterSundayForYear = HolidayHelper.EasterSunday( currentYear );
            var easterWeekNumberOfYear = easterSundayForYear.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, RockDateTime.FirstDayOfWeek );
            var christmasWeekNumberOfYear = new DateTime( currentYear, 12, 25 ).GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, RockDateTime.FirstDayOfWeek );

            while ( generateDate <= endDate )
            {
                if ( currentYear != generateDate.Year )
                {
                    currentYear = generateDate.Year;
                    holidayDatesForYear = HolidayHelper.GetHolidayList( currentYear );
                    easterSundayForYear = HolidayHelper.EasterSunday( currentYear );
                    easterWeekNumberOfYear = easterSundayForYear.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, RockDateTime.FirstDayOfWeek );
                    christmasWeekNumberOfYear = new DateTime( currentYear, 12, 25 ).GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, RockDateTime.FirstDayOfWeek );
                }

                AnalyticsSourceDate analyticsSourceDate = new AnalyticsSourceDate();
                analyticsSourceDate.DateKey = generateDate.ToString( "yyyyMMdd" ).AsInteger();
                analyticsSourceDate.Date = generateDate.Date;

                // Long Date (Monday January 1st, 2016)
                analyticsSourceDate.FullDateDescription = generateDate.ToLongDateString();

                analyticsSourceDate.DayOfWeek = generateDate.DayOfWeek.ConvertToInt();
                analyticsSourceDate.DayOfWeekName = generateDate.DayOfWeek.ConvertToString();

                // luckily, DayOfWeek abbreviations are just the first 3 chars
                analyticsSourceDate.DayOfWeekAbbreviated = analyticsSourceDate.DayOfWeekName.Substring( 0, 3 );
                analyticsSourceDate.DayNumberInCalendarMonth = generateDate.Day;
                analyticsSourceDate.DayNumberInCalendarYear = generateDate.DayOfYear;

                // from http://stackoverflow.com/a/4655207/1755417
                DateTime endOfMonth = new DateTime( generateDate.Year, generateDate.Month, DateTime.DaysInMonth( generateDate.Year, generateDate.Month ) );

                analyticsSourceDate.LastDayInMonthIndictor = generateDate == endOfMonth;

                // note, this ends up doing the same thing as WeekOfMonth in https://www.mssqltips.com/sqlservertip/4054/creating-a-date-dimension-or-calendar-table-in-sql-server/, but with RockDateTime.FirstDayOfWeek
                analyticsSourceDate.WeekNumberInMonth = generateDate.GetWeekOfMonth( RockDateTime.FirstDayOfWeek );

                analyticsSourceDate.SundayDate = generateDate.SundayDate();

                analyticsSourceDate.CalendarWeek = generateDate.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, RockDateTime.FirstDayOfWeek );
                analyticsSourceDate.CalendarMonth = generateDate.Month;
                analyticsSourceDate.CalendarMonthName = generateDate.ToString( "MMMM" );
                analyticsSourceDate.CalendarMonthNameAbbreviated = generateDate.ToString( "MMM" );

                analyticsSourceDate.CalendarYearMonth = generateDate.ToString( "yyyyMM" );
                analyticsSourceDate.CalendarYearMonthName = generateDate.ToString( "yyyy MMM" );

                int quarter = ( generateDate.Month + 2 ) / 3;

                analyticsSourceDate.CalendarQuarter = string.Format( "Q{0}", quarter );
                analyticsSourceDate.CalendarYearQuarter = string.Format( "{0} Q{1}", generateDate.Year, quarter );
                analyticsSourceDate.CalendarYear = generateDate.Year;

                /* Fiscal Calendar Stuff */
                int fiscalWeekNumber = GetFiscalWeek( generateDate, fiscalStartMonth, RockDateTime.FirstDayOfWeek, 4 );
                int fiscalYear = GetFiscalYear( generateDate, fiscalWeekNumber, fiscalStartMonth, RockDateTime.FirstDayOfWeek, 4 );
                var fiscalYearStart = GetFirstDayOfFiscalYear( generateDate, fiscalStartMonth );

                // figure out the fiscalMonthNumber and QTR.  For example, if the Fiscal Start Month is April, and Today is April 1st, the Fiscal Month Number would be 1
                int fiscalMonthNumber = GetFiscalMonthNumber( generateDate, fiscalStartMonth );
                int fiscalQuarter = GetFiscalQuarter( generateDate, fiscalStartMonth, RockDateTime.FirstDayOfWeek, 4 );

                analyticsSourceDate.FiscalWeek = fiscalWeekNumber;
                analyticsSourceDate.FiscalWeekNumberInYear = generateDate.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstFourDayWeek, RockDateTime.FirstDayOfWeek );
                analyticsSourceDate.FiscalMonth = generateDate.ToString( "MMMM" );
                analyticsSourceDate.FiscalMonthAbbreviated = generateDate.ToString( "MMM" );
                analyticsSourceDate.FiscalMonthNumberInYear = fiscalMonthNumber;
                analyticsSourceDate.FiscalMonthYear = $"{fiscalMonthNumber:00} {fiscalYear}";
                analyticsSourceDate.FiscalQuarter = string.Format( "Q{0}", fiscalQuarter );
                analyticsSourceDate.FiscalYearQuarter = string.Format( "{0} Q{1}", fiscalYear, fiscalQuarter );
                analyticsSourceDate.FiscalHalfYear = GetFiscalHalfYear( fiscalQuarter );
                analyticsSourceDate.FiscalYear = fiscalYear;

                // DayNumberInFiscalMonth is simply the same as DayNumberInCalendarMonth since we only let them pick a FiscalStartMonth ( not a Month and Day )
                analyticsSourceDate.DayNumberInFiscalMonth = analyticsSourceDate.DayNumberInCalendarMonth;

                if ( fiscalStartMonth == 1 )
                {
                    analyticsSourceDate.DayNumberInFiscalYear = analyticsSourceDate.DayNumberInCalendarYear;
                }
                else
                {
                    if ( fiscalYearStart <= generateDate )
                    {
                        // fiscal start is the same year as the generated date year
                        analyticsSourceDate.DayNumberInFiscalYear = ( generateDate - fiscalYearStart ).Days + 1;
                    }
                    else
                    {
                        // fiscal start is the year previous to the generated date year
                        analyticsSourceDate.DayNumberInFiscalYear = ( generateDate - fiscalYearStart.AddYears( -1 ) ).Days + 1;
                    }
                }

                // NOTE: This is complicated, see comments on AnalyticsSourceDate.GivingMonth
                if ( givingMonthUseSundayDate )
                {
                    var givingMonthSundayDate = generateDate.SundayDate();
                    int sundayFiscalYear = GetFiscalYear( givingMonthSundayDate, fiscalWeekNumber, fiscalStartMonth, RockDateTime.FirstDayOfWeek, 4 );

                    if ( sundayFiscalYear != fiscalYear )
                    {
                        // if the SundayDate ends up landing in a different year, don't use the Sunday Date, just use the actual generate date
                        analyticsSourceDate.GivingMonth = generateDate.Month;
                        analyticsSourceDate.GivingMonthName = generateDate.ToString( "MMMM" );
                    }
                    else
                    {
                        analyticsSourceDate.GivingMonth = givingMonthSundayDate.Month;
                        analyticsSourceDate.GivingMonthName = givingMonthSundayDate.ToString( "MMMM" );
                    }
                }
                else
                {
                    analyticsSourceDate.GivingMonth = generateDate.Month;
                    analyticsSourceDate.GivingMonthName = generateDate.ToString( "MMMM" );
                }

                analyticsSourceDate.EasterIndicator = generateDate == easterSundayForYear;

                // Traditional Easter Week starts the Sunday before Easter (Palm Sunday) and ends on Holy Saturday (the day before Easter Sunday),
                // However, for the purposes of Rock Metrics, this is just week starting the Monday of or before the Holiday date
                int daysUntilEaster = ( easterSundayForYear - generateDate ).Days;
                analyticsSourceDate.EasterWeekIndicator = easterWeekNumberOfYear == analyticsSourceDate.CalendarWeek;

                analyticsSourceDate.ChristmasIndicator = generateDate.Month == 12 && generateDate.Day == 25;

                // Traditional Christmas week is 12/24-12/30
                // However, for the purposes of Rock Metrics, this is just week starting the Monday of or before the Holiday date
                analyticsSourceDate.ChristmasWeekIndicator = christmasWeekNumberOfYear == analyticsSourceDate.CalendarWeek;

                analyticsSourceDate.HolidayIndicator = holidayDatesForYear.Any( a => a.Date == generateDate ) || analyticsSourceDate.ChristmasIndicator || analyticsSourceDate.EasterIndicator;

                // Week Inclusive starting Monday for all holidays
                analyticsSourceDate.WeekHolidayIndicator =
                    analyticsSourceDate.ChristmasWeekIndicator
                    || analyticsSourceDate.EasterWeekIndicator
                    || holidayDatesForYear.Any( a => a.HolidayWeekNumberOfYear == analyticsSourceDate.CalendarWeek );

                analyticsSourceDate.Count = 1;

                generatedDates.Add( analyticsSourceDate );

                generateDate = generateDate.AddDays( 1 );
            }

            using ( var rockContext = new RockContext() )
            {
                // NOTE: We can't use rockContext.BulkInsert because that enforces that the <T> is Rock.Data.IEntity, so we'll just use EFBatchOperation directly
                EFBatchOperation.For( rockContext, rockContext.Set<AnalyticsSourceDate>() ).InsertAll( generatedDates );

                // Update the Sunday of the Year data
                rockContext.Database.ExecuteSqlCommand( SundayOfTheYearSql );
            }
        }

        /// <summary>
        /// This method calculates the fiscal week number of a specified date.
        /// </summary>
        /// <param name="date">The date to calculate the fiscal week for.</param>
        /// <param name="fiscalYearStartMonth">The number of the month that represents the start of the fiscal calendar (e.g., April = 4).</param>
        /// <param name="firstDayOfWeek">The first day of the week (<see cref="RockDateTime.FirstDayOfWeek"/>).</param>
        /// <param name="minimumDaysRequiredInFirstWeek">The minimum number of days that need to be in the starting week for it to count as the first week.</param>
        /// <returns></returns>
        internal static int GetFiscalWeek( DateTime date, int fiscalYearStartMonth, DayOfWeek firstDayOfWeek, int minimumDaysRequiredInFirstWeek = 4 )
        {
            var fiscalYearStart = GetFirstDayOfFiscalYear( date, fiscalYearStartMonth );
            var fiscalYearWeekStart = GetDateOfFirstWeekOfFiscalYear( fiscalYearStart, firstDayOfWeek, minimumDaysRequiredInFirstWeek );

            // If the fiscal year week start is greater than the date then the date is in the previous fiscal year
            if ( date < fiscalYearWeekStart )
            {
                fiscalYearWeekStart = GetDateOfFirstWeekOfFiscalYear( fiscalYearStart.AddYears( -1 ), firstDayOfWeek, minimumDaysRequiredInFirstWeek );
            }

            // Get weeks since the start of the fiscal year
            var weekNumber = GetWeekNumberFromDate( date, fiscalYearWeekStart, firstDayOfWeek );

            return weekNumber;
        }

        /// <summary>
        /// Determines the fiscal half-year ("First" or "Second") based on the provided fiscal quarter.
        /// </summary>
        /// <param name="fiscalQuarter">
        /// The fiscal quarter as an integer (1 through 4).
        /// </param>
        /// <returns>
        /// A string indicating the half of the fiscal year:
        /// "First" for quarters 1 and 2, "Second" for quarters 3 and 4.
        /// </returns>
        internal static string GetFiscalHalfYear( int fiscalQuarter )
        {
            return fiscalQuarter < 3 ? "First" : "Second";
        }

        /// <summary>
        /// Calculates the fiscal month number for a given date based on the specified fiscal year start month.
        /// </summary>
        /// <param name="date">The date for which to determine the fiscal month number.</param>
        /// <param name="fiscalYearStartMonth">
        /// The starting month of the fiscal year (1 = January, 12 = December).
        /// </param>
        /// <returns>
        /// An integer from 1 to 12 representing the fiscal month number of the provided date,
        /// where 1 corresponds to the first month of the fiscal year.
        /// </returns>
        internal static int GetFiscalMonthNumber( DateTime date, int fiscalYearStartMonth )
        {
            return new DateTime( date.Year, date.Month, 1 ).AddMonths( 1 - fiscalYearStartMonth ).Month;
        }

        /// <summary>
        /// This method calculates the fiscal year of a specified date.
        /// </summary>
        /// <param name="date">The date to calculate the fiscal year for.</param>
        /// <param name="fiscalWeekNumber">The fiscal week number of the given date.</param>
        /// <param name="fiscalYearStartMonth">The number of the month that represents the start of the fiscal calendar (e.g., April = 4).</param>
        /// <param name="firstDayOfWeek">The first day of the week (<see cref="RockDateTime.FirstDayOfWeek"/>).</param>
        /// <param name="minimumDaysRequiredInFirstWeek">The minimum number of days that need to be in the starting week for it to count as the first week.</param>
        /// <returns></returns>
        internal static int GetFiscalYear( DateTime date, int fiscalWeekNumber, int fiscalYearStartMonth, DayOfWeek firstDayOfWeek, int minimumDaysRequiredInFirstWeek = 4 )
        {
            int fiscalYearStart = date.AddMonths( -( fiscalYearStartMonth - 1 ) ).Year;
            int fiscalYear = fiscalYearStartMonth == 1 ? fiscalYearStart : fiscalYearStart + 1;

            // Make an adjustment if the fiscal week number is 52 or higher, but the date is before the fiscal year start date.
            // In that case, the fiscal year should still be the previous year.
            if ( fiscalWeekNumber >= 52 && date.Month == fiscalYearStartMonth )
            {
                var fiscalYearStartDate = GetFirstDayOfFiscalYear( date, fiscalYearStartMonth );
                var fiscalYearWeekStart = GetDateOfFirstWeekOfFiscalYear( fiscalYearStartDate, firstDayOfWeek, minimumDaysRequiredInFirstWeek );
                if ( date < fiscalYearWeekStart )
                {
                    fiscalYear--;
                }
            }
            return fiscalYear;
        }

        /// <summary>
        /// Calculates the fiscal quarter for a given date based on the specified fiscal year start month,
        /// the first day of the week, and the minimum number of days required in the first week of the fiscal year.
        /// </summary>
        /// <param name="date">The date for which to determine the fiscal quarter.</param>
        /// <param name="fiscalYearStartMonth">The starting month of the fiscal year (1 = January, 12 = December).</param>
        /// <param name="firstDayOfWeek">The first day of the week used to determine the start of a week.</param>
        /// <param name="minimumDaysRequiredInFirstWeek">The minimum number of days required in the first week of the fiscal year. Defaults to 4, following ISO 8601-like rules.</param>
        /// <returns>
        /// An integer from 1 to 4 indicating the fiscal quarter the date falls into. Returns 4 if the date falls into
        /// the week prior to the fiscal year start despite being in the fiscal start month.
        /// </returns>
        internal static int GetFiscalQuarter( DateTime date, int fiscalYearStartMonth, DayOfWeek firstDayOfWeek, int minimumDaysRequiredInFirstWeek = 4 )
        {
            int adjustedMonth = ( ( date.Month - fiscalYearStartMonth + 12 ) % 12 ) + 1;
            int fiscalQuarter = ( ( adjustedMonth - 1 ) / 3 ) + 1;

            // Make an adjustment if the fiscal quarter is 1 but the date is before the fiscal year start date.
            // In that case, the fiscal quarter should be Q4 of the previous year.
            if ( fiscalQuarter == 1 )
            {
                var fiscalYearStart = GetFirstDayOfFiscalYear( date, fiscalYearStartMonth );
                var fiscalYearWeekStart = GetDateOfFirstWeekOfFiscalYear( fiscalYearStart, firstDayOfWeek, minimumDaysRequiredInFirstWeek );
                if ( date < fiscalYearWeekStart )
                {
                    fiscalQuarter = 4;
                }
            }

            return fiscalQuarter;
        }

        /// <summary>
        /// Gets the first date the fiscal year for a specified date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="fiscalYearStartMonth">The number of the month that represents the start of the fiscal calendar (e.g., April = 4).</param>
        /// <returns></returns>
        private static DateTime GetFirstDayOfFiscalYear( DateTime date, int fiscalYearStartMonth )
        {
            int fiscalStartYear = date.Month >= fiscalYearStartMonth ||
                 ( date.Month == fiscalYearStartMonth && date.Day >= 1 )
                 ? date.Year
                 : date.Year - 1;

            return new DateTime( fiscalStartYear, fiscalYearStartMonth, 1 );
        }

        /// <summary>
        /// Gets the date of the start of the first week of the fiscal year. If the week does not have the
        /// minimum number of days required in the first week then we'll use the next week.
        /// </summary>
        /// <param name="fiscalStartDate">The date the fiscal year began.</param>
        /// <param name="firstDayOfWeek">The first day of the week (<see cref="RockDateTime.FirstDayOfWeek"/>).</param>
        /// <param name="minimumDaysRequiredInFirstWeek">The minimum number of days that need to be in the starting week for it to count as the first week.</param>
        /// <returns></returns>
        private static DateTime GetDateOfFirstWeekOfFiscalYear( DateTime fiscalStartDate, DayOfWeek firstDayOfWeek, int minimumDaysRequiredInFirstWeek = 4 )
        {
            // Get the start of the week for first day of the fiscal year
            var fiscalWeekStart = GetFirstDayOfWeek( fiscalStartDate, firstDayOfWeek );

            // Get the number of days in the first week of the fiscal year
            var daysInFirstFiscalWeek = 7 - ( fiscalStartDate - fiscalWeekStart ).Days;

            // If we don't have the minimum number of days in the week then move the first week of the fiscal year
            // to the next week
            if ( daysInFirstFiscalWeek < minimumDaysRequiredInFirstWeek )
            {
                return fiscalWeekStart.AddDays( 7 );
            }

            return fiscalStartDate;
        }

        /// <summary>
        /// Calculates the first date of a week.
        /// </summary>
        /// <param name="date">The date to calculate the fiscal week for.</param>
        /// <param name="firstDayOfWeek">The first day of the week (<see cref="RockDateTime.FirstDayOfWeek"/>).</param>
        /// <returns></returns>
        private static DateTime GetFirstDayOfWeek( DateTime date, DayOfWeek firstDayOfWeek )
        {
            int daysToSubtract = ( ( int ) date.DayOfWeek - ( int ) firstDayOfWeek + 7 ) % 7;
            return date.AddDays( -daysToSubtract );
        }

        /// <summary>
        /// Calculates the week number for a specific date, based on a starting date (the beginning of a fiscal year).
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="beginningDate">The start date to compare (i.e., the beginning of the fiscal year).</param>
        /// <param name="firstDayOfWeek">The first day of the week (<see cref="RockDateTime.FirstDayOfWeek"/>).</param>
        /// <returns></returns>
        private static int GetWeekNumberFromDate( DateTime date, DateTime beginningDate, DayOfWeek firstDayOfWeek )
        {
            var weekStartDate = GetFirstDayOfWeek( date, firstDayOfWeek );
            var beginningDateWeekStart = GetFirstDayOfWeek( beginningDate, firstDayOfWeek );

            var daysBetween = weekStartDate - beginningDateWeekStart;

            return ( daysBetween.Days / 7 ) + 1;
        }
    }
}