//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Abstract class that all custom field types should inherit from
    /// </summary>
    public abstract class Field : IFieldType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        public Field()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="qualifierValues">The qualifier values.</param>
        public Field( Dictionary<string, KeyValuePair<string, string>> qualifierValues )
        {
            this.QualifierValues = qualifierValues;
        }

        /// <summary>
        /// Gets or sets the qualifier values.
        /// </summary>
        /// <value>
        /// The qualifier values. The Dictionary's key contains the qualifier key, the KeyValuePair's
        /// key contains the qualifier name, and the KeyValuePair's value contains the qualifier
        /// value
        /// </value>
        public Dictionary<string, KeyValuePair<string, string>> QualifierValues { get; set; }

        /// <summary>
        /// Gets the qualifiers.
        /// </summary>
        public virtual List<FieldQualifier> Qualifiers
        {
            get { return null; }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public virtual string FormatValue( Control parentControl, string value, bool condensed )
        {
            return value;
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual bool IsValid( string value, bool required, out string message )
        {
            if ( required && string.IsNullOrWhiteSpace( value ) )
            {
                message = "value is required.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="value">The current value</param>
        /// <param name="setValue">Should the control's value be set</param>
        /// <returns>The control</returns>
        public virtual Control CreateControl( string value, bool required, bool setValue )
        {
            TextBox textBox = new TextBox();
            if (setValue)
                textBox.Text = value;
            return textBox;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the RenderEdit() method</param>
        /// <returns></returns>
        public virtual string ReadValue( Control control )
        {
            if ( control != null && control is TextBox )
                return ( ( TextBox )control ).Text;
            return null;
        }

        /// <summary>
        /// Creates a client-side function that can be called to render the HTML used to update this field and register an event handler
        /// so that updates to the html are saved to a target element.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public virtual string RegisterUpdateScript( Page page )
        {
            string functionName = this.GetType().Name + "_update";

            string script = string.Format( @"
    function {0}($parent, $target, value){{
        $parent.html('<input type=""text"" value=""' + value + '"" class=""field-value"">' );
        $parent.find('input.field-value').change(function(){{
            $target.val($(this).val());
        }});
    }}
", functionName);
            ScriptManager.RegisterStartupScript( page, this.GetType(), functionName, script, true );

            return functionName;
        }
    }

}