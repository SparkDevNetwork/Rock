# Code Conventions

Formatting, coding style, and project organization patterns for Rock RMS. Always loaded.

---

## Copyright Headers

### C# (`.cs`) and TypeScript (`.ts`)

```
// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
```

Note the trailing `//` — intentional and consistent across the codebase.

### Obsidian Vue (`.obs`, `.partial.obs`)

One-liner, first line before `<template>`:

```html
<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
```

---

## SQL Formatting

All SQL in Rock (migrations, scripts, inline) must follow these rules:

- **UPPERCASE** keywords: `SELECT`, `FROM`, `WHERE`, `JOIN`, `INSERT`, `UPDATE`, `DELETE`, `ORDER BY`, etc.
- **Bracket-wrap** all table and column names: `[Person].[FirstName]`
- **JOIN syntax** — not WHERE-based joins
- **Aliases** with `AS`: `[Person] AS [p]`
- **IF NOT EXISTS / IF EXISTS** guards for idempotency in migrations

### String escaping in `$@"..."` SQL

When using interpolated verbatim strings for SQL in C#:
- Single quotes must be doubled: `'O''Brien'` not `'O'Brien'`
- Curly braces must be doubled for literals: `'{{""key"": ""{value}""}}'`
- Missing escaping causes runtime SQL errors or C# compilation errors

---

## Rock Domain Names

Valid `[RockDomain]` values (also used for `[Enums.EnumDomain]` and commit message domains):

AI, Blocks, CheckIn, Cms, Communication, Connection, Controls, Core, Crm, Engagement, Event, Finance, Geography, Group, Lms, Mobile, Net, Observability, Reporting, Security, WebFarm, Workflow

---

## Strings and Constants

- Use `private static readonly` for strings that may change.
- Use `const` only for true constants that will never change (e.g., `SystemGuid`, `SystemSetting`).
- For large strings used in settings attributes, create an `AttributeStrings` region with a `private const string`.

---

## Method Design

- Keep methods small and focused on a single responsibility (SOLID principles).
- Document every method (public, private, internal) and any non-obvious code block.
- Comments should explain **why** code exists, not just what it does.
- Aim for a comment roughly every 50-100 lines.
- Method comments should use proper English (capitalized, ending with a period).

**Behavior-modifying parameters — use an Options POCO:**

When a method takes multiple parameters that alter its behavior, use a POCO instead of individual parameters:

```csharp
// Instead of:
GetCampuses( bool includeInactive, CampusType campusType, CampusStatus campusStatus )

// Use:
GetCampuses( CampusQueryOptions options = null )

class CampusQueryOptions
{
    public bool IncludeInactive { get; set; }
    public List<CampusType> CampusTypes { get; set; } = null;
    public List<CampusStatus> CampusStatuses { get; set; } = null;
}
```

- `RockContext` and data-source parameters are **not** behavior parameters and do not belong in the POCO.
- Default values on the POCO must preserve existing behavior.
- Document each property to explain how the query changes based on its value — not just "gets or sets X."
- Place Options POCOs for `Rock.Model` in: `Rock/Model/[Domain]/[Entity]/Options/[PocoName].cs` with a matching namespace.

---

## Logging and Error Handling

- Use `RockLogger.Log.<Level>( RockLogDomains.X, ... )` for structured logging.
- Available levels: `Debug`, `Information`, `Warning`, `Error`, `Fatal`.
- **Never** log inside tight loops — it can flood the log and degrade performance.
- Use `ExceptionLogService.LogException()` for application exceptions that should appear in Rock's Exception Log.
- **Try/Catch/Ignore** should be rare. When used, include an intentional comment explaining why the exception is swallowed:

```csharp
try
{
    // Attempt optional cleanup.
}
catch
{
    // Intentionally ignored: cleanup is best-effort and failure is non-critical.
}
```

---

## Enums

- New enums go in the **`Rock.Enums`** project under the appropriate domain folder.
- **File:** `Rock.Enums/[Domain]/EnumName.cs`. Nested sub-domains are fine (e.g., `Rock.Enums/Blocks/Security/Login/LoginMethod.cs`).
- **Namespace:** match the folder — `Rock.Enums.[Domain]` (e.g., `Rock.Enums.Blocks.Security.Login`). Do **not** use `Rock.Model`; that is a legacy holdover from where enums lived before they were moved into the `Rock.Enums` project.
- **`[Enums.EnumDomain( "Domain" )]` attribute:** only for the legacy enums still declared in the `Rock.Model` namespace, where the namespace does not convey the domain. New enums in a `Rock.Enums.[Domain]` namespace must **not** carry it — the namespace already carries the domain. Do not add it to new enums, and do not add it when moving an enum into a `Rock.Enums.*` namespace.
- Enums whose name could collide with an entity name should use the `*Specifier` suffix (e.g., `GroupTypeSpecifier`).

---

## Namespaces

- Do not add new namespaces without approval (except adding a standard model domain that already follows an established pattern).
- For `Rock.ViewModels` and `Rock.Enums`, do not add classes/enums to the root namespace. Valid patterns:
  - `[Domain]` (e.g., `Rock.ViewModels.CMS`)
  - `Blocks.[Domain].[BlockName]` (e.g., `Rock.Blocks.Core.CampusDetail`)
  - `Controls`, `Utility`

---

## Deprecation

- Only deprecate methods with approval from the technical lead.
- Use `[Obsolete()]` and `[RockObsolete( "X.Y" )]` where `X.Y` is the Rock version.
- Add an engineering note above the method explaining why it was obsoleted.

---

## `[RockInternal]` Attribute

Use this attribute in three cases:

1. **Permanently internal** — code never intended for plugins (set `keepInternalForever: true` if RockWeb access requires `public`).
2. **Temporarily internal** — new feature with unconfirmed API, considered experimental but intended to go public eventually.
3. **Graduating to public** — once confirmed stable, remove the attribute and make it `public`.

Always include the Rock version string as the first parameter: `[RockInternal( "1.16" )]`.
