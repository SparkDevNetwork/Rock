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

using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Lava
{
    /// <summary>
    /// Tests the accessibility of different container-type variables when resolving a Lava template.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class IfTagTests : LavaUnitTestBase
    {
        /// <summary>
        /// Tests the Liquid standard if / else
        /// </summary>
        [TestMethod]
        public void IfTag_IfElse_ShouldIf()
        {
            TestHelper.AssertTemplateOutput( " CORRECT ", "{% if true %} CORRECT {% else %} NO {% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / else
        /// </summary>
        [TestMethod]
        public void IfTag_IfElse_ShouldElse()
        {
            TestHelper.AssertTemplateOutput( " CORRECT ", "{% if false %} NO {% else %} CORRECT {% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / elsif / else
        /// </summary>
        [TestMethod]
        public void IfTag_IfElsIf_ShouldIf()
        {
            TestHelper.AssertTemplateOutput( "CORRECT", "{% if 1 == 1 %}CORRECT{% elsif 1 == 1%}1{% else %}2{% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / elsif / else
        /// </summary>
        [TestMethod]
        public void IfTag_IfElsIf_ShouldElsIf()
        {
            TestHelper.AssertTemplateOutput( "CORRECT", "{% if 1 == 0 %}0{% elsif 1 == 1%}CORRECT{% else %}2{% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / elsif / else
        /// </summary>
        [TestMethod]
        public void IfTag_IfElsIf_ShouldElse()
        {
            TestHelper.AssertTemplateOutput( "CORRECT", "{% if 2 == 0 %}0{% elsif 2 == 1%}1{% else %}CORRECT{% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / else
        /// </summary>
        [TestMethod]
        public void IfTag_IfElseIf_ShouldElseIf()
        {
            TestHelper.AssertTemplateOutput( "CORRECT", "{% if 1 == 0 %}0{% elseif 1 == 1%}CORRECT{% else %}2{% endif %}" );
        }

        /// <summary>
        /// Tests the Liquid standard if / else
        /// </summary>
        [TestMethod]
        public void IfTag_IfElseIf_ShouldElse()
        {
            TestHelper.AssertTemplateOutput( "CORRECT", "{% if 1 == 0 %}0{% elseif 1 == 2%}1{% else %}CORRECT{% endif %}" );
        }

        /// <summary>
        /// Tests the standard Liquid if/elsif/endif construct.
        /// </summary>
        [TestMethod]
        public void IfTag_WithMultipleElsIfClauses_ShouldRenderCorrectClause()
        {
            var input = @"
{% for i in (1..4) %}
    {% if i == 1 %}
    3...
    {% elsif i == 2 %}
    2...
    {% elsif i == 3 %}
    1...
    {% else %}
    go!
    {% endif %}
{% endfor %}
";
            var output = "3...2...1...go!";

            TestHelper.AssertTemplateOutput( output, input, ignoreWhitespace: true );
        }

        /// <summary>
        /// Tests the Lava if/elseif/endif construct.
        /// </summary>
        [TestMethod]
        public void IfTag_WithMultipleElseIfClauses_ShouldRenderCorrectClause()
        {
            var input = @"
{% for i in (1..4) %}
    {% if i == 1 %}
    3...
    {% elseif i == 2 %}
    2...
    {% elseif i == 3 %}
    1...
    {% else %}
    go!
    {% endif %}
{% endfor %}
";
            var output = "3...2...1...go!";

            TestHelper.AssertTemplateOutput( output, input, ignoreWhitespace:true );
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

            TestHelper.AssertTemplateOutput( expectedOutput, input, ignoreWhitespace:true );
        }

        [TestMethod]
        public void IfTag_WithBooleanVariableAndNoOperator_IsParsedAsTruthy()
        {
            var input = @"
{% assign isTruthy = true | AsBoolean %}
{% if isTruthy %}true{% else %}false{% endif %}
";

            TestHelper.AssertTemplateOutput( "true", input, ignoreWhitespace:true );
        }

        [TestMethod]
        public void IfTag_WithStringVariableAndNoOperator_IsParsedAsTruthy()
        {
            var input = @"
{% assign isTruthy = 'true' %}
{% if isTruthy %}true{% else %}false{% endif %}
";

            TestHelper.AssertTemplateOutput( "true", input, ignoreWhitespace:true );
        }
    }
}
