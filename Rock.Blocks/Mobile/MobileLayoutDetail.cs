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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Mobile.MobileLayoutDetail;
using Rock.Web.Cache;

namespace Rock.Blocks.Mobile
{
    /// <summary>
    /// Displays the details of a particular layout.
    /// </summary>
    /// <seealso cref="RockDetailBlockType" />
    [DisplayName( "Mobile Layout Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of a mobile layout." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "e83c989b-5ecb-4de4-b5bf-11af7fc2cca3" )]
    [SystemGuid.BlockTypeGuid( "c64f92cc-38a6-4562-8eae-d4f30b4af017" )]
    public class MobileLayoutDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LayoutId = "LayoutId";
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
                var box = new DetailBlockBox<MobileLayoutBag, MobileLayoutDetailOptionsBag>();

                SetBoxInitialEntityState( box, true, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Layout>();

                return box;
            }
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<MobileLayoutBag, MobileLayoutDetailOptionsBag> box, bool loadAttributes, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity != null )
            {
                //
                // Ensure the layout is part of a mobile site.
                //
                if ( entity.Site != null && entity.Site.SiteType != SiteType.Mobile )
                {
                    box.ErrorMessage = "This block can only edit mobile layouts.";
                    return;
                }

                var isViewable = BlockCache.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
                box.IsEditable = BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

                if ( loadAttributes )
                {
                    entity.LoadAttributes( rockContext );
                }

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        box.Entity = GetEntityBag( entity );
                        box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Layout.FriendlyTypeName );
                    }
                }
                else
                {
                    // New entity is being created, prepare for edit mode by default.
                    if ( box.IsEditable )
                    {
                        box.Entity = GetEntityBag( entity );
                        box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Layout.FriendlyTypeName );
                    }
                }
            }
            else
            {
                box.ErrorMessage = $"The {Layout.FriendlyTypeName} was not found.";
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="MobileLayoutBag"/> that represents the entity.</returns>
        private MobileLayoutBag GetEntityBag( Layout entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new MobileLayoutBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                LayoutMobilePhone = entity.LayoutMobilePhone,
                Name = entity.Name,
                LayoutMobileTablet = entity.LayoutMobileTablet
            };
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( Layout entity, DetailBlockBox<MobileLayoutBag, MobileLayoutDetailOptionsBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.LayoutMobilePhone ),
                () => entity.LayoutMobilePhone = box.Entity.LayoutMobilePhone );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.LayoutMobileTablet ),
                () => entity.LayoutMobileTablet = box.Entity.LayoutMobileTablet );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Layout"/> to be viewed or edited on the page.</returns>
        private Layout GetInitialEntity( RockContext rockContext )
        {
            var layout = GetInitialEntity<Layout, LayoutService>( rockContext, PageParameterKey.LayoutId );

            if ( layout.Id == 0 )
            {
                var siteId = RequestContext.GetPageParameter( PageParameterKey.SiteId )?.AsIntegerOrNull();
                if ( siteId.HasValue )
                {
                    layout.SiteId = siteId.Value;
                }
            }

            return layout;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    { "SiteId", PageParameter( "SiteId" ) },
                    { "Tab", "Layouts" }
                } )
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
        /// <param name="entity">The entity being viewed or edited on this block.</param>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( IHasAttributes entity )
        {
            return new Rock.Security.SecurityGrant()
                .AddRulesForAttributes( entity, RequestContext.CurrentPerson )
                .ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Layout entity, out BlockActionResult error )
        {
            var entityService = new LayoutService( rockContext );
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
                entity = new Layout();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Layout.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Layout.FriendlyTypeName}." );
                return false;
            }

            return true;
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

                var box = new DetailBlockBox<MobileLayoutBag, MobileLayoutDetailOptionsBag>
                {
                    Entity = GetEntityBag( entity )
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
        public BlockActionResult Save( DetailBlockBox<MobileLayoutBag, MobileLayoutDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                var isNew = entity.Id == 0;
                entity.FileName = box.Entity.Name + ".xaml";

                if ( isNew )
                {
                    entity.SiteId = PageParameter( PageParameterKey.SiteId ).AsInteger();
                }

                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
                {
                    { "SiteId", PageParameter( "SiteId" ) },
                    { "Tab", "Layouts" }
                } ) );
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
                var entityService = new LayoutService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        #endregion
    }
}
