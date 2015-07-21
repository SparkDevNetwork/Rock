using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rock.Apps.CheckScannerUtility
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Attempts to convert string to Guid.  Returns Guid.Empty if unsuccessful.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static Guid AsGuid( this string str )
        {
            return str.AsGuidOrNull() ?? Guid.Empty;
        }

        /// <summary>
        /// Attempts to convert string to Guid.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static Guid? AsGuidOrNull( this string str )
        {
            Guid value;
            if ( Guid.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Ensures the trailing backslash. Handy when combining folder paths.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string EnsureTrailingBackslash( this string value )
        {
            return value.TrimEnd( new char[] { '\\', '/' } ) + "\\";
        }

        /// <summary>
        /// Ensures the trailing forward slash. Handy when combining url paths.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string EnsureTrailingForwardslash( this string value )
        {
            return value.TrimEnd( new char[] { '\\', '/' } ) + "/";
        }

        /// <summary>
        /// Splits a Camel or Pascal cased identifier into seperate words.
        /// </summary>
        /// <param name="str">The identifier.</param>
        /// <returns></returns>
        public static string SplitCase( this string str )
        {
            if ( str == null )
                return null;

            return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
        }

        /// <summary>
        /// Returns the string "True" or "False".
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string ToTrueFalse( this bool value )
        {
            return value ? "True" : "False";
        }

        /// <summary>
        /// Removes special characters from the string so that only Alpha, Numeric, '.' and '_' remain;
        /// </summary>
        /// <param name="str">The identifier.</param>
        /// <returns></returns>
        public static string RemoveSpecialCharacters( this string str )
        {
            StringBuilder sb = new StringBuilder();
            foreach ( char c in str )
            {
                if ( ( c >= '0' && c <= '9' ) || ( c >= 'A' && c <= 'Z' ) || ( c >= 'a' && c <= 'z' ) || c == '.' || c == '_' )
                {
                    sb.Append( c );
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts to the enum value to its string value.
        /// </summary>
        /// <param name="eff">The eff.</param>
        /// <param name="SplitCase">if set to <c>true</c> [split case].</param>
        /// <returns></returns>
        public static String ConvertToString( this Enum eff, bool SplitCase = true )
        {
            if ( SplitCase )
            {
                return Enum.GetName( eff.GetType(), eff ).SplitCase();
            }
            else
            {
                return Enum.GetName( eff.GetType(), eff );
            }
        }

        /// <summary>
        /// Attempts to convert string to an integer.  Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public static int? AsIntegerOrNull( this string str )
        {
            int value;
            if ( int.TryParse( str, out value ) )
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Concatonate the items into a Delimited string
        /// </summary>
        /// <example>
        /// FirstNamesList.AsDelimited(",") would be "Ted,Suzy,Noah"
        /// FirstNamesList.AsDelimited(", ", " and ") would be "Ted, Suzy and Noah"
        /// </example>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="finalDelimiter">The final delimiter. Set this if the finalDelimiter should be a different delimiter</param>
        /// <returns></returns>
        public static string AsDelimited<T>( this List<T> items, string delimiter, string finalDelimiter = null )
        {
            List<string> strings = new List<string>();
            foreach ( T item in items )
            {
                strings.Add( item.ToString() );
            }

            if ( finalDelimiter != null && strings.Count > 1 )
            {
                return String.Join( delimiter, strings.Take( strings.Count - 1 ).ToArray() ) + string.Format( "{0}{1}", finalDelimiter, strings.Last() );
            }
            else
            {
                return String.Join( delimiter, strings.ToArray() );
            }
        }
    }
}
