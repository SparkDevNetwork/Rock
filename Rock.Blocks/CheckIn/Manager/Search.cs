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
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.Manager.Search;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Searches the current day's check-in attendance by name (and optionally
    /// by security/tag code) and lists the matching people in a roster grid.
    /// Selecting a row navigates to the configured Person Page.
    /// </summary>

    [DisplayName( "Search" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block used to search current check-in." )]
    [IconCssClass( "ti ti-search" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Person Page",
        Description = "The page used to display a selected person's details.",
        Order = 0,
        Key = AttributeKey.PersonPage )]

    [BooleanField(
        "Search By Code",
        Description = "A flag indicating if security codes should also be evaluated in the search box results.",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.SearchByCode )]

    [AttributeCategoryField(
        "Check-in Roster Alert Icon Category",
        Description = "The Person Attribute category to get the Alert Icon attributes from",
        Key = AttributeKey.CheckInRosterAlertIconCategory,
        DefaultValue = Rock.SystemGuid.Category.PERSON_ATTRIBUTES_CHECK_IN_ROSTER_ALERT_ICON,
        EntityType = typeof( Rock.Model.Person ),
        AllowMultiple = false,
        Order = 2 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "F1754C3E-074D-4651-8A12-8D0E4EFFFB16" )]
    [Rock.SystemGuid.BlockTypeGuid( "72B20276-AD6F-4110-BD73-F4FC3BAAE042" )]
    public class Search : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys for block attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string PersonPage = "PersonPage";
            public const string SearchByCode = "SearchByCode";
            public const string CheckInRosterAlertIconCategory = "CheckInRosterAlertIconCategory";
        }

        /// <summary>
        /// Keys for page (query string) parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string Person = "Person";
        }

        /// <summary>
        /// Keys for the navigation URLs sent to the client.
        /// </summary>
        private static class NavigationUrlKey
        {
            public const string PersonPage = "PersonPage";
        }

        private const int MinimumSearchLength = 3;

        #endregion Keys

        #region RockBlockType Implementation

        /// <inheritdoc />
        public override object GetObsidianBlockInitialization()
        {
            return new ListBlockBox<SearchOptionsBag>
            {
                Options = new SearchOptionsBag
                {
                    IsSearchByCodeEnabled = GetAttributeValue( AttributeKey.SearchByCode ).AsBoolean()
                },
                GridDefinition = GetGridBuilder().BuildDefinition(),
                NavigationUrls = GetBoxNavigationUrls()
            };
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Searches the current day's attendance for people matching the
        /// specified value and returns the grid data for the results.
        /// </summary>
        /// <param name="searchValue">The name or security/tag code to search for.</param>
        /// <returns>The grid data containing the matching attendees.</returns>
        [BlockAction]
        public BlockActionResult GetSearchResults( string searchValue )
        {
            var trimmedSearchValue = searchValue?.Trim();

            // Require a minimum length before searching so short/accidental
            // input does not scan the entire day's attendance.
            if ( trimmedSearchValue.IsNullOrWhiteSpace() || trimmedSearchValue.Length < MinimumSearchLength )
            {
                return ActionOk( GetGridBuilder().Build( new List<SearchAttendeeBag>() ) );
            }

            var attendees = GetAttendees( trimmedSearchValue );
            var alertIconAttributes = GetAlertIconAttributes();
            var rows = attendees.Select( a => MapAttendeeToBag( a, alertIconAttributes ) ).ToList();

            return ActionOk( GetGridBuilder().Build( rows ) );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the navigation URLs sent to the client.
        /// </summary>
        /// <returns>A dictionary of navigation keys to URLs.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.PersonPage] = this.GetLinkedPageUrl( AttributeKey.PersonPage, new Dictionary<string, string>
                {
                    { PageParameterKey.Person, "((Key))" }
                } )
            };
        }

        /// <summary>
        /// Gets the sorted list of attendees matching the search value.
        /// </summary>
        /// <param name="searchValue">The trimmed search value.</param>
        /// <returns>The matching attendees, ordered for display.</returns>
        private IList<RosterAttendee> GetAttendees( string searchValue )
        {
            var startDateTime = RockDateTime.Today;
            var currentDateTime = RockDateTime.Now;

            var attendanceQuery = new AttendanceService( RockContext )
                .Queryable()
                .Where( a =>
                    a.StartDateTime >= startDateTime
                    && a.DidAttend == true
                    && a.StartDateTime <= currentDateTime
                    && a.PersonAliasId.HasValue
                    && a.Occurrence.GroupId.HasValue
                    && a.Occurrence.ScheduleId.HasValue
                    && a.Occurrence.LocationId.HasValue );

            var personIds = GetMatchingPersonIds( searchValue, startDateTime );

            if ( !personIds.Any() )
            {
                return new List<RosterAttendee>();
            }

            // Materialize today's attendance through the roster projection and
            // join the matched person ids in memory. The projection is a
            // constant-shape query, so EF compiles its (large) plan once and
            // reuses it — filtering it per-person instead would defeat that
            // caching and recompile the projection on every search.
            var attendanceList = RosterAttendeeAttendance.Select( attendanceQuery ).ToList();

            var personIdLookup = new HashSet<int>( personIds );
            var matchedAttendances = attendanceList
                .Where( a => personIdLookup.Contains( a.PersonId ) )
                .ToList();

            var attendees = RosterAttendee.GetFromAttendanceList( matchedAttendances );

            return attendees
                .OrderByDescending( a => a.MeetsRosterStatusFilter( RosterStatusFilter.Present ) )
                .ThenByDescending( a => a.CheckInTime )
                .ThenBy( a => a.PersonGuid )
                .ToList();
        }

        /// <summary>
        /// Resolves the ids of the people matching the search value — first by
        /// security/tag code (when enabled) and then by name.
        /// </summary>
        /// <param name="searchValue">The trimmed search value.</param>
        /// <param name="startDateTime">The start of the current day.</param>
        /// <returns>The distinct matching person ids.</returns>
        private List<int> GetMatchingPersonIds( string searchValue, DateTime startDateTime )
        {
            // When enabled, try to match on today's attendance security code
            // first. Filtering on AttendanceCode (Code + a half-open
            // today/tomorrow IssueDateTime range) lets SQL use the unique
            // IX_Code_IssueDateTime index (Code is the leading key) as a tight
            // seek, instead of scanning attendance by StartDateTime.
            if ( GetAttributeValue( AttributeKey.SearchByCode ).AsBoolean() )
            {
                var tomorrow = startDateTime.AddDays( 1 );

                var codePersonIds = new AttendanceService( RockContext )
                    .Queryable()
                    .Where( a =>
                        a.AttendanceCode.IssueDateTime >= startDateTime
                        && a.AttendanceCode.IssueDateTime < tomorrow
                        && a.AttendanceCode.Code == searchValue )
                    .Select( a => a.PersonAlias.PersonId )
                    .Distinct()
                    .ToList();

                if ( codePersonIds.Any() )
                {
                    return codePersonIds;
                }
            }

            // If code matching was disabled or found nobody, search by name.
            // allowFirstNameOnly is true so a first (or partial first) name matches.
            return new PersonService( RockContext )
                .GetByFullName( searchValue, false, false, true, out _ )
                .AsNoTracking()
                .Select( a => a.Id )
                .ToList();
        }

        /// <summary>
        /// Gets the person attributes configured to display as roster alert
        /// icons.
        /// </summary>
        /// <returns>The alert-icon attributes, or an empty list when none are configured.</returns>
        private List<AttributeCache> GetAlertIconAttributes()
        {
            var categoryGuid = GetAttributeValue( AttributeKey.CheckInRosterAlertIconCategory ).AsGuidOrNull();
            if ( !categoryGuid.HasValue )
            {
                return new List<AttributeCache>();
            }

            var categoryId = CategoryCache.GetId( categoryGuid.Value );
            if ( !categoryId.HasValue )
            {
                return new List<AttributeCache>();
            }

            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;

            return AttributeCache.GetByEntityType( personEntityTypeId )
                .Where( a => a.CategoryIds.Contains( categoryId.Value ) )
                .ToList();
        }

        /// <summary>
        /// Maps a <see cref="RosterAttendee"/> to a <see cref="SearchAttendeeBag"/>
        /// for the results grid.
        /// </summary>
        /// <param name="attendee">The attendee to map.</param>
        /// <param name="alertIconAttributes">The alert-icon attributes used to render badges.</param>
        /// <returns>The populated bag.</returns>
        private SearchAttendeeBag MapAttendeeToBag( RosterAttendee attendee, List<AttributeCache> alertIconAttributes )
        {
            // GetBadgesHtml loads each person's alert-icon attribute values on demand.
            return new SearchAttendeeBag
            {
                PersonGuid = attendee.PersonGuid,
                FullName = attendee.FullName,
                Tag = attendee.Tag,
                ServiceTimes = attendee.ServiceTimes,
                PhotoHtml = attendee.GetPersonPhotoImageHtmlTag(),
                NameHtml = attendee.GetAttendeeNameHtml(),
                BadgesHtml = $"<div>{attendee.GetBadgesHtml( alertIconAttributes )}</div>",
                DesktopStatusTagHtml = attendee.GetStatusIconHtmlTag( false ),
                MobileStatusIconHtml = attendee.GetStatusIconHtmlTag( true ),
                MobileTagAndSchedulesHtml = attendee.GetMobileTagAndSchedulesHtml()
            };
        }

        /// <summary>
        /// Builds the grid builder used for the block's grid definition and data
        /// serialization.
        /// </summary>
        /// <returns>The configured grid builder.</returns>
        private GridBuilder<SearchAttendeeBag> GetGridBuilder()
        {
            return new GridBuilder<SearchAttendeeBag>()
                .WithBlock( this )
                .AddTextField( "personGuid", a => a.PersonGuid.ToString() )
                .AddTextField( "fullName", a => a.FullName )
                .AddTextField( "tag", a => a.Tag )
                .AddTextField( "serviceTimes", a => a.ServiceTimes )
                .AddTextField( "photoHtml", a => a.PhotoHtml )
                .AddTextField( "nameHtml", a => a.NameHtml )
                .AddTextField( "badgesHtml", a => a.BadgesHtml )
                .AddTextField( "desktopStatusTagHtml", a => a.DesktopStatusTagHtml )
                .AddTextField( "mobileStatusIconHtml", a => a.MobileStatusIconHtml )
                .AddTextField( "mobileTagAndSchedulesHtml", a => a.MobileTagAndSchedulesHtml );
        }

        #endregion Private Methods
    }
}
