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
using System.Security.Cryptography;
using System.Text;

using Rock.Attribute;

namespace Rock.SendGrid.Webhook
{
    /// <summary>
    /// Verifies SendGrid Signed Event Webhook signatures. SendGrid signs the
    /// timestamp header value followed by the raw request body using ECDSA on
    /// the P-256 curve with SHA-256. The signature header is a base64 DER
    /// signature and the verification key is a base64 DER public key.
    /// </summary>
    [RockInternal( "17.12", true )]
    public static class SendGridEventWebhookSignature
    {
        #region Fields

        /// <summary>
        /// The fixed DER SubjectPublicKeyInfo prefix of an uncompressed P-256
        /// public key. The 64 bytes that follow it are the X and Y coordinates.
        /// </summary>
        private static readonly byte[] _p256PublicKeyPrefix = new byte[]
        {
            0x30, 0x59, 0x30, 0x13, 0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x02, 0x01,
            0x06, 0x08, 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x03, 0x01, 0x07, 0x03, 0x42, 0x00, 0x04
        };

        /// <summary>
        /// The total length of a DER encoded P-256 public key.
        /// </summary>
        private const int P256PublicKeyLength = 91;

        /// <summary>
        /// The length of one P-256 coordinate or signature component.
        /// </summary>
        private const int P256FieldLength = 32;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Determines whether the signature is a valid SendGrid signature of
        /// the timestamp followed by the body, made with the private key that
        /// matches the verification key.
        /// </summary>
        /// <param name="verificationKey">The base64 DER public key from SendGrid. PEM armor lines and whitespace are allowed.</param>
        /// <param name="body">The raw request body bytes, exactly as received.</param>
        /// <param name="signature">The value of the X-Twilio-Email-Event-Webhook-Signature header.</param>
        /// <param name="timestamp">The value of the X-Twilio-Email-Event-Webhook-Timestamp header.</param>
        /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>, including for any blank or malformed argument.</returns>
        public static bool IsValid( string verificationKey, byte[] body, string signature, string timestamp )
        {
            if ( verificationKey.IsNullOrWhiteSpace() || body == null || signature.IsNullOrWhiteSpace() || timestamp.IsNullOrWhiteSpace() )
            {
                return false;
            }

            /*
                10/21/2026 - MSE

                SendGrid signs the timestamp followed by the body with nothing between
                them, so the same signature also verifies if leading body bytes are moved
                to the end of the timestamp. SendGrid timestamps are Unix seconds, so
                accepting only ASCII digits keeps the timestamp and body boundary fixed.

                Reason: A signed request must not verify with its body split differently.
            */
            if ( !IsAsciiDigits( timestamp ) )
            {
                return false;
            }

            try
            {
                var keyBytes = Convert.FromBase64String( RemovePemArmor( verificationKey ) );
                if ( !IsP256PublicKey( keyBytes ) )
                {
                    return false;
                }

                var x = new byte[P256FieldLength];
                var y = new byte[P256FieldLength];
                Buffer.BlockCopy( keyBytes, _p256PublicKeyPrefix.Length, x, 0, P256FieldLength );
                Buffer.BlockCopy( keyBytes, _p256PublicKeyPrefix.Length + P256FieldLength, y, 0, P256FieldLength );

                var rs = ConvertDerSignatureToRs( Convert.FromBase64String( signature.Trim() ) );
                if ( rs == null )
                {
                    return false;
                }

                var timestampBytes = Encoding.UTF8.GetBytes( timestamp );
                var data = new byte[timestampBytes.Length + body.Length];
                Buffer.BlockCopy( timestampBytes, 0, data, 0, timestampBytes.Length );
                Buffer.BlockCopy( body, 0, data, timestampBytes.Length, body.Length );

                var parameters = new ECParameters
                {
                    Curve = ECCurve.NamedCurves.nistP256,
                    Q = new ECPoint { X = x, Y = y }
                };

                using ( var ecdsa = ECDsa.Create( parameters ) )
                {
                    return ecdsa.VerifyData( data, rs, HashAlgorithmName.SHA256 );
                }
            }
            catch ( FormatException )
            {
                // Intentionally ignored: the key or signature is not valid base64, so the request is not signed by SendGrid.
                return false;
            }
            catch ( ArgumentException )
            {
                // Intentionally ignored: the key or signature has an invalid shape, so the request is not signed by SendGrid.
                return false;
            }
            catch ( CryptographicException )
            {
                // Intentionally ignored: the key or signature could not be used, so the request cannot be verified.
                return false;
            }
            catch ( PlatformNotSupportedException )
            {
                /*
                    10/21/2026 - MSE

                    On .NET Framework, ECDsa.Create( ECParameters ) reports a public key
                    whose point is not on the P-256 curve as PlatformNotSupportedException
                    (wrapping a CryptographicException), not as CryptographicException.
                    A mistyped key must reject the request, not fail it with an error.

                    Reason: A bad verification key must return false, not throw.
                */
                return false;
            }
        }

        /// <summary>
        /// Determines whether the value is one or more ASCII digits (0 to 9).
        /// <see cref="char.IsDigit(char)"/> is not used because it also accepts
        /// digits from other scripts.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value is not empty and every character is 0 to 9; otherwise <c>false</c>.</returns>
        private static bool IsAsciiDigits( string value )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return false;
            }

            foreach ( var character in value )
            {
                if ( character < '0' || character > '9' )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes PEM armor lines (those starting with five hyphens) and all
        /// whitespace from the key, so a pasted PEM block or a key with extra
        /// spaces or line breaks still works. This matches the SendGrid library.
        /// </summary>
        /// <param name="key">The key as entered by the administrator.</param>
        /// <returns>The bare base64 text of the key.</returns>
        private static string RemovePemArmor( string key )
        {
            var builder = new StringBuilder();

            foreach ( var line in key.Split( '\n' ) )
            {
                var trimmedLine = line.Trim();
                if ( trimmedLine.StartsWith( "-----" ) )
                {
                    continue;
                }

                foreach ( var character in trimmedLine )
                {
                    if ( !char.IsWhiteSpace( character ) )
                    {
                        builder.Append( character );
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Determines whether the bytes are a DER encoded uncompressed P-256 public key.
        /// </summary>
        /// <param name="keyBytes">The decoded key bytes.</param>
        /// <returns><c>true</c> if the key has the expected length and prefix; otherwise <c>false</c>.</returns>
        private static bool IsP256PublicKey( byte[] keyBytes )
        {
            if ( keyBytes.Length != P256PublicKeyLength )
            {
                return false;
            }

            for ( var i = 0; i < _p256PublicKeyPrefix.Length; i++ )
            {
                if ( keyBytes[i] != _p256PublicKeyPrefix[i] )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Converts a DER signature, SEQUENCE { INTEGER r, INTEGER s }, to the
        /// 64 byte r followed by s form that .NET expects. A P-256 DER signature
        /// is always shorter than 128 bytes, so every length is a single byte.
        /// </summary>
        /// <param name="der">The DER signature bytes.</param>
        /// <returns>The 64 byte signature, or <c>null</c> if the DER has any other shape.</returns>
        private static byte[] ConvertDerSignatureToRs( byte[] der )
        {
            if ( der.Length < 8 || der[0] != 0x30 || der[1] != der.Length - 2 )
            {
                return null;
            }

            var offset = 2;
            var r = ReadDerInteger( der, ref offset );
            var s = ReadDerInteger( der, ref offset );

            if ( r == null || s == null || offset != der.Length )
            {
                return null;
            }

            var rs = new byte[P256FieldLength * 2];
            Buffer.BlockCopy( r, 0, rs, P256FieldLength - r.Length, r.Length );
            Buffer.BlockCopy( s, 0, rs, ( P256FieldLength * 2 ) - s.Length, s.Length );

            return rs;
        }

        /// <summary>
        /// Reads one DER INTEGER at the offset and moves the offset past it.
        /// A 33 byte value must start with the zero byte that only marks it as
        /// positive; that byte is dropped so the value fits in 32 bytes.
        /// </summary>
        /// <param name="der">The DER signature bytes.</param>
        /// <param name="offset">The offset of the INTEGER tag. On return, the offset just past the value.</param>
        /// <returns>The value bytes (1 to 32 bytes), or <c>null</c> if the INTEGER is malformed.</returns>
        private static byte[] ReadDerInteger( byte[] der, ref int offset )
        {
            if ( offset + 2 > der.Length || der[offset] != 0x02 )
            {
                return null;
            }

            var length = ( int ) der[offset + 1];
            offset += 2;

            if ( length < 1 || length > P256FieldLength + 1 || offset + length > der.Length )
            {
                return null;
            }

            var start = offset;
            var count = length;
            offset += length;

            if ( count == P256FieldLength + 1 )
            {
                if ( der[start] != 0x00 )
                {
                    return null;
                }

                start++;
                count -= 1;
            }

            var value = new byte[count];
            Buffer.BlockCopy( der, start, value, 0, count );

            return value;
        }

        #endregion Methods
    }
}
