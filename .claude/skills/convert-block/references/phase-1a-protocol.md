# Phase 1A Protocol — Parity Map

Phase 1A is sequential (not parallel). It produces the foundation artifact (`parity-map.md`) and conditionally produces `figma-design.md` and `new-features.md` based on the conversion's scope. Phase 1B fan-out fires after this phase if the block is large or if redesign / new features are in scope.

**Exhaustiveness mandate.** When in doubt, include it. Do not skim. If you catch yourself summarizing a section as "looks fine", re-read it line by line. After the initial read, sweep the source top to bottom one more time. The second pass always finds something. (This mirrors the same mandate in `review-conversion/SKILL.md`.)

---

## 1A.0: Detect conversion scope

A conversion has three orthogonal scope axes. Detect each before doing any other work; later sub-steps fire conditionally based on what you find.

| Axis | Source signal | If detected |
|---|---|---|
| **Translation** (always present) | The `.ascx.cs` exists | Continue with 1A.1+ |
| **Redesign** (optional) | A Figma URL in `$ARGUMENTS` or the immediate user message — match against `https?://(www\.)?figma\.(com\|design)/[^\s]+` | Capture the URL; 1A.9 fires |
| **New features** (optional) | Phrases in `$ARGUMENTS` like "add", "new feature", "also support", "and include", OR Figma frames depicting Obsidian-only behavior surfaced by 1A.9 | 1A.10 fires |

Record the scope explicitly in the Phase 1A summary:

> **Scope:** translation only | translation + redesign | translation + new features | translation + redesign + new features
> **Figma URL:** [URL or "none"]

If a Figma URL is present, **Phase 1B fires regardless of block size.** Redesigns introduce architectural decisions (component swaps, frame-to-panel mapping, design-token reconciliation) that benefit from the Pattern Reviewer subagent's structured output.

---

## 1A.1: Resolve the block path

- If a full path was given (e.g., `Core/ExceptionDetail`): read `RockWeb/Blocks/$ARGUMENTS.ascx.cs`
- If only a block name was given: use `Glob` to search `RockWeb/Blocks/**/$ARGUMENTS.ascx.cs`
- If no match is found: read `references/troubleshooting.md` for resolution steps. Do not guess.

---

## 1A.2: Read the required input set

Read both of the following fully before classifying:

1. `RockWeb/Blocks/{Category}/{BlockName}.ascx.cs` (full source)
2. `RockWeb/Blocks/{Category}/{BlockName}.ascx` (markup)

---

## 1A.3: Create the /working/ folder

Compute the kebab-case folder name from the block name (`ContentChannelItemDetail` → `content-channel-item-detail`). Create `/working/{block-name-kebab}/`.

---

## 1A.4: Scan the source for problems

Walk the `.ascx.cs` and `.ascx` looking for two classes of issue:

**Unsupported WebForms patterns** (need a deliberate replacement, not a port):
- `System.Web.HttpContext` (not just `System.Web` in `using`)
- `ScriptManager.RegisterStartupScript`
- `UpdatePanel` with complex partial postback logic
- `ViewState` for non-trivial state management
- Nested `UserControl` references (`.ascx` includes)
- `Session[` access

**Performance issues** (must be fixed in conversion, even if WebForms had them):
- N+1 queries: service `.Get()` / `.Queryable()` inside `foreach`/`for` loops
- Lazy-load traps: navigation-property access in loops without `.Include()` or pre-fetching
- Repeated instantiation: `new XService(rockContext)` per iteration
- Cache misuse: service queries for entities with cache classes (`DefinedTypeCache`, `CampusCache`, `GroupTypeCache`, `EntityTypeCache`, `CategoryCache`)

Note both with line numbers. Unsupported patterns get called out in Phase 2 design discussion. Performance issues get fix proposals in Phase 2 (pre-fetch into dictionary, add `.Include()`, swap to cache class).

---

## 1A.5: Classify the block type

| Signs in the WebForms block | Type |
|---|---|
| Renders a `GridView` or `<Rock:Grid>`, queries a collection, has add/delete buttons, links to a detail page or inline modal | **List** |
| Displays one entity with view + edit modes, Save/Cancel/Delete buttons, possibly breadcrumbs | **Detail** |
| None of the above (dashboard, widget, context setter, Lava block, utility) | **Custom** |

For detail blocks, assume `IBreadCrumbBlock` is needed unless the WebForms block clearly did not use breadcrumbs.

---

## 1A.6: Load type-specific reference

- Read `references/common-patterns.md` (always)
- Read the type-specific reference based on classification (`detail-block-patterns.md`, `list-block-patterns.md`, or `custom-block-patterns.md`)

---

## 1A.7: Identify the canonical reference block

Use the "Canonical Reference Blocks" table in `common-patterns.md`. You will read the specific canonical block files later when implementing.

---

## 1A.8: Produce parity-map.md

Read `references/working/parity-map-template.md`. It documents the column structure (matching `/review-conversion`'s Phase 2 verbatim) and the seven trace dimensions to organize the rows by. Write `/working/{block-name-kebab}/parity-map.md` per the template. If new features are in scope, the template's Trace 8 captures Obsidian-only behaviors.

The "Obsidian Equivalent (planned)" column starts empty and stays empty through Phase 3. `/review-conversion` fills it in post-implementation while walking the actual code. Phase 3 instead writes a per-trace architectural summary at the top of `parity-map.md` (see SKILL.md § Phase 3). "Verdict (planned)" stays empty until `/review-conversion` runs.

---

## 1A.9: Read the Figma design (only if a Figma URL was captured in 1A.0)

Skip this step if no Figma URL is in scope.

Use the Figma MCP tools available in the user's environment, in this order:

1. `get_design_context` — pull the structured component tree, frames, and layout hierarchy from the linked node.
2. `get_metadata` — capture component names, frame IDs, page structure.
3. `get_screenshot` — capture visuals for reference. One per major frame; do NOT bulk-load every frame.
4. `get_variable_defs` — extract design tokens (colors, spacing, typography) the design references.
5. `use_figma` / `get_libraries` — for any aspect not covered by the structured tools above.

**Save screenshots to disk under `/working/{block-name-kebab}/figma/` and reference them by path.** Do not embed screenshots inline in the artifact; they bloat the context window.

Read `references/working/figma-design-template.md`. Write `/working/{block-name-kebab}/figma-design.md` per the template. The artifact captures: frame inventory, component-to-control mapping, design-token-to-Rock-CSS-var mapping, frame-to-panel mapping, and a list of behaviors implied by the design but NOT in WebForms (which feeds `new-features.md`).

---

## 1A.10: Identify new features (only if scope includes new features)

Skip this step if no new features are in scope.

Source signals (already captured in 1A.0 and 1A.9):
- Phrases in `$ARGUMENTS` describing new functionality.
- Figma frames in `figma-design.md` § 6 that depict behaviors not present in the WebForms block.
- User-attached design docs / specs.

Read `references/working/new-features-template.md`. Write `/working/{block-name-kebab}/new-features.md` per the template. Each candidate feature gets a stable ID (`N1`, `N2`, ...) and acceptance criteria. The "In-scope for this PR?" column starts as `TBD (Phase 2)`; Phase 2 confirms with the user.

If new-features.md ends up genuinely empty, stub it with "No new features in scope" and continue.

---

## Phase 1A Quality Gate

Before presenting Phase 1 results, verify:
- [ ] Conversion scope detected (translation only / + redesign / + new features / + both)
- [ ] Block files found and fully read
- [ ] /working/{block-name-kebab}/ folder created
- [ ] Unsupported patterns identified (or confirmed none)
- [ ] Performance issues scanned
- [ ] Classification justified with specific evidence from the code
- [ ] Base class selected with reasoning (use `common-patterns.md` § Base Class Selection)
- [ ] `common-patterns.md` and type-specific reference loaded
- [ ] parity-map.md drafted (rows for every method, query, security check, navigation handler, preference, and UI behavior)
- [ ] If Figma URL captured: figma-design.md written; screenshots saved to `/working/{block-name-kebab}/figma/`
- [ ] If new features in scope: new-features.md written with stable IDs and acceptance criteria

---

## Phase 1A presentation format

Present Phase 1A results in this format:
- **Scope:** translation only | translation + redesign | translation + new features | translation + redesign + new features
- **Figma URL:** [URL or "none"]
- **Block:** [name], [line count] lines (.ascx.cs)
- **/working/ folder:** `/working/{block-name-kebab}/`
- **Classification:** Detail | List | Custom, [one-line justification]
- **Base class:** [selected class], [reason]
- **Unsupported patterns:** [list with line numbers, or "None found"]
- **Performance issues:** [list with line numbers, or "None found"]
- **Canonical reference:** [block name from table]
- **parity-map.md:** [N rows across 7 traces, +M rows in Trace 8 if new features]
- **figma-design.md:** [N frames in scope, K frames out of scope] | "not applicable, no Figma URL"
- **new-features.md:** [N candidate features] | "no new features in scope"

Then decide whether to fan out to Phase 1B per `references/phase-1b-protocol.md` § Trigger.
