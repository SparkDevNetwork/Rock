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
