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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Mobile.MobileLayoutDetail;
using Rock.ViewModels.Utility;
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
    [SupportedSiteTypes( SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "e83c989b-5ecb-4de4-b5bf-11af7fc2cca3" )]
    [SystemGuid.BlockTypeGuid( "c64f92cc-38a6-4562-8eae-d4f30b4af017" )]
    public class MobileLayoutDetail : RockEntityDetailBlockType<Layout, MobileLayoutBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LayoutId = "LayoutId";
            public const string SiteId = "SiteId";
            public const string Tab = "Tab";
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
            var box = new DetailBlockBox<MobileLayoutBag, MobileLayoutDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<MobileLayoutBag, MobileLayoutDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

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

                entity.LoadAttributes( RockContext );

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        box.Entity = GetEntityBag( entity );
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

            PrepareDetailBox( box, entity );
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

        /// <inheritdoc/>
        protected override MobileLayoutBag GetEntityBagForView( Layout entity )
        {
            return GetEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override MobileLayoutBag GetEntityBagForEdit( Layout entity )
        {
            return GetEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Layout entity, ValidPropertiesBox<MobileLayoutBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.LayoutMobilePhone ),
                () => entity.LayoutMobilePhone = box.Bag.LayoutMobilePhone );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.LayoutMobileTablet ),
                () => entity.LayoutMobileTablet = box.Bag.LayoutMobileTablet );

            return true;
        }

        /// <inheritdoc/>
        protected override Layout GetInitialEntity()
        {
            var layout = GetInitialEntity<Layout, LayoutService>( RockContext, PageParameterKey.LayoutId );

            if ( layout?.Id == 0 )
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
                    { PageParameterKey.SiteId, PageParameter( PageParameterKey.SiteId ) },
                    { PageParameterKey.Tab, "Layouts" }
                } )
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Layout entity, out BlockActionResult error )
        {
            var entityService = new LayoutService( RockContext );
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<MobileLayoutBag>
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
        public BlockActionResult Save( ValidPropertiesBox<MobileLayoutBag> box )
        {
            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            var isNew = entity.Id == 0;
            entity.FileName = box.Bag.Name + ".xaml";

            if ( isNew )
            {
                entity.SiteId = PageParameter( PageParameterKey.SiteId ).AsInteger();
            }

            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
            {
                { PageParameterKey.SiteId, PageParameter( PageParameterKey.SiteId ) },
                { PageParameterKey.Tab, "Layouts" }
            } ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new LayoutService( RockContext );

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

            return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
            {
                { PageParameterKey.SiteId, PageParameter( PageParameterKey.SiteId ) },
                { PageParameterKey.Tab, "Layouts" }
            } ) );
        }

        #endregion
    }
}
