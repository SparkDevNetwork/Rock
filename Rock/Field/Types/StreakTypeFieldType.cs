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
using System.Collections.Generic;
using System.Linq;
using Rock.Model;
using System;
using Rock.Web.Cache;
using Rock.Attribute;
using Rock.Data;
using Rock.ViewModels.Utility;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of streak types and allow a single selection.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.STREAK_TYPE )]
    public class StreakTypeFieldType : EntitySingleSelectionListFieldTypeBase<StreakType>, IEntityReferenceFieldType
    {
        private const string VALUES_PUBLIC_KEY = "values";

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            using ( var rockContext = new RockContext() )
            {
                publicConfigurationValues[VALUES_PUBLIC_KEY] = StreakTypeCache.All()
                    .Where( s => s.IsActive )
                    .OrderBy( o => o.Name )
                    .Select( o => new ListItemBag
                    {
                        Value = o.Guid.ToString(),
                        Text = o.Name
                    } )
                    .ToCamelCaseJson( false, true );
            }

            return publicConfigurationValues;
        }

        #endregion

        /// <summary>
        /// Returns a user-friendly description of the entity.
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        protected override string OnFormatValue( Guid entityGuid )
        {
            var entity = GetEntity( entityGuid.ToString() ) as StreakType;
            return entity?.Name ?? string.Empty;
        }

        /// <summary>
        /// Returns a dictionary of the items available for selection.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<Guid, string> OnGetItemList()
        {
            return StreakTypeCache.All()
                .Where( s => s.IsActive )
                .OrderBy( s => s.Name )
                .Select( s => new
                {
                    s.Guid,
                    s.Name,
                } )
                .ToDictionary( s => s.Guid, s => s.Name );
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

            var streakType = StreakTypeCache.Get( guid.Value );

            if ( streakType == null )
            {
                return null;
            }

            return new List<ReferencedEntity>
            {
                new ReferencedEntity( EntityTypeCache.GetId<StreakType>().Value, streakType.Id )
            };
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a StreakType and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<StreakType>().Value, nameof( StreakType.Name ) )
            };
        }

        #endregion
    }
}
