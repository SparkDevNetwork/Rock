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

using Rock.Lava;
using Rock.Lava.DotLiquid;
using Rock.Lava.Fluid;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.FileSystem
{
    [TestClass]
    public class LavaFileSystemTests : LavaIntegrationTestBase
    {
        /// <summary>
        /// Verify that an include file containing merge fields correctly renders the context values from the parent template.
        /// </summary>
        [TestMethod]
        public void IncludeStatement_ForFileContainingMergeFields_ReturnsMergedOutput()
        {
            var fileSystem = GetMockFileProvider();

            var input = @"
Name: Ted Decker

** Contact
{% include '_contact.lava' %}
**
";

            var expectedOutput = @"
Name: Ted Decker

** Contact
Mobile: (623) 555-3323
Home: (623) 555-3322
Work : (623) 555-2444
Email: ted@rocksolidchurch.com
**
";

            var mergeValues = new LavaDataDictionary { { "mobilePhone", "(623) 555-3323" }, { "homePhone", "(623) 555-3322" }, { "workPhone", "(623) 555-2444" }, { "email", "ted@rocksolidchurch.com" } };

            var options = new LavaTestRenderOptions { MergeFields = mergeValues };

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                // Create a new engine instance of the same type, but with a test file system configuration.
                var testEngine = LavaService.NewEngineInstance( engine.GetType(), new LavaEngineConfigurationOptions { FileSystem = fileSystem } );

                TestHelper.AssertTemplateOutput( testEngine, expectedOutput, input, options );
            } );
        }

        /// <summary>
        /// Verify that an include file can change the value of an outer scope variable.
        /// Although this may be somewhat unintuitive, it is a scoping rule observed by Liquid.
        /// </summary>
        [TestMethod]
        public void IncludeStatement_ModifyingLocalVariableSameAsOuterVariable_ModifiesOuterVariable()
        {
            var fileSystem = GetMockFileProvider();

            var input = @"
{% assign a = 'a' %}
Outer 'a' = {{ a }}
{% include '_assign.lava' %}
Outer 'a' =  {{ a }}
";

            if ( LavaIntegrationTestHelper.DotLiquidEngineIsEnabled )
            {
                var expectedOutputLiquid = @"
Outer 'a' = a
Included 'a' = b
Outer 'a' = b
";

	            var testEngineDotLiquid = LavaService.NewEngineInstance( typeof( DotLiquidEngine ), new LavaEngineConfigurationOptions { FileSystem = fileSystem } );

                TestHelper.AssertTemplateOutput( testEngineDotLiquid, expectedOutputLiquid, input );
            }

            if ( LavaIntegrationTestHelper.FluidEngineIsEnabled )
            {
                // The behavior in Fluid is different from standard Liquid.
                // The include file maintains a local scope for new variables.
                var expectedOutputFluid = @"
Outer 'a' = a
Included 'a' = b
Outer 'a' = a
";

	            var testEngineFluid = LavaService.NewEngineInstance( typeof( FluidEngine ), new LavaEngineConfigurationOptions { FileSystem = fileSystem } );

                TestHelper.AssertTemplateOutput( testEngineFluid, expectedOutputFluid, input );
            }
        }

        [TestMethod]
        public void IncludeStatement_ForNonexistentFile_ShouldRenderError()
        {
            var fileSystem = GetMockFileProvider();

            var input = @"
{% include '_unknown.lava' %}
";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var testEngine = LavaService.NewEngineInstance( engine.GetType(), new LavaEngineConfigurationOptions { FileSystem = fileSystem } );

                var result = testEngine.RenderTemplate( input, new LavaRenderParameters { ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput } );

                TestHelper.DebugWriteRenderResult( engine, input, result.Text );

                Assert.That.Contains( result.Error.Messages().JoinStrings( "//" ), "File Load Failed." );
            } );
        }

        [TestMethod]
        public void IncludeStatement_ShouldRenderError_IfFileSystemIsNotConfigured()
        {
            var input = @"
{% include '_template.lava' %}
";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var testEngine = LavaService.NewEngineInstance( engine.GetType(), new LavaEngineConfigurationOptions() );

                var result = testEngine.RenderTemplate( input );

                Assert.That.Contains( result.Error.Messages().JoinStrings( "//" ), "File Load Failed." );
            } );
        }

        private MockFileProvider GetMockFileProvider()
        {
            var fileProvider = new MockFileProvider();

            // Add a lava template that references merge fields from the outer template.
            var contactDetailsTemplate = @"
Mobile: {{ mobilePhone }}
Home: {{ homePhone }} 
Work: {{ workPhone }}
Email: {{ email }}
";

            fileProvider.Add( "_contact.lava", contactDetailsTemplate );

            // This template contains the Lava-specific keyword "elseif" that is not recognized as a Liquid keyword.
            var lavaSyntaxTemplate = @"
{% assign speed = 50 %}
{% if speed > 70 -%}
Fast
{% elseif speed > 30 -%}
Moderate
{% else -%}
Slow
{% endif -%}
";

            fileProvider.Add( "_lavasyntax.lava", lavaSyntaxTemplate );

            // Add a lava template that assigns a variable.
            var assignTemplate = @"
{% assign a = 'b' %}
Included 'a' = {{ a }}
";

            fileProvider.Add( "_assign.lava", assignTemplate );

            return fileProvider;
        }
    }
}
