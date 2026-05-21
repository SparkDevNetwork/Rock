---
name: convert-block
description: >-
  Converts a Rock RMS WebForms block (.ascx/.ascx.cs) to Obsidian (Vue 3 + C# RockBlockType).
  Triggers on: "convert block", "migrate block to obsidian", "obsidian conversion", or a
  WebForms block path like "RockWeb/Blocks/Core/Foo.ascx.cs" with conversion intent.
  Also handles attached Figma design URLs (translation + redesign) and net-new feature
  scope ("add inline comments", "also support drag-to-reorder", etc.) when the user
  describes them alongside the conversion target.
  Produces a `/working/{block}/` research workspace, classifies the block as Detail/List/Custom,
  generates the C# block + bags + .d.ts placeholders + Vue SFC + partials, chops the WebForms
  files, and offers to archive the research as a frozen spec once `/review-conversion` passes.
  Do NOT use for non-block tasks, creating new blocks from scratch, or general Rock RMS questions.
---

**CRITICAL: Enter plan mode immediately.** Do all research first, ask clarifying questions, write a plan, and get user approval before writing any files.

You are converting a Rock RMS ASP.NET WebForms block to the Obsidian framework.

Quality is more important than speed. Read the full WebForms block carefully before classifying. Do not skip validation steps or abbreviate generated file contents.

The block to convert is: **$ARGUMENTS**

If `$ARGUMENTS` contains a Figma URL, treat it as the redesign source for this conversion (see Phase 1A.0). If it contains phrasing about new functionality, treat that as new-feature scope.

---

## Conversion Philosophy

**You are not a 1:1 translator.** The WebForms block is a _requirements reference_, not a code template. Your job is to produce an idiomatic Obsidian block that delivers the same _behavior_, not to replicate the same _implementation_.

**When WebForms and correctness conflict, correctness wins.** If the WebForms code has bugs, N+1 queries, missing null checks, service calls where cache exists, or patterns that don't apply to Obsidian, fix them. Do not carry forward problems just because "the original did it this way."

The full catalog of patterns to preserve, patterns to improve, and forbidden carry-forwards lives in `references/common-patterns.md` (loaded after Phase 1A classification). Do not duplicate that content here.

---

## The /working/ Workspace

Research depth is the value of v2. Every research artifact lives under `/working/{block-name-kebab}/` at the repo root. The folder is created in Phase 1A and persists through implementation, review, and optional archival to `/specs/completed/{Domain}/`. (Whether `/working/` is gitignored is up to the project; the skill does not assume either way.)

The artifacts are produced once and consumed three times:

1. **Plan composition** (Phase 3): the model authoring plan.md cites them.
2. **Checkpoint verification** (during implementation): each checkpoint reads the relevant subset to verify the code matches the analysis.
3. **Post-conversion audit** (after `/review-conversion`, and any time later): truth-of-record about what was found, what was decided, and what was deferred.

The full artifact list:

```
/working/{block-name-kebab}/
├── parity-map.md                ← functional parity table (7 traces; 8 if new features)
├── state-machine.md             ← UI states + transitions
├── logic-graph.md               ← call graph, conditional branches, data flow
├── data-model.md                ← entities, FKs, queries, sibling-block scan
├── completeness-analysis.md     ← second-sweep findings, hidden behavior
├── improvement-analysis.md      ← inefficiencies, suboptimal patterns, redesigns
├── redundancy-report.md         ← duplicate / dead / hand-rolled-where-utility-exists
├── edge-cases.md                ← null cases, error paths, what could break
├── obsidian-pattern-analysis.md ← idiomatic shape; alternatives + rationale
├── clarifying-questions.md      ← Phase 2 Q&A audit trail
├── test-scenarios.md            ← behaviors that must verify post-conversion
├── figma-design.md              ← only if a Figma URL was attached (translation + redesign)
├── new-features.md              ← only if scope includes net-new functionality
├── figma/                       ← screenshots referenced by figma-design.md (not embedded)
└── plan.md                      ← THE distilled implementation guide
```

**Adaptive depth.** Not every conversion produces every artifact. Templates are skeletons, not contracts. Use guidance language. Default to producing the artifact; collapse to a stub ("Not applicable for this block, see parity-map.md") when the block is genuinely simple along that dimension. See `references/working/*-template.md` for what each artifact covers.

---

## Reference Routing Table

Load reference files progressively, only when needed for the current phase.

| Reference File | Load When |
|---|---|
| `references/phase-1a-protocol.md` | Entering Phase 1A (always) |
| `references/common-patterns.md` | After Phase 1A classification (always) |
| `references/detail-block-patterns.md` | Block classified as **Detail** |
| `references/list-block-patterns.md` | Block classified as **List** |
| `references/custom-block-patterns.md` | Block classified as **Custom** |
| `references/working/parity-map-template.md` | Phase 1A.8 (always) |
| `references/working/figma-design-template.md` | Phase 1A.9 (only if a Figma URL was captured in 1A.0) |
| `references/working/new-features-template.md` | Phase 1A.10 (only if scope includes new features) |
| `references/phase-1b-protocol.md` | Entering Phase 1B |
| `references/working/{state-machine,logic-graph,data-model,completeness-analysis,improvement-analysis,redundancy-report,edge-cases,obsidian-pattern-analysis,test-scenarios}-template.md` | Phase 1B; each subagent reads only its own template |
| `references/improvement-checklist.md` | Phase 1B Improvement Analyst subagent |
| `references/phase-2-protocol.md` | Entering Phase 2 |
| `references/working/clarifying-questions-template.md` | Phase 2 (always, even when Phase 1B was skipped) |
| `references/plan-template.md` | Phase 3 |
| `references/checkpoint-protocol.md` | Phase 4, at the first checkpoint |
| `references/implementation-details.md` | Phase 4 Step 2 |
| `references/conversion-spec-format.md` | Phase 5 archival (only if user picks "archive now") |
| `references/troubleshooting.md` | When errors occur, patterns seem unsupported, or build fails |
| `references/examples.md` | Index of per-type examples; load when unsure about output format for any phase |
| `references/examples/{detail,list,custom,detail-with-figma}-example.md` | Per-type example; load only the file matching this conversion's classification (and Figma scope) |

`review-conversion`'s `references/review-checklist.md` is **referenced** by checkpoint-protocol.md and improvement-checklist.md, not duplicated. Load it at the same checkpoint protocol moment.

Do not read all reference files upfront. Read them as each phase requires.

---

## Scripts

| Script | When to use |
|---|---|
| `scripts/generate-guids.js` | Implementation, before writing the C# block file |
| `scripts/validate-conversion.js` | Implementation, quality gate |

---

## Phase 1A: Parity Map (always, sequential)

Foundation phase. Detects scope (translation, redesign, new features), classifies the block, scans for unsupported patterns and performance issues, and produces `parity-map.md` (plus `figma-design.md` and `new-features.md` when applicable).

Read `references/phase-1a-protocol.md` for sub-steps 1A.0-1A.10, the quality gate, and the presentation format.

After Phase 1A completes, decide whether to fan out to Phase 1B per the trigger in `references/phase-1b-protocol.md` § Trigger.

---

## Phase 1B: Parallel Research Fan-Out (adaptive)

Conditional phase. Fires for large blocks, blocks with multiple modes / many entities, blocks with redesign or new-feature scope, or on explicit "deep research" request. Spawns up to 8 subagents in parallel, each producing one structured artifact.

Read `references/phase-1b-protocol.md` for the trigger thresholds, subagent table (with always-run vs. skip-when classification), per-subagent briefing requirements, reconciliation step, and quality gate.

---

## Phase 2: Propose design and ask clarifying questions

Bridges research and the plan. Surfaces unsupported pattern callouts, required performance fixes, and a curated set of clarifying questions whose answers feed Phase 3.

Read `references/phase-2-protocol.md` for the design proposal structure, the always-ask question topics (with Figma + new-feature additions), the infer-don't-ask list, and the response format.

Wait for the user to answer all questions before continuing to Phase 3.

---

## Phase 3: Write and approve the plan

**The plan is the bridge from research to code.** It cites /working/ artifacts as sources rather than repeating them. Plan length scales with the block; the constraint is no-duplication-with-/working/, not a target line count.

Read `references/plan-template.md`. Write `/working/{block-name-kebab}/plan.md` using the §1-§6 structure documented there:

```
§1 Block summary                    (1-2 paragraphs)
§2 Key design decisions             (with citations into /working/clarifying-questions.md and /working/obsidian-pattern-analysis.md)
§3 Files to create / files to delete  (paths)
§4 Implementation steps             (the 10-step structure)
§5 Checkpoints                      (each cites which /working/ artifacts to verify against)
§6 Open issues / blockers
```

While writing the plan, add a **per-trace architectural summary** to `parity-map.md` directly under its header, above the trace tables. One bullet per trace (1 through 7, plus 8 if new features are in scope), each capturing how that trace's rows collapse into Obsidian patterns at the architectural level — not row by row. Example: "Trace 1 (Methods M1-M47): `OnInit`/`OnLoad` collapse into `GetObsidianBlockInitialization` + `GetEntityBagForView`/`GetEntityBagForEdit`. `lbSave_Click` → `Save` block-action with the post-save chain preserved (slugs, tags, personalization, intents, occurrence join). Child/parent grid handlers → grid emits + matching block actions."

The row-by-row "Obsidian Equivalent (planned)" column stays empty here. `/review-conversion` fills it in post-implementation while walking the actual code, which produces a more honest mapping than a Phase-3 prediction.

### Phase 3 Quality Gate

Before presenting the plan, verify:
- [ ] Every file has a full, concrete path (no unresolved placeholders)
- [ ] All user answers from Phase 2 are recorded in `clarifying-questions.md` and reflected in the plan
- [ ] Unsupported patterns have documented replacements
- [ ] Implementation §4 includes all 10 steps with block-specific values filled in
- [ ] Canonical reference file paths are listed explicitly
- [ ] §5 checkpoints are present (count per `references/checkpoint-protocol.md` "When checkpoints fire") and each cites the /working/ artifacts to read
- [ ] `parity-map.md` has a per-trace architectural summary (1 bullet per trace) directly under its header. Row-level "Obsidian Equivalent (planned)" cells stay empty for `/review-conversion` to fill post-implementation.

**CRITICAL: Do not write a single block file until the user explicitly approves this plan.**

---

## Phase 4: Implementation (after plan approval)

Exit plan mode. Follow plan.md §4 step by step. The 10-step structure is defined in `references/plan-template.md` §4 and instantiated for this block in plan.md §4. Do not improvise; execute the steps as written.

After each implementation step that triggers a checkpoint per plan.md §5, **pause and run the checkpoint protocol** documented in `references/checkpoint-protocol.md`. The protocol covers checkpoint count by block size, what each reads, the PASS/DRIFT FOUND/ESCALATE verdict format, the four cross-cutting checks (view/edit bag split, cross-block ID format, read-source for cross-language types, deduplication sweep), the compile-and-typecheck gate (a hard prerequisite for PASS — see § 4.5), and the framework-edit gate.

**A checkpoint cannot return PASS while compile or type errors exist.** Lint passing is not enough; ESLint allows whole categories of failures (control-prop type widening, picker `modelValue` shape mismatches, `Guid?`/`null` coercions) that only the compiler catches. Run the gate commands listed in `references/checkpoint-protocol.md § 4.5` before scoring the verdict — the C# build for any C# changes, `vue-tsc` for any `.obs`/`.ts` changes, plus the framework-control type-check whenever a file in `Rock.JavaScript.Obsidian/Framework/` was edited.

### After Step 10 (validation passes)

In order:

1. **Run Rock.CodeGeneration** (WPF app at `Rock.CodeGeneration/`) to regenerate `.d.ts` files from C# ViewModels. The conversion is not done until this is run and the auto-generated `.d.ts` matches the bags 1:1.
2. **No migration is needed for the chop.** `BlockTypeService.StagePossibleMigrateWebFormsToObsidianBlock` rewrites the existing WebForms `BlockType` row at Rock startup whenever it finds the new entity-based class carries the WebForms block's GUID on its `[BlockTypeGuid]` attribute. Confirm the C# class's attribute matches the WebForms `[BlockTypeGuid]` per `references/common-patterns.md` § GUID Assignment Rules; do not write `AddOrUpdateEntityBlockType` or any other registration migration.
3. **Run `/review-conversion`** to verify functional parity. v2 of that skill verifies code against every applicable /working/ artifact, fills the parity-map.md verdict columns, appends `## Verification (review-conversion, ...)` sections to the other artifacts (improvement-analysis.md, redundancy-report.md, etc.), and writes a `review-findings.md` summary alongside them. Wait for the report's verdict to be PASS (or PASS WITH NOTES with no material findings) before moving on.
4. **If `improvement-analysis.md` flagged inline styles** or hard-coded design tokens, run `/css-cleanup` next.
5. **Phase 5 archival** (below) becomes available once `/review-conversion` passes.

---

## Phase 5: Post-conversion archival

After `/review-conversion` returns PASS, offer to archive the /working/ folder as a frozen spec.

Read `references/conversion-spec-format.md` for the compaction format and conventions.

Offer:
> The /working/{block-name-kebab}/ folder captures the full research. Want me to archive it as a frozen spec under /specs/completed/{Domain}/?
> 1. Yes, archive now
> 2. Not yet, wait until the PR merges
> 3. No, leave it at /working/ (or delete)

If the user picks **Yes**, follow `references/conversion-spec-format.md` for the spec.md body shape, the move, and the INDEX.md update.

If the user picks **Not yet** or **No**: leave /working/ in place. The user can manually archive later.

---

## Examples

See `references/examples.md` (the index) and the per-type example files under `references/examples/`. Load only the file matching this conversion's classification (and Figma scope, if present).
