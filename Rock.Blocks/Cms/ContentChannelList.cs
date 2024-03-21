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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentChannelList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of content channels.
    /// </summary>
    [DisplayName( "Content Channel List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of content channels." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the content channel details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "de1ab18e-c973-4333-832e-a8b4754f0571" )]
    [Rock.SystemGuid.BlockTypeGuid( "f381936b-0d8c-43f0-8da5-401383e40883" )]
    [CustomizedGrid]
    public class ContentChannelList : RockListBlockType<ContentChannelList.ContentChannelListBag>
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

        private static class UserPreferenceKey
        {
            public const string Type = "Type";
            public const string Categories = "Categories";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ContentChannelListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddDeleteEnabled();
            box.IsDeleteEnabled = GetIsAddDeleteEnabled();
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ContentChannelListOptionsBag GetBoxOptions()
        {
            var options = new ContentChannelListOptionsBag
            {
                ContentChannelTypeItems = ContentChannelTypeCache.All().OrderBy( c => c.Name ).ToListItemBagList()
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ContentChannelId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<ContentChannelListBag> GetListQueryable( RockContext rockContext )
        {
            return GetGridDataQueryable( rockContext );
        }

        /// <inheritdoc/>
        protected override GridBuilder<ContentChannelListBag> GetGridBuilder()
        {
            return new GridBuilder<ContentChannelListBag>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "contentChannelType", a => a.ContentChannelType )
                .AddTextField( "channelUrl", a => a.ChannelUrl )
                .AddField( "totalItems", a => a.TotalItems )
                .AddField( "activeItems", a => a.ActiveItems )
                .AddDateTimeField( "itemLastCreated", a => a.ItemLastCreated )
                .AddField( "isSecurityDisabled", a => !a.ContentChannel.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        private IQueryable<ContentChannelListBag> GetGridDataQueryable( RockContext rockContext )
        {
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );

            var qry = contentChannelService.Queryable()
                .Include( a => a.ContentChannelType )
                .Include( a => a.Items )
                .Where( a => a.ContentChannelType.ShowInChannelList );

            var typeGuid = GetBlockPersonPreferences().GetValue( UserPreferenceKey.Type ).AsGuidOrNull();
            if ( typeGuid.HasValue )
            {
                qry = qry.Where( c => c.ContentChannelType.Guid == typeGuid.Value );
            }

            var selectedCategories = GetBlockPersonPreferences().GetValue( UserPreferenceKey.Categories ).FromJsonOrNull<List<ListItemBag>>();
            if ( selectedCategories?.Any() == true )
            {
                var selectedCategoryGuids = selectedCategories.Select( c => c.Value.AsGuid() );
                qry = qry.Where( a => a.Categories.Any( c => selectedCategoryGuids.Contains( c.Guid ) ) );
            }

            var channels = qry.AsEnumerable().Where( c => c.IsAuthorized( Rock.Security.Authorization.VIEW, GetCurrentPerson() ) );
            var now = RockDateTime.Now;
            var items = channels.Select( c => new ContentChannelListBag
            {
                IdKey = c.IdKey,
                Name = c.Name,
                ContentChannelType = c.ContentChannelType.Name,
                EnableRss = c.EnableRss,
                ChannelUrl = c.ChannelUrl,
                ItemLastCreated = c.Items.Max( i => i.CreatedDateTime ),
                ContentChannel = c,
                TotalItems = c.Items.Count,
                ActiveItems = c.Items
                    .Count( i =>
                        ( i.StartDateTime.CompareTo( now ) < 0 ) &&
                        ( !i.ExpireDateTime.HasValue || i.ExpireDateTime.Value.CompareTo( now ) > 0 ) &&
                        ( i.ApprovedByPersonAliasId.HasValue || !c.RequiresApproval )
                )
            } ).AsQueryable();

            return items;
        }

        /// <inheritdoc/>
        protected override IQueryable<ContentChannelListBag> GetOrderedListQueryable( IQueryable<ContentChannelListBag> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( c => c.Name );
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
            using ( var rockContext = new RockContext() )
            {
                var entityService = new ContentChannelService( rockContext );
                var contentChannelItemService = new ContentChannelItemService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{ContentChannel.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${ContentChannel.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entity.ParentContentChannels.Clear();

                rockContext.WrapTransaction( () =>
                {
                    var channelItemsToDelete = contentChannelItemService
                        .Queryable()
                        .Where( t => t.ContentChannelId == entity.Id );

                    contentChannelItemService.DeleteRange( channelItemsToDelete );

                    entityService.Delete( entity );
                    rockContext.SaveChanges();
                } );

                return ActionOk();
            }
        }

        #endregion

        #region Helper Classes

        public sealed class ContentChannelListBag
        {
            public string IdKey { get; set; }
            public string Name { get; set; }
            public string ContentChannelType { get; set; }
            public bool EnableRss { get; set; }
            public string ChannelUrl { get; set; }
            public DateTime? ItemLastCreated { get; set; }
            public int TotalItems { get; set; }
            public int ActiveItems { get; set; }
            public ContentChannel ContentChannel { get; set; }
        }

        #endregion
    }
}
