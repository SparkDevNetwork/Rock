using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Utility
{
    /// <summary>
    /// Helper class to convert a Guid to and from a ShortString Guid
    /// See https://stackoverflow.com/a/40917033/1755417
    /// </summary>
    public static class GuidHelper
    {
        /// <summary>
        /// Converts the Guid to a shortened string that can be converted back to a Guid using <seealso cref="FromShortString(string)"/>.
        /// See https://stackoverflow.com/a/40917033/1755417
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public static string ToShortString( Guid guid )
        {
            // see https://stackoverflow.com/a/40917033/1755417
            var base64Guid = Convert.ToBase64String( guid.ToByteArray() );

            // Remove the trailing ==
            return base64Guid.Substring( 0, base64Guid.Length - 2 );
        }

        /// <summary>
        /// Converts Guid shortened with <seealso cref="ToShortString(Guid)"/> back to a Guid.
        /// Returns Guid.Empty if unsuccessful.
        /// See https://stackoverflow.com/a/40917033/1755417,
        /// </summary>
        /// <param name="shortString">The short string.</param>
        /// <returns></returns>
        public static Guid FromShortString( string shortString)
        {
            return FromShortStringOrNull( shortString ) ?? Guid.Empty;
        }

        /// <summary>
        /// Converts Guid shortened with <seealso cref="ToShortString(Guid)"/> back to a Guid.
        /// Returns null if unsuccessful.
        /// See https://stackoverflow.com/a/40917033/1755417,
        /// </summary>
        /// <param name="shortString">The short string.</param>
        /// <returns></returns>
        public static Guid? FromShortStringOrNull( string shortString )
        {
            if ( shortString.IsNullOrWhiteSpace() )
            {
                return null;
            }

            try
            {
                var byteArray = Convert.FromBase64String( shortString + "==" );
                return new Guid( byteArray );
            }
            catch
            {
                return null;
            }
        }
    }
}
