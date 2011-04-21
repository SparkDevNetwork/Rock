using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        /// <summary>
        /// Adds a query parameter and value to a given address checking for the existance of the ? and if the parameter already exists
        /// </summary>
        /// <param name="value">String to format</param>
        /// <returns></returns>
        public static string AppendQueryParameter( string address, string parameter, string value )
        {
            string finalAddress = "";
            parameter = HttpUtility.HtmlEncode(parameter);
            value = HttpUtility.HtmlEncode(value);

            // check if address already has a ?
            if(address.Contains("?")){          
                // rebuild the address removing the paramenter if it's already there
                string[] addressParts = address.Split( '?' );

                finalAddress = addressParts[0];

                // break apart key value pairs
                string[] pairs = addressParts[1].Split( '&' );

                bool firstPair = true;

                // go through each pair
                foreach ( string pair in pairs )
                {                    
                    string[] keyValue = pair.Split( '=' );
                    
                    // check to ensure it has both a key and value
                    if ( keyValue.Count() == 2 )
                    {
                        // check to see if key is the same as the one to add.  if so skip
                        if ( HttpUtility.HtmlDecode(keyValue[0]) != parameter )
                        {
                            if ( firstPair )
                            {
                                finalAddress += "?" + HttpUtility.HtmlEncode( keyValue[0] ) + "=" + HttpUtility.HtmlEncode( keyValue[1] );
                                firstPair = false;
                            }
                            else
                                finalAddress += "&" + HttpUtility.HtmlEncode( keyValue[0] ) + "=" + HttpUtility.HtmlEncode( keyValue[1] );
                        }
                    }
                }

                // added requested parameter
                if (firstPair)
                    finalAddress += "?" + HttpUtility.HtmlEncode(parameter) + "=" + HttpUtility.HtmlEncode(value);
                else
                    finalAddress += "&" + HttpUtility.HtmlEncode( parameter ) + "=" + HttpUtility.HtmlEncode( value );
            }
            else
                finalAddress = address + "?" + HttpUtility.HtmlEncode(parameter) + "=" + HttpUtility.HtmlEncode(value);

            return finalAddress;
        }
    }
}