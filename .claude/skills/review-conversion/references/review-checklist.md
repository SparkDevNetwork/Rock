# Review Checklist

Work through each category below. Check every item against both the WebForms source and the Obsidian implementation.

---

## How to use this checklist

`/review-conversion` v2 splits work between **/working/-driven audit passes** (Phase 2A-2E in `SKILL.md`) and **this generic checklist** (Phase 3). When the convert-block research artifacts are present, several sections here become spot-checks — duplicating them in full is wasted effort.

| Section | Run mode (with /working/) | Run mode (without /working/) |
|---|---|---|
| §1 Functional Parity | **Spot-check** — covered by Phase 2A (parity-map.md) | Full sweep |
| §2 Performance and Modernization | **Spot-check** — covered by Phase 2B (improvement-analysis.md) | Full sweep |
| §3 Bug Patterns | **Spot-check** — mostly covered by Phase 2B; sweep code added post-research | Full sweep |
| §4 Rock RMS Patterns | **Full sweep** — naming/style not in /working/ | Full sweep |
| §5 Obsidian-Specific | **Full sweep** — bag types / block actions / reactive state not in /working/ | Full sweep |
| §6 Grid Column Type Matrix | **Full sweep** — highly specialized, not in /working/ | Full sweep |
| §7 Modernization Checks | **Full sweep** — general checks beyond improvement-analysis.md | Full sweep |

Spot-check = scan for issues the per-block artifacts didn't capture (e.g., a bug introduced after convert-block research finished). Full sweep = walk every checkbox.

---

## 1. Functional Parity

### Data Access
- [ ] All database queries from WebForms are replicated with equivalent filters
- [ ] `.Include()` / eager loading matches (no missing navigation properties)
- [ ] Ordering/sorting is preserved
- [ ] Pagination logic is preserved (if applicable)
- [ ] Any `.Select()` projections produce equivalent data

### CRUD Operations
- [ ] Create/Save logic matches (all fields saved, validation preserved)
- [ ] Delete logic matches (soft delete vs hard delete, cascade behavior)
- [ ] Edit/Update logic matches (all editable fields, pre/post-save hooks)
- [ ] Entity attributes are saved/loaded if the WebForms block handled them

### Security
- [ ] `IsUserAuthorized( Authorization.EDIT )` and similar checks preserved
- [ ] Entity-level security checks preserved (e.g., `campus.IsAuthorized()`)
- [ ] Block security attribute (`[SecurityAction]`) matches WebForms
- [ ] Admin-only features gated the same way

### Navigation
- [ ] All linked page navigations preserved (detail page, parent page, etc.)
- [ ] Page parameters use `PageParameterKey` constants
- [ ] Page parameter names match WebForms (or use the correct Obsidian convention)
- [ ] Return URL / cancel navigation works correctly
- [ ] Breadcrumb behavior matches (if `IBreadCrumbBlock` is implemented)

### User Preferences
- [ ] Any `PersonPreference` (or legacy `UserPreference`) values saved in WebForms are preserved
- [ ] Preference keys match or are properly migrated
- [ ] Grid filter preferences persist correctly

### UI Behavior
- [ ] Show/hide logic for panels matches WebForms conditions
- [ ] Validation messages match (required fields, format validation)
- [ ] Success/error notification messages preserved
- [ ] Loading states handled (no flash of empty content)
- [ ] Modal dialogs replicated if WebForms used them

---

## 2. Performance and Modernization

### N+1 and Query Performance
- [ ] No service `.Get()` or `.Queryable()` calls inside `foreach`/`for` loops — must pre-fetch
- [ ] No `new XService( rockContext )` created per loop iteration — instantiate once before the loop
- [ ] Navigation properties accessed in loops have matching `.Include()` or are pre-fetched into a dictionary
- [ ] Large result sets use `IQueryable` subqueries, not in-memory `List.Contains()`

### Cache vs Service
- [ ] `DefinedTypeCache` / `DefinedValueCache` used instead of `DefinedTypeService` / `DefinedValueService` for read-only lookups
- [ ] `CampusCache.All()` used instead of `CampusService.Queryable()` for read-only campus lists
- [ ] `GroupTypeCache` / `EntityTypeCache` / `CategoryCache` used where applicable
- [ ] Service queries reserved for writes, complex joins, or entities without cache classes

### WebForms Carry-Forward (should NOT be present)
- [ ] No commented-out WebForms code
- [ ] No `ViewState` or `IsPostBack` remnants
- [ ] No `string.Format()` — use string interpolation
- [ ] No deep null-check nesting — use `?.` / `??` / early returns
- [ ] No empty `catch { }` blocks without intentional-ignore comments
- [ ] No unused `#region` blocks or `using` statements carried from WebForms

### Grid / Template (for List blocks)
- [ ] Column components match data types (`BooleanColumn` for bools, `NumberColumn` for numbers, etc.)
- [ ] `PersonColumn` has a `filterValue` function (not `TextColumn`)
- [ ] Complex/HTML columns have `filterValue` and `quickFilterValue` functions
- [ ] Display-only computed columns have `:excludeFromExport="true"`
- [ ] `v-for` `:key` uses `index` as fallback (not `''`) when key could be null
- [ ] Name columns that display a description / subtitle below the name use `HighlightDetailColumn` (or `PersonColumn` for people, with `:hideAvatar="true"` if no avatar is wanted) — NOT a custom `<template #format>` with `<b>...</b><br>` or `text-semibold` + `text-muted`
- [ ] If a Name column uses `HighlightDetailColumn` with `detailField`, any standalone Description column displaying the same field has been removed
- [ ] No redundant `sortValue` / `filterValue` / `quickFilterValue` props on `HighlightDetailColumn` that just concatenate `field + detailField` (the column does this by default)
- [ ] No custom two-line `<template #skeleton>` on `HighlightDetailColumn` (the default skeleton handles it)

---

## 3. Bug Patterns

### C# Block
- [ ] No `new RockContext()` — use `RockContext` from base class
- [ ] No `using` blocks around `RockContext` (kills lazy loading)
- [ ] Nullable types handled (`.Value` used safely, null checks before access)
- [ ] `int.Parse` / `int.TryParse` used correctly (not swapped, not missing error handling)
- [ ] Entity lookups handle "not found" case (null check after `.Get()`)
- [ ] String comparisons use `StringComparison.OrdinalIgnoreCase` where appropriate
- [ ] No `System.Web` references outside `#if WEBFORMS` blocks
- [ ] `RockDateTime` used instead of `DateTime`

### TypeScript / Vue
- [ ] Block action calls handle error responses (check `isSuccess` or `data` before using)
- [ ] Reactive state updated correctly (no direct prop mutation)
- [ ] `parseInt()` / type coercion used safely (NaN handling)
- [ ] Event handlers don't swallow errors silently
- [ ] Computed properties don't have side effects
- [ ] No TypeScript `any` types where a proper type exists

### Bag Design
- [ ] All bag properties populated in C# (no properties defined but never set)
- [ ] All bag properties consumed in TypeScript (no unused properties)
- [ ] Options bag contains only static/config data (not per-request data)
- [ ] Main bag contains the dynamic/entity data
- [ ] Bag property types match between C# and `.d.ts` (especially nullables)

---

## 4. Rock RMS Patterns (per CLAUDE.md)

### Naming
- [ ] C# classes/methods: PascalCase
- [ ] C# variables/parameters: camelCase
- [ ] TypeScript functions/variables: camelCase
- [ ] Boolean properties start with `Is` or `Has`
- [ ] No abbreviations or single-character variables (except loop `i`)

### Block Architecture
- [ ] `AttributeKey` constants in nested `private static class`
- [ ] `PageParameterKey` constants in nested `private static class`
- [ ] `PersonPreferenceKey` constants if preferences are used
- [ ] Field attributes declared vertically with property assignment (not constructor params)
- [ ] Page parameters accessed via `PageParameter( PageParameterKey.X )` — not `Request.Params`

### Code Style
- [ ] Braces on all `if`/`for`/`else`/`while` — even single-line
- [ ] Early returns used (not deeply nested if/else)
- [ ] `var` used consistently
- [ ] Methods are focused (single responsibility)
- [ ] Comments explain "why", not "what"

### Entity Handling
- [ ] Favor `Id` over `Guid` in LINQ `.Where()` clauses (especially for cached entities)
- [ ] Entity lookup uses `service.Get( key, !PageCache.Layout.Site.DisablePredictableIds )` pattern for page params
- [ ] Large `WHERE IN` lists avoided (use `IQueryable` subquery instead)

---

## 5. Obsidian-Specific Patterns

### C# Block Structure
- [ ] Correct base class used (`RockBlockType`, `RockDetailBlockType`, `RockListBlockType`, or `RockEntityDetailBlockType`)
- [ ] `[BlockType]` attribute has correct `Name`, `Category`, `Description`
- [ ] `[SystemGuid.BlockType]` attribute present with valid GUID
- [ ] `[SystemGuid.EntityType]` attribute present with valid GUID
- [ ] `GetObsidianBlockInitialization` populates both bags correctly
- [ ] Block actions decorated with `[BlockAction]`
- [ ] Block actions validate security before performing operations
- [ ] Block actions return `ActionBadRequest()` for invalid input, not exceptions

### Vue Component
- [ ] Uses `defineComponent` with proper setup
- [ ] `useConfigurationValues` for options bag
- [ ] `useInvokeBlockAction` for server calls
- [ ] Template uses Obsidian UI components (not raw HTML where components exist)
- [ ] Emits and props properly typed

### Integration
- [ ] `.d.ts` files match bag property names and types exactly
- [ ] Bag property names in C# (PascalCase) map to camelCase in TypeScript
- [ ] Enum values serialized correctly between C# and TypeScript

---

## 6. Grid Column Type Matrix (for List blocks)

Use this matrix to verify every grid column uses the correct component for its data type:

| C# Bag Property Type | Correct Column Component | Common Mistake |
|---|---|---|
| `bool` / `bool?` | `BooleanColumn` | Using `TextColumn` |
| `DateTime` / `DateTime?` | `DateColumn` or `DateTimeColumn` | Using `TextColumn` |
| `int` / `decimal` / `double` | `NumberColumn` | Using `TextColumn` |
| `string` (name with adjacent description / subtitle) | `HighlightDetailColumn` (`field` + `detailField`) | Hand-rolled `<template #format>` with `<b>...</b><br>` or `text-semibold` + `text-muted` |
| `string` (person name) | `PersonColumn` (with `:hideAvatar="true"` if no avatar wanted; `detailField` for subtext) | Using `TextColumn` without `filterValue`; using `HighlightDetailColumn` for a person |
| `string` (currency) | `CurrencyColumn` | Using `TextColumn` or `NumberColumn` |
| `string` (enum display) | `TextColumn` with `filterValue` | Missing `filterValue` for filtering |
| `string` (HTML content) | `TextColumn` with `filterValue` + `quickFilterValue` | Missing filter functions, missing `:excludeFromExport` |
| `ListItemBag` | Column appropriate for display | Not extracting `.text` for display |

### Grid filter requirements
- [ ] Every column that displays non-plain-text data has a `filterValue` function
- [ ] `PersonColumn` always has `filterValue` returning the person's name as text
- [ ] Columns displaying HTML or rich content have `quickFilterValue` for search
- [ ] Computed/display-only columns (e.g., concatenated fields) have `:excludeFromExport="true"`
- [ ] Grid settings modal emits `close` and the parent watches for it (standard pattern)
- [ ] `HighlightDetailColumn` consumers do NOT redefine `filterValue` / `quickFilterValue` to `${field} ${detailField}` — the column already does this via `getCombinedFilterValue`

---

## 7. Modernization Checks (should NOT carry forward from WebForms)

These patterns are acceptable in WebForms but should be improved during conversion. Flag as Warning if found in the Obsidian code:

### Query patterns to modernize
- [ ] No N+1 queries — if WebForms had `.Get()` in a loop, Obsidian must pre-fetch into a dictionary/list
- [ ] No service lookups for cached entities — use `DefinedTypeCache`, `CampusCache`, `GroupTypeCache`, `EntityTypeCache`, `CategoryCache` for read-only access
- [ ] No redundant round-trips — data available at initialization should be in the bag, not a separate block action
- [ ] No `ToList()` followed by in-memory filtering when the filter could run in the database query

### Code patterns to modernize
- [ ] No `string.Format()` — use string interpolation
- [ ] No deep null-check nesting — use `?.`, `??`, and early returns
- [ ] No empty `catch { }` without intentional-ignore comment
- [ ] No `#region` blocks carried from WebForms (Obsidian blocks should be clean enough not to need them)
- [ ] No commented-out WebForms code
- [ ] No `ViewState` or `IsPostBack` remnants or equivalents

### Architecture to modernize
- [ ] Logic that ran on every postback in WebForms should only run when actually needed in Obsidian
- [ ] Multiple sequential WebForms events (e.g., button click → save → redirect) should be a single block action
- [ ] WebForms modal popup patterns should use Obsidian's `Modal` component, not custom show/hide logic
