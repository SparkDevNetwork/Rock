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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of step programs and allow a single selection.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( "33875369-7D2B-4CD7-BB89-ABC29906CCAE" )]
    public class StepProgramFieldType : EntitySingleSelectionListFieldTypeBase<Rock.Model.StepProgram>, IEntityReferenceFieldType
    {
        private const string VALUES_PUBLIC_KEY = "values";

        /// <summary>
        /// Returns a user-friendly description of the entity.
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        protected override string OnFormatValue( Guid entityGuid )
        {
            var entity = GetEntity( entityGuid.ToString() );

            return entity?.Name ?? string.Empty;
        }

        /// <summary>
        /// Returns a dictionary of the items available for selection.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<Guid, string> OnGetItemList()
        {
            var service = new StepProgramService( new RockContext() );

            var items = service
                .Queryable()
                .AsNoTracking()
                .OrderBy( o => o.Name )
                .Select( o => new
                {
                    o.Guid,
                    o.Name,
                } )
                .ToDictionary( o => o.Guid, o => o.Name );

            return items;
        }
        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // The Guids in the ListItemBag list in the publicConfigurationValues happen to be in the lowercase
            // Sending the publicValue too in the lower case ensure that it's casing is consistent with the publicConfigurationValues.
            // This will enable the remote devices to not worry about the casing while comparing the strings across the publicValue and the publicConfigurationValues.
            return privateValue.ToLower();
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );
            publicConfigurationValues[VALUES_PUBLIC_KEY] = StepProgramCache.All()
                .OrderBy( o => o.Name )
                .ToListItemBagList()
                .ToCamelCaseJson( false, true );
            return publicConfigurationValues;
        }

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
                var stepProgram = new StepProgramService( rockContext ).GetId( guid.Value );

                if ( !stepProgram.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<StepProgram>().Value, stepProgram.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a StepProgram and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<StepProgram>().Value, nameof( StepProgram.Name ) )
            };
        }

        #endregion
    }
}
