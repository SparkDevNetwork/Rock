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
using System.Linq;
using System.Text;

using Rock.Security.Options;

namespace Rock.Security
{
    /// <summary>
    /// Support methods for working with passwords.
    /// </summary>
    internal static class Password
    {
        #region Constants

        /// <summary>
        /// The alpha numeric characters that can be used generated in passwords.
        /// </summary>
        private static readonly char[] PasswordGenerationAlphaNumericCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        /// <summary>
        /// The non-alpha numeric characters that can be used generated in passwords.
        /// </summary>
        private static readonly char[] PasswordGenerationNonAlphaNumericCharacters = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

        /// <summary>
        /// The total set of characters that can be used during password generation.
        /// </summary>
        private static readonly char[] PasswordGenerationCharacters = PasswordGenerationAlphaNumericCharacters.Concat( PasswordGenerationNonAlphaNumericCharacters ).ToArray();

        #endregion

        #region Methods


        /// <summary>
        /// Generates a password that meets a standard set of options.
        /// </summary>
        /// <returns>A string that represents the generated password.</returns>
        public static string GeneratePassword()
        {
            return GeneratePassword( new PasswordRequirementOptions
            {
                MinimumLength = 12,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true
            } );
        }

        /// <summary>
        /// Generates a password that meets the specified password options.
        /// </summary>
        /// <param name="options">The options that dictate how the password is generated.</param>
        /// <returns>A string that represents the generated password.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">options</exception>
        public static string GeneratePassword( PasswordRequirementOptions options )
        {
            if ( options.MinimumLength <= 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( options ), $"{nameof( options.MinimumLength )} must be greater than 0." );
            }

            // Get all the flags that determine the required characters that must
            // be generated on the password. This lets us change the flags later.
            var needNonAlphaNumeric = options.RequireNonAlphanumeric;
            var needDigit = options.RequireDigit;
            var needLowercase = options.RequireLowercase;
            var needUppercase = options.RequireUppercase;

            // Create a random number generator and a place to store the password
            // while it is being generated.
            var rng = new Random();
            var bytes = new byte[options.MinimumLength];
            var password = new StringBuilder();

            // Fill the buffer with random bytes and then use that buffer to
            // fill in the password characters.
            rng.NextBytes( bytes );
            for ( int i = 0; i < bytes.Length; i++ )
            {
                var character = PasswordGenerationCharacters[bytes[i] % PasswordGenerationCharacters.Length];

                // Mark any flags as no longer needed if we have a matching
                // character.
                if ( char.IsLower( character ) )
                {
                    needLowercase = false;
                }
                else if ( char.IsUpper( character ) )
                {
                    needUppercase = false;
                }
                else if ( char.IsDigit( character ) )
                {
                    needDigit = false;
                }
                else if ( !char.IsLetterOrDigit( character ) )
                {
                    needNonAlphaNumeric = false;
                }

                password.Append( character );
            }

            // Append any additional characters required to fulfill the password
            // requirements.
            if ( needLowercase )
            {
                password.Append( ( char ) rng.Next( 97, 123 ) );
            }

            if ( needUppercase )
            {
                password.Append( ( char ) rng.Next( 65, 91 ) );
            }

            if ( needDigit )
            {
                password.Append( ( char ) rng.Next( 48, 58 ) );
            }

            if ( needNonAlphaNumeric )
            {
                password.Append( PasswordGenerationNonAlphaNumericCharacters[rng.Next( PasswordGenerationNonAlphaNumericCharacters.Length )] );
            }

            return password.ToString();
        }

        #endregion
    }
}