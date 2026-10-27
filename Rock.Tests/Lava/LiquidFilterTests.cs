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

using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava
{
    /// <summary>
    /// Tests the processing of standard Liquid filters where some variation has been identified between the Liquid frameworks supported by Rock.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class LiquidFilterTests
    {
        #region Slice

        /// <summary>
        /// Verify the operation of the Liquid Slice filter.
        /// </summary>
        [TestMethod]
        public void LiquidSliceFilter_AppliedToStringInput_ReturnsSubstring()
        {
            var template = @"{{ 'GX925' | Slice:2,3 }}";
            var expectedOutput = @"925";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Verify the operation of the Liquid Slice filter. (Fluid Only)
        /// </summary>
        [TestMethod]
        public void LiquidSliceFilter_AppliedToArrayInput_PartitionsArray()
        {
            var template = @"
{% assign list = '1,2,3,4,5,6,7,8,9,10' | Split:',' %}
{% assign sublist = list | Slice:2,3 %}
{% for i in sublist %}<li>{{ i }}</li>
{% endfor %}
";

            var expectedOutput = "\n\n\n<li>3</li>\n<li>4</li>\n<li>5</li>\n\n";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #endregion
    }
}
