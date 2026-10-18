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

using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava
{
    /// <summary>
    /// Tests specific aspects of rendering in Lava that are not specifically defined in the Liquid language.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class RenderTests
    {
        /// <summary>
        /// Enum types should render as a name rather than an integer value.
        /// </summary>
        [TestMethod]
        public void Render_EnumType_RendersAsEnumName()
        {
            var enumValue = ContentChannelItemStatus.Approved;

            var mergeValues = new LavaDataDictionary { { "EnumValue", enumValue } };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var options = new LavaRenderOptions { MergeFields = mergeValues };
                var output = LavaRenderTestHelper.Render( engine, "{{ EnumValue }}", options );

                Assert.AreEqual( "Approved", output );
            } );
        }

        /// <summary>
        /// Rendering a template with the EncodeStringsAsXml option enabled should produce encoded output.
        /// </summary>
        [TestMethod]
        public void Render_StringVariableWithXmlEncodingOption_RendersEncodedString()
        {
            var mergeValues = new LavaDataDictionary { { "StringToEncode", "Ted & Cindy" } };
            var template = @"Xml Encoded String: {{ StringToEncode }}";
            var expectedOutput = @"Xml Encoded String: Ted &amp; Cindy";

            var options = new LavaRenderOptions
            {
                MergeFields = mergeValues,
                ShouldEncodeStringsAsXml = true
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Rendering a template with the EncodeStringsAsXml option disabled should produce unencoded output.
        /// </summary>
        [TestMethod]
        public void Render_StringVariableWithXmlEncodingOption_RendersUnencodedString()
        {
            var mergeValues = new LavaDataDictionary { { "UnencodedString", "Ted & Cindy" } };
            var template = @"Unencoded String: {{ UnencodedString }}";
            var expectedOutput = @"Unencoded String: Ted & Cindy";

            var options = new LavaRenderOptions
            {
                MergeFields = mergeValues,
                ShouldEncodeStringsAsXml = false
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Rendering a template with the EncodeStringsAsXml option enabled should encode markup
        /// in output values while keeping the markup written in the template itself. The SMS
        /// Conversations Person Info template relies on this to show stored names as text.
        /// </summary>
        [TestMethod]
        public void Render_HtmlInVariableWithXmlEncodingOption_EncodesValueAndKeepsTemplateMarkup()
        {
            var person = new Person
            {
                NickName = "<img src=data:, onerror=console.log('U1')>",
                LastName = "O'Brien & Co"
            };
            var mergeValues = new LavaDataDictionary { { "Person", person } };
            var template = @"<b>{{ Person.NickName }} {{ Person.LastName }}</b>";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var parameters = new LavaRenderParameters
                {
                    ShouldEncodeStringsAsXml = true,
                    Context = LavaRenderContext.FromMergeValues( mergeValues )
                };

                var output = TestHelper.GetTemplateOutput( engine, template, parameters );

                Assert.IsTrue( output.StartsWith( "<b>" ) && output.EndsWith( "</b>" ), $"Template markup was not kept: {output}" );
                Assert.IsFalse( output.Contains( "<img" ), $"Value markup was not encoded: {output}" );
                Assert.IsTrue( output.Contains( "&lt;img" ), $"Value markup was not encoded: {output}" );
                Assert.IsTrue( output.Contains( "&amp;" ), $"Ampersand was not encoded: {output}" );
            } );
        }

        /// <summary>
        /// Attempting to render a Lava template containing a syntax error renders a valid error message.
        /// </summary>
        [TestMethod]
        public void Render_TemplateParsingError_RendersUserFriendlyErrorMessage()
        {
            var template = @"{% assignnnnn test = 'sd' %}";

            // Fluid Engine
            var expectedOutputFluid = @"
Lava Error: Unknown tag 'assignnnnn' at (1:14)
";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var result = LavaRenderTestHelper.RenderResult( engine, template );

                Assert.IsTrue( result.HasErrors );
                Assert.That.AreEqualIgnoreWhitespace( expectedOutputFluid, result.Text );
            } );
        }
    }
}
