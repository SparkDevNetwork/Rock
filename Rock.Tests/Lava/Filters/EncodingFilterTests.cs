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

using Rock.Lava.Fluid;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Lava.Filters
{
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class EncodingFilterTests
    {
        #region URL Encoding

        /// <summary>
        /// Ensure that a plain text string containing URL special characters is encoded in such a way that it can be trasmitted in a URL.
        /// </summary>
        [TestMethod]
        public void Escape_WithInputContainingUrlReservedCharacter_IsEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, @"{{ ""Have you read 'James & the Giant Peach'?"" | Escape }}" );

                Assert.AreEqual( "Have you read &#39;James &amp; the Giant Peach&#39;?", output );
            } );
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
            // The two assign tags each leave the newline that followed them, so the
            // output opens with three line feeds rather than the single one that
            // starts the literal below.
            var expectedOutput = "\n\n" + @"
Source Text: Have you read 'The Lion, The Witch & the Wardrobe by C.S. Lewis'?
Applying the Escape filter twice to the source text:
Have you read &amp;#39;The Lion, The Witch &amp;amp; the Wardrobe by C.S. Lewis&amp;#39;?
Applying the EscapeOnce filter twice to the source text:
Have you read &#39;The Lion, The Witch &amp; the Wardrobe by C.S. Lewis&#39;?
".NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Ensure that a plain text string containing URL special characters is encoded in such a way that it can be trasmitted in a URL.
        /// </summary>
        [TestMethod]
        public void EscapeOnce_WithInputContainingReservedCharacter_IsEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '1 < 2 & 3' | EscapeOnce }}" );

                Assert.AreEqual( "1 &lt; 2 &amp; 3", output );
            } );
        }

        /// <summary>
        /// Ensure that a plain text string containing URL special characters is encoded in such a way that it can be trasmitted in a URL.
        /// </summary>
        [TestMethod]
        public void EscapeOnce_WithInputContainingEscapeSequence_IsNotEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '1 &lt; 2 &amp; 3' | EscapeOnce }}" );

                Assert.AreEqual( "1 &lt; 2 &amp; 3", output );
            } );
        }

        #endregion

        /// <summary>
        /// Ensure that a plain text string encoded using the Base64 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void ToBase64_EncodePlainText_IsEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'RockIsAwesome!' | ToBase64 }}" );

                Assert.AreEqual( "Um9ja0lzQXdlc29tZSE=", output );
            } );
        }

        /// <summary>
        /// Ensure that an empty string encoded using the Base64 scheme results in an empty Base64 payload.
        /// </summary>
        [TestMethod]
        public void ToBase64_EncodeEmptyString_IsEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '' | ToBase64 }}" );

                Assert.AreEqual( "", output );
            } );
        }

        /// <summary>
        /// Ensure that Unicode text is encoded correctly (UTF-8).
        /// </summary>
        [TestMethod]
        public void ToBase64_EncodeUnicodeText_IsEncoded()
        {
            // "Rock 🚀" in UTF-8 Base64.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Rock 🚀' | ToBase64 }}" );

                Assert.AreEqual( "Um9jayDwn5qA", output );
            } );
        }

        /// <summary>
        /// Ensure that a string containing non-printable / binary-like bytes is encoded correctly.
        /// </summary>
        [TestMethod]
        public void ToBase64_EncodeBinaryLikeString_IsEncoded()
        {
            // Bytes: 00 01 02 03 FF FE FD 41 42 43
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '\\x00\\x01\\x02\\x03\\xFF\\xFE\\xFD\\x41\\x42\\x43' | ToBase64 }}" );

                Assert.AreEqual( "AAECA8O/w77DvUFCQw==", output );
            } );
        }

        /// <summary>
        /// Ensure that ToBase64 and FromBase64 can round-trip a UTF-8 string value.
        /// </summary>
        [TestMethod]
        public void ToBase64_RoundTripUTF8String_IsPreserved()
        {
            // This validates that the output of ToBase64 can be decoded back to the same string.
            // Note: We compare Base64-to-Base64 to avoid issues asserting raw null bytes in template output.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Rock 🚀' | ToBase64 | FromBase64:true }}" );

                Assert.AreEqual( "Rock 🚀", output );
            } );
        }

        /// <summary>
        /// Ensure that ToBase64 and FromBase64 can round-trip without using a string intermediary.
        /// </summary>
        [TestMethod]
        public void ToBase64_RoundTripUTF8StringToBinaryToString_IsPreserved()
        {
            // This validates that the output of ToBase64 can be decoded back to the same string.
            // Note: We compare Base64-to-Base64 to avoid issues asserting raw null bytes in template output.
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'Rock 🚀' | ToBase64 | FromBase64 | ToBase64 }}" );

                Assert.AreEqual( "Um9jayDwn5qA", output );
            } );
        }

        /// <summary>
        /// Ensure that a Base64 encoded string is decoded correctly.
        /// </summary>
        [TestMethod]
        public void FromBase64_DecodeBase64ToString_IsDecoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'aGVsbG8=' | FromBase64:true }}" );

                Assert.AreEqual( "hello", output );
            } );
        }

        /// <summary>
        /// Ensure that a plain text string encoded using the HmacSha1 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void HmacSha1_EncodePlainText_IsEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'RockIsAwesome!' | HmacSha1:'secret_key' }}" );

                Assert.AreEqual( "17dbf467d8f49e9f541c7af8adf26c8422bdb342", output );
            } );
        }

        /// <summary>
        /// Ensure that a plain text string encoded using the HmacSha256 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void HmacSha256_EncodePlainText_IsEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'RockIsAwesome!' | HmacSha256:'secret_key' }}" );

                Assert.AreEqual( "3518d7aa4ad81041e14033f2bbfa317e8f2f5aa26d6f48f719783aeaebe481ae", output );
            } );

        }

        /// <summary>
        /// Ensure that a plain text string encoded using the Md5 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void Md5_EncodePlainText_IsEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'hi@example.com' | Md5 }}" );

                Assert.AreEqual( "a8277e7e83abe10c3f8bc249809293ca", output );
            } );
        }

        /// <summary>
        /// Ensure that a plain text string encoded using the Sha1 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void Sha1_EncodePlainText_IsEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'RockIsAwesome!' | Sha1 }}" );

                Assert.AreEqual( "845b0f246f221697761d085847fbc056652d03d0", output );
            } );
        }

        /// <summary>
        /// Ensure that a plain text string encoded using the Sha256 scheme is encoded correctly.
        /// </summary>
        [TestMethod]
        public void Sha256_EncodePlainText_IsEncoded()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 'RockIsAwesome!' | Sha256 }}" );

                Assert.AreEqual( "06530e8aabeb6becaabcd0c357134f3cd0a340d87500002b0a14929d92e0ac78", output );
            } );
        }

        #region ToIdHash

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "abc" )]
        [DataRow( "123abc" )]
        public void ToIdHash_WithNonIntegerInput_ReturnsEmptyOutput( string inputHash )
        {
            var input = "{{ '" + inputHash + "' | ToIdHash }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        #endregion ToIdHash

        #region FromIdHash

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "abc" )]
        public void FromIdHash_WithInvalidHashInput_ReturnsEmptyOutput( string inputHash )
        {
            var input = "{{ '" + inputHash + "' | FromIdHash }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( string.Empty, output );
            } );
        }

        [TestMethod]
        public void FromIdHash_WithIntegerInput_ReturnsIntegerOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ 123 | FromIdHash }}" );

                Assert.AreEqual( "123", output );
            } );
        }

        [TestMethod]
        public void FromIdHash_WithIntegerStringInput_ReturnsIntegerOutput()
        {
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, "{{ '123' | FromIdHash }}" );

                Assert.AreEqual( "123", output );
            } );
        }

        #endregion FromIdHash

        #region Encrypt and Decrypt

        [TestMethod]
        public void Decrypt_DocumentationExample_ReturnsExpectedOutput()
        {
            var text = "Hello there!";
            var encryptedString = Rock.Security.Encryption.EncryptString( text );

            var input = "{{ '" + encryptedString + "' | Decrypt }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( text, output );
            } );
        }

        [TestMethod]
        public void Encrypt_DocumentationExample_ReturnsExpectedOutput()
        {
            // The same input text returns a different encrypted string on each
            // execution, because the encryption method (AES) includes an
            // initialization vector to randomize the result. The encrypted value is
            // therefore matched as a wildcard.
            var input = """
                {% assign encryptedText = 'This is my secret!' | Encrypt %}
                <p>The encrypted message is: {{ encryptedText }}</p>
                {% assign decryptedText = encryptedText | Decrypt %}
                <p>The decrypted message is: {{ decryptedText }}</p>
                """;

            var expected = """

                <p>The encrypted message is: *</p>

                <p>The decrypted message is: This is my secret!</p>
                """.NormalizeLineEndings();

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                LavaAssert.Matches( expected, output, "*" );
            } );
        }

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "This is my secret!" )]
        public void Encrypt_WithStringInput_ReturnsEncryptedOutput( string input )
        {
            /*
                9/26/26 - CLAUDE

                The predecessor test built its first template with the placeholder
                "<input>" and never substituted the data row value into it, so every
                row encrypted the literal text "<input>" rather than its own input.
                That is why the empty-string row passed an assertion that the
                encrypted text differs from the input.

                With the substitution corrected, the filter returns an empty string
                for empty input, so the "was transformed" assertion only applies to
                a non-empty input. The round trip is asserted for every row.

                The original also carried a null data row, which is
                indistinguishable from the empty-string row once substituted into
                the template, so it is not repeated here.

                Reason: Substitute the data row value the test was always meant to
                use, and assert only what holds for each input.
            */
            var encryptTemplate = "{{ '" + input + "' | Encrypt }}";

            var roundTripTemplate = "{{ '" + input + "' | Encrypt | Decrypt }}";

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var encrypted = LavaRenderTestHelper.Render( engine, encryptTemplate );

                if ( input.IsNotNullOrWhiteSpace() )
                {
                    // Verify that the filter has transformed the input in some way.
                    Assert.AreNotEqual( input, encrypted );
                }

                // Verify that the input text can be encrypted and decrypted successfully.
                var roundTripped = LavaRenderTestHelper.Render( engine, roundTripTemplate );

                Assert.AreEqual( input, roundTripped );
            } );
        }

        #endregion Encrypt and Decrypt
    }
}
