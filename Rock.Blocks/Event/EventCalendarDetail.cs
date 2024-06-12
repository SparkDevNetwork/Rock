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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.EventCalendarDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the details of a particular event calendar.
    /// </summary>

    [DisplayName( "Calendar Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of the given Event Calendar." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b033f86d-c166-4642-b999-0677f2ca2daf" )]
    [Rock.SystemGuid.BlockTypeGuid( "2dc334ac-c2c2-4031-9e1c-6a5b6fbcae9c" )]
    public class EventCalendarDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
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
                var box = new DetailBlockBox<EventCalendarBag, EventCalendarDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<EventCalendar>();

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
        private EventCalendarDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new EventCalendarDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the EventCalendar for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="eventCalendar">The EventCalendar to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the EventCalendar is valid, <c>false</c> otherwise.</returns>
        private bool ValidateEventCalendar( EventCalendar eventCalendar, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<EventCalendarBag, EventCalendarDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {EventCalendar.FriendlyTypeName} was not found.";
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
                    box.Entity.CanAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) || entity.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( EventCalendar.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( EventCalendar.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="EventCalendarBag"/> that represents the entity.</returns>
        private EventCalendarBag GetCommonEntityBag( EventCalendar entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new EventCalendarBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IconCssClass = entity.IconCssClass,
                IsActive = entity.IsActive,
                IsIndexEnabled = entity.IsIndexEnabled,
                Name = entity.Name,
                ExportFeedUrl = string.Format( "{0}GetEventCalendarFeed.ashx?CalendarId={1}", GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ), entity.Id )
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="EventCalendarBag"/> that represents the entity.</returns>
        private EventCalendarBag GetEntityBagForView( EventCalendar entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="rockContext"></param>
        /// <returns>A <see cref="EventCalendarBag"/> that represents the entity.</returns>
        private EventCalendarBag GetEntityBagForEdit( EventCalendar entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            var eventAttributes = GetEventAttributes( rockContext, entity.Id.ToString() );
            bag.EventAttributes = eventAttributes.ConvertAll( e => new EventAttributeBag()
            {
                Attribute = PublicAttributeHelper.GetPublicEditableAttributeViewModel( e ),
                FieldType = FieldTypeCache.Get( e.FieldTypeId )?.Name,
            } );

            bag.SavedContentChannels = new EventCalendarContentChannelService( rockContext )
            .Queryable()
                .Where( c => c.EventCalendarId == entity.Id )
                .Select( e => new ListItemBag()
                {
                    Value = e.ContentChannel.Guid.ToString(),
                    Text = e.ContentChannel.Name
                } )
                .ToList();

            bag.ContentChannels = new ContentChannelService( rockContext )
                .Queryable()
                .Select( c => new ListItemBag()
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name
                } )
                .OrderBy( c => c.Text )
                .ToList();

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( EventCalendar entity, DetailBlockBox<EventCalendarBag, EventCalendarDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IconCssClass ),
                () => entity.IconCssClass = box.Entity.IconCssClass );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.IsIndexEnabled ),
                () => entity.IsIndexEnabled = box.Entity.IsIndexEnabled );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

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
        /// <returns>The <see cref="EventCalendar"/> to be viewed or edited on the page.</returns>
        private EventCalendar GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<EventCalendar, EventCalendarService>( rockContext, PageParameterKey.EventCalendarId );
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
        private string GetSecurityGrantToken( EventCalendar entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out EventCalendar entity, out BlockActionResult error )
        {
            var entityService = new EventCalendarService( rockContext );
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
                entity = new EventCalendar();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{EventCalendar.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${EventCalendar.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save attributes associated with this event.
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

        /// <summary>
        /// Gets the Event attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="eventIdQualifierValue">The event identifier qualifier value.</param>
        /// <returns></returns>
        private static List<Model.Attribute> GetEventAttributes( RockContext rockContext, string eventIdQualifierValue )
        {
            return new AttributeService( rockContext ).GetByEntityTypeId( new EventCalendarItem().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "EventCalendarId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( eventIdQualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

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

                var box = new DetailBlockBox<EventCalendarBag, EventCalendarDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<EventCalendarBag, EventCalendarDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new EventCalendarService( rockContext );
                var eventCalendarContentChannelService = new EventCalendarContentChannelService( rockContext );
                var contentChannelService = new ContentChannelService( rockContext );

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
                if ( !ValidateEventCalendar( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );

                    var dbChannelGuids = eventCalendarContentChannelService.Queryable()
                        .Where( c => c.EventCalendarId == entity.Id )
                        .Select( c => c.Guid )
                        .ToList();

                    var uiChannelGuids = box.Entity.SavedContentChannels.Select( c => c.Value.AsGuid() ).ToList();

                    var toDelete = eventCalendarContentChannelService
                        .Queryable()
                        .Where( c =>
                            dbChannelGuids.Contains( c.Guid ) &&
                            !uiChannelGuids.Contains( c.Guid ) );

                    eventCalendarContentChannelService.DeleteRange( toDelete );
                    contentChannelService.Queryable()
                        .Where( c =>
                            uiChannelGuids.Contains( c.Guid ) &&
                            !dbChannelGuids.Contains( c.Guid ) )
                        .ToList()
                        .ForEach( c =>
                        {
                            var eventCalendarContentChannel = new EventCalendarContentChannel();
                            eventCalendarContentChannel.EventCalendarId = entity.Id;
                            eventCalendarContentChannel.ContentChannelId = c.Id;
                            eventCalendarContentChannelService.Add( eventCalendarContentChannel );
                        } );

                    /* Save Event Attributes */
                    var eventAttributes = box.Entity.EventAttributes.ConvertAll( e => e.Attribute );
                    SaveAttributes( new EventCalendarItem().TypeId, "EventCalendarId", entity.Id.ToString(), eventAttributes, rockContext );

                    entity = entityService.Get( entity.Id );

                    if ( entity != null )
                    {
                        if ( !entity.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                        {
                            entity.AllowPerson( Authorization.VIEW, GetCurrentPerson(), rockContext );
                        }
                        if ( !entity.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
                        {
                            entity.AllowPerson( Authorization.EDIT, GetCurrentPerson(), rockContext );
                        }
                        if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) )
                        {
                            entity.AllowPerson( Authorization.ADMINISTRATE, GetCurrentPerson(), rockContext );
                        }
                    }
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.EventCalendarId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
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
                var entityService = new EventCalendarService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<EventCalendarBag, EventCalendarDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<EventCalendarBag, EventCalendarDetailOptionsBag>
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
        /// Gets the attribute.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
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
