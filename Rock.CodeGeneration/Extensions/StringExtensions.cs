using System.Text.RegularExpressions;

namespace Rock.CodeGeneration
{
    /// <summary>
    /// Extensions to the string object.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Splits a Camel or Pascal cased identifier into separate words.
        /// </summary>
        /// <param name="str">The identifier.</param>
        /// <returns></returns>
        public static string SplitCase( this string str )
        {
            if ( str == null )
            {
                return null;
            }

            return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
        }

        /// <summary>
        /// Converts a string to camel case.
        /// </summary>
        /// <param name="str">The string to be converted.</param>
        /// <returns>A string in camel case.</returns>
        public static string CamelCase( this string str )
        {
            if ( str.Length == 0 )
            {
                return str;
            }
            else if ( str.Length == 1 )
            {
                return str.ToLowerInvariant();
            }
            else
            {
                return str.Substring( 0, 1 ).ToLowerInvariant() + str.Substring( 1 );
            }
        }
    }
}
