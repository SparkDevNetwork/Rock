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
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Lava
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

            TestHelper.AssertAction( ( engine ) =>
            {
                var testEngine = LavaEngine.NewEngineInstance( engine.EngineType, new LavaEngineConfigurationOptions { FileSystem = fileSystem } );

                //var lavaContext = testEngine.NewRenderContext( mergeValues );

                TestHelper.AssertTemplateOutput( testEngine.EngineType, expectedOutput, input, options );
            } );
        }

        /// <summary>
        /// Verify that an include file containing Lava syntax that is not Liquid-compatible is rendered correctly.
        /// </summary>
//        [TestMethod]
//        public void IncludeStatement_ForFileContainingLavaSpecificSyntax_ReturnsMergedOutput()
//        {
//            var fileSystem = GetMockFileProvider();

//            var input = @"
//{% include '_lavaSyntax.lava' %}
//";

//            var expectedOutput = @"
//Moderate
//";

//            //var mergeValues = new LavaDataDictionary { { "mobilePhone", "(623) 555-3323" }, { "homePhone", "(623) 555-3322" }, { "workPhone", "(623) 555-2444" }, { "email", "ted@rocksolidchurch.com" } };

//            //var options = new LavaTestRenderOptions { MergeFields = mergeValues };

//            TestHelper.AssertAction( ( engine ) =>
//            {
//                var testEngine = LavaEngine.NewEngineInstance( engine.EngineType, new LavaEngineConfigurationOptions { FileSystem = fileSystem } );

//                //var lavaContext = testEngine.NewRenderContext( mergeValues );

//                TestHelper.AssertTemplateOutput( testEngine.EngineType, expectedOutput, input ); //, options );
//            } );
//        }

        [TestMethod]
        public void IncludeStatement_ForNonexistentFile_ShouldRenderError()
        {
            var fileSystem = GetMockFileProvider();

            var input = @"
{% include '_unknown.lava' %}
";

            TestHelper.AssertAction( ( engine ) =>
            {
                var testEngine = LavaEngine.NewEngineInstance( engine.EngineType, new LavaEngineConfigurationOptions { FileSystem = fileSystem } );

                var output = testEngine.RenderTemplate( input );

                Assert.That.IsTrue( output.Contains( "File Load Failed." ) );
            } );

        }

        [TestMethod]
        public void IncludeStatement_ShouldRenderError_IfFileSystemIsNotConfigured()
        {
            var input = @"
{% include '_template.lava' %}
";

            TestHelper.AssertAction( ( engine ) =>
            {
                var testEngine = LavaEngine.NewEngineInstance( engine.EngineType, new LavaEngineConfigurationOptions() );

                var output = testEngine.RenderTemplate( input );

                Assert.That.IsTrue( output.Contains( "File Load Failed." ) );
            } );
        }

        private MockFileProvider GetMockFileProvider()
        {
            var fileProvider = new MockFileProvider();

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

            return fileProvider;
        }
    }
}
