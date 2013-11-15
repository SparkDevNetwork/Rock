//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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

        private const string EDITOR_MODE = "editorMode";
        private const string EDITOR_THEME = "editorTheme";
        
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

            if ( configurationValues != null && configurationValues.ContainsKey( EDITOR_MODE )) {
                editor.EditorMode = (CodeEditorMode)Enum.Parse(typeof(CodeEditorMode), configurationValues[EDITOR_MODE].Value);
            }

            if (configurationValues != null && configurationValues.ContainsKey(EDITOR_THEME))
            {
                editor.EditorTheme = (CodeEditorTheme)Enum.Parse(typeof(CodeEditorTheme), configurationValues[EDITOR_THEME].Value);
            }

            return editor;
        }
    }
}