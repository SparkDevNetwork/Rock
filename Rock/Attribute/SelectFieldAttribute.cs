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
namespace Rock.Attribute
{
    /// <summary>
    /// Parent field attribute class for field types that are used for selection lists ( Checkbox lists, radio buttons, etc... ).
    /// Adds an option to specify the number of columns to use.
    /// </summary>
    /// <seealso cref="Rock.Attribute.FieldAttribute" />
    public abstract class SelectFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// The key value for the repeatColumns value to use in the FieldConfigurationValues dictionary
        /// </summary>
        private const string REPEAT_COLUMNS = "repeatColumns";

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
        /// <param name="fieldTypeClass">The field type class.</param>
        /// <param name="fieldTypeAssembly">The field type assembly.</param>
        public SelectFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null, string fieldTypeClass = null, string fieldTypeAssembly = "Rock" )
            : base( name, description, required, defaultValue, category, order, key, fieldTypeClass, fieldTypeAssembly )
        {
        }

        /// <summary>
        /// Gets or sets the number of columns to use for the list. Other things, such as available space and length of list values may affect this and reduce the acutal number of columns displayed.
        /// </summary>
        /// <value>
        /// The repeat columns.
        /// </value>
        public int RepeatColumns
        {
            get
            {
                var cols = FieldConfigurationValues.GetValueOrNull( REPEAT_COLUMNS ).AsInteger();
                return cols > 0 ? cols : 4;
            }

            set
            {
                value = value == 0 ? 4: value;
                FieldConfigurationValues.AddOrReplace( REPEAT_COLUMNS, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}
