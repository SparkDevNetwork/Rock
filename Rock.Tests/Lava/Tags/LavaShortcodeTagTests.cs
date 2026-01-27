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
using Rock.Tests.Shared;

namespace Rock.Tests.Lava.Tags
{
    /// <summary>
    /// Verifies Lava shortcode behavior.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class LavaShortcodeTagTests : LavaUnitTestBase
    {
        /// <summary>
        /// A dynamic shortcode should produce the expected output.
        /// </summary>
        [TestMethod]
        public void CustomDynamicLavaShortCode_ProducesExpectedOutput()
        {
            var shortcodeTemplate = @"
{% assign x = 42 %}
The answer is {{ x }}.
";
            // Create a new test shortcode with the "execute" command permission.
            var shortcodeDefinition = new DynamicShortcodeDefinition();

            shortcodeDefinition.ElementType = LavaShortcodeTypeSpecifier.Inline;
            shortcodeDefinition.TemplateMarkup = shortcodeTemplate;
            shortcodeDefinition.Name = "shortcode_execute";

            var input = @"
{[ shortcode_execute ]}
";
            var expectedOutput = "The answer is 42.";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, ignoreWhitespace: true );
            } );
        }
    }
}
