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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Core.Lava.Shortcodes
{
    [TestClass]
    [TestCategory( "Core.Lava.Shortcodes" )]
    public class ChartShortcodeTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void ChartShortcode_MultipleCharts_RendersCorrectLabels()
        {
            var input = @"
{[ chart type:'line' ]}
    [[ dataitem label:'Small Groups' value:'45' ]] [[ enddataitem ]]
    [[ dataitem label:'Serving Groups' value:'38' ]] [[ enddataitem ]]
{[ endchart ]}

{[ chart type:'line' yaxismin:'0' labels:'2015,2016,2017' ]}
    [[ dataset label:'Small Groups' data:'12, 15, 34' fillcolor:'#059BFF' ]] [[ enddataset ]]
    [[ dataset label:'Serving Teams' data:'10, 22, 41' fillcolor:'#FF3D67' ]] [[ enddataset ]]
{[ endchart ]}
";

            // This test verifies issue #6470. Due to variable leakage between
            // shortcode executions, the dataitem values bled into the second
            // shortcode. This caused, among other things, the labels to not
            // render correctly because it thought it had dataitems instead of
            // datasets.
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, input, new LavaTestRenderOptions() );

                Assert.Contains( @"labels: [""Small Groups"", ""Serving Groups""],", output, "Labels from first shortcode are missing." );
                Assert.Contains( @"labels: [""2015"",""2016"",""2017""],", output, "Labels from second shortcode are missing." );
            } );
        }
    }
}
