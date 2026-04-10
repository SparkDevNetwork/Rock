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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Tasks;
using Rock.UniversalSearch;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.UniversalSearchControlPanel;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Block to configure Rock's universal search features.
    /// </summary>
    [DisplayName( "Universal Search Control Panel" )]
    [Category( "Core" )]
    [Description( "Block to configure Rock's universal search features." )]
    [IconCssClass( "ti ti-zoom-in" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "1A99978C-B16F-4ED5-A51B-2D81C06C526E" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "E84ACB6E-36CB-40DB-8BEB-A2AA42644E87" )]
    [Rock.SystemGuid.BlockTypeGuid( "59F03418-0638-48E0-877D-B2F15B52C540" )]
    public class UniversalSearchControlPanel : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<UniversalSearchControlPanelBag, UniversalSearchControlPanelOptionsBag>();

            box.Bag = new UniversalSearchControlPanelBag
            {
                IndexStatus = GetIndexStatus(),
                SmartSearchSettings = GetSmartSearchSettings(),
                IndexableEntities = GetIndexableEntities()
            };

            box.Options = new UniversalSearchControlPanelOptionsBag
            {
                SearchTypeOptions = GetSearchTypeOptions(),
                IndexableEntityOptions = GetIndexableEntityOptions()
            };

            return box;
        }

        /// <summary>
        /// Gets the status of the active universal search index component.
        /// </summary>
        /// <returns>An <see cref="IndexStatusBag"/> containing the current status.</returns>
        private IndexStatusBag GetIndexStatus()
        {
            var status = new IndexStatusBag();

            foreach ( var indexType in IndexContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive )
                {
                    status.IsSearchEnabled = true;
                    status.ComponentName = component.EntityType.FriendlyName;
                    status.IsConnected = component.IsConnected;
                    status.IndexLocation = component.IndexLocation;

                    if ( !component.IsConnected )
                    {
                        var warningMessage = $"Could not connect to the {component.EntityType.FriendlyName} server at {component.IndexLocation}.";

                        // Add a friendly check to see if the URL provided is valid.
                        bool isValidUrl = Uri.TryCreate( component.IndexLocation, UriKind.Absolute, out var uriTest )
                            && ( uriTest.Scheme == Uri.UriSchemeHttp || uriTest.Scheme == Uri.UriSchemeHttps );

                        if ( !isValidUrl )
                        {
                            warningMessage += " Note that the URL provided is not valid. The pattern should be http(s)://server:port.";
                        }

                        status.WarningMessage = warningMessage;
                    }

                    break;
                }
            }

            if ( !status.IsSearchEnabled )
            {
                status.WarningMessage = "No universal search index components are currently enabled. You must enable a index component under <span class='navigation-tip'>Admin Tools &gt; System Settings &gt; Universal Search Index Components</span>.";
            }

            return status;
        }

        /// <summary>
        /// Gets the current smart search settings from system settings.
        /// </summary>
        /// <returns>A <see cref="SmartSearchSettingsBag"/> containing the current settings.</returns>
        private SmartSearchSettingsBag GetSmartSearchSettings()
        {
            var settings = new SmartSearchSettingsBag();

            var entitySetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchEntities" );

            if ( !string.IsNullOrWhiteSpace( entitySetting ) )
            {
                var entityIds = entitySetting.Split( ',' ).ToList();
                settings.SelectedEntityIds = entityIds;

                var entityIdInts = entityIds.Select( id => id.AsInteger() ).Where( id => id > 0 ).ToList();
                var entityNames = EntityTypeCache.All()
                    .Where( e => entityIdInts.Contains( e.Id ) )
                    .Select( e => e.FriendlyName )
                    .ToList();

                settings.SelectedEntityNames = string.Join( ", ", entityNames );
            }

            settings.FieldCriteria = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchFieldCriteria" );

            var searchTypeValue = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchSearchType" );
            var searchType = searchTypeValue.ConvertToEnumOrNull<SearchType>() ?? SearchType.Wildcard;
            settings.SearchType = ( ( int ) searchType ).ToString();
            settings.SearchTypeText = searchType.ToString();

            return settings;
        }

        /// <summary>
        /// Gets the list of indexable entity types for the grid.
        /// </summary>
        /// <returns>A list of <see cref="IndexableEntityBag"/> for display in the grid.</returns>
        private List<IndexableEntityBag> GetIndexableEntities()
        {
            var entities = new EntityTypeService( RockContext ).Queryable()
                .AsNoTracking()
                .ToList();

            return entities
                .Where( e => e.IsIndexingSupported )
                .Select( e => new IndexableEntityBag
                {
                    IdKey = e.IdKey,
                    Name = e.FriendlyName,
                    IsIndexingEnabled = e.IsIndexingEnabled,
                    AllowsInteractiveBulkIndexing = GetAllowsInteractiveBulkIndexing( e )
                } )
                .ToList();
        }

        /// <summary>
        /// Determines whether the entity type allows interactive bulk indexing
        /// by instantiating the entity and checking the <see cref="IRockIndexable.AllowsInteractiveBulkIndexing"/> property.
        /// </summary>
        /// <param name="entityType">The entity type to check.</param>
        /// <returns><c>true</c> if the entity allows interactive bulk indexing; otherwise, <c>false</c>.</returns>
        private bool GetAllowsInteractiveBulkIndexing( EntityType entityType )
        {
            if ( string.IsNullOrWhiteSpace( entityType.AssemblyName ) )
            {
                return false;
            }

            try
            {
                var modelType = Type.GetType( entityType.AssemblyName );
                if ( modelType == null )
                {
                    return false;
                }

                var modelInstance = Activator.CreateInstance( modelType ) as IRockIndexable;
                return modelInstance?.AllowsInteractiveBulkIndexing ?? false;
            }
            catch
            {
                // Intentionally ignored: reflection failures for individual entity types
                // should not prevent the rest of the grid from loading.
                return false;
            }
        }

        /// <summary>
        /// Gets the search type options as a list of <see cref="ListItemBag"/> for the dropdown.
        /// </summary>
        /// <returns>A list of search type options.</returns>
        private List<ListItemBag> GetSearchTypeOptions()
        {
            return Enum.GetValues( typeof( SearchType ) )
                .Cast<SearchType>()
                .Select( s => new ListItemBag
                {
                    Value = ( ( int ) s ).ToString(),
                    Text = s.ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the indexable entity options for the smart search entity checkbox list.
        /// </summary>
        /// <returns>A list of entity type options.</returns>
        private List<ListItemBag> GetIndexableEntityOptions()
        {
            return EntityTypeCache.All()
                .Where( e => e.IsIndexingSupported )
                .Select( e => new ListItemBag
                {
                    Value = e.Id.ToString(),
                    Text = e.FriendlyName
                } )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the smart search settings to system settings.
        /// </summary>
        /// <param name="bag">The settings to save.</param>
        /// <returns>The updated smart search settings for the view mode.</returns>
        [BlockAction]
        public BlockActionResult SaveSmartSearchSettings( SaveSmartSearchSettingsRequestBag bag )
        {
            Rock.Web.SystemSettings.SetValue( "core_SmartSearchUniversalSearchEntities", string.Join( ",", bag.EntityIds ?? new List<string>() ) );
            Rock.Web.SystemSettings.SetValue( "core_SmartSearchUniversalSearchFieldCriteria", bag.FieldCriteria ?? string.Empty );
            Rock.Web.SystemSettings.SetValue( "core_SmartSearchUniversalSearchSearchType", bag.SearchType ?? string.Empty );

            return ActionOk( GetSmartSearchSettings() );
        }

        /// <summary>
        /// Enables or disables indexing for the specified entity type.
        /// When enabled, creates the index and queues a bulk index operation.
        /// When disabled, deletes the index.
        /// </summary>
        /// <param name="key">The IdKey of the entity type.</param>
        /// <param name="isEnabled">Whether to enable or disable indexing.</param>
        /// <returns>The updated list of indexable entities.</returns>
        [BlockAction]
        public BlockActionResult EnableEntityIndexing( string key, bool isEnabled )
        {
            var entityTypeService = new EntityTypeService( RockContext );
            var entityType = entityTypeService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entityType == null )
            {
                return ActionBadRequest( "Entity type not found." );
            }

            var indexModelType = entityType.IndexModelType;
            if ( indexModelType == null )
            {
                return ActionBadRequest( "Entity type does not have a valid index model." );
            }

            entityType.IsIndexingEnabled = isEnabled;
            RockContext.SaveChanges();

            if ( isEnabled )
            {
                IndexContainer.CreateIndex( indexModelType );

                var processEntityTypeBulkIndexMsg = new ProcessEntityTypeBulkIndex.Message
                {
                    EntityTypeId = entityType.Id
                };

                processEntityTypeBulkIndexMsg.Send();
            }
            else
            {
                IndexContainer.DeleteIndex( indexModelType );
            }

            return ActionOk( GetIndexableEntities() );
        }

        /// <summary>
        /// Queues a bulk index operation for the specified entity type.
        /// </summary>
        /// <param name="key">The IdKey of the entity type.</param>
        /// <returns>A success message or error.</returns>
        [BlockAction]
        public BlockActionResult BulkLoadDocuments( string key )
        {
            var entityTypeCache = GetEntityTypeCacheFromKey( key );

            if ( entityTypeCache == null )
            {
                return ActionBadRequest( "An error occurred launching the bulk index request. Could not find the entity type." );
            }

            var processEntityTypeBulkIndexMsg = new ProcessEntityTypeBulkIndex.Message
            {
                EntityTypeId = entityTypeCache.Id
            };

            processEntityTypeBulkIndexMsg.Send();

            return ActionOk( $"A request has been sent to index {entityTypeCache.FriendlyName.Pluralize()}." );
        }

        /// <summary>
        /// Recreates the index for the specified entity type by deleting and recreating all related indexes.
        /// </summary>
        /// <param name="key">The IdKey of the entity type.</param>
        /// <returns>A success message or error.</returns>
        [BlockAction]
        public BlockActionResult RecreateIndex( string key )
        {
            var entityTypeCache = GetEntityTypeCacheFromKey( key );

            if ( entityTypeCache == null )
            {
                return ActionBadRequest( "Could not find the entity type." );
            }

            var indexesToRecreate = IndexHelper.GetRelatedIndexes( entityTypeCache.IndexModelType );
            foreach ( var indexType in indexesToRecreate )
            {
                IndexContainer.DeleteIndex( indexType );
                IndexContainer.CreateIndex( indexType );
            }

            return ActionOk( $"The index for {entityTypeCache.FriendlyName} has been re-created." );
        }

        /// <summary>
        /// Resolves an entity type cache from the provided IdKey.
        /// </summary>
        /// <param name="key">The IdKey of the entity type.</param>
        /// <returns>The <see cref="EntityTypeCache"/> or null.</returns>
        private EntityTypeCache GetEntityTypeCacheFromKey( string key )
        {
            var entityType = new EntityTypeService( RockContext ).GetNoTracking( key, !PageCache.Layout.Site.DisablePredictableIds );
            if ( entityType == null )
            {
                return null;
            }

            return EntityTypeCache.Get( entityType.Id );
        }

        #endregion Block Actions
    }
}
