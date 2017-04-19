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
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// NOTE: Most of the logic for processing the Attributes is in Rock.Rest.MetricsController.GetHtmlForBlock
    /// </summary>
    /// <seealso cref="Rock.Reporting.Dashboard.DashboardWidget" />
    [DisplayName( "Liquid Dashboard Widget" )]
    [Category( "Reporting > Dashboard" )]
    [Description( "Dashboard Widget from Liquid using YTD metric values" )]
    [EntityField( "Series Partition", "Select the series partition entity (Campus, Group, etc) to be used to limit the metric values for the selected metrics.", "Either select a specific {0} or leave {0} blank to get it from the page context.", false, Key = "Entity", Order = 3 )]
    [MetricCategoriesField( "Metric", "Select the metric(s) to be made available to liquid", Key = "MetricCategories", Order = 4 )]
    [BooleanField( "Round Values", "Round Y values to the nearest whole number. For example, display 25.00 as 25.", true, Order = 5 )]
    [CodeEditorField( "Liquid Template", "The text (or html) to display as a dashboard widget", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, Order = 6, DefaultValue =
@"
{% for metric in Metrics %}
    <h1>{{ metric.Title }}</h1>
    <h4>{{ metric.Subtitle }}</h4>
    <p>{{ metric.Description }}</p>
    <div class='row'>    
        <div class='col-md-6'>
            {{ metric.LastValueDate | Date: 'MMM' }}
              <span style='font-size:40px'>{{ metric.LastValue }}</span>
            <p>
                YTD {{ metric.CumulativeValue }} {% if metric.GoalValue %} GOAL {{ metric.GoalValue }} {% endif %}
            </p>
        </div>
        <div class='col-md-6'>
            <i class='{{ metric.IconCssClass }} fa-5x'></i>
        </div>
    </div>
{% endfor %}
" )]

    public partial class LiquidDashboardWidget : DashboardWidget
    {
        /// <summary>
        /// Gets the rest URL.
        /// </summary>
        /// <value>
        /// The rest URL.
        /// </value>
        public string RestUrl
        {
            get
            {
                string result = ResolveUrl( "~/api/Metrics/GetHtmlForBlock/" ) + this.BlockId.ToString();
                string[] entityValues = ( GetAttributeValue( "Entity" ) ?? string.Empty ).Split( '|' );
                if ( entityValues.Length == 2 && !string.IsNullOrEmpty( entityValues[1] ) )
                {
                    var entityType = EntityTypeCache.Read( entityValues[0].AsGuid() );
                    if ( entityType != null )
                    {
                        result += string.Format( "?entityTypeId={0}", entityType.Id );
                        int? entityId = entityValues[1].AsIntegerOrNull();
                        if ( entityId.HasValue )
                        {
                            result += string.Format( "&entityId={0}", entityId );
                        }
                    }
                }
                else
                {
                    if ( this.ContextEntity() != null )
                    {
                        var entityType = EntityTypeCache.Read( this.ContextEntity().GetType(), false );
                        if ( entityType != null )
                        {
                            result += string.Format( "?entityTypeId={0}&entityId={1}", entityType.Id, this.ContextEntity().Id );
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( System.EventArgs e )
        {
            base.OnLoad( e );
            pnlDashboardTitle.Visible = !string.IsNullOrEmpty( this.Title );
            pnlDashboardSubtitle.Visible = !string.IsNullOrEmpty( this.Subtitle );
            lDashboardTitle.Text = this.Title;
            lDashboardSubtitle.Text = this.Subtitle;
        }
    }
}