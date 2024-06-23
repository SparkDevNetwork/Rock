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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Enums.Crm;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsSourceDate table.
    /// See https://www.mssqltips.com/sqlservertip/4054/creating-a-date-dimension-or-calendar-table-in-sql-server/ for some background
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceDate" )]
    [DataContract]
    [HideFromReporting]
    [CodeGenExclude( CodeGenFeature.ViewModelFile )]
    [IncludeForModelMap]
    public partial class AnalyticsSourceDate
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
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
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
        public string CalendarMonthNameAbbreviated { get; set; }

        /// <summary>
        /// Gets or sets the calendar in month name abbreviated. Format: "MMM"
        /// </summary>
        /// <value>
        /// The calendar in month name abbreviated.
        /// </value>
        [RockObsolete("1.13")]
        [Obsolete("Use CalendarMonthNameAbbreviated instead", true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
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
        public string FiscalMonthAbbreviated { get; set; }

        /// <summary>
        /// Gets or sets the fiscal month abbreviated.
        /// </summary>
        /// <value>
        /// The fiscal month abbreviated.
        /// </value>
        [RockObsolete("1.13")]
        [Obsolete("Use FiscalMonthAbbreviated instead", true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
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

        /// <summary>
        /// Gets or sets the week of year.
        /// </summary>
        /// <value>The week of year.</value>
        [DataMember]
        public int WeekOfYear { get; set; }

        /// <summary>
        /// Gets or sets the week counter.
        /// </summary>
        /// <value>The week counter.</value>
        [DataMember]
        public int WeekCounter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the containing year is a leap year.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the year is a leap year; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool LeapYearIndicator { get; set; }

        /// <summary>
        /// Gets or sets the sunday date year.
        /// </summary>
        /// <value>The sunday date year.</value>
        [DataMember]
        public int SundayDateYear { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        [DataMember]
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the age bracket.
        /// </summary>
        /// <value>
        /// The age bracket.
        /// </value>
        [DataMember]
        public AgeBracket? AgeBracket { get; set; }

        #endregion Entity Properties

        #region Entity Properties Specific to Analytics

        /// <summary>
        /// Gets or sets the count.
        /// NOTE:  This always has a (hard-coded) value of 1. It is stored in the table to assist with analytics calculations.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; } = 1;

        #endregion Entity Properties Specific to Analytics
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsSourceDate Configuration Class
    /// </summary>
    public partial class AnalyticsSourceDateConfiguration : EntityTypeConfiguration<AnalyticsSourceDate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSourceDateConfiguration"/> class.
        /// </summary>
        public AnalyticsSourceDateConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
