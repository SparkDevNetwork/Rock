//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Web.UI;
using CKEditor.NET;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class HtmlFieldType : FieldType
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
            editor.Toolbar = "RockCustomConfigLight";
            return editor;
        }
    }
}