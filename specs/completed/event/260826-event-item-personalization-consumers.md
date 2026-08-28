---
author: Jon Edmiston
date_created: 2026-08-26
summary: >-
  Event items can be tagged with personalization segments and request filters,
  but the public calendar surfaces ignore them. Adds opt-in filtering to the
  Calendar Lava block and the CalendarEvents Lava command.
contributors: []
related_docs:
  - docs/event/event-overview.md
---

# Event Personalization Filtering for Calendar Surfaces

## Summary

[260727-event-item-detail-polish-personalization](completed/event/260727-event-item-detail-polish-personalization.md)
added the ability to tag an event item with personalization Audience Segments
and Request Filters, and explicitly deferred the consuming side. This spec
covers the consuming side for exactly two surfaces:

1. **Calendar Lava block** (`RockWeb/Blocks/Event/CalendarLava.ascx.cs`) — two
   new boolean block attributes, "Filter by Personalization Segments" and
   "Filter by Request Filters".
2. **`CalendarEvents` Lava command** (`Rock/Lava/Blocks/CalendarEventsBlock.cs`)
   — the equivalent as two command parameters.

Both are opt-in and default to off. Nothing else changes.

## Motivation

An administrator can tag an event item with segments and request filters today
and see no effect on the public calendar. The admin UI implies a capability that
does not exist on the visitor side, which will generate support cases.

Content Collections were originally suspected of needing this work too. They do
not; personalization on event items has been confirmed working there in a live
test, because the Content Collection index pipeline and the boost query are both
entity-type agnostic. That surface is out of scope, and the finding is recorded
below so it is not re-investigated.

## Background

Three pieces, briefly, for readers new to this area.

**Personalization Segments** answer "who is this person?" Membership is computed
ahead of time by the `UpdatePersonalizationData` job. At request time Rock
resolves the current person's or visitor's segments into
`RockRequestContext.PersonalizationSegmentIds`.

**Request Filters** answer "what is this request?" They match live request
properties (device, browser, query string, referrer) into
`RockRequestContext.PersonalizationRequestFilterIds`. No person record needed,
so they work for anonymous visitors.

**`PersonalizedEntity`** is the generic link table joining either of the above
to any entity, keyed on `EntityTypeId + EntityId + PersonalizationType +
PersonalizationEntityId`. Content channel items were the first consumer; event
items are the second.

Both are gated on the **site** setting `Site.EnablePersonalization`
(`Rock/Net/RockRequestContext.cs:452`). When it is off, neither id list is ever
populated and no personalization feature works anywhere on that site. Worth
knowing while testing.

## Requirements

### Filter semantics

- Filtering MUST be per personalization type and independently controllable. A
  caller can filter by segments only, request filters only, both, or neither.
- An event item with **no tags** of a given type MUST always be visible,
  regardless of the setting. Tagging narrows an item's audience; it never
  widens it.
- An event item MUST be hidden only when it has one or more tags of an enabled
  type and none of them match the current request.
- When both filters are enabled, an item MUST satisfy both independently. An
  item tagged with a non-matching segment is hidden even if its request filter
  matches.
- Filtering MUST apply to the event item, and therefore to every occurrence of
  that event item.

### Calendar Lava block

- MUST gain two boolean block attributes: **Filter by Personalization Segments**
  and **Filter by Request Filters**.
- The attribute keys and backing identifiers MUST be
  `FilterByPersonalizationSegments` and `FilterByRequestFilters`. The fuller
  "Personalization" wording is used on the block because block attribute names
  are user-facing prose read by administrators who may not know that "segments"
  means personalization segments.
- Both MUST default to `false`, so no existing site changes behavior on upgrade.
- Help text MUST state that this affects visibility in this block only and is
  not a security setting.

### CalendarEvents Lava command

- MUST gain two boolean parameters providing the same two controls.
- The parameter names MUST be `filterbysegments` and `filterbyrequestfilters`,
  **not** `filterbypersonalizationsegments`. See "Lava parameter naming" below
  for the evidence; Lava consistently refers to these as `segment` and
  `requestfilter` and diverging here would be the inconsistency.
- Both MUST default to `false` when the parameter is absent. Absent, empty, and
  any non-true value all mean off.
- The parameters MUST participate in the command's existing unknown-parameter
  validation (`settings.GetUnmatchedAttributes`), so a typo is reported rather
  than silently ignored.
- The filter MUST NOT affect the `EventScheduledInstance` command, which shares
  the same data source class. See "The shared data source" below.

### Documentation

The public Lava reference is hosted on the Rock community site, which runs on
Spark's own Rock server. The two kinds of Lava reference are stored differently
there, and this change touches the HTML-page kind:

| Lava reference type | Where it lives |
|---|---|
| Lava **filters** | A content channel on the community site |
| Lava **commands** | An HTML page on the community site |

`{% calendarevents %}` is a command, so **the deliverable is an edit to the
community site HTML page for Lava commands**, not a content channel item.

- The `{% calendarevents %}` command page MUST be updated with both new
  parameters, their default of `false`, and the filter semantics (an event with
  no tags of an enabled type is always visible).
- This edit happens outside this repository and requires access to the community
  site content. It MUST be treated as part of shipping this change, not a
  follow-up. A code change that adds undocumented Lava parameters is not done.
- The page SHOULD also mention that the Calendar Lava block has the equivalent
  two block attributes, so someone reading about one surface discovers the other.
- In this repository, `docs/event/calendar-and-occurrences.md:61` already
  mentions `{% calendarevents %}` and SHOULD gain a note that both the command
  and the Calendar Lava block support personalization filtering.

## Design

### One shared extension method

Both consumers build an `IQueryable<EventItemOccurrence>` and filter through the
`EventItem` navigation property:

- `RockWeb/Blocks/Event/CalendarLava.ascx.cs:435`
- `Rock/Lava/Blocks/CalendarEventsLavaDataSource.cs:495` (`GetBaseEventOccurrenceQuery`)

So one extension method serves both. Add it to the existing public static class
`Rock/Model/Event/EventItem/EventItemServiceExtensions.cs`, alongside
`HasActiveCalendarItems`, `InCalendar`, and friends:

```csharp
/// <summary>
/// Filters out occurrences whose event item is tagged for personalization
/// segments or request filters that the current request does not match.
/// Event items with no tags of an enabled type are always included.
/// </summary>
public static IQueryable<EventItemOccurrence> FilterByPersonalization(
    this IQueryable<EventItemOccurrence> occurrences,
    RockContext rockContext,
    bool filterByPersonalizationSegments,
    bool filterByRequestFilters,
    IEnumerable<int> matchedSegmentIds,
    IEnumerable<int> matchedRequestFilterIds )
```

Implementation, per enabled type, mirroring the proven predicate shape in
`ContentChannelView` (`Rock.Blocks/Cms/ContentChannelView.cs:1387-1391`):

```csharp
var entityTypeId = EntityTypeCache.Get<EventItem>().Id;

// Unexecuted IQueryables so EF emits subqueries rather than a large WHERE IN.
var allTaggedIds = rockContext.Set<PersonalizedEntity>()
    .Where( pe => pe.EntityTypeId == entityTypeId && pe.PersonalizationType == type )
    .Select( pe => pe.EntityId );

var matchedIds = rockContext.Set<PersonalizedEntity>()
    .Where( pe => pe.EntityTypeId == entityTypeId
        && pe.PersonalizationType == type
        && matchedIdList.Contains( pe.PersonalizationEntityId ) )
    .Select( pe => pe.EntityId );

occurrences = occurrences.Where( o =>
    !allTaggedIds.Contains( o.EventItemId ) || matchedIds.Contains( o.EventItemId ) );
```

The `!allTagged || matched` shape is what implements "untagged is always
visible". Applied once per enabled type, which gives the required independent
AND behavior.

### The accessibility constraint (why the ids are parameters)

`RockRequestContext.PersonalizationSegmentIds` and
`PersonalizationRequestFilterIds` are `internal`
(`Rock/Net/RockRequestContext.cs:160,168`), and `LavaPersonalizationHelper` is
an `internal static class`. **`RockWeb` is not in Rock's `InternalsVisibleTo`
list** (`Rock/Properties/AssemblyInfo.cs:19-36`), so `CalendarLava.ascx.cs`
cannot read either one directly.

Taking the matched id lists as ordinary parameters sidesteps this entirely and
keeps the method unit-testable. Add a convenience overload in the Rock assembly
that accepts the public `RockRequestContext` type and reads the internal
properties itself, so the WebForms block passes `RockPage.RequestContext`
(`Rock/Web/UI/RockPage.cs:308`, public) and never touches an internal member:

```csharp
public static IQueryable<EventItemOccurrence> FilterByPersonalization(
    this IQueryable<EventItemOccurrence> occurrences,
    RockContext rockContext,
    bool filterByPersonalizationSegments,
    bool filterByRequestFilters,
    RockRequestContext requestContext )
```

Do **not** solve this by making the `RockRequestContext` properties public.
That widens the public API surface for one caller's convenience, against the
guidance in `.claude/rules/code-conventions.md`.

### Call site: Calendar Lava block

Add the two `[BooleanField]` attributes with `DefaultBooleanValue = false`. None
of the block's existing 20 attributes set a `Category`, so these are declared
uncategorized to match. (The `[Category( "Event" )]` on the class is the block
*type* category and is unrelated to attribute grouping.) Then
apply the filter in the occurrence query at
`RockWeb/Blocks/Event/CalendarLava.ascx.cs:435-482`, after the existing audience
and campus filters:

```csharp
var filterByPersonalizationSegments = GetAttributeValue( AttributeKey.FilterByPersonalizationSegments ).AsBoolean();
var filterByRequestFilters = GetAttributeValue( AttributeKey.FilterByRequestFilters ).AsBoolean();

if ( filterByPersonalizationSegments || filterByRequestFilters )
{
    qry = qry.FilterByPersonalization( rockContext, filterByPersonalizationSegments, filterByRequestFilters, RockPage.RequestContext );
}
```

The guard matters: when both are off, no extra subqueries are emitted at all,
so the default upgrade path has zero query cost.

### Call site: CalendarEvents Lava command

Add two parameters, both defaulting to `false` when absent:

```liquid
{% calendarevents calendarid:'1' filterbysegments:'true' filterbyrequestfilters:'true' %}
```

Register both in `EventOccurrencesLavaDataSource`'s parameter list and include
them in the `GetUnmatchedAttributes` call
(`Rock/Lava/Blocks/CalendarEventsLavaDataSource.cs:96`) so typos surface as
errors.

#### Lava parameter naming

These are named `filterbysegments` / `filterbyrequestfilters` rather than
`filterbypersonalizationsegments`, deliberately, because Lava already has an
established vocabulary for these two concepts and it does not include the word
"personalization":

| Existing Lava surface | Parameter / value |
|---|---|
| `{% personalize %}` block | `segment` (alias `segments`) — `Rock/Lava/Blocks/PersonalizeBlock.cs:41,223` |
| `{% personalize %}` block | `requestfilter` (alias `requestfilters`) — `PersonalizeBlock.cs:45,171` |
| `AppendSegments` filter | `segments` — `Rock/Lava/Filters/LavaFilters.Personalization.cs:325` |

A template author who has written `{% personalize segment:'Young Families' %}`
should not have to learn a second word for the same thing on the next command.
`filterbypersonalizationsegments` would also be 31 characters, which is
unpleasant inline.

The C# side of the block uses the fuller `FilterByPersonalizationSegments`
because a block attribute label is administrator-facing prose where the extra
word earns its place. The asymmetry is intentional: different audiences, different
conventions. If the team would rather have one name across both surfaces, the
Lava side is the one that should not move.

Because this code lives in the Rock assembly, it can resolve the matched ids
through the established Lava path rather than a request context:

- `LavaPersonalizationHelper.GetPersonalizationSegmentIdListForPersonFromContextCookie`
  (`Rock/Lava/Helpers/LavaPersonalizationHelper.cs:45`), which handles the
  segment cookie and anonymous visitors.
- `LavaPersonalizationHelper.GetPersonalizationRequestFilterIdList`
  (`Rock/Lava/Helpers/LavaPersonalizationHelper.cs:165`).

### The shared data source

This is the one place the implementation can go quietly wrong.

`CalendarEvents` and `EventScheduledInstance` are two Lava commands served by
one class. Both funnel into `GetFilteredEventOccurrenceSummaries`
(`Rock/Lava/Blocks/CalendarEventsLavaDataSource.cs:506`):

- `CalendarEvents` scopes by calendar (`:140`)
- `EventScheduledInstance` scopes to one named event item (`:215`)

The obvious place to add the filter is that shared method. Doing so would
silently apply personalization filtering to `EventScheduledInstance`, where the
template author named a specific event and must get it back. Filtering it to
nothing reads as a bug, not personalization.

Apply the filter on the `CalendarEvents` path only, or pass it as a setting
that only the `CalendarEvents` block populates. A unit test MUST pin this
behavior (see Verification step 6).

### Why filter-only, with no Prioritize

Rock's existing `PersonalizationFilterType` enum offers `Ignore`, `Prioritize`,
and `Filter`, and `ContentChannelView` exposes all three. This spec deliberately
uses two independent booleans instead, for two reasons.

`Prioritize` means "show everything, sort matching items first". Both surfaces
here render occurrences in chronological order, which is the entire point of a
calendar. Re-sorting a calendar by segment match would produce nonsense output.
So `Prioritize` has no meaningful implementation on these surfaces.

Two booleans are also more expressive than the enum in the dimension that
matters here: the enum cannot express "filter by segments but ignore request
filters", while two flags can.

This is a deliberate divergence from the `ContentChannelView` vocabulary. If a
future event surface has a relevance-ordered result set, `Prioritize` can be
added there without disturbing this design.

## Out of Scope

Recorded so they are not lost, and so nobody re-derives them.

- **Content Collections.** Confirmed working in a live test. `EventItemDocument`
  routes through `AddPersonalizationData`
  (`Rock/Cms/ContentCollection/IndexDocuments/IndexDocumentBase.cs:503`), which
  is entity-generic, and the boost query in `ContentCollectionView` matches on
  index field names rather than item type. Note that Content Collections
  **boost** relevance and never hard-filter, so they are a different behavior
  from what this spec adds, not a duplicate of it.
- **`EventScheduledInstance` Lava command** and **`EventItemOccurrenceLava`
  block.** Both render a single explicitly-identified event. See "The shared
  data source" for why `EventScheduledInstance` needs active protection rather
  than mere omission.
- **The other seven event surfaces** (`EventItemListLava`,
  `EventItemOccurrenceListLava`, `EventItemOccurrenceListByAudienceLava`,
  `EventItemOccurrencesSearchLava`, `EventDetailWithOccurrencesSearchLava`, and
  the three mobile event blocks). Same extension method will serve them if and
  when they are wanted. Not needed now.
- **Extracting a generic personalization filter helper and refactoring
  `ContentChannelView` onto it.** The event-specific extension proposed here is
  a small amount of duplication against `ContentChannelView`'s private methods.
  Generalizing across both entity types is a worthwhile future cleanup but is
  not required for two consumers.
- **The missing re-index on personalization change.** Saving personalization
  tags does not trigger a Content Collection re-index, for event items or
  content channel items. Real bug, unrelated to these two consumers (neither
  uses the search index), and pre-existing. Should be filed separately.
- **`ContentCollectionView` boost defaults.** `SegmentBoostAmount` defaults to
  null and the `?? 1.0d` fallback makes that a no-op, while the
  `BoostMatchingSegments` / `BoostMatchingRequestFilters` checkboxes are never
  read in the boost path. Found while diagnosing why personalization appeared
  not to work. Real bugs, separate spec.
- **Adding `EnablePersonalization` to `EventCalendar`.** With per-block opt-in
  there is no need for a calendar-level flag. Site-level
  `Site.EnablePersonalization` remains the master switch.

## Fix Risks

1. **Personalization is not security.** These filters hide events from a
   calendar listing. They do not protect the event detail page, the occurrence
   page, or registration. A partner who treats segment filtering as access
   control will be surprised. Both attributes' help text must say so.
2. **Query cost.** Each enabled filter adds two correlated subqueries against
   `PersonalizedEntity`. Written as unexecuted `IQueryable`s so EF emits
   subqueries rather than a large `WHERE IN`, per
   `.claude/rules/data-model.md`. Verify the generated SQL on a calendar with
   many occurrences.
3. **Silent empty calendar.** If a site has `Site.EnablePersonalization` off but
   an administrator enables these block attributes, the matched id lists are
   empty, so every tagged event is hidden while untagged events still show. That
   is technically correct behavior but confusing. Consider a block-level
   validation message or documentation note.
4. **Divergence from `ContentChannelView`.** Two booleans here versus a
   three-value enum there. Deliberate and explained above, but it is a second
   vocabulary for the same feature area and should be called out in
   documentation.
5. **Output caching.** Checked: neither `CalendarLava` nor the `CalendarEvents`
   command caches rendered output or item lists, so there is no risk of serving
   one visitor's personalized calendar to another. Any future caching added to
   either surface must vary on the personalization id lists.

## Verification Steps

1. Unit-test the extension method against a fixture of event items that are
   untagged, tagged-matching, and tagged-not-matching, for segments only,
   request filters only, and both. Assert the exact visible set in each of the
   four flag combinations.
2. Confirm an untagged event item is visible in every combination.
3. Confirm an event item tagged with a non-matching segment is hidden when
   segment filtering is on, and visible when it is off.
4. Confirm an event item tagged with a matching segment but a non-matching
   request filter is hidden when both filters are on.
5. In Calendar Lava with both attributes off (the default), confirm the rendered
   calendar is byte-identical to current behavior and the generated SQL contains
   no `PersonalizedEntity` subquery.
6. Render `EventScheduledInstance` for a tagged event as a person matching none
   of its segments; the event MUST still render. This is the regression pin for
   the shared data source.
7. Render `CalendarEvents` with `filterbysegments:'true'` and confirm it matches
   the Calendar Lava block's behavior for the same visitor and calendar.
8. Pass a misspelled parameter to `CalendarEvents` and confirm it is reported as
   an unmatched attribute.
9. With `Site.EnablePersonalization` off and both attributes on, confirm the
   behavior matches Fix Risk 3 (tagged hidden, untagged shown) and is not an
   exception.
10. Confirm the `{% calendarevents %}` command page on the community site
    documents both new parameters and their defaults. This is a required step,
    not a nicety; see the Documentation requirements.

## Considered but Rejected

### Use the PersonalizationFilterType enum for consistency with ContentChannelView
Rejected. `Prioritize` is meaningless on a chronologically ordered calendar, and
the enum cannot express filtering by one personalization type but not the other.
See "Why filter-only" above.

### Add the filter inside GetFilteredEventOccurrenceSummaries
Rejected. That method also serves `EventScheduledInstance`, which must never
filter. Applying it upstream on the `CalendarEvents` path keeps the excluded
command safe by construction rather than by convention.

### Make RockRequestContext.PersonalizationSegmentIds public
Rejected. `RockWeb` cannot see internals, but the fix is to pass the data or the
public context object into a helper in the Rock assembly, not to widen the
public API for one caller.

### Extract a fully generic helper and refactor ContentChannelView first
Rejected for this scope. Correct eventually, but it turns a two-consumer feature
into a cross-cutting refactor of a heavily used CMS block. Noted in Out of Scope.

### A single "Enable Personalization" boolean instead of two
Rejected. Segments and request filters answer different questions (who the
person is versus what the request is) and administrators will reasonably want
one without the other.

## Related

- Prior spec that added the write side and deferred this: [Event Item Detail Polish + Personalization](completed/event/260727-event-item-detail-polish-personalization.md)
- Reference filter predicate to mirror: `Rock.Blocks/Cms/ContentChannelView.cs:1387-1391`
- Link table: `Rock/Model/CRM/PersonalizedEntity/PersonalizedEntity.cs`
- Consumer 1: `RockWeb/Blocks/Event/CalendarLava.ascx.cs:435`
- Consumer 2: `Rock/Lava/Blocks/CalendarEventsBlock.cs`, `Rock/Lava/Blocks/CalendarEventsLavaDataSource.cs:96,140,215,495,506`
- Proposed helper home: `Rock/Model/Event/EventItem/EventItemServiceExtensions.cs`
- Request context and the site gate: `Rock/Net/RockRequestContext.cs:160,168,452`
- Lava id resolution: `Rock/Lava/Helpers/LavaPersonalizationHelper.cs:45,165`
- Internals visibility constraint: `Rock/Properties/AssemblyInfo.cs:19-36`
