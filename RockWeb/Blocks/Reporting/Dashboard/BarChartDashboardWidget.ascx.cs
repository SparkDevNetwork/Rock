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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    [DisplayName( "Bar Chart" )]
    [Category( "Reporting > Dashboard" )]
    [Description( "Bar Chart Dashboard Widget" )]
    [Rock.SystemGuid.BlockTypeGuid( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD" )]
    public partial class BarChartDashboardWidget : MetricChartDashboardWidget
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

            //var rockChart = metricChart as IRockChart;

            ////metricChart.CombineValues = this.CombineValues;

            //var detailPageGuid = ( GetAttributeValue( "DetailPage" ) ?? string.Empty ).AsGuidOrNull();
            //if ( detailPageGuid.HasValue )
            //{
            //    rockChart.ChartClick += ChartClickHandler;
            //}

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? string.Empty );

            var metricValueType = this.MetricValueType;
            var entityPartitionValues = this.GetPartitionEntityIdentifiers();

            //var entityValues = ( GetAttributeValue( "Entity" ) ?? string.Empty ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            //EntityTypeCache entityType = null;
            //if ( entityValues.Length >= 1 )
            //{
            //    entityType = EntityTypeCache.Get( entityValues[0].AsGuid() );
            //}

            //int? entityTypeId = null;
            //int? entityId = null;

            //if ( entityValues.Length == 2 )
            //{
            //    // entity id specified by block setting
            //    if ( entityType != null )
            //    {
            //        entityTypeId = entityType.Id;
            //        entityId = entityValues[1].AsIntegerOrNull();
            //    }
            //}
            //else
            //{
            //    // entity id comes from context
            //    Rock.Data.IEntity contextEntity;
            //    if ( entityType != null )
            //    {
            //        contextEntity = this.ContextEntity( entityType.Name );
            //    }
            //    else
            //    {
            //        contextEntity = this.ContextEntity();
            //    }

            //    if ( contextEntity != null )
            //    {
            //        entityTypeId = EntityTypeCache.GetId( contextEntity.GetType() );
            //        entityId = contextEntity.Id;
            //    }
            //}

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
                entityPartitionValues );

            var metricItems = qryMetric.ToList();

            metricChart.SetChartDataItems( metricItems, metricName );

            nbMetricWarning.Visible = !metricItems.Any();
        }

    }
}