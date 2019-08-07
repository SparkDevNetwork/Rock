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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using EntityFramework.Utilities;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// see https://www.mssqltips.com/sqlservertip/4054/creating-a-date-dimension-or-calendar-table-in-sql-server/ for some background
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceDate" )]
    [DataContract]
    [HideFromReporting]
    public class AnalyticsSourceDate
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the date key in YYYYMMDD format
        /// </summary>
        /// <value>
        /// The date key.
        /// </value>
        [DataMember]
        [Key, DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int DateKey { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        [Index( IsUnique = true )]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the full date description.
        /// </summary>
        /// <value>
        /// The full date description.
        /// </value>
        [DataMember]
        public string FullDateDescription { get; set; }

        /// <summary>
        /// Gets or sets the day of week (Sunday=0)
        /// </summary>
        /// <value>
        /// The day of week.
        /// </value>
        [DataMember]
        public int DayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the day of week.
        /// </summary>
        /// <value>
        /// The day of week.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string DayOfWeekName { get; set; }

        /// <summary>
        /// Gets or sets the day of week abbreviated.
        /// </summary>
        /// <value>
        /// The day of week abbreviated.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string DayOfWeekAbbreviated { get; set; }

        /// <summary>
        /// Gets or sets the day number in calendar month.
        /// </summary>
        /// <value>
        /// The day number in calendar month.
        /// </value>
        [DataMember]
        public int DayNumberInCalendarMonth { get; set; }

        /// <summary>
        /// Gets or sets the day number in calendar year.
        /// </summary>
        /// <value>
        /// The day number in calendar year.
        /// </value>
        [DataMember]
        public int DayNumberInCalendarYear { get; set; }

        /// <summary>
        /// Gets or sets the day number in fiscal month.
        /// </summary>
        /// <value>
        /// The day number in fiscal month.
        /// </value>
        [DataMember]
        public int DayNumberInFiscalMonth { get; set; }

        /// <summary>
        /// Gets or sets the day number in fiscal year.
        /// </summary>
        /// <value>
        /// The day number in fiscal year.
        /// </value>
        [DataMember]
        public int DayNumberInFiscalYear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [last day in month indictor].
        /// </summary>
        /// <value>
        /// <c>true</c> if [last day in month indictor]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool LastDayInMonthIndictor { get; set; }

        /// <summary>
        /// Gets or sets the week number in month.
        /// </summary>
        /// <value>
        /// The week number in month.
        /// </value>
        [DataMember]
        public int WeekNumberInMonth { get; set; }

        /// <summary>
        /// Gets or sets the sunday date.
        /// </summary>
        /// <value>
        /// The sunday date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime SundayDate { get; set; }

        /// <summary>
        /// Gets or sets the giving month.
        /// This is based on two options that they choose in the Date Generator UI 1) Fiscal Month and 2) Giving Month: Use Sunday Date
        /// If they choose the "Use Sunday Date" option, it will use whatever the SundayDate of Date is, but not if it crosses the Fiscal Year
        /// For example, if their Fiscal year starts on April 1st, it won't use the SundayDate for any of the last days of March if it ends up being in April
        /// </summary>
        /// <value>
        /// The giving month.
        /// </value>
        [DataMember]
        public int GivingMonth { get; set; }

        

        /// <summary>
        /// Gets or sets the giving month name.
        /// </summary>
        /// <value>
        /// The giving month.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string GivingMonthName { get; set; }

        /// <summary>
        /// Gets or sets the calendar week number.
        /// </summary>
        /// <value>
        /// The calendar week number.
        /// </value>
        [DataMember]
        public int CalendarWeek { get; set; }

        /// <summary>
        /// Gets or sets the calendar month number. Numeric Month (Jan = 1)
        /// </summary>
        /// <value>
        /// The calendar month.
        /// </value>
        [DataMember]
        public int CalendarMonth { get; set; }

        /// <summary>
        /// Gets or sets the name of the calendar in month. Format: "MMMM"
        /// </summary>
        /// <value>
        /// The name of the calendar in month.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string CalendarMonthName { get; set; }

        /// <summary>
        /// Gets or sets the calendar in month name abbreviated. Format: "MMM"
        /// </summary>
        /// <value>
        /// The calendar in month name abbreviated.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string CalendarMonthNameAbbrevated { get; set; }

        /// <summary>
        /// Gets or sets the calendar year month. Format: "yyyyMM" 
        /// </summary>
        /// <value>
        /// The calendar year month.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string CalendarYearMonth { get; set; }

        /// <summary>
        /// Gets or sets the name of the calendar year month. Format: "yyyy MMM", for example "2017 Mar"
        /// </summary>
        /// <value>
        /// The name of the calendar year month.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string CalendarYearMonthName { get; set; }

        /// <summary>
        /// Gets or sets the calendar quarter. Format: "Q{#}", for example "Q2"
        /// </summary>
        /// <value>
        /// The calendar quarter.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string CalendarQuarter { get; set; }

        /// <summary>
        /// Gets or sets the calendar year quarter. Format: "yyyy Q{#}", for example "2017 Q2"
        /// </summary>
        /// <value>
        /// The calendar year quarter.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string CalendarYearQuarter { get; set; }

        /// <summary>
        /// Gets or sets the calendar year. Format: "yyyy"
        /// </summary>
        /// <value>
        /// The calendar year.
        /// </value>
        [DataMember]
        public int CalendarYear { get; set; }

        /// <summary>
        /// Gets or sets the fiscal week.
        /// </summary>
        /// <value>
        /// The fiscal week.
        /// </value>
        [DataMember]
        public int FiscalWeek { get; set; }

        /// <summary>
        /// Gets or sets the fiscal week number in year.
        /// </summary>
        /// <value>
        /// The fiscal week number in year.
        /// </value>
        [DataMember]
        public int FiscalWeekNumberInYear { get; set; }

        /// <summary>
        /// Gets or sets the fiscal month.
        /// </summary>
        /// <value>
        /// The fiscal month.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string FiscalMonth { get; set; }

        /// <summary>
        /// Gets or sets the fiscal month abbreviated.
        /// </summary>
        /// <value>
        /// The fiscal month abbreviated.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string FiscalMonthAbbrevated { get; set; }

        /// <summary>
        /// Gets or sets the fiscal month number in year.
        /// </summary>
        /// <value>
        /// The fiscal month number in year.
        /// </value>
        [DataMember]
        public int FiscalMonthNumberInYear { get; set; }

        /// <summary>
        /// Gets or sets the name of the fiscal month year 
        /// </summary>
        /// <value>
        /// The fiscal month year.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string FiscalMonthYear { get; set; }

        /// <summary>
        /// Gets or sets the fiscal quarter.
        /// </summary>
        /// <value>
        /// The fiscal quarter.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string FiscalQuarter { get; set; }

        /// <summary>
        /// Gets or sets the fiscal year quarter.
        /// </summary>
        /// <value>
        /// The fiscal year quarter.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string FiscalYearQuarter { get; set; }

        /// <summary>
        /// Gets or sets the fiscal half year.
        /// </summary>
        /// <value>
        /// The fiscal half year.
        /// </value>
        [DataMember]
        [MaxLength( 450 )]
        public string FiscalHalfYear { get; set; }

        /// <summary>
        /// Gets or sets the fiscal year.
        /// </summary>
        /// <value>
        /// The fiscal year.
        /// </value>
        [DataMember]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [holiday indicator].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [holiday indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool HolidayIndicator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [week holiday indicator].
        /// </summary>
        /// <value>
        /// <c>true</c> if [week holiday indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool WeekHolidayIndicator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [easter indicator].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [easter indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EasterIndicator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [easter week indicator].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [easter week indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EasterWeekIndicator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [christmas indicator].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [christmas indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ChristmasIndicator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [christmas week indicator].
        /// </summary>
        /// <value>
        /// <c>true</c> if [christmas week indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ChristmasWeekIndicator { get; set; }

        #endregion

        #region Entity Properties specific to Analytics

        /// <summary>
        /// Gets or sets the count.
        /// NOTE: this always has a hardcoded value of 1. It is stored in the table because it is supposed to help do certain types of things in analytics
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; } = 1;

        #endregion

        /// <summary>
        /// Determines the Fiscal Year (the calendar year in which the fiscal year ends) for the specified date and fiscal startmonth
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
        /// Populates the AnalyticsSourceDate table (and associated Views). It will first empty the AnalyticsSourceDate table if there is already data in it.
        /// </summary>
        /// <param name="fiscalStartMonth">The fiscal start month.</param>
        /// <param name="givingMonthUseSundayDate">if set to <c>true</c> [giving month use sunday date].</param>
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
                analyticsSourceDate.CalendarMonthNameAbbrevated = generateDate.ToString( "MMM" );

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
                fiscalWeek = fiscalWeek < 1 ? fiscalWeek + 52 : fiscalWeek;

                analyticsSourceDate.FiscalWeek = fiscalWeek;
                analyticsSourceDate.FiscalWeekNumberInYear = generateDate.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstFourDayWeek, RockDateTime.FirstDayOfWeek );
                analyticsSourceDate.FiscalMonth = generateDate.ToString( "MMMM" );
                analyticsSourceDate.FiscalMonthAbbrevated = generateDate.ToString( "MMM" );
                analyticsSourceDate.FiscalMonthNumberInYear = generateDate.Month;
                analyticsSourceDate.FiscalMonthYear = generateDate.ToString( "MM yyyy" );
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
                EFBatchOperation.For( rockContext, rockContext.AnalyticsSourceDates ).InsertAll( generatedDates );
            }
        }
    }

    /// <summary>
    /// A little helper to get a list of US Holidays http://www.usa.gov/citizens/holidays.shtml  for a specified Year  
    /// C# from http://stackoverflow.com/a/18790381/1755417
    /// </summary>
    public static class HolidayHelper
    {
        /// <summary>
        /// 
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
        /// Gets 
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
