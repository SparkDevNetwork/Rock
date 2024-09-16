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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Shortcodes
{
    [TestClass]
    [TestCategory( "Core.Lava.Shortcodes" )]
    public class KpiShortcodeTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void KpiShortcode_WithShortcodeParametersSpecified_RendersExpectedOutput()
        {
            TestHelper.AssertRenderTestIsValid( GetTestCaseForShortcodeParameters1() );
            TestHelper.AssertRenderTestIsValid( GetTestCaseForShortcodeParameters2() );
        }

        [TestMethod]
        public void KpiShortcode_WithKpiItemParametersSpecified_RendersExpectedOutput()
        {
            TestHelper.AssertRenderTestIsValid( GetTestCaseForKpiItemParameters1() );
            TestHelper.AssertRenderTestIsValid( GetTestCaseForKpiItemParameters2() );
            TestHelper.AssertRenderTestIsValid( GetTestCaseForKpiItemParameters3() );
        }

        [TestMethod]
        public void KpiShortcode_DocumentationExamples_RenderExpectedOutput()
        {
            TestHelper.AssertRenderTestIsValid( GetTestCaseForDocumentationExampleBasicUsage() );
            TestHelper.AssertRenderTestIsValid( GetTestCaseForDocumentationExampleStyle() );
            TestHelper.AssertRenderTestIsValid( GetTestCaseForDocumentationExampleAdvancedOptions() );
        }

        [TestMethod]
        public void KpiShortcode_ApplicationTestTemplate_CanRender()
        {
            var testCases = new List<LavaTemplateRenderTestCase>();

            testCases.Add( GetTestCaseForShortcodeParameters1() );
            testCases.Add( GetTestCaseForShortcodeParameters2() );
            testCases.Add( GetTestCaseForKpiItemParameters1() );
            testCases.Add( GetTestCaseForKpiItemParameters2() );
            testCases.Add( GetTestCaseForKpiItemParameters3() );

            testCases.Add( GetTestCaseForDocumentationExampleBasicUsage() );
            testCases.Add( GetTestCaseForDocumentationExampleStyle() );
            testCases.Add( GetTestCaseForDocumentationExampleAdvancedOptions() );

            var testTemplate = TestHelper.BuildRenderTestTemplate( testCases,
                "KPI Shortcode Tests rev20240613.1" );

            TestHelper.AssertTemplateIsValid( testTemplate );
        }

        private LavaTemplateRenderTestCase GetTestCaseForShortcodeParameters1()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Shortcode Parameters",
                Name = "title/subtitle/showtitleseparator/columncount/columnmin/tooltipdelay",
                Description = "columncount=1, columnmin=12, tooltipdelay=2000"
            };

            testCase.InputTemplate = @"
{[kpis title:'Chess Pieces' subtitle:'A selection of chess pieces' showtitleseparator:'true' columncount:'1' columnmin:'12' size:'lg' tooltipdelay:'2000']}
  [[ kpi icon:'fa-chess-king' value:'100' description:'king' ]][[ endkpi ]]
  [[ kpi icon:'fa-chess-queen' value:'101' description:'queen' ]][[ endkpi ]]
  [[ kpi icon:'fa-chess-rook' value:'102' description:'rook' ]][[ endkpi ]]
  [[ kpi icon:'fa-chess-knight' value:'103' description:'knight' ]][[ endkpi ]]
  [[ kpi icon:'fa-chess-bishop' value:'104' description:'bishop' ]][[ endkpi ]]
{[endkpis]}
";

            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
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
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForShortcodeParameters2()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Shortcode Parameters",
                Name = "size, iconbackground"
            };

            testCase.InputTemplate = @"
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

            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
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
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForKpiItemParameters1()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "KPI Item Parameters",
                Name = "icon/label/labellocation/value/description/color/textalign/icon/url",
                Description = "labellocation=top, textalign=right"
            };

            testCase.InputTemplate = @"
{[ kpis ]}
[[ kpi icon:'fa-users' value:'30' label:'Groups' textalign:'right' labellocation:'top' description:'Tooltip: highlighters' color:'yellow-700' url:'/people/groups']][[ endkpi ]]
{[ endkpis ]}
";

            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
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
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForKpiItemParameters2()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "KPI Item Parameters",
                Name = "height"
            };

            testCase.InputTemplate = @"
{[ kpis columncount:'1' columnmin:'12' ]}
[[ kpi icon:'fa-users' value:'200px' label:'Tall' height:'200px']][[ endkpi ]]
[[ kpi icon:'fa-users' value:'100px' label:'Medium' height:'100px']][[ endkpi ]]
[[ kpi icon:'fa-users' value:'50px' label:'Short' height:'50px']][[ endkpi ]]
{[ endkpis ]}
";

            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
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
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForKpiItemParameters3()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "KPI Item Parameters",
                Name = "icontype",
                Description = "icontype=xyz"
            };

            testCase.InputTemplate = @"
{[ kpis ]}
[[ kpi value:'789' color:'orange' icontype:'xyz' icon:'icon-class' ]][[ endkpi ]] 
{[ endkpis ]}
";

            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
<div class=""kpi-container"" >
    <div class=""kpi  kpi-card has-icon-bg "" style=""color:orange;border-color:rgba(255, 165, 0, 0.5)"" >
            <div class=""kpi-icon"">
                <img class=""svg-placeholder"" src=""data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;"">
                <div class=""kpi-content""><i class=""xyz fa-fw icon-class""></i></div>
            </div><div class=""kpi-stat "">
                
                <span class=""kpi-value text-color"">789</span>
                <span class=""kpi-label""></span>
            

            </div>
        </div>
    
</div> 
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForDocumentationExampleBasicUsage()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Documentation Examples",
                Name = "Basic Usage"
            };

            testCase.InputTemplate = @"
{[kpis]}
  [[ kpi icon:'fa-highlighter' value:'4' label:'Highlighters' color:'yellow-700']][[ endkpi ]]
  [[ kpi icon:'fa-pen-fancy' value:'8' label:'Pens' color:'indigo-700']][[ endkpi ]]
  [[ kpi icon:'fa-pencil-alt' value:'15' label:'Pencils' color:'green-600']][[ endkpi ]]
{[endkpis]}
";

            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
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
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForDocumentationExampleStyle()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Documentation Examples",
                Name = "Style"
            };

            testCase.InputTemplate = @"
{[kpis title:'Card Style' style:'card' ]}
  [[ kpi icon:'fa-check-square' value:'42' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}
{[kpis title:'Edgeless Style' style:'edgeless' ]}
  [[ kpi icon:'fa-check-square' value:'42' label:'Steps Completed' color:'indigo-700']][[ endkpi ]]
{[endkpis]}
";

            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
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
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForDocumentationExampleAdvancedOptions()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Documentation Examples",
                Name = "Advanced Options"
            };

            testCase.InputTemplate = @"
{[kpis title:'With Parameters' subtitle: 'subvalue, secondarylabel' ]}
  [[ kpi icon:'fa-user' value:'92' label:'Individuals Completing Program' secondarylabel:'Secondary Label' subvalue:'+49 YTD' color:'indigo-700' ]][[ endkpi ]]
{[endkpis]}
";

            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
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
" ) );

            return testCase;
        }
    }
}
