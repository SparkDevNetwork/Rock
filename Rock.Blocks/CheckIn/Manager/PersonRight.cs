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

using Newtonsoft.Json;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.CheckIn.Manager.PersonRight;
using Rock.ViewModels.Crm;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Displays a checked-in person's recent attendances, badges, and
    /// Reprint Labels action inside the Check-in Manager. Sits opposite the
    /// PersonLeft profile card on the Check-in Manager Person Profile page.
    /// </summary>

    [DisplayName( "Person Recent Attendances" )]
    [Category( "Check-in > Manager" )]
    [Description( "Shows most recent attendances for a person." )]
    [IconCssClass( "ti ti-history" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [SecurityAction( SecurityActionKey.ReprintLabels, "The roles and/or users that can reprint labels for the selected person." )]

    #region Block Attributes

    [LinkedPage(
        "Manager Page",
        Key = AttributeKey.ManagerPage,
        Description = "Page used to manage check-in locations",
        IsRequired = true,
        Order = 0 )]

    [LinkedPage(
        "Attendance Detail Page",
        Key = AttributeKey.AttendanceDetailPage,
        Description = "Page to show details of an attendance.",
        DefaultValue = Rock.SystemGuid.Page.CHECK_IN_MANAGER_ATTENDANCE_DETAIL,
        IsRequired = true,
        Order = 1 )]

    [BooleanField(
        "Allow Label Reprinting",
        Key = AttributeKey.AllowLabelReprinting,
        Description = "Determines if reprinting labels should be allowed.",
        DefaultBooleanValue = false,
        Category = "Manager Settings",
        Order = 5 )]

    [BadgesField(
        "Badges - Left",
        Key = AttributeKey.BadgesLeft,
        Description = "The badges to display on the left side of the badge bar.",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Badge.FAMILY_ATTENDANCE,
        Order = 6 )]

    [BadgesField(
        "Badges - Right",
        Key = AttributeKey.BadgesRight,
        Description = "The badges to display on the right side of the badge bar.",
        IsRequired = false,
        DefaultValue =
            Rock.SystemGuid.Badge.LAST_VISIT_ON_EXTERNAL_SITE + ","
            + Rock.SystemGuid.Badge.FAMILY_16_WEEK_ATTENDANCE + ","
            + Rock.SystemGuid.Badge.BAPTISM + ","
            + Rock.SystemGuid.Badge.IN_SERVING_TEAM,
        Order = 7 )]

    [LinkedPage(
        "Attendance History Page",
        Key = AttributeKey.PersonAttendanceHistoryPage,
        Description = "Page to shows a history of changes to person's attendances.",
        DefaultValue = Rock.SystemGuid.Page.CHECK_IN_MANAGER_PERSON_ATTENDANCE_CHANGE_HISTORY,
        IsRequired = true,
        Order = 8 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "5E466752-CEB8-47CC-A986-9CBEDB268056" )]
    [Rock.SystemGuid.BlockTypeGuid( "486892AE-B5FD-447C-9E27-15A4BF3667CB" )]
    public class PersonRight : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ManagerPage = "ManagerPage";
            public const string AllowLabelReprinting = "AllowLabelReprinting";
            public const string BadgesLeft = "BadgesLeft";
            public const string BadgesRight = "BadgesRight";
            public const string AttendanceDetailPage = "AttendanceDetailPage";
            public const string PersonAttendanceHistoryPage = "PersonAttendanceHistoryPage";
        }

        private static class SecurityActionKey
        {
            public const string ReprintLabels = "ReprintLabels";
        }

        private static class PageParameterKey
        {
            /// <summary>
            /// The person Guid (legacy).
            /// </summary>
            public const string PersonGuid = "Person";

            /// <summary>
            /// A page parameter that accepts an integer Id, an IdKey, or a Guid.
            /// </summary>
            public const string PersonId = "PersonId";

            /// <summary>
            /// The area Guid, carried forward on links that navigate back to
            /// the manager page.
            /// </summary>
            public const string AreaGuid = "Area";

            /// <summary>
            /// The location identifier, populated on the manager-page link
            /// for a currently-active attendance.
            /// </summary>
            public const string LocationId = "LocationId";

            /// <summary>
            /// The attendance identifier. When PersonId is absent, the
            /// block resolves the person from this attendance and redirects.
            /// </summary>
            public const string AttendanceId = "AttendanceId";
        }

        private static class ReprintMode
        {
            public const string Legacy = "Legacy";
            public const string NextGen = "NextGen";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            if ( !BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return new PersonRightInitializationBox { IsVisible = false };
            }

            if ( TryRedirectFromAttendance() )
            {
                return new PersonRightInitializationBox { IsVisible = false };
            }

            var person = ResolvePerson();
            if ( person == null )
            {
                return new PersonRightInitializationBox { IsVisible = false };
            }

            var box = new PersonRightInitializationBox
            {
                IsVisible = true,
                GenderHtml = BuildGenderHtml( person ),
                AgeHtml = BuildAgeHtml( person ),
                GradeHtml = BuildGradeHtml( person ),
                LeftBadges = BuildBadges( person, AttributeKey.BadgesLeft ),
                RightBadges = BuildBadges( person, AttributeKey.BadgesRight ),
                IsReprintLabelsVisible = GetAttributeValue( AttributeKey.AllowLabelReprinting ).AsBoolean()
                    && BlockCache.IsAuthorized( SecurityActionKey.ReprintLabels, RequestContext.CurrentPerson ),
                AttendanceHistoryUrl = BuildAttendanceHistoryUrl( person ),
                Attendances = BuildAttendanceRows( person )
            };

            return box;
        }

        /// <summary>
        /// Resolves the target person from the page parameters, accepting the
        /// modern IdKey/Id form via <c>PersonId</c> as well as the legacy
        /// Guid-only <c>Person</c> parameter.
        /// </summary>
        private Person ResolvePerson()
        {
            var personService = new PersonService( RockContext );
            int? personId = null;

            var personKey = PageParameter( PageParameterKey.PersonId );
            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                personId = personService.Get( personKey, !PageCache.Layout.Site.DisablePredictableIds )?.Id;
            }

            if ( !personId.HasValue )
            {
                var personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();
                if ( personGuid.HasValue )
                {
                    personId = personService.GetId( personGuid.Value );
                }
            }

            if ( !personId.HasValue )
            {
                return null;
            }

            return personService.Queryable( true, true )
                .Include( a => a.Aliases )
                .FirstOrDefault( a => a.Id == personId.Value );
        }

        /// <summary>
        /// When the URL supplies an <c>AttendanceId</c> without a
        /// <c>PersonId</c>, resolves the person's IdKey and issues a redirect
        /// that adds <c>PersonId=&lt;idKey&gt;</c> to the URL. Matches the
        /// PersonLeft block's behavior so sibling blocks on the page always
        /// see PersonId.
        /// </summary>
        private bool TryRedirectFromAttendance()
        {
            if ( PageParameter( PageParameterKey.PersonId ).IsNotNullOrWhiteSpace()
                || PageParameter( PageParameterKey.PersonGuid ).IsNotNullOrWhiteSpace() )
            {
                return false;
            }

            var attendanceId = PageParameter( PageParameterKey.AttendanceId ).AsIntegerOrNull();
            if ( !attendanceId.HasValue )
            {
                return false;
            }

            var personId = new AttendanceService( RockContext )
                .GetSelect( attendanceId.Value, a => ( int? ) a.PersonAlias.PersonId );

            if ( !personId.HasValue )
            {
                return false;
            }

            var personIdKey = IdHasher.Instance.GetHash( personId.Value );
            if ( personIdKey.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var currentUrl = this.GetCurrentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.PersonId] = personIdKey
            } );

            RequestContext.Response.RedirectToUrl( currentUrl );
            return true;
        }

        /// <summary>
        /// Builds the gender-letter widget markup ("M" or "F" wrapped in the
        /// WebForms-style text-semibold uppercase container). Returns an
        /// empty string when the person's gender is Unknown.
        /// </summary>
        private string BuildGenderHtml( Person person )
        {
            if ( person.Gender == Gender.Unknown )
            {
                return string.Empty;
            }

            return $"<div class=\"text-semibold text-uppercase\">{person.Gender.ConvertToString().Substring( 0, 1 )}</div>";
        }

        /// <summary>
        /// Builds the age widget markup. When the birth year is known the
        /// widget shows age-in-years over the short birth date. When the
        /// year is unknown the widget shows the month-and-day (e.g. "Nov 8")
        /// as the primary term with a "Birthday" subtitle -- matching the
        /// Bio block's treatment and avoiding the DateTime.MinValue-Year
        /// artifact "11/8/0001".
        /// </summary>
        private string BuildAgeHtml( Person person )
        {
            if ( !person.BirthDate.HasValue )
            {
                return string.Empty;
            }

            if ( person.BirthYear.HasValue && person.BirthYear != DateTime.MinValue.Year )
            {
                return $"<div class=\"text-semibold\">{person.BirthDate.Value.Age()}yrs</div>"
                    + $"<div class=\"text-sm text-muted\">{person.BirthDate.Value.ToShortDateString()}</div>";
            }

            return $"<div class=\"text-semibold\">{person.BirthDate.Value.ToString( "MMM d" )}</div>"
                + "<div class=\"text-sm text-muted\">Birthday</div>";
        }

        /// <summary>
        /// Builds the grade widget markup. Splits the person's formatted
        /// grade into a primary word (large) and remainder (small subtitle).
        /// Handles the UK "Year N" case where the number is the primary
        /// word.
        /// </summary>
        private string BuildGradeHtml( Person person )
        {
            var grade = person.GradeFormatted;
            if ( grade.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var gradeParts = grade.Split( ' ' );
            if ( gradeParts.Length < 2 )
            {
                return grade;
            }

            var firstWord = gradeParts[0];
            var remainderWords = gradeParts.Skip( 1 ).ToList().AsDelimited( " " );

            /*
                MDP 2020-10-21 (at request of GJ)

                Special case if formatted grade is 'Year 1', 'Year 2', etc.
                (see https://separatedbyacommonlanguage.blogspot.com/2006/12/types-of-schools-school-years.html)
                Make the number the top word so the widget still reads "1" above "Year".
            */
            if ( firstWord.Equals( "Year", StringComparison.OrdinalIgnoreCase ) )
            {
                return $"<div class=\"text-semibold\">{remainderWords}</div><div class=\"text-sm text-muted\">{firstWord}</div>";
            }

            return $"<div class=\"text-semibold\">{firstWord}</div><div class=\"text-sm text-muted\">{remainderWords}</div>";
        }

        /// <summary>
        /// Renders every badge configured for the given attribute against
        /// the person and returns the badges that produced HTML or
        /// JavaScript content.
        /// </summary>
        private List<RenderedBadgeBag> BuildBadges( Person person, string attributeKey )
        {
            var badgeGuids = GetAttributeValue( attributeKey ).SplitDelimitedValues().AsGuidList();
            if ( !badgeGuids.Any() )
            {
                return new List<RenderedBadgeBag>();
            }

            var currentPerson = RequestContext.CurrentPerson;

            var badges = badgeGuids
                .Select( g => BadgeCache.Get( g ) )
                .Where( b => b != null && b.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .OrderBy( b => b.Order )
                .ToList();

            return badges
                .Select( b => b.RenderBadge( person ) )
                .Where( b => b.Html.IsNotNullOrWhiteSpace() || b.JavaScript.IsNotNullOrWhiteSpace() )
                .ToList();
        }

        /// <summary>
        /// Builds the Attendance History link. Passes the person's IdKey so
        /// the target block can accept it directly.
        /// </summary>
        private string BuildAttendanceHistoryUrl( Person person )
        {
            return this.GetLinkedPageUrl( AttributeKey.PersonAttendanceHistoryPage, new Dictionary<string, string>
            {
                [PageParameterKey.PersonId] = person.IdKey
            } );
        }

        /// <summary>
        /// Builds the 20-most-recent attendance rows for the check-in
        /// history grid. Rows carry the pre-rendered HTML for each visible
        /// cell along with a navigation URL to the Attendance Detail page.
        /// </summary>
        private List<PersonRightAttendanceRowBag> BuildAttendanceRows( Person person )
        {
            var personAliasIds = person.Aliases.Select( a => a.Id ).ToList();

            // Filter check-in schedules via the navigation property rather
            // than a materialized List<int> of every check-in schedule id.
            // The old code inlined thousands of ids into a "WHERE IN (...)"
            // list -- this generates a clean JOIN + WHERE on Schedule.
            var attendanceRows = new AttendanceService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a =>
                    a.PersonAliasId.HasValue
                    && personAliasIds.Contains( a.PersonAliasId.Value )
                    && a.Occurrence.ScheduleId.HasValue
                    && a.Occurrence.GroupId.HasValue
                    && a.Occurrence.LocationId.HasValue
                    && a.DidAttend.HasValue
                    && a.DidAttend.Value
                    && a.Occurrence.Schedule.CheckInStartOffsetMinutes.HasValue )
                .OrderByDescending( a => a.StartDateTime )
                .Take( 20 )
                .Select( a => new
                {
                    a.Id,
                    a.StartDateTime,
                    a.EndDateTime,
                    a.CampusId,
                    a.CheckedInByPersonAliasId,
                    OccurrenceDate = a.Occurrence.OccurrenceDate,
                    ScheduleId = a.Occurrence.ScheduleId.Value,
                    LocationId = a.Occurrence.LocationId.Value,
                    LocationName = a.Occurrence.Location.Name,
                    GroupName = a.Occurrence.Group.Name,
                    Code = a.AttendanceCode != null ? a.AttendanceCode.Code : null,
                    SearchResultGroupName = a.SearchResultGroup != null ? a.SearchResultGroup.Name : null
                } )
                .ToList();

            if ( !attendanceRows.Any() )
            {
                return new List<PersonRightAttendanceRowBag>();
            }

            // Only fetch the Schedule entities the top-20 rows actually need
            // (for the in-memory IsCurrentlyCheckedIn computation and their
            // display names). Bounded to at most 20 ids -- a safe IN clause.
            var neededScheduleIds = attendanceRows.Select( r => r.ScheduleId ).Distinct().ToList();
            var schedulesById = new ScheduleService( RockContext )
                .Queryable().AsNoTracking()
                .Where( s => neededScheduleIds.Contains( s.Id ) )
                .ToList()
                .ToDictionary( s => s.Id );

            // Sort by OccurrenceDate then Schedule start-time (in-memory,
            // using the schedule dictionary since StartTimeOfDay is a
            // [NotMapped] computed property EF cannot translate into SQL).
            attendanceRows = attendanceRows
                .OrderByDescending( a => a.OccurrenceDate )
                .ThenByDescending( a => schedulesById.TryGetValue( a.ScheduleId, out var s ) ? s.StartTimeOfDay : TimeSpan.Zero )
                .ToList();

            // Batch the "checked in by" person lookup instead of firing one
            // PersonAliasService.GetPerson query per row inside the render loop.
            var checkedInByAliasIds = attendanceRows
                .Where( a => a.CheckedInByPersonAliasId.HasValue )
                .Select( a => a.CheckedInByPersonAliasId.Value )
                .Distinct()
                .ToList();

            var checkedInByPersonsByAliasId = new Dictionary<int, (Guid Guid, string FullName)>();
            if ( checkedInByAliasIds.Any() )
            {
                checkedInByPersonsByAliasId = new PersonAliasService( RockContext )
                    .Queryable().AsNoTracking()
                    .Where( pa => checkedInByAliasIds.Contains( pa.Id ) )
                    .Select( pa => new
                    {
                        AliasId = pa.Id,
                        pa.Person.Guid,
                        pa.Person.NickName,
                        pa.Person.LastName
                    } )
                    .ToList()
                    .ToDictionary(
                        pa => pa.AliasId,
                        pa => ( pa.Guid, $"{pa.NickName} {pa.LastName}".Trim() ) );
            }

            // Include both PersonId and AttendanceId on the row URL so the
            // Attendance Detail page (and any sibling PersonLeft block on the
            // same page) can resolve the person directly without a second
            // AttendanceId-only redirect.
            var attendanceDetailPageUrlTemplate = this.GetLinkedPageUrl( AttributeKey.AttendanceDetailPage, new Dictionary<string, string>
            {
                [PageParameterKey.PersonId] = person.IdKey,
                [PageParameterKey.AttendanceId] = "((AttendanceId))"
            } );

            var managerPageQuery = new Dictionary<string, string>
            {
                [PageParameterKey.LocationId] = string.Empty
            };

            var areaGuid = PageParameter( PageParameterKey.AreaGuid );
            if ( areaGuid.IsNotNullOrWhiteSpace() )
            {
                managerPageQuery.Add( PageParameterKey.AreaGuid, areaGuid );
            }

            var rows = new List<PersonRightAttendanceRowBag>();

            foreach ( var attendance in attendanceRows )
            {
                // Compute IsCurrentlyCheckedIn against the schedule dictionary
                // we already loaded above -- no lazy loads on Occurrence.Schedule.
                schedulesById.TryGetValue( attendance.ScheduleId, out var schedule );
                var isActive = Attendance.CalculateIsCurrentlyCheckedIn(
                    attendance.StartDateTime,
                    attendance.EndDateTime,
                    attendance.CampusId,
                    schedule );

                var scheduleName = schedule?.Name ?? string.Empty;
                var locationName = attendance.LocationName ?? string.Empty;
                var groupName = attendance.GroupName ?? string.Empty;
                var code = attendance.Code ?? string.Empty;
                var searchResultGroupName = attendance.SearchResultGroupName ?? string.Empty;

                var whenHtml = $"<span class=\"text-sm\">{attendance.StartDateTime.ToShortDateString()}</span>"
                    + $"<span class=\"d-block text-sm text-muted\">{scheduleName}</span>";

                if ( attendance.CheckedInByPersonAliasId.HasValue
                    && checkedInByPersonsByAliasId.TryGetValue( attendance.CheckedInByPersonAliasId.Value, out var checkedInByPerson ) )
                {
                    var byUrl = this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.PersonGuid] = checkedInByPerson.Guid.ToString()
                    } );

                    whenHtml += $"<br /><a href=\"{byUrl}\">by: {checkedInByPerson.FullName}</a>";
                }

                string locationHtml;
                if ( isActive )
                {
                    managerPageQuery[PageParameterKey.LocationId] = attendance.LocationId.ToString();
                    var managerUrl = this.GetLinkedPageUrl( AttributeKey.ManagerPage, managerPageQuery );
                    locationHtml = $"<span class=\"text-sm\"><a href=\"{managerUrl}\">{locationName}</a></span>"
                        + $"<span class=\"d-block text-sm text-muted\">{groupName}</span>";
                }
                else
                {
                    locationHtml = $"<span class=\"text-sm\">{locationName}</span>"
                        + $"<span class=\"d-block text-sm text-muted\">{groupName}</span>";
                }

                var codeHtml = isActive
                    ? $"{code} <span class=\"label label-success align-middle\">Current</span>"
                    : code;

                var idKey = Rock.Utility.IdHasher.Instance.GetHash( attendance.Id );
                var rowUrl = attendanceDetailPageUrlTemplate.Replace( "((AttendanceId))", idKey );

                rows.Add( new PersonRightAttendanceRowBag
                {
                    IdKey = idKey,
                    WhenHtml = whenHtml,
                    LocationHtml = locationHtml,
                    CodeHtml = codeHtml,
                    SearchResultGroupName = searchResultGroupName,
                    IsActive = isActive,
                    RowUrl = rowUrl
                } );
            }

            return rows;
        }

        /// <summary>
        /// Resolves the currently-selected person Id for the reprint-labels
        /// flow. Uses the PersonId page parameter (int, IdKey, or Guid)
        /// first, falling back to the legacy Person Guid parameter.
        /// </summary>
        private int? ResolvePersonIdForReprint()
        {
            var personService = new PersonService( RockContext );

            var personKey = PageParameter( PageParameterKey.PersonId );
            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                var id = personService.Get( personKey, !PageCache.Layout.Site.DisablePredictableIds )?.Id;
                if ( id.HasValue )
                {
                    return id;
                }
            }

            var personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();
            if ( personGuid.HasValue )
            {
                return personService.GetId( personGuid.Value );
            }

            return null;
        }

        /// <summary>
        /// Builds the printer dropdown option list. Includes a leading empty
        /// option, a "(local printer)" entry when the browser reported a
        /// Zebra client printer, and every configured printer device.
        /// </summary>
        private List<ListItemBag> GetPrinterOptions( bool hasClientPrinter )
        {
            var options = new List<ListItemBag>
            {
                new ListItemBag { Text = string.Empty, Value = string.Empty }
            };

            if ( hasClientPrinter )
            {
                options.Add( new ListItemBag { Text = "local printer", Value = Guid.Empty.ToString() } );
            }

            var printers = new DeviceService( RockContext )
                .GetByDeviceTypeGuid( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ) )
                .OrderBy( d => d.Name )
                .Select( d => new { d.Name, d.Guid } )
                .ToList();

            foreach ( var printer in printers )
            {
                options.Add( new ListItemBag { Text = printer.Name, Value = printer.Guid.ToString() } );
            }

            return options;
        }

        /// <summary>
        /// Splits a delimited list of attendance IdKeys into the integer
        /// attendance ids expected by the Zebra print helpers.
        /// </summary>
        private static List<int> ResolveAttendanceIds( List<string> attendanceIdKeys )
        {
            if ( attendanceIdKeys == null || !attendanceIdKeys.Any() )
            {
                return new List<int>();
            }

            return attendanceIdKeys
                .Select( key => IdHasher.Instance.GetId( key ) ?? key.AsIntegerOrNull() )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Builds the reprint-labels modal data for the currently-active
        /// attendances belonging to the person. Prefers legacy labels;
        /// falls back to next-gen labels; returns an error message when
        /// neither has any options.
        /// </summary>
        [BlockAction]
        public BlockActionResult ShowReprintLabelsModal( PersonRightShowReprintLabelsRequestBag bag, List<string> attendanceIdKeys )
        {
            if ( !GetAttributeValue( AttributeKey.AllowLabelReprinting ).AsBoolean()
                || !BlockCache.IsAuthorized( SecurityActionKey.ReprintLabels, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to reprint labels." );
            }

            var personId = ResolvePersonIdForReprint();
            if ( !personId.HasValue )
            {
                return ActionOk( new PersonRightReprintModalDataBag
                {
                    ErrorMessage = "No person was found."
                } );
            }

            var attendanceIds = ResolveAttendanceIds( attendanceIdKeys );
            if ( !attendanceIds.Any() )
            {
                return ActionOk( new PersonRightReprintModalDataBag
                {
                    ErrorMessage = "No labels were found for re-printing."
                } );
            }

            var hasClientPrinter = bag?.HasClientPrinter ?? false;
            var printers = GetPrinterOptions( hasClientPrinter );

            // Pre-select the printer the user last picked (persisted to the
            // check-in manager cookie by every successful print). Only pass
            // the guid through if it exists in the current options list --
            // otherwise the dropdown would show blank anyway.
            var lastPrinterGuid = CheckinManagerHelper
                .GetCheckinManagerConfigurationFromCookie()
                ?.LabelPrinterGuid;
            var selectedPrinterGuid = lastPrinterGuid.HasValue
                && printers.Any( p => p.Value == lastPrinterGuid.Value.ToString() )
                ? lastPrinterGuid.Value.ToString()
                : null;

            // Legacy labels take precedence when they exist. Only fall
            // back to next-gen labels when the legacy set is empty.
            var legacyLabels = ZebraPrint.GetLabelTypesForPerson( personId.Value, attendanceIds );
            if ( legacyLabels != null && legacyLabels.Any() )
            {
                var labelBags = legacyLabels
                    .OrderBy( l => l.Name )
                    .Select( l => new ListItemBag
                    {
                        Text = l.Name,
                        Value = l.FileGuid.ToString()
                    } )
                    .ToList();

                return ActionOk( new PersonRightReprintModalDataBag
                {
                    Mode = ReprintMode.Legacy,
                    Labels = labelBags,
                    Printers = printers,
                    SelectedPrinterGuid = selectedPrinterGuid
                } );
            }

            var nextGenLabels = ZebraPrint.GetReprintNextGenLabelTypes( attendanceIds );
            if ( nextGenLabels == null || !nextGenLabels.Any() )
            {
                return ActionOk( new PersonRightReprintModalDataBag
                {
                    ErrorMessage = "No labels were found for re-printing."
                } );
            }

            return ActionOk( new PersonRightReprintModalDataBag
            {
                Mode = ReprintMode.NextGen,
                Labels = nextGenLabels,
                Printers = printers,
                SelectedPrinterGuid = selectedPrinterGuid
            } );
        }

        /// <summary>
        /// Prints the selected legacy labels for the given attendance ids
        /// on the selected printer (server or local client). Persists the
        /// printer choice as a cookie.
        /// </summary>
        [BlockAction]
        public BlockActionResult PrintLegacyLabels( PersonRightPrintLegacyLabelsRequestBag bag, List<string> attendanceIdKeys )
        {
            if ( !GetAttributeValue( AttributeKey.AllowLabelReprinting ).AsBoolean()
                || !BlockCache.IsAuthorized( SecurityActionKey.ReprintLabels, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to reprint labels." );
            }

            if ( bag == null || bag.FileGuids == null || !bag.FileGuids.Any() )
            {
                return ActionBadRequest( "Please select at least one label." );
            }

            if ( !bag.PrinterGuid.HasValue )
            {
                return ActionBadRequest( "Please select a printer." );
            }

            var personId = ResolvePersonIdForReprint();
            if ( !personId.HasValue )
            {
                return ActionBadRequest( "No person was found." );
            }

            var attendanceIds = ResolveAttendanceIds( attendanceIdKeys );

            ReprintLabelOptions reprintLabelOptions;
            if ( bag.PrinterGuid.Value == Guid.Empty )
            {
                // The empty Guid is the "(local printer)" sentinel.
                reprintLabelOptions = new ReprintLabelOptions
                {
                    PrintFrom = PrintFrom.Client
                };
            }
            else
            {
                var printerIPAddress = new DeviceService( RockContext )
                    .GetSelect( bag.PrinterGuid.Value, s => s.IPAddress );

                reprintLabelOptions = new ReprintLabelOptions
                {
                    PrintFrom = PrintFrom.Server,
                    ServerPrinterIPAddress = printerIPAddress
                };
            }

            CheckinManagerHelper.SaveSelectedLabelPrinterToCookie( bag.PrinterGuid );

            var (messages, clientLabels) = ZebraPrint.ReprintZebraLabels( bag.FileGuids, personId.Value, attendanceIds, reprintLabelOptions );

            // When any labels are meant for the client-side Zebra plugin, the
            // WebForms flow prefixes each LabelFile with the request root and
            // ships the JSON down for the plugin to consume. We do the same
            // and return the JSON so the client can invoke the plugin.
            string clientLabelsJson = null;
            if ( clientLabels != null && clientLabels.Any() )
            {
                var urlRoot = RequestContext.RootUrlPath ?? string.Empty;
                foreach ( var label in clientLabels )
                {
                    label.LabelFile = urlRoot + label.LabelFile;
                }

                clientLabelsJson = clientLabels
                    .OrderBy( l => l.PersonId )
                    .ThenBy( l => l.Order )
                    .ToList()
                    .ToJson();
            }

            return ActionOk( new PersonRightPrintResultBag
            {
                Message = messages != null && messages.Any() ? string.Join( "<br>", messages ) : "Labels printed.",
                ClientLabelsJson = clientLabelsJson
            } );
        }

        /// <summary>
        /// Prints the selected next-gen labels for the given attendance ids
        /// on the selected printer. When the printer is a client-side
        /// printer, returns the JSON payload the WebView native bridge
        /// (<c>window.RockCheckinNative.PrintV2Labels</c>) should print.
        /// </summary>
        [BlockAction]
        public BlockActionResult PrintNextGenLabels( PersonRightPrintNextGenLabelsRequestBag bag, List<string> attendanceIdKeys )
        {
            if ( !GetAttributeValue( AttributeKey.AllowLabelReprinting ).AsBoolean()
                || !BlockCache.IsAuthorized( SecurityActionKey.ReprintLabels, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to reprint labels." );
            }

            if ( bag == null || bag.LabelTypeValues == null || !bag.LabelTypeValues.Any() )
            {
                return ActionBadRequest( "Please select at least one label." );
            }

            if ( !bag.PrinterGuid.HasValue )
            {
                return ActionBadRequest( "Please select a printer." );
            }

            var attendanceIds = ResolveAttendanceIds( attendanceIdKeys );

            var printer = DeviceCache.Get( bag.PrinterGuid.Value );
            var printFrom = printer != null ? PrintFrom.Server : PrintFrom.Client;

            ZebraPrint.TryReprintNextGenLabels( attendanceIds, null, printer, printFrom, bag.LabelTypeValues, out var messages, out var clientLabels );

            string clientLabelsJson = null;
            if ( clientLabels != null && clientLabels.Any() )
            {
                clientLabelsJson = JsonConvert.SerializeObject( clientLabels );
            }

            var message = messages != null && messages.Any()
                ? string.Join( "<br>", messages )
                : "Labels printed.";

            return ActionOk( new PersonRightPrintResultBag
            {
                Message = message,
                ClientLabelsJson = clientLabelsJson
            } );
        }

        #endregion Block Actions
    }
}
