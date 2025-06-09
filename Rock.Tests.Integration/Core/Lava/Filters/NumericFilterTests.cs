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

namespace Rock.Tests.Integration.Core.Lava.Filters
{
    [TestClass]
    public class NumericFilterTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void Round_InputValueIsLessThanMidpoint_ResultIsRoundedDown()
        {
            TestHelper.AssertTemplateOutput( typeof( Rock.Lava.Fluid.FluidEngine ),
                "0", "{{ 0.4 | Round }}" );
        }

        [TestMethod]
        public void Round_InputValueIsMidpoint_ResultIsRoundedUp()
        {
            TestHelper.AssertTemplateOutput( typeof( Rock.Lava.Fluid.FluidEngine ),
                "2", "{{ 1.5 | Round }}" );
        }

        [TestMethod]
        public void Round_WithSpecifiedPrecision_ResultIsCorrectPrecision()
        {
            TestHelper.AssertTemplateOutput( typeof( Rock.Lava.Fluid.FluidEngine ),
                "183.36", "{{ 183.357 | Round:2 }}" );
        }

        #region FormatAsCurrency

        // These rely on GlobalAttributesCache.Value( "CurrencySymbol" )

        /// <summary>
        /// Decimal input should be formatted using the supplied currency symbol.
        /// </summary>
        [TestMethod]
        public void FormatAsCurrency_DecimalInputWithNoSymbol_ProducesDefaultCurrencyFormat()
        {
            TestHelper.AssertTemplateOutput( "$1,234,567.89", "{{ '1234567.89' | FormatAsCurrency }}" );
        }

        /// <summary>
        /// The Format filter should format a numeric input using a recognized .NET format string correctly.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1234567.89", "$1,234,567.89", "en-US" )]
        [DataRow( "1234567.89", "$123.456.789,00", "de-DE" )]
        public void FormatAsCurrency_ProducesValidNumber_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                TestHelper.AssertTemplateOutput( expectedResult, "{{ '" + input + "' | FormatAsCurrency }}" );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// The Format filter should format a numeric input using a recognized .NET format string correctly.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1234567.89", "$1,234,567.89", "en-US" )]
        [DataRow( "1234567.89", "$1,234,567.89", "de-DE" )]
        public void FormatAsCurrency_WithSetCulture_AsInvariant_UsingValidDotNetCustomFormatString_ProducesValidNumber_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                TestHelper.AssertTemplateOutput( expectedResult, "{% setculture culture:'invariant' %}{{ '" + input + "' | FormatAsCurrency }}{% endsetculture %}" );
                return null;
            }, clientCulture );
        }

        /// <summary>
        /// The Format filter should format a numeric input using a recognized .NET format string correctly.
        /// </summary>
        [DataTestMethod]
        [DataRow( "1234567.89", "$1,234,567.89", "en-US" )]
        [DataRow( "1234567.89", "$123.456.789,00", "de-DE" )]
        public void FormatAsCurrency_WithSetCulture_AsClient_UsingValidDotNetCustomFormatString_ProducesValidNumber_AgainstClientCulture( string input, string expectedResult, string clientCulture )
        {
            TestConfigurationHelper.ExecuteWithCulture<object>( () =>
            {
                TestHelper.AssertTemplateOutput( expectedResult, "{% setculture culture:'client' %}{{ '" + input + "' | FormatAsCurrency }}{% endsetculture %}" );
                return null;
            }, clientCulture );
        }

        #endregion
    }

}
