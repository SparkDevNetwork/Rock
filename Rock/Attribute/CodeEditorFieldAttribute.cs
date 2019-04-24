// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

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
        /// Initializes a new instance of the <see cref="DateRangeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="theme">The theme.</param>
        /// <param name="height">The height.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public CodeEditorFieldAttribute( string name, string description = "", CodeEditorMode mode = CodeEditorMode.Text, CodeEditorTheme theme = CodeEditorTheme.Rock, int height = 200, bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.CodeEditorFieldType ).FullName )
        {
            EditorMode = mode;
            EditorTheme = theme;
            EditorHeight = height;
        }

        /// <summary>
        /// Gets or sets the editor mode.
        /// </summary>
        /// <value>
        /// The editor mode.
        /// </value>
        public virtual CodeEditorMode EditorMode
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( EDITOR_MODE ).ConvertToEnumOrNull<CodeEditorMode>() ?? CodeEditorMode.Text;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( EDITOR_MODE, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets the editor theme.
        /// </summary>
        /// <value>
        /// The editor theme.
        /// </value>
        public virtual CodeEditorTheme EditorTheme
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( EDITOR_MODE ).ConvertToEnumOrNull<CodeEditorTheme>() ?? CodeEditorTheme.Rock;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( EDITOR_MODE, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets the height of the editor.
        /// </summary>
        /// <value>
        /// The height of the editor.
        /// </value>
        public virtual int EditorHeight
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( EDITOR_MODE ).AsIntegerOrNull() ?? 200;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( EDITOR_MODE, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}