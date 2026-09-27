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
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Engine
{
    /// <summary>
    /// Tests for parallel execution and multi-threading issues.
    /// </summary>
    /// <remarks>
    /// Every test here registers a component against the engine it renders with,
    /// so each builds its own engine rather than mutating the one shared by the
    /// rest of the assembly.
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class ParallelExecutionTests
    {
        /// <summary>
        /// Verify that when a thread is aborted while the Lava Engine is rendering a template, the ThreadAbortException is propagated correctly.
        /// </summary>
        [TestMethod]
        public void ThreadExecution_ThreadAbortedWhileExecutingRender_PropagatesThreadAbortException()
        {
            var input = "{{ 'test' | Abort }}";

            var engine = LavaTestEngineFactory.CreateFluidEngine();
            var methodInfo = GetType().GetMethod( nameof( ThreadAbortFilter ) );

            engine.RegisterFilter( methodInfo, "Abort" );

            var options = new LavaRenderOptions
            {
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput
            };

            try
            {
                LavaRenderTestHelper.Render( engine, input, options );

                Assert.Fail( "ThreadAbortException expected but not encountered." );
            }
            catch ( ThreadAbortException )
            {
                // Resetting the abort status is the only method of preventing this exception from being rethrown.
                Thread.ResetAbort();
            }
        }

        /// <summary>
        /// This filter simulates the effect of executing the PageRedirect filter in the web application.
        /// A redirect terminates the current thread and starts a new thread to service the redirect page.
        /// </summary>
        /// <param name="input">The filter input, which is ignored.</param>
        /// <returns>Never returns; the thread is aborted first.</returns>
        public static string ThreadAbortFilter( string input )
        {
            Thread.CurrentThread.Abort();

            return string.Empty;
        }

        [TestMethod]
        public void ParallelExecution_ShortcodeWithParameters_ResolvesParameterCorrectly()
        {
            var shortcodeTemplate = """
                Font Name: {{ fontname }}
                Font Size: {{ fontsize }}
                Font Bold: {{ fontbold }}
                """;

            var input = """
                {[ shortcodetest fontname:'Arial' fontsize:'{{ fontsize }}' fontbold:'true' ]}
                {[ endshortcodetest ]}
                """;

            /*
                9/27/26 - CLAUDE

                The font size is asserted as the iteration's own value rather than
                as a wildcard. This test exists to prove that concurrent renders do
                not see each other's merge fields, and a wildcard matches one
                thread's value as readily as another's, so the defect the test is
                named for could not fail it.

                Reason: Assert the value this iteration supplied, not any value.
            */
            var expectedTemplate = """
                Font Name: Arial
                Font Size: {fontsize}
                Font Bold: true
                """.NormalizeLineEndings();

            var shortcodeDefinition = new DynamicShortcodeDefinition
            {
                ElementType = LavaShortcodeTypeSpecifier.Block,
                TemplateMarkup = shortcodeTemplate,
                Name = "shortcodetest",
                Parameters = new Dictionary<string, string> { { "fontname", "Arial" }, { "fontsize", "0" }, { "fontbold", "true" } }
            };

            var engine = LavaTestEngineFactory.CreateFluidEngine();

            engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => shortcodeDefinition );

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 100 };

            Parallel.For( 1, 1000, parallelOptions, ( x ) =>
            {
                var options = new LavaRenderOptions
                {
                    MergeFields = new Dictionary<string, object> { { "fontsize", x } }
                };

                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedTemplate.Replace( "{fontsize}", x.ToString() ), output );
            } );
        }

        [TestMethod]
        public void ParallelExecution_ShortcodeWithChildItems_EmitsCorrectHtml()
        {
            var shortcodeTemplate = """
                Parameter 1: {{ parameter1 }}
                Parameter 2: {{ parameter2 }}
                Items:
                {%- for item in items -%}
                {{ item.title }} - {{ item.content }}
                {%- endfor -%}
                """;

            var input = """
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
                """;

            // The item lines run together because the for loop in the shortcode
            // template is written with whitespace-trim markers, which consume the
            // newline after each iteration.
            //
            // As above, the iteration number is asserted as its own value rather
            // than as a wildcard, so that a render seeing another thread's
            // iteration fails the test.
            var expectedTemplate = """
                ***
                Iteration: {iteration}
                ***
                Parameter 1: value1
                Parameter 2: value2
                Items:Panel 1 - Panel 1 content.Panel 2 - Panel 2 content.Panel 3 - Panel 3 content.
                """.NormalizeLineEndings();

            var shortcodeDefinition = new DynamicShortcodeDefinition
            {
                ElementType = LavaShortcodeTypeSpecifier.Block,
                TemplateMarkup = shortcodeTemplate,
                Name = "shortcodetest",
                Parameters = new Dictionary<string, string> { { "parameter1", "value1" }, { "parameter2", "value2" } }
            };

            var engine = LavaTestEngineFactory.CreateFluidEngine();

            engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => shortcodeDefinition );

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Parallel.For( 0, 1000, parallelOptions, ( x ) =>
            {
                var options = new LavaRenderOptions
                {
                    MergeFields = new Dictionary<string, object> { { "iteration", x } }
                };

                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedTemplate.Replace( "{iteration}", x.ToString() ), output );
            } );
        }
    }
}
