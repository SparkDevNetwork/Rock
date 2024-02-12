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
using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Data View Field Attribute.  Stored as DataViews Guid's
    /// </summary>
    public class DataViewsFieldAttribute : FieldAttribute
    {
        private const string ENTITY_TYPE_NAME_KEY = "entityTypeName";
        private const string DISPLAY_PERSISTED_ONLY_KEY = "displayPersistedOnly";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataViewFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public DataViewsFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string entityTypeName = "", string category = "", int order = 0, string key = null ) :
            base( name, description, required, defaultValue, category, order, key, typeof( DataViewsFieldType ).FullName )
        {
            EntityTypeName = entityTypeName;
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        public string EntityTypeName
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ENTITY_TYPE_NAME_KEY ) ?? string.Empty;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ENTITY_TYPE_NAME_KEY, new Field.ConfigurationValue( value ) );
            }
        }

        /// <summary>
        /// Gets or sets whether to show only <see cref="Rock.Model.DataView"/>s which are persisted.
        /// </summary>
        public bool DisplayPersistedOnly
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( DISPLAY_PERSISTED_ONLY_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( DISPLAY_PERSISTED_ONLY_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}
