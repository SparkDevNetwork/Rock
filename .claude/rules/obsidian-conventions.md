# Obsidian Conventions

TypeScript and Vue (`.obs`) style rules for `Rock.JavaScript.Obsidian` and `Rock.JavaScript.Obsidian.Blocks`. Loaded when working in those projects.

These rules mirror what the projects' `.eslintrc.js` files enforce. If this document and the configs disagree, the configs win and this document should be updated to match. Any PR that edits an `.eslintrc.js` file in either project should also touch this document.

Source-of-truth files:
- `Rock.JavaScript.Obsidian/.eslintrc.js`
- `Rock.JavaScript.Obsidian.Blocks/.eslintrc.js`

Both configs extend `eslint:recommended` and `plugin:@typescript-eslint/recommended`, so dozens of additional rules (`no-empty`, `no-redeclare`, `no-explicit-any`, etc.) are active beyond what's documented here. This document covers the Rock-specific overrides and the rules that most often trip up contributors; the absence of a rule from this document does not mean the rule is off.

---

## Whitespace and Punctuation

- 4-space indentation by convention. Lint enforces only `no-tabs`; indentation width is not lint-checked, so respect the existing files when adding code.
- Double quotes for strings. Single quotes are only acceptable when they avoid escaping. Template literals are allowed.
- Semicolons are required on every statement.
- Stroustrup brace style. `else`, `catch`, and `finally` go on their own line after the closing brace, matching Rock's C# style:

```ts
if (isActive) {
    doThing();
}
else {
    doOtherThing();
}
```

- No spaces inside parentheses. Write `foo(bar)`, not `foo( bar )`. This is different from Rock's C# convention, which uses padded parens. Watch for it when switching contexts.
- Use `===` and `!==` rather than `==` and `!=`. Comparisons against `null` and literal-to-literal comparisons are exempt.

---

## TypeScript Behavior

- Specify a return type on every function declaration. Function expressions and arrow expressions are exempt.
- Inferrable type annotations are fine when they aid readability. `const value: number = 5` is not flagged.
- Unused variables and parameters must be removed or renamed with a leading underscore (`_unusedArg`). The underscore prefix is **only** for unused identifiers; it is not a general "private" marker.
- `no-undef` is disabled in `.ts` and `.obs` files. TypeScript itself catches undefined references.

---

## Naming Conventions

Default: camelCase. Exceptions below.

| Category | Format | Notes |
|---|---|---|
| Functions, variables, parameters | camelCase | Default |
| Exported variables | camelCase or PascalCase | Either is acceptable |
| Imports (default) | camelCase or PascalCase | Match the upstream module's convention |
| Classes, types, interfaces, enums | PascalCase | All type-like declarations |
| Interfaces | PascalCase with `I` prefix | e.g. `IPersonBag`, `IBlockConfig`. A `type` alias of the same shape is exempt — the `I`-prefix selector applies only to `interface` declarations, so converting to `type` is a legitimate way to drop the prefix. |
| Enum members | PascalCase | |
| Unused variables/parameters | camelCase with optional `_` prefix | `_unusedArg` silences the unused-vars warning |
| Object literal properties and methods | Any format | Third-party API payloads (JSON keys, Google Maps options, etc.) often dictate the casing |

---

## Project Scope

`npm run lint` covers these directories:

- `Rock.JavaScript.Obsidian/Framework`
- `Rock.JavaScript.Obsidian.Blocks/src`

Other folders have their own ESLint configs but are not in the default lint script:

- `Rock.JavaScript.Obsidian/Build/.eslintrc.js` and `Rock.JavaScript.Obsidian.Blocks/build/.eslintrc.js` apply to Node.js build scripts. They use a simpler ruleset (no TypeScript or Vue) and are not held to the full conventions above.
- `Rock.JavaScript.Obsidian/Framework/.eslintrc.js` and `Rock.JavaScript.Obsidian/System/.eslintrc.js` are placeholder configs (`root: false`) and contribute no rules.

`Framework/ViewModels/` is excluded from linting in the Obsidian project. Files there are generated from C# ViewModels and are not edited directly.

---

## Copyright Headers

A one-line copyright header is required at the top of every `.obs` and `.partial.obs` file before the `<template>` tag. See `.claude/rules/code-conventions.md` for the exact text.

`.ts` files use the standard multi-line copyright block, also documented in `.claude/rules/code-conventions.md`. The same applies to `.partial.ts` files in this project.
