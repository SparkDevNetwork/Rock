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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Administration.PageProperties;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Displays the details of a particular page.
    /// </summary>
    [DisplayName( "Page Properties" )]
    [Category( "Administration" )]
    [Description( "Displays the page properties." )]
    [IconCssClass( "fa fa-question" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Enable Full Edit Mode",
        Key = AttributeKey.EnableFullEditMode,
        Description = "Have the block initially show a readonly summary view, in a panel, with Edit and Delete buttons. Also include Save and Cancel buttons.",
        DefaultBooleanValue = false,
        Order = 1 )]

    [LinkedPage(
        "Median Time to Serve Detail Page",
        Key = AttributeKey.MedianTimeDetailPage,
        Description = "The page that shows details about the median time to serve was calculated.",
        DefaultValue = SystemGuid.Page.PAGE_VIEWS,
        Order = 2 )]

    #endregion Block Attributes

    [SystemGuid.EntityTypeGuid( "d256a348-e8dc-4886-a055-eae44e71ce92" )]
    [SystemGuid.BlockTypeGuid( "4c2e12b8-dcd5-4ea6-a853-a02a5b121d13" )]
    public class PageProperties : RockEntityDetailBlockType<Page, PagePropertiesBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EnableFullEditMode = "EnableFullEditMode";
            public const string MedianTimeDetailPage = "MedianTimeDetailPage";
        }

        private static class PageParameterKey
        {
            public const string Page = "Page";
            public const string ParentPageId = "ParentPageId";
            public const string ExpandedIds = "ExpandedIds";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string Page = "Page";
            public const string MedianTimeDetailPage = "MedianTimeDetailPage";
        }

        #endregion Keys

        protected DefinedTypeCache InteractionIntentDefinedTypeCache => DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.INTERACTION_INTENT ) );

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<PagePropertiesBag, PagePropertiesOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view or edit the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private PagePropertiesOptionsBag GetBoxOptions( Page entity )
        {
            var site = GetSite( entity );
            var options = new PagePropertiesOptionsBag()
            {
                IsBlockVisible = PageParameter( PageParameterKey.Page ).IsNotNullOrWhiteSpace(),
                SiteName = site?.Name,
                CanAdministrate = entity.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ),
                SitesItems = LoadSites(),
                LayoutItems = LoadLayouts( site ),
                DisplayWhenItems = typeof( DisplayInNavWhen ).ToEnumListItemBag(),
                IntentDefinedTypeGuid = InteractionIntentDefinedTypeCache?.Guid.ToString(),
                EnableFullEditMode = this.GetAttributeValue( AttributeKey.EnableFullEditMode ).AsBoolean()
            };
            return options;
        }

        /// <summary>
        /// Gets the page's site.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        private SiteCache GetSite( Page page )
        {
            if ( page.Layout != null )
            {
                return SiteCache.Get( page.Layout.SiteId );
            }
            else if ( page.ParentPageId.HasValue )
            {
                var parentPageCache = PageCache.Get( page.ParentPageId.Value );
                if ( parentPageCache?.Layout != null )
                {
                    return SiteCache.Get( parentPageCache.Layout.SiteId );
                }
            }

            return null;
        }

        /// <summary>
        /// Loads the layouts based on the specified site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns></returns>
        private List<ListItemBag> LoadLayouts( SiteCache site )
        {
            if ( site == null )
            {
                return new List<ListItemBag>();
            }

            LayoutService.RegisterLayouts( "~" , site );

            var layouts = new LayoutService( RockContext ).GetBySiteId( site.Id ).Select( l => new ListItemBag()
            {
                Text = l.Name,
                Value = l.Guid.ToString(),
            } ).ToList();

            return layouts;
        }

        /// <summary>
        /// Loads the sites available in the system.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> LoadSites()
        {
            var sites = new List<ListItemBag>();

            foreach ( SiteCache site in new SiteService( RockContext )
                .Queryable()
                .OrderBy( s => s.Name )
                .Select( a => a.Id )
                .AsEnumerable()
                .Select( a => SiteCache.Get( a ) ) )
            {
                sites.Add( new ListItemBag() { Text = site.Name, Value = site.Guid.ToString() } );
            }

            return sites;
        }

        /// <summary>
        /// Validates the Page for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="page">The Page to be validated.</param>
        /// <param name="editorRoutes">The added page routes from the client.</param>
        /// <param name="response">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Page is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePage( Page page, IEnumerable<string> editorRoutes, out PagePropertiesValidationResponseBag response )
        {
            response = new PagePropertiesValidationResponseBag();

            var databasePageRoutes = page?.PageRoutes.ToList() ?? new List<PageRoute>();
            var routeService = new PageRouteService( RockContext );

            if ( editorRoutes.Any() )
            {
                int? siteId = null;
                if ( page?.Layout != null )
                {
                    siteId = page.Layout.SiteId;
                }

                // validate for any duplicate routes
                var duplicateRouteQry = routeService.Queryable()
                    .Where( r =>
                        r.PageId != page.Id &&
                        editorRoutes.Contains( r.Route ) );
                if ( siteId.HasValue )
                {
                    duplicateRouteQry = duplicateRouteQry
                        .Where( r =>
                            r.Page != null &&
                            r.Page.Layout != null &&
                            r.Page.Layout.SiteId == siteId.Value );
                }

                var duplicateRoutes = duplicateRouteQry
                    .Select( r => r.Route )
                    .Distinct()
                    .ToList();

                if ( duplicateRoutes.Any() )
                {
                    // Duplicate routes
                    response.Title = "Duplicate Route(s)";
                    response.Message = string.Format( "<strong>Duplicate Route(s)</strong><p>The page route <strong>{0}</strong>, already exists for another page in the same site. Please choose a different route name.</p>",
                        duplicateRoutes.AsDelimited( "</strong> and <strong>" ) );
                    response.CurrentTab = "Advanced Settings";

                    return false;
                }
            }

            // validate if removed routes can be deleted
            foreach ( var pageRoute in databasePageRoutes )
            {
                if ( !editorRoutes.Contains( pageRoute.Route ) && !routeService.CanDelete( pageRoute, out string errorMessage ) )
                {
                    response.Message = string.Format( "The page route <strong>{0}</strong>, cannot be removed. {1}", pageRoute.Route, errorMessage );
                    response.CurrentTab = "Advanced Settings";

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<PagePropertiesBag, PagePropertiesOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Page.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            var enableFullEditMode = this.GetAttributeValue( AttributeKey.EnableFullEditMode ).AsBoolean();

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = enableFullEditMode ? GetEntityBagForEdit( entity ) : GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Page.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Page.FriendlyTypeName );
                }
            }

            box.Options = GetBoxOptions( entity );

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="PagePropertiesBag"/> that represents the entity.</returns>
        private PagePropertiesBag GetCommonEntityBag( Page entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var pageReference = new PageReference( entity.Id );
            var pageUrl = pageReference.BuildUrl();
            var intents = new List<ListItemBag>();

            if ( entity.Id > 0 && InteractionIntentDefinedTypeCache != null )
            {
                var intentValuesIds = new EntityIntentService( RockContext ).GetIntentValueIds<Page>( entity.Id );
                intents.AddRange( intentValuesIds.ConvertAll( id => DefinedValueCache.Get( id ).ToListItemBag() ) );
            }

            var defaultCacheability = new RockCacheability()
            {
                RockCacheablityType = RockCacheablityType.Private,
                MaxAge = new TimeInterval() { Unit = TimeIntervalUnit.Minutes },
            };
            var cacheability = entity.CacheControlHeaderSettings.FromJsonOrNull<RockCacheability>() ?? defaultCacheability;
            var site = GetSite( entity );

            var layout = entity.Layout == null ? entity.ParentPage?.Layout.ToListItemBag() : entity.Layout.ToListItemBag();

            var bag = new PagePropertiesBag
            {
                IdKey = entity.IdKey,
                AllowIndexing = entity.AllowIndexing,
                BodyCssClass = entity.BodyCssClass,
                BreadCrumbDisplayIcon = entity.BreadCrumbDisplayIcon,
                BreadCrumbDisplayName = entity.BreadCrumbDisplayName,
                BrowserTitle = entity.BrowserTitle,
                CacheControlHeaderSettings = cacheability.ToCacheabilityBag(),
                Description = entity.Description,
                DisplayInNavWhen = entity.DisplayInNavWhen,
                EnableViewState = entity.EnableViewState,
                HeaderContent = entity.HeaderContent,
                IconBinaryFile = entity.IconBinaryFile.ToListItemBag(),
                IconBinaryFileId = entity.IconBinaryFileId,
                IconCssClass = entity.IconCssClass,
                IncludeAdminFooter = entity.IncludeAdminFooter,
                InternalName = entity.InternalName,
                IsSystem = entity.IsSystem,
                Layout = layout,
                MedianPageLoadTimeDurationSeconds = entity.MedianPageLoadTimeDurationSeconds,
                MenuDisplayChildPages = entity.MenuDisplayChildPages,
                MenuDisplayDescription = entity.MenuDisplayDescription,
                MenuDisplayIcon = entity.MenuDisplayIcon,
                Order = entity.Order,
                PageDisplayBreadCrumb = entity.PageDisplayBreadCrumb,
                PageDisplayDescription = entity.PageDisplayDescription,
                PageDisplayIcon = entity.PageDisplayIcon,
                PageDisplayTitle = entity.PageDisplayTitle,
                PageTitle = entity.PageTitle,
                ParentPage = ToListItemBag( entity.ParentPage ),
                ParentPageId = entity.ParentPageId,
                RateLimitPeriodDurationSeconds = entity.RateLimitPeriodDurationSeconds,
                RateLimitRequestPerPeriod = entity.RateLimitRequestPerPeriod,
                RequiresEncryption = entity.RequiresEncryption,
                PageUrl = pageUrl,
                Site = entity.Layout?.Site.ToListItemBag() ?? site.ToListItemBag(),
                PageRoute = string.Join( ",", entity.PageRoutes.Select( route => route.Route ).ToArray() ),
                Intents = intents,
                PageId = entity.Id
            };

            var pageCache = PageCache.Get( entity.Id );
            if ( pageCache?.IsAuthorized( Authorization.ADMINISTRATE, this.GetCurrentPerson() ) == true )
            {
                var blockContexts = new List<BlockContextInfoBag>();
                var entityTypeBlocks = new Dictionary<string, List<BlockCache>>();

                foreach ( var block in pageCache.Blocks )
                {
                    foreach ( var entityType in block.ContextTypesRequired )
                    {
                        var context = blockContexts.Find( t => t.EntityTypeName == entityType.Name );

                        if ( context == null )
                        {
                            context = new BlockContextInfoBag
                            {
                                EntityTypeName = entityType.Name,
                                EntityTypeFriendlyName = entityType.FriendlyName,
                                HelpText = string.Format( "The page parameter name that contains the id of this context entity. This parameter will be used by the following {0}: {1}",
                                    "block",
                                    block.ToString() ),
                                Text = pageCache.PageContexts.ContainsKey( entityType.Name ) ? pageCache.PageContexts[entityType.Name] : null
                            };

                            blockContexts.Add( context );
                            entityTypeBlocks[entityType.Name] = new List<BlockCache>();
                        }

                        entityTypeBlocks[entityType.Name].Add( block );
                    }
                }

                foreach ( var item in entityTypeBlocks )
                {
                    var context = blockContexts.Find( t => t.EntityTypeName == item.Key );
                    context.HelpText = string.Format( "The page parameter name that contains the id of this context entity. This parameter will be used by the following {0}: {1}",
                        "block".PluralizeIf( item.Value.Count > 1 ),
                        string.Join( ", ", item.Value ) );
                }

                bag.BlockContexts = blockContexts;
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override PagePropertiesBag GetEntityBagForView( Page entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        //// <inheritdoc/>
        protected override PagePropertiesBag GetEntityBagForEdit( Page entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Page entity, ValidPropertiesBox<PagePropertiesBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AllowIndexing ),
                () => entity.AllowIndexing = box.Bag.AllowIndexing );

            box.IfValidProperty( nameof( box.Bag.BodyCssClass ),
                () => entity.BodyCssClass = box.Bag.BodyCssClass );

            box.IfValidProperty( nameof( box.Bag.BreadCrumbDisplayIcon ),
                () => entity.BreadCrumbDisplayIcon = box.Bag.BreadCrumbDisplayIcon );

            box.IfValidProperty( nameof( box.Bag.BreadCrumbDisplayName ),
                () => entity.BreadCrumbDisplayName = box.Bag.BreadCrumbDisplayName );

            box.IfValidProperty( nameof( box.Bag.BrowserTitle ),
                () => entity.BrowserTitle = ( entity.Id == 0 && box.Bag.BrowserTitle.IsNullOrWhiteSpace() ) ? box.Bag.InternalName : box.Bag.BrowserTitle );

            box.IfValidProperty( nameof( box.Bag.CacheControlHeaderSettings ),
                () => entity.CacheControlHeaderSettings = ToCacheability( box.Bag ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.DisplayInNavWhen ),
                () => entity.DisplayInNavWhen = box.Bag.DisplayInNavWhen );

            box.IfValidProperty( nameof( box.Bag.EnableViewState ),
                () => entity.EnableViewState = box.Bag.EnableViewState );

            box.IfValidProperty( nameof( box.Bag.HeaderContent ),
                () => entity.HeaderContent = box.Bag.HeaderContent );

            box.IfValidProperty( nameof( box.Bag.IconBinaryFile ),
                () => entity.IconBinaryFileId = box.Bag.IconBinaryFile.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.IconBinaryFileId ),
                () => entity.IconBinaryFileId = box.Bag.IconBinaryFileId );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.IncludeAdminFooter ),
                () => entity.IncludeAdminFooter = box.Bag.IncludeAdminFooter );

            box.IfValidProperty( nameof( box.Bag.InternalName ),
                () => entity.InternalName = box.Bag.InternalName );

            box.IfValidProperty( nameof( box.Bag.Layout ),
                () => entity.LayoutId = box.Bag.Layout.GetEntityId<Layout>( RockContext ).Value );

            box.IfValidProperty( nameof( box.Bag.MedianPageLoadTimeDurationSeconds ),
                () => entity.MedianPageLoadTimeDurationSeconds = box.Bag.MedianPageLoadTimeDurationSeconds );

            box.IfValidProperty( nameof( box.Bag.MenuDisplayChildPages ),
                () => entity.MenuDisplayChildPages = box.Bag.MenuDisplayChildPages );

            box.IfValidProperty( nameof( box.Bag.MenuDisplayDescription ),
                () => entity.MenuDisplayDescription = box.Bag.MenuDisplayDescription );

            box.IfValidProperty( nameof( box.Bag.MenuDisplayIcon ),
                () => entity.MenuDisplayIcon = box.Bag.MenuDisplayIcon );

            box.IfValidProperty( nameof( box.Bag.Order ),
                () => entity.Order = box.Bag.Order );

            box.IfValidProperty( nameof( box.Bag.PageDisplayBreadCrumb ),
                () => entity.PageDisplayBreadCrumb = box.Bag.PageDisplayBreadCrumb );

            box.IfValidProperty( nameof( box.Bag.PageDisplayDescription ),
                () => entity.PageDisplayDescription = box.Bag.PageDisplayDescription );

            box.IfValidProperty( nameof( box.Bag.PageDisplayIcon ),
                () => entity.PageDisplayIcon = box.Bag.PageDisplayIcon );

            box.IfValidProperty( nameof( box.Bag.PageDisplayTitle ),
                () => entity.PageDisplayTitle = box.Bag.PageDisplayTitle );

            box.IfValidProperty( nameof( box.Bag.PageTitle ),
                () => entity.PageTitle = ( entity.Id == 0 && box.Bag.PageTitle.IsNullOrWhiteSpace() ) ? box.Bag.InternalName : box.Bag.PageTitle );

            box.IfValidProperty( nameof( box.Bag.ParentPage ),
                () => entity.ParentPageId = box.Bag.ParentPage.GetEntityId<Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ParentPageId ),
                () => entity.ParentPageId = box.Bag.ParentPageId );

            box.IfValidProperty( nameof( box.Bag.RateLimitPeriodDurationSeconds ),
                () => entity.RateLimitPeriodDurationSeconds = box.Bag.RateLimitPeriodDurationSeconds );

            box.IfValidProperty( nameof( box.Bag.RateLimitRequestPerPeriod ),
                () => entity.RateLimitRequestPerPeriod = box.Bag.RateLimitRequestPerPeriod );

            box.IfValidProperty( nameof( box.Bag.RequiresEncryption ),
                () => entity.RequiresEncryption = box.Bag.RequiresEncryption );

            box.IfValidProperty( nameof( box.Bag.PageRoute ),
                () => SavePageRoutes( box.Bag, entity ) );

            box.IfValidProperty( nameof( box.Bag.PageRoute ),
                () => SaveContexts( box.Bag, entity ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Saves the page's contexts.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="page">The page.</param>
        private void SaveContexts( PagePropertiesBag bag, Page page )
        {
            var contextService = new PageContextService( RockContext );
            // Clear existing contexts in preparation for update
            foreach ( var pageContext in page.PageContexts.ToList() )
            {
                contextService.Delete( pageContext );
            }

            // Set new contexts.
            page.PageContexts.Clear();
            foreach ( var contextInfo in bag.BlockContexts )
            {
                if ( !string.IsNullOrWhiteSpace( contextInfo.EntityTypeName ) && !string.IsNullOrWhiteSpace( contextInfo.Text ) )
                {
                    var pageContext = new PageContext
                    {
                        Entity = contextInfo.EntityTypeName,
                        IdParameter = contextInfo.Text
                    };
                    page.PageContexts.Add( pageContext );
                }
            }
        }

        /// <summary>
        /// Saves the page's routes.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="page">The page.</param>
        private void SavePageRoutes( PagePropertiesBag bag, Page page )
        {
            var editorRoutes = bag.PageRoute.SplitDelimitedValues().Distinct();
            var databasePageRoutes = page.PageRoutes.ToList();
            var routeService = new PageRouteService( RockContext );

            // take care of deleted routes
            foreach ( var pageRoute in databasePageRoutes )
            {
                if ( !editorRoutes.Contains( pageRoute.Route ) )
                {
                    // if they removed the Route, remove it from the database
                    page.PageRoutes.Remove( pageRoute );

                    routeService.Delete( pageRoute );
                }
            }

            // take care of added routes
            foreach ( string route in editorRoutes )
            {
                // if they added the Route, add it to the database
                if ( !databasePageRoutes.Exists( a => a.Route == route ) )
                {
                    var pageRoute = new PageRoute
                    {
                        Route = route.TrimStart( '/' ),
                        Guid = Guid.NewGuid()
                    };
                    page.PageRoutes.Add( pageRoute );
                }
            }
        }

        /// <summary>
        /// Converts the ParentPage to ListItemBag, the ToListItemBag extension method calls ToString
        /// to get the text property of the which returns the Page title, the picker however uses the
        /// InternalName of the page so we need to return that.
        /// </summary>
        /// <param name="parentPage">The parent page.</param>
        /// <returns></returns>
        private ListItemBag ToListItemBag( Page parentPage )
        {
            if ( parentPage == null )
            {
                return null;
            }

            return new ListItemBag()
            {
                Text = parentPage.InternalName,
                Value = parentPage.Guid.ToString()
            };
        }

        /// <inheritdoc/>
        protected override Page GetInitialEntity()
        {
            var pageService = new PageService( RockContext );
            var pageId = PageParameter( PageParameterKey.Page );
            var parentPageId = PageParameter( PageParameterKey.ParentPageId ).AsIntegerOrNull();
            var id = !PageCache.Layout.Site.DisablePredictableIds ? pageId.AsIntegerOrNull() : null;
            var guid = pageId.AsGuidOrNull();

            // If a zero identifier is specified then create a new entity.
            if ( ( id.HasValue && id.Value == 0 ) || ( guid.HasValue && guid.Value == Guid.Empty ) || ( !id.HasValue && !guid.HasValue && pageId.IsNullOrWhiteSpace() ) )
            {
                return CreateNewPage( pageService, parentPageId );
            }

            var page = pageService.GetQueryableByKey( pageId, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( "Layout" )
                .Include( "PageRoutes" )
                .FirstOrDefault();

            if ( page == null )
            {
                page = CreateNewPage( pageService, parentPageId );
            }

            return page;
        }

        /// <summary>
        /// Creates the new page.
        /// </summary>
        /// <param name="pageService">The page service.</param>
        /// <returns></returns>
        private static Page CreateNewPage( PageService pageService, int? parentPageId )
        {
            var page = new Page { Id = 0, IsSystem = false, ParentPageId = parentPageId };

            // fetch the ParentPage (if there is one) so that security can check it, and also default some stuff based on the ParentPage
            if ( parentPageId.HasValue )
            {
                page.ParentPage = pageService.Get( parentPageId.Value );
                if ( page.ParentPage != null )
                {
                    page.AllowIndexing = page.ParentPage.AllowIndexing;
                    page.LayoutId = page.ParentPage.LayoutId;
                    page.Layout = page.ParentPage.Layout;
                }
            }

            return page;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.MedianTimeDetailPage] = this.GetLinkedPageUrl( AttributeKey.MedianTimeDetailPage, NavigationUrlKey.Page, "((Key))" ),
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Page entity, out BlockActionResult error )
        {
            var entityService = new PageService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new Page();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Page.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Page.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Serializes the CacheControlHeaderSettings.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        private static string ToCacheability( PagePropertiesBag bag )
        {
            var cacheability = bag.CacheControlHeaderSettings.ToCacheability();
            return cacheability?.ToJson();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<PagePropertiesBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<PagePropertiesBag> box )
        {
            var entityService = new PageService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var editorRoutes = box.Bag.PageRoute.SplitDelimitedValues().Distinct();

            // Ensure everything is valid before saving.
            if ( !ValidatePage( entity, editorRoutes, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage.ToCamelCaseJson( true, false ) );
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            var parentPageId = entity.ParentPageId;

            if ( parentPageId.HasValue && parentPageId != 0 && entity.Id == 0 )
            {
                // newly added page, make sure the Order is correct
                var lastPage = entityService.GetByParentPageId( parentPageId ).OrderByDescending( b => b.Order ).FirstOrDefault();
                if ( lastPage != null )
                {
                    entity.Order = lastPage.Order + 1;
                }
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );

                // Interaction Intents
                if ( InteractionIntentDefinedTypeCache != null )
                {
                    var pageIntentIds = new List<int>();
                    foreach ( var listItemBag in box.Bag.Intents )
                    {
                        var definedValue = DefinedValueCache.Get( listItemBag.Value.AsGuid() );
                        if ( definedValue != null )
                        {
                            pageIntentIds.Add( definedValue.Id );
                        }
                    }
                    new EntityIntentService( RockContext )
                        .SetIntents<Page>( entity.Id, pageIntentIds );

                    RockContext.SaveChanges();
                }
            } );

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.Page] = entity.IdKey;

            string expandedIds = this.PageParameter( PageParameterKey.ExpandedIds );
            if ( expandedIds != null )
            {
                // remove the current pageId param to avoid extra treeview flash
                var expandedIdList = expandedIds.SplitDelimitedValues().AsIntegerList();
                expandedIdList.Remove( entity.Id );

                // add the parentPageId to the expanded ids
                var parentPageParam = this.PageParameter( PageParameterKey.ParentPageId );
                if ( !string.IsNullOrEmpty( parentPageParam ) && parentPageId.HasValue && !expandedIdList.Contains( parentPageId.Value ) )
                {
                    expandedIdList.Add( parentPageId.Value );
                }

                qryParams[PageParameterKey.ExpandedIds] = expandedIdList.AsDelimited( "," );
            }

            return ActionOk( this.GetCurrentPageUrl( qryParams ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="bag">Details on the entity to be deleted, includes confirmation on whether or not page interactions should be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( DeletePageRequestBag bag )
        {
            var pageService = new PageService( RockContext );
            var siteService = new SiteService( RockContext );

            if ( !TryGetEntityForEditAction( bag.Key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !pageService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            foreach ( var site in siteService.Queryable() )
            {
                if ( site.DefaultPageId == entity.Id )
                {
                    site.DefaultPageId = null;
                    site.DefaultPageRouteId = null;
                }

                if ( site.LoginPageId == entity.Id )
                {
                    site.LoginPageId = null;
                    site.LoginPageRouteId = null;
                }

                if ( site.RegistrationPageId == entity.Id )
                {
                    site.RegistrationPageId = null;
                    site.RegistrationPageRouteId = null;
                }
            }

            int? parentPageId = entity.ParentPageId;

            pageService.Delete( entity );

            if ( bag.DeleteInteractions )
            {
                var deleteInteractionsMsg = new DeleteInteractions.Message
                {
                    PageId = entity.Id,
                    SiteId = entity.SiteId
                };

                deleteInteractionsMsg.Send();
            }

            RockContext.SaveChanges();

            // reload page, selecting the deleted page's parent
            var qryParams = new Dictionary<string, string>();
            if ( parentPageId.HasValue )
            {
                qryParams[PageParameterKey.Page] = parentPageId.ToString();

                string expandedIds = this.RequestContext.GetPageParameter( PageParameterKey.ExpandedIds );
                if ( expandedIds != null )
                {
                    // remove the current pageId param to avoid extra treeview flash
                    var expandedIdList = expandedIds.SplitDelimitedValues().AsIntegerList();
                    expandedIdList.Remove( parentPageId.Value );

                    qryParams[PageParameterKey.ExpandedIds] = expandedIdList.AsDelimited( "," );
                }
            }

            return ActionOk( this.GetCurrentPageUrl( qryParams ) );
        }

        /// <summary>
        /// Loads the site layouts.
        /// </summary>
        /// <param name="siteGuid">The site guid.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult LoadSiteLayouts( Guid siteGuid )
        {
            var site = SiteCache.Get( siteGuid );

            var layouts = LoadLayouts( site );

            return ActionOk( new { Layouts = layouts } );
        }

        /// <summary>
        /// Copies the specified page.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult CopyPage( CopyPageRequestBag bag )
        {
            var pageService = new PageService( RockContext );
            var qryParams = new Dictionary<string, string>();
            var page = PageCache.GetByIdKey( bag.Key );

            if ( page != null )
            {
                var copiedPageGuid = pageService.CopyPage( page.Id, bag.IncludeChildPages, this.GetCurrentPerson().PrimaryAliasId );
                if ( copiedPageGuid.HasValue )
                {
                    var copiedPage = PageCache.Get( copiedPageGuid.Value );

                    // reload page (Assuming we are using Page Builder UI) to the new copied page
                    if ( copiedPage != null )
                    {
                        qryParams[PageParameterKey.Page] = copiedPage.IdKey;

                        string expandedIds = this.PageParameter( PageParameterKey.ExpandedIds );
                        if ( expandedIds != null )
                        {
                            // remove the current pageId param to avoid extra treeview flash
                            var expandedIdList = expandedIds.SplitDelimitedValues().AsIntegerList();
                            expandedIdList.Remove( copiedPage.Id );

                            // add the parentPageId to the expanded ids
                            var parentPageParam = this.PageParameter( PageParameterKey.ParentPageId );
                            if ( !string.IsNullOrEmpty( parentPageParam ) )
                            {
                                var parentPageId = parentPageParam.AsIntegerOrNull();
                                if ( parentPageId.HasValue && !expandedIdList.Contains( parentPageId.Value ) )
                                {
                                    expandedIdList.Add( parentPageId.Value );
                                }
                            }

                            qryParams[PageParameterKey.ExpandedIds] = expandedIdList.AsDelimited( "," );
                        }
                    }
                }
            }

            return ActionOk( this.GetCurrentPageUrl( qryParams ) );
        }

        #endregion
    }
}
