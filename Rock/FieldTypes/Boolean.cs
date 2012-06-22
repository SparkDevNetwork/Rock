//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    public class Boolean : Field
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, bool condensed )
        {
            if ( string.IsNullOrEmpty(value) ? false : System.Boolean.Parse( value ) )
                return condensed ? "Y" : "Yes";
            else
                return condensed ? "N" : "No";
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            bool boolValue = false;
            if ( !bool.TryParse( value, out boolValue ) )
            {
                message = "Invalid boolean value";
                return false;
            }

            return base.IsValid( value, required, out message );
        }

        /// <summary>
        /// Renders the controls neccessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public override Control CreateControl( string value, bool required, bool setValue )
        {
            CheckBox cb = new CheckBox();
            if (setValue)
                cb.Checked = string.IsNullOrEmpty(value) ? false : System.Boolean.Parse( value );
            return cb;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public override string ReadValue( Control control )
        {
            if ( control != null && control is CheckBox )
                return ( ( CheckBox )control ).Checked.ToString();
            return null;
        }

        /// <summary>
        /// Creates a client-side function that can be called to render the HTML used to update this field and register an event handler
        /// so that updates to the html are saved to a target element.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public override string RegisterUpdateScript( Page page )
        {
            string functionName = this.GetType().Name + "_update";

            string script = string.Format( @"
    function {0}($parent, $target, value){{
        $parent.html('<input type=""checkbox"" ((value.toLowerCase() === 'true') ? ' checked=""checked""' : '') + ' class=""field-value"">' );
        $parent.find('input.field-value').change(function(){{
            $target.val($(this).is(':checked'));
        }});
    }}
", functionName );
            ScriptManager.RegisterStartupScript( page, this.GetType(), functionName, script, true );

            return functionName;
        }

    }
}