//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class AudiencePrimarySecondaryField : SelectFromListFieldType
    {
        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> ListSource
        {
            get
            {
                Dictionary<string, string> choices = new Dictionary<string, string>();
                choices.Add( "1", "Primary" );
                choices.Add( "2", "Secondary" );
                return choices;
            }
        }
    }
}
