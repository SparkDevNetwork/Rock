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

using Rock.Lava;
using Rock.Tests.Lava.Shared;

namespace Rock.Tests.Integration.Core.Lava.Shortcodes
{
    [TestClass]
    [TestCategory( "Core.Lava.Shortcodes" )]
    public class KpiShortcodeTests : LavaIntegrationTestBase
    {
        #region Templates

        /*
            The kpis shortcode emits a sizeable block of markup, and the expected
            output below is the whole of it. The templates are named constants
            rather than inline literals because the last test in this class renders
            all of them together, as a single document that can be pasted into a
            Rock instance to check the cards by eye.

            These assertions compare with whitespace removed. The shortcode's own
            Lava template leaves blank and trailing-whitespace-only lines behind,
            which no editor or diff tool preserves reliably, so asserting on the
            layout would make the tests fail on a stray trim rather than on a
            change in behavior. What the markup contains is what is under test.
        */

        private const string TitleAndColumnParametersTemplate = @"
{[kpis title:'Chess Pieces' subtitle:'A selection of chess pieces' showtitleseparator:'true' columncount:'1' columnmin:'12' size:'lg' tooltipdelay:'2000']}
  [[ kpi icon:'fa-chess-king' value:'100' description:'king' ]][[ endkpi ]]
  [[ kpi icon:'fa-chess-queen' value:'101' description:'queen' ]][[ endkpi ]]
  [[ kpi icon:'fa-chess-rook' value:'102' description:'rook' ]][[ endkpi ]]
  [[ kpi icon:'fa-chess-knight' value:'103' description:'knight' ]][[ endkpi ]]
  [[ kpi icon:'fa-chess-bishop' value:'104' description:'bishop' ]][[ endkpi ]]
{[endkpis]}
";

        private const string SizeAndIconBackgroundParametersTemplate = @"
{[kpis title:'Small' size:'sm']}
  [[ kpi icon:'fa-user' value:'1' color:'red' ]][[ endkpi ]]
{[endkpis]}
{[kpis title:'Default' iconbackground:'false']}
  [[ kpi icon:'fa-user' value:'10' color:'yellow' ]][[ endkpi ]]
{[endkpis]}
{[kpis title:'Large' size:'lg']}
  [[ kpi icon:'fa-user' value:'100' color:'green' ]][[ endkpi ]]
{[endkpis]}
{[kpis title:'Extra-Large' size:'xl' iconbackground:'false']}
  [[ kpi icon:'fa-user' value:'1000' color:'blue' ]][[ endkpi ]]
{[endkpis]}
";

        private const string ItemLabelAndUrlParametersTemplate = @"
{[ kpis ]}
[[ kpi icon:'fa-users' value:'30' label:'Groups' textalign:'right' labellocation:'top' description:'Tooltip: highlighters' color:'yellow-700' url:'/people/groups']][[ endkpi ]]
{[ endkpis ]}
";

        private const string ItemHeightParameterTemplate = @"
{[ kpis columncount:'1' columnmin:'12' ]}
[[ kpi icon:'fa-users' value:'200px' label:'Tall' height:'200px']][[ endkpi ]]
[[ kpi icon:'fa-users' value:'100px' label:'Medium' height:'100px']][[ endkpi ]]
[[ kpi icon:'fa-users' value:'50px' label:'Short' height:'50px']][[ endkpi ]]
{[ endkpis ]}
";

        private const string ItemIconTypeParameterTemplate = @"
{[ kpis ]}
[[ kpi value:'789' color:'orange' icontype:'xyz' icon:'icon-class' ]][[ endkpi ]]
{[ endkpis ]}
";

        private const string DocumentationBasicUsageTemplate = @"
{[kpis]}
  [[ kpi icon:'fa-highlighter' value:'4' label:'Highlighters' color:'yellow-700']][[ endkpi ]]
  [[ kpi icon:'fa-pen-fancy' value:'8' label:'Pens' color:'indigo-700']][[ endkpi ]]
  [[ kpi icon:'fa-pencil-alt' value:'15' label:'Pencils' color:'green-600']][[ endkpi ]]
{[endkpis]}
";

        private const string DocumentationStyleTemplate = @"
{[kpis title:'Card Style' style:'card' ]}
  [[ kpi icon:'fa-check-square' value:'42' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}
{[kpis title:'Edgeless Style' style:'edgeless' ]}
  [[ kpi icon:'fa-check-square' value:'42' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}
";

        private const string DocumentationAdvancedOptionsTemplate = @"
{[kpis title:'With Parameters' subtitle: 'subvalue, secondarylabel' ]}
  [[ kpi icon:'fa-user' value:'92' label:'Individuals Completing Program' secondarylabel:'Secondary Label' subvalue:'+49 YTD' color:'indigo-700' ]][[ endkpi ]]
{[endkpis]}
";

        #endregion Templates

        #region Icon Tests

        [TestMethod]
        public void KpiShortcode_WithFontAwesomeIcon_RendersFontAwesomePrefix()
        {
            var inputTemplate = "{[kpis]}[[ kpi icon:'fa-user' value:'0' label:'Test' ]][[ endkpi ]]{[endkpis]}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate );

                Assert.Contains( "class=\"fa fa-fw fa-user\"", output );
            } );
        }

        [TestMethod]
        public void KpiShortcode_WithTablerIcon_RendersTablerPrefix()
        {
            var inputTemplate = "{[kpis]}[[ kpi icon:'ti-user' value:'0' label:'Test' ]][[ endkpi ]]{[endkpis]}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate );

                Assert.Contains( "class=\"ti ti-fw ti-user\"", output );
            } );
        }

        #endregion Icon Tests

        #region Shortcode Parameters

        /// <summary>
        /// Verifies title, subtitle, showtitleseparator, columncount, columnmin,
        /// size and tooltipdelay.
        /// </summary>
        [TestMethod]
        public void KpiShortcode_WithTitleAndColumnParameters_RendersExpectedOutput()
        {
            var expectedOutput = @"
<h3 id=""chess-pieces"" class=""kpi-title"">Chess Pieces</h3><p class=""kpi-subtitle"">A selection of chess pieces</p><hr class=""mt-3 mb-4""><div class=""kpi-container"" style=""--kpi-col-lg:100%;--kpi-col-md:100%;--kpi-col-sm:100%;--kpi-min-width:12; "">
    <div class=""kpi kpi-lg kpi-card has-icon-bg ""  data-toggle=""tooltip"" title=""king"" data-delay='2000'>
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-chess-king""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">100</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
    <div class=""kpi kpi-lg kpi-card has-icon-bg ""  data-toggle=""tooltip"" title=""queen"" data-delay='2000'>
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-chess-queen""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">101</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
    <div class=""kpi kpi-lg kpi-card has-icon-bg ""  data-toggle=""tooltip"" title=""rook"" data-delay='2000'>
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-chess-rook""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">102</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
    <div class=""kpi kpi-lg kpi-card has-icon-bg ""  data-toggle=""tooltip"" title=""knight"" data-delay='2000'>
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-chess-knight""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">103</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
    <div class=""kpi kpi-lg kpi-card has-icon-bg ""  data-toggle=""tooltip"" title=""bishop"" data-delay='2000'>
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-chess-bishop""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">104</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
</div>
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, TitleAndColumnParametersTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( new[] { expectedOutput }, output );
            } );
        }

        /// <summary>
        /// Verifies the size and iconbackground parameters.
        /// </summary>
        [TestMethod]
        public void KpiShortcode_WithSizeAndIconBackgroundParameters_RendersExpectedOutput()
        {
            var expectedOutput = @"
<h3 id=""small"" class=""kpi-title"">Small</h3><div class=""kpi-container"" >
    <div class=""kpi kpi-sm kpi-card has-icon-bg "" style=""color:red;border-color:rgba(255, 0, 0, 0.5)"" >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-user""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">1</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
</div>
<h3 id=""default"" class=""kpi-title"">Default</h3><div class=""kpi-container"" >
    <div class=""kpi  kpi-card  "" style=""color:yellow;border-color:rgba(255, 255, 0, 0.5)"" >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-user""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">10</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
</div>
<h3 id=""large"" class=""kpi-title"">Large</h3><div class=""kpi-container"" >
    <div class=""kpi kpi-lg kpi-card has-icon-bg "" style=""color:green;border-color:rgba(0, 128, 0, 0.5)"" >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-user""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">100</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
</div>
<h3 id=""extra-large"" class=""kpi-title"">Extra-Large</h3><div class=""kpi-container"" >
    <div class=""kpi kpi-xl kpi-card  "" style=""color:blue;border-color:rgba(0, 0, 255, 0.5)"" >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-user""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">1000</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
</div>
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, SizeAndIconBackgroundParametersTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( new[] { expectedOutput }, output );
            } );
        }

        #endregion Shortcode Parameters

        #region KPI Item Parameters

        /// <summary>
        /// Verifies the item icon, label, labellocation, value, description, color,
        /// textalign and url parameters.
        /// </summary>
        [TestMethod]
        public void KpiShortcode_WithItemLabelAndUrlParameters_RendersExpectedOutput()
        {
            var expectedOutput = @"
<div class=""kpi-container"" >
    <div class=""kpi  kpi-card has-icon-bg text-yellow-700 border-yellow-500""  data-toggle=""tooltip"" title=""Tooltip: highlighters"" >
            <a href=""/people/groups"" class=""stretched-link""></a><div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-users""></i></div>
            </div><div class=""kpi-stat text-right"">
                <span class=""kpi-label"">Groups</span>
                <span class=""kpi-value text-color"">30</span>
            </div>
        </div>
</div>
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, ItemLabelAndUrlParametersTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( new[] { expectedOutput }, output );
            } );
        }

        /// <summary>
        /// Verifies that the item height parameter is emitted as a minimum height.
        /// </summary>
        [TestMethod]
        public void KpiShortcode_WithItemHeightParameter_RendersExpectedOutput()
        {
            var expectedOutput = @"
<div class=""kpi-container"" style=""--kpi-col-lg:100%;--kpi-col-md:100%;--kpi-col-sm:100%;--kpi-min-width:12; "">
    <div class=""kpi  kpi-card has-icon-bg "" style=""min-height: 200px;"" >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-users""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">200px</span>
                <span class=""kpi-label"">Tall</span>
            </div>
        </div>
    <div class=""kpi  kpi-card has-icon-bg "" style=""min-height: 100px;"" >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-users""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">100px</span>
                <span class=""kpi-label"">Medium</span>
            </div>
        </div>
    <div class=""kpi  kpi-card has-icon-bg "" style=""min-height: 50px;"" >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-users""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">50px</span>
                <span class=""kpi-label"">Short</span>
            </div>
        </div>
</div>
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, ItemHeightParameterTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( new[] { expectedOutput }, output );
            } );
        }

        /// <summary>
        /// Verifies that an unrecognized icontype is emitted as the icon class
        /// prefix rather than being rejected.
        /// </summary>
        [TestMethod]
        public void KpiShortcode_WithItemIconTypeParameter_RendersExpectedOutput()
        {
            var expectedOutput = @"
<div class=""kpi-container"" >
    <div class=""kpi  kpi-card has-icon-bg "" style=""color:orange;border-color:rgba(255, 165, 0, 0.5)"" >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""xyz ti-fw icon-class""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">789</span>
                <span class=""kpi-label""></span>
            </div>
        </div>
</div>
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, ItemIconTypeParameterTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( new[] { expectedOutput }, output );
            } );
        }

        #endregion KPI Item Parameters

        #region Documentation Examples

        [TestMethod]
        public void KpiShortcode_DocumentationExampleBasicUsage_RendersExpectedOutput()
        {
            var expectedOutput = @"
<div class=""kpi-container"" >
    <div class=""kpi  kpi-card has-icon-bg text-yellow-700 border-yellow-500""  >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-highlighter""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">4</span>
                <span class=""kpi-label"">Highlighters</span>
            </div>
        </div>
    <div class=""kpi  kpi-card has-icon-bg text-indigo-700 border-indigo-500""  >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-pen-fancy""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">8</span>
                <span class=""kpi-label"">Pens</span>
            </div>
        </div>
    <div class=""kpi  kpi-card has-icon-bg text-green-600 border-green-400""  >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-pencil-alt""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">15</span>
                <span class=""kpi-label"">Pencils</span>
            </div>
        </div>
</div>
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, DocumentationBasicUsageTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( new[] { expectedOutput }, output );
            } );
        }

        [TestMethod]
        public void KpiShortcode_DocumentationExampleStyle_RendersExpectedOutput()
        {
            var expectedOutput = @"
<h3 id=""card-style"" class=""kpi-title"">Card Style</h3><div class=""kpi-container"" >
    <div class=""kpi  kpi-card has-icon-bg text-indigo-700 border-indigo-500""  >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-check-square""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">42</span>
                <span class=""kpi-label"">Steps Completed</span>
            </div>
        </div>
</div>
<h3 id=""edgeless-style"" class=""kpi-title"">Edgeless Style</h3><div class=""kpi-container"" >
    <div class=""kpi   has-icon-bg text-indigo-700 border-indigo-500""  >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-check-square""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">42</span>
                <span class=""kpi-label"">Steps Completed</span>
            </div>
        </div>
</div>
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, DocumentationStyleTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( new[] { expectedOutput }, output );
            } );
        }

        [TestMethod]
        public void KpiShortcode_DocumentationExampleAdvancedOptions_RendersExpectedOutput()
        {
            var expectedOutput = @"
<h3 id=""with-parameters"" class=""kpi-title"">With Parameters</h3><div class=""kpi-container"" >
    <div class=""kpi  kpi-card has-icon-bg text-indigo-700 border-indigo-500""  >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""fa fa-fw fa-user""></i></div>
            </div><div class=""kpi-stat "">
                <span class=""kpi-value text-color"">92<span class=""kpi-subvalue "">+49 YTD</span></span>
                <span class=""kpi-label"">Individuals Completing Program</span>
                <span class=""kpi-secondary-label"">
                Secondary Label
                </span>
            </div>
        </div>
</div>
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, DocumentationAdvancedOptionsTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( new[] { expectedOutput }, output );
            } );
        }

        #endregion Documentation Examples

        #region Combined Document

        /// <summary>
        /// Renders every kpis example in this class as a single document.
        /// </summary>
        /// <remarks>
        /// The assertions above each cover one example's markup. This renders the
        /// same templates together, so that the output can be pasted into a Rock
        /// page to confirm the cards actually draw, and so that a template which
        /// only renders in isolation is caught.
        /// </remarks>
        [TestMethod]
        public void KpiShortcode_ApplicationTestTemplate_CanRender()
        {
            var documentTemplate = string.Join( "\n", new[]
            {
                "<h1>KPI Shortcode Tests rev20240613.1</h1>",
                "<h3>title/subtitle/showtitleseparator/columncount/columnmin/tooltipdelay</h3><p>columncount=1, columnmin=12, tooltipdelay=2000</p><hr>",
                TitleAndColumnParametersTemplate,
                "<h3>size, iconbackground</h3><hr>",
                SizeAndIconBackgroundParametersTemplate,
                "<h3>icon/label/labellocation/value/description/color/textalign/url</h3><p>labellocation=top, textalign=right</p><hr>",
                ItemLabelAndUrlParametersTemplate,
                "<h3>height</h3><hr>",
                ItemHeightParameterTemplate,
                "<h3>icontype</h3><p>icontype=xyz</p><hr>",
                ItemIconTypeParameterTemplate,
                "<h3>Basic Usage</h3><hr>",
                DocumentationBasicUsageTemplate,
                "<h3>Style</h3><hr>",
                DocumentationStyleTemplate,
                "<h3>Advanced Options</h3><hr>",
                DocumentationAdvancedOptionsTemplate
            } );

            var options = new LavaRenderOptions
            {
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Throw
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var result = LavaRenderTestHelper.RenderResult( engine, documentTemplate, options );

                Assert.IsFalse( result.HasErrors, $"Document template reported an error. [Error={result.Error?.Message}]" );
            } );
        }

        #endregion Combined Document
    }
}
