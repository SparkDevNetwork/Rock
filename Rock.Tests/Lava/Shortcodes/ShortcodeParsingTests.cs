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

using Rock.Lava;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Shortcodes
{
    /// <summary>
    /// Tests for shortcode parsing and parameter resolution, using shortcodes the
    /// test defines for itself.
    /// </summary>
    /// <remarks>
    /// The tests that render a shortcode defined as a LavaShortcode row, or that
    /// depend on the execute or workflowactivate commands, stay in
    /// Rock.Tests.Integration.
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class ShortcodeParsingTests
    {
        [TestMethod]
        public void Shortcode_WithMergeFieldAsParameter_CorrectlyResolvesParameters()
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

            var expected = """
                Font Name: Arial
                Font Size: 99
                Font Bold: true
                """.NormalizeLineEndings();

            var shortcodeDefinition = new DynamicShortcodeDefinition
            {
                ElementType = LavaShortcodeTypeSpecifier.Block,
                TemplateMarkup = shortcodeTemplate,
                Name = "shortcodetest"
            };

            var options = new LavaRenderOptions
            {
                MergeFields = new Dictionary<string, object> { { "fontsize", 99 } }
            };

            var engine = LavaTestEngineFactory.CreateFluidEngine();

            engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => shortcodeDefinition );

            var output = LavaRenderTestHelper.Render( engine, input, options );

            Assert.AreEqual( expected, output );
        }

        [TestMethod]
        [TestCategory( "ShortcodeScopeBehavior" )]
        public void Shortcode_ReferencingItemFromParentScope_CorrectlyResolvesItem()
        {
            var shortcodeTemplate = "ValueInShortcodeScope = {{ Value }}";

            var input = """
                ValueInOuterScope = {{ Value }}
                {[ debug ]}
                """;

            var expected = """
                ValueInOuterScope = 99
                ValueInShortcodeScope = 99
                """.NormalizeLineEndings();

            var shortcodeDefinition = new DynamicShortcodeDefinition
            {
                ElementType = LavaShortcodeTypeSpecifier.Inline,
                TemplateMarkup = shortcodeTemplate,
                Name = "debug"
            };

            var options = new LavaRenderOptions
            {
                MergeFields = new Dictionary<string, object> { { "Value", 99 } }
            };

            var engine = LavaTestEngineFactory.CreateFluidEngine();

            engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => shortcodeDefinition );

            var output = LavaRenderTestHelper.Render( engine, input, options );

            Assert.AreEqual( expected, output );
        }

        /// <summary>
        /// Verify that an undefined shortcode name throws a shortcode parsing error.
        /// </summary>
        [TestMethod]
        public void ShortcodeParsing_UndefinedShortcodeTag_ThrowsUnknownShortcodeParsingError()
        {
            var input = """
                <p>Document start.</p>
                {[ testshortcode1 ]}
                <p>Document end.</p>
                """;

            var options = new LavaRenderOptions
            {
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Ignore
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var result = LavaRenderTestHelper.RenderResult( engine, input, options );

                Assert.IsInstanceOfType<LavaException>( result.Error, "Lava Exception expected but not encountered." );
                Assert.Contains( "Unknown shortcode 'testshortcode1'", result.Error.Message, "Unexpected Lava error message." );
            } );
        }

        /// <summary>
        /// Verify that an undefined shortcode name throws a parsing error that names
        /// the shortcode even when it is embedded in an if/endif block.
        /// </summary>
        [TestMethod]
        public void ShortcodeParsing_UndefinedShortcodeEmbeddedInIfBlock_ThrowsCorrectParsingError()
        {
            var input = """
                {% if 1 == 1 %}
                    {[ invalidshortcode ]}
                {% endif %}
                """;

            var options = new LavaRenderOptions
            {
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Ignore
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var result = LavaRenderTestHelper.RenderResult( engine, input, options );

                Assert.IsInstanceOfType<LavaException>( result.Error, "Lava Exception expected but not encountered." );
                Assert.Contains( "Unknown shortcode 'invalidshortcode'", result.Error.Message, "Unexpected Lava error message." );
            } );
        }
    }
}
