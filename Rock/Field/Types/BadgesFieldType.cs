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

namespace Rock.Field.Types
{
    /// <summary>
    /// Select multiple Badges from a checkbox list. Stored as a comma-delimited list of Badge Guids
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( "602F273B-7EC2-42E6-9AA7-A36A268192A3")]
    public class BadgesFieldType : SelectFromListFieldType, IEntityReferenceFieldType
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
            List<BadgeCache> badges = null;
            int? entityTypeId = null;

            if ( configurationValues != null && configurationValues.TryGetValue( BadgesFieldAttribute.ENTITY_TYPE_KEY, out var value ) )
            {
                entityTypeId = ( value.Value as string ).AsIntegerOrNull();
            }

            if ( entityTypeId.HasValue )
            {
                badges = BadgeCache.All( entityTypeId.Value );
            }
            else
            {
                badges = BadgeCache.All();
            }

            var orderedBadges = badges.OrderBy( x => x.Order )
                .ToDictionary( k => k.Guid.ToString(), v => v.Name );

            return orderedBadges;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var valueGuidList = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();

            var ids = valueGuidList
                .Select( guid => BadgeCache.GetId( guid ) )
                .Where( id => id.HasValue )
                .ToList();

            var badgeEntityTypeId = EntityTypeCache.GetId<Rock.Model.Badge>().Value;

            return ids
                .Select( id => new ReferencedEntity( badgeEntityTypeId, id.Value ) )
                .ToList();
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Badge and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Rock.Model.Badge>().Value, nameof( Rock.Model.Badge.Name ) )
            };
        }

        #endregion
    }
}
