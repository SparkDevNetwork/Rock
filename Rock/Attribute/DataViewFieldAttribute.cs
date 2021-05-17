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
using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Data View Field Attribute.  Stored as DataView's Guid
    /// </summary>
    public class DataViewFieldAttribute : FieldAttribute
    {
        private const string ENTITY_TYPE_NAME_KEY = "entityTypeName";

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
        public DataViewFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string entityTypeName = "", string category = "", int order = 0, string key = null ) : 
            base( name, description, required, defaultValue, category, order, key, typeof( DataViewFieldType ).FullName )
        {
            if ( !string.IsNullOrWhiteSpace( entityTypeName ) )
            {
                FieldConfigurationValues.Add( ENTITY_TYPE_NAME_KEY, new Field.ConfigurationValue( entityTypeName ) );
            }
        }

        /// <summary>
        /// The EntityType to limit the DataView selection to <seealso cref="EntityType"/>
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
        /// The EntityType to limit the DataView selection to
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public Type EntityType
        {
            get
            {
                string entityTypeName = this.EntityTypeName;
                return entityTypeName.IsNotNullOrWhiteSpace() ? Type.GetType( entityTypeName ) : null;
            }

            set
            {
                this.EntityTypeName = value.FullName;
            }
        }
    }
}
