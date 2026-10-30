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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.SendGrid.Webhook;
using Rock.Tests.Shared;

namespace Rock.Tests.Communication
{
    /// <summary>
    /// Tests for <see cref="SendGridEventWebhookSignature"/>.
    /// </summary>
    [TestCategory( TestFeatures.Communications )]
    [TestClass]
    public class SendGridEventWebhookSignatureTests
    {
        #region Test Vectors

        /*
            The vectors below were made with OpenSSL 3.2.4: a P-256 key pair, and
            "openssl dgst -sha256 -sign" over the timestamp followed by the body,
            which is exactly what SendGrid signs. Both signatures were also checked
            with "openssl dgst -verify". The second signature has a 31 byte r value,
            which proves short DER integers are padded correctly.
        */

        private const string OpenSslVerificationKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEADr48ysGFTn3yLIHlKjixRYT24VBV3PIXbE6ToBbifijGOttfm/99ph1y8be3fopOZs9p3LW6ZAf+mVI/wQlgw==";

        private const string OpenSslTimestamp = "1761000000";

        private const string OpenSslBody = "[{\"email\":\"th008-test@example.org\",\"event\":\"bounce\",\"timestamp\":1761000000}]";

        private const string OpenSslSignature = "MEYCIQCRHbncmq9rCcoYh4A6Wu+/son7RNYxmK3EhBS2v6TeIgIhAPV9EYOYnPzjywXvYJHVEO33oD2Mn4CfiA5sK/GHkR/L";

        private const string OpenSslShortRSignature = "MEMCHwSVKiHQZijkgqtnuAKdwtW9S+Pjh8jE+F75ZIJLOHgCIHvGpdLv3AUeYiA9rQglAW8JPc7gkXmheMTkgdv0xg5O";

        #endregion Test Vectors

        #region OpenSSL Vectors

        [TestMethod]
        public void IsValid_OpenSslSignature_ReturnsTrue()
        {
            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsValid_OpenSslSignatureWithShortR_ReturnsTrue()
        {
            Assert.AreEqual( 0x1F, Convert.FromBase64String( OpenSslShortRSignature )[3], "The test vector must have a 31 byte r value." );

            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), OpenSslShortRSignature, OpenSslTimestamp );

            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsValid_TamperedBody_ReturnsFalse()
        {
            var body = GetOpenSslBody();
            body[10] ^= 0x01;

            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, body, OpenSslSignature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_WrongTimestamp_ReturnsFalse()
        {
            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), OpenSslSignature, "1761000001" );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_PemArmoredKey_ReturnsTrue()
        {
            var pemKey = "-----BEGIN PUBLIC KEY-----\r\n"
                + OpenSslVerificationKey.Substring( 0, 64 ) + "\r\n"
                + OpenSslVerificationKey.Substring( 64 ) + "\r\n"
                + "-----END PUBLIC KEY-----\r\n";

            var isValid = SendGridEventWebhookSignature.IsValid( pemKey, GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

            Assert.IsTrue( isValid );
        }

        [TestMethod]
        public void IsValid_KeyWithSurroundingWhitespace_ReturnsTrue()
        {
            var isValid = SendGridEventWebhookSignature.IsValid( "  " + OpenSslVerificationKey + " \r\n", GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

            Assert.IsTrue( isValid );
        }

        #endregion OpenSSL Vectors

        #region Generated Keys

        [TestMethod]
        public void IsValid_GeneratedKeyRoundTrip_ReturnsTrue()
        {
            var timestamp = "1761234567";
            var body = Encoding.UTF8.GetBytes( "[{\"email\":\"round-trip@example.org\",\"event\":\"delivered\"}]\r\n" );

            using ( var ecdsa = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            {
                var verificationKey = GetVerificationKey( ecdsa );
                var signature = Sign( ecdsa, timestamp, body );

                var isValid = SendGridEventWebhookSignature.IsValid( verificationKey, body, signature, timestamp );

                Assert.IsTrue( isValid );
            }
        }

        [TestMethod]
        public void IsValid_GeneratedKeyManySignatures_AllReturnTrue()
        {
            // Many signatures cover r and s values of every DER length (31, 32 and 33 bytes).
            var timestamp = "1761234567";

            using ( var ecdsa = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            {
                var verificationKey = GetVerificationKey( ecdsa );

                for ( var i = 0; i < 200; i++ )
                {
                    var body = Encoding.UTF8.GetBytes( $"[{{\"email\":\"person{i}@example.org\",\"event\":\"open\"}}]" );
                    var signature = Sign( ecdsa, timestamp, body );

                    Assert.IsTrue( SendGridEventWebhookSignature.IsValid( verificationKey, body, signature, timestamp ), $"Signature {i} failed." );
                }
            }
        }

        [TestMethod]
        public void IsValid_SignedWithDifferentKey_ReturnsFalse()
        {
            var timestamp = "1761234567";
            var body = Encoding.UTF8.GetBytes( "[{\"email\":\"wrong-key@example.org\",\"event\":\"bounce\"}]" );

            using ( var signingKey = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            using ( var otherKey = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            {
                var signature = Sign( signingKey, timestamp, body );

                var isValid = SendGridEventWebhookSignature.IsValid( GetVerificationKey( otherKey ), body, signature, timestamp );

                Assert.IsFalse( isValid );
            }
        }

        [TestMethod]
        public void IsValid_OpenSslSignatureWithWrongKey_ReturnsFalse()
        {
            using ( var otherKey = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            {
                var isValid = SendGridEventWebhookSignature.IsValid( GetVerificationKey( otherKey ), GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

                Assert.IsFalse( isValid );
            }
        }

        #endregion Generated Keys

        #region Blank And Malformed Arguments

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "   " )]
        public void IsValid_BlankKey_ReturnsFalse( string verificationKey )
        {
            var isValid = SendGridEventWebhookSignature.IsValid( verificationKey, GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "   " )]
        public void IsValid_BlankSignature_ReturnsFalse( string signature )
        {
            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), signature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "   " )]
        public void IsValid_BlankTimestamp_ReturnsFalse( string timestamp )
        {
            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), OpenSslSignature, timestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        [DataRow( "1761234567a" )]
        [DataRow( " 1761234567" )]
        [DataRow( "-1761234567" )]
        [DataRow( "1761234567[" )]
        [DataRow( "١٧٦١" )]
        public void IsValid_NonDigitTimestampWithValidSignature_ReturnsFalse( string timestamp )
        {
            // The signature is valid for this exact timestamp, so only the digit rule can reject it.
            var body = Encoding.UTF8.GetBytes( "[{\"email\":\"non-digit@example.org\",\"event\":\"bounce\"}]" );

            using ( var ecdsa = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            {
                var signature = Sign( ecdsa, timestamp, body );

                var isValid = SendGridEventWebhookSignature.IsValid( GetVerificationKey( ecdsa ), body, signature, timestamp );

                Assert.IsFalse( isValid );
            }
        }

        [TestMethod]
        public void IsValid_LeadingBodyBytesMovedIntoTimestamp_ReturnsFalse()
        {
            var timestamp = "1761234567";
            var body = Encoding.UTF8.GetBytes( "[{\"email\":\"split@example.org\",\"event\":\"bounce\"}]" );

            using ( var ecdsa = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            {
                var verificationKey = GetVerificationKey( ecdsa );
                var signature = Sign( ecdsa, timestamp, body );

                // Move the first two body bytes to the end of the timestamp. The signed data is unchanged.
                var shiftedTimestamp = timestamp + "[{";
                var shiftedBody = body.Skip( 2 ).ToArray();

                Assert.IsTrue( SendGridEventWebhookSignature.IsValid( verificationKey, body, signature, timestamp ), "The original split must verify." );
                Assert.IsFalse( SendGridEventWebhookSignature.IsValid( verificationKey, shiftedBody, signature, shiftedTimestamp ) );
            }
        }

        [TestMethod]
        public void IsValid_NullBody_ReturnsFalse()
        {
            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, null, OpenSslSignature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_EmptyBody_ReturnsFalse()
        {
            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, new byte[0], OpenSslSignature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_KeyNotBase64_ReturnsFalse()
        {
            var isValid = SendGridEventWebhookSignature.IsValid( "abc!", GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_SignatureNotBase64_ReturnsFalse()
        {
            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), "abc!", OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_KeyWrongLength_ReturnsFalse()
        {
            var isValid = SendGridEventWebhookSignature.IsValid( "MFkw", GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_TruncatedKey_ReturnsFalse()
        {
            var keyBytes = Convert.FromBase64String( OpenSslVerificationKey );
            var truncatedKey = Convert.ToBase64String( keyBytes.Take( keyBytes.Length - 1 ).ToArray() );

            var isValid = SendGridEventWebhookSignature.IsValid( truncatedKey, GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_KeyWrongPrefix_ReturnsFalse()
        {
            var keyBytes = Convert.FromBase64String( OpenSslVerificationKey );
            keyBytes[0] ^= 0xFF;

            var isValid = SendGridEventWebhookSignature.IsValid( Convert.ToBase64String( keyBytes ), GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_KeyPointNotOnCurve_ReturnsFalse()
        {
            var keyBytes = Convert.FromBase64String( OpenSslVerificationKey );
            keyBytes[90] ^= 0x01;

            var isValid = SendGridEventWebhookSignature.IsValid( Convert.ToBase64String( keyBytes ), GetOpenSslBody(), OpenSslSignature, OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_SignatureWithTrailingJunk_ReturnsFalse()
        {
            var der = Convert.FromBase64String( OpenSslSignature ).Concat( new byte[] { 0x00 } ).ToArray();

            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), Convert.ToBase64String( der ), OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_SignatureWithTooLongInteger_ReturnsFalse()
        {
            // 30 46 02 22 00 00 <32 bytes> 02 20 <32 bytes>: a 34 byte INTEGER is never valid for P-256.
            var der = BuildDerSignature( new byte[] { 0x00, 0x00 }.Concat( Enumerable.Repeat( ( byte ) 0x11, 32 ) ).ToArray(), Enumerable.Repeat( ( byte ) 0x22, 32 ).ToArray() );

            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), Convert.ToBase64String( der ), OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_SignatureWith33ByteIntegerWithoutLeadingZero_ReturnsFalse()
        {
            var der = BuildDerSignature( Enumerable.Repeat( ( byte ) 0x11, 33 ).ToArray(), Enumerable.Repeat( ( byte ) 0x22, 32 ).ToArray() );

            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), Convert.ToBase64String( der ), OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_SignatureWithWrongSequenceLength_ReturnsFalse()
        {
            var der = Convert.FromBase64String( OpenSslSignature );
            der[1] = ( byte ) ( der[1] - 1 );

            var isValid = SendGridEventWebhookSignature.IsValid( OpenSslVerificationKey, GetOpenSslBody(), Convert.ToBase64String( der ), OpenSslTimestamp );

            Assert.IsFalse( isValid );
        }

        [TestMethod]
        public void IsValid_RawSignatureInsteadOfDer_ReturnsFalse()
        {
            var timestamp = "1761234567";
            var body = Encoding.UTF8.GetBytes( "[]" );

            using ( var ecdsa = ECDsa.Create( ECCurve.NamedCurves.nistP256 ) )
            {
                var rawSignature = ecdsa.SignData( GetSignedData( timestamp, body ), HashAlgorithmName.SHA256 );

                var isValid = SendGridEventWebhookSignature.IsValid( GetVerificationKey( ecdsa ), body, Convert.ToBase64String( rawSignature ), timestamp );

                Assert.IsFalse( isValid );
            }
        }

        #endregion Blank And Malformed Arguments

        #region Helpers

        /// <summary>
        /// Gets the bytes of the OpenSSL test vector body.
        /// </summary>
        /// <returns>A new array each call, so a test can change it.</returns>
        private static byte[] GetOpenSslBody()
        {
            return Encoding.UTF8.GetBytes( OpenSslBody );
        }

        /// <summary>
        /// Gets the SendGrid style verification key (base64 DER SubjectPublicKeyInfo) for the key pair.
        /// </summary>
        /// <param name="ecdsa">The P-256 key pair.</param>
        /// <returns>The base64 verification key.</returns>
        private static string GetVerificationKey( ECDsa ecdsa )
        {
            var parameters = ecdsa.ExportParameters( false );
            var prefix = new byte[]
            {
                0x30, 0x59, 0x30, 0x13, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x02, 0x01,
                0x06, 0x08, 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x03, 0x01, 0x07, 0x03, 0x42, 0x00, 0x04
            };

            return Convert.ToBase64String( prefix.Concat( parameters.Q.X ).Concat( parameters.Q.Y ).ToArray() );
        }

        /// <summary>
        /// Gets the data SendGrid signs: the timestamp followed by the body.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="body">The body.</param>
        /// <returns>The data to sign.</returns>
        private static byte[] GetSignedData( string timestamp, byte[] body )
        {
            return Encoding.UTF8.GetBytes( timestamp ).Concat( body ).ToArray();
        }

        /// <summary>
        /// Signs the timestamp and body like SendGrid does and returns the base64 DER signature.
        /// </summary>
        /// <param name="ecdsa">The P-256 key pair.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="body">The body.</param>
        /// <returns>The base64 DER signature.</returns>
        private static string Sign( ECDsa ecdsa, string timestamp, byte[] body )
        {
            // .NET Framework returns r followed by s (64 bytes); SendGrid sends DER.
            var rs = ecdsa.SignData( GetSignedData( timestamp, body ), HashAlgorithmName.SHA256 );

            var r = ToDerIntegerValue( rs.Take( 32 ).ToArray() );
            var s = ToDerIntegerValue( rs.Skip( 32 ).ToArray() );

            return Convert.ToBase64String( BuildDerSignature( r, s ) );
        }

        /// <summary>
        /// Converts an unsigned big endian value to its minimal DER INTEGER value bytes.
        /// </summary>
        /// <param name="value">The unsigned value.</param>
        /// <returns>The DER INTEGER value bytes.</returns>
        private static byte[] ToDerIntegerValue( byte[] value )
        {
            var trimmed = value.SkipWhile( b => b == 0x00 ).ToList();

            if ( trimmed.Count == 0 )
            {
                trimmed.Add( 0x00 );
            }

            if ( ( trimmed[0] & 0x80 ) != 0 )
            {
                trimmed.Insert( 0, 0x00 );
            }

            return trimmed.ToArray();
        }

        /// <summary>
        /// Builds a DER SEQUENCE of two INTEGER values, without any checks.
        /// </summary>
        /// <param name="r">The r value bytes, used as is.</param>
        /// <param name="s">The s value bytes, used as is.</param>
        /// <returns>The DER bytes.</returns>
        private static byte[] BuildDerSignature( byte[] r, byte[] s )
        {
            var content = new List<byte>();
            content.Add( 0x02 );
            content.Add( ( byte ) r.Length );
            content.AddRange( r );
            content.Add( 0x02 );
            content.Add( ( byte ) s.Length );
            content.AddRange( s );

            var der = new List<byte> { 0x30, ( byte ) content.Count };
            der.AddRange( content );

            return der.ToArray();
        }

        #endregion Helpers
    }
}
