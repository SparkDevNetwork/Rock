using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.bemaservices.RemoteCheckDeposit
{
    /// <summary>
    /// Provides extra methods to the string class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns the leftmost part of a string, of at most size characters.
        /// </summary>
        /// <param name="value">The string value to be trimmed.</param>
        /// <param name="size">The maximum string size to return.</param>
        /// <returns>A truncated string of no more than size characters.</returns>
        public static string Left( this string value, int size )
        {
            if ( value == null )
            {
                return null;
            }

            if ( value.Length > size )
            {
                return value.Substring( 0, size );
            }

            return value;
        }

        /// <summary>
        /// Returns the rightmost part of a string, of at most size characters.
        /// </summary>
        /// <param name="value">The string value to be trimmed.</param>
        /// <param name="size">The maximum string size to return.</param>
        /// <returns>A truncated string of no more than size characters.</returns>
        public static string Right( this string value, int size )
        {
            if ( value == null )
            {
                return null;
            }

            if ( value.Length > size )
            {
                return value.Substring( value.Length - size, size );
            }

            return value;
        }
    }
}
