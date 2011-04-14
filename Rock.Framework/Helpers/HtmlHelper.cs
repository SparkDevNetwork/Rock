using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Helpers
{
    public class HtmlHelper
    {

        /// <summary>
        /// Formats a string for use as a CCS class or id value
        /// </summary>
        /// <param name="value">String to format</param>
        /// <returns></returns>
        public static string CssClassFormat( string value )
        {
            value = value.ToLower();
            value = value.Replace( ' ', '-' );

            return value;
        }
    }
}