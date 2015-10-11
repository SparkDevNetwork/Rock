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
    [CustomDropdownListField( "Number of Columns", "", "1,2,3,4,5,6,7,8,9,10,11,12", false, DefaultValue = "12", Order = 1 )]
    [CustomDropdownListField( "Metric Display Type", "",  "Text,Line,Donut", false, "Text", Order = 2 )]
    [TextField( "Metric Key", "If this is used, do not select a metric source", Order = 3 )]
    [MetricCategoriesField( "Primary Metric Source", "Select the metric to include in this chart.", Order = 4 )]
    [CustomRadioListField( "Metric Comparison", "Is this metric a sum of the selected sources, or a percentage?", "Sum,Percentage", false, "Sum", Order = 5 )]
    [MetricCategoriesField( "Secondary Metric Source", "This should only be used if you are creating a Percentage comparison.", Order = 6 )]
    [CustomCheckboxListField( "Respect Page Context", "", "Yes", Order = 7 )]
    [SlidingDateRangeField( "Date Range", Key = "SlidingDateRange", Order = 8 )]
    [CustomRadioListField( "Custom Dates", "If not using date range, please select a custom date from here", "This Week Last Year", Order = 9 )]
    [CustomCheckboxListField( "Compare Against Last Year", "", "Yes", Order = 10 )]
    

    public partial class MinistryMetrics : Rock.Web.UI.RockBlock
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
        protected string MetricCompareLastYear
        {
            get
            {
                var viewStateValue = ViewState[string.Format( "MetricCompareLastYear_{0}", BlockId )] as string;
                return viewStateValue ?? string.Empty;
            }
            set
            {
                ViewState[string.Format( "MetricCompareLastYear_{0}", BlockId )] = value;
            }
        }

        #endregion

        // Let's create null context values so they are available
        IEntity campusContext = new List<IEntity>() as IEntity;
        IEntity scheduleContext = new List<IEntity>() as IEntity;
        IEntity groupContext = new List<IEntity>() as IEntity;

        string metricKey = string.Empty;

        private int TimeSpanDifference( Rock.DateRange dateRange )
        {
            DateTime dateRangeStart = dateRange.Start ?? DateTime.Now;
            DateTime dateRangeEnd = dateRange.End ?? DateTime.Now;

            TimeSpan ts = dateRangeEnd - dateRangeStart;

            return ts.Days + 1;
        }

        private decimal MetricQuery( 
                List<int> metricSource,
                Rock.DateRange dateRange
            )
        {

            var metricData = new MetricService( new RockContext() ).Queryable();

            if ( metricKey != "" )
            {
                var metricKeyData = new MetricService( new RockContext() ).Queryable();
                var metricKeyIds = metricKeyData.Where( a => a.Title.Contains( metricKey ) ).Select( a => a.Id ).ToList() as List<int>;

                metricData = new MetricService( new RockContext() ).GetByIds( metricKeyIds );

            } else {
                metricData = new MetricService( new RockContext() ).GetByIds( metricSource );
            }

            var queryable = metricData.SelectMany( a => a.MetricValues ).AsQueryable().AsNoTracking();

            if ( dateRange != null )
            {
                queryable = queryable.Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End );
            }

            if ( campusContext != null )
            {
                queryable = queryable.Where( a => a.EntityId == campusContext.Id );
            }

            if ( scheduleContext != null )
            {
                var scheduleTime = new ScheduleService( new RockContext() ).Get( scheduleContext.Guid ).StartTimeOfDay;
                queryable = queryable.Where( a => DbFunctions.CreateTime( a.MetricValueDateTime.Value.Hour, a.MetricValueDateTime.Value.Minute, a.MetricValueDateTime.Value.Second ) == scheduleTime );
            }

            if ( groupContext != null )
            {
                queryable = queryable.Where( a => a.ForeignId == groupContext.Id );
            }

            var executeQuery = queryable.ToList();

            return executeQuery.Select( a => a.YValue ).Sum() ?? 0;
        }

        private decimal
            MetricValueFunction( 
                List<int> primaryMetricSource,
                List<int> secondaryMetricSource,
                Rock.DateRange dateRange,
                string numberFormat="sum"
            )
        {

            if ( numberFormat == "Percentage")
            {
                decimal primaryMetricValue = MetricQuery( primaryMetricSource, dateRange );
                decimal secondaryMetricValue = MetricQuery( secondaryMetricSource, dateRange );

                if ( primaryMetricValue != 0 && secondaryMetricValue != 0 )
                {
                    decimal percentage = primaryMetricValue / secondaryMetricValue;
                    return percentage * 100;
                }
                else
                {
                    return 0.0M;
                }
            }
            // This is the default function, which is a sum of all the values
            else
            {
                return MetricQuery( primaryMetricSource, dateRange);
            }

        }

        #region Control Methods

        // <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Should The Blocks Respect Page Context?
            var pageContext = GetAttributeValue( "RespectPageContext" ).AsBooleanOrNull();

            // If the blocks respect page context let's set those vars
            if ( pageContext.HasValue )
            {
                // Get Current Campus Context
                campusContext = RockPage.GetCurrentContext( EntityTypeCache.Read( "Rock.Model.Campus" ) );

                // Get Current Schedule Context
                scheduleContext = RockPage.GetCurrentContext( EntityTypeCache.Read( "Rock.Model.Schedule" ) );

                // Get Current Group Context
                groupContext = RockPage.GetCurrentContext( EntityTypeCache.Read( "Rock.Model.Group" ) );
            }

            // Let's Set Some Globally Used Vars

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? string.Empty );

            // Output variables direct to the ascx
            metricBlockNumber.Value = BlockId.ToString();
            metricBlockId.Value = BlockName.Replace( " ", "" ).ToString();
            metricTitle.Value = BlockName;
            metricDisplay.Value = GetAttributeValue( "MetricDisplayType" );
            metricWidth.Value = GetAttributeValue( "NumberofColumns" );

            var metricComparison = GetAttributeValue( "MetricComparison" );

            var metricCustomDates = GetAttributeValue( "CustomDates" );

            metricKey = GetAttributeValue( "MetricKey" );

            List<int> primaryMetricSource = GetMetricIds( "PrimaryMetricSource" );
            List<int> secondaryMetricSource = GetMetricIds( "SecondaryMetricSource" );
            var churchMetricPeriod = GetAttributeValue( "MetricPeriod" );

            MetricCompareLastYear = GetAttributeValue( "CompareAgainstLastYear" ).ToString();

            var newMetric = new MetricService( new RockContext() ).GetByIds( primaryMetricSource ).FirstOrDefault();

            // Show the warning if metric source or a metric key is not selected
            if ( primaryMetricSource.Any() )
            {
                churchMetricWarning.Visible = false;
            } else if ( metricKey.ToString() != "" ) {
                churchMetricWarning.Visible = false;
            }

            // This sets the var to do a Week of Year calculation
            var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;

            // Show data if metric source is selected
            if ( newMetric != null || metricKey != "" )
            {
                if ( GetAttributeValue( "MetricDisplayType" ) == "Text" )
                {
                    // This is using the date range picker
                    if ( dateRange.Start.HasValue && dateRange.End.HasValue )
                    {

                        var differenceInDays = TimeSpanDifference( dateRange );

                        var compareMetricValue = new DateRange
                        {
                            Start = dateRange.Start.Value.AddDays( -differenceInDays ),
                            End = dateRange.End.Value.AddDays( -differenceInDays )
                        };

                        metricKey = GetAttributeValue( "MetricKey" );

                        decimal? currentRangeMetricValue = MetricValueFunction( primaryMetricSource, secondaryMetricSource, dateRange, metricComparison );

                        decimal? previousRangeMetricValue = MetricValueFunction( primaryMetricSource, secondaryMetricSource, compareMetricValue, metricComparison );


                        if ( currentRangeMetricValue == 0 && metricComparison == "Percentage" )
                        {
                            currentMetricValue.Value = "-";
                        }
                        else
                        {
                            currentMetricValue.Value = string.Format( "{0:n0}", currentRangeMetricValue );

                            if ( metricComparison == "Percentage" )
                            {
                                metricComparisonDisplay.Value = "%";
                            }
                        }

                        // Check to make sure that current and previous have a value to compare
                        if ( currentRangeMetricValue.ToString() != "0.0" && previousRangeMetricValue.ToString() != "0.0" )
                        {
                            if ( currentRangeMetricValue > previousRangeMetricValue )
                            {
                                metricClass.Value = "fa-caret-up brand-success";
                            }
                            else if ( currentRangeMetricValue < previousRangeMetricValue )
                            {
                                metricClass.Value = "fa-caret-down brand-danger";
                            }
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
                    }

                    // This Week Last Year
                    else if ( metricCustomDates == "This Week Last Year" )
                    {
                        //currentMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
                        //.Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == calendar.GetWeekOfYear( DateTime.Now.AddYears( -1 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) && a.MetricValueDateTime.Value.Year.ToString() == DateTime.Now.AddYears( -1 ).ToString() )
                        //.Select( a => a.YValue )
                        //.Sum()
                        //);

                    }
                }
                else if ( GetAttributeValue( "MetricDisplayType" ) == "Line" && newMetric != null )
                {
                    var metricLabelsList = new List<string>();
                    var metricCurrentYearValues = new List<string>();
                    var metricPreviousYearValues = new List<string>();

                    // Create empty lists for the search to be performed next
                    var metricCurrentYear = new List<MetricJson>();
                    var metricPreviousYear = new List<MetricJson>();

                    // Search for data if a source is selected
                    if ( dateRange.Start.HasValue && dateRange.End.HasValue )
                    {
                        if ( campusContext != null && pageContext.HasValue )
                        {
                            metricCurrentYear = newMetric.MetricValues
                                .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End && a.EntityId.ToString() == campusContext.Id.ToString() )
                                .OrderBy( a => a.MetricValueDateTime )
                                .Select( a => new MetricJson
                                {
                                    date = a.MetricValueDateTime.Value.Date,
                                    week = calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ),
                                    year = a.MetricValueDateTime.Value.Year,
                                    value = string.Format( "{0:0}", a.YValue )
                                } )
                                .ToList();

                            if ( GetAttributeValue( "CompareAgainstLastYear" ) == "Yes" )
                            {
                                metricPreviousYear = newMetric.MetricValues
                                    .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddYears( -1 ) && a.MetricValueDateTime <= dateRange.End.Value.AddYears( -1 ) && a.EntityId.ToString() == campusContext.Id.ToString() )
                                    .OrderBy( a => a.MetricValueDateTime )
                                    .Select( a => new MetricJson
                                    {
                                        date = a.MetricValueDateTime.Value.Date,
                                        week = calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ),
                                        year = a.MetricValueDateTime.Value.Year,
                                        value = string.Format( "{0:0}", a.YValue )
                                    } )
                                    .ToList();
                            }
                        }
                        else
                        {
                            metricCurrentYear = newMetric.MetricValues
                                .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End )
                                .OrderBy( a => a.MetricValueDateTime )
                                .Select( a => new MetricJson
                                {
                                    date = a.MetricValueDateTime.Value.Date,
                                    week = calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ),
                                    year = a.MetricValueDateTime.Value.Year,
                                    value = string.Format( "{0:0}", a.YValue )
                                } )
                                .ToList();

                            if ( GetAttributeValue( "CompareAgainstLastYear" ) == "Yes" )
                            {
                                metricPreviousYear = newMetric.MetricValues
                                    .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddYears( -1 ) && a.MetricValueDateTime <= dateRange.End.Value.AddYears( -1 ) )
                                    .OrderBy( a => a.MetricValueDateTime )
                                    .Select( a => new MetricJson
                                    {
                                        date = a.MetricValueDateTime.Value.Date,
                                        week = calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ),
                                        year = a.MetricValueDateTime.Value.Year,
                                        value = string.Format( "{0:0}", a.YValue )
                                    } )
                                    .ToList();
                            }
                        }
                    }

                    foreach ( var currentMetric in metricCurrentYear )
                    {
                        metricLabelsList.Add( new DateTime( currentMetric.date.Year, currentMetric.date.Month, currentMetric.date.Day ).ToString( "MMMM dd" ) );
                        metricCurrentYearValues.Add( currentMetric.value );

                        if ( metricPreviousYear.Count != 0 )
                        {
                            var count = 0;

                            foreach ( var previousMetric in metricPreviousYear )
                            {
                                var previousMetricCount = count++;
                                if ( currentMetric.week == previousMetric.week )
                                {
                                    metricPreviousYearValues.Add( previousMetric.value );
                                    break;
                                }
                                else if ( count == metricPreviousYear.Count )
                                {
                                    metricPreviousYearValues.Add( "0" );
                                    break;
                                }
                            }
                        }
                        else
                        {
                            metricPreviousYearValues.Add( "0" );
                        }
                    }

                    metricLabels.Value = "'" + metricLabelsList.AsDelimited( "," ).Replace( ",", "','" ) + "'";

                    metricDataPointsCurrent.Value = "'" + metricCurrentYearValues.AsDelimited( "," ).Replace( ",", "','" ) + "'";

                    metricDataPointsPrevious.Value = "'" + metricPreviousYearValues.AsDelimited( "," ).Replace( ",", "','" ) + "'";
                }
                else if ( GetAttributeValue( "MetricDisplayType" ) == "Donut" && newMetric != null )
                {
                    var donutMetrics = new MetricService( new RockContext() ).GetByIds( primaryMetricSource ).ToArray();

                    // Current Week of Year
                    var currentWeekOfYear = calendar.GetWeekOfYear( DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Sunday );

                    // Last Week
                    var lastWeekOfYear = calendar.GetWeekOfYear( DateTime.Now.AddDays( -7 ), CalendarWeekRule.FirstDay, DayOfWeek.Sunday );

                    var blockValues = new List<MetricValue>();

                    var i = 0;

                    // Get the metric values from the donutMetrics
                    foreach ( var metricItem in donutMetrics )
                    {
                        var metricItemCount = i++;
                        var metricItemTitle = metricItem.Title;

                        // Create empty lists for the search to be performed next
                        var currentWeekMetric = new decimal?();
                        var previousWeekMetric = new decimal?();

                        // Search DB Based on Current Week of Year
                        if ( dateRange.Start.HasValue && dateRange.End.HasValue )
                        {
                            if ( campusContext != null && pageContext.HasValue )
                            {
                                currentMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
                                    .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End && a.EntityId.ToString() == campusContext.Id.ToString() )
                                    .Select( a => a.YValue )
                                    .FirstOrDefault()
                                    );
                            }
                            else
                            {
                                currentWeekMetric = metricItem.MetricValues
                                    .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End )
                                    .Select( a => a.YValue )
                                    .FirstOrDefault();
                            }
                        }
                        else
                        {
                            currentWeekMetric = metricItem.MetricValues
                                .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.AddDays( -7 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == currentWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.Year )
                                .Select( a => a.YValue )
                                .FirstOrDefault();

                            previousWeekMetric = metricItem.MetricValues
                                .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.AddDays( -7 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == lastWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.Year )
                                .Select( a => a.YValue )
                                .FirstOrDefault();
                        }

                        // Assign Colors to Var
                        string metricItemColor = "#6bac43";

                        if ( metricItemCount % 2 != 0 )
                        {
                            metricItemColor = "#1c683e";
                        }
                        else if ( metricItemCount % 3 == 0 )
                        {
                            metricItemColor = "#2a4930";
                        }

                        // Create JSON array of data
                        if ( currentWeekMetric != null )
                        {
                            blockValues.Add( new MetricValue() { value = (int)currentWeekMetric.Value, color = metricItemColor, highlight = metricItemColor, label = metricItemTitle } );
                        }
                        else if ( previousWeekMetric != null )
                        {
                            blockValues.Add( new MetricValue() { value = (int)previousWeekMetric.Value, color = metricItemColor, highlight = metricItemColor, label = metricItemTitle } );
                        }
                        else
                        {
                            blockValues.Add( new MetricValue() { value = 0, color = metricItemColor, highlight = metricItemColor, label = metricItemTitle } );
                        }
                    }

                    MetricBlockValues = JsonConvert.SerializeObject( blockValues.ToArray() );
                }
            }
        }

        #endregion

        #region Classes

        /// <summary>
        /// Check-In information class used to bind the selected grid.
        /// </summary>
        [Serializable]
        protected class MetricValue
        {
            public int value { get; set; }

            public string color { get; set; }

            public string highlight { get; set; }

            public string label { get; set; }
        }

        [Serializable]
        protected class MetricJson
        {
            public System.DateTime date { get; set; }

            public string value { get; set; }

            public int week { get; set; }

            public int year { get; set; }
        }

        public static class MyStaticValues
        {
            public static bool metricCompareLastYear { get; set; }
        }

        public class MetricValueList
        {
            public string Name { get; set; }
            public decimal Value { get; set; }
        }

        #endregion Classes

        #region Internal Methods

        /// <summary>
        /// Gets the metric ids.
        /// </summary>
        /// <param name="metricAttribute">The metric attribute.</param>
        /// <returns></returns>
        public List<int> GetMetricIds( string metricAttribute )
        {
            var metricCategories = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( metricAttribute ) );

            var metricGuids = metricCategories.Select( a => a.MetricGuid ).ToList();
            return new MetricService( new Rock.Data.RockContext() ).GetByGuids( metricGuids ).Select( a => a.Id ).ToList();
        }

        #endregion
    }
}