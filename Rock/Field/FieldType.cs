//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Field
{
    /// <summary>
    /// Abstract class that all custom field types should inherit from
    /// </summary>
    public abstract class FieldType : IFieldType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        public FieldType()
        {
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
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <returns></returns>
        public virtual Control[] ConfigurationControls()
        {
            return null;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="control">The controls.</param>
        /// <returns></returns>
        public virtual Dictionary<string, ConfigurationValue> GetConfigurationValues( Control[] controls )
        {
            return null;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="control">The controls.</param>
        /// <param name="values">The values.</param>
        public virtual void SetConfigurationValues( Control[] controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="value">The current value</param>
        /// <param name="setValue">Should the control's value be set</param>
        /// <returns>The control</returns>
        public virtual Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
        {
            return new TextBox();
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <returns></returns>
        public virtual string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is TextBox )
                return ( ( TextBox )control ).Text;
            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value.</param>
        public virtual void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is TextBox )
                ( ( TextBox )control ).Text = value;
        }

    }

}