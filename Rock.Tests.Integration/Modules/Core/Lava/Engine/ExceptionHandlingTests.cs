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

namespace Rock.Tests.Integration.Modules.Core.Lava.Engine
{
    [TestClass]
    public class ExceptionHandlingTests : LavaIntegrationTestBase
    {
        /// <summary>
        /// If a render error is encountered and the exception handling strategy is set to "ignore", the render output should be empty.
        /// </summary>
        [TestMethod]
        public void ExceptionHandling_RenderErrorWithStrategyNone_ReturnsNullOutput()
        {
            var input = @"{% invalidTagName %}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var renderOptions = new LavaRenderParameters { ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Ignore };

                var result = TestHelper.GetTemplateRenderResult( engine, input, renderOptions );

                Assert.IsNotNull( result.Error, "Expected render exception not returned." );
                Assert.IsNull( result.Text, "Unexpected render output returned." );
            } );
        }

        /// <summary>
        /// If a render error is encountered and the exception handling strategy is set to "ignore", the render output should be empty.
        /// </summary>
        [TestMethod]
        public void ExceptionHandling_RenderMergeFieldsExtensionMethod_IsRenderedToOutput()
        {
            var input = @"{% invalidTagName %}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                engine.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput;

                LavaService.SetCurrentEngine( engine );

                var outputText = input.ResolveMergeFields( new Dictionary<string, object>() );

                TestHelper.DebugWriteRenderResult( engine, input, outputText );

                Assert.IsTrue( outputText.Contains("Unknown tag" ) && outputText.Contains("invalidTagName") );
            } );
        }
    }
}
