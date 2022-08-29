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
using System.Security.Cryptography;
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
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[options.MinimumLength];
            var password = new StringBuilder();

            // Fill the buffer with random bytes and then use that buffer to
            // fill in the password characters.
            rng.GetBytes( bytes );
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
                password.Append( ( char ) GetRandomInteger( rng, 97, 123 ) );
            }

            if ( needUppercase )
            {
                password.Append( ( char ) GetRandomInteger( rng, 65, 91 ) );
            }

            if ( needDigit )
            {
                password.Append( ( char ) GetRandomInteger( rng, 48, 58 ) );
            }

            if ( needNonAlphaNumeric )
            {
                password.Append( PasswordGenerationNonAlphaNumericCharacters[GetRandomInteger( rng, 0, PasswordGenerationNonAlphaNumericCharacters.Length )] );
            }

            return password.ToString();
        }

        /// <summary>
        /// Gets a random 32-bit signed integer that is greater than or equal to minimumValue and less than exclusiveUpperBound
        /// using the <see cref="RandomNumberGenerator" /> class.
        /// </summary>
        /// <param name="rng">The RNG.</param>
        /// <param name="minimumValue">The minimum value of the random number.</param>
        /// <param name="exclusiveUpperBound">The exclusive upper bound of the random number (must be greater than minimumValue).</param>
        /// <returns>System.Int32.</returns>
        private static int GetRandomInteger( RandomNumberGenerator rng, int minimumValue, int exclusiveUpperBound )
        {
            if ( exclusiveUpperBound <= minimumValue )
            {
                exclusiveUpperBound = minimumValue + 1;
            }

            // Generate four random bytes to convert into a 32-bit integer.
            var bytes = new byte[4];
            rng.GetBytes( bytes );
            var random = BitConverter.ToUInt32( bytes, 0 );

            // Convert our random value to a signed integer >= minimumValue and < exclusiveUpperBound.
            return ( int ) ( minimumValue + ( exclusiveUpperBound - minimumValue ) * ( random / ( uint.MaxValue + 1.0 ) ) );
        }

        #endregion
    }
}