---
name: convert-block
description: >-
  Converts a Rock RMS WebForms block (.ascx/.ascx.cs) to Obsidian (Vue 3 + C# RockBlockType).
  Triggers on: "convert block", "migrate block to obsidian", "obsidian conversion", or a
  WebForms block path like "RockWeb/Blocks/Core/Foo.ascx.cs" with conversion intent.
  Classifies blocks as Detail, List, or Custom; generates C# block, bags, .d.ts placeholders,
  Vue SFC, partials; chops WebForms files; creates a feature branch.
  Do NOT use for non-block tasks, creating new blocks from scratch, or general Rock RMS questions.
argument-hint: "Category/BlockName (e.g. Core/ExceptionDetail or Security/ForgotUserName)"
compatibility: Requires Claude Code CLI with Node.js for script execution (generate-guids.js, validate-conversion.js).
metadata:
  version: "1.12"
  author: "Maxwell Eley"
---

**CRITICAL: Enter plan mode immediately.** Do all research first, ask clarifying questions, write a plan, and get user approval before writing any files.

You are converting a Rock RMS ASP.NET WebForms block to the Obsidian framework.

Quality is more important than speed. Read the full WebForms block carefully before classifying. Do not skip validation steps or abbreviate generated file contents.

The block to convert is: **$ARGUMENTS**

---

## Conversion Philosophy

**You are not a 1:1 translator.** The WebForms block is a _requirements reference_, not a code template. Your job is to produce an idiomatic Obsidian block that delivers the same _behavior_ — not to replicate the same _implementation_.

**When WebForms and correctness conflict, correctness wins.** If the WebForms code has bugs, N+1 queries, missing null checks, service calls where cache exists, or patterns that don't apply to Obsidian — fix them. Do not carry forward problems just because "the original did it this way."

What to preserve from WebForms:
- **User-facing behavior** — what the block does, what the user sees, how they interact
- **Business rules** — authorization checks, validation logic, data transformations
- **Configuration** — block settings, attribute keys, linked pages

What to improve during conversion:
- **Page parameter resolution** — WebForms reads page-parameter entities by integer Id (`PageParameter( ... ).AsInteger()` → `.Get( id )`). Obsidian's standard is IdKey. Resolve the raw string key with the IdKey-aware overload (`Get( key, !PageCache.Layout.Site.DisablePredictableIds )`) so Id, IdKey, and Guid all work. This is a **required** improvement — see `references/common-patterns.md` § "Page Parameter Resolution".
- **Performance** — fix N+1 queries, use cache instead of service for read-only lookups, pre-fetch data before loops
- **Null safety** — use `?.`, `??`, and early returns instead of deep null-check nesting
- **Modern C#** — string interpolation, pattern matching, null-conditional operators, collection expressions where appropriate
- **Dead code** — do not carry forward commented-out code, unused variables, or WebForms-only plumbing (`ViewState`, `PostBack` checks, `UpdatePanel` logic)
- **Obsidian controls** — use standard Obsidian controls (`TextBox`, `DropDownList`, `DatePicker`, etc.) instead of hand-rolling HTML that the WebForms block may have used
- **Rock utilities** — use `RockDateTime`, `ListItemBag`, cache classes, and other Rock helpers instead of raw .NET equivalents

**Do not mention WebForms in code comments.** The converted file should read as if written from scratch — phrases like "matches WebForms", "mirrors the WebForms behavior", or references to original methods (`btnSave_Click`, `ShowReadonlyDetails`, `NavigateToParentPage`, etc.) are forbidden. Only reference WebForms when a future reader genuinely needs that context (documented surprising divergence, carried-over bug-fix rationale). See `references/common-patterns.md` § "Comments in Converted Code".

---

## Reference Routing Table

Load reference files progressively — only when needed for the current phase.

| Reference File | Load When |
|---|---|
| `references/common-patterns.md` | After Phase 1 classification (always) |
| `references/detail-block-patterns.md` | Block classified as **Detail** |
| `references/list-block-patterns.md` | Block classified as **List** |
| `references/custom-block-patterns.md` | Block classified as **Custom** |
| `references/implementation-details.md` | Implementation (after plan approval) |
| `references/troubleshooting.md` | When errors occur, patterns seem unsupported, or build fails |
| `references/examples.md` | If unsure about output format for any phase |

**Do NOT read all reference files upfront.** Read them as each phase requires.

---

## Scripts

| Script | When to use |
|---|---|
| `scripts/generate-guids.js` | Implementation — before writing the C# block file |
| `scripts/validate-conversion.js` | Implementation — quality gate |

---

## Phase 1 — Research (read-only)

1. **Resolve the block path:**
   - If a full path was given (e.g., `Core/ExceptionDetail`): read `RockWeb/Blocks/$ARGUMENTS.ascx.cs`
   - If only a block name was given: use `Glob` to search `RockWeb/Blocks/**/$ARGUMENTS.ascx.cs`
   - **If no match is found:** read `references/troubleshooting.md` for resolution steps. Do not guess.

2. **Read the `.ascx.cs` and `.ascx` files fully.**

3. **Scan for unsupported patterns** — check for these before proceeding:
   - `System.Web.HttpContext` (not just `System.Web` in using statements)
   - `ScriptManager.RegisterStartupScript`
   - `UpdatePanel` with complex partial postback logic
   - `ViewState` for non-trivial state management
   - Nested `UserControl` references (`.ascx` includes)
   - `Session[` access

   If found, note them — they'll be called out in Phase 2.

4. **Scan for performance issues** — identify these even if the WebForms code had them. The Obsidian conversion must fix them:
   - **N+1 queries:** Service `.Get()` or `.Queryable()` calls inside `foreach`/`for` loops
   - **Lazy-load traps:** Navigation property access (e.g., `item.PersonAlias.Person.FullName`) inside loops without `.Include()` or pre-fetching
   - **Repeated instantiation:** `new XService( rockContext )` created per iteration instead of once before the loop
   - **Cache misuse:** Service queries for data available via cache classes (`DefinedTypeCache`, `CampusCache`, `GroupTypeCache`, etc.)

   **Fix strategy:** Pre-fetch all needed data into a dictionary or list before the loop. Use `.Include()` for required navigation properties. Use cache classes for read-only lookups. These fixes are **required** — do not carry N+1 patterns forward from WebForms.

5. **Scan for page parameter resolution** — find every entity or cache lookup driven by a page parameter (`PageParameter( PageParameterKey.X )`). WebForms almost always fetches these by integer Id (`.AsInteger()` → `.Get( id )`). Obsidian's standard is IdKey, so each one **must** be converted to the IdKey-aware overload that accepts Id, IdKey, or Guid:

   ```csharp
   var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
   // or, for a cache class:
   var site = SiteCache.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
   ```

   This is a **required** fix — do not carry forward `.AsInteger()` + `.Get( int )`. The only exception is a parameter that is genuinely a number (page index, count, year), which is not an entity lookup. See `references/common-patterns.md` § "Page Parameter Resolution".

6. **Classify the block type:**

   | Signs in the WebForms block | Type |
   |---|---|
   | Renders a `GridView` or `<Rock:Grid>`, queries a collection, has add/delete buttons, links to a detail page or inline modal | **List** |
   | Displays one entity with view + edit modes, Save/Cancel/Delete buttons, possibly breadcrumbs | **Detail** |
   | None of the above — dashboard, widget, context setter, Lava block, utility | **Custom** |

   For detail blocks, assume `IBreadCrumbBlock` is needed — implement it unless the WebForms block clearly did not use breadcrumbs or is a non-navigated block.

7. **Load references:**
   - Read `references/common-patterns.md` (always)
   - Read the type-specific reference file based on classification

8. **Identify the matching canonical reference block** from the "Canonical Reference Blocks" table in `common-patterns.md`. You will read the specific canonical block files later when implementing — not now.

### Phase 1 Quality Gate

Before presenting results, verify:
- [ ] Block files found and fully read
- [ ] Unsupported patterns identified (or confirmed none)
- [ ] Performance issues scanned (N+1, cache misuse, lazy-load traps)
- [ ] Page parameter entity/cache lookups identified and flagged for IdKey conversion (or confirmed none)
- [ ] Classification justified with specific evidence from the code
- [ ] Base class selected with reasoning (use the "Base Class Selection" table in `common-patterns.md`)
- [ ] `common-patterns.md` and type-specific reference loaded

Present Phase 1 results in this format:
- **Block:** [name] — [line count] lines (.ascx.cs)
- **Classification:** Detail | List | Custom — [one-line justification from code evidence]
- **Base class:** [selected class] — [reason from Base Class Selection table]
- **Unsupported patterns:** [list with line numbers, or "None found"]
- **Performance issues:** [list N+1 patterns, cache misuse, lazy-load traps with line numbers, or "None found"]
- **Page parameter lookups:** [list each `PageParameter(...)` entity/cache fetch with line numbers and the IdKey-aware resolution to use, or "None found"]
- **Canonical reference:** [block name from table]

Wait for user input before continuing.

---

## Phase 2 — Propose design and ask clarifying questions

### Unsupported pattern callouts

If Phase 1 found unsupported patterns, present them first:
> **Patterns requiring special attention:**
> - `[pattern]` — found at line X. Proposed replacement: [approach]

If the block relies heavily on `System.Web` throughout, flag that it may need redesign rather than line-by-line conversion.

### Performance fix proposals

If Phase 1 identified performance issues (N+1 queries, cache misuse, lazy-load traps), present the fix strategy for each:
> **Performance fixes:**
> - N+1 at line X: [pre-fetch into dictionary / use `.Include()` / batch query]
> - Cache misuse at line Y: [replace `XService` with `XCache.Get()`]

These fixes are **required** — do not carry performance issues forward from WebForms even if "it worked before."

### Design proposal

Present:
- Block type and base class with reasoning
- Overall architecture (files to create, partials needed)
- Key behaviors being carried forward from WebForms

### Clarifying questions

**You must ask questions — do not silently assume.** The user would rather answer a quick question than debug a wrong assumption after implementation.

**Two categories of questions:**

#### Always ask (unless the answer is 100% unambiguous from the code)

These decisions have high cost if wrong. Ask even if you have a reasonable guess — state your guess and ask the user to confirm or correct.

| Topic | Why it matters |
|---|---|
| **Which fields to show on the view panel** (detail blocks) | WebForms often dumps everything; Obsidian should be curated. Propose a list and ask. |
| **Grid columns and ordering** (list blocks) | Column selection directly affects UX. Propose what WebForms had and ask if any should be dropped/added/reordered. |
| **Filter approach** (list blocks) | Column-only vs. server-side filters is an architectural decision. State your recommendation and confirm. |
| **Entity attributes** (detail blocks) | If the entity has `LoadAttributes()` in WebForms, confirm whether attributes should be on view, edit, or both. |
| **Behaviors that look buggy or intentional** | If WebForms code does something odd (e.g., filtering by hardcoded ID, unusual sort, hidden field), ask whether it's intentional or a bug to fix. |

#### Infer from the code (don't ask)

These have clear answers in the WebForms source. Asking about them wastes time.

| Topic | How to infer |
|---|---|
| Base class selection | Determined by block classification — use the table |
| Navigation / linked pages | Copy from WebForms `AttributeKey` and `GetAttributeValue` calls |
| Security model (block vs entity) | Match what WebForms does — `IsUserAuthorized` = block-level, `entity.IsAuthorized` = entity-level |
| Breadcrumb implementation | If WebForms has `IBreadCrumbBlock` or sets breadcrumb text, include it |
| IsSystem guard | If entity has `IsSystem` property, include the guard |
| ContentSection layout | Use the WebForms panel/section structure as a guide; only ask if it's genuinely unclear how to group fields |

#### Format

Present questions as numbered items with your proposed answer in brackets. This lets the user quickly confirm or override:

> 1. **View panel fields** — I'd show Name, Description, IsActive, and Campus. The WebForms block also shows InternalCode and CreatedDate — include those? **[Proposed: exclude both]**
> 2. **Grid filters** — The WebForms block has 3 server-side filters (Status, Campus, Date Range) that reduce the DB query. Keep all three as server-side? **[Proposed: yes, keep server-side]**
> 3. **Line 142 oddity** — The WebForms block filters by `CategoryId == 5` (hardcoded). Intentional or bug? **[Proposed: replace with block setting]**

**Wait for the user to answer before continuing.**

---

## Phase 3 — Write and approve the plan

**The plan is the single source of truth for implementation.** After approval, you will exit plan mode and follow the plan — SKILL.md will not be re-read. Everything the agent needs must be in the plan itself.

### Plan structure

Write a plan with these sections, adapting the template below to the specific block. Replace all `[placeholders]` with concrete values.

**Section 1 — Design** (from Phases 1-2):
- Block type, base class, and reasoning
- All files to create (full paths) and files to delete
- Key design decisions and how clarifying questions were resolved
- Unsupported pattern replacements (if any)

**Section 2 — Implementation** (the agent's step-by-step execution guide):

```
## Implementation

### Step 1: Create feature branch
git branch -a | grep "feature-v" | head -5
# Extract version number (e.g., feature-v19-claude-foo → 19)
git checkout -b feature-v[version]-claude-[blocknamelower]
git branch --show-current
# STOP if output is develop, main, or master. Fix before proceeding.

### Step 2: Load references
Read these files before writing any code:
- `references/implementation-details.md` — bag rules, type mapping table, file checklist
- Canonical reference block files: [list the specific C# and .obs files from Phase 1]

### Step 3: Create bags
Path: `Rock.ViewModels/Blocks/[Category]/[BlockName]/`
Files: [list each bag file with its full path]

### Step 4: Create .d.ts placeholders
Path: `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/[Category]/[BlockName]/`
One per bag. Follow the type mapping table from implementation-details.md.

### Step 5: Generate GUIDs
Run: `node .claude/skills/convert-block/scripts/generate-guids.js`
Use the output VERBATIM for EntityTypeGuid and BlockTypeGuid.
Do NOT fabricate GUIDs. Both values must be uppercase.

### Step 6: Create C# block
Path: `Rock.Blocks/[Category]/[BlockName].cs`
Use the canonical reference block as the structural template.

### Step 7: Create Obsidian .obs component
Path: `Rock.JavaScript.Obsidian.Blocks/src/[Category]/[blockName].obs`
Use the canonical reference block's .obs file for layout and structure.

### Step 8: Create partials
[List each partial file with its full path — e.g., viewPanel.partial.obs, editPanel.partial.obs, types.partial.ts]

### Step 9: Chop WebForms
Delete:
- `RockWeb/Blocks/[Category]/[BlockName].ascx`
- `RockWeb/Blocks/[Category]/[BlockName].ascx.cs`

### Step 10: Validate
Run: `node .claude/skills/convert-block/scripts/validate-conversion.js [Category] [BlockName] [detail|list|custom]`
Fix ALL failures before continuing.
If build errors or unexpected patterns occur, read `references/troubleshooting.md`.

### Next steps (after validation passes)
- Inform the user to run **Rock.CodeGeneration** (WPF app at `Rock.CodeGeneration/`) to regenerate .d.ts files from C# ViewModels
- Remind: when writing the migration to register this block, use `AddOrUpdateEntityBlockType()` — **never** `UpdateBlockTypeByGuid()` for Obsidian blocks (see `.claude/rules/data-model.md` § Block Type Methods)
```

### Phase 3 Quality Gate

Before presenting the plan, verify:
- [ ] Every file has a full, concrete path (no unresolved placeholders)
- [ ] All user answers from Phase 2 are incorporated
- [ ] Unsupported patterns have documented replacements
- [ ] Implementation section includes all 10 steps with block-specific values filled in
- [ ] Canonical reference file paths are listed explicitly in Step 2

**CRITICAL: Do not write a single file until the user explicitly approves this plan.**
