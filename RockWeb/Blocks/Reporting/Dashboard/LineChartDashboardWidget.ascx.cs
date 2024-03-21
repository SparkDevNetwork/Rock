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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rock.Chart;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// A dashboard widget that displays a line chart based on one or more Metrics.
    /// </summary>
    [DisplayName( "Line Chart" )]
    [Category( "Reporting > Dashboard" )]
    [Description( "Line Chart Dashboard Widget" )]
    [Rock.SystemGuid.BlockTypeGuid( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1" )]
    public partial class LineChartDashboardWidget : MetricChartDashboardWidget
    {
        protected override IRockChart GetChartControl()
        {
            return metricChart;
        }

        public override void OnLoadChart()
        {
            // Configure the chart appearance and layout.
            lDashboardTitle.Text = this.Title;
            pnlDashboardTitle.Visible = !string.IsNullOrEmpty( this.Title );

            lDashboardSubtitle.Text = this.Subtitle;
            pnlDashboardSubtitle.Visible = !string.IsNullOrEmpty( this.Subtitle );

            metricChart.ShowTooltip = true;
            metricChart.ShowLegend = this.ShowLegend;
            metricChart.LegendPosition = this.LegendPosition;

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? string.Empty );

            nbMetricWarning.Visible = false;

            // Configure the chart for the specified Metric.
            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );

            var metric = metricService.Get( this.MetricId.GetValueOrDefault( 0 ) );

            if ( metric == null )
            {
                nbMetricWarning.Visible = true;
                return;
            }

            if ( string.IsNullOrWhiteSpace( metricChart.XAxisLabel ) )
            {
                // if XAxisLabel hasn't been set, and this is a metric, automatically set it to the metric.XAxisLabel
                metricChart.XAxisLabel = metric.XAxisLabel;
            }

            if ( string.IsNullOrWhiteSpace( metricChart.YAxisLabel ) )
            {
                // if YAxisLabel hasn't been set, and this is a metric, automatically set it to the metric.YAxisLabel
                metricChart.YAxisLabel = metric.YAxisLabel;
            }

            // Build the chart data.
            var builder = new MetricChartDataSourceBuilder();
            builder.MetricIdList = new List<int> { this.MetricId ?? 0 };
            builder.ValueType = this.MetricValueType ?? Rock.Model.MetricValueType.Measure;
            builder.StartDate = dateRange.Start;
            builder.EndDate = dateRange.End;
            builder.PartitionValues = this.GetSelectedPartitionEntityIdentifiers();
            builder.CombineValues = this.CombineValues;
            builder.DefaultSeriesName = metric.Title;

            var dataSets = builder.GetTimeSeriesDatasets();
            if ( !dataSets.Any() )
            {
                nbMetricWarning.Visible = true;
                return;
            }

            metricChart.SetChartDataItems( dataSets );
        }
    }
}