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
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Line Chart DashboardWidget" )]
    [Category( "Dashboard" )]
    [Description( "Line Chart dashboard widget for developers to use to start a new LineChartDashboardWidget block." )]
    public partial class LineChartDashboardWidget : DashboardWidget
    {
        #region Base Control Methods

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

            RockPage.AddScriptLink( "https://www.google.com/jsapi", false );
            RockPage.AddScriptLink( "~/Scripts/jquery.smartresize.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // Options for Chart
                var chartOptions = new ChartOptions();
                chartOptions.vAxis.title = this.Title;
                if ( !string.IsNullOrWhiteSpace( this.Subtitle ) )
                {
                    chartOptions.vAxis.title += Environment.NewLine + this.Subtitle;
                }

                chartOptions.vAxis.minValue = 0;
                chartOptions.vAxis.titleTextStyle = new
                {
                    color = "#515151",
                    italic = false
                };

                chartOptions.colors = new string[] { "#8498ab", "#a4b4c4", "#b9c7d5", "#c6d2df", "#d8e1ea" };
                chartOptions.hAxis = new
                {
                    textStyle = new
                    {
                        color = "#515151"
                    },
                    baselineColor = "#515151"
                };

                chartOptions.width = null;
                chartOptions.height = null;
                chartOptions.legend = new
                {
                    position = "bottom",
                    textStyle = new
                    {
                        color = "#515151"
                    }
                };

                chartOptions.backgroundColor = "transparent";

                hfOptions.Value = ( chartOptions as object ).ToJson();
                List<ColumnDefinition> columnDefinitions = new List<ColumnDefinition>();
                columnDefinitions.Add( new ColumnDefinition( "Date", ColumnDataType.date ) );
                columnDefinitions.Add( new ColumnDefinition( "Attendance", ColumnDataType.number ) );
                columnDefinitions.Add( new ChartTooltip() );
                hfColumns.Value = columnDefinitions.ToJson();

                // Data for Chart
                Guid attendanceMetricGuid = new Guid( "D4752628-DFC9-4681-ADB3-01936B8F38CA" );
                var qry = new MetricValueService( new RockContext() ).Queryable().Where( w => w.MetricValueDateTime.HasValue && w.Metric.Guid == attendanceMetricGuid );
                DateTime startDate = new DateTime( 2013, 1, 1 );
                DateTime endDate = new DateTime( 2014, 1, 1 );
                qry = qry.Where( a => a.MetricValueDateTime.Value >= startDate && a.MetricValueDateTime.Value < endDate );

                var dataList = qry.ToList().Select( a => new object[]
                    {
                        a.MetricValueDateTime,
                        a.YValue,
                        HttpUtility.JavaScriptStringEncode(a.Note)
                    } );

                hfDataTable.Value = JsonConvert.SerializeObject( dataList, new JsonSerializerSettings { Converters = new JsonConverter[] { new ChartDateTimeJsonConverter() } } );
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        #endregion

        #region Methods

        #endregion
    }
}