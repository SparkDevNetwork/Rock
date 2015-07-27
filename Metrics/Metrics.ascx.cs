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
using Newtonsoft.Json;
// using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.Metrics
{
    /// <summary>
    /// All Church Metrics Block
    /// </summary>

    [DisplayName( "Church Metrics" )]
    [Category( "NewSpring" )]
    [Description( "Custom church metrics block using the Chart.js library" )]
    [CustomDropdownListField( "Number of Columns", "", "1,2,3,4,5,6,7,8,9,10,11,12", false, DefaultValue = "12", Order = 1 )]
    [CustomDropdownListField( "Metric Display Type", "", "Text,Line,Donut", Order = 2 )]
    [SlidingDateRangeField( "Date Range", Key = "SlidingDateRange", Order = 3 )]
    [CustomRadioListField( "Custom Dates", "If not using date range, please select a custom date from here", "One Year Ago", Order = 4 )]
    [CustomCheckboxListField( "Compare Against Last Year", "", "Yes", Order = 5 )]
    [MetricCategoriesField( "Metric Source", "Select the metric to include in this chart.", false, "", "", 6 )]
    [CustomCheckboxListField( "Respect Campus Context", "", "Yes", Order = 7 )]

    public partial class Metrics : Rock.Web.UI.RockBlock
    {
        #region Fields

        /// <summary>
        /// Gets or sets the metric block values.
        /// </summary>
        /// <value>
        /// The metric block values.
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
                
                var savedValue = ViewState[ string.Format( "MetricCompareLastYear_{0}", BlockId) ] as string;

                if ( savedValue != null )
                {
                    return savedValue.ToString();
                }

                return string.Empty;
            }
            set
            {
                ViewState[ string.Format( "MetricCompareLastYear_{0}", BlockId ) ] = value;
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

            #region Campus Context

            var campusEntityType = EntityTypeCache.Read( "Rock.Model.Campus" );

            var campus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

            var campusContext = GetAttributeValue( "RespectCampusContext" ).AsBooleanOrNull();

            #endregion

            #region Global Variables

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? string.Empty );
            
            // Output variables direct to the ascx
            metricBlockNumber.Value = BlockId.ToString();
            metricBlockId.Value = BlockName.Replace( " ", "" ).ToString();
            metricTitle.Value = BlockName;
            metricDisplay.Value = GetAttributeValue( "MetricDisplayType" );
            metricWidth.Value = GetAttributeValue( "NumberofColumns" );

            var metricCustomDates = GetAttributeValue( "CustomDates" );

            var churchMetricSource = GetMetricIds( "MetricSource" );
            var churchMetricPeriod = GetAttributeValue( "MetricPeriod" );

            MetricCompareLastYear = GetAttributeValue( "CompareAgainstLastYear" ).ToString();

            var newMetric = new MetricService( new RockContext() ).GetByIds( churchMetricSource ).FirstOrDefault();

            // Show the warning if metric source is selected
            churchMetricWarning.Visible = !churchMetricSource.Any();

            // This sets the var to do a Week of Year calculation
            var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;

            #endregion

            // Show data if metric source is selected
            if ( newMetric != null )
            {
                if ( GetAttributeValue( "MetricDisplayType" ) == "Text" && newMetric != null )
                {
                    var churchMetricValue = newMetric.MetricValues;

                    if ( dateRange != null )
                    {
                        if ( campus != null && campusContext.HasValue )
                        {
                            currentMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
                                .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End && a.EntityId.ToString() == campus.Id.ToString() )
                                .Select( a => a.YValue )
                                .Sum()
                                );

                            if ( MetricCompareLastYear == "Yes" )
                            {
                                previousMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
                                    .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddYears( -1 ) && a.MetricValueDateTime <= dateRange.End.Value.AddYears( -1 ) && a.EntityId.ToString() == campus.Id.ToString() )
                                    .Select( a => a.YValue )
                                    .Sum()
                                    );
                            }
                        }
                        else
                        {
                            currentMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
                                .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End )
                                .Select( a => a.YValue )
                                .Sum()
                                );

                            if ( MetricCompareLastYear == "Yes" )
                            {
                                previousMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
                                    .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddYears( -1 ) && a.MetricValueDateTime <= dateRange.End.Value.AddYears( -1 ) )
                                    .Select( a => a.YValue )
                                    .Sum()
                                    );
                            }
                        }
                        
                    }
                    else if ( metricCustomDates == "One Year Ago" )
                    {
                        currentMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
                            .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == calendar.GetWeekOfYear( DateTime.Now.AddYears(-1).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) )
                            .Select( a => a.YValue )
                            .Sum()
                            );
                    }

                    // Still Need to add the trending arrows back in

                    // Get The CSS Classes For The Trend Arrow
                    // if ( currentWeekMetric > lastWeekMetric )
                    // {
                    //  metricClass.Value = "fa-caret-up brand-success";
                    // }
                    // else
                    // {
                    //  metricClass.Value = "fa-caret-down brand-danger";
                    // }
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
                    if ( dateRange != null )
                    {

                        if ( campus != null && campusContext.HasValue )
                        {
                            metricCurrentYear = newMetric.MetricValues
                                .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End && a.EntityId.ToString() == campus.Id.ToString() )
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
                                    .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddYears( -1 ) && a.MetricValueDateTime <= dateRange.End.Value.AddYears( -1 ) && a.EntityId.ToString() == campus.Id.ToString() )
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
                    var donutMetrics = new MetricService( new RockContext() ).GetByIds( churchMetricSource ).ToArray();

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
                        if ( dateRange != null )
                        {
                            if ( campus != null && campusContext.HasValue )
                            {
                                currentMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
                                    .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End && a.EntityId.ToString() == campus.Id.ToString() )
                                    .Select( a => a.YValue )
                                    .FirstOrDefault()
                                    );
                            }

                            currentWeekMetric = metricItem.MetricValues
                                .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End )
                                .Select( a => a.YValue )
                                .FirstOrDefault();

                        } else {

                            currentWeekMetric = metricItem.MetricValues
                                .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.AddDays(-7).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == currentWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.Year )
                                .Select( a => a.YValue )
                                .FirstOrDefault();

                            previousWeekMetric = metricItem.MetricValues
                                .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.AddDays(-7).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == lastWeekOfYear && a.MetricValueDateTime.Value.Year == DateTime.Now.Year )
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