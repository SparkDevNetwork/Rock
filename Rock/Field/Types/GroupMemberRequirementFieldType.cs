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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) group member requiement.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( "C0797A18-B489-46C7-8C30-F5E4F8246E23" )]
    public class GroupMemberRequirementFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = privateValue;

            GroupMemberRequirement groupMemberRequirement = null;

            using ( var rockContext = new RockContext() )
            {
                Guid? guid = privateValue.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    groupMemberRequirement = new GroupMemberRequirementService( rockContext ).GetNoTracking( guid.Value );
                }

                if ( groupMemberRequirement != null )
                {
                    formattedValue = groupMemberRequirement.ToString();
                }
            }

            return formattedValue;
        }

        #endregion

        #region Edit Control

        // simple text box implemented by base FieldType

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            using ( var rockContext = new RockContext() )
            {
                return new GroupMemberRequirementService( rockContext ).GetId( guid );
            }
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            using ( var rockContext = new RockContext() )
            {
                var itemGuid = new GroupMemberRequirementService( rockContext ).GetGuid( id ?? 0 );
                string guidValue = itemGuid.HasValue ? itemGuid.ToString() : string.Empty;
                SetEditValue( control, configurationValues, guidValue );
            }
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                return new GroupMemberRequirementService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var groupId = new GroupMemberRequirementService( rockContext ).GetId( guid.Value );

                if ( !groupId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<GroupMemberRequirement>().Value, groupId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Group Member Requirement and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.NickName ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.LastName ) ),
                new ReferencedProperty( EntityTypeCache.GetId<GroupRequirementType>().Value, nameof( GroupRequirementType.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Group>().Value, nameof( Group.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<GroupType>().Value, nameof( GroupType.Name ) )
            };
        }

        #endregion
    }
}