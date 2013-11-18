//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock.Web.UI.Controls;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute for adding a code editor.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class CodeEditorFieldAttribute : FieldAttribute
    {
        private const string EDITOR_MODE = "editorMode";
        private const string EDITOR_THEME = "editorTheme";
        private const string EDITOR_HEIGHT = "editorHeight";

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRangeFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public CodeEditorFieldAttribute( string name, string description = "", CodeEditorMode mode = CodeEditorMode.Text, CodeEditorTheme theme = CodeEditorTheme.Rock, int height = 200, bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.CodeEditorFieldType ).FullName )
        {
            FieldConfigurationValues.Add(EDITOR_MODE, new Field.ConfigurationValue(mode.ToString()));
            FieldConfigurationValues.Add(EDITOR_THEME, new Field.ConfigurationValue(theme.ToString()));
            FieldConfigurationValues.Add(EDITOR_HEIGHT, new Field.ConfigurationValue(height.ToString()));
        }
    }
}