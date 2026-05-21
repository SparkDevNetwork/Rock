# Conversion Spec Format

The compaction format for archiving a `/working/{block-name-kebab}/` folder as a frozen spec under `/specs/completed/{Domain}/`. Borrows conventions from the `/spec` skill but does the archival itself (since `/spec` is built for from-scratch authoring or completing an existing spec; we have post-hoc compaction of /working/ artifacts).

This file is loaded only after `/review-conversion` returns PASS and the user opts to archive (Phase 5 of SKILL.md).

---

## Conventions borrowed from /spec

- **Filename format:** `YYMMDD-convert-{block-name-kebab}.md`
- **Date stamp:** current local date, day precision. Get with `date +%y%m%d`.
- **Folder destination:** `/specs/completed/{Domain}/` where `{Domain}` is the lowercase folder form per `.claude/rules/rock-domains.md` (`cms`, `core`, `crm`, `lava`, etc.).
- **INDEX.md location:** `/specs/completed/INDEX.md`. Single index across all domain folders.
- **INDEX.md row format:** `| Spec | Domain | Author | Summary | Commit |`. The `Domain` column uses release-note casing (`CMS`, `Core`, `CRM`); the folder name uses lowercase casing.
- **Frozen-after-completion rule:** never edit the body of an archived spec. The historical record is the point.

If `/spec` evolves (filename format changes, frontmatter shape changes, INDEX.md schema changes), update **this file** to track. Convert-block does its own archival, but the conventions are /spec's.

---

## What convert-block does itself

The /working/ folder is too detailed for a spec body but too valuable to discard. Convert-block writes a compacted `spec.md` file that **summarizes** the conversion and **points to** the preserved /working/ artifacts.

### Filename

`YYMMDD-convert-{block-name-kebab}.md`

Example: `260504-convert-content-channel-item-detail.md`

### Compacted spec.md body

The body is short. Detail lives in /working/. Format:

```markdown
---
author: {git config user.name}
date_created: YYYY-MM-DD
summary: >-
  Converted the {Category}/{BlockName} WebForms block to Obsidian (Vue 3 + C# RockBlockType).
  {One sentence on the most notable improvement applied or design decision made.}
contributors: []
---

# Convert {Category}/{BlockName} to Obsidian

## Summary

Two or three sentences. What block was converted, classification, line count of the original `.ascx.cs`, scale of the conversion (small / medium / large in v2 terms, i.e., did Phase 1B fan-out fire). One sentence on the headline improvement.

## Key decisions made during conversion

A bulleted list, ~5-10 items. Each cites the matching Q in /working/clarifying-questions.md. Format:

- {short statement of the decision} (cite: clarifying-questions.md Q{N})

Examples:
- View bag and edit bag split: API keys, OAuth credentials, and raw template content excluded from view-mode response (cite: clarifying-questions.md Q1)
- Tabbed content: replaced manual nav-tabs with `<TabbedContent>` to preserve URL query-param sync (cite: clarifying-questions.md Q3)
- Sibling block fix: updated WebForms `ContentChannelDynamicView.ascx.cs` to accept idKey, in-scope (cite: clarifying-questions.md Q2)

## Improvements applied

A bulleted list of the P0 and P1 rows from /working/improvement-analysis.md that were fixed in this conversion. Each cites the row ID. Format:

- {Severity} {short description} (cite: improvement-analysis.md I{N})

Examples:
- P0 view/edit bag split (cite: improvement-analysis.md I1)
- P0 cross-block IdKey resolver added to sibling WebForms block (cite: improvement-analysis.md I2)
- P1 N+1 query in BindGrid replaced with single batched fetch (cite: improvement-analysis.md I3)
- P1 hand-rolled tabs replaced with `<TabbedContent>` (cite: improvement-analysis.md I7)

P2 rows can be batched: "Standard modernization sweep applied: string interpolation, early returns, null-conditional access, dead-code removal."

## Carried-forward behaviors that may warrant follow-up

A bulleted list of behaviors that were preserved verbatim despite being suboptimal, with rationale. These become starting points for future bugfix or improvement work. Cite /working/edge-cases.md or improvement-analysis.md "deferred" rows. Drop the section entirely if there are none.

Examples:
- Hardcoded `CategoryId == 5` filter preserved per Phase 2 user decision; documented in clarifying-questions.md Q4 as an organization-wide convention. Future readers should validate that convention before changing.
- Inline styles in editPanel were flagged but deferred to /css-cleanup post-conversion. 12 instances; see improvement-analysis.md I9.

## Supplementary material

The full /working/ folder is preserved at `./working/` (relative to this spec). It contains:

- `parity-map.md`, 7-trace functional parity table; verdict columns filled by `/review-conversion`
- `state-machine.md`, UI states + transitions
- `logic-graph.md`, call graph + conditional flows
- `data-model.md`, entities, FKs, queries, sibling-block scan, view/edit field split, C# enum values
- `completeness-analysis.md`, implicit / hidden behavior captured in second sweep
- `improvement-analysis.md`, full list of inefficiencies / improvements applied (P0/P1/P2)
- `redundancy-report.md`, duplicate / dead / hand-rolled-where-utility-exists code dropped or consolidated
- `edge-cases.md`, null cases, error paths, boundary conditions
- `obsidian-pattern-analysis.md`, idiomatic Obsidian shape; alternatives + rationale
- `clarifying-questions.md`, Phase 2 Q&A audit trail (preserves user design decisions)
- `test-scenarios.md`, behaviors that must verify post-conversion
- `plan.md`, distilled implementation guide (the lean bridge plan)
- `review-findings.md`, `/review-conversion` audit summary written post-implementation (if /review-conversion was run before archival)

Most of these artifacts also carry a `## Verification (review-conversion, ...)` section appended by `/review-conversion` recording row-by-row audit verdicts. parity-map.md additionally has its "Obsidian Equivalent (planned)" and "Verdict (planned)" columns filled in. Together with `review-findings.md`, these constitute the audit trail for the conversion.

These artifacts are the historical record. Do not edit them after archival, the whole point is to preserve what the analysis and the audit found at the time of conversion.
```

---

## Move and index steps

After writing the spec.md body:

### 1. Move the /working/ folder

Source: `/working/{block-name-kebab}/`
Destination: `/specs/completed/{Domain-lowercase}/{filename-stem}/working/`

Where `{filename-stem}` is the spec filename without the `.md` extension. Example: spec file `260504-convert-content-channel-item-detail.md`, stem `260504-convert-content-channel-item-detail`.

```bash
# If /working/ is tracked by git, use git mv:
git mv working/content-channel-item-detail specs/completed/cms/260504-convert-content-channel-item-detail/working

# Otherwise plain mv:
mv working/content-channel-item-detail specs/completed/cms/260504-convert-content-channel-item-detail/working
```

(Whether `/working/` is gitignored is a project decision; check with `git check-ignore working/{block-name-kebab}` if unsure.)

Verify the move succeeded before updating the index. The /working/ folder must be empty at the source (or removed entirely) and fully present at the destination.

### 2. Append to /specs/completed/INDEX.md

If INDEX.md does not exist, create it with the standard /spec header:

```markdown
# Completed Specs Index

This index lists every spec that has been moved into `specs/completed/`. It is maintained by the `spec` and `convert-block` skills, please do not edit by hand.

| Spec | Domain | Author | Summary | Commit |
|------|--------|--------|---------|--------|
```

Append one row at the bottom:

```markdown
| [Convert {Category}/{BlockName} to Obsidian]({domain-lowercase}/{filename}) | {Domain release-note casing} | {Author} | {Summary from frontmatter} | |
```

The Commit column stays empty unless the user provides a hash. Convert-block does NOT prompt for the commit hash at archival time (the conversion may not be merged yet); the user can fill it in later or leave it blank.

### 3. Confirm

After moving and indexing, state:
1. The new spec path
2. That INDEX.md was updated
3. That the /working/ folder is now archived under the spec
4. Stop. Do not paste the spec or index back into chat.

---

## What this archival does NOT do

- **Does not invoke /spec.** /spec is for from-scratch authoring or marking an existing spec complete. Convert-block does its own archival.
- **Does not edit any /working/ artifact.** The artifacts are frozen at archival.
- **Does not commit.** The user runs `/commit` separately if they want the archive committed.
- **Does not collapse /working/ into the spec body.** The /working/ artifacts persist under the spec; the body is a navigation surface, not a replacement.

---

## What if the user picks "Not yet" or "No"?

If the user picks "Not yet", do nothing. The /working/ folder stays at the repo root. The user can run the archival steps manually later (re-invoke `/convert-block` and ask for archival, or do it by hand following this file).

If the user picks "No", do nothing. The user can `rm -rf working/{block-name-kebab}/` whenever they want.

Either way, the conversion is still considered done. Archival is optional.
