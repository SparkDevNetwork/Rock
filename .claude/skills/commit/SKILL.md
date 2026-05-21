---
name: commit
description: >-
  Orchestrate a git commit for Rock RMS. Runs two pre-commit checks before handing off
  to the standard git commit flow: (1) detect active specs in `specs/` that look related
  to the change and offer to mark them complete after the commit lands; (2) ask whether
  the change belongs in the release notes and draft a Rock-formatted message accordingly
  (`+ (Domain) Fixed/Added/Improved/Updated ...` for release-note commits, `- ...` for
  minor commits). Use when the user says "commit", "commit this", "commit my changes",
  "create a commit", "make a commit", or invokes `/commit`. Do NOT use for: viewing
  commit history, amending an existing commit, force-pushing, or generating messages
  for code that has not been written yet.
argument-hint: "Optional: brief description of the change, an issue number (#XXXX), or 'minor'/'release-note' to skip the prompt"
compatibility: Requires Claude Code CLI with git access to the Rock RMS repository.
metadata:
  version: "1.0"
  author: "Jon Edmiston"
---

# Commit Workflow

You are creating a git commit for Rock RMS. This skill orchestrates two pre-commit checks (related specs, release-note classification) and hands off to the standard git commit flow defined in the system prompt's "Committing changes with git" section. **Do not duplicate or override the system prompt's safety rules**: this skill layers on top of them, it does not replace them.

**Argument:** $ARGUMENTS

---

## Output Style

**Be terse. Do not narrate the workflow.**

- Do NOT announce phases ("Phase 1:", "Phase 2:", "Skipping Phase 1"). Phases are internal structure for this skill, not user-facing labels.
- Do NOT explain what you are about to do or what you skipped. If a phase has nothing to ask the user about, move on silently.
- Ask direct questions only. Use yes/no for binary choices, or a numbered list (1. 2. 3.) when there are several options. Never ask an open-ended question if a closed one will do.
- Always include the **drafted commit message in a fenced code block** when asking the user to confirm. The user should be able to read the exact text that will be committed.
- One question per turn unless they are tightly related (e.g., release-note y/n + the draft can come together).

Example of the right tone:

> Found one spec that may be related: **Lava entity ToString regression** (`specs/260428-lava-entity-select-tostring-regression.md`).
>
> Mark it complete after this commit?
> 1. Yes
> 2. No

Example of the wrong tone:

> ## Phase 1: Detect related specs
>
> I'll now look through the active specs to see if any of them might be related to the changes in this commit. Let me check each one and report back...

---

## Workflow

The phases run in order. **Run them silently if there is nothing to ask the user about.** Surface a question only when the skill genuinely needs an answer.

1. **Detect related specs.** Find active specs that look related to the diff. If any score, ask about each. If none score, move on without comment.
2. **Classify the commit.** Ask whether this belongs in release notes. Draft the commit message and show it for confirmation.
3. **Commit.** Hand off to the standard git commit flow with the confirmed message.
4. **Complete flagged specs.** After the commit succeeds, capture the hash and invoke the `spec` skill in completion mode for each spec the user flagged.

If the diff is empty (`git diff --cached` and `git diff` both empty), stop and tell the user there is nothing to commit.

---

## Phase 1 — Detect Related Specs

### 1.1 Gather signals

Run these in parallel:

- `git status --porcelain` to see what files changed.
- `git diff --stat HEAD` to see the size and shape of the change.
- `git rev-parse --abbrev-ref HEAD` to get the current branch name.
- Glob `specs/*.md` to list active specs (do NOT include `specs/completed/**`).

If `specs/` is empty or only contains `completed/`, skip the rest of Phase 1.

### 1.2 Score specs for relevance

For each active spec, read its YAML frontmatter and any "Affected Code Paths" or "Proposed Fix" section. A spec is **likely related** if any of the following hold:

- **Branch-name match.** The branch contains a recognizable token from the spec's filename (excluding the timestamp prefix) or from the spec's H1 title.
- **File-path overlap.** The diff touches a file listed in the spec's `related_files` frontmatter or in its "Affected Code Paths" section.
- **Conversation mention.** The user explicitly named the spec in this conversation (by topic, filename, or title).

Score conservatively. **Only ask about specs with a real signal.** Asking about every active spec on every commit is noise; the user will start ignoring the prompt. If no spec scores, skip ahead to Phase 2 silently.

### 1.3 Ask about each candidate

If one candidate, ask in yes/no form. If two or more, present a numbered list and let the user pick which to mark complete.

**One candidate:**

> Found one spec that may be related: **[spec H1 title]** (`[path]`).
>
> Mark it complete after this commit?
> 1. Yes
> 2. No

**Multiple candidates:**

> Found these specs that may be related:
>
> 1. **[spec 1 H1 title]** (`[path 1]`)
> 2. **[spec 2 H1 title]** (`[path 2]`)
> 3. **[spec 3 H1 title]** (`[path 3]`)
>
> Which (if any) should be marked complete after this commit? (numbers, comma-separated, or "none")

Record the user's answers. Flag the chosen ones for Phase 4. Do not narrate "skipping the unselected ones".

If the user picks a spec whose diff looks partial (e.g., the spec proposes changes to five files and the diff only touches two), warn briefly with a yes/no:

> The spec lists 5 files but the diff only touches 2. Mark complete anyway?
> 1. Yes, mark complete
> 2. No, wait for the rest

If no candidates score, do not say "no related specs found". Move on silently.

---

## Phase 2 — Classify the Commit

### 2.1 Ask the release-notes question

Ask the user the binary question with the drafted message attached so they can answer once and confirm in the same turn. Internal heuristic for the recommendation: **yes** if a Rock customer reading the changelog would benefit from knowing about the change; **no** if it is internal cleanup, test infrastructure, comment-only edits, or a rebase artifact. Include a one-line rationale for your recommendation so the user can override it cleanly.

Format the prompt like this (terse, two drafts side by side, both rendered exactly as they would commit):

> Release-note commit?
>
> 1. Yes, release-note (`+` prefix)
> 2. No, minor commit (`-` prefix)
>
> My recommendation: **[1 or 2]**. [One-sentence rationale.]
>
> If 1, the message will be:
>
> ```
> + (Domain) Verb description. (Fixes #XXXX)
> ```
>
> If 2, the message will be:
>
> ```
> - description
> ```
>
> Confirm or tell me what to change.

If the user provided an explicit signal in `$ARGUMENTS` (e.g., "minor", "release-note", "+", "-"), skip the binary question. Show only the one drafted message and ask for confirmation:

> Drafting as a [release-note | minor] commit:
>
> ```
> [exact drafted message]
> ```
>
> Confirm or tell me what to change.

### 2.2 Draft the message

The full message format is documented in `CLAUDE.md` under "Commit Messages". The canonical domain list and path-to-domain mapping live in `.claude/rules/rock-domains.md`. **Do not duplicate either here. Read those files when in doubt.**

#### If yes (release-note commit)

Format:

```
+ (Domain) Verb description. (Fixes #XXXX)
```

- **Prefix:** `+`. Always.
- **Domain:** one of the release-note domains in `.claude/rules/rock-domains.md`. Pick by inspecting the changed file paths against the path-to-domain mapping in that file. If the change spans two domains, pick the one closest to the user-visible feature, not the supporting infrastructure. If the change is genuinely cross-cutting and no single domain fits, use `Other`.
- **Verb:** `Fixed` for bugs, `Added` for new features, `Improved` for enhancements, `Updated` for changes that don't fit "Fixed" or "Improved" cleanly. The verb decides classification in release notes; pick it deliberately.
- **Description:** complete, readable as a release-note. The reader should understand what changed without seeing the code. Past tense.
- **Issue number:** append `(Fixes #XXXX)` if the user provided one or it is in `$ARGUMENTS`. Skip the parenthetical entirely if there is no issue.

#### If no (minor commit)

Format:

```
- description
```

- **Prefix:** `-`. Always.
- **No domain, no verb classification, no period required.** Minor commits are not parsed for release notes.
- **Description:** short and direct. Imperative or past tense, either is fine.

#### Examples

```
+ (Core) Fixed the friendly schedule text display for single-date schedules. (Fixes #6694)
+ (Finance) Added support for ACH refunds on the NMI gateway.
+ (CRM) Improved the duplicate detection merge process to preserve giving records.
+ (Group) Updated the Group Scheduler to allow Warning-state members to be scheduled.
- Fixed typo in variable name
- Removed unused using statement
- Reformatted long lines in GroupMemberService
```

### 2.3 Confirmation

The drafted message is already shown in 2.1. Wait for the user's response. Acceptable responses:

- A bare number (`1` or `2`) to pick a draft option.
- "Yes" / "go" / "ship it" / "lgtm" — accept the recommended draft.
- A correction ("change domain to Group", "drop the issue number", "say 'Improved' instead of 'Updated'") — apply the change and re-show only the updated message in a fenced block, then ask once more for confirmation.

Do NOT re-explain the format or list adjustment options unprompted. The user knows what they want.

---

## Phase 3 — Commit

Hand off to the system prompt's "Committing changes with git" workflow with the confirmed message. The system prompt handles:

- Parallel `git status` / `git diff` / `git log` for context (already done in Phase 1, but the system prompt may re-do).
- Staging via specific file names (avoid `git add -A` per the system prompt's safety rules).
- HEREDOC-formatted commit message.
- The `Co-Authored-By: Claude ...` trailer.
- All commit safety rules (no `--no-verify`, no force push without explicit user instruction, no amending pushed commits, etc.).

**Do not bypass these rules.** The skill is an orchestrator on top of the system prompt's commit flow, not a replacement for it.

If a pre-commit hook fails, **stop and surface the error**. Fix the underlying issue and create a NEW commit. Do not amend; the failed commit did not happen.

---

## Phase 4 — Complete Flagged Specs

Run only if Phase 1 flagged one or more specs for completion AND Phase 3's commit succeeded.

### 4.1 Capture the commit hash

```bash
git rev-parse HEAD
```

### 4.2 Hand off to the spec skill

For each flagged spec, invoke the `spec` skill in completion mode (it auto-detects completion intent from the trigger phrasing). Pass:

- The spec path (e.g., `specs/260428-foo.md`).
- The commit hash captured in 4.1.

The spec skill will:

- Move the file to `specs/completed/{domain}/`.
- Update `specs/completed/INDEX.md` with the spec row, including the commit hash.
- Optionally prompt about updating related docs (handled by the spec skill's own completion flow).

### 4.3 Confirm

After all flagged specs are completed, summarize:

- The commit hash and one-line message.
- The list of specs that were moved into `specs/completed/{domain}/`.
- Any docs that were updated as a side effect.

Stop. Do not paste the commit diff or the spec contents back into chat.

---

## Safety Rules

- **Never skip hooks** (`--no-verify`, `--no-gpg-sign`) unless the user has explicitly asked. If a hook fails, fix the underlying issue and create a new commit.
- **Never auto-amend.** A failed commit is not a commit; amending the previous one risks losing prior work.
- **Never force-push.** Especially never force-push to `main` or `master`. If the user asks for a force push, warn first and confirm.
- **Stage specific files.** Prefer adding files by name. `git add -A` and `git add .` can include `.env`, credentials, or large binaries the user did not intend.
- **Never share secrets.** If the diff contains anything that looks like a credential, API key, or `.env` value, stop and warn before staging.
- **Do not auto-run `/check`.** Offer it before commit if the user has not run it; do not impose it. Some commits are intentionally WIP or doc-only.

---

## Edge Cases

**The diff is empty.** Tell the user there is nothing to commit. Do not invent changes.

**The user wants to skip the spec check.** If the user says "skip the spec check" or similar in `$ARGUMENTS`, skip Phase 1 entirely.

**The user wants to skip the release-notes prompt.** If `$ARGUMENTS` contains "minor", "release-note", "+", or "-", use that as the answer and skip Phase 2's question.

**The diff spans multiple domains.** Pick the domain closest to the user-visible feature per `.claude/rules/rock-domains.md`. If genuinely cross-cutting, use `Other` and explain in the description.

**The branch name suggests a spec but the diff does not touch any of its files.** Ask the user. The branch may be misnamed, or the diff may be the first of several commits intended to close the spec. Do not auto-flag.

**Multiple specs match the same diff.** Ask about each separately. The user may be closing several at once, or may want to close some and not others.

**A pre-commit hook fails after Phase 2.** Phase 3 stops. Do not run Phase 4. Do not amend. Fix the issue and start over.

---

## Examples

These show the **user-facing turns** the skill produces. Internal phase reasoning is silent.

### Example 1 — Bug fix that closes a spec

User: *"Commit this fix for the Lava entity ToString regression. Fixes #7012."*

Skill (one turn):

> Found one spec that may be related: **Lava entity ToString regression** (`specs/260428-lava-entity-select-tostring-regression.md`).
>
> Mark it complete after this commit?
> 1. Yes
> 2. No

User: *"1"*

Skill (next turn):

> Release-note commit?
>
> 1. Yes, release-note (`+` prefix)
> 2. No, minor commit (`-` prefix)
>
> My recommendation: **1**. Customer-visible bug fix in `Rock/Lava/`.
>
> If 1, the message will be:
>
> ```
> + (Lava) Fixed the Lava entity command select clause to allow .ToString() on properties whose name matches a CLR predefined type. (Fixes #7012)
> ```
>
> If 2, the message will be:
>
> ```
> - Fixed Lava entity ToString regression
> ```
>
> Confirm or tell me what to change.

User: *"go"*

Skill: stages files, creates commit, captures hash, hands off to spec skill in completion mode for the flagged spec, then summarizes (commit hash + one-line message + spec moved to `specs/completed/lava/`). Done.

### Example 2 — Minor cleanup

User: *"Commit this. Just removing some dead code."*

Skill (one turn — no related specs, so no Phase 1 prompt):

> Release-note commit?
>
> 1. Yes, release-note (`+` prefix)
> 2. No, minor commit (`-` prefix)
>
> My recommendation: **2**. Internal cleanup, no customer-visible behavior change.
>
> If 1, the message will be:
>
> ```
> + (Other) Updated GroupMemberService to remove unused private helpers.
> ```
>
> If 2, the message will be:
>
> ```
> - Removed unused private helpers in GroupMemberService
> ```
>
> Confirm or tell me what to change.

User: *"2"*

Skill: stages files, creates commit, summarizes. Done.

### Example 3 — Argument signals minor, single confirmation

User: *"commit minor: dropped the redundant null check in GroupCache"*

Skill (one turn — `minor` is in `$ARGUMENTS`, no related specs):

> Drafting as a minor commit:
>
> ```
> - Removed redundant null check in GroupCache
> ```
>
> Confirm or tell me what to change.

User: *"go"*

Skill: commit. Done.

### Example 4 — Partial spec match

User: *"Commit. This is part of the group requirement caching work."*

Skill (one turn):

> Found one spec that may be related: **Group requirement caching** (`specs/260315-group-requirement-caching.md`).
>
> The spec lists 5 files but the diff only touches 2. Mark complete anyway?
> 1. Yes, mark complete
> 2. No, wait for the rest

User: *"2"*

Skill: continues to release-notes prompt as normal, no spec flagged for completion.
