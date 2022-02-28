﻿// <copyright>
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
using Rock.Lava.Fluid;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Test the compatibility of the Lava parser with the Liquid language syntax.
    /// </summary>
    [TestClass]
    public class LiquidLanguageCompatibilityTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void Variables_VariableNamesThatDifferOnlyByCase_AreReferencedAsDifferentVariables()
        {
            var input = @"
{%- assign text = 'lowercase' -%}
{%- assign TEXT = 'uppercase' -%}
Text (lower) = {{ text }}, Text (upper) = {{ TEXT }}
";
            var expectedOutput = @"Text (lower) = lowercase, Text (upper) = uppercase";

            TestHelper.AssertTemplateOutput( expectedOutput, input, new LavaTestRenderOptions { IgnoreWhiteSpace = false } );
        }


        [TestMethod]
        public void Whitespace_TrimInOutputTagWithVariable_RemovesWhitespace()
        {
            var input = @"{%- assign text = 'hello' -%}--> {{- text -}} <--";
            var expectedOutput = @"-->hello<--";

            TestHelper.AssertTemplateOutput( expectedOutput, input, new LavaTestRenderOptions { IgnoreWhiteSpace = false } );
        }

        [TestMethod]
        [Ignore("Not supported in Fluid. The empty output tag throws a parsing error.")]
        public void Whitespace_TrimInEmptyOutputTag_RemovesWhitespace()
        {
            var input = @"{{- -}}";
            var expectedOutput = @"";

            TestHelper.AssertTemplateOutput( expectedOutput, input, new LavaTestRenderOptions { IgnoreWhiteSpace = false } );
        }

        [TestMethod]
        public void Whitespace_TrimInOutputTagWithEmptyString_RemovesWhitespace()
        {
            var input = @"--> {{- '' -}} <--";
            var expectedOutput = @"--><--";

            TestHelper.AssertTemplateOutput( expectedOutput, input, new LavaTestRenderOptions { IgnoreWhiteSpace = false } );
        }

        /// <summary>
        /// Verify the operation of the whitespace trim character (-) when used in a comment tag.
        /// </summary>
        /// <remarks>
        /// This represents valid Liquid syntax that failed to parse in Fluid v1.
        /// The behavior has been fixed in Fluid v2.
        /// </remarks>
        [TestMethod]
        public void Whitespace_TrimInCommentTag_RemovesWhitespace()
        {
            var input = @"
-->  {%- comment %}  Comment text.  {% endcomment -%}  <--
";

            TestHelper.AssertTemplateOutput( "--><--", input, new LavaTestRenderOptions { IgnoreWhiteSpace = false } );
        }

        [TestMethod]
        public void Keywords_ElseIfKeyword_IsParsedAsElsIf()
        {
            var input = @"
{% assign speed = 50 %}
{% if speed > 70 -%}
Fast
{% elseif speed > 30 -%}
Moderate
{% else -%}
Slow
{% endif -%}
";
            var expectedOutput = @"Moderate";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void Operators_IfWithNoOperator_BooleanTrueIsParsedAsTruthy()
        {
            var input = @"
{% assign isTruthy = true | AsBoolean %}
{% if isTruthy %}true{% else %}false{% endif %}
";

            TestHelper.AssertTemplateOutput( "true", input );
        }

        [TestMethod]
        public void Operators_IfWithNoOperator_StringWithContentIsParsedAsTruthy()
        {
            var input = @"
{% assign isTruthy = 'true' %}
{% if isTruthy %}true{% else %}false{% endif %}
";

            TestHelper.AssertTemplateOutput( "true", input );
        }

        /// <summary>
        /// The double-ampersand (&&) boolean comparison syntax for "and" is not recognized Liquid syntax.
        /// It is also not part of the documented Lava syntax, but it is supported by the DotLiquid framework and has been found in some existing core templates.
        /// This test is designed to document the expected behavior.
        /// </summary>
        [TestMethod]

        [Ignore("Supported in DotLiquid, but not in Fluid. This syntax is not officially supported in Liquid or Lava.")]
        public void Operators_ConditionalExpressionUsingDoubleAmpersand_EmitsErrorMessage()
        {
            var input = @"
{% assign speed = 50 %}
{% if speed > 40 && speed < 60 -%}
Illegal Boolean Operator!
{% endif -%}
";

            // This test does not apply to the DotLiquid framework.
            if ( LavaIntegrationTestHelper.FluidEngineIsEnabled )
            {
                TestHelper.AssertTemplateIsInvalid( typeof( FluidEngine ), input );
            }
        }

        /// <summary>
        /// Comment tags containing nested comment tags do not parse correctly in Shopify Liquid.
        /// DotLiquid correctly parses these tags, but Fluid does not.
        /// </summary>
        [TestMethod]
        [Ignore( "Supported in DotLiquid, but not in Fluid. This tag structure is not officially supported in Liquid or Lava." )]
        public void Tags_NestedCommentTags_AreProcessedAsCommentContent()
        {
            var input = @"
{%comment%}Outer Comment{%comment%}Inner Comment{%endcomment%}{%endcomment%}
";

            TestHelper.AssertTemplateOutput( string.Empty, input );
        }

        /// <summary>
        /// In Lava, the ">" operator returns true for a string comparison if the left value would be sorted after the right value.
        /// Default Liquid syntax does not support string comparison using this operator.
        /// Therefore, the condition "{% if mystring > '' %}" returns false, even if "mystring" is assigned a value.
        /// This operator is supported by the DotLiquid framework.
        /// </summary>
        [TestMethod]
        [Ignore( "Supported in DotLiquid, but not in Fluid. This syntax is not officially supported in Liquid or Lava." )]
        public void Operators_GreaterThanStringComparison_IsProcessedCorrectly()
        {
            var input = @"
{% if 'abc' > 'aba' -%}
abc > aba.
{% endif -%}
{% if 'abc' >= 'abc' -%}
abc >= abc.
{% endif -%}
{% if 'abc' > 'abd' -%}
abc > abd (!!!)
{% endif -%}
";

            var expectedOutput = @"
abc > aba.
abc >= abc.
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        /// <summary>
        /// In Lava, the "<" operator returns true for a string comparison if the left value would be sorted before the right value.
        /// Default Liquid syntax does not support string comparison using this operator.
        /// Therefore, the condition "{% if 'a' < 'b' %}" returns false.
        /// This operator is supported natively by the DotLiquid framework.
        /// </summary>
        [TestMethod]
        [Ignore( "Supported in DotLiquid, but not in Fluid. This syntax is not officially supported in Liquid or Lava." )]
        public void Operators_LessThanStringComparison_IsProcessedCorrectly()
        {
            var input = @"
{% if 'abc' < 'abd' -%}
abc < abd.
{% endif -%}
{% if 'abc' <= 'abc' -%}
abc <= abc.
{% endif -%}
{% if 'abc' < 'abb' -%}
abc < abb (!!!)
{% endif -%}
";

            var expectedOutput = @"
abc < abd.
abc <= abc.
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        [Ignore( "Requires a fix for the Fluid library. Tags embedded in a raw tag are incorrectly parsed by the Fluid engine." )]
        public void Tags_RawTagWithEmbeddedTag_ReturnsLiteralTagText()
        {
            var inputTemplate = @"
{% capture lava %}{% raw %}{% assign test = 'hello' %}{{ test }}{% endraw %}{% endcapture %}
{{ lava | RunLava }}
";

            TestHelper.AssertTemplateOutput( "hello", inputTemplate );
        }

        [TestMethod]
        public void LavaToLiquidConverter_LavaTemplateWithElseIfKeyword_IsReplacedWithElsif()
        {
            var input = @"
{% assign speed = 50 %}
{% if speed > 70 -%}
Fast
{% elseif speed > 30 -%}
Moderate
{% else -%}
Slow
{% endif -%}
";
            var expectedOutput = @"
{% assign speed = 50 %}
{% if speed > 70 -%}
Fast
{% elsif speed > 30 -%}
Moderate
{% else -%}
Slow
{% endif -%}
";

            var converter = new LavaToLiquidTemplateConverter();

            var output = converter.ReplaceElseIfKeyword( input );

            Assert.That.AreEqual( expectedOutput, output );
        }

        [TestMethod]
        public void LavaToLiquidConverter_LavaShortcodeWithMultipleParameters_IsReplacedWithRenamedBlock()
        {
            var input = @"{[ shortcodetest fontname:'Arial' fontsize:'{{ fontsize }}' fontbold:'true' ]}{[ endshortcodetest ]}";
            var expectedOutput = @"{% shortcodetest_ fontname:'Arial' fontsize:'{{ fontsize }}' fontbold:'true' %}{% endshortcodetest_ %}";

            var converter = new LavaToLiquidTemplateConverter();

            var output = converter.ReplaceTemplateShortcodes( input );

            Assert.That.AreEqualIgnoreWhitespace( expectedOutput, output );
        }

        [TestMethod]
        public void LavaToLiquidConverter_LavaShortcodeWithNoParameters_IsReplacedWithRenamedBlock()
        {
            var input = @"{[ shortcodetest ]}{[ endshortcodetest ]}";
            var expectedOutput = @"{% shortcodetest_ %}{% endshortcodetest_ %}";

            var converter = new LavaToLiquidTemplateConverter();

            var output = converter.ReplaceTemplateShortcodes( input );

            Assert.That.AreEqualIgnoreWhitespace( expectedOutput, output );
        }
    }
}
