using System.Collections.Generic;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class HtmlField : FieldType
    {
        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
        {
            return new CKEditor.NET.CKEditorControl();
        }
    }
}