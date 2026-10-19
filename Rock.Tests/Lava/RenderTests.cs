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
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Lava
{
    /// <summary>
    /// Tests specific aspects of rendering in Lava that are not specifically defined in the Liquid language.
    /// </summary>
    [TestClass]
    public class RenderTests : LavaUnitTestBase
    {
        /// <summary>
        /// Enum types should render as a name rather than an integer value.
        /// </summary>
        [TestMethod]
        public void Render_EnumType_RendersAsEnumName()
        {
            var enumValue = ContentChannelItemStatus.Approved;

            var mergeValues = new LavaDataDictionary { { "EnumValue", enumValue } };

            TestHelper.AssertTemplateOutput( "Approved", "{{ EnumValue }}", mergeValues );
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

            var parameters = new LavaRenderParameters
            {
                ShouldEncodeStringsAsXml = true,
                Context = LavaRenderContext.FromMergeValues( mergeValues )
            };
            TestHelper.AssertTemplateOutput( expectedOutput, template, parameters );
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

            var parameters = new LavaRenderParameters
            {
                ShouldEncodeStringsAsXml = false,
                Context = LavaRenderContext.FromMergeValues( mergeValues )
            };
            TestHelper.AssertTemplateOutput( expectedOutput, template, parameters );
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
        /// Rendering a template with the EncodeStringsAsXml option enabled should keep a stored value
        /// printed inside a single quoted attribute from ending the attribute, while a link with an
        /// ampersand in its query string still points at the same address. The Interactions blocks
        /// print stored page view URLs this way.
        /// </summary>
        [TestMethod]
        public void Render_ValueInQuotedAttributeWithXmlEncodingOption_CannotEndAttribute()
        {
            var mergeValues = new LavaDataDictionary
            {
                { "Url", "/page/1?a=1&b=2'><b>TH184</b>" },
                { "Name", "José" }
            };
            var template = @"<a href='{{ Url }}'>{{ Name }}</a>";
            var expectedOutput = @"<a href='/page/1?a=1&amp;b=2&#x27;&gt;&lt;b&gt;TH184&lt;/b&gt;'>Jos&#xE9;</a>";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var parameters = new LavaRenderParameters
                {
                    ShouldEncodeStringsAsXml = true,
                    Context = LavaRenderContext.FromMergeValues( mergeValues )
                };

                var output = TestHelper.GetTemplateOutput( engine, template, parameters );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Rendering a template with the EncodeStringsAsXml option enabled encodes the result of the
        /// Escape filter again, so a template that already escapes a value shows it encoded twice.
        /// The release note for the Interactions blocks depends on this.
        /// </summary>
        [TestMethod]
        public void Render_EscapeFilterWithXmlEncodingOption_EncodesValueTwice()
        {
            var mergeValues = new LavaDataDictionary { { "Name", "Jo & Al" } };
            var template = @"{{ Name | Escape }}";

            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var encodedParameters = new LavaRenderParameters
                {
                    ShouldEncodeStringsAsXml = true,
                    Context = LavaRenderContext.FromMergeValues( mergeValues )
                };
                var unencodedParameters = new LavaRenderParameters
                {
                    ShouldEncodeStringsAsXml = false,
                    Context = LavaRenderContext.FromMergeValues( mergeValues )
                };

                Assert.AreEqual( "Jo &amp;amp; Al", TestHelper.GetTemplateOutput( engine, template, encodedParameters ) );
                Assert.AreEqual( "Jo &amp; Al", TestHelper.GetTemplateOutput( engine, template, unencodedParameters ) );
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

            var fluidEngine = TestHelper.GetEngineInstance( typeof( FluidEngine ) );

            var result = fluidEngine.RenderTemplate( template );
            Assert.IsTrue( result.HasErrors );
            Assert.That.AreEqualIgnoreWhitespace( expectedOutputFluid, result.Text );
        }
    }
}
