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
using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// A CategoryField attribute specifically for Group Categories for a specific GroupType.Guid
    /// Stored as Category.Guid or a comma-delimited list of Category.Guids
    /// </summary>
    public class GroupCategoryFieldAttribute : FieldAttribute
    {
        private const string ENTITY_TYPE_NAME_KEY = "entityTypeName";
        private const string QUALIFIER_COLUMN_KEY = "qualifierColumn";
        private const string QUALIFIER_VALUE_KEY = "qualifierValue";
        private const string ALLOW_MULTIPLE_KEY = "allowMultiple";

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCategoryFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <exception cref="System.Exception">
        /// groupTypeGuid must be specified
        /// or
        /// A valid groupTypeGuid must be specified
        /// </exception>
        public GroupCategoryFieldAttribute( string name, string description = "", bool allowMultiple = false, string groupTypeGuid = null,
             bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null ) :
            base( name, description, required, defaultValue, category, order, key,
                ( allowMultiple ? typeof( CategoriesFieldType ).FullName : typeof( CategoryFieldType ).FullName ) )
        {
            FieldConfigurationValues.Add( ENTITY_TYPE_NAME_KEY, new Field.ConfigurationValue( "Rock.Model.Group" ) );
            FieldConfigurationValues.Add( QUALIFIER_COLUMN_KEY, new Field.ConfigurationValue( "GroupTypeId" ) );

            GroupTypeGuid = groupTypeGuid;
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
        /// Gets or sets the group type unique identifier.
        /// </summary>
        /// <value>
        /// The group type unique identifier.
        /// </value>
        public string GroupTypeGuid
        {
            get
            {
                var groupTypeId = FieldConfigurationValues.GetValueOrNull( QUALIFIER_VALUE_KEY ).AsIntegerOrNull();

                if ( groupTypeId.HasValue )
                {
                    return GroupTypeCache.Get( groupTypeId.Value )?.Guid.ToString();
                }
                else
                {
                    return null;
                }
            }

            set
            {
                var groupTypeGuid = value.AsGuidOrNull();
                var groupTypeId = groupTypeGuid.HasValue && RockInstanceConfig.DatabaseIsAvailable ? GroupTypeCache.GetId( groupTypeGuid.Value ) : null;
                FieldConfigurationValues.AddOrReplace( QUALIFIER_VALUE_KEY, new Field.ConfigurationValue( groupTypeId?.ToString() ) );
            }
        }

    }
}
