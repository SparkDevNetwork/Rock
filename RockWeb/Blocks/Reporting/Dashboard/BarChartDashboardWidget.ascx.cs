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
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            flotChart.Options.xaxis = new AxisOptions { mode = AxisMode.categories, tickLength = 0 };
            flotChart.Options.series.bars.barWidth = 0.6;
            flotChart.Options.series.bars.align = "center";
        }
    }
}