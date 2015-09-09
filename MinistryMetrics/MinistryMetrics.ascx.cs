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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using Newtonsoft.Json;
using System.Web.UI.WebControls;
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

namespace RockWeb.Plugins.cc_newspring.Blocks.MinistryMetrics
{
    /// <summary>
    /// Ministry Metrics Block
    /// </summary>
    /// 
    [DisplayName( "Ministry Metrics" )]
    [Category( "NewSpring" )]
    [Description( "Custom church metrics block using the Chart.js library" )]
    [CustomDropdownListField(
        "Number of Columns",
        "",
        "1,2,3,4,5,6,7,8,9,10,11,12",
        false,
        DefaultValue = "12",
        Order = 1
    )]
    [CustomDropdownListField(
        "Metric Type",
        "",
        @"
            # Vols Roles Filled,
            Roster #,
            % of Roster Serving,
            Unique Vols Attended,
            Goal #,
            % of Goal,
            MS Student #,
            HS Student #,
            Total Attendee #,
            Sunday Attendance #,
            % of Attendee to Sunday #,
            Total Vol #,
            1st Time Attendee #,
            4 week Return %,
            Fuse Salvation #,
            Salv #,
            Bapt #,
            Care Room Visit #,
            Ownership Class #,
            Financial Coaching #,
            Group Leader #,
            Group Participant #,
            Avg Group Attendance #,
            % Group participation,
            Total Group Members,
            Salv Attributes,
            % Roster # Changed,
            % Unique Vols # Changed,
            Vol Assignments per Vol,
            1st Serves,
            Ratio of Vols to Attendees
        ",
        false,
        Order = 2
    )]
    [CustomRadioListField(
        "Modifier",
        "",
        @"
            By Service,
            By Group,
            Change 6w,
            Change 1y,
            YTD,
            Change 4w,
            Totals
        ",
        Order = 3
    )]

    public partial class MinistryMetrics : Rock.Web.UI.RockBlock
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

                var savedValue = ViewState[string.Format( "MetricCompareLastYear_{0}", BlockId )] as string;

                if ( savedValue != null )
                {
                    return savedValue.ToString();
                }

                return string.Empty;
            }
            set
            {
                ViewState[string.Format( "MetricCompareLastYear_{0}", BlockId )] = value;
            }
        }



        #endregion Fields

        #region Control Methods

        // <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnInit( e );

            #region Drop Down Context

            //var campusEntityType = EntityTypeCache.Read( "Rock.Model.Campus" );

            //var campus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

            //var campusContext = GetAttributeValue( "RespectCampusContext" ).AsBooleanOrNull();

            #endregion Drop Down Context

            #region Global Variables

            // var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? string.Empty );

            var metricType = GetAttributeValue( "MetricType" );
            var metricModifier = GetAttributeValue( "Modifier" );

            // Output variables direct to the ascx
            // metricBlockId.Value = BlockName.Replace( " ", "" ).ToString();
            String metricTitle = BlockName;
            // metricDisplay.Value = GetAttributeValue( "MetricDisplayType" );
            metricWidth.Value = GetAttributeValue( "NumberofColumns" );

            String currentMetricValue = string.Empty;

            // var metricCustomDates = GetAttributeValue( "CustomDates" );

            // var churchMetricSource = GetMetricIds( "MetricSource" );
            // var churchMetricPeriod = GetAttributeValue( "MetricPeriod" );

            // MetricCompareLastYear = GetAttributeValue( "CompareAgainstLastYear" ).ToString();

            // var newMetric = new MetricService( new RockContext() ).GetByIds( churchMetricSource ).FirstOrDefault();

            #endregion Global Variables

            #region Metric Modifiers

            #endregion Metric Modifiers

            #region Metric Type Conditionals

            if ( metricType == "Test") {

            }
            else if ( metricType == "This" )
            {

            }

            #endregion Metric Type Conditionals

            #region Original Metrics Code

            // Output variables direct to the ascx
            // metricBlockNumber.Value = BlockId.ToString();
            // metricBlockId.Value = BlockName.Replace( " ", "" ).ToString();
            // metricTitle.Value = BlockName;
            // metricDisplay.Value = GetAttributeValue( "MetricDisplayType" );
            // metricWidth.Value = GetAttributeValue( "NumberofColumns" );

            // var metricCustomDates = GetAttributeValue( "CustomDates" );

            // var churchMetricSource = GetMetricIds( "MetricSource" );
            // var churchMetricPeriod = GetAttributeValue( "MetricPeriod" );

            // MetricCompareLastYear = GetAttributeValue( "CompareAgainstLastYear" ).ToString();

            // var newMetric = new MetricService( new RockContext() ).GetByIds( churchMetricSource ).FirstOrDefault();

            // Show the warning if metric source is selected
            // churchMetricWarning.Visible = !churchMetricSource.Any();

            // This sets the var to do a Week of Year calculation
            // var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;

            // Show data if metric source is selected
            //if ( newMetric != null )
            //{
            //    if ( GetAttributeValue( "MetricDisplayType" ) == "Text" && newMetric != null )
            //    {
            //        var churchMetricValue = newMetric.MetricValues;

            //        if ( dateRange.Start.HasValue && dateRange.End.HasValue )
            //        {
            //            if ( campus != null && campusContext.HasValue )
            //            {

            //                var currentweekMetricValue = newMetric.MetricValues
            //                    .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End && a.EntityId.ToString() == campus.Id.ToString() )
            //                    .Select( a => a.YValue )
            //                    .Sum();

            //                currentMetricValue.Value = string.Format( "{0:n0}", currentweekMetricValue );

            //                // Compare currentMetricValue to the same date range 7 days previous

            //                var previousweekMetricValue = newMetric.MetricValues
            //                    .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddDays( -7 ) && a.MetricValueDateTime <= dateRange.End.Value.AddDays( -7 ) && a.EntityId.ToString() == campus.Id.ToString() )
            //                    .Select( a => a.YValue )
            //                    .Sum();

            //                if ( currentweekMetricValue > previousweekMetricValue )
            //                {
            //                    metricClass.Value = "fa-caret-up brand-success";
            //                }
            //                else if ( currentweekMetricValue < previousweekMetricValue )
            //                {
            //                    metricClass.Value = "fa-caret-down brand-danger";
            //                }

            //                if ( MetricCompareLastYear == "Yes" )
            //                {
            //                    previousMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
            //                        .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddYears( -1 ) && a.MetricValueDateTime <= dateRange.End.Value.AddYears( -1 ) && a.EntityId.ToString() == campus.Id.ToString() )
            //                        .Select( a => a.YValue )
            //                        .Sum()
            //                        );
            //                }
            //            }
            //            else
            //            {
            //                var currentweekMetricValue = newMetric.MetricValues
            //                    .Where( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime <= dateRange.End )
            //                    .Select( a => a.YValue )
            //                    .Sum();

            //                currentMetricValue.Value = string.Format( "{0:n0}", currentweekMetricValue );

            //                // Compare currentMetricValue to the same date range 7 days previous

            //                var previousweekMetricValue = newMetric.MetricValues
            //                    .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddDays( -7 ) && a.MetricValueDateTime <= dateRange.End.Value.AddDays( -7 ) )
            //                    .Select( a => a.YValue )
            //                    .Sum();

            //                if ( currentweekMetricValue > previousweekMetricValue )
            //                {
            //                    metricClass.Value = "fa-caret-up brand-success";
            //                }
            //                else if ( currentweekMetricValue < previousweekMetricValue )
            //                {
            //                    metricClass.Value = "fa-caret-down brand-danger";
            //                }

            //                //if ( MetricCompareLastYear == "Yes" )
            //                //{
            //                //    previousMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
            //                //        .Where( a => a.MetricValueDateTime >= dateRange.Start.Value.AddYears( -1 ) && a.MetricValueDateTime <= dateRange.End.Value.AddYears( -1 ) )
            //                //        .Select( a => a.YValue )
            //                //        .Sum()
            //                //        );
            //                //}
            //            }

            //        }
            //        else if ( metricCustomDates == "One Year Ago" )
            //        {
            //            //if ( campus != null && campusContext.HasValue )
            //            //{

            //            //    currentMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
            //            //        .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == calendar.GetWeekOfYear( DateTime.Now.AddYears( -1 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) && a.MetricValueDateTime.Value.Year.ToString() == DateTime.Now.AddYears(-1).ToString() && a.EntityId.ToString() == campus.Id.ToString() )
            //            //        .Select( a => a.YValue )
            //            //        .Sum()
            //            //    );
            //            //}
            //            //else
            //            //{
            //            //    currentMetricValue.Value = string.Format( "{0:n0}", newMetric.MetricValues
            //            //    .Where( a => calendar.GetWeekOfYear( a.MetricValueDateTime.Value.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) == calendar.GetWeekOfYear( DateTime.Now.AddYears( -1 ).Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday ) && a.MetricValueDateTime.Value.Year.ToString() == DateTime.Now.AddYears( -1 ).ToString() )
            //            //    .Select( a => a.YValue )
            //            //    .Sum()
            //            //    );
            //            //}
            //        }

            //        // Still Need to add the trending arrows back in

            //        // Get The CSS Classes For The Trend Arrow
            //        // if ( currentWeekMetric > lastWeekMetric )
            //        // {
            //        //  metricClass.Value = "fa-caret-up brand-success";
            //        // }
            //        // else
            //        // {
            //        //  metricClass.Value = "fa-caret-down brand-danger";
            //        // }
            //    }
            //}
        }

            #endregion Original Metrics Code

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

        //public List<int> GetMetricIds( string metricAttribute )
        //{
        //    var metricCategories = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( metricAttribute ) );

        //    var metricGuids = metricCategories.Select( a => a.MetricGuid ).ToList();
        //    return new MetricService( new Rock.Data.RockContext() ).GetByGuids( metricGuids ).Select( a => a.Id ).ToList();
        //}

        #endregion
        }

        #endregion Control Methods

}