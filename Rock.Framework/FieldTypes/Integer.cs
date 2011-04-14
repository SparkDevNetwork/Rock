using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field used to save and dispaly a text value
    /// </summary>
    public class Integer : Field
    {
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