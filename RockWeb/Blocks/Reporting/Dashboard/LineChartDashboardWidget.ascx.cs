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
        /// Loads the chart.
        /// </summary>
        public override void LoadChart()
        {
            lcExample.StartDate = new DateTime( 2013, 1, 1 );
            lcExample.EndDate = new DateTime( 2014, 1, 1 );
            lcExample.MetricValueType = this.MetricValueType;
            lcExample.MetricId = this.MetricId;
            lcExample.EntityId = this.EntityId;
            lcExample.Title = this.Title;
            lcExample.Subtitle = this.Subtitle;
            lcExample.CombineValues = this.CombineValues;

            lcExample.ShowTooltip = true;
            
            lcExample.Options.SetChartStyle( this.ChartStyle );

            string debug = this.ChartStyle.ToJson( false );
            
            nbMetricWarning.Visible = !this.MetricId.HasValue;
        }
    }
}