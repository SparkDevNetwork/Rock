# clarifying-questions.md Template

The audit trail of every design decision the user made during Phase 2. Preserves the user's choices in case you need to revisit a decision later, and gives the post-conversion archived spec a clean record of "what was asked and what the user said".

---

## Output location

`/working/{block-name-kebab}/clarifying-questions.md`

After the conversion ships, `/review-conversion` appends a `## Verification (review-conversion, ...)` section to this file recording whether the code honored each user decision (Honored / Violated / Deferred). That section is review's territory — do not pre-populate it during convert-block phases.

---

## Format

A numbered list. Each entry has three parts:

```
Q1, View panel fields
   Asked: I'd show Name, Description, IsActive, Campus. WebForms also shows InternalCode and CreatedDate.
          Should I include InternalCode? Should I include CreatedDate?
   Proposed: exclude both
   Answered: 2026-05-04, confirmed exclude both. Reasoning: InternalCode is admin-only data, CreatedDate is audit-table info that doesn't help end users.

Q2, Grid filters
   Asked: WebForms has 3 server-side filters (Status, Campus, Date Range) that reduce the DB query. Keep all server-side?
   Proposed: yes
   Answered: 2026-05-04, yes, keep all three. Add a fourth on RegistrationCount > 0 if cheap.
   Follow-up: RegistrationCount filter requires a new computed field on the bag; documented in improvement-analysis.md I7.

Q3, Hardcoded CategoryId == 5 at line 142
   Asked: WebForms filters by CategoryId == 5 (hardcoded). Intentional or bug?
   Proposed: replace with block setting
   Answered: 2026-05-04, keep hardcoded. It's an organization-wide convention; making it configurable would invite drift.
```

Required fields per entry:
- **#** stable ID (`Q1`, `Q2`, ...)
- **Asked** the actual question presented to the user (verbatim or near-verbatim from the Phase 2 prompt)
- **Proposed** the answer the model proposed
- **Answered** date and the user's resolution; capture any reasoning the user gave

Optional:
- **Follow-up** anything the answer triggered downstream (e.g., a new row in `improvement-analysis.md`)

---

## What goes here vs elsewhere

| Question type | Goes in |
|---|---|
| Design decisions the user makes during Phase 2 | here |
| Implementation details that are unambiguous from the code | NOT a question; just decide and document in plan.md |
| Bugs / improvements / redundancies the model identified | `improvement-analysis.md` and `redundancy-report.md`; these are not questions |
| Things the user deferred ("decide later") | here, with status `Deferred, see plan.md §6 Open Issues` |
| Things the user explicitly skipped ("ignore this question; carry forward as-is") | here, with that status, it's the audit trail of the explicit non-decision |

---

## Re-opening a question during implementation

If a checkpoint or implementation step surfaces something that contradicts an answered question (e.g., "user said server-side filters but the bag shape can't carry the preferred PreferenceKey serialization"), do not silently revise the design. Re-ask:

```
Q2, Grid filters (RE-OPENED 2026-05-04 during implementation)
   Original: yes, keep all three server-side
   New context: the SlidingDateRange preference doesn't round-trip cleanly through the existing bag shape.
                Two options: (a) add SlidingDateRangeBag to the bag, (b) drop date range to a column filter.
   Re-asked: which approach?
   Answered: (a), add the bag field. Documented in plan.md §3 file changes.
```

The original Q stays; the re-opening is appended. Future readers can see both rounds.

---

## Phase 2 doesn't always need every question

If the block is genuinely simple and the WebForms code is unambiguous, Phase 2 can be a one-line presentation: "Block design is straightforward; no clarifying questions. Confirm to proceed to plan?" In that case `clarifying-questions.md` is a single line:

```
No clarifying questions; the user confirmed the proposed design without changes.
```

Don't manufacture questions to fill the file.
