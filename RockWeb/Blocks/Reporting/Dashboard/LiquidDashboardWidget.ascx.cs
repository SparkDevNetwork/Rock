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
using System.Web.UI;
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Liquid Dashboard Widget" )]
    [Category( "Dashboard" )]
    [Description( "DashboardWidget from Liquid using YTD metric values" )]
    [MetricCategoriesField( "Metric", "Select the metric(s) to be made available to liquid", Key = "MetricCategories", Order = 3 )]
    [CodeEditorField( "Display Text", "The text (or html) to display as a dashboard widget", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, Order = 4, DefaultValue =
@"
{% for metric in Metrics %}
    <h1>{{ metric.Title }}</h1>
    <h4>{{ metric.Subtitle }}</h4>
    <p>{{ metric.Description }}</p>
    <div>    
    <i class='{{ metric.IconCssClass }}'></i>
    </div>
{% endfor %}
" )]
    
    [BooleanField( "Enable Debug", "Outputs the object graph to help create your liquid syntax.", false, Order = 5 )]
    public partial class LiquidDashboardWidget : DashboardWidget
    {
    }
}