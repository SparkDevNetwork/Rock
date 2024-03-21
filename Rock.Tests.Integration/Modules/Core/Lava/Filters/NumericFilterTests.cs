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

namespace Rock.Tests.Integration.Modules.Core.Lava.Filters
{
    [TestClass]
    public class NumericFilterTests : LavaIntegrationTestBase
    {
        /// <summary>
        /// Decimal input should be formatted using the supplied currency symbol.
        /// </summary>
        [TestMethod]
        public void FormatAsCurrency_DecimalInputWithNoSymbol_ProducesDefaultCurrencyFormat()
        {
            TestHelper.AssertTemplateOutput( "$1,234,567.89", "{{ '1234567.89' | FormatAsCurrency }}" );
        }

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

    }

}
