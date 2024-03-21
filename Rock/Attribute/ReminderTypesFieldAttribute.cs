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

using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or more Reminder Types. Stored as a comma-delimited list of Reminder Type Guids
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class ReminderTypesFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// The entity type key
        /// </summary>
        public static string ENTITY_TYPE_KEY = "entitytype";

        /// <summary>
        /// The enhanced selection key
        /// </summary>
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReminderTypesFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultReminderTypeGuids">The default reminder type guids.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        public ReminderTypesFieldAttribute( string name = "Reminder Types", string description = "", bool required = true, string defaultReminderTypeGuids = "", string category = "",
            int order = 0, string key = null, string entityTypeGuid = "" )
            : base( name, description, required, defaultReminderTypeGuids, category, order, key, typeof( Rock.Field.Types.ReminderTypesFieldType ).FullName )
        {
            EntityTypeGuid = entityTypeGuid;
        }

        /// <summary>
        /// Gets or sets the entity type unique identifier.
        /// </summary>
        /// <value>
        /// The defined type unique identifier.
        /// </value>
        public string EntityTypeGuid
        {
            get
            {
                var entityTypeId = FieldConfigurationValues.GetValueOrNull( ENTITY_TYPE_KEY ).AsIntegerOrNull();

                if ( entityTypeId.HasValue )
                {
                    return EntityTypeCache.Get( entityTypeId.Value )?.Guid.ToString();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                var entityTypeGuid = value.AsGuidOrNull();
                var entityTypeId = entityTypeGuid.HasValue && RockInstanceConfig.DatabaseIsAvailable ? EntityTypeCache.GetId( entityTypeGuid.Value ) : null;
                FieldConfigurationValues.AddOrReplace( ENTITY_TYPE_KEY, new Field.ConfigurationValue( entityTypeId?.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether enhanced selection mode is used.
        /// </summary>
        /// <value><c>true</c> if enhanced selection mode is used; otherwise, <c>false</c>.</value>
        public bool EnhancedSelection
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ENHANCED_SELECTION_KEY ).AsBoolean();
            }
            set
            {
                FieldConfigurationValues.AddOrReplace( ENHANCED_SELECTION_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}