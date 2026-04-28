# Rock Utility Class Catalog

Quick-lookup reference for all available Rock and Bootstrap utility classes. Organized by CSS property.

Source: `RockWeb/Styles/styles-v2/`

---

## Spacing

### Rock Semantic Spacing (preferred)

Sizes: `none` (0), `tiny` (4px), `xs` (8px), `sm` (12px), `md` (16px), `lg` (24px), `xl` (48px), `huge` (80px)

| Property | Classes |
|---|---|
| Gap | `.gap-spacing-{none\|tiny\|xs\|sm\|md\|lg\|xl\|huge}` |
| Padding (all) | `.p-spacing-{size}` |
| Padding (Y axis) | `.py-spacing-{size}` |
| Padding (X axis) | `.px-spacing-{size}` |
| Padding (individual) | `.pt-spacing-{size}`, `.pb-spacing-{size}`, `.pl-spacing-{size}`, `.pr-spacing-{size}` |
| Margin (all) | `.m-spacing-{size}` |
| Margin (Y axis) | `.my-spacing-{size}` |
| Margin (X axis) | `.mx-spacing-{size}` |
| Margin (individual) | `.mt-spacing-{size}`, `.mb-spacing-{size}`, `.ml-spacing-{size}`, `.mr-spacing-{size}` |

### CSS Variables (for scoped styles)

```
--spacing-tiny:    4px
--spacing-xsmall:  8px
--spacing-small:  12px
--spacing-medium: 16px
--spacing-large:  24px
--spacing-xlarge: 48px
--spacing-huge:   80px
```

### Px-to-Rock Mapping

| Hard-coded | Rock Variable | Utility Suffix |
|---|---|---|
| `0` | — | `none` |
| `4px` | `--spacing-tiny` | `tiny` |
| `8px` | `--spacing-xsmall` | `xs` |
| `12px` | `--spacing-small` | `sm` |
| `16px` | `--spacing-medium` | `md` |
| `24px` | `--spacing-large` | `lg` |
| `48px` | `--spacing-xlarge` | `xl` |
| `80px` | `--spacing-huge` | `huge` |

For non-standard values (e.g., `6px`, `10px`, `20px`), round to the nearest semantic size or use the CSS variable directly in scoped style if exact value matters.

---

## Colors

### Interface Colors (theme-aware — light/dark adaptive)

| Utility Class | CSS Variable | Light Mode |
|---|---|---|
| `.bg-interface-strongest` / `.text-interface-strongest` | `--color-interface-strongest` | `#0a0a0a` |
| `.bg-interface-stronger` / `.text-interface-stronger` | `--color-interface-stronger` | `#0D1117` |
| `.bg-interface-strong` / `.text-interface-strong` | `--color-interface-strong` | `#3D444D` |
| `.bg-interface-medium` / `.text-interface-medium` | `--color-interface-medium` | `#8B8BA7` |
| `.bg-interface-soft` / `.text-interface-soft` | `--color-interface-soft` | `#D9D9E3` |
| `.bg-interface-softer` / `.text-interface-softer` | `--color-interface-softer` | `#F8F8FC` |
| `.bg-interface-softest` / `.text-interface-softest` | `--color-interface-softest` | `#fdfdfd` |

### Semantic Colors (each has soft, strong, tint, shade variants)

For each `{name}`: `.bg-{name}-{variant}` and `.text-{name}-{variant}`

| Name | Base Color | Variants |
|---|---|---|
| `critical` | `#ee7725` (orange) | `soft`, `strong`, `tint`, `shade` |
| `danger` | `#D70015` (red) | `soft`, `strong`, `tint`, `shade` |
| `info` | `#005DD7` (blue) | `soft`, `strong`, `tint`, `shade` |
| `success` | `#01A11E` (green) | `soft`, `strong`, `tint`, `shade` |
| `warning` | `#E58600` (orange) | `soft`, `strong`, `tint`, `shade` |

### System Colors

| Utility | Variable | Base Color |
|---|---|---|
| `.bg-primary` / `.text-primary` | `--color-primary` | `#FF791D` |
| `.bg-primary-tint` / `.text-primary-tint` | `--color-primary-tint` | — |
| `.bg-primary-shade` / `.text-primary-shade` | `--color-primary-shade` | — |
| `.bg-secondary` / `.text-secondary` | `--color-secondary` | `#83758F` |
| `.bg-secondary-tint` / `.text-secondary-tint` | `--color-secondary-tint` | — |
| `.bg-secondary-shade` / `.text-secondary-shade` | `--color-secondary-shade` | — |
| `.bg-link` / `.text-link` | `--color-link` | `#006DCC` |
| `.bg-link-tint` / `.text-link-tint` | `--color-link-tint` | — |
| `.bg-link-shade` / `.text-link-shade` | `--color-link-shade` | — |

### Common Hex-to-Rock Mapping

| Hard-coded | Rock Replacement |
|---|---|
| `#fff`, `#ffffff`, `white` | `var(--color-interface-softest)` or `.text-interface-softest` |
| `#000`, `#000000`, `black` | `var(--color-interface-strongest)` or `.text-interface-strongest` |
| `#8B8BA7`, gray/muted text | `var(--color-interface-medium)` or `.text-interface-medium` |
| `#D9D9E3`, light border | `var(--color-interface-soft)` |
| `#F8F8FC`, light background | `var(--color-interface-softer)` or `.bg-interface-softer` |
| Red / error colors | `.text-danger` or `var(--color-danger-strong)` |
| Green / success colors | `.text-success` or `var(--color-success-strong)` |
| Blue / info colors | `.text-info` or `var(--color-info-strong)` |
| Orange / warning colors | `.text-warning` or `var(--color-warning-strong)` |

---

## Display

Source: `styles-v2/utilities/_display.scss`

Responsive breakpoints: (base), `-sm` (768px), `-md` (992px), `-lg` (1200px), `-print`

```
.d-{breakpoint?}-none
.d-{breakpoint?}-inline
.d-{breakpoint?}-inline-block
.d-{breakpoint?}-block
.d-{breakpoint?}-table
.d-{breakpoint?}-table-row
.d-{breakpoint?}-table-cell
.d-{breakpoint?}-flex
.d-{breakpoint?}-inline-flex
.d-{breakpoint?}-grid
.d-{breakpoint?}-inline-grid
```

---

## Flexbox

Source: `styles-v2/utilities/_flex.scss`

All support responsive breakpoints (`-sm`, `-md`, `-lg`).

| Category | Classes |
|---|---|
| Direction | `.flex-{bp?}-row`, `.flex-{bp?}-column`, `.flex-{bp?}-row-reverse`, `.flex-{bp?}-column-reverse` |
| Wrap | `.flex-{bp?}-wrap`, `.flex-{bp?}-nowrap`, `.flex-{bp?}-wrap-reverse` |
| Grow/Shrink | `.flex-{bp?}-grow-0`, `.flex-{bp?}-grow-1`, `.flex-{bp?}-shrink-0`, `.flex-{bp?}-shrink-1` |
| Fill | `.flex-{bp?}-fill`, `.flex-{bp?}-eq` |
| Justify | `.justify-content-{bp?}-{start\|end\|center\|between\|around\|evenly}` |
| Align Items | `.align-items-{bp?}-{start\|end\|center\|baseline\|stretch}` |
| Align Content | `.align-content-{bp?}-{start\|end\|center\|between\|around\|stretch}` |
| Align Self | `.align-self-{bp?}-{auto\|start\|end\|center\|baseline\|stretch}` |

---

## CSS Grid

Source: `styles-v2/utilities/_cssgrid.scss`

| Category | Classes |
|---|---|
| Columns | `.grid-cols-{bp?}-{1-12\|none\|subgrid}` |
| Column Span | `.col-span-{bp?}-{1-12\|full}` |
| Column Start/End | `.col-start-{bp?}-{1-13\|auto}`, `.col-end-{bp?}-{1-13\|auto}` |
| Rows | `.grid-rows-{1-6}` |
| Row Span | `.row-span-{1-6}` |

---

## Typography

### Rock Utility Classes

| Class | CSS Variable | Value |
|---|---|---|
| `.font-size-xs` | `--font-size-xsmall` | `12px` |
| `.font-size-sm` | `--font-size-small` | `14px` |
| `.font-size-regular` | `--font-size-regular` | `16px` |
| `.font-size-h6` | `--font-size-h6` | `16px` |
| `.font-size-h5` | `--font-size-h5` | `18px` |
| `.font-size-h4` | `--font-size-h4` | `22px` |
| `.font-size-h3` | `--font-size-h3` | `28px` |
| `.font-size-h2` | `--font-size-h2` | `36px` |
| `.font-size-h1` | `--font-size-h1` | `44px` |

### Line Height

| Class | Variable | Value |
|---|---|---|
| `.line-height-compact` | `--line-height-compact` | `1.055` |
| `.line-height-tight` | `--line-height-tight` | `1.1` |
| `.line-height-normal` | `--line-height-normal` | `1.5` |

Additional variables (no utility class): `--line-height-loose` (1.75), `--line-height-xloose` (2.0)

### Font Weight Variables

```
--font-weight-light:    300
--font-weight-regular:  400
--font-weight-medium:   500
--font-weight-semibold: 600
--font-weight-bold:     700
--font-weight-black:    900
```

Utility classes: `.font-weight-light`, `.font-weight-lighter`, `.font-weight-normal`, `.font-weight-bold`, `.font-weight-bolder`, `.text-light`, `.text-bold`

### Text Utilities

Source: `styles-v2/utilities/_text.scss`

| Category | Classes |
|---|---|
| Alignment (responsive) | `.text-{bp?}-left`, `.text-{bp?}-center`, `.text-{bp?}-right` |
| Transform | `.text-lowercase`, `.text-uppercase`, `.text-capitalize` |
| Wrapping | `.text-wrap`, `.text-nowrap`, `.text-break`, `.text-truncate`, `.text-justify` |
| Monospace | `.font-monospace` |
| Overflow | `.overflow-auto`, `.overflow-hidden`, `.overflow-visible`, `.overflow-y-auto`, `.overflow-y-hidden` |

---

## Borders

Source: `styles-v2/utilities/_borders.scss`

| Category | Classes |
|---|---|
| Add border | `.border`, `.border-top`, `.border-right`, `.border-bottom`, `.border-left` |
| Remove border | `.border-0`, `.border-top-0`, `.border-right-0`, `.border-bottom-0`, `.border-left-0` |
| Border color | `.border-primary`, `.border-success`, `.border-danger`, `.border-warning`, `.border-info`, `.border-critical`, `.border-panel` |
| Radius | `.rounded`, `.rounded-sm`, `.rounded-lg`, `.rounded-circle`, `.rounded-pill`, `.rounded-0` |
| Radius (side) | `.rounded-top`, `.rounded-right`, `.rounded-bottom`, `.rounded-left` |

### Border Radius Variables

```
--rounded-tiny:    2px
--rounded-xsmall:  4px
--rounded-small:   6px
--rounded-medium:  8px
--rounded-large:  12px
--rounded-xlarge: 16px
--rounded-huge:   24px
```

---

## Shadows

Source: `styles-v2/utilities/_shadows.scss`

`.shadow`, `.shadow-md`, `.shadow-lg`, `.shadow-xl`, `.shadow-2xl`, `.shadow-inner`, `.shadow-none`

### Shadow Variables

```
--box-shadow          (default subtle shadow)
--box-shadow-strong   (prominent shadow)
--popup-box-shadow    (popover/modal shadow)
--dropdown-box-shadow (dropdown menus)
--input-box-shadow    (form inputs)
--input-focus-box-shadow (focus ring)
```

---

## Opacity

Source: `styles-v2/utilities/_opacity.scss`

`.o-0`, `.o-10`, `.o-20`, `.o-30`, `.o-40`, `.o-50`, `.o-60`, `.o-70`, `.o-80`, `.o-90`, `.o-100`

---

## Sizing

Source: `styles-v2/utilities/_sizing.scss`

| Category | Classes |
|---|---|
| Width | `.w-1`, `.w-20`, `.w-25`, `.w-50`, `.w-75`, `.w-100`, `.w-auto` |
| Width (named) | `.width-quarter`, `.width-third`, `.width-half`, `.width-full` |
| Max width | `.mw-100` |
| Min width | `.min-w-0` |
| Viewport width | `.vw-100`, `.min-vw-100` |
| Height | `.h-25`, `.h-50`, `.h-75`, `.h-100`, `.h-auto` |
| Max height | `.mh-100` |
| Viewport height | `.vh-100`, `.min-vh-100` |

---

## Position

Source: `styles-v2/utilities/_position.scss`

`.position-static`, `.position-relative`, `.position-absolute`, `.position-fixed`

`.fixed-top`, `.fixed-bottom`

`.inset-0`, `.top-0`, `.right-0`, `.bottom-0`, `.left-0`

---

## Cursor

Source: `styles-v2/utilities/_cursor.scss`

`.cursor-auto`, `.cursor-default`, `.cursor-pointer` (alias: `.clickable`), `.cursor-wait`, `.cursor-text`, `.cursor-move`, `.cursor-not-allowed`, `.cursor-grab`, `.cursor-grabbing`, `.cursor-zoom-in`, `.cursor-zoom-out`

---

## Z-Index

Source: `styles-v2/utilities/_zindex.scss`

`.z-auto`, `.z-0`, `.z-10`, `.z-20`, `.z-30`, `.z-40`, `.z-50`, `.-z-10`

---

## Other Utilities

| Source | Classes |
|---|---|
| `_align.scss` | Vertical alignment utilities |
| `_background.scss` | Background color classes |
| `_object-fit.scss` | `.object-fit-contain`, `.object-fit-cover`, etc. |
| `_order.scss` | Flex order utilities |
| `_ratios.scss` | Aspect ratio utilities |
| `_userselect.scss` | `.user-select-all`, `.user-select-auto`, `.user-select-none` |

---

## Bootstrap-to-Rock Migration

Common Bootstrap classes and their preferred Rock replacements:

| Bootstrap | Rock | Why |
|---|---|---|
| `mb-1` | `mb-spacing-tiny` | Semantic, theme-adaptive |
| `mb-2` | `mb-spacing-xs` | Semantic, theme-adaptive |
| `mb-3` | `mb-spacing-sm` | Semantic, theme-adaptive |
| `mb-4` | `mb-spacing-lg` | Semantic, theme-adaptive |
| `p-1` | `p-spacing-tiny` | Theme-adaptive |
| `p-2` | `p-spacing-xs` | Theme-adaptive |
| `p-3` | `p-spacing-sm` | Theme-adaptive |
| `p-4` | `p-spacing-lg` | Theme-adaptive |
| `bg-light` | `bg-interface-softer` | Theme-aware (adapts to dark mode) |
| `bg-dark` | `bg-interface-stronger` | Theme-aware |
| `text-muted` | `text-interface-medium` | Semantic, theme-aware |
| `text-dark` | `text-interface-stronger` | Theme-aware |
| `text-white` | `text-interface-softest` | Theme-aware |

**Note:** Not all Bootstrap classes need replacing. `d-flex`, `d-none`, `text-center`, `rounded`, `border`, `w-100`, etc. are part of Rock's extended utility set and are fine to keep. Only replace Bootstrap spacing/color classes that have Rock semantic equivalents.

---

## Font Families (CSS Variables Only)

```
--font-family-title      (sans-serif, for headings)
--font-family-body       (sans-serif, for body text)
--font-family-sans       (system sans-serif stack)
--font-family-serif      (serif stack)
--font-family-monospace  (monospace stack)
--font-family-segoe      (Segoe UI stack)
```
