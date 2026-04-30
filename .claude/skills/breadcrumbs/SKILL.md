---
name: breadcrumbs
description: >-
  Add custom breadcrumbs to a Rock RMS Obsidian block by implementing
  IBreadCrumbBlock and a GetBreadCrumbs method. Use when the developer says
  "add breadcrumbs", "custom breadcrumbs", "add a breadcrumb to this block",
  "the breadcrumb should show the entity name", "make the breadcrumb dynamic",
  "implement IBreadCrumbBlock", or "the page title should show [entity] name".
  Also use when invoked by /block-design or /convert-block after the developer
  confirms they want custom breadcrumbs for a Detail block. Do NOT use for
  WebForms breadcrumbs or for non-Rock breadcrumb concepts. Do NOT use for
  general breadcrumb-navigation questions; this skill writes the C# code.
argument-hint: "Path to the block file (e.g., 'Rock.Blocks/AI/KnowledgeBaseDetail.cs') or block name. If invoked from another skill, the calling skill should pass the block path."
compatibility: Requires Claude Code with read/write access to the Rock RMS codebase.
metadata:
  version: "1.0"
  author: "Kyle Henning, Triumph Tech"
---

# Rock RMS Custom Breadcrumbs

You are adding custom breadcrumbs to a Rock RMS Obsidian block. Custom breadcrumbs replace the page's static breadcrumb text with a dynamic value pulled from the entity (e.g., showing the actual Step Program's `Name` instead of the page name "Step Program Detail").

**The user's request:** $ARGUMENTS

---

## When to use this skill

- Detail blocks that need to display the entity's `Name` (or similar) in the breadcrumb trail.
- Blocks where the page parameter identifies a specific record and the breadcrumb should reflect that record.
- Nested-entity Detail blocks (e.g., `StepTypeDetail` under `StepProgramDetail`) that need to preserve parent page parameters in the breadcrumb's PageReference.

## When NOT to use this skill

- List blocks: list pages typically use the static page title; do not add `IBreadCrumbBlock` unless there is a clear reason.
- Blocks that have no entity context (the breadcrumb has nothing dynamic to show).
- WebForms blocks: breadcrumbs there are handled differently and are not in scope.

---

## Canonical reference

Use the **MediaElementDetail / MediaFolderDetail** pattern as the primary canonical reference. The Step blocks (`StepProgramDetail.cs`, `StepTypeDetail.cs`) work for single-entity cases but their nested pattern is brittle — it only works because Step descendant URLs always carry the parent `ProgramId`. The Media blocks use a more robust mechanism that tolerates URLs that omit ancestor IDs.

- `Rock.Blocks/Cms/MediaFolderDetail.cs:GetBreadCrumbs` — nested-entity breadcrumb that projects the parent FK in the same `GetSelect` and pushes it up via `AdditionalParameters`. Use this for any block whose entity has a parent in the page hierarchy.
- `Rock.Blocks/Cms/MediaElementDetail.cs:GetBreadCrumbs` — same pattern at the leaf level (pushes its parent's FK up).
- `Rock.Blocks/Engagement/StepProgramDetail.cs:GetBreadCrumbs` — fine for a true root-level entity that has no parent in the breadcrumb chain.

**Why it matters:** Rock's framework iterates the page hierarchy from current → root and threads a single shared `trackedPageParameters` dictionary through each ancestor's `GetBreadCrumbs` call. A descendant's `BreadCrumbResult.AdditionalParameters` is merged into that dictionary, so the next ancestor up gets the parent IDs it needs even when those IDs were not in the original request URL. Without `AdditionalParameters`, an ancestor on a deep page sees a null key and falls back to "New X" — producing breadcrumbs like `Home > New Knowledge Base > [Folder] > [Document]`. This is the bug the skill is designed to prevent.

**Do not use the older `using ( var rockContext = new RockContext() )` pattern** seen in some legacy blocks (`StreakTypeDetail.cs`, etc.). Use the inherited `RockContext` property as the Media and Step blocks do.

---

## Process

### Step 1 — Identify the block and its entity

If the user supplied a block path, read that file. Otherwise, ask which block needs breadcrumbs.

From the block, identify:

- The **C# class name** (e.g., `KnowledgeBaseDetail`).
- The **base class** — confirm it is a Detail block (`RockEntityDetailBlockType<TEntity, TBag>` or similar). If it is not a Detail-shaped block, ask the developer what the breadcrumb should display before continuing.
- The **primary entity** type (e.g., `KnowledgeBase`).
- The **PageParameterKey** constant the block uses to identify the entity (e.g., `KnowledgeBaseId`).
- The **immediate parent FK property** on the entity (e.g., `KnowledgeBaseFolder.KnowledgeBaseId`, `KnowledgeBaseDocument.KnowledgeBaseFolderId`). This drives `AdditionalParameters` in Template B. Skip this only when the entity is a true root with no parent in the page hierarchy.
- The **page parameter name** that the parent's breadcrumb block reads (e.g., `KnowledgeBaseId`). This is the dictionary key you push via `AdditionalParameters`. It does not need to exist in the current block's `PageParameterKey` constants — the framework merges it into a shared dictionary that the parent block reads.

### Step 2 — Confirm the breadcrumb display value

The default display is `entity.Name` falling back to `"New {EntityFriendlyName}"`. Confirm this matches the developer's intent before writing code. Common variations:

- An entity that has a `Title` instead of `Name` — use `Title`.
- An entity where the breadcrumb should combine fields (e.g., "FirstName LastName") — call this out and confirm.
- A composite breadcrumb where the parent entity should appear too — handle via the `BreadCrumbResult.BreadCrumbs` list (one entry per crumb).

If the entity has neither `Name` nor an obvious display field, stop and ask the developer.

### Step 3 — Apply the changes

Make the following edits to the block's C# file:

**1. Add `using Rock.Web;` to the using block** if it is not already present. This is the namespace for `BreadCrumbResult`, `BreadCrumbLink`, `IBreadCrumb`, and `PageReference`.

**2. Add `IBreadCrumbBlock` to the class declaration:**

```csharp
public class KnowledgeBaseDetail : RockEntityDetailBlockType<KnowledgeBase, KnowledgeBaseBag>, IBreadCrumbBlock
```

**3. Add the `GetBreadCrumbs` method** inside the `#region Methods` (or before `GetObsidianBlockInitialization` if no region exists). Use the appropriate template below.

#### Template A — Single-entity breadcrumb (mirrors StepProgramDetail)

```csharp
/// <inheritdoc/>
public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
{
    var key = pageReference.GetPageParameter( PageParameterKey.KnowledgeBaseId );
    var pageParameters = new Dictionary<string, string>();

    var name = new KnowledgeBaseService( RockContext )
        .GetSelect( key, kb => kb.Name );

    if ( name != null )
    {
        pageParameters.Add( PageParameterKey.KnowledgeBaseId, key );
    }

    var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
    var breadCrumb = new BreadCrumbLink( name ?? "New Knowledge Base", breadCrumbPageRef );

    return new BreadCrumbResult
    {
        BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
    };
}
```

#### Template B — Nested entity that pushes parent FK up via AdditionalParameters (mirrors MediaFolderDetail)

Use this template **whenever the block's entity has any parent in the page hierarchy**, even if the parent's page parameter is currently in the URL. `AdditionalParameters` is what lets ancestor breadcrumb blocks resolve their own entity even when their page parameter is missing from the descendant's URL.

```csharp
/// <inheritdoc/>
public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
{
    var key = pageReference.GetPageParameter( PageParameterKey.KnowledgeBaseFolderId );
    var pageParameters = new Dictionary<string, string>();
    var additionalParameters = new Dictionary<string, string>();

    var data = new KnowledgeBaseFolderService( RockContext )
        .GetSelect( key, kbf => new
        {
            kbf.Name,
            kbf.KnowledgeBaseId
        } );

    if ( data != null )
    {
        pageParameters.Add( PageParameterKey.KnowledgeBaseFolderId, key );
        additionalParameters.Add( PageParameterKey.KnowledgeBaseId, data.KnowledgeBaseId.ToString() );
    }

    var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
    var breadCrumb = new BreadCrumbLink( data?.Name ?? "New Knowledge Base Folder", breadCrumbPageRef );

    return new BreadCrumbResult
    {
        BreadCrumbs = new List<IBreadCrumb> { breadCrumb },
        AdditionalParameters = additionalParameters
    };
}
```

Notes on this template:

- The `GetSelect` projection pulls **both** the display field (`Name`) and the parent FK column (`KnowledgeBaseId`) in a single query.
- `pageParameters` (used for the breadcrumb's link URL) holds only this entity's own ID. Do NOT include the parent ID here — the link URL stays minimal and the breadcrumb chain reconstructs context dynamically each time.
- `additionalParameters` holds the parent FK. The framework merges this into the shared `trackedPageParameters` dictionary that gets passed to ancestor `GetBreadCrumbs` calls.
- For multi-level hierarchies, each level only pushes its **immediate** parent's ID. The chain handles deeper ancestors: leaf pushes mid's ID → mid runs, looks up its own entity, pushes root's ID → root runs and finds its ID waiting for it.

### Step 4 — Verify

After writing the code:

- Confirm `using Rock.Web;` is in the file.
- Confirm `IBreadCrumbBlock` was added to the class declaration (do not duplicate if already present).
- Confirm the `PageParameterKey` constant referenced in `GetBreadCrumbs` exists; if not, the constant must be added to the `PageParameterKey` static class.
- Confirm the entity service (`KnowledgeBaseService`, etc.) exists in the codebase. If it does not, fall back to `EntityService<TEntity>` or whatever pattern the surrounding block uses to load the entity.

Build is not required, but if the block is in a partially-implemented state ask the developer whether they want a build run before declaring done.

### Step 5 — Report

Summarize:

- The block file modified.
- Which template was used (A or B).
- Any parent params preserved.
- The fallback display string (e.g., "New Knowledge Base").
- The page-side breadcrumb reminder from Step 6 below (always include).

---

### Step 6 — Page-side breadcrumb reminder (always, plus action when in scope)

A custom block-supplied breadcrumb is paired with a page setting that controls whether the page's own static name shows up in the trail. The relevant page column is `BreadCrumbDisplayName` (default `true`). When the dynamic crumb is in play, the page should set `BreadCrumbDisplayName = 0` so the trail does not show both "Knowledge Base Detail" *and* the dynamic entity name side-by-side.

**Always do this:** in the report from Step 5, include a callout like:

> **Page-side reminder:** the page hosting this block needs `BreadCrumbDisplayName` set to `0` (Display in Breadcrumb = off) so the dynamic breadcrumb is the only one. This is typically done in the migration that registers the page; verify it before this ships. If the page already exists without the setting, an `UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '...'` migration covers it.

**Take action when in scope:** if the developer is in the same session adding the block to a page (writing or recently wrote a migration that calls `AddPage`/registers the page Guid this block lives on, or has an open EF/plugin migration referencing the page), do **not** stop at a callout. Make sure the page setting gets handled:

- `RockMigrationHelper.AddPage(...)` does not take a `breadCrumbDisplayName` parameter — it always inserts `BreadCrumbDisplayName = 1`. To turn it off, add a follow-up `Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '<page-guid>'" )` call right after the `AddPage` call in the migration's `Up()`.
- In the migration's `Down()`, if you intend to revert this, the page row is typically deleted via `DeletePage` so no inverse `UPDATE` is required. If the page is being preserved, restore `BreadCrumbDisplayName = 1` for symmetry.
- If the developer is using `/migration` or `/plugin-migration` in the same flow, add this work to their migration content rather than spawning a follow-up task.

**Do not modify migrations that are not in scope.** If the page registration migration was authored long ago and is already shipped, the callout is the right outcome — surface it and move on. Editing a shipped migration retroactively is wrong; if the live environment needs the fix, that becomes a new plugin-migration / hotfix and should be flagged via `mcp__ccd_session__spawn_task` rather than silently applied.

---

## Common pitfalls

- **Using `new RockContext()` instead of inherited `RockContext`.** Some legacy blocks (e.g., `StreakTypeDetail.cs`) wrap the breadcrumb logic in `using ( var rockContext = new RockContext() )`. This is the older pattern. Use the inherited `RockContext` property as Step blocks do.
- **Forgetting `AdditionalParameters` in nested blocks.** Without it, an ancestor breadcrumb block on a deep page sees a `null` page parameter for its own entity ID and renders the "New X" fallback — producing trails like `Home > New Knowledge Base > [Folder] > [Document]`. Always use Template B (the Media pattern) for any block whose entity has a parent in the page hierarchy. Do not assume the descendant URL contains all ancestor IDs; rely on `AdditionalParameters` to push them up.
- **Reading the parent ID from `pageReference` instead of from the entity.** Older Step-style code reads the parent param from the URL with `pageReference.GetPageParameter(...)` and copies it into `pageParameters`. This is brittle — the parent ID may not be in the URL on deeper pages. Project the parent FK from the entity itself in the `GetSelect` lambda and push it via `AdditionalParameters`.
- **Adding the parent ID to `pageParameters`.** Don't. `pageParameters` is the link URL for *this* breadcrumb, and it should hold only this entity's own ID. Putting the parent ID there bloats the URL but does not help ancestor resolution — only `AdditionalParameters` does that.
- **Adding `IBreadCrumbBlock` to a List block by reflex.** List blocks generally do not need it. Confirm before adding.
- **Hard-coding the fallback display.** "New Knowledge Base" should match the entity's friendly type name. If the entity has `[FriendlyTypeName("...")]`, prefer that exact phrasing.
- **Forgetting to add the `Dictionary<string, string>` using.** `using System.Collections.Generic;` is almost always already present in block files; verify.
- **Skipping the `BreadCrumbDisplayName = 0` page setting.** Adding `IBreadCrumbBlock` without disabling the page's static breadcrumb name produces a doubled trail (page name + dynamic entity name). Always emit the Step 6 callout, and act on it when a migration that registers the page is in the current session's scope.
