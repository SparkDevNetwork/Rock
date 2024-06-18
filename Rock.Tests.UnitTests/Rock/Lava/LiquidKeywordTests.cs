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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Data;
using System.Collections.Generic;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Enums.Blocks.Crm.FamilyPreRegistration;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Lava
{
    /// <summary>
    /// Tests the processing of standard Liquid keywords where some variation has been identified between the Liquid frameworks supported by Rock.
    /// </summary>
    [TestClass]
    public class LiquidKeywordTests : LavaUnitTestBase
    {
        #region Case Statement

        /// <summary>
        /// Verifies the resolution of Issue #5232.
        /// https://github.com/SparkDevNetwork/Rock/issues/5232
        /// </summary>
        [TestMethod]
        public void LiquidCaseBlock_WithWhitespace_IsParsedCorrectly()
        {
            var template = @"
{% comment %}
The whitespace indentation in the case statement below causes a parsing error in the current version of the Fluid library (1.0.0-beta-9660).
{% endcomment %}
{% assign number = '2' %}
{% case number %}
    {% when '1' %}
        {% assign color = 'red' %}
    {% when '2' %}
        {% assign color = 'green' %}
    {% else %}
        {% assign color = 'orange' %}
{% endcase %}
Number: {{ number }}
Color: {{ color }}
";

            var expectedOutput = @"
Number: 2
Color: green
";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        [TestMethod]
        public void LiquidCaseBlock_WithMultipleMatchedCases_RendersAllMatches()
        {
            var template = @"
{% assign number = '1' %}
{% case number %}
    {% when '1' %}
        First Case matched.
    {% when '1' or '2' %}
        Second Case matched.
    {% when '3' %}
        Third Case matched.
    {% else %}
        No Case matched.
{% endcase %}
";

            var expectedOutput = @"
First Case matched. Second Case matched.
";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verifies the resolution of Issue #4910 (Part 2).
        /// https://github.com/SparkDevNetwork/Rock/issues/4910
        /// </summary>
        [TestMethod]
        public void LiquidCaseBlock_WithEnumCase_MatchesEquivalentValueInWhen()
        {
            var person = new
            {
                NickName = "Ted",
                CommunicationPreference = CommunicationPreference.Email
            };

            Assert.That.AreEqual( 1, (int)CommunicationPreference.Email );

            var template = @"
{{ person.NickName }}, your communication preference is:
{% case person.CommunicationPreference %} {% when 1 %}Email{% else %}Not Email!{% endcase %}
";

            var expectedOutput = @"
Ted, your communication preference is: Email
";

            var mergeDictionary = new LavaDataDictionary { { "person", person } };

            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeDictionary, ignoreWhitespace: true );
        }

        [TestMethod]
        public void LiquidCaseBlock_WithEnumCase_MatchesEquivalentNameInWhen()
        {
            var person = new
            {
                NickName = "Ted",
                CommunicationPreference = CommunicationPreference.Email
            };

            Assert.That.AreEqual( 1, ( int ) CommunicationPreference.Email );

            var template = @"
{{ person.NickName }}, your communication preference is:
{% case person.CommunicationPreference %} {% when 'Email' %}Email{% else %}Not Email!{% endcase %}
";

            var expectedOutput = @"
Ted, your communication preference is: Email
";

            var mergeDictionary = new LavaDataDictionary { { "person", person } };

            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeDictionary, ignoreWhitespace: true );
        }

        /// <summary>
        /// This test case reproduces the inverse of Issue #4910 (Part 2).
        /// This is not a reported use case, and is not currently supported in Fluid.
        /// </summary>
        [TestMethod]
        [Ignore( "The Fluid framework does not natively support using an Enum name in a case statement." )]
        public void LiquidCaseBlock_WithEnumWhen_MatchesEquivalentNameInCase()
        {
            var person = new
            {
                NickName = "Ted",
                CommunicationPreference = CommunicationPreference.Email
            };

            Assert.That.AreEqual( 1, ( int ) CommunicationPreference.Email );

            var template = @"
{{ person.NickName }}, your communication preference is:
{% case 'Email' %} {% when person.CommunicationPreference %}Email{% else %}Not Email!{% endcase %}
";

            var expectedOutput = @"
Ted, your communication preference is: Email
";

            var mergeDictionary = new LavaDataDictionary { { "person", person } };

            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeDictionary, ignoreWhitespace: true );
        }

        [TestMethod]
        public void LiquidCaseBlock_WithEnumWhen_MatchesEquivalentValueInCase()
        {
            var person = new
            {
                NickName = "Ted",
                CommunicationPreference = CommunicationPreference.Email
            };

            Assert.That.AreEqual( 1, ( int ) CommunicationPreference.Email );

            var template = @"
{{ person.NickName }}, your communication preference is:
{% case 1 %}{% when person.CommunicationPreference %}Email{% else %}Not Email!{% endcase %}
";

            var expectedOutput = @"
Ted, your communication preference is: Email
";

            var mergeDictionary = new LavaDataDictionary { { "person", person } };

            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeDictionary, ignoreWhitespace: true );
        }

        #endregion

        #region Cycle Tag

        /// <summary>
        /// Referencing a valid property of an input object should return the property value.
        /// </summary>
        [TestMethod]
        public void CycleTag_UngroupedCycle_ReturnsIteratedOutput()
        {
            var template = @"
{% cycle 'red', 'green', 'blue' %}
{% cycle 'red', 'green', 'blue' %}
{% cycle 'red', 'green', 'blue' %}
";

            var expectedOutput = @"
red
green
blue
";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );

        }

        /// <summary>
        /// Referencing a valid property of an input object should return the property value.
        /// </summary>
        [TestMethod]
        public void CycleTag_GroupedCycles_ReturnsPropertyValue()
        {
            var template = @"
{% cycle 'colors': 'red', 'green', 'blue' %} - {% cycle 'numbers': 'one', 'two', 'three' %}
{% cycle 'colors': 'red', 'green', 'blue' %} - {% cycle 'numbers': 'one', 'two', 'three' %}
{% cycle 'colors': 'red', 'green', 'blue' %} - {% cycle 'numbers': 'one', 'two', 'three' %}
";

            var expectedOutput = @"
red - one 
green - two
blue - three
";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );

        }

        #endregion

        #region Null/Nil/Empty/Blank

        /// <summary>
        /// Keyword "Null" is not case-sensitive.
        /// </summary>
        [TestMethod]
        public void Null_UpperCaseOrLowerCase_IsNotCaseSensitive()
        {
            var template = @"
{% assign value = 1 %}
{% if value != Null %}
True
{% endif %}
{% if value != null %}
true
{% endif %}

{% if value == Null %}
False
{% endif %}
{% if value == null %}
False
{% endif %}

";

            var expectedOutput = @"True true";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Keyword "empty" is not case-sensitive.
        /// </summary>
        [TestMethod]
        public void Empty_UpperCaseOrLowerCase_IsNotCaseSensitive()
        {

            var mergeValues = new LavaDataDictionary { { "Items", new List<string>() } };

            var template = @"
{% if Items == Empty %}
True
{% endif %}
{% if Items == empty %}
true
{% endif %}

{% if Items != Empty %}
False
{% endif %}
{% if Items != empty %}
false
{% endif %}
";

            var expectedOutput = @"True true";

            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeValues, ignoreWhitespace: true );
        }

        /// <summary>
        /// Keyword "blank" is not case-sensitive.
        /// </summary>
        [TestMethod]
        public void Blank_UpperCaseOrLowerCase_IsNotCaseSensitive()
        {

            var mergeValues = new LavaDataDictionary { { "Items", new List<string>() } };

            var template = @"
{% if Items == Blank %}
True
{% endif %}
{% if Items == blank %}
true
{% endif %}

{% if Items != Blank %}
False
{% endif %}
{% if Items != blank %}
false
{% endif %}
";

            var expectedOutput = @"True true";

            TestHelper.AssertTemplateOutput( expectedOutput, template, mergeValues, ignoreWhitespace: true );
        }

        /// <summary>
        /// Keyword "nil" is not case-sensitive.
        /// </summary>
        [TestMethod]
        public void Nil_UpperCaseOrLowerCase_IsNotCaseSensitive()
        {

            var template = @"
{% if undefinedVariable == Nil %}
True
{% endif %}
{% if undefinedVariable == nil %}
true
{% endif %}

{% if undefinedVariable != Nil %}
False
{% endif %}
{% if undefinedVariable != nil %}
false
{% endif %}
";

            var expectedOutput = @"True true";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Keyword "true" is not case-sensitive.
        /// </summary>
        [TestMethod]
        public void True_UpperCaseOrLowerCase_IsNotCaseSensitive()
        {
            var template = @"
{% assign value = true %}

{% if value == true %}
passed
{% endif %}
{% if value == True %}
Passed
{% endif %}
{% if value == TRUE %}
PASSED
{% endif %}

{% if value != true %}
failed
{% endif %}
{% if value != True %}
Failed
{% endif %}
{% if value != TRUE %}
FAILED
{% endif %}

";

            var expectedOutput = @"passed Passed PASSED";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Keyword "false" is not case-sensitive.
        /// </summary>
        [TestMethod]
        public void False_UpperCaseOrLowerCase_IsNotCaseSensitive()
        {
            var template = @"
{% assign value = false %}

{% if value == false %}
passed
{% endif %}
{% if value == False %}
Passed
{% endif %}
{% if value == FALSE %}
PASSED
{% endif %}

{% if value != false %}
failed
{% endif %}
{% if value != False %}
Failed
{% endif %}
{% if value != FALSE %}
FAILED
{% endif %}

";

            var expectedOutput = @"passed Passed PASSED";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// A variable name that includes a Liquid keyword should be parsed correctly as a variable name.
        /// </summary>
        [TestMethod]
        public void Keyword_IncludedInVariableName_IsParsedCorrectly()
        {
            var template = @"
{% assign emptyString = 'Empty String' %}{{ emptyString }}
{% assign trueString = 'True String' %}{{ trueString }}
";
            var expectedOutput = @"
Empty String
True String
";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        #endregion

        #region {% liquid %} and {% lava %} tag

        /// <summary>
        /// Verify that the {% liquid %} tag can be parsed correctly by the Lava Fluid engine.
        /// Our Lava Fluid parser has been modified to process open/close tokens in a non-standard way to implement shortcode syntax,
        /// so we need to verify that this also works for open/close tag tokens implied by the {% liquid %} tag.
        /// </summary>
        [TestMethod]
        public void LiquidTag_WithStandardSyntax_ShouldParseCorrectly()
        {
            var template = @"
{% liquid 
   echo 
      'welcome ' | Upcase 
   echo 'to the liquid tag' 
    | Upcase 
%}
";

            var expectedOutput = @"
WELCOME TO THE LIQUID TAG
";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify that the liquid tag correctly parses any block-type flow control constructs in its content.
        /// </summary>
        [TestMethod]
        public void LiquidTag_WithInnerBlockConstruct_ShouldParseCorrectly()
        {
            var template = @"
{% assign i = 3 %}

{% liquid
case i
    when 3 
        echo 'Match'
    else
        echo 'No Match'
endcase  %}
";

            var expectedOutput = @"
Match
";
            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// A shortcode that enables a specific command should not cause that command to be enabled outside the scope of the shortcode.
        /// </summary>
        [TestMethod]
        public void LiquidTag_WithInnerShortcode_ShouldRenderShortcodeOutput()
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
{% liquid
    shortcode_execute
%}
";

            var expectedOutput = "The answer is 42.";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // The RockLiquid engine does not support this tag.
                if ( engine.GetType() == typeof( RockLiquidEngine ) )
                {
                    return;
                }

                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                TestHelper.AssertTemplateOutput( engine, expectedOutput, input, ignoreWhitespace: true );
            } );
        }

        /// <summary>
        /// Verify that the {% lava %} tag is correctly aliased to the default {% liquid %} tag.
        /// </summary>
        [TestMethod]
        public void LavaTag_AsAliasForLiquidTag_IsProcessedAsLiquidTag()
        {
            var template = @"
{% lava 
   echo 
      'welcome ' | Upcase 
   echo 'to the lava tag' 
    | Upcase 
%}
";

            var expectedOutput = @"
WELCOME TO THE LAVA TAG
";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
        }

        #endregion

        #region Raw Tag

        /// <summary>
        /// The raw tag should preserve all whitespace in its content.
        /// </summary>
        [TestMethod]
        public void RawTag_ContainingWhitespace_PreservesWhitespace()
        {
            var template = @"{% raw %}{{- -}}{% endraw %}";

            var expectedOutput = @"{{- -}}";

            // This only works correctly in the Fluid engine.
            TestHelper.AssertTemplateOutput( typeof(FluidEngine), expectedOutput, template, ignoreWhitespace: false );

        }

        #endregion

    }
}
