---
name: css-cleanup
description: >-
  Audit and refactor CSS in Obsidian .obs block files to follow Rock's styling priorities.
  Replaces inline styles, hard-coded values, and unnecessary scoped CSS with Rock utility classes
  and CSS variables. Use when the user says "clean up css", "css cleanup", "polish styles",
  "refactor css", "style audit", "fix the styling", "use rock utilities", or after running
  /convert-block on a block. Also use when reviewing .obs files and noticing inline styles,
  hard-coded hex colors, or excessive scoped CSS that could use Rock's utility system.
  Do NOT use for: writing new blocks, C# changes, JavaScript logic changes, or creating new
  SCSS files in styles-v2/. Do NOT use for general code review (use /review-conversion instead).
argument-hint: "Path to .obs file or Category/BlockName (e.g. 'Core/TagsByLetter' or 'Rock.JavaScript.Obsidian.Blocks/src/Core/tagsByLetter.obs')"
compatibility: Requires Claude Code CLI with access to the Rock RMS codebase.
metadata:
  version: "1.1"
  author: "Maxwell Eley"
---

# Obsidian CSS Cleanup

You are refactoring CSS in Obsidian .obs block files to follow Rock's styling priority system.

**The block must look identical before and after.** This is a pure refactor — swap implementation details (inline styles, hard-coded values, redundant scoped rules) for Rock's utility classes and CSS variables, but the rendered result in the browser must not change. If a replacement would alter spacing, color, size, or layout even slightly, keep the original and leave a comment explaining why. The one exception: replacing Bootstrap numeric spacing (e.g., `mb-3`) with Rock semantic spacing (e.g., `mb-spacing-sm`) is expected — those values are close but intentionally theme-adaptive.

**Target:** $ARGUMENTS

---

## CSS Priority Order

This is the hierarchy. Always prefer a higher-priority option before falling to the next:

1. **Rock Utility Classes** — from `styles-v2/utilities/` and `styles-v2/rock-utilities/`. These are theme-aware, responsive, and consistent. Always the first choice.
2. **Block Classes** — semantic CSS class names on block wrapper elements for structural layout. Check if the block's category already has shared styles in `styles-v2/blocks/_blocks-[category].scss`.
3. **Scoped Styles** — `<style scoped>` rules. Use only when utilities cannot express the styling, OR when the styling is specific enough to justify a scoped rule (e.g., `:deep()` overrides, complex selectors, animations). Always use CSS variables for values inside scoped rules.
4. **Bootstrap** — only as a last resort when no Rock utility exists for the same purpose.

---

## Reference Routing

| Reference | Load When |
|---|---|
| `references/utility-catalog.md` | Phase 2 — always (needed for replacement lookup) |

---

## Phase 1: Locate and Read

1. **Resolve file paths.** If given a block name like `Core/TagsByLetter`, find:
   - Main: `Rock.JavaScript.Obsidian.Blocks/src/[Category]/[camelCaseBlockName].obs`
   - Partials: `Rock.JavaScript.Obsidian.Blocks/src/[Category]/[camelCaseBlockName]/*.partial.obs`
2. **Read every .obs file** for this block (main + all partials).
3. **Check for block-level SCSS** at `RockWeb/Styles/styles-v2/blocks/_blocks-[category].scss`. If it exists, read it — these classes are already globally available and should not be duplicated in scoped styles.

---

## Phase 2: Audit

Load `references/utility-catalog.md` now.

Scan each file and produce a findings table:

| # | File | Line | Category | Current | Replacement |
|---|---|---|---|---|---|

### Anti-Pattern Categories

Scan for these, in order of severity:

1. **Inline styles** — `style="..."` on template elements. Replace with utility classes or scoped rules.
2. **Hard-coded spacing** — `padding: 8px`, `margin: 16px`, `gap: 12px` in scoped CSS. Replace with Rock spacing utilities (`.gap-spacing-xs`) or `var(--spacing-*)`.
3. **Hard-coded colors** — hex codes (`#fff`, `#ee7725`), `rgb(...)`, named colors in scoped CSS. Replace with `var(--color-*)` or Rock color utility classes.
4. **Hard-coded font sizes** — `font-size: 14px`. Replace with `var(--font-size-*)` or `.font-size-*` class.
5. **Hard-coded font weights** — `font-weight: 600`. Replace with `var(--font-weight-*)`.
6. **Hard-coded border-radius** — `border-radius: 4px`. Replace with `var(--rounded-*)`.
7. **Scoped CSS duplicating a utility** — a scoped rule that just does `display: flex` when `.d-flex` exists, or `gap: 8px` when `.gap-spacing-xs` exists.
8. **Bootstrap when Rock exists** — e.g., `mb-3` when `mb-spacing-sm` is the semantic Rock alternative.
9. **Missing `:deep()`** — scoped style targeting a child component's internal DOM without `:deep()`.
10. **Excessive scoped rules** — more than ~10 scoped rules in a single file; look for rules that could be replaced by utility classes on the element.

After the table, output a summary count by category.

---

## Phase 3: Apply Decision Framework

For each finding, walk this tree:

```
1. Can a Rock utility class replace this?
   YES → Use the utility class on the element. DONE.

2. Can a combination of Rock utility classes replace this?
   YES → Use the classes. DONE.

3. Does the block's category SCSS already define a class for this?
   YES → Use the existing block class. DONE.

4. Is a scoped style appropriate?
   - The styling is block-specific layout that utilities can't express?
   - It targets a child component's internals (needs :deep())?
   - It's a complex selector, animation, or pseudo-element?
   YES → Keep as scoped style, but tokenize all values:
         spacing → var(--spacing-*)
         colors  → var(--color-*)
         fonts   → var(--font-size-*), var(--font-weight-*)
         radii   → var(--rounded-*)

5. No Rock utility exists and it's a standard CSS property?
   → Use Bootstrap if available. Otherwise, scoped style with CSS variables.

6. No replacement produces an exact visual match?
   → Leave the original code as-is. Do not force a swap that changes appearance.
```

**When in doubt, leave it alone.** A hard-coded value that renders correctly is better than a token swap that shifts layout by a few pixels.

---

## Phase 4: Refactor

Apply changes file by file.

### Template Changes
- Replace inline `style="..."` with utility classes on the element.
- Add utility classes to elements where a scoped CSS rule can now be removed.
- Dynamic styles (`:style="computedValue"`) are acceptable — but check if the computed value could instead be a conditional class binding (`:class="{ ... }"`).

### Scoped Style Changes
- Remove rules now handled by utility classes.
- Replace all hard-coded values with CSS variables in remaining rules.
- Ensure every selector targeting a child component uses `:deep()`.
- If all scoped rules are eliminated, remove the entire `<style scoped>` block.

### Scoped Style Ordering
```css
<style scoped>
/* Block-level structural classes */
.block-wrapper { ... }

/* Element-specific classes */
.header-title { ... }

/* State modifiers */
.is-active { ... }

/* :deep() child component overrides */
:deep(.form-control) { ... }
</style>
```

### Custom Class Hooks

Only add custom CSS class names on elements where theme or plugin authors would realistically want to customize appearance:

- **Always:** The outermost block wrapper div.
- **Sometimes:** Major structural sections (header, body, footer of a detail view), prominent visual elements (cards, badges, status indicators).
- **Never:** Internal layout divs that are purely structural, individual form fields, hidden elements.

**Naming:** `[block-name]-[element]` in kebab-case.
Examples: `tag-list-header`, `prayer-request-card`, `step-participant-status`.

---

## Quality Gate

Before presenting changes, verify:

- [ ] No remaining hard-coded hex colors in scoped CSS (except where no CSS variable exists)
- [ ] No hard-coded px spacing in scoped CSS (use `var(--spacing-*)`)
- [ ] No hard-coded px font sizes in scoped CSS (use `var(--font-size-*)`)
- [ ] No hard-coded px border-radius in scoped CSS (use `var(--rounded-*)`)
- [ ] No scoped rules that duplicate an available Rock utility class
- [ ] All `:deep()` selectors are correct (child component targets)
- [ ] No unused scoped styles remaining
- [ ] `<style>` block removed entirely if empty after cleanup
- [ ] `scoped` attribute preserved on remaining `<style>` blocks
- [ ] Rendered appearance is unchanged — every replacement produces the same visual result
- [ ] Bootstrap-to-Rock swaps (e.g., `mb-3` to `mb-spacing-sm`) are the only intentional value shifts
- [ ] Any hard-coded value kept as-is has a comment explaining why no token matches

---

## Troubleshooting

### Dynamic styles that can't be replaced
Some `:style` bindings compute values at runtime (e.g., user-configured colors from the server). These cannot be replaced with utility classes. Keep the `:style` binding but extract any static portions into classes.

### Utility class doesn't exist for this value
If a hard-coded value doesn't match any Rock spacing/color/typography token (e.g., `width: 250px`, `max-height: 400px`), keep it as a scoped style rule using the nearest CSS variable if one is close, or use the literal value with a comment explaining why no token fits.

### Scoped style seems to do nothing
Before removing a scoped rule, verify it's truly unused. Vue scoped styles with `:deep()` target child component internals that may not be visible in the current file. Check the child component to confirm.

### Block SCSS file not found
Not every block category has a `_blocks-[category].scss` file. If none exists, skip the block SCSS check and proceed with the audit using only Rock utilities and scoped styles.

---

## Examples

### Example 1: Inline style to utility classes

```html
<!-- BEFORE -->
<div style="display: flex; gap: 8px; align-items: center">

<!-- AFTER -->
<div class="d-flex gap-spacing-xs align-items-center">
```

### Example 2: Scoped rule replaced by utilities

```html
<!-- BEFORE -->
<div class="exception-header">
<style scoped>
.exception-header { display: flex; flex-direction: column; gap: 12px; }
</style>

<!-- AFTER -->
<div class="d-flex flex-column gap-spacing-sm">
<!-- scoped rule removed -->
```

### Example 3: Tokenize remaining scoped styles

```css
/* BEFORE */
.entity-picker :deep(.form-group label) {
    margin-right: 8px;
    font-size: 14px;
    color: #8B8BA7;
}

/* AFTER */
.entity-picker :deep(.form-group label) {
    margin-right: var(--spacing-xsmall);
    font-size: var(--font-size-small);
    color: var(--color-interface-medium);
}
```

### Example 4: Bootstrap to Rock semantic

```html
<!-- BEFORE -->
<div class="mb-3 p-2 bg-light text-muted">

<!-- AFTER -->
<div class="mb-spacing-sm p-spacing-xs bg-interface-softer text-interface-medium">
```

### Example 5: Dynamic style to class binding

```html
<!-- BEFORE -->
<span :style="`background-color: ${statusColor}; color: #fff;`">

<!-- AFTER (if status values are known/finite) -->
<span :class="statusClass" class="text-interface-softest">

<!-- OR if truly dynamic, keep :style but tokenize what you can -->
<span :style="{ backgroundColor: statusColor }" class="text-interface-softest">
```
