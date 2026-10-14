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

using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Engine
{
    /// <summary>
    /// Tests for the scoping of variables in a Lava context.
    /// </summary>
    /// <remarks>
    /// The test covering a variable referenced from inside an execute block stays
    /// in Rock.Tests.Integration, because the C# it executes queries a person.
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class ScopeTests
    {
        /// <summary>
        /// Documents a valid but unexpected behavior of the Assign statement in Liquid.
        /// </summary>
        /// <remarks>
        /// The Assign statement either creates or replaces an existing variable of
        /// the same name, in the current scope or any higher level scope. The
        /// nesting level at which an Assign statement first creates an internal
        /// variable determines the nesting level of that variable for the entire
        /// document. This does not align with the scoping rules for other
        /// programming languages.
        /// </remarks>
        [TestMethod]
        public void Assign_InnerScopeAssign_ModifiesOuterVariable()
        {
            var input = """
                Context Value (Level 0): {{ currentBlock }}
                {%- assign currentBlock = 'document_1' -%}
                Document Value (Level 1): {{ currentBlock }}
                {%- for i in (1..3) -%}
                    {%- assign currentBlock = i | Prepend:'for_loop_' -%}
                    Loop Value (Level 2): {{ currentBlock }}
                {%- endfor -%}
                Document Value (Level 1): {{ currentBlock }}
                {%- assign currentBlock = 'document_2' -%}
                Document Value (Level 1): {{ currentBlock }}
                """;

            /*
                9/26/26 - CLAUDE

                Every tag in this template carries whitespace-trim markers, so the
                engine emits no line breaks at all - the output is one unbroken
                line. The expected value is written as concatenated parts so that it
                stays readable while remaining exact.

                The predecessor test compared with all whitespace stripped from both
                sides, and its expected value was laid out over several lines as
                though the engine produced them. It did not.

                Reason: Assert the output the engine actually emits, not a
                prettified version of it.
            */
            var expected =
                "Context Value (Level 0): context" +
                "Document Value (Level 1): document_1" +
                "Loop Value (Level 2): for_loop_1" +
                "Loop Value (Level 2): for_loop_2" +
                "Loop Value (Level 2): for_loop_3" +
                "Document Value (Level 1): for_loop_3" +
                "Document Value (Level 1): document_2";

            var options = new LavaRenderOptions
            {
                MergeFields = new Dictionary<string, object> { { "currentBlock", "context" } }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expected, output );
            } );
        }

        [TestMethod]
        public void ForLoop_InnerLoop_Works()
        {
            var input = """
                {%- for i in (1..3) -%}
                > Outer Loop Value: {{ i }}
                    {%- for j in (1..3) -%}
                --> Inner Loop Value: {{i }}.{{ j }}
                    {%- endfor -%}
                {%- endfor -%}
                """;

            // As above, the trim markers leave no line breaks in the output.
            var expected =
                "> Outer Loop Value: 1" +
                "--> Inner Loop Value: 1.1" +
                "--> Inner Loop Value: 1.2" +
                "--> Inner Loop Value: 1.3" +
                "> Outer Loop Value: 2" +
                "--> Inner Loop Value: 2.1" +
                "--> Inner Loop Value: 2.2" +
                "--> Inner Loop Value: 2.3" +
                "> Outer Loop Value: 3" +
                "--> Inner Loop Value: 3.1" +
                "--> Inner Loop Value: 3.2" +
                "--> Inner Loop Value: 3.3";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }
    }
}
