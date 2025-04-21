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
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceLinkageList;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a list of event item occurrence group maps.
    /// </summary>
    [DisplayName( "Registration Instance - Linkage List" )]
    [Category( "Event" )]
    [Description( "Displays the linkages associated with an event registration instance." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the event item occurrence group map details.",
        Key = AttributeKey.DetailPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_INSTANCE_LINKAGE,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage( "Group Detail Page",
        "The page for viewing details about a group",
        Key = AttributeKey.GroupDetailPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage( "Calendar Item Page",
        "The page to view calendar item details",
        Key = AttributeKey.CalendarItemDetailPage,
        DefaultValue = Rock.SystemGuid.Page.EVENT_DETAIL,
        IsRequired = false,
        Order = 3 )]

    [LinkedPage( "Content Item Page",
        "The page for viewing details about a content channel item",
        Key = AttributeKey.ContentItemDetailPage,
        DefaultValue = Rock.SystemGuid.Page.CONTENT_DETAIL,
        IsRequired = false,
        Order = 4 )]

    [Rock.SystemGuid.EntityTypeGuid( "7c5b1e75-0571-4d62-90a5-0b2431ebb9e8" )]
    [Rock.SystemGuid.BlockTypeGuid( "aaa65861-b711-4659-8e80-975c72a2aa52" )]
    [CustomizedGrid]
    public class RegistrationInstanceLinkageList : RockEntityListBlockType<EventItemOccurrenceGroupMap>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "LinkagePage";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string CalendarItemDetailPage = "CalendarItemDetailPage";
            public const string ContentItemDetailPage = "ContentItemDetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string GroupDetailPage = "GroupDetailPage";
        }

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationInstanceLinkageId = "LinkageId";
            public const string EventCalendarId = "EventCalendarId";
            public const string EventItemId = "EventItemId";
            public const string ContentItemId = "ContentItemId";
        }

        private static class PreferenceKey
        {
            public const string FilterCampuses = "filter-campuses";
        }

        #endregion Keys

        #region Fields

        private RegistrationInstance _registrationInstance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the campuses to filter the results by.
        /// </summary>
        protected List<Guid> FilterCampuses => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToRegistrationTemplate( PreferenceKey.FilterCampuses ) )
            .FromJsonOrNull<List<Guid>>() ?? new List<Guid>();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceLinkageListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private RegistrationInstanceLinkageListOptionsBag GetBoxOptions()
        {
            var registrationInstance = GetRegistrationInstance();
            var options = new RegistrationInstanceLinkageListOptionsBag()
            {
                CampusItems = CampusCache.All().ToListItemBagList(),
                RegistrationTemplateIdKey = registrationInstance?.RegistrationTemplate?.IdKey,
                ExportFileName = $"{registrationInstance?.Name} RegistrationLinkages",
                ExportTitle = $"{registrationInstance?.Name} - Registration Linkages"
            };
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var registrationInstance = GetRegistrationInstance();

            var queryParams = new Dictionary<string, string>()
            {
                { PageParameterKey.RegistrationInstanceLinkageId, "((Key))" },
                { PageParameterKey.RegistrationInstanceId, registrationInstance?.IdKey },
            };
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams ),
                [NavigationUrlKey.GroupDetailPage] = this.GetLinkedPageUrl( AttributeKey.GroupDetailPage, "GroupID", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<EventItemOccurrenceGroupMap> GetListQueryable( RockContext rockContext )
        {
            var registrationInstance = GetRegistrationInstance();
            IEnumerable<EventItemOccurrenceGroupMap> linkages = new List<EventItemOccurrenceGroupMap>();

            if ( registrationInstance != null )
            {
                linkages = new EventItemOccurrenceGroupMapService( rockContext )
                    .Queryable( "EventItemOccurrence.EventItem.EventCalendarItems.EventCalendar,EventItemOccurrence.ContentChannelItems.ContentChannelItem,Group" )
                    .AsNoTracking()
                    .Where( r => r.RegistrationInstanceId == registrationInstance.Id );

                if ( FilterCampuses.Any() )
                {
                    linkages = linkages.Where( l => l.CampusId.HasValue && FilterCampuses.Contains( l.Campus.Guid ) );
                }
            }

            return linkages.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<EventItemOccurrenceGroupMap> GetOrderedListQueryable( IQueryable<EventItemOccurrenceGroupMap> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( l => l.CreatedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<EventItemOccurrenceGroupMap> GetGridBuilder()
        {
            return new GridBuilder<EventItemOccurrenceGroupMap>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "campus", a => a.Campus?.Name )
                .AddTextField( "publicName", a => a.PublicName )
                .AddTextField( "group", a => a.Group?.Name )
                .AddTextField( "groupIdKey", a => a.Group?.IdKey )
                .AddTextField( "urlSlug", a => a.UrlSlug )
                .AddField( "calendarItems", a => GetCalendarItems( a ) )
                .AddField( "contentItems", a => GetContentItems( a ) );
        }

        /// <summary>
        /// Gets the content items.
        /// </summary>
        /// <param name="eventItemOccurrenceGroupMap">The event item occurrence group map.</param>
        /// <returns></returns>
        private List<string> GetContentItems( EventItemOccurrenceGroupMap eventItemOccurrenceGroupMap )
        {
            var contentItems = new List<string>();

            if ( eventItemOccurrenceGroupMap?.EventItemOccurrence?.ContentChannelItems?.Any() == true )
            {
                foreach ( var contentItem in eventItemOccurrenceGroupMap.EventItemOccurrence.ContentChannelItems
                    .Where( c => c.ContentChannelItem != null )
                    .Select( c => c.ContentChannelItem ) )
                {
                    var qryParams = new Dictionary<string, string>
                    {
                        { PageParameterKey.ContentItemId, contentItem.Id.ToString() }
                    };

                    var contentItemUrl = this.GetLinkedPageUrl( AttributeKey.ContentItemDetailPage, qryParams );

                    if ( string.IsNullOrWhiteSpace( contentItemUrl ) )
                    {
                        contentItems.Add( contentItem.Title );
                    }
                    else
                    {
                        contentItems.Add( string.Format( "<a href='{0}'>{1}</a>", contentItemUrl, contentItem.Title ) );
                    }
                }
            }

            return contentItems;
        }

        /// <summary>
        /// Gets the calendar items.
        /// </summary>
        /// <param name="eventItemOccurrenceGroupMap">The event item occurrence group map.</param>
        /// <returns></returns>
        private List<string> GetCalendarItems( EventItemOccurrenceGroupMap eventItemOccurrenceGroupMap )
        {
            var calendarItems = new List<string>();

            if ( eventItemOccurrenceGroupMap?.EventItemOccurrence != null )
            {
                foreach ( var calendarItem in eventItemOccurrenceGroupMap.EventItemOccurrence.EventItem.EventCalendarItems )
                {
                    if ( calendarItem.EventItem != null && calendarItem.EventCalendar != null )
                    {
                        var qryParams = new Dictionary<string, string>
                        {
                            { PageParameterKey.EventCalendarId, calendarItem.EventCalendarId.ToString() },
                            { PageParameterKey.EventItemId, calendarItem.EventItem.Id.ToString() }
                        };

                        var calendarEventUrl = this.GetLinkedPageUrl( AttributeKey.CalendarItemDetailPage, qryParams );

                        if ( string.IsNullOrWhiteSpace( calendarEventUrl ) )
                        {
                            calendarItems.Add( string.Format( "{0} ({1})", calendarItem.EventItem.Name, eventItemOccurrenceGroupMap.EventItemOccurrence.Campus?.Name ?? "All Campuses" ) );
                        }
                        else
                        {
                            calendarItems.Add( string.Format( "<a href='{0}'>{1}</a> ({2})", calendarEventUrl, calendarItem.EventItem.Name, eventItemOccurrenceGroupMap.EventItemOccurrence.Campus?.Name ?? "All Campuses" ) );
                        }
                    }
                }
            }

            return calendarItems;
        }

        /// <summary>
        /// Makes the key unique to the registration template.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToRegistrationTemplate( string key )
        {
            var registrationTemplate = GetRegistrationInstance()?.RegistrationTemplate;

            if ( registrationTemplate != null )
            {
                return $"{registrationTemplate.IdKey}-{key}";
            }

            return key;
        }

        /// <summary>
        /// Gets the registration instance.
        /// </summary>
        /// <returns></returns>
        private RegistrationInstance GetRegistrationInstance()
        {
            if ( _registrationInstance == null )
            {
                var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

                if ( registrationInstanceId.HasValue )
                {
                    _registrationInstance = new RegistrationInstanceService( RockContext )
                        .Queryable()
                        .Include( a => a.RegistrationTemplate )
                        .Include( a => a.Account )
                        .Include( a => a.RegistrationTemplate.Forms )
                        .Include( a => a.RegistrationTemplate.Forms.Select( s => s.Fields ) )
                        .Where( a => a.Id == registrationInstanceId.Value )
                        .AsNoTracking().FirstOrDefault();

                    if ( _registrationInstance == null )
                    {
                        return null;
                    }

                    // Load the Registration Template.
                    if ( _registrationInstance.RegistrationTemplate == null && _registrationInstance.RegistrationTemplateId > 0 )
                    {
                        _registrationInstance.RegistrationTemplate = new RegistrationTemplateService( RockContext ).Get( _registrationInstance.RegistrationTemplateId );
                    }
                }
            }

            return _registrationInstance;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new EventItemOccurrenceGroupMapService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{EventItemOccurrenceGroupMap.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {EventItemOccurrenceGroupMap.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
