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

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Flot DashboardWidget" )]
    [Category( "Dashboard" )]
    [Description( "DashboardWidget using flotcharts" )]
    public partial class FlotDashboardWidget : DashboardWidget
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/flot/jquery.flot.js" );
            RockPage.AddScriptLink( "~/Scripts/flot/jquery.flot.time.js" );
            RockPage.AddScriptLink( "~/Scripts/flot/jquery.flot.resize.js" );
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
                DateTime? startDate = new DateTime( 2013, 1, 1 );
                DateTime? endDate = new DateTime( 2014, 1, 1 );
                MetricValueType? metricValueType = MetricValueType.Measure;
                if (metricValueType.HasValue)
                {
                    hfRestUrlParams.Value = string.Format( "{0}?metricValueType={1}&", this.MetricId, metricValueType.ConvertToString() );
                }
                else
                {
                    hfRestUrlParams.Value = string.Format( "{0}?", this.MetricId );
                }

                hfRestUrlParams.Value += string.Format( "$filter=MetricValueDateTime ge DateTime'{0}' and MetricValueDateTime lt DateTime'{1}'", ( startDate ?? DateTime.MinValue ).ToString( "o" ), ( endDate ?? DateTime.MaxValue ).ToString( "o" ) );

                var metric = new MetricService( new RockContext() ).Get( this.MetricId ?? 0 );
                if (metric != null)
                {
                    lblMetricTitle.Text = metric.Title;
                    hfXAxisLabel.Value = metric.XAxisLabel;
                }

                nbMetricWarning.Visible = !this.MetricId.HasValue;
            }
        }
    }
}