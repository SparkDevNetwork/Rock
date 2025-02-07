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
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.AdaptiveMessageDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular adaptive message.
    /// </summary>

    [DisplayName( "Adaptive Message Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular adaptive message." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Adaptive Message Adaptation Detail Page",
        Description = "The page that will show the adaptive message adaptation details.",
        Key = AttributeKey.AdaptationDetailPage )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "d88ce6cf-c175-4c8f-bff1-d90c590abb3e" )]
    [Rock.SystemGuid.BlockTypeGuid( "a81fe4e0-df9f-4978-83a7-eb5459f37938" )]
    public class AdaptiveMessageDetail : RockEntityDetailBlockType<AdaptiveMessage, AdaptiveMessageBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string AdaptationDetailPage = "AdaptationDetailPage";
        }

        private static class PageParameterKey
        {
            public const string AdaptiveMessageId = "AdaptiveMessageId";
            public const string AdaptiveMessageCategoryId = "AdaptiveMessageCategoryId";
            public const string ExpandedIds = "ExpandedIds";
            public const string ParentCategoryId = "ParentCategoryId";
            public const string CategoryId = "CategoryId";
            public const string AutoEdit = "AutoEdit";
        }

        private static class NavigationUrlKey
        {
            public const string CurrentPage = "CurrentPage";
            public const string AdaptationDetailPage = "AdaptationDetailPage";
            public const string CurrentPageWithoutMessageId = "CurrentPageWithoutMessageId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            if ( PageParameter( PageParameterKey.AdaptiveMessageId ).IsNullOrWhiteSpace() && PageParameter( PageParameterKey.AdaptiveMessageCategoryId ).IsNullOrWhiteSpace() )
            {
                return new DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag>();
            }

            var box = new DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag>();

            SetBoxInitialEntityState( box );
            if ( box.Entity == null )
            {
                return box;
            }

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable, box.Entity );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private AdaptiveMessageDetailOptionsBag GetBoxOptions( bool isEditable, AdaptiveMessageBag entity )
        {
            var options = new AdaptiveMessageDetailOptionsBag();

            var parentCategoryId = PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull();
            if ( parentCategoryId.HasValue )
            {
                var parentCategory = new CategoryService( RockContext ).Get( parentCategoryId.Value );
                if ( parentCategory != null && parentCategory.EntityType.Guid == Rock.SystemGuid.EntityType.ADAPTIVE_MESSAGE_CATEGORY.AsGuid() )
                {
                    options.ParentCategory = parentCategory.ToListItemBag();
                }
            }

            return options;
        }

        /// <summary>
        /// Validates the AdaptiveMessage for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="adaptiveMessage">The AdaptiveMessage to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AdaptiveMessage is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAdaptiveMessage( AdaptiveMessage adaptiveMessage, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AdaptiveMessage.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( AdaptiveMessage.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AdaptiveMessage.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AdaptiveMessageBag"/> that represents the entity.</returns>
        private AdaptiveMessageBag GetCommonEntityBag( AdaptiveMessage entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new AdaptiveMessageBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IsActive = entity.IsActive,
                Key = entity.Key,
                Name = entity.Name,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Categories = entity.AdaptiveMessageCategories.Select( a => a.Category ).ToListItemBagList(),
            };
        }

        /// <inheritdoc/>
        protected override AdaptiveMessageBag GetEntityBagForView( AdaptiveMessage entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.ADAPTIVE_MESSAGES.AsGuid() );
            var interactionQry = new InteractionService( RockContext ).Queryable().Where( a => a.InteractionComponent.InteractionChannelId == interactionChannelId );
            var adaptations = entity.AdaptiveMessageAdaptations.Select( b => new AdaptiveMessageAdaptationData
            {
                AdaptiveMessageAdaptation = b,
                Views = interactionQry.Where( a => a.EntityId == b.Id ).Count()
            } ).OrderBy( a => a.AdaptiveMessageAdaptation.Order ).ThenBy( a => a.AdaptiveMessageAdaptation.Name );
            bag.AdaptationsGridData = GetAdaptationsGridBuilder().Build( adaptations );
            bag.AdaptationsGridDefinition = GetAdaptationsGridBuilder().BuildDefinition();
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <summary>
        /// Gets the attributes grid builder.
        /// </summary>
        /// <returns></returns>
        private GridBuilder<AdaptiveMessageAdaptationData> GetAdaptationsGridBuilder()
        {
            return new GridBuilder<AdaptiveMessageAdaptationData>()
                .AddField( "order", a => a.AdaptiveMessageAdaptation.Order )
                .AddField( "guid", a => a.AdaptiveMessageAdaptation.Guid )
                .AddTextField( "idKey", a => a.AdaptiveMessageAdaptation.IdKey )
                .AddTextField( "name", a => a.AdaptiveMessageAdaptation.Name )
                .AddTextField( "saturation", a => GetSaturation( a.AdaptiveMessageAdaptation ) )
                .AddField( "views", p => p.Views )
                .AddField( "segments", a => a.AdaptiveMessageAdaptation.AdaptiveMessageAdaptationSegments.Select( b => b.PersonalizationSegment?.Name ).ToList() );
        }

        /// <summary>
        /// Get the saturation.
        /// </summary>
        /// <param name="adaptation">The adaptation.</param>
        /// <returns>A <see cref="string"/> saturation.</returns>
        private string GetSaturation( AdaptiveMessageAdaptation adaptation )
        {
            string saturation;

            // If this is a Person, use the Person properties.
            if ( !adaptation.ViewSaturationInDays.HasValue || adaptation.ViewSaturationInDays == default( int ) )
            {
                saturation = "None";
            }
            else
            {
                saturation = $"{adaptation.ViewSaturationCount ?? 0} views in {adaptation.ViewSaturationInDays} days";
            }

            return saturation;
        }

        /// <inheritdoc/>
        protected override AdaptiveMessageBag GetEntityBagForEdit( AdaptiveMessage entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            var inheritedAttributes = GetAdaptationAttributes( RockContext, null, null );
            var attributes = GetAdaptationAttributes( RockContext, "AdaptiveMessageId", entity.Id.ToString() );

            bag.AdaptationSharedAttributes = new List<PublicEditableAttributeBag>();
            bag.AdaptationSharedAttributes.AddRange( inheritedAttributes.Select( attribute => PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute ) ) );

            bag.AdaptationAttributes = new List<PublicEditableAttributeBag>();
            bag.AdaptationAttributes.AddRange( attributes.Select( attribute => PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute ) ) );

            var messageQry = new AdaptiveMessageService( RockContext ).Queryable().AsNoTracking();
            var reservedKeyNames = bag.AdaptationSharedAttributes.Select( a => a.Key ).ToList();
            reservedKeyNames.AddRange( bag.AdaptationAttributes.Select( a => a.Key ).ToList() );
            bag.ReservedKeyNames = reservedKeyNames;
            bag.MessageReservedKeyNames = messageQry.Where( a => a.Id != entity.Id ).Select( a => a.Key ).ToList();

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <summary>
        /// Gets the adaptation attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="siteIdQualifierValue">The site identifier qualifier value.</param>
        /// <returns></returns>
        private static List<Model.Attribute> GetAdaptationAttributes( RockContext rockContext, string qualifierColumn, string qualifierValue )
        {
            var qry = new AttributeService( rockContext ).GetByEntityTypeId( new AdaptiveMessageAdaptation().TypeId, true ).AsQueryable();

            if ( qualifierColumn.IsNullOrWhiteSpace() )
            {
                qry = qry.Where( t =>
                      ( t.EntityTypeQualifierColumn == null || t.EntityTypeQualifierColumn == string.Empty ) &&
                    ( t.EntityTypeQualifierValue == null || t.EntityTypeQualifierValue == string.Empty ) );
            }
            else
            {
                qry = qry.Where( a =>
                    a.EntityTypeQualifierColumn.Equals( qualifierColumn, StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) );
            }

            return qry.OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( AdaptiveMessage entity, ValidPropertiesBox<AdaptiveMessageBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Key ),
                () => entity.Key = box.Bag.Key );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Categories ),
                () => UpdateCategories( entity, box.Bag ) );

            box.IfValidProperty( nameof( box.Bag.StartDate ),
                () => entity.StartDate = box.Bag.StartDate );

            box.IfValidProperty( nameof( box.Bag.EndDate ),
                () => entity.EndDate = box.Bag.EndDate );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override AdaptiveMessage GetInitialEntity()
        {
            var entity = GetInitialEntity<AdaptiveMessage, AdaptiveMessageService>( RockContext, PageParameterKey.AdaptiveMessageId );

            if ( PageParameter( PageParameterKey.AdaptiveMessageCategoryId ).IsNotNullOrWhiteSpace() )
            {
                var adaptiveMessageCategoryId = PageParameter( PageParameterKey.AdaptiveMessageCategoryId ).AsIntegerOrNull();
                if ( adaptiveMessageCategoryId.HasValue )
                {
                    var adaptiveMessageCategory = new AdaptiveMessageCategoryService( RockContext ).Get( adaptiveMessageCategoryId.Value );
                    if ( adaptiveMessageCategory != null )
                    {
                        entity = adaptiveMessageCategory.AdaptiveMessage;
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.CurrentPage] = this.GetCurrentPageUrl(),
                [NavigationUrlKey.AdaptationDetailPage] = this.GetLinkedPageUrl( AttributeKey.AdaptationDetailPage, "AdaptiveMessageAdaptationId", "((Key))" ),
                [NavigationUrlKey.CurrentPageWithoutMessageId] = GetPageLinkWithoutMessageId()
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out AdaptiveMessage entity, out BlockActionResult error )
        {
            var entityService = new AdaptiveMessageService( RockContext );
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
                entity = new AdaptiveMessage();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AdaptiveMessage.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AdaptiveMessage.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the categories.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="bag">The bag.</param>
        private void UpdateCategories( AdaptiveMessage entity, AdaptiveMessageBag bag )
        {
            var categoryService = new CategoryService( RockContext );
            var adaptiveMessageCategoryService = new AdaptiveMessageCategoryService( RockContext );
            var adaptiveMessageCategories = entity.AdaptiveMessageCategories.ToList();
            foreach ( var adaptiveMessageCategory in adaptiveMessageCategories )
            {
                var category = categoryService.Get( adaptiveMessageCategory.CategoryId );

                if ( category != null )
                {
                    if ( !bag.Categories.Any( a => a.Value == category.Guid.ToString() ) )
                    {
                        entity.AdaptiveMessageCategories.Remove( adaptiveMessageCategory );
                        adaptiveMessageCategoryService.Delete( adaptiveMessageCategory );
                    };
                }
            }

            foreach ( var categoryGuid in bag.Categories.Select( c => c.Value.AsGuid() ) )
            {
                var category = categoryService.Get( categoryGuid );

                if ( category != null )
                {
                    if ( !entity.AdaptiveMessageCategories.Any( a => a.CategoryId == category.Id ) )
                    {
                        entity.AdaptiveMessageCategories.Add( new AdaptiveMessageCategory { CategoryId = category.Id } );
                    };
                }
            }
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            // Exclude the auto edit and return URL parameters from the page reference parameters (if any).
            var excludedParamKeys = new[] { PageParameterKey.AutoEdit.ToLower() };
            var paramsToInclude = pageReference.Parameters
                .Where( kv => !excludedParamKeys.Contains( kv.Key.ToLower() ) )
                .ToDictionary( kv => kv.Key, kv => kv.Value );

            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, paramsToInclude );
            var adaptiveMessageId = pageReference.GetPageParameter( PageParameterKey.AdaptiveMessageId );
            var adaptiveMessageCategoryId = pageReference.GetPageParameter( PageParameterKey.AdaptiveMessageCategoryId );

            if ( adaptiveMessageCategoryId == "0" )
            {
                adaptiveMessageId = "0";
            }
            else if ( adaptiveMessageId == null )
            {
                adaptiveMessageId = new AdaptiveMessageCategoryService( RockContext ).Get( adaptiveMessageCategoryId )?.AdaptiveMessageId.ToString();

                if ( adaptiveMessageId == null )
                {
                    return new BreadCrumbResult
                    {
                        BreadCrumbs = new List<IBreadCrumb>()
                    };
                }
            }

            var title = new AdaptiveMessageService( RockContext ).Get( adaptiveMessageId )?.Name ?? "New Adaptive Message";
            var breadCrumb = new BreadCrumbLink( title, breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>
                {
                    breadCrumb
                }
            };
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

            return ActionOk( new ValidPropertiesBox<AdaptiveMessageBag>
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
        public BlockActionResult Save( ValidPropertiesBox<AdaptiveMessageBag> box )
        {
            var entityService = new AdaptiveMessageService( RockContext );

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
            if ( !ValidateAdaptiveMessage( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();

                if ( box.Bag.AdaptationAttributes.Count > 0 )
                {
                    SaveAttributes( new AdaptiveMessageAdaptation().TypeId, "AdaptiveMessageId", entity.Id.ToString(), box.Bag.AdaptationAttributes );
                }

                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.AdaptiveMessageId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<AdaptiveMessageBag>
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
            var entityService = new AdaptiveMessageService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var adaptiveMessageCategoryId = PageParameter( PageParameterKey.AdaptiveMessageCategoryId ).AsIntegerOrNull();
            // reload page, selecting the deleted data view's parent
            var qryParams = new Dictionary<string, string>();
            if ( adaptiveMessageCategoryId.HasValue )
            {
                var adaptiveMessageCategory = entity.AdaptiveMessageCategories.FirstOrDefault( a => a.Id == adaptiveMessageCategoryId.Value );
                if ( adaptiveMessageCategory != null )
                {
                    qryParams["CategoryId"] = adaptiveMessageCategory.CategoryId.ToString();
                }
            }

            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( ( new Rock.Web.PageReference( this.PageCache.Guid.ToString(), qryParams ) ).BuildUrl() );
        }

        /// <summary>
        /// Changes the ordered position of a single adaptation in the adaption list.
        /// </summary>
        /// <param name="key">The identifier of the content collection whose sources will be reordered.</param>
        /// <param name="guid">The unique identifier of the source that will be moved.</param>
        /// <param name="beforeGuid">The unique identifier of the source it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderAdaptation( string key, Guid guid, Guid? beforeGuid )
        {
            var adaptiveMessageService = new AdaptiveMessageService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var collection, out var actionError ) )
            {
                return actionError;
            }

            // Put them in a properly ordered list.
            var sources = collection.AdaptiveMessageAdaptations
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Id )
                .ToList();

            if ( !sources.ReorderEntity( guid.ToString(), beforeGuid?.ToString() ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified adaptation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult DeleteAdaptation( string key )
        {
            var entityService = new AdaptiveMessageAdaptationService( RockContext );
            AdaptiveMessageAdaptation entity = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( key.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
            }

            if ( entity == null )
            {
                return ActionBadRequest( $"{AdaptiveMessageAdaptation.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${AdaptiveMessageAdaptation.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Save attributes associated with this message.
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="qualifierColumn"></param>
        /// <param name="qualifierValue"></param>
        /// <param name="viewStateAttributes"></param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> viewStateAttributes )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( RockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                RockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, RockContext );
            }
        }

        private string GetPageLinkWithoutMessageId()
        {
            var qryParams = new Dictionary<string, string>();
            var parentCategoryId = PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull();
            if ( parentCategoryId.HasValue )
            {
                qryParams[PageParameterKey.CategoryId] = parentCategoryId.ToString();
            }

            qryParams[PageParameterKey.ExpandedIds] = PageParameter( PageParameterKey.ExpandedIds );
            var currentPageRef = new Rock.Web.PageReference( this.PageCache.Guid.ToString(), qryParams );
            return currentPageRef.BuildUrl();
        }


        #endregion

        #region Support Classes

        /// <summary>
        /// The temporary data format to use when building the results for the
        /// grid.
        /// </summary>
        public class AdaptiveMessageAdaptationData
        {
            /// <summary>
            /// Gets or sets the whole message object from the database.
            /// </summary>
            /// <value>
            /// The whole message object from the database.
            /// </value>
            public AdaptiveMessageAdaptation AdaptiveMessageAdaptation { get; set; }

            /// <summary>
            /// Gets or sets the number of transactions in this adaptation.
            /// </summary>
            /// <value>
            /// The number of transactions in this adaptation.
            /// </value>
            public int Views { get; set; }
        }

        #endregion
    }
}
