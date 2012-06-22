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
    /// Field Type used to display a dropdown list of Defined Values for a specific Defined Type
    /// </summary>
    public class SelectDefinedValue : Field
    {
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
                        "DefinedType",
                        "Defined Type",
                        "The Defined Type to select a value from",
                        new SelectDefinedType() ) );
                }

                return qualifiers;
            }
        }
        private List<FieldQualifier> qualifiers = null;

        /// <summary>
        /// Renders the controls neccessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public override Control CreateControl( string value, bool required, bool setValue )
        {
            DropDownList list = new DropDownList();

            if ( !required )
                list.Items.Add( new ListItem( string.Empty, "0" ) );

            int definedTypeId = 0;
            if ( int.TryParse(this.QualifierValues["DefinedType"].Value, out definedTypeId ) )
            {
                Rock.Core.DefinedValueService definedValueService = new Core.DefinedValueService();
                foreach ( var definedValue in definedValueService.GetByDefinedTypeId(definedTypeId) )
                    list.Items.Add( new ListItem( definedValue.Name, definedValue.Id.ToString() ) );
            }

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