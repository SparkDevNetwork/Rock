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
using Rock.Cms.StructuredContent;
using Rock.Constants;
using Rock.Model;
using Rock.Model.CMS.ContentChannelItem.Options;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Tasks;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentChannelItemDetail;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details for a content channel item.
    /// </summary>
    [DisplayName( "Content Channel Item Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details for a content channel item." )]
    [IconCssClass( "ti ti-note" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [ContentChannelField( "Content Channel",
        Description = "If set the block will ignore content channel query parameters",
        IsRequired = false,
        Key = AttributeKey.ContentChannel,
        Order = 0 )]

    [LinkedPage( "Event Occurrence Page",
        Key = AttributeKey.EventOccurrencePage,
        IsRequired = false,
        Order = 1 )]

    [BooleanField( "Show Delete Button",
        Description = "Shows a delete button for the current item.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowDeleteButton,
        Order = 2 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "AAB99814-B76A-4F45-91AD-DCA659F00E99" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "3DD206BC-8076-47F7-AA63-9C2F2492C989" )]
    [Rock.SystemGuid.BlockTypeGuid( "5B99687B-5FE9-4EE2-8679-5040CAEB9E2E" )]
    public class ContentChannelItemDetail : RockEntityDetailBlockType<ContentChannelItem, ContentChannelItemBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EventOccurrencePage = "EventOccurrencePage";
            public const string ShowDeleteButton = "ShowDeleteButton";
            public const string ContentChannel = "ContentChannel";
        }

        private static class PageParameterKey
        {
            public const string ContentItemId = "ContentItemId";
            public const string ContentChannelId = "ContentChannelId";
            public const string ReturnUrl = "returnUrl";
            public const string Hierarchy = "Hierarchy";
            public const string EventItemOccurrenceId = "EventItemOccurrenceId";
            public const string EventCalendarId = "EventCalendarId";
            public const string EventItemId = "EventItemId";
            public const string AutoEdit = "autoEdit";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Fields

        private static readonly string NoContentChannelErrorMessage = "No content channel could be resolved. Supply a ContentChannelId page parameter or set the block's Content Channel setting.";

        /// <summary>
        /// Cached content channel for the current request. Set on first entity resolution; null when no channel resolves.
        /// </summary>
        private ContentChannelCache _contentChannelCache;

        /// <summary>
        /// The EntityTypeId for ContentChannelItem, used by the personalization queries.
        /// </summary>
        private int ContentChannelItemEntityTypeId => EntityTypeCache.Get<ContentChannelItem>().Id;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<ContentChannelItemBag, ContentChannelItemDetailOptionsBag>();
            var entity = GetInitialEntity();

            SetBoxInitialEntityState( box, entity );

            if ( box.Entity != null )
            {
                box.Options = GetBoxOptions( entity );
            }

            box.NavigationUrls = GetBoxNavigationUrls( entity );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view or edit the entity.
        /// </summary>
        private ContentChannelItemDetailOptionsBag GetBoxOptions( ContentChannelItem entity )
        {
            var contentChannelType = entity != null
                ? ContentChannelTypeCache.Get( entity.ContentChannelTypeId )
                : null;

            // The grouped Topic options travel only when the library is enabled (the
            // flag also gates the whole library region client-side); a non-library
            // channel ships null and the partial coalesces it to an empty list.
            var isContentLibraryEnabled = IsChannelContentLibraryEnabled();
            var isContentIntentShown = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.INTERACTION_INTENT.AsGuid() ) != null;
            var isPersonalizationShown = _contentChannelCache?.EnablePersonalization ?? false;

            var options = new ContentChannelItemDetailOptionsBag
            {
                ChannelName = entity?.ContentChannel?.Name,
                IncludeTime = contentChannelType?.IncludeTime ?? true,
                DateRangeType = contentChannelType?.DateRangeType ?? ContentChannelDateType.SingleDate,
                IsPriorityHidden = contentChannelType?.DisablePriority ?? false,
                OccurrenceLabels = GetOccurrenceLabels( entity ),
                IsApprovalToggleShown = IsApprovalToggleShown( entity ),
                IsContentLibraryEnabled = isContentLibraryEnabled,
                ContentTopics = isContentLibraryEnabled ? GetContentTopicListItems() : null,
                IsDeleteButtonShown = GetAttributeValue( AttributeKey.ShowDeleteButton ).AsBoolean(),
                IsPersonalizationShown = isPersonalizationShown,
                IsContentIntentShown = isContentIntentShown,
                IntentOptions = isContentIntentShown ? BuildIntentOptions( entity ) : null,
                IsTaggingShown = _contentChannelCache?.IsTaggingEnabled ?? false,
                TagCategoryGuid = _contentChannelCache?.ItemTagCategory?.Guid.ToString(),
                ChildItemsGridDefinition = GetChildItemsGridBuilder().WithLaunchWorkflow( this ).BuildDefinition(),
                ParentItemsGridDefinition = GetParentItemsGridBuilder().WithLaunchWorkflow( this ).BuildDefinition(),
                AddChildChannelOptions = BuildAddChildChannelOptions()
            };

            // Personalization sets two option lists on the bag, so it runs after construction.
            if ( isPersonalizationShown )
            {
                BuildPersonalizationOptions( options );
            }

            return options;
        }

        /// <inheritdoc/>
        protected override ContentChannelItem GetInitialEntity()
        {
            var entity = GetInitialEntity<ContentChannelItem, ContentChannelItemService>( RockContext, PageParameterKey.ContentItemId );

            if ( entity != null && entity.Id != 0 )
            {
                _contentChannelCache = ContentChannelCache.Get( entity.ContentChannelId );

                return entity;
            }

            entity = entity ?? new ContentChannelItem();

            ApplyNewContentChannelItemDefaultValues( entity );

            // Without a resolvable channel there is no authority to anchor a new
            // item to; returning null lets SetBoxInitialEntityState surface the
            // dead-end error.
            return entity.ContentChannel == null ? null : entity;
        }

        /// <summary>
        /// Sets the initial entity state of the box based on the entity and current person's permissions.
        /// </summary>
        private void SetBoxInitialEntityState( DetailBlockBox<ContentChannelItemBag, ContentChannelItemDetailOptionsBag> box, ContentChannelItem entity )
        {
            if ( entity == null )
            {
                box.ErrorMessage = NoContentChannelErrorMessage;
                return;
            }

            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( !entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( ContentChannelItem.FriendlyTypeName );
                    box.Options = new ContentChannelItemDetailOptionsBag { IsUnauthorizedErrorShown = true };
                    return;
                }

                box.Entity = GetEntityBagForView( entity );
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( !box.IsEditable )
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( ContentChannelItem.FriendlyTypeName );
                    box.Options = new ContentChannelItemDetailOptionsBag { IsUnauthorizedErrorShown = true };
                    return;
                }

                box.Entity = GetEntityBagForEdit( entity );
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag fields common to both view and edit modes.
        /// </summary>
        private ContentChannelItemBag GetCommonEntityBag( ContentChannelItem entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new ContentChannelItemBag
            {
                IdKey = entity.IdKey,
                Title = entity.Title,
                StartDateTime = entity.StartDateTime.ToRockDateTimeOffset(),
                ExpireDateTime = entity.ExpireDateTime?.ToRockDateTimeOffset(),
                Priority = entity.Priority
            };
        }

        /// <inheritdoc/>
        protected override ContentChannelItemBag GetEntityBagForView( ContentChannelItem entity )
        {
            return GetCommonEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override ContentChannelItemBag GetEntityBagForEdit( ContentChannelItem entity )
        {
            var bag = GetCommonEntityBag( entity );

            if ( bag == null )
            {
                return null;
            }

            bag.Status = entity.Status;
            bag.ApprovedByName = GetApproverDisplayName( entity );
            bag.ApprovedDateTime = entity.ApprovedDateTime?.ToString( "s" );
            bag.IsReadOnlyStatusShown = IsReadOnlyStatusShown( entity );

            var editorState = ResolveContentEditorState( entity );
            bag.ContentEditorType = editorState.EditorType;
            bag.Content = entity.Content;
            bag.StructuredContent = entity.StructuredContent;
            bag.StructuredContentToolValueGuid = editorState.ToolValueGuid;
            bag.IsContentEditorStartingInCodeMode = editorState.IsStartingInCodeMode;
            bag.EncryptedContentImageRootFolder = editorState.EncryptedImageRootFolder;
            bag.EncryptedContentDocumentRootFolder = editorState.EncryptedDocumentRootFolder;

            bag.UrlSlugs = GetUrlSlugs( entity );
            bag.SlugUrlPrefix = GetSlugPrefix();

            // Null for new items; the AssignItemGlobalKey PreSave hook mints it on first save.
            bag.ItemGlobalKey = entity.ItemGlobalKey;

            if ( IsChannelContentLibraryEnabled() )
            {
                var libraryState = ResolveLibraryStatus( entity );

                bag.LibraryStatus = libraryState.Status;
                bag.LibraryLicenseName = libraryState.LicenseName;
                bag.LibraryByPersonName = libraryState.ByPersonName;
                bag.LibraryOnDateTime = libraryState.OnDateTime;
                bag.ExperienceLevel = entity.ExperienceLevel;
                bag.ContentLibraryContentTopicGuid = entity.ContentLibraryContentTopicId.HasValue
                    ? ContentTopicCache.Get( entity.ContentLibraryContentTopicId.Value )?.Guid.ToString()
                    : null;
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            LoadPersonalizationSelections( entity, bag );
            LoadIntentSelections( entity, bag );
            LoadRelatedItemsState( entity, bag );

            return bag;
        }

        /// <summary>
        /// Applies new-item defaults: content channel and type, start date, and approval status.
        /// Shared by <see cref="GetInitialEntity"/> and <see cref="TryGetEntityForEditAction"/>
        /// so Add construction cannot drift.
        /// </summary>
        private void ApplyNewContentChannelItemDefaultValues( ContentChannelItem entity )
        {
            if ( entity == null || entity.Id != 0 )
            {
                return;
            }

            var contentChannel = ResolveContentChannel();

            if ( contentChannel == null )
            {
                return;
            }

            // The channel and type must be wired before any authorization
            // check: a content channel item's ParentAuthority is its channel,
            // so an unwired new item would resolve security against the wrong
            // authority.
            entity.ContentChannel = contentChannel;
            entity.ContentChannelId = contentChannel.Id;
            entity.ContentChannelType = contentChannel.ContentChannelType;
            entity.ContentChannelTypeId = contentChannel.ContentChannelTypeId;
            entity.StartDateTime = RockDateTime.Now;

            _contentChannelCache = ContentChannelCache.Get( contentChannel.Id );

            SeedParentAssociationFromHierarchy( entity, contentChannel );

            if ( contentChannel.RequiresApproval )
            {
                entity.Status = ContentChannelItemStatus.PendingApproval;
            }
            else
            {
                entity.Status = ContentChannelItemStatus.Approved;
                entity.ApprovedDateTime = RockDateTime.Now;
                entity.ApprovedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
            }
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( ContentChannelItem entity, ValidPropertiesBox<ContentChannelItemBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Title ),
                () => entity.Title = box.Bag.Title );

            box.IfValidProperty( nameof( box.Bag.Priority ),
                () => entity.Priority = box.Bag.Priority );

            box.IfValidProperty( nameof( box.Bag.StartDateTime ),
                () => entity.StartDateTime = box.Bag.StartDateTime?.DateTime ?? RockDateTime.Now );

            box.IfValidProperty( nameof( box.Bag.ExpireDateTime ),
                () => entity.ExpireDateTime = box.Bag.ExpireDateTime?.DateTime );

            // The Item Global Key is editable for an existing item only: persist the
            // (possibly regenerated) value the editor sees, mirroring the WebForms
            // existing-item write. A new item is left to the AssignItemGlobalKey
            // PreSave hook (blank-only generation). Only a non-blank value is written
            // so a missing bag value never blanks an existing key.
            if ( entity.Id != 0 )
            {
                box.IfValidProperty( nameof( box.Bag.ItemGlobalKey ),
                    () =>
                    {
                        if ( box.Bag.ItemGlobalKey.IsNotNullOrWhiteSpace() )
                        {
                            entity.ItemGlobalKey = box.Bag.ItemGlobalKey;
                        }
                    } );
            }

            if ( box.Bag.ContentEditorType == ContentChannelItemContentEditor.Html )
            {
                box.IfValidProperty( nameof( box.Bag.Content ),
                    () => entity.Content = box.Bag.Content );
            }

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out ContentChannelItem entity, out BlockActionResult error )
        {
            var entityService = new ContentChannelItemService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    error = ActionBadRequest( $"{ContentChannelItem.FriendlyTypeName} not found." );
                    return false;
                }

                _contentChannelCache = ContentChannelCache.Get( entity.ContentChannelId );
            }
            else
            {
                entity = new ContentChannelItem();

                ApplyNewContentChannelItemDefaultValues( entity );

                if ( entity.ContentChannel == null )
                {
                    error = ActionBadRequest( NoContentChannelErrorMessage );
                    return false;
                }

                entityService.Add( entity );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {ContentChannelItem.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<IBreadCrumb>();
            var contentChannelItemService = new ContentChannelItemService( RockContext );
            var allowIntegerIds = !PageCache.Layout.Site.DisablePredictableIds;

            // Resolve each ancestor and the current item once, deduped by Id in
            // encounter order, eager-loading the channel. Read from
            // pageReference, not the live request (IBreadCrumbBlock contract).
            var resolvedItems = new List<ContentChannelItem>();
            var seenItemIds = new HashSet<int>();

            foreach ( var hierarchyKey in ParseHierarchy( pageReference.GetPageParameter( PageParameterKey.Hierarchy ) ) )
            {
                var hierarchyItem = contentChannelItemService.GetInclude( hierarchyKey, item => item.ContentChannel, allowIntegerIds );
                if ( hierarchyItem != null && seenItemIds.Add( hierarchyItem.Id ) )
                {
                    resolvedItems.Add( hierarchyItem );
                }
            }

            var currentItem = contentChannelItemService.GetInclude( pageReference.GetPageParameter( PageParameterKey.ContentItemId ), item => item.ContentChannel, allowIntegerIds );
            var currentItemId = currentItem?.Id ?? 0;
            if ( currentItem != null && seenItemIds.Add( currentItem.Id ) )
            {
                resolvedItems.Add( currentItem );
            }

            // Optional ReturnUrl channel crumb, with the same relative-only
            // guard as the return URL. Channel name from the first resolved
            // item, else the ContentChannelId parameter.
            var returnUrl = GetSafeRelativeUrl( pageReference.GetPageParameter( PageParameterKey.ReturnUrl ) );
            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                var channelName = resolvedItems.FirstOrDefault()?.ContentChannel?.Name;

                if ( channelName.IsNullOrWhiteSpace() )
                {
                    var channelParam = pageReference.GetPageParameter( PageParameterKey.ContentChannelId );
                    channelName = new ContentChannelService( RockContext ).Get( channelParam, allowIntegerIds )?.Name;
                }

                breadCrumbs.Add( new BreadCrumbLink( channelName, returnUrl ) );
            }

            // One crumb per ancestor, with the accumulating IdKey trail.
            var currentHierarchy = new List<string>();
            foreach ( var contentItem in resolvedItems )
            {
                var crumbItemKey = contentItem.IdKey ?? contentItem.Id.ToString();
                var breadCrumbPageReference = BuildAncestorCrumbReference( pageReference, crumbItemKey, currentHierarchy );

                breadCrumbs.Add( new BreadCrumbLink( contentItem.Title, breadCrumbPageReference ) );

                currentHierarchy.Add( crumbItemKey );
            }

            // New-item fallback leaf when nothing resolved.
            if ( !resolvedItems.Any() && currentItemId == 0 )
            {
                breadCrumbs.Add( new BreadCrumbLink( "New Content Item", pageReference ) );
            }

            return new BreadCrumbResult
            {
                BreadCrumbs = breadCrumbs
            };
        }

        /// <summary>
        /// Builds the page reference for an ancestor breadcrumb, rewriting ContentItemId and Hierarchy.
        /// </summary>
        private static PageReference BuildAncestorCrumbReference( PageReference source, string contentItemKey, List<string> hierarchyTrail )
        {
            var breadCrumbParameters = new Dictionary<string, string>( source.Parameters, StringComparer.OrdinalIgnoreCase );
            breadCrumbParameters.Remove( PageParameterKey.ContentItemId );
            breadCrumbParameters.Remove( PageParameterKey.Hierarchy );
            breadCrumbParameters[PageParameterKey.ContentItemId] = contentItemKey;
            breadCrumbParameters[PageParameterKey.Hierarchy] = hierarchyTrail.Any() ? hierarchyTrail.JoinStrings( "," ) : string.Empty;
            breadCrumbParameters[PageParameterKey.AutoEdit] = "true";

            return new PageReference( source.PageId, 0, breadCrumbParameters );
        }

        /// <summary>
        /// Gets the box navigation URLs. ParentPage is the return URL used by both Save and Cancel.
        /// </summary>
        private Dictionary<string, string> GetBoxNavigationUrls( ContentChannelItem entity )
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = GetReturnUrl( entity )
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the entity bag for the edit form.
        /// </summary>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<ContentChannelItemBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity. Returns the new IdKey for a new item, or the updated view bag for an existing one.
        /// </summary>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<ContentChannelItemBag> box )
        {
            var entityService = new ContentChannelItemService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag?.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var isNewItem = entity.Id == 0;

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            ApplyDateMode( entity );
            ApplyNewItemOrder( entity );
            ApplyStatus( entity, box.Bag );
            var structuredContentChanges = ApplyContent( entity, box, out var structuredContentHelper );
            ApplyLibraryFields( entity, box );

            if ( !entity.IsValid )
            {
                return ActionBadRequest( entity.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( "; " ) );
            }

            RockContext.WrapTransaction( () =>
            {
                if ( structuredContentHelper != null )
                {
                    structuredContentHelper.ApplyDatabaseChanges( structuredContentChanges, RockContext );
                }

                // Assigns a new item's Id so co-saves below can key on entity.Id.
                RockContext.SaveChanges();

                if ( isNewItem )
                {
                    ApplyStagedSlugs( entity, box.Bag );
                }

                entity.SaveAttributeValues( RockContext );

                ApplyPersonalization( entity, box );
                ApplyOccurrenceAssociation( entity );
                ApplyIntents( entity, box );
            } );

            new ProcessContentCollectionDocument.Message
            {
                EntityTypeId = entity.TypeId,
                EntityId = entity.Id
            }.Send();

            if ( isNewItem )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, entity.IdKey );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<ContentChannelItemBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the entity and returns the redirect URL, or an error if delete is not allowed.
        /// </summary>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var contentItemService = new ContentChannelItemService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !contentItemService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Resolved before delete while the entity is still live.
            var returnUrl = GetReturnUrl( entity );

            contentItemService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( returnUrl );
        }

        /// <summary>
        /// Persists a slug for an existing item immediately. New-item slugs are staged in the bag and written inside Save.
        /// </summary>
        [BlockAction]
        public BlockActionResult SaveSlug( SaveSlugRequestBag request )
        {
            // EDIT re-check closes the authorization gap from legacy REST slug actions.
            if ( !TryGetEntityForEditAction( request.IdKey, out var entity, out var error ) )
            {
                return error;
            }

            // A new item's slugs are staged client-side and never call this action.
            if ( entity.Id == 0 )
            {
                return ActionBadRequest( "URL slugs for a new item are saved when the item is saved." );
            }

            // Guard blank before normalizing: a null slug would surface as a 500.
            if ( request.Slug.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "The URL slug could not be saved. Enter at least one letter or number." );
            }

            var slugService = new ContentChannelItemSlugService( RockContext );

            // Confirm the slug row belongs to this item to prevent cross-item slug forgery.
            if ( request.ContentChannelItemSlugId.HasValue )
            {
                var existingSlug = slugService.Get( request.ContentChannelItemSlugId.Value );

                if ( existingSlug == null || existingSlug.ContentChannelItemId != entity.Id )
                {
                    return ActionBadRequest( "The URL slug could not be found." );
                }
            }

            // Pass the known channel id so slug uniqueness stays channel-scoped; the id-only overload resolves the channel via cache, which can degrade to global scope.
            var savedSlug = slugService.SaveSlug( entity.Id, entity.ContentChannelId, request.Slug, request.ContentChannelItemSlugId );

            if ( savedSlug == null )
            {
                return ActionBadRequest( "The URL slug could not be saved. Enter at least one letter or number." );
            }

            return ActionOk( new SaveSlugResponseBag
            {
                Id = savedSlug.Id,
                Slug = savedSlug.Slug
            } );
        }

        /// <summary>
        /// Deletes a persisted slug for an existing item immediately.
        /// </summary>
        [BlockAction]
        public BlockActionResult DeleteSlug( DeleteSlugRequestBag request )
        {
            if ( !TryGetEntityForEditAction( request.IdKey, out var entity, out var error ) )
            {
                return error;
            }

            var slugService = new ContentChannelItemSlugService( RockContext );
            var slug = slugService.Get( request.ContentChannelItemSlugId );

            // Confirm the slug belongs to this item before deleting.
            if ( slug == null || slug.ContentChannelItemId != entity.Id )
            {
                return ActionBadRequest( "The URL slug could not be found." );
            }

            slugService.Delete( slug );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Returns a channel-unique slug candidate without persisting it.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetUniqueSlug( GetUniqueSlugRequestBag request )
        {
            if ( !TryGetEntityForEditAction( request.IdKey, out var entity, out var error ) )
            {
                return error;
            }

            // Guard blank before generating.
            if ( request.Slug.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A valid URL slug could not be generated. Enter at least one letter or number." );
            }

            var uniqueSlug = new ContentChannelItemSlugService( RockContext )
                .GetUniqueSlugForContentChannel( request.Slug, entity.ContentChannelId, request.ContentChannelItemSlugId );

            if ( uniqueSlug.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A valid URL slug could not be generated. Enter at least one letter or number." );
            }

            return ActionOk( new { Slug = uniqueSlug } );
        }

        /// <summary>
        /// Returns a regenerated unique Item Global Key candidate derived from the given title, without persisting it.
        /// </summary>
        [BlockAction]
        public BlockActionResult RefreshItemGlobalKey( string key, string title )
        {
            if ( !TryGetEntityForEditAction( key, out _, out var error ) )
            {
                return error;
            }

            if ( title.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Enter a title before regenerating the item identifier." );
            }

            var newKey = new ContentChannelItemSlugService( RockContext ).GetUniqueContentSlug( title, null );

            if ( newKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A valid item identifier could not be generated. Enter at least one letter or number." );
            }

            return ActionOk( new { ItemGlobalKey = newKey } );
        }

        /// <summary>
        /// Re-downloads the item from the Content Library, overwriting title, content, and library fields.
        /// </summary>
        [BlockAction]
        public BlockActionResult Redownload( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var contentChannel = entity.ContentChannel;
            if ( contentChannel == null || !entity.ContentLibrarySourceIdentifier.HasValue )
            {
                return ActionBadRequest( "This item is not linked to a Content Library source and cannot be re-downloaded." );
            }

            var contentChannelItemService = new ContentChannelItemService( RockContext );

            try
            {
                contentChannelItemService.AddFromContentLibrary( new ContentLibraryItemDownloadOptions
                {
                    ContentLibraryItemGuidToDownload = entity.ContentLibrarySourceIdentifier.Value,
                    DownloadIntoContentChannelGuid = contentChannel.Guid,
                    CurrentPersonPerformingDownload = RequestContext.CurrentPerson
                } );
            }
            catch ( AddFromContentLibraryException ex )
            {
                return ActionBadRequest( ex.Message );
            }
            catch ( ArgumentException ex )
            {
                return ActionBadRequest( ex.Message );
            }

            // AddFromContentLibrary commits internally; re-read the now-overwritten item.
            var refreshed = contentChannelItemService.Get( entity.Id );

            var bag = GetEntityBagForEdit( refreshed );

            return ActionOk( new ValidPropertiesBox<ContentChannelItemBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Returns the current child-items grid data and the live reorder-enabled flag.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetChildItemsGridData( string key )
        {
            if ( !TryGetEntityForViewAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            return ActionOk( GetChildItemsGridBuilder().Build( BuildChildItemRows( entity, out _ ) ) );
        }

        /// <summary>
        /// Returns the current parent-items grid data.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetParentItemsGridData( string key )
        {
            if ( !TryGetEntityForViewAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            return ActionOk( GetParentItemsGridBuilder().Build( BuildParentItemRows( entity ) ) );
        }

        /// <summary>
        /// Links an existing item as a child of the current item.
        /// </summary>
        [BlockAction]
        public BlockActionResult AddExistingChildItem( string key, string childItemKey )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var childItem = new ContentChannelItemService( RockContext )
                .Get( childItemKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( childItem == null )
            {
                return ActionBadRequest( $"{ContentChannelItem.FriendlyTypeName} not found." );
            }

            if ( !childItem.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to view {ContentChannelItem.FriendlyTypeName}." );
            }

            if ( childItem.Id == entity.Id )
            {
                return ActionBadRequest( "An item cannot be added as its own child." );
            }

            var isInChildChannel = _contentChannelCache?.ChildContentChannels
                .Any( c => c.Id == childItem.ContentChannelId ) ?? false;

            if ( !isInChildChannel )
            {
                return ActionBadRequest( "The selected item is not in one of this channel's child channels." );
            }

            var associationService = new ContentChannelItemAssociationService( RockContext );

            // Idempotent: a duplicate means a stale client; the refetch shows the row.
            var isAlreadyChild = associationService.Queryable()
                .AsNoTracking()
                .Any( a => a.ContentChannelItemId == entity.Id && a.ChildContentChannelItemId == childItem.Id );

            if ( isAlreadyChild )
            {
                return ActionOk();
            }

            var maxOrder = associationService.Queryable()
                .AsNoTracking()
                .Where( a => a.ContentChannelItemId == entity.Id )
                .Select( a => ( int? ) a.Order )
                .DefaultIfEmpty()
                .Max();

            associationService.Add( new ContentChannelItemAssociation
            {
                ContentChannelItemId = entity.Id,
                ChildContentChannelItemId = childItem.Id,
                Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0
            } );

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Unlinks a child item from the current item without deleting the child.
        /// </summary>
        [BlockAction]
        public BlockActionResult RemoveChildAssociation( string key, string childItemKey )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var childItemId = new ContentChannelItemService( RockContext )
                .GetSelect( childItemKey, i => ( int? ) i.Id, !PageCache.Layout.Site.DisablePredictableIds );

            // Child may have been hard-deleted by another editor; associations die with it.
            if ( !childItemId.HasValue )
            {
                return ActionOk();
            }

            var associationService = new ContentChannelItemAssociationService( RockContext );
            var association = associationService.Queryable()
                .Where( a =>
                    a.ContentChannelItemId == entity.Id &&
                    a.ChildContentChannelItemId == childItemId.Value )
                .FirstOrDefault();

            if ( association != null )
            {
                associationService.Delete( association );
                RockContext.SaveChanges();
            }

            return ActionOk();
        }

        /// <summary>
        /// Hard-deletes a child item and its associations.
        /// </summary>
        [BlockAction]
        public BlockActionResult DeleteChildItem( string key, string childItemKey )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var itemService = new ContentChannelItemService( RockContext );
            var childItem = itemService.Get( childItemKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( childItem == null )
            {
                return ActionBadRequest( $"{ContentChannelItem.FriendlyTypeName} not found." );
            }

            if ( !childItem.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {ContentChannelItem.FriendlyTypeName}." );
            }

            if ( !itemService.CanDelete( childItem, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Clean both sides: rows where the deleted item is the child and rows where it is the parent.
            var associationService = new ContentChannelItemAssociationService( RockContext );
            var associations = associationService.Queryable()
                .Where( a =>
                    a.ChildContentChannelItemId == childItem.Id ||
                    a.ContentChannelItemId == childItem.Id )
                .ToList();

            associationService.DeleteRange( associations );
            itemService.Delete( childItem );

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Reorders the child associations. Refuses when the list is VIEW-filtered.
        /// </summary>
        [BlockAction]
        public BlockActionResult ReorderChildItem( string key, string childItemKey, string beforeChildItemKey )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var associations = GetChildItemAssociationList( entity, out var isFiltered )
                .OrderBy( a => a.Order )
                .ToList();

            if ( isFiltered )
            {
                return ActionBadRequest( "Cannot reorder a filtered list." );
            }

            /*
                6/9/26 - MSE

                The grid keys on the CHILD ITEM's IdKey while the order column
                lives on the ASSOCIATION rows, so the keys are mapped first: each
                child-item key resolves to its item Id, the Id finds its
                association row, and ReorderEntity then runs on the ordered
                association list using the associations' own IdKeys (renumbering
                Order via IOrdered).

                Reason: Child-item grid keys must map to association rows before
                the reorder applies.
            */
            var allowIntegerIds = !PageCache.Layout.Site.DisablePredictableIds;
            var itemService = new ContentChannelItemService( RockContext );

            var childItemId = itemService.GetSelect( childItemKey, i => ( int? ) i.Id, allowIntegerIds );
            var association = childItemId.HasValue
                ? associations.FirstOrDefault( a => a.ChildContentChannelItemId == childItemId.Value )
                : null;

            if ( association == null )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            ContentChannelItemAssociation beforeAssociation = null;

            if ( beforeChildItemKey.IsNotNullOrWhiteSpace() )
            {
                var beforeChildItemId = itemService.GetSelect( beforeChildItemKey, i => ( int? ) i.Id, allowIntegerIds );
                beforeAssociation = beforeChildItemId.HasValue
                    ? associations.FirstOrDefault( a => a.ChildContentChannelItemId == beforeChildItemId.Value )
                    : null;

                if ( beforeAssociation == null )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }
            }

            if ( !associations.ReorderEntity( association.IdKey, beforeAssociation?.IdKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Returns items in the selected child channel eligible to be added as a child of the current item.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetAddChildItemOptions( string key, string channelKey )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Resolve against configured child channels to prevent enumerating outside the hierarchy.
            var channelGuid = channelKey.AsGuidOrNull();
            var childChannel = channelGuid.HasValue
                ? _contentChannelCache?.ChildContentChannels.FirstOrDefault( c => c.Guid == channelGuid.Value )
                : null;

            if ( childChannel == null )
            {
                return ActionBadRequest( "Content channel not found." );
            }

            var existingChildItemIds = new ContentChannelItemAssociationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.ContentChannelItemId == entity.Id )
                .Select( a => a.ChildContentChannelItemId )
                .ToList();

            var eligibleItems = new ContentChannelItemService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( i => i.ContentChannel )
                .Where( i =>
                    i.ContentChannelId == childChannel.Id &&
                    i.Id != entity.Id )
                .ToList()
                .Where( i => !existingChildItemIds.Contains( i.Id )
                    && i.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();

            eligibleItems = childChannel.ItemsManuallyOrdered
                ? eligibleItems.OrderBy( i => i.Order ).ToList()
                : eligibleItems.OrderByDescending( i => i.StartDateTime ).ToList();

            var itemOptions = eligibleItems
                .Select( i => new ListItemBag
                {
                    Value = i.Guid.ToString(),
                    Text = $"{i.Title} ({i.StartDateTime.ToShortDateString()})"
                } )
                .ToList();

            return ActionOk( itemOptions );
        }

        /// <summary>
        /// Builds the navigation URL for a clicked child or parent grid row.
        /// </summary>
        [BlockAction]
        public BlockActionResult NavigateToRelatedItem( string key, string selectedItemKey )
        {
            if ( !TryGetEntityForViewAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var selectedItemId = new ContentChannelItemService( RockContext )
                .GetSelect( selectedItemKey, i => ( int? ) i.Id, !PageCache.Layout.Site.DisablePredictableIds );

            if ( !selectedItemId.HasValue )
            {
                return ActionBadRequest( $"{ContentChannelItem.FriendlyTypeName} not found." );
            }

            var selectedItemIdKey = IdHasher.Instance.GetHash( selectedItemId.Value );

            // Trim the trail up to (but not including) the selected key, so drilling
            // up through a grid row shortens the trail and drilling down extends it.
            var trimmedTrail = new List<string>();

            foreach ( var trailKey in BuildNavigationHierarchyTrail( entity ) )
            {
                if ( trailKey == selectedItemIdKey )
                {
                    break;
                }

                trimmedTrail.Add( trailKey );
            }

            var qryParams = new Dictionary<string, string>
            {
                [PageParameterKey.ContentItemId] = selectedItemIdKey,
                [PageParameterKey.Hierarchy] = trimmedTrail.JoinStrings( "," ),
                [PageParameterKey.AutoEdit] = "true"
            };

            var returnUrl = PageParameter( PageParameterKey.ReturnUrl );
            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.ReturnUrl] = returnUrl;
            }

            return ActionOk( this.GetCurrentPageUrl( qryParams, skipExistingParameters: true ) );
        }

        /// <summary>
        /// Builds the Add New child navigation URL for the selected child channel.
        /// </summary>
        [BlockAction]
        public BlockActionResult NavigateToNewChildItem( string key, string channelKey )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Restrict to configured child channels, same as AddExistingChildItem.
            var channelGuid = channelKey.AsGuidOrNull();
            var childChannel = channelGuid.HasValue
                ? _contentChannelCache?.ChildContentChannels.FirstOrDefault( c => c.Guid == channelGuid.Value )
                : null;

            if ( childChannel == null )
            {
                return ActionBadRequest( "Content channel not found." );
            }

            // Convert channel Guid to IdKey for the ContentChannelId URL parameter.
            var qryParams = new Dictionary<string, string>
            {
                [PageParameterKey.ContentItemId] = "0",
                [PageParameterKey.ContentChannelId] = childChannel.IdKey,
                [PageParameterKey.Hierarchy] = BuildNavigationHierarchyTrail( entity ).JoinStrings( "," )
            };

            var returnUrl = PageParameter( PageParameterKey.ReturnUrl );
            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.ReturnUrl] = returnUrl;
            }

            return ActionOk( this.GetCurrentPageUrl( qryParams, skipExistingParameters: true ) );
        }

        /// <summary>
        /// Creates an entity set from the child items grid selection and returns
        /// its identifier so the caller can redirect to the Launch Workflow page.
        /// </summary>
        /// <param name="entitySet">The entity set bag from the grid.</param>
        /// <returns>The new entity set identifier.</returns>
        [BlockAction]
        public BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        #endregion Block Actions

        #region Helper Methods

        /// <summary>
        /// Resolves the content channel for a new item. The block setting wins over the ContentChannelId page parameter.
        /// </summary>
        private ContentChannel ResolveContentChannel()
        {
            var contentChannelService = new ContentChannelService( RockContext );
            var contentChannelSetting = GetAttributeValue( AttributeKey.ContentChannel );

            if ( contentChannelSetting.IsNullOrWhiteSpace() )
            {
                var contentChannelKey = PageParameter( PageParameterKey.ContentChannelId );

                if ( contentChannelKey.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                return contentChannelService.Get( contentChannelKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            var contentChannelGuid = contentChannelSetting.AsGuid();

            return contentChannelService.Get( contentChannelGuid );
        }

        /// <summary>
        /// Computes the redirect URL for Save and Cancel: ReturnUrl wins if safe-relative, then event-occurrence, hierarchy drill-up, or channel-only.
        /// </summary>
        private string GetReturnUrl( ContentChannelItem entity )
        {
            var safeReturnUrl = GetSafeRelativeUrl( PageParameter( PageParameterKey.ReturnUrl ) );
            if ( safeReturnUrl.IsNotNullOrWhiteSpace() )
            {
                return safeReturnUrl;
            }

            return GetEventOccurrenceReturnUrl()
                ?? GetHierarchyDrillUpReturnUrl( entity )
                ?? GetChannelOnlyReturnUrl( entity );
        }

        /// <summary>
        /// Returns the event-occurrence return URL, or null when no EventItemOccurrenceId resolves.
        /// </summary>
        private string GetEventOccurrenceReturnUrl()
        {
            var allowIntegerIds = !PageCache.Layout.Site.DisablePredictableIds;

            var eventItemOccurrenceIdKey = new EventItemOccurrenceService( RockContext )
                .GetNoTracking( PageParameter( PageParameterKey.EventItemOccurrenceId ), allowIntegerIds )?.IdKey;

            if ( eventItemOccurrenceIdKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var qryParams = new Dictionary<string, string>();

            var eventCalendarIdKey = new EventCalendarService( RockContext )
                .GetNoTracking( PageParameter( PageParameterKey.EventCalendarId ), allowIntegerIds )?.IdKey;
            if ( eventCalendarIdKey.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.EventCalendarId] = eventCalendarIdKey;
            }

            var eventItemIdKey = new EventItemService( RockContext )
                .GetNoTracking( PageParameter( PageParameterKey.EventItemId ), allowIntegerIds )?.IdKey;
            if ( eventItemIdKey.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.EventItemId] = eventItemIdKey;
            }

            qryParams[PageParameterKey.EventItemOccurrenceId] = eventItemOccurrenceIdKey;

            var occurrenceChannelIdKey = new ContentChannelService( RockContext )
                .GetNoTracking( PageParameter( PageParameterKey.ContentChannelId ), allowIntegerIds )?.IdKey;
            if ( occurrenceChannelIdKey.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.ContentChannelId] = occurrenceChannelIdKey;
            }

            return this.GetParentPageUrl( qryParams );
        }

        /// <summary>
        /// Returns the hierarchy drill-up return URL, or null when no ancestor remains.
        /// </summary>
        private string GetHierarchyDrillUpReturnUrl( ContentChannelItem entity )
        {
            var allowIntegerIds = !PageCache.Layout.Site.DisablePredictableIds;
            var currentItemKey = entity?.IdKey ?? string.Empty;

            var currentItemId = ( entity?.Id ?? 0 ) > 0 ? entity.Id.ToString() : null;
            var contentChannelItemService = new ContentChannelItemService( RockContext );
            var trimmedHierarchy = new List<string>();

            foreach ( var hierarchyKey in ParseHierarchy( PageParameter( PageParameterKey.Hierarchy ) ) )
            {
                if ( hierarchyKey == currentItemKey || hierarchyKey == currentItemId )
                {
                    break;
                }

                var ancestorIdKey = contentChannelItemService.GetNoTracking( hierarchyKey, allowIntegerIds )?.IdKey;
                if ( ancestorIdKey.IsNotNullOrWhiteSpace() )
                {
                    trimmedHierarchy.Add( ancestorIdKey );
                }
            }

            if ( !trimmedHierarchy.Any() )
            {
                return null;
            }

            var qryParams = new Dictionary<string, string>();

            if ( trimmedHierarchy.Count > 1 )
            {
                qryParams[PageParameterKey.Hierarchy] = trimmedHierarchy.Take( trimmedHierarchy.Count - 1 ).JoinStrings( "," );
            }

            qryParams[PageParameterKey.ContentItemId] = trimmedHierarchy.Last();

            return this.GetCurrentPageUrl( qryParams, skipExistingParameters: true );
        }

        /// <summary>
        /// Seeds the new item's parent association from the last Hierarchy entry, in memory only.
        /// </summary>
        private void SeedParentAssociationFromHierarchy( ContentChannelItem entity, ContentChannel contentChannel )
        {
            var hierarchy = ParseHierarchy( PageParameter( PageParameterKey.Hierarchy ) );
            if ( !hierarchy.Any() )
            {
                return;
            }

            var allowIntegerIds = !PageCache.Layout.Site.DisablePredictableIds;
            var parentItem = new ContentChannelItemService( RockContext )
                .Get( hierarchy.Last(), allowIntegerIds );

            var isSeedable = parentItem != null
                && parentItem.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                && parentItem.ContentChannel.ChildContentChannels.Any( c => c.Id == contentChannel.Id );

            if ( !isSeedable )
            {
                return;
            }

            var maxOrder = parentItem.ChildItems
                .Select( a => ( int? ) a.Order )
                .DefaultIfEmpty()
                .Max();

            entity.ParentItems.Add( new ContentChannelItemAssociation
            {
                ContentChannelItemId = parentItem.Id,
                Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0
            } );
        }

        /// <summary>
        /// Returns the channel-only return URL; always returns a value, terminating the chain.
        /// </summary>
        private string GetChannelOnlyReturnUrl( ContentChannelItem entity )
        {
            var qryParams = new Dictionary<string, string>();

            var channelIdKey = new ContentChannelService( RockContext )
                .GetNoTracking( entity?.ContentChannelId ?? 0 )?.IdKey;
            if ( channelIdKey.IsNotNullOrWhiteSpace() )
            {
                qryParams[PageParameterKey.ContentChannelId] = channelIdKey;
            }

            return this.GetParentPageUrl( qryParams );
        }

        /// <summary>
        /// Returns the URL only when it is a safe site-relative target (single leading slash, not protocol-relative, well-formed), otherwise null.
        /// </summary>
        private static string GetSafeRelativeUrl( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( !url.StartsWith( "/" ) || url.StartsWith( "//" ) )
            {
                return null;
            }

            return Uri.IsWellFormedUriString( url, UriKind.Relative ) ? url : null;
        }

        /// <summary>
        /// Splits the raw Hierarchy parameter value into ordered keys.
        /// </summary>
        private static List<string> ParseHierarchy( string hierarchyValue )
        {
            if ( hierarchyValue.IsNullOrWhiteSpace() )
            {
                return new List<string>();
            }

            return hierarchyValue.SplitDelimitedValues( false ).ToList();
        }

        /// <summary>
        /// Builds the header-area labels for associated event occurrences.
        /// </summary>
        private List<OccurrenceLabelBag> GetOccurrenceLabels( ContentChannelItem entity )
        {
            var labels = new List<OccurrenceLabelBag>();

            if ( entity == null || entity.Id == 0 )
            {
                return labels;
            }

            var occurrences = new EventItemOccurrenceChannelItemService( RockContext )
                .Queryable( "EventItemOccurrence.EventItem,EventItemOccurrence.Campus" )
                .AsNoTracking()
                .Where( a => a.ContentChannelItemId == entity.Id && a.EventItemOccurrence != null )
                .ToList()
                .Select( a => a.EventItemOccurrence );

            foreach ( var occurrence in occurrences )
            {
                var url = this.GetLinkedPageUrl( AttributeKey.EventOccurrencePage, new Dictionary<string, string>
                {
                    [PageParameterKey.EventItemOccurrenceId] = occurrence.IdKey
                } );

                labels.Add( new OccurrenceLabelBag
                {
                    Text = occurrence.ToString(),
                    Url = url.IsNotNullOrWhiteSpace() ? url : null,
                    IconCssClass = "ti ti-calendar"
                } );
            }

            return labels;
        }

        /// <summary>
        /// Loads the persisted slug rows for an existing item, or an empty list for a new item.
        /// </summary>
        private List<UrlSlugBag> GetUrlSlugs( ContentChannelItem entity )
        {
            if ( entity == null || entity.Id == 0 )
            {
                return new List<UrlSlugBag>();
            }

            return new ContentChannelItemSlugService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( s => s.ContentChannelItemId == entity.Id )
                .Select( s => new UrlSlugBag { Id = s.Id, Slug = s.Slug } )
                .ToList();
        }

        /// <summary>
        /// Returns the channel-URL prefix for slugs by stripping the trailing {{Slug}} token from ItemUrl.
        /// </summary>
        private string GetSlugPrefix()
        {
            var itemUrl = _contentChannelCache?.ItemUrl;

            if ( itemUrl.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            itemUrl = itemUrl.RemoveSpaces();

            if ( itemUrl.EndsWith( "{{Slug}}" ) )
            {
                return itemUrl.Replace( "{{Slug}}", string.Empty );
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns true when the status gate is active: the channel requires approval and the type does not disable status. Shared by the toggle and read-only displays.
        /// </summary>
        private bool IsStatusGateActive( ContentChannelItem entity )
        {
            if ( entity == null )
            {
                return false;
            }

            var requiresApproval = _contentChannelCache?.RequiresApproval ?? false;
            var disableStatus = ContentChannelTypeCache.Get( entity.ContentChannelTypeId )?.DisableStatus ?? false;

            return requiresApproval && !disableStatus;
        }

        /// <summary>
        /// Returns true when the approval toggle should render: the status gate is active and the current person has APPROVE.
        /// </summary>
        private bool IsApprovalToggleShown( ContentChannelItem entity )
        {
            return IsStatusGateActive( entity ) && entity.IsAuthorized( Authorization.APPROVE, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Returns true when the read-only status display should render: the status gate is active but the current person lacks APPROVE.
        /// </summary>
        private bool IsReadOnlyStatusShown( ContentChannelItem entity )
        {
            return IsStatusGateActive( entity ) && !entity.IsAuthorized( Authorization.APPROVE, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Resolves the approver display name from ApprovedByPersonAliasId, or null if no approval has occurred.
        /// </summary>
        private string GetApproverDisplayName( ContentChannelItem entity )
        {
            if ( entity == null || !entity.ApprovedByPersonAliasId.HasValue )
            {
                return null;
            }

            return new PersonAliasService( RockContext )
                .GetPersonNoTracking( entity.ApprovedByPersonAliasId.Value )?.FullName;
        }

        /// <summary>
        /// Resolves which content editor renders, its configuration, and the encrypted roots.
        /// Called once at load; the result round-trips to save so the editor selection is stable mid-edit.
        /// </summary>
        private ContentEditorState ResolveContentEditorState( ContentChannelItem entity )
        {
            var state = new ContentEditorState();

            // Channel facts from cache to avoid lazy-loads on the no-tracking entity.
            var contentChannelType = ContentChannelTypeCache.Get( entity.ContentChannelTypeId );
            var contentChannel = _contentChannelCache;

            // Disabled content field or unresolvable channel: no editor renders.
            if ( contentChannel == null || ( contentChannelType?.DisableContentField ?? false ) )
            {
                return state;
            }

            // Channel flag drives the editor; two per-item overrides handle existing-item edge cases.
            var useStructured = contentChannel.IsStructuredContent;

            if ( useStructured && entity.Id != 0 && entity.StructuredContent.IsNullOrWhiteSpace() )
            {
                // Existing HTML item on a now-structured channel: keep in HTML editor.
                useStructured = false;
            }
            else if ( !useStructured && entity.Id != 0 && entity.StructuredContent.IsNotNullOrWhiteSpace() )
            {
                // Existing structured item on a now-HTML channel: keep in structured editor.
                useStructured = true;
            }

            if ( useStructured )
            {
                state.EditorType = ContentChannelItemContentEditor.Structured;

                if ( contentChannel.StructuredContentToolValueId.HasValue )
                {
                    // Resolve int to Guid via cache; null ToolValueGuid uses the system default.
                    state.ToolValueGuid = DefinedValueCache.Get( contentChannel.StructuredContentToolValueId.Value )?.Guid;
                }

                return state;
            }

            state.EditorType = ContentChannelItemContentEditor.Html;

            // ContentControlType defaults to CodeEditor (0), so unset channels start in code mode.
            state.IsStartingInCodeMode = contentChannel.ContentControlType == ContentControlType.CodeEditor;

            if ( contentChannel.RootImageDirectory.IsNotNullOrWhiteSpace() )
            {
                // The HTML editor wants encrypted roots; both image and document use the same path.
                var encryptedRoot = Encryption.EncryptString( contentChannel.RootImageDirectory );
                state.EncryptedImageRootFolder = encryptedRoot;
                state.EncryptedDocumentRootFolder = encryptedRoot;
            }

            return state;
        }

        /// <summary>
        /// Returns true when the item's content channel has the Content Library feature enabled.
        /// </summary>
        private bool IsChannelContentLibraryEnabled()
        {
            return _contentChannelCache?.ContentLibraryConfiguration?.IsEnabled == true;
        }

        /// <summary>
        /// Returns the grouped Content Topic dropdown options ordered by domain order, name, topic order, and name.
        /// </summary>
        private List<ListItemBag> GetContentTopicListItems()
        {
            /*
                6/9/26 - MSE

                The query applies no IsActive filter, so it lists every content
                topic. If active-only topics are ever required, add a
                .Where( t => t.IsActive ) clause here. Matches Webforms.

                Reason: All topics are listed; there is no IsActive filter.
            */
            return new ContentTopicService( RockContext )
                .Queryable()
                .OrderBy( t => t.ContentTopicDomain.Order )
                .ThenBy( t => t.ContentTopicDomain.Name )
                .ThenBy( t => t.Order )
                .ThenBy( t => t.Name )
                .Select( t => new
                {
                    t.Guid,
                    t.Name,
                    DomainName = t.ContentTopicDomain.Name
                } )
                .ToList()
                .Select( t => new ListItemBag
                {
                    Value = t.Guid.ToString(),
                    Text = t.Name,
                    Category = t.DomainName
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the Segment and Request Filter option lists for the options bag.
        /// </summary>
        private void BuildPersonalizationOptions( ContentChannelItemDetailOptionsBag options )
        {
            options.SegmentOptions = PersonalizationSegmentCache.All()
                .OrderBy( s => s.Name )
                .ToListItemBagList();

            options.RequestFilterOptions = RequestFilterCache.All()
                .OrderBy( f => f.Name )
                .ToListItemBagList();
        }

        /// <summary>
        /// Preloads the personalization segment and request filter Guids for an existing item.
        /// </summary>
        private void LoadPersonalizationSelections( ContentChannelItem entity, ContentChannelItemBag bag )
        {
            bag.SelectedSegmentGuids = new List<string>();
            bag.SelectedRequestFilterGuids = new List<string>();

            var isPersonalizationEnabled = _contentChannelCache?.EnablePersonalization ?? false;

            if ( !isPersonalizationEnabled || entity.Id == 0 )
            {
                return;
            }

            var entityTypeId = ContentChannelItemEntityTypeId;

            // Ids from DB, Guids from cache to avoid a second query.
            bag.SelectedSegmentGuids = new PersonalizationSegmentService( RockContext )
                .GetPersonalizedEntitySegmentQuery( entityTypeId, entity.Id )
                .Select( a => a.PersonalizationEntityId )
                .ToList()
                .Select( id => PersonalizationSegmentCache.Get( id )?.Guid.ToString() )
                .Where( g => g != null )
                .ToList();

            bag.SelectedRequestFilterGuids = new RequestFilterService( RockContext )
                .GetPersonalizedEntityRequestFilterQuery( entityTypeId, entity.Id )
                .Select( a => a.PersonalizationEntityId )
                .ToList()
                .Select( id => RequestFilterCache.Get( id )?.Guid.ToString() )
                .Where( g => g != null )
                .ToList();
        }

        /// <summary>
        /// Builds the Content Intent options: active values plus any currently selected, deduplicated.
        /// </summary>
        private List<ListItemBag> BuildIntentOptions( ContentChannelItem entity )
        {
            var intentType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.INTERACTION_INTENT.AsGuid() );

            if ( intentType == null )
            {
                return new List<ListItemBag>();
            }

            // Active values first, then selected values to retain retired-but-selected entries.
            var optionsByGuid = new Dictionary<Guid, DefinedValueCache>();

            foreach ( var activeValue in intentType.DefinedValues.Where( v => v.IsActive ) )
            {
                optionsByGuid[activeValue.Guid] = activeValue;
            }

            if ( entity.Id != 0 )
            {
                var selectedValueIds = new EntityIntentService( RockContext )
                    .GetIntentValueIds<ContentChannelItem>( entity.Id );

                foreach ( var selectedId in selectedValueIds )
                {
                    var selectedValue = DefinedValueCache.Get( selectedId );

                    if ( selectedValue != null )
                    {
                        optionsByGuid[selectedValue.Guid] = selectedValue;
                    }
                }
            }

            return optionsByGuid.Values
                .OrderBy( v => v.Order )
                .ToListItemBagList();
        }

        /// <summary>
        /// Preloads the selected Content Intent Guids for an existing item.
        /// </summary>
        private void LoadIntentSelections( ContentChannelItem entity, ContentChannelItemBag bag )
        {
            bag.SelectedIntentGuids = new List<string>();

            if ( DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.INTERACTION_INTENT.AsGuid() ) == null || entity.Id == 0 )
            {
                return;
            }

            bag.SelectedIntentGuids = new EntityIntentService( RockContext )
                .GetIntentValueIds<ContentChannelItem>( entity.Id )
                .Select( id => DefinedValueCache.Get( id )?.Guid.ToString() )
                .Where( g => g != null )
                .ToList();
        }

        /// <summary>
        /// Resolves the content-library status panel data: which panel renders, the license, and attribution.
        /// </summary>
        private ContentLibraryState ResolveLibraryStatus( ContentChannelItem entity )
        {
            var state = new ContentLibraryState();

            // Null license yields null name; client omits the chip.
            var license = entity.ContentLibraryLicenseTypeValueId.HasValue
                ? DefinedValueCache.Get( entity.ContentLibraryLicenseTypeValueId.Value )
                : null;
            state.LicenseName = license != null ? $"{license.Value} License" : null;

            if ( entity.IsUploadedToContentLibrary )
            {
                state.Status = ContentChannelItemLibraryStatus.Uploaded;
                state.ByPersonName = ResolveLibraryPersonName( entity.ContentLibraryUploadedByPersonAliasId );
                state.OnDateTime = entity.ContentLibraryUploadedDateTime;
            }
            else if ( entity.IsDownloadedFromContentLibrary )
            {
                state.Status = ContentChannelItemLibraryStatus.Downloaded;
                state.ByPersonName = ResolveLibraryPersonName( entity.CreatedByPersonAliasId );
                state.OnDateTime = entity.CreatedDateTime;
            }

            return state;
        }

        /// <summary>
        /// Resolves a person's display name from a person-alias id using a no-tracking lookup.
        /// </summary>
        private string ResolveLibraryPersonName( int? personAliasId )
        {
            if ( !personAliasId.HasValue )
            {
                return null;
            }

            return new PersonAliasService( RockContext )
                .GetPersonNoTracking( personAliasId.Value )?.FullName;
        }

        /// <summary>
        /// Loads an existing content channel item and verifies VIEW. The read-only companion of TryGetEntityForEditAction.
        /// </summary>
        private bool TryGetEntityForViewAction( string idKey, out ContentChannelItem entity, out BlockActionResult error )
        {
            error = null;
            entity = new ContentChannelItemService( RockContext )
                .Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                error = ActionBadRequest( $"{ContentChannelItem.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to view {ContentChannelItem.FriendlyTypeName}." );
                return false;
            }

            _contentChannelCache = ContentChannelCache.Get( entity.ContentChannelId );

            return true;
        }

        /// <summary>
        /// Loads the related-items region into the edit bag: stack visibility flags, reorder flag, and grid data.
        /// </summary>
        private void LoadRelatedItemsState( ContentChannelItem entity, ContentChannelItemBag bag )
        {
            bag.IsChildItemsStackShown = entity.Id > 0 && ( _contentChannelCache?.ChildContentChannels.Any() ?? false );
            bag.IsParentItemsStackShown = entity.Id > 0 && ( _contentChannelCache?.ParentContentChannels.Any() ?? false );

            if ( bag.IsChildItemsStackShown )
            {
                var childRows = BuildChildItemRows( entity, out var isReorderEnabled );

                bag.IsChildReorderEnabled = isReorderEnabled;
                bag.ChildItemsGridData = GetChildItemsGridBuilder().Build( childRows );
            }

            if ( bag.IsParentItemsStackShown )
            {
                bag.ParentItemsGridData = GetParentItemsGridBuilder().Build( BuildParentItemRows( entity ) );
            }
        }

        /// <summary>
        /// Gets the current item's child associations, filtered by VIEW on the child item.
        /// </summary>
        private List<ContentChannelItemAssociation> GetChildItemAssociationList( ContentChannelItem contentItem, out bool isFiltered )
        {
            isFiltered = false;

            var associations = new ContentChannelItemAssociationService( RockContext )
                .Queryable()
                .Include( a => a.ChildContentChannelItem.ContentChannel )
                .Where( a => a.ContentChannelItemId == contentItem.Id )
                .ToList();

            var authorizedAssociations = new List<ContentChannelItemAssociation>();

            foreach ( var association in associations )
            {
                if ( association.ChildContentChannelItem.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    authorizedAssociations.Add( association );
                }
                else
                {
                    isFiltered = true;
                }
            }

            return authorizedAssociations;
        }

        /// <summary>
        /// Builds the VIEW-filtered child grid rows, sorted by Order in reorder mode or StartDateTime descending otherwise.
        /// </summary>
        private List<ChildItemRow> BuildChildItemRows( ContentChannelItem entity, out bool isReorderEnabled )
        {
            var associations = GetChildItemAssociationList( entity, out var isFiltered );

            isReorderEnabled = ( _contentChannelCache?.ChildItemsManuallyOrdered ?? false ) && !isFiltered;
            var isReorderMode = isReorderEnabled;

            var rows = associations
                .Select( a =>
                {
                    var childItem = a.ChildContentChannelItem;
                    var childChannel = ContentChannelCache.Get( childItem.ContentChannelId );
                    var childType = ContentChannelTypeCache.Get( childItem.ContentChannelTypeId );
                    var isStatusShown = ( childChannel?.RequiresApproval ?? false ) && !( childType?.DisableStatus ?? false );

                    return new ChildItemRow
                    {
                        IdKey = childItem.IdKey,
                        Guid = childItem.Guid,
                        Title = childItem.Title,
                        ChannelName = childChannel?.Name,
                        StartDateTime = childItem.StartDateTime,
                        ExpireDateTime = childType?.DateRangeType == ContentChannelDateType.DateRange ? childItem.ExpireDateTime : null,
                        Status = isStatusShown ? childItem.Status.ConvertToString() : string.Empty,
                        Order = isReorderMode ? ( int? ) a.Order : null
                    };
                } )
                .ToList();

            return isReorderMode
                ? rows.OrderBy( r => r.Order ).ToList()
                : rows.OrderByDescending( r => r.StartDateTime ).ToList();
        }

        /// <summary>
        /// Builds the VIEW-filtered parent grid rows, sorted StartDateTime descending.
        /// </summary>
        private List<ParentItemRow> BuildParentItemRows( ContentChannelItem entity )
        {
            var associations = new ContentChannelItemAssociationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( a => a.ContentChannelItem.ContentChannel.ContentChannelType )
                .Where( a => a.ChildContentChannelItemId == entity.Id )
                .ToList();

            var rows = new List<ParentItemRow>();

            foreach ( var association in associations )
            {
                var parentItem = association.ContentChannelItem;

                if ( !parentItem.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    continue;
                }

                var parentChannel = ContentChannelCache.Get( parentItem.ContentChannelId );
                var parentType = ContentChannelTypeCache.Get( parentItem.ContentChannelTypeId );
                var isStatusShown = ( parentChannel?.RequiresApproval ?? false ) && !( parentType?.DisableStatus ?? false );

                rows.Add( new ParentItemRow
                {
                    IdKey = parentItem.IdKey,
                    Guid = parentItem.Guid,
                    Title = parentItem.Title,
                    ChannelName = parentChannel?.Name,
                    StartDateTime = parentType?.DateRangeType != ContentChannelDateType.NoDates ? parentItem.StartDateTime : ( DateTime? ) null,
                    ExpireDateTime = parentType?.DateRangeType == ContentChannelDateType.DateRange ? parentItem.ExpireDateTime : null,
                    Status = isStatusShown ? parentItem.Status.ConvertToString() : string.Empty
                } );
            }

            return rows.OrderByDescending( r => r.StartDateTime ).ToList();
        }

        /// <summary>
        /// Returns the child-items grid builder.
        /// </summary>
        private GridBuilder<ChildItemRow> GetChildItemsGridBuilder()
        {
            return new GridBuilder<ChildItemRow>()
                .AddTextField( "idKey", r => r.IdKey )
                .AddField( "guid", r => r.Guid )
                .AddTextField( "title", r => r.Title )
                .AddTextField( "channel", r => r.ChannelName )
                .AddDateTimeField( "startDateTime", r => r.StartDateTime )
                .AddDateTimeField( "expireDateTime", r => r.ExpireDateTime )
                .AddTextField( "status", r => r.Status )
                .AddField( "order", r => r.Order );
        }

        /// <summary>
        /// Returns the parent-items grid builder.
        /// </summary>
        private GridBuilder<ParentItemRow> GetParentItemsGridBuilder()
        {
            return new GridBuilder<ParentItemRow>()
                .AddTextField( "idKey", r => r.IdKey )
                .AddField( "guid", r => r.Guid )
                .AddTextField( "title", r => r.Title )
                .AddTextField( "channel", r => r.ChannelName )
                .AddDateTimeField( "startDateTime", r => r.StartDateTime )
                .AddDateTimeField( "expireDateTime", r => r.ExpireDateTime )
                .AddTextField( "status", r => r.Status );
        }

        /// <summary>
        /// Builds the Add Child modal's channel options from the current channel's child channels.
        /// </summary>
        private List<ListItemBag> BuildAddChildChannelOptions()
        {
            if ( _contentChannelCache == null )
            {
                return new List<ListItemBag>();
            }

            return _contentChannelCache.ChildContentChannels
                .Where( c => c.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( c => c.Name )
                .ToListItemBagList();
        }

        /// <summary>
        /// Builds the outbound hierarchy trail: inbound entries re-emitted as IdKeys with the current item appended.
        /// </summary>
        private List<string> BuildNavigationHierarchyTrail( ContentChannelItem entity )
        {
            var allowIntegerIds = !PageCache.Layout.Site.DisablePredictableIds;
            var contentChannelItemService = new ContentChannelItemService( RockContext );
            var trail = new List<string>();

            foreach ( var hierarchyKey in ParseHierarchy( PageParameter( PageParameterKey.Hierarchy ) ) )
            {
                var hierarchyItemIdKey = contentChannelItemService.GetNoTracking( hierarchyKey, allowIntegerIds )?.IdKey;
                if ( hierarchyItemIdKey.IsNotNullOrWhiteSpace() )
                {
                    trail.Add( hierarchyItemIdKey );
                }
            }

            if ( entity.Id > 0 && entity.IdKey.IsNotNullOrWhiteSpace() )
            {
                trail.Add( entity.IdKey );
            }

            return trail;
        }

        /// <summary>
        /// Normalizes the entity's dates on save: nulls ExpireDateTime unless DateRange, and defaults a missing Start.
        /// </summary>
        private void ApplyDateMode( ContentChannelItem entity )
        {
            var contentChannelType = ContentChannelTypeCache.Get( entity.ContentChannelTypeId );

            // Defensive fallback: scalar map already coalesces, but guard here too.
            if ( entity.StartDateTime == default( DateTime ) )
            {
                entity.StartDateTime = ( contentChannelType?.IncludeTime ?? true ) ? RockDateTime.Now : RockDateTime.Today;
            }

            // Expire only persists for DateRange; stale values in the bag are dropped.
            if ( contentChannelType?.DateRangeType != ContentChannelDateType.DateRange )
            {
                entity.ExpireDateTime = null;
            }
        }

        /// <summary>
        /// Assigns Order for a new item in a manually-sorted channel.
        /// </summary>
        private void ApplyNewItemOrder( ContentChannelItem entity )
        {
            if ( entity.Id == 0 && ( _contentChannelCache?.ItemsManuallyOrdered ?? false ) )
            {
                entity.Order = new ContentChannelItemService( RockContext ).GetNextItemOrderValueForContentChannel( entity.ContentChannelId );
            }
        }

        /// <summary>
        /// Applies approval status on save: DisableStatus forces Approved; approvers can change status; non-approvers are always demoted to PendingApproval.
        /// </summary>
        private void ApplyStatus( ContentChannelItem entity, ContentChannelItemBag bag )
        {
            var disableStatus = ContentChannelTypeCache.Get( entity.ContentChannelTypeId )?.DisableStatus ?? false;

            if ( disableStatus )
            {
                entity.Status = ContentChannelItemStatus.Approved;
                return;
            }

            var canApprove = entity.IsAuthorized( Authorization.APPROVE, RequestContext.CurrentPerson );

            if ( bag.Status != entity.Status && canApprove )
            {
                entity.Status = bag.Status;

                if ( entity.Status == ContentChannelItemStatus.PendingApproval )
                {
                    entity.ApprovedDateTime = null;
                    entity.ApprovedByPersonAliasId = null;
                }
                else
                {
                    entity.ApprovedDateTime = RockDateTime.Now;
                    entity.ApprovedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                }
            }

            /*
                6/10/26 - MSE

                The demotion deliberately has no RequiresApproval term: a
                non-approver's save always resets the item to PendingApproval and
                clears the approver fields, even on a channel that does not require
                approval and even when the status was untouched. Matches Webforms.

                Reason: A non-approver's save always demotes to PendingApproval.
            */
            if ( !canApprove )
            {
                entity.ApprovedDateTime = null;
                entity.ApprovedByPersonAliasId = null;
                entity.Status = ContentChannelItemStatus.PendingApproval;
            }
        }

        /// <summary>
        /// Applies the structured-content write on save and returns the detected changes for the transaction.
        /// </summary>
        private StructuredContentChanges ApplyContent( ContentChannelItem entity, ValidPropertiesBox<ContentChannelItemBag> box, out StructuredContentHelper helper )
        {
            helper = null;

            if ( box.Bag.ContentEditorType != ContentChannelItemContentEditor.Structured
                || !box.IsValidProperty( nameof( box.Bag.StructuredContent ) ) )
            {
                return null;
            }

            helper = new StructuredContentHelper( box.Bag.StructuredContent );

            // Detect against the pre-overwrite value; UpdateEntityFromBox deliberately skips StructuredContent.
            var changes = helper.DetectChanges( entity.StructuredContent );

            entity.StructuredContent = helper.Content;
            entity.Content = helper.Render();

            return changes;
        }

        /// <summary>
        /// Writes ExperienceLevel and ContentLibraryContentTopicId when the channel has the Content Library enabled.
        /// </summary>
        private void ApplyLibraryFields( ContentChannelItem entity, ValidPropertiesBox<ContentChannelItemBag> box )
        {
            if ( !IsChannelContentLibraryEnabled() )
            {
                return;
            }

            box.IfValidProperty( nameof( box.Bag.ExperienceLevel ),
                () => entity.ExperienceLevel = box.Bag.ExperienceLevel );

            box.IfValidProperty( nameof( box.Bag.ContentLibraryContentTopicGuid ), () =>
            {
                var contentTopicGuid = box.Bag.ContentLibraryContentTopicGuid.AsGuidOrNull();
                entity.ContentLibraryContentTopicId = contentTopicGuid.HasValue
                    ? ContentTopicCache.Get( contentTopicGuid.Value )?.Id
                    : null;
            } );
        }

        /// <summary>
        /// Persists the bag's staged slug rows for a new item, inside the Save transaction after the first SaveChanges.
        /// </summary>
        private void ApplyStagedSlugs( ContentChannelItem entity, ContentChannelItemBag bag )
        {
            if ( bag.UrlSlugs == null )
            {
                return;
            }

            var slugService = new ContentChannelItemSlugService( RockContext );

            foreach ( var stagedSlug in bag.UrlSlugs.Where( s => s.Id == 0 ) )
            {
                slugService.SaveSlug( entity.Id, entity.ContentChannelId, stagedSlug.Slug, null );
            }
        }

        /// <summary>
        /// Reconciles the item's personalization segment and request-filter associations from the bag's selected Guids.
        /// </summary>
        private void ApplyPersonalization( ContentChannelItem entity, ValidPropertiesBox<ContentChannelItemBag> box )
        {
            if ( !( _contentChannelCache?.EnablePersonalization ?? false ) )
            {
                return;
            }

            var entityTypeId = ContentChannelItemEntityTypeId;

            box.IfValidProperty( nameof( box.Bag.SelectedSegmentGuids ), () =>
            {
                var segmentIds = ( box.Bag.SelectedSegmentGuids ?? new List<string>() )
                    .Select( g => PersonalizationSegmentCache.Get( g.AsGuid() )?.Id )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList();

                new PersonalizationSegmentService( RockContext ).UpdatePersonalizedEntityForSegments( entityTypeId, entity.Id, segmentIds );
            } );

            box.IfValidProperty( nameof( box.Bag.SelectedRequestFilterGuids ), () =>
            {
                var requestFilterIds = ( box.Bag.SelectedRequestFilterGuids ?? new List<string>() )
                    .Select( g => RequestFilterCache.Get( g.AsGuid() )?.Id )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList();

                new RequestFilterService( RockContext ).UpdatePersonalizedEntityForRequestFilters( entityTypeId, entity.Id, requestFilterIds );
            } );
        }

        /// <summary>
        /// Idempotently associates the item with the EventItemOccurrenceId page parameter, when present.
        /// </summary>
        private void ApplyOccurrenceAssociation( ContentChannelItem entity )
        {
            var eventItemOccurrenceId = new EventItemOccurrenceService( RockContext )
                .GetQueryableByKey( PageParameter( PageParameterKey.EventItemOccurrenceId ), !PageCache.Layout.Site.DisablePredictableIds )
                .Select( io => io.Id )
                .FirstOrDefault();

            if ( eventItemOccurrenceId == 0 )
            {
                return;
            }

            var occurrenceChannelItemService = new EventItemOccurrenceChannelItemService( RockContext );

            var hasAssociation = occurrenceChannelItemService.Queryable()
                .Any( c => c.ContentChannelItemId == entity.Id && c.EventItemOccurrenceId == eventItemOccurrenceId );

            if ( hasAssociation )
            {
                return;
            }

            occurrenceChannelItemService.Add( new EventItemOccurrenceChannelItem
            {
                ContentChannelItemId = entity.Id,
                EventItemOccurrenceId = eventItemOccurrenceId
            } );

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Reconciles the item's interaction intents from the bag's selected Guids.
        /// </summary>
        private void ApplyIntents( ContentChannelItem entity, ValidPropertiesBox<ContentChannelItemBag> box )
        {
            if ( DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.INTERACTION_INTENT.AsGuid() ) == null )
            {
                return;
            }

            box.IfValidProperty( nameof( box.Bag.SelectedIntentGuids ), () =>
            {
                var intentValueIds = ( box.Bag.SelectedIntentGuids ?? new List<string>() )
                    .Select( g => DefinedValueCache.Get( g.AsGuid() )?.Id )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList();

                new EntityIntentService( RockContext ).SetIntents<ContentChannelItem>( entity.Id, intentValueIds );

                RockContext.SaveChanges();
            } );
        }

        #endregion Helper Methods

        #region Helper Classes

        /// <summary>
        /// Resolved content-editor state; result type for ResolveContentEditorState.
        /// </summary>
        private sealed class ContentEditorState
        {
            /// <summary>Gets or sets which editor renders. Defaults to None.</summary>
            public ContentChannelItemContentEditor EditorType { get; set; } = ContentChannelItemContentEditor.None;

            /// <summary>Gets or sets the structured tool-set Guid; null uses the system default.</summary>
            public Guid? ToolValueGuid { get; set; }

            /// <summary>Gets or sets whether the HTML editor opens in code mode. False for non-HTML editors.</summary>
            public bool IsStartingInCodeMode { get; set; }

            /// <summary>Gets or sets the encrypted image root for the HTML editor, or null.</summary>
            public string EncryptedImageRootFolder { get; set; }

            /// <summary>Gets or sets the encrypted document root for the HTML editor. Set from the same channel root as EncryptedImageRootFolder.</summary>
            public string EncryptedDocumentRootFolder { get; set; }
        }

        /// <summary>
        /// Resolved content-library status panel data; result type for ResolveLibraryStatus.
        /// </summary>
        private sealed class ContentLibraryState
        {
            /// <summary>Gets or sets which status panel renders. Defaults to None.</summary>
            public ContentChannelItemLibraryStatus Status { get; set; } = ContentChannelItemLibraryStatus.None;

            /// <summary>Gets or sets the license display name, or null when no license resolves.</summary>
            public string LicenseName { get; set; }

            /// <summary>Gets or sets the uploader for an uploaded item; the creator for a downloaded item.</summary>
            public string ByPersonName { get; set; }

            /// <summary>Gets or sets the upload date for an uploaded item; the creation date for a downloaded item.</summary>
            public DateTime? OnDateTime { get; set; }
        }

        /// <summary>
        /// One child-items grid row.
        /// </summary>
        private sealed class ChildItemRow
        {
            /// <summary>Gets or sets the grid key field.</summary>
            public string IdKey { get; set; }

            public Guid Guid { get; set; }

            public string Title { get; set; }

            /// <summary>Gets or sets the Channel column value.</summary>
            public string ChannelName { get; set; }

            public DateTime StartDateTime { get; set; }

            /// <summary>Gets or sets the expire date; projected only for DateRange child types.</summary>
            public DateTime? ExpireDateTime { get; set; }

            /// <summary>Gets or sets the status text; empty when the per-row gate fails.</summary>
            public string Status { get; set; }

            /// <summary>Gets or sets the association order; carried only in reorder mode.</summary>
            public int? Order { get; set; }
        }

        /// <summary>
        /// One parent-items grid row.
        /// </summary>
        private sealed class ParentItemRow
        {
            /// <summary>Gets or sets the grid key field.</summary>
            public string IdKey { get; set; }

            public Guid Guid { get; set; }

            public string Title { get; set; }

            /// <summary>Gets or sets the Channel column value.</summary>
            public string ChannelName { get; set; }

            /// <summary>Gets or sets the start date; null for NoDates parent types.</summary>
            public DateTime? StartDateTime { get; set; }

            /// <summary>Gets or sets the expire date; projected only for DateRange parent types.</summary>
            public DateTime? ExpireDateTime { get; set; }

            /// <summary>Gets or sets the status text; empty when the per-row gate fails.</summary>
            public string Status { get; set; }
        }

        #endregion Helper Classes
    }
}
