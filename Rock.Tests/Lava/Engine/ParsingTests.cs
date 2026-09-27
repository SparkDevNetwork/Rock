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

namespace Rock.Tests.Lava.Engine
{
    /// <summary>
    /// Test the compatibility of the Lava parser with the Liquid language syntax.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class LiquidLanguageCompatibilityTests
    {
        #region Whitespace

        [TestMethod]
        public void Whitespace_TagLeftAndRight_ProducesCorrectOutput()
        {
            // The tag is wrapped in newlines so that the trim markers have
            // something on each side to remove. Without them the assertion would
            // hold whether or not the markers did anything.
            var input = "\n{%- assign now = 'Now' | Date:'yyyy-MM-ddTHH:mm:sszzz' | AsDateTime -%}\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        [TestMethod]
        public void Whitespace_TrimInOutputTagWithVariable_RemovesWhitespace()
        {
            var input = "{%- assign text = 'hello' -%}--> {{- text -}} <--";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "-->hello<--", output );
            } );
        }

        [TestMethod]
        [Ignore( "Not supported in Fluid. The empty output tag throws a parsing error." )]
        public void Whitespace_TrimInEmptyOutputTag_RemovesWhitespace()
        {
            var input = "{{- -}}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        [TestMethod]
        public void Whitespace_TrimInOutputTagWithEmptyString_RemovesWhitespace()
        {
            var input = "--> {{- '' -}} <--";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "--><--", output );
            } );
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
            var input = "-->  {%- comment %}  Comment text.  {% endcomment -%}  <--";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "--><--", output );
            } );
        }

        #endregion Whitespace

        #region Variables

        [TestMethod]
        public void Variables_VariableNamesThatDifferOnlyByCase_AreReferencedAsDifferentVariables()
        {
            var input = """
                {%- assign text = 'lowercase' -%}
                {%- assign TEXT = 'uppercase' -%}
                Text (lower) = {{ text }}, Text (upper) = {{ TEXT }}
                """;

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "Text (lower) = lowercase, Text (upper) = uppercase", output );
            } );
        }

        [TestMethod]
        public void Variables_VariableNameBeginningWithNumber_IsValid()
        {
            var input = "{% assign 1st = 'first' %}{{ 1st }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "first", output );
            } );
        }

        #endregion Variables

        #region Keywords

        [TestMethod]
        public void Keywords_ElseIfKeyword_IsParsedAsElsIf()
        {
            var input = """
                {% assign speed = 50 %}
                {% if speed > 70 -%}
                Fast
                {% elseif speed > 30 -%}
                Moderate
                {% else -%}
                Slow
                {% endif -%}
                """;

            // The surrounding newlines are written explicitly rather than as a raw
            // string literal, because the whitespace is the subject of the test and
            // a blank line inside a literal is invisible to a reader.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "\nModerate\n", output );
            } );
        }

        #endregion Keywords

        #region Operators

        [TestMethod]
        [DataRow( "true | AsBoolean", "true" )]
        [DataRow( "'true'", "true" )]
        [DataRow( "''", "true" )]
        public void Operators_IfWithNoOperatorAndAnyDefinedValue_ReturnsTrue( string value, string expectedResult )
        {
            var input = $$"""
                {% assign value = {{value}} %}
                {% if value %}true{% else %}false{% endif %}
                """;

            var expected = $"\n{expectedResult}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        [TestMethod]
        public void Operators_IfWithNoOperatorAndUndefinedVariable_ReturnsFalse()
        {
            var input = "{% if noVariable %}true{% else %}false{% endif %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "false", output );
            } );
        }

        [TestMethod]
        public void Operators_IfWithNoOperatorAndNullVariable_ReturnsFalse()
        {
            var input = "{% if value %}true{% else %}false{% endif %}";

            var options = new LavaRenderOptions
            {
                MergeFields = new Dictionary<string, object> { { "value", null } }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( "false", output );
            } );
        }

        #endregion Operators

        #region Tags

        [TestMethod]
        public void Tags_RawTagWithEmbeddedTag_ReturnsLiteralTagText()
        {
            var input = "{% raw %}{% assign test = 'hello' %}{% endraw %}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( "{% assign test = 'hello' %}", output );
            } );
        }

        /// <summary>
        /// Verify that a comment tag correctly ignores any other tags that are contained within it.
        /// </summary>
        [TestMethod]
        public void Tags_CommentTagContainingInvalidRawTag_IsParsedCorrectly()
        {
            var input = """
                Comment-->
                {% comment %}Open-ended tag-->{% raw %}, Invalid tag-->{% invalid_tag %}{% endcomment %}
                <--Comment
                """;

            var expected = """
                Comment-->

                <--Comment
                """.NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expected, output );
            } );
        }

        #endregion Tags

        #region LavaToLiquidTemplateConverter

        [TestMethod]
        public void LavaToLiquidConverter_LavaTemplateWithElseIfKeyword_IsReplacedWithElsif()
        {
            var input = """
                {% assign speed = 50 %}
                {% if speed > 70 -%}
                Fast
                {% elseif speed > 30 -%}
                Moderate
                {% else -%}
                Slow
                {% endif -%}
                """;

            var expected = """
                {% assign speed = 50 %}
                {% if speed > 70 -%}
                Fast
                {% elsif speed > 30 -%}
                Moderate
                {% else -%}
                Slow
                {% endif -%}
                """;

            var converter = new LavaToLiquidTemplateConverter();

            var output = converter.ReplaceElseIfKeyword( input );

            Assert.AreEqual( expected, output );
        }

        [TestMethod]
        public void LavaToLiquidConverter_LavaShortcodeWithMultipleParameters_IsReplacedWithRenamedBlock()
        {
            /*
                9/26/26 - CLAUDE

                The converter emits two spaces before the closing "%}" delimiter,
                and one before the closing "%}" of the end tag. This is harmless -
                Liquid ignores the extra whitespace inside a tag - but it is not
                intentional either. The predecessor test asserted with
                AreEqualIgnoreWhitespace, which could not see it.

                The expected values below record what the converter actually
                produces. If ReplaceTemplateShortcodes is ever tidied up, these are
                the assertions that will tell you.

                Reason: Record the converter's real output rather than a whitespace-
                insensitive approximation of it.
            */
            var input = "{[ shortcodetest fontname:'Arial' fontsize:'{{ fontsize }}' fontbold:'true' ]}{[ endshortcodetest ]}";
            var expected = "{% shortcodetest_ fontname:'Arial' fontsize:'{{ fontsize }}' fontbold:'true'  %}{% endshortcodetest_  %}";

            var converter = new LavaToLiquidTemplateConverter();

            var output = converter.ReplaceTemplateShortcodes( input );

            Assert.AreEqual( expected, output );
        }

        [TestMethod]
        public void LavaToLiquidConverter_LavaShortcodeWithNoParameters_IsReplacedWithRenamedBlock()
        {
            // See the note on the preceding test regarding the doubled space before
            // the closing delimiter.
            var input = "{[ shortcodetest ]}{[ endshortcodetest ]}";
            var expected = "{% shortcodetest_  %}{% endshortcodetest_  %}";

            var converter = new LavaToLiquidTemplateConverter();

            var output = converter.ReplaceTemplateShortcodes( input );

            Assert.AreEqual( expected, output );
        }

        #endregion LavaToLiquidTemplateConverter
    }

    /// <summary>
    /// Test the parsing of the parameters supplied to a Lava block or shortcode.
    /// </summary>
    [TestClass]
    public class ParameterParsingTests
    {
        [TestMethod]
        public void BlockParameters_WithDelimiterInParameterLavaValue_EvaluatesValueCorrectly()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var parameterString = "workflowtype:'51FE9641-FB8F-41BF-B09E-235900C3E53E' workflowname:'{{WorkflowName}}'";

                var mergeFields = new LavaDataDictionary
                {
                    { "WorkflowName", "Ted's Workflow" }
                };

                var context = engine.NewRenderContext( mergeFields );
                var settings = LavaElementAttributes.NewFromMarkup( parameterString, context );

                Assert.AreEqual( "Ted's Workflow", settings.GetString( "workflowname" ) );
            } );
        }

        [TestMethod]
        public void BlockParameters_NamesThatDifferOnlyByCase_AreReferencedAsTheSameParameter()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var parameterString = "param1:'1' PARAM2:'2'";

                var context = engine.NewRenderContext();
                var settings = LavaElementAttributes.NewFromMarkup( parameterString, context );

                Assert.AreEqual( "2", settings.GetString( "param2" ) );
            } );
        }

        [TestMethod]
        public void BlockParameters_WithLavaOutputTagContainingDelimiters_IsParsedCorrectly()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var parameterString = "where:'ContentChannelId == 1 && Title == \"{{ 'Blog Posts' }}\"' iterator:'items' sort:'StartDateTime'";

                var context = engine.NewRenderContext();
                var settings = LavaElementAttributes.NewFromMarkup( parameterString, context );

                Assert.AreEqual( "ContentChannelId == 1 && Title == \"Blog Posts\"", settings.GetStringOrNull( "where" ) );
                Assert.AreEqual( "items", settings.GetStringOrNull( "iterator" ) );
                Assert.AreEqual( "StartDateTime", settings.GetStringOrNull( "sort" ) );
            } );
        }

        [TestMethod]
        public void BlockParameters_WithUndelimitedParameterValue_IsParsedCorrectly()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var parameterString = "param1:1 param2:'2' param3:abc";

                var context = engine.NewRenderContext();
                var settings = LavaElementAttributes.NewFromMarkup( parameterString, context );

                Assert.AreEqual( "1", settings.GetStringOrNull( "param1" ) );
                Assert.AreEqual( "2", settings.GetStringOrNull( "param2" ) );
                Assert.AreEqual( "abc", settings.GetStringOrNull( "param3" ) );
            } );
        }

        [TestMethod]
        public void BlockParameters_WithEmptyParameterValue_IsParsedCorrectly()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var parameterString = "param1:'' param2:";

                var context = engine.NewRenderContext();
                var settings = LavaElementAttributes.NewFromMarkup( parameterString, context );

                Assert.AreEqual( "", settings.GetStringOrNull( "param1" ) );
                Assert.AreEqual( "", settings.GetStringOrNull( "param2" ) );
            } );
        }
    }
}
