# CLAUDE.md — Rock RMS Development Guidelines

## Project Overview

Rock RMS is an open-source church management system. The codebase is C# (.NET) + TypeScript/Vue 3.
- **Obsidian** = the Vue 3 + C# block framework replacing legacy WebForms (.ascx) blocks.
- **Lava** = Rock's DotLiquid-based templating language.
- **WebForms** = legacy ASP.NET block system being phased out.

---

## The Prime Directive

**Follow established patterns in the existing codebase.** Do not invent your own patterns. If you are aware of an alternative or newer pattern, state it explicitly but default to what already exists.

---

## When the Request Is Ambiguous

Before implementing, if the request has multiple reasonable interpretations, **stop and ask**. Don't silently pick.

- State assumptions you're making so the user can correct them.
- If you see two or more ways to read the task (e.g., "make it faster" could mean latency, throughput, or perceived UX), present the options and ask which one matters.
- If a simpler approach than what was asked fits the goal, say so before coding it the long way.

This does **not** apply to questions already settled by Rock conventions (rules, skills, or memory). Ask about **goals and scope**, not about conventions — looking up the answer yourself is faster than a round-trip.

---

## Project Architecture

| What you're creating | Where it goes |
|---|---|
| C# block class | `Rock.Blocks/[Domain]/` |
| ViewModels / bags | `Rock.ViewModels/Blocks/[Domain]/[BlockName]/` |
| Obsidian Vue component | `Rock.JavaScript.Obsidian.Blocks/src/[Domain]/` |
| Obsidian partials | `Rock.JavaScript.Obsidian.Blocks/src/[Domain]/[blockName]/` |
| Entity model | `Rock/Model/[Domain]/[EntityName]/` |
| Enums | `Rock.Enums/[Domain]/` |
| Migrations | `Rock.Migrations/Migrations/` |
| SystemGuid constants | `Rock/SystemGuid/` |

---

## Critical Rules

- **Never break backward compatibility** unless explicitly instructed.
- Use `RockDateTime` instead of `DateTime`. Format as ISO 8601: `RockDateTime.ToString("s")`.
- Do not add optional parameters that change a public method's signature — add a new overload and keep the original intact. (Plugins are not recompiled as often as core.)
- `System.Web` is the enemy — wrap any usage in `#if WEBFORMS` blocks. Obsidian blocks and shared code must not reference `System.Web`.
- Do not use delimiters for persisting configuration — use **JSON**.
- All page parameters (query string params) must be **PascalCase** (e.g., `AccountId`).
- Be intentional with `public`. Prefer `internal`, `protected`, or `private` to reduce breaking-change risk.
- Avoid `lock()` without first consulting the prompter. Use database-level unique constraints instead — clustered/web-farm environments make in-process locking unreliable.
- When passing custom objects to Lava, use `LavaDataObject` (not `RockDynamic`). Name custom Lava objects with an `Info` suffix (e.g., `CampusInfo`).
- Avoid `Guid` in LINQ `.Where()` clauses when `Id` is available (e.g., from cached items).
- Do not declare class variables on singletons — not thread-safe. Rock has many singletons (Workflow Actions, FieldTypes, Cache types, etc.).

---

## Naming Conventions

**C#:** PascalCase for classes/methods, camelCase for variables/params, `I` prefix for interfaces, underscore prefix for private fields (no Hungarian notation). Meaningful names — no abbreviations, no single-char variables (except `i` in loops).

**TypeScript:** PascalCase for classes/interfaces/enums/types, camelCase for functions/variables/params/**filenames**. The leading-underscore convention (`_unusedArg`) silences the unused-vars warning; do not use `_` as a general private-field marker. See `.claude/rules/obsidian-conventions.md` for the full Obsidian/TypeScript style guide.

---

## Booleans

- Names must answer a question: `IsActive`, not `Active`; `IsCategoryFieldVisible`, not `ShowCategoryField`.
- For Obsidian components, the **default value must be `false`** and the name must reflect that default (e.g., `IsPanelShown = false` for a normally-hidden panel).
- `Has` is acceptable instead of `Is` when it reads more naturally.

---

## Code Style

- Always use braces — even for single-line `if`, `for`, `else`.
- Use early returns to avoid nested `if` statements.
- Use variables to document intent of complex conditions rather than inline logic.
- Use `var` for consistency (except when the type cannot be inferred).

---

## Engineering Notes

When code is confusing or non-obvious, add a note explaining **why**:

```
/*
    3/5/26 - CLAUDE

    <Why this code exists or why this change was made.>

    Reason: <One-line summary for scanning.>
*/
```

---

## Copyright Headers

Always include the appropriate copyright header at the top of every new file. See `.claude/rules/code-conventions.md` for the exact templates.

---

## Commit Messages

Commits use `+` (release notes) or `-` (trivial):

### Release note commits (`+`)

```
+ ([Domain]) [Message]. (Fixes #0000)
```

**Domain** must be exactly one of the following (wrapped in parentheses):
`AI`, `API`, `CMS`, `Check-in`, `Communication`, `Connection`, `Core`, `CRM`, `Engagement`, `Event`, `Farm`, `Finance`, `Group`, `Lava`, `LMS`, `Mobile`, `Prayer`, `Reporting`, `Workflow`, `Other`
IMPORTANT: You may only use one of the domains listed. You may not make new ones up.

**Starting word determines classification:**

| Starting Word | Classification |
|---|---|
| `Fixes` / `Fixed` | Bug Fix |
| `Improve` / `Improved` / `Updated` | Improvement |
| `Add` / `Added` | New Feature |

The message should be descriptive enough to serve as the full release note text. Append `(Fixes #0000)` if it resolves a tracked issue.

### Examples

```
+ (Core) Fixed the friendly schedule text display for single-date schedules. (Fixes #6694)
+ (Finance) Added support for ACH refunds on the NMI gateway.
+ (CRM) Improved the duplicate detection merge process to preserve giving records.
- Fixed typo in variable name.
- Removed unused using statement.
```
