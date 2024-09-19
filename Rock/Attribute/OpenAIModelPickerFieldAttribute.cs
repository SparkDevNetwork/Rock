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

using Rock.Enums.Controls;

namespace Rock.Attribute
{
    /// <summary>
    /// Parent field attribute class for field types that are used for selection lists ( Checkbox lists, radio buttons, etc... ).
    /// Adds an option to specify the number of columns to use.
    /// </summary>
    /// <seealso cref="Rock.Attribute.FieldAttribute" />
    public class OpenAIModelPickerFieldAttribute : FieldAttribute
    {
        /// ConfigurationValue Keys
        private const string COLUMN_COUNT = "columnCount";
        private const string DISPLAY_STYLE = "displayStyle";
        private const string ENHANCE_FOR_LONG_LISTS = "enhanceForLongLists";
        private const string IS_MULTIPLE = "isMultiple";

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public OpenAIModelPickerFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.OpenAIModelPickerFieldType ).FullName )
        {
        }

        /// <summary>
        /// Gets or sets the number of columns to use when displaying the values in list.
        /// </summary>
        public int ColumnCount
        {
            get
            {
                var cols = FieldConfigurationValues.GetValueOrNull( COLUMN_COUNT ).AsInteger();
                return cols > 0 ? cols : 4;
            }

            set
            {
                value = value == 0 ? 4: value;
                FieldConfigurationValues.AddOrReplace( COLUMN_COUNT, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets the display style to use when rendering the edit control.
        /// </summary>
        public UniversalItemValuePickerDisplayStyle DisplayStyle
        {
            get
            {
                Enum.TryParse<UniversalItemValuePickerDisplayStyle>( FieldConfigurationValues.GetValueOrNull( DISPLAY_STYLE ), out var parsedEnumValue );
                return parsedEnumValue;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( DISPLAY_STYLE, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets whether the edit control should be rendered
        /// with enhanced selection mode. This provides search capabilities to
        /// the list of items in condensed display mode.
        /// </summary>
        public bool EnhanceForLongLists
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ENHANCE_FOR_LONG_LISTS ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ENHANCE_FOR_LONG_LISTS, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets whether this field type supports multiple
        /// selection or only single selection.
        /// </summary>
        public bool IsMultiple
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( IS_MULTIPLE ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( IS_MULTIPLE, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}
