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
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.SiteDetail;
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

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "88ce8a0b-35b6-4427-817f-2fdf485d0241" )]
    [Rock.SystemGuid.BlockTypeGuid( "3e935e45-4796-4389-ab1c-98d2403faedf" )]
    public class SiteDetail : RockDetailBlockType
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

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<SiteBag, SiteDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext, string.Empty );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Site>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="key">The id identifier of the entity.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private SiteDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext, string key )
        {
            var options = new SiteDetailOptionsBag();
            var attributes = GetSiteAttributes( rockContext, key );
            options.SiteAttributes = new List<PublicEditableAttributeBag>();
            options.SiteAttributes.AddRange( attributes.Select( attribute => PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute ) ) );
            return options;
        }

        /// <summary>
        /// Validates the Site for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="site">The Site to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Site is valid, <c>false</c> otherwise.</returns>
        private bool ValidateSite( Site site, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<SiteBag, SiteDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Site.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
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
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Site.FriendlyTypeName );
                }
            }
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
                EnablePageViewGeoTracking = entity.EnablePageViewGeoTracking,
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
                Theme = entity.Theme
            };

            if ( entity.SiteDomains != null )
            {
                bag.SiteDomains = string.Join( "\n", entity.SiteDomains.OrderBy( d => d.Order ).Select( d => d.Domain ).ToArray( ) );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="SiteBag"/> that represents the entity.</returns>
        private SiteBag GetEntityBagForView( Site entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="SiteBag"/> that represents the entity.</returns>
        private SiteBag GetEntityBagForEdit( Site entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( Site entity, DetailBlockBox<SiteBag, SiteDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.AllowedFrameDomains ),
                () => entity.AllowedFrameDomains = ReformatDomains( box.Entity.AllowedFrameDomains ) );

            box.IfValidProperty( nameof( box.Entity.AllowIndexing ),
                () => entity.AllowIndexing = box.Entity.AllowIndexing );

            box.IfValidProperty( nameof( box.Entity.ChangePasswordPage ),
                () => entity.ChangePasswordPageId = box.Entity.ChangePasswordPage.GetEntityId<Page>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.ChangePasswordPageRoute ),
                () => entity.ChangePasswordPageRouteId = box.Entity.ChangePasswordPageRoute.GetEntityId<PageRoute>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.CommunicationPage ),
                () => entity.CommunicationPageId = box.Entity.CommunicationPage.GetEntityId<Page>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.CommunicationPageRoute ),
                () => entity.CommunicationPageRouteId = box.Entity.CommunicationPageRoute.GetEntityId<PageRoute>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.DefaultPage ),
                () => entity.DefaultPageId = box.Entity.DefaultPage.GetEntityId<Page>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.DefaultPageRoute ),
                () => entity.DefaultPageRouteId = box.Entity.DefaultPageRoute.GetEntityId<PageRoute>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.DisablePredictableIds ),
                () => entity.DisablePredictableIds = box.Entity.DisablePredictableIds );

            box.IfValidProperty( nameof( box.Entity.EnabledForShortening ),
                () => entity.EnabledForShortening = box.Entity.EnabledForShortening );

            box.IfValidProperty( nameof( box.Entity.EnableExclusiveRoutes ),
                () => entity.EnableExclusiveRoutes = box.Entity.EnableExclusiveRoutes );

            box.IfValidProperty( nameof( box.Entity.EnableMobileRedirect ),
                () => entity.EnableMobileRedirect = box.Entity.EnableMobileRedirect );

            box.IfValidProperty( nameof( box.Entity.EnablePageViewGeoTracking ),
                () => entity.EnablePageViewGeoTracking = box.Entity.EnablePageViewGeoTracking );

            box.IfValidProperty( nameof( box.Entity.EnablePageViews ),
                () => entity.EnablePageViews = box.Entity.EnablePageViews );

            box.IfValidProperty( nameof( box.Entity.EnablePersonalization ),
                () => entity.EnablePersonalization = box.Entity.EnablePersonalization );

            box.IfValidProperty( nameof( box.Entity.EnableVisitorTracking ),
                () => entity.EnableVisitorTracking = box.Entity.EnableVisitorTracking );

            box.IfValidProperty( nameof( box.Entity.ErrorPage ),
                () => entity.ErrorPage = box.Entity.ErrorPage );

            box.IfValidProperty( nameof( box.Entity.ExternalUrl ),
                () => entity.ExternalUrl = box.Entity.ExternalUrl );

            box.IfValidProperty( nameof( box.Entity.FavIconBinaryFile ),
                () => entity.FavIconBinaryFileId = box.Entity.FavIconBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.GoogleAnalyticsCode ),
                () => entity.GoogleAnalyticsCode = box.Entity.GoogleAnalyticsCode );

            box.IfValidProperty( nameof( box.Entity.IndexStartingLocation ),
                () => entity.IndexStartingLocation = box.Entity.IndexStartingLocation );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.IsIndexEnabled ),
                () => entity.IsIndexEnabled = box.Entity.IsIndexEnabled );

            box.IfValidProperty( nameof( box.Entity.IsSystem ),
                () => entity.IsSystem = box.Entity.IsSystem );

            box.IfValidProperty( nameof( box.Entity.LoginPage ),
                () => entity.LoginPageId = box.Entity.LoginPage.GetEntityId<Page>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.LoginPageRoute ),
                () => entity.LoginPageRouteId = box.Entity.LoginPageRoute.GetEntityId<PageRoute>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.MobilePage ),
                () => entity.MobilePageId = box.Entity.MobilePage.GetEntityId<Page>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.PageHeaderContent ),
                () => entity.PageHeaderContent = box.Entity.PageHeaderContent );

            box.IfValidProperty( nameof( box.Entity.PageNotFoundPage ),
                () => entity.PageNotFoundPageId = box.Entity.PageNotFoundPage.GetEntityId<Page>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.PageNotFoundPageRoute ),
                () => entity.PageNotFoundPageRouteId = box.Entity.PageNotFoundPageRoute.GetEntityId<PageRoute>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.RedirectTablets ),
                () => entity.RedirectTablets = box.Entity.RedirectTablets );

            box.IfValidProperty( nameof( box.Entity.RegistrationPage ),
                () => entity.RegistrationPageId = box.Entity.RegistrationPage.GetEntityId<Page>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.RegistrationPageRoute ),
                () => entity.RegistrationPageRouteId = box.Entity.RegistrationPageRoute.GetEntityId<PageRoute>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.RequiresEncryption ),
                () => entity.RequiresEncryption = box.Entity.RequiresEncryption );

            box.IfValidProperty( nameof( box.Entity.SiteLogoBinaryFile ),
                () => entity.SiteLogoBinaryFileId = box.Entity.SiteLogoBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Theme ),
                () => entity.Theme = box.Entity.Theme );

            box.IfValidProperty( nameof( box.Entity.Theme ),
                () => entity.Theme = box.Entity.Theme );

            box.IfValidProperty( nameof( box.Entity.SiteDomains ),
                () => UpdateSiteDomains( entity, box, rockContext ) );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Site"/> to be viewed or edited on the page.</returns>
        private Site GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Site, SiteService>( rockContext, PageParameterKey.SiteId );
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

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( Site entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Site entity, out BlockActionResult error )
        {
            var entityService = new SiteService( rockContext );
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
        private void UpdateSiteDomains( Site entity, DetailBlockBox<SiteBag, SiteDetailOptionsBag> box, RockContext rockContext )
        {
            var siteDomainService = new SiteDomainService( rockContext );
            var currentDomains = box.Entity.SiteDomains.SplitDelimitedValues().ToList();
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
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var id = IdHasher.Instance.GetId( key );
                var box = new DetailBlockBox<SiteBag, SiteDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity ),
                    Options = GetBoxOptions( true, rockContext, id.ToString() )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<SiteBag, SiteDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new SiteService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                int? existingIconId = entity.FavIconBinaryFileId;
                int? existingLogoId = entity.SiteLogoBinaryFileId;

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateSite( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    if ( box.Options.SiteAttributes.Count > 0 )
                    {
                        SaveAttributes( new Page().TypeId, "SiteId", entity.Id.ToString(), box.Options.SiteAttributes, rockContext );
                    }

                    if ( existingIconId.HasValue && existingIconId.Value != entity.FavIconBinaryFileId )
                    {
                        var binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( existingIconId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = true;
                            rockContext.SaveChanges();
                        }
                    }

                    if ( existingLogoId.HasValue && existingLogoId.Value != entity.SiteLogoBinaryFileId )
                    {
                        var binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( existingLogoId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = true;
                            rockContext.SaveChanges();
                        }
                    }

                    if ( isNew )
                    {
                        Rock.Security.Authorization.CopyAuthorization( PageCache.Layout.Site, entity, rockContext, Rock.Security.Authorization.EDIT );
                        Rock.Security.Authorization.CopyAuthorization( PageCache.Layout.Site, entity, rockContext, Rock.Security.Authorization.ADMINISTRATE );
                        Rock.Security.Authorization.CopyAuthorization( PageCache.Layout.Site, entity, rockContext, Rock.Security.Authorization.APPROVE );
                    }
                } );

                // add/update for the InteractionChannel for this site and set the RetentionPeriod
                var interactionChannelService = new InteractionChannelService( rockContext );
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
                interactionChannelForSite.RetentionDuration = box.Entity.RetentionDuration;
                interactionChannelForSite.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.Page>().Id;

                rockContext.SaveChanges();

                // Create the default page is this is a new site
                if ( !entity.DefaultPageId.HasValue && isNew )
                {
                    var siteCache = SiteCache.Get( entity.Id );
                    var pageService = new PageService( rockContext );

                    // Create the layouts for the site, and find the first one
                    // LayoutService.RegisterLayouts( HttpRequest.MapPath( "~" ), siteCache );

                    var layoutService = new LayoutService( rockContext );
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

                        rockContext.SaveChanges();

                        entity = entityService.Get( siteCache.Id );
                        entity.DefaultPageId = page.Id;

                        rockContext.SaveChanges();
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
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new SiteService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var sitePages = new List<int> {
                    entity.DefaultPageId ?? -1,
                    entity.LoginPageId ?? -1,
                    entity.RegistrationPageId ?? -1,
                    entity.PageNotFoundPageId ?? -1
                };

                var pageService = new PageService( rockContext );
                foreach ( var page in pageService.Queryable( "Layout" )
                    .Where( t => !t.IsSystem && ( t.Layout.SiteId == entity.Id || sitePages.Contains( t.Id ) ) ) )
                {
                    if ( pageService.CanDelete( page, out string deletePageErrorMessage ) )
                    {
                        pageService.Delete( page );
                    }
                }

                var layoutService = new LayoutService( rockContext );
                var layoutQry = layoutService.Queryable()
                    .Where( l =>
                    l.SiteId == entity.Id );
                layoutService.DeleteRange( layoutQry );

                rockContext.SaveChanges( true );

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<SiteBag, SiteDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<SiteBag, SiteDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetAttribute( Guid? attributeGuid )
        {
            PublicEditableAttributeBag editableAttribute;
            string modalTitle;
            var rockContext = new RockContext();

            var entity = GetInitialEntity( rockContext );
            var siteIdQualifierValue = entity.Id.ToString();
            var attributes = GetSiteAttributes( rockContext, siteIdQualifierValue );

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

        #endregion
    }
}
