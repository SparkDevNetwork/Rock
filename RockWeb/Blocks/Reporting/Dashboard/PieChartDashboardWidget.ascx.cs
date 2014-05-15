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
    [DisplayName( "Pie Chart" )]
    [Category( "Dashboard" )]
    [Description( "DashboardWidget using flotcharts" )]
    public partial class PieChartDashboardWidget : DashboardWidget
    {
        /// <summary>
        /// Loads the chart.
        /// </summary>
        public override void LoadChart()
        {
            //pcExample.StartDate = new DateTime( 2013, 1, 1 );
            //pcExample.EndDate = new DateTime( 2014, 1, 1 );
            pcExample.MetricValueType = this.MetricValueType;
            pcExample.MetricId = this.MetricId;
            pcExample.EntityId = this.EntityId;
            pcExample.Title = this.Title;
            pcExample.Subtitle = this.Subtitle;
            pcExample.CombineValues = this.CombineValues;

            pcExample.ShowTooltip = true;
            //pcExample.ShowDebug = true;
            pcExample.Options.SetChartStyle( this.ChartStyle );

            string debug = this.ChartStyle.ToJson( false );
            
            nbMetricWarning.Visible = !this.MetricId.HasValue;
        }
    }
}