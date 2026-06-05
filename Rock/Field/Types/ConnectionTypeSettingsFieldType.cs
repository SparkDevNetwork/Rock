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
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type that stores a Connection Type plus optional Opportunity, Status, and Type Source
    /// selections as a single attribute value.
    /// </summary>
    /// <remarks>
    /// Stored as a pipe-delimited string in fixed slot order:
    /// <c>ConnectionType.Guid|ConnectionOpportunity.Guid|ConnectionStatus.Guid|ConnectionTypeSource.Guid</c>.
    /// Any slot may be empty. The persisted Type anchors the editor and is used to scope
    /// the three children. Obsidian-only.
    /// </remarks>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CONNECTION_TYPE_SETTINGS )]
    public class ConnectionTypeSettingsFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Keys

        /// <summary>
        /// Keys used in the public configuration value dictionary sent to the editor.
        /// </summary>
        private static class ConfigKey
        {
            public const string ConnectionTypes = "connectionTypes";
            public const string ConnectionOpportunitiesByType = "connectionOpportunitiesByType";
            public const string ConnectionStatusesByType = "connectionStatusesByType";
            public const string ConnectionTypeSourcesByType = "connectionTypeSourcesByType";
        }

        #endregion Keys

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            ParseDelimitedGuids( privateValue, out var typeGuid, out var opportunityGuid, out _, out _ );

            if ( !typeGuid.HasValue && !opportunityGuid.HasValue )
            {
                return string.Empty;
            }

            var typeName = typeGuid.HasValue ? ConnectionTypeCache.Get( typeGuid.Value )?.Name : null;
            string opportunityName = null;

            if ( opportunityGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    opportunityName = new ConnectionOpportunityService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( o => o.Guid == opportunityGuid.Value )
                        .Select( o => o.Name )
                        .FirstOrDefault();
                }
            }

            if ( typeName != null && opportunityName != null )
            {
                return $"{typeName} > {opportunityName}";
            }

            return opportunityName ?? typeName ?? string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        #endregion Formatting

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            GetModelsFromAttributeValue( privateValue, out var type, out var opportunity, out var status, out var source );

            var edit = new ConnectionTypeSettingsEditValue
            {
                ConnectionType = type?.ToListItemBag(),
                ConnectionOpportunity = opportunity?.ToListItemBag(),
                ConnectionStatus = status?.ToListItemBag(),
                ConnectionTypeSource = source?.ToListItemBag()
            };

            return edit.ToCamelCaseJson( false, true );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var edit = publicValue.FromJsonOrNull<ConnectionTypeSettingsEditValue>();

            var typeGuid = edit?.ConnectionType?.Value.AsGuidOrNull();
            var opportunityGuid = edit?.ConnectionOpportunity?.Value.AsGuidOrNull();
            var statusGuid = edit?.ConnectionStatus?.Value.AsGuidOrNull();
            var sourceGuid = edit?.ConnectionTypeSource?.Value.AsGuidOrNull();

            if ( typeGuid == null && opportunityGuid == null && statusGuid == null && sourceGuid == null )
            {
                return null;
            }

            return $"{typeGuid}|{opportunityGuid}|{statusGuid}|{sourceGuid}";
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            if ( usage != ConfigurationValueUsage.Edit && usage != ConfigurationValueUsage.Configure )
            {
                return publicConfigurationValues;
            }

            using ( var rockContext = new RockContext() )
            {
                var types = ConnectionTypeCache.All()
                    .Where( t => t.IsActive )
                    .OrderBy( t => t.Name )
                    .Select( t => new ListItemBag { Value = t.Guid.ToString(), Text = t.Name } )
                    .ToList();

                var opportunitiesByType = new ConnectionOpportunityService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( o => o.IsActive && o.ConnectionType.IsActive )
                    .OrderBy( o => o.Name )
                    .Select( o => new
                    {
                        TypeGuid = o.ConnectionType.Guid,
                        Bag = new ListItemBag { Value = o.Guid.ToString(), Text = o.Name }
                    } )
                    .ToList()
                    .GroupBy( o => o.TypeGuid )
                    .ToDictionary( g => g.Key.ToString(), g => g.Select( o => o.Bag ).ToList() );

                var statusesByType = new ConnectionStatusService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( s => s.IsActive && s.ConnectionType.IsActive )
                    .OrderBy( s => s.Order ).ThenBy( s => s.Name )
                    .Select( s => new
                    {
                        TypeGuid = s.ConnectionType.Guid,
                        Bag = new ListItemBag { Value = s.Guid.ToString(), Text = s.Name }
                    } )
                    .ToList()
                    .GroupBy( s => s.TypeGuid )
                    .ToDictionary( g => g.Key.ToString(), g => g.Select( s => s.Bag ).ToList() );

                var sourcesByType = new ConnectionTypeSourceService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( s => s.ConnectionType.IsActive )
                    .OrderBy( s => s.Name )
                    .Select( s => new
                    {
                        TypeGuid = s.ConnectionType.Guid,
                        Bag = new ListItemBag { Value = s.Guid.ToString(), Text = s.Name }
                    } )
                    .ToList()
                    .GroupBy( s => s.TypeGuid )
                    .ToDictionary( g => g.Key.ToString(), g => g.Select( s => s.Bag ).ToList() );

                publicConfigurationValues[ConfigKey.ConnectionTypes] = types.ToCamelCaseJson( false, true );
                publicConfigurationValues[ConfigKey.ConnectionOpportunitiesByType] = opportunitiesByType.ToCamelCaseJson( false, true );
                publicConfigurationValues[ConfigKey.ConnectionStatusesByType] = statusesByType.ToCamelCaseJson( false, true );
                publicConfigurationValues[ConfigKey.ConnectionTypeSourcesByType] = sourcesByType.ToCamelCaseJson( false, true );
            }

            return publicConfigurationValues;
        }

        #endregion Edit Control

        #region Parse Helpers

        /// <summary>
        /// Splits the persisted pipe-delimited value into its four guid slots.
        /// </summary>
        /// <param name="value">The persisted attribute value.</param>
        /// <param name="connectionTypeGuid">The Connection Type guid, if present.</param>
        /// <param name="connectionOpportunityGuid">The Connection Opportunity guid, if present.</param>
        /// <param name="connectionStatusGuid">The Connection Status guid, if present.</param>
        /// <param name="connectionTypeSourceGuid">The Connection Type Source guid, if present.</param>
        public static void ParseDelimitedGuids( string value, out Guid? connectionTypeGuid, out Guid? connectionOpportunityGuid, out Guid? connectionStatusGuid, out Guid? connectionTypeSourceGuid )
        {
            var parts = ( value ?? string.Empty ).Split( '|' );
            connectionTypeGuid = parts.Length > 0 ? parts[0].AsGuidOrNull() : null;
            connectionOpportunityGuid = parts.Length > 1 ? parts[1].AsGuidOrNull() : null;
            connectionStatusGuid = parts.Length > 2 ? parts[2].AsGuidOrNull() : null;
            connectionTypeSourceGuid = parts.Length > 3 ? parts[3].AsGuidOrNull() : null;
        }

        private static void GetModelsFromAttributeValue( string value, out ConnectionType type, out ConnectionOpportunity opportunity, out ConnectionStatus status, out ConnectionTypeSource source )
        {
            type = null;
            opportunity = null;
            status = null;
            source = null;

            ParseDelimitedGuids( value, out var typeGuid, out var opportunityGuid, out var statusGuid, out var sourceGuid );

            if ( !typeGuid.HasValue && !opportunityGuid.HasValue && !statusGuid.HasValue && !sourceGuid.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                if ( typeGuid.HasValue )
                {
                    type = new ConnectionTypeService( rockContext ).Queryable().AsNoTracking().FirstOrDefault( t => t.Guid == typeGuid.Value );
                }

                if ( opportunityGuid.HasValue )
                {
                    opportunity = new ConnectionOpportunityService( rockContext ).Queryable().AsNoTracking().FirstOrDefault( o => o.Guid == opportunityGuid.Value );
                }

                if ( statusGuid.HasValue )
                {
                    status = new ConnectionStatusService( rockContext ).Queryable().AsNoTracking().FirstOrDefault( s => s.Guid == statusGuid.Value );
                }

                if ( sourceGuid.HasValue )
                {
                    source = new ConnectionTypeSourceService( rockContext ).Queryable().AsNoTracking().FirstOrDefault( s => s.Guid == sourceGuid.Value );
                }
            }
        }

        #endregion Parse Helpers

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            ParseDelimitedGuids( privateValue, out var typeGuid, out var opportunityGuid, out var statusGuid, out var sourceGuid );

            if ( !typeGuid.HasValue && !opportunityGuid.HasValue && !statusGuid.HasValue && !sourceGuid.HasValue )
            {
                return null;
            }

            var references = new List<ReferencedEntity>();

            using ( var rockContext = new RockContext() )
            {
                if ( typeGuid.HasValue )
                {
                    var id = new ConnectionTypeService( rockContext ).GetId( typeGuid.Value );
                    if ( id.HasValue )
                    {
                        references.Add( new ReferencedEntity( EntityTypeCache.GetId<ConnectionType>().Value, id.Value ) );
                    }
                }

                if ( opportunityGuid.HasValue )
                {
                    var id = new ConnectionOpportunityService( rockContext ).GetId( opportunityGuid.Value );
                    if ( id.HasValue )
                    {
                        references.Add( new ReferencedEntity( EntityTypeCache.GetId<ConnectionOpportunity>().Value, id.Value ) );
                    }
                }

                if ( statusGuid.HasValue )
                {
                    var id = new ConnectionStatusService( rockContext ).GetId( statusGuid.Value );
                    if ( id.HasValue )
                    {
                        references.Add( new ReferencedEntity( EntityTypeCache.GetId<ConnectionStatus>().Value, id.Value ) );
                    }
                }

                if ( sourceGuid.HasValue )
                {
                    var id = new ConnectionTypeSourceService( rockContext ).GetId( sourceGuid.Value );
                    if ( id.HasValue )
                    {
                        references.Add( new ReferencedEntity( EntityTypeCache.GetId<ConnectionTypeSource>().Value, id.Value ) );
                    }
                }
            }

            return references.Count > 0 ? references : null;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<ConnectionType>().Value, nameof( ConnectionType.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<ConnectionOpportunity>().Value, nameof( ConnectionOpportunity.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<ConnectionStatus>().Value, nameof( ConnectionStatus.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<ConnectionTypeSource>().Value, nameof( ConnectionTypeSource.Name ) )
            };
        }

        #endregion IEntityReferenceFieldType

        /// <summary>
        /// Wire shape of the public edit value: one nullable ListItemBag per persisted slot.
        /// </summary>
        private class ConnectionTypeSettingsEditValue
        {
            public ListItemBag ConnectionType { get; set; }
            public ListItemBag ConnectionOpportunity { get; set; }
            public ListItemBag ConnectionStatus { get; set; }
            public ListItemBag ConnectionTypeSource { get; set; }
        }
    }
}
