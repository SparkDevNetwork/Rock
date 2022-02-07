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
using Rock.Model;

namespace Rock.Tests.UnitTests.Lava
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
    }
}
