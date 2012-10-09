using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Rock.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class RouteAttribute : ValidationAttribute
    {
        /// <summary>
        /// Determines whether the specified value of the object is valid.
        /// </summary>
        /// <param name="value">The value of the object to validate.</param>
        /// <returns>
        /// true if the specified value is valid; otherwise, false.
        /// </returns>
        public override bool IsValid( object value )
        {
            try
            {
                string routeText = ( value as string );
                Route testRoute = new Route( routeText, null );
                return true;
            }
            catch ( Exception ex )
            {
                string[] exceptionLines = ex.Message.Split( new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries );
                
                // just get first line of message, and escape any braces that might be in it
                ErrorMessage = exceptionLines[0].Replace( "{", "&#123;" ).Replace( "}", "&#125;" );
                return false;
            }
        }
    }
}