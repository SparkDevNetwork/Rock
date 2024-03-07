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
using EntityFramework.Utilities;
using Rock.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

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
        private static int GetFiscalYear( int fiscalStartMonth, DateTime date )
        {
            int fiscalYearStart = date.AddMonths( -( fiscalStartMonth - 1 ) ).Year;
            int fiscalYear = fiscalStartMonth == 1 ? fiscalYearStart : fiscalYearStart + 1;
            return fiscalYear;
        }

        /// <summary>
        /// Gets the count of weeks in the specified year.
        /// https://stackoverflow.com/a/17391168
        /// </summary>
        /// <param name="year">The year for which to determine the count of weeks.</param>
        /// <param name="calendarWeekRule">The calendar week rule.</param>
        /// <param name="firstDayOfWeek">The first day of the week.</param>
        /// <returns>The count of weeks in the specified year.</returns>
        private static int GetWeeksInYear( int year, System.Globalization.CalendarWeekRule calendarWeekRule, System.DayOfWeek firstDayOfWeek )
        {
            var lastDayOfYear = new DateTime( year, 12, 31 );
            var calendar = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;
            return calendar.GetWeekOfYear( lastDayOfYear, calendarWeekRule, firstDayOfWeek );
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

                // figure out the fiscalMonthNumber and QTR.  For example, if the Fiscal Start Month is April, and Today is April 1st, the Fiscal Month Number would be 1
                int fiscalMonthNumber = new System.DateTime( generateDate.Year, generateDate.Month, 1 ).AddMonths( 1 - fiscalStartMonth ).Month;
                int fiscalQuarter = ( fiscalMonthNumber + 2 ) / 3;

                int fiscalYear = GetFiscalYear( fiscalStartMonth, generateDate );

                DateTime fiscalStartDate = new DateTime( generateDate.Year, fiscalStartMonth, 1 );

                // see http://www.filemaker.com/help/12/fmp/en/html/func_ref1.31.28.html and do it that way, except using RockDateTime.FirstDayOfWeek
                int fiscalWeekOffset = fiscalStartDate.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstFourDayWeek, RockDateTime.FirstDayOfWeek ) - 1;
                int fiscalWeek = generateDate.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstFourDayWeek, RockDateTime.FirstDayOfWeek ) - fiscalWeekOffset;
                int weeksInYear = GetWeeksInYear( fiscalYear, System.Globalization.CalendarWeekRule.FirstFourDayWeek, RockDateTime.FirstDayOfWeek );
                fiscalWeek = fiscalWeek < 1 ? fiscalWeek + weeksInYear : fiscalWeek;

                analyticsSourceDate.FiscalWeek = fiscalWeek;
                analyticsSourceDate.FiscalWeekNumberInYear = generateDate.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstFourDayWeek, RockDateTime.FirstDayOfWeek );
                analyticsSourceDate.FiscalMonth = generateDate.ToString( "MMMM" );
                analyticsSourceDate.FiscalMonthAbbreviated = generateDate.ToString( "MMM" );
                analyticsSourceDate.FiscalMonthNumberInYear = generateDate.Month;
                analyticsSourceDate.FiscalMonthYear = $"{fiscalMonthNumber:00} {fiscalYear}";
                analyticsSourceDate.FiscalQuarter = string.Format( "Q{0}", fiscalQuarter );
                analyticsSourceDate.FiscalYearQuarter = string.Format( "{0} Q{1}", fiscalYear, fiscalQuarter );
                analyticsSourceDate.FiscalHalfYear = fiscalQuarter < 3 ? "First" : "Second";
                analyticsSourceDate.FiscalYear = fiscalYear;

                // DayNumberInFiscalMonth is simply the same as DayNumberInCalendarMonth since we only let them pick a FiscalStartMonth ( not a Month and Day )
                analyticsSourceDate.DayNumberInFiscalMonth = analyticsSourceDate.DayNumberInCalendarMonth;

                if ( fiscalStartMonth == 1 )
                {
                    analyticsSourceDate.DayNumberInFiscalYear = analyticsSourceDate.DayNumberInCalendarYear;
                }
                else
                {
                    if ( fiscalStartDate <= generateDate )
                    {
                        // fiscal start is the same year as the generated date year
                        analyticsSourceDate.DayNumberInFiscalYear = ( generateDate - fiscalStartDate ).Days + 1;
                    }
                    else
                    {
                        // fiscal start is the year previous to the generated date year
                        analyticsSourceDate.DayNumberInFiscalYear = ( generateDate - fiscalStartDate.AddYears( -1 ) ).Days + 1;
                    }
                }

                // NOTE: This is complicated, see comments on AnalyticsSourceDate.GivingMonth
                if ( givingMonthUseSundayDate )
                {
                    var givingMonthSundayDate = generateDate.SundayDate();
                    int sundayFiscalYear = GetFiscalYear( fiscalStartMonth, givingMonthSundayDate );

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
    }
}
