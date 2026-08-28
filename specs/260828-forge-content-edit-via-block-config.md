---
author: Kyle Henning
date_created: 2026-08-28
summary: >-
  Remove the inline Edit button from the Forge Content block's view mode and
  move editing behind a pencil custom action shown in Block Configuration
  mode, following the IHasCustomActions pattern used by other Obsidian blocks.
contributors: []
---

# Forge Content Edit via Block Configuration

## Summary

The Forge Content block currently renders an "Edit" / "Add Component" button in the top-right of its view mode for anyone with EDIT authorization. That button sits inside the page content itself, which clutters the rendered output and does not match how other Obsidian blocks expose administrative editing. This spec moves editing behind a pencil icon that appears in the block's configuration bar when the page is in Block Configuration mode, using the existing `IHasCustomActions` pattern.

## Motivation

The inline button is visible whenever an authorized person views the page normally, even though editing is an administrative task. Rock's convention for admin-only block editing (Dynamic Data, Content Collection View, Category Detail, and others) is a custom action icon in Block Configuration mode. Aligning Forge Content with that convention removes visual noise from the rendered page and puts the edit affordance where administrators already look for it.

## Requirements

- The view mode of the block MUST NOT render any edit button or edit affordance. It renders only the compiled component (or nothing when unconfigured).
- The block MUST expose a custom action (pencil icon) via `IHasCustomActions.GetCustomActions()` that is visible only in Block Configuration mode.
- The custom action MUST be gated to people with EDIT authorization on the block (the `canEdit` parameter).
- Clicking the pencil MUST open the existing source editor (the `EditPanel` experience) in a modal, seeded with the current source, or the starter source when the block has no content yet.
- Save MUST continue to compile on the server through the existing `SaveContent` block action, surface compile errors inline, and store nothing on failure.
- After a successful save the block MUST reload so the visitor-facing view immediately shows the new compiled output (`useReloadBlock`).
- Cancel/close MUST discard unsaved edits without any change to stored content.
- The clean source MUST no longer be delivered in the view-mode initialization box; it is fetched on demand by the editor modal via a block action that re-verifies EDIT authorization.
- The "This block has no component yet" notification SHOULD remain for authorized viewers, with its text updated to point at the pencil in Block Configuration mode instead of the removed button.

## Design

Follow the established custom-actions pattern (reference: `Rock.Blocks/Reporting/DynamicData.cs:1430` and `Rock.JavaScript.Obsidian.Blocks/src/Core/categoryDetailCustomSettings.obs`).

### C# (`Rock.Blocks/Cms/ForgeContentDetail.cs`)

- Implement `IHasCustomActions`. `GetCustomActions( canEdit, canAdministrate )` returns one `BlockCustomActionBag` when `canEdit` is true:
  - `IconCssClass`: `ti ti-pencil`
  - `Tooltip`: `Edit Content`
  - `ComponentFileUrl`: `/Obsidian/Blocks/Cms/forgeContentDetailEditContent.obs`
- Add a `[BlockAction] GetEditContent()` that returns the stored source (and the starter/default source when none exists) after checking `BlockCache.IsAuthorized( Authorization.EDIT, ... )`.
- `GetObsidianBlockInitialization()` stops sending `Source`; it still sends `CompiledContent` and `IsEditable` (the latter only drives the "no component yet" hint).
- `SaveContent` is unchanged.

### Obsidian

- `Rock.JavaScript.Obsidian.Blocks/src/Cms/forgeContentDetail.obs`: delete the edit button, edit-mode `Panel`, `onEdit`/`onSave`/`onCancel` handlers, and edit-related state. The component becomes view-only: render `ViewPanel` when compiled content exists, otherwise the info notification for authorized viewers.
- New `Rock.JavaScript.Obsidian.Blocks/src/Cms/forgeContentDetailEditContent.obs`: a `Modal`-based component (modeled on `categoryDetailCustomSettings.obs`) that:
  - Opens immediately, loads the source via `GetEditContent`, and hosts the existing `EditPanel` partial.
  - Saves through the existing `SaveContent` action, shows compile errors inline via the `EditPanel`'s `compileError` prop, and stays open on failure.
  - On success closes and calls `useReloadBlock()`.
  - Emits `close` when dismissed, per the custom-settings component contract.
- The starter source template moves out of `forgeContentDetail.obs` (server supplies it via `GetEditContent`, keeping the default in one place).

### ViewModels (`Rock.ViewModels/Blocks/Cms/ForgeContentDetail/`)

- `ForgeContentDetailInitializationBox`: remove `Source` (block has not shipped in a release, so this is not a compatibility break; confirm before removing if that assumption is wrong).
- New response bag for `GetEditContent` carrying the source.

## Considered but Rejected

### Keep the inline button but hide it outside Block Configuration mode
Rejected. The Vue component has no clean signal for configuration mode; the supported mechanism for config-mode affordances is `IHasCustomActions`, and inventing a parallel one violates the prime directive of following established patterns.

### Use the standard custom settings modal (SaveCustomSettings) instead of the existing SaveContent action
Rejected. The source is not a block setting; it lives in the `ForgeContent` entity and must pass through the server-side compile pipeline. Reusing `SaveContent` keeps one compile path.

## Verification Steps

1. As an administrator, view a page with a configured Forge Content block: no edit button renders; the compiled component displays.
2. Enter Block Configuration mode: a pencil icon appears on the block. Click it; the editor modal opens with the current source.
3. Save a valid change: the modal closes and the block re-renders with the new output without a full page reload.
4. Save an invalid change: the compile error displays in the modal, nothing is stored, and the modal stays open.
5. On an unconfigured block, the pencil opens the modal seeded with the starter source, and the view shows the "no component yet" hint to authorized viewers.
6. As a person without EDIT authorization, confirm the pencil does not appear and `GetEditContent` returns forbidden when invoked directly.
7. Confirm the view-mode initialization payload no longer contains the raw source.

## Out of Scope

- Any change to the compile pipeline, `ForgeContentCompiler`, or the agent skill's `AddOrUpdateForgeContent` tool.
- Block settings (the standard gear/settings dialog) for Forge Content.
- Versioning or audit history of authored source.

## Related

- `Rock.Blocks/Cms/ForgeContentDetail.cs`
- `Rock.JavaScript.Obsidian.Blocks/src/Cms/forgeContentDetail.obs`
- Pattern references: `Rock.Blocks/Reporting/DynamicData.cs:1430`, `Rock.JavaScript.Obsidian.Blocks/src/Core/categoryDetailCustomSettings.obs`
