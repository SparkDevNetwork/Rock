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

namespace Rock.Lava
{
    /// <summary>
    /// Defines filter methods available for use with the Lava library.
    /// </summary>
    internal static partial class LavaFilters
    {
        /// <summary>
        /// Encrypts a string of text with the encryption key configured for the current Rock instance.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        /// <remarks>
        /// The encryption algorithm uses the DataEncryptionKey configured for the current Rock installation.
        /// Decryption of a string encrypted using this method can only be performed with a matching DataEncryptionKey.
        /// Furthermore, the encryption process includes an initialization vector (IV) to randomize the result,
        /// so subsequent executions of this filter will produce different results for the same input text.
        /// </remarks>
        public static string Encrypt( string input )
        {
            if ( input == null )
            {
                return input;
            }

            var encryptedString = Rock.Security.Encryption.EncryptString( input );
            return encryptedString;
        }

        /// <summary>
        /// Decrypts a string previously encrypted with the <see cref="Encrypt(string)"/> filter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Decrypt( string input )
        {
            if ( input == null )
            {
                return input;
            }

            return Rock.Security.Encryption.DecryptString( input );
        }

        /// <summary>
        /// Converts the input text to Markdown. The <paramref name="sourceType"/> parameter
        /// controls how the input is interpreted; "html" is currently the only supported value
        /// and is the default. The parameter is included so that additional source formats can
        /// be supported in the future without changing the filter's signature.
        /// </summary>
        /// <param name="input">The input to convert.</param>
        /// <param name="sourceType">The format of the input. Defaults to "html".</param>
        /// <returns>A Markdown representation of the input, or an empty string when the input is null or empty.</returns>
        public static string ToMarkdown( object input, string sourceType = "html" )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            var inputString = input.ToString();

            if ( string.IsNullOrEmpty( inputString ) )
            {
                return string.Empty;
            }

            var format = ( sourceType ?? "html" ).Trim();

            if ( string.Equals( format, "html", StringComparison.OrdinalIgnoreCase ) )
            {
                var converter = new ReverseMarkdown.Converter();
                return converter.Convert( inputString );
            }

            // Unsupported source types are returned unchanged so that existing templates
            // do not fail if a future source type name is passed to an older version of Rock.
            return inputString;
        }
    }
}
