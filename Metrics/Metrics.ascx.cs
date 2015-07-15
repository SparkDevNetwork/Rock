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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Newtonsoft.Json;

namespace RockWeb.Plugins.cc_newspring.Blocks.Metrics
{
    /// <summary>
    /// All Church Metrics Block
    /// </summary>
    [DisplayName( "All Church Metrics Block" )]
    [Category( "NewSpring" )]
    [Description( "All Church Metrics Block" )]
    [TextField( "Number of Columns", "", false, DefaultValue = "3", Order = 1 )]
    [CustomDropdownListField( "Metric Display Type", "", "Text,Line,Donut", Order = 2 )]
    [CustomDropdownListField( "Metric Period", "", "This Week,Last Week,One Year Ago,YTD", DefaultValue = "YTD", Order = 3 )]
    [MetricCategoriesField( "Metric Source", "Select the metric to include in this chart.", false, "", "", 4 )]
    // [SlidingDateRangeField( "Date Range", Key = "SlidingDateRange", DefaultValue = "1||4||", Order = 7 )]
    public partial class Metrics : Rock.Web.UI.RockBlock
    {
        

        #region Fields

        /// <summary>
        /// Gets or sets a value indicating whether [remove label from client queue].
        /// </summary>
        /// <value>
        /// <c>true</c> if [remove label from client queue]; otherwise, <c>false</c>.
        /// </value>
        protected string metricBlockValues
        {
            get
            {
                var metricBlockValues = ViewState["metricBlockValues"] as string;

                if ( metricBlockValues != null )
                {
                    return metricBlockValues;
                }

                return string.Empty;
            }
            set
            {
                ViewState["metricBlockValues"] = value;
            }
        }

        #endregion

        #region Control Methods

        // <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnInit( e );

            // Output variables direct to the ascx
            metricBlockId.Value = BlockName.Replace( " ", "" ).ToString();
            metricTitle.Value = BlockName;
            metricDisplay.Value = GetAttributeValue( "MetricDisplayType" );
            metricWidth.Value = GetAttributeValue( "NumberofColumns" );

            var churchMetricSource = GetMetricIds( "MetricSource" );
            var churchMetricPeriod = GetAttributeValue( "MetricPeriod" );

            var newMetric = new MetricService( new RockContext() ).GetByIds( churchMetricSource ).FirstOrDefault();

            // Show the warning if metric source is selected
            churchMetricWarning.Visible = !churchMetricSource.Any();

            // Show data if metric source is selected
            if ( newMetric != null )
            {
                if ( GetAttributeValue( "MetricDisplayType" ) == "Text" && newMetric != null )
                {
                    var churchMetricValue = newMetric.MetricValues;

                    if ( churchMetricPeriod == "YTD" )
                    {
                        metricNumber.Value = string.Format( "{0:n0}", churchMetricValue.Where( a => a.MetricValueDateTime > new DateTime( DateTime.Now.Year, 1, 1 ) ).Select( a => a.YValue ).Sum() );
                    }
                    else
                    {
                        var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;

                        // Current Week of Year
                        var currentWeekOfYear = calendar.GetWeekOfYear( DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Sunday );

                        // Last Week
                        var lastWeekOfYear = calendar.GetWeekOfYear( DateTime.Now.AddDays( -7 ), CalendarWeekRule.FirstDay, DayOfWeek.Sunday );

                        // Search DB Based on Current Week of Year
                        var currentWeekMetric = churchMetricValue.Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == currentWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.Year ).Select( a => a.YValue ).FirstOrDefault();

                        // Search DB Based on Last Week
                        var lastWeekMetric = churchMetricValue.Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == lastWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.Year ).Select( a => a.YValue ).FirstOrDefault();

                        if ( churchMetricPeriod == "This Week" && currentWeekMetric != null )
                        {
                            // This Week Metric Value
                            metricNumber.Value = string.Format( "{0:n0}", currentWeekMetric );

                            // Get The CSS Classes For The Trend Arrow
                            if ( currentWeekMetric > lastWeekMetric )
                            {
                                metricClass.Value = "fa-caret-up brand-success";
                            }
                            else
                            {
                                metricClass.Value = "fa-caret-down brand-danger";
                            }
                        }
                        else if ( churchMetricPeriod == "Last Week" && lastWeekMetric != null )
                        {
                            // Last Week Metric Value
                            metricNumber.Value = string.Format( "{0:n0}", lastWeekMetric );
                        }
                        else if ( churchMetricPeriod == "One Year Ago" )
                        {
                            // Search DD For Metric From This Week Last Year
                            var lastYearMetric = churchMetricValue.Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == currentWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.AddYears( -1 ).Year ).Select( a => a.YValue ).FirstOrDefault();

                            if ( lastYearMetric != null )
                            {
                                // This Week Last Year Metric
                                metricNumber.Value = string.Format( "{0:n0}", lastYearMetric );
                            }
                            else
                            {
                                // This Week Last Year Metric
                                metricNumber.Value = "0";
                            }
                        }
                        else
                        {
                            metricNumber.Value = "0";
                        }
                    }
                }
                else if ( GetAttributeValue( "MetricDisplayType" ) == "Line" && newMetric != null )
                {
                    //foreach ( var metric in newMetric )
                    //{
                    var churchMetricValue = newMetric.MetricValues;

                    var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;

                    // var lastYearMetric = churchMetricValue.Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == currentWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.AddYears( -1 ).Year ).Select( a => a.YValue ).ToList();

                    currentYear.Value = DateTime.Now.Year.ToString();
                    previousYear.Value = DateTime.Now.AddYears( -1 ).Year.ToString();

                    // Create an array of of labels
                    var metricLabelsArray = churchMetricValue
                        .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) > calendar.GetWeekOfYear( a.MetricValueDateTime.Value.AddDays( -42 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) && a.MetricValueDateTime.Value.Year == DateTime.Now.Year )
                        .OrderBy( a => a.MetricValueDateTime )
                        .Select( a => new DateTime( a.MetricValueDateTime.Value.Year, a.MetricValueDateTime.Value.Month, a.MetricValueDateTime.Value.Day ).ToString( "MMMM dd" ) )
                        .ToArray();

                    var metricLabelsString = string.Join( ",", metricLabelsArray );

                    // Format the array of labels for output
                    metricLabels.Value = "'" + metricLabelsString.Replace( ",", "','" ) + "'";

                    // Create an array of data points (Current Year)
                    //var metricDataPointSumsCurrentYear = churchMetricValue
                    //    .Where( a => a.MetricValueDateTime > DateTime.Now.AddMonths( -6 ) )
                    //    .Select( a => new { Month = a.MetricValueDateTime.Value.Month, YValue = a.YValue } )
                    //    .GroupBy( a => a.Month )
                    //    .Select( a => new { a.Key, Sum = a.Sum( v => v.YValue ).ToString() } )
                    //    .OrderBy( a => a.Key )
                    //    .Select( a => a.Sum)
                    //    .ToArray();

                    var metricDataPointSumsCurrentYear = churchMetricValue
                        .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) > calendar.GetWeekOfYear( a.MetricValueDateTime.Value.AddDays( -42 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) && a.MetricValueDateTime.Value.Year == DateTime.Now.Year )
                        .OrderBy( a => a.MetricValueDateTime )
                        .Select( a => string.Format( "{0:0}", a.YValue ) )
                        .ToArray();

                    var metricDataPointStringCurrent = string.Join( ",", metricDataPointSumsCurrentYear );

                    // Format the array of sums for output
                    metricDataPointsCurrent.Value = "'" + metricDataPointStringCurrent.Replace( ",", "','" ) + "'";

                    // Create an array of data points (Current Year)
                    var metricDataPointSumsPreviousYear = churchMetricValue
                        .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) > calendar.GetWeekOfYear( a.MetricValueDateTime.Value.AddDays( -42 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) && a.MetricValueDateTime.Value.Year == DateTime.Now.AddYears( -1 ).Year )
                        .OrderBy( a => a.MetricValueDateTime )
                        .Select( a => string.Format( "{0:0}", a.YValue ) )
                        .ToArray();

                    var metricDataPointStringPrevious = string.Join( ",", metricDataPointSumsPreviousYear );

                    // Format the array of sums for output
                    metricDataPointsPrevious.Value = "'" + metricDataPointStringPrevious.Replace( ",", "','" ) + "'";
                    //}
                }
                else if ( GetAttributeValue( "MetricDisplayType" ) == "Donut" && newMetric != null )
                {
                    var donutMetrics = new MetricService( new RockContext() ).GetByIds( churchMetricSource ).ToArray();

                    var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;

                    // Current Week of Year
                    var currentWeekOfYear = calendar.GetWeekOfYear( DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Sunday );

                    // Last Week
                    var lastWeekOfYear = calendar.GetWeekOfYear( DateTime.Now.AddDays( -7 ), CalendarWeekRule.FirstDay, DayOfWeek.Sunday );

                    var blockValues = new List<MetricValue>();

                    // Get the metric values from the donutMetrics
                    foreach ( var metricItem in donutMetrics )
                    {
                        // var metricItemCount = i++.ToString();
                        var metricItemTitle = metricItem.Title;

                        // Search DB Based on Current Week of Year
                        var currentWeekMetric = metricItem.MetricValues
                            .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == currentWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.Year )
                            .Select( a => a.YValue )
                            .FirstOrDefault();

                        var previousWeekMetric = metricItem.MetricValues
                                .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.AddDays( -1 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == lastWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.Year )
                                .Select( a => a.YValue )
                                .FirstOrDefault();

                        if ( currentWeekMetric != null )
                        {
                            blockValues.Add( new MetricValue() { value = (int)currentWeekMetric.Value, color = "#6bac43", highlight = "#6bac43", label = metricItemTitle } );
                        }
                        else if ( previousWeekMetric != null)
                        {
                            blockValues.Add( new MetricValue() { value = (int)previousWeekMetric.Value, color = "#6bac43", highlight = "#6bac43", label = metricItemTitle } );
                        }
                        else
                        {
                            blockValues.Add( new MetricValue() { value = 0, color = "#6bac43", highlight = "#6bac43", label = metricItemTitle } );
                        }
                    }

                    metricBlockValues = JsonConvert.SerializeObject( blockValues.ToArray() );
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

        #endregion Classes

        #region Internal Methods

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <returns></returns>
        ///
        /// <value>
        /// The metrics.
        /// </value>
        public List<int> GetMetricIds( string metricAttribute )
        {
            var metricCategories = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( metricAttribute ) );

            var metricGuids = metricCategories.Select( a => a.MetricGuid ).ToList();
            return new MetricService( new Rock.Data.RockContext() ).GetByGuids( metricGuids ).Select( a => a.Id ).ToList();
        }

        #endregion
    }
}