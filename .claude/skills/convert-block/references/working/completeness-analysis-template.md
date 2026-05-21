# completeness-analysis.md Template

Implicit / hidden / non-obvious behavior the parity map flattened. The point is to surface things that *could* have been parity-table rows but didn't fit the row shape, plus the second-sweep findings that would otherwise go uncaptured.

The shorthand: parity-map.md captures behavior the source code states explicitly; completeness-analysis.md captures behavior the source code achieves through omission, side effect, or convention.

---

## Output location

`/working/{block-name-kebab}/completeness-analysis.md`

After the conversion ships, `/review-conversion` appends a `## Verification (review-conversion, ...)` section to this file with audit verdicts per `C{N}` row (whether the subtle/hidden behavior is preserved in Obsidian). That section is review's territory — do not pre-populate it during convert-block phases.

---

## When to write a full completeness-analysis.md

Always full if Phase 1B fires. Stub allowed when the block is small AND Trace 7 of parity-map.md found nothing.

A stub is one paragraph: "Second sweep complete; parity-map.md Trace 7 captures everything. No additional implicit behavior found."

---

## Body

For each implicit behavior, write a short subsection. Use these category prompts to find candidates:

### Silent error swallowing

Empty `catch` blocks. `try { ... } catch { /* nothing */ }`. Methods that return null on bad input without logging or notifying. Validation that runs but never surfaces a message.

For each: state what's swallowed, where, and what the Obsidian conversion will do (typically: surface the error via `nbWarning` / `Notification` / `ActionBadRequest`).

### ViewState as state machine

`ViewState["foo"]` reads/writes that affect downstream rendering. These often act as flags ("did the user expand this section yet?", "is the modal in 'add' or 'edit' mode?") and are easy to miss because they don't appear in the markup or bind directly to controls.

For each: name the key, what it gates, and where the equivalent state lives in Obsidian (component ref, block config, none-needed-because-postback-model-is-gone).

### Postback timing tricks

Things that work because of ASP.NET's lifecycle: `IsPostBack` guards, `EnsureChildControls`, `OnPreRender` mutations, dynamic control creation in `OnInit`. These don't translate to Vue's reactivity model and need to be reasoned about explicitly.

For each: what the trick is, what behavior it produces in WebForms, and how the Obsidian version achieves the same outcome (or whether it can be dropped because the postback model is gone).

### Hidden control state

Controls whose visibility, enabled-state, or value depends on multiple flags read from elsewhere. Common in WebForms because the UI tree is C#-managed; rare in Vue because reactivity makes such dependencies explicit.

For each: name the control, list the gating flags, document the equivalent Vue state and whether it stays implicit (computed) or becomes explicit (refs).

### Default values and fall-throughs

Methods that return a default when input is null/empty/invalid. Switches without `default` clauses. Conditionals that omit an `else`. Behavior that depends on what *isn't* there.

For each: state the default behavior in WebForms and confirm Obsidian preserves it (often via `?? defaultValue`, `??=`, or an early return).

### Implicit field initialization

Fields auto-initialized via `<form>` post or hidden controls (`__VIEWSTATE`, `__EVENTTARGET`). Whether the WebForms block depends on these is rare but not zero.

If the block uses `Page.Request.Params["__EVENTTARGET"]` or similar, name it, capture what behavior it gates, and propose the Obsidian equivalent.

### Behavior buried in markup

Logic that lives in `<%# Eval %>`, `<%# Bind %>`, `<asp:Repeater>` ItemDataBound handlers, or anywhere the `.ascx` does work that the `.ascx.cs` doesn't make obvious.

For each: cite the line in the `.ascx`, name the behavior, and document where it lives in Obsidian (template binding, computed prop, or moved to the C# block).

### Convention-driven behavior

Things the block does because that's how Rock works, even though the code doesn't say so explicitly:
- Audit columns auto-populate (CreatedByPersonAlias, ModifiedDateTime)
- `.SaveChanges()` triggers entity hooks (validation, history, indexing)
- `entity.IsAuthorized()` consults inherited security
- Cache classes auto-invalidate on `.SaveChanges()`

These usually don't need rows, Obsidian preserves them automatically. List them only when the conversion *might* break the convention (e.g., if you're skipping `.SaveChanges()` for a perf reason, the audit columns won't update).

---

## Quality checks

- [ ] Every empty catch block in the source has been considered
- [ ] Every ViewState key has been considered
- [ ] Every conditional with no `else` has been considered (most are fine; flag the ones where omission is meaningful)
- [ ] Markup-embedded logic in the `.ascx` has been read

If a row from this analysis becomes a planned change in the conversion, link it to the row in `improvement-analysis.md` so the checkpoint can verify it.

---

## What this is NOT

- Not a duplicate of `parity-map.md` Trace 7. Trace 7 captures things that fit the parity-table row format. This file captures things that don't.
- Not a code-quality scan. Bugs go in `edge-cases.md`. Improvements go in `improvement-analysis.md`. Redundancies go in `redundancy-report.md`. Completeness is specifically about *behavior the source code achieves implicitly*.
