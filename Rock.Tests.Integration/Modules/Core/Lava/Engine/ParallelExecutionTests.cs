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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Lava.RockLiquid;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Engine
{
    /// <summary>
    /// Tests for parallel execution and multi-threading issues.
    /// </summary>
    [TestClass]
    public class ParallelExecutionTests : LavaIntegrationTestBase
    {
        /// <summary>
        /// Verify that when a thread is aborted while the Lava Engine is rendering a template, the ThreadAbortException is propagated correctly.
        /// </summary>
        [TestMethod]
        public void ThreadExecution_ThreadAbortedWhileExecutingRender_PropagatesThreadAbortException()
        {
            var template = "{{ 'test' | Abort }}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // This test is not applicable to the RockLiquid engine implementation.
                if ( engine.GetType() == typeof ( RockLiquidEngine ) )
                {
                    return;
                }

                var methodInfo = this.GetType().GetMethod( "ThreadAbortFilter" );

                engine.RegisterFilter( methodInfo, "Abort" );

                var renderContext = engine.NewRenderContext( new List<string> { "Sql" } );

                try
                {
                    var result = engine.RenderTemplate( template,
                            new LavaRenderParameters { Context = renderContext, ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput } );

                    Assert.Fail( "ThreadAbortException expected but not encountered." );
                }
                catch ( ThreadAbortException )
                {
                    // Resetting the abort status is the only method of preventing this exception from being rethrown.
                    Thread.ResetAbort();
                }
            } );
        }

        /// <summary>
        /// This filter simulates the effect of executing the PageRedirect filter in the web application.
        /// A redirect terminates the current thread and starts a new thread to service the redirect page.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ThreadAbortFilter( string input )
        {
            Thread.CurrentThread.Abort();

            return string.Empty;
        }

        [TestMethod]
        public void ParallelExecution_ShortcodeWithParameters_ResolvesParameterCorrectly()
        {
            var shortcodeTemplate = @"
Font Name: {{ fontname }}
Font Size: {{ fontsize }}
Font Bold: {{ fontbold }}
";

            var input = @"
{[ shortcodetest fontname:'Arial' fontsize:'{{ fontsize }}' fontbold:'true' ]}
{[ endshortcodetest ]}
";

            var expectedOutput = @"
Font Name: Arial
Font Size: <?>
Font Bold: true
";

            // Create a new test shortcode.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Block;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "shortcodetest";
            shortcodeDefinition.Parameters = new Dictionary<string, string> { { "fontname", "Arial" }, { "fontsize", "0" }, { "fontbold", "true" } };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    TestHelper.DebugWriteRenderResult( engine, "(Ignored)", "(Ignored)" );
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 100 };

                Parallel.For( 1, 1000, parallelOptions, ( x ) =>
                {
                    var context = new LavaDataDictionary()
                    {
                    { "fontsize", x },
                    };
                    context["fontsize"] = x;

                    var options = new LavaTestRenderOptions() { MergeFields = context, Wildcards = new List<string> { "<?>" } };

                    TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
                } );
            } );
        }

        [TestMethod]
        public void ParallelExecution_ShortcodeWithChildItems_EmitsCorrectHtml()
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
***
Iteration: {{ iteration }}
***
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
***
Iteration: <?>
***
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
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    TestHelper.DebugWriteRenderResult( engine, "(Ignored)", "(Ignored)" );
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };

                Parallel.For( 0, 1000, parallelOptions, ( x ) =>
                {
                    var context = new LavaDataDictionary();
                    context["iteration"] = x;

                    var options = new LavaTestRenderOptions() { MergeFields = context, Wildcards = new List<string> { "<?>" } };

                    TestHelper.AssertTemplateOutput( engine, expectedOutput, input, options );
                } );
            } );

        }

    }
}
