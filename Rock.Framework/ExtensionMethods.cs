using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rock
{
    /// <summary>
    /// Extension Methods
    /// </summary>
    public static class ExtensionMethods
    {
        #region String Extensions

        /// <summary>
        /// Splits a Camel or Pascal cased identifier into seperate words.
        /// </summary>
        /// <param name="str">The identifier.</param>
        /// <returns></returns>
        public static string SplitCase( this string str )
        {
            return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
        }

        #endregion

        #region MembershipUser Extensions

        /// <summary>
        /// Returns the PersonId associated with the <see cref="System.Web.Security.MembershipUser"/> object
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public static int? PersonId( this System.Web.Security.MembershipUser user )
        {
            if ( user.ProviderUserKey != null )
                return ( int )user.ProviderUserKey;
            else
                return null;
        }

        #endregion
    }
}