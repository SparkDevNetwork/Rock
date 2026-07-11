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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.CalendarTypes;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the calendars that the current person is authorized to view.
    /// </summary>

    [DisplayName( "Calendar Types" )]
    [Category( "Event" )]
    [Description( "Displays the calendars that user is authorized to view." )]
    [IconCssClass( "ti ti-border-all" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "Page used to view details of an event calendar.",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [LinkedPage( "Calendar Attributes Page",
        Description = "Page used to configure attributes for event calendars.",
        Key = AttributeKey.CalendarAttributesPage,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "26A73629-260C-4883-84A3-B7F62C8DF727" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "EFD85CF6-0444-4976-8A73-0E587803F3DF" )]
    [Rock.SystemGuid.BlockTypeGuid( "041B5C23-5F1F-4B02-A767-FB7F4B1A5345" )]
    public class CalendarTypes : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string CalendarAttributesPage = "CalendarAttributesPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string CalendarAttributesPage = "CalendarAttributesPage";
        }

        private static class PageParameterKey
        {
            public const string EventCalendarId = "EventCalendarId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<CalendarTypesBag, CalendarTypesOptionsBag>
            {
                Bag = new CalendarTypesBag
                {
                    Calendars = GetCalendars(),
                    CanAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
                },
                Options = new CalendarTypesOptionsBag(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            return box;
        }

        /// <summary>
        /// Gets the event calendar tiles the current person is authorized to view,
        /// ordered by name. Both active and inactive calendars are included.
        /// </summary>
        /// <returns>The list of event calendar tiles.</returns>
        private List<CalendarTypesCalendarBag> GetCalendars()
        {
            var currentPerson = RequestContext.CurrentPerson;

            return EventCalendarCache.All()
                .OrderBy( c => c.Name )
                .Where( c => c.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Select( c => new CalendarTypesCalendarBag
                {
                    IdKey = c.IdKey,
                    Name = c.Name,
                    IconCssClass = c.IconCssClass
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the navigation URLs required for the block to operate.
        /// </summary>
        /// <returns>A dictionary of navigation URL keys and values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.EventCalendarId, "((Key))" ),
                [NavigationUrlKey.CalendarAttributesPage] = this.GetLinkedPageUrl( AttributeKey.CalendarAttributesPage )
            };
        }

        #endregion Methods
    }
}
