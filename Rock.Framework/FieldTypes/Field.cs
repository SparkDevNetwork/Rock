using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Models.Core;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Abstract class that all custom field types should inherit from
    /// </summary>
    public abstract class Field : IFieldType
    {
        public Field()
        {
        }

        public Field( Dictionary<string, KeyValuePair<string, string>> qualifierValues )
        {
            this.QualifierValues = qualifierValues;
        }

        public Dictionary<string, KeyValuePair<string, string>> QualifierValues { get; set; }

        public virtual List<FieldQualifier> Qualifiers
        {
            get { return null; }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="fieldValue">Information about the value</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public virtual string FormatValue( string value, bool condensed )
        {
            return value;
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual bool IsValid( string value, out string message )
        {
            message = string.Empty;
            return true;
        }

        /// <summary>
        /// Renders the controls neccessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="parentControl">Control that the field type's controls will be added to</param>
        /// <param name="fieldValues">Collection of existing values for the field</param>
        public virtual Control CreateControl( string value )
        {
            TextBox textBox = new TextBox();
            textBox.Text = value;
            return textBox;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="parentControl">Parent control that controls were added to in the RenderEdit() method</param>
        /// <returns></returns>
        public virtual string ReadValue( Control control )
        {
            if ( control != null && control is TextBox )
                return ( ( TextBox )control ).Text;
            return null;
        }
    }

}