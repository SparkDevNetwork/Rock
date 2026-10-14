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

namespace Rock.Tests.Lava.Filters
{
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public partial class ColorFilterTests
    {
        /// <summary>
        /// Applying the AdjustHue color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void AdjustHue_WithPercentageParameter_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '#ee7625' | AdjustHue:'20%' }}" );

                Assert.AreEqual( "#75ee25", output );
            } );
        }

        /// <summary>
        /// Applying the Darken color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void Darken_WithPercentageParameter_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '#ee7625' | Darken:'20%' }}" );

                Assert.AreEqual( "#a0480d", output );
            } );
        }

        /// <summary>
        /// Applying the Desaturate color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void Desaturate_WithPercentageParameter_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '#ee7625' | Desaturate:'20%' }}" );

                Assert.AreEqual( "#d77b3c", output );
            } );
        }

        /// <summary>
        /// Applying the FadeIn color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void FadeIn_WithPercentageParameter_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'rgba( 238, 118, 37, 0.8 )' | FadeIn:'20%' }}" );

                Assert.AreEqual( "rgba(238, 118, 37, 1)", output );
            } );
        }

        /// <summary>
        /// Applying the FadeOut color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void FadeOut_WithPercentageParameter_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '#ee7625' | FadeOut:'20%' }}" );

                Assert.AreEqual( "rgba(238, 118, 37, 0.8)", output );
            } );
        }

        /// <summary>
        /// Applying the Grayscale color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void Grayscale_AppliedToKnownColor_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '#ee7625' | Grayscale }}" );

                Assert.AreEqual( "#8a8a8a", output );
            } );
        }

        /// <summary>
        /// Applying the Lighten color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void Lighten_WithPercentageParameter_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '#ee7625' | Lighten:'20%' }}" );

                Assert.AreEqual( "#f5b184", output );
            } );
        }

        /// <summary>
        /// Applying the Mix color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void Mix_WithPercentageParameter_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '#ee7625' | Mix:'#4286f4','20%' }}" );

                Assert.AreEqual( "#cc794e", output );
            } );
        }

        /// <summary>
        /// Applying the Saturate color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void Saturate_WithPercentageParameter_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '#ee7625' | Saturate:'20%' }}" );

                Assert.AreEqual( "#ff7314", output );
            } );
        }

        /// <summary>
        /// Applying the Shade color filter returns the expected value.
        /// </summary>
        [TestMethod]
        public void Shade_WithPercentageParameter_ReturnsCorrectValue()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '#ee7625' | Shade:'20%' }}" );

                Assert.AreEqual( "#be5e1e", output );
            } );
        }

    }
}
