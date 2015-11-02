// <copyright>
// Copyright 2015 by NewSpring Church
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.Metrics
{
    /// <summary>
    /// All Church Metrics Block
    /// </summary>
    [DisplayName( "Ministry Metrics" )]
    [Category( "NewSpring" )]
    [Description( "Custom church metrics block using the Chart.js library" )]
    [CustomDropdownListField( "Number of Columns", "", "1,2,3,4,5,6,7,8,9,10,11,12", false, DefaultValue = "4", Order = 1 )]
    [CustomDropdownListField( "Metric Display Type", "", "Text,Line,Donut", false, "Text", Order = 2 )]
    [TextField( "Primary Metric Key", "Enter the metric title to pull display values from.", false, Order = 3 )]
    [MetricCategoriesField( "Primary Metric Source", "Select the primary metric(s) to include in this chart.", false, Order = 4 )]
    [TextField( "Comparison Metric Key", "Enter the metric title to calculate against the Primary Source/Key.", false, Order = 5 )]
    [MetricCategoriesField( "Comparison Metric Source", "Select the metric(s) to calculate against the Primary Source/Key.", false, Order = 6 )]
    [CustomRadioListField( "Display Comparison As", "Choose to display the comparison result as an integer or percentage", "Integer,Percentage", true, "Integer", order: 7 )]
    [CustomRadioListField( "Context Scope", "The scope of context to set", "None,Page", true, "Page", order: 8 )]

    //[SlidingDateRangeField( "Date Range", Key = "SlidingDateRange", Order = 9 )]
    //[CustomRadioListField( "Custom Dates", "If not using date range, please select a custom date from here", "This Week Last Year", Order = 9 )]
    //[CustomCheckboxListField( "Compare Against Last Year", "", "Yes", Order = 10 )]
    public partial class MinistryMetrics : RockBlock
    {
        #region Fields

        /// <summary>
        /// Gets or sets the metric block values.
        /// </summary>
        /// <value>
        /// The metric block values.
        /// </value>
        protected string MetricBlockValues
        {
            get
            {
                var viewStateValues = ViewState["MetricBlockValues"] as string;
                return viewStateValues ?? string.Empty;
            }
            set
            {
                ViewState["MetricBlockValues"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the metric should compare to last year.
        /// </summary>
        /// <value>
        /// <c>true</c> if the metric should compare to last year; otherwise, <c>false</c>.
        /// </value>
        //protected string MetricCompareLastYear
        //{
        //    get
        //    {
        //        var viewStateValue = ViewState[string.Format( "MetricCompareLastYear_{0}", BlockId )] as string;
        //        return viewStateValue ?? string.Empty;
        //    }
        //    set
        //    {
        //        ViewState[string.Format( "MetricCompareLastYear_{0}", BlockId )] = value;
        //    }
        //}

        // Let's create null context values so they are available
        protected IEntity CampusContext = null;

        protected IEntity ScheduleContext = null;

        protected IEntity GroupContext = null;

        protected IEntity GroupTypeContext = null;

        protected IEntity DateContext = null;

        protected string PrimaryMetricKey = string.Empty;

        protected string ComparisonMetricKey = string.Empty;

        #endregion

        #region Control Methods

        // <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // check the page context
                bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";

                // If the blocks respect page context let's set those vars
                if ( pageScope )
                {
                    // Get Current Campus Context
                    CampusContext = RockPage.GetCurrentContext( EntityTypeCache.Read( typeof( Campus ) ) );

                    // Get Current Schedule Context
                    ScheduleContext = RockPage.GetCurrentContext( EntityTypeCache.Read( typeof( Schedule ) ) );

                    // Get Current Group Context
                    GroupContext = RockPage.GetCurrentContext( EntityTypeCache.Read( typeof( Group ) ) );

                    // Get Current GroupType Context
                    GroupTypeContext = RockPage.GetCurrentContext( EntityTypeCache.Read( typeof( GroupType ) ) );
                }

                // Output variables direct to the ascx
                metricBlockNumber.Value = BlockId.ToString();
                metricBlockId.Value = BlockName.Replace( " ", "" ).ToString();
                metricTitle.Value = BlockName;
                metricDisplay.Value = GetAttributeValue( "MetricDisplayType" );
                metricWidth.Value = GetAttributeValue( "NumberofColumns" );

                PrimaryMetricKey = GetAttributeValue( "PrimaryMetricKey" );
                ComparisonMetricKey = GetAttributeValue( "ComparisonMetricKey" );

                var churchMetricPeriod = GetAttributeValue( "MetricPeriod" );
                var metricComparison = GetAttributeValue( "MetricComparison" );
                var metricDisplayType = GetAttributeValue( "MetricDisplayType" );

                var rockContext = new RockContext();
                var metricService = new MetricService( rockContext );

                var primarySourceGuids = GetAttributeValue( "PrimaryMetricSource" )
                    .SplitDelimitedValues()
                    .AsGuidList();

                var comparisonSourceGuids = GetAttributeValue( "ComparisonMetricSource" )
                    .SplitDelimitedValues()
                    .AsGuidList();

                // lookup the metric sources
                List<int> primaryMetricSource = metricService.GetByGuids( primarySourceGuids )
                    .Select( a => a.Id ).ToList();

                List<int> comparisonMetricSource = metricService.GetByGuids( comparisonSourceGuids )
                    .Select( a => a.Id ).ToList();

                DateRange dateRange = new DateRange( DateTime.Now.AddMonths( -6 ), DateTime.Now );

                // Show data if metric source is selected
                if ( primaryMetricSource.Any() || !string.IsNullOrEmpty( PrimaryMetricKey ) )
                {
                    if ( metricDisplayType.Equals( "Text" ) )
                    {
                        DisplayTextValue( dateRange, primaryMetricSource, comparisonMetricSource );
                    }
                    else if ( metricDisplayType.Equals( "Line" ) )
                    {
                        DisplayLineValue( dateRange, primaryMetricSource );
                    }
                    else if ( metricDisplayType.Equals( "Donut" ) )
                    {
                        DisplayDonutValue( dateRange, primaryMetricSource );
                    }
                }
                else
                {
                    // nothing selected, display an error message
                    churchMetricWarning.Visible = true;
                }
            }

            // unused variables
            // var metricCustomDates = GetAttributeValue( "CustomDates" );
            // MetricCompareLastYear = GetAttributeValue( "CompareAgainstLastYear" ).ToString();
            // var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? string.Empty );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Formats the values.
        /// </summary>
        /// <param name="primaryMetricSource">The primary metric source.</param>
        /// <param name="percentageMetricSource">The percentage metric source.</param>
        /// <param name="dateRange">The date range.</param>
        /// <returns></returns>
        protected decimal FormatValues( List<int> primaryMetricSource, List<int> comparisonMetricSource, DateRange dateRange )
        {
            var primaryMetricValues = GetMetricValues( primaryMetricSource, dateRange, PrimaryMetricKey );
            var primaryValueSum = primaryMetricValues.Select( a => a.YValue ).Sum() ?? 0.0M;

            // if comparing values, make sure we have a valid percentage source
            if ( primaryValueSum > 0 && ( comparisonMetricSource.Any() || !string.IsNullOrEmpty( ComparisonMetricKey ) ) )
            {
                var comparisonMetricValues = GetMetricValues( comparisonMetricSource, dateRange, ComparisonMetricKey );
                var comparisonValueSum = comparisonMetricValues.Select( a => a.YValue ).Sum() ?? 0.0M;

                if ( comparisonValueSum > 0 )
                {
                    decimal comparison = primaryValueSum / comparisonValueSum;

                    if ( GetAttributeValue( "DisplayComparisonAs" ).Equals( "Integer" ) )
                    {
                        return comparison;
                    }
                    else
                    {
                        return comparison * 100;
                    }
                }
                else
                {
                    return 0.0M;
                }
            }

            return primaryValueSum;
        }

        /// <summary>
        /// Builds the metric query.
        /// </summary>
        /// <param name="metricSource">The metric source.</param>
        /// <param name="dateRange">The date range.</param>
        /// <returns></returns>
        protected List<MetricValue> GetMetricValues( List<int> metricSource, DateRange dateRange, string metricKey )
        {
            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );
            var metricQueryable = new MetricService( rockContext ).Queryable();

            if ( !string.IsNullOrEmpty( metricKey ) )
            {
                metricQueryable = metricService.Queryable().Where( a => a.Title.EndsWith( metricKey ) );
            }
            else
            {
                metricQueryable = metricService.GetByIds( metricSource );
            }

            var metricValueQueryable = metricQueryable.SelectMany( a => a.MetricValues ).AsQueryable().AsNoTracking();

            // filter by date context
            if ( dateRange != null )
            {
                metricValueQueryable = metricValueQueryable.Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End );
            }

            // filter by campus context
            if ( CampusContext != null )
            {
                metricValueQueryable = metricValueQueryable.Where( a => a.EntityId == CampusContext.Id );
            }

            // filter by schedule context
            if ( ScheduleContext != null )
            {
                var scheduleTime = new ScheduleService( rockContext ).Get( ScheduleContext.Guid ).StartTimeOfDay;
                metricValueQueryable = metricValueQueryable.Where( a => scheduleTime == DbFunctions.CreateTime(
                        a.MetricValueDateTime.Value.Hour,
                        a.MetricValueDateTime.Value.Minute,
                        a.MetricValueDateTime.Value.Second
                    )
                );
            }

            // filter by group context
            if ( GroupContext != null )
            {
                metricValueQueryable = metricValueQueryable.Where( a => a.ForeignId == GroupContext.Id );
            }

            return metricValueQueryable.ToList();
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Displays the text value.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="primaryMetricSource">The primary metric source.</param>
        /// <param name="comparisonMetricSource">The comparison metric source.</param>
        private void DisplayTextValue( DateRange dateRange, List<int> primaryMetricSource, List<int> comparisonMetricSource )
        {
            // this may be a little complicated to compare date ranges while accepting two metric keys/sources
            decimal currentMetricValues = FormatValues( primaryMetricSource, comparisonMetricSource, dateRange );
            decimal comparisonMetricValues = 0;

            // if doing a date comparison
            //DateTime dateRangeStart = dateRange.Start ?? DateTime.Now;
            //DateTime dateRangeEnd = dateRange.End ?? DateTime.Now;
            //TimeSpan ts = dateRangeEnd - dateRangeStart;

            //if ( ts.Days > 0 )
            //{
            //    var differenceInDays = ts.Days + 1;

            //    var comparisonDateRange = new DateRange
            //    {
            //        Start = dateRange.Start.Value.AddDays( -differenceInDays ),
            //        End = dateRange.End.Value.AddDays( -differenceInDays )
            //    };

            //    comparisonMetricValues = FormatValues( primaryMetricSource, percentageMetricSource, comparisonDateRange );
            //}

            if ( currentMetricValues > 0 )
            {
                currentMetricValue.Value = string.Format( "{0:n0}", currentMetricValues );

                if ( comparisonMetricValues > 0 )
                {
                    if ( currentMetricValues > comparisonMetricValues )
                    {
                        metricClass.Value = "fa-caret-up brand-success";
                    }
                    else if ( currentMetricValues < comparisonMetricValues )
                    {
                        metricClass.Value = "fa-caret-down brand-danger";
                    }
                }

                if ( ( comparisonMetricSource.Any() || !string.IsNullOrEmpty( ComparisonMetricKey ) ) && GetAttributeValue( "DisplayComparisonAs" ).Equals( "Percentage" ) )
                {
                    metricComparisonDisplay.Value = "%";
                }
            }
            else
            {
                currentMetricValue.Value = "—";
            }

            //if ( MetricCompareLastYear == "Yes" )
            //{
            //    var comparePreviousYearMetricValue = new DateRange
            //    {
            //        Start = dateRange.Start.Value.AddYears( -1 ),
            //        End = dateRange.End.Value.AddYears( -1 )
            //    };

            //    decimal? previousYearRangeMetricValue = MetricValueFunction( primaryMetricSource, comparePreviousYearMetricValue, campusContext, groupContext, scheduleContext );

            //    previousMetricValue.Value = string.Format( "{0:n0}", previousYearRangeMetricValue );
            //}

            // This Week Last Year
            //else if ( metricCustomDates == "This Week Last Year" )
            //{
            //currentMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
            //.Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == calendar.GetWeekOfYear( DateTime.Now.AddYears( -1 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) && a.MetricValueDateTime.Value.Year.ToString() == DateTime.Now.AddYears( -1 ).ToString() )
            //.Select( a => a.YValue )
            //.Sum()
            //);
            //}
        }

        /// <summary>
        /// Displays the line value.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="primaryMetricSource">The primary metric source.</param>
        private void DisplayLineValue( DateRange dateRange, List<int> primaryMetricSource )
        {
            var metricLegend = new List<string>();
            var metricCurrentYearValues = new List<string>();

            // if compare to previous year was set, also grab values from the previous year

            //var previousYearValues = GetMetricValues( primaryMetricSource, dateRange, PrimaryMetricKey );

            //if ( GetAttributeValue( "CompareAgainstLastYear" ) == "Yes" )
            //{
            //    metricPreviousYear = newMetric.MetricValues
            //        .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddYears( -1 ) && a.MetricValueDateTime <= dateRange.End.Value.AddYears( -1 ) && a.EntityId.ToString() == CampusContext.Id.ToString() )
            //        .OrderBy( a => a.MetricValueDateTime )
            //        .Select( a => new MetricJson
            //        {
            //            date = a.MetricValueDateTime.Value.Date,
            //            week = calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ),
            //            year = a.MetricValueDateTime.Value.Year,
            //            value = string.Format( "{0:0}", a.YValue )
            //        } )
            //        .ToList();
            //}

            var currentDateValues = GetMetricValues( primaryMetricSource, dateRange, PrimaryMetricKey );

            foreach ( var currentValue in currentDateValues )
            {
                var metricDate = currentValue.MetricValueDateTime.Value.Date;
                var currentMetricValue = string.Format( "{0:0}", currentValue.YValue );

                var currentMetricLabel = new DateTime( metricDate.Year, metricDate.Month, metricDate.Day )
                    .ToString( "MMMM dd" );

                // format these with ticks so the JS can parse it as an array
                // possibly pass this as a ToArray call and see what happens?
                currentMetricLabel = string.Format( "'{0}'", currentMetricLabel );
                currentMetricValue = string.Format( "'{0}'", currentMetricValue );

                metricLegend.Add( currentMetricLabel );
                metricCurrentYearValues.Add( currentMetricValue );

                // if compare to previous year
                // var metricPreviousYear = new List<MetricJson>();
                //if ( metricPreviousYear.Count > 0 )
                //{
                //    var count = 0;

                //    foreach ( var previousMetric in metricPreviousYear )
                //    {
                //        var previousMetricCount = count++;
                //        if ( currentMetric.week == previousMetric.week )
                //        {
                //            metricPreviousYearValues.Add( previousMetric.value );
                //            break;
                //        }
                //        else if ( count == metricPreviousYear.Count )
                //        {
                //            metricPreviousYearValues.Add( "0" );
                //            break;
                //        }
                //    }
                //}
                //else
                //{
                //    metricPreviousYearValues.Add( "0" );
                //}
            }

            metricLabels.Value = string.Format( "'{0}'", metricLegend.ToString() );

            metricDataPointsCurrent.Value = string.Format( "'{0}'", metricCurrentYearValues.ToString() );
        }

        /// <summary>
        /// Displays the metric as a donut value.
        /// </summary>
        /// <param name="pageContext">The page context.</param>
        /// <param name="dateRange">The date range.</param>
        /// <param name="primaryMetricSource">The primary metric source.</param>
        private void DisplayDonutValue( DateRange dateRange, List<int> primaryMetricSource )
        {
            var currentWeekValues = GetMetricValues( primaryMetricSource, dateRange, PrimaryMetricKey );

            // does a donut compare to the previous week?
            //var previousWeekValues = GetMetricValues( primaryMetricSource, dateRange - 7 , PrimaryMetricKey );

            var blockValues = new List<MetricValueItem>();

            int currentItemId = 0;
            foreach ( var currentValue in currentWeekValues )
            {
                // color each metric by offset
                string metricItemColor = "#6bac43";

                if ( currentItemId % 2 != 0 )
                {
                    metricItemColor = "#1c683e";
                }
                else if ( currentItemId % 3 == 0 )
                {
                    metricItemColor = "#2a4930";
                }

                // Create JSON array of data
                if ( currentValue != null )
                {
                    blockValues.Add( new MetricValueItem()
                    {
                        value = (int)currentValue.YValue,
                        color = metricItemColor,
                        highlight = metricItemColor,
                        label = currentValue.Metric.Title
                    } );
                }
                else
                {
                    // i don't think this ever gets hit
                    blockValues.Add( new MetricValueItem()
                    {
                        value = 0,
                        color = metricItemColor,
                        highlight = metricItemColor,
                        label = currentValue.Metric.Title
                    } );
                }

                currentItemId++;
            }

            MetricBlockValues = JsonConvert.SerializeObject( blockValues.ToArray() );

            //unused variables
            //var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;

            //var donutMetrics = new MetricService( new RockContext() ).GetByIds( primaryMetricSource );

            //// Current Week of Year
            //var currentWeekOfYear = calendar.GetWeekOfYear( DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Sunday );

            //// Last Week
            //var lastWeekOfYear = calendar.GetWeekOfYear( DateTime.Now.AddDays( -7 ), CalendarWeekRule.FirstDay, DayOfWeek.Sunday );
        }

        #endregion

        #region Classes

        /// <summary>
        /// Metric information used to bind the selected grid.
        /// </summary>
        [Serializable]
        protected class MetricValueItem
        {
            public int value { get; set; }

            public string color { get; set; }

            public string highlight { get; set; }

            public string label { get; set; }
        }

        /// <summary>
        /// Metric information as a JSON object
        /// </summary>
        [Serializable]
        protected class MetricJson
        {
            public System.DateTime date { get; set; }

            public string value { get; set; }

            public int week { get; set; }

            public int year { get; set; }
        }

        #endregion Classes
    }
}