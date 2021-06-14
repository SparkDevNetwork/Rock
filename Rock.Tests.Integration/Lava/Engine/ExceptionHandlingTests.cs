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

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class ExceptionHandlingTests : LavaIntegrationTestBase
    {
        /// <summary>
        /// If a custom Lava component encounters an error and the exception handling strategy is set to "render",
        /// the Exception thrown by the component should be visible in the render output because it may contain important configuration information.
        /// </summary>
        [TestMethod]
        public void ExceptionHandling_CommandConfigurationException_IsRenderedToOutput()
        {
            // If a RockEntity command block is included without specifying any parameters, the block throws an Exception.
            // The Exception is wrapped in higher-level LavaExceptions, but we want to ensure that the original message 
            // is displayed in the render output to alert the user.
            var input = @"
{% person %}
    {% for person in personItems %}
        {{ person.FullName }} <br/>
    {% endfor %}
{% endperson %}
            ";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var context = engine.NewRenderContext();

                context.SetEnabledCommands( "RockEntity" );

                var renderOptions = new LavaRenderParameters { Context = context, ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput };

                var output = TestHelper.GetTemplateRenderResult( engine, input, renderOptions );

                TestHelper.DebugWriteRenderResult( engine.EngineType, input, output.Text );

                Assert.IsTrue( output.Text.Contains( "No parameters were found in your command." ), "Expected message not found." );
            } );
        }

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

                TestHelper.DebugWriteRenderResult( engine.EngineType, input, outputText );

                Assert.IsTrue( outputText.StartsWith( "Error resolving Lava merge fields: Unknown tag 'invalidTagName'\n" ) );
            } );
        }
    }
}
