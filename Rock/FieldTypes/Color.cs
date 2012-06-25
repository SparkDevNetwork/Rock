//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field Type used to display a dropdown list of System.Drawing.Color options
    /// </summary>
    public class Color : Field
    {
        /// <summary>
        /// Renders the controls neccessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public override Control CreateControl( string value, bool required, bool setValue )
        {
            DropDownList ddl = new DropDownList();

            Type colors = typeof( System.Drawing.Color );
            PropertyInfo[] colorInfo = colors.GetProperties( BindingFlags.Public | BindingFlags.Static );
            foreach ( PropertyInfo info in colorInfo )
            {
                ListItem li = new ListItem( info.Name, info.Name );
                if (setValue)
                    li.Selected = info.Name == value;
                ddl.Items.Add( li );
            }

            return ddl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public override string ReadValue( Control control )
        {
            if ( control != null && control is DropDownList )
                return ( ( DropDownList )control ).SelectedValue;
            return null;
        }
    }
}