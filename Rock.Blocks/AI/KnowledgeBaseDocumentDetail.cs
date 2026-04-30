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
using Rock.Enums.AI;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.AI.KnowledgeBaseDocumentDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.AI
{
    /// <summary>
    /// Displays the details of a particular knowledge base document.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockEntityDetailBlockType{TEntity, TEntityBag}" />

    [DisplayName( "Knowledge Base Document Detail" )]
    [Category( "AI" )]
    [Description( "Displays the details of a particular knowledge base document." )]
    [IconCssClass( "ti ti-file-text" )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "6F2A4D8C-1B3E-4970-8D5A-7C9F4B2E1A30" )]
    [Rock.SystemGuid.BlockTypeGuid( "8D3C6F9E-2A4B-4D81-B7E5-3F1A8C9D4B62" )]
    public class KnowledgeBaseDocumentDetail : RockEntityDetailBlockType<KnowledgeBaseDocument, KnowledgeBaseDocumentBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string KnowledgeBaseDocumentId = "KnowledgeBaseDocumentId";
            public const string KnowledgeBaseFolderId = "KnowledgeBaseFolderId";
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
            var key = pageReference.GetPageParameter( PageParameterKey.KnowledgeBaseDocumentId );
            var pageParameters = new Dictionary<string, string>();
            var additionalParameters = new Dictionary<string, string>();

            var data = new KnowledgeBaseDocumentService( RockContext )
                .GetSelect( key, kbd => new
                {
                    kbd.Name,
                    kbd.KnowledgeBaseFolderId
                } );

            if ( data != null )
            {
                pageParameters.Add( PageParameterKey.KnowledgeBaseDocumentId, key );
                additionalParameters.Add( PageParameterKey.KnowledgeBaseFolderId, data.KnowledgeBaseFolderId.ToString() );
            }

            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( data?.Name ?? "New Knowledge Base Document", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb },
                AdditionalParameters = additionalParameters
            };
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<KnowledgeBaseDocumentBag, KnowledgeBaseDocumentDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls( GetInitialEntity() );

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Inherits security from
        /// the parent <see cref="KnowledgeBase"/> via the document's folder.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<KnowledgeBaseDocumentBag, KnowledgeBaseDocumentDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {KnowledgeBaseDocument.FriendlyTypeName} was not found.";
                return;
            }

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( KnowledgeBaseDocument.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( KnowledgeBaseDocument.FriendlyTypeName );
                }
            }

            box.Options = GetBoxOptions( entity );

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the box options. Carries the parent folder's source-binding
        /// metadata so the edit panel can render the correct Source Key
        /// picker for the document. For Content Channel folders, also
        /// projects the full set of Content Channel Items belonging to the
        /// parent folder's channel so the dropdown can render without an
        /// async fetch.
        /// </summary>
        /// <param name="entity">The document being viewed or edited.</param>
        /// <returns>A populated <see cref="KnowledgeBaseDocumentDetailOptionsBag"/>.</returns>
        private KnowledgeBaseDocumentDetailOptionsBag GetBoxOptions( KnowledgeBaseDocument entity )
        {
            var parentFolder = ResolveParentFolder( entity );
            var sourceEntityTypeId = parentFolder?.SourceEntityTypeId;
            var isContentChannelSource = IsContentChannelSource( sourceEntityTypeId );

            var options = new KnowledgeBaseDocumentDetailOptionsBag
            {
                SourceTypeName = GetSourceTypeName( sourceEntityTypeId ),
                IsContentChannelSource = isContentChannelSource
            };

            // For Content Channel folders, project all items belonging to the
            // parent folder's channel so the dropdown can render statically.
            // The folder's source binding is read-only after creation, so this
            // option set is stable for the lifetime of the block instance.
            if ( isContentChannelSource && parentFolder?.SourceKey.IsNotNullOrWhiteSpace() == true
                && int.TryParse( parentFolder.SourceKey, out var contentChannelId ) )
            {
                options.ContentChannelItemOptions = new ContentChannelItemService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( cci => cci.ContentChannelId == contentChannelId )
                    .OrderBy( cci => cci.Title )
                    .Select( cci => new
                    {
                        cci.Id,
                        cci.Title
                    } )
                    .ToList()
                    .Select( cci => new ListItemBag
                    {
                        Value = cci.Id.ToString(),
                        Text = cci.Title
                    } )
                    .ToList();
            }

            return options;
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The document to be represented as a bag.</param>
        /// <returns>A <see cref="KnowledgeBaseDocumentBag"/>.</returns>
        private KnowledgeBaseDocumentBag GetCommonEntityBag( KnowledgeBaseDocument entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new KnowledgeBaseDocumentBag
            {
                IdKey = entity.IdKey,
                Name = entity.Name,
                SourceName = entity.SourceName,
                SourceKey = entity.SourceKey,
                DocumentKey = entity.DocumentKey,
                Url = entity.Url,
                BinaryFile = entity.BinaryFile.ToListItemBag(),
                Content = entity.Content,
                IndexStatus = entity.IndexStatus,
                IndexDateTime = entity.IndexDateTime,
                IsIndexDirty = entity.IsIndexDirty,
                KnowledgeBaseFolder = entity.KnowledgeBaseFolder.ToListItemBag()
            };

            // For Content Channel documents, hydrate the picker's value from
            // the entity's SourceKey (a stringified item Id). The dropdown
            // binds Value = item Id (as string), Text = item Title, so the
            // hydrate side mirrors that shape. Manual documents skip this
            // and the bag's text-field SourceKey/SourceName are used directly.
            var parentFolder = ResolveParentFolder( entity );
            if ( IsContentChannelSource( parentFolder?.SourceEntityTypeId )
                && entity.SourceKey.IsNotNullOrWhiteSpace()
                && int.TryParse( entity.SourceKey, out var contentChannelItemId ) )
            {
                var itemTitle = new ContentChannelItemService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( cci => cci.Id == contentChannelItemId )
                    .Select( cci => cci.Title )
                    .FirstOrDefault();

                if ( itemTitle != null )
                {
                    bag.SourceContentChannelItem = new ListItemBag
                    {
                        Value = entity.SourceKey,
                        Text = itemTitle
                    };
                }
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override KnowledgeBaseDocumentBag GetEntityBagForView( KnowledgeBaseDocument entity )
        {
            return GetCommonEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override KnowledgeBaseDocumentBag GetEntityBagForEdit( KnowledgeBaseDocument entity )
        {
            return GetCommonEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( KnowledgeBaseDocument entity, ValidPropertiesBox<KnowledgeBaseDocumentBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Url ),
                () => entity.Url = box.Bag.Url );

            box.IfValidProperty( nameof( box.Bag.BinaryFile ),
                () => entity.BinaryFileId = box.Bag.BinaryFile.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Content ),
                () => entity.Content = box.Bag.Content );

            // SourceKey/SourceName branch on the parent folder's source kind.
            // Content Channel documents derive both from the picked dropdown
            // item: Value (item Id as string) → SourceKey; Text (item Title)
            // → SourceName cache. Manual documents take both as free text.
            var parentFolder = ResolveParentFolder( entity );
            if ( IsContentChannelSource( parentFolder?.SourceEntityTypeId ) )
            {
                box.IfValidProperty( nameof( box.Bag.SourceContentChannelItem ),
                    () =>
                    {
                        var picked = box.Bag.SourceContentChannelItem;
                        if ( picked?.Value.IsNotNullOrWhiteSpace() == true )
                        {
                            entity.SourceKey = picked.Value;
                            entity.SourceName = picked.Text;
                        }
                        else
                        {
                            entity.SourceKey = null;
                            entity.SourceName = null;
                        }
                    } );
            }
            else
            {
                box.IfValidProperty( nameof( box.Bag.SourceName ),
                    () => entity.SourceName = box.Bag.SourceName );

                box.IfValidProperty( nameof( box.Bag.SourceKey ),
                    () => entity.SourceKey = box.Bag.SourceKey );
            }

            // DocumentKey, IndexStatus, IndexDateTime, and IsIndexDirty are
            // managed by the sync worker and intentionally not writable from
            // the bag in v1.

            return true;
        }

        /// <inheritdoc/>
        protected override KnowledgeBaseDocument GetInitialEntity()
        {
            var entity = GetInitialEntity<KnowledgeBaseDocument, KnowledgeBaseDocumentService>( RockContext, PageParameterKey.KnowledgeBaseDocumentId );

            // Bootstrap parent folder for new documents from the URL so the
            // document inherits its folder context.
            if ( entity != null && entity.Id == 0 )
            {
                var parentKey = PageParameter( PageParameterKey.KnowledgeBaseFolderId );
                if ( parentKey.IsNotNullOrWhiteSpace() )
                {
                    var parentFolder = new KnowledgeBaseFolderService( RockContext )
                        .Get( parentKey, !PageCache.Layout.Site.DisablePredictableIds );

                    if ( parentFolder != null )
                    {
                        entity.KnowledgeBaseFolderId = parentFolder.Id;
                    }
                }

                // New documents start in Pending and dirty so the sync worker
                // picks them up on the next pass. The DocumentKey column is
                // Required at the schema level but is owned by the indexing
                // service (Ragie) and not populated until the document is
                // accepted. Seed it with the entity's Guid as a placeholder
                // so manual seeding can save the row; the sync worker will
                // overwrite it with Ragie's real document id on first
                // successful index.
                entity.IndexStatus = IndexStatus.Pending;
                entity.IsIndexDirty = true;
                entity.DocumentKey = entity.Guid.ToString();
            }

            return entity;
        }

        /// <summary>
        /// Resolves the parent <see cref="KnowledgeBase"/> for the document
        /// via its folder. Used for security inheritance.
        /// </summary>
        /// <param name="entity">The document being viewed or edited.</param>
        /// <returns>The cached parent knowledge base, or null.</returns>
        private KnowledgeBaseCache ResolveParentKnowledgeBase( KnowledgeBaseDocument entity )
        {
            if ( entity.KnowledgeBaseFolderId <= 0 )
            {
                return null;
            }

            var folderCache = KnowledgeBaseFolderCache.Get( entity.KnowledgeBaseFolderId );

            if ( folderCache != null )
            {
                return KnowledgeBaseCache.Get( folderCache.KnowledgeBaseId );
            }

            // Fall back to the entity's eager-loaded folder if cache lookup
            // fails (e.g., the folder was just created in this same context).
            var folder = entity.KnowledgeBaseFolder
                ?? new KnowledgeBaseFolderService( RockContext ).Get( entity.KnowledgeBaseFolderId );

            return folder != null ? KnowledgeBaseCache.Get( folder.KnowledgeBaseId ) : null;
        }

        /// <summary>
        /// Resolves the parent <see cref="KnowledgeBaseFolder"/> via the
        /// folder cache so the document detail block can read the folder's
        /// source-binding metadata (SourceEntityTypeId, SourceKey) without an
        /// extra query.
        /// </summary>
        /// <param name="entity">The document being viewed or edited.</param>
        /// <returns>The cached parent folder, or null.</returns>
        private KnowledgeBaseFolderCache ResolveParentFolder( KnowledgeBaseDocument entity )
        {
            if ( entity == null || entity.KnowledgeBaseFolderId <= 0 )
            {
                return null;
            }

            return KnowledgeBaseFolderCache.Get( entity.KnowledgeBaseFolderId );
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
        /// Gets a friendly display label for a source entity type. Returns
        /// "Manual" when no source entity type is bound.
        /// </summary>
        /// <param name="sourceEntityTypeId">The parent folder's source entity type identifier, if any.</param>
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
        /// Gets the box navigation URLs required for the page to operate.
        /// The parent page (Knowledge Base Folder Detail) needs the parent
        /// folder's page parameter so it knows which folder to display when
        /// the user navigates back from this document detail.
        /// </summary>
        /// <param name="entity">The document being viewed or edited, used to resolve the parent folder.</param>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( KnowledgeBaseDocument entity )
        {
            var queryParams = new Dictionary<string, string>();

            if ( entity != null && entity.KnowledgeBaseFolderId > 0 )
            {
                var parentFolderCache = KnowledgeBaseFolderCache.Get( entity.KnowledgeBaseFolderId );
                if ( parentFolderCache != null )
                {
                    queryParams[PageParameterKey.KnowledgeBaseFolderId] = parentFolderCache.IdKey;
                }
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams )
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out KnowledgeBaseDocument entity, out BlockActionResult error )
        {
            var entityService = new KnowledgeBaseDocumentService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                entity = new KnowledgeBaseDocument
                {
                    IndexStatus = IndexStatus.Pending,
                    IsIndexDirty = true
                };

                // The DocumentKey column is Required at the schema level but
                // is owned by the indexing service (Ragie) and not populated
                // until the document is accepted. Seed with the entity's
                // Guid; the sync worker will overwrite with Ragie's real
                // document id on first successful index.
                entity.DocumentKey = entity.Guid.ToString();

                var parentKey = PageParameter( PageParameterKey.KnowledgeBaseFolderId );
                if ( parentKey.IsNotNullOrWhiteSpace() )
                {
                    var parentFolder = new KnowledgeBaseFolderService( RockContext )
                        .Get( parentKey, !PageCache.Layout.Site.DisablePredictableIds );

                    if ( parentFolder == null )
                    {
                        error = ActionBadRequest( "A parent knowledge base folder is required to create a document." );
                        entity = null;
                        return false;
                    }

                    entity.KnowledgeBaseFolderId = parentFolder.Id;
                }
                else
                {
                    error = ActionBadRequest( "A parent knowledge base folder is required to create a document." );
                    entity = null;
                    return false;
                }

                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{KnowledgeBaseDocument.FriendlyTypeName} not found." );
                return false;
            }

            var parentKnowledgeBase = ResolveParentKnowledgeBase( entity );
            if ( parentKnowledgeBase == null || !parentKnowledgeBase.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {KnowledgeBaseDocument.FriendlyTypeName}." );
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

            return ActionOk( new ValidPropertiesBox<KnowledgeBaseDocumentBag>
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
        public BlockActionResult Save( ValidPropertiesBox<KnowledgeBaseDocumentBag> box )
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

            // Mark the document dirty whenever an admin edits it so the
            // sync worker re-indexes on its next pass.
            entity.IsIndexDirty = true;

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.KnowledgeBaseDocumentId] = entity.IdKey
                } ) );
            }

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<KnowledgeBaseDocumentBag>
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
            var entityService = new KnowledgeBaseDocumentService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Resolve the parent folder before deleting so we can route the
            // user back to the parent folder's detail page after the document
            // is gone.
            var parentNavigationUrls = GetBoxNavigationUrls( entity );

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( parentNavigationUrls[NavigationUrlKey.ParentPage] );
        }

        #endregion
    }
}
