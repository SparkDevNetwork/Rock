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
using System.ComponentModel;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.Dashboard
{
    /// <summary>
    /// NOTE: Most of the logic for processing the Attributes is in Rock.Rest.MetricsController.GetHtmlForBlock
    /// </summary>
    [DisplayName( "NewSpring Dashboard Widget" )]
    [Category( "NewSpring" )]
    [Description( "Dashboard Widget from Liquid using YTD metric values" )]
    [EntityField( "Series Partition", "Select the series partition entity (Campus, Group, etc) to be used to limit the metric values for the selected metrics.", "Either select a specific {0} or leave {0} blank to get it from the page context.", Key = "Entity", Order = 3 )]
    [MetricCategoriesField( "Metric", "Select the metric(s) to be made available to liquid", Key = "MetricCategories", Order = 4 )]
    [BooleanField( "Round Values", "Round Y values to the nearest whole number. For example, display 25.00 as 25.", true, Order = 5 )]
    [CodeEditorField( "Liquid Template", "The text (or html) to display as a dashboard widget", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, Order = 6, DefaultValue =
@"
{% for metric in Metrics %}
    <h2 class='flush'>{{ metric.LastValue }}</h2>
{% endfor %}
" )]
    [BooleanField( "Enable Debug", "Outputs the object graph to help create your liquid syntax.", false, Order = 7 )]
    public partial class NewSpringDashboardWidget : DashboardWidget
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
                string[] entityValues = ( GetAttributeValue( "Entity" ) ?? "" ).Split( '|' );
                if ( entityValues.Length == 2 )
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