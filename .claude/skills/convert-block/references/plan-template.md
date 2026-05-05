# Plan Template

The structure of `/working/{block-name-kebab}/plan.md`. Plan.md is the **bridge from research to code**, not the research itself. Cite /working/ artifacts; don't repeat them.

There is no target length. Plan.md scales with the block: a 300-line block produces a short plan, a 2000-line block produces a longer one, and that's fine.

The constraint is **no duplication with /working/ artifacts**. If a section starts repeating the parity table, the improvement analysis, or any other /working/ content, replace it with a citation. The /working/ folder is the source of truth; plan.md links into it.

This template uses guidance language. The model should adapt sections that don't fit the block; do not pad sections that aren't needed.

---

## §1 Block Summary

One or two paragraphs covering:
- Block name, line count of the original `.ascx.cs`, classification (Detail / List / Custom).
- Selected base class and one-line rationale.
- Whether Phase 1B fan-out fired and which trigger(s) hit.
- Selected canonical reference block.

This section is an overview, not analysis. Anything beyond the four bullets above belongs in `parity-map.md` or `data-model.md`.

---

## §2 Key Design Decisions

A bulleted list of design decisions made during research and Phase 2. Each bullet should:
- State the decision in one sentence.
- Cite the /working/ artifact that supports it (e.g., "see `clarifying-questions.md` Q2", "see `obsidian-pattern-analysis.md` § Edit panel root element").
- Note the alternative considered, in one phrase, when there was a real choice.

Group decisions by category. Use the categories that apply to this conversion's scope; drop the rest.

### Carry-forward and improvements

Decisions about preserving WebForms behavior and applying bug-fix improvements:
- View vs edit bag split (which fields go where, why)
- Edit panel root element (`<fieldset>` vs `<ContentSectionContainer>`)
- Filter approach (column-only vs server-side; if server-side, which preferences)
- Whether to register with `IBreadCrumbBlock`
- How to handle a sibling-block ID-format mismatch (in-scope or follow-up)
- Any improvement applied during conversion that materially diverges from the WebForms behavior

### Redesign decisions (only when scope includes redesign)

Decisions driven by the Figma design. Cite `figma-design.md`. Examples:
- Frame-to-panel mapping (cite `figma-design.md` § 5)
- Component swaps that diverge from canonical-reference convention because the design requires it
- Token/variable choices that need a `/css-cleanup` follow-up
- Carry-forward conflicts with the redesign — for each WebForms behavior the design explicitly drops, state the decision and cite the user's confirmation in `clarifying-questions.md`

### New-feature scope (only when scope includes new features)

Decisions about behaviors with no WebForms baseline. Cite `new-features.md`. Examples:
- Which `new-features.md` rows are in scope for THIS PR vs. follow-up (cite Phase 2 confirmation)
- New entities, services, or migrations the new feature requires
- New permission semantics introduced by the feature
- Any architectural choice that diverges from the canonical reference because the new feature requires it

If there are zero non-trivial design decisions, the section can be a single line: "Standard {Detail/List/Custom} block; no significant departures from canonical reference."

---

## §3 Files to Create / Files to Delete

Two flat lists of paths. No prose.

### Files to create

```
Rock.ViewModels/Blocks/[Category]/[BlockName]/[BlockName]OptionsBag.cs
Rock.ViewModels/Blocks/[Category]/[BlockName]/[BlockName]Bag.cs
Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/[Category]/[BlockName]/[bagNameCamelCase].d.ts
...
Rock.Blocks/[Category]/[BlockName].cs
Rock.JavaScript.Obsidian.Blocks/src/[Category]/[blockName].obs
Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/viewPanel.partial.obs
Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/editPanel.partial.obs
Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/types.partial.ts
```

### Files to delete

```
RockWeb/Blocks/[Category]/[BlockName].ascx
RockWeb/Blocks/[Category]/[BlockName].ascx.cs
```

### Files to edit (framework or sibling blocks)

If any edit lands outside the block's own folders (e.g., a sibling WebForms detail block that needs to accept idKey, a framework `.d.ts` that needs a new prop), list it here with one-line justification. **Each entry triggers the framework-edit gate at implementation time.** Do not surprise the user with these edits later.

```
RockWeb/Blocks/Cms/SiblingDetail.ascx.cs    ← accept idKey alongside int (sibling unblocking)
```

### Files added for new features (only when scope includes new features)

New features may add files that don't follow the standard naming pattern (a new partial for an Obsidian-only behavior, a new entity, a new EF migration). Note these explicitly with the new-features.md row they implement.

```
Rock.JavaScript.Obsidian.Blocks/src/Cms/ContentChannelItemDetail/commentsPanel.partial.obs    ← N1 inline comments
Rock/Model/CMS/ContentChannelItemComment/ContentChannelItemComment.cs                          ← N1 (new entity)
Rock.Migrations/Migrations/202605..._AddContentChannelItemComment.cs                           ← N1 (entity migration)
```

---

## §4 Implementation Steps

The 10-step structure. Replace `[placeholders]` with concrete values. Do not abbreviate; the agent follows this verbatim after exiting plan mode.

```
Step 1: Create feature branch
git branch -a | grep "feature-v" | head -5
# Extract version number (e.g., feature-v19-claude-foo → 19)
git checkout -b feature-v[version]-claude-[blocknamelower]
git branch --show-current
# STOP if output is develop, main, or master. Fix before proceeding.

Step 2: Load references
Read these files before writing any code:
- references/implementation-details.md
- Canonical reference C# block: [path]
- Canonical reference .obs: [path]

Step 3: Create bags
Path: Rock.ViewModels/Blocks/[Category]/[BlockName]/
Files: [enumerate each bag file]

Step 4: Create .d.ts placeholders
Path: Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/[Category]/[BlockName]/
One per bag. Type mapping per implementation-details.md.
Cross-language types (enums, ListItemBag fields): read the C# source. Do NOT
infer enum values from how the Vue template uses them. data-model.md
documents the C# enum integer values verbatim, match exactly. Before declaring
any enum locally in `types.partial.ts`, check whether the C# source lives in
`Rock.Enums/` and whether the auto-generated TS twin already exists at
`Rock.JavaScript.Obsidian/Framework/Enums/[Domain]/[enumName].ts` — if it does,
import from `@Obsidian/Enums/[Domain]/[enumName]` instead of redeclaring. See
`references/common-patterns.md` § Enum Management for the full rule.

Step 5: Generate GUIDs
Run: node .claude/skills/convert-block/scripts/generate-guids.js
Take the EntityTypeGuid output verbatim. The BlockTypeGuid output is informational only — it goes in a `// was [Rock.SystemGuid.BlockTypeGuid( "..." )]` comment immediately above the active attribute. The active `[Rock.SystemGuid.BlockTypeGuid(...)]` MUST reuse the existing WebForms block's GUID (read it from `RockWeb/Blocks/[Category]/[BlockName].ascx.cs`'s own `[Rock.SystemGuid.BlockTypeGuid(...)]`). This is what enables the chop without a registration migration; see `references/common-patterns.md` § GUID Assignment Rules.

Step 6: Create C# block
Path: Rock.Blocks/[Category]/[BlockName].cs
Run `dotnet build Rock.Blocks/Rock.Blocks.csproj -v:q` and fix every error
and warning your block contributed before declaring this step complete. Do
not move on with errors deferred. The compile-and-typecheck gate is part of
the checkpoint protocol — see references/checkpoint-protocol.md § 4.5.
[Checkpoint #1 fires here, see §5]

Step 7: Create Obsidian .obs component
Path: Rock.JavaScript.Obsidian.Blocks/src/[Category]/[blockName].obs

Step 7.5: Implement Obsidian-only behaviors (only when new-features.md is non-empty)
Cite the in-scope rows from /working/[block-name-kebab]/new-features.md and the
Figma frames they implement (from figma-design.md if present). Each in-scope row
should map to specific files (a new partial, a new entity, a new migration, etc.)
already enumerated in §3 above.
Acceptance criteria from new-features.md drive what "done" looks like;
/review-conversion verifies them against parity-map.md Trace 8.
Skip this step if no new features are in scope.

Step 8: Create partials
[List each partial file]
After every partial is written, run BOTH:
  (cd Rock.JavaScript.Obsidian.Blocks && npx eslint src/[Category]/[blockName].obs src/[Category]/[BlockName]/)
  (cd Rock.JavaScript.Obsidian.Blocks && npx vue-tsc -p src/[Category]/tsconfig.json --noEmit --pretty false 2>&1 | grep -i [BlockName])
ESLint finds style/unused issues; vue-tsc finds the type errors ESLint
silently allows (StandardListItemBag prop mismatches, control v-model
contracts, Guid?/null coercion). Both must produce no output for the new
files before the step is complete. See references/checkpoint-protocol.md § 4.5.
[Checkpoint #2 fires here, see §5]

Step 8.5: Framework edit (only when plan §3 lists files outside the block's own folders)
After the framework file is written, re-run the Step 8 gate AND
  (cd Rock.JavaScript.Obsidian && npx vue-tsc -p Framework/Controls/tsconfig.json --noEmit --pretty false)
to make sure no other consumer of the edited control was broken.

Step 9: Chop WebForms
Delete:
- RockWeb/Blocks/[Category]/[BlockName].ascx
- RockWeb/Blocks/[Category]/[BlockName].ascx.cs

Confirm the C# class's `[Rock.SystemGuid.BlockTypeGuid(...)]` reuses the WebForms block's GUID (per Step 5). This — combined with deleting the .ascx files — IS the chop. `BlockTypeService.StagePossibleMigrateWebFormsToObsidianBlock` runs at the next Rock startup, finds the existing WebForms `BlockType` row by GUID, points its `EntityType` at the new entity-based class, and re-points every `Block` instance on every page. No migration is required for the swap.

Step 10: Final checkpoint + validation
[Final checkpoint fires here, see §5]
Re-run the full compile-and-typecheck gate (per references/checkpoint-protocol.md § 4.5):
  - `dotnet build Rock.Blocks/Rock.Blocks.csproj -v:q` → 0 errors / 0 new warnings
  - `(cd Rock.JavaScript.Obsidian.Blocks && npx vue-tsc -p src/[Category]/tsconfig.json --noEmit)` → 0 errors
  - If a framework file was edited: `(cd Rock.JavaScript.Obsidian && npx vue-tsc -p Framework/Controls/tsconfig.json --noEmit)` → 0 errors
THEN run: node .claude/skills/convert-block/scripts/validate-conversion.js [Category] [BlockName] [detail|list|custom]
Fix ALL failures. Do not declare the conversion complete while any of the gates report errors.

Next steps:
- Inform user to run Rock.CodeGeneration to regenerate .d.ts files
- Confirm no `Rock.Migrations` migration was added for the chop. The chop is automatic at Rock startup; writing `AddOrUpdateEntityBlockType` would create a duplicate BlockType row and existing pages would not pick up the Obsidian block. Migrations are only needed when the conversion's new-features scope adds entities, schema, or seed data.
- Suggest /review-conversion
- If improvement-analysis.md flagged inline styles, suggest /css-cleanup
```

---

## §5 Checkpoints

State the checkpoint count chosen for this block (1, 2, 3, or 4) and the trigger size.

For each checkpoint, list:
- **When** it fires (which step in §4)
- **What /working/ artifacts** it reads (NOT plan.md)
- **Which subset of `review-conversion/references/review-checklist.md`** it runs

Example (a 1500-line detail block, 3 checkpoints):

| # | Fires after | Reads | Runs |
|---|---|---|---|
| 1 | Step 6 (C# block written) | `parity-map.md`, `data-model.md`, `improvement-analysis.md` | review-checklist §1 Functional Parity, §3 Bug Patterns (C#), §5 Obsidian C# Block Structure |
| 2 | Step 8 (.obs + partials written) | `parity-map.md`, `state-machine.md`, `obsidian-pattern-analysis.md` | review-checklist §3 (TypeScript/Vue), §5 (Vue Component), §6 Grid Column Type Matrix |
| 3 | Step 10 (final, before validation) | All /working/ artifacts | review-checklist §2 Performance, §7 Modernization, full parity completeness |

The actual checkpoint protocol (PASS / DRIFT FOUND / ESCALATE format) is documented in `references/checkpoint-protocol.md`. Do not duplicate it here.

---

## §6 Open Issues / Blockers

Anything unresolved at plan-approval time. Drop the section if there are none.

Examples:
- A clarifying question the user deferred ("decide later: should the deprecated 'priority' field be carried forward?")
- A sibling-block edit the user said "skip for now", flag here so /review-conversion sees it as an open thread
- A framework concern the model wants surfaced before merge ("the new `tooltip` prop on `PanelAction` should get a separate review")

---

## What this template does NOT contain

- The functional parity table (lives in `parity-map.md`)
- The improvement analysis (lives in `improvement-analysis.md`)
- The redundancy report (lives in `redundancy-report.md`)
- Test scenarios (live in `test-scenarios.md`)
- Per-method code sketches (lives in the canonical reference; cite, don't copy)

Plan.md is a guide, not a transcript of the research. If a decision needs extensive justification, that justification belongs in the matching /working/ artifact (typically `obsidian-pattern-analysis.md` or `improvement-analysis.md`); plan.md states the decision and cites.
