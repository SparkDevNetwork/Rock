//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field Type used to display a dropdown list of Defined Types
    /// </summary>
    public class SelectDefinedType : Field
    {
        /// <summary>
        /// Renders the controls neccessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public override Control CreateControl( string value, bool required, bool setValue )
        {
            DropDownList list = new DropDownList();

            if (!required)
                list.Items.Add(new ListItem(string.Empty, "0"));

            Rock.Core.DefinedTypeService definedTypeService = new Core.DefinedTypeService();
            foreach ( var definedType in definedTypeService.Queryable().OrderBy( d => d.Order ) )
                list.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );

            return list;
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