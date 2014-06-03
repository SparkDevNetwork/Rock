// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Pie Chart" )]
    [Category( "Dashboard" )]
    [Description( "DashboardWidget using flotcharts" )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", Order = 3 )]
    [MetricCategoriesField( "Metrics", "Select the metrics to include in the pie chart.  Each Metric will be a section of the pie.", false, "", "", 4, "MetricCategories" )]
    [CustomCheckboxListField( "Metric Value Types", "Select which metric value types to display in the chart", "Goal,Measure", false, "Measure", Order = 5 )]
    [SlidingDateRangeField( "Date Range", Key = "SlidingDateRange", DefaultValue = "1||4||", Order = 6 )]
    [LinkedPage( "Detail Page", "Select the page to navigate to when the chart is clicked", Order = 7 )]
    public partial class PieChartDashboardWidget : DashboardWidget
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            LoadChart();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadChart();
        }
        
        /// <summary>
        /// Loads the chart.
        /// </summary>
        public void LoadChart()
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? "" );
            pcChart.StartDate = dateRange.Start;
            pcChart.EndDate = dateRange.End;
            pcChart.Title = this.Title;
            pcChart.Subtitle = this.Subtitle;
            //pcExample.CombineValues = this.CombineValues;
            pcChart.ShowTooltip = true;

            pcChart.Options.SetChartStyle( this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );
            //pcChart.DataSourceUrl = "TODO";

            nbMetricWarning.Visible = !this.Metrics.Any();
        }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>
        /// The metrics.
        /// </value>
        public List<Metric> Metrics
        {
            get
            {
                var metricCategoryGuids = ( GetAttributeValue( "MetricCategories" ) ?? string.Empty ).Split( ',' ).Select( a => a.AsGuidOrNull() ?? Guid.Empty ).ToList();
                return new MetricCategoryService( new Rock.Data.RockContext() ).GetByGuids( metricCategoryGuids ).Select( a => a.Metric ).ToList();
            }
        }
    }
}