---
name: docs
description: >-
  Author or update technical documentation in the repo's `docs/` directory. Docs are the
  living "as built" reference for how Rock currently works, organized by Rock domain
  (`docs/core/`, `docs/lava/`, `docs/group/`, etc.). Audience is core developers and
  quasi-technical community members. Use when the user says "write docs for X",
  "document this feature", "update the docs", "how is X documented", "audit the docs",
  or describes a feature/subsystem they want a written reference for. Also use when the
  spec skill hands off after completing a spec, or when the commit skill detects that
  changed files are referenced in some doc's frontmatter.
  Do NOT use for: writing end-user tutorials or how-to guides (different audience),
  generating API reference (separate tool), writing release notes, drafting RFCs or
  proposals (those are specs).
argument-hint: "Topic to document, OR a doc path to update/audit (e.g. 'group requirements', 'docs/lava/entity-commands.md')"
compatibility: Requires Claude Code CLI with access to the Rock RMS codebase.
metadata:
  version: "1.1"
  author: "Jon Edmiston"
---

# Documentation Authoring Workflow

You are creating or maintaining technical documentation in the repo's `docs/` directory.

**Argument:** $ARGUMENTS

---

## Audience and Scope

- **Audience:** core Rock developers, plugin authors, and quasi-technical community members. Assume the reader can read C# and SQL but does not yet know this corner of Rock.
- **Voice:** terse, opinionated, code-anchored. Prefer concrete file paths and method names over abstract description. Match the tone of the existing `.claude/rules/*.md` and skill files.
- **Time horizon:** "as built" — the current truth about how Rock works today. NOT a changelog and NOT a museum of past decisions.

### Specs vs Docs

| | Spec | Doc |
|---|---|---|
| Tense | "as designed at a point in time" | "as built today" |
| Mutability | Immutable after completion | Continuously updated |
| Audience | Reviewers of a proposed change | Engineers needing to understand current behavior |
| Lives in | `specs/` then `specs/completed/{domain}/` | `docs/{domain}/` |
| Linked from | Other specs (rare) | Source code, other docs, sibling specs |

If the request is really for a spec (capturing a decision before code lands), redirect to the `spec` skill. If it is really for end-user help, an API reference, or release notes, redirect appropriately.

---

## Decide the Mode

Read `$ARGUMENTS` and the conversation, then pick one of three modes:

- **Create / update mode** (default) — write a new doc or update an existing one. Steps 1 through 7 apply.
- **Audit mode** — given an existing doc path, verify its claims against current source code and report drift. Triggered by phrases like "audit the docs for X", "check if the docs are still accurate", "verify these docs against the code". Skip to **Audit Mode** at the bottom of this file.
- **Update-from-spec mode** — invoked by the `spec` skill in completion mode. The caller passes a completed-spec path and one or more candidate doc paths. Skip to **Update-From-Spec Mode** at the bottom of this file.

---

## Step 1 — Determine Topic, Domain, and Existing Coverage

### 1.1 Determine the topic

Read the conversation, recent diffs (if available), the branch name, and `$ARGUMENTS` to identify what is being documented. Do not invent topics. If the topic is genuinely unclear, ask one question and stop.

### 1.2 Pick the domain folder

The canonical list of domains, the path-to-domain mapping, and the translation between release-note casing and folder names live in `.claude/rules/rock-domains.md`. Use that file as the single source of truth. Do not duplicate or guess.

`docs/` uses the lowercase-kebab folder form (same convention as `specs/completed/`). The H1 of `docs/{folder}/README.md` uses the release-note form for human readability.

If the topic is genuinely cross-cutting and no single domain fits, use `other`. Do not invent new domain folders.

### 1.3 Search for existing coverage (mandatory)

Before writing anything new, search `docs/` for existing files that already cover this topic. Use both:

- `Glob` for filename matches in `docs/{domain}/` and across all `docs/`.
- `Grep` for keyword matches inside doc bodies (the topic may live under a different filename than expected).

Possible outcomes:

| Result | Action |
|---|---|
| One file already covers it well | Update mode. Use that file. |
| One file partially covers it | Update mode. Extend that file. |
| Multiple overlapping files | Stop. Surface the overlap to the user before proceeding; offer to consolidate or to scope the new file narrowly. |
| No existing coverage | Create mode. Proceed to Step 2. |

State the search result before writing or modifying any file. The user should know whether you are creating or updating, and why.

### 1.4 Identify cross-cutting concepts before writing

Some topics include concepts that are not specific to this domain but recur across many areas of Rock. These belong in `docs/core/` (or another genuinely cross-cutting folder), not embedded in every domain doc that touches them. Examples:

- Entity reference resolution (`GetQueryableByKey`, IdKey vs raw int, the "Disable Predictable Ids" site setting).
- PersonAlias semantics in audit columns and custom person FKs.
- `RockContext` lifecycle and lazy loading.
- Cache invalidation patterns.
- Lava data object conventions (`LavaDataObject`, naming with `Info` suffix).
- The Obsidian block lifecycle, security pattern, and bag conventions.

If the topic includes a cross-cutting concept:

1. Search `docs/core/` (and any other cross-cutting folder) for existing coverage.
2. If a core doc covers it, **link to it; do not re-explain it inline**. Domain docs link, they do not duplicate.
3. If no core doc covers it, **ask the user before writing** whether to (a) create the core doc first and link to it from the current doc, or (b) scope a brief inline explanation here this once. Default to (a) when the concept will obviously recur in other domain docs.

Heuristic: if you would write substantively the same paragraph in three different domain docs, it belongs in `docs/core/` instead. The IdKey vs raw-int discussion is a canonical example: every Obsidian block that resolves an entity reference faces the same question, so it lives once in `docs/core/entity-reference-resolution.md` and every block doc links to it.

---

## Step 2 — Read the Code (mandatory before any "How it works" content)

LLMs cheerfully invent plausible-sounding documentation that is wrong. **Do not infer mechanics from the conversation.** Before writing or updating any "How It Works", "Data Model", "Extension Points", or "Limitations" section, read the actual source files.

Minimum source reading:

- The primary entity / service / block class being documented.
- Any service class methods called out as part of the topic.
- The `using` graph one level out (the immediate dependencies).

Pull file:line references into the doc as you write. Every non-trivial mechanical claim in the doc should be traceable to a specific path in the codebase.

If the source has been refactored since the topic was last documented (e.g. the conversation references method names that no longer exist), say so and ask before proceeding.

### Sources for the "why"

Code answers "what." Engineering notes, commit messages, and specs answer "why." For any non-trivial behavior the doc describes, find the why and lead with it. The "Why It Exists" and "Key Architectural Decisions" sections of the doc live or die on this step.

Three primary sources, in order of usefulness:

1. **Engineering notes in the source files.** Rock convention is multi-line comments with the developer's name or initials (often a `Reason:` one-liner). Pattern from CLAUDE.md:

   ```
   /*
       3/5/26 - JE

       <Why this code exists or why this change was made.>

       Reason: <One-line summary for scanning.>
   */
   ```

   These are the highest-signal source. Grep `related_files` for blocks like this and quote or paraphrase the reason directly into the doc. The original phrasing is often better than anything you could rewrite.

2. **Recent commit messages on `related_files`.** Run `git log --format="%h %ai %s%n%n%b" -- {related_files}` and read the bodies, not just the subjects. Release-note commits (subjects starting with `+ (`) are written for an external audience and frequently include the why.

3. **Linked specs in `related_specs`.** Specs in `specs/completed/` capture the design intent and the alternatives that were rejected. The Motivation and Considered but Rejected sections of a spec map directly to the corresponding sections of the doc.

If you cannot find a why for a specific design choice, that is a finding worth surfacing to the user. Do not invent one. A doc that says "the design rationale is unclear, see {file:line}; consider asking {original author}" is more useful than a doc that confidently fabricates.

---

## Step 3 — Filename and Path

```
docs/{domain}/{topic-kebab}.md
```

- Domain folder: lowercase-kebab as defined above.
- File name: lowercase, hyphens for spaces. Strip leading articles. No timestamp prefix (unlike specs).
- Names should be **noun-phrases scoped to a feature**, not verbs and not entire subsystems.

Examples:

| Good | Bad |
|---|---|
| `docs/group/group-requirements.md` | `docs/group/groups.md` (too broad) |
| `docs/lava/entity-commands.md` | `docs/lava/how-to-use-lava.md` (verb, end-user voice) |
| `docs/core/cache-invalidation.md` | `docs/core/cache.md` (likely too broad) |
| `docs/finance/refund-flow-nmi.md` | `docs/finance/finance.md` (entire domain) |

If a file would exceed roughly 500 lines, split it. One file per coherent concept, not one file per domain.

---

## Step 4 — Write or Update the YAML Frontmatter

Every doc begins with this block:

```yaml
---
title: Group Requirements
last_updated: 2026-04-28
related_specs:
  - specs/completed/group/240118-group-requirement-caching.md
  - specs/completed/group/250903-group-requirement-override.md
related_files:
  - Rock/Model/Group/GroupRequirement.cs
  - Rock/Model/Group/GroupRequirementService.cs
  - Rock.Blocks/Group/GroupRequirementsList.cs
---
```

- `title` — the human-readable title. Mirrors the H1.
- `last_updated` — ISO-8601 date of this edit. Bump on every meaningful change. Get with `date +%Y-%m-%d`.
- `related_specs` — list of completed-spec paths **relative to the repo root**. Active specs (still in `specs/` root) MUST NOT appear here. Used by the auto-rendered Related Specs section, which computes the relative-to-doc form for the markdown link at render time. Omit the key if there are none.
- `related_files` — list of source-code paths **relative to the repo root** that this doc materially depends on. Used by the commit/audit hooks to detect when source has changed and the doc may need refresh, and as the scope for the `git log` query that sources Recent Impactful Changes. Omit the key if there are none.

Both lists store **repo-root-relative paths** in YAML so they are stable when docs move and so downstream tools (`git log`, `Glob`, audit-mode lookups) can consume them without translation.

---

## Step 5 — Write the Body

The structure below is **deliberately top-loaded with the why and the conceptual content**, and back-loads the dense reference material. A developer who reads only the top half should come away knowing why this exists, how to think about it, and the practical things that bite people. The technical reference at the bottom is for the developer actively writing code in the area.

The audience is core developers and quasi-technical community members. The voice is "what a senior engineer would tell a colleague who just walked into this corner of the codebase." Why first, concept second, gotchas third, code archaeology last.

Sections are recommended but optional. Include only the sections that apply. If a topic needs a section that is not on this list, add it.

```markdown
# {Title}

## Overview
{2-4 sentences. The 30-second answer to "what is this and why does it matter." A reader scanning the directory should know whether the rest of the doc is relevant after reading this paragraph alone.}

## Why It Exists
{2-4 sentences. The problem this system solves and the cost of not having it. Source the answer from engineering notes in the related source files, recent commit messages on those files, and the Motivation section of any linked specs (see Step 2's "Sources for the why"). Lead with the why; do not bury it under mechanics. Skip this section only if the why is genuinely obvious from the Overview, but err on the side of including it.}

## Mental Model
{How to *think about* this thing. The conceptual shape. Key distinctions, analogies, the "right frame" for reasoning about it. NO file paths in this section. NO line numbers. NO column lists. This is where a developer who has never touched this corner gets oriented in plain language. Two or three short paragraphs is usually enough.}

## What You Need to Know
{The practical rules of the road. The non-obvious behaviors that bite first-time visitors. The conventions a developer absolutely must internalize before they start changing code in this area. Use bold leads on each item or short subheadings when you have several distinct points. Each item should be the kind of thing you would warn a colleague about over coffee. Limit file:line references to the minimum needed to make the warning concrete; the dense references go in Technical Reference at the bottom.}

## Common Scenarios
{Optional. Pattern recipes for common tasks. "How do I add a member to a synced group", "How do I change a requirement type without re-evaluating", "How do I record attendance for a cancelled occurrence". Skip this section entirely if the topic does not have recurring patterns worth showing.}

## Key Architectural Decisions
{Why it is designed this way. Each decision: a short heading and the *why*. Source the why from engineering notes and commit messages whenever possible; quote them when the original phrasing is good. This section helps a developer decide whether to work with the grain of the design or push back on it.}

## Considered but Rejected
{Optional. Alternatives that have been evaluated and dismissed. For each:
- A short heading (the alternative).
- One-line description.
- Why it was rejected.
- A date in (YYYY-MM-DD) form **only if you can verify it from a spec, commit, or comment in source.** Omit the date otherwise; never approximate.

This section preempts re-litigation. Include whenever there were real alternatives, even briefly considered.}
```

After the template above, the doc continues with the Technical Reference section and the auto-rendered Recent Impactful Changes section. These are described below as prose rather than as nested template blocks, because both have authoring rules that are easier to read outside a `{...}` placeholder.

### Technical Reference

Everything dense, code-anchored, and reference-shaped goes here. A developer actively changing code in this area lives in this section; a developer just trying to understand the system can stop reading at the previous section.

Use subheadings to organize. Suggested subheadings (include only those that apply):

```markdown
## Technical Reference

### Data Model
{Entities, key columns, FKs and cascade behavior, indices. The dense table. File:line citations.}

### Save Hook Behavior
{Pre-save and post-save logic per state (Added, Modified, Deleted). Validation. Cache invalidation. Workflow triggers.}

### Service / API Surface
{Non-trivial public methods on the service classes. What they do, when to use them.}

### Caching
{How caching applies to this subsystem.}

### Affected Blocks and UI Surfaces
{Block paths (C# and Obsidian). Where this subsystem is consumed in the UI.}

### Extension Points
{Workflow triggers, Lava filters, plugin hooks, configuration attributes.}

### File Index
{A quick path index for the most-relevant files. Useful when a reader knows the topic but cannot remember where the code lives.}
```

Add or drop subheadings as the topic warrants. A doc on a pure data concept may not have "Save Hook Behavior". A doc on a UI surface may not have "Data Model". That is fine.

### Recent Impactful Changes

```markdown
## Recent Impactful Changes
```

At most 5 bullets, newest first.

**Mandatory: source every entry from `git log`. Never write a date you have not seen in git output.** If you cannot verify it, drop the entry. Do not paraphrase the date, do not write fuzzy values like "2024-late" or "early 2025", do not back-infer from issue numbers in commit subjects.

Recommended command (scopes to `related_files` from this doc's frontmatter):

```bash
git log -n 30 --format="%h|%ai|%s" --since="18 months ago" -- {related_files}
```

From that output, prefer release-note commits (subject begins with `+ (`). Each bullet:

```markdown
- **YYYY-MM-DD** ([commit `{short-hash}`](https://github.com/SparkDevNetwork/Rock/commit/{short-hash})). One-line summary, paraphrased from the commit subject. Append `(Fixes #NNNN)` only if the commit subject contains it.
```

If `git log` returns fewer than 5 release-note commits in the window, write fewer bullets. Three accurate entries beat five plausible ones. If there are zero, omit the section entirely.

### The split between "What You Need to Know" and "Technical Reference"

The most common authoring mistake is to dump every gotcha into a giant "Limitations" section at the bottom. Resist this. A gotcha that *every* developer working in the area must internalize belongs in **What You Need to Know** at the top, in plain language. A gotcha that only matters when you are actively touching a specific service method belongs in **Technical Reference** next to that method.

If you cannot decide where something belongs, put it in **What You Need to Know**. The cost of telling a developer something they already knew is small; the cost of letting them miss something that bites them is large.

---

## Step 6 — Render the Related Specs Section

If `related_specs` in the YAML frontmatter is non-empty, render this section at the **very bottom** of the doc, after every other section:

```markdown
## Related Specs

- [Group requirement caching](../../specs/completed/group/240118-group-requirement-caching.md) — 2024-01-18 (Daniel Hazelbaker)
- [Group requirement override](../../specs/completed/group/250903-group-requirement-override.md) — 2025-09-03 (Jon Edmiston)
```

Rules:

- Source the list from `related_specs` in the YAML. Do not duplicate the data anywhere else.
- Paths in YAML are repo-root-relative; **compute the relative-to-doc form at render time** for the markdown link so the link is clickable from the doc's location. Do not store the relative form in YAML.
- **Only completed specs.** Filter to paths under `specs/completed/`. If a path in `related_specs` is not under `completed/`, drop it from the rendered list AND warn the user (it is probably a stale or premature link).
- Link text is the spec's H1 title (or YAML `title` if there is no H1). Do not use the filename.
- Each line: linked title, separator ` — ` (the one sanctioned em-dash use, because the renderer needs a visible separator that is not a hyphen), date from spec's YAML `date_created`, primary author from spec's YAML `author` in parens.
- Sort by date **descending** (newest first).
- If `related_specs` is empty or absent, omit this section entirely. No empty placeholder.
- The skill regenerates this section from frontmatter on every doc save. Manual edits inside this section will be overwritten.

---

## Step 7 — Update or Create the Domain README.md

Each `docs/{domain}/` folder MUST contain a `README.md` that indexes every doc file in that folder. The README is fully owned by this skill — regenerate freely on every doc add/rename/remove.

Structure:

```markdown
# {Domain Title} Documentation

{1 to 2 paragraphs: what this domain covers in Rock, who tends to work in it, where to look in code for the entry points. Plain prose, not a list.}

## Files in this directory

| Doc | Summary |
|---|---|
| [Group Requirements](group-requirements.md) | How group requirements are evaluated, cached, and overridden. |
| [Group Member Workflows](group-member-workflows.md) | Workflows that fire on group member add/remove/role-change. |
```

- Domain Title in the H1 uses release-note casing (`Group`, `Lava`, `CRM`, `Check-in`). The folder name is lowercase; the heading is human-friendly.
- The intro prose is regenerated by the skill but should preserve its meaning across regenerations. If you have new prose to add, fold it in rather than replacing wholesale.
- The table lists every `*.md` file in the folder except `README.md` itself, alphabetical by file name. Summary comes from the doc's YAML `summary` if present, otherwise from the doc's first non-heading paragraph after the H1.

If a `docs/README.md` exists at the repo-`docs` root, the skill is also responsible for keeping that one current with the list of domain folders. Same auto-regenerated rules apply.

---

## Step 8 — Output

After writing or updating files:

1. State the path of every file written or modified (the doc, its domain README, possibly the root `docs/README.md`).
2. State whether this was a create or update.
3. Stop. Do not paste the doc content back into chat.

---

## Style Rules

- **Why first, concept second, code third.** Lead with why this exists (Overview, Why It Exists). Build the conceptual model second (Mental Model). Code archaeology comes last (Technical Reference). A reader who only reads the top half should come away knowing why this matters and how to think about it; the file:line citations are a separate read for active contributors. Source the why from engineering notes in cited source files, recent commit messages on `related_files`, and Motivation sections of linked specs.
- **Capture the why, not just the what.** Code shows what; this skill exists to surface the why. Engineering notes (multi-line code comments with developer initials and an optional `Reason:` line) and release-note commit messages are the highest-signal sources. Quote them when the original phrasing is good; paraphrase when not. If the why is genuinely unrecoverable, say so in the doc rather than fabricating one.
- **Concise over comprehensive.** A doc is read more often than it is written. If a section can be a sentence, do not make it a paragraph. If a section has nothing to add, drop it.
- **Code-anchored where it counts.** Mechanical claims in **Technical Reference** should cite `path/to/file.ext:line`. Conceptual content in **Overview**, **Mental Model**, and **What You Need to Know** should NOT be drowned in citations; one or two anchor references at most.
- **Mermaid diagrams are welcome** when a picture genuinely clarifies the system. Use a fenced ` ```mermaid ` block. Useful types: `flowchart` for control flow and lifecycle, `sequenceDiagram` for multi-actor interactions, `classDiagram` or `erDiagram` for entity relationships, `stateDiagram-v2` for state machines. Diagrams typically belong in **Mental Model** (the conceptual shape) or **Technical Reference > Data Model** (entity relationships and state machines). Keep them small and focused: one diagram per concept beats one giant diagram with everything. Skip the diagram if the surrounding prose already conveys the same information.
- **No em-dashes.** Use commas, parentheses, sentence breaks. (Project rule. The example "Related Specs" line uses ` — ` because the renderer needs a visible separator that is not a hyphen; that is the only sanctioned use.)
- **Linking conventions.** Source citations (Mental Model, What You Need to Know, Technical Reference) use repo-root-relative paths in `path/to/file.cs:line` form. The harness auto-links them and they remain valid as the reader's working copy evolves. Historical citations (Recent Impactful Changes, Considered but Rejected) use GitHub commit URLs in the form `[commit \`{short-hash}\`](https://github.com/SparkDevNetwork/Rock/commit/{short-hash})`; commits are immutable, so the link is forever-stable, and the citation IS the commit, not where the code lives now.
- **Document the right pattern, not the shortcut.** When the system has a site or global setting that callers should respect, document the read-and-pass-through pattern. Hardcoding the easy value to bypass the setting is the wrong example to teach. Example: when resolving a Group reference, callers must honor `Site.DisablePredictableIds`; document the pass-through, not `allowIntegerIdentifier: true`.
- **Never write an unverified date.** Dates in Recent Impactful Changes, Considered but Rejected, and frontmatter `last_updated` must come from a verifiable source (`git log`, spec frontmatter, `date +%Y-%m-%d`). If you cannot verify, omit. Approximate dates ("2024-late", "early 2025", "Q3") are forbidden.
- **No marketing tone.** This is internal technical reference, not a feature blurb.
- **Prefer terse opinionated writing over comprehensive overviews.** Generic mush is worse than a missing section.
- **Write like you are briefing a colleague who just walked in.** What is the smallest set of things they need to know to be productive? Lead with that.

---

## Audit Mode

Given a doc path (or a domain folder), verify the doc against current source.

1. Read the doc and pull out every concrete claim: file paths, method names, line references, behavior assertions.
2. Read the cited source files. Compare.
3. Report drift as a list. Categories:
   - **Stale path** — file or method no longer exists or has moved.
   - **Stale line reference** — `path:line` citation no longer points at the claimed code (line is past EOF, or the named symbol no longer appears at that line). Caveat: line numbers drift constantly with unrelated edits (a using-statement add bumps every line below it). Surface the list, but the user decides which are worth fixing; do not treat this as high-priority drift on its own.
   - **Behavior drift** — the code now does something different from what the doc says.
   - **Unverified date** — any date in Recent Impactful Changes that does not appear in `git log` for the cited commit, or any date in Considered but Rejected that does not appear in a referenced spec, commit, or source comment. The skill must NEVER hand-wave dates; this category catches earlier hallucinations.
   - **Stale Considered-but-Rejected** — entries with dates older than three years; flag for re-evaluation, do not auto-resolve.
   - **Stale `last_updated`** — frontmatter `last_updated` is more than six months old AND any `related_files` entry has been modified in git since then.
   - **Broken cross-link** — `related_specs` paths that do not exist, or that point outside `specs/completed/`. Also flag any path in `related_specs` or `related_files` that is not repo-root-relative (e.g. starts with `../` or with a drive letter).

**Do not auto-fix.** Surface the drift list and ask the user how to proceed. Audit is read-only by default.

---

## Update-From-Spec Mode

When invoked by the `spec` skill in completion mode, the caller passes:

- The path of the just-completed spec (now under `specs/completed/{domain}/...`).
- One or more candidate doc paths to update.

Workflow:

1. Read the completed spec for the actual change that landed (the "Proposed Fix", any updates the user made before completion, the final file/line references).
2. For each candidate doc:
   - Read it.
   - Update the affected sections to reflect the new "as built" state. Common edits: revise "How It Works" steps, add a row to "Recent Impactful Changes" (and prune to 5), update "Limitations" if the spec resolved one, add to "Considered but Rejected" if the spec rejected an alternative.
   - Append the spec's path to `related_specs` (if not already present), keeping the list filtered to `specs/completed/`.
   - Bump `last_updated` to today.
   - Re-render the Related Specs section.
3. After all candidate docs are updated, regenerate the affected domain README(s) so summaries reflect any new content.
4. Output a list of files modified.

If a candidate doc clearly should NOT be updated (e.g. the spec's actual landed change does not affect what the doc says), skip it and explicitly note that you skipped it and why. Do not edit defensively.

---

## Examples

**Example 1 — fresh topic, no existing coverage**

User: *"Write docs for how group requirements get evaluated and cached."*

You: Determine domain (`group`). Search `docs/group/` and find no existing file on this topic. Read the relevant source under `Rock/Model/Group/` and `Rock.Blocks/Group/`. Create `docs/group/group-requirements.md` with the full template. Generate `docs/group/README.md` if missing or update it to include the new entry. Output paths.

**Example 2 — topic already partially documented**

User: *"Document the new attribute caching behavior."*

You: Determine domain (`core`). Search and find `docs/core/cache-invalidation.md` already covers caching at a high level. Stop and say: "I found `docs/core/cache-invalidation.md` which already covers cache invalidation in general. Should I extend that file with an Attributes-specific section, or create a focused `docs/core/attribute-cache.md`?" Wait for the user before writing.

**Example 3 — invoked from spec completion**

Spec skill hands off: completed spec at `specs/completed/lava/260428-lava-entity-select-tostring-regression.md`, candidate doc `docs/lava/entity-commands.md`.

You: Read the spec to learn the landed change. Read the doc. Update the "How It Works" section to mention the `ParsingConfig` now used. Add a new "Recent Impactful Changes" bullet. Append the spec path to `related_specs`. Bump `last_updated`. Re-render the Related Specs section. Regenerate `docs/lava/README.md`. Output the file list.

**Example 4 — audit request**

User: *"Audit `docs/lava/entity-commands.md` against the current code."*

You: Read the doc. Read every cited source file. Compare. Report drift as a bulleted list categorized as Stale path / Behavior drift / Stale Considered-but-Rejected / Stale last_updated / Broken cross-link. Do not modify the file.
