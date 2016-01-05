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
using System.Data;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Dynamic Chart" )]
    [Category( "Reporting" )]
    [Description( "Block to display a chart using SQL as the chart datasource" )]

    [CodeEditorField( "SQL", @"The SQL for the datasource. Output columns must be as follows:
<ul>
    <li>Bar or Line Chart
        <ul>
           <li>[SeriesID] : string or numeric </li>
           <li>[DateTime] : DateTime </li>
           <li>[YValue] : numeric </li>
        </ul>
    </li>
    <li>Pie Chart
        <ul>
           <li>[MetricTitle] : string </li>
           <li>[YValueTotal] : numeric </li>
        </ul>
    </li>
</ul>

Example: 
<code><pre>
-- get top 25 viewed pages from the last 30 days (excluding Home)
select top 25  * from (
    select 
        distinct
        pv.PageTitle [SeriesID], 
        convert(date, pv.DateTimeViewed) [DateTime], 
        count(*) [YValue] 
    from 
        PageView pv
    where PageTitle is not null    
    group by pv.PageTitle, convert(date, pv.DateTimeViewed)
    ) x where SeriesID != 'Home' 
and DateTime > DateAdd(day, -30, SysDateTime())
order by YValue desc
</pre>
</code>",
              CodeEditorMode.Sql )]

    [IntegerField( "Chart Height", "", false, 200 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", order: 3 )]

    [BooleanField( "Show Legend", "", true, order: 7 )]
    [CustomDropdownListField( "Legend Position", "Select the position of the Legend (corner)", "ne,nw,se,sw", false, "ne", order: 8 )]
    [CustomDropdownListField( "Chart Type", "", "Line,Bar,Pie", false, "Line", order: 9 )]
    [DecimalField( "Pie Inner Radius", "If this is a pie chart, specific the inner radius to have a donut hole. For example, specify: 0.75 to have the inner radius as 75% of the outer radius.", false, 0, order: 10 )]
    [BooleanField( "Pie Show Labels", "If this is a pie chart, specify if labels show be shown", true, "", order: 11 )]
    public partial class DynamicChart : Rock.Reporting.Dashboard.DashboardWidget
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            var pageReference = new Rock.Web.PageReference( this.PageCache.Id );
            pageReference.QueryString = new System.Collections.Specialized.NameValueCollection();
            pageReference.QueryString.Add( "GetChartData", "true" );
            pageReference.QueryString.Add( "GetChartDataBlockId", this.BlockId.ToString() );
            pageReference.QueryString.Add( "TimeStamp", RockDateTime.Now.ToJavascriptMilliseconds().ToString() );
            lcLineChart.DataSourceUrl = pageReference.BuildUrl();
            lcLineChart.ChartHeight = this.GetAttributeValue( "ChartHeight" ).AsIntegerOrNull() ?? 200;
            lcLineChart.Options.SetChartStyle( this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );
            lcLineChart.Options.legend = lcLineChart.Options.legend ?? new Legend();
            lcLineChart.Options.legend.show = this.GetAttributeValue( "ShowLegend" ).AsBooleanOrNull();
            lcLineChart.Options.legend.position = this.GetAttributeValue( "LegendPosition" );

            bcBarChart.DataSourceUrl = pageReference.BuildUrl();
            bcBarChart.ChartHeight = this.GetAttributeValue( "ChartHeight" ).AsIntegerOrNull() ?? 200;
            bcBarChart.Options.SetChartStyle( this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );
            bcBarChart.Options.xaxis = new AxisOptions { mode = AxisMode.categories, tickLength = 0 };
            bcBarChart.Options.series.bars.barWidth = 0.6;
            bcBarChart.Options.series.bars.align = "center";

            bcBarChart.Options.legend = lcLineChart.Options.legend ?? new Legend();
            bcBarChart.Options.legend.show = this.GetAttributeValue( "ShowLegend" ).AsBooleanOrNull();
            bcBarChart.Options.legend.position = this.GetAttributeValue( "LegendPosition" );

            pcPieChart.DataSourceUrl = pageReference.BuildUrl();
            pcPieChart.ChartHeight = this.GetAttributeValue( "ChartHeight" ).AsIntegerOrNull() ?? 200;
            pcPieChart.Options.SetChartStyle( this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );

            pcPieChart.PieOptions.label = new PieLabel { show = this.GetAttributeValue( "PieShowLabels" ).AsBooleanOrNull() ?? true };
            pcPieChart.PieOptions.label.formatter = @"
function labelFormatter(label, series) {
	return ""<div style='font-size:8pt; text-align:center; padding:2px; '>"" + label + ""<br/>"" + Math.round(series.percent) + ""%</div>"";
}
".Trim();
            pcPieChart.Legend.show = this.GetAttributeValue( "ShowLegend" ).AsBooleanOrNull();

            pcPieChart.PieOptions.innerRadius = this.GetAttributeValue( "PieInnerRadius" ).AsDoubleOrNull();

            lcLineChart.Visible = false;
            bcBarChart.Visible = false;
            pcPieChart.Visible = false;
            var chartType = this.GetAttributeValue( "ChartType" );
            if ( chartType == "Pie" )
            {
                pcPieChart.Visible = true;
            }
            else if ( chartType == "Bar" )
            {
                bcBarChart.Visible = true;
            }
            else
            {
                lcLineChart.Visible = true;
            }

            pnlDashboardTitle.Visible = !string.IsNullOrEmpty( this.Title );
            pnlDashboardSubtitle.Visible = !string.IsNullOrEmpty( this.Subtitle );
            lDashboardTitle.Text = this.Title;
            lDashboardSubtitle.Text = this.Subtitle;

            var sql = this.GetAttributeValue( "SQL" );

            if ( string.IsNullOrWhiteSpace( sql ) )
            {
                nbConfigurationWarning.Visible = true;
                nbConfigurationWarning.Text = "SQL needs to be configured in block settings";
            }
            else
            {
                nbConfigurationWarning.Visible = false;
            }

            if ( PageParameter( "GetChartData" ).AsBoolean() && ( PageParameter( "GetChartDataBlockId" ).AsInteger() == this.BlockId ) )
            {
                GetChartData();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class DynamicChartData : Rock.Chart.IChartData
        {
            /// <summary>
            /// Gets the date time stamp.
            /// </summary>
            /// <value>
            /// The date time stamp.
            /// </value>
            public long DateTimeStamp { get; set; }

            /// <summary>
            /// Gets the y value (for Line and Bar Charts)
            /// </summary>
            /// <value>
            /// The y value.
            /// </value>
            public decimal? YValue { get; set; }

            /// <summary>
            /// Gets or sets the metric title (for pie charts)
            /// </summary>
            /// <value>
            /// The metric title.
            /// </value>
            public string MetricTitle { get; set; }

            /// <summary>
            /// Gets the y value (for pie charts)
            /// </summary>
            /// <value>
            /// The y value.
            /// </value>
            public decimal? YValueTotal { get; set; }

            /// <summary>
            /// Gets the series identifier.
            /// </summary>
            /// <value>
            /// The series identifier.
            /// </value>
            public string SeriesId { get; set; }
        }

        /// <summary>
        /// Gets the chart data (ajax call from Chart)
        /// </summary>
        private void GetChartData()
        {
            try
            {
                var sql = this.GetAttributeValue( "SQL" );

                if ( string.IsNullOrWhiteSpace( sql ) )
                {
                    //
                }
                else
                {
                    var mergeFields = GetDynamicDataMergeFields();
                    sql = sql.ResolveMergeFields( mergeFields );
                    
                    DataSet dataSet = DbService.GetDataSet( sql, System.Data.CommandType.Text, null );
                    List<DynamicChartData> chartDataList = new List<DynamicChartData>();
                    foreach ( var row in dataSet.Tables[0].Rows.OfType<DataRow>() )
                    {
                        var chartData = new DynamicChartData();

                        if ( row.Table.Columns.Contains( "SeriesID" ) )
                        {
                            chartData.SeriesId = Convert.ToString( row["SeriesID"] );
                        }

                        if ( row.Table.Columns.Contains( "YValue" ) )
                        {
                            chartData.YValue = Convert.ToDecimal( row["YValue"] );
                        }

                        if ( row.Table.Columns.Contains( "MetricTitle" ) )
                        {
                            chartData.MetricTitle = Convert.ToString( row["MetricTitle"] );
                        }
                        else
                        {
                            chartData.MetricTitle = chartData.SeriesId;
                        }

                        if ( row.Table.Columns.Contains( "YValueTotal" ) )
                        {
                            chartData.YValueTotal = Convert.ToDecimal( row["YValueTotal"] );
                        }
                        else
                        {
                            chartData.YValueTotal = chartData.YValue;
                        }

                        if ( row.Table.Columns.Contains( "DateTime" ) )
                        {
                            chartData.DateTimeStamp = ( row["DateTime"] as DateTime? ).Value.ToJavascriptMilliseconds();
                        }

                        chartDataList.Add( chartData );
                    }

                    chartDataList = chartDataList.OrderBy( a => a.SeriesId ).ThenBy( a => a.DateTimeStamp ).ToList();

                    Response.Clear();
                    Response.Write( chartDataList.ToJson() );
                    Response.End();
                }
            }
            catch ( Exception ex )
            {
                LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // reload the full page since controls are dynamically created based on block settings
            NavigateToPage( this.CurrentPageReference );
        }

        /// <summary>
        /// Gets the dynamic data merge fields.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDynamicDataMergeFields()
        {
            var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            if ( CurrentPerson != null )
            {
                mergeFields.Add( "CurrentPerson", CurrentPerson );
            }

            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
            mergeFields.Add( "Campuses", CampusCache.All() );
            mergeFields.Add( "PageParameter", PageParameters() );
            mergeFields.Add( "CurrentPage", this.PageCache );

            var contextObjects = new Dictionary<string, object>();
            foreach ( var contextEntityType in RockPage.GetContextEntityTypes() )
            {
                var contextEntity = RockPage.GetCurrentContext( contextEntityType );
                if ( contextEntity != null && contextEntity is DotLiquid.ILiquidizable )
                {
                    var type = Type.GetType( contextEntityType.AssemblyName ?? contextEntityType.Name );
                    if ( type != null )
                    {
                        contextObjects.Add( type.Name, contextEntity );
                    }
                }
            }

            if ( contextObjects.Any() )
            {
                mergeFields.Add( "Context", contextObjects );
            }

            return mergeFields;
        }
    }
}
