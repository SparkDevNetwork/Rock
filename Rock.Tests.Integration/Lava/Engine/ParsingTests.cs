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

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Test the compatibility of the Lava parser with the Liquid language syntax.
    /// </summary>
    [TestClass]
    public class LiquidLanguageCompatibilityTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void Parsing_LavaTemplateWithElseIfKeyword_EmitsCorrectOutput()
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

        /// <summary>
        /// The double-ampersand (&&) boolean comparison syntax for "and" is not recognized Liquid syntax.
        /// It is also not part of the documented Lava syntax, but it is supported by the DotLiquid framework and has been found in some existing core templates.
        /// This test is designed to document the expected behavior.
        /// </summary>
        [TestMethod]
        public void Parsing_ConditionalExpressionUsingDoubleAmpersand_EmitsErrorMessage()
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
                var engine = LavaEngine.NewEngineInstance( LavaEngineTypeSpecifier.Fluid, new LavaEngineConfigurationOptions() );

                TestHelper.AssertTemplateIsInvalid( LavaEngineTypeSpecifier.Fluid, input );
            }
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
