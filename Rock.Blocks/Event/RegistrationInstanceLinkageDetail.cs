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
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceLinkageDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the details of a registration instance's particular linkage
    /// </summary>
    ///
    [DisplayName( "Registration Instance Linkage Detail" )]
    [Category( "Event" )]
    [Description( "Block for editing a linkage associated to an event registration instance." )]
    [IconCssClass( "ti ti-question-mark" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "4E9F4E79-56E0-41F8-A43F-F06277E2A780" )]
    [Rock.SystemGuid.BlockTypeGuid( "D341EF12-406B-477D-8A85-16EBDDF2B04B" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "E4437A42-C396-45C8-A657-57BE658DC319" )]
    public class RegistrationInstanceLinkageDetail : RockDetailBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the friendly type name of the entity being managed.
        /// </summary>
        private string FriendlyTypeName => "Registration Instance Linkage";

        /// <summary>
        /// Gets a value indicating whether the current person has authorization to administrate or edit the block.
        /// </summary>
        private bool IsCurrentPersonAuthorized =>
                BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) ||
                BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

        private EventItemOccurrenceGroupMapService EventItemOccurrenceGroupMapService => new EventItemOccurrenceGroupMapService( RockContext );
        private EventCalendarService EventCalendarService => new EventCalendarService( RockContext );
        private RegistrationInstanceService RegistrationInstanceService => new RegistrationInstanceService( RockContext );

        #endregion Properties

        #region Keys

        /// <summary>
        /// Page Parameter Keys
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The registration instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            /// <summary>
            /// The linkage identifier
            /// </summary>
            public const string LinkageId = "LinkageId";
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
            var box = new DetailBlockBox<RegistrationInstanceLinkageDetailBag, RegistrationInstanceLinkageDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private RegistrationInstanceLinkageDetailOptionsBag GetBoxOptions()
        {
            var options = new RegistrationInstanceLinkageDetailOptionsBag()
            {
                Calendars = EventCalendarService.Queryable().AsNoTracking()
                    .Where( c => c.IsActive )
                    .OrderBy( c => c.Name )
                    .Select( c => new ListItemBag
                    {
                        Text = c.Name,
                        Value = c.Guid.ToString()
                    } )
                    .ToList()
            };

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var qryParams = new Dictionary<string, string>();

            var instance = RegistrationInstanceService.GetNoTracking( PageParameter( PageParameterKey.RegistrationInstanceId ) );

            if ( instance != null )
            {
                qryParams.Add( "RegistrationInstanceId", instance.Id.ToString() );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( qryParams )
            };
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<RegistrationInstanceLinkageDetailBag, RegistrationInstanceLinkageDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            // New entity is being created, prepare for edit mode by default.
            if ( box.IsEditable )
            {
                box.Entity = GetEntityBagForEdit( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( EventItem.FriendlyTypeName );
            }
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <returns>The <see cref="EventItem"/> to be viewed or edited on the page.</returns>
        private EventItemOccurrenceGroupMap GetInitialEntity()
        {
            var entity = EventItemOccurrenceGroupMapService.Get( PageParameter( PageParameterKey.LinkageId ) );

            if ( entity == null )
            {
                entity = new EventItemOccurrenceGroupMap { Id = 0 };
            }

            return entity;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="RegistrationInstanceLinkageDetailBag"/> that represents the entity.</returns>
        private RegistrationInstanceLinkageDetailBag GetEntityBagForEdit( EventItemOccurrenceGroupMap entity )
        {
            var bag = new RegistrationInstanceLinkageDetailBag();

            // Prepare context first in case no entity exists
            bag.Context = new RegistrationInstanceLinkageContextBag
            {
                RegistrationInstanceGroupTypeGuid = RegistrationInstanceService.Get(
                    PageParameter( PageParameterKey.RegistrationInstanceId ),
                    !PageCache.Layout.Site.DisablePredictableIds
                )
                ?.RegistrationTemplate
                ?.GroupType
                ?.Guid,
            };

            if ( entity == null )
            {
                return bag;
            }

            // Prepare remainder of bag from existing entity
            bag.IdKey = entity.IdKey;
            bag.CalendarItem = new RegistrationInstanceLinkageDetailCalendarItemBag
            {
                SelectedCalendarItem = entity.EventItemOccurrence != null && entity.EventItemOccurrence.EventItem != null ?
                    new ListItemBag
                    {
                        Text = entity.EventItemOccurrence.EventItem.Name,
                        Value = entity.EventItemOccurrence.EventItem.Guid.ToString()
                    } :
                    new ListItemBag(),
                SelectedOccurrence = new ListItemBag
                {
                    Text = entity.EventItemOccurrence != null ? entity.EventItemOccurrence.ToString() : string.Empty,
                    Value = entity.EventItemOccurrence != null ? entity.EventItemOccurrence.Guid.ToString() : string.Empty
                },
            };
            bag.Group = entity.Group != null ? new ListItemBag { Text = entity.Group.Name, Value = entity.Group.Guid.ToString() } : new ListItemBag();
            bag.Campus = entity.Campus != null ? new ListItemBag { Text = entity.Campus.Name, Value = entity.Campus.Guid.ToString() } : new ListItemBag();
            bag.UrlSlug = entity.UrlSlug;
            bag.PublicName = entity.PublicName;

            return bag;
        }

        /// <summary>
        /// Determines whether the specified URL slug is valid
        /// and unique among event item occurrence group mappings.
        /// </summary>
        /// <remarks>
        /// This method checks for uniqueness of the URL slug,
        /// excluding the provided mapping if specified. Use this method
        /// to ensure that URL slugs do not collide when creating or updating
        /// event item occurrence group mappings.
        /// </remarks>
        /// <param name="urlSlug">The URL slug to validate. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="linkage">The event item occurrence group mapping to exclude from uniqueness validation, or null to validate against
        /// all mappings.</param>
        /// <returns>
        /// <see langword="true"/> if the URL slug is valid and does not conflict with existing mappings; otherwise, <see langword="false"/>.
        /// </returns>
        protected bool ValidateUrlSlug( string urlSlug, EventItemOccurrenceGroupMap linkage, out string errorMessage )
        {
            errorMessage = string.Empty;
            if ( urlSlug.IsNullOrWhiteSpace() )
            {
                // Empty URL slugs are considered valid (no slug)
                return true;
            }

            // Check for valid format (lowercase letters, numbers, hyphens)
            var isValidFormat = System.Text.RegularExpressions.Regex.IsMatch( urlSlug, "^[a-z0-9]+(?:-[a-z0-9]+)*$" );
            if ( !isValidFormat )
            {
                errorMessage = "The URL Slug can only contain lowercase letters, numbers, and hyphens.";
                return false;
            }

            // Check for uniqueness
            var eventMappingService = new EventItemOccurrenceGroupMapService( RockContext );
            var duplicateSlugs = eventMappingService.Queryable().AsNoTracking().Where( m => m.UrlSlug == urlSlug );
            if ( linkage != null )
            {
                duplicateSlugs = duplicateSlugs.Where( m => m.Id != linkage.Id );
            }

            if ( duplicateSlugs.Any() )
            {
                errorMessage = $"URL Slug must be unique across all events. Url slug '{urlSlug}' is already in use.";
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<RegistrationInstanceLinkageDetailBag, RegistrationInstanceLinkageDetailOptionsBag> box )
        {
            if ( !IsCurrentPersonAuthorized )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( FriendlyTypeName ) );
            }

            var service = new EventItemOccurrenceGroupMapService( RockContext );

            EventItemOccurrenceGroupMap linkage = null;

            var bag = box.Entity;
            if ( bag != null )
            {
                var linkageId = bag.IdKey;
                if ( !string.IsNullOrEmpty( linkageId ) )
                {
                    linkage = service.Get( linkageId );
                }

                if ( linkage == null )
                {
                    linkage = new EventItemOccurrenceGroupMap();

                    var instance = RegistrationInstanceService.Get( PageParameter( PageParameterKey.RegistrationInstanceId ) );
                    if ( instance != null )
                    {
                        linkage.RegistrationInstanceId = instance.Id;
                        service.Add( linkage );
                    }
                    else
                    {
                        return ActionBadRequest( "Invalid registration instance." );
                    }
                }

                if ( !ValidateUrlSlug( bag.UrlSlug, linkage, out string errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                var selectedOccurenceGuid = bag.CalendarItem.SelectedOccurrence?.Value.AsGuid();
                var eventCalendarItem = new EventItemOccurrenceService( RockContext ).Queryable().AsNoTracking()
                    .Where( o =>
                        o.Guid == selectedOccurenceGuid
                    )
                    .FirstOrDefault();

                linkage.EventItemOccurrenceId = eventCalendarItem?.Id;
                linkage.GroupId = GroupCache.Get( bag.Group.Value )?.Id;
                linkage.CampusId = CampusCache.Get( bag.Campus.Value )?.Id;
                linkage.UrlSlug = bag.UrlSlug;
                linkage.PublicName = bag.PublicName;

                if ( !linkage.IsValid )
                {
                    return ActionBadRequest( "Invalid data." );
                }
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
            } );

            return ActionOk( box.NavigationUrls[NavigationUrlKey.ParentPage] );
        }

        [BlockAction]
        public BlockActionResult FetchGroupType( Guid groupGuid )
        {
            if ( !IsCurrentPersonAuthorized )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( FriendlyTypeName ) );
            }

            var group = GroupCache.Get( groupGuid );

            if ( group != null && group.GroupType != null )
            {
                return ActionOk( group.GroupType.Guid );
            }

            return ActionBadRequest( "Group not found or has no group type." );
        }

        #endregion
    }
}
