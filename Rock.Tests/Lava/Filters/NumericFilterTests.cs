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
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Lava.Fluid;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Tests.Shared.Utility;

using RockAppTestHelper = Rock.Tests.Shared.TestFramework.TestHelper;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Filters
{
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class NumericFilterTests
    {
        [TestMethod]
        public void BigIntegerInput_ProducesValidValues()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 3536345354364353525 | Abs }}" );

                Assert.AreEqual( "3536345354364353525", output );
            } );
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{% if 3536345354364353525 > 3536345354364353520 %}true{% else %}false{% endif %}" );

                Assert.AreEqual( "true", output );
            } );
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{% if 3536345354364353525 < 3536345354364353520 %}true{% else %}false{% endif %}" );

                Assert.AreEqual( "false", output );
            } );
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{% if 3536345354364353520 < 3536345354364353525 %}true{% else %}false{% endif %}" );

                Assert.AreEqual( "true", output );
            } );
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{% if 3536345354364353520 > 3536345354364353525 %}true{% else %}false{% endif %}" );

                Assert.AreEqual( "false", output );
            } );
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, $"{{{{ {4_500_000_000_000_000_000} | Times:2 }}}}" );

                Assert.AreEqual( $"{9_000_000_000_000_000_000}", output );
            } );
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 3536345354364353525 | DividedBy:10,2 }}" );

                Assert.AreEqual( "353634535436435352.5", output );
            } );
        }

        [TestMethod]
        public void Abs_DocumentationExample_ProducesExpectedOutput()
        {
            var inputTemplate = @"
{% assign myNumber = '50' %}
{% assign guess1 = 18 %}
{% assign guess2 = 63 %}
{% assign guess3 = 50.5 %}
Guess My Number - Results Summary<br>
Guess 1 was {{ myNumber | Minus:guess1 | Abs }} from the target number.<br>
Guess 2 was {{ myNumber | Minus:guess2 | Abs }} from the target number.<br>
Guess 3 was {{ myNumber | Minus:guess3 | Abs }} from the target number!<br>
";
            // Each of the four assign tags leaves the newline that followed it, so
            // the output opens with five line feeds rather than the one that starts
            // the literal below.
            var expectedOutput = "\n\n\n\n" + @"
Guess My Number - Results Summary<br>
Guess 1 was 32 from the target number.<br>
Guess 2 was 13 from the target number.<br>
Guess 3 was 0.5 from the target number!<br>
".NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void Abs_NumericInput_ProducesAbsoluteValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ -17 | Abs }}" );

                Assert.AreEqual( "17", output );
            } );
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 4 | Abs }}" );

                Assert.AreEqual( "4", output );
            } );
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, @"{{ ""-19.86"" | Abs }}" );

                Assert.AreEqual( "19.86", output );
            } );
        }

        [TestMethod]
        public void Abs_NonnumericInput_ProducesZero()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'abc' | Abs }}" );

                Assert.AreEqual( "0", output );
            } );
        }

        #region Filter Tests: Format

        /*
         * The Format filter is implemented using the standard .NET Format() function, so we do not need to exhaustively test all supported formats.
        */

        /// <summary>
        /// The Format filter should format a numeric input using a recognized .NET format string correctly.
        /// </summary>
        [TestMethod]
        public void Format_UsingValidDotNetCustomFormatString_ProducesValidNumber()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '1234567.89' | Format:'#,##0.00' }}" );

                Assert.AreEqual( "1,234,567.89", output );
            } );
        }

        /// <summary>
        /// The Date filter should translate a date input using a standard .NET format string correctly.
        /// </summary>
        [TestMethod]
        public void Format_EmptyInput_ProducesZeroLengthStringOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '' | Format:'#,##0.00' }}" );

                Assert.AreEqual( "", output );
            } );
        }

        /// <summary>
        /// Non-numeric input should be returned unchanged in the output.
        /// </summary>
        [TestMethod]
        public void Format_NonNumericInput_ProducesUnchangedOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'not_a_number' | Format:'#,##0.00' }}" );

                Assert.AreEqual( "not_a_number", output );
            } );
        }

        /// <summary>
        /// The Format filter should format a numeric input using a recognized .NET format string correctly.
        /// </summary>
        [TestMethod]
        [DataRow( "1234567.89", "#,##0.00", "1,234,567.89", "en-US" )]
        [DataRow( "1234567.89", "#,##0.00", "123.456.789,00", "de-DE" )]
        public void Format_UsingValidDotNetCustomFormatString_ProducesValidNumber_AgainstClientCulture( string input, string format, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | Format:'" + format + "' }}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// The Format filter should format a numeric input using a recognized .NET format string correctly.
        /// </summary>
        [TestMethod]
        [DataRow( "1234567.89", "#,##0.00", "1,234,567.89", "en-US" )]
        [DataRow( "1234567.89", "#,##0.00", "1,234,567.89", "de-DE" )]
        public void Format_WithSetCulture_AsInvariant_UsingValidDotNetCustomFormatString_ProducesValidNumber_AgainstClientCulture( string input, string format, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'invariant' %}{{ '" + input + "' | Format:'" + format + "' }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// The Format filter should format a numeric input using a recognized .NET format string correctly.
        /// </summary>
        [TestMethod]
        [DataRow( "1234567.89", "#,##0.00", "1,234,567.89", "en-US" )]
        [DataRow( "1234567.89", "#,##0.00", "123.456.789,00", "de-DE" )]
        public void Format_WithSetCulture_AsClient_UsingValidDotNetCustomFormatString_ProducesValidNumber_AgainstClientCulture( string input, string format, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'client' %}{{ '" + input + "' | Format:'" + format + "' }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Numeric input formatted with a preset format string should be formatted correctly.
        /// </summary>
        /// <remarks>
        /// All valid .NET format strings are supported in Lava:
        /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings?redirectedfrom=MSDN
        /// Test output assumes culture setting is "en-US".
        /// </remarks>
        [TestMethod]
        [DataRow( "123.456", "C", "$123.46" )]
        [DataRow( "123.456", "C3", "$123.456" )]
        [DataRow( "'123.456'", "C3", "$123.456" )]
        [DataRow( "123.456", "D", "123" )]
        [DataRow( "1", "D3", "001" )]
        [DataRow( "'1'", "D3", "001" )]
        [DataRow( "1052.0329112756", "E", "1.052033E+003" )]
        [DataRow( "1052.0329112756", "E2", "1.05E+003" )]
        [DataRow( "'1052.0329112756'", "E2", "1.05E+003" )]
        [DataRow( "1234.567", "F", "1234.57" )]
        [DataRow( "1234.56", "F4", "1234.5600" )]
        [DataRow( "'1234.56'", "F4", "1234.5600" )]
        [DataRow( "123.456", "G", "123.456" )]
        [DataRow( "123.4546", "G4", "123.5" )]
        [DataRow( "'123.4546'", "G4", "123.5" )]
        [DataRow( "1234.567", "N", "1,234.57" )]
        [DataRow( "1234", "N1", "1,234.0" )]
        [DataRow( "'1234'", "N1", "1,234.0" )]
        [DataRow( "1", "P", "100.00%" )]
        [DataRow( "-0.39678", "P1", "-39.7%" )]
        [DataRow( "'-0.39678'", "P1", "-39.7%" )]
        [DataRow( "123456789.1234567", "R", "123456789.1234567" )]
        [DataRow( "255", "X", "FF" )]
        [DataRow( "255.4", "X", "FF" )]
        [DataRow( "'255.4'", "X", "FF" )]
        public void Format_UsingValidDotNetStandardFormatString_ProducesValidNumber( string input, string format, string expectedOutput )
        {
            var inputTemplate = @"{% assign number = $1 %}{{ number | Format:'$2' }}"
                .Replace( "$1", input )
                .Replace( "$2", format );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Integer input formatted with the preset decimal format string should be formatted correctly.
        /// </summary>
        /// <remarks>
        /// Verifies a fix for Issue #4958: https://github.com/SparkDevNetwork/Rock/issues/4958
        /// </remarks>
        [TestMethod]
        public void Format_DecimalFormatAppliedToIntegerInput_ProducesIntegerOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{% assign number = 1 %}{{ number | Format:'D3' }}" );

                Assert.AreEqual( "001", output );
            } );
        }

        /// <summary>
        /// Numeric input formatted with an invalid format string should return the format string.
        /// </summary>
        [TestMethod]
        public void Format_InvalidFormatStringAppliedToNumericInput_ProducesFormatString()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{% assign number = 1 %}{{ number | Format:'<invalidFormatString>' }}" );

                Assert.AreEqual( "<invalidFormatString>", output );
            } );
        }

        #endregion

        /// <summary>
        /// Input integer "1" should produce formatted output "1st".
        /// </summary>
        [TestMethod]
        public void NumberToOrdinal_IntegerInput_ProducesValidOrdinal()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 1 | NumberToOrdinal }}" );

                Assert.AreEqual( "1st", output );
            } );
        }

        /// <summary>
        /// Input integer "3" should produce formatted output "third".
        /// </summary>
        [TestMethod]
        public void NumberToOrdinalWords_IntegerInput_ProducesValidWords()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 3 | NumberToOrdinalWords }}" );

                Assert.AreEqual( "third", output );
            } );
        }

        /// <summary>
        /// Input integer "3" should produce formatted output "third".
        /// </summary>
        [TestMethod]
        public void NumberToRomanNumerals_IntegerInput_ProducesValidNumerals()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 7 | NumberToRomanNumerals }}" );

                Assert.AreEqual( "VII", output );
            } );
        }

        /// <summary>
        /// Input integer "1" should produce formatted output "one".
        /// </summary>
        [TestMethod]
        public void NumberToWords_IntegerInput_ProducesValidWords()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 1 | NumberToWords }}" );

                Assert.AreEqual( "one", output );
            } );
        }

        /// <summary>
        /// Input having count of "1" should produce a singular item description.
        /// </summary>
        [TestMethod]
        public void ToQuantity_EqualToOne_ProducesSingularDescription()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'phone number' | ToQuantity:1 }}" );

                Assert.AreEqual( "1 phone number", output );
            } );
        }

        /// <summary>
        /// Input having count of "3" should produce a pluralized item description.
        /// </summary>
        [TestMethod]
        public void ToQuantity_GreaterThanOne_ProducesPluralDescription()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'phone number' | ToQuantity:3 }}" );

                Assert.AreEqual( "3 phone numbers", output );
            } );
        }

        /// <summary>
        /// Common text equivalents of True should return a value of True.
        /// </summary>
        [TestMethod]
        [DataRow( "true" )]
        [DataRow( "T" )]
        [DataRow( "yes" )]
        [DataRow( "Y" )]
        [DataRow( "1" )]
        public void AsBoolean_Theory_CanConvertCommonTextRepresentationsOfTrue( string input )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | AsBoolean }}" );

                Assert.AreEqual( "true", output );
            } );
        }

        /// <summary>
        /// Common text equivalents of False or unrecognized text should return a value of False.
        /// The purpose of this theory is to assure that none of these known or unknown inputs return True.
        /// </summary>
        [TestMethod]
        [DataRow( "false" )]
        [DataRow( "F" )]
        [DataRow( "no" )]
        [DataRow( "N" )]
        [DataRow( "0" )]
        [DataRow( "xyzzy" )]
        public void AsBoolean_Theory_CanConvertCommonTextRepresentationsOfFalse( string input )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | AsBoolean }}" );

                Assert.AreEqual( "false", output );
            } );
        }

        /// <summary>
        /// Common text representations of decimal values should return a decimal.
        /// </summary>
        [TestMethod]
        [DataRow( "0.1234", 0.1234 )]
        [DataRow( "1.1", 1.1 )]
        [DataRow( "1,234,567.89", 1234567.89 )]
        [DataRow( "-987.65", -987.65 )]
        [DataRow( "0", 0 )]
        public void AsDecimal_NumericInput_ReturnsDecimal( string input, double expectedResult )
        {
            // Convert to a decimal here, because the DataRow Attribute does not allow a decimal parameter to be explicitly specified.
            var expectedDecimal = Convert.ToDecimal( expectedResult );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | AsDecimal }}" );

                Assert.AreEqual( expectedDecimal.ToString(), output );
            } );
        }

        /// <summary>
        /// Non-numeric text should return a null value.
        /// </summary>
        [TestMethod]
        public void AsDecimal_NonNumericInput_ReturnsEmptyString()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'xyzzy' | AsDecimal }}" );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        /// <summary>
        /// Representations of decimal values should return the decimal value below when run against a client culture
        /// (such as Germany) that uses a comma as the decimal separator.
        /// </summary>
        [TestMethod]
        [DataRow( "0.1234", 1234, "de-DE" )]
        [DataRow( "1.1", 11, "de-DE" )]
        //[DataRow( "1,234,567.89", null, "de-DE" )] // This is not a valid decimal in de-DE.
        [DataRow( "-987.65", -98765, "de-DE" )]
        [DataRow( "0", 0, "de-DE" )]
        public void AsDecimal_AgainstClientCulture( string input, double expectedResult, string clientCulture )
        {
            // Note: We need to set expectedResult to a string using InvariantCulture because the thread where the
            // assertion is taking place will be using the client culture of the test.
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | AsDecimal }}" );

                    Assert.AreEqual( expectedResult.ToString( CultureInfo.InvariantCulture ), output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Representations of decimal values should return the correct (invariant) decimal regardless of the culture when using
        /// the setculture Lava command.
        /// </summary>
        [TestMethod]
        [DataRow( "0.1234", 0.1234, "de-DE" )]
        [DataRow( "1.1", 1.1, "de-DE" )]
        [DataRow( "1,234,567.89", 1234567.89, "de-DE" )]
        [DataRow( "-987.65", -987.65, "de-DE" )]
        [DataRow( "0", 0, "de-DE" )]
        public void AsDecimal_WithSetCulture_AsInvariant_AgainstClientCulture( string input, double expectedResult, string clientCulture )
        {
            // Note: We need to set expectedResult to a string using InvariantCulture because the thread where the
            // assertion is taking place will be using the client culture of the test.
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'invariant' %}{{ '" + input + "' | AsDecimal }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult.ToString( CultureInfo.InvariantCulture ), output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Representations of decimal values should return the decimal when using
        /// the setculture Lava command with input that is in the client culture.
        /// </summary>
        [TestMethod]
        [DataRow( "0,1234", 0.1234, "de-DE" )]
        [DataRow( "1,1", 1.1, "de-DE" )]
        [DataRow( "1.234.567,89", 1234567.89, "de-DE" )]
        [DataRow( "-987,65", -987.65, "de-DE" )]
        [DataRow( "0", 0, "de-DE" )]
        public void AsDecimal_WithSetCulture_AsClient_AgainstClientCulture( string input, double expectedResult, string clientCulture )
        {
            // Note: We need to set expectedResult to a string using InvariantCulture because the thread where the
            // assertion is taking place will be using the client culture of the test.
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'client' %}{{ '" + input + "' | AsDecimal }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult.ToString( CultureInfo.InvariantCulture ), output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Common text representations of double-precision values should return a double.
        /// </summary>
        [TestMethod]
        [DataRow( "0.1234", 0.1234 )]
        [DataRow( "1.1", 1.1 )]
        [DataRow( "1,234,567.89", 1234567.89 )]
        [DataRow( "-987.65", -987.65 )]
        [DataRow( "0", 0 )]
        public void AsDouble_NumericText_ReturnsNumber( string input, double expectedResult )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | AsDouble }}" );

                Assert.AreEqual( expectedResult.ToString(), output );
            } );
        }

        /// <summary>
        /// Non-numeric text should return an empty string.
        /// </summary>
        [TestMethod]
        public void AsDouble_NonNumericInput_ReturnsEmptyString()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'xyzzy' | AsDouble }}" );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        /// <summary>
        /// Representations of double-precision values should return the double value below when run against a client culture
        /// (such as Germany) that uses a comma as the decimal separator.
        /// </summary>
        [TestMethod]
        [DataRow( "0.1234", "0.1234", "en-US" )]
        [DataRow( "0.1234", "1234", "de-DE" )]
        [DataRow( "1.1", "1.1", "en-US" )]
        [DataRow( "1.1", "11", "de-DE" )]
        [DataRow( "1,234,567.89", "1234567.89", "en-US" )]
        [DataRow( "1,234,567.89", "", "de-DE" )] // Not a valid number in the German culture.
        [DataRow( "-987.65", "-987.65", "en-US" )]
        [DataRow( "-987.65", "-98765", "de-DE" )] // Because the German culture uses the dot as the thousands separator.
        [DataRow( "0", "0", "en-US" )]
        [DataRow( "0", "0", "de-DE" )]
        public void AsDouble_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | AsDouble }}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Representations of double values should return the double when using
        /// the setculture Lava command with input that is in the client culture.
        /// </summary>
        [TestMethod]
        [DataRow( "0.1234", "0.1234", "en-US" )]
        [DataRow( "0.1234", "0.1234", "de-DE" )]
        [DataRow( "1.1", "1.1", "en-US" )]
        [DataRow( "1.1", "1.1", "de-DE" )]
        [DataRow( "1,234,567.89", "1234567.89", "en-US" )]
        [DataRow( "1,234,567.89", "1234567.89", "de-DE" )]
        [DataRow( "-987.65", "-987.65", "en-US" )]
        [DataRow( "-987.65", "-987.65", "de-DE" )]
        [DataRow( "0", "0", "en-US" )]
        [DataRow( "0", "0", "de-DE" )]
        public void AsDouble_WithSetCulture_AsInvariant_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'invariant' %}{{ '" + input + "' | AsDouble }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Representations of double values should return the double when using
        /// the setculture Lava command with input that is in the client culture.
        /// </summary>
        [TestMethod]
        [DataRow( "0,1234", 0.1234, "de-DE" )]
        [DataRow( "1,1", 1.1, "de-DE" )]
        [DataRow( "1.234.567,89", 1234567.89, "de-DE" )]
        [DataRow( "-987,65", -987.65, "de-DE" )]
        [DataRow( "0", 0, "de-DE" )]
        public void AsDouble_WithSetCulture_AsClient_AgainstClientCulture( string input, double expectedResult, string clientCulture )
        {
            // Note: We need to set expectedResult to a string using InvariantCulture because the thread where the
            // assertion is taking place will be using the client culture of the test.
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'client' %}{{ '" + input + "' | AsDouble }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult.ToString( CultureInfo.InvariantCulture ), output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Representations of double values should return the double when using
        /// the setculture Lava command with input that is in the client culture and
        /// it should reset back to the client culture automatically after using the setculture command.
        ///
        /// NOTE: If we create a place for our custom Lava commands, we can move this particular one there
        ///       since it's really about testing the functionality of the setculture command to reset the culture
        ///       back to the original client culture after the setculture command is used.
        /// </summary>
        [TestMethod]
        [DataRow( "1.1", "1.1 and 1.1", "en-US" )]
        [DataRow( "1.1", "1.1 and 11", "de-DE" )]
        public void AsDouble_WithSetCulture_AsInvariant_AgainstClientCulture_AndBackToClient( string input, string expectedResult, string clientCulture )
        {
            // Note: We need to set expectedResult to a string using InvariantCulture because the thread where the
            // assertion is taking place will be using the client culture of the test.
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'invariant' %}{{ '" + input + "' | AsDouble }}{% endsetculture %} and {{ '" + input + "' | AsDouble }}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Common text representations of integer values should return an integer.
        /// </summary>
        [TestMethod]
        [DataRow( "123", 123 )]
        [DataRow( "-987", -987 )]
        [DataRow( "0", 0 )]
        [DataRow( "10.4", 10 )]
        [DataRow( "1,234,567", 1234567 )]
        [DataRow( "$1000", 1000 )]
        public void AsInteger_FormattedNumericInput_ReturnsNumber( string input, int expectedResult )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | AsInteger }}" );

                Assert.AreEqual( expectedResult.ToString(), output );
            } );
        }

        /// <summary>
        /// Non-numeric text input should return a null value.
        /// </summary>
        [TestMethod]
        public void AsInteger_NonNumericInput_ReturnsEmptyString()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'xyzzy' | AsInteger }}" );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        /// <summary>
        /// Representations of integer values should return the integer value below when run against a client culture 
        /// (such as Germany) that uses a comma as the decimal separator.
        /// </summary>
        [TestMethod]
        //[DataRow( "123", 123, "de-DE" )]
        //[DataRow( "-987", -987, "de-DE" )]
        //[DataRow( "0", 0, "de-DE" )]
        //[DataRow( "10.4", 10, "en-US" )] 
        //[DataRow( "10.4", 104, "de-DE" )] // because 10.4 → 104, when parsing with de-DE.
        //[DataRow( "1,234.99", 1234, "en-US" )]

        [DataRow( "10.4", "10", "en-US" )]
        [DataRow( "10.4", "104", "de-DE" )] // because 10.4 → 104, when parsing with de-DE.
        [DataRow( "10,4", "10", "de-DE" )]  // because 10,4 → 10.4, when parsing with de-DE.
        [DataRow( "1,234.99", "", "de-DE" )] // This is not a valid number in the German culture.
        [DataRow( "1,234.99", "1234", "en-US" )]
        [DataRow( "1.234,99", "", "en-US" )]  // Not a valid number in the English culture.
        [DataRow( "1.234,99", "1234", "de-DE" )]
        [DataRow( "1.234.567,99", "1234567", "de-DE" )]
        public void AsInteger_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | AsInteger }}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Representations of integer values should return the integer value when using
        /// the setculture Lava command with input that is in the client culture.
        /// </summary>
        [TestMethod]
        [DataRow( "10.4", "10", "en-US" )]
        [DataRow( "10.4", "10", "de-DE" )]
        [DataRow( "10,4", "104", "de-DE" )] // because 10,4 → 104, when parsing with invariant
        [DataRow( "1,234.99", "1234", "de-DE" )]
        [DataRow( "1,234.99", "1234", "en-US" )]
        [DataRow( "1.234,99", "", "en-US" )] // Not a valid number in invariant culture.
        [DataRow( "1.234,99", "", "de-DE" )] // Not a valid number in invariant culture.
        [DataRow( "1.234.567,99", "", "de-DE" )] // Not a valid number for invariant culture.
        public void AsInteger_WithSetCulture_AsInvariant_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'invariant' %}{{ '" + input + "' | AsInteger }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Invalid representations of integer values should return "" run against a client culture 
        /// (such as Germany) that uses a comma as the decimal separator or vice versa.
        /// </summary>
        [TestMethod]
        [DataRow( "1,234.99", "", "de-DE" )] // This is not a valid number in the German culture.
        [DataRow( "1.234,99", "", "en-US" )] // This is not a valid number in the English culture.
        public void AsInteger_WithInvalidStrings_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | AsInteger }}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Should divide two strings (containing mixed integers and decimals) and return the values below depending on the culture.
        /// </summary>
        [TestMethod]
        [DataRow( "6", "2", "3", "de-DE" )]
        [DataRow( "6.0", "2.0", "3", "en-US" )]
        [DataRow( "6.0", "2.0", "3", "de-DE" )]
        [DataRow( "6", "2.0", "0.3", "de-DE" )] // 6 → 6, 2.0 → 20, → 6/20 = 0.3
        [DataRow( "6", "2.0", "3", "en-US" )]
        [DataRow( "6.0", "2", "30", "de-DE" )] // 6.0 → 60, 2 → 2, → 60/2 = 30
        [DataRow( "6.0", "2", "3.0", "en-US" )] // should preserve the decimal
        public void DividedBy_AgainstClientCulture( string input1, string input2, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + input1 + "' | DividedBy: '" + input2 + "' }}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Valid numeric values should return a numeric result.
        /// </summary>
        [TestMethod]
        [DataRow( "3", "2", "1" )]
        [DataRow( "3.0", "2.0", "1.0" )]
        [DataRow( "3.1", "2", "1.1" )]
        [DataRow( "3", "2.1", "0.9" )]
        public void Minus_ValidNumericOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ " + input1 + " | Minus: " + input2 + " }}" );

                Assert.AreEqual( expectedResult, output );
            } );
        }

        /// <summary>
        /// String representations of valid numeric values should return a numeric result.
        /// </summary>
        [TestMethod]
        [DataRow( "3", "2", "1" )]
        [DataRow( "3.0", "2.0", "1.0" )]
        public void Minus_ValidNumericStringOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            // Insert the operands as string values.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '" + input1 + "' | Minus: '" + input2 + "' }}" );

                Assert.AreEqual( expectedResult, output );
            } );
        }

        /// <summary>
        /// Should subtract two strings (containing mixed integers and decimals) and return the correct int or decimal using
        /// the given client culture.
        /// </summary>
        [TestMethod]
        [DataRow( "3", "2", "1", "de-DE" )]
        [DataRow( "3.0", "2.0", "10", "de-DE" )] // 3.0 → 30, 2.0 → 20 → 30-20=10 (de-DE uses comma, so parsed as int)
        [DataRow( "3", "2.0", "-17", "de-DE" )]  // 3 → 3, 2.0 → 20 → 3-20 = -17
        [DataRow( "3.0", "2", "28", "de-DE" )]   // 3.0 → 30, 2 → 2 → 30-2 = 28
        public void Minus_AgainstClientCulture( string input1, string input2, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + input1 + "' | Minus: '" + input2 + "' }}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Should subtract two strings (converted to AsDecimal) and return the expected int or decimal
        /// regardless of the running culture.
        /// </summary>
        [TestMethod]
        [DataRow( "5", "2", "3", "de-DE" )]
        [DataRow( "5.0", "2.0", "3.0", "en-US" )] // should preserve the decimal
        [DataRow( "5.0", "2.0", "3.0", "de-DE" )] // should preserve the decimal
        [DataRow( "5", "2.0", "3.0", "en-US" )] // should preserve the decimal
        [DataRow( "5", "2.0", "3.0", "de-DE" )] // should preserve the decimal
        [DataRow( "5.0", "2", "3.0", "de-DE" )] // should preserve the decimal
        public void Minus_WithSetCulture_AsInvariant_AgainstClientCulture( string input1, string input2, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'invariant' %}{% assign operand = '" + input2 + "' | AsDecimal %}{{ '" + input1 + "' |  AsDecimal | Minus: operand }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Should subtract two numbers (converted to AsDecimal) and return the value in the test for the given culture.
        /// </summary>
        [TestMethod]
        [DataRow( "5", "2", "3", "de-DE" )]
        [DataRow( "5.0", "2.0", "3.0", "en-US" )] // should preserve the decimal
        [DataRow( "5.0", "2.0", "30", "de-DE" )] // 5.0 → 50, 2.0 → 20 → 50-20=30 (de-DE uses comma, so parsed as int)
        [DataRow( "5", "2.0", "3.0", "en-US" )]  // should preserve the decimal
        [DataRow( "5", "2.0", "-15", "de-DE" )]  // 2.0 → 20, 5 → 5 → 5-20 = -15
        [DataRow( "5.0", "2", "3.0", "en-US" )]  // should preserve the decimal
        [DataRow( "5.0", "2", "48", "de-DE" )]   // 5.0 → 50, 2 → 2 → 50 - 2 = 48
        public void Minus_WithSetCulture_AsClient_AgainstClientCulture( string input1, string input2, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'client' %}{% assign operand = '" + input2 + "' | AsDecimal %}{{ '" + input1 + "' |  AsDecimal | Minus: operand }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// Valid numeric values should return a numeric result.
        /// </summary>
        [TestMethod]
        [DataRow( "3", "2", "5" )]
        [DataRow( "3.0", "2.0", "5.0" )]
        [DataRow( "3.1", "2", "5.1" )]
        [DataRow( "3", "2.1", "5.1" )]
        public void Plus_ValidNumericOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ " + input1 + " | Plus: " + input2 + " }}" );

                Assert.AreEqual( expectedResult, output );
            } );
        }

        /// <summary>
        /// String representations of valid numeric values should return a numeric result.
        /// </summary>
        [TestMethod]
        [DataRow( "3", "2", "5" )]
        [DataRow( "3.0", "2.0", "5.0" )]
        public void Plus_ValidNumericStringOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            // Insert the operands as string values.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '" + input1 + "' | Plus: '" + input2 + "' }}" );

                Assert.AreEqual( expectedResult, output );
            } );
        }

        /// <summary>
        /// Valid numeric values should return a numeric result.
        /// </summary>
        [TestMethod]
        [DataRow( "3", "2", "6" )]
        [DataRow( "3.1", "2.1", "6.51" )]
        [DataRow( "3.0", "2.0", "6.0" )] // We're not even certain this is correct, but it's how Fluid 2.25 behaves now.
        public void Times_ValidNumericOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ " + input1 + " | Times: " + input2 + " }}" );

                Assert.AreEqual( expectedResult, output );
            } );
        }

        /// <summary>
        /// String representations of valid numeric values should return a numeric result.
        /// </summary>
        [TestMethod]
        [DataRow( "3", "2", "6" )]
        [DataRow( "3.1", "2.1", "6.51" )]
        [DataRow( "3.0", "2.0", "6.0" )] // We're not even certain this is correct, but it's how Fluid 2.25 behaves now.
        public void Times_ValidNumericStringOperands_ReturnsNumericResult( string input1, string input2, string expectedResult )
        {
            // Insert the operands as string values.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '" + input1 + "' | Times: '" + input2 + "' }}" );

                Assert.AreEqual( expectedResult, output );
            } );
        }

        /// <summary>
        /// String representations of valid numeric values should return a numeric result.
        /// </summary>
        [TestMethod]
        [DataRow( "Repeat", "3", "RepeatRepeatRepeat" )]
        [DataRow( "NoRepeat", "0", "" )]
        public void Times_StringAndNumericOperand_ReturnsRepeatedString( string input1, string input2, string expectedResult )
        {
            // Insert the operands as string values.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '" + input1 + "' | Times: " + input2 + " }}" );

                Assert.AreEqual( expectedResult, output );
            } );
        }

        /// <summary>
        /// Should multiply two strings (containing mixed integers and decimals) and return the value below depending on the culture.
        /// </summary>
        [TestMethod]
        [DataRow( "3", "2", "6", "de-DE" )]
        [DataRow( "3.0", "2.0", "6.0", "en-US" )] // You might expect this to be 6 or 6.0 but it is 6.00 in Fluid 2.5 and 6.0 in Fluid 2.25. (C# preserves the scale (number of decimals) based on the operands' scales.)
        [DataRow( "3.0", "2.0", "600", "de-DE" )] // 3.0 → 30, 2.0 → 20 → 30*20 = 600 (de-DE uses comma, so parsed as int)
        [DataRow( "3", "2.0", "6.0", "en-US" )] // should preserve the decimal
        [DataRow( "3", "2.0", "60", "de-DE" )]  // does not preserve the decimal
        [DataRow( "3.0", "2", "6.0", "en-US" )] // should preserve the decimal
        [DataRow( "3.0", "2", "60", "de-DE" )]  // does not preserve the decimal
        public void Times_AgainstClientCulture( string input1, string input2, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + input1 + "' | Times: '" + input2 + "' }}" );

                    Assert.AreEqual( expectedResult, output );
                } );
                return null;
            }, clientCulture );
        }

        #region Floor

        /// <summary>
        /// The Floor filter should work as it has in the past.
        /// </summary>
        [TestMethod]
        [DataRow( "3.1", "en-US", "3" )]
        [DataRow( "3.1", "de-DE", "31" )]
        public void Floor_ProducesExpectedValue( string input, string runAsClientCulture, string expectedResult )
        {
            var template = @"{{ '" + input + "' | Floor }}";

            TestConfigurationHelper.ExecuteWithCulture( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, template );

                    Assert.AreEqual( expectedResult, output );
                } );
            }
            , runAsClientCulture );
        }

        /// <summary>
        /// The Floor filter should work as expected when using the setculture Lava command and the AsDecimal.
        /// </summary>
        [TestMethod]
        [DataRow( "3.1", "client", "en-US", "3" )]
        [DataRow( "3.1", "client", "de-DE", "31" )]
        [DataRow( "3.1", "invariant", "de-DE", "3" )]
        //[DataRow( "3.1", "server", "en-US", "3" )] // We can't do this because we're not sure what the server culture is.
        //[DataRow( "3.1", "server", "de-DE", "3" )] // We can't do this because we're not sure what the server culture is.
        public void Floor_WithSetCulture_AsDecimal_ProducesCorrectValue( string input, string setCulture, string runAsClientCulture, string expectedResult )
        {
            var template = @"{% setculture culture:'" + setCulture + "' %}{{ '" + input + "' | AsDecimal | Floor }}{% endsetculture %}";

            TestConfigurationHelper.ExecuteWithCulture( () =>
            {
                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, template );

                    Assert.AreEqual( expectedResult, output );
                } );
            }
            , runAsClientCulture );
        }
        #endregion

        #region Round

        [TestMethod]
        public void Round_InputValueIsLessThanMidpoint_ResultIsRoundedDown()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 0.4 | Round }}" );

                Assert.AreEqual( "0", output );
            } );
        }

        [TestMethod]
        public void Round_InputValueIsMidpoint_ResultIsRoundedUp()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 1.5 | Round }}" );

                Assert.AreEqual( "2", output );
            } );
        }

        [TestMethod]
        public void Round_WithSpecifiedPrecision_ResultIsCorrectPrecision()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 183.357 | Round:2 }}" );

                Assert.AreEqual( "183.36", output );
            } );
        }

        #endregion Round

        #region FormatAsCurrency

        /// <summary>
        /// Decimal input should be formatted using the configured currency symbol.
        /// </summary>
        [TestMethod]
        public void FormatAsCurrency_DecimalInputWithNoSymbol_ProducesDefaultCurrencyFormat()
        {
            using var app = CreateScopedAppWithCurrencySymbol();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '1234567.89' | FormatAsCurrency }}" );

                Assert.AreEqual( "$1,234,567.89", output );
            } );
        }

        [TestMethod]
        [DataRow( "1234567.89", "$1,234,567.89", "en-US" )]
        [DataRow( "1234567.89", "$123.456.789,00", "de-DE" )]
        public void FormatAsCurrency_ProducesValidNumber_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                using var app = CreateScopedAppWithCurrencySymbol();

                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{{ '" + input + "' | FormatAsCurrency }}" );

                    Assert.AreEqual( expectedResult, output );
                } );

                return null;
            }, clientCulture );
        }

        [TestMethod]
        [DataRow( "1234567.89", "$1,234,567.89", "en-US" )]
        [DataRow( "1234567.89", "$1,234,567.89", "de-DE" )]
        public void FormatAsCurrency_WithSetCultureAsInvariant_ProducesValidNumber_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                using var app = CreateScopedAppWithCurrencySymbol();

                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'invariant' %}{{ '" + input + "' | FormatAsCurrency }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult, output );
                } );

                return null;
            }, clientCulture );
        }

        [TestMethod]
        [DataRow( "1234567.89", "$1,234,567.89", "en-US" )]
        [DataRow( "1234567.89", "$123.456.789,00", "de-DE" )]
        public void FormatAsCurrency_WithSetCultureAsClient_ProducesValidNumber_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                using var app = CreateScopedAppWithCurrencySymbol();

                LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
                {
                    var output = LavaRenderTestHelper.Render( engine, "{% setculture culture:'client' %}{{ '" + input + "' | FormatAsCurrency }}{% endsetculture %}" );

                    Assert.AreEqual( expectedResult, output );
                } );

                return null;
            }, clientCulture );
        }

        #endregion FormatAsCurrency

        #region Support Methods

        /// <summary>
        /// Creates a scoped app whose mocked database holds the CurrencySymbol
        /// global attribute that the FormatAsCurrency filter reads.
        /// </summary>
        /// <remarks>
        /// The filter resolves the symbol through GlobalAttributesCache, which
        /// falls back to the attribute's default value when no attribute value row
        /// exists - so seeding the attribute alone is enough.
        /// </remarks>
        /// <returns>The scope, which the caller disposes.</returns>
        private static RockAppTestHelper.RockAppScope CreateScopedAppWithCurrencySymbol()
        {
            var app = RockAppTestHelper.CreateScopedRockApp();

            var rockContext = RockApp.Current.CreateRockContext();

            MockData.CreateAttribute( rockContext, "CurrencySymbol", "Currency Symbol", defaultValue: "$" );

            return app;
        }

        #endregion Support Methods
    }

}
