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
#if WEBFORMS
using System.Web.UI;
#endif
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Step
    /// as Step.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( "829803DB-7CA3-44F6-B1CB-669D61ED6E92" )]
    public class StepFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = privateValue;

            Step step = null;

            using ( var rockContext = new RockContext() )
            {
                Guid? guid = privateValue.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    step = new StepService( rockContext ).GetNoTracking( guid.Value );
                }

                if ( step != null )
                {
                    formattedValue = step.ToString();
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
                return new StepService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var ids = new StepService( rockContext )
                    .Queryable()
                    .Where( s => s.Guid == guid.Value )
                    .Select( s => new
                    {
                        s.Id,
                        s.PersonAliasId,
                        s.PersonAlias.PersonId,
                        s.StepTypeId
                    } )
                    .FirstOrDefault();

                if ( ids == null )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<Step>().Value, ids.Id ),
                    new ReferencedEntity( EntityTypeCache.GetId<PersonAlias>().Value, ids.PersonAliasId ),
                    new ReferencedEntity( EntityTypeCache.GetId<Person>().Value, ids.PersonId ),
                    new ReferencedEntity( EntityTypeCache.GetId<StepType>().Value, ids.StepTypeId )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Step>().Value, nameof( Step.Id ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Step>().Value, nameof( Step.Order ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Step>().Value, nameof( Step.Id ) ),
                new ReferencedProperty( EntityTypeCache.GetId<PersonAlias>().Value, nameof( PersonAlias.PersonId ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.NickName ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.LastName ) ),
                new ReferencedProperty( EntityTypeCache.GetId<StepType>().Value, nameof( StepType.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<StepType>().Value, nameof( StepType.AllowMultiple ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

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
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            using ( var rockContext = new RockContext() )
            {
                return new StepService( rockContext ).GetId( guid );
            }
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            using ( var rockContext = new RockContext() )
            {
                var itemGuid = new StepService( rockContext ).GetGuid( id ?? 0 );
                string guidValue = itemGuid.HasValue ? itemGuid.ToString() : string.Empty;
                SetEditValue( control, configurationValues, guidValue );
            }
        }

#endif
        #endregion
    }
}