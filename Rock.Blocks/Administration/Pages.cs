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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Tasks;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Administration.Pages;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Lists the child pages of a page and allows them to be reordered, added,
    /// renamed, copied, and deleted.
    /// </summary>
    [DisplayName( "Pages" )]
    [Category( "Administration" )]
    [Description( "Lists pages in Rock." )]
    [IconCssClass( "ti ti-files" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "0CE697C7-F101-4326-8BFD-3A2DC936F877" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "EE48BABA-C094-4A5F-A393-383B9BDE16E2" )]
    [Rock.SystemGuid.BlockTypeGuid( "AEFC2DBE-37B6-4CAB-882C-B214F587BF2E" )]
    [CustomizedGrid]
    public class Pages : RockEntityListBlockType<Rock.Model.Page>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string EditPage = "EditPage";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The page whose children are being listed, resolved from the <see cref="PageParameterKey.EditPage"/>
        /// parameter. Access via <see cref="GetEditPage"/>. <c>null</c> when no valid page is specified.
        /// </summary>
        private PageCache _editPage;

        /// <summary>
        /// Indicates whether <see cref="_editPage"/> has been resolved, so a missing page is not re-queried.
        /// </summary>
        private bool _isEditPageLoaded;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PagesOptionsBag>();
            var builder = GetGridBuilder();
            var canConfigure = GetCanConfigure();

            box.IsAddEnabled = canConfigure;
            box.IsDeleteEnabled = canConfigure;

            box.ExpectedRowCount = GetEditPage()?.ChildPages.Count;
            box.Options = GetBoxOptions( canConfigure );
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <param name="canConfigure">Whether the current person is authorized to configure the page.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private PagesOptionsBag GetBoxOptions( bool canConfigure )
        {
            var options = new PagesOptionsBag
            {
                IsBlockVisible = canConfigure
            };

            if ( !canConfigure )
            {
                options.AuthorizationMessage = "You are not authorized to configure this page";
                return options;
            }

            var editPage = GetEditPage();
            var siteId = editPage?.SiteId ?? PageCache.SiteId;

            options.Layouts = LayoutCache.All()
                .Where( l => l.SiteId == siteId )
                .OrderBy( l => l.Name )
                .Select( l => new ListItemBag { Value = l.Guid.ToString(), Text = l.Name } )
                .ToList();

            var parentPageId = editPage?.ParentPageId;
            options.ParentPageUrl = parentPageId.HasValue
                ? RequestContext.ResolveRockUrl( $"~/page/{parentPageId.Value}" )
                : null;

            return options;
        }

        /// <inheritdoc/>
        protected override GridDataBag GetGridDataBag( RockContext rockContext )
        {
            if ( !GetCanConfigure() )
            {
                return new GridDataBag { Rows = new List<Dictionary<string, object>>() };
            }

            return base.GetGridDataBag( rockContext );
        }

        /// <inheritdoc/>
        protected override IQueryable<Rock.Model.Page> GetListQueryable( RockContext rockContext )
        {
            var editPage = GetEditPage();

            return new PageService( rockContext ).GetByParentPageId( editPage?.Id, "Layout" );
        }

        /// <inheritdoc/>
        protected override IQueryable<Rock.Model.Page> GetOrderedListQueryable( IQueryable<Rock.Model.Page> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( p => p.Order );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Rock.Model.Page> GetGridBuilder()
        {
            var pageUrlBase = RequestContext.ResolveRockUrl( "~/page/" );

            return new GridBuilder<Rock.Model.Page>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "id", a => a.Id )
                .AddTextField( "internalName", a => a.InternalName )
                .AddTextField( "layout", a => a.Layout?.Name )
                .AddTextField( "pageUrl", a => $"{pageUrlBase}{a.Id}" );
        }

        /// <summary>
        /// Gets the page whose children are being listed, resolved once from the page parameter.
        /// </summary>
        /// <returns>The <see cref="PageCache"/>, or <c>null</c> when no valid page is specified.</returns>
        private PageCache GetEditPage()
        {
            if ( !_isEditPageLoaded )
            {
                var editPageKey = PageParameter( PageParameterKey.EditPage );
                _editPage = PageCache.Get( editPageKey, !PageCache.Layout.Site.DisablePredictableIds );
                _isEditPageLoaded = true;
            }

            return _editPage;
        }

        /// <summary>
        /// Determines whether the current person is authorized to configure the page. When a page is
        /// specified, authorization is checked against that page; otherwise it falls back to the block.
        /// </summary>
        /// <returns><c>true</c> if the current person can configure the page; otherwise <c>false</c>.</returns>
        private bool GetCanConfigure()
        {
            var editPage = GetEditPage();

            if ( editPage != null )
            {
                return editPage.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
            }

            return BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the page information for the add/edit form.
        /// </summary>
        /// <param name="key">The identifier of the page to edit, or empty to add a new child page.</param>
        /// <returns>A <see cref="PageBag"/> that represents the page being added or edited.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !GetCanConfigure() )
            {
                return ActionBadRequest( "Not authorized to configure this page." );
            }

            var page = new PageService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( page != null )
            {
                return ActionOk( new PageBag
                {
                    IdKey = page.IdKey,
                    InternalName = page.InternalName,
                    Layout = LayoutCache.Get( page.LayoutId )?.Guid.ToString()
                } );
            }

            // No page found, so default the layout for a new child page to the parent page's layout.
            var editPage = GetEditPage();
            var defaultLayoutId = editPage?.LayoutId ?? PageCache.LayoutId;

            return ActionOk( new PageBag
            {
                IdKey = string.Empty,
                InternalName = string.Empty,
                Layout = LayoutCache.Get( defaultLayoutId )?.Guid.ToString()
            } );
        }

        /// <summary>
        /// Adds a new child page or saves changes to an existing one.
        /// </summary>
        /// <param name="bag">The information required to add or edit the page.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( PageBag bag )
        {
            if ( !GetCanConfigure() )
            {
                return ActionBadRequest( "Not authorized to configure this page." );
            }

            if ( bag == null )
            {
                return ActionBadRequest( "Invalid page data." );
            }

            var layoutId = LayoutCache.Get( bag.Layout.AsGuid() )?.Id;

            if ( !layoutId.HasValue )
            {
                return ActionBadRequest( "A valid layout is required." );
            }

            var pageService = new PageService( RockContext );
            var isNew = bag.IdKey.IsNullOrWhiteSpace();
            var editPage = GetEditPage();
            Rock.Model.Page page;

            if ( isNew )
            {
                int? parentPageId = editPage?.Id;

                page = new Rock.Model.Page
                {
                    ParentPageId = parentPageId,
                    AllowIndexing = editPage?.AllowIndexing ?? true,
                    PageTitle = bag.InternalName,
                    BrowserTitle = bag.InternalName,
                    EnableViewState = true,
                    IncludeAdminFooter = true,
                    MenuDisplayChildPages = true
                };

                // Place the new page at the end of its siblings.
                var lastSiblingOrder = pageService.GetByParentPageId( parentPageId )
                    .OrderByDescending( p => p.Order )
                    .Select( p => ( int? ) p.Order )
                    .FirstOrDefault();

                page.Order = lastSiblingOrder.HasValue ? lastSiblingOrder.Value + 1 : 0;

                pageService.Add( page );
            }
            else
            {
                page = pageService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            if ( page == null )
            {
                return ActionBadRequest( $"{Rock.Model.Page.FriendlyTypeName} not found." );
            }

            page.LayoutId = layoutId.Value;
            page.InternalName = bag.InternalName;

            if ( !page.IsValid )
            {
                return ActionBadRequest( page.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() );
            }

            RockContext.SaveChanges();

            /*
                6/18/26 - MSE

                Only NEW child pages inherit the parent page's authorization. The legacy
                WebForms block called CopyAuthorization on every save (new and existing)
                whenever a parent page was present. Because CopyAuthorization is destructive
                (it deletes the target's existing Auth rows and recreates them from the
                source), re-copying on edit silently reset a child page's custom security
                whenever it was renamed or relayouted. We intentionally diverge from that
                behavior and copy authorization only on creation.

                Reason: Editing a page must not clobber its custom authorization.
            */
            if ( isNew && editPage != null )
            {
                Authorization.CopyAuthorization( editPage, page, RockContext );
            }

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified page.
        /// </summary>
        /// <param name="key">The identifier of the page to delete.</param>
        /// <param name="deleteInteractions">Whether the interactions for the page should also be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key, bool deleteInteractions )
        {
            if ( !GetCanConfigure() )
            {
                return ActionBadRequest( "Not authorized to configure this page." );
            }

            var pageService = new PageService( RockContext );
            var page = pageService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( page == null )
            {
                return ActionBadRequest( $"{Rock.Model.Page.FriendlyTypeName} not found." );
            }

            if ( !pageService.CanDelete( page, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Clear any site references to this page so the delete does not violate a foreign key.
            var sites = new SiteService( RockContext ).Queryable()
                .Where( s => s.DefaultPageId == page.Id || s.LoginPageId == page.Id || s.RegistrationPageId == page.Id )
                .ToList();

            foreach ( var site in sites )
            {
                if ( site.DefaultPageId == page.Id )
                {
                    site.DefaultPageId = null;
                    site.DefaultPageRouteId = null;
                }

                if ( site.LoginPageId == page.Id )
                {
                    site.LoginPageId = null;
                    site.LoginPageRouteId = null;
                }

                if ( site.RegistrationPageId == page.Id )
                {
                    site.RegistrationPageId = null;
                    site.RegistrationPageRouteId = null;
                }
            }

            var pageId = page.Id;
            var siteId = page.SiteId;

            pageService.Delete( page );
            RockContext.SaveChanges();

            if ( deleteInteractions )
            {
                new DeleteInteractions.Message
                {
                    PageId = pageId,
                    SiteId = siteId
                }.Send();
            }

            return ActionOk();
        }

        /// <summary>
        /// Copies the specified page and all of its child pages.
        /// </summary>
        /// <param name="key">The identifier of the page to copy.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            if ( !GetCanConfigure() )
            {
                return ActionBadRequest( "Not authorized to configure this page." );
            }

            var pageService = new PageService( RockContext );
            var page = pageService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( page == null )
            {
                return ActionBadRequest( $"{Rock.Model.Page.FriendlyTypeName} not found." );
            }

            pageService.CopyPage( page.Id, true, RequestContext.CurrentPerson?.PrimaryAliasId );

            return ActionOk();
        }

        /// <summary>
        /// Changes the ordered position of a single page.
        /// </summary>
        /// <param name="key">The identifier of the page that will be moved.</param>
        /// <param name="beforeKey">The identifier of the page it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            if ( !GetCanConfigure() )
            {
                return ActionBadRequest( "Not authorized to configure this page." );
            }

            // Only the children of a specified page may be reordered. Without a page
            // the list would be the root pages, which this block does not manage.
            if ( GetEditPage() == null )
            {
                return ActionBadRequest( "No page was specified to reorder." );
            }

            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );

            // Get the entities from the database.
            var items = GetListItems( qry, RockContext );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}
