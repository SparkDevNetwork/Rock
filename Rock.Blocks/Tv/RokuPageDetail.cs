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
using Rock.Security;
using Rock.Tv;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Tv.AppleTvPageDetail;
using Rock.ViewModels.Blocks.Tv.RokuPageDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using static System.Net.Mime.MediaTypeNames;

namespace Rock.Blocks.Tv
{
    /// <summary>
    /// Displays the details of a particular page.
    /// </summary>

    [DisplayName( "Roku Page Detail" )]
    [Category( "TV > TV Apps" )]
    [Description( "Displays the details of a particular page." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "ddd1acc4-7fc4-42c8-b66d-64346c026fd1" )]
    [Rock.SystemGuid.BlockTypeGuid( "97c8a25d-8cb3-4662-8371-a37cc28b6f36" )]
    public class RokuPageDetail : RockEntityDetailBlockType<Page, RokuPageBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SitePageId = "SitePageId";
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
            var box = new DetailBlockBox<RokuPageBag, RokuPageDetailOptionsBag>();

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
        private RokuPageDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new RokuPageDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Page for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="page">The Page to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Page is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePage( Page page, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<RokuPageBag, RokuPageDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Page.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="PageBag"/> that represents the entity.</returns>
        private RokuPageBag GetCommonEntityBag( Page entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var additionalSettings = entity.GetAdditionalSettings<RokuTvPageSettings>();
            var cacheability = entity.CacheControlHeaderSettings.FromJsonOrNull<RockCacheability>();

            return new RokuPageBag
            {
                IdKey = entity.IdKey,
                ShowInMenu = entity.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed,
                InternalName = entity.InternalName,
                Description = entity.Description,
                Scenegraph = additionalSettings?.ScenegraphContent,
                RockCacheability = cacheability.ToCacheabilityBag(),
            };
        }

        /// <inheritdoc/>
        protected override RokuPageBag GetEntityBagForView( Page entity )
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
        protected override RokuPageBag GetEntityBagForEdit( Page entity )
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
        protected override bool UpdateEntityFromBox( Page entity, ValidPropertiesBox<RokuPageBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            //box.IfValidProperty( nameof( box.Bag.DisplayInNavWhen ),
            //    () => entity.DisplayInNavWhen = box.Bag./* TODO: Unknown property type 'DisplayInNavWhen' for conversion to bag. */ );

            box.IfValidProperty( nameof( box.Bag.InternalName ),
                () =>
                {
                    entity.InternalName = box.Bag.InternalName;
                    entity.PageTitle = box.Bag.InternalName;
                    entity.BrowserTitle = box.Bag.InternalName;
                } );

            box.IfValidProperty( nameof( box.Bag.ShowInMenu ), () =>
            {
                entity.DisplayInNavWhen = box.Bag.ShowInMenu ? DisplayInNavWhen.WhenAllowed : DisplayInNavWhen.Never;
            } );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.Scenegraph ),
                () =>
                {
                    var additionalSettings = entity.GetAdditionalSettings<RokuTvPageSettings>() ?? new RokuTvPageSettings();
                    additionalSettings.ScenegraphContent = box.Bag.Scenegraph;
                    entity.SetAdditionalSettings( additionalSettings );
                } );


            box.IfValidProperty( nameof( box.Bag.RockCacheability ),
                () => UpdateCacheability( box.Bag, entity ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );


            return true;
        }

        /// <inheritdoc/>
        protected override Page GetInitialEntity()
        {
            return GetInitialEntity<Page, PageService>( RockContext, PageParameterKey.SitePageId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParameters = new Dictionary<string, string>
            {
                [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId )
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParameters )
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
        /// Updates the <see cref="Page.CacheControlHeaderSettings"/> with the <see cref="RokuPageBag.RockCacheability"/>.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="entity">The entity.</param>
        private static void UpdateCacheability( RokuPageBag bag, Page entity )
        {
            var cacheability = bag.RockCacheability.ToCacheability();
            entity.CacheControlHeaderSettings = cacheability?.ToJson();
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

            return ActionOk( new ValidPropertiesBox<RokuPageBag>
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
        public BlockActionResult Save( ValidPropertiesBox<RokuPageBag> box )
        {
            var applicationId = PageParameter( PageParameterKey.SiteId );
            var entityService = new PageService( RockContext );
            var site = SiteCache.Get( applicationId, !PageCache.Layout.Site.DisablePredictableIds );

            if ( site == null )
            {
                return ActionBadRequest( "Please provide the Roku Application this page belongs to." );
            }

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidatePage( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            if ( isNew )
            {
                entity.ParentPageId = site.DefaultPageId;
                entity.LayoutId = site.DefaultPage.LayoutId;

                // Set the order of the new page to be the last one
                var currentMaxOrder = entityService.GetByParentPageId( site.DefaultPageId )
                    .OrderByDescending( p => p.Order )
                    .Select( p => p.Order )
                    .FirstOrDefault();
                entity.Order = currentMaxOrder + 1;
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
                RockContext.SaveChanges();
            } );

            RockContext.SaveChanges();

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            var parameters = new Dictionary<string, string>
            {
                [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId )
            };
            return ActionOk( this.GetParentPageUrl( parameters ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new PageService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
