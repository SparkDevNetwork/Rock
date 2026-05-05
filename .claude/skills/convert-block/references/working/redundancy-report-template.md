# redundancy-report.md Template

Duplicate / dead / hand-rolled-where-utility-exists code that the conversion will drop or consolidate. Distinct from `improvement-analysis.md` (which is about *improving* surviving code), this file is about code that goes away.

This artifact is **always full**, even on small blocks. Legacy code accretes redundancy at every size.

---

## Output location

`/working/{block-name-kebab}/redundancy-report.md`

After the conversion ships, `/review-conversion` appends a `## Verification (review-conversion, ...)` section to this file with row-by-row Applied / Missed / Partial verdicts. That section is review's territory — do not pre-populate it during convert-block phases.

---

## Body

### Redundancies

A numbered list. Each row covers one redundancy:

```
R1, Custom BuildEnumListItemBags<T>() helper
   Source: parity-map.md M14 (helper method)
   Location: original lines 412-428
   Replaces with: typeof(T).ToEnumListItemBag()
   Effort: small (one-line replacement at 3 call sites)
```

Required fields per row:
- **#** stable ID (`R1`, `R2`, ...)
- **Source** which artifact / parity-map row this responds to
- **Location** lines in the `.ascx.cs` (or `.ascx` for markup redundancies)
- **Replaces with** the framework API or shared helper that supersedes it (or "delete entirely; nothing replaces it" for dead code)
- **Effort** small / medium / large (most redundancies are small)

Categories of redundancy (use as prompts):

#### Dead code

- Methods defined but never called
- Properties declared but never read
- `using` statements not actually used
- Commented-out blocks (delete them; git history preserves)
- Empty `#region` wrappers

For each: cite line number, confirm it's truly dead by grepping the codebase (the helper isn't called from another block), and document deletion.

#### Duplicate logic

- The same converter / formatter / validator written in two or more places
- The same query expression repeated across methods
- The same conditional check duplicated rather than extracted

For each: name the duplicate, list the locations, propose the consolidation point (`utils.partial.ts`, a private helper on the C# block, or `Service` extension if it's a query).

#### Hand-rolled where framework exists

Common examples:
- Custom `BuildEnumListItemBags<T>()` → `typeof(T).ToEnumListItemBag()`
- Custom `GetAllCategoriesCached()` → `CategoryCache.All(RockContext)`
- `Dictionary<int, string>` per-request cache for a single-lookup key → just look it up
- `.JoinStrings(string.Empty)` → `string.Concat()`

For each: cite the framework API that replaces it, confirm the API exists in Rock (grep the codebase), and document the swap.

#### v-model adapter duplication (Vue side)

When the same converter (e.g., `listItemToPageRoute` / `pageRouteToListItem`) is written inline in 3+ panels:

- List the panels it appears in
- Propose the shared file (`utils.partial.ts` is the convention)
- Note: the deduplication sweep at the final checkpoint catches this if it's missed during writing

#### Block-action plumbing

- Block actions whose response a separate `Refresh` action could fold into
- Multiple block actions doing the same thing with slight variation (Save vs SaveAndRedirect vs SavePartial, usually one block action with a flag)

For each: name the actions, propose the consolidation, note any callers that need updating.

#### Markup redundancy (in the `.ascx`)

- Repeated panel structures that could become a partial
- Duplicate validation summaries
- Multiple update panels updating the same target

These usually evaporate in conversion (Vue components naturally consolidate this), but flag any that don't.

### Out-of-scope items

If the analysis surfaces redundancy that crosses block boundaries (the same dead helper exists in 4 blocks), do NOT delete from the others as part of this conversion. Use `mcp__ccd_session__spawn_task` to file the cleanup separately and note the spawn here.

---

## Quality checks

- [ ] Every v-model adapter candidate (same converter inline in 3+ panels) has a row, OR the block has only one panel and the deduplication sweep is moot
- [ ] Every framework-API rediscovery candidate (hand-rolled helper overlapping with existing Rock API) has a row
- [ ] Dead code rows have been confirmed by grepping the codebase, not assumed
- [ ] Out-of-scope cleanups are spawned, not silently dropped

---

## What this is NOT

- Not a refactor list. Refactors that preserve all the code go in `improvement-analysis.md`.
- Not a list of "things WebForms shouldn't have done." Those go in `improvement-analysis.md` too. This file is for code that the Obsidian version simply will not have.
