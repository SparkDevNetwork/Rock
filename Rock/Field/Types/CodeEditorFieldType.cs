//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Web.UI;

using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class CodeEditorFieldType : FieldType
    {
        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editor = new CodeEditor { ID = id };
            editor.EditorTheme = CodeEditorTheme.Rock;
            editor.EditorMode = CodeEditorMode.CSharp;
            return editor;
        }
    }
}