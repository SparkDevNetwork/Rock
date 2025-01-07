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
// </copyright//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Lava.Shortcodes;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Cms.LavaShortcodeDetail;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of Lava shortcodes.
    /// </summary>
    [DisplayName( "Lava Shortcode List" )]
    [Category( "CMS" )]
    [Description( "Lists Lava Shortcode in the system." )]
    [IconCssClass( "fa fa-list" )]
    [LinkedPage( "Detail Page", Key = AttributeKey.DetailPage )]
    // [SupportedSiteTypes(Model.SiteType.Web)]

    [Rock.SystemGuid.EntityTypeGuid( "B02078CC-FA42-4249-ABE0-7E166C63D2B6" )]
    [Rock.SystemGuid.BlockTypeGuid( "09FD3746-48D1-4B94-AAA9-6896443AA43E" )]
    public class LavaShortcodeList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <summary>
        /// Gets the categories from meta data. If the metaData does not include any
        /// categories, that's still ok. In that case this will just return an empty
        /// list of categories.
        /// </summary>
        /// <param name="metaData">The meta data.</param>
        /// <returns>List&lt;Category&gt;.</returns>
        private List<Category> GetCategoriesFromMetaData( LavaShortcodeMetadataAttribute metaData )
        {
            var shortcodeCategoryGuids = metaData?.Categories?.Split( ',' ).AsGuidList();

            return shortcodeCategoryGuids.Any() ?
                new CategoryService( RockContext ).GetByGuids( shortcodeCategoryGuids ).ToList() :
                new List<Category>();
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new LavaShortcodeListBox();
            box.UserCanEdit = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.DetailPage = this.GetLinkedPageUrl( AttributeKey.DetailPage, "LavaShortcodeId", "((Key))" );

            var shortcodes = GetLavaShortcodes();

            box.LavaShortcodes = shortcodes.Select( s => new LavaShortcodeBag
            {
                IdKey = s.IdKey,
                Categories = s.Categories.ToListItemBagList(),
                Description = s.Description,
                Documentation = s.Documentation,
                IsActive = s.IsActive,
                IsSystem = s.IsSystem,
                Markup = s.Markup,
                Name = s.Name,
                TagName = s.TagName,
                TagType = s.TagType.ToString(),
            } ).ToList();

            box.NavigationUrls = GetBoxNavigationUrls();

            // Get the distinct categories for all Lava Shortcodes
            // and order them by name.
            box.Categories = shortcodes
                .SelectMany( l => l.Categories )
                .DistinctBy( c => c.Id )
                .OrderBy( c => c.Name )
                .ToListItemBagList();

            return box;
        }

        /// <summary>
        /// Converts a List of LavaShortcodeCache objects to LavaShortcodes.
        /// </summary>
        /// <param name="shortcodeCaches">The LavaShortcodeCache records to convert.</param>
        /// <returns>A List of LavaShortcode's.</returns>
        private List<LavaShortcode> ConvertToLavaShortcode( IEnumerable<LavaShortcodeCache> shortcodeCaches )
        {
            var distinctCategoryIds = shortcodeCaches.SelectMany( s => s.CategoryIds ).Distinct().ToList();
            var distinctCategories = CategoryCache.GetMany( distinctCategoryIds )
                .Select( c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Guid = c.Guid
                } ).ToList();

            return shortcodeCaches.Select( l => new LavaShortcode
            {
                Id = l.Id,
                Name = l.Name,
                TagName = l.TagName,
                TagType = l.TagType,
                IsActive = l.IsActive,
                IsSystem = l.IsSystem,
                Description = l.Description,
                Documentation = l.Documentation,
                Categories = distinctCategories.Where( c => l.CategoryIds.Contains( c.Id ) ).ToList()
            } ).ToList();
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    { "LavaShortcodeId", "((Key))" },
                } )
            };
        }

        /// <summary>
        /// Loads the shortcodes from the cache/database
        /// and through reflection for shortcodes that aren't stored in the database.
        /// </summary>
        private List<LavaShortcode> GetLavaShortcodes()
        {
            // Get all the shortcodes from the cache/database first.
            var shortcodeList = ConvertToLavaShortcode( LavaShortcodeCache.All() );

            // Start with block items
            var shortcodeTypes = Rock.Reflection.FindTypes( typeof( ILavaShortcode ) ).Values.ToList();

            foreach ( var shortcode in shortcodeTypes )
            {
                var shortcodeMetadataAttribute = shortcode
                    .GetCustomAttributes( typeof( LavaShortcodeMetadataAttribute ), true )
                    .FirstOrDefault() as LavaShortcodeMetadataAttribute;

                // Ignore shortcodes with no metadata.
                if ( shortcodeMetadataAttribute == null )
                {
                    continue;
                }

                try
                {
                    var shortcodeInstance = Activator.CreateInstance( shortcode ) as ILavaShortcode;
                    var shortcodeType = shortcodeInstance.ElementType;

                    shortcodeList.Add( new LavaShortcode
                    {
                        Id = -1,
                        Name = shortcodeMetadataAttribute.Name,
                        TagName = shortcodeMetadataAttribute.TagName,
                        TagType = ( shortcodeType == LavaShortcodeTypeSpecifier.Inline ) ? TagType.Inline : TagType.Block,
                        IsActive = true,
                        IsSystem = true,
                        Description = shortcodeMetadataAttribute.Description,
                        Documentation = shortcodeMetadataAttribute.Documentation,
                        Categories = GetCategoriesFromMetaData( shortcodeMetadataAttribute )
                    } );

                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }

            return shortcodeList;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new LavaShortcodeService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{LavaShortcode.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {LavaShortcode.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Unregister the shortcode.
            LavaService.DeregisterShortcode( entity.TagName );
            entityService.Delete( entity );

            RockContext.SaveChanges();
            return ActionOk();
        }

        #endregion
    }
}