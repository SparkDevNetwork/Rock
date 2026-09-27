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
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.FileSystem
{
    /// <summary>
    /// Tests for the include statement and the file system it resolves against.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class LavaFileSystemTests
    {
        #region Tests

        /// <summary>
        /// Verify that an include file containing merge fields correctly renders the context values from the parent template.
        /// </summary>
        [TestMethod]
        public void IncludeStatement_ForFileContainingMergeFields_ReturnsMergedOutput()
        {
            var input = """
                Name: Ted Decker

                ** Contact
                {% include '_contact.lava' %}
                **
                """;

            var expected = """
                Name: Ted Decker

                ** Contact
                Mobile: (623) 555-3323
                Home: (623) 555-3322
                Work: (623) 555-2444
                Email: ted@rocksolidchurch.com
                **
                """.NormalizeLineEndings();

            var options = new LavaRenderOptions
            {
                MergeFields = new Dictionary<string, object>
                {
                    { "mobilePhone", "(623) 555-3323" },
                    { "homePhone", "(623) 555-3322" },
                    { "workPhone", "(623) 555-2444" },
                    { "email", "ted@rocksolidchurch.com" }
                }
            };

            var engine = CreateEngineWithTestFileSystem();

            var output = LavaRenderTestHelper.Render( engine, input, options );

            Assert.AreEqual( expected, output );
        }

        /// <summary>
        /// Verify that an include file can change the value of an outer scope variable.
        /// </summary>
        /// <remarks>
        /// The behavior in Fluid differs from standard Liquid: the include file
        /// keeps a local scope for new variables, so the outer value is unchanged.
        /// </remarks>
        [TestMethod]
        public void IncludeStatement_ModifyingLocalVariableSameAsOuterVariable_ModifiesOuterVariable()
        {
            var input = """
                {% assign a = 'a' %}
                Outer 'a' = {{ a }}
                {% include '_assign.lava' %}
                Outer 'a' =  {{ a }}
                """;

            var expected = """

                Outer 'a' = a

                Included 'a' = b
                Outer 'a' =  a
                """.NormalizeLineEndings();

            var engine = CreateEngineWithTestFileSystem();

            var output = LavaRenderTestHelper.Render( engine, input );

            Assert.AreEqual( expected, output );
        }

        [TestMethod]
        public void IncludeStatement_ForNonexistentFile_ShouldRenderError()
        {
            var input = "{% include '_unknown.lava' %}";

            var engine = CreateEngineWithTestFileSystem();

            var result = LavaRenderTestHelper.RenderResult( engine, input, new LavaRenderOptions
            {
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.RenderToOutput
            } );

            Assert.Contains( "File Load Failed.", result.Error.Messages().JoinStrings( "//" ) );
        }

        [TestMethod]
        public void IncludeStatement_ShouldRenderError_IfFileSystemIsNotConfigured()
        {
            var input = "{% include '_template.lava' %}";

            var engine = LavaTestEngineFactory.CreateFluidEngine( new LavaEngineConfigurationOptions() );

            var result = LavaRenderTestHelper.RenderResult( engine, input );

            Assert.Contains( "File Load Failed.", result.Error.Messages().JoinStrings( "//" ) );
        }

        #endregion Tests

        #region Support Methods

        /// <summary>
        /// Builds an engine backed by an in-memory file system holding the
        /// templates these tests include.
        /// </summary>
        /// <returns>The engine.</returns>
        private ILavaEngine CreateEngineWithTestFileSystem()
        {
            var fileProvider = new MockFileProvider();

            // A template that references merge fields from the outer template.
            fileProvider.Add( "_contact.lava", """
                Mobile: {{ mobilePhone }}
                Home: {{ homePhone }}
                Work: {{ workPhone }}
                Email: {{ email }}
                """ );

            // A template that assigns a variable.
            fileProvider.Add( "_assign.lava", """
                {% assign a = 'b' %}
                Included 'a' = {{ a }}
                """ );

            return LavaTestEngineFactory.CreateFluidEngine( new LavaEngineConfigurationOptions { FileSystem = fileProvider } );
        }

        #endregion Support Methods
    }
}
