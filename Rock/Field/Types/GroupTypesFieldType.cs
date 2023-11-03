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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select 0 or more GroupTypes 
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.GROUP_TYPES )]
    public class GroupTypesFieldType : SelectFromListFieldType, IEntityReferenceFieldType
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

                    if ( publicValuesList != null )
                    {
                        publicValues = publicValuesList
                            .Where( v => selectedValuesList.Contains( v.Value.ToLower() ) )
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
            GroupTypeService groupTypeService = new GroupTypeService( new RockContext() );
            return groupTypeService.Queryable().OrderBy( a => a.Name ).ToDictionary( k => k.Guid.ToString(), v => v.Name );
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
                .Select( guid => GroupTypeCache.GetId( guid ) )
                .Where( id => id.HasValue )
                .ToList();

            var groupTypeEntityTypeId = EntityTypeCache.GetId<GroupType>().Value;

            return ids
                .Select( id => new ReferencedEntity( groupTypeEntityTypeId, id.Value ) )
                .ToList();
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a GroupType and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<GroupType>().Value, nameof( GroupType.Name ) )
            };
        }

        #endregion
    }
}