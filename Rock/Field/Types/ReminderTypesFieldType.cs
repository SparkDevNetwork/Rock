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
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.Field.Types
{
    /// <summary>
    /// Select multiple Badges from a checkbox list. Stored as a comma-delimited list of Badge Guids
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.REMINDER_TYPES )]
    public class ReminderTypesFieldType : SelectFromListFieldType
    {
        #region Configuration

        private const string VALUES_PUBLIC_KEY = "values";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";
        private const string REPEAT_COLUMNS_KEY = "repeatColumns";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );
            var publicValues = publicConfigurationValues.GetValueOrNull( VALUES_PUBLIC_KEY );

            if ( usage == ConfigurationValueUsage.View )
            {
                if ( publicValues != null )
                {
                    var selectedValuesList = privateValue.ToLower().Split( ',' );
                    var publicValuesList = publicValues.FromJsonOrNull<List<ListItemBag>>();

                    if (publicValuesList != null)
                    {
                        publicValues = publicValuesList
                            .Where( v => selectedValuesList.Contains(v.Value.ToLower()) )
                            .ToCamelCaseJson( false, true );

                        publicConfigurationValues[VALUES_PUBLIC_KEY] = publicValues;
                    }
                }

                publicConfigurationValues.Remove( ENHANCED_SELECTION_KEY );
                publicConfigurationValues.Remove( REPEAT_COLUMNS_KEY );
            }

            return publicConfigurationValues;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues )
        {
            var reminderTypesQuery = new ReminderTypeService( new Data.RockContext() ).Queryable();

            int? entityTypeId = null;
            if ( configurationValues != null && configurationValues.TryGetValue( ReminderTypesFieldAttribute.ENTITY_TYPE_KEY, out var entityTypeIdValue ) )
            {
                entityTypeId = ( entityTypeIdValue.Value as string ).AsIntegerOrNull();
            }

            if ( entityTypeId.HasValue )
            {
                reminderTypesQuery = reminderTypesQuery.Where( t => t.EntityTypeId == entityTypeId );
            }

            var orderedReminderTypes = reminderTypesQuery
                .OrderBy( t => t.EntityType.FriendlyName )
                .ThenBy( t => t.Name )
                .ToDictionary( t => t.Guid.ToString(), t => t.EntityType.FriendlyName + " - " + t.Name );

            return orderedReminderTypes;
        }

        #endregion Methods
    }
}
