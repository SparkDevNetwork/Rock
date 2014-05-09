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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Reporting.Dashboard;
using System.Drawing;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Bar Chart" )]
    [Category( "Dashboard" )]
    [Description( "DashboardWidget using flotcharts" )]
    public partial class BarChartDashboardWidget : DashboardWidget
    {
        /// <summary>
        /// Loads the chart.
        /// </summary>
        public override void LoadChart()
        {
            bcExample.StartDate = new DateTime( 2013, 1, 1 );
            bcExample.EndDate = new DateTime( 2014, 1, 1 );
            bcExample.MetricValueType = this.MetricValueType;
            bcExample.MetricId = this.MetricId;
            bcExample.EntityId = this.PageParameter( "EntityId" ).AsInteger();
            if ( bcExample.EntityId == null && this.ContextEntity() != null )
            {
                bcExample.EntityId = this.ContextEntity().Id;
            }

            bcExample.Title = this.Title;
            bcExample.Subtitle = this.Subtitle;

            bcExample.Options.SetTheme( this.ChartTheme );

            nbMetricWarning.Visible = !this.MetricId.HasValue;
        }
    }
}