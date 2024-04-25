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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Tests.Shared;

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

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), expectedResult.ToString(), template, ignoreWhitespace: true );
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
        /// Verify the "equals" comparison operator for boolean values using the DotLiquid framework.
        /// </summary>
        [DataTestMethod]
        [DataRow( "'False' == true", false )]
        [DataRow( "'False' == false", true )]
        [DataRow( "true == 'False'", false )]
        [DataRow( "false == 'False'", true )]
        [DataRow( "true == true", true )]
        [DataRow( "false == false", true )]
        [DataRow( "true == false", false )]
        public void RockLiquidEqual_BooleanAndOtherOperandType_PerformsImplicitConversionToBoolean( string expression, bool expectedResult )
        {
            var template = "{% if " + expression + " %}True{% else %}False{% endif %}";

            /* [2024-03-24] DJL

               This test only applies to the RockLiquid Engine.
               Implementations of Lava using the DotLiquid framework support duck-typing and implicit conversion
               of operands when comparing values, so string values such as 'True' and 'False' are interpreted as
               boolean values, and the expression {% 'False' == false %} evaluates as true.
               This does not align with the Shopify Liquid standard, but the rule is preserved for consistency.
            */
            TestHelper.AssertTemplateOutput( typeof( RockLiquidEngine ), expectedResult.ToString(), template, ignoreWhitespace: true );
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

        #region Equality Tests

        [TestMethod]
        public void LavaEqual_EqualityTruthTable_ProducesExpectedValues()
        {
            var tests = GetTruthTests();

            WriteLavaExpressionTestTableToDebugOutput( tests );

            /// This test documents the truth table for boolean equality comparisons in the RockLiquid Lava Engine v16.
            /// Changes to these results should be clearly documented, because they may break compatibility
            /// with templates created in previous versions of Rock.
            AssertExpressionsForEngine( tests, typeof( RockLiquidEngine ) );

            // This test applies to the current implementation of Lava with the Fluid engine.
            // The Fluid framework natively aligns with the Shopify equality rules, and the Lava implementation
            // modified to be compatible with previous versions of Lava.
            AssertExpressionsForEngine( tests, typeof( FluidEngine ) );
        }

        /// <summary>
        /// A test case to evaluate the result of a Lava predicate.
        /// </summary>
        private class PredicateTruthTest
        {
            /// <summary>
            /// A truth test that has the same result for both the DotLiquid and Fluid implementations of Lava.
            /// </summary>
            /// <param name="expression"></param>
            /// <param name="lavaResult"></param>
            /// <returns></returns>
            public static PredicateTruthTest NewLavaTest( string expression, bool lavaResult )
            {
                var test = new PredicateTruthTest
                {
                    ConditionalExpression = expression,
                    RockLiquidLavaResult = lavaResult,
                    FluidLavaResult = lavaResult
                };
                return test;
            }

            /// <summary>
            /// A truth test that has a different result for the DotLiquid and Fluid implementations of Lava.
            /// </summary>
            /// <param name="expression">A Lava predicate; an expression that evaluates to a true/false result.</param>
            /// <param name="rockLiquidResult">The expected result when this expression is processed by Lava using the RockLiquid engine.</param>
            /// <param name="fluidResult">The expected result when this expression is processed by Lava using the Fluid engine.</param>
            /// <param name="note">An optional note explaining the reason for the variation.</param>
            /// <returns></returns>
            public static PredicateTruthTest NewDifferentialTest( string expression, bool rockLiquidResult, bool fluidResult, string note = null )
            {
                var test = new PredicateTruthTest
                {
                    ConditionalExpression = expression,
                    RockLiquidLavaResult = rockLiquidResult,
                    FluidLavaResult = fluidResult,
                    Note = note
                };
                return test;
            }

            public string ConditionalExpression;
            public bool FluidLavaResult;
            public bool RockLiquidLavaResult;
            public string Note;
        }

        private List<PredicateTruthTest> GetTruthTests()
        {
            var tests = new List<PredicateTruthTest>();

            //
            // Boolean/Boolean comparison.
            //
            tests.Add( PredicateTruthTest.NewLavaTest( "true == true", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == True", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == false", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == False", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "True == True", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "True == true", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "True == false", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "True == False", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == false", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == False", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == true", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == True", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "False == False", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "False == false", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "False == true", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "False == True", false ) );

            //
            // String/String comparison.
            //
            tests.Add( PredicateTruthTest.NewLavaTest( "'True' == 'True'", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'True' == 'true'", false ) );

            //
            // Boolean/String comparison.
            //
            tests.Add( PredicateTruthTest.NewLavaTest( "True == 'True'", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "True == 'true'", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == 'true'", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == 'True'", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == 'False'", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "False == 'False'", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "False == 'false'", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == 'false'", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == 'False'", true ) );

            tests.Add( PredicateTruthTest.NewLavaTest( "true != ''", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false != ''", true ) );

            // "truthy/falsey" values.
            tests.Add( PredicateTruthTest.NewLavaTest( "true == ''", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == 'any_text'", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == 'yes'", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == 'y'", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "true == '1'", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == ''", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == 'any_text'", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == 'no'", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == 'n'", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "false == '0'", false ) );

            //
            // String/Boolean comparison.
            //
            tests.Add( PredicateTruthTest.NewLavaTest( "'True' == True", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'True' == true", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'False' == False", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'False' == false", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'false' == true", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'true' == false", false ) );

            tests.Add( PredicateTruthTest.NewLavaTest( "'' == true", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'' == false", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'any_text' == true", false ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'any_text' == false", false ) );

            tests.Add( PredicateTruthTest.NewLavaTest( "'' != true", true ) );
            tests.Add( PredicateTruthTest.NewLavaTest( "'' != false", true ) );

            /* 
               2024-19-04 - DJL

               These comparisons yield the opposite result from RockLiquid.
               However, the outcomes are logically consistent with the preceding String/Boolean comparisons
               and fix an inconsistency in the RockLiquid implementation.
            */
            var comment1 = "DotLiquid has an inconsistency caused by RHS Boolean->String conversion.";
            tests.Add( PredicateTruthTest.NewDifferentialTest( "'true' == true", false, true, comment1 ) );
            tests.Add( PredicateTruthTest.NewDifferentialTest( "'true' == True", false, true, comment1 ) );
            tests.Add( PredicateTruthTest.NewDifferentialTest( "'false' == false", false, true, comment1 ) );
            tests.Add( PredicateTruthTest.NewDifferentialTest( "'false' == False", false, true, comment1 ) );

            return tests;
        }

        private void AssertExpressionsForEngine( List<PredicateTruthTest> tests, Type engineType )
        {
            var errorMessages = new List<string>();

            foreach ( var testEntry in tests )
            {
                var input = $"{engineType.Name} | {testEntry.ConditionalExpression} | Expected=";

                input += "{% if " + testEntry.ConditionalExpression + " %}true{% else %}false{% endif %}";

                bool expectedResult;
                if ( engineType == typeof( RockLiquidEngine ) )
                {
                    expectedResult = testEntry.RockLiquidLavaResult;
                }
                else if ( engineType == typeof( FluidEngine ) )
                {
                    expectedResult = testEntry.FluidLavaResult;
                }
                else
                {
                    throw new Exception( "Engine Type is invalid." );
                }

                var expectedOutput = $"{engineType.Name} | {testEntry.ConditionalExpression} | Expected={expectedResult.ToString().ToLower()} ";

                var output = TestHelper.GetTemplateOutput( engineType, input );

                if ( output.RemoveWhiteSpace() != expectedOutput.RemoveWhiteSpace() )
                {
                    errorMessages.Add( expectedOutput );
                }
            }

            if ( errorMessages.Any() )
            {
                Assert.Fail( "The following tests failed:\n" + errorMessages.AsDelimited( "\n" ) );
            }
        }

        /// <summary>
        /// Writes a Lava template containing a test expression table to debug output so that it can be copy/pasted
        /// to a Rock instance for testing.
        /// </summary>
        /// <param name="tests"></param>
        /// <param name="description"></param>

        private void WriteLavaExpressionTestTableToDebugOutput( List<PredicateTruthTest> tests )
        {
            var version = global::Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber();

            var template = new StringBuilder();
            template.Append( @"
<h1>Lava Equality Comparison Truth Table</h1>
<h4>Template created on $createDate with Rock $version</h4>
<hr>
{% assign currentEngine = 'LavaEngine' | RockInstanceConfig %}
<p><strong>Current DateTime:</strong> {{ 'Now' | Date:'yyyy-MM-dd HH:mm:ss' }}<br/>
<strong>Current Engine:</strong> {{ currentEngine }}</p>
<style>
    table,
    th,
    td { padding: 10px; border: 1px solid black; border-collapse: collapse; }
    .successtext { color: green;; font-weight: bold; }
    .failtext { color: red; font-weight: bold; }
</style>
<table>
    <tr>
        <td>#</td>
        <td>Expression</td>
        <td>Expected (DotLiquid)</td>
        <td>Expected (Fluid)</td>
        <td>Actual ({{ currentEngine }})</td>
        <td>Result</td>
        <td>Notes</td>
    </tr>"
            .Replace( "$createDate", DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ) )
            .Replace( "$version", version )
            );

            var i = 0;
            foreach ( var testEntry in tests )
            {
                i++;
                var expression = @"
    <tr>
        <td>$i</td>
        <td>$expression</td>
        <td>$expectedDotLiquid</td>
        <td>$expectedFluid</td>
        <td>{% capture result %}{% if $expression %}true{% else %}false{% endif %}{% endcapture %}{{ result }}</td>
{% if currentEngine == 'Fluid' %}{% assign expected = '$expectedFluid' | ToBoolean %}{% else %}{% assign expected = '$expectedDotLiquid' | ToBoolean %}{% endif %}
        <td>{% assign result = result | ToBoolean %}{% if result == expected %}<span class='successtext'>Pass</span>{% else %}<span class='failtext'>Fail</span>{% endif %}</td>
        <td>$notes</td>
    </tr>"
                    .Replace( "$i", i.ToString() )
                    .Replace( "$expression", testEntry.ConditionalExpression )
                    .Replace( "$expectedDotLiquid", testEntry.RockLiquidLavaResult.ToString().ToLower() )
                    .Replace( "$expectedFluid", testEntry.FluidLavaResult.ToString().ToLower() )
                    .Replace( "$notes", testEntry.Note );

                template.Append( expression );
            }

            template.Append( @"
</table>
" );

            LogHelper.Log( $"**\n** Lava Truth Table Template\n**\n{template}" );
        }

        #endregion
    }
}
