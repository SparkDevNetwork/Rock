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
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Utility
{
    /// <summary>
    /// A helper class for generating random keys.
    /// </summary>
    public static class KeyHelper
    {
        /// <summary>
        /// Generates a random alpha numeric key while making sure the generated key doesn't match the filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static string GenerateKey( Func<RockContext, string, bool> filter )
        {
            using ( var rockContext = new RockContext() )
            {
                var key = string.Empty;
                do
                {
                    key = GenerateKey();
                } while ( filter( rockContext, key ) );

                return key;
            }
        }

        /// <summary>
        /// Generates a random alpha numeric key.
        /// </summary>
        /// <returns></returns>
        public static string GenerateKey()
        {
            var sb = new StringBuilder();
            var codeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            var poolSize = codeCharacters.Length;

            for ( int i = 0; i < 24; i++ )
            {
                int next = GetRandomInteger( 0, poolSize );
                sb.Append( codeCharacters[next] );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a random 32-bit signed integer that is greater than or equal to minimumValue and less than exclusiveUpperBound
        /// using the <see cref="RandomNumberGenerator"/> class.
        /// </summary>
        /// <param name="minimumValue">The minimum value of the random number.</param>
        /// <param name="exclusiveUpperBound">The exclusive upper bound of the random number (must be greater than minimumValue).</param>
        /// <returns></returns>
        private static int GetRandomInteger( int minimumValue, int exclusiveUpperBound )
        {
            if ( exclusiveUpperBound <= minimumValue )
            {
                exclusiveUpperBound = minimumValue + 1;
            }

            // Generate four random bytes to convert into a 32-bit integer.
            var bytes = new byte[4];
            RandomNumberGenerator.Create().GetBytes( bytes );
            var random = BitConverter.ToUInt32( bytes, 0 );

            // Convert our random value to a signed integer >= minimumValue and < exclusiveUpperBound.
            return ( int ) ( minimumValue + ( exclusiveUpperBound - minimumValue ) * ( random / ( uint.MaxValue + 1.0 ) ) );
        }
    }
}
