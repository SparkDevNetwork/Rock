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

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.AI.KnowledgeBaseFolderDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.AI
{
    /// <summary>
    /// Displays the details of a particular knowledge base folder. The Source
    /// Key picker is rendered conditionally based on the folder's source kind
    /// (Content Channel, Manual, etc.).
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockEntityDetailBlockType{TEntity, TEntityBag}" />

    [DisplayName( "Knowledge Base Folder Detail" )]
    [Category( "AI" )]
    [Description( "Displays the details of a particular knowledge base folder." )]
    [IconCssClass( "ti ti-folder" )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "1B8D6F2A-3E47-4C95-A8F3-2D9C7E6B4A18" )]
    [Rock.SystemGuid.BlockTypeGuid( "4E7A0C1F-58B6-4D32-A917-6F8D2E5C3B49" )]
    public class KnowledgeBaseFolderDetail : RockEntityDetailBlockType<KnowledgeBaseFolder, KnowledgeBaseFolderBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string KnowledgeBaseFolderId = "KnowledgeBaseFolderId";
            public const string KnowledgeBaseId = "KnowledgeBaseId";
            public const string SourceEntityTypeId = "SourceEntityTypeId";
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
            var key = pageReference.GetPageParameter( PageParameterKey.KnowledgeBaseFolderId );
            var pageParameters = new Dictionary<string, string>();
            var additionalParameters = new Dictionary<string, string>();

            var data = new KnowledgeBaseFolderService( RockContext )
                .GetSelect( key, kbf => new
                {
                    kbf.Name,
                    kbf.KnowledgeBaseId
                } );

            if ( data != null )
            {
                pageParameters.Add( PageParameterKey.KnowledgeBaseFolderId, key );
                additionalParameters.Add( PageParameterKey.KnowledgeBaseId, data.KnowledgeBaseId.ToString() );
            }

            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( data?.Name ?? "New Knowledge Base Folder", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb },
                AdditionalParameters = additionalParameters
            };
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<KnowledgeBaseFolderBag, KnowledgeBaseFolderDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls( GetInitialEntity() );

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Inherits security from
        /// the parent <see cref="KnowledgeBase"/>.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<KnowledgeBaseFolderBag, KnowledgeBaseFolderDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {KnowledgeBaseFolder.FriendlyTypeName} was not found.";
                return;
            }

            // Folders are not secured directly; their security is inherited
            // from the parent KnowledgeBase. Resolve the parent and gate
            // visibility/editability against it.
            var parentKnowledgeBase = ResolveParentKnowledgeBase( entity );

            if ( parentKnowledgeBase == null )
            {
                box.ErrorMessage = $"The parent {KnowledgeBase.FriendlyTypeName} was not found.";
                return;
            }

            var currentPerson = RequestContext.CurrentPerson;
            var isViewable = parentKnowledgeBase.IsAuthorized( Authorization.VIEW, currentPerson );
            box.IsEditable = parentKnowledgeBase.IsAuthorized( Authorization.EDIT, currentPerson );

            if ( entity.Id != 0 )
            {
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( KnowledgeBaseFolder.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( KnowledgeBaseFolder.FriendlyTypeName );
                }
            }

            box.Options = GetBoxOptions( entity );

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the box options. Populates the source-type label and the
        /// content channel options when the folder's source kind matches.
        /// </summary>
        /// <param name="entity">The folder being viewed or edited.</param>
        /// <returns>A populated <see cref="KnowledgeBaseFolderDetailOptionsBag"/>.</returns>
        private KnowledgeBaseFolderDetailOptionsBag GetBoxOptions( KnowledgeBaseFolder entity )
        {
            var isContentChannelSource = IsContentChannelSource( entity.SourceEntityTypeId );

            var options = new KnowledgeBaseFolderDetailOptionsBag
            {
                SourceTypeName = GetSourceTypeName( entity.SourceEntityTypeId ),
                IsContentChannelSource = isContentChannelSource
            };

            if ( isContentChannelSource )
            {
                options.ContentChannelOptions = new ContentChannelService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .OrderBy( cc => cc.Name )
                    .Select( cc => new
                    {
                        cc.Id,
                        cc.Name
                    } )
                    .ToList()
                    .Select( cc => new ListItemBag
                    {
                        Value = cc.Id.ToString(),
                        Text = cc.Name
                    } )
                    .ToList();
            }

            return options;
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The folder to be represented as a bag.</param>
        /// <returns>A <see cref="KnowledgeBaseFolderBag"/> that represents the entity.</returns>
        private KnowledgeBaseFolderBag GetCommonEntityBag( KnowledgeBaseFolder entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new KnowledgeBaseFolderBag
            {
                IdKey = entity.IdKey,
                Name = entity.Name,
                Description = entity.Description,
                ContextHint = entity.ContextHint,
                SourceEntityTypeId = entity.SourceEntityTypeId
            };

            // Hydrate the parent KB ListItemBag so the view panel can show it
            // without an extra navigation property load on the client.
            var parentKnowledgeBase = entity.KnowledgeBaseId > 0
                ? KnowledgeBaseCache.Get( entity.KnowledgeBaseId )
                : null;

            if ( parentKnowledgeBase != null )
            {
                bag.KnowledgeBase = new ListItemBag
                {
                    Value = parentKnowledgeBase.Guid.ToString(),
                    Text = parentKnowledgeBase.Name
                };
            }

            // Resolve the picked content channel for Content Channel folders
            // by reading the entity's SourceKey (a stringified channel Id).
            if ( IsContentChannelSource( entity.SourceEntityTypeId ) && entity.SourceKey.IsNotNullOrWhiteSpace() )
            {
                if ( int.TryParse( entity.SourceKey, out var contentChannelId ) )
                {
                    var channelName = new ContentChannelService( RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( cc => cc.Id == contentChannelId )
                        .Select( cc => cc.Name )
                        .FirstOrDefault();

                    if ( channelName != null )
                    {
                        bag.SourceContentChannel = new ListItemBag
                        {
                            Value = contentChannelId.ToString(),
                            Text = channelName
                        };
                    }
                }
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override KnowledgeBaseFolderBag GetEntityBagForView( KnowledgeBaseFolder entity )
        {
            return GetCommonEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override KnowledgeBaseFolderBag GetEntityBagForEdit( KnowledgeBaseFolder entity )
        {
            return GetCommonEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( KnowledgeBaseFolder entity, ValidPropertiesBox<KnowledgeBaseFolderBag> box )
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

            // SourceEntityTypeId is fixed at create time. Only allow the
            // initial create-time value to be written through; subsequent
            // saves leave it untouched even if the bag tries to change it.
            if ( entity.Id == 0 )
            {
                box.IfValidProperty( nameof( box.Bag.SourceEntityTypeId ),
                    () => entity.SourceEntityTypeId = box.Bag.SourceEntityTypeId );
            }

            // SourceKey is derived from the source-type-specific picker. Read
            // only the picker that matches the entity's source kind.
            if ( IsContentChannelSource( entity.SourceEntityTypeId ) )
            {
                box.IfValidProperty( nameof( box.Bag.SourceContentChannel ),
                    () => entity.SourceKey = box.Bag.SourceContentChannel?.Value );
            }
            else
            {
                // Manual or unbound folders carry no source key.
                entity.SourceKey = null;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override KnowledgeBaseFolder GetInitialEntity()
        {
            var entity = GetInitialEntity<KnowledgeBaseFolder, KnowledgeBaseFolderService>( RockContext, PageParameterKey.KnowledgeBaseFolderId );

            // For add mode, bootstrap the parent KnowledgeBaseId and the
            // optional SourceEntityTypeId from the URL so the conditional
            // picker can render correctly on first paint.
            if ( entity != null && entity.Id == 0 )
            {
                var parentKey = PageParameter( PageParameterKey.KnowledgeBaseId );
                var parentKnowledgeBase = ResolveParentKnowledgeBaseFromKey( parentKey );

                if ( parentKnowledgeBase != null )
                {
                    entity.KnowledgeBaseId = parentKnowledgeBase.Id;
                }

                var sourceEntityTypeIdParam = PageParameter( PageParameterKey.SourceEntityTypeId );
                if ( sourceEntityTypeIdParam.IsNotNullOrWhiteSpace() && int.TryParse( sourceEntityTypeIdParam, out var sourceEntityTypeId ) )
                {
                    entity.SourceEntityTypeId = sourceEntityTypeId;
                }
            }

            return entity;
        }

        /// <summary>
        /// Resolves the parent <see cref="KnowledgeBase"/> for an existing
        /// folder, or for a new folder the parent comes from the URL.
        /// </summary>
        /// <param name="entity">The folder being viewed or edited.</param>
        /// <returns>The cached parent knowledge base, or null if it cannot be resolved.</returns>
        private KnowledgeBaseCache ResolveParentKnowledgeBase( KnowledgeBaseFolder entity )
        {
            if ( entity.KnowledgeBaseId > 0 )
            {
                return KnowledgeBaseCache.Get( entity.KnowledgeBaseId );
            }

            return ResolveParentKnowledgeBaseFromKey( PageParameter( PageParameterKey.KnowledgeBaseId ) );
        }

        /// <summary>
        /// Resolves a <see cref="KnowledgeBase"/> from a URL key (Id, IdKey, or Guid).
        /// </summary>
        /// <param name="key">The page parameter value.</param>
        /// <returns>The cached knowledge base, or null.</returns>
        private KnowledgeBaseCache ResolveParentKnowledgeBaseFromKey( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var disablePredictableIds = PageCache.Layout.Site.DisablePredictableIds;

            if ( !disablePredictableIds && int.TryParse( key, out var parsedId ) )
            {
                var byId = KnowledgeBaseCache.Get( parsedId );
                if ( byId != null )
                {
                    return byId;
                }
            }

            if ( Guid.TryParse( key, out var parsedGuid ) )
            {
                var byGuid = KnowledgeBaseCache.Get( parsedGuid );
                if ( byGuid != null )
                {
                    return byGuid;
                }
            }

            return KnowledgeBaseCache.GetByIdKey( key );
        }

        /// <summary>
        /// Gets a friendly display label for a source entity type. Returns
        /// "Manual" when no source entity type is bound.
        /// </summary>
        /// <param name="sourceEntityTypeId">The folder's source entity type identifier, if any.</param>
        /// <returns>A display label for the source type.</returns>
        private string GetSourceTypeName( int? sourceEntityTypeId )
        {
            if ( !sourceEntityTypeId.HasValue )
            {
                return "Manual";
            }

            var entityType = EntityTypeCache.Get( sourceEntityTypeId.Value );

            return entityType?.FriendlyName ?? "Unknown";
        }

        /// <summary>
        /// Determines whether a source entity type identifier points at the
        /// Content Channel entity type.
        /// </summary>
        /// <param name="sourceEntityTypeId">The candidate identifier.</param>
        /// <returns><c>true</c> if the identifier resolves to the Content Channel entity type.</returns>
        private bool IsContentChannelSource( int? sourceEntityTypeId )
        {
            if ( !sourceEntityTypeId.HasValue )
            {
                return false;
            }

            var contentChannelEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.CONTENT_CHANNEL.AsGuid() );

            return contentChannelEntityType != null && contentChannelEntityType.Id == sourceEntityTypeId.Value;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// The parent page (Knowledge Base Detail) needs the parent KB's
        /// page parameter so it knows which knowledge base to display when
        /// the user navigates back from this detail.
        /// </summary>
        /// <param name="entity">The folder being viewed or edited, used to resolve the parent KB.</param>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( KnowledgeBaseFolder entity )
        {
            var queryParams = new Dictionary<string, string>();

            var parentKnowledgeBase = entity != null ? ResolveParentKnowledgeBase( entity ) : null;
            if ( parentKnowledgeBase != null )
            {
                queryParams[PageParameterKey.KnowledgeBaseId] = parentKnowledgeBase.IdKey;
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams )
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out KnowledgeBaseFolder entity, out BlockActionResult error )
        {
            var entityService = new KnowledgeBaseFolderService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // New folder. Bootstrap parent KB and source type from URL
                // so the entity carries enough state for UpdateEntityFromBox
                // to validate against.
                entity = new KnowledgeBaseFolder();

                var parentKey = PageParameter( PageParameterKey.KnowledgeBaseId );
                var parentKnowledgeBase = ResolveParentKnowledgeBaseFromKey( parentKey );

                if ( parentKnowledgeBase == null )
                {
                    error = ActionBadRequest( "A parent knowledge base is required to create a folder." );
                    entity = null;
                    return false;
                }

                entity.KnowledgeBaseId = parentKnowledgeBase.Id;

                var sourceEntityTypeIdParam = PageParameter( PageParameterKey.SourceEntityTypeId );
                if ( sourceEntityTypeIdParam.IsNotNullOrWhiteSpace() && int.TryParse( sourceEntityTypeIdParam, out var sourceEntityTypeId ) )
                {
                    entity.SourceEntityTypeId = sourceEntityTypeId;
                }

                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{KnowledgeBaseFolder.FriendlyTypeName} not found." );
                return false;
            }

            var parentKb = ResolveParentKnowledgeBase( entity );
            if ( parentKb == null || !parentKb.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {KnowledgeBaseFolder.FriendlyTypeName}." );
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

            return ActionOk( new ValidPropertiesBox<KnowledgeBaseFolderBag>
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
        public BlockActionResult Save( ValidPropertiesBox<KnowledgeBaseFolderBag> box )
        {
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
                    [PageParameterKey.KnowledgeBaseFolderId] = entity.IdKey
                } ) );
            }

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<KnowledgeBaseFolderBag>
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
            var entityService = new KnowledgeBaseFolderService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Resolve the parent KB before deleting so we can route the user
            // back to the parent KB's detail page after the folder is gone.
            var parentNavigationUrls = GetBoxNavigationUrls( entity );

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( parentNavigationUrls[NavigationUrlKey.ParentPage] );
        }

        #endregion
    }
}
