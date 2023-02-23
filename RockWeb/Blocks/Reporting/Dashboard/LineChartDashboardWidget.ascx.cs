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
using System.ComponentModel;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// 
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
            lDashboardTitle.Text = this.Title;
            pnlDashboardTitle.Visible = !string.IsNullOrEmpty( this.Title );

            lDashboardSubtitle.Text = this.Subtitle;
            pnlDashboardSubtitle.Visible = !string.IsNullOrEmpty( this.Subtitle );

            metricChart.ShowTooltip = true;
            metricChart.ShowLegend = this.ShowLegend;
            metricChart.LegendPosition = this.LegendPosition;

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? string.Empty );

            var entityPartitionValues = this.GetPartitionEntityIdentifiers();
            //var entityPartitionValues = new List<MetricService.EntityIdentifierByTypeAndId>();

            //if ( this.GetEntityFromContextEnabled )
            //{
            //    var metricPartitionEntityIds = GetPrimaryMetricPartitionEntityIdFromContext();
            //    metricPartitionEntityIds = metricPartitionEntityIds.Where( a => a.MetricPartition != null ).ToList();
            //    if ( metricPartitionEntityIds.Any() )
            //    {
            //            entityPartitionValues = metricPartitionEntityIds
            //                .Select( a => new MetricService.EntityIdentifierByTypeAndId
            //                {
            //                    EntityTypeId = a.MetricPartition.EntityTypeId.GetValueOrDefault(0),
            //                    EntityId = a.EntityId.GetValueOrDefault(0)
            //                } )
            //                .ToList();

            //    }
            //}
            //else
            //{
            //    var metricEntityIdList = GetAttributeValue( "MetricEntityTypeEntityIds" );
            //    entityPartitionValues = metricEntityIdList.Split( ',' )
            //        .Select( a => a.Split( '|' ) )
            //        .Where( a => a.Length == 2 )
            //        .Select( a => new MetricService.EntityIdentifierByTypeAndId
            //        {
            //            EntityTypeId = a[0].AsInteger(),
            //            EntityId = a[1].AsInteger()
            //        } )
            //        .ToList();
            //}


            var metricValueType = this.MetricValueType;

            //
            // Get the data.
            //
            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );
            string metricName = null;

            // Configure for Metric
            var metric = metricService.Get( this.MetricId.GetValueOrDefault( 0 ) );

            if ( metric != null )
            {
                metricName = metric.Title;

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
            }

            // Add the Metric data.
            var metricIdList = new List<int> { this.MetricId ?? 0 };

            var qryMetric = metricService.GetMetricValuesQuery( metricIdList,
                metricValueType,
                dateRange.Start,
                dateRange.End,
                entityPartitionValues);

            var metricItems = qryMetric.ToList();

            metricChart.SetChartDataItems( metricItems, metricName );

            nbMetricWarning.Visible = !metricItems.Any();
        }
    }
}