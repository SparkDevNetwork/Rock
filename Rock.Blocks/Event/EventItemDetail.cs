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
using Rock.Security;
using Rock.Tasks;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.EventItemDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the details of a particular event item.
    /// </summary>

    [DisplayName( "Calendar Event Item Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of the given calendar event item." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "e09743b1-cc81-4d00-b3e1-5825a178a473" )]
    [Rock.SystemGuid.BlockTypeGuid( "63d0dfb8-1f9e-464a-a603-2252034bc6af" )]
    public class EventItemDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string EventItemId = "EventItemId";
            public const string EventCalendarId = "EventCalendarId";
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
                var box = new DetailBlockBox<EventItemBag, EventItemDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<EventItem>();

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
        private EventItemDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new EventItemDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the EventItem for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="eventItem">The EventItem to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the EventItem is valid, <c>false</c> otherwise.</returns>
        private bool ValidateEventItem( EventItem eventItem, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<EventItemBag, EventItemDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {EventItem.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( EventItem.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( EventItem.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="EventItemBag"/> that represents the entity.</returns>
        private EventItemBag GetCommonEntityBag( EventItem entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new EventItemBag
            {
                IdKey = entity.IdKey,
                ApprovedByPersonAlias = entity.ApprovedByPersonAlias.ToListItemBag(),
                ApprovedOnDateTime = entity.ApprovedOnDateTime,
                Description = entity.Description,
                DetailsUrl = entity.DetailsUrl,
                IsActive = entity.IsActive,
                IsApproved = entity.IsApproved,
                Name = entity.Name,
                Photo = entity.Photo.ToListItemBag(),
                Summary = entity.Summary,
                Audiences = entity.EventItemAudiences.Select( ea => new ListItemBag() { Text = ea.DefinedValue.Value, Value = ea.DefinedValue.Guid.ToString() } ).ToList()
            };

            if ( entity.EventCalendarItems != null )
            {
                bag.Calendars = entity.EventCalendarItems.Select( ec => ec.EventCalendar?.Guid.ToString() ).ToList();
                bag.SelectedCalendarNames = entity.EventCalendarItems.Select( ec => ec.EventCalendar?.Name ).ToList();
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="EventItemBag"/> that represents the entity.</returns>
        private EventItemBag GetEntityBagForView( EventItem entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.PhotoId.HasValue )
            {
                bag.PhotoUrl = FileUrlHelper.GetImageUrl( entity.PhotoId.Value );
            }
            else
            {
                bag.PhotoUrl = string.Empty;
            }

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// /// <param name="rockContext">The rock context</param>
        /// <returns>A <see cref="EventItemBag"/> that represents the entity.</returns>
        private EventItemBag GetEntityBagForEdit( EventItem entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            var eventAttributes = GetEventAttributes( rockContext, entity.Id.ToString() );
            bag.EventOccurenceAttributes = eventAttributes.ConvertAll( e => new EventItemOccurenceAttributeBag()
            {
                Attribute = PublicAttributeHelper.GetPublicEditableAttributeViewModel( e ),
                FieldType = FieldTypeCache.Get( e.FieldTypeId )?.Name,
            } );

            bag.IsApproved = entity.Id == 0 ? entity.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) : entity.IsApproved;
            SetCalendars( rockContext, bag );
            bag.EventCalendarItemAttributes = GetEventItemAttributes( entity, rockContext );

            if ( entity.IsApproved &&
                entity.ApprovedOnDateTime.HasValue &&
                entity.ApprovedByPersonAlias?.Person != null )
            {
                bag.ApprovalText = string.Format(
                    "Approved at {0} on {1} by {2}.",
                    entity.ApprovedOnDateTime.Value.ToShortTimeString(),
                    entity.ApprovedOnDateTime.Value.ToShortDateString(),
                    entity.ApprovedByPersonAlias.Person.FullName );
            }

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( EventItem entity, DetailBlockBox<EventItemBag, EventItemDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.DetailsUrl ),
                () => entity.DetailsUrl = box.Entity.DetailsUrl );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.Photo ),
                () => entity.PhotoId = box.Entity.Photo.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Summary ),
                () => entity.Summary = box.Entity.Summary );

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
        /// <returns>The <see cref="EventItem"/> to be viewed or edited on the page.</returns>
        private EventItem GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<EventItem, EventItemService>( rockContext, PageParameterKey.EventItemId );
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
        private string GetSecurityGrantToken( EventItem entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out EventItem entity, out BlockActionResult error )
        {
            var entityService = new EventItemService( rockContext );
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
                entity = new EventItem();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{EventItem.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${EventItem.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the site attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="eventIdQualifierValue">The site identifier qualifier value.</param>
        /// <returns></returns>
        private static List<Model.Attribute> GetEventAttributes( RockContext rockContext, string eventIdQualifierValue )
        {
            return new AttributeService( rockContext ).GetByEntityTypeId( new EventItemOccurrence().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "EventItemId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( eventIdQualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the calendars.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private void SetCalendars( RockContext rockContext, EventItemBag bag )
        {
            var calendarId = RequestContext.GetPageParameter( PageParameterKey.EventCalendarId ).AsInteger();
            bag.AvailableCalendars = new List<ListItemBag>();

            foreach ( var calendar in new EventCalendarService( rockContext )
                .Queryable().AsNoTracking()
                .OrderBy( c => c.Name ) )
            {
                if ( calendar.Id == calendarId )
                {
                    bag.IsApproved = bag.IsApproved ||
                        calendar.IsAuthorized( Authorization.APPROVE, GetCurrentPerson() ) ||
                        calendar.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() );
                }

                if (  BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) || calendar.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
                {
                    bag.AvailableCalendars.Add( new ListItemBag() { Text = calendar.Name, Value = calendar.Guid.ToString() } );
                }
            }
        }

        /// <summary>
        /// Marks the old image as temporary.
        /// </summary>
        /// <param name="oldbinaryFileId">The binary file identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private void MarkOldImageAsTemporary( int? oldbinaryFileId, int? newBinaryFileId, RockContext rockContext )
        {
            var binaryFileService = new BinaryFileService( rockContext );

            if ( oldbinaryFileId != newBinaryFileId )
            {
                var oldImageTemplatePreview = binaryFileService.Get( oldbinaryFileId ?? 0 );
                if ( oldImageTemplatePreview != null )
                {
                    // the old image won't be needed anymore, so make it IsTemporary and have it get cleaned up later
                    oldImageTemplatePreview.IsTemporary = true;
                }
            }
        }

        /// <summary>
        /// Ensures the current image is not marked as temporary.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void EnsureCurrentImageIsNotMarkedAsTemporary( int? binaryFileId, RockContext rockContext )
        {
            var binaryFileService = new BinaryFileService( rockContext );

            if ( binaryFileId.HasValue )
            {
                var imageTemplatePreview = binaryFileService.Get( binaryFileId.Value );
                if ( imageTemplatePreview != null && imageTemplatePreview.IsTemporary )
                {
                    imageTemplatePreview.IsTemporary = false;
                }
            }
        }

        /// <summary>
        /// Save attributes associated with this Event Item.
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="qualifierColumn"></param>
        /// <param name="qualifierValue"></param>
        /// <param name="eventAttributes"></param>
        /// <param name="rockContext"></param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> eventAttributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = eventAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in eventAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        /// <summary>
        /// Gets the event item attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        private List<EventCalendarItemAttributeBag> GetEventItemAttributes( EventItem entity, RockContext rockContext )
        {
            var attributeBags = new List<EventCalendarItemAttributeBag>();
            var eventCalendarService = new EventCalendarService( rockContext );

            foreach ( var eventCalendarItem in entity.EventCalendarItems )
            {
                eventCalendarItem.LoadAttributes();

                if ( eventCalendarItem.Attributes.Count > 0 )
                {
                    var attributeBag = new EventCalendarItemAttributeBag
                    {
                        EventCalendarGuid = eventCalendarItem.EventCalendar?.Guid ?? eventCalendarService.Get( eventCalendarItem.EventCalendarId ).Guid,
                        EventCalendarName = eventCalendarItem.EventCalendar?.Name ?? eventCalendarService.Get( eventCalendarItem.EventCalendarId ).Name,
                        Attributes = eventCalendarItem.GetPublicAttributesForView( GetCurrentPerson(), true ),
                        AttributeValues = eventCalendarItem.GetPublicAttributeValuesForEdit( GetCurrentPerson(), true )
                    };

                    attributeBags.Add( attributeBag );
                }
            }

            return attributeBags;
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

                var box = new DetailBlockBox<EventItemBag, EventItemDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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
        public BlockActionResult Save( DetailBlockBox<EventItemBag, EventItemDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new EventItemService( rockContext );
                var eventCalendarService = new EventCalendarService( rockContext );
                var eventCalendarItemService = new EventCalendarItemService( rockContext );
                var eventItemAudienceService = new EventItemAudienceService( rockContext );

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
                if ( !ValidateEventItem( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                if ( !entity.IsApproved && box.Entity.IsApproved )
                {
                    entity.ApprovedByPersonAliasId = GetCurrentPerson().PrimaryAliasId;
                    entity.ApprovedOnDateTime = RockDateTime.Now;
                }

                entity.IsApproved = box.Entity.IsApproved;
                if ( !entity.IsApproved )
                {
                    entity.ApprovedByPersonAliasId = null;
                    entity.ApprovedByPersonAlias = null;
                    entity.ApprovedOnDateTime = null;
                }

                if ( box.Entity.Photo != null )
                {
                    var binaryFileId = box.Entity.Photo.GetEntityId<BinaryFile>( rockContext );
                    if ( entity.PhotoId != binaryFileId )
                    {
                        MarkOldImageAsTemporary( entity.PhotoId, binaryFileId, rockContext );
                        entity.PhotoId = binaryFileId;
                        // Ensure that the Image is not set as IsTemporary=True
                        EnsureCurrentImageIsNotMarkedAsTemporary( entity.PhotoId, rockContext );
                    }
                }

                // Remove existing EventItemAudiences that are not selected
                var removeEventItemAudience = entity.EventItemAudiences
                    .Where( ea => !box.Entity.Audiences.Any( a => a.Value == ea.DefinedValue.Guid.ToString() ) ).ToList();

                foreach ( var eventItemAudience in removeEventItemAudience )
                {
                    entity.EventItemAudiences.Remove( eventItemAudience );
                    eventItemAudienceService.Delete( eventItemAudience );
                }

                // Add selected EventItemAudiences that are not existing
                var addEventItemAudiencesDefinedValueGuids = box.Entity.Audiences
                    .Where( lb => !entity.EventItemAudiences.Any( eia => eia.DefinedValue.Guid.ToString() == lb.Value ) )
                    .Select( statGuid => statGuid.Value.AsGuid() )
                    .ToList();

                foreach ( var definedValueGuid in addEventItemAudiencesDefinedValueGuids )
                {
                    var eventItemAudience = new EventItemAudience
                    {
                        DefinedValueId = DefinedValueCache.Get( definedValueGuid ).Id,
                    };
                    entity.EventItemAudiences.Add( eventItemAudience );
                }

                // Remove existing Calendar Items that are not selected
                var removeCalendars = entity.EventCalendarItems
                    .Where( ec => !box.Entity.Calendars.Any( a => a == ec.EventCalendar.Guid.ToString() ) ).ToList();

                foreach ( var eventCalendarItem in removeCalendars)
                {
                    // Make sure user is authorized to remove calendar (they may not have seen every calendar due to security)
                    if ( BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) || eventCalendarItem.EventCalendar.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
                    {
                        entity.EventCalendarItems.Remove( eventCalendarItem );
                        eventCalendarItemService.Delete( eventCalendarItem );
                    }
                }

                // Add selected Calendars that are not existing
                var addCalendarsGuids = box.Entity.Calendars
                    .Where( cGuid => !entity.EventCalendarItems.Any( ec => ec.EventCalendar.Guid.ToString() == cGuid ) )
                    .Select( cGuid => cGuid.AsGuid() )
                    .ToList();

                foreach ( var eventCalendar in eventCalendarService.GetByGuids( addCalendarsGuids ) )
                {
                    entity.EventCalendarItems.Add( new EventCalendarItem
                    {
                        EventCalendarId = eventCalendar.Id,
                    } );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );

                    foreach ( EventCalendarItem eventCalendarItem in entity.EventCalendarItems )
                    {
                        var eventCalendarAttribute = box.Entity.EventCalendarItemAttributes.Find( a => a.EventCalendarGuid == eventCalendarItem.EventCalendar?.Guid );

                        if ( eventCalendarAttribute != null )
                        {
                            eventCalendarItem.LoadAttributes();
                            eventCalendarItem.SetPublicAttributeValues( eventCalendarAttribute.AttributeValues, RequestContext.CurrentPerson );
                            eventCalendarItem.SaveAttributeValues();
                        }
                    }

                    var eventAttributes = box.Entity.EventOccurenceAttributes.ConvertAll( e => e.Attribute );
                    SaveAttributes( new EventItemOccurrence().TypeId, "EventItemId", entity.Id.ToString(), eventAttributes, rockContext );
                } );

                // Update the content collection index.
                new ProcessContentCollectionDocument.Message
                {
                    EntityTypeId = entity.TypeId,
                    EntityId = entity.Id
                }.Send();

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.EventItemId] = entity.IdKey
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
                var entityService = new EventItemService( rockContext );

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
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<EventItemBag, EventItemDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<EventItemBag, EventItemDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity , rockContext )
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
        /// Gets the attribute to be edited or created.
        /// </summary>
        /// <param name="attributeGuid">The attribute identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetAttribute( Guid? attributeGuid )
        {
            PublicEditableAttributeBag editableAttribute;
            var rockContext = new RockContext();

            var entity = GetInitialEntity( rockContext );
            var eventIdQualifierValue = entity.Id.ToString();
            var attributes = GetEventAttributes( rockContext, eventIdQualifierValue );

            if ( !attributeGuid.HasValue )
            {
                editableAttribute = new PublicEditableAttributeBag
                {
                    FieldTypeGuid = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Guid
                };
            }
            else
            {
                var attribute = attributes.Find( a => a.Guid == attributeGuid );
                editableAttribute = PublicAttributeHelper.GetPublicEditableAttributeViewModel( attribute );
            }

            var reservedKeyNames = new List<string>();
            attributes.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );

            return ActionOk( new { editableAttribute, reservedKeyNames } );
        }

        #endregion
    }
}
