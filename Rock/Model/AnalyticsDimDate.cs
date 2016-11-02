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
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// see https://www.mssqltips.com/sqlservertip/4054/creating-a-date-dimension-or-calendar-table-in-sql-server/ for some background
    /// </summary>
    /// <seealso cref="Rock.Data.Entity{Rock.Model.AnalyticsDimDate}" />
    [Table( "AnalyticsDimDate" )]
    [DataContract]
    public class AnalyticsDimDate : Rock.Data.Entity<AnalyticsDimDate>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the date key in YYYYMMDD format
        /// </summary>
        /// <value>
        /// The date key.
        /// </value>
        [DataMember]
        public int DateKey { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
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
        /// Gets or sets the day of week.
        /// </summary>
        /// <value>
        /// The day of week.
        /// </value>
        [DataMember]
        public string DayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the day of week abbreviated.
        /// </summary>
        /// <value>
        /// The day of week abbreviated.
        /// </value>
        [DataMember]
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
        /// </summary>
        /// <value>
        /// The giving month.
        /// </value>
        [DataMember]
        public int GivingMonth { get; set; }

        /// <summary>
        /// Gets or sets the calendar week number in year.
        /// </summary>
        /// <value>
        /// The calendar week number in year.
        /// </value>
        [DataMember]
        public int CalendarWeekNumberInYear { get; set; }

        /// <summary>
        /// Gets or sets the name of the calendar in month.
        /// </summary>
        /// <value>
        /// The name of the calendar in month.
        /// </value>
        [DataMember]
        public string CalendarInMonthName { get; set; }

        /// <summary>
        /// Gets or sets the calendar in month name abbrevated.
        /// </summary>
        /// <value>
        /// The calendar in month name abbrevated.
        /// </value>
        [DataMember]
        public string CalendarInMonthNameAbbrevated { get; set; }

        /// <summary>
        /// Gets or sets the calendar month number in year.
        /// </summary>
        /// <value>
        /// The calendar month number in year.
        /// </value>
        [DataMember]
        public int CalendarMonthNumberInYear { get; set; }

        /// <summary>
        /// Gets or sets the calendar year month.
        /// </summary>
        /// <value>
        /// The calendar year month.
        /// </value>
        [DataMember]
        public string CalendarYearMonth { get; set; }

        /// <summary>
        /// Gets or sets the calendar quarter.
        /// </summary>
        /// <value>
        /// The calendar quarter.
        /// </value>
        [DataMember]
        public string CalendarQuarter { get; set; }

        /// <summary>
        /// Gets or sets the calendar year quarter.
        /// </summary>
        /// <value>
        /// The calendar year quarter.
        /// </value>
        [DataMember]
        public string CalendarYearQuarter { get; set; }

        /// <summary>
        /// Gets or sets the calendar year.
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
        public string FiscalMonth { get; set; }

        /// <summary>
        /// Gets or sets the fiscal month abbrevated.
        /// </summary>
        /// <value>
        /// The fiscal month abbrevated.
        /// </value>
        [DataMember]
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
        /// Gets or sets the fiscal month year.
        /// </summary>
        /// <value>
        /// The fiscal month year.
        /// </value>
        [DataMember]
        public int FiscalMonthYear { get; set; }

        /// <summary>
        /// Gets or sets the fiscal quarter.
        /// </summary>
        /// <value>
        /// The fiscal quarter.
        /// </value>
        [DataMember]
        public string FiscalQuarter { get; set; }

        /// <summary>
        /// Gets or sets the fiscal year quarter.
        /// </summary>
        /// <value>
        /// The fiscal year quarter.
        /// </value>
        [DataMember]
        public string FiscalYearQuarter { get; set; }

        /// <summary>
        /// Gets or sets the fiscal half year.
        /// </summary>
        /// <value>
        /// The fiscal half year.
        /// </value>
        [DataMember]
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
    }
}
