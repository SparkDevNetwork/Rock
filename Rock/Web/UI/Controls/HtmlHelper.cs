//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Security.Cryptography;
using System.Text;

namespace Rock.Web.UI.Controls
    
    /// <summary>
    /// Provides helper methods for use in rendering HTML
    /// </summary>
    public static class HtmlHelper
        

        /// <summary>
        /// Formats a string for use as a CCS class or id value
        /// </summary>
        /// <param name="value">String to format</param>
        /// <returns></returns>
        public static string CssClassFormat( string value )
            
            value = value.ToLower();
            value = value.Replace( ' ', '-' );

            return value;
        }

        /// <summary>
        /// Hashes a string using MD5
        /// </summary>
        /// <param name="input">String to format</param>
        /// <returns></returns>
        public static string CalculateMD5Hash( string input )
            
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes( input );
            byte[] hash = md5.ComputeHash( inputBytes );

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for ( int i = 0; i < hash.Length; i++ )
                
                sb.Append( hash[i].ToString( "X2" ) );
            }
            return sb.ToString().ToLower();
        }
    }
}