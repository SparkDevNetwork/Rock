using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Rock.Web2
{
    /// <summary>
    /// Provides support for reading and writing legacy WebForms cookies from
    /// Rock.
    /// </summary>
    /// <remarks>
    /// Much of this code is based on the .NET Framework reference source published
    /// by Microsoft. While there was not any code directly copied and pasted much
    /// of it was patterned after the original code to ensure compatibility.
    /// </remarks>
    public class WebFormsCookieDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        /// <summary>
        /// The encryption key used for AES encryption/decryption.
        /// </summary>
        private readonly byte[] _encryptionKey;

        /// <summary>
        /// The validation key used for HMACSHA1 signing/verification.
        /// </summary>
        private readonly byte[] _validationKey;

        /// <summary>
        /// The path used when creating the cookie.
        /// </summary>
        private readonly string _cookiePath;

        /// <summary>
        /// The default expiration if one is not specified on the ticket.
        /// </summary>
        private readonly TimeSpan _defaultExpiration;

        /// <summary>
        /// Creates a new instance of the <see cref="WebFormsCookieDataFormat"/> class.
        /// </summary>
        /// <param name="masterDecryptionHexKey">The master decryption key in hexadecimal format.</param>
        /// <param name="masterValidationHexKey">The master validation key in hexadecimal format.</param>
        /// <param name="cookiePath">The path used when creating the cookie.</param>
        /// <param name="defaultExpiration">The default expiration if one is not specified on the ticket.</param>
        public WebFormsCookieDataFormat( string masterDecryptionHexKey, string masterValidationHexKey, string cookiePath, TimeSpan defaultExpiration )
        {
            var masterDecryptionKey = Enumerable.Range( 0, masterDecryptionHexKey.Length / 2 )
                .Select( i => Convert.ToByte( masterDecryptionHexKey.Substring( i * 2, 2 ), 16 ) ).ToArray();

            var masterValidationKey = Enumerable.Range( 0, masterValidationHexKey.Length / 2 )
                .Select( i => Convert.ToByte( masterValidationHexKey.Substring( i * 2, 2 ), 16 ) ).ToArray();

            _encryptionKey = Sp800_108_CounterMode_HmacSha512( masterDecryptionKey, "FormsAuthentication.Ticket", masterDecryptionKey.Length );
            _validationKey = Sp800_108_CounterMode_HmacSha512( masterValidationKey, "FormsAuthentication.Ticket", masterValidationKey.Length );
            _cookiePath = cookiePath;
            _defaultExpiration = defaultExpiration;

        }

        /// <inheritdoc/>
        public string Protect( AuthenticationTicket data )
        {
            using var memoryStream = new MemoryStream();
            using var serializingBinaryWriter = new SerializingBinaryWriter( memoryStream );

            serializingBinaryWriter.Write( ( byte ) 1 );
            serializingBinaryWriter.Write( ( byte ) 1 );
            serializingBinaryWriter.Write( ( data.Properties.IssuedUtc ?? DateTimeOffset.Now ).Ticks );
            serializingBinaryWriter.Write( ( byte ) 254 );
            serializingBinaryWriter.Write( ( data.Properties.ExpiresUtc ?? DateTimeOffset.Now.Add( _defaultExpiration ) ).Ticks );
            serializingBinaryWriter.Write( data.Properties.IsPersistent );
            serializingBinaryWriter.WriteBinaryString( data.Principal.Claims.First( c => c.Type == ClaimTypes.Name ).Value );
            serializingBinaryWriter.WriteBinaryString( data.Principal.Claims.FirstOrDefault( c => c.Type == ClaimTypes.UserData )?.Value ?? string.Empty );
            serializingBinaryWriter.WriteBinaryString( _cookiePath );
            serializingBinaryWriter.Write( byte.MaxValue );

            var bytes = Encrypt( memoryStream.ToArray() );

            return string.Concat( bytes.Select( b => b.ToString( "X2" ) ) );
        }

        /// <inheritdoc/>
        public string Protect( AuthenticationTicket data, string purpose )
        {
            return Protect( data );
        }

        /// <inheritdoc/>
        public AuthenticationTicket Unprotect( string protectedText )
        {
            if ( string.IsNullOrEmpty( protectedText ) )
            {
                return null;
            }

            var protectedBytes = Enumerable.Range( 0, protectedText.Length / 2 )
                .Select( i => Convert.ToByte( protectedText.Substring( i * 2, 2 ), 16 ) )
                .ToArray();

            var unencryptedBytes = Decrypt( protectedBytes );

            if ( unencryptedBytes == null )
            {
                return null;
            }

            using var memoryStream = new MemoryStream( unencryptedBytes );
            using var reader = new SerializingBinaryReader( memoryStream );

            if ( reader.ReadByte() != 1 )
            {
                return null;
            }

            if ( reader.ReadByte() != 1 )
            {
                return null;
            }

            var issueDateUtc = new DateTimeOffset( reader.ReadInt64(), TimeSpan.Zero );

            if ( reader.ReadByte() != 254 )
            {
                return null;
            }

            var expirationUtc = new DateTimeOffset( reader.ReadInt64(), TimeSpan.Zero );
            var isPersistent = reader.ReadByte() == 1;
            var name = reader.ReadBinaryString();
            var userData = reader.ReadBinaryString();

            // Cookie Path, we ignore this for now because it is configured
            // on the cookie itself.
            reader.ReadBinaryString();

            if ( reader.ReadByte() != byte.MaxValue )
            {
                return null;
            }

            if ( memoryStream.Position != unencryptedBytes.Length )
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, name),
                new(ClaimTypes.UserData, userData),
            };

            var identity = new ClaimsIdentity( claims, CookieAuthenticationDefaults.AuthenticationScheme );
            var principal = new ClaimsPrincipal( identity );

            var properties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                IssuedUtc = issueDateUtc,
                ExpiresUtc = expirationUtc,
            };

            return new AuthenticationTicket( principal, properties, CookieAuthenticationDefaults.AuthenticationScheme );
        }

        /// <inheritdoc/>
        public AuthenticationTicket Unprotect( string protectedText, string purpose )
        {
            return Unprotect( protectedText );
        }

        /// <summary>
        /// Decrypts AES-encrypted data from the cookie.
        /// </summary>
        private byte[] Decrypt( byte[] encryptedBytes )
        {
            // SHA1 signature is 20 bytes
            int signatureLength = 20;
            if ( encryptedBytes.Length <= signatureLength )
            {
                return null;
            }

            // Split encrypted data and signature
            var cipherBytes = new byte[encryptedBytes.Length - signatureLength];
            Array.Copy( encryptedBytes, cipherBytes, cipherBytes.Length );

            var signature = new byte[signatureLength];
            Array.Copy( encryptedBytes, encryptedBytes.Length - signatureLength, signature, 0, signatureLength );

            // Validate HMACSHA1
            using ( var hmac = new HMACSHA1( _validationKey ) )
            {
                var computedSignature = hmac.ComputeHash( cipherBytes );
                if ( !computedSignature.SequenceEqual( signature ) )
                {
                    // Signature mismatch, tampered or invalid cookie
                    return null;
                }
            }

            // AES block size is 16 bytes
            var iv = new byte[16];
            Array.Copy( cipherBytes, 0, iv, 0, iv.Length );

            var actualCipher = new byte[cipherBytes.Length - iv.Length];
            Array.Copy( cipherBytes, iv.Length, actualCipher, 0, actualCipher.Length );

            using var aes = Aes.Create();

            aes.Key = _encryptionKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream( actualCipher );
            using var cs = new CryptoStream( ms, decryptor, CryptoStreamMode.Read );
            using var outMs = new MemoryStream();

            cs.CopyTo( outMs );

            return outMs.ToArray();
        }

        /// <summary>
        /// Encrypts the plain bytes using AES algorithm.
        /// </summary>
        /// <param name="plainBytes">The byte array to be encrypted.</param>
        /// <returns>The encrypted byte array.</returns>
        private byte[] Encrypt( byte[] plainBytes )
        {
            using var aes = Aes.Create();

            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var ms = new MemoryStream();

            // Prepend IV
            ms.Write( aes.IV, 0, aes.IV.Length );

            using ( var encryptor = aes.CreateEncryptor() )
            using ( var cs = new CryptoStream( ms, encryptor, CryptoStreamMode.Write ) )
            {
                cs.Write( plainBytes, 0, plainBytes.Length );
                cs.FlushFinalBlock();
            }

            var encryptedData = ms.ToArray();

            // Compute HMACSHA1 signature
            using var hmac = new HMACSHA1( _validationKey );

            var signature = hmac.ComputeHash( encryptedData );

            // Concatenate encrypted data + signature
            var result = new byte[encryptedData.Length + signature.Length];

            Buffer.BlockCopy( encryptedData, 0, result, 0, encryptedData.Length );
            Buffer.BlockCopy( signature, 0, result, encryptedData.Length, signature.Length );

            return result;
        }

        /// <summary>
        /// A SP800-108 KDF in Counter Mode using HMACSHA512 as the PRF. This
        /// matches what is used in ASP.NET for key derivation.
        /// </summary>
        /// <param name="masterKey">The master key.</param>
        /// <param name="purpose">The purpose string to use when deriving the key.</param>
        /// <param name="keyLengthBytes">The target key length in bytes.</param>
        /// <returns>The data of the derived key.</returns>
        private static byte[] Sp800_108_CounterMode_HmacSha512( byte[] masterKey, string purpose, int keyLengthBytes )
        {
            var label = Encoding.UTF8.GetBytes( purpose );
            var context = Array.Empty<byte>();
            var result = new byte[keyLengthBytes];
            int hashLength = 64; // SHA512 output size
            int blocks = ( int ) Math.Ceiling( ( double ) keyLengthBytes / hashLength );

            using ( var hmac = new HMACSHA512( masterKey ) )
            {
                for ( int i = 1, offset = 0; i <= blocks; i++ )
                {
                    // Counter (4 bytes, big-endian)
                    var counter = BitConverter.GetBytes( i );
                    if ( BitConverter.IsLittleEndian )
                    {
                        Array.Reverse( counter );
                    }

                    // KDF input: counter || label || 0x00 || context || keyLength (4 bytes, big-endian)
                    using var ms = new MemoryStream();

                    ms.Write( counter, 0, 4 );
                    ms.Write( label, 0, label.Length );
                    ms.WriteByte( 0x00 );
                    ms.Write( context, 0, context.Length );

                    var lengthBytes = BitConverter.GetBytes( keyLengthBytes * 8 );

                    if ( BitConverter.IsLittleEndian )
                    {
                        Array.Reverse( lengthBytes );
                    }

                    ms.Write( lengthBytes, 0, 4 );

                    var kdfInput = ms.ToArray();
                    var block = hmac.ComputeHash( kdfInput );
                    int toCopy = Math.Min( hashLength, keyLengthBytes - offset );

                    Array.Copy( block, 0, result, offset, toCopy );

                    offset += toCopy;
                }
            }

            return result;
        }

        /// <summary>
        /// Binary reader that supports the WebForms style of reading strings.
        /// </summary>
        private sealed class SerializingBinaryReader : BinaryReader
        {
            public SerializingBinaryReader( Stream input )
                : base( input )
            {
            }

            public string ReadBinaryString()
            {
                var num = Read7BitEncodedInt();
                var array = ReadBytes( num * 2 );
                var array2 = new char[num];

                for ( int i = 0; i < array2.Length; i++ )
                {
                    array2[i] = ( char ) ( array[2 * i] | ( array[2 * i + 1] << 8 ) );
                }

                return new string( array2 );
            }
        }

        /// <summary>
        /// Binary writer that supports the WebForms style of writing strings.
        /// </summary>
        private sealed class SerializingBinaryWriter : BinaryWriter
        {
            public SerializingBinaryWriter( Stream output )
                : base( output )
            {
            }

            public void WriteBinaryString( string value )
            {
                var array = new byte[value.Length * 2];

                for ( int i = 0; i < value.Length; i++ )
                {
                    char c = value[i];
                    array[2 * i] = ( byte ) c;
                    array[2 * i + 1] = ( byte ) ( ( int ) c >> 8 );
                }

                Write7BitEncodedInt( value.Length );
                Write( array );
            }
        }
    }
}
