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
using Rock.UniversalSearch;
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
    public class EventCalendarDetail : RockEntityDetailBlockType<EventCalendar, EventCalendarBag>
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
            var box = new DetailBlockBox<EventCalendarBag, EventCalendarDetailOptionsBag>();

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
        private EventCalendarDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new EventCalendarDetailOptionsBag()
            {
                IndexingEnabled = IndexContainer.IndexingEnabled
            };
            return options;
        }

        /// <summary>
        /// Validates the EventCalendar for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="eventCalendar">The EventCalendar to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the EventCalendar is valid, <c>false</c> otherwise.</returns>
        private bool ValidateEventCalendar( EventCalendar eventCalendar, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<EventCalendarBag, EventCalendarDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {EventCalendar.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( EventCalendar.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( EventCalendar.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
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
                ExportFeedUrl = string.Format( "{0}GetEventCalendarFeed.ashx?CalendarId={1}", GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ), entity.Id ),
                CanAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) || entity.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() )
            };
        }

        // <inheritdoc/>
        protected override EventCalendarBag GetEntityBagForView( EventCalendar entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        // <inheritdoc/>
        protected override EventCalendarBag GetEntityBagForEdit( EventCalendar entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            var eventAttributes = GetEventAttributes( entity.Id.ToString() );
            bag.EventAttributes = eventAttributes.ConvertAll( e => new EventAttributeBag()
            {
                Attribute = PublicAttributeHelper.GetPublicEditableAttributeViewModel( e ),
                FieldType = FieldTypeCache.Get( e.FieldTypeId )?.Name,
            } );

            bag.SavedContentChannels = new EventCalendarContentChannelService( RockContext )
            .Queryable()
                .Where( c => c.EventCalendarId == entity.Id )
                .Select( e => new ListItemBag()
                {
                    Value = e.ContentChannel.Guid.ToString(),
                    Text = e.ContentChannel.Name
                } )
                .ToList();

            bag.ContentChannels = new ContentChannelService( RockContext )
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

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( EventCalendar entity, ValidPropertiesBox<EventCalendarBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.IsIndexEnabled ),
                () => entity.IsIndexEnabled = box.Bag.IsIndexEnabled );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override EventCalendar GetInitialEntity()
        {
            return GetInitialEntity<EventCalendar, EventCalendarService>( RockContext, PageParameterKey.EventCalendarId );
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

        // <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out EventCalendar entity, out BlockActionResult error )
        {
            var entityService = new EventCalendarService( RockContext );
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

        /// <summary>
        /// Gets the Event attributes.
        /// </summary>
        /// <param name="eventIdQualifierValue">The event identifier qualifier value.</param>
        /// <returns></returns>
        private List<Model.Attribute> GetEventAttributes( string eventIdQualifierValue )
        {
            return new AttributeService( RockContext ).GetByEntityTypeId( new EventCalendarItem().TypeId, true ).AsQueryable()
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<EventCalendarBag>
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
        public BlockActionResult Save( ValidPropertiesBox<EventCalendarBag> box )
        {
            var entityService = new EventCalendarService( RockContext );
            var eventCalendarContentChannelService = new EventCalendarContentChannelService( RockContext );
            var contentChannelService = new ContentChannelService( RockContext );

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
            if ( !ValidateEventCalendar( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );

                var dbChannelGuids = eventCalendarContentChannelService.Queryable()
                    .Where( c => c.EventCalendarId == entity.Id )
                    .Select( c => c.Guid )
                    .ToList();

                var uiChannelGuids = box.Bag.SavedContentChannels.Select( c => c.Value.AsGuid() ).ToList();

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
                var eventAttributes = box.Bag.EventAttributes.ConvertAll( e => e.Attribute );
                SaveAttributes( new EventCalendarItem().TypeId, "EventCalendarId", entity.Id.ToString(), eventAttributes );

                entity = entityService.Get( entity.Id );

                if ( entity != null )
                {
                    if ( !entity.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                    {
                        entity.AllowPerson( Authorization.VIEW, GetCurrentPerson(), RockContext );
                    }
                    if ( !entity.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
                    {
                        entity.AllowPerson( Authorization.EDIT, GetCurrentPerson(), RockContext );
                    }
                    if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) )
                    {
                        entity.AllowPerson( Authorization.ADMINISTRATE, GetCurrentPerson(), RockContext );
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
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<EventCalendarBag>
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
            var entityService = new EventCalendarService( RockContext );

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

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="attributeGuid">The attribute identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetAttribute( Guid? attributeGuid )
        {
            PublicEditableAttributeBag editableAttribute;
            var entity = GetInitialEntity();
            var eventIdQualifierValue = entity.Id.ToString();
            var attributes = GetEventAttributes( eventIdQualifierValue );

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

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="guid">The identifier of the item that will be moved.</param>
        /// <param name="beforeGuid">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderAttributes( string idKey, Guid guid, Guid? beforeGuid )
        {
            // Get the queryable and make sure it is ordered correctly.
            var id = Rock.Utility.IdHasher.Instance.GetId( idKey );

            var attributes = GetEventAttributes( id?.ToString() );

            if ( !attributes.ReorderEntity( guid.ToString(), beforeGuid.ToString() ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
