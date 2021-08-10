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

    }
}
