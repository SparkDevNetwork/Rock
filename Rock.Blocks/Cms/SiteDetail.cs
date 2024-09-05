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
using System.IO;
using System.Linq;
using System.Web.Hosting;

using DotLiquid;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.SiteDetail;
using Rock.ViewModels.Blocks.Communication.SnippetTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular site.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Site Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular site." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BinaryFileTypeField( "Default File Type",
        Key = AttributeKey.DefaultFileType,
        Description = "The default file type to use while uploading Favicon",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.BinaryFiletype.DEFAULT, // this was previously defaultBinaryFileTypeGuid which maps to base default value
        Category = "",
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "88ce8a0b-35b6-4427-817f-2fdf485d0241" )]
    [Rock.SystemGuid.BlockTypeGuid( "3e935e45-4796-4389-ab1c-98d2403faedf" )]
    public class SiteDetail : RockEntityDetailBlockType<Site, SiteBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string DefaultFileType = "DefaultFileType";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<SiteBag, SiteDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );
            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private SiteDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new SiteDetailOptionsBag();

            options.Themes = new List<ListItemBag>();

            var physicalRootFolder = AppDomain.CurrentDomain.BaseDirectory;
            string physicalFolder = Path.Combine( physicalRootFolder, RequestContext.ResolveRockUrl( "~~/" ).RemoveLeadingForwardslash() );
            var di = new DirectoryInfo( physicalFolder );
            options.Themes.AddRange( di.Parent.EnumerateDirectories().OrderBy( a => a.Name ).Select( themeDir => new ListItemBag() { Text = themeDir.Name, Value = themeDir.Name } ) );
            return options;
        }

        /// <summary>
        /// Validates the Site for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="site">The Site to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Site is valid, <c>false</c> otherwise.</returns>
        private bool ValidateSite( Site site, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<SiteBag, SiteDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Site.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Site.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.Options.ReservedKeyNames = box.Entity.SiteAttributes.Select( a => a.Key ).ToList();
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Site.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="SiteBag"/> that represents the entity.</returns>
        private SiteBag GetCommonEntityBag( Site entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new SiteBag
            {
                IdKey = entity.IdKey,
                AllowedFrameDomains = entity.AllowedFrameDomains,
                AllowIndexing = entity.AllowIndexing,
                ChangePasswordPage = entity.ChangePasswordPage.ToListItemBag(),
                ChangePasswordPageRoute = entity.ChangePasswordPageRoute.ToListItemBag(),
                CommunicationPage = entity.CommunicationPage.ToListItemBag(),
                CommunicationPageRoute = entity.CommunicationPageRoute.ToListItemBag(),
                DefaultPage = entity.DefaultPage.ToListItemBag(),
                DefaultPageRoute = entity.DefaultPageRoute.ToListItemBag(),
                Description = entity.Description,
                DisablePredictableIds = entity.DisablePredictableIds,
                EnabledForShortening = entity.EnabledForShortening,
                EnableExclusiveRoutes = entity.EnableExclusiveRoutes,
                EnableMobileRedirect = entity.EnableMobileRedirect,
                EnablePageViews = entity.EnablePageViews,
                EnablePersonalization = entity.EnablePersonalization,
                EnableVisitorTracking = entity.EnableVisitorTracking,
                ErrorPage = entity.ErrorPage,
                ExternalUrl = entity.ExternalUrl,
                FavIconBinaryFile = entity.FavIconBinaryFile.ToListItemBag(),
                GoogleAnalyticsCode = entity.GoogleAnalyticsCode,
                IndexStartingLocation = entity.IndexStartingLocation,
                IsActive = entity.IsActive,
                IsIndexEnabled = entity.IsIndexEnabled,
                IsSystem = entity.IsSystem,
                LoginPage = entity.LoginPage.ToListItemBag(),
                LoginPageRoute = entity.LoginPageRoute.ToListItemBag(),
                MobilePage = entity.MobilePage.ToListItemBag(),
                Name = entity.Name,
                PageHeaderContent = entity.PageHeaderContent,
                PageNotFoundPage = entity.PageNotFoundPage.ToListItemBag(),
                PageNotFoundPageRoute = entity.PageNotFoundPageRoute.ToListItemBag(),
                RedirectTablets = entity.RedirectTablets,
                RegistrationPage = entity.RegistrationPage.ToListItemBag(),
                RegistrationPageRoute = entity.RegistrationPageRoute.ToListItemBag(),
                RequiresEncryption = entity.RequiresEncryption,
                SiteLogoBinaryFile = entity.SiteLogoBinaryFile.ToListItemBag(),
                Theme = entity.Theme,
                SiteUrl = $"{this.RequestContext.ResolveRockUrl( "~/page/" )}{entity.DefaultPageId}"
            };

            if ( entity.SiteDomains != null )
            {
                bag.SiteDomains = string.Join( "\n", entity.SiteDomains.OrderBy( d => d.Order ).Select( d => d.Domain ).ToArray() );
            }

            var attributes = GetSiteAttributes( RockContext, entity.Id.ToString() );

            bag.SiteAttributes = new List<PublicEditableAttributeBag>();
            bag.SiteAttributes.AddRange( attributes.Select( attribute => PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute ) ) );
            bag.BinaryFileTypeGuid = GetAttributeValue( AttributeKey.DefaultFileType ).AsGuid();


            return bag;
        }

        /// <inheritdoc/>
        protected override SiteBag GetEntityBagForView( Site entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            bag.AllowsCompile = new Rock.Web.UI.RockTheme( entity.Theme ).AllowsCompile;
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override SiteBag GetEntityBagForEdit( Site entity )
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
        protected override bool UpdateEntityFromBox( Site entity, ValidPropertiesBox<SiteBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AllowedFrameDomains ),
                () => entity.AllowedFrameDomains = ReformatDomains( box.Bag.AllowedFrameDomains ) );

            box.IfValidProperty( nameof( box.Bag.AllowIndexing ),
                () => entity.AllowIndexing = box.Bag.AllowIndexing );

            box.IfValidProperty( nameof( box.Bag.ChangePasswordPage ),
                () => entity.ChangePasswordPageId = box.Bag.ChangePasswordPage.GetEntityId<Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ChangePasswordPageRoute ),
                () => entity.ChangePasswordPageRouteId = box.Bag.ChangePasswordPageRoute.GetEntityId<PageRoute>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CommunicationPage ),
                () => entity.CommunicationPageId = box.Bag.CommunicationPage.GetEntityId<Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CommunicationPageRoute ),
                () => entity.CommunicationPageRouteId = box.Bag.CommunicationPageRoute.GetEntityId<PageRoute>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.DefaultPage ),
                () => entity.DefaultPageId = box.Bag.DefaultPage.GetEntityId<Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.DefaultPageRoute ),
                () => entity.DefaultPageRouteId = box.Bag.DefaultPageRoute.GetEntityId<PageRoute>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.DisablePredictableIds ),
                () => entity.DisablePredictableIds = box.Bag.DisablePredictableIds );

            box.IfValidProperty( nameof( box.Bag.EnabledForShortening ),
                () => entity.EnabledForShortening = box.Bag.EnabledForShortening );

            box.IfValidProperty( nameof( box.Bag.EnableExclusiveRoutes ),
                () => entity.EnableExclusiveRoutes = box.Bag.EnableExclusiveRoutes );

            box.IfValidProperty( nameof( box.Bag.EnableMobileRedirect ),
                () => entity.EnableMobileRedirect = box.Bag.EnableMobileRedirect );

            box.IfValidProperty( nameof( box.Bag.EnablePageViews ),
                () => entity.EnablePageViews = box.Bag.EnablePageViews );

            box.IfValidProperty( nameof( box.Bag.EnablePersonalization ),
                () => entity.EnablePersonalization = box.Bag.EnablePersonalization );

            box.IfValidProperty( nameof( box.Bag.EnableVisitorTracking ),
                () => entity.EnableVisitorTracking = box.Bag.EnableVisitorTracking );

            box.IfValidProperty( nameof( box.Bag.ErrorPage ),
                () => entity.ErrorPage = box.Bag.ErrorPage );

            box.IfValidProperty( nameof( box.Bag.ExternalUrl ),
                () => entity.ExternalUrl = box.Bag.ExternalUrl );

            box.IfValidProperty( nameof( box.Bag.FavIconBinaryFile ),
                () => entity.FavIconBinaryFileId = box.Bag.FavIconBinaryFile.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.GoogleAnalyticsCode ),
                () => entity.GoogleAnalyticsCode = box.Bag.GoogleAnalyticsCode );

            box.IfValidProperty( nameof( box.Bag.IndexStartingLocation ),
                () => entity.IndexStartingLocation = box.Bag.IndexStartingLocation );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.IsIndexEnabled ),
                () => entity.IsIndexEnabled = box.Bag.IsIndexEnabled );

            box.IfValidProperty( nameof( box.Bag.IsSystem ),
                () => entity.IsSystem = box.Bag.IsSystem );

            box.IfValidProperty( nameof( box.Bag.LoginPage ),
                () => entity.LoginPageId = box.Bag.LoginPage.GetEntityId<Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.LoginPageRoute ),
                () => entity.LoginPageRouteId = box.Bag.LoginPageRoute.GetEntityId<PageRoute>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.MobilePage ),
                () => entity.MobilePageId = box.Bag.MobilePage.GetEntityId<Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.PageHeaderContent ),
                () => entity.PageHeaderContent = box.Bag.PageHeaderContent );

            box.IfValidProperty( nameof( box.Bag.PageNotFoundPage ),
                () => entity.PageNotFoundPageId = box.Bag.PageNotFoundPage.GetEntityId<Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.PageNotFoundPageRoute ),
                () => entity.PageNotFoundPageRouteId = box.Bag.PageNotFoundPageRoute.GetEntityId<PageRoute>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.RedirectTablets ),
                () => entity.RedirectTablets = box.Bag.RedirectTablets );

            box.IfValidProperty( nameof( box.Bag.RegistrationPage ),
                () => entity.RegistrationPageId = box.Bag.RegistrationPage.GetEntityId<Page>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.RegistrationPageRoute ),
                () => entity.RegistrationPageRouteId = box.Bag.RegistrationPageRoute.GetEntityId<PageRoute>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.RequiresEncryption ),
                () => entity.RequiresEncryption = box.Bag.RequiresEncryption );

            box.IfValidProperty( nameof( box.Bag.SiteLogoBinaryFile ),
                () => entity.SiteLogoBinaryFileId = box.Bag.SiteLogoBinaryFile.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Theme ),
                () => entity.Theme = box.Bag.Theme );

            box.IfValidProperty( nameof( box.Bag.Theme ),
                () => entity.Theme = box.Bag.Theme );

            box.IfValidProperty( nameof( box.Bag.SiteDomains ),
                () => UpdateSiteDomains( entity, box, RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override Site GetInitialEntity()
        {
            return GetInitialEntity<Site, SiteService>( RockContext, PageParameterKey.SiteId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        // <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Site entity, out BlockActionResult error )
        {
            var entityService = new SiteService( RockContext );
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
                entity = new Site();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Site.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Site.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Format domain list by replacing commas and new lines with a single space
        /// </summary>
        /// <param name="allowedDomains"></param>
        /// <returns></returns>
        private string ReformatDomains( string allowedDomains )
        {
            if ( string.IsNullOrEmpty( allowedDomains ) )
            {
                return allowedDomains;
            }
            return allowedDomains
                .Replace( ", ", " " )
                .Replace( ",", " " )
                .Replace( Environment.NewLine, " " )
                .Replace( "\n", " " )
                .Replace( "  ", " " );
        }

        /// <summary>
        /// Update the domain list of the site using the provided values from the box
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="box"></param>
        /// <param name="rockContext"></param>
        private void UpdateSiteDomains( Site entity, ValidPropertiesBox<SiteBag> box, RockContext rockContext )
        {
            var siteDomainService = new SiteDomainService( rockContext );
            var currentDomains = box.Bag.SiteDomains.SplitDelimitedValues().ToList();
            entity.SiteDomains = entity.SiteDomains ?? new List<SiteDomain>();

            // Remove any deleted domains
            foreach ( var domain in entity.SiteDomains.Where( w => !currentDomains.Contains( w.Domain ) ).ToList() )
            {
                entity.SiteDomains.Remove( domain );
                siteDomainService.Delete( domain );
            }

            int order = 0;
            foreach ( string domain in currentDomains )
            {
                SiteDomain sd = entity.SiteDomains.FirstOrDefault( d => d.Domain == domain );
                if ( sd == null )
                {
                    sd = new SiteDomain
                    {
                        Domain = domain,
                        Guid = Guid.NewGuid()
                    };
                    entity.SiteDomains.Add( sd );
                }
                sd.Order = order++;
            }
        }

        /// <summary>
        /// Save attributes associated with this site.
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="qualifierColumn"></param>
        /// <param name="qualifierValue"></param>
        /// <param name="viewStateAttributes"></param>
        /// <param name="rockContext"></param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> viewStateAttributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
            }

            rockContext.SaveChanges();

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        /// <summary>
        /// Gets the site attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="siteIdQualifierValue">The site identifier qualifier value.</param>
        /// <returns></returns>
        private static List<Model.Attribute> GetSiteAttributes( RockContext rockContext, string siteIdQualifierValue )
        {
            return new AttributeService( rockContext ).GetByEntityTypeId( new Page().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "SiteId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( siteIdQualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
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

            var box = new DetailBlockBox<SiteBag, SiteDetailOptionsBag>
            {
                Entity = GetEntityBagForEdit( entity ),
                Options = GetBoxOptions( true )
            };

            box.Options.ReservedKeyNames = box.Entity.SiteAttributes.Select( a => a.Key ).ToList();

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<SiteBag> box )
        {
            var entityService = new SiteService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            int? existingIconId = entity.FavIconBinaryFileId;
            int? existingLogoId = entity.SiteLogoBinaryFileId;

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateSite( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );

                if ( box.Bag.SiteAttributes.Count > 0 )
                {

                    SaveAttributes( new Page().TypeId, "SiteId", entity.Id.ToString(), box.Bag.SiteAttributes, RockContext );
                }

                if ( existingIconId.HasValue && existingIconId.Value != entity.FavIconBinaryFileId )
                {
                    var binaryFileService = new BinaryFileService( RockContext );
                    var binaryFile = binaryFileService.Get( existingIconId.Value );
                    if ( binaryFile != null )
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                        RockContext.SaveChanges();
                    }
                }

                if ( existingLogoId.HasValue && existingLogoId.Value != entity.SiteLogoBinaryFileId )
                {
                    var binaryFileService = new BinaryFileService( RockContext );
                    var binaryFile = binaryFileService.Get( existingLogoId.Value );
                    if ( binaryFile != null )
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                        RockContext.SaveChanges();
                    }
                }

                if ( isNew )
                {
                    Rock.Security.Authorization.CopyAuthorization( PageCache.Layout.Site, entity, RockContext, Rock.Security.Authorization.EDIT );
                    Rock.Security.Authorization.CopyAuthorization( PageCache.Layout.Site, entity, RockContext, Rock.Security.Authorization.ADMINISTRATE );
                    Rock.Security.Authorization.CopyAuthorization( PageCache.Layout.Site, entity, RockContext, Rock.Security.Authorization.APPROVE );
                }
            } );

            // add/update for the InteractionChannel for this site and set the RetentionPeriod
            var interactionChannelService = new InteractionChannelService( RockContext );
            int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            var interactionChannelForSite = interactionChannelService.Queryable()
                .Where( a => a.ChannelTypeMediumValueId == channelMediumWebsiteValueId && a.ChannelEntityId == entity.Id ).FirstOrDefault();

            if ( interactionChannelForSite == null )
            {
                interactionChannelForSite = new InteractionChannel();
                interactionChannelForSite.ChannelTypeMediumValueId = channelMediumWebsiteValueId;
                interactionChannelForSite.ChannelEntityId = entity.Id;
                interactionChannelService.Add( interactionChannelForSite );
            }

            interactionChannelForSite.Name = entity.Name;
            interactionChannelForSite.RetentionDuration = box.Bag.RetentionDuration;
            interactionChannelForSite.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.Page>().Id;

            RockContext.SaveChanges();

            // Create the default page is this is a new site
            if ( !entity.DefaultPageId.HasValue && isNew )
            {
                var siteCache = SiteCache.Get( entity.Id );
                var pageService = new PageService( RockContext );

                // Create the layouts for the site, and find the first one
                string applicationRootPath = HostingEnvironment.MapPath( "~" );
                LayoutService.RegisterLayouts( applicationRootPath, siteCache );

                var layoutService = new LayoutService( RockContext );
                var layouts = layoutService.GetBySiteId( siteCache.Id );
                var layout = layouts.FirstOrDefault( l => l.FileName.Equals( "FullWidth", StringComparison.OrdinalIgnoreCase ) );
                if ( layout == null )
                {
                    layout = layouts.FirstOrDefault();
                }

                if ( layout != null )
                {
                    var page = new Page();
                    page.LayoutId = layout.Id;
                    page.PageTitle = siteCache.Name + " Home Page";
                    page.InternalName = page.PageTitle;
                    page.BrowserTitle = page.PageTitle;
                    page.EnableViewState = true;
                    page.IncludeAdminFooter = true;
                    page.MenuDisplayChildPages = true;

                    var lastPage = pageService.GetByParentPageId( null ).OrderByDescending( b => b.Order ).FirstOrDefault();

                    page.Order = lastPage != null ? lastPage.Order + 1 : 0;
                    pageService.Add( page );

                    RockContext.SaveChanges();

                    entity = entityService.Get( siteCache.Id );
                    entity.DefaultPageId = page.Id;

                    RockContext.SaveChanges();
                }
            }

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );
            return ActionOk( new ValidPropertiesBox<SiteBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new SiteService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var pageService = new PageService( RockContext );
            var layoutService = new LayoutService( RockContext );

            var sitePages = new List<int> {
                entity.DefaultPageId ?? -1,
                entity.LoginPageId ?? -1,
                entity.RegistrationPageId ?? -1,
                entity.PageNotFoundPageId ?? -1
            };

            var otherSitesQry = entityService.Queryable().Where( s => s.Id != entity.Id );

            var pageQry = pageService.Queryable( "Layout" )
                .Where( t =>
                    !t.IsSystem &&
                    ( t.Layout.SiteId == entity.Id ||
                    sitePages.Contains( t.Id ) ) );

            pageQry = pageQry.Where( p => !otherSitesQry.Any( s => s.DefaultPageId == p.Id || s.LoginPageId == p.Id || s.RegistrationPageId == p.Id || s.PageNotFoundPageId == p.Id ) );

            foreach ( var page in pageQry )
            {
                if ( pageService.CanDelete( page, out string deletePageErrorMessage ) )
                {
                    pageService.Delete( page );
                }
            }

            var layoutQry = layoutService.Queryable()
                .Where( l =>
                    !l.IsSystem &&
                    l.SiteId == entity.Id );
            layoutService.DeleteRange( layoutQry );
            RockContext.SaveChanges( true );

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="attributeGuid">The attribute identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetAttribute( Guid? attributeGuid )
        {
            PublicEditableAttributeBag editableAttribute;
            string modalTitle;

            var entity = GetInitialEntity();
            var siteIdQualifierValue = entity.Id.ToString();
            var attributes = GetSiteAttributes( RockContext, siteIdQualifierValue );

            if ( !attributeGuid.HasValue )
            {
                editableAttribute = new PublicEditableAttributeBag
                {
                    FieldTypeGuid = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Guid
                };
                modalTitle = ActionTitle.Add( "attribute for pages of site " + entity.Name );
            }
            else
            {
                var attribute = attributes.FirstOrDefault( a => a.Guid == attributeGuid );
                editableAttribute = PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute );
                modalTitle = ActionTitle.Edit( "attribute for pages of site " + entity.Name );
            }

            var reservedKeyNames = new List<string>();
            attributes.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );

            return ActionOk( new { editableAttribute, reservedKeyNames, modalTitle } );
        }

        /// <summary>
        /// Handles the Click event of the CompileTheme button.
        /// </summary>
        [BlockAction]
        public BlockActionResult CompileTheme( string idKey )
        {
            var rockContext = new RockContext();
            SiteService siteService = new SiteService( rockContext );
            Site site = siteService.Get( idKey );

            if ( site == null )
            {
                return ActionBadRequest( "Unable to find the requested site." );
            }

            string messages;
            var theme = new Rock.Web.UI.RockTheme( site.Theme );
            bool success = theme.Compile( out messages );

            if ( success )
            {
                return ActionOk( new { message = "Theme was successfully compiled." } );
            }
            else
            {
                return ActionBadRequest( string.Format( "An error occurred compiling the theme {0}. Message: {1}.", site.Theme, messages ) );
            }
        }

        #endregion
    }
}
