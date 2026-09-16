# Memory Templates and Conventions

Body templates and decision rules for the `/remember` skill. Load before drafting candidate memories in Phase 2.

---

## File Structure

Every memory file lives directly in the auto-memory folder (no subdirectories) and follows this shape:

```markdown
---
name: {short title, sentence case}
description: {one-line description used to decide relevance in future conversations}
type: {user | feedback | project | reference}
---

{body, structured per type below}
```

The frontmatter `description` is the single most important field. It is what your future self uses to decide whether to load this memory in a new conversation. Be specific. Avoid generic phrasing like "preferences about X"; prefer "rule: do Y instead of Z".

---

## Type 1: `user`

Captures who the user is, their role, expertise, responsibilities, and how they prefer to collaborate.

### Template

```markdown
---
name: {who they are or how they work}
description: {one-line description}
type: user
---

{2-4 sentences describing the user's role, expertise, or collaboration preference. State the fact directly. No Why/How block required.}
```

### Realistic example

```markdown
---
name: Maxwell's role and Rock RMS depth
description: Rock RMS contributor at Triumph Tech, deep familiarity with Obsidian conversions and the convert-block skill
type: user
---

Maxwell is a contributor to Rock RMS at Triumph Tech (email maxwell@triumph.tech).
His current focus is the Obsidian conversion effort: tens of WebForms blocks
already converted via the `/convert-block` skill, with strong opinions formed about
patterns. Frame block-related explanations assuming familiarity with the conversion
playbook; do not over-explain WebForms-to-Obsidian translation basics.
```

### When to write a user memory

- The user states their role, team, or area of focus.
- The user describes how they prefer to receive information (terse, with code samples, with tradeoffs, etc.).
- The user reveals a specialty or knowledge gap that should shape future explanations.

### When NOT to write a user memory

- Anecdotal information that does not generalize (a one-off comment about their morning).
- Anything already in the existing user memory; UPDATE instead.

---

## Type 2: `feedback`

Captures corrections, course adjustments, and validated non-obvious approaches. This is the most common type and the highest-value type for a long-running project.

### Template

```markdown
---
name: {the rule, in one short phrase}
description: {one-line description that names the rule clearly}
type: feedback
---

{The rule itself, in one or two sentences. Lead with the rule, not the context.}

**Why:** {The reason the user gave. Include the incident or strong preference behind the rule. Without this, future-you cannot judge edge cases.}

**How to apply:** {When and where this rule kicks in. Be specific about the scenario so the rule does not over-apply.}
```

### Realistic example

```markdown
---
name: Don't flag speculative bugs in reviews
description: Only list findings with a concrete repro; "WebForms did it too" is strong evidence a pattern is fine
type: feedback
---

When reviewing converted Obsidian blocks, only flag a finding if there is a
concrete reproduction path. Do not list speculative or hypothetical bugs.

**Why:** Speculative findings inflate review reports, waste the reviewer's time
ranking what is real, and make Maxwell distrust the next review. If the WebForms
original had the same pattern and shipped without incident, that is strong
evidence the pattern is acceptable.

**How to apply:** During `/review-conversion` and any other code review pass.
For each candidate finding, ask: "Can I describe the steps that would trigger
this bug today?" If no, drop it from the report or move it to a "low-confidence"
section explicitly labeled as such.
```

### When to write a feedback memory

- The user corrected an approach you took: "no, do it this way", "don't do X", "stop X".
- The user accepted a non-obvious choice without pushback ("yeah, the bundled PR was right").
- The user explained their reasoning behind a decision in a way that should generalize.

### When NOT to write a feedback memory

- One-off task-specific correction ("this variable should be `count` not `n`" in a single function).
- Style guidance that is already in `CLAUDE.md` or `.claude/rules/`.
- A correction that contradicts an existing feedback memory; UPDATE the existing memory instead.
- A correction that arguably belongs in team-shared rules — surface as a BUBBLE UP candidate instead. See § "Bubble-up vs auto-memory" below.

---

## Type 3: `project`

Captures ongoing initiatives, deadlines, stakeholders, and decisions whose context is not in the code or git history.

### Template

```markdown
---
name: {the initiative or fact}
description: {one-line description}
type: project
---

{The fact or decision, in one or two sentences. Always convert relative dates to absolute (e.g. "Thursday" becomes "2026-05-08").}

**Why:** {The motivation. Often a constraint, deadline, stakeholder ask, or compliance requirement.}

**How to apply:** {How this should shape future suggestions. Project memories decay quickly, so the why matters; future-you needs to judge whether the memory is still load-bearing.}
```

### Realistic example

```markdown
---
name: Merge freeze for mobile release cut
description: Non-critical merges paused after 2026-05-08 for the mobile team's release branch
type: project
---

A merge freeze for non-critical work begins 2026-05-08. The mobile team is
cutting a release branch and needs main quiet through cut.

**Why:** Mobile cuts a stable branch from main, and ad-hoc unrelated merges
during the cut window create avoidable conflicts and risk shipping unintended
changes.

**How to apply:** Flag any non-critical PR or merge work scheduled after
2026-05-08 in this conversation. Critical bug fixes and security patches still
go through. The freeze ends once the mobile team confirms the cut is clean.
```

### When to write a project memory

- A specific deadline, freeze window, or release date.
- The motivation behind a refactor or initiative ("the auth rewrite is driven by compliance, not tech debt").
- Who is responsible for what in an ongoing initiative.

### When NOT to write a project memory

- Today's task or this PR's scope; that is conversation context, not memory.
- Code architecture facts derivable from the repo.

---

## Type 4: `reference`

Pointers to where information lives in external systems (Asana, Slack, dashboards, internal wikis).

### Template

```markdown
---
name: {what the reference is}
description: {one-line description that names the system and its purpose}
type: reference
---

{What the reference is, where it lives (URL, GID, channel name, file path), and what it contains.}

**Consult when:** {The trigger condition, e.g. "the user mentions a pipeline bug" or "you need to check oncall latency".}
```

### Realistic example

```markdown
---
name: Asana project for Obsidian block conversions
description: Backlog of WebForms blocks queued for Obsidian conversion lives in this Asana project
type: reference
---

The Obsidian conversion backlog lives in Asana project GID `1208355424236487`,
specifically the "Ready for pipeline" section. Each card represents a WebForms
block awaiting conversion and is prioritized by `.ascx.cs` line count under 1,000.

**Consult when:** Maxwell mentions picking the next block to convert, asks
"what's next in the queue", or references a specific Asana ticket without
giving you the GID.
```

### When to write a reference memory

- The user mentions an external system as authoritative ("bugs are tracked in Linear project X", "oncall watches dashboard Y").
- The user gives you a URL, GID, channel name, or path that you should consult later.

### When NOT to write a reference memory

- A URL the user shared once for an in-conversation lookup with no expectation of future relevance.
- Internal Rock paths (those go in code/CLAUDE.md, not memory).

---

## Bubble-up vs auto-memory

When a candidate is real and worth recording, decide whether it belongs in private auto-memory (NEW / UPDATE) or in team-shared rules (BUBBLE UP).

| Question | Auto-memory if... | Bubble-up if... |
|---|---|---|
| Who benefits from the rule? | Only this user benefits | Any contributor on the project benefits |
| What does the rule describe? | Personal preference, communication style, individual workflow | Project convention, code pattern, architecture rule |
| Could the rule be stated without "I" / "the user" / "my preferences"? | No | Yes |
| Is there an existing entry in `CLAUDE.md` § / `.claude/rules/` for this topic area? | No, and it is calibrated to this user | Yes — the rule extends or sharpens that entry |

When in doubt, prefer BUBBLE UP and let the user override. A team-shared rule that gets demoted to private memory is cheap; a private memory that should have been team-shared rots silently in someone's auto-memory folder.

The skill never writes to `CLAUDE.md` or `.claude/rules/`. Bubble-up candidates are surfaced as suggestions; the user moves them manually via `/memory` or a PR.

---

## Skip Criteria with Examples

Each candidate must pass these gates. If it fails any gate, move it to SKIPPED with the reason.

### Gate 1: Not derivable from code or git

**Skip:** "Rock uses `RockDateTime` instead of `DateTime`." (Already a CLAUDE.md rule. Derivable.)

**Save:** "Maxwell prefers row-click-to-edit only for blocks that already use it; new conversions should match the WebForms UI flow."

### Gate 2: Not already in CLAUDE.md or .claude/rules/

**Skip:** "Always use braces, even for single-line if statements." (Already in CLAUDE.md § Code Style.)

**Save:** "When the WebForms original lacked a null check that produces no observable bug, do not add the null check during conversion; carrying forward the pattern is acceptable."

### Gate 3: Not already covered by an existing memory file

**Skip:** "PersonPicker emits a PersonAlias Guid, not a Person Guid." (Already in `feedback_person_picker_alias_guid.md`. UPDATE if there's a new facet, otherwise SKIP.)

**Save:** A genuinely new rule on a topic with no current memory file.

### Gate 4: Not session-specific task state

**Skip:** "We are currently working on the FoobarList block." (Conversation context, not memory.)

**Save:** "Block conversions in the Finance domain typically need a separate refund-handling pattern" (generalizes beyond the current task).

### Gate 5: Would change behavior in a future session

If you cannot articulate how a future Claude conversation would behave differently because of this memory, do not save it.

**Skip:** "Maxwell prefers a 4-space indent." (TS/C# tooling already enforces this. Saving it changes nothing.)

**Save:** "When applying review-conversion findings, fix only what the user explicitly approves and stop; do not auto-fix the entire report." (This will change how I behave in future review sessions.)

---

## Naming Conventions

Filename pattern: `{type}_{snake_case_topic}.md`.

- `type` is one of: `user`, `feedback`, `project`, `reference`.
- `topic` is short, descriptive, snake_case, no version numbers, no dates in the filename.
- Avoid generic topics; prefer the specific rule. `feedback_grid_imports.md` is better than `feedback_imports.md`.

Examples drawn from the existing folder (good patterns to match):

- `feedback_grid_imports.md`
- `feedback_person_picker_alias_guid.md`
- `feedback_dont_flag_speculative_bugs.md`
- `feedback_use_sliding_date_range.md`
- `project_rx_prospecting_dataviews.md`

---

## Update vs New

Use this heuristic when an existing memory file covers a related topic:

| Situation | Decision |
|---|---|
| Same rule, same scope, no new info | SKIP. Already covered. |
| Same rule, new edge case or example | UPDATE. Add the new facet to the existing file. |
| Same rule, contradicting new guidance | UPDATE. Rewrite the rule with the new guidance and note the change in the body ("Replaces prior guidance: X becomes Y. Reason: ..."). |
| Related topic, distinct rule | NEW. Create a separate file with a distinct topic name. |
| Different topic | NEW. |

When in doubt, prefer UPDATE over NEW. Memory bloat is harder to clean up than memory consolidation.

---

## MEMORY.md Hook Style

The MEMORY.md index file holds one-line pointers to each memory file. Format:

```
- [Title](filename.md) one-line hook
```

Constraints:

- One line per entry. No multi-line entries.
- Total length under 150 characters.
- Title in brackets: short, sentence-cased, references the rule or fact.
- Hook after the link: one phrase that helps you decide whether to load the file. Avoid restating the title.
- No em-dashes (project rule). Use a separator like a colon, parentheses, or just a space if it reads cleanly.

Good examples (matching the existing index):

```
- [Don't flag speculative bugs in reviews](feedback_dont_flag_speculative_bugs.md) only list findings with a concrete repro
- [Grid imports on one line](feedback_grid_imports.md) keep Grid destructured imports horizontal, not vertical
- [Cache GetBlockPersonPreferences()](feedback_cache_block_person_preferences.md) lazy-resolve PersonPreferences once via a property
```

Place new entries under the right section in MEMORY.md (User / Feedback / Project / Reference). Keep the section ordering stable.
