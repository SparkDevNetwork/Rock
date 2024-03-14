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
using Rock.Attribute;
using Rock.Chart;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// The Pie Chart shows the relative proportions of a collection of Metrics.
    /// Each Metric represents a slice of the pie.
    /// </summary>
    [DisplayName( "Pie Chart" )]
    [Category( "Reporting > Dashboard" )]
    [Description( "Pie Chart Dashboard Widget" )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES,
        Name = "Chart Style",
        Order = 3 )]
    [EntityField( "Series Partition",
        "Select the series partition entity (Campus, Group, etc) to be used to limit the metric values for the selected metrics.",
        "Either select a specific {0} or leave {0} blank to get it from the page context.",
        false,
        Key = "Entity",
        Order = 4 )]
    [MetricCategoriesField( "Metrics",
        Description = "Select the metrics to include in the pie chart.  Each Metric will be a section of the pie.",
        IsRequired = false,
        Key = "MetricCategories",
        Order = 5 )]
    [CustomRadioListField( "Metric Value Type", "Select which metric value type to display in the chart", "Goal,Measure", false, "Measure", Order = 6 )]
    [SlidingDateRangeField( "Date Range",
        Key = "SlidingDateRange",
        DefaultValue = "1||4||",
        Order = 7 )]
    [LinkedPage( "Detail Page",
        Description = "Select the page to navigate to when the chart is clicked",
        IsRequired = false,
        Order = 8 )]
    [Rock.SystemGuid.BlockTypeGuid( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D" )]
    public partial class PieChartDashboardWidget : MetricChartDashboardWidget
    {
        protected override IRockChart GetChartControl()
        {
            return metricChart;
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
        /// Loads the chart.
        /// </summary>
        public override void OnLoadChart()
        {
            // Configure the chart appearance and layout.
            pnlDashboardTitle.Visible = !string.IsNullOrEmpty( this.Title );
            pnlDashboardSubtitle.Visible = !string.IsNullOrEmpty( this.Subtitle );

            lDashboardTitle.Text = this.Title;
            lDashboardSubtitle.Text = this.Subtitle;

            metricChart.ShowTooltip = true;
            metricChart.ShowLegend = this.ShowLegend;
            metricChart.LegendPosition = this.LegendPosition;

            nbMetricWarning.Visible = false;

            // Get the collection of Metrics that will comprise the pie chart.
            // Each Metric is displayed as a separate section of the pie.
            var metricIdList = this.GetConfiguredMetricIds();
            if ( !metricIdList.Any() )
            {
                nbMetricWarning.Visible = true;
                return;
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? string.Empty );

            // Build the chart data.
            var builder = new MetricChartDataSourceBuilder();
            builder.MetricIdList = metricIdList;

            builder.ValueType = this.MetricValueType ?? Rock.Model.MetricValueType.Measure;
            builder.StartDate = dateRange.Start;
            builder.EndDate = dateRange.End;
            builder.DefaultSeriesName = "Total";
            builder.PartitionValues = this.GetConfiguredPartitionValues();
            builder.CombineValues = this.CombineValues;

            var dataSet = builder.GetCategorySeriesDataset();
            if ( !dataSet.DataPoints.Any() )
            {
                nbMetricWarning.Visible = true;
                return;
            }

            metricChart.SetChartDataItems( dataSet );
        }

        private List<MetricService.EntityIdentifierByTypeAndId> GetConfiguredPartitionValues()
        {
            var entityValues = ( GetAttributeValue( "Entity" ) ?? string.Empty ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            EntityTypeCache entityType = null;
            if ( entityValues.Length >= 1 )
            {
                entityType = EntityTypeCache.Get( entityValues[0].AsGuid() );
            }

            int? entityTypeId = null;
            int? entityId = null;

            if ( entityValues.Length == 2 )
            {
                // entity id specified by block setting
                if ( entityType != null )
                {
                    entityTypeId = entityType.Id;
                    entityId = entityValues[1].AsIntegerOrNull();
                }
            }
            else
            {
                // entity id comes from context
                Rock.Data.IEntity contextEntity;
                if ( entityType != null )
                {
                    contextEntity = this.ContextEntity( entityType.Name );
                }
                else
                {
                    contextEntity = this.ContextEntity();
                }

                if ( contextEntity != null )
                {
                    entityTypeId = EntityTypeCache.GetId( contextEntity.GetType() );
                    entityId = contextEntity.Id;
                }
            }

            var partitionValues = new List<MetricService.EntityIdentifierByTypeAndId>
                {
                    new MetricService.EntityIdentifierByTypeAndId
                    {
                        EntityTypeId = entityTypeId.GetValueOrDefault(),
                        EntityId = entityId.GetValueOrDefault()
                    }
                };

            return partitionValues;
        }

        private List<int> GetConfiguredMetricIds()
        {
            var metricCategories = Rock.Attribute.MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( "MetricCategories" ) );

            var metricGuids = metricCategories.Select( a => a.MetricGuid ).ToList();
            return new MetricService( new Rock.Data.RockContext() ).GetByGuids( metricGuids ).Select( a => a.Id ).ToList();
        }
    }
}