---
author: Joshua Henninger
date_created: 2026-09-21
summary: >-
  Proof of concept for the Rock Page Builder. Drag module types from a sidebar into
  page-builder-enabled zones on a live page rendered in an iframe. Each drop saves a
  real Canvas block that stores its module in block attributes and renders the module's
  Lava, with a reusable Obsidian Sheet control for editing a placed module.
contributors: []
---

# Page Builder POC

## Summary

The Page Builder lets an administrator compose a page visually: a sidebar of module
types on the left, the live page rendered in an iframe on the right, and drag and drop
between them. This spec covers a proof of concept only. It targets modules (not blocks
or freeform elements), runs against an internal Page Builder page, and exists to prove
two things are buildable inside Rock before the full feature is scoped: drag and drop
across an iframe boundary, and a general purpose Sheet control.

**The POC saves Canvas blocks but adds no tables.** Dropping a module calls a block action
that creates a real Canvas block at the drop position and reloads the frame, so the result
behaves like any other block on the page and survives a refresh. The Canvas keeps its
module's type and settings in block attributes, which is a POC shortcut. Module types are a
hard coded list on the server, so their Lava can be resolved there. The only migration seeds
the builder's layouts, pages, and blocks. The entities under `MVP Design` below are recorded
for the follow-on work and are not built here.

## Motivation

Two pieces of this feature carry most of its technical risk, and both are cheaper to
answer now than after the data model is locked in.

The first is the iframe. The page being composed has to render as the real page, with
the site's own CSS, block markup, and scripts, which means it lives in an iframe. Native
HTML5 drag events do not cross into a nested browsing context, so the sidebar cannot
simply drop onto a target inside the frame.

The second is the Sheet. Stakeholders asked for it to be built as a reusable Obsidian
control rather than throwaway POC code, because the same pattern (a resizable,
repositionable panel that floats over content, like a toolbar window in a classic
Windows application) is wanted elsewhere. That makes its API surface a long lived
decision rather than a POC detail.

The budget is 40 goal hours, 50 approved.

## Requirements

### Builder shell

- The builder MUST render the target page inside an iframe and present a sidebar listing
  available module types.
- Zones configured for the page builder MUST be visually identified with an outline and
  a labeled chip. An empty enabled zone MUST show them along with its empty state prompting
  a drag, and every enabled zone MUST show them while a drag is in progress. An enabled zone
  with content shows no builder chrome at rest.
- Zones that are not configured for the page builder MUST render as ordinary page content
  with no builder decoration.
- A builder-enabled zone MUST identify itself in the rendered DOM so the parent frame can
  resolve drop targets without asking the server.

### Drag and drop

- Dragging a module type tile MUST show a drag mirror carrying that type's icon and name.
- Drag and drop MUST work with a mouse, touch, or pen, on desktop and phone layouts.
- Dropping onto an enabled zone MUST, in one gesture: create a Canvas block in that zone at
  the drop position, render the module type's Lava in it, select it, and open the Sheet.
- Each drop MUST produce its own Canvas block. The POC does not put two modules in one
  Canvas.
- The Canvas block MUST be saved, so it survives a refresh and behaves like any other block
  on the page, including Rock's block configuration bar.
- A drop MUST NOT be placed above a site or layout block in the same zone, because Rock
  always renders those ahead of page blocks.
- Drag and drop is the only specified way to place a module. No click to add affordance
  exists in the design.

### Selection

- A selected module MUST show an outline, a name chip, and three controls on its right
  edge: a drag handle, edit, and delete.
- Selection chrome MUST be visually distinct from zone chrome so a user can tell a
  droppable area from a selected object.

### Sheet control

- The Sheet MUST be resizable and repositionable by the user.
- The Sheet MUST remember its last position and size in memory for the session only. A
  page refresh MUST return it to its default position.
- The Sheet MUST be written as a general purpose Obsidian control with no Page Builder
  specific logic, so other blocks can adopt it.
- Within the builder, the Sheet MUST present three sections (Module Settings, Module
  Items, Display Settings) and a footer with Save and Save and Close.
- Module Settings MUST render the fields declared by the module type. Save writes them to
  the Canvas block's Module Settings attribute and re-renders the block in the frame.

### Block behavior

- The Page Builder block MUST call `onConfigurationValuesChanged(useReloadBlock())` so it
  reloads when its settings change.
- Adding a module MUST require Administrate permission on the target page, the same
  permission Rock requires to configure a page's blocks.
- The Canvas block MUST render its module's Lava on the server, so the output is in the page
  HTML rather than built in the browser.

## Design

### Pages and layouts

The migration seeds two internal pages. The Page Builder page sits under CMS Configuration
at `admin/cms/page-builder` and hosts the Page Builder block on a new **Full Screen** layout.
Full Screen is the Blank layout without its container padding, with every wrapper down to the
Obsidian mount stretched to the window and a viewport meta tag for phones, so whatever block
it hosts fills the browser. Blank itself is unchanged because other pages rely on it.

The Page Builder Sample page sits under the Page Builder page, hidden from navigation, on a
new **Page Builder** layout with one builder-enabled zone (Builder) and one ordinary zone
(Aside). The internal site runs the RockNextGen theme, so both layouts are authored in
`Rock.Frontend.Styles/src/themes/RockNextGen/Layouts/` and copied into `RockWeb/Themes` by
the build. The Page Builder block's Target Page setting defaults to the sample page.

### Module types

Module types are a hard coded list on the server for the POC. Each supplies a key, a display
name, an icon, a web Lava template, and the fields the Sheet renders under Module Settings.
The list lives on the server rather than in the builder's TypeScript because the Canvas block
resolves the Lava there, and the Page Builder block passes the list to its sidebar. There are
no `ModuleType` rows and no way to create or edit a type.

### Canvas block

The Canvas is a real Obsidian block type (`Rock.Blocks.Cms.Canvas`) registered with
`AddOrUpdateEntityBlockType()`. Like the Redirect block it has no Vue component: it returns
its output from `GetInitialHtmlContent()`, so the module renders on the server and appears in
the page HTML.

Each Canvas holds exactly one module in two block attributes:

| Attribute | Holds |
|---|---|
| Module Type | The key of the module type from the server-side list |
| Module Settings | The module's settings as JSON |

The Canvas resolves its module type's Lava with the common merge fields plus the module's
settings. The template stays with the type rather than being copied into the block, so
changing a type's template changes every Canvas that uses it, the way a `ModuleType` row
eventually will.

Keeping module data in block attributes is a POC shortcut and is replaced by the tables under
`MVP Design`. It works here only because each Canvas holds one module. The reasoning against
an attribute for an ordered list of modules still stands for the MVP.

### Adding a module

The Page Builder block gains an `AddModule` block action taking the zone name, the module type
key, and the id of the block the new one goes in front of (or none for the end of the zone).
It:

1. Checks that the current person has Administrate permission on the target page.
2. Creates a Canvas `Block` on the target page in that zone.
3. Renumbers `Order` across the zone's page blocks with the new block in position.
4. Sets the Module Type attribute and default Module Settings.
5. Returns the new block's id.

Block order is `Block.Order`, scoped to a zone. Rock renders a zone's site blocks first, then
its layout blocks, then its page blocks, each sorted by `Order`, so a page block can only be
positioned among other page blocks. The frame reads each block wrapper's
`data-zone-location` and keeps the drop line below any site or layout blocks in the zone,
which keeps every drop position one the server can honor.

On success the builder reloads the frame so Rock renders the new block itself, then selects
it by its `bid_{id}` wrapper. A reload is simpler than injecting the returned markup and keeps
the frame identical to what a visitor sees.

### Drag and drop across the iframe

The Obsidian Email Builder already solves this and the POC should follow it rather than
invent a second approach.

```mermaid
sequenceDiagram
    participant Tile as Sidebar tile (parent doc)
    participant Parent as Parent page
    participant Frame as Page iframe
    Tile->>Parent: pointerdown, capture the pointer, show overlay and drag mirror
    Parent->>Parent: pointermove, translate clientX/Y by frame rect
    Parent->>Frame: pass translated coordinates as a request
    Frame->>Frame: elementFromPoint(x, y) resolves the zone and position
    Frame->>Frame: draw insertion line or highlight an empty zone
    Parent->>Frame: pointerup, pass the release point as the drop request
```

The parent never tries to deliver a pointer event into the frame. The drag uses Pointer
Events rather than native HTML5 drag and drop, for two reasons. The browser's native drag
image cannot be styled, so it cannot show the tilted copy of the tile the Email Builder
shows. And a touch drag keeps sending its events to the element where it started, so hit
testing has to run from the captured pointer rather than from events on an overlay.

On `pointerdown` the tile captures the pointer and a styled copy of it follows the cursor,
matching the Email Builder's drag mirror (`sidePanel.partial.obs:835`). Each move is
converted to frame relative coordinates by subtracting the frame's bounding rect and handed
to the frame, which resolves the target from its own DOM. A transparent overlay still covers
the frame for the duration so the framed page cannot react to the pointer. The drop carries
the release point and the frame resolves the target again from it, because a fast drag can
release somewhere the last move never reported. A cancelled pointer, such as a touch the
browser turns into a scroll, never drops.

On small screens the sidebar becomes a horizontal strip of tiles above the frame. Tiles use
`touch-action: pan-x` there and `pan-y` in the vertical sidebar, so a swipe along the list
scrolls it and a pull toward the page starts a drag.

The difference for the Page Builder is target resolution, not drag mechanics. The Email
Builder owns its iframe document through `srcdoc` and knows every element in it. The Page
Builder points the frame at a real Rock page, so enabled zones have to be discoverable
from the DOM. That is what the next section covers.

### Zone exposure

Rock already wraps every zone at render time in
`RockPage.AddZoneElements()` (`Rock/Web/UI/RockPage.cs:3638`):

```html
<div id="zone-main" class="zone-instance can-configure {CssClass}">
  <div class="zone-configuration config-bar">...</div>
  <div class="zone-content">...blocks...</div>
</div>
```

The id is `zone-` plus the lowercased zone key under `ClientIDMode.Static`, and
`.zone-content` is the element that actually holds blocks, which makes it the drop
container rather than the wrapper itself.

Builder-enabled zones mark themselves with a data attribute. `Rock.Web.UI.Controls.Zone`
gains an `EnablePageBuilder` property (default `false`), and `AddZoneElements` emits
`data-pagebuilder="true"` on the wrapper when it is set. A theme opts a zone in with the
syntax the design already specifies:

```xml
<Rock:Zone ID="Main" runat="server" EnablePageBuilder="true" />
```

The frame then resolves a drop target by taking the element under the pointer, finding its
closest `[data-pagebuilder="true"]` ancestor, and using that zone's `.zone-content`, with no
server round trip in the drag path and the DOM as the single source of truth. The drop
position is the first content block whose vertical midpoint is below the pointer. The change
is additive and defaults to off, so no existing theme changes behavior.

`pagebuilderaccepts` is not implemented here, but it extends the same way through a second
attribute when blocks mode arrives.

### Zone chrome

The zone outline, the chip, the empty state, and the insertion line follow the Figma
dropzone mockup (node `695:40288`) and are styled with Rock's CSS variables
(`--color-primary`, `--color-primary-soft`, the `--color-interface-*` scale,
`--font-size-*`, `--spacing-*`, `--rounded-*`). The builder adds them to the framed page's
DOM when it loads, along with a stylesheet. The framed page can belong to any site and theme,
so any of those variables it does not define are copied from the builder's own document.
Variables the page already defines are left alone, so the builder never restyles the site's
own content.

### Suppressing stock zone chrome

The Page Builder only runs for administrators, so `canConfigPage` is true inside the iframe
and Rock's own zone configuration bars render there. Those collide with the builder's zone
outline and Builder chip visually, and their click targets compete with drop targets. The
POC has to either suppress the stock zone chrome while in builder mode or render the frame
with `canConfigPage` false while keeping builder decoration on. See the open question below.

### Sheet control placement

New control, proposed at `Rock.JavaScript.Obsidian/Framework/Controls/Internal/sheet.obs`.
Internal initially because the API surface is unproven, on the same reasoning as
`[RockInternal]` on the C# side. It graduates out of `Internal/` once a second consumer
confirms the shape.

### Overlay extraction

`Controls/Internal/EmailEditor/overlay.obs` is already free of email specific logic and has
exactly one importer. Promote it to `Controls/Internal/overlay.obs` and update that import,
so the Page Builder is not a second copy. This is the only extraction worth taking from the
Email Builder; the coordinate translation is ten lines and belongs to each consumer.

## MVP Design

The POC builds the Canvas block type but none of the entities below. It keeps each Canvas's
module in block attributes instead, and the follow-on work needs these decisions made rather
than rediscovered when that data moves into tables.

### Entities

Four new entities, plus the existing `PersonalizedEntity`.

```mermaid
erDiagram
    ModuleType ||--o{ ModuleInstance : "typed by"
    ModuleType ||--o{ ModuleInstanceItem : "scopes attributes for"
    ModuleInstance ||--o{ ModuleInstanceItem : "contains"
    ModuleInstanceItem ||--o{ ModuleInstanceItemCampus : "filtered by"
```

| Entity | Notable columns |
|---|---|
| `ModuleType` | `Name`, `AreItemsSupported`, `ItemTerm`, `IconCssClass`, `CategoryId`, `IsPersonalizationEnabled`, `WebLavaTemplate`, `MobileLavaTemplate`, `AdditionalSettingsJson` |
| `ModuleInstance` | `Name`, `ModuleTypeId`, `IsShareable` |
| `ModuleInstanceItem` | `Name`, `ModuleTypeId`, `IsShareable` |
| `ModuleInstanceItemCampus` | `ModuleItemId`, `CampusId` |

`ModuleInstanceItem.ModuleTypeId` is denormalized deliberately. It exists so attribute
qualifiers can scope item attributes by module type without walking up to the parent
instance.

`AreItemsSupported` and `ItemTerm` together drive the Module Items section: the section
appears only for types that support items, and `ItemTerm` supplies its user facing label
(Slides, Links, Cards, and so on).

### Canvas block and module instances

The Canvas block type from the POC carries forward, registered through
`AddOrUpdateEntityBlockType()`, never `UpdateBlockTypeByGuid()`. The latter issues a
`DELETE FROM [BlockType] WHERE [Path] = ...` and entity based block types have an empty
path, so the wrong helper can wipe every entity based block type in the database.

In the MVP a Canvas holds more than one module and its module data moves out of block
attributes. How a Canvas references its module instances is unresolved. A placement join entity
(`BlockId`, `ModuleInstanceId`, `Order`) models `IsShareable` correctly and puts order on
the placement where it belongs. A nullable `BlockId` and `Order` directly on
`ModuleInstance` mirrors `ForgeContent` one for one but means an instance can only live in
one Canvas, which makes `IsShareable` meaningless for placement. Storing an ordered JSON
array in a block attribute value was rejected: attribute values are for configuration, and
Rock's two precedents for block owned content (`HtmlContent`, `ForgeContent`) both chose a
table.

### Modules and elements

Modules and elements differ in who authors the type. A module type is data: an admin writes
Lava, it is a row, and the set grows at runtime. An element type is code: Rock ships Title,
Paragraph, Image, Button, Video and Divider, and the set changes with a release. That is
why elements should not be modelled as `ModuleType` rows, which would put a closed set of
code primitives into an open data registry where someone can edit or delete the Divider.

Modules and elements do not intermix within a single Canvas. Whether a Canvas hosts
elements at all, or elements get their own block type, is open.

## Open Questions

1. **Does the Canvas host elements as well as modules, or do elements get their own block
   type?** Open with the technical lead. Modules and elements do not intermix within one
   Canvas either way, so this is about whether one block type serves both in separate
   instances or two block types exist. It does not affect the POC.
2. **How does a Canvas reference its module instances once they move into tables?** Options
   and reasoning are under `MVP Design` above. It does not affect the POC, which keeps one
   module per Canvas in block attributes, but it should be settled before the MVP schema is
   written.
3. **Is the stock zone configuration chrome acceptable in builder mode?** Rock's own zone
   bars render inside the frame whenever the viewer can administrate the page, so they sit
   alongside the builder's zone outline and Builder chip. During a drag the parent overlay
   covers the frame, so this is cosmetic collision plus live click targets when idle, not a
   mechanical problem. The POC accepts it. If it is worth removing later, calling
   `AddZoneElements` with `canConfigPage` false already emits the wrapper and `.zone-content`
   without the bars.

## Considered but Rejected

### Reuse the Email Builder control directly
Rejected. `emailIFrame.partial.obs` and `utils.partial.ts` are roughly 440 KB of email
domain logic (sections, columns, inlined styles, email client quirks). The drag pattern is
worth copying; the control is not.

### Use Rock's existing drag and drop directive
Rejected. `Rock.JavaScript.Obsidian/Framework/Directives/dragDrop.ts` wraps dragula and is
the house pattern everywhere else (Grid, sortableTree, GroupedPanel, listItems). Dragula is
pointer based and scoped to containers within a single document, so it cannot reach into an
iframe. This is why the Email Builder hand rolled its own, and the same reasoning applies
here.

### Add a click to add affordance as an early milestone
Rejected. The empty zone's plus glyph is empty state decoration, not a button, and the
accompanying copy instructs the user to drag. Adding a click path would be a change to the
interaction model, not a shortcut through it.

### Resolve enabled zones from a server-supplied list instead of the DOM
Rejected. The block could return the builder-enabled zone keys and the parent could match
`#zone-{key}` with no change to `RockPage` or `Zone` at all. It is the lowest risk option,
but it puts enabled-ness in the parent's memory rather than the rendered page, so the list
and the DOM can disagree when a layout renders zones conditionally, and it adds a server
round trip ahead of hit testing.

### Carry the flag in the zone's CssClass
Rejected. `Zone` already has a `CssClass` property that lands on the wrapper, so a theme
could write `CssClass="pagebuilder-enabled"` and the parent could query
`.zone-instance.pagebuilder-enabled` with no C# change whatsoever. That makes behavior
configuration masquerade as styling, and it is a convention rather than a real attribute,
so it would have to be replaced before the feature ships.

### Keep the POC entirely client side
Rejected. The first version injected Canvas markup into the frame and saved nothing. That
proved the drag across the iframe, but not how a placed module behaves as a block: its order
among other blocks, Rock's block configuration bar, server rendered Lava, and surviving a
refresh. Those are the parts the follow-on work most needs to see.

### Add the module tables in the POC
Rejected for the POC. Adding `ModuleType` and `ModuleInstance` now would force open question 2
before anything exercises it, and put tables in a migration that are likely to change. With
one module per Canvas, block attributes hold everything the POC needs.

### Emit the accepts value instead of a boolean
Rejected for the POC. `data-pagebuilder-accepts="modules"`, with absence meaning disabled,
would fold `pagebuilderaccepts` in now rather than later. It requires settling the
semantics of blocks and both modes before there is anything to exercise them, and the
boolean extends to it cleanly when that time comes.

## Out of Scope

- **New entities and tables.** Module data lives in Canvas block attributes. See `MVP Design`.
- **More than one module per Canvas.**
- Moving or deleting a placed module from the builder. Rock's own block configuration bar
  still works on a saved Canvas block.
- Standard / Block mode and Elements mode.
- The admin footer toolbar replacement and the drawer that replaces it on every page.
- Module Presets. Pulled in only if the budget allows once the two named deliverables land.
- Content Channel as an item source for Module Items.
- Creating or editing Module Types. Module types are a hard coded server-side list in the POC.
- Enforcing `pagebuilderaccepts="(modules|blocks|both)"`. `EnablePageBuilder` is
  implemented because hit testing depends on it; the accepts value is not.
- The caching and personalization strategy flagged in the design notes.

## Related

- [Asana: Page Builder POC (DEV-15820)](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1218707041477970) (40 goal hours, 50 approved, targeted at v21)
- [Figma: Spark Essentials, Page Builder canvas](https://www.figma.com/design/JJiznbtHqJc1yj96Z5Kfuh/Spark-Essentials?node-id=168-5667) (reviewed 2026-09-21, treated as canonical)
- [Figma: data model](https://www.figma.com/design/JJiznbtHqJc1yj96Z5Kfuh/Spark-Essentials?node-id=610-12122), [UX/UI notes](https://www.figma.com/design/JJiznbtHqJc1yj96Z5Kfuh/Spark-Essentials?node-id=750-27577)
- Email Builder prior art: `Rock.JavaScript.Obsidian/Framework/Controls/Internal/EmailEditor/emailDesigner.partial.obs`, `emailIFrame.partial.obs`

### Design cross-check

Three conflicts were found between the Figma Notes panel and the mockups. The mockups are
canonical in each case.

| Notes panel says | Mockups show | Resolution |
|---|---|---|
| A selected module has four controls (Drag Icon, Settings, Configurations, Delete) | Three controls: drag handle, edit, delete | Three. The Notes list was never drawn. |
| "Customize Module: Click and open a modal" | A floating panel, and item 4 of the same notes calls it a right pane | Sheet, per stakeholder direction |
| "Page Builder block will be created upon first drag/drop" | n/a | Canvas block, per Kyle Henning |

![Module data model](artifacts/260921-page-builder-poc/data-model.png)

![Empty zone placeholder](artifacts/260921-page-builder-poc/empty-zone-placeholder.png)

![UX/UI notes panel](artifacts/260921-page-builder-poc/ux-ui-notes.png)
