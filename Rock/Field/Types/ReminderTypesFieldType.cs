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

namespace Rock.Field.Types
{
    /// <summary>
    /// Select multiple Badges from a checkbox list. Stored as a comma-delimited list of Badge Guids
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( "C66E6BF9-4A73-4429-ACAD-D94D5E3A89B7" )]
    public class ReminderTypesFieldType : SelectFromListFieldType
    {
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
