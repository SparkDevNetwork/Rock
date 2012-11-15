using System.Collections.Generic;
using System.Web.UI;
using CKEditor.NET;

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
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
        {
            CKEditorControl editor = new CKEditorControl();
            editor.Toolbar = "RockCustomConfig";
            return editor;
        }
    }
}