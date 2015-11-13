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
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Dynamic Chart" )]
    [Category( "Reporting" )]
    [Description( "Block to display a chart using SQL as the chart datasource" )]

    [CodeEditorField( "SQL", @"The SQL for the datasource. Output columns must be [SeriesID], [DateTime], [YValue]. Example: 
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
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", Order = 3 )]

    [BooleanField( "Show Legend", "", true, Order = 7 )]
    [CustomDropdownListField( "Legend Position", "Select the position of the Legend (corner)", "ne,nw,se,sw", false, "ne", Order = 8 )]
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
            pageReference.QueryString.Add( "TimeStamp", RockDateTime.Now.ToJavascriptMilliseconds().ToString() );
            lcLineChart.DataSourceUrl = pageReference.BuildUrl();
            lcLineChart.ChartHeight = this.GetAttributeValue( "ChartHeight" ).AsIntegerOrNull() ?? 200;
            lcLineChart.Options.SetChartStyle( this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull() );
            lcLineChart.Options.legend = lcLineChart.Options.legend ?? new Legend();
            lcLineChart.Options.legend.show = this.GetAttributeValue( "ShowLegend" ).AsBooleanOrNull();
            lcLineChart.Options.legend.position = this.GetAttributeValue( "LegendPosition" );

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

            if ( PageParameter( "GetChartData" ).AsBoolean() )
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
            /// Gets the y value.
            /// </summary>
            /// <value>
            /// The y value.
            /// </value>
            public decimal? YValue { get; set; }

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
            var sql = this.GetAttributeValue( "SQL" );

            if ( string.IsNullOrWhiteSpace( sql ) )
            {
                //
            }
            else
            {
                DataSet dataSet = DbService.GetDataSet( sql, System.Data.CommandType.Text, null );
                List<DynamicChartData> chartDataList = new List<DynamicChartData>();
                foreach ( var row in dataSet.Tables[0].Rows.OfType<DataRow>() )
                {
                    var chartData = new DynamicChartData
                    {
                        SeriesId = Convert.ToString( row["SeriesID"] ),
                        DateTimeStamp = ( row["DateTime"] as DateTime? ).Value.ToJavascriptMilliseconds(),
                        YValue = Convert.ToDecimal( row["YValue"] )
                    };

                    chartDataList.Add( chartData );
                }

                chartDataList = chartDataList.OrderBy( a => a.SeriesId ).ThenBy( a => a.DateTimeStamp ).ToList();

                Response.Clear();
                Response.Write( chartDataList.ToJson() );
                Response.End();
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
    }
}
