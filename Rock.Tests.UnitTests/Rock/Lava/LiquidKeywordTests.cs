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
        /// Verify the resolution of a specific issue in the Fluid framework where a {% case %} statement containing whitespace throws a parsing error.
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

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace:true );
        }

        #endregion
    }
}
