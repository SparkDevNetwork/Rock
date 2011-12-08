//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    public class SelectMulti : Field
    {
        private List<FieldQualifier> qualifiers = null;

        /// <summary>
        /// Gets the qualifiers.
        /// </summary>
        public override List<FieldQualifier> Qualifiers
        {
            get
            {
                if ( qualifiers == null )
                {
                    qualifiers = new List<FieldQualifier>();
                    qualifiers.Add( new FieldQualifier( 
                        "Values",
                        "Values", 
                        "Comma-Separated list of values for user to select from",
                        new Text() ) );
                }

                return qualifiers;
            }
        }
    }
}