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
        //#region Constructors

        //[ClassInitialize]
        //public static void Initialize( TestContext context )
        //{
        //    //_helper.LavaEngine.RegisterSafeType( typeof( TestPerson ) );
        //    //_helper.LavaEngine.RegisterSafeType( typeof( TestCampus ) );

        //}

        //#endregion

        /// <summary>
        /// Referencing a valid property of an input object should return the property value.
        /// </summary>
        [TestMethod]
        public void IncludeStatement_ForFileContainingMergeFields_ReturnsMergedOutput()
        {
            var fileSystem = GetMockFileProvider();

            TestHelper.LavaEngine.Initialize( new LavaEngineConfigurationOptions { FileSystem = fileSystem } );

            var input = @"
Name: Ted Decker

** Contact
{% include '_contact.lava' %}
**
";

            var mergeValues = new LavaDataDictionary { { "mobilePhone", "(623) 555-3323" }, { "homePhone", "(623) 555-3322" }, { "workPhone", "(623) 555-2444" }, { "email", "ted@rocksolidchurch.com" } };

            var expectedOutput = @"
Name: Ted Decker

** Contact
Mobile: (623) 555-3323
Home: (623) 555-3322
Work : (623) 555-2444
Email: ted@rocksolidchurch.com
**
";

            TestHelper.AssertTemplateOutput( expectedOutput, input, mergeValues, ignoreWhitespace:true );
        }

        [TestMethod]
        public void IncludeStatement_ForNonexistentFile_ShouldRenderError()
        {
            var fileSystem = GetMockFileProvider();

            TestHelper.LavaEngine.Initialize( new LavaEngineConfigurationOptions { FileSystem = fileSystem } );

            var input = @"
{% include '_unknown.lava' %}
";

            var output = TestHelper.GetTemplateOutput( input );

            Assert.That.IsTrue( output.Contains( "File Load Failed." ) );
        }

        [TestMethod]
        public void IncludeStatement_ShouldRenderError_IfFileSystemIsNotConfigured()
        {
            TestHelper.LavaEngine.Initialize();// new LavaEngineConfigurationOptions { FileSystem = fileSystem } );
            //TestHelper.LavaEngine.Initialize( null );

            var input = @"
{% include '_template.lava' %}
";

            var output = TestHelper.GetTemplateOutput( input );

            Assert.That.IsTrue( output.Contains( "File Load Failed." ) );
        }

        private MockFileProvider GetMockFileProvider()
        {
            var fileProvider = new MockFileProvider();

            var fileContent = @"
Mobile: {{ mobilePhone }}
Home: {{ homePhone }} 
Work: {{ workPhone }}
Email: {{ email }}
";

            fileProvider.Add( "_contact.lava", fileContent );

            return fileProvider;
        }

        /*
                [TestMethod]
                public async Task IncludeSatement_ShouldLoadPartial_IfThePartialsFolderExist()
                {
                    var expression = new LiteralExpression( new StringValue( "_Partial.liquid" ) );
                    var sw = new StringWriter();

                    var fileProvider = new MockFileProvider();
                    fileProvider.Add( "_Partial.liquid", @"{{ 'Partial Content' }}
        Partials: '{{ Partials }}'
        color: '{{ color }}'
        shape: '{{ shape }}'" );

                    var context = new TemplateContext
                    {
                        FileProvider = fileProvider
                    };
                    var expectedResult = @"Partial Content
        Partials: ''
        color: ''
        shape: ''";

                    await new IncludeStatement( expression ).WriteToAsync( sw, HtmlEncoder.Default, context );

                    Assert.Equal( expectedResult, sw.ToString() );
                }

                [Fact]
                public async Task IncludeSatement_WithInlinevariableAssignment_ShouldBeEvaluated()
                {
                    var expression = new LiteralExpression( new StringValue( "_Partial.liquid" ) );
                    var assignStatements = new List<AssignStatement>
                    {
                        new AssignStatement("color", new LiteralExpression(new StringValue("blue"))),
                        new AssignStatement("shape", new LiteralExpression(new StringValue("circle")))
                    };
                    var sw = new StringWriter();

                    var fileProvider = new MockFileProvider();
                    fileProvider.Add( "_Partial.liquid", @"{{ 'Partial Content' }}
        Partials: '{{ Partials }}'
        color: '{{ color }}'
        shape: '{{ shape }}'" );

                    var context = new TemplateContext
                    {
                        FileProvider = fileProvider
                    };
                    var expectedResult = @"Partial Content
        Partials: ''
        color: 'blue'
        shape: 'circle'";

                    await new IncludeStatement( expression, assignStatements: assignStatements ).WriteToAsync( sw, HtmlEncoder.Default, context );

                    Assert.Equal( expectedResult, sw.ToString() );
                }

                [TestMethod]
                public async Task IncludeSatement_WithTagParams_ShouldBeEvaluated()
                {
                    var pathExpression = new LiteralExpression( new StringValue( "color" ) );
                    var withExpression = new LiteralExpression( new StringValue( "blue" ) );
                    var sw = new StringWriter();

                    var fileProvider = new MockFileProvider();
                    fileProvider.Add( "color.liquid", @"{{ 'Partial Content' }}
        Partials: '{{ Partials }}'
        color: '{{ color }}'
        shape: '{{ shape }}'" );

                    var context = new TemplateContext
                    {
                        FileProvider = fileProvider
                    };
                    var expectedResult = @"Partial Content
        Partials: ''
        color: 'blue'
        shape: ''";

                    await new IncludeStatement( pathExpression, with: withExpression ).WriteToAsync( sw, HtmlEncoder.Default, context );

                    Assert.Equal( expectedResult, sw.ToString() );
                }

                [TestMethod]
                public async Task IncludeSatement_ShouldLimitRecursion()
                {
                    var expression = new LiteralExpression( new StringValue( "_Partial.liquid" ) );
                    var sw = new StringWriter();

                    var fileProvider = new MockFileProvider();
                    fileProvider.Add( "_Partial.liquid", @"{{ 'Partial Content' }} {% include '_Partial' %}" );

                    var context = new TemplateContext
                    {
                        FileProvider = fileProvider
                    };

                    await Assert.ThrowsAsync<InvalidOperationException>( () => new IncludeStatement( expression ).WriteToAsync( sw, HtmlEncoder.Default, context ).AsTask() );
                }

                */
    }
}
