//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Rock.Field
{
    /// <summary>
    /// Interface that a custom field type must implement
    /// </summary>
    public interface IFieldType
    {
        /// <summary>
        /// Formats the value based on the type and qualifiers
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed );

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        bool IsValid( string value, bool required, out string message );

        /// <summary>
        /// Gets a list of the configuration keys.
        /// </summary>
        /// <returns></returns>
        List<string> ConfigurationKeys();

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        List<Control> ConfigurationControls();

        /// <summary>
        /// Gets the configuration values
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls );

        /// <summary>
        /// Sets the configuration values.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Creates an HTML control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        Control EditControl( Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Reads the value of the control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value );


    }
}
