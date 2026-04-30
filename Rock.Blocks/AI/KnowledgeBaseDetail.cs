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

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.AI.KnowledgeBaseDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.AI
{
    /// <summary>
    /// Displays the details of a particular knowledge base.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockEntityDetailBlockType{TEntity, TEntityBag}" />

    [DisplayName( "Knowledge Base Detail" )]
    [Category( "AI" )]
    [Description( "Displays the details of a particular knowledge base." )]
    [IconCssClass( "ti ti-book-2" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "5F2E7C8A-4B91-4D2E-A6F3-1B8C9D4E7F02" )]
    [Rock.SystemGuid.BlockTypeGuid( "8A3B6D1E-9C24-4F5A-B7C8-2E5D9F1A3B40" )]
    public class KnowledgeBaseDetail : RockEntityDetailBlockType<KnowledgeBase, KnowledgeBaseBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string KnowledgeBaseId = "KnowledgeBaseId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.KnowledgeBaseId );
            var pageParameters = new Dictionary<string, string>();

            var name = new KnowledgeBaseService( RockContext )
                .GetSelect( key, kb => kb.Name );

            if ( name != null )
            {
                pageParameters.Add( PageParameterKey.KnowledgeBaseId, key );
            }

            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( name ?? "New Knowledge Base", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
            };
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<KnowledgeBaseBag, KnowledgeBaseDetailOptionsBag>();

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
        private KnowledgeBaseDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            return new KnowledgeBaseDetailOptionsBag();
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<KnowledgeBaseBag, KnowledgeBaseDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {KnowledgeBase.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( KnowledgeBase.FriendlyTypeName );
                }
            }
            else
            {
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( KnowledgeBase.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="KnowledgeBaseBag"/> that represents the entity.</returns>
        private KnowledgeBaseBag GetCommonEntityBag( KnowledgeBase entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new KnowledgeBaseBag
            {
                IdKey = entity.IdKey,
                Name = entity.Name,
                Description = entity.Description,
                ContextHint = entity.ContextHint
            };
        }

        /// <inheritdoc/>
        protected override KnowledgeBaseBag GetEntityBagForView( KnowledgeBase entity )
        {
            return GetCommonEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override KnowledgeBaseBag GetEntityBagForEdit( KnowledgeBase entity )
        {
            return GetCommonEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( KnowledgeBase entity, ValidPropertiesBox<KnowledgeBaseBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.ContextHint ),
                () => entity.ContextHint = box.Bag.ContextHint );

            return true;
        }

        /// <inheritdoc/>
        protected override KnowledgeBase GetInitialEntity()
        {
            return GetInitialEntity<KnowledgeBase, KnowledgeBaseService>( RockContext, PageParameterKey.KnowledgeBaseId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out KnowledgeBase entity, out BlockActionResult error )
        {
            var entityService = new KnowledgeBaseService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                entity = new KnowledgeBase();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{KnowledgeBase.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {KnowledgeBase.FriendlyTypeName}." );
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

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<KnowledgeBaseBag>
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
        public BlockActionResult Save( ValidPropertiesBox<KnowledgeBaseBag> box )
        {
            var entityService = new KnowledgeBaseService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            var isNew = entity.Id == 0;

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.KnowledgeBaseId] = entity.IdKey
                } ) );
            }

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<KnowledgeBaseBag>
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
            var entityService = new KnowledgeBaseService( RockContext );

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
