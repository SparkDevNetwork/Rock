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

/** Keys for the linked-page navigation URLs supplied in the initialization box. */
export const enum NavigationUrlKey {
    EditReminderPage = "EditReminderPage"
}

/** Block-scoped Person Preference keys. Must match the C# PreferenceKey constants verbatim. */
export const enum PreferenceKey {
    Sort = "filter-sort",
    Completion = "filter-completion",
    EntityType = "filter-entity-type",
    EntityGuid = "filter-entity-guid",
    ReminderType = "filter-reminder-type",
    Due = "filter-due",
    DueDateRange = "filter-due-date-range"
}

/** Sort selections shown in the View Options modal. */
export const enum SortBy {
    DueDateAsc = "DueDateAsc",
    DueDateDesc = "DueDateDesc",
    NameAsc = "NameAsc",
    NameDesc = "NameDesc"
}

/** Entity type filter values. People and Groups are well-known; any other entity type uses its IdKey directly. */
export const enum EntityTypeFilter {
    All = "All",
    People = "People",
    Groups = "Groups"
}

/** Due-date filter values controlling how the reminder list is windowed. */
export const enum DueFilter {
    All = "All",
    Overdue = "Due",
    DueThisWeek = "DueThisWeek",
    DueThisMonth = "DueThisMonth",
    CustomDateRange = "CustomDateRange"
}

/** Completion filter values rendered as the segmented buttons in the panel header. */
export const enum CompletionFilter {
    All = "All",
    Active = "Active",
    Complete = "Complete"
}

/**
 * Local Vue ref shape for the reminder-list filter state. Hydrated from
 * Person Preferences on mount, written back via the watcher whenever the
 * user changes any filter. Pure client-side type — the C# layer reads
 * preferences directly, so this never crosses the wire.
 */
export type ReminderFilter = {
    sort: string;
    completion: string;
    entityType: string;
    entityGuid: string;
    reminderTypeIdKey: string;
    due: string;
    dueDateRange: string;
};
