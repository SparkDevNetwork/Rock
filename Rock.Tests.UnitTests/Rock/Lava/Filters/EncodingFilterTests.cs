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
using Rock.Lava.Fluid;

namespace Rock.Tests.UnitTests.Lava
{
    [TestClass]
    public class EncodingFilterTests : LavaUnitTestBase
    {
        #region URL Encoding

        /// <summary>
        /// Ensure that a plain text string containing URL special characters is encoded in such a way that it can be trasmitted in a URL.
        /// </summary>
        [TestMethod]
        public void Escape_WithInputContainingUrlReservedCharacter_IsEncoded()
        {
            TestHelper.AssertTemplateOutput( typeof( FluidEngine ),
                "Have you read &#39;James &amp; the Giant Peach&#39;?",
                @"{{ ""Have you read 'James & the Giant Peach'?"" | Escape }}" );
        }

        /// <summary>
        /// Ensure that a plain text string containing URL special characters is encoded in such a way that it can be trasmitted in a URL.
        /// </summary>
        [TestMethod]
        public void EscapeOnce_DocumentationExample_ReturnsExpectedOutput()
        {
            var inputTemplate = @"
{% assign unescaped = ""Have you read 'The Lion, The Witch & the Wardrobe by C.S. Lewis'?"" %}
{% assign escaped = unescaped | Escape %}
Source Text: {{ unescaped }}
Applying the Escape filter twice to the source text:
{{ unescaped | Escape | Escape }}
Applying the EscapeOnce filter twice to the source text:
{{ unescaped | EscapeOnce | EscapeOnce }}
";
            var expectedOutput = @"
Source Text: Have you read 'The Lion, The Witch & the Wardrobe by C.S. Lewis'?
Applying the Escape filter twice to the source text:
Have you read &amp;#39;The Lion, The Witch &amp;amp; the Wardrobe by C.S. Lewis&amp;#39;?
Applying the EscapeOnce filter twice to the source text:
Have you read &#39;The Lion, The Witch &amp; the Wardrobe by C.S. Lewis&#39;?
";

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ),
                expectedOutput,
                inputTemplate, ignoreWhitespace:true );
        }

        /// <summary>
        /// Ensure that a plain text string containing URL special characters is encoded in such a way that it can be trasmitted in a URL.
        /// </summary>
        [TestMethod]
        public void EscapeOnce_WithInputContainingReservedCharacter_IsEncoded()
        {
            TestHelper.AssertTemplateOutput( typeof( FluidEngine ),
                "1 &lt; 2 &amp; 3",
                "{{ '1 < 2 & 3' | EscapeOnce }}" );
        }

        /// <summary>
        /// Ensure that a plain text string containing URL special characters is encoded in such a way that it can be trasmitted in a URL.
        /// </summary>
        [TestMethod]
        public void EscapeOnce_WithInputContainingEscapeSequence_IsNotEncoded()
        {
            TestHelper.AssertTemplateOutput( typeof( FluidEngine ),
                "1 &lt; 2 &amp; 3",
                "{{ '1 &lt; 2 &amp; 3' | EscapeOnce }}" );
        }

        #endregion

        /// <summary>
        /// Ensure that a plain text string encoded using the Base64 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void Base64_EncodePlainText_IsEncoded()
        {
            TestHelper.AssertTemplateOutput( "Um9ja0lzQXdlc29tZSE=", "{{ 'RockIsAwesome!' | Base64 }}" );
        }

        /// <summary>
        /// Ensure that a Base64 encoded string is decoded correctly.
        /// </summary>
        [TestMethod]
        public void FromBase64_DecodeBase64ToString_IsDecoded()
        {
            TestHelper.AssertTemplateOutput( "hello", "{{ 'aGVsbG8=' | FromBase64:true }}" );
        }

        /// <summary>
        /// Ensure that a plain text string encoded using the HmacSha1 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void HmacSha1_EncodePlainText_IsEncoded()
        {
            TestHelper.AssertTemplateOutput( "17dbf467d8f49e9f541c7af8adf26c8422bdb342", "{{ 'RockIsAwesome!' | HmacSha1:'secret_key' }}" );
        }

        /// <summary>
        /// Ensure that a plain text string encoded using the HmacSha256 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void HmacSha256_EncodePlainText_IsEncoded()
        {
            TestHelper.AssertTemplateOutput( "3518d7aa4ad81041e14033f2bbfa317e8f2f5aa26d6f48f719783aeaebe481ae", "{{ 'RockIsAwesome!' | HmacSha256:'secret_key' }}" );

        }

        /// <summary>
        /// Ensure that a plain text string encoded using the Md5 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void Md5_EncodePlainText_IsEncoded()
        {
            TestHelper.AssertTemplateOutput( "a8277e7e83abe10c3f8bc249809293ca", "{{ 'hi@example.com' | Md5 }}" );
        }

        /// <summary>
        /// Ensure that a plain text string encoded using the Sha1 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void Sha1_EncodePlainText_IsEncoded()
        {
            TestHelper.AssertTemplateOutput( "845b0f246f221697761d085847fbc056652d03d0", "{{ 'RockIsAwesome!' | Sha1 }}" );
        }

        /// <summary>
        /// Ensure that a plain text string encoded using the Sha256 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void Sha256_EncodePlainText_IsEncoded()
        {
            TestHelper.AssertTemplateOutput( "06530e8aabeb6becaabcd0c357134f3cd0a340d87500002b0a14929d92e0ac78", "{{ 'RockIsAwesome!' | Sha256 }}" );
        }
    }
}
