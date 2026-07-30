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

using Rock;
using Rock.Attribute;
using Rock.Crm.ConnectionStatusChangeReport;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.ConnectionStatusChangeReport;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Shows changes of Connection Status for people within a specific period.
    /// </summary>
    [DisplayName( "Connection Status Changes" )]
    [Category( "Connection" )]
    [Description( "Shows changes of Connection Status for people within a specific period." )]
    [IconCssClass( "ti ti-arrows-right-left" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage(
        "Person Detail Page",
        Key = AttributeKey.PersonDetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 0 )]

    [Rock.SystemGuid.EntityTypeGuid( "67FE9B98-2BF4-4FF5-A6B5-54B72CA2D643" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "038F7A89-8299-43A3-AF97-B3DB65462534" )]
    [Rock.SystemGuid.BlockTypeGuid( "FE50DDE5-3D8C-47EC-817D-21348717AD38" )]
    [CustomizedGrid]
    public class ConnectionStatusChangeReport : RockListBlockType<ConnectionStatusChangeReport.StatusChangeRow>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PersonDetailPage = "PersonDetailPage";
        }

        private static class AttributeCategory
        {
            public const string LinkedPages = "Linked Pages";
        }

        private static class NavigationUrlKey
        {
            public const string PersonDetailPage = "PersonDetailPage";
        }

        /// <summary>
        /// Keys for the page parameters that allow a report view to be deep-linked.
        /// </summary>
        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string Period = "Period";
            public const string FromStatusId = "FromStatusId";
            public const string ToStatusId = "ToStatusId";
        }

        /// <summary>
        /// Keys for the filter person preferences.
        /// </summary>
        private static class PreferenceKey
        {
            public const string DateRange = "date-range";
            public const string Campus = "campus";
            public const string FromStatus = "from-status";
            public const string ToStatus = "to-status";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ConnectionStatusChangeReportOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Resolves the effective filter selections for the current request.
        /// </summary>
        /// <remarks>
        /// Selections begin with the individual's saved block preferences and are then overlaid
        /// with any matching page parameters. A page parameter overrides only its own filter, and
        /// only for this request; an omitted parameter leaves the saved value untouched.
        /// </remarks>
        /// <returns>The effective filter selections.</returns>
        private FilterSelections GetFilterSelections()
        {
            var preferences = GetBlockPersonPreferences();

            // Begin with whatever the individual has saved.
            var selections = new FilterSelections
            {
                DateRangeValue = preferences.GetValue( PreferenceKey.DateRange ),
                Campus = GetCampusFromKey( preferences.GetValue( PreferenceKey.Campus ) ),
                FromConnectionStatus = GetConnectionStatusFromKey( preferences.GetValue( PreferenceKey.FromStatus ) ),
                ToConnectionStatus = GetConnectionStatusFromKey( preferences.GetValue( PreferenceKey.ToStatus ) )
            };

            // Overlay any deep-link page parameters, applying only those that are present.
            var periodKey = PageParameter( PageParameterKey.Period );
            if ( periodKey.IsNotNullOrWhiteSpace() )
            {
                // The page parameter and the sliding-date-range preference share an identical
                // format; only the delimiter differs (the URL value uses commas).
                selections.DateRangeValue = new TimePeriod( periodKey, "," ).ToDelimitedString( "|" );
            }

            var campusKey = PageParameter( PageParameterKey.CampusId );
            if ( campusKey.IsNotNullOrWhiteSpace() )
            {
                selections.Campus = GetCampusFromKey( campusKey );
            }

            var fromStatusKey = PageParameter( PageParameterKey.FromStatusId );
            if ( fromStatusKey.IsNotNullOrWhiteSpace() )
            {
                selections.FromConnectionStatus = GetConnectionStatusFromKey( fromStatusKey );
            }

            var toStatusKey = PageParameter( PageParameterKey.ToStatusId );
            if ( toStatusKey.IsNotNullOrWhiteSpace() )
            {
                selections.ToConnectionStatus = GetConnectionStatusFromKey( toStatusKey );
            }

            return selections;
        }

        /// <summary>
        /// Resolves a campus from a key that may be an Id, IdKey, or Guid.
        /// </summary>
        /// <param name="key">The raw key, from a saved preference or a page parameter.</param>
        /// <returns>The matching <see cref="CampusCache"/>, or <see langword="null"/>.</returns>
        private CampusCache GetCampusFromKey( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return CampusCache.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the box options required for the component to render the filter panel.
        /// </summary>
        /// <returns>The options that provide the panel's initial filter selections.</returns>
        private ConnectionStatusChangeReportOptionsBag GetBoxOptions()
        {
            var selections = GetFilterSelections();

            return new ConnectionStatusChangeReportOptionsBag
            {
                DateRange = selections.DateRangeValue,
                Campus = selections.Campus?.ToListItemBag(),
                FromConnectionStatus = selections.FromConnectionStatus?.ToListItemBag(),
                ToConnectionStatus = selections.ToConnectionStatus?.ToListItemBag()
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the grid to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.PersonDetailPage] = this.GetLinkedPageUrl( AttributeKey.PersonDetailPage, "PersonId", "((Key))" )
            };
        }

        /// <summary>
        /// Builds the report settings from the effective filter selections.
        /// </summary>
        /// <returns>The settings used to drive the report builder.</returns>
        private ConnectionStatusChangeReportSettings GetFilterSettings()
        {
            var selections = GetFilterSelections();

            // Default to the current year (the report's long-standing default) when the
            // individual has not selected a date range, so the result set stays bounded.
            var defaultDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Current,
                TimeUnit = TimeUnitType.Year
            };

            var dateRange = selections.DateRangeValue
                .ToSlidingDateRangeBagOrNull()
                .Validate( defaultDateRange )
                .ActualDateRange;

            return new ConnectionStatusChangeReportSettings
            {
                ReportPeriod = new TimePeriod( dateRange?.Start, dateRange?.End ),
                CampusId = selections.Campus?.Id,
                FromConnectionStatusId = selections.FromConnectionStatus?.Id,
                ToConnectionStatusId = selections.ToConnectionStatus?.Id
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<StatusChangeRow> GetListQueryable( RockContext rockContext )
        {
            var settings = GetFilterSettings();

            var report = new ConnectionStatusChangeReportBuilder( rockContext, settings ).CreateReport();
            var changeEvents = report.ChangeEvents ?? new List<ConnectionStatusChangeEventInfo>();

            var rows = changeEvents
                .Select( ( changeEvent, index ) => new StatusChangeRow
                {
                    RowKey = index.ToString(),
                    PersonId = changeEvent.PersonId,
                    FirstName = changeEvent.FirstName,
                    LastName = changeEvent.LastName,
                    PhotoId = changeEvent.PhotoId,
                    Gender = changeEvent.Gender,
                    RecordTypeValueId = changeEvent.RecordTypeValueId,
                    Age = changeEvent.Age,
                    BirthDate = changeEvent.BirthDate,
                    DeceasedDate = changeEvent.DeceasedDate,
                    DateChanged = changeEvent.EventDate,
                    ChangedBy = changeEvent.CreatedBy,
                    OriginalStatus = changeEvent.OldConnectionStatusName,
                    UpdatedStatus = changeEvent.NewConnectionStatusName
                } )
                .ToList();

            return rows.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<StatusChangeRow> GetOrderedListQueryable( IQueryable<StatusChangeRow> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.DateChanged );
        }

        /// <inheritdoc/>
        protected override GridBuilder<StatusChangeRow> GetGridBuilder()
        {
            return new GridBuilder<StatusChangeRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.RowKey )
                .AddTextField( "personIdKey", a => a.PersonId.AsIdKey() )
                .AddPersonField( "person", a => GetGridPerson( a ) )
                .AddDateTimeField( "dateChanged", a => a.DateChanged )
                .AddTextField( "changedBy", a => a.ChangedBy )
                .AddTextField( "originalStatus", a => a.OriginalStatus )
                .AddTextField( "updatedStatus", a => a.UpdatedStatus );
        }

        /// <summary>
        /// Builds the lightweight <see cref="Person"/> used to populate the grid's person cell.
        /// Only the scalar fields that <see cref="Person.PhotoUrl"/> reads are set (including a
        /// precalculated <see cref="Person.AgeClassification"/>), so rendering the avatar never
        /// triggers a database lookup.
        /// </summary>
        /// <param name="row">The row whose person should be built.</param>
        /// <returns>A <see cref="Person"/> suitable for the grid's person field.</returns>
        private static Person GetGridPerson( StatusChangeRow row )
        {
            var ageClassification = ( row.Age.HasValue && row.Age.Value < 18 )
                ? AgeClassification.Child
                : AgeClassification.Adult;

            var person = new Person
            {
                Id = row.PersonId,
                NickName = row.FirstName,
                LastName = row.LastName,
                PhotoId = row.PhotoId,
                Gender = row.Gender,
                RecordTypeValueId = row.RecordTypeValueId,
                AgeClassification = ageClassification,
                DeceasedDate = row.DeceasedDate
            };

            person.SetBirthDate( row.BirthDate );

            return person;
        }

        /// <summary>
        /// Resolves a connection status defined value from a page-parameter key that may be a
        /// defined-value Id or Guid.
        /// </summary>
        /// <param name="key">The raw page-parameter value.</param>
        /// <returns>The matching <see cref="DefinedValueCache"/>, or <see langword="null"/>.</returns>
        private static DefinedValueCache GetConnectionStatusFromKey( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var guid = key.AsGuidOrNull();
            if ( guid.HasValue )
            {
                return DefinedValueCache.Get( guid.Value );
            }

            var id = key.AsIntegerOrNull();
            if ( id.HasValue )
            {
                return DefinedValueCache.Get( id.Value );
            }

            return null;
        }

        #endregion Methods

        #region Support Classes

        /// <summary>
        /// The effective filter selections for a single request, resolved from the individual's
        /// saved preferences and overlaid with any deep-link page parameters.
        /// </summary>
        private class FilterSelections
        {
            /// <summary>
            /// Gets or sets the date range as a sliding-date-range delimited string.
            /// </summary>
            public string DateRangeValue { get; set; }

            /// <summary>
            /// Gets or sets the selected campus, or <see langword="null"/> for all campuses.
            /// </summary>
            public CampusCache Campus { get; set; }

            /// <summary>
            /// Gets or sets the original (from) connection status, or <see langword="null"/> for any.
            /// </summary>
            public DefinedValueCache FromConnectionStatus { get; set; }

            /// <summary>
            /// Gets or sets the updated (to) connection status, or <see langword="null"/> for any.
            /// </summary>
            public DefinedValueCache ToConnectionStatus { get; set; }
        }

        /// <summary>
        /// A single connection-status-change event projected for the grid.
        /// </summary>
        public class StatusChangeRow
        {
            /// <summary>
            /// Gets or sets the unique, stable key for this grid row.
            /// </summary>
            public string RowKey { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person whose status changed.
            /// </summary>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the person's first name.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the person's last name.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person's photo, if any.
            /// </summary>
            public int? PhotoId { get; set; }

            /// <summary>
            /// Gets or sets the person's gender.
            /// </summary>
            public Gender Gender { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person's record type.
            /// </summary>
            public int? RecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's age in years, if known.
            /// </summary>
            public int? Age { get; set; }

            /// <summary>
            /// Gets or sets the person's birth date, if known.
            /// </summary>
            public DateTime? BirthDate { get; set; }

            /// <summary>
            /// Gets or sets the person's deceased date, if any.
            /// </summary>
            public DateTime? DeceasedDate { get; set; }

            /// <summary>
            /// Gets or sets the date and time the status change occurred.
            /// </summary>
            public DateTime DateChanged { get; set; }

            /// <summary>
            /// Gets or sets the name of the person who recorded the change.
            /// </summary>
            public string ChangedBy { get; set; }

            /// <summary>
            /// Gets or sets the connection status the person changed from.
            /// </summary>
            public string OriginalStatus { get; set; }

            /// <summary>
            /// Gets or sets the connection status the person changed to.
            /// </summary>
            public string UpdatedStatus { get; set; }
        }

        #endregion Support Classes
    }
}
