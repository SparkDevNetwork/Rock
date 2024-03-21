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
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Lava.RockLiquid;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Commands
{
    /// <summary>
    /// Test for shortcodes that are defined and implemented as parameterized Lava templates rather than code components.
    /// </summary>
    [TestClass]
    public class ShortcodeTemplateTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void ShortcodeBlock_WithChildItems_EmitsCorrectHtml()
        {
            var shortcodeTemplate = @"
Parameter 1: {{ parameter1 }}
Parameter 2: {{ parameter2 }}
Items:
{%- for item in items -%}
{{ item.title }} - {{ item.content }}
{%- endfor -%}
";

            // Create a new test shortcode.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Block;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "shortcodetest";
            shortcodeDefinition.Parameters = new Dictionary<string, string> { { "parameter1", "value1" }, { "parameter2", "value2" } };

            var input = @"
{[ shortcodetest ]}

    [[ item title:'Panel 1' ]]
        Panel 1 content.
    [[ enditem ]]
    
    [[ item title:'Panel 2' ]]
        Panel 2 content.
    [[ enditem ]]
    
    [[ item title:'Panel 3' ]]
        Panel 3 content.
    [[ enditem ]]

{[ endshortcodetest ]}
";

            var expectedOutput = @"
Parameter 1: value1
Parameter 2: value2
Items:
Panel 1 - Panel 1 content.
Panel 2 - Panel 2 content.
Panel 3 - Panel 3 content.
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // The RockLiquid engine does not support dynamic shortcode definitions.
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input );
            } );
        }

        [TestMethod]
        public void ShortcodeBlock_WithEntityCommandEnabledAndEmbeddedEntityCommand_EmitsCorrectHtml()
        {
            var shortcodeTemplate = @"
{% for item in items %}
<Item>
{{ item.title }} --- {{ item.content }}
</Item>
{% endfor %}
";

            // Create a new test shortcode.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Block;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "shortcodetest";
            shortcodeDefinition.Parameters = new Dictionary<string, string> { { "title", "(unnamed)" } };
            shortcodeDefinition.EnabledLavaCommands = new List<string> { "RockEntity" };

            var input = @"
{[ shortcodetest ]}
    {% definedvalue where:'DefinedTypeId == 30' sort:'Order' %}
        {% for dvi in definedvalueItems %}
            [[ item title:'{{ dvi.Value }}' ]]
                Id: {{dvi.Id}} - Guid: {{ dvi.Guid }}
            [[ enditem ]]
        {% endfor %}
    {% enddefinedvalue %}
{[ endshortcodetest ]}
";

            var expectedOutput = @"
<Item>
Infant --- Id: 138 - Guid: c4550426-ed87-4cb0-957e-c6e0bc96080f
</Item>
<Item>
Crawling or Walking --- Id: 139 - Guid: f78d64d3-6ba1-4eca-a9ec-058fbdf8e586
</Item>
<Item>
Potty Trained --- Id: 141 - Guid: e6905502-4c23-4879-a60f-8c4ceb3ee2e9
</Item>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    TestHelper.DebugWriteRenderResult( engine, "(Not Implemented)", "(Not Implemented)" );
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input );
            } );

        }

        /// <summary>
        /// This test verifies a workaround for an issue with the Fluid parser where repeatedly processing the same shortcode
        /// fails because of some persistent state that is maintained within the Fluid framework.
        /// </summary>
        [TestMethod]
        public void ShortcodeBlock_ProcessingMultipleShortcodesInSeries_ProducesIdenticalResult()
        {
            ShortcodeBlock_WithParameters_CanResolveParameters();

            // This subsequent iteration of the same test will fail with an error if the issue with the Fluid framework is not fixed.
            ShortcodeBlock_WithParameters_CanResolveParameters();
        }

        [TestMethod]
        public void ShortcodeBlock_WithParameters_CanResolveParameters()
        {
            var shortcodeTemplate = @"
Font Name: {{ fontname }}
Font Size: {{ fontsize }}
Font Bold: {{ fontbold }}
";

            // Create a new test shortcode.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Block;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "shortcodetest";
            shortcodeDefinition.Parameters = new Dictionary<string, string> { { "speed", "10" } };

            var input = @"
{[ shortcodetest fontname:'Arial' fontsize:'14' fontbold:'true' ]}
{[ endshortcodetest ]}
";

            var expectedOutput = @"
Font Name: Arial
Font Size: 14
Font Bold: true
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    TestHelper.DebugWriteRenderResult( engine, "(Not Implemented)", "(Not Implemented)" );
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input );
            } );
        }

        /// <summary>
        /// This test exists to document an observed behavior in Lava.
        /// for more information, refer to https://github.com/SparkDevNetwork/Rock/issues/3605
        /// </summary>
        [TestMethod]
        public void ShortcodeBlock_WithParameterVariableContainingQuotes_CanResolveParameters()
        {
            var shortcodeTemplate = @"
Parameter 1: {{ parameterstring }}
";

            // Create a new test shortcode.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Block;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "shortcodeparametertest";
            shortcodeDefinition.Parameters = new Dictionary<string, string> { { "parameterstring", "(default)" } };

            var input = @"
{[ shortcodeparametertest parameterstring:parameterStringValue ]}
{[ endshortcodeparametertest ]}
";

            var expectedOutput = @"
Parameter 1: Testing 'single' quotes...
";

            var mergeFields = new Dictionary<string, object> { { "parameterStringValue", "Testing 'single' quotes..." } };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    // Unfortunately, there is no way of testing dynamically-defined shortcodes with RockLiquid.
                    TestHelper.DebugWriteRenderResult( engine, "(Not Implemented)", "(Not Implemented)" );
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                var options = new LavaTestRenderOptions { MergeFields = mergeFields };

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
            } );
        }

        [TestMethod]
        public void ShortcodeBlock_RepeatedShortcodeBlock_ProducesExpectedOutput()
        {
            var shortcodeTemplate = @"
Font Name: {{ fontname }}
Font Size: {{ fontsize }}
Font Bold: {{ fontbold }}
";

            // Create a new test shortcode.
            var shortcode1 = new DynamicShortcodeDefinition();

            shortcode1.ElementType = LavaShortcodeTypeSpecifier.Block;
            shortcode1.TemplateMarkup = shortcodeTemplate;
            shortcode1.Name = "shortcodetest1";

            var shortcode2 = new DynamicShortcodeDefinition();

            shortcode2.ElementType = LavaShortcodeTypeSpecifier.Block;
            shortcode2.TemplateMarkup = shortcodeTemplate;
            shortcode2.Name = "shortcodetest2";

            var input = @"
{[ shortcodetest1 fontname:'Arial' fontsize:'14' fontbold:'true' ]}
{[ endshortcodetest1 ]}
";

            var expectedOutput = @"
Font Name: Arial
Font Size: 14
Font Bold: true
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // The RockLiquid engine does not support dynamic shortcode definitions.
                if ( engine.GetType() == typeof ( RockLiquidEngine ) )
                {
                    return;
                }

                engine.RegisterShortcode( shortcode1.Name, ( shortcodeName ) => { return shortcode1; } );
                engine.RegisterShortcode( shortcode2.Name, ( shortcodeName ) => { return shortcode2; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input );

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 30 };

                Parallel.For( 0, 1000, parallelOptions, ( x ) => TestHelper.AssertTemplateOutput( engine, expectedOutput, input ) );
            } );
        }

        /// <summary>
        /// Verifies the precendence of parsing a raw tag before a shortcode.
        /// </summary>
        [TestMethod]
        public void ShortcodeBlock_EmbeddedInRawTag_IsRenderedAsLiteralText()
        {
            var input = @"
    {% raw %}
        {[ panel title:'Example' ]}
            This is a sample panel.
        {[ endpanel ]}
    {% endraw %}
";

            var expectedOutput = @"
{[ panel title:'Example' ]}
    This is a sample panel.
{[ endpanel ]}
";

            expectedOutput = expectedOutput.Replace( "`", @"""" );
            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ShortcodeBlock_WithInvalidShortcodeInRawTag_IsRenderedAsLiteralText()
        {
            var input = @"
{[ panel title:'Important Stuff' icon:'fa fa-star' ]}
    This is a super simple panel.
    {% raw %}
        This is some literal text containing an invalid shortcode: {[ panel title:'Example' ]}
    {% endraw %}
{[ endpanel ]}
";

            var expectedOutput = @"
<div class=`panel panel-default`>
    <div class=`panel-heading`>
        <h3 class=`panel-title`><i class=`fa fa-star`></i>Important Stuff</h3>
    </div>
    <div class=`panel-body`>
       This is a super simple panel.
       This is some literal text containing an invalid shortcode: {[ panel title:'Example' ]}
    </div>  
</div>
";
            expectedOutput = expectedOutput.Replace( "`", @"""" );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ShortcodeBlock_OpenShortcodeInRawBlock_IsRenderedAsLiteralText()
        {
            var input = @"
{% raw %}
    This is some literal text containing an invalid shortcode: {[ panel title:'Example' ]}
{% endraw %}
";

            var expectedOutput = @"
This is some literal text containing an invalid shortcode: {[ panel title:'Example' ]}
";
            expectedOutput = expectedOutput.Replace( "`", @"""" );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #region Accordion

        [TestMethod]
        public void AccordionShortcodeBlock_DefaultOptions_EmitsCorrectHtml()
        {
            var input = @"
{[ accordion ]}

    [[ item title:'Lorem Ipsum' ]]
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut pretium tortor et orci ornare 
        tincidunt. In hac habitasse platea dictumst. Aliquam blandit dictum fringilla. 
    [[ enditem ]]
    
    [[ item title:'In Commodo Dolor' ]]
        In commodo dolor vel ante porttitor tempor. Ut ac convallis mauris. Sed viverra magna nulla, quis 
        elementum diam ullamcorper et. 
    [[ enditem ]]
    
    [[ item title:'Vivamus Sollicitudin' ]]
        Vivamus sollicitudin, leo quis pulvinar venenatis, lorem sem aliquet nibh, sit amet condimentum
        ligula ex a risus. Curabitur condimentum enim elit, nec auctor massa interdum in.
    [[ enditem ]]

{[ endaccordion ]}
";

            var expectedOutput = @"
<div class=``panel-group`` id=``accordion-id-<<guid>>`` role=``tablist`` aria-multiselectable=``true``>
      
      <div class=``panel panel-default``>
        <div class=``panel-heading`` role=``tab`` id=``heading1-id-<<guid>>``>
          <h4 class=``panel-title``>
            <a role=``button`` data-toggle=``collapse`` data-parent=``#accordion-id-<<guid>>`` href=``#collapse1-id-<<guid>>`` aria-expanded=``true`` aria-controls=``collapse1``>
              Lorem Ipsum
            </a>
          </h4>
        </div>
        <div id=``collapse1-id-<<guid>>`` class=``panel-collapse collapse in`` role=``tabpanel`` aria-labelledby=``heading1-id-<<guid>>``>
          <div class=``panel-body``>
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut pretium tortor et orci ornare 
        tincidunt. In hac habitasse platea dictumst. Aliquam blandit dictum fringilla.
          </div>
        </div>
      </div>
      
      <div class=``panel panel-default``>
        <div class=``panel-heading`` role=``tab`` id=``heading2-id-<<guid>>``>
          <h4 class=``panel-title``>
            <a role=``button`` data-toggle=``collapse`` data-parent=``#accordion-id-<<guid>>`` href=``#collapse2-id-<<guid>>`` aria-expanded=``false`` aria-controls=``collapse2``>
              In Commodo Dolor
            </a>
          </h4>
        </div>
        <div id=``collapse2-id-<<guid>>`` class=``panel-collapse collapse`` role=``tabpanel`` aria-labelledby=``heading2-id-<<guid>>``>
          <div class=``panel-body``>
            In commodo dolor vel ante porttitor tempor. Ut ac convallis mauris. Sed viverra magna nulla, quis 
        elementum diam ullamcorper et.
          </div>
        </div>
      </div>
      
      <div class=``panel panel-default``>
        <div class=``panel-heading`` role=``tab`` id=``heading3-id-<<guid>>``>
          <h4 class=``panel-title``>
            <a role=``button`` data-toggle=``collapse`` data-parent=``#accordion-id-<<guid>>`` href=``#collapse3-id-<<guid>>`` aria-expanded=``false`` aria-controls=``collapse3``>
              Vivamus Sollicitudin
            </a>
          </h4>
        </div>
        <div id=``collapse3-id-<<guid>>`` class=``panel-collapse collapse`` role=``tabpanel`` aria-labelledby=``heading3-id-<<guid>>``>
          <div class=``panel-body``>
            Vivamus sollicitudin, leo quis pulvinar venenatis, lorem sem aliquet nibh, sit amet condimentum
        ligula ex a risus. Curabitur condimentum enim elit, nec auctor massa interdum in.
          </div>
        </div>
      </div>
</div>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region Chart

        [TestMethod]
        public void ChartShortcode_BarChartWithDefaultOptions_EmitsCorrectHtml()
        {
            var input = @"
{[ chart type:'bar' ]}
    [[ dataitem label:'Small Groups' value:'45' ]] [[ enddataitem ]]
    [[ dataitem label:'Serving Groups' value:'38' ]] [[ enddataitem ]]
    [[ dataitem label:'General Groups' value:'34' ]] [[ enddataitem ]]
    [[ dataitem label:'Fundraising Groups' value:'12' ]] [[ enddataitem ]]
{[ endchart ]}
";

            var expectedOutput = @"
<script src='~/Scripts/moment.min.js' type='text/javascript'></script>
<script src='~/Scripts/Chartjs/Chart.min.js' type='text/javascript'></script>
              
<div class=``chart-container`` style=``position: relative; height:400px; width:100%``>
    <canvas id=``chart-id-<<guid>>``></canvas>
</div>

<script>

var options = {
  maintainAspectRatio: false,
    legend: {
        position: 'bottom',
        display: false
    },
    tooltips: {
        enabled: true,
        backgroundColor: '#000',
        bodyFontColor: '#fff',
        titleFontColor: '#fff',
        callbacks: {
            label: function(tooltipItem,data) { returnIntl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]); }
        }
    },
    scales: {
        xAxes:[ {
            display: true,
        } ],
        yAxes:[ {
            display: true,
            ticks: {
                callback: function(label,index,labels) { returnIntl.NumberFormat().format(label); },
            },
        } ]
    }
};
var data = {
    labels: [``Small Groups``, ``Serving Groups``, ``General Groups``, ``Fundraising Groups``],
    datasets: [{
          fill: false, 
          backgroundColor: 'rgba(5,155,255,.6)',
          borderColor: '#059BFF',
          borderWidth: 0,
          pointRadius: 3,
          pointBackgroundColor: '#059BFF',
          pointBorderColor: '#059BFF',
          pointBorderWidth: 0,
          pointHoverBackgroundColor: 'rgba(5,155,255,.6)',
          pointHoverBorderColor: 'rgba(5,155,255,.6)',
          pointHoverRadius: '3',
          data: [45,38,34,12],
      }
      ],
    borderWidth: 0
};

Chart.defaults.global.defaultFontColor = '#777';
Chart.defaults.global.defaultFontFamily = ``sans-serif``;

var ctx = document.getElementById('chart-id-<<guid>>').getContext('2d');
var chart = new Chart(ctx, {
    type: 'bar',
    data: data,
    options: options
});

</script>";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void ChartShortcode_PieChartWithDefaultOptions_EmitsCorrectHtml()
        {
            var input = @"
{[ chart type:'pie' ]}
    [[ dataitem label:'Small Groups' value:'45' ]] [[ enddataitem ]]
    [[ dataitem label:'Serving Groups' value:'38' ]] [[ enddataitem ]]
    [[ dataitem label:'General Groups' value:'34' ]] [[ enddataitem ]]
    [[ dataitem label:'Fundraising Groups' value:'12' ]] [[ enddataitem ]]
{[ endchart ]}
";

            var expectedOutput = @"
<script src='~/Scripts/moment.min.js' type='text/javascript'></script>
<script src='~/Scripts/Chartjs/Chart.min.js' type='text/javascript'></script>

<div class=``chart-container`` style=``position: relative; height:400px; width:100%``>
    <canvas id=``chart-id-<<guid>>``></canvas>
</div>

<script>
var options = {
  maintainAspectRatio: false,
    legend: {
        position: 'bottom',
        display: false
    },
    tooltips: {
        enabled: true,
        backgroundColor: '#000',
        bodyFontColor: '#fff',
        titleFontColor: '#fff',
        callbacks: {
            label: function(tooltipItem,data) { return data.labels[tooltipItem.index] + ``:`` + Intl.NumberFormat().format( data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index] ); }
        }
    }
};
var data = {
    labels: [``Small Groups``, ``Serving Groups``, ``General Groups``, ``Fundraising Groups``],
    datasets: [{
          fill: false,
          backgroundColor: 'rgba(5,155,255,.6)',
          borderColor: '#059BFF',
          borderWidth: 0,
          pointRadius: 3,
          pointBackgroundColor: '#059BFF',
          pointBorderColor: '#059BFF',
          pointBorderWidth: 0,
          pointHoverBackgroundColor: 'rgba(5,155,255,.6)',
          pointHoverBorderColor: 'rgba(5,155,255,.6)',
          pointHoverRadius: '3',
          data: [45,38,34,12],
      }
      ],
    borderWidth: 0
};

Chart.defaults.global.defaultFontColor = '#777';
Chart.defaults.global.defaultFontFamily = ``sans-serif``;

var ctx = document.getElementById('chart-id-<<guid>>').getContext('2d');
var chart = new Chart(ctx, {
    type: 'pie',
    data: data,
    options: options
});
</script>";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [TestMethod]
        public void ChartShortcode_EmbeddedInPanelShortcode_RendersCorrectly()
        {
            var input = @"
<div class=`col-md-6`>
    {[ panel title:'<i class=`fa fa-facebook`></i> Facebook Fans' ]}
        {[ chart chartheight:'300' type:'line' xaxistype:'time' fillcolor:'rgba(75, 169, 223, .25)' bordercolor:'#4ba9df' pointcolor:'#4ba9df' pointbordercolor:'#fff' pointhovercolor:'#fff' pointhoverbordercolor:'#4ba9df' filllinearea:'true' pointradius:'4' pointhoverradius:'4' borderwidth:'2' ]}
            {% for item in facebookFans %}
                [[ dataitem label:'{{ item.MetricValueDateTime }}' value:'{{ item.YValue }}' ]] [[ enddataitem ]]
            {% endfor %}
        {[ endchart ]}
    {[ endpanel ]}
</div>
";

            var expectedOutput = @"
<div class=`col-md-6`>
  <div class=`panel panel-default`>
    <div class=`panel-heading`>
      <h3 class=`panel-title`><i class=`fa fa-facebook`></i> Facebook Fans</h3>
    </div>
    <div class=`panel-body`>
      <script src='~/Scripts/moment.min.js' type='text/javascript'></script>

      <script
        src='~/Scripts/Chartjs/Chart.min.js'
        type='text/javascript'
      ></script>

      <div class=`alert alert-warning`>
        When using datasets you must provide labels on the shortcode to define
        each unit of measure. {[ chart labels:'Red, Green, Blue' ... ]}
      </div>

      <div
        class=`chart-container`
        style=`position: relative; height: 300; width: 100%`
      >
        <canvas id=`chart-id-<<guid>>`></canvas>
      </div>

      <script>
        var options = {
          maintainAspectRatio: false,
          legend: {
            position: 'bottom',
            display: false
          },
          tooltips: {
            enabled: true,

            backgroundColor: '#000',
            bodyFontColor: '#fff',
            titleFontColor: '#fff',

            callbacks: {
              label: function (tooltipItem, data) {
                return Intl.NumberFormat().format(
                  data.datasets[tooltipItem.datasetIndex].data[
                    tooltipItem.index
                  ]
                );
              }
            }
          },

          scales: {
            xAxes: [
              {
                type: `time`,
                display: true,
                scaleLabel: {
                  display: true,
                  labelString: 'Date'
                }
              }
            ],
            yAxes: [
              {
                display: true,

                ticks: {
                  callback: function (label, index, labels) {
                    return Intl.NumberFormat().format(label);
                  },
                }
              }
            ]
          }
        };
        var data = {
          labels: [],
          datasets: [],
          borderWidth: 2
        };

        Chart.defaults.global.defaultFontColor = '#777';
        Chart.defaults.global.defaultFontFamily = `sans-serif`;

        var ctx = document
          .getElementById('chart-id-<<guid>>')
          .getContext('2d');
        var chart = new Chart(ctx, {
          type: 'line',
          data: data,
          options: options
        });
      </script>
    </div>
  </div>
</div>
";

            input = input.Replace( "`", @"""" );
            expectedOutput = expectedOutput.Replace( "`", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" }, IgnoreWhiteSpace = true };
            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region Easy Pie Chart

        [TestMethod]
        [Ignore( "Fix required. Fluid produces a decimal result for '{%- assign itemtrackwidth = chartwidth | DividedBy:8.5,0 -%}' rather than int,  which causes this test to fail for Fluid." )]
        public void EasyPieChartShortcodeTag_SmallChart_EmitsCorrectHtml()
        {
            var input = @"
{[ easypie value:'25' scalelinelength:'0' chartwidth:'50' ]} {[ endeasypie ]}
";

            var expectedOutput = @"
<script src='https://cdnjs.cloudflare.com/ajax/libs/easy-pie-chart/2.1.6/jquery.easypiechart.min.js' type='text/javascript'></script>

<script>(function(){
  
$( document ).ready(function() {
    $(``.js-easy-pie-chart``).each(function() {
        var e = $(this)
          , t = e.data(``color``) || e.css(``color``)
          , a = e.data(``trackcolor``) || ``rgba(0,0,0,0.04)``
          , n = parseInt(e.data(``piesize``)) || 50
          , i = e.data(``scalecolor``)
          , r = parseInt(e.data(``scalelinelength``)) || 0
          , o = parseInt(e.data(``trackwidth``)) || parseInt(n / 8.5)
          , s = e.data(``linecap``) || ``butt``
          , x = e.data(``animateduration``) || 1500;
        e.easyPieChart({
            size: n,
            barColor: t,
            trackColor: a,
            scaleColor: i,
            scaleLength: r,
            lineCap: s,
            lineWidth: o,
            animate: {
                duration: x,
                enabled: !0
            },
            onStep: function(e, t, a) {
                $(this.el).find(``.js-percent``).text(Math.round(a))
            }
        }),
        e = null
    })
});

})();</script>

<style>
.easy-pie-chart {
  position: relative;
  display: -webkit-inline-box;
  display: -ms-inline-flexbox;
  display: inline-flex;
  -ms-flex-align: center;
  -ms-flex-pack: center;
  align-items: center;
  justify-content: center;
  -webkit-box-pack: center;
  -webkit-box-align: center;
  text-align: center;
}

.easy-pie-contents {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  -ms-flex-align: center;
  -ms-flex-direction: column;
  flex-direction: column;
  -ms-flex-pack: center;
  align-items: center;
  justify-content: center;
  line-height: 1.2;
  -webkit-box-align: center;
  -webkit-box-pack: center;
  -webkit-box-orient: vertical;
  -webkit-box-direction: normal;
}

.easy-pie-contents .chart-label {
  opacity: .7;
}
</style>

<div class=``easy-pie-chart id-<<guid>>  js-easy-pie-chart`` data-percent=``25`` data-piesize=``50`` data-scalelinelength=``0`` data-scalecolor=#dfe0e0 data-trackwidth=``6`` data-linecap=``butt`` data-animateduration=``1500`` style=``color:#ee7625``>
  <div class=``easy-pie-contents id-<<guid>>-contents``>
      <style>
      .id-<<guid>>-contents > img {
          border-radius: 50%;
          max-width: 26px;
      }
      </style>
      
<span class=``chart-label small`` ></span>

  </div>
</div>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region GoogleMap

        [TestMethod]
        public void GoogleStaticMapShortcode_SinglePoint_EmitsCorrectHtml()
        {
            var input = @"
{[ googlemap ]}
    [[ marker location:'33.640705,-112.280198' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

            var expectedOutput = @"
<div class=``alert alert-warning``>
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>

<script src='https://maps.googleapis.com/maps/api/js?key=' type='text/javascript'></script>

<style>

.id<<guid>> {
    width: 100%;
}

#map-container-id<<guid>> {
    position: relative;
}

#id<<guid>> {
    height: 600px;
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

</style>


<div class=``map-container id<<guid>>``>
    <div id=``map-container-id<<guid>>``></div>
	<div id=``id<<guid>>``></div>
</div>	


<script>
    // create javascript array of marker info
    var markersid<<guid>> = [
                                                                                                        [33.640705, -112.280198,'','',''],
            ];

	//Set Map
	function initializeid<<guid>>() {
        var bounds = new google.maps.LatLngBounds();
        
            	var centerLatlng = new google.maps.LatLng( 33.640705,-112.280198 );
    	    	
    	var mapOptions = {
    		    		    zoom: 11,
    		    		scrollwheel: true,
    		draggable: true,
    		    		    center: centerLatlng,
    		    		mapTypeId: 'roadmap',
    		zoomControl: true,
            mapTypeControl: false,
            gestureHandling: 'cooperative',
            streetViewControl: false,
            fullscreenControl: true
    		
	    }

		var map = new google.maps.Map(document.getElementById('id<<guid>>'), mapOptions);
		var infoWindow = new google.maps.InfoWindow(), marker, i;
		
		// place each marker on the map  
        for( i = 0; i < markersid<<guid>>.length; i++ ) {
            var position = new google.maps.LatLng(markersid<<guid>>[i][0], markersid<<guid>>[i][1]);
            bounds.extend(position);
            marker = new google.maps.Marker({
                position: position,
                map: map,
                animation: null,
                title: markersid<<guid>>[i][2],
                icon: markersid<<guid>>[i][4]
            });

            // Add info window to marker    
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markersid<<guid>>[i][3] != ''){
                        infoWindow.setContent(markersid<<guid>>[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
           
        }
		
        // Center the map to fit all markers on the screen
                
		//Resize Function
		google.maps.event.addDomListener(window, ``resize``, function() {
			var center = map.getCenter();
			google.maps.event.trigger(map, ``resize``);
			map.setCenter(center);
		});
	}

    google.maps.event.addDomListener(window, 'load', initializeid<<guid>>);

</script>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region GoogleStaticMap

        [TestMethod]
        public void GoogleStaticMapShortcode_DefaultOptions_EmitsCorrectHtml()
        {

            // TODO: This causes a problem because it contains the string literal: '//', which truncates the line.


            var input = @"
{[ googlemap ]}
    [[ marker location:'33.640705,-112.280198' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

            var expectedOutput = @"
<div class=``alert alert-warning``>
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>

<script src='https://maps.googleapis.com/maps/api/js?key=' type='text/javascript'></script>

<style>

.id<<guid>> {
    width: 100%;
}

#map-container-id<<guid>> {
    position: relative;
}

#id<<guid>> {
    height: 600px;
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

</style>


<div class=``map-container id<<guid>>``>
    <div id=``map-container-id<<guid>>``></div>
	<div id=``id<<guid>>``></div>
</div>	


<script>
    // create javascript array of marker info
    var markersid<<guid>> = [
                                                                                                        [33.640705, -112.280198,'','',''],
            ];

	//Set Map
	function initializeid<<guid>>() {
        var bounds = new google.maps.LatLngBounds();
        
            	var centerLatlng = new google.maps.LatLng( 33.640705,-112.280198 );
    	    	
    	var mapOptions = {
    		    		    zoom: 11,
    		    		scrollwheel: true,
    		draggable: true,
    		    		    center: centerLatlng,
    		    		mapTypeId: 'roadmap',
    		zoomControl: true,
            mapTypeControl: false,
            gestureHandling: 'cooperative',
            streetViewControl: false,
            fullscreenControl: true
    		
	    }

		var map = new google.maps.Map(document.getElementById('id<<guid>>'), mapOptions);
		var infoWindow = new google.maps.InfoWindow(), marker, i;
		
		// place each marker on the map  
        for( i = 0; i < markersid<<guid>>.length; i++ ) {
            var position = new google.maps.LatLng(markersid<<guid>>[i][0], markersid<<guid>>[i][1]);
            bounds.extend(position);
            marker = new google.maps.Marker({
                position: position,
                map: map,
                animation: null,
                title: markersid<<guid>>[i][2],
                icon: markersid<<guid>>[i][4]
            });

            // Add info window to marker    
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markersid<<guid>>[i][3] != ''){
                        infoWindow.setContent(markersid<<guid>>[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            }) (marker, i));
           
        }
		
        // Center the map to fit all markers on the screen                

		//Resize Function
		google.maps.event.addDomListener(window, ``resize``, function() {
			var center = map.getCenter();
			google.maps.event.trigger(map, ``resize``);
			map.setCenter(center);
		});
	}

    google.maps.event.addDomListener(window, 'load', initializeid<<guid>>);

</script>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region KPI

        [TestMethod]
        public void KpiShortcode_DocumentationExample_EmitsCorrectHtml()
        {
            var input = @"
{[kpis]}
  [[ kpi icon:'fa-highlighter' value:'4' label:'Highlighters' color:'yellow-700']][[ endkpi ]]
  [[ kpi icon:'fa-pen-fancy' value:'8' label:'Pens' color:'indigo-700']][[ endkpi ]]
  [[ kpi icon:'fa-pencil-alt' value:'15' label:'Pencils' color:'green-600']][[ endkpi ]]
{[endkpis]}
";

            var expectedOutput = @"
<div class=``kpi - container``>
    <div class=``kpi  kpi-card has-icon-bg text-yellow-700 border-yellow-500``>
        <div class=``kpi-icon``>
            <img class=``svg-placeholder`` src=``data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;``>
            <div class=``kpi-content``><i class=``fa fa-fw fa-highlighter``></i></div>
        </div>
        <div class=``kpi-stat ``>
            <span class=``kpi-value text-color``>4</span>
            <span class=``kpi-label``>Highlighters</span>
        </div>
    </div>
    <div class=``kpi  kpi-card has-icon-bg text-indigo-700 border-indigo-500``>
        <div class=``kpi-icon``>
            <img class=``svg-placeholder`` src=``data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;``>
            <div class=``kpi-content``><i class=``fa fa-fw fa-pen-fancy``></i></div>
        </div><div class=``kpi-stat ``>
            <span class=``kpi-value text-color``>8</span>
            <span class=``kpi-label``>Pens</span>
        </div>
    </div>
    <div class=``kpi  kpi-card has-icon-bg text-green-600 border-green-400``>
        <div class=``kpi-icon``>
            <img class=``svg-placeholder`` src=``data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;``>
            <div class=``kpi-content``><i class=``fa fa-fw fa-pencil-alt``></i></div>
        </div><div class=``kpi-stat ``>
            <span class=``kpi-value text-color``>15</span>
            <span class=``kpi-label``>Pencils</span>
        </div>
    </div>
</div>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #endregion

        #region Panel

        [TestMethod]
        public void PanelShortcode_DefaultOptions_EmitsCorrectHtml()
        {
            var input = @"
{[ panel title:'Important Stuff' icon:'fa fa-star' ]}
This is a super simple panel.
{[ endpanel ]}
";

            var expectedOutput = @"
<div class=``panel panel-default``>
  
      <div class=``panel-heading``>
        <h3 class=``panel-title``>
            
                <i class=``fa fa-star``></i> 
            
            Important Stuff</h3>
      </div>
    <div class=``panel-body``>
    This is a super simple panel.
  </div>
  
</div>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #endregion

        #region Parallax

        [TestMethod]
        public void ParallaxShortcode_DefaultOptions_EmitsCorrectHtml()
        {
            var input = @"
{[ parallax image:'http://cdn.wonderfulengineering.com/wp-content/uploads/2014/09/star-wars-wallpaper-4.jpg' contentpadding:'20px' speed:'75' ]}
    <h1>Hello World</h1>
{[ endparallax ]}
";

            var expectedOutput = @"
<div id=``id-<<guid>>`` data-jarallax class=``jarallax`` data-type=``scroll`` data-speed=``1.75`` data-img-position=```` data-object-position=```` data-background-position=```` data-zindex=``2`` data-no-android=```` data-no-ios=``false``>
        <img class=``jarallax-img`` src=``http://cdn.wonderfulengineering.com/wp-content/uploads/2014/09/star-wars-wallpaper-4.jpg`` alt=````>

                    <div class=``parallax-content``>
                <h1>Hello World</h1>
            </div>
            </div>


<style>
#id-<<guid>> {
    /* eventually going to change the height using media queries with mixins using sass, and then include only the classes I want for certain parallaxes */
    min-height: 200px;
    background: transparent;
    position: relative;
    z-index: 0;
}

#id-<<guid>> .jarallax-img {
    position: absolute;
    object-fit: cover;
    /* support for plugin https://github.com/bfred-it/object-fit-images */
    font-family: 'object-fit: cover;';
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
}

#id-<<guid>> .parallax-content{
    display: inline-block;
    margin: 20px;
    color: #fff;
    text-align: center;
	width: 100%;
}
</style>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region Sparkline Chart

        [TestMethod]
        public void SparklineShortcodeTag_DefaultOptions_EmitsHtmlWithDefaultSettings()
        {
            var input = @"
{[ sparkline type:'line' data:'5,6,7,9,9,5,3,2,2,4,6,7' ]}
";

            var expectedOutput = @"
<script src='~/Scripts/sparkline/jquery-sparkline.min.js' type='text/javascript'></script>

<span class=``sparkline sparkline-id-<<guid>>``>Loading...</span>

  <script>
    $(``.sparkline-id-<<guid>>``).sparkline([5,6,7,9,9,5,3,2,2,4,6,7], {
      type: 'line'
      , width: 'auto'
      , height: 'auto'
      , lineColor: '#ee7625'
      , fillColor: '#f7c09b'
      , lineWidth: 1
      , spotColor: '#f80'
      , minSpotColor: '#f80'
      , maxSpotColor: '#f80'
      , highlightSpotColor: ''
      , highlightLineColor: ''
      , spotRadius: 1.5
      , chartRangeMin: undefined
      , chartRangeMax: undefined
      , chartRangeMinX: undefined
      , chartRangeMaxX: undefined
      , normalRangeMin: undefined
      , normalRangeMax: undefined
      , normalRangeColor: '#ccc'
    });
  
  </script>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region Vimeo

        [TestMethod]
        public void VimeoShortcodeTag_DefaultOptions_EmitsCorrectHtml()
        {
            var input = @"
{[ vimeo id:'180467014' ]}
";

            var expectedOutput = @"
<style>
.embed-container { 
    position: relative; 
    padding-bottom: 56.25%; 
    height: 0; 
    overflow: hidden; 
    max-width: 100%; } 
.embed-container iframe, 
.embed-container object, 
.embed-container embed { position: absolute; top: 0; left: 0; width: 100%; height: 100%; }
</style>

<div id='id-<<guid>>' style='width:100%;'>
    <div class='embed-container'><iframe src='https://player.vimeo.com/video/180467014?autoplay=0&autoplay=0&loop=0&title=0&byline=0&portrait=0' frameborder='0' webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe></div>
</div>
";

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region Word Cloud

        [TestMethod]
        public void WordCloudShortcodeBlock_DefaultOptions_EmitsCorrectHtml()
        {
            var input = @"
{[ wordcloud ]}
How the Word Cloud Generator Works
The layout algorithm for positioning words without overlap is available on GitHub under an open source license as d3-cloud. Note that this is the only the layout algorithm and any code for converting text into words and rendering the final output requires additional development.
As word placement can be quite slow for more than a few hundred words, the layout algorithm can be run asynchronously, with a configurable time step size. This makes it possible to animate words as they are placed without stuttering. It is recommended to always use a time step even without animations as it prevents the browser’s event loop from blocking while placing the words.
The layout algorithm itself is incredibly simple. For each word, starting with the most “important”:
Attempt to place the word at some starting point: usually near the middle, or somewhere on a central horizontal line.
If the word intersects with any previously placed words, move it one step along an increasing spiral. Repeat until no intersections are found.
The hard part is making it perform efficiently! According to Jonathan Feinberg, Wordle uses a combination of hierarchical bounding boxes and quadtrees to achieve reasonable speeds.
Glyphs in JavaScript
There isn’t a way to retrieve precise glyph shapes via the DOM, except perhaps for SVG fonts. Instead, we draw each word to a hidden canvas element, and retrieve the pixel data.
Retrieving the pixel data separately for each word is expensive, so we draw as many words as possible and then retrieve their pixels in a batch operation.
Sprites and Masks
My initial implementation performed collision detection using sprite masks. Once a word is placed, it doesn't move, so we can copy it to the appropriate position in a larger sprite representing the whole placement area.
The advantage of this is that collision detection only involves comparing a candidate sprite with the relevant area of this larger sprite, rather than comparing with each previous word separately.
Somewhat surprisingly, a simple low-level hack made a tremendous difference: when constructing the sprite I compressed blocks of 32 1-bit pixels into 32-bit integers, thus reducing the number of checks (and memory) by 32 times.
In fact, this turned out to beat my hierarchical bounding box with quadtree implementation on everything I tried it on (even very large areas and font sizes). I think this is primarily because the sprite version only needs to perform a single collision test per candidate area, whereas the bounding box version has to compare with every other previously placed word that overlaps slightly with the candidate area.
Another possibility would be to merge a word’s tree with a single large tree once it is placed. I think this operation would be fairly expensive though compared with the analagous sprite mask operation, which is essentially ORing a whole block.
{[ endwordcloud ]}
";

            var expectedOutput = @"
<script src='~/Scripts/d3-cloud/d3.layout.cloud.js' type='text/javascript'></script>


<script src='~/Scripts/d3-cloud/d3.min.js' type='text/javascript'></script>


<div id=``id-<<guid>>`` style=``width: 960; height: 420;``></div>



<script>
    $( document ).ready(function() {
        Rock.controls.wordcloud.initialize({
            inputTextId: 'hf-id-<<guid>>',
            visId: 'id-<<guid>>',
            width: '960',
            height: '420',
            fontName: 'Impact',
            maxWords: 255,
            scaleName: 'log',
            spiralName: 'archimedean',
            colors: [ '#0193B9','#F2C852','#1DB82B','#2B515D','#ED3223'],
        });
    });
</script>


<input type=``hidden`` id=``hf-id-<<guid>>`` value=``How the Word Cloud Generator Works
The layout algorithm for positioning words without overlap is available on GitHub under an open source license as d3-cloud. Note that this is the only the layout algorithm and any code for converting text into words and rendering the final output requires additional development.
As word placement can be quite slow for more than a few hundred words, the layout algorithm can be run asynchronously, with a configurable time step size. This makes it possible to animate words as they are placed without stuttering. It is recommended to always use a time step even without animations as it prevents the browser’s event loop from blocking while placing the words.
The layout algorithm itself is incredibly simple. For each word, starting with the most “important”:
Attempt to place the word at some starting point: usually near the middle, or somewhere on a central horizontal line.
If the word intersects with any previously placed words, move it one step along an increasing spiral. Repeat until no intersections are found.
The hard part is making it perform efficiently! According to Jonathan Feinberg, Wordle uses a combination of hierarchical bounding boxes and quadtrees to achieve reasonable speeds.
Glyphs in JavaScript
There isn’t a way to retrieve precise glyph shapes via the DOM, except perhaps for SVG fonts. Instead, we draw each word to a hidden canvas element, and retrieve the pixel data.
Retrieving the pixel data separately for each word is expensive, so we draw as many words as possible and then retrieve their pixels in a batch operation.
Sprites and Masks
My initial implementation performed collision detection using sprite masks. Once a word is placed, it doesn't move, so we can copy it to the appropriate position in a larger sprite representing the whole placement area.
The advantage of this is that collision detection only involves comparing a candidate sprite with the relevant area of this larger sprite, rather than comparing with each previous word separately.
Somewhat surprisingly, a simple low-level hack made a tremendous difference: when constructing the sprite I compressed blocks of 32 1-bit pixels into 32-bit integers, thus reducing the number of checks (and memory) by 32 times.
In fact, this turned out to beat my hierarchical bounding box with quadtree implementation on everything I tried it on (even very large areas and font sizes). I think this is primarily because the sprite version only needs to perform a single collision test per candidate area, whereas the bounding box version has to compare with every other previously placed word that overlaps slightly with the candidate area.
Another possibility would be to merge a word’s tree with a single large tree once it is placed. I think this operation would be fairly expensive though compared with the analagous sprite mask operation, which is essentially ORing a whole block.`` />
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region YouTube

        [TestMethod]
        public void YouTubeShortcodeTag_DefaultOptions_EmitsCorrectHtml()
        {
            var input = @"
{[ youtube id:'8kpHK4YIwY4' ]}
";

            var expectedOutput = @"
<style>

#id-<<guid>> {
    width: 100%;
}

.embed-container { 
    position: relative; 
    padding-bottom: 56.25%; 
    height: 0; 
    overflow: hidden; 
    max-width: 100%; } 
.embed-container iframe, 
.embed-container object, 
.embed-container embed { position: absolute; top: 0; left: 0; width: 100%; height: 100%; }
</style>

<div id='id-<<guid>>'>
    <div class='embed-container'><iframe src='https://www.youtube.com/embed/8kpHK4YIwY4?rel=0&showinfo=0&controls=1&autoplay=0' frameborder='0' allowfullscreen></iframe></div>
</div>
";

            var options = new LavaTestRenderOptions() { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion
    }
}
