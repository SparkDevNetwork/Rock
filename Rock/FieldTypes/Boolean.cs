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
        public override bool IsValid( string value, out string message )
        {
            bool boolValue = false;
            if ( !bool.TryParse( value, out boolValue ) )
            {
                message = "Invalid boolean value";
                return false;
            }

            return base.IsValid( value, out message );
        }

        /// <summary>
        /// Renders the controls neccessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public override Control CreateControl( string value, bool setValue )
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
        /// Creates a client-side function that can be called to display appropriate html and event handler to update the target element.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="id">The id.</param>
        /// <param name="value"></param>
        /// <param name="parentElement">The parent element.</param>
        /// <param name="targetElement">The target element.</param>
        /// <returns></returns>
        public override string ClientUpdateScript( Page page, string id, string value, string parentElement, string targetElement )
        {
            string uniqueId = parentElement + ( string.IsNullOrWhiteSpace( id ) ? "" : "_" + id );
            string functionName = uniqueId + "_Save_" + this.GetType().Name;

            string script = string.Format( @"

    function {0}(value){{
        $('#{1}').html('<input type=""checkbox"" id=""{2}"" name=""{2}""' + ((value.toLowerCase() === 'true') ? ' checked=""checked""' : '') + '>' );
        $('#{1} #{2}').change(function(){{
            $('#{3}').val($(this).is(':checked'));
        }});
    }}

", functionName, parentElement, uniqueId, targetElement );

            ScriptManager.RegisterStartupScript( page, this.GetType(), functionName, script, true );

            return functionName;
        }

        /// <summary>
        /// Registers a client change script that will update a target element with a controls value whenever it is changed.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="targetElement">The target element.</param>
        public override void RegisterClientChangeScript( Control control, string targetElement )
        {
            string script = string.Format( @"

    Sys.Application.add_load(function () {{
        $('#{0}').change(function(){{
            $('#{1}').val($(this).is(':checked'));
        }});
    }})

", control.ClientID, targetElement );

            ScriptManager.RegisterStartupScript( control.Page, this.GetType(), "Save_" + control.ClientID, script, true );
        }
    }
}