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
            if ( str.IsNullOrWhiteSpace() )
            {
                return str;
            }

            var chars = str.ToCharArray();

            // Taken from https://github.com/dotnet/runtime/blob/v8.0.6/src/libraries/System.Text.Json/Common/JsonCamelCaseNamingPolicy.cs
            // so that we can correctly match the camel case naming scheme until we
            // can move away from .NET Framework.
            for ( int i = 0; i < chars.Length; i++ )
            {
                if ( i == 1 && !char.IsUpper( chars[i] ) )
                {
                    break;
                }

                bool hasNext = ( i + 1 < chars.Length );

                // Stop when next char is already lowercase.
                if ( i > 0 && hasNext && !char.IsUpper( chars[i + 1] ) )
                {
                    // If the next char is a space, lowercase current char before exiting.
                    if ( chars[i + 1] == ' ' )
                    {
                        chars[i] = char.ToLowerInvariant( chars[i] );
                    }

                    break;
                }

                chars[i] = char.ToLowerInvariant( chars[i] );
            }

            return new string( chars );
        }
    }
}
