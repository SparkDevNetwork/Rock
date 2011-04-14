using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field Type used to display a dropdown list of System.Drawing.Color options
    /// </summary>
    public class Color : Field
    {
        public override Control CreateControl( string value )
        {
            DropDownList ddl = new DropDownList();

            Type colors = typeof( System.Drawing.Color );
            PropertyInfo[] colorInfo = colors.GetProperties( BindingFlags.Public | BindingFlags.Static );
            foreach ( PropertyInfo info in colorInfo )
            {
                ListItem li = new ListItem( info.Name, info.Name );
                li.Selected = info.Name == value;
                ddl.Items.Add( li );
            }

            return ddl;
        }

        public override string ReadValue( Control control )
        {
            if ( control != null && control is DropDownList )
                return ( ( DropDownList )control ).SelectedValue;
            return null;
        }
    }
}