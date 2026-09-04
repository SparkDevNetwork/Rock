using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration.ConnectedServices;

namespace Rock.Tests.Configuration.ConnectedServices
{
    [TestClass]
    public class Base64UrlEncoderTests
    {
        #region Encode( byte[] )

        [TestMethod]
        public void EncodeBytes_WithNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () => Base64UrlEncoder.Encode( ( byte[] ) null ) );
        }

        [TestMethod]
        public void EncodeBytes_WithEmpty_ReturnsEmpty()
        {
            var result = Base64UrlEncoder.Encode( new byte[0] );

            Assert.AreEqual( string.Empty, result );
        }

        [TestMethod]
        public void EncodeBytes_WithValueRequiringUrlSafeAlphabet_ReplacesPlusAndSlash()
        {
            // These 3 bytes force both '+' and '/' in standard base64 output:
            // 0xFB, 0xFF, 0xBF -> "+/+/" in standard base64.
            var input = new byte[] { 0xFB, 0xFF, 0xBF };

            var result = Base64UrlEncoder.Encode( input );

            Assert.AreEqual( "-_-_", result );
        }

        [TestMethod]
        public void EncodeBytes_WithValueRequiringSinglePadding_StripsPadding()
        {
            // Two bytes produce a base64 string with a single '=' pad.
            var input = new byte[] { 0x01, 0x02 };

            var result = Base64UrlEncoder.Encode( input );

            // Standard base64: "AQI=" -> stripped to "AQI".
            Assert.AreEqual( "AQI", result );
        }

        [TestMethod]
        public void EncodeBytes_WithValueRequiringDoublePadding_StripsPadding()
        {
            // A single byte produces a base64 string with two '=' pads.
            var input = new byte[] { 0x01 };

            var result = Base64UrlEncoder.Encode( input );

            // Standard base64: "AQ==" -> stripped to "AQ".
            Assert.AreEqual( "AQ", result );
        }

        #endregion

        #region Encode( string )

        [TestMethod]
        public void EncodeString_WithNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () => Base64UrlEncoder.Encode( ( string ) null ) );
        }

        [TestMethod]
        public void EncodeString_WithEmpty_ReturnsEmpty()
        {
            var result = Base64UrlEncoder.Encode( string.Empty );

            Assert.AreEqual( string.Empty, result );
        }

        [TestMethod]
        public void EncodeString_WithAsciiValue_ReturnsExpectedEncoding()
        {
            var result = Base64UrlEncoder.Encode( "hello" );

            // Standard base64: "aGVsbG8=" -> stripped to "aGVsbG8".
            Assert.AreEqual( "aGVsbG8", result );
        }

        [TestMethod]
        public void EncodeString_WithUnicodeValue_UsesUtf8Encoding()
        {
            // "héllo" is 6 UTF-8 bytes (68 C3 A9 6C 6C 6F) which base64url-encode
            // to "aMOpbGxv" (no '+', '/', or '=' produced, so url-safe and
            // standard base64 happen to match here). A Latin1 (or any 5-byte)
            // interpretation of "héllo" would encode to something shorter, so
            // this hardcoded value would fail if the impl swapped encodings.
            var result = Base64UrlEncoder.Encode( "héllo" );

            Assert.AreEqual( "aMOpbGxv", result );
        }

        #endregion

        #region Decode

        [TestMethod]
        public void Decode_WithNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () => Base64UrlEncoder.Decode( null ) );
        }

        [TestMethod]
        public void Decode_WithEmpty_ReturnsEmpty()
        {
            var result = Base64UrlEncoder.Decode( string.Empty );

            Assert.IsEmpty( result );
        }

        [TestMethod]
        public void Decode_WithUrlSafeAlphabet_ConvertsBackToStandardAndDecodes()
        {
            var result = Base64UrlEncoder.Decode( "-_-_" );

            CollectionAssert.AreEqual( new byte[] { 0xFB, 0xFF, 0xBF }, result );
        }

        [TestMethod]
        public void Decode_WithLengthRequiringSinglePad_RestoresPadding()
        {
            var result = Base64UrlEncoder.Decode( "AQI" );

            CollectionAssert.AreEqual( new byte[] { 0x01, 0x02 }, result );
        }

        [TestMethod]
        public void Decode_WithLengthRequiringDoublePad_RestoresPadding()
        {
            var result = Base64UrlEncoder.Decode( "AQ" );

            CollectionAssert.AreEqual( new byte[] { 0x01 }, result );
        }

        [TestMethod]
        public void Decode_WithInvalidLength_ThrowsFormatException()
        {
            // Length % 4 == 1 is not a valid base64url length.
            Assert.ThrowsExactly<FormatException>( () => Base64UrlEncoder.Decode( "A" ) );
        }

        #endregion

        #region DecodeToString

        [TestMethod]
        public void DecodeToString_WithNull_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () => Base64UrlEncoder.DecodeToString( null ) );
        }

        [TestMethod]
        public void DecodeToString_WithAsciiValue_ReturnsExpectedString()
        {
            var result = Base64UrlEncoder.DecodeToString( "aGVsbG8" );

            Assert.AreEqual( "hello", result );
        }

        [TestMethod]
        public void DecodeToString_WithUnicodeValue_UsesUtf8Encoding()
        {
            // See EncodeString_WithUnicodeValue_UsesUtf8Encoding for the
            // math -- decoding "aMOpbGxv" as UTF-8 must yield "héllo".
            // A Latin1 (or any non-UTF-8) decode would yield garbage.
            var result = Base64UrlEncoder.DecodeToString( "aMOpbGxv" );

            Assert.AreEqual( "héllo", result );
        }

        #endregion

        #region Round-trip

        [TestMethod]
        public void EncodeDecode_RoundTrip_PreservesBytes()
        {
            var input = new byte[] { 0x00, 0x01, 0x7F, 0x80, 0xFB, 0xFF, 0xBF };

            var encoded = Base64UrlEncoder.Encode( input );
            var decoded = Base64UrlEncoder.Decode( encoded );

            CollectionAssert.AreEqual( input, decoded );
        }

        #endregion
    }
}
