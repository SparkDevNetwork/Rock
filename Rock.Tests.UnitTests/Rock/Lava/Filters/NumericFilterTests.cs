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
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Lava
{
    [TestClass]
    public class NumericFilterTests : LavaUnitTestBase
    {
        #region Filter Tests: Format

        /*
         * The Format filter is implemented using the standard .NET Format() function, so we do not need to exhaustively test all supported formats.
        */

        /// <summary>
        /// The Format filter should format a numeric input using a recognized .NET format string correctly.
        /// </summary>
        [TestMethod]
        public void Format_UsingValidDotNetFormatString_ProducesValidNumber()
        {
            TestHelper.AssertTemplateOutput( "1,234,567.89", "{{ '1234567.89' | Format:'#,##0.00' }}" );
        }

        /// <summary>
        /// The Date filter should translate a date input using a standard .NET format string correctly.
        /// </summary>
        [TestMethod]
        public void Format_EmptyInput_ProducesZeroLengthStringOutput()
        {
            TestHelper.AssertTemplateOutput( "", "{{ '' | Format:'#,##0.00' }}" );
        }

        /// <summary>
        /// Non-numeric input should be returned unchanged in the output.
        /// </summary>
        [TestMethod]
        public void Format_NonnumericInput_ProducesUnchangedOutput()
        {
            TestHelper.AssertTemplateOutput( "not_a_number", "{{ 'not_a_number' | Format:'#,##0.00' }}" );
        }

        #endregion

        /// <summary>
        /// Input integer "1" should produce formatted output "1st".
        /// </summary>
        [TestMethod]
        public void NumberToOrdinal_IntegerInput_ProducesValidOrdinal()
        {
            TestHelper.AssertTemplateOutput( "1st", "{{ 1 | NumberToOrdinal }}" );
        }

        /// <summary>
        /// Input integer "3" should produce formatted output "third".
        /// </summary>
        [TestMethod]
        public void NumberToOrdinalWords_IntegerInput_ProducesValidWords()
        {
            TestHelper.AssertTemplateOutput( "third", "{{ 3 | NumberToOrdinalWords }}" );
        }

        /// <summary>
        /// Input integer "3" should produce formatted output "third".
        /// </summary>
        [TestMethod]
        public void NumberToRomanNumerals_IntegerInput_ProducesValidNumerals()
        {
            TestHelper.AssertTemplateOutput( "VII", "{{ 7 | NumberToRomanNumerals }}" );
        }

        /// <summary>
        /// Input integer "1" should produce formatted output "one".
        /// </summary>
        [TestMethod]
        public void NumberToWords_IntegerInput_ProducesValidWords()
        {
            TestHelper.AssertTemplateOutput( "one", "{{ 1 | NumberToWords }}" );
        }

        /// <summary>
        /// Input having count of "1" should produce a singular item description.
        /// </summary>
        [TestMethod]
        public void ToQuantity_EqualToOne_ProducesSingularDescription()
        {
            TestHelper.AssertTemplateOutput( "1 phone number", "{{ 'phone number' | ToQuantity:1 }}" );
        }

        /// <summary>
        /// Input having count of "3" should produce a pluralized item description.
        /// </summary>
        [TestMethod]
        public void ToQuantity_GreaterThanOne_ProducesPluralDescription()
        {
            TestHelper.AssertTemplateOutput( "3 phone numbers", "{{ 'phone number' | ToQuantity:3 }}" );
        }

        /// <summary>
        /// Common text equivalents of True should return a value of True.
        /// </summary>
        [DataTestMethod]
        [DataRow( "true" )]
        [DataRow( "T" )]
        [DataRow( "yes" )]
        [DataRow( "Y" )]
        [DataRow( "1" )]
        public void AsBoolean_Theory_CanConvertCommonTextRepresentationsOfTrue( string input )
        {
            TestHelper.AssertTemplateOutput( "true", "{{ '" +  input + "' | AsBoolean }}" );
        }

        /// <summary>
        /// Common text equivalents of False or unrecognized text should return a value of False.
        /// The purpose of this theory is to assure that none of these known or unknown inputs return True.
        /// </summary>
        [DataTestMethod]
        [DataRow( "false" )]
        [DataRow( "F" )]
        [DataRow( "no" )]
        [DataRow( "N" )]
        [DataRow( "0" )]
        [DataRow( "xyzzy" )]
        public void AsBoolean_Theory_CanConvertCommonTextRepresentationsOfFalse( string input )
        {
            TestHelper.AssertTemplateOutput( "false", "{{ '" + input + "' | AsBoolean }}" );
        }

        /// <summary>
        /// Common text representations of decimal values should return a decimal.
        /// </summary>
        [DataTestMethod]
        [DataRow( "0.1234", 0.1234 )]
        [DataRow( "1.1", 1.1 )]
        [DataRow( "1,234,567.89", 1234567.89 )]
        [DataRow( "-987.65", -987.65 )]
        [DataRow( "0", 0 )]
        public void AsDecimal_NumericInput_ReturnsDecimal( string input, double expectedResult )
        {
            // Convert to a decimal here, because the DataRow Attribute does not allow a decimal parameter to be explicitly specified.
            var expectedDecimal = Convert.ToDecimal( expectedResult );

            TestHelper.AssertTemplateOutput( expectedDecimal.ToString(), "{{ '" + input + "' | AsDecimal }}" );
        }

        /// <summary>
        /// Non-numeric text should return a null value.
        /// </summary>
        [TestMethod]
        public void AsDecimal_NonNumericInput_ReturnsEmptyString()
        {
            TestHelper.AssertTemplateOutput( string.Empty, "{{ 'xyzzy' | AsDecimal }}" );
        }

        /// <summary>
        /// Common text representations of double-precision values should return a double.
        /// </summary>
        [DataTestMethod]
        [DataRow( "0.1234", 0.1234 )]
        [DataRow( "1.1", 1.1 )]
        [DataRow( "1,234,567.89", 1234567.89 )]
        [DataRow( "-987.65", -987.65 )]
        [DataRow( "0", 0 )]
        public void AsDouble_NumericText_ReturnsNumber( string input, double expectedResult )
        {
            TestHelper.AssertTemplateOutput( expectedResult.ToString(), "{{ '" + input + "' | AsDouble }}" );
        }

        /// <summary>
        /// Non-numeric text should return an empty string.
        /// </summary>
        [TestMethod]
        public void AsDouble_NonNumericInput_ReturnsEmptyString()
        {
            TestHelper.AssertTemplateOutput( string.Empty, "{{ 'xyzzy' | AsDouble }}" );
        }

        /// <summary>
        /// Common text representations of integer values should return an integer.
        /// </summary>
        [DataTestMethod]
        [DataRow( "123", 123 )]
        [DataRow( "-987", -987 )]
        [DataRow( "0", 0 )]
        [DataRow( "10.4", 10 )]
        [DataRow( "1,234,567", 1234567 )]
        [DataRow( "$1000", 1000 )]
        public void AsInteger_FormattedNumericInput_ReturnsNumber( string input, int expectedResult )
        {
            TestHelper.AssertTemplateOutput( expectedResult.ToString(), "{{ '" + input + "' | AsInteger }}" );
        }

        /// <summary>
        /// Non-numeric text input should return a null value.
        /// </summary>
        [TestMethod]
        public void AsInteger_NonNumericInput_ReturnsEmptyString()
        {
            TestHelper.AssertTemplateOutput( string.Empty, "{{ 'xyzzy' | AsInteger }}" );
        }

        /// <summary>
        /// Valid numeric values should return a numeric result.
        /// </summary>
        [DataTestMethod]
        [DataRow( "3", "2", "1" )]
        [DataRow( "3.0", "2.0","1.0" )]
        [DataRow( "3.1", "2", "1.1" )]
        [DataRow( "3", "2.1", "0.9" )]
        public void Minus_ValidNumericOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            TestHelper.AssertTemplateOutput( expectedResult, "{{ " + input1 + " | Minus: " + input2 + " }}" );
        }

        /// <summary>
        /// String representations of valid numeric values should return a numeric result.
        /// </summary>
        [DataTestMethod]
        [DataRow( "3", "2", "1" )]
        [DataRow( "3.0", "2.0", "1.0" )]
        public void Minus_ValidNumericStringOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            // Insert the operands as string values.
            TestHelper.AssertTemplateOutput( expectedResult, "{{ '" + input1 + "' | Minus: '" + input2 + "' }}" );
        }

        /// <summary>
        /// Valid numeric values should return a numeric result.
        /// </summary>
        [DataTestMethod]
        [DataRow( "3", "2", "5" )]
        [DataRow( "3.0", "2.0", "5.0" )]
        [DataRow( "3.1", "2", "5.1" )]
        [DataRow( "3", "2.1", "5.1" )]
        public void Plus_ValidNumericOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            TestHelper.AssertTemplateOutput( expectedResult, "{{ " + input1 + " | Plus: " + input2 + " }}" );
        }

        /// <summary>
        /// String representations of valid numeric values should return a numeric result.
        /// </summary>
        [DataTestMethod]
        [DataRow( "3", "2", "5" )]
        [DataRow( "3.0", "2.0", "5.0" )]
        public void Plus_ValidNumericStringOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            // Insert the operands as string values.
            TestHelper.AssertTemplateOutput( expectedResult, "{{ '" + input1 + "' | Plus: '" + input2 + "' }}" );
        }

        /// <summary>
        /// Valid numeric values should return a numeric result.
        /// </summary>
        [DataTestMethod]
        [DataRow( "3", "2", "6" )]
        [DataRow( "3.0", "2.0", "6.00" )]
        public void Times_ValidNumericOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            TestHelper.AssertTemplateOutput( expectedResult, "{{ " + input1 + " | Times: " + input2 + " }}" );
        }

        /// <summary>
        /// String representations of valid numeric values should return a numeric result.
        /// </summary>
        [DataTestMethod]
        [DataRow( "3", "2", "6" )]
        [DataRow( "3.0", "2.0", "6.00" )]
        public void Times_ValidNumericStringOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            // Insert the operands as string values.
            TestHelper.AssertTemplateOutput( expectedResult, "{{ '" + input1 + "' | Times: '" + input2 + "' }}" );
        }

        /// <summary>
        /// String representations of valid numeric values should return a numeric result.
        /// </summary>
        [DataTestMethod]
        [DataRow( "Repeat", "3", "RepeatRepeatRepeat" )]
        [DataRow( "NoRepeat", "0", "" )]
        public void Times_StringAndNumericOperand_ReturnsRepeatedString( string input1, string input2, string expectedResult )
        {
            // Insert the operands as string values.
            TestHelper.AssertTemplateOutput( expectedResult, "{{ '" + input1 + "' | Times: " + input2 + " }}" );
        }
    }

}
