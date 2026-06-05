# Check-in Areas & Groups: Legacy (WebForms) vs Obsidian

A differences-only comparison between the legacy WebForms **Check-in Areas** block (`RockWeb/Blocks/CheckIn/Config/CheckinAreas.ascx[.cs]`, plus the `CheckinArea` / `CheckinGroup` / `CheckinAreaRow` / `CheckinGroupRow` server controls) and the new Obsidian **Check-in Areas and Groups** block (`Rock.Blocks/CheckIn/Configuration/CheckInAreasAndGroups.cs` plus the `checkInAreasAndGroups.obs` SFC and its partials).

Rows that behave identically are omitted by design. A short "Verified at parity" list at the end records the items that were checked and found equivalent, so they are not mistaken for gaps.

> **Scope note.** The legacy page paired two blocks: **Check-in Types** (the template list) and **Check-in Areas** (this editor). Only **Check-in Areas** is compared here. The Check-in Types list moved to its own Obsidian block (**Check-in Configuration List**), and a check-in type's own settings moved to **Check-in Configuration Settings**. Those two are out of scope; they are mentioned only where they explain a hosting change.

## Legend

| Marker | Meaning |
|:--:|---|
| 🟢 | **Added**: present in the new block, no legacy equivalent |
| 🔴 | **Dropped**: present in legacy, intentionally removed (or relocated to another block) |
| 🟡 | **Changed**: present in both, but renamed, restyled, or behaves differently |
| ✅ | **Resolved**: a legacy-only gap at first review, since addressed in the Obsidian block |

## At a glance

- The conversion deliberately targets the Figma redesign, not a one-for-one port, so most differences are **Added** (new capabilities) or **Changed** (redesigned UI). No core editing capability was lost.
- The biggest behavioral shifts: a **client-side placeholder create flow** (deferred persistence) replacing legacy's create-immediately-in-DB; **campus context** and an **area slicer** that legacy never had; and a **live kiosk-refresh push** on every mutation.
- The few true **Dropped** items are the embedded "secondary block" hosting model, the conditional hiding of "Special Needs", and the always-on Classic Labels grid (now a toggle). The browser tab-close (`beforeunload`) prompt was the one gap closed during this review (see §9).

---

## 1. Architecture & Hosting

| Feature | Legacy "Check-in Areas" | New "Check-in Areas and Groups" | Δ |
|---|---|---|:--:|
| Block framework | WebForms (`.ascx` + code-behind, ViewState, postbacks) | Obsidian (C# `RockBlockType` + Vue SFC, block actions) | 🟡 |
| Block name | "Check-in Areas" | "Check-in Areas and Groups" | 🟡 |
| Page parameter | `CheckInTypeId` (integer id) | `CheckInConfiguration` (IdKey / Guid) | 🟡 |
| Hosting model | `ISecondaryBlock`: lived on the same page as the Check-in Types list and hid itself (`pnlDetails.Visible = false`) until a type was selected | Standalone block on its own route; the type list and type settings are separate pages/blocks | 🔴 |
| Panel title | Static text "Areas and Groups" | The selected check-in **type's name** (dynamic), since the block no longer sits under a type-list for context | 🟡 |
| Campus awareness | None | `[ContextAware(Campus)]` + a campus context slicer | 🟢 |
| Block settings | None | "Enable Classic Check-in Labels" (boolean, default on) | 🟢 |
| Page location | Deeply nested under check-in settings | Migration reparents the config pages directly under Admin Tools and adds a dedicated Areas-and-Groups page/route | 🟡 |

---

## 2. List / Tree Pane & Filtering

| Feature | Legacy | New | Δ |
|---|---|---|:--:|
| Area slicer | None: always renders the entire tree | "All Areas" + per-area dropdown that scopes the left tree to one area's branch | 🟢 |
| Campus slicer | None | Campus context picker (scopes the location picker and named-location display, not the tree) | 🟢 |
| "Show Inactive Groups" | Checkbox in the panel header (auto-postback) | Same option, relocated into a "List Settings" modal | 🟡 |
| List Settings modal | None | "Areas & Groups List Settings" modal (opened from a gear/adjust button) | 🟢 |
| "Disable Auto-Collapse" | None: every row always rendered expanded | New preference; help: "Enable to keep multiple Areas and Groups expanded at once. By default, selecting one will collapse the others." | 🟢 |
| Auto-collapse behavior | None (no real collapse concept; rows always expanded) | Selecting a node collapses branches outside its ancestor chain (unless disabled) | 🟢 |
| 5-level nesting cap | No limit on nesting depth | Inline "Add Area"/"Add Group" hidden once a row sits at level 5 (preexisting deeper trees still render) | 🟢 |
| Loading indicators | None (full-page postbacks) | Delayed spinners for tree refetch and detail load | 🟢 |
| Empty states | None (empty list + Add button) | Left: "No Groups or Areas to Configure" / "Click + Add Area to Get Started". Right: "Nothing To Show" / "Select an Area or Group to View or Configure" | 🟢 |
| Inactive row display | Appends " (Inactive)" text to the name; `is-inactive` class | Faded `.inactive` row styling, no text suffix | 🟡 |
| Row icons & states | Area `ti-folder-open`, group `ti-circle-check`; reorder via `ti-menu-2` | Area `ti-stack-2`, group `ti-users-group`; hover swaps icon to grip handle; redesigned selected/drag states (Figma) | 🟡 |
| Deep-linking | None (selection in ViewState only) | Selected node mirrored to an `AreaOrGroup` query param (back/forward supported) | 🟢 |
| Reorder mechanism | jQuery-UI sortable + full postback; `GroupType/Group.Reorder` over rendered siblings | dragula + per-op `ReorderArea`/`ReorderGroup` block actions; optimistic with revert-on-failure | 🟡 |
| Inactive siblings in reorder | Reorders only the rendered (visible) rows | `ReorderGroup` always renumbers across active + inactive siblings, so dragging a visible group can shift an adjacent hidden inactive one | 🟡 |

---

## 3. Add & Create Flow

| Feature | Legacy | New | Δ |
|---|---|---|:--:|
| Create semantics | Clicking Add **inserts the GroupType/Group into the database immediately** (named "New Area"/"New Group"), then selects it | Inserts a **client-side placeholder**; persists only on Save. Cancel discards with no DB row | 🟡 |
| Empty name handling | Auto-named "New Area"/"New Group" | Pre-seeded name; clearing it produces a required-field validation error rather than silently auto-naming | 🟡 |
| Add-action tooltips | "Add New Area" / "Add Sub-Area" / "Add New Group" | "Add Area" / "Add Group" (same wording at top level and inline) | 🟡 |
| Top-level Add Area | Button at the bottom of the left column | "+ Add Area" button in the list-pane header | 🟡 |
| New-group filter schema | Group created first, so inherited filter fields appear only after creation | `GetPlaceholderGroupSchema` pre-loads the inherit-chain schema so "Check-in Filters" render immediately on a brand-new group | 🟢 |

---

## 4. Area Editor

| Feature | Legacy | New | Δ |
|---|---|---|:--:|
| Layout | Flat list of fields | Grouped into collapsible "Group Check-in Settings" and "Printing & Label Settings" sections | 🟡 |
| Area name field | Label "Check-in Area Name" | Label "Area Name" | 🟡 |
| Inherit-from field | Label "Inherit from" | Label "Inherit Check-in Setup Type From" | 🟡 |
| Membership rule control | "Check-in Rule" **dropdown** | "Group Membership Requirement" **radio list** (same help text) | 🟡 |
| Print To control | "Print To" **dropdown** | "Print To" **radio list** (same help text) | 🟡 |
| Inherited attributes refresh | Changing inherit-from required **Save + page reload** before the attribute editor updated (legacy "null then LoadAttributes" two-stage) | Schema swaps **instantly client-side** when inherit-from changes (no round-trip), inside a `ConditionalWell` | 🟡 |
| "Check-in Labels" grid | Refers to **classic binary-file** labels | Refers to **next-gen `CheckInLabel`** entities (terminology flipped, see Labels section) | 🟡 |
| Add-label UX | Dedicated modal with a "Select Check-in Label" dropdown | Inline grid picker modal; empty state "No labels are available to add." | 🟡 |
| "Create Label" link | None | External-tab link to create a new label design from within the grid header | 🟢 |

---

## 5. Group Editor

| Feature | Legacy | New | Δ |
|---|---|---|:--:|
| Group name field | Label "Check-in Group Name" | Label "Group Name" | 🟡 |
| "Special Needs" checkbox | Shown **only when** the configuration enables special-needs logic (`AreNonSpecialNeedsGroupsRemoved` or `AreSpecialNeedsGroupsRemoved`) | **Always shown** (same help text) | 🟡 |
| Attribute presentation | All group attributes (own + inherited filters) rendered together in one block | Split into a "Group Attributes" section (own) and a "Check-in Filters - {inherited type}" section (inherited), each conditional | 🟡 |
| Variant titling | No dynamic section title | "Check-in Filters" title appends the inherited setup-type name (By Ability Level / Age Range / Data View / Grade) | 🟢 |

---

## 6. Named Locations (Group Editor)

| Feature | Legacy | New | Δ |
|---|---|---|:--:|
| Section / grid labels | "Locations" and "Overflow Locations" headings | "Configure Named Locations" section with "Main" and "Overflow" grids | 🟡 |
| Picker modal | "Select Check-in Location" | "Select Location" | 🟡 |
| Campus scoping | None: picker shows the full location tree; no campus filter | Picker root scoped to the active campus; displayed Main/Overflow grids filter to the active campus (instant, client-side) | 🟢 |
| Add-location server validation | Resolved by id only | `ResolveNamedLocation` rejects locations outside the active campus before they enter the working copy | 🟢 |
| Duplicate prevention | Silent no-op when a location is already attached | Explicit inline error: "\"{name}\" is already attached to this group." | 🟡 |

> Main/Overflow grids, the overflow help text, and reorder are at parity (see the parity list).

---

## 7. Check-in Labels (terminology flip)

The phrase "Check-in Labels" refers to **different things** in each block. Watch this when training staff or reading old docs.

| Storage model | Legacy grid title | New grid title | Δ |
|---|---|---|:--:|
| `CheckInLabel` entities (next-gen) | "Next-Gen Check-in Labels" | "Check-in Labels" | 🟡 |
| Binary-file label attributes (classic) | "Check-in Labels" | "Classic Check-in Labels" | 🟡 |
| Classic-labels grid visibility | Always shown | Shown only when the "Enable Classic Check-in Labels" block setting is on (or the area already has classic labels attached) | 🟢 |

---

## 8. Save, Delete & Validation

| Feature | Legacy | New | Δ |
|---|---|---|:--:|
| Save model | Single "Save" button persisting whichever editor (area or group) is visible | Each editor owns its own Save via `SaveArea` / `SaveGroup` block actions | 🟡 |
| Cancel for unsaved entity | None (entities were already persisted, so only "Delete" existed) | Footer shows "Cancel" (discard placeholder, no prompt) for unsaved entities and "Delete" once persisted | 🟢 |
| Validation display | `ValidationSummary` + a "Please correct the following" notification at the top | Inline `RockValidation` errors inside the editor body; top-of-form stack suppressed | 🟡 |
| Delete-blocked feedback | Top-of-block warning notification | Server `CanDelete` reason surfaced inline in the editor | 🟡 |
| Delete confirmation wording | `"{name}" check-in area` / `"{name}" check-in group` + "This action cannot be undone." (group adds the attendance-loss caution) | "Are you sure you want to delete this \"{name}\" check-in area/group? ..." (same meaning; group keeps the attendance-loss caution) | 🟡 |

---

## 9. Unsaved Changes, Kiosk Refresh & Security

| Feature | Legacy | New | Δ |
|---|---|---|:--:|
| In-app unsaved-changes guard | `isDirty()` JS confirm: "You have not saved your changes. Are you sure you want to continue?" | `navigationGuard()` confirm: "Changes have been made that have not yet been saved. Do you want to leave without saving?" | 🟡 |
| Guard coverage | Row clicks and the top-level Add button | Row clicks, **plus** the area slicer (reverts the dropdown on cancel) and campus context changes | 🟢 |
| Browser tab-close prompt | Native `beforeunload` dialog on navigating away/closing the tab | ✅ Matched: a `beforeunload` guard wired to `isEditorDirty` was added (alongside the in-app guard), so tab close / refresh / external nav prompts while an edit is unsaved | ✅ |
| Kiosk refresh | `KioskDevice.Clear()` (cache clear only) | `RefreshConnectedKiosks()`: clears the cache **and** pushes `SendRefreshKioskConfiguration` so connected kiosks refresh live | 🟢 |
| Cross-configuration guard | None: entities resolved by Guid globally | Every read/mutate action refuses ids that resolve outside the configuration in the URL | 🟢 |

---

## Verified at parity (not differences)

These were compared and found equivalent, so they are intentionally absent from the tables above:

- **Sibling-only, same-type reorder** (areas among areas, groups among groups under the same parent).
- **Group soft-delete cascade**: inactivate when the group has attendance history, hard-delete otherwise. Replicated exactly.
- **Area delete guards**: `CanDelete` plus the "assigned as an inherited group type" block; association cleanup before delete.
- **Inherit-from resolution** three-way logic (take new / clear / preserve orphaned reference). Replicated.
- **Prevent Concurrent Check-in**, **Matching Logic** (conditional on "Already Enrolled in Group"), and their help text.
- **Overflow help text** ("Overflow locations will be used if all non-overflow locations are at capacity..."), verbatim.
- **Named-location Main/Overflow grids** with add / delete / reorder.
- **`PrintTo.Default` normalized to Kiosk** on save (neither UI can represent Default).
- **Per-variant filter fields** (Ability Level / Age Range / Data View / Grade) are attribute/`FieldType`-driven on both sides, so the rendered controls match.
- **Default group role** auto-linked to the first ("Member") role on a new area.
- **Save-success message** "Changes have been saved."
- **"Requires Background Check"** and similar group-type attributes (rendered on both sides; the new block just relocates them into the "Group Attributes" section).

---

## Sources

- Legacy block: `RockWeb/Blocks/CheckIn/Config/CheckinAreas.ascx[.cs]` (recovered from git `HEAD`; the files are chopped in the working tree).
- Legacy editor controls: `Rock/Web/UI/Controls/Checkin Configuration Controls/CheckinArea.cs`, `CheckinGroup.cs`, `CheckinAreaRow.cs`, `CheckinGroupRow.cs`.
- New block: `Rock.Blocks/CheckIn/Configuration/CheckInAreasAndGroups.cs`; `Rock.JavaScript.Obsidian.Blocks/src/CheckIn/Configuration/checkInAreasAndGroups.obs` and its partials.
- Design intent and accepted deltas: `specs/260506-check-in-areas-and-groups-obsidian-conversion.md` (see its "Accepted Behavior Differences from Legacy" section).
