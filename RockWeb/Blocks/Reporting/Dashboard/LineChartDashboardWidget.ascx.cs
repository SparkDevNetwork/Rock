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
using System.ComponentModel;
using System.Web.UI;

using Rock;
using Rock.Model;
using Rock.Reporting.Dashboard;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Line Chart" )]
    [Category( "Dashboard" )]
    [Description( "DashboardWidget using flotcharts" )]
    public partial class LineChartDashboardWidget : DashboardWidget
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                lcExample.StartDate = new DateTime( 2013, 1, 1 );
                lcExample.EndDate = new DateTime( 2014, 1, 1 );
                lcExample.MetricValueType = this.MetricValueType;
                lcExample.MetricId = this.MetricId;
                lcExample.EntityId = this.PageParameter( "EntityId" ).AsInteger();
                if ( lcExample.EntityId == null && this.ContextEntity() != null )
                {
                    lcExample.EntityId = this.ContextEntity().Id;
                }

                lcExample.Title = this.Title;
                lcExample.Subtitle = this.Subtitle;

                ChartTheme testTheme = new ChartTheme();
                testTheme.SeriesColors = new string[] { 
                    "#FF0000",
                    "#515151"
                };

                testTheme.GridBackgroundColorGradiant = new string[] { "#FFFFFF", "#EEEEEE" };
                testTheme.GridColor = "#101010";
                testTheme.XAxis = new AxisStyle { Color = "black", Font = new ThemeFont( "green", "consolas", 10 ) };
                testTheme.YAxis = new AxisStyle { Color = "gray", Font = new ThemeFont( "olive", "consolas", 10 ) };
                testTheme.FillOpacity = .1;
                //testTheme.FillColor = "rgba(25, 125, 54, 0.25)";

                lcExample.Options.SetTheme( testTheme );

                nbMetricWarning.Visible = !this.MetricId.HasValue;
            }
        }
    }
}