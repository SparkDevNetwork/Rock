# Conversion Example — Detail Block with Figma Redesign + New Features

End-to-end example for a conversion where the user attaches a Figma URL and the design implies new functionality. This shows the 1A.0 → 1A.9 → 1A.10 path and how `figma-design.md` and `new-features.md` flow through the rest of the phases.

---

## Trigger

User invokes:

```
/convert-block Cms/ContentChannelItemDetail https://figma.com/design/abc123/CMS-Refresh?node-id=1-234

Please also add inline comments per the Figma frame "Comments" and a real-time
status badge that polls every 30 seconds.
```

---

## Phase 1A.0: Scope detection

The skill detects:
- Translation: yes (the .ascx.cs exists)
- Redesign: yes (Figma URL captured)
- New features: yes ("add inline comments", "real-time status badge that polls")

**Scope:** translation + redesign + new features. Phase 1B fires regardless of size.

---

## Phase 1A.9: Figma read

Uses Figma MCP tools:
- `get_design_context` returns 12 frames; 5 are in scope, 7 are for sibling blocks or future state
- `get_screenshot` saved to `/working/content-channel-item-detail/figma/`:
  - `view-detail.png`, `edit-detail.png`, `comments-tab.png`, `version-history.png`, `mobile-view.png` (out of scope)
- `get_variable_defs` extracts 14 design tokens; 11 map to existing Rock CSS vars, 3 require `/css-cleanup` follow-up

Writes `/working/content-channel-item-detail/figma-design.md`. Excerpt:

> ### 5. Frame-to-panel mapping
>
> | Frame | Implements | Target file |
> |---|---|---|
> | FR1 (View — Detail) | view panel | `viewPanel.partial.obs` |
> | FR2 (Edit — Detail) | edit panel | `editPanel.partial.obs` |
> | FR3 (Comments tab) | new feature N1 | `commentsPanel.partial.obs` (block-local; new for this conversion) |
> | FR4 (Version history) | existing modal | `versionHistoryModal.partial.obs` (carry-forward) |
>
> ### 6. Behaviors implied by the design but NOT in WebForms
>
> | Behavior | Frame source | new-features.md row |
> |---|---|---|
> | Inline comment editor | FR3 | N1 |
> | Real-time status badge | FR1 (annotation) | N2 |

---

## Phase 1A.10: New features

Writes `/working/content-channel-item-detail/new-features.md`:

> | # | Feature | Source | In-scope for this PR? | Acceptance criteria |
> |---|---|---|---|---|
> | N1 | Inline comment editor | Figma FR3 + user prompt | TBD (Phase 2) | New entity ContentChannelItemComment; add/edit/delete with auth; comments shown in view mode |
> | N2 | Real-time status badge | Figma FR1 + user prompt | TBD (Phase 2) | Status badge polls every 30s; reflects entity status changes without page reload |

---

## Phase 1A summary

> **Scope:** translation + redesign + new features
> **Figma URL:** https://figma.com/design/abc123/CMS-Refresh?node-id=1-234
> **Block:** ContentChannelItemDetail, 2014 lines (.ascx.cs)
> **/working/ folder:** `/working/content-channel-item-detail/`
> **Classification:** Detail
> **Base class:** `RockEntityDetailBlockType<ContentChannelItem, ContentChannelItemBag>` with `IBreadCrumbBlock`
> **parity-map.md:** 87 rows across 7 traces, +2 rows in Trace 8 (N1, N2)
> **figma-design.md:** 5 frames in scope, 7 frames out of scope
> **new-features.md:** 2 candidate features (N1, N2)
> **Phase 1B:** fired (Figma URL present + 2014 lines > 1000 + 6 modes ≥ 3)

---

## Phase 2 clarifying questions (Figma-aware)

> 1. **Figma frame coverage**, `figma-design.md` flagged 5 frames in scope (FR1-FR5). FR6-FR12 are mobile / sibling-block / marketing-comp frames I'm treating as out of scope. Confirm? **[Proposed: yes]**
> 2. **N1 inline comments scope for this PR**, full implementation requires a new entity, migration, and security model. Ship in this PR (large) or split into a follow-up? **[Proposed: ship in this PR; user requested it explicitly and it's the central new feature]**
> 3. **N2 real-time status polling scope for this PR**, polling adds load and may need caching. Ship in this PR or follow-up? **[Proposed: follow-up; needs perf review separate from this conversion]**
> 4. **Carry-forward conflicts with redesign**, FR2 (edit panel) drops the WebForms "Internal Slug" field — design shows it auto-generated. Confirm dropping? **[Proposed: yes, design moves to auto-generation; existing slugs preserved on edit]**

User answered: 1 yes; 2 yes (in this PR); 3 follow-up (spawned task: "Implement real-time status polling for ContentChannelItem"); 4 yes (drop manual slug input).

`new-features.md` updated:
- N1: In-scope = Yes
- N2: In-scope = No (deferred to follow-up issue)

---

## Phase 3: Plan §2 — design decisions (excerpt)

```
§2 Key Design Decisions

Carry-forward and improvements:
- View/edit bag split: view-only excludes RawTemplateContent, ItemGlobalKey (cite clarifying-questions.md Q1)
- Replace manual nav-tabs with TabbedContent (cite obsidian-pattern-analysis.md §2)
- Move Save/Cancel to root DetailBlock footer slots (cite obsidian-pattern-analysis.md §5)

Redesign decisions (cite figma-design.md):
- Frame-to-panel mapping per figma-design.md §5
- Status badge component swap: HighlightLabel → custom AnimatedStatusBadge (block-local)
- Drop "Internal Slug" manual input — design auto-generates (cite clarifying-questions.md Q4)

New-feature scope (cite new-features.md):
- N1 (inline comments) IN SCOPE: new entity ContentChannelItemComment, add/edit/delete block actions,
  commentsPanel.partial.obs, EF migration adds the table
- N2 (real-time status polling) DEFERRED to follow-up issue (Phase 2 Q3)
```

---

## Phase 3: Plan §4 — implementation (excerpt)

Step 7.5 fires because new-features.md has an in-scope row:

```
Step 7.5: Implement Obsidian-only behaviors
- N1 (inline comments) implementation:
  - Entity: Rock/Model/CMS/ContentChannelItemComment/ContentChannelItemComment.cs
  - Migration: Rock.Migrations/Migrations/202605..._AddContentChannelItemComment.cs
  - Block actions: AddComment, EditComment, DeleteComment in C# block
  - Frontend: commentsPanel.partial.obs (cites figma-design.md FR3)
  - Acceptance criteria from new-features.md N1 drive verification
```

---

## Phase 4: Implementation + checkpoint with new-feature verification

Final checkpoint (Step 10) reads all /working/ artifacts and verifies:
- Trace 8 row T8-1 (N1 implementation) matches the C# entity, migration, and Vue component
- Acceptance criteria from new-features.md N1 are testable in code
- /css-cleanup follow-up flagged for the 3 design tokens that don't map to existing Rock vars

`/review-conversion` then verifies the same artifacts post-conversion.
