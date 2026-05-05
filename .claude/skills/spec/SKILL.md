---
name: spec
description: >-
  Author or complete a spec document in the repo's `specs/` directory. Specs capture
  requirements and architecture decisions BEFORE coding, or document a known issue
  (problem, root cause, proposed fix) for review. Use when the user says "write a spec",
  "create a spec", "spec this out", "draft a spec for X", "document the design for Y",
  or asks for a written decision record/RFC. Also use when the user wants to formalize
  the analysis of a bug or regression before applying the fix. ALSO use when the user
  says "complete this spec", "mark spec as done", "the X spec is finished", "archive
  this spec", or similar — the skill will move the file into `specs/completed/{Domain}/`
  and update `specs/completed/INDEX.md`.
  ALSO use when the user says "reject this spec", "won't fix", "won't do", "mark spec as
  rejected", "abandon this spec", or similar — the skill will capture a rejection reason,
  append a Rejection section to the spec, move the file into `specs/rejected/{Domain}/`,
  and update `specs/rejected/INDEX.md`.
  Do NOT use for: writing code, writing release notes, or writing CLAUDE.md / rule files.
argument-hint: "Topic of the spec, OR the path/name of an existing spec to mark complete"
compatibility: Requires Claude Code CLI with access to the repo and bash for the date stamp.
metadata:
  version: "1.2"
  author: "Jon Edmiston"
---

# Spec Authoring Workflow

You are working on a spec document. A spec captures requirements, architectural decisions, or the analysis of a known issue **before code is written**. It is the artifact reviewers read to agree on direction.

**Argument:** $ARGUMENTS

---

## Decide the Mode

Read `$ARGUMENTS` and the conversation context, then pick one of three modes:

- **Authoring mode** — the user wants to create or convert a new spec. Follow Steps 1 through 7 below.
- **Completion mode** — the user signals that an existing spec is done. Phrases like "the spec is complete", "mark X spec as done", "archive this spec", "spec is finished", "move spec to completed". Skip Steps 1 through 7 and jump to **Completing a Spec** below.
- **Rejection mode** — the user signals that a spec will not be implemented. Phrases like "reject this spec", "won't fix", "won't do", "mark X spec as rejected", "abandon this spec". Skip Steps 1 through 7 and jump to **Rejecting a Spec** below.

If the intent is ambiguous, ask one clarifying question before proceeding. If the user uses "done" ambiguously, ask whether they mean the spec was completed (shipped) or rejected (decided against).

---

## Step 1 — Gather Inputs

Before drafting, collect the following. Ask the user only for what is genuinely missing — do not re-ask things already in the conversation.

1. **Topic / title** — a short noun phrase (under ~70 chars). Derive from `$ARGUMENTS` if provided.
2. **Spec type** — pick one based on intent:
   - **Feature / architecture** — new feature, refactor, or design decision.
   - **Issue / regression** — analysis of an existing bug or unwanted behavior.
   - **Hybrid** — a feature with a specific problem motivating it. Use both section sets.
3. **Contributors** — **ALWAYS ASK** at the start: "Who else should be listed as a contributor on this spec? (full names — first and last — comma-separated, or 'none')". Do not skip this step. If the user says "none", set the field to `[]`. Every contributor name MUST include both a first and last name; if a single name is given (e.g. "Daniel"), ask for the last name before writing the file. Do not guess or use initials.
4. **Author** — read from `git config user.name`. The value MUST be a full human name (first AND last name). If git returns only a first name, a single token, initials, or empty, ask the user for their full name before writing the file. Do not invent a name.
5. **Brief summary** — one or two sentences. Derive from the conversation if obvious; otherwise ask.
6. **External resource links** — collect any external URLs the user has referenced (Figma, Asana, GitHub issues/PRs, Notion pages, design system docs, customer tickets, related public docs). These will be cross-checked in Step 2 before the spec is written.
7. **Local artifacts** — any photos, screenshots, PDFs, mockups, sample data, or other supporting files the user wants attached to the spec. Capture the paths or filenames; placement is handled in Step 5.

---

## Step 2 — Cross-check External Resources

If the spec references any external URLs (collected in Step 1.6, mentioned in the conversation, or appearing in any draft text the user pasted), fetch and read each one before writing the spec. The goal is to catch discrepancies between the user's stated requirements and what the external resource actually says.

External resources can be **stale or contradictory**. Figma files in particular are often updated after a spec discussion and can disagree with what the user remembers, what got decided in a meeting, or what got written in another doc. Treat them as inputs to be reconciled, not as ground truth.

### What to do

For each external link:

1. **Fetch the resource.** Use `WebFetch` for public URLs. For Figma, follow the file/frame URL and extract the visible requirements (screen flow, copy, field constraints, states, components). For Asana/Notion/GitHub, read the linked page or issue.
2. **Compare against the user's stated requirements.** Look for:
   - **Field/control mismatches** — the user said "a single text field"; the design has two fields.
   - **State coverage gaps** — the design shows error/empty/loading states the user didn't mention, or vice versa.
   - **Copy/label drift** — the design uses different button labels, headings, or placeholder text than the user described.
   - **Flow differences** — the design implies a step the user didn't mention, or skips one the user did.
   - **Stale designs** — last-modified dates well before the conversation, or notes/comments in the file that contradict the rendered state.
   - **Conflicts between resources** — Figma says one thing, the linked Asana ticket says another.
3. **Surface every discrepancy.** Do NOT silently pick a side. List the conflicts in a short message and ask the user which interpretation is canonical. If the user says "Figma is canonical," capture that decision; if they say "use what I told you, the Figma is stale," note that in the spec's Open Questions or Considered but Rejected section so future readers know the design was knowingly diverged from.
4. **Record the cross-check in the spec.** In the spec's **Related** section, list each external resource link AND a one-line note about its status: "Figma file (last modified YYYY-MM-DD, treated as canonical)" or "Asana ticket (referenced for context, requirements live in this spec)". This tells future readers how much weight to put on each link.

### When to skip the cross-check

- Trivial specs (a typo fix, a doc-only spec) where no external resource has been referenced.
- The user has explicitly said "don't read the Figma, just write what I told you" — but still note that decision in the spec.
- The link is to a generic reference (e.g. "the C# language spec", a blog post about a pattern) rather than a project-specific source of requirements.

### What to do when fetching fails

If a link is private, requires auth, or returns an error, do NOT silently drop it. Tell the user the link could not be read and ask whether they want to:
1. Paste the relevant content directly.
2. Skip cross-checking this link (and accept the risk that the spec may diverge from it).
3. Provide an alternate URL.

If the user picks option 2, note in the spec's Related section that the link was not verifiable: "Figma file (auth required, contents not verified)".

---

## Step 3 — Generate the Filename

Filenames are date-stamped to act as a stable hash and to sort chronologically.

**Format:** `YYMMDD-{topic-kebab}.md`

- `YYMMDD` — current local date, zero-padded, **day precision** (no hours or minutes). Get it with:
  ```bash
  date +%y%m%d
  ```
- `{topic-kebab}` — short kebab-case phrase (under ~60 chars). Lowercase, words separated by hyphens, no spaces, no punctuation other than hyphens. Strip leading articles ("the", "a").
- The separator between the date and the topic is a single hyphen (`-`). No spaces anywhere in the filename.

**Example:** `260428-lava-entity-select-tostring-regression.md`

If two specs are created on the same day, the topic-kebab portion is what disambiguates them. If two specs on the same day need the same topic-kebab (rare), suffix the second with `-2`, the third with `-3`, immediately before the `.md` (e.g. `260428-other-topic-2.md`).

Write the file to the repo's top-level `specs/` directory. If `specs/` does not exist, create it.

---

## Step 4 — Write the YAML Frontmatter

Every spec begins with this block, in this order:

```yaml
---
author: {Full Name}
date_created: YYYY-MM-DD
summary: >-
  One or two sentence summary that someone scanning the specs/ directory
  could read and understand the gist of the spec.
contributors:
  - {Name 1}
  - {Name 2}
related_docs:
  - docs/{domain}/{topic}.md
---
```

Notes:
- `date_created` is ISO-8601 (`YYYY-MM-DD`), not the timestamp from the filename. Get with `date +%Y-%m-%d`.
- `contributors` is a YAML list. If empty, write `contributors: []`.
- `related_docs` is an optional YAML list of paths into `docs/` that this spec touches or supersedes. Used by the `docs` skill to find candidates for update when this spec is completed. If unknown, omit the key entirely (do not write `related_docs: []`).
- Use the `>-` folded-block-scalar form for `summary` so it stays readable when long.

---

## Step 5 — Write the Body

Choose sections based on spec type. **Include only sections that are relevant** — empty placeholder sections add noise. The order below is canonical; keep it.

### For all specs

- **`# {Title}`** — H1 matching the topic. Same string used in the filename.
- **`## Summary`** — 2-4 sentences. Restates the YAML summary with a bit more context. The reader who only reads this section should understand what's being proposed and why it matters.
- **`## Motivation`** — include this section **whenever the "why" behind the change is germane to the use case** and is not self-evident from the title. Capture the pain point, the user problem, the strategic driver, the regression that prompted the work, the customer ask, the deadline, the dependency, or any other context that explains why this spec exists *now*. Reviewers and future readers use Motivation to evaluate whether the requirements and design actually solve the right problem. If the motivation is genuinely obvious (e.g. "fix typo in error message"), drop the section. When in doubt, include it.
- **`## Requirements`** — bulleted list of what the system must do. Use MUST / SHOULD / MAY language where precision helps. Group by capability if the list is long.

### For feature / architecture specs (add)

- **`## Design`** or **`## Proposed Approach`** — the architectural decision. Diagrams in mermaid are welcome. Describe data flow, key types, integration points.
- **`## Open Questions`** — anything the spec deliberately leaves unresolved for reviewer input. Drop the section if there are none.

### For issue / regression specs (add — these mirror the structure of a debugging writeup)

- **`## Problem Statement`** — concise, one paragraph. What is broken or unwanted.
- **`## Reproduction`** — minimal steps or a code/markup sample that triggers the issue. If the issue is environmental (version-bound), state which versions are affected.
- **`## Root Cause`** — the actual mechanism. Cite file paths and line numbers (`path/to/file.cs:123`) so reviewers can jump to the source.
- **`## Affected Code Paths`** — bulleted list of files / call sites impacted. Differentiate primary (where the fix lands) from secondary (downstream consumers).
- **`## Workarounds`** — what users / operators can do today without a code change. Mark as "user-side" if applicable.
- **`## Proposed Fix`** — the recommended change. Be specific: which file, which method, what new behavior. Include code sketches only if they clarify; this is not the implementation.
- **`## Fix Risks`** — what could go wrong with the proposed fix. Performance, security, backward compatibility, blast radius.
- **`## Verification Steps`** — numbered list. How a reviewer or QA confirms the fix works and doesn't regress adjacent behavior.
- **`## Out of Scope`** — explicit list of related issues this spec does NOT address. Prevents scope creep in review.

### When relevant (any spec type)

- **`## Considered but Rejected`** — alternatives that were evaluated and dismissed. For each: a short heading, a one-line description, and **why** it was rejected. This is one of the most valuable sections of a spec — it preempts "have you thought about X?" review comments. Include this section whenever there were real alternatives, even if only briefly considered.

  Example structure:

  ```markdown
  ## Considered but Rejected

  ### Use a global ParsingConfig singleton
  Rejected. Would require a public API change on `RockEntityBlock` and risks
  callers in plugins picking up the config implicitly. Per-call config keeps
  the change scoped to the entity command flow.

  ### Roll back System.Linq.Dynamic.Core to 1.6.6
  Rejected. The 1.7.0 bump pulled in security fixes and dependency updates;
  reverting would force re-pinning across the solution.
  ```

- **`## Related`** — links to issues, PRs, prior specs, external documentation, or commit hashes that provide context. Use markdown links. Include commit hashes as plain monospace if there is no PR / issue URL.

### Adding Artifacts to a Spec

Specs frequently include supporting files: reference screenshots, mockup images, sample data, PDFs of design briefs, redacted customer correspondence, etc. These are stored alongside the spec, not inline in the markdown.

#### Location

Active specs store artifacts under a per-spec subfolder of `specs/artifacts/`:

```
specs/
  {timestamp}-{topic-kebab}.md
  artifacts/
    {timestamp}-{topic-kebab}/        ← subfolder name = spec filename minus .md
      mockup.png
      reference-design.pdf
      sample-payload.json
```

The subfolder name MUST match the spec's filename without the `.md` extension (e.g. `260501-foo.md` ⇒ `specs/artifacts/260501-foo/`). Create the `specs/artifacts/` directory and the per-spec subfolder if they don't exist.

#### Linking from the spec

Reference artifacts using a relative markdown link from the spec file. From a spec at `specs/{name}.md`, the relative path to its artifacts is `artifacts/{name}/{file}`:

```markdown
See the proposed layout in [Mockup A](artifacts/260501-foo/mockup-a.png).

![Mockup A](artifacts/260501-foo/mockup-a.png)
```

Use this exact form. **Do NOT** use absolute paths, repo-rooted paths (`/specs/...`), or `../` traversals — those break when the spec is moved during completion or rejection.

#### When the user supplies artifacts

If the user attaches files to the conversation, drops them somewhere on disk, or pastes image data:

1. Create `specs/artifacts/{spec-filename-without-ext}/` if it doesn't exist.
2. Copy or move the file into that subfolder. Preserve the original filename when reasonable; rename only if it's unclear (e.g. `image.png` → `mockup-checkout-flow.png`).
3. Reference it in the spec body using the relative form above.
4. If the file is large or binary and the user wants it tracked by git, confirm before adding (some teams gitignore `specs/artifacts/` entirely; check the repo's `.gitignore` first).

#### When the user references but does not supply

If the user describes an artifact ("the Figma export I sent in Slack last week") but does not provide it, do NOT invent a path. Ask whether they want to attach it now, or note in the spec's Related section that the artifact is referenced externally.

---

## Step 6 — Style Rules

- **Concise over comprehensive.** A spec is read more than it is written. If a section can be a sentence, do not make it a paragraph. If a section is empty, drop it.
- **No em-dashes.** Use `—` only when quoting; otherwise prefer commas, parentheses, or sentence breaks. (Project rule.)
- **File and line references** use the format `path/to/file.ext:line` so they auto-link in the harness.
- **Code blocks** are fenced with the language tag (` ```csharp `, ` ```liquid `, etc.).
- **Tables** are fine for trade-off matrices and option comparisons.
- **Mermaid diagrams are welcome** when a picture clarifies the design or the failure mode. Use a fenced ` ```mermaid ` block. Common useful types: `flowchart` for control flow and decision trees, `sequenceDiagram` for multi-actor interactions (client / API / DB / job), `classDiagram` for entity relationships, `stateDiagram-v2` for state machines, `erDiagram` for data-model relationships. Keep diagrams small and focused; one diagram per concept beats one big diagram with everything. Drop the diagram entirely if it does not add information beyond the surrounding prose.
- **No trailing summary** at the end of the spec. The last section is the last section.

---

## Step 7 — Output

After writing the file:

1. Confirm the path you wrote to.
2. Offer a one-line next step (e.g., "Want me to open a PR with this spec?" or "Want me to implement the proposed fix on a branch?"). Skip the offer if the user already signaled what comes next.

Do not paste the full spec back into chat — the user can read the file. A short confirmation of what was written and where is enough.

---

## Examples

**Example 1 — feature spec request**

User: *"Write a spec for the new attendance bulk-import block."*

You: Ask for contributors. Default author from git. Ask for any external links or artifacts. Pick spec type: feature. Cross-check any provided links before drafting. Sections: Summary, Requirements, Design, Open Questions, Considered but Rejected (if alternatives discussed), Related. Skip the issue-specific sections entirely.

**Example 1b — feature spec with a Figma link**

User: *"Write a spec for the new bulk-import block. Here's the Figma: https://figma.com/file/abc123/Bulk-Import"*

You: Ask for contributors. Fetch the Figma file. Compare what the user said in the conversation against what the Figma shows. If the Figma shows a "preview before commit" step the user did not mention, surface it: "The Figma includes a preview step between upload and commit; was that intentional, or should the spec describe a single-step import?". Capture the Figma's last-modified date in the spec's Related section. Place mockup screenshots from the Figma into `specs/artifacts/{spec-name}/` if the user attaches exports.

**Example 2 — issue spec request (continuing from a debugging conversation)**

User: *"Now write that up as a spec."* (after diagnosing a regression in chat)

You: Ask for contributors. Default author from git. Pick spec type: issue. Use Problem Statement / Reproduction / Root Cause / Affected Code Paths / Workarounds / Proposed Fix / Fix Risks / Verification Steps / Out of Scope / Related. Pull the analysis straight from the conversation — do not re-investigate.

**Example 3 — hybrid (fixing a problem with a new design)**

User: *"Spec the move from inline parsing to a shared ParsingConfig — also document why the current state is broken."*

You: Use both section sets. Lead with Problem Statement (what's broken now), then Requirements + Design (what we're moving to), then Verification Steps + Out of Scope. Considered but Rejected almost always belongs here.

---

## Completing a Spec

When a spec's work has shipped (or the decision it documented is no longer pending), it should be archived out of the active `specs/` directory and into `specs/completed/`, organized by domain.

### Directory layout

```
specs/
  {timestamp}-{topic-kebab}.md      ← active specs
  completed/
    INDEX.md                        ← single index of every completed spec
    core/
      {timestamp}-{topic-kebab}.md
    lava/
      {timestamp}-{topic-kebab}.md
    finance/
      ...
    {other domain folders as needed}
```

All directory names under `specs/completed/` MUST be lowercase, with hyphens for spaces (`check-in`, `lms`, `crm`, etc.). This matches the convention used by the `docs/` skill so navigation is consistent across both systems. Do not use release-note casing for folder names; that casing is reserved for commit messages and the index's `Domain` column.

### Step C1 — Identify the spec

If the user named the spec by topic, path, or partial filename, locate it in `specs/`. If multiple files match, list them and ask which. If none match, list the contents of `specs/` and ask the user to pick. Never guess.

### Step C2 — Pick the domain folder

Read the spec's Title, Summary, and Affected Code Paths to determine the Rock domain. The canonical list of domains, the path-to-domain mapping, and the translation between release-note casing and folder names live in `.claude/rules/rock-domains.md`. Use that file as the single source of truth. Do not duplicate or guess.

The `Domain` column in `INDEX.md` uses the release-note form. The folder name on disk uses the lowercase-kebab folder form. If the chosen folder does not exist under `specs/completed/`, create it.

### Step C3 — Capture the commit hash (optional but preferred)

Ask the user: "Is there a commit (or PR) that implemented this spec? Paste the hash, PR URL, or 'none'." If the user provides nothing, leave the commit column blank in the index.

If the user gestures at a commit without a hash ("the one I just made", "the merge from yesterday"), run `git log --oneline -10` to surface candidates and confirm with the user before recording.

### Step C4 — Move the file

Move the spec from `specs/{filename}` to `specs/completed/{Domain}/{filename}`. Preserve the original filename verbatim (timestamp prefix included) so the chronological hash stays intact.

Use `git mv` if the source file is tracked by git, otherwise plain `mv`. Verify the move succeeded before updating the index.

### Step C5 — Move the spec's artifacts (if any) and adjust links

If the spec has an artifacts subfolder at `specs/artifacts/{spec-filename-without-ext}/`, move it alongside the spec.

1. **Check** for `specs/artifacts/{spec-filename-without-ext}/`. If it does not exist, skip this step.
2. **Create** `specs/completed/{Domain}/artifacts/` if it does not exist.
3. **Move** the per-spec subfolder into the completed domain's artifacts folder:
   - Source: `specs/artifacts/{spec-filename-without-ext}/`
   - Destination: `specs/completed/{Domain}/artifacts/{spec-filename-without-ext}/`
   - Use `git mv` for the entire subfolder if tracked by git; otherwise `mv`.
4. **Audit links** in the moved spec. Open the spec at its new location and check every relative link or image reference. Because the spec is now at `specs/completed/{Domain}/{filename}` and the artifacts are at `specs/completed/{Domain}/artifacts/{spec-filename-without-ext}/`, the relative form `artifacts/{spec-filename-without-ext}/{file}` continues to resolve correctly. **However**, if the original spec used any other form (e.g. `../artifacts/...`, repo-rooted paths, or absolute paths), rewrite those links to the new relative form so they resolve from the new location.
5. **Verify each link resolves** to a real file at the new location before moving on. Do not skip the audit; broken links in completed specs are easy to ship and hard to find later.

If `specs/artifacts/` is now empty after the move (no other active specs have artifacts), leave it in place. Do not delete the parent directory.

### Step C6 — Update `specs/completed/INDEX.md`

`INDEX.md` is a single markdown table listing every completed spec across every domain folder. Schema:

| Column | Source | Notes |
|---|---|---|
| Spec | The spec's H1 title (or the YAML title if there is no H1). Linked to the moved file. | Use a relative markdown link from `specs/completed/INDEX.md` (e.g. `[Title](core/260428-foo.md)`). The folder segment in the link is the lowercase folder name. |
| Domain | The Rock domain in **release-note casing** (`Core`, `Lava`, `CRM`, `Check-in`, ...). | This is for human readability of the index. The folder name itself is lowercase, but the index displays the prettier form. |
| Author | The `author:` value from the spec's YAML frontmatter. | Full name. |
| Summary | The `summary:` value from the spec's YAML frontmatter. | One-liner. Keep it readable; do not wrap. |
| Commit | The hash, PR link, or merge commit captured in Step C3. | Plain monospace hash with optional GitHub URL, or empty. |

If `INDEX.md` does not yet exist, create it with this header:

```markdown
# Completed Specs Index

This index lists every spec that has been moved into `specs/completed/`. It is maintained by the `spec` skill — please do not edit by hand.

| Spec | Domain | Author | Summary | Commit |
|------|--------|--------|---------|--------|
```

Append the new row at the bottom. Do not re-sort existing rows. (Filenames are timestamp-prefixed, so chronological order is preserved naturally if rows are appended.)

If a row for this spec already exists in the index (e.g. the user is re-completing or fixing an earlier mistake), update the existing row in place rather than adding a duplicate.

### Step C7 — Offer to update related docs

After the file is moved and the index is updated, ask the user:

> "Should any docs in `docs/` be updated to reflect this spec? (paths or 'no')"

Surface candidates automatically when possible:

- The completed spec's `related_docs` frontmatter field, if present.
- Any doc whose `related_specs` frontmatter already references this spec.
- Any doc whose `related_files` overlaps with the spec's "Affected Code Paths" section.

If the user names paths or accepts the suggestions, hand the candidate paths and the completed-spec path to the `docs` skill in update mode. The docs skill will:

- Update the affected sections of those docs to reflect the new "as built" state.
- Add this spec's path to each doc's `related_specs` frontmatter (so it surfaces in the "Related Specs" section at the bottom of the doc).
- Bump each doc's `last_updated`.

If the user declines or says "no", proceed without updating any docs. Never silently rewrite documentation without explicit consent.

### Step C8 — Confirm

After moving, indexing, and any docs updates:

1. State the new path of the spec.
2. State that `INDEX.md` has been updated.
3. List any doc files that were updated, or note that none were.
4. Stop. Do not paste the spec, index, or docs back into chat.

### Completion mode rules

- **Never edit the body of the spec during completion.** The whole point is to preserve the historical record.
- **Never delete a spec.** Completion always means "move and index", not "remove."
- **If the spec is already in `specs/completed/`, do nothing and tell the user.** Do not re-move or re-index.
- **If the user explicitly asks to revert a completion** (move from `completed/` back to `specs/`), do it: move the file back to `specs/` and remove the row from `INDEX.md`.

---

## Rejecting a Spec

When a spec is decided against (the proposal is dismissed, the issue is intentionally not going to be fixed, or the design has been superseded by a different approach), it should be archived out of the active `specs/` directory and into `specs/rejected/`, organized by domain. Rejection preserves the historical record so future contributors can find the reasoning instead of re-proposing the same idea.

### Directory layout

```
specs/
  rejected/
    INDEX.md                        ← single index of every rejected spec
    core/
      {timestamp}-{topic-kebab}.md
    lava/
      {timestamp}-{topic-kebab}.md
    {other domain folders as needed}
```

Same lowercase-kebab folder rules as `specs/completed/`. Do not use release-note casing.

### Step R1 — Identify the spec

Same as Step C1 of completion. If the user named the spec by topic, path, or partial filename, locate it in `specs/`. If multiple files match, list them and ask which. If none match, list the contents of `specs/` and ask the user to pick. Never guess.

### Step R2 — Pick the domain folder

Same logic as Step C2. Read the spec's Title, Summary, and Affected Code Paths to determine the Rock domain. Reference `.claude/rules/rock-domains.md` for the canonical list and the path-to-domain mapping.

The `Domain` column in `INDEX.md` uses the release-note form. The folder name on disk uses the lowercase-kebab folder form. If the chosen folder does not exist under `specs/rejected/`, create it.

### Step R3 — Capture the rejection reason

ASK the user: "What is the reason for rejecting this spec? (one or two sentences)". This is mandatory. Do not proceed without a reason.

The reason becomes the historical record of why this idea was dismissed. Future contributors who propose the same thing should read this section first and decide whether the rejection still applies.

If the user gives a one-word answer or a vague reason ("not needed", "later"), push for specifics. Acceptable reasons name a concrete blocker: technical, strategic, scope, cost, dependency, regression risk, conflict with another in-flight design, security concern. A pure "not now" is a deferral, not a rejection, and the spec should stay active.

### Step R4 — Append a Rejection section to the spec body

Before moving the file, edit the spec to append a new section at the very end:

```markdown
## Rejection

**Rejected on:** {YYYY-MM-DD, get with `date +%Y-%m-%d`}
**Rejected by:** {Full name from `git config user.name`, same author rules as Step 1}

{The rejection reason captured in R3. One or two paragraphs. Be specific about what was rejected and why; future readers should be able to tell whether the rejection still applies given today's context.}
```

Do not edit any other section of the spec. The point is to preserve the original proposal verbatim as a historical record; the Rejection section is the only addition.

### Step R5 — Move the file

Move the spec from `specs/{filename}` to `specs/rejected/{Domain}/{filename}`. Preserve the original filename verbatim (timestamp prefix included).

Use `git mv` if the source file is tracked by git, otherwise plain `mv`. Verify the move succeeded before updating the index.

### Step R6 — Move the spec's artifacts (if any) and adjust links

Same logic as Step C5 in completion mode, but the destination is `specs/rejected/{Domain}/artifacts/{spec-filename-without-ext}/`.

1. **Check** for `specs/artifacts/{spec-filename-without-ext}/`. If it does not exist, skip this step.
2. **Create** `specs/rejected/{Domain}/artifacts/` if it does not exist.
3. **Move** the per-spec subfolder:
   - Source: `specs/artifacts/{spec-filename-without-ext}/`
   - Destination: `specs/rejected/{Domain}/artifacts/{spec-filename-without-ext}/`
   - Use `git mv` if tracked by git; otherwise `mv`.
4. **Audit links** in the moved spec. The relative form `artifacts/{spec-filename-without-ext}/{file}` continues to resolve from the new location. Rewrite any non-relative or `../`-based links so they resolve against the new location, and verify each link points to a real file.

Rejection preserves the historical record, so artifacts that were referenced by the original proposal need to remain accessible. Do not drop them on the floor.

### Step R7 — Update `specs/rejected/INDEX.md`

`INDEX.md` is a single markdown table listing every rejected spec across every domain folder. Schema:

| Column | Source | Notes |
|---|---|---|
| Spec | The spec's H1 title (or the YAML title if there is no H1). Linked to the moved file. | Use a relative markdown link from `specs/rejected/INDEX.md`. |
| Domain | The Rock domain in **release-note casing** (`Core`, `Lava`, `CRM`, `Check-in`, ...). | Human-readable form. The folder itself is lowercase. |
| Author | The `author:` value from the spec's YAML frontmatter. | The original proposer. |
| Summary | The `summary:` value from the spec's YAML frontmatter. | One-liner. |
| Rejected On | The ISO date from the appended Rejection section. | YYYY-MM-DD. |
| Rejection Reason | A short paraphrase of the appended Rejection section, fitting one row. | One sentence. The full reason lives in the spec; the index is a quick scan. |

If `INDEX.md` does not yet exist, create it with this header:

```markdown
# Rejected Specs Index

This index lists every spec that has been moved into `specs/rejected/`. It is maintained by the `spec` skill, please do not edit by hand. Specs land here when a proposal is dismissed; the goal is to preserve the historical reasoning so future contributors can find it before re-proposing the same idea.

| Spec | Domain | Author | Summary | Rejected On | Rejection Reason |
|------|--------|--------|---------|-------------|------------------|
```

Append the new row at the bottom. Do not re-sort existing rows.

If a row for this spec already exists in the index (e.g. the user is updating a previously-rejected spec), update the existing row in place rather than adding a duplicate.

### Step R8 — Confirm

After moving and indexing:

1. State the new path of the spec.
2. State that `INDEX.md` has been updated.
3. State the rejection reason captured (one line).
4. Stop. Do not paste the spec or index back into chat.

### Rejection mode rules

- **Never edit the body of the spec other than appending the Rejection section.** The proposal must be preserved verbatim so future readers see what was actually proposed.
- **Never delete a rejected spec.** Rejection always means "move and index", not "remove."
- **If the spec is already in `specs/rejected/`, do nothing and tell the user.** Do not re-move or re-index.
- **If the user explicitly asks to revert a rejection** (move from `rejected/` back to `specs/`), do it: remove the appended Rejection section, move the file back to `specs/`, and remove the row from `INDEX.md`.
- **Rejection differs from completion.** A completed spec shipped; a rejected spec was decided against. Do not conflate the two paths. If the user uses ambiguous language ("the spec is done"), ask whether they mean completed (shipped) or rejected (won't fix).
