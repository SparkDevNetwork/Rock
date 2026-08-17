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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.CacheManager;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Block used to view cache statistics and clear the existing cache.
    /// </summary>

    [DisplayName( "Cache Manager" )]
    [Category( "CMS" )]
    [Description( "Block used to view cache statistics and clear the existing cache." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "EDE17016-ABCC-41E1-A686-D9D466CC203A" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "E481208F-EFAA-4635-8C5E-9CD094A67A59" )]
    [Rock.SystemGuid.BlockTypeGuid( "48AD1B85-C51C-4C51-A902-E2DC4586B903" )]
    public class CacheManager : RockBlockType
    {
        #region Initialization

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CacheManagerOptionsBag>();
            var builder = GetGridBuilder();

            box.GridDefinition = builder.BuildDefinition();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <summary>
        /// Gets the options bag for the block initialization.
        /// </summary>
        /// <returns>A <see cref="CacheManagerOptionsBag"/> containing the block options.</returns>
        private CacheManagerOptionsBag GetBoxOptions()
        {
            return new CacheManagerOptionsBag
            {
                CacheTypes = GetCacheTypeItems(),
                IsStatisticsEnabled = Rock.Web.SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.CACHE_MANAGER_ENABLE_STATISTICS ).AsBoolean(),
                IsEditAuthorized = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
            };
        }

        #endregion

        #region Grid

        /// <summary>
        /// Gets the grid builder for the cache tags grid.
        /// </summary>
        /// <returns>The grid builder for the cache tags grid.</returns>
        private GridBuilder<CacheTagRow> GetGridBuilder()
        {
            return new GridBuilder<CacheTagRow>()
                .WithBlock( this )
                .AddTextField( "idKey", r => r.IdKey )
                .AddTextField( "tagName", r => r.TagName )
                .AddTextField( "tagDescription", r => r.TagDescription )
                .AddField( "linkedKeys", r => r.LinkedKeys );
        }

        /// <summary>
        /// Gets the cache tag rows for the grid.
        /// </summary>
        /// <returns>A list of <see cref="CacheTagRow"/> representing all cache tags.</returns>
        private List<CacheTagRow> GetCacheTagRows()
        {
            var cacheTagDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
            var definedValueService = new DefinedValueService( RockContext );
            var cacheTags = definedValueService.Queryable()
                .AsNoTracking()
                .Where( v => v.DefinedTypeId == cacheTagDefinedTypeId )
                .ToList();

            return cacheTags.Select( tag => new CacheTagRow
            {
                IdKey = tag.IdKey,
                TagName = tag.Value,
                TagDescription = tag.Description,
                LinkedKeys = RockCache.GetCountOfCachedItemsForTag( tag.Value )
            } ).ToList();
        }

        #endregion

        #region Cache Statistics

        /// <summary>
        /// Gets the list of cache types as ListItemBag items for the dropdown.
        /// </summary>
        /// <returns>A list of <see cref="ListItemBag"/> representing cache types.</returns>
        private List<ListItemBag> GetCacheTypeItems()
        {
            var items = new List<ListItemBag>
            {
                new ListItemBag { Value = "all", Text = "All Cached Items" }
            };

            var cacheStats = RockCache.GetAllStatistics();
            foreach ( var cacheItemStat in cacheStats.OrderBy( s => s.Name ) )
            {
                items.Add( new ListItemBag { Value = cacheItemStat.FullName, Text = cacheItemStat.Name } );
            }

            return items;
        }

        /// <summary>
        /// Computes aggregated cache statistics for the specified cache type.
        /// </summary>
        /// <param name="cacheType">The cache type full name, or "all" for all types.</param>
        /// <returns>A <see cref="CacheStatisticsBag"/> with aggregated statistics.</returns>
        private CacheStatisticsBag ComputeCacheStatistics( string cacheType )
        {
            var cacheItemStatistics = new List<CacheItemStatistics>();

            if ( cacheType == "all" )
            {
                cacheItemStatistics = RockCache.GetAllStatistics().OrderBy( s => s.Name ).ToList();
            }
            else
            {
                cacheItemStatistics.Add( RockCache.GetStatisticsForType( cacheType ) );
            }

            long hits = 0;
            long misses = 0;
            long adds = 0;
            long gets = 0;
            long clears = 0;

            foreach ( var cacheItemStat in cacheItemStatistics )
            {
                foreach ( var cacheHandleStat in cacheItemStat.HandleStats )
                {
                    hits += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "hits" ).Select( s => s.Count ).FirstOrDefault();
                    misses += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "misses" ).Select( s => s.Count ).FirstOrDefault();
                    adds += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "add calls" ).Select( s => s.Count ).FirstOrDefault();
                    adds += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "put calls" ).Select( s => s.Count ).FirstOrDefault();
                    gets += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "get calls" ).Select( s => s.Count ).FirstOrDefault();
                    clears += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "clear calls" ).Select( s => s.Count ).FirstOrDefault();
                    clears += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "clear region calls" ).Select( s => s.Count ).FirstOrDefault();
                }
            }

            return new CacheStatisticsBag
            {
                Hits = hits,
                Misses = misses,
                Adds = adds,
                Gets = gets,
                Clears = clears
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the grid data for the cache tags grid.
        /// </summary>
        /// <returns>A <see cref="BlockActionResult"/> containing the grid data.</returns>
        [BlockAction]
        public BlockActionResult GetGridData()
        {
            var rows = GetCacheTagRows();
            var gridDataBag = GetGridBuilder().Build( rows );

            return ActionOk( gridDataBag );
        }

        /// <summary>
        /// Saves a new cache tag or updates an existing one.
        /// </summary>
        /// <param name="idKey">The IdKey of the existing tag to update, or null to create a new tag.</param>
        /// <param name="tagName">The name of the tag (used only for new tags).</param>
        /// <param name="description">The description of the tag.</param>
        /// <returns>A <see cref="BlockActionResult"/> indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult SaveCacheTag( string idKey, string tagName, string description )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to edit cache tags." );
            }

            var cacheTagDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
            var definedValueService = new DefinedValueService( RockContext );

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // Update existing tag — only the description is editable.
                var existingTag = definedValueService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( existingTag == null )
                {
                    return ActionNotFound( "Cache tag not found." );
                }

                existingTag.Description = description.Trim();
                RockContext.SaveChanges();

                return ActionOk();
            }
            else
            {
                // Create a new tag.
                var normalizedName = tagName.Trim().ToLower().Replace( " ", "-" );

                if ( normalizedName.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( "Tag name is required." );
                }

                // Check for duplicate tag names.
                var isDuplicate = definedValueService.Queryable()
                    .AsNoTracking()
                    .Any( v => v.DefinedTypeId == cacheTagDefinedTypeId && v.Value == normalizedName );

                if ( isDuplicate )
                {
                    return ActionBadRequest( $"Tag name \"{normalizedName}\" is already in use." );
                }

                int order = 0;

                var existingTags = definedValueService.Queryable()
                    .AsNoTracking()
                    .Where( v => v.DefinedTypeId == cacheTagDefinedTypeId );

                if ( existingTags.Any() )
                {
                    order = existingTags.Max( v => v.Order ) + 1;
                }

                var definedValue = new DefinedValue
                {
                    DefinedTypeId = cacheTagDefinedTypeId,
                    Value = normalizedName,
                    Description = description.Trim(),
                    Order = order
                };

                definedValueService.Add( definedValue );
                RockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Clears all cached items associated with the specified cache tag.
        /// </summary>
        /// <param name="idKey">The IdKey of the cache tag DefinedValue.</param>
        /// <returns>A <see cref="BlockActionResult"/> with a success message.</returns>
        [BlockAction]
        public BlockActionResult ClearCacheForTag( string idKey )
        {
            var definedValue = DefinedValueCache.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( definedValue == null )
            {
                return ActionNotFound( "Cache tag not found." );
            }

            RockCache.RemoveForTags( definedValue.Value );

            return ActionOk( $"Removed cached items tagged with \"{definedValue.Value}\"." );
        }

        /// <summary>
        /// Gets cache statistics for the specified cache type.
        /// </summary>
        /// <param name="cacheType">The cache type full name, or "all" for all types.</param>
        /// <returns>A <see cref="BlockActionResult"/> containing the <see cref="CacheStatisticsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetCacheStatistics( string cacheType )
        {
            var stats = ComputeCacheStatistics( cacheType ?? "all" );
            return ActionOk( stats );
        }

        /// <summary>
        /// Clears cached items for the specified cache type or all types.
        /// </summary>
        /// <param name="cacheType">The cache type full name, or "all" to clear all cached items.</param>
        /// <returns>A <see cref="BlockActionResult"/> with a success message.</returns>
        [BlockAction]
        public BlockActionResult ClearCache( string cacheType )
        {
            if ( cacheType == "all" )
            {
                RockCache.ClearAllCachedItems();
            }
            else
            {
                RockCache.ClearCachedItemsForType( cacheType );
            }

            return ActionOk( "All cached items have been cleared." );
        }

        /// <summary>
        /// Enables or disables cache statistics tracking by updating the web.config setting.
        /// Note: This will cause the Rock application to restart.
        /// </summary>
        /// <param name="enabled">If set to <c>true</c>, enables statistics; otherwise, disables them.</param>
        /// <returns>A <see cref="BlockActionResult"/> indicating success.</returns>
        [BlockAction]
        public BlockActionResult SetEnableStatistics( bool enabled )
        {
            Rock.Web.SystemSettings.SetValueToWebConfig( Rock.SystemKey.SystemSetting.CACHE_MANAGER_ENABLE_STATISTICS, enabled.ToString() );
            return ActionOk();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// POCO for cache tag grid rows.
        /// </summary>
        private class CacheTagRow
        {
            /// <summary>
            /// Gets or sets the IdKey for the DefinedValue representing this tag.
            /// </summary>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the name of the tag.
            /// </summary>
            public string TagName { get; set; }

            /// <summary>
            /// Gets or sets the description of the tag.
            /// </summary>
            public string TagDescription { get; set; }

            /// <summary>
            /// Gets or sets the number of cached items linked to this tag.
            /// </summary>
            public long LinkedKeys { get; set; }
        }

        #endregion
    }
}
