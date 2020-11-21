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
            var rnd = new Random();
            var codeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            var poolSize = codeCharacters.Length;

            for ( int i = 0; i < 24; i++ )
            {
                sb.Append( codeCharacters[rnd.Next( poolSize )] );
            }

            return sb.ToString();
        }
    }
}
