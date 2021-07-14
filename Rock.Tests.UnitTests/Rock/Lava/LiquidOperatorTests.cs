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

namespace Rock.Tests.UnitTests.Lava
{
    /// <summary>
    /// Tests the processing of standard Liquid operators where some variation has been identified between the Liquid frameworks supported by Rock.
    /// The Liquid language standard can be verified at https://liquidjs.com/playground.html
    /// </summary>
    [TestClass]
    public class LiquidOperatorTests : LavaUnitTestBase
    {
        #region Operators: <

        /// <summary>
        /// Verify the "less than" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1 < 2", true )]
        [DataRow( "'1' < 2", true )]
        [DataRow( "1 < '2'", true )]
        [DataRow( "2 < 1", false )]
        [DataRow( "'2' < 1", false )]
        [DataRow( "2 < '1'", false )]
        public void LiquidLessThan_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "less than" comparison operator extensions for Fluid.
        /// These tests represents Liquid-compatible expressions that are not supported in RockLiquid.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1 < 1.1", true )]
        [DataRow( "1.1 < 1.2", true )]
        [DataRow( "1.2 < 1.1", false )]

        public void FluidLessThan_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( LavaEngineTypeSpecifier.Fluid, expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "less than" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidLessThan_LeftDateAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% assign right = '1/1/2020 11:00' | AsDateTime %}
{% if left < right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "less than" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidLessThan_LeftDateAndRightString_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% if left < '1/1/2020 11:00' %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "less than" comparison operator.
        /// </summary>
        [TestMethod]
        public void FluidLessThan_LeftStringAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign right = '1/1/2020 11:00' | AsDateTime %}
{% if '1/1/2020 10:00' < right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( LavaEngineTypeSpecifier.Fluid, expectedOutput, template, ignoreWhitespace: true );
        }


        /// <summary>
        /// Verify the "Less than" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "'aba' < 'abc'", true )]
        [DataRow( "'' < 'abc'", true )]
        [DataRow( "'' < ' '", true )]
        [DataRow( "'abd' < 'abc'", false )]
        [DataRow( "'' < ''", false )]
        public void LiquidLessThan_StringOperands_PerformsTextComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        #endregion

        #region Operators: <=

        /// <summary>
        /// Verify the "less than" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1 <= 1", true )]
        [DataRow( "'1' <= 1", true )]
        [DataRow( "1 <= '1'", true )]
        [DataRow( "2 <= 1", false )]
        [DataRow( "'2' <= 1", false )]
        [DataRow( "2 <= '1'", false )]
        public void LiquidLessThanOrEqual_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "less than" comparison operator extensions for Fluid.
        /// These tests represents Liquid-compatible expressions that are not supported in RockLiquid.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1.1 <= 1.1", true )]
        [DataRow( "1.2 <= 1.1", false )]
        public void FluidLessThanOrEqual_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( LavaEngineTypeSpecifier.Fluid, expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "less than" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidLessThanOrEqual_LeftDateAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% assign right = '1/1/2020 10:00' | AsDateTime %}
{% if left <= right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "less than" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidLessThanOrEqual_LeftDateAndRightString_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% if left <= '1/1/2020 10:00' %}True{% else %}False{% endif %}<br>
{% if left <= '1/1/2020 9:00' %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True<br>False";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "less than" comparison operator.
        /// </summary>
        [TestMethod]
        public void FluidLessThanOrEqual_LeftStringAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign right = '1/1/2020 10:00' | AsDateTime %}
{% if '1/1/2020 10:00' <= right %}True{% else %}False{% endif %}<br>
{% if '1/1/2020 11:00' <= right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True<br>False";

            TestHelper.AssertTemplateOutput( LavaEngineTypeSpecifier.Fluid, expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "'aba' <= 'aba'", true )]
        [DataRow( "'' <= ' '", true )]
        [DataRow( "'' <= ''", true )]
        [DataRow( "'abd' <= 'abc'", false )]

        public void FluidLessThanOrEqual_StringOperands_PerformsTextComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        #endregion

        #region Operators: >

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "2 > 1", true )]
        [DataRow( "'2' > 1", true )]
        [DataRow( "2 > '1'", true )]
        [DataRow( "1 > 2", false )]
        [DataRow( "'1' > 2", false )]
        [DataRow( "1 > '2'", false )]
        public void LiquidGreaterThan_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator extensions for Fluid.
        /// These tests represents Liquid-compatible expressions that are not supported in RockLiquid.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1.1 > 1", true )]
        [DataRow( "1.2 > 1.1", true )]
        [DataRow( "1.1 > 1.2", false )]

        public void FluidGreaterThan_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( LavaEngineTypeSpecifier.Fluid, expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidGreaterThan_LeftDateAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 9:00' | AsDateTime %}
{% assign right = '1/1/2020 8:00' | AsDateTime %}
{% if left > right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidGreaterThan_LeftDateAndRightString_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% if left > '1/1/2020 9:00' %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [TestMethod]
        public void FluidGreaterThan_LeftStringAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign right = '1/1/2020 9:00' | AsDateTime %}
{% if '1/1/2020 10:00' > right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( LavaEngineTypeSpecifier.Fluid, expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "'abc' > 'aba'", true )]
        [DataRow( "'abc' > ''", true )]
        [DataRow( "' ' > ''", true )]
        [DataRow( "'abc' > 'abd'", false )]
        [DataRow( "'' > ''", false )]
        public void LiquidGreaterThan_StringOperands_PerformsTextComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        #endregion

        #region Operators: >=

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1 >= 1", true )]
        [DataRow( "'1' >= 1", true )]
        [DataRow( "1 >= '1'", true )]
        [DataRow( "1 >= 2", false )]
        [DataRow( "'1' >= 2", false )]
        [DataRow( "1 >= '2'", false )]
        public void LiquidGreaterThanOrEqual_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator extensions for Fluid.
        /// These tests represents Liquid-compatible expressions that are not supported in RockLiquid.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1.1 >= 1.1", true )]
        [DataRow( "1.1 >= 1.2", false )]
        public void FluidGreaterThanOrEqual_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( LavaEngineTypeSpecifier.Fluid, expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidGreaterThanOrEqual_LeftDateAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% assign right = '1/1/2020 10:00' | AsDateTime %}
{% if left >= right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidGreaterThanOrEqual_LeftDateAndRightString_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% if left >= '1/1/2020 10:00' %}True{% else %}False{% endif %}<br>
{% if left >= '1/1/2020 11:00' %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True<br>False";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [TestMethod]
        public void FluidGreaterThanOrEqual_LeftStringAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign right = '1/1/2020 8:00' | AsDateTime %}
{% if '1/1/2020 8:00' >= right %}True{% else %}False{% endif %}<br>
{% if '1/1/2020 7:00' >= right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True<br>False";

            TestHelper.AssertTemplateOutput( LavaEngineTypeSpecifier.Fluid, expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// In Lava, the ">" operator returns true for a string comparison if the left value would be sorted after the right value.
        /// Default Liquid syntax does not support string comparison using this operator.
        /// Therefore, the condition "{% if mystring > '' %}" returns false, even if mystring is assigned a value.
        /// This operator is supported natively by the DotLiquid framework.
        /// </summary>
//        [TestMethod]
//        public void FluidGreaterThan_StringOperands_PerformsTextComparison()
//        {
//            var input = @"
//{% if 'abc' > 'aba' -%}
//abc > aba.
//{% endif -%}
//{% if 'abc' >= 'abc' -%}
//abc >= abc.
//{% endif -%}
//{% if 'abc' > 'abd' -%}
//abc > abd (!!!)
//{% endif -%}
//";

//            var expectedOutput = @"
//abc > aba.
//abc >= abc.
//";

//            TestHelper.AssertTemplateOutput( expectedOutput, input );
//        }

        /// <summary>
        /// Verify the "Greater than" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "'aba' >= 'aba'", true )]
        [DataRow( "' ' >= ''", true )]
        [DataRow( "'' >= ''", true )]
        [DataRow( "'abc' >= 'abd'", false )]
        
        public void FluidGreaterThanOrEqual_StringOperands_PerformsTextComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        #endregion
    }
}
