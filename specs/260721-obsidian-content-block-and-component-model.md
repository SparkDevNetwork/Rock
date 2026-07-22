---
author: Kyle Henning
date_created: 2026-07-21
summary: >-
  A prototype for author-defined Obsidian UI in Rock. An admin drops a single
  Obsidian Content block on a page and, in place, writes clean Vue. The source
  is compiled to a loadable module in the admin's own browser at save time and
  stored, alongside the original source, on a brand-new ObsidianContent entity
  owned by that block placement. Visitors are served only the precompiled
  output, which renders using Rock's existing Obsidian controls and API, with no
  repository files and no full Rock build. The model is designed so a reusable
  component library can be layered on later without schema rework.
contributors: []
status: draft
---

# Obsidian Content Block and Model

## Summary

Today the only way to build UI with Rock's Obsidian controls is to author a `.obs` file in the repository, compile it with Rock's build, and deploy. This prototype adds a runtime path: an admin drops a single **Obsidian Content** block on a page, clicks edit, and writes normal Vue (a `<template>` plus a script) directly in place. On save the source is compiled to a browser-loadable module **in the admin's own browser**, and both the compiled output and the original source are stored on a new `ObsidianContent` entity owned by that block placement. In view mode, and for every visitor, the block renders the precompiled output through Rock's already-present Obsidian framework: the SystemJS import map, the compiled control library served under `/Obsidian/`, and the existing `dynamicComponent` control.

The design deliberately puts the one costly, security-sensitive step (compilation, which needs eval-style execution) on the admin's in-place edit action only. Every visitor-facing render is served finished JavaScript with no compiler and no eval, exactly like any other Obsidian block.

This replaces an earlier direction that extended the WebForms HTML Content block. That approach was dropped because it entangled a heavily-used core block, carried backward-compatibility risk, and required an awkward compile handshake inside a WebForms postback. A brand-new block on a brand-new model avoids all three.

## Decisions locked for this prototype

- **Per-instance content.** Content is owned per block placement (like `HtmlContent`), not a reusable library. Confirmed with the requester.
- **Reusable library is a future direction, not this run.** The requester wants to explore it later. The model is shaped to allow it without a migration (see the nullable `BlockId` below).
- **Scope: through Phase 1.** A working end-to-end feature at template-plus-script fidelity.
- **Compiled content delivery: blob URL** (not a served endpoint) for the prototype. Superseded during implementation by direct in-browser module instantiation; see "As-built deviations (Phase 1)".
- **Full single-file-component and TypeScript fidelity: deferred** to a later phase.

## Naming

- **Entity / model:** `ObsidianContent` (table `[ObsidianContent]`).
- **Block:** `ObsidianContentDetail` (mirrors `HtmlContentDetail`: one block that renders in view mode and edits in place). Named distinctly so it does not collide with the entity.

## Goals

- Let a trusted admin author a Vue component in place on a page, with no repository file and no full Rock build.
- Use Rock's real Obsidian controls (for example `RockButton`, `RockLabel`) and call the Rock API as the current user from within the authored component.
- Keep the compiler and its eval requirement confined to the admin's edit action. Visitor-facing renders receive only precompiled JavaScript.
- Store the original source so the content can be re-edited cleanly and recompiled on a future Rock upgrade.
- Shape the model so a reusable component library can be added later without schema rework.

## Non-goals (for the prototype)

- A reusable component library, a management List, a picker, or a separate display block. These belong to the future reusable direction.
- Full single-file-component fidelity (`<script setup>`, TypeScript, `<style scoped>`). The prototype targets a `<template>` plus a plain script object.
- Import/export, sharing, or a marketplace.
- Fine-grained delegated authoring. Editing is gated to administrators only.
- Server-side compilation. Investigated and rejected: Rock previously ran an in-process V8 engine (ReactJS.NET via JavaScriptEngineSwitcher) and deliberately removed it, and requiring Node on every host conflicts with Rock's deployment model.

## Background and key constraints

Findings from the codebase that shape the design:

- Rock ships the **runtime-only** build of Vue. There is no template compiler on a normal page, so a raw `<template>` cannot be turned into runnable code in the browser unless a compiler is explicitly loaded.
- Rock serves every compiled control as a module under `RockWeb/Obsidian/` and resolves `@Obsidian/...` names through a **SystemJS import map** set up in `obsidian-core.js` (see `RockPage.cs`, which emits `System.import(...)` and gates framework loading on `_pageNeedsObsidian`).
- The framework only loads when the viewer can administer the page or when the page has a real Obsidian block. Because this block is itself an Obsidian block, the framework loads automatically, so it works for non-admin visitors.
- `Rock.JavaScript.Obsidian.Framework/Controls/dynamicComponent.obs` already loads a component from a URL via `await import(url)`; because that control is compiled to SystemJS, the dynamic import runs through SystemJS and resolves `@Obsidian/...` imports through the import map. This is the render primitive we reuse.
- `ObsidianDynamicComponentWrapper` exists to host such components inside WebForms, and is evidence the runtime-load pattern is already sanctioned.

Decisive technical choice: **compile to SystemJS format and load through Rock's loader**, not native ESM. Native `import()` would not resolve the bare `@Obsidian/...` specifiers, because Rock uses a SystemJS import map rather than a native browser import map.

## Architecture overview

Two new pieces plus reuse of existing framework machinery:

1. **`ObsidianContent` entity** (new model): stores a record's source, compiled output, the Vue version it was compiled against, and a nullable `BlockId` identifying the block placement that owns it.
2. **`ObsidianContentDetail` block** (new, Obsidian): a single block that renders its owned record in view mode and, for an authorized admin, edits it in place. The edit action loads the compiler and compiles on save.
3. **Compiler assets** (new static files): the Vue template compiler plus a module-format transform, served under `/Obsidian/Libs/` and loaded only when the block enters edit mode.

The block resolves its `ObsidianContent` record by its own `BlockId` (the `HtmlContent` pattern). The first save creates the record; later saves update it.

### Data flow

Editing in place (admin only):

```
Admin views the block on a page and clicks Edit
  -> edit mode loads compiler + module transform (only now, only for the admin)
  -> admin writes Vue source in a code editor
  -> on Save: browser compiles source -> render function -> SystemJS module string
  -> if compile fails: show error inline, block save
  -> SaveContent block action upserts the ObsidianContent row for this BlockId
     with { Source, CompiledContent, CompiledVueVersion }
```

Rendering to a visitor (and admin view mode):

```
Visitor loads the page
  -> block's config box includes the stored CompiledContent for this BlockId
  -> block instantiates the compiled SystemJS module directly (see the deviation note):
       captures its System.register call through a local System shim,
       resolves the @Obsidian/... and vue dependencies through the loader,
       then drives the setters and execute step to obtain the component
  -> @Obsidian/... imports resolve via the existing import map
  -> component mounts; it can call the API as the current user
```

## Data model: `ObsidianContent`

New entity under the CMS domain. `Model<T>` supplies `Id`, `Guid`, audit columns, and the `Foreign*` columns; do not redeclare those.

| Column | Type | Notes |
|---|---|---|
| `BlockId` | int, nullable, FK to `[Block]` | The block placement that owns this record (per-instance). Null is reserved for a future reusable-library record. `ON DELETE CASCADE` from Block, since a per-instance record has no meaning without its block. |
| `Name` | nvarchar(100), nullable | Optional; unused in the per-instance flow, present for the future library. |
| `Source` | nvarchar(max), nullable | The clean Vue the author wrote. Source of truth for editing and recompiles. |
| `CompiledContent` | nvarchar(max), nullable | The SystemJS module string served to browsers. |
| `CompiledVueVersion` | nvarchar(50), nullable | Vue version the compile targeted, for recompile-on-upgrade. |
| `CompiledDateTime` | datetime, nullable | When the stored compile was produced (stamp server-side on save). |
| `IsActive` | bit, default 1 | Present for the future library; per-instance records are active. |

Note on the `BlockId` cascade: this is the rare parent-child ownership case where cascade delete is correct, because a per-instance record is meaningless once its block is gone. Future reusable records carry a null `BlockId` and are unaffected.

Files (following the entity-model conventions):

- `Rock/Model/CMS/ObsidianContent/ObsidianContent.cs` (entity, with `EntityTypeConfiguration`).
- `Rock/Model/CMS/ObsidianContent/ObsidianContentService.cs` (service layer; add a "get or create by block id" helper here).
- `Rock/SystemGuid/EntityType.cs` entry for the new entity type.
- Migration in `Rock.Migrations/Migrations/` creating `[ObsidianContent]` with the FK conventions above.

## The block: `ObsidianContentDetail`

A single Obsidian block, dropped on any page, that mirrors the `HtmlContentDetail` render-and-edit-in-place pattern.

- **View mode (everyone):** renders the owned record's `CompiledContent` by instantiating the compiled SystemJS module directly (see the deviation note). If the block has no record yet, it renders nothing (or an admin-only "not configured" hint).
- **Edit mode (authorized admins only):** shows a Rock `CodeEditor` for the source and a Save action. Entering edit mode lazy-loads the compiler assets. Save compiles in the browser and calls the block action.

C# side:

- `Rock.Blocks/Cms/ObsidianContentDetail.cs` (entity-based `RockBlockType`). Resolves its `ObsidianContent` by `BlockId`; includes `CompiledContent` in the view config box.
- A `SaveContent` block action accepting `{ source, compiled, vueVersion }`, gated behind EDIT/ADMINISTRATE, that upserts the row for this `BlockId` and stamps `CompiledDateTime`.
- Bags under `Rock.ViewModels/Blocks/Cms/ObsidianContentDetail/`.

Vue side:

- `Rock.JavaScript.Obsidian.Blocks/src/Cms/obsidianContentDetail.obs`, with partials as needed under `.../obsidianContentDetail/` (for example a `viewPanel` and an `editPanel`).

## The browser compile pipeline

Runs only when an admin is editing. Prototype scope is `<template>` plus a plain script object, which needs only the lighter template compiler.

1. Lazy-load the compiler asset when the block enters edit mode (not before, and never for a plain viewer).
2. On Save, compile the template into a render function using the browser compiler's **function mode** (see the deviation note; module mode is not available in the shipped build).
3. Assemble a **SystemJS** `System.register` module directly from the author's imports, the function-mode render code, and the component options object, so it loads through Rock's loader with `@Obsidian/...` left as bare imports the import map resolves. No general ES-module-to-SystemJS transform (for example Babel) is shipped; the constrained Phase 1 authoring contract makes a small hand-written assembler sufficient.
4. On success, call `SaveContent` with the source, the compiled string, and the Vue version.
5. On failure, surface the error in the editor and do not save.

The compiler asset ships as a static file under `RockWeb/Obsidian/Libs/vueCompiler.js` (the browser build of `@vue/compiler-dom`) and is referenced only by the edit path.

Fidelity gradient, deferred: swap the template compiler for the full single-file-component compiler and add a TypeScript transpiler to support `<script setup>`, TypeScript, and `<style scoped>`.

## Security and safety

- Authored code runs in the visitor's browser as the visitor, and can call any API the visitor can. Editing must therefore be restricted to administrators. Gate `SaveContent` and the edit affordance behind EDIT/ADMINISTRATE.
- The eval exception (needed to compile) exists only in the admin edit path. In a dev environment this needs no special handling; a production rollout must decide how that path's Content-Security-Policy is relaxed without loosening the rest of Rock.
- During editing, the live preview should run inside a sandboxed iframe (the CodePen model) so half-finished code cannot touch the real authenticated session until it is saved. This lands in Phase 2.
- The compiled string is stored as-is and served to browsers; there is no server-side sanitization of what the component does. This is acceptable only because authoring is admin-gated.

## As-built deviations (Phase 1)

Two decisions the spec had locked were changed during implementation, each forced by what Rock's shipped Vue tooling actually permits. Both were validated end to end before landing.

1. **Function-mode compilation instead of module mode.** The plan assumed the compiler could emit ES-module output that a module transform would convert to SystemJS. The shipped browser build of `@vue/compiler-dom` rejects `mode: "module"` and `prefixIdentifiers` at compile time (Vue compiler error code 48, "ES module mode is not supported in this build of compiler"), because those are gated behind its `__BROWSER__` flag. The only mode the browser build supports is the default "function" mode. Function mode emits a render function body that references the Vue runtime through a free `Vue` variable and uses a `with (_ctx)` block. The pipeline therefore wraps that body in a non-strict IIFE that receives the Vue runtime namespace, and the generated `System.register` module is deliberately not marked strict (a `with` statement is illegal in strict mode). The Vue compiler still runs only in the admin edit path; the stored module is finished JavaScript with no compiler on the visitor. A consequence is that the Phase 1 authoring fidelity is bounded by function mode (template plus a plain script object, no `prefixIdentifiers`), which matches the intended Phase 1 scope. Full single-file-component and TypeScript fidelity (Phase 3) would require bundling a non-browser compiler build, which is the "compiler weight" risk this spec already flagged.

2. **Direct module instantiation instead of a blob URL.** The plan was to serve the compiled module via a `blob:` URL loaded through `dynamicComponent`. Rock's SystemJS loader wraps `resolve` with two overrides: one appends a `.js` extension to any specifier containing a slash, the other appends a `?<fingerprint>` query. A `blob:` (or `data:`) URL has slashes and cannot carry a query string, so the loader mangles it into an unfetchable URL and the import rejects. The view path instead instantiates the stored `System.register` module directly: it captures the module's registration through a local `System` shim, resolves each real `@Obsidian/...` and `vue` dependency through the loader (those specifiers resolve correctly through the import map), then drives the setters and execute step to obtain the component. This is the same class of module evaluation SystemJS performs internally, so no template compiler runs on the visitor; it simply avoids routing a synthetic URL through the resolver.

## Phased implementation plan

### Phase 0 — De-risking spike (no new files, no branch)

In a throwaway HTML Content block, prove the whole chain end to end:

1. Load the compiler and module transform.
2. Compile a hardcoded template to a render function and transform it to SystemJS.
3. Turn the string into a blob URL and load it via `System.import`.
4. Mount it and confirm a real `RockButton` renders and a click handler fires.

Success here validates the single riskiest assumption before any model or block work.

### Phase 1 — Model plus working per-instance block (this run)

- Create the `ObsidianContent` entity (with nullable `BlockId`), service, SystemGuid, and migration.
- Build `ObsidianContentDetail`: view mode renders the owned record via `dynamicComponent` from a blob URL; edit mode provides the code editor and browser-side compile on save (template plus script only), upserting the record by `BlockId`.
- Ship the compiler assets under `/Obsidian/Libs/`, loaded only in edit mode.
- Register the block type (migration) so it can be placed on a page.
- Prove the counter example (Rock button plus Rock label) authored in place and rendered on the page for a non-admin visitor.

### Phase 2 — Authoring quality (future)

- Sandboxed live preview in the editor.
- Inline compile-error reporting; block save on error.

### Phase 3 — Fidelity (future)

- Full single-file-component and TypeScript support.
- `<style scoped>` handling.

### Phase 4 — Reusable library and productionization (future)

- Reusable records (`BlockId` null): a List/management block, a picker, and a display block that selects a record.
- Recompile-all job triggered on Rock upgrade, using stored `Source` and `CompiledVueVersion`.
- Cacheable endpoint that serves `CompiledContent` so multiple renders share one download.
- The formal edit-path CSP decision.
- Categories on the model for organizing library records.

## File manifest (Phases 0 and 1)

| Purpose | Path |
|---|---|
| Entity | `Rock/Model/CMS/ObsidianContent/ObsidianContent.cs` |
| Service | `Rock/Model/CMS/ObsidianContent/ObsidianContentService.cs` |
| Entity type GUID | `Rock/SystemGuid/EntityType.cs` (new constant) |
| Migration (table + block type) | `Rock.Migrations/Migrations/.../<timestamp>_AddObsidianContent.cs` |
| Block (C#) | `Rock.Blocks/Cms/ObsidianContentDetail.cs` |
| Block bags | `Rock.ViewModels/Blocks/Cms/ObsidianContentDetail/*` |
| Block (Vue) | `Rock.JavaScript.Obsidian.Blocks/src/Cms/obsidianContentDetail.obs` |
| Block partials (Vue) | `Rock.JavaScript.Obsidian.Blocks/src/Cms/obsidianContentDetail/*` (`viewPanel`, `editPanel`) |
| Compile pipeline (TS) | `Rock.JavaScript.Obsidian.Blocks/src/Cms/obsidianContentDetail/obsidianContentCompiler.partial.ts` |
| Compiler asset | `RockWeb/Obsidian/Libs/vueCompiler.js` (browser build of `@vue/compiler-dom`) |

## Open decisions (none block Phase 1)

1. **Exact compiler asset build to bundle.** Resolved during implementation: ship the browser (global) build of `@vue/compiler-dom` at the version matching Rock's shipped Vue (3.3.10), as `RockWeb/Obsidian/Libs/vueCompiler.js`. The development build is used rather than the production build so that authoring errors surface as readable messages instead of numeric codes. No separate module transform is bundled; see "As-built deviations (Phase 1)".
2. **Empty-state behavior** when a block has no record yet (render nothing versus an admin-only hint). Resolved: render nothing for visitors, and for an administrator show a hint with an "Add Content" affordance that enters edit mode.

## Risks

1. **Compiler weight and the eval exception**, both confined to the edit path. Measure the editor load; decide the CSP story before production.
2. **Fidelity gap** between the browser template compiler and Rock's Rollup build. Contained by starting template-only.
3. **Version drift** between the shipped compiler assets and Rock's Vue version. Because Rock ships both, keep them in lockstep in the build.
4. **Content rot** across upgrades, addressed by storing `Source` and the Phase 4 recompile job.
