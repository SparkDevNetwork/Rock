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

namespace Rock.Tests.Integration.Core.Lava.Engine
{
    /// <summary>
    /// Tests for Lava comments that appear inside a shortcode.
    /// </summary>
    /// <remarks>
    /// The panel and accordion shortcodes used here are LavaShortcode rows rather
    /// than classes, so they can only be registered against a database. Every
    /// other Lava comment test lives in Rock.Tests as a unit test.
    /// </remarks>
    [TestClass]
    public class LavaCommentsInShortcodeTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void ShorthandLineComment_InShortcodeItem_IsIgnored()
        {
            var input = @"
{[ accordion ]}
    [[ item title:'Item 1' ]]
        <p>This is an item.</p>
        //- this is a single line comment in a block shortcode item
    [[ enditem ]]
{[ endaccordion ]}
";

            var expectedOutput = "\n"
                + "<div class=\"panel-group\" id=\"accordion-id-<guid>\" role=\"tablist\" aria-multiselectable=\"true\"><div class=\"panel panel-default\">\n"
                + "        <div class=\"panel-heading\" role=\"tab\" id=\"heading1-id-<guid>\">\n"
                + "          <h4 class=\"panel-title\">\n"
                + "            <a role=\"button\" data-toggle=\"collapse\" data-parent=\"#accordion-id-<guid>\" href=\"#collapse1-id-<guid>\" aria-expanded=\"true\" aria-controls=\"collapse1\">\n"
                + "              Item 1\n"
                + "            </a>\n"
                + "          </h4>\n"
                + "        </div>\n"
                + "        <div id=\"collapse1-id-<guid>\" class=\"panel-collapse collapse in\" role=\"tabpanel\" aria-labelledby=\"heading1-id-<guid>\">\n"
                + "          <div class=\"panel-body\">\n"
                + "            <p>This is an item.</p>\n"
                + "          </div>\n"
                + "        </div>\n"
                + "      </div></div>\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                // The accordion generates an identifier per render.
                LavaAssert.Matches( expectedOutput, output, "<guid>" );
            } );
        }

        [TestMethod]
        public void ShorthandBlockComment_InShortcode_DoesNotRender()
        {
            var input = @"
{[ panel title:'Test' ]}
Line 1<br>
Line 2 Start<br>/- This is an inline block comment -/Line 2 End<br>
Line 3<br>
{[ endpanel ]}
";

            var expectedOutput = "\n"
                + "<div class=\"panel panel-default\" >\n"
                + "  \n"
                + "      <div class=\"panel-heading\">\n"
                + "        <h3 class=\"panel-title\">\n"
                + "            \n"
                + "            Test</h3>\n"
                + "      </div>\n"
                + "  <div class=\"panel-body\">\n"
                + "    Line 1<br>\n"
                + "Line 2 Start<br>Line 2 End<br>\n"
                + "Line 3<br>\n"
                + "  </div>\n"
                + "  \n"
                + "</div>\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void ShorthandLineComment_InShortcode_DoesNotRender()
        {
            var input = @"
{[ panel title:'Test' ]}
Line 1<br>
Line 2<br>//- This is a single line comment.
Line 3<br>
{[ endpanel ]}
";

            var expectedOutput = "<div class=\"panel panel-default\" >\n"
                + "  \n"
                + "      <div class=\"panel-heading\">\n"
                + "        <h3 class=\"panel-title\">\n"
                + "            \n"
                + "            Test</h3>\n"
                + "      </div>\n"
                + "  <div class=\"panel-body\">\n"
                + "    Line 1<br>\n"
                + "Line 2<br>\n"
                + "Line 3<br>\n"
                + "  </div>\n"
                + "  \n"
                + "</div>";

            input = input.Trim();
            expectedOutput = expectedOutput.Trim().Replace( "`", @"""" );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expectedOutput, output );
            } );
        }
    }
}
