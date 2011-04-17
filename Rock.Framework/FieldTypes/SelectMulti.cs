using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    public class SelectMulti : Field
    {
        private List<FieldQualifier> qualifiers = null;
        public override List<FieldQualifier> Qualifiers
        {
            get
            {
                if ( qualifiers == null )
                {
                    qualifiers = new List<FieldQualifier>();
                    qualifiers.Add( new FieldQualifier( 
                        Rock.Framework.Properties.Text.Values, 
                        Rock.Framework.Properties.Text.CommaSeparatedList,
                        new Text() ) );
                }

                return qualifiers;
            }
        }
    }
}