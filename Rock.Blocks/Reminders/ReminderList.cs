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
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Reminders.ReminderList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Reminders
{
    /// <summary>
    /// Displays the current person's reminders as a card list with inline
    /// Reschedule / Reassign / Mark Complete / Delete actions and a View Options
    /// modal for filtering and sorting.
    /// </summary>
    [DisplayName( "Reminder List" )]
    [Category( "Reminders" )]
    [Description( "Block to show a list of reminders." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Edit Reminder Page",
        Description = "The page used to edit a reminder.",
        DefaultValue = Rock.SystemGuid.Page.REMINDER_EDIT,
        Order = 1,
        Key = AttributeKey.EditReminderPage )]

    [ReminderTypesField(
        "Reminder Types Include",
        Description = "Reminder types to show in this block. Leave all unchecked to show all active reminder types.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.ReminderTypesInclude )]

    [ReminderTypesField(
        "Reminder Types Exclude",
        Description = "Reminder types to exclude from this block. Only applies when no types are selected in Reminder Types Include.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.ReminderTypesExclude )]

    [BooleanField(
        "Show Filters",
        Description = "Displays reminder filters above the list.",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.ShowFilters )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "7F0DAAFE-7312-4929-9159-9C138A119339" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "3F21FC19-D9A3-41F6-BFF5-1381B0BDD815" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_LIST )]
    public class ReminderList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EditReminderPage = "EditReminderPage";
            public const string ReminderTypesInclude = "ReminderTypesInclude";
            public const string ReminderTypesExclude = "ReminderTypesExclude";
            public const string ShowFilters = "ShowFilters";
        }

        /*
            4/28/26 - MSE

            EntityTypeId, EntityId, and ReminderTypeId are honored as one-time
            overrides of the saved person preference because Rock registers
            five page routes for the Reminder List page that pass these values
            via URL path segments ( see HotFix #161 ):

                reminders
                reminders/{EntityTypeId}
                reminders/{EntityTypeId}/{EntityId}
                reminders/{EntityTypeId}/{ReminderTypeId}
                reminders/{EntityTypeId}/{ReminderTypeId}/{EntityId}

            The default reminder notification email also builds its "View All
            Reminders" link as `reminders/{EntityTypeId}` ( see HotFix #164 ).
            Dropping support for any of these would break the daily email and
            any deep links that target the documented routes.

            Reason: These page parameters back Rock's registered routes and the daily reminder email.
        */
        private static class PageParameterKey
        {
            public const string EntityTypeId = "EntityTypeId";
            public const string EntityId = "EntityId";
            public const string ReminderTypeId = "ReminderTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string EditReminderPage = "EditReminderPage";
        }

        private static class PreferenceKey
        {
            public const string Sort = "filter-sort";
            public const string Completion = "filter-completion";
            public const string EntityType = "filter-entity-type";
            public const string EntityGuid = "filter-entity-guid";
            public const string ReminderType = "filter-reminder-type";
            public const string Due = "filter-due";
            public const string DueDateRange = "filter-due-date-range";
        }

        private static class FilterValue
        {
            public const string EntityTypeAll = "All";
            public const string EntityTypePeople = "People";
            public const string EntityTypeGroups = "Groups";

            public const string CompletionActive = "Active";
            public const string CompletionComplete = "Complete";

            public const string DueAll = "All";
            public const string DueOverdue = "Due";
            public const string DueThisWeek = "DueThisWeek";
            public const string DueThisMonth = "DueThisMonth";
            public const string DueCustomRange = "CustomDateRange";

            public const string SortDueDateAsc = "DueDateAsc";
            public const string SortDueDateDesc = "DueDateDesc";
            public const string SortNameAsc = "NameAsc";
            public const string SortNameDesc = "NameDesc";
        }

        #endregion Keys

        #region Fields

        private PersonPreferenceCollection _personPreferences;
        private FilterState _filterState;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the lazily-resolved block-scoped Person Preferences — the
        /// single source of truth for filter state.
        /// </summary>
        private PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = this.GetBlockPersonPreferences();
                }
                return _personPreferences;
            }
        }

        /// <summary>
        /// Gets whether filter affordances are enabled. When false, filters
        /// collapse to the default Active / Due view.
        /// </summary>
        private bool ShowFilters => GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean();

        /// <summary>
        /// Gets the lazily-resolved filter state for this request. Computed once
        /// (page parameters + Person Preferences + cached EntityType ids) and reused
        /// across every consumer. Subsequent reads in the same request are free.
        /// </summary>
        private FilterState Filters
        {
            get
            {
                if ( _filterState == null )
                {
                    _filterState = ResolveFilterState();
                }
                return _filterState;
            }
        }

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ReminderListInitializationBox
            {
                NavigationUrls = GetBoxNavigationUrls(),
                ShowFilters = ShowFilters
            };

            var currentPerson = GetCurrentPerson();
            if ( currentPerson?.Id == null )
            {
                box.IsAuthenticated = false;
                return box;
            }

            box.IsAuthenticated = true;

            // Recalculate the bell-icon count any time the user lands on a page
            // with this block so the badge stays in sync.
            new ReminderService( RockContext ).RecalculateReminderCount( currentPerson.Id );

            return box;
        }

        /// <summary>
        /// Builds the navigation URL dictionary keyed by <see cref="NavigationUrlKey"/>.
        /// </summary>
        /// <returns>The navigation URL dictionary.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.EditReminderPage] = this.GetLinkedPageUrl( AttributeKey.EditReminderPage )
            };
        }

        /// <summary>
        /// Resolves a page parameter to an integer entity Id. Accepts either an
        /// IdKey hash or a raw integer Id. The integer fallback is unconditional —
        /// the registered <c>reminders/{EntityTypeId}/...</c> page routes (HotFix
        /// #161) and the daily reminder notification email (HotFix #164) emit raw
        /// integers, and gating those on <c>DisablePredictableIds</c> would silently
        /// break those external surfaces on hardened sites. Returns null when the
        /// parameter is missing or unparseable.
        /// </summary>
        /// <param name="pageParameterKey">The page parameter key to resolve.</param>
        /// <returns>The resolved integer Id, or null.</returns>
        private int? GetIdFromPageParameter( string pageParameterKey )
        {
            var raw = PageParameter( pageParameterKey );
            if ( string.IsNullOrWhiteSpace( raw ) )
            {
                return null;
            }

            var idFromHash = IdHasher.Instance.GetId( raw );
            if ( idFromHash.HasValue )
            {
                return idFromHash;
            }

            return raw.AsIntegerOrNull();
        }

        /// <summary>
        /// Loads, filters, sorts, and projects reminders into the response shape.
        /// Single source of truth for the list — every block action returns the
        /// same payload so the client can re-render without an additional fetch.
        /// </summary>
        /// <param name="currentPerson">The logged-in person.</param>
        /// <returns>The response bag with reminders, count, options, and resolved entity selection.</returns>
        private GetRemindersResponseBag LoadReminders( Person currentPerson )
        {
            var reminderService = new ReminderService( RockContext );
            var includedReminderTypeIds = GetIncludedReminderTypeIds();
            var excludedReminderTypeIds = GetExcludedReminderTypeIds();

            var reminderQuery = BuildReminderQuery(
                reminderService,
                currentPerson,
                includedReminderTypeIds,
                excludedReminderTypeIds );

            // Filter for completion status.
            if ( Filters.Completion == FilterValue.CompletionActive )
            {
                reminderQuery = reminderQuery.Where( r => !r.IsComplete );
            }
            else if ( Filters.Completion == FilterValue.CompletionComplete )
            {
                reminderQuery = reminderQuery.Where( r => r.IsComplete );
            }

            // Filter for due window.
            ApplyDueFilter( ref reminderQuery );

            // Sort. Date sorts execute at the DB layer; name sorts skip the DB
            // OrderBy entirely and apply once the bags are projected, since the
            // resolved entity name only exists post-projection.
            switch ( Filters.Sort )
            {
                case FilterValue.SortDueDateDesc:
                    reminderQuery = reminderQuery.OrderByDescending( r => r.ReminderDate );
                    break;
                case FilterValue.SortNameAsc:
                case FilterValue.SortNameDesc:
                    // Defer ordering to the in-memory sort below.
                    break;
                default:
                    reminderQuery = reminderQuery.OrderBy( r => r.ReminderDate );
                    break;
            }

            var reminders = reminderQuery
                .Include( r => r.ReminderType.EntityType )
                .Include( r => r.PersonAlias.Person )
                .ToList();

            // Resolve entities and project to the wire format. Skip orphans whose
            // referenced entity has been deleted; clean them up once after the loop.
            var entities = reminderService.GetReminderEntities( reminderQuery );

            // Batch-load the Person for each PersonAlias target in a single query.
            // EF relationship fixup populates the alias .Person navigations, so the
            // personAlias.Person accesses in BuildReminderBag don't lazy-load one
            // query per reminder. PersonAliasService is used rather than
            // PersonService because the latter excludes deceased and nameless
            // records by default, and reminders can still target those.
            var targetAliasIds = entities.Values
                .OfType<PersonAlias>()
                .Select( a => a.Id )
                .ToList();

            if ( targetAliasIds.Any() )
            {
                new PersonAliasService( RockContext )
                    .Queryable()
                    .Where( a => targetAliasIds.Contains( a.Id ) )
                    .Select( a => a.Person )
                    .ToList();
            }

            var bags = new List<ReminderListBag>( reminders.Count );
            var orphanedReminders = new List<Reminder>();

            foreach ( var reminder in reminders )
            {
                if ( !entities.TryGetValue( reminder.Id, out var entity ) || entity == null )
                {
                    orphanedReminders.Add( reminder );
                    continue;
                }

                bags.Add( BuildReminderBag( reminder, entity, Filters.PersonAliasEntityTypeId, Filters.GroupEntityTypeId ) );
            }

            if ( orphanedReminders.Count > 0 )
            {
                reminderService.DeleteRange( orphanedReminders );
                RockContext.SaveChanges();
            }

            // Name-sort path: the DB query above intentionally skipped OrderBy
            // because the resolved entity name only exists post-projection.
            if ( Filters.Sort == FilterValue.SortNameAsc )
            {
                bags = bags.OrderBy( b => b.EntityName, StringComparer.OrdinalIgnoreCase ).ToList();
            }
            else if ( Filters.Sort == FilterValue.SortNameDesc )
            {
                bags = bags.OrderByDescending( b => b.EntityName, StringComparer.OrdinalIgnoreCase ).ToList();
            }

            var options = BuildOptions(
                currentPerson,
                includedReminderTypeIds,
                excludedReminderTypeIds,
                out var entityTypesWithRemindersCount );

            return new GetRemindersResponseBag
            {
                Reminders = bags,
                Options = options,
                SelectedEntity = ResolveSelectedEntity(),
                HasNoReminders = entityTypesWithRemindersCount == 0
            };
        }

        /// <summary>
        /// Builds the base reminder query honoring the entity type and entity id
        /// filters. When the entity type filter is People with a specific entity
        /// drilldown, broadens the alias-id list so reminders attached to
        /// previous PersonAliases are included.
        /// </summary>
        /// <param name="reminderService">The reminder service.</param>
        /// <param name="currentPerson">The logged-in person.</param>
        /// <param name="includedReminderTypeIds">The block-attribute include list.</param>
        /// <param name="excludedReminderTypeIds">The block-attribute exclude list.</param>
        /// <returns>The base reminder query.</returns>
        private IQueryable<Reminder> BuildReminderQuery(
            ReminderService reminderService,
            Person currentPerson,
            List<int> includedReminderTypeIds,
            List<int> excludedReminderTypeIds )
        {
            int? entityTypeId = ResolveEntityTypeId( Filters.EntityType, Filters.PersonAliasEntityTypeId, Filters.GroupEntityTypeId );
            int? entityId = Filters.EntityId;
            int? reminderTypeId = Filters.ReminderTypeId;

            IQueryable<Reminder> queryable;

            // Special case: when filtering to a specific person, broaden to all of
            // their PersonAliases so we don't miss reminders attached to merged-in
            // aliases. Mirrors the GetExistingReminders() pattern in ReminderLinks block.
            if ( entityTypeId == Filters.PersonAliasEntityTypeId && entityId.HasValue )
            {
                var personAliasService = new PersonAliasService( RockContext );
                var targetPersonId = personAliasService.GetPersonId( entityId.Value );
                var aliasIds = targetPersonId.HasValue
                    ? personAliasService.Queryable()
                        .Where( a => a.PersonId == targetPersonId.Value )
                        .Select( a => a.Id )
                        .ToList()
                    : new List<int>();

                queryable = reminderService.GetReminders( currentPerson.Id, entityTypeId, null, reminderTypeId )
                    .Where( r => aliasIds.Contains( r.EntityId ) );
            }
            else
            {
                queryable = reminderService.GetReminders( currentPerson.Id, entityTypeId, entityId, reminderTypeId );
            }

            if ( includedReminderTypeIds.Any() )
            {
                queryable = queryable.Where( r => includedReminderTypeIds.Contains( r.ReminderTypeId ) );
            }
            else if ( excludedReminderTypeIds.Any() )
            {
                queryable = queryable.Where( r => !excludedReminderTypeIds.Contains( r.ReminderTypeId ) );
            }

            return queryable;
        }

        /// <summary>
        /// Applies the due-date window filter to the query.
        /// </summary>
        /// <param name="query">The query to filter (passed by ref so the LINQ chain stays composable).</param>
        private void ApplyDueFilter( ref IQueryable<Reminder> query )
        {
            var now = RockDateTime.Now;

            switch ( Filters.Due )
            {
                case FilterValue.DueOverdue:
                    query = query.Where( r => r.ReminderDate <= now );
                    break;

                case FilterValue.DueThisWeek:
                    var nextWeekStart = now.EndOfWeek( RockDateTime.FirstDayOfWeek ).AddDays( 1 );
                    var startOfWeek = nextWeekStart.AddDays( -7 );
                    query = query.Where( r => r.ReminderDate >= startOfWeek && r.ReminderDate < nextWeekStart );
                    break;

                case FilterValue.DueThisMonth:
                    var startOfMonth = now.StartOfMonth();
                    var startOfNextMonth = now.AddMonths( 1 ).StartOfMonth();
                    query = query.Where( r => r.ReminderDate >= startOfMonth && r.ReminderDate < startOfNextMonth );
                    break;

                case FilterValue.DueCustomRange:
                    var actualRange = Filters.DueDateRange?.ToActualDateRange();
                    if ( actualRange?.Start.HasValue == true )
                    {
                        var rangeStart = actualRange.Start.Value;
                        query = query.Where( r => r.ReminderDate >= rangeStart );
                    }
                    if ( actualRange?.End.HasValue == true )
                    {
                        var rangeEnd = actualRange.End.Value;
                        query = query.Where( r => r.ReminderDate < rangeEnd );
                    }
                    break;

                default:
                    // "All" or unset — no date constraint.
                    break;
            }
        }

        /// <summary>
        /// Maps an EntityType filter value to its integer entity-type id, or null
        /// to mean "no filter".
        /// </summary>
        /// <param name="entityTypeValue">The filter value.</param>
        /// <param name="personAliasEntityTypeId">The PersonAlias entity type id.</param>
        /// <param name="groupEntityTypeId">The Group entity type id.</param>
        /// <returns>The entity type id, or null.</returns>
        private static int? ResolveEntityTypeId( string entityTypeValue, int personAliasEntityTypeId, int groupEntityTypeId )
        {
            if ( string.IsNullOrWhiteSpace( entityTypeValue ) || entityTypeValue == FilterValue.EntityTypeAll )
            {
                return null;
            }

            if ( entityTypeValue == FilterValue.EntityTypePeople )
            {
                return personAliasEntityTypeId;
            }

            if ( entityTypeValue == FilterValue.EntityTypeGroups )
            {
                return groupEntityTypeId;
            }

            // Any other value is treated as a generic entity-type IdKey.
            return IdHasher.Instance.GetId( entityTypeValue );
        }

        /// <summary>
        /// Resolves the stored entity drilldown id to a <see cref="ListItemBag"/>
        /// so the View Options modal can render the picker with the correct text.
        /// </summary>
        /// <returns>The resolved selection or null when no drilldown is active.</returns>
        private ListItemBag ResolveSelectedEntity()
        {
            var entityId = Filters.EntityId;
            if ( !entityId.HasValue )
            {
                return null;
            }

            if ( Filters.EntityType == FilterValue.EntityTypePeople )
            {
                var personAlias = new PersonAliasService( RockContext )
                    .Queryable()
                    .Include( a => a.Person )
                    .FirstOrDefault( a => a.Id == entityId.Value );

                if ( personAlias?.Person != null )
                {
                    return new ListItemBag
                    {
                        Value = personAlias.Guid.ToString(),
                        Text = personAlias.Person.FullName
                    };
                }
            }
            else if ( Filters.EntityType == FilterValue.EntityTypeGroups )
            {
                var group = new GroupService( RockContext ).Get( entityId.Value );
                if ( group != null )
                {
                    return new ListItemBag
                    {
                        Value = group.Guid.ToString(),
                        Text = group.Name
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Projects a single reminder + its resolved entity into the wire format
        /// expected by the Vue card.
        /// </summary>
        /// <param name="reminder">The reminder.</param>
        /// <param name="entity">The entity the reminder is attached to.</param>
        /// <param name="personAliasEntityTypeId">The PersonAlias entity type id.</param>
        /// <param name="groupEntityTypeId">The Group entity type id.</param>
        /// <returns>The card-ready bag.</returns>
        private static ReminderListBag BuildReminderBag( Reminder reminder, IEntity entity, int personAliasEntityTypeId, int groupEntityTypeId )
        {
            var bag = new ReminderListBag
            {
                IdKey = reminder.IdKey,
                ReminderDate = reminder.ReminderDate.ToRockDateTimeOffset(),
                Note = reminder.Note,
                ReminderTypeName = reminder.ReminderType?.Name,
                HighlightColor = reminder.ReminderType?.HighlightColor,
                IsComplete = reminder.IsComplete,
                IsRenewing = reminder.IsRenewing,
                EntityName = entity.ToString(),
                IsPersonReminder = entity.TypeId == personAliasEntityTypeId,
                IsGroupReminder = entity.TypeId == groupEntityTypeId
            };

            /*
                6/4/26 - MSE

                Person reminders attach to a PersonAlias, so resolve the underlying
                Person once. It supplies the profile photo and IdKey below, and it is
                also the entity merged into the link URL, because the Person entity
                type's LinkUrlLavaTemplate (`~/Person/{{ Entity.Id }}`) expects a
                Person.Id. Merging the PersonAlias routed users to the wrong person.

                Reason: Person reminder links navigated to the wrong person record.
            */
            var person = bag.IsPersonReminder && entity is PersonAlias personAlias
                ? personAlias.Person
                : null;

            // Person reminders carry a profile photo + person IdKey for the avatar
            // hover popover.
            if ( person != null )
            {
                bag.ProfilePhotoUrl = Person.GetPersonPhotoUrl( person, 50, 50 );
                bag.PersonIdKey = person.IdKey;
            }

            // Group reminders use the group type icon (no avatar).
            if ( bag.IsGroupReminder && entity is Rock.Model.Group group )
            {
                var groupTypeIconCss = GroupTypeCache.Get( group.GroupTypeId )?.IconCssClass;
                bag.EntityIconCssClass = string.IsNullOrWhiteSpace( groupTypeIconCss )
                    ? "ti ti-users"
                    : groupTypeIconCss;
            }

            // Resolve the entity URL using the EntityType's LinkUrlLavaTemplate.
            var entityType = bag.IsPersonReminder
                ? EntityTypeCache.Get<Person>()
                : EntityTypeCache.Get( reminder.ReminderType?.EntityTypeId ?? 0 );

            if ( !string.IsNullOrWhiteSpace( entityType?.LinkUrlLavaTemplate ) )
            {
                var mergeFields = new Dictionary<string, object>
                {
                    ["Entity"] = ( IEntity ) person ?? entity
                };

                var url = entityType.LinkUrlLavaTemplate.ResolveMergeFields( mergeFields );
                if ( url.StartsWith( "~/" ) )
                {
                    var baseUrl = GlobalAttributesCache.Value( "InternalApplicationRoot" );
                    url = url.Replace( "~/", baseUrl.EnsureTrailingForwardslash() );
                }

                bag.EntityUrl = url;
            }

            // Pre-compute the friendly due label + color so the client stays simple.
            ApplyDueLabel( bag, reminder );

            // Pre-compute the renewing recurrence text.
            if ( reminder.IsRenewing && reminder.RenewPeriodDays.HasValue && reminder.RenewPeriodDays.Value > 0 )
            {
                bag.RecurrenceText = reminder.RenewPeriodDays.Value == 1
                    ? "Every Day"
                    : $"Every {reminder.RenewPeriodDays.Value} Days";
            }

            return bag;
        }

        /// <summary>
        /// Computes the relative-due label and a semantic color hint for the bag.
        /// </summary>
        /// <param name="bag">The bag to update.</param>
        /// <param name="reminder">The reminder.</param>
        private static void ApplyDueLabel( ReminderListBag bag, Reminder reminder )
        {
            if ( reminder.IsComplete )
            {
                bag.DueLabel = string.Empty;
                bag.DueLabelColor = "default";
                return;
            }

            var today = RockDateTime.Today;
            var reminderDate = reminder.ReminderDate.Date;
            var daysDiff = ( reminderDate - today ).Days;

            if ( daysDiff < 0 )
            {
                var pastDays = Math.Abs( daysDiff );
                bag.DueLabel = pastDays == 1 ? "Due 1 Day Ago" : $"Due {pastDays} Days Ago";
                bag.DueLabelColor = "danger";
            }
            else if ( daysDiff == 0 )
            {
                bag.DueLabel = "Due Today";
                bag.DueLabelColor = "warning";
            }
            else
            {
                bag.DueLabel = daysDiff == 1 ? "Due in 1 Day" : $"Due in {daysDiff} Days";
                bag.DueLabelColor = "default";
            }
        }

        /// <summary>
        /// Builds the option lists shipped to the client. Reminder types are
        /// scoped by the current entity-type filter so the dropdown only shows
        /// types relevant to the active drilldown.
        /// </summary>
        /// <param name="currentPerson">The logged-in person (for authorization checks on reminder types).</param>
        /// <param name="includedReminderTypeIds">The block-attribute include list.</param>
        /// <param name="excludedReminderTypeIds">The block-attribute exclude list.</param>
        /// <param name="entityTypesWithRemindersCount">Outputs the number of distinct
        /// entity types this person has any reminder for (block-attribute include / exclude
        /// applied; user-applied filters NOT applied). Zero means the user has no reminders
        /// at all and the client should collapse to the no-data screen.</param>
        /// <returns>The options bag.</returns>
        private ReminderListOptionsBag BuildOptions(
            Person currentPerson,
            List<int> includedReminderTypeIds,
            List<int> excludedReminderTypeIds,
            out int entityTypesWithRemindersCount )
        {
            var reminderService = new ReminderService( RockContext );

            int? scopedEntityTypeId = ResolveEntityTypeId( Filters.EntityType, Filters.PersonAliasEntityTypeId, Filters.GroupEntityTypeId );

            var reminderTypes = reminderService.GetReminderTypesByPerson( scopedEntityTypeId, currentPerson );
            if ( includedReminderTypeIds.Any() )
            {
                reminderTypes = reminderTypes.Where( t => includedReminderTypeIds.Contains( t.Id ) ).ToList();
            }
            else if ( excludedReminderTypeIds.Any() )
            {
                reminderTypes = reminderTypes.Where( t => !excludedReminderTypeIds.Contains( t.Id ) ).ToList();
            }

            // Build the entity-type filter list. Always include "All Entities".
            // Add People / Groups when the user has reminders for those types.
            var entityTypesWithReminders = reminderService.GetReminderEntityTypesByPerson(
                currentPerson.Id,
                includedReminderTypeIds,
                excludedReminderTypeIds ).ToList();

            entityTypesWithRemindersCount = entityTypesWithReminders.Count;

            var entityTypeFilters = new List<ListItemBag>
            {
                new ListItemBag { Value = FilterValue.EntityTypeAll, Text = "All Entities" }
            };

            if ( entityTypesWithReminders.Any( t => t.Id == Filters.PersonAliasEntityTypeId ) )
            {
                entityTypeFilters.Add( new ListItemBag { Value = FilterValue.EntityTypePeople, Text = "People" } );
            }

            if ( entityTypesWithReminders.Any( t => t.Id == Filters.GroupEntityTypeId ) )
            {
                entityTypeFilters.Add( new ListItemBag { Value = FilterValue.EntityTypeGroups, Text = "Groups" } );
            }

            // Other entity types appear by friendly-name pluralization.
            foreach ( var entityType in entityTypesWithReminders.Where( t => t.Id != Filters.PersonAliasEntityTypeId && t.Id != Filters.GroupEntityTypeId ) )
            {
                entityTypeFilters.Add( new ListItemBag
                {
                    Value = IdHasher.Instance.GetHash( entityType.Id ),
                    Text = entityType.FriendlyName.Pluralize()
                } );
            }

            return new ReminderListOptionsBag
            {
                ReminderTypes = reminderTypes
                    .Select( t => new ListItemBag { Value = t.IdKey, Text = t.Name } )
                    .ToList(),
                EntityTypeFilters = entityTypeFilters,
                SortOptions = new List<ListItemBag>
                {
                    new ListItemBag { Value = FilterValue.SortDueDateAsc, Text = "Due Date (Soonest First)" },
                    new ListItemBag { Value = FilterValue.SortDueDateDesc, Text = "Due Date (Latest First)" },
                    new ListItemBag { Value = FilterValue.SortNameAsc, Text = "Name (A-Z)" },
                    new ListItemBag { Value = FilterValue.SortNameDesc, Text = "Name (Z-A)" }
                },
                DueFilterOptions = new List<ListItemBag>
                {
                    new ListItemBag { Value = FilterValue.DueAll, Text = "All Dates" },
                    new ListItemBag { Value = FilterValue.DueOverdue, Text = "Due" },
                    new ListItemBag { Value = FilterValue.DueThisWeek, Text = "Due This Week" },
                    new ListItemBag { Value = FilterValue.DueThisMonth, Text = "Due This Month" },
                    new ListItemBag { Value = FilterValue.DueCustomRange, Text = "Date Range" }
                }
            };
        }

        /// <summary>
        /// Returns the include list of reminder type ids declared in the block
        /// settings, or an empty list if none are configured.
        /// </summary>
        /// <returns>The include list.</returns>
        private List<int> GetIncludedReminderTypeIds()
        {
            return GetReminderTypeIdsFromAttribute( AttributeKey.ReminderTypesInclude );
        }

        /// <summary>
        /// Returns the exclude list of reminder type ids declared in the block
        /// settings, or an empty list if none are configured.
        /// </summary>
        /// <returns>The exclude list.</returns>
        private List<int> GetExcludedReminderTypeIds()
        {
            return GetReminderTypeIdsFromAttribute( AttributeKey.ReminderTypesExclude );
        }

        /// <summary>
        /// Resolves a list of reminder type guids stored on a block attribute
        /// down to integer ids in a single query.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns>The integer ids of the configured reminder types.</returns>
        private List<int> GetReminderTypeIdsFromAttribute( string attributeKey )
        {
            var guids = GetAttributeValue( attributeKey ).SplitDelimitedValues().AsGuidList();
            if ( !guids.Any() )
            {
                return new List<int>();
            }

            return new ReminderTypeService( RockContext )
                .Queryable()
                .Where( t => guids.Contains( t.Guid ) )
                .Select( t => t.Id )
                .ToList();
        }

        /// <summary>
        /// Gets the <see cref="Reminder"/> with the given hashed id and verifies
        /// it belongs to the current person. Returns null for any failure so the
        /// caller can respond with a generic "invalid reminder" error regardless
        /// of whether the id was missing, unhashable, or not owned — avoids
        /// leaking which reminders exist.
        /// </summary>
        /// <param name="reminderIdKey">The hashed reminder identifier.</param>
        /// <param name="currentPerson">The logged-in person.</param>
        /// <returns>The reminder or null.</returns>
        private Reminder GetOwnedReminder( string reminderIdKey, Person currentPerson )
        {
            if ( reminderIdKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var reminderId = IdHasher.Instance.GetId( reminderIdKey );
            if ( !reminderId.HasValue )
            {
                return null;
            }

            var reminder = new ReminderService( RockContext )
                .Queryable()
                .Include( r => r.PersonAlias )
                .Include( r => r.ReminderType )
                .FirstOrDefault( r => r.Id == reminderId.Value );

            if ( reminder == null || reminder.PersonAlias?.PersonId != currentPerson.Id )
            {
                return null;
            }

            return reminder;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the filtered, sorted reminder list. Reads filter selections
        /// fresh from Person Preferences on every call — the frontend writes
        /// preferences and is the source of truth for filter state.
        /// </summary>
        /// <returns>A <see cref="GetRemindersResponseBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetReminders()
        {
            var currentPerson = GetCurrentPerson();
            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            return ActionOk( LoadReminders( currentPerson ) );
        }

        /// <summary>
        /// Toggles the IsComplete state of the specified reminder. Renewing
        /// reminders advance to the next due date when completed.
        /// </summary>
        /// <param name="reminderIdKey">The hashed reminder identifier.</param>
        /// <returns>A refreshed <see cref="GetRemindersResponseBag"/>.</returns>
        [BlockAction]
        public BlockActionResult ToggleReminderComplete( string reminderIdKey )
        {
            var currentPerson = GetCurrentPerson();
            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            var reminder = GetOwnedReminder( reminderIdKey, currentPerson );
            if ( reminder == null )
            {
                return ActionBadRequest( "Invalid reminder." );
            }

            if ( reminder.IsComplete )
            {
                reminder.ResetCompletedReminder();
            }
            else
            {
                reminder.CompleteReminder();
            }

            RockContext.SaveChanges();
            new ReminderService( RockContext ).RecalculateReminderCount( currentPerson.Id );

            return ActionOk( LoadReminders( currentPerson ) );
        }

        /// <summary>
        /// Updates the reminder date.
        /// </summary>
        /// <param name="bag">The reschedule request.</param>
        /// <returns>A refreshed <see cref="GetRemindersResponseBag"/>.</returns>
        [BlockAction]
        public BlockActionResult RescheduleReminder( RescheduleReminderRequestBag bag )
        {
            var currentPerson = GetCurrentPerson();
            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            if ( bag == null || !bag.NewReminderDate.HasValue )
            {
                return ActionBadRequest( "A reminder date is required." );
            }

            var reminder = GetOwnedReminder( bag.ReminderIdKey, currentPerson );
            if ( reminder == null )
            {
                return ActionBadRequest( "Invalid reminder." );
            }

            reminder.ReminderDate = bag.NewReminderDate.Value.Date;

            // The Reminder SaveHook adjusts the owner's bell count when
            // ReminderDate changes flips IsActive — no manual recount needed.
            RockContext.SaveChanges();

            return ActionOk( LoadReminders( currentPerson ) );
        }

        /// <summary>
        /// Updates the assigned PersonAlias for the specified reminder.
        /// </summary>
        /// <param name="bag">The reassign request. The PersonPicker emits a
        /// <see cref="PersonAlias.Guid"/>, which is resolved to the corresponding
        /// PersonAliasId server-side.</param>
        /// <returns>A refreshed <see cref="GetRemindersResponseBag"/>.</returns>
        [BlockAction]
        public BlockActionResult ReassignReminder( ReassignReminderRequestBag bag )
        {
            var currentPerson = GetCurrentPerson();
            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            if ( bag == null || !bag.NewPersonAliasGuid.HasValue || bag.NewPersonAliasGuid.Value == Guid.Empty )
            {
                return ActionBadRequest( "A person is required." );
            }

            var reminder = GetOwnedReminder( bag.ReminderIdKey, currentPerson );
            if ( reminder == null )
            {
                return ActionBadRequest( "Invalid reminder." );
            }

            var newAlias = new PersonAliasService( RockContext ).Get( bag.NewPersonAliasGuid.Value );
            if ( newAlias == null )
            {
                return ActionBadRequest( "Invalid person." );
            }

            // Reassigning a reminder transfers ownership: it shouldn't return in
            // GetReminders for the original assignee anymore. The Reminder
            // SaveHook handles the bell-count transfer between the previous
            // and new owners when PersonAliasId changes on an active reminder.
            reminder.PersonAliasId = newAlias.Id;
            RockContext.SaveChanges();

            return ActionOk( LoadReminders( currentPerson ) );
        }

        /// <summary>
        /// Deletes the specified reminder.
        /// </summary>
        /// <param name="reminderIdKey">The hashed reminder identifier.</param>
        /// <returns>A refreshed <see cref="GetRemindersResponseBag"/>.</returns>
        [BlockAction]
        public BlockActionResult DeleteReminder( string reminderIdKey )
        {
            var currentPerson = GetCurrentPerson();
            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            var reminder = GetOwnedReminder( reminderIdKey, currentPerson );
            if ( reminder == null )
            {
                return ActionBadRequest( "Invalid reminder." );
            }

            var reminderService = new ReminderService( RockContext );
            reminderService.Delete( reminder );
            RockContext.SaveChanges();
            reminderService.RecalculateReminderCount( currentPerson.Id );

            return ActionOk( LoadReminders( currentPerson ) );
        }

        #endregion Block Actions

        #region Filter State

        /// <summary>
        /// Snapshot of the resolved filter state for a single request. Holds the
        /// effective Sort / Completion / Due / DueDateRange / EntityType / EntityId /
        /// ReminderTypeId values along with cached PersonAlias and Group entity
        /// type ids so consumers don't repeatedly hit <see cref="EntityTypeCache"/>.
        /// </summary>
        private class FilterState
        {
            public int PersonAliasEntityTypeId { get; set; }

            public int GroupEntityTypeId { get; set; }

            public string Sort { get; set; }

            public string Completion { get; set; }

            public string Due { get; set; }

            public SlidingDateRangeBag DueDateRange { get; set; }

            /// <summary>
            /// One of "All" / "People" / "Groups" / an EntityType IdKey hash.
            /// </summary>
            public string EntityType { get; set; }

            /// <summary>
            /// The resolved drilldown integer id (PersonAlias.Id or Group.Id), or null
            /// when no drilldown is active. Resolution may issue a single DB lookup
            /// to translate a stored Guid; that lookup happens once per request.
            /// </summary>
            public int? EntityId { get; set; }

            public int? ReminderTypeId { get; set; }
        }

        /// <summary>
        /// Builds the request-scoped filter snapshot. When <see cref="ShowFilters"/>
        /// is false the filters collapse to the WebForms default (Active / Due);
        /// otherwise page parameters override saved Person Preferences for the
        /// three external-facing keys (EntityTypeId / EntityId / ReminderTypeId).
        /// </summary>
        /// <returns>The resolved filter state.</returns>
        private FilterState ResolveFilterState()
        {
            var personAliasEntityTypeId = EntityTypeCache.GetId<PersonAlias>() ?? 0;
            var groupEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.GROUP.AsGuid() ) ?? 0;

            if ( !ShowFilters )
            {
                return new FilterState
                {
                    PersonAliasEntityTypeId = personAliasEntityTypeId,
                    GroupEntityTypeId = groupEntityTypeId,
                    Sort = FilterValue.SortDueDateAsc,
                    Completion = FilterValue.CompletionActive,
                    Due = FilterValue.DueOverdue,
                    DueDateRange = null,
                    EntityType = FilterValue.EntityTypeAll,
                    EntityId = null,
                    ReminderTypeId = null
                };
            }

            var entityType = ResolveEntityTypeFilter( personAliasEntityTypeId, groupEntityTypeId );
            var entityId = ResolveEntityIdFilter( entityType );
            var reminderTypeId = ResolveReminderTypeIdFilter();

            // Empty / unset Completion and Due preferences default to the WebForms
            // defaults so first-time visitors land on actionable reminders rather
            // than every reminder ever.
            var storedCompletion = PersonPreferences.GetValue( PreferenceKey.Completion );
            var storedDue = PersonPreferences.GetValue( PreferenceKey.Due );

            return new FilterState
            {
                PersonAliasEntityTypeId = personAliasEntityTypeId,
                GroupEntityTypeId = groupEntityTypeId,
                Sort = PersonPreferences.GetValue( PreferenceKey.Sort ),
                Completion = storedCompletion.IsNullOrWhiteSpace() ? FilterValue.CompletionActive : storedCompletion,
                Due = storedDue.IsNullOrWhiteSpace() ? FilterValue.DueOverdue : storedDue,
                DueDateRange = PersonPreferences.GetValue( PreferenceKey.DueDateRange ).ToSlidingDateRangeBagOrNull(),
                EntityType = entityType,
                EntityId = entityId,
                ReminderTypeId = reminderTypeId
            };
        }

        /// <summary>
        /// Resolves the entity-type filter value. <c>EntityTypeId</c> page parameter
        /// (raw Id or IdKey) takes precedence; falls back to the saved preference.
        /// </summary>
        /// <param name="personAliasEntityTypeId">The PersonAlias entity type id.</param>
        /// <param name="groupEntityTypeId">The Group entity type id.</param>
        /// <returns>"All", "People", "Groups", or an EntityType IdKey hash.</returns>
        private string ResolveEntityTypeFilter( int personAliasEntityTypeId, int groupEntityTypeId )
        {
            var fromUrl = GetIdFromPageParameter( PageParameterKey.EntityTypeId );
            if ( fromUrl.HasValue )
            {
                if ( fromUrl.Value == personAliasEntityTypeId )
                {
                    return FilterValue.EntityTypePeople;
                }
                if ( fromUrl.Value == groupEntityTypeId )
                {
                    return FilterValue.EntityTypeGroups;
                }

                return IdHasher.Instance.GetHash( fromUrl.Value );
            }

            return PersonPreferences.GetValue( PreferenceKey.EntityType );
        }

        /// <summary>
        /// Resolves the drilldown entity Id. <c>EntityId</c> page parameter (raw
        /// Id or IdKey) takes precedence; falls back to the saved preference, which
        /// stores a PersonAlias / Group <c>Guid</c> emitted by the View Options
        /// picker. The stored Guid is resolved against the appropriate service
        /// based on the current entity-type filter.
        /// </summary>
        /// <param name="entityType">The resolved entity-type filter.</param>
        /// <returns>The resolved integer Id, or null.</returns>
        private int? ResolveEntityIdFilter( string entityType )
        {
            var fromUrl = GetIdFromPageParameter( PageParameterKey.EntityId );
            if ( fromUrl.HasValue )
            {
                return fromUrl;
            }

            var stored = PersonPreferences.GetValue( PreferenceKey.EntityGuid );
            if ( stored.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( Guid.TryParse( stored, out var guid ) )
            {
                if ( entityType == FilterValue.EntityTypePeople )
                {
                    return new PersonAliasService( RockContext )
                        .Queryable()
                        .Where( a => a.Guid == guid )
                        .Select( a => ( int? ) a.Id )
                        .FirstOrDefault();
                }

                if ( entityType == FilterValue.EntityTypeGroups )
                {
                    return new GroupService( RockContext )
                        .Queryable()
                        .Where( g => g.Guid == guid )
                        .Select( g => ( int? ) g.Id )
                        .FirstOrDefault();
                }
            }

            return IdHasher.Instance.GetId( stored );
        }

        /// <summary>
        /// Resolves the reminder type Id. <c>ReminderTypeId</c> page parameter
        /// (raw Id or IdKey) takes precedence; falls back to the saved IdKey preference.
        /// </summary>
        /// <returns>The resolved reminder type id, or null.</returns>
        private int? ResolveReminderTypeIdFilter()
        {
            var fromUrl = GetIdFromPageParameter( PageParameterKey.ReminderTypeId );
            if ( fromUrl.HasValue )
            {
                return fromUrl;
            }

            return IdHasher.Instance.GetId( PersonPreferences.GetValue( PreferenceKey.ReminderType ) );
        }

        #endregion Filter State
    }
}
