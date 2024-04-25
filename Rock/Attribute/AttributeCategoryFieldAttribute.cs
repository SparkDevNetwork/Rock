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
using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public class AttributeCategoryFieldAttribute : FieldAttribute
    {
        private const string ENTITY_TYPE_NAME_KEY = "entityTypeName";
        private const string QUALIFIER_COLUMN_KEY = "qualifierColumn";
        private const string QUALIFIER_VALUE_KEY = "qualifierValue";
        private const string ALLOW_MULTIPLE_KEY = "allowMultiple";

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public AttributeCategoryFieldAttribute( string name, string description = "", bool allowMultiple = false,
            string entityTypeName = "", bool required = true, string defaultValue = "", string category = "",
            int order = 0, string key = null ) :
            base( name, description, required, defaultValue, category, order, key,
                ( allowMultiple ? typeof( CategoriesFieldType ).FullName : typeof( CategoryFieldType ).FullName ) )
        {
            EntityTypeName = entityTypeName;
        }

        /// <summary>
        /// Gets or sets a value indicating whether allow multiple is true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allow multiple; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiple
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new Field.ConfigurationValue( value.ToString() ) );

                FieldTypeClass = value ? typeof( CategoriesFieldType ).FullName : typeof( CategoryFieldType ).FullName;
            }
        }

        /// <summary>
        /// Gets or sets the name of the entity type <seealso cref="EntityType"/>
        /// </summary>
        public string EntityTypeName
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ENTITY_TYPE_NAME_KEY ) ?? string.Empty;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ENTITY_TYPE_NAME_KEY, new Field.ConfigurationValue( "Rock.Model.Attribute" ) );
                FieldConfigurationValues.AddOrReplace( QUALIFIER_COLUMN_KEY, new Field.ConfigurationValue( "EntityTypeId" ) );

                if ( RockInstanceConfig.DatabaseIsAvailable )
                {
                    var entityType = EntityTypeCache.Get( value, false );
                    if ( entityType != null )
                    {
                        FieldConfigurationValues.AddOrReplace( QUALIFIER_VALUE_KEY, new Field.ConfigurationValue( entityType.Id.ToString() ) );
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the entity for this Attribute Category
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
