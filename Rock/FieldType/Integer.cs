using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.FieldType
{
    /// <summary>
    /// Field used to save and dispaly a numeric value
    /// </summary>
    public class Integer : Field
    {
        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool IsValid( string value, out string message )
        {
            int result;

            if ( Int32.TryParse( value, out result ) )
            {
                message = string.Empty;
                return true;
            }
            else
            {
                message = "The input provided is not a valid integer.";
                return true;
            }
        }
    }
}