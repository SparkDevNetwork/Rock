---
name: remember
description: >-
  Audit the current session for things worth saving to auto-memory and present
  them as a plan-mode draft for explicit user approval before writing. Also
  identifies bubble-up candidates — rules that arguably belong in team-shared
  memory (CLAUDE.md or .claude/rules/) instead of private auto-memory — and
  surfaces them as suggestions without writing them. Use when the user says
  "remember", "/remember", "save what we learned", "capture this session", or
  wants a deliberate retrospective pass on a long session before context fades.
  Reads MEMORY.md plus every individual memory file first to dedupe; classifies
  each candidate as user / feedback / project / reference; drafts full bodies;
  awaits ExitPlanMode approval before writing anything. Do NOT use for: editing
  CLAUDE.md (that is the /memory slash command's job), saving session-specific
  task state (memory is for cross-session learning), or recording git history
  or code patterns that are derivable from the repo.
argument-hint: "Optional scope hint, e.g. 'focus on review feedback' (defaults to full-session audit)"
compatibility: "Requires Claude Code CLI with the auto-memory folder configured via system prompt."
metadata:
  version: "1.1"
  author: "Maxwell Eley"
---

# Remember: Auto-Memory Curation Workflow

**CRITICAL: Enter plan mode immediately.** Do all reading and drafting in plan mode. Present the candidate memories via `ExitPlanMode`. Only after the user approves may you write to the memory folder.

You are auditing the current session for cross-session memory. The memory folder is the one declared in your system prompt under the auto-memory section (path of the form `~/.claude/projects/{encoded-cwd}/memory/`). Do not hardcode the path; use the one already in your system prompt at runtime.

Optional scope hint from the user: **$ARGUMENTS**

If `$ARGUMENTS` is empty, run a full-session audit. Otherwise, treat the argument as a scope filter (for example, "focus on review feedback") and only surface candidates that fit the scope.

---

## Why This Skill Exists

The auto-memory system (driven by your system prompt) saves things in the moment as the user corrects you or validates non-obvious choices. That works, but it has two complementary failure modes:

1. **Sometimes it does — but in the wrong layer.** Auto-save fires the moment a correction lands; it does not pause to ask whether the rule generalizes to the rest of the team. Some rules saved as private feedback are actually project conventions the whole team should know.
2. **Sometimes it doesn't — and the pattern is missed entirely.** Quiet confirmations (the user accepted a non-obvious choice three times in a session), patterns that only emerge across many turns, end-of-session insights — auto-save sees individual moments; it misses the shape across them.

`/remember` addresses both:

- It walks the full transcript retrospectively to catch what auto-save missed.
- It identifies **bubble-up candidates** — entries that look more like team-shared rules than private memory — and surfaces them for the user to move into `CLAUDE.md` or `.claude/rules/` manually. The skill never writes to team-shared rules itself; that is an explicit, reviewable change.

---

## Reference Routing Table

| Reference File | Load When |
|---|---|
| `references/memory-templates.md` | Always, before drafting candidate bodies |

---

## Scale to the Session

Match effort to the session size:

- **Short session, no corrections.** Surface a brief "Nothing memory-worthy" plan. Do not invent candidates.
- **Medium session with one or two clear corrections.** Draft each as a feedback candidate, dedupe, present.
- **Long session with many decisions.** Walk the transcript carefully. Group related insights into the smallest viable set of memories rather than fragmenting.

The goal is a high signal-to-noise ratio in memory, not coverage of every turn.

---

## Phase 1: Audit (read-only)

Stay in plan mode for this entire phase. No writes.

### 1.1 Read existing memory

Use Glob to list every `.md` file in the memory folder. Then Read:

1. `MEMORY.md` (the index).
2. Every individual memory file.

Build a mental index of what is already covered, organized by topic. You need this to dedupe in Phase 2.

### 1.2 Read static team rules

Read these files so you do not propose memories that duplicate them, and so you can identify bubble-up candidates accurately:

- `CLAUDE.md` (project root).
- Every file under `.claude/rules/` (use Glob then Read).

If something the user said in this session is already a documented rule, it is not memory-worthy. The rule already covers it.

### 1.3 Detect compaction

Look for signals that earlier turns in this session have been compressed: gaps, summary blocks, "earlier turns omitted" markers, or your own context cues that the conversation has been compacted. If you see them, note this for the plan body so the user knows the audit may be partial.

### 1.4 Scan the conversation

Walk the transcript turn by turn. For each user turn, ask:

- Did the user correct me, contradict me, or push back? Candidate type: **feedback**.
- Did the user accept a non-obvious choice without pushback (a quiet confirmation)? Candidate type: **feedback** (validated approach).
- Did I learn something stable about who they are, their role, or how they work? Candidate type: **user**.
- Did I learn about an active initiative, deadline, stakeholder, or ongoing project? Candidate type: **project**.
- Did I learn about an external system (Asana ID, Slack channel, dashboard, internal URL)? Candidate type: **reference**.

For each candidate, also ask: **does this rule apply to anyone working on this project, or only to my collaboration with this user?** If it would apply to anyone — i.e. it is a project convention, code pattern, or architectural rule rather than a personal preference or collaboration style — tag the candidate as a **bubble-up candidate** in addition to its type. Bubble-up candidates are surfaced for the user to move into team-shared rules manually; they are NOT written to auto-memory by this skill.

Collect raw candidates. Filtering happens in Phase 2.

---

## Phase 2: Draft (still plan mode)

For each raw candidate from Phase 1:

### 2.1 Apply the skip filter

Reject anything that fails any of these gates:

- Derivable from current code, git history, or `git blame`.
- Already documented in `CLAUDE.md` or any `.claude/rules/` file.
- Already substantially covered by an existing memory file (more than ~80% topical overlap).
- Session-specific task state (current branch, in-progress edit, today's plan, the file we just edited).
- Would not change behavior in a future session.

If a candidate fails the filter, move it to the SKIPPED list with a one-line reason. Do not silently drop it; the user needs to see what you rejected so they can override.

### 2.2 Classify

Pick exactly one type per candidate: `user`, `feedback`, `project`, or `reference`.

### 2.3 Decide action

For each surviving candidate:

- **NEW**: no existing file covers this topic and the rule is personal / collaboration-scoped — write a new auto-memory file.
- **UPDATE**: an existing auto-memory file covers the topic and should be revised, extended, or corrected.
- **BUBBLE UP**: the rule is real and worth recording, but it belongs in team-shared memory (`CLAUDE.md` or `.claude/rules/`), not in private auto-memory. Surface as a suggestion; do not write a memory file.
- **SKIP**: filtered out in 2.1, or covered with no new information.

When deciding NEW vs UPDATE, see `references/memory-templates.md` § "Update vs new" for the heuristic.

When deciding NEW vs BUBBLE UP, ask:

- Would another developer on this team benefit from knowing this rule, even one who has never collaborated with the current user?
- Is the rule about project conventions, code patterns, or architecture (rather than communication style, personal taste, or workflow preferences specific to this user)?
- Could the rule be stated without referencing *me* / *the user* / *my preferences*?

If yes to any, prefer BUBBLE UP. The user can override on review.

### 2.4 Draft the body (NEW and UPDATE only)

Load `references/memory-templates.md` and draft each NEW or UPDATE body using the matching template. Required content:

- Frontmatter with `name`, `description`, `type`.
- Feedback and project memories MUST include `**Why:**` and `**How to apply:**` lines.
- User memories explain who the user is or how to collaborate with them.
- Reference memories include a pointer (URL, GID, or path) and a "consult when" line.

### 2.5 Prepare the bubble-up summary (BUBBLE UP only)

For each BUBBLE UP candidate, do NOT draft a memory body. Instead, prepare:

- A short rule statement (one sentence).
- A proposed location: `CLAUDE.md § {section name}` or `.claude/rules/{filename}.md`.
- A one-line reason for why it belongs there.
- The triggering session evidence (one or two lines of context so the user can verify).

### 2.6 Choose filename (NEW only)

Naming convention: `{type}_{snake_case_topic}.md`. Topic is short, descriptive, snake_case. Examples already in this folder: `feedback_grid_imports.md`, `feedback_person_picker_alias_guid.md`, `project_rx_prospecting_dataviews.md`.

### 2.7 Draft the MEMORY.md hook (NEW only)

One-liner under 150 characters in the form: `- [Title](file.md) one-line hook`. The hook should be useful at a glance; it is what your future self sees first.

---

## Phase 3: Review (call ExitPlanMode)

Present the plan body in this exact shape so the user can scan it quickly:

```
# Memory candidates from this session

## Compaction note
{Only if applicable. One sentence: "Earlier turns may be compressed. Add anything
I missed by hand if needed."}

## Summary
- {N} new memories proposed
- {N} updates to existing memories proposed
- {N} bubble-up candidates surfaced (suggestions for team-shared rules)
- {N} candidates skipped (listed at the bottom)

## NEW: feedback_xxx.md
**Filename:** feedback_xxx.md
**MEMORY.md hook:** - [Title](feedback_xxx.md) one-line hook

**Body:**
~~~markdown
---
name: ...
description: ...
type: feedback
---

{Full body, including **Why:** and **How to apply:**}
~~~

## UPDATE: feedback_yyy.md
**Existing content excerpt:**
> {abbreviated current content}

**Proposed change:**
> {what to add or rewrite, and why}

## BUBBLE UP (suggested for team-shared rules; not written by this skill)

### {short topic title}
**Proposed location:** CLAUDE.md § {section} | .claude/rules/{file}.md
**Rule:** {one-line summary}
**Why team-shared:** {one line: why this generalizes beyond the current user's collaboration}
**Session evidence:** {one or two lines of triggering context}
**Action requested:** Move to {location} via the `/memory` slash command or in the next PR. The `/remember` skill will not write to team-shared rules — that is an explicit, reviewable change.

## SKIPPED (with reason)
- {Topic}: covered by feedback_zzz.md
- {Topic}: derivable from code at path/to/file.cs
- {Topic}: session-specific task state
```

Then call `ExitPlanMode`. The harness will gate the auto-memory writes until the user approves.

---

## Phase 4: Execute (after approval)

Auto mode is fine here; the user has approved.

1. **Write each NEW file** with the Write tool, into the auto-memory folder.
2. **Edit each UPDATE target** with the Edit tool. Never overwrite an existing memory file with Write; always Edit. Preserve frontmatter unless the user approved a change to it.
3. **Update MEMORY.md.** For NEW entries, Edit the index to add the hook line under the right section (User / Feedback / Project / Reference). For UPDATE entries, leave MEMORY.md alone unless the title or one-line hook changed.
4. **Do NOT modify `CLAUDE.md` or `.claude/rules/`.** BUBBLE UP candidates are suggestions only. The user moves them via `/memory` or a PR. This is intentional — team-shared rules require team-visible review, and the skill should never auto-modify code that everyone reads.
5. **Confirm.** Output one short paragraph: `Saved N new, updated M, bubbled up K (not written), skipped L. Memory folder is now {X} files / ~{Y} KB.`

---

## Edge Cases

- **No candidates.** Present a plan that says "Nothing memory-worthy from this session." Call `ExitPlanMode` anyway so the user can push back if they think you missed something.
- **Compacted session.** Include the compaction note in the plan body. Do not silently miss the early part of the session.
- **Conflicting candidate.** A new candidate contradicts an existing memory. Treat as UPDATE and explicitly call out the contradiction in the proposed change: "Replaces prior guidance: 'X' becomes 'Y'. Reason: {new context}."
- **Candidate that could be both auto-memory and team-shared.** Default to BUBBLE UP and note it explicitly. The user can override and tell you to write it as private auto-memory if they prefer.
- **Bubble-up candidate that the user rejects.** If the user says "no, save it as private memory," reclassify as NEW and write the auto-memory file in Phase 4.
- **Scoped audit via $ARGUMENTS.** Honor the scope. Out-of-scope candidates appear in SKIPPED with reason "out of scope: {reason}" so the user can override.
- **User says "save everything" or similar override.** Lower the filter slightly, but do not abandon it. The "would this change behavior in a future session?" gate still applies. Memory bloat is the failure mode that makes memory systems quietly useless over time.

---

## Examples

### Example 1: Short session with one correction

User: "We use SlidingDateRangePicker for grid date filters now, not the WebForms-style two-date thing."

Plan body:

```
## NEW: feedback_use_sliding_date_range.md
{full body with Why and How to apply}

## SKIPPED
(none)
```

(In this case the existing memory already covers SlidingDateRange, so the skill should detect that and either UPDATE the existing file or SKIP with reason "covered by feedback_use_sliding_date_range.md".)

### Example 2: Long conversion session, partial compaction

Plan body opens with a compaction note, then:

```
## NEW: feedback_xyz.md
...
## UPDATE: feedback_dont_flag_speculative_bugs.md
**Proposed change:** add a third example: when a reviewer flags a missing
null check that the WebForms original also lacks, that is not a finding.

## BUBBLE UP (suggested for team-shared rules)

### IdKey is the canonical ID format in conversions
**Proposed location:** .claude/rules/data-model.md § "ID format in cross-block navigation"
**Rule:** When an Obsidian list block links into a still-WebForms detail block, update the WebForms block to accept idKey rather than reverting the Obsidian block to integer IDs.
**Why team-shared:** This is a project-wide convention every contributor will hit during conversion work; it is not specific to one developer's collaboration style.
**Session evidence:** User pushed back when I proposed changing the upstream block to emit integer IDs to satisfy a downstream WebForms consumer.
**Action requested:** Move into .claude/rules/data-model.md, or surface as a one-paragraph note in CLAUDE.md.

## SKIPPED
- "always run /build before committing": covered by CLAUDE.md
- "current PR branch is feature/xyz": session-specific task state
```

### Example 3: User scopes the audit

User: `/remember focus on the review feedback`

Plan body:

```
## NEW: feedback_review_xyz.md
...

## SKIPPED
- "Asana board GID for new project": out of scope (reference memory, not review feedback)
- "TS const naming preference": out of scope (general code style, not review feedback)
```

---

## Troubleshooting

- **Memory folder path is unclear.** The auto-memory system declares the path in your system prompt. If you cannot find it, ask the user to confirm before reading or writing.
- **Plan body is very long.** That is acceptable. The user is approving the actual text that will land on disk; truncating bodies defeats the purpose. If the plan exceeds ~2,000 lines, group related candidates and offer to split the approval into rounds.
- **User declines a candidate.** Honor it. Do not re-propose the same candidate later in the same session. If they decline with a reason, consider whether that reason itself is memory-worthy (a feedback memory about what NOT to save).
- **MEMORY.md grows past 200 lines.** The system prompt notes lines past 200 are truncated. Remove stale or redundant entries before adding new ones; consolidate where two entries cover the same rule.
- **Many bubble-up candidates pile up unactioned.** That is OK — the skill surfaces, the user actions on their own cadence. If the same bubble-up candidate appears in three consecutive sessions, raise that fact ("this has been suggested for `.claude/rules/` three times now — want to action it?") so the user is reminded.
