# Checkpoint Protocol

Each checkpoint is a self-verification gate during implementation. The point is to catch drift before it compounds across multiple files.

The checklist sections are imported from `review-conversion/references/review-checklist.md`. Do not invent new criteria; if you find a gap, edit `review-checklist.md` and the change flows back here.

---

## When checkpoints fire

The checkpoint count for a block is set in `plan.md` §5 based on `.ascx.cs` size:

| Size | Checkpoints | Fires after |
|---|---|---|
| < 500 lines | 1 | Step 10 (final, before validation) |
| 500-1500 lines | 2 | Step 6 (C# block); Step 10 |
| 1500-2500 lines | 3 | Step 6; Step 8 (.obs + partials); Step 10 |
| 2500+ lines | 4 | Step 6; Step 7 (.obs root); Step 8 (partials); Step 10 |

---

## What each checkpoint reads

| Checkpoint | /working/ artifacts loaded | review-checklist.md sections run |
|---|---|---|
| After C# block written | `parity-map.md`, `data-model.md`, `improvement-analysis.md` | §1 Functional Parity, §3 Bug Patterns (C#), §5 Obsidian C# Block Structure |
| After .obs / partials written | `parity-map.md`, `state-machine.md`, `obsidian-pattern-analysis.md` | §3 (TypeScript/Vue), §5 (Vue Component), §6 Grid Column Type Matrix (list blocks only) |
| Final checkpoint | All /working/ artifacts | §2 Performance, §7 Modernization Checks, full §1 parity completeness |

Read **only** the listed artifacts. Reading plan.md at a checkpoint defeats the purpose; plan.md was the bridge to writing the code, the /working/ artifacts are the source of truth for verifying it.

---

## Protocol

For each checkpoint, in order:

### 1. Read the relevant /working/ artifacts

Per the table above. Skim, don't memorize. Take notes on what you're looking for in the code.

### 2. Read the relevant review-checklist.md sections

`.claude/skills/review-conversion/references/review-checklist.md`. Run only the listed sections.

### 3. Walk the code written so far

Read each file produced in this implementation phase end to end. For each /working/ artifact row that should be reflected in this code (parity-map row, improvement-analysis item, etc.), find it in the code or note it as missing/different.

While walking the code, also grep the new production files for `/working/` artifact identifiers and filenames. None should appear. The `/working/` folder is research scaffolding — it gets archived under `/specs/completed/` (or deleted) once `/review-conversion` passes, after which any `// I12`, `// Per N2`, `// (improvement-analysis.md I16)`, `// Per audit B1 / B2 / B10`, etc. citations become orphan references future readers can't resolve. Treat any hit as **DRIFT FOUND** and rewrite the comment to explain the underlying intent, or delete it. See `references/common-patterns.md` § Comments in Converted Code → "Forbidden: /working/ artifact identifiers" for the exhaustive forbidden-pattern list and rewrite examples.

```bash
# Grep template — run with the actual block name and category:
grep -nE 'improvement-analysis|parity-map|new-features|figma-design|state-machine|obsidian-pattern|redundancy-report|completeness-analysis|edge-cases|working/|\bN[0-9]+\b|\bI[0-9]+\b|\bQ[0-9]+\b|\bS[0-9]+\b|\bB[0-9]+\b|\bR[0-9]+\b|\bT[0-9]+\b|\bM[0-9]+\b|\bU[0-9]+\b|\bZ[0-9]+\b' \
    Rock.Blocks/[Category]/[BlockName].cs \
    Rock.JavaScript.Obsidian.Blocks/src/[Category]/[blockName].obs \
    Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/*.obs \
    Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/*.ts \
    Rock.ViewModels/Blocks/[Category]/[BlockName]/*.cs
```

The grep should return **no output**. (Naming a TS-side enum value like `ContentChannelDateType.SingleDate` is fine — that's a real cross-language type, not a `/working/` row ID. The forbidden case is `// per Q3`, `// I12 …`, `// Trace 4 row T8-1` style citations.)

Also grep the new `.obs` and `.ts` files for single-line JSDoc; the convention is multi-line even for short descriptions, per `references/common-patterns.md` § Comments in Converted Code → "JSDoc style for `.obs` and `.ts` files":

```bash
grep -nE '/\*\*[^*]+\*/' \
    Rock.JavaScript.Obsidian.Blocks/src/[Category]/[blockName].obs \
    Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/*.obs \
    Rock.JavaScript.Obsidian.Blocks/src/[Category]/[BlockName]/*.ts
```

This grep matches `/** anything-not-asterisk */` on one line — i.e. the single-line `/** Foo. */` form. It should return no output. Any hit is **DRIFT FOUND**: rewrite into the three-line `/**\n * Foo.\n */` form.

### 4. Run the cross-cutting checks (final checkpoint only)

These four are recurring conversion failure modes seen across past blocks. The final checkpoint is the moment to enforce them; earlier checkpoints can include them if applicable to the code written so far.

- **View / edit bag split (P0).** Confirm view-mode bags do NOT carry sensitive fields (API keys, secrets, OAuth credentials, raw template strings, picker selections). Cross-reference `data-model.md`'s view-safe vs edit-only field split.
- **Cross-block ID format mismatch (P0).** Confirm any list-to-detail navigation uses idKey format AND the linked-to detail block (whether it's the new Obsidian block or a still-WebForms sibling) accepts idKey. Cross-reference `data-model.md`'s sibling-block scan.
- **Read source for cross-language types (P1).** For any TS-side enum, first check whether the C# source lives in `Rock.Enums/` and whether `Rock.JavaScript.Obsidian/Framework/Enums/[Domain]/[enumName].ts` already exists — if it does, the new code MUST import from `@Obsidian/Enums/[Domain]/[enumName]` instead of redeclaring the enum in `types.partial.ts`. The auto-generated TS file ships matching `*Description` maps and a type alias the local copy can't replace. Only when the C# enum lives outside `Rock.Enums/` (or genuinely has no TS twin) is a local re-declaration appropriate, and even then the values must match the C# source byte-for-byte. See `references/common-patterns.md` § Enum Management → "Always import existing TS enums; never redeclare them in `types.partial.ts`".
- **Deduplication sweep (P1).** Search the generated files for converters or helpers written more than once. Common candidates: `*ToListItem`, `*FromListItem`, normalization functions, type guards. If any helper appears in two files, extract to `utils.partial.ts` (or a similar shared file) before the final checkpoint passes.

### 4.5. Run the compile-and-typecheck gate (every checkpoint, blocking)

A checkpoint cannot return PASS while the code emits any compile or type error. **Lint is necessary but not sufficient — ESLint silently allows whole categories of failures (`StandardListItemBag` mismatches, prop type widening, missing-await `Promise<T>` lvalues, control v-model contracts) that the TypeScript compiler catches.** Run the relevant gate for the code written in this phase BEFORE evaluating the verdict in step 5. If any error appears, treat the checkpoint as **DRIFT FOUND**, fix every error, then re-run this step.

| Checkpoint | Required commands (must report 0 errors) |
|---|---|
| After C# block written (Step 6) | `dotnet build Rock.Blocks/Rock.Blocks.csproj -v:q` (only the Cms-related warnings should be the pre-existing ones — your block must contribute 0 new errors / 0 new warnings) |
| After .obs / partials written (Step 8) | `npx eslint src/[Category]/[blockName].obs src/[Category]/[BlockName]/` AND `npx vue-tsc -p src/[Category]/tsconfig.json --noEmit --pretty false 2>&1 \| grep -i [BlockName]` (run from `Rock.JavaScript.Obsidian.Blocks/`) — both must produce no output for the new files |
| After Step 8.5 (framework edit) | The Step-8 commands AND `npx vue-tsc -p Framework/Controls/tsconfig.json --noEmit --pretty false` (run from `Rock.JavaScript.Obsidian/`) — every consumer of the edited framework control must still type-check clean |
| Final (Step 10) | `dotnet build Rock.Blocks/Rock.Blocks.csproj -v:q` AND `npx vue-tsc -p src/[Category]/tsconfig.json --noEmit` AND (if a framework file was edited) `npx vue-tsc -p Framework/Controls/tsconfig.json --noEmit` — zero errors across the board, including any non-block files that import the new bags / partials |

**Common type-checker findings ESLint misses on Obsidian work:**
- `ButtonGroup` / `RadioButtonList` / `CheckBoxList` — `items` is typed as `StandardListItemBag[]` (non-null `text` and `value`). Annotating an items literal with `: ListItemBag[]` widens to `string | null | undefined` and the picker rejects it. Drop the explicit annotation and let TS infer the literal type.
- `DefinedValuePicker` / `ListBox` / similar — `modelValue` is `ListItemBag | ListItemBag[] | null`, not `string[]`. A bag field declared as `string[]` of guids needs an in/out converter at the picker boundary.
- C# `Guid?` → TS `Guid | null`, but most framework controls accept `Guid | undefined`. Coerce with `?? undefined`.
- C# `string` → TS `string | null | undefined`, but framework controls often accept only `string | undefined`. Coerce with `?? ""` or `?? undefined`.
- A `<TagList v-model="x">` (or any framework component without a declared `modelValue` prop) silently does nothing. The component must expose `defineExpose` or emit-style hooks for the parent to read its state.

If you wrote framework code (Step 8.5) you also need to check downstream consumers. The framework type-check command compiles every `.obs` that imports the edited control, so an additive change that breaks a different block surfaces here.

### 5. Pick a verdict

| Verdict | Meaning | Action |
|---|---|---|
| **PASS** | Code matches every applicable /working/ artifact row + every applicable checklist section | Continue silently. Do not surface the checkpoint result to the user unless they ask. |
| **DRIFT FOUND** | Code diverges from the analysis or violates a checklist item | Surface the specific drift to the user as numbered findings. Apply fixes inline if low-risk; ask if any fix is non-obvious. Re-run the checkpoint after fixing. |
| **ESCALATE** | The drift suggests the analysis itself is wrong, or there's an architectural decision the user needs to make | Stop. Surface the issue. Do not patch around it. |

The verdict format for surfacing DRIFT FOUND or ESCALATE:

```
[Checkpoint #N] DRIFT FOUND
File: [path]:[line]
Source: /working/[artifact].md row [X] / review-checklist §[Y]
Detail: [one or two sentences]
Fix: [the specific change planned, OR a question to the user if non-obvious]
```

---

## Framework-edit gate (always on)

Independent of the checkpoint count above, **every** edit outside the block's own folders triggers a pause-and-confirm before the file is written. Block-owned folders are:

- `Rock.Blocks/[Category]/`
- `Rock.ViewModels/Blocks/[Category]/`
- `Rock.JavaScript.Obsidian.Blocks/src/[Category]/`
- `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/[Category]/`

Anything else (a sibling WebForms block, a framework `.d.ts`, a base control template, a system migration) is a separate concern with separate review.

The pause format:

> This conversion needs to edit `{path}` (outside the block's own folders) because `{reason}`. Proceed?
> 1. Yes, edit it as part of this conversion
> 2. Skip, flag in plan.md §6 Open Issues for a follow-up PR
> 3. Cancel and ask me to design around it

**Do not silently edit a framework file even when the analysis recommended it.** Plan.md §3 should already list the file under "Files to edit" if the design knew about it; the gate fires either way as the final confirmation.

---

## What checkpoints are NOT

- Not a place to redesign the block. If the analysis was wrong, fix the analysis (update the /working/ artifact) and ask the user before re-implementing.
- Not a substitute for `/review-conversion`. /review-conversion runs after all files are written and uses the same parity table /working/ produces. Checkpoints catch drift early; review-conversion is the post-flight inspection.
- Not user reviews. The user only sees the result if it's DRIFT FOUND or ESCALATE. PASS is silent.
- Not exhaustive. Each checkpoint runs a focused subset. The final checkpoint runs the full set; earlier ones run only what's relevant to the code written so far.

---

## Calibration

If a checkpoint produces drift findings that turn out to be false positives:

- Was it comparing against the wrong artifact? Fix the routing in this file (the table at the top).
- Was the artifact itself wrong? Fix the template under `references/working/` so future blocks generate a better artifact.
- Was the checklist section too broad? File feedback against `review-conversion/references/review-checklist.md`.

If a checkpoint passes but `/review-conversion` later finds drift:

- Was the artifact missing the row that would have caught it? Fix the /working/ template.
- Was the checkpoint not reading the right artifact? Fix the routing.
- Was it a class of issue not caught anywhere? Add a new entry to the cross-cutting checks at step 4 above.
