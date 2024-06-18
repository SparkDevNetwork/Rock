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
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;

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

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedResult.ToString(), template, ignoreWhitespace: true );
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

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
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

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedResult.ToString(), template, ignoreWhitespace: true );
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

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
        }

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

        [DataTestMethod]
        [DataRow( "null <= null", false )]
        [DataRow( "1 <= null", false )]
        [DataRow( "'' <= null", false )]
        [DataRow( "true <= null", false )]
        [DataRow( "false <= null", false )]
        public void FluidLessThanOrEqual_NullOperands_PerformsExpectedComparison( string expression, bool expectedResult )
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

            TestHelper.AssertTemplateOutput( typeof ( FluidEngine ), expectedResult.ToString(), template, ignoreWhitespace: true );
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

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
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

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedResult.ToString(), template, ignoreWhitespace: true );
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

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
        }

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

        [DataTestMethod]
        [DataRow( "null >= null", false )]
        [DataRow( "1 >= null", false )]
        [DataRow( "'' >= null", false )]
        [DataRow( "true >= null", false )]
        [DataRow( "false >= null", false )]
        public void FluidGreaterThanOrEqual_NullOperands_PerformsExpectedComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        #endregion

        #region Operators: ==

        /// <summary>
        /// Verify the "equals" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "blank == 0", false )]
        [DataRow( "empty == 0", false )]
        [DataRow( "null == 0", false )]
        [DataRow( "'' == 0", false )]
        [DataRow( "'zero' == 0", false )]
        [DataRow( "1 == 1", true )]
        [DataRow( "'1' == 1", true )]
        [DataRow( "1 == '1'", true )]
        [DataRow( "2 == 1", false )]
        [DataRow( "'2' == 1", false )]
        [DataRow( "2 == '1'", false )]
        public void LiquidEqual_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "equals" comparison operator extensions for Fluid.
        /// These tests represents Liquid-compatible expressions that are not supported in RockLiquid.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1.0 == 1.0", true )]
        [DataRow( "1.1 == 1.1", true )]
        [DataRow( "1.2 == 1.1", false )]

        public void LiquidEqual_DecimalAndOtherOperandType_PerformsDecimalComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "equals" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidEqual_LeftDateAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% assign right = '1/1/2020 10:00' | AsDateTime %}
{% if left == right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "equals" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidEqual_LeftDateAndRightString_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% if left == '1/1/2020 10:00' %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "equals" comparison operator.
        /// </summary>
        [TestMethod]
        public void FluidEqual_LeftStringAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign right = '1/1/2020 10:00' | AsDateTime %}
{% if '1/1/2020 10:00' == right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "equals" comparison operator for Enum values.
        /// </summary>
        [DataTestMethod]
        [DataRow( "DayOfWeekMonday == 1", true )]
        [DataRow( "DayOfWeekMonday == 'Monday'", true )]
        [DataRow( "1 == DayOfWeekMonday", true )]
        [DataRow( "'Monday' == DayOfWeekMonday", true )]
        [DataRow( "DayOfWeekMonday == 2", false )]
        [DataRow( "DayOfWeekMonday == 'Tuesday'", false )]
        [DataRow( "2 == DayOfWeekMonday", false )]
        [DataRow( "'Tuesday' == DayOfWeekMonday", false )]
        public void FluidEqual_EnumOperands_PerformsEnumComparison( string expression, bool expectedResult )
        {
            var values = new LavaDataDictionary();
            values.Add( "DayOfWeekMonday", DayOfWeek.Monday );

            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, values, ignoreWhitespace: true );
        }


        /// <summary>
        /// Verify the "equals" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "'abc' == 'abc'", true )]
        [DataRow( "' ' == ' '", true )]
        [DataRow( "'' == ''", true )]
        [DataRow( "'abd' == 'abc'", false )]
        [DataRow( "'' == ' '", false )]
        public void LiquidEqual_StringOperands_PerformsTextComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        #endregion

        #region Operators: !=

        /// <summary>
        /// Verify the "not equals" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1 != 2", true )]
        [DataRow( "'1' != 2", true )]
        [DataRow( "1 != '2'", true )]
        [DataRow( "1 != 1", false )]
        [DataRow( "'1' != 1", false )]
        [DataRow( "1 != '1'", false )]
        public void LiquidNotEqual_IntegerAndOtherOperandType_PerformsIntegerComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "not equals" comparison operator extensions for Fluid.
        /// These tests represents Liquid-compatible expressions that are not supported in RockLiquid.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1.0 != 2.0", true )]
        [DataRow( "1.1 != 1.2", true )]
        [DataRow( "1.1 != 1.1", false )]

        public void LiquidNotEqual_DecimalAndOtherOperandType_PerformsDecimalComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "not equals" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidNotEqual_LeftDateAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% assign right = '1/1/2020 11:00' | AsDateTime %}
{% if left != right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "not equals" comparison operator.
        /// </summary>
        [TestMethod]
        public void LiquidNotEqual_LeftDateAndRightString_PerformsDateComparison()
        {
            var template = @"
{% assign left = '1/1/2020 10:00' | AsDateTime %}
{% if left != '1/1/2020 11:00' %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "not equals" comparison operator.
        /// </summary>
        [TestMethod]
        public void FluidNotEqual_LeftStringAndRightDate_PerformsDateComparison()
        {
            var template = @"
{% assign right = '1/1/2020 10:00' | AsDateTime %}
{% if '1/1/2020 11:00' != right %}True{% else %}False{% endif %}
";

            var expectedOutput = @"True";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "not equals" comparison operator.
        /// </summary>
        [TestMethod]
        public void FluidNotEqual_BooleanOperandAndEmptyString_AlwaysReturnsTrue()
        {
            var template = @"
{% assign operandTrue = true %}
{% assign operandFalse = false %}
{% if operandTrue != '' %}1{% endif %}
{% if operandFalse != '' %}2{% endif %}
{% if '' != operandTrue %}3{% endif %}
{% if '' != operandFalse %}4{% endif %}
";

            var expectedOutput = @"1234";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedOutput, template, ignoreWhitespace: true );
        }

        /// <summary>
        /// Verify the "not equals" comparison operator.
        /// </summary>
        [DataTestMethod]
        [DataRow( "'abc' != 'abd'", true )]
        [DataRow( "'' != ' '", true )]
        [DataRow( "' ' != ''", true )]
        [DataRow( "'abc' != 'abc'", false )]
        [DataRow( "'' != ''", false )]
        public void LiquidNotEqual_StringOperands_PerformsTextComparison( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            TestHelper.AssertTemplateOutput( expectedResult.ToString(), template, ignoreWhitespace: true );
        }

        #endregion
    }
}
