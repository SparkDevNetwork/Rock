# Review Findings — Crm/Disc (WebForms → Obsidian)

**Reviewed:** 2026-06-18
**Mode:** Fresh-eye audit (no `/working/disc/` artifacts present; no archived spec)
**Verdict:** NEEDS FIXES — 3 critical, 3 warning, 9 note
**Update 2026-06-18:** All 3 critical findings FIXED in review (C1, C2, C3). 3 warnings + 9 notes remain open.

Files reviewed:
- `Rock.Blocks/Crm/Disc.cs`
- `Rock.ViewModels/Blocks/Crm/Disc/DiscBox.cs` (DiscInitializationBox, AssessmentResponseBag, AssessmentResultsBag)
- `Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Crm/Disc/*.d.ts`
- `Rock.JavaScript.Obsidian.Blocks/src/Crm/disc.obs`
- Ground truth: `RockWeb/Blocks/Crm/Disc.ascx[.cs]`, `Rock/Model/CRM/Assessment/DiscService.cs`

---

## Functional Parity Table

| # | WebForms behavior | Obsidian equivalent | Verdict |
|---|---|---|---|
| M1 | `OnInit` resolves target person (action identifier / url key / CurrentPerson) | `TargetPerson` property, same resolution | Matched |
| M2 | `OnInit` → `RockPage.AddCSSLink("~/Styles/Blocks/Crm/Disc.css")` | `<style scoped>` block ported into `disc.obs` | Matched (fixed in review) |
| M3 | `SetPanelTitleAndIcon` from attributes | `panelTitle` / `panelIcon` computed | Matched |
| M4 | `ShowAssessment` use-case branching (complete/pending/error) | `GetDiscBox()` — same branch logic | Matched |
| M5 | `ShowInstructions` resolves Lava merge fields | `GetDiscInstructions` + `v-html` | Matched |
| M6 | `ShowQuestions` builds randomized responses | `GetAssessmentResponses` (same shuffle) | Matched |
| M7 | `btnStart_Click` sets StartDateTime = now | `onStartClick` toggles sections; does **not** set start time | Differs (N1) |
| M8 | `btnNext_Click` paginate / score+save | `onNextClick` / `onFinishClick` → `Save` | Matched |
| M9 | `btnPrevious_Click` capture answers + page back | `onPreviousClick` (answers held live by v-model) | Matched |
| M10 | `btnRetakeTest_Click` resets to instructions, forces new assessment | `onRetakeClick` + `IsRetake` flag → new assessment | Matched (fixed in review) |
| M11 | `ShowResult` heading / print tip | `targetPersonName`, print-tip `v-if` | Matched |
| M12 | `ShowResult` assessment-date label ("Assessment Date: {shortdate}") | `<HighlightLabel v-model>` — slot-only control, renders empty | **Differs (W1)** |
| M13 | `ShowResult` retake eligibility | `canRetakeTest` (same predicate) | Matched |
| M14 | `PlotGraph` natural-score bars + primary highlight | computed scores/percents/`*IsPrimary` | Matched (chart CSS missing — C2) |
| M15 | `ShowExplaination` Literal (raw HTML) descriptions | `{{ }}` text interpolation | **Differs (W2)** |
| Q1 | Scoring counts most/least D/I/S/C across all responses | `SaveAssessment` identical counts | Matched |
| Q2 | `DiscService.Score/Save/LoadSavedAssessmentResults` | reused unchanged | Matched |
| S1 | No explicit IsAuthorized (self-service link-based) | Same | Matched |
| S2 | (none) RequiresRequest re-check on save | Added guard (good) — but `== 0` misses null | Differs (W3) |
| Z1 | `btnNext_Click` try/catch → user-facing error + `LogException` | `Save` returns `ActionBadRequest` on failure → error shown | Matched (fixed in review) |
| Z2 | ViewState `AssessmentState` / `StartDateTime` | client `responses` ref + `box.startDateTime` round-trip | Matched |
| P1 | Page params AssessmentId / Person | `PageParameterKey` same | Matched |
| U1 | per-question auto-scroll on select; Alt+arrow shortcuts | not replicated | Differs (N5) |

---

## Critical

> **All three critical findings were fixed during review on 2026-06-18.** See the FIXED note under each.

### C1 — "Retake Test" button is dead — ✅ FIXED
`Rock.JavaScript.Obsidian.Blocks/src/Crm/disc.obs:109`
WebForms `btnRetakeTest_Click` (`Disc.ascx.cs:466`) set `hfAssessmentId=0` and re-showed instructions so the next save created a *new* assessment. The Obsidian `<RockButton>Retake Test</RockButton>` has no `@click`, so clicking does nothing. Underlying gap: even if wired, the server `Save` reuses the `AssessmentId` page parameter (`Disc.cs:440`), so a retake would overwrite the existing assessment instead of creating a new one — there's no client-controllable "this is a retake" signal equivalent to the old hidden field.
Fix: add `onRetakeClick` that clears results/`personalityType`, resets section visibility to instructions, and re-shows fresh questions; and give `Save` a way to force a new assessment on retake (e.g. a `box.IsRetake` flag, or clear the reused id).
**FIXED:** Added `IsRetake` to `DiscInitializationBox` (+ `.d.ts`); wired `@click="onRetakeClick"` on the button (`disc.obs:109`); `onRetakeClick` clears `personalityType`/`results`, blanks responses, resets to page 1, shows instructions, and sets `isRetake`; `onFinishClick` sends `boxToSend.isRetake`; `SaveAssessment` skips the page-param id reuse when `box.IsRetake` so a new assessment is created (`Disc.cs`).

### C2 — DISC results bar chart has no styles in Obsidian — ✅ FIXED
`disc.obs:65-80` (chart markup); CSS at `RockWeb/Styles/Blocks/Crm/Disc.css`
The `.discchart` / `.discbar` / `.discbar-primary` / `.discchart-midpoint` / `.discbar-label` rules exist **only** in `Disc.css`, which the WebForms block loaded via `AddCSSLink` (`Disc.ascx.cs:222`). The Obsidian block never loads it and has no `<style>` block, and the classes are not in `_blocks-crm.less` or `styles-v2`. The bars set inline `height`/`title` but lose the container height (425px), `inline-block` layout, background color, midline, and the `::before` score label — so the results chart renders broken. (The question-table classes `disc-assessment`/`disc-question`/`grid-select-field` *are* global, so only the chart is affected.)
Fix: port the chart rules into a `<style>` block in `disc.obs` (or into a bundled stylesheet).
**FIXED:** Added a `<style scoped>` block to `disc.obs` porting the `.discchart`/`.discbar`/`.discchart-midpoint`/`.discbar-label`/`.discbar-primary` rules from `Disc.css`.

### C3 — Save failure swallows the error and strands the user on a blank panel — ✅ FIXED
`Rock.Blocks/Crm/Disc.cs:464-467`
WebForms wrapped scoring/save in try/catch and on failure showed "Something went wrong while trying to save your test results." plus `LogException` (`Disc.ascx.cs:381-387`). Obsidian's `SaveAssessment` catch only does `Logger.LogError("", ex)` and swallows. `Save` then returns `ActionOk(box)` with no results and no `errorMessage`. Client-side `onFinishClick` sees success, hides instructions+questions, and `isResultsSectionVisible` is false (empty `personalityType`) — leaving an empty panel with no feedback and the user's answers lost. This is a regression from WebForms.
Fix: on catch, set `box.ErrorMessage` (or return `ActionBadRequest`) so the UI surfaces an error; log with a real message / `ExceptionLogService`.
**FIXED:** `SaveAssessment` now returns `bool` (logs via `Logger.LogError(ex, "Unable to save the DISC assessment results.")` and returns `false` on failure); `Save` returns `ActionBadRequest("Something went wrong while trying to save your test results.")` when it fails, so `onFinishClick` surfaces the error and leaves the questions visible.

---

## Warning

### W1 — Assessment date never displays
`disc.obs:7`, `disc.obs:222`
`<HighlightLabel labelType="info" v-model="assessmentDate" />` — `HighlightLabel` exposes a **default slot only** (no `modelValue` prop/emit, verified in `highlightLabel.obs`), so the bound date never renders. WebForms showed `"Assessment Date: {LastSaveDate.ToShortDateString()}"`. Both the value and the label/format are lost.
Fix: `<HighlightLabel v-if="assessmentDate" labelType="info">Assessment Date: {{ formattedDate }}</HighlightLabel>`, formatting `lastSavedDate` to a short date.

### W2 — Result descriptions render as escaped text, not HTML
`disc.obs:83,86,89,93,96,99,102,105`
Description / Strengths / Challenges / Under Pressure / Motivation / Team Contribution / Leadership Style / Follower Style use `{{ }}` interpolation. WebForms rendered each via `<asp:Literal>` (raw HTML) — `Disc.ascx.cs:782-790`. The shipped DISC Results DefinedValue descriptions contain HTML, so they'll show literal markup. These are admin/system-managed DefinedValue attributes (trusted), so HTML rendering matches intent.
Fix: render with `v-html` (matching the WebForms Literal behavior).

### W3 — RequiresRequest save guard misses the null AssessmentId case
`Rock.Blocks/Crm/Disc.cs:565`
`if ( AssessmentId == 0 && assessmentType.RequiresRequest && !hasAssessment )` — `AssessmentId` is `int?`; when the page param is absent it is `null`, so `== 0` is false and the guard (whose comment is explicitly anti-manipulation) is skipped. A crafted request with no `AssessmentId` for a RequiresRequest type could still save.
Fix: `AssessmentId.GetValueOrDefault() == 0` (or `AssessmentId.ToIntSafe() == 0`).

---

## Note

- **N1** `Disc.cs:230` StartDateTime is captured at page load, not on Start click (WebForms `btnStart_Click`), so the saved `TimeToTake` includes instruction-reading time — and contradicts the bag doc ("should be after clicking start"). Set `box.startDateTime` in `onStartClick`.
- **N2** No block-instance swap/chop migration from the WebForms type `A161D12D` (`BlockType.DISC`) to the new Obsidian type `F9261A63`. The CodeGenerated v17 migration only *registers* the new type; existing DISC page placements still point at the WebForms block. Confirm a chop migration is planned before the `.ascx` is removed.
- **N3** `disc.obs:44,49` radio `v-if="response.mostScore !== null"` / `leastScore !== null` are effectively always true (values normalized to `undefined` at init, never set to `null`). Dead/confusing — remove.
- **N4** `disc.obs:279-288` tie handling: every score equal to the max gets `discbar-primary`; WebForms (`DiscService.PlotOneGraph`) highlights only the first max.
- **N5** Lost minor affordances: per-question auto-scroll on selection and Alt+◄/► keyboard shortcuts (`Disc.ascx:206-207`), `DataLoadingText`.
- **N6** Style: inline `v-on:update:modelValue` handlers (project preference is watchers); `type="submit"` on `<a>` is meaningless; Retake uses `<RockButton>` while Start/Next/Finish use raw styled anchors (inconsistent).
- **N7** CodeGenerated migration sets `SetPageIcon` default `"fa fa-chart-bar"` while the block attribute default is `"ti ti-chart-bar"` (`Disc.cs:52`) — stale.
- **N8** `Disc.cs:466` `Logger.LogError("", ex)` uses an empty message; WebForms used `LogException`. Give it a real message / route to the Exception Log.
- **N9** `[DisplayName]` changed "DISC" → "Disc" (cosmetic block-library name change).

---

## Manual verification suggested

No `test-scenarios.md` exists. After fixes, manually exercise: (a) complete a fresh assessment as the current person and confirm the bar chart renders styled; (b) click Retake and confirm a new assessment is created; (c) force a save error and confirm an error message shows; (d) view a completed result and confirm the assessment-date label and HTML descriptions render; (e) open as a different person via a `Person` link for both completed and pending-with/without-prior-completed cases.
