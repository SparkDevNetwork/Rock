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
    public class AdaptiveMessageDetail : RockDetailBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string AdaptationDetailPage = "AdaptationDetailPage";
        }

        private static class PageParameterKey
        {
            public const string AdaptiveMessageId = "AdaptiveMessageId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string AdaptationDetailPage = "AdaptationDetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<AdaptiveMessage>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private AdaptiveMessageDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new AdaptiveMessageDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the AdaptiveMessage for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="adaptiveMessage">The AdaptiveMessage to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AdaptiveMessage is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAdaptiveMessage( AdaptiveMessage adaptiveMessage, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AdaptiveMessage.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( AdaptiveMessage.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    var reservedKeyNames = box.Entity.AdaptationSharedAttributes.Select( a => a.Key ).ToList();
                    reservedKeyNames.AddRange( box.Entity.AdaptationAttributes.Select( a => a.Key ).ToList() );
                    box.Options.ReservedKeyNames = reservedKeyNames;
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AdaptiveMessage.FriendlyTypeName );
                }
            }
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
                Categories = entity.AdaptiveMessageCategories.Select( a => a.Category ).ToListItemBagList(),
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="AdaptiveMessageBag"/> that represents the entity.</returns>
        private AdaptiveMessageBag GetEntityBagForView( AdaptiveMessage entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.ADAPTIVE_MESSAGES.AsGuid() );
            var interactionQry = new InteractionService( new RockContext() ).Queryable().Where( a => a.InteractionComponent.InteractionChannelId == interactionChannelId );
            var adaptations = entity.AdaptiveMessageAdaptations.Select( b => new AdaptiveMessageAdaptationData
            {
                AdaptiveMessageAdaptation = b,
                Views = interactionQry.Where( a => a.EntityId == b.Id ).Count()
            } ).OrderBy( a => a.AdaptiveMessageAdaptation.Order ).ThenBy( a => a.AdaptiveMessageAdaptation.Name );
            bag.AdaptationsGridData = GetAdaptationsGridBuilder().Build( adaptations );
            bag.AdaptationsGridDefinition = GetAdaptationsGridBuilder().BuildDefinition();
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

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
            if ( !adaptation.ViewSaturationInDays.HasValue || adaptation.ViewSaturationInDays == default(int))
            {
                saturation = "None";
            }
            else
            {
                saturation = $"{adaptation.ViewSaturationCount ?? 0} views in {adaptation.ViewSaturationInDays} days";
            }

            return saturation;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="AdaptiveMessageBag"/> that represents the entity.</returns>
        private AdaptiveMessageBag GetEntityBagForEdit( AdaptiveMessage entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            var inheritedAttributes = GetAdaptationAttributes( rockContext, null, null );
            var attributes = GetAdaptationAttributes( rockContext, "AdaptiveMessageId", entity.Id.ToString() );

            bag.AdaptationSharedAttributes = new List<PublicEditableAttributeBag>();
            bag.AdaptationSharedAttributes.AddRange( inheritedAttributes.Select( attribute => PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute ) ) );

            bag.AdaptationAttributes = new List<PublicEditableAttributeBag>();
            bag.AdaptationAttributes.AddRange( attributes.Select( attribute => PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute ) ) );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

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

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( AdaptiveMessage entity, DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Key ),
                () => entity.Key = box.Entity.Key );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.Categories ),
                () => UpdateCategories( rockContext, entity, box.Entity ) );

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
        /// <returns>The <see cref="AdaptiveMessage"/> to be viewed or edited on the page.</returns>
        private AdaptiveMessage GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<AdaptiveMessage, AdaptiveMessageService>( rockContext, PageParameterKey.AdaptiveMessageId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.AdaptationDetailPage] = this.GetLinkedPageUrl( AttributeKey.AdaptationDetailPage, "AdaptiveMessageAdaptationId", "((Key))" )
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
        private string GetSecurityGrantToken( AdaptiveMessage entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out AdaptiveMessage entity, out BlockActionResult error, Func<IQueryable<AdaptiveMessage>, IQueryable<AdaptiveMessage>> qryAdditions = null )
        {
            var entityService = new AdaptiveMessageService( rockContext );
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
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="bag">The bag.</param>
        private void UpdateCategories( RockContext rockContext, AdaptiveMessage entity, AdaptiveMessageBag bag )
        {
            var categoryService = new CategoryService( rockContext );
            var adaptiveMessageCategoryService = new AdaptiveMessageCategoryService(rockContext );
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

                var box = new DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
                };

                var reservedKeyNames = box.Entity.AdaptationSharedAttributes.Select( a => a.Key ).ToList();
                reservedKeyNames.AddRange( box.Entity.AdaptationAttributes.Select( a => a.Key ).ToList() );
                box.Options.ReservedKeyNames = reservedKeyNames;

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new AdaptiveMessageService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateAdaptiveMessage( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    if ( box.Entity.AdaptationAttributes.Count > 0 )
                    {
                        SaveAttributes( new AdaptiveMessageAdaptation().TypeId, "AdaptiveMessageId", entity.Id.ToString(), box.Entity.AdaptationAttributes, rockContext );
                    }

                    entity.SaveAttributeValues( rockContext );
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
                var entityService = new AdaptiveMessageService( rockContext );

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
            using ( var rockContext = new RockContext() )
            {
                var adaptiveMessageService = new AdaptiveMessageService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var collection, out var actionError, qry => qry.Include( l => l.AdaptiveMessageAdaptations ) ) )
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

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Deletes the specified adaptation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult DeleteAdaptation( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new AdaptiveMessageAdaptationService( rockContext );
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
        public BlockActionResult RefreshAttributes( DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<AdaptiveMessageBag, AdaptiveMessageDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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
        /// Save attributes associated with this message.
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
