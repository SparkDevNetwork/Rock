using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    public class Boolean : Field
    {
        public override string FormatValue( string value, bool condensed )
        {
            if ( string.IsNullOrEmpty(value) ? false : System.Boolean.Parse( value ) )
                return condensed ? Rock.Framework.Properties.Text.Y : Rock.Framework.Properties.Text.Yes;
            else
                return condensed ? Rock.Framework.Properties.Text.N : Rock.Framework.Properties.Text.No;
        }

        public override bool IsValid( string value, out string message )
        {
            bool boolValue = false;
            if ( !bool.TryParse( value, out boolValue ) )
            {
                message = Rock.Framework.Properties.Text.InvalidBooleanValue;
                return false;
            }

            return base.IsValid( value, out message );
        }

        public override Control CreateControl( string value )
        {
            CheckBox cb = new CheckBox();
            cb.Checked = string.IsNullOrEmpty(value) ? false : System.Boolean.Parse( value );
            return cb;
        }

        public override string ReadValue( Control control )
        {
            if ( control != null && control is CheckBox )
                return ( ( CheckBox )control ).Checked.ToString();
            return null;
        }
    }
}