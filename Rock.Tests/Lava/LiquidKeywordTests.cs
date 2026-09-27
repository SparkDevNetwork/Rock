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

using Rock.Enums.Blocks.Crm.FamilyPreRegistration;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava
{
    /// <summary>
    /// Tests the processing of standard Liquid keywords where some variation has been identified between the Liquid frameworks supported by Rock.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class LiquidKeywordTests
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

            var expectedOutput = "\n\n\n\n        \n    \nNumber: 2\nColor: green\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            // Both the "1" and the "1 or 2" cases match, and each keeps the
            // indentation and trailing newline of the source template.
            var expectedOutput = "\n\n\n        First Case matched.\n    \n        Second Case matched.\n    \n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            var template = @"
{{ person.NickName }}, your communication preference is:
{% case person.CommunicationPreference %} {% when 1 %}Email{% else %}Not Email!{% endcase %}
";

            var expectedOutput = "\nTed, your communication preference is:\nEmail\n";

            var mergeDictionary = new LavaDataDictionary { { "person", person } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeDictionary };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void LiquidCaseBlock_WithEnumCase_MatchesEquivalentNameInWhen()
        {
            var person = new
            {
                NickName = "Ted",
                CommunicationPreference = CommunicationPreference.Email
            };

            var template = @"
{{ person.NickName }}, your communication preference is:
{% case person.CommunicationPreference %} {% when 'Email' %}Email{% else %}Not Email!{% endcase %}
";

            var expectedOutput = "\nTed, your communication preference is:\nEmail\n";

            var mergeDictionary = new LavaDataDictionary { { "person", person } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeDictionary };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void LiquidCaseBlock_WithEnumWhen_MatchesEquivalentValueInCase()
        {
            var person = new
            {
                NickName = "Ted",
                CommunicationPreference = CommunicationPreference.Email
            };

            var template = @"
{{ person.NickName }}, your communication preference is:
{% case 1 %}{% when person.CommunicationPreference %}Email{% else %}Not Email!{% endcase %}
";

            var expectedOutput = "\nTed, your communication preference is:\nEmail\n";

            var mergeDictionary = new LavaDataDictionary { { "person", person } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeDictionary };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
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
".NormalizeLineEndings();

            var expectedOutput = @"
red
green
blue
".NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );

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

            var expectedOutput = "\nred - one\ngreen - two\nblue - three\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );

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

            var expectedOutput = "\n\n\nTrue\n\n\ntrue\n\n\n\n\n\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            var expectedOutput = "\n\nTrue\n\n\ntrue\n\n\n\n\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            var expectedOutput = "\n\nTrue\n\n\ntrue\n\n\n\n\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            var expectedOutput = "\n\nTrue\n\n\ntrue\n\n\n\n\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            // Each if/else branch leaves the newline that followed its tag, so the
            // three matched words arrive separated by blank lines.
            var expectedOutput = "\n\n\n\npassed\n\n\nPassed\n\n\nPASSED\n\n\n\n\n\n\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            // Each if/else branch leaves the newline that followed its tag, so the
            // three matched words arrive separated by blank lines.
            var expectedOutput = "\n\n\n\npassed\n\n\nPassed\n\n\nPASSED\n\n\n\n\n\n\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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
".NormalizeLineEndings();
            var expectedOutput = @"
Empty String
True String
".NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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
".NormalizeLineEndings();

            var expectedOutput = @"
WELCOME TO THE LIQUID TAG
".NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            var expectedOutput = "\n\n\nMatch\n";
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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

            var expectedOutput = "\nThe answer is 42.\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                engine.RegisterShortcode( shortcodeDefinition.Name, ( shortcodeName ) => { return shortcodeDefinition; } );

                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Verify that the {% liquid %} tag correctly parses comments within its content.
        /// </summary>
        [TestMethod]
        public void LiquidTag_WithComment_ShouldParseCorrectly()
        {
            var template = @"
{% 
   liquid 
    comment
      this is a comment
    endcomment

   echo 
      'welcome ' | Upcase 
   echo 'to the liquid tag' 
    | Upcase 
%}";
            var expectedOutput = "\nWELCOME TO THE LIQUID TAG";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
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
".NormalizeLineEndings();

            var expectedOutput = @"
WELCOME TO THE LAVA TAG
".NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void LavaTag_WithInnerForLoop_ShouldParseCorrectly()
        {
            var template = @"
{% liquid 
    for i in (1..5)
        if i > 3
            continue
        endif
        echo i
    endfor
%}";
            var expectedOutput = "\n123";
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Verify that the {% lava %} tag is correctly aliased to the default {% liquid %} tag.
        /// </summary>
        [TestMethod]
        public void LavaTag_WrappedWithForLoop_IsProcessedCorrectly()
        {
            var template = @"
{%- for i in (1..5) %}
    {%- liquid 
        if i > 3
            continue
        endif
        echo i
    %}
{%- endfor %}
";

            var expectedOutput = "123\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Verify that Rock's custom "elseif" works inside a {% lava %} {% liquid %} tag.
        /// </summary>
        [TestMethod]
        public void LavaTag_WithInnerIfElseIfTag_IsProcessedCorrectly()
        {

            var mergeValues = new LavaDataDictionary { { "CurrentPerson", LavaTestData.GetTestPersonTedDecker() } };

            var template = @"
{% liquid

    assign lastName = CurrentPerson.LastName
    if lastName == ''
        assign result = 'nope'
    else
        if CurrentPerson.NickName == 'Cindy'
            assign result = 'female'
        elsif CurrentPerson.NickName == 'Ted'
            assign result = 'male'
        else
            assign result = 'unknown'
        endif
    endif

    echo result
%}
";
            var expectedOutput = "\nmale\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );

        }

        /// <summary>
        /// Verify that Rock's custom "//-" comments works inside an IF tag that is inside a {% lava %} {% liquid %} tag.
        /// </summary>
        [TestMethod]
        public void LavaTag_WithInnerIfTagAndLineComments_IsProcessedCorrectly()
        {
            var template = @"
{% liquid

    //- Comment level one
    assign isTest = true
    if isTest
        
        //- Comment level two
        assign isTest = false
        
    endif
    echo isTest

%}
";
            var expectedOutput = "\nfalse\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }


        /// <summary>
        /// Verify that Rock's custom "/- -/" block comments works inside an IF tag that is inside a {% lava %} {% liquid %} tag.
        /// </summary>
        [TestMethod]
        public void LavaTag_WithCommentesNestedInIfTag_IsProcessedCorrectly()
        {
            var template = @"
{% lava
    //- Comment level one
    assign isTest = true
    if isTest
        
        /-
        assign isTest = false
        -/

    endif
    echo isTest
%}
";
            var expectedOutput = "\ntrue\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );

        }

        /// <summary>
        /// Verify that Rock's custom "/- -/" block comments works inside an IF tag that is inside a {% lava %} {% liquid %} tag.
        /// </summary>
        [TestMethod]
        public void LavaTag_WithInnerIfTagAndBlockComment_SpanningMultipleLines_IsProcessedCorrectly()
        {
            var template = @"
{% liquid
    assign isTest = true
    /- This is a block comment...
   ... spanning multiple lines. -/
    if isTest
        
        assign isTest = false
        
    endif
    echo isTest
%}
";
            var expectedOutput = "\nfalse\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Verify that Rock's custom "//-" block comments works inside a FOR tag that is inside a {% lava %} {% liquid %} tag.
        /// </summary>
        [TestMethod]
        public void LavaTag_WithSingleLineCommentInsideForTag_IsProcessedCorrectly()
        {
            var template = @"
{% lava
    //- Comment level one
    assign test = -1
    assign loopCount = 5
    for i in (0..loopCount)
        
        //- Comment level two
        assign test = i
        
    endfor
    echo test
%}
";
            var expectedOutput = "\n5\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Verify that Rock's custom "//-" block comments works inside a FOR tag that is inside a {% lava %} {% liquid %} tag.
        /// </summary>
        [TestMethod]
        public void LavaTag_WithCommentBlockInsideForTag_IsProcessedCorrectly()
        {
            var template = @"
{% lava
    //- Comment level one
    assign test = -1
    assign loopCount = 5
    for i in (0..loopCount)
        
        /-
        assign test = i
        -/
        
    endfor
    echo test
%}
";
            var expectedOutput = "\n-1\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Verify that Liquid's "#" comments works inside a {% lava %} {% liquid %} tag.
        /// NOTE: This is not supported in Rock (or Fluid as far as I can tell).
        /// </summary>
        [TestMethod]
        [Ignore] // Ignored because this is not currently supported in Fluid.
        public void LavaTag_WithSingleLineHashComment_IsProcessedCorrectly()
        {
            var template = @"
{% liquid
    
    # This is a comment
    assign test = 5
        
    echo test
%}
";
            var expectedOutput = @"5";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void LavaTag_WithInnerCustomLavaBlock_RendersBlockContentUsingLiquidBodyGrammar()
        {
            var template = @"
{% lava
    testpassthrough
        assign greeting = 'hello'
        echo greeting | Upcase
    endtestpassthrough
%}
";

            var expectedOutput = "\nHELLO\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                engine.RegisterBlock( "testpassthrough", ( blockName ) => new TestPassthroughBlock() );

                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void LavaTag_WithInnerCustomLavaBlockContainingForLoop_RendersCorrectly()
        {
            var template = @"
{% lava
    testpassthrough
        for i in (1..4)
            echo i
        endfor
    endtestpassthrough
%}
";

            var expectedOutput = "\n1234\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                engine.RegisterBlock( "testpassthrough", ( blockName ) => new TestPassthroughBlock() );

                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }


        /// <summary>
        /// Verifies the resolution of Issue #6993 (Part 1).
        /// Verify that a {% comment %}...{% endcomment %} block written in line-delimited form
        /// inside a {% lava %} tag parses correctly (this regressed in v19 when Fluid was upgraded).
        /// The comment body should be skipped, and the statements after it should still run.
        /// </summary>
        [TestMethod]
        public void LavaTag_WithAssignmentCommenttedOut_IsProcessedCorrectly()
        {
            var template = @"
{% lava
    assign fruit = 'x,y,z' | Split:','
    comment
        assign fruit = 'apple,banana,cherry' | Split:','
    endcomment

    for i in fruit
        echo i
    endfor
%}
";
            // If the parser mistakenly executed the commented assignment we would see 'applebananacherry'.
            // The correct behavior is that only the assignment AFTER the comment block takes effect.
            var expectedOutput = "\nxyz\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Verifies the resolution of Issue #6993 (Part 2).
        /// Control test: the same {% lava %} body without the surrounding {% comment %} block
        /// should execute normally and echo every value.
        /// </summary>
        [TestMethod]
        public void LavaTag_WithAssignmentWithoutComment_IsProcessedCorrectly()
        {
            var template = @"
{% lava
    assign fruit = 'x,y,z' | Split:','
    assign fruit = 'apple,banana,cherry' | Split:','
    for i in fruit
        echo i
    endfor
%}
";
            var expectedOutput = "\napplebananacherry\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
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
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );

        }

        #endregion

        private class TestPassthroughBlock : LavaBlockBase
        {
            public TestPassthroughBlock()
            {
                SourceElementName = "testpassthrough";
            }
        }
    }
}


