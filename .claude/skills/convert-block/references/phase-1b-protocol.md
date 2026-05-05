# Phase 1B Protocol — Parallel Research Fan-Out (adaptive)

Phase 1B is conditional. It fires only when the block's scope or size warrants the additional structured research artifacts. The trigger table below decides whether to fan out, and the subagent table decides which subagents fire.

---

## Trigger

Phase 1B (parallel research fan-out) fires if **any** of these is true:

| Signal | Threshold | How to count |
|---|---|---|
| `.ascx.cs` line count | > 1000 | `wc -l` on the file |
| Number of operating modes | ≥ 3 | A "mode" is a top-level UI state with its own panel or significant chrome change (view, edit, edit-attributes, add-modal, history-modal, approval-modal). Don't count error/loading sub-states. |
| Number of distinct entity types written-to | ≥ 4 | Read-only nav-property access doesn't count. An entity counts if `.SaveChanges()`, an explicit `.Add()`, an `.Update*()`, or a nested-collection mutation touches it. |
| Block has nested user controls (`.ascx` includes) | yes | grep the `.ascx` for `<%@ Register Src=` or `<UC:` |
| Figma URL was captured in 1A.0 | yes | scope includes redesign |
| Scope includes new features (1A.10 produced non-stub new-features.md) | yes | scope includes new features |
| User explicitly says "deep research" / "full audit" / similar in `$ARGUMENTS` | yes | string match on the argument |

If none trigger, **skip Phase 1B**. Phase 1A's parity-map.md plus `improvement-analysis.md` and `redundancy-report.md` (always produced even on small blocks; legacy issues exist there too) are the foundation. Other artifacts collapse to stubs that point back at parity-map.md.

State the decision explicitly in the Phase 1A summary:
> **Phase 1B:** [fired / skipped], [signal that triggered, or "below all thresholds"]

---

## Fan-out

**Parallelism is for COVERAGE, not speed.** Each subagent gets a focused prompt and a structured output template. Independent dimensions parallelize; dependent ones (state-machine derives from logic-graph reading) share an agent.

Spawn the surviving subagents (use the Agent tool with `subagent_type=general-purpose`, multiple invocations in a single message for parallelism). Each subagent reads its template first, then produces its artifact. State which subagents fired and which were pruned in the Phase 1B summary.

| Subagent | Output artifact | Template | Always run? | Skip when |
|---|---|---|---|---|
| Data Modeler | `data-model.md` | `references/working/data-model-template.md` | **Yes** | — |
| Improvement Analyst | `improvement-analysis.md` | `references/working/improvement-analysis-template.md` + `references/improvement-checklist.md` | **Yes** | — |
| Redundancy Detector | `redundancy-report.md` | `references/working/redundancy-report-template.md` | **Yes** | — |
| Obsidian Pattern Reviewer | `obsidian-pattern-analysis.md` | `references/working/obsidian-pattern-analysis-template.md`; reads `figma-design.md` if present | **Yes** | — |
| State & Logic Cartographer | `state-machine.md` + `logic-graph.md` | `references/working/state-machine-template.md`, `references/working/logic-graph-template.md` | No | Block has only one mode (single panel, no modals) |
| Completeness Sweep | `completeness-analysis.md` | `references/working/completeness-analysis-template.md` | No | parity-map.md Trace 7 covers it |
| Edge-Case Hunter | `edge-cases.md` | `references/working/edge-cases-template.md` | No | parity-map covers all branches AND block has no user-supplied input beyond IDs |
| Test Scenario Deriver | `test-scenarios.md` | `references/working/test-scenarios-template.md` | No | Simple list block where parity map is sufficient input for /review-conversion |

Every subagent prompt must brief the agent with:
- The block path and line count
- The /working/ folder path to write into
- The path to its template

After all subagents return, **reconcile**: scan the artifacts for contradictions or duplications. Resolve the larger issue inline; flag the rest in `clarifying-questions.md` for Phase 2.

---

## Phase 1B Quality Gate

- [ ] Every applicable artifact written (or stubbed with one-line justification)
- [ ] Sibling-block scan recorded in `data-model.md` (linked-to blocks identified, their state checked, ID-format mismatches surfaced)
- [ ] Reconciliation complete; contradictions resolved or routed to `clarifying-questions.md`

If you find an out-of-scope issue (e.g., the redundancy detector spots the same legacy bug in a different block), flag it as a separate task instead of widening this conversion's scope. If `mcp__ccd_session__spawn_task` is available, use it; otherwise note the find in `improvement-analysis.md` under "Out-of-scope items" so the user can spin up follow-up work manually.
