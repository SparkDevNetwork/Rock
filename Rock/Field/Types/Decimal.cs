﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and dispaly a decimal numeric value
    /// </summary>
    [Serializable]
    public class Decimal : FieldType
    {
        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                decimal result;
                if ( !decimal.TryParse( value, out result ) )
                {
                    message = "The input provided is not a valid decimal value.";
                    return true;
                }
            }

            return base.IsValid( value, required, out message );
        }
    }
}