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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Tests.Lava.Shared;

using RockAppTestHelper = Rock.Tests.Shared.TestFramework.TestHelper;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Engine
{
    /// <summary>
    /// Tests for how the Lava engine reports an error encountered while rendering.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class ExceptionHandlingTests
    {
        /// <summary>
        /// If a render error is encountered and the exception handling strategy is set to "ignore", the render output should be empty.
        /// </summary>
        [TestMethod]
        public void ExceptionHandling_RenderErrorWithStrategyNone_ReturnsNullOutput()
        {
            var input = "{% invalidTagName %}";

            var options = new LavaRenderOptions
            {
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Ignore
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var result = LavaRenderTestHelper.RenderResult( engine, input, options );

                Assert.IsNotNull( result.Error, "Expected render exception not returned." );
                Assert.IsNull( result.Text, "Unexpected render output returned." );
            } );
        }

        /// <summary>
        /// The ResolveMergeFields extension method renders through the engine
        /// registered as current, so an error it encounters should reach the output.
        /// </summary>
        [TestMethod]
        public void ExceptionHandling_RenderMergeFieldsExtensionMethod_IsRenderedToOutput()
        {
            var input = "{% invalidTagName %}";

            // ResolveMergeFields reads global attributes, which resolve through
            // RockApp.Current.CreateRockContext(). A scoped app supplies a mocked
            // context so the call does not need a database.
            using var app = RockAppTestHelper.CreateScopedRockApp();

            // ResolveMergeFields reads the strategy from the engine rather than
            // accepting one, so this test needs an engine of its own to set it on.
            // The engine shared by the other tests must not be mutated.
            var engine = LavaTestEngineFactory.CreateFluidEngine( new LavaEngineConfigurationOptions
            {
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput,
                HostService = new WebsiteLavaHost()
            } );

            LavaService.SetCurrentEngine( engine );

            var output = input.ResolveMergeFields( new Dictionary<string, object>() );

            Assert.Contains( "Unknown tag", output );
            Assert.Contains( "invalidTagName", output );
        }
    }
}
