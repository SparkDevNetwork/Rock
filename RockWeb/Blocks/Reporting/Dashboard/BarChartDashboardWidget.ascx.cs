﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.ComponentModel;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Bar Chart" )]
    [Category( "Reporting > Dashboard" )]
    [Description( "Bar Chart Dashboard Widget" )]
    public partial class BarChartDashboardWidget : LineBarPointsChartDashboardWidget
    {
        /// <summary>
        /// Gets the flot chart control.
        /// </summary>
        /// <value>
        /// The flot chart control.
        /// </value>
        public override FlotChart FlotChartControl
        {
            get { return bcChart; }
        }

        /// <summary>
        /// Gets the metric warning control.
        /// </summary>
        /// <value>
        /// The metric warning control.
        /// </value>
        public override Rock.Web.UI.Controls.NotificationBox MetricWarningControl
        {
            get { return nbMetricWarning; }
        }

        /// <summary>
        /// Loads the chart.
        /// </summary>
        public override void LoadChart()
        {
            base.LoadChart();
            pnlDashboardTitle.Visible = !string.IsNullOrEmpty( this.Title );
            pnlDashboardSubtitle.Visible = !string.IsNullOrEmpty( this.Subtitle );
            lDashboardTitle.Text = this.Title;
            lDashboardSubtitle.Text = this.Subtitle;
        }
    }
}