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
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or more Attributes for the given EntityType Guid.  Stored as Attribute.Guid.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class AttributeFieldAttribute : FieldAttribute
    {
        private const string ENTITY_TYPE_KEY = "entitytype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFieldAttribute" /> class.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public AttributeFieldAttribute( string entityTypeGuid, string name = "", string description = "", bool required = true, bool allowMultiple = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.AttributeFieldType ).FullName )
        {
            var entityTypeConfigValue = new Field.ConfigurationValue( entityTypeGuid );
            FieldConfigurationValues.Add( ENTITY_TYPE_KEY, entityTypeConfigValue );

            var allowMultipleConfigValue = new Field.ConfigurationValue( allowMultiple.ToString() );
            FieldConfigurationValues.Add( ALLOW_MULTIPLE_KEY, allowMultipleConfigValue );

            if ( string.IsNullOrWhiteSpace( Name ) )
            {
                var entityType = Rock.Web.Cache.EntityTypeCache.Read( new Guid( entityTypeGuid ) );
                name = ( entityType != null ? entityType.Name : "Entity" ) + " Attribute";
            }

            if ( string.IsNullOrWhiteSpace( Key ) )
            {
                Key = Name.Replace( " ", string.Empty );
            }
        }
    }
}