\# CLAUDE.md — Rock RMS Development Guidelines



\## The Prime Directive



\*\*Follow established patterns in the existing codebase.\*\* Do not invent your own patterns. If you are aware of an alternative or newer pattern, state it explicitly but default to what already exists.



---



\## Core Rules



\- \*\*Never break backward compatibility\*\* unless explicitly instructed.

\- Use `RockDateTime` instead of `DateTime`.

\- Format DateTime values as ISO 8601: `RockDateTime.ToString("s")` — not `RockDateTime.ToString()`.

\- Do not add optional parameters that change a public method's signature. Instead, add a new overload and keep the original intact. (Plugins are not recompiled as often as core.)

\- Always use braces — even for single-line `if`, `for`, `else`, etc.

\- Use early returns to avoid nested `if` statements.

\- Use variables to document the intent of complex conditions rather than inline logic.

\- In general, use `var` for consistency (except when the type cannot be inferred).

\- Avoid `Guid` in LINQ `.Where()` clauses when you can use `Id` (e.g., for cached items).

\- Do not declare class variables on singletons — this is not thread-safe. Rock has many singletons (Workflow Actions, FieldTypes, DataFilters, Financial Gateways, Cache types, etc.).



---



\## Booleans



\- Boolean property names must answer a question: `IsActive`, not `Active`; `IsCategoryFieldVisible`, not `ShowCategoryField`.

\- For Obsidian components, the \*\*default value must be `false`\*\* and the name must reflect that default:

&nbsp; - Panel normally hidden → `IsPanelShown = false`

&nbsp; - Feature normally enabled → `IsFooDisabled = false`

&nbsp; - Feature normally disabled → `IsFooEnabled = false`

\- `Has` is acceptable instead of `Is` when it reads more naturally.



---



\## Naming Conventions



\*\*C#\*\*

\- Classes and methods: PascalCase

\- Variables and parameters: camelCase

\- Interfaces: `IEntityName` (prefix `I` + PascalCase)

\- Private member fields: prefix with underscore only — no Hungarian notation

\- Use meaningful, descriptive names. No abbreviations. No single-character variables (exception: `i` in `for` loops).



\*\*TypeScript\*\*

\- Classes, interfaces, namespaces, types, enums: PascalCase

\- Functions, variables, parameters: camelCase

\- Filenames: camelCase

\- Do \*\*not\*\* prefix private fields with `\_`

\- Use meaningful, descriptive names. No abbreviations.



---



\## Strings and Constants



\- Use `private static readonly` for strings that may change.

\- Use `const` only for true constants that will never change (e.g., `SystemGuid`, `SystemSetting`).

\- For large strings used in settings attributes, create an `AttributeStrings` region with a `private const string`.



---



\## Configuration and Persistence



\- Do not use delimiters for persisting configuration (Data Views, Field Types, etc.) — use \*\*JSON\*\* instead.

\- All page parameters (query string params) should be in \*\*PascalCase\*\* (e.g., `AccountId`).



---



\## Block Architecture



\- Declare `FieldAttribute`s vertically, assigning properties (not constructor parameters).

\- Define attribute keys as constants in a nested `private static class AttributeKey`.

\- Define page parameter keys in a `private static class PageParameterKey`.

\- Define `AttributeCategory` constants in a nested `private static class AttributeCategory` if breaking attributes into categories.

\- Define Person Preference keys in a `private static class PersonPreferenceKey`.



\*\*Accessing page parameters:\*\*

```csharp

// Correct

PageParameter( PageParameterKey.Group )



// Incorrect

Request.Params\["Group"]

PageParameter( "Group" )

```



\*\*Favor simple entity name\*\* for page parameters that accept Id, IdKey, or Guid. Retrieve using:

```csharp

var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

```



\*\*Creating linked page URLs:\*\*

```csharp

var pageParams = new Dictionary<string, string>();

pageParams.Add( PageParameterKey.PersonId, Person.Id.ToString() );

pageParams.Add( PageParameterKey.GroupId, group.Id.ToString() );

var url = LinkedPageUrl( AttributeKey.AttendancePageAttribute, pageParams );

```



---



\## LINQ and Database Queries



\- Add reusable `.Where()` expressions to the service layer rather than duplicating them in blocks.

\- Be cautious with `.Where( p => list.Contains( p.Field ) )` — this produces a `WHERE IN` clause. Large lists can exceed the batch size limit (~65,536 × network packet size, default 4KB).

&nbsp; - \*\*Preferred:\*\* Use an unexecuted `IQueryable` as the basis for `Contains()` so EF generates a subquery instead.

&nbsp; - \*\*Fallback:\*\* Break the query into smaller batches and reassemble in memory. Weigh performance carefully.

\- Avoid `Guid` in LINQ joins when an `Id` from a cached item is available.



---



\## Business Logic



\- Methods that interact with the database belong on the \*\*service layer\*\* (e.g., `GetImpliedGroup()`).

\- If there is a clear case for a model-level method, implement it by calling the service. This requires Developer Discussion approval.



---



\## Method Design



\- Keep methods small and focused on a single responsibility (SOLID principles).

\- Document every method (public, private, internal) and any non-obvious code block.

\- Comments should explain \*\*why\*\* code exists, not just what it does.

\- Aim for a comment roughly every 50–100 lines.

\- Method comments should use proper English (capitalized, ending with a period).



\*\*Behavior-modifying parameters — use an Options POCO:\*\*



When a method takes multiple parameters that alter its behavior, use a POCO instead of individual parameters:



```csharp

// Instead of:

GetCampuses( bool includeInactive, CampusType campusType, CampusStatus campusStatus )



// Use:

GetCampuses( CampusQueryOptions options = null )



class CampusQueryOptions

{

&nbsp;   public bool IncludeInactive { get; set; }

&nbsp;   public List<CampusType> CampusTypes { get; set; } = null;

&nbsp;   public List<CampusStatus> CampusStatuses { get; set; } = null;

}

```



\- `RockContext` and data-source parameters are \*\*not\*\* behavior parameters and do not belong in the POCO.

\- Default values on the POCO must preserve existing behavior.

\- Document each property to explain how the query changes based on its value — not just "gets or sets X."

\- Place Options POCOs for `Rock.Model` in: `Rock/Model/\[Domain]/\[Entity]/Options/\[PocoName].cs` with a matching namespace.



---



\## Deprecation



\- Only deprecate methods with approval from the technical lead.

\- Use `\[Obsolete()]` and `\[RockObsolete( "X.Y" )]` where `X.Y` is the Rock version.

\- Add an engineering note above the method explaining why it was obsoleted.



---



\## Visibility / Access Modifiers



\- Be intentional with `public`. If `internal`, `protected`, or `private` works, prefer those to reduce breaking change risk.



---



\## Namespaces



\- Do not add new namespaces without DSD/PO approval (except adding a standard model domain that already follows an established pattern).

\- For `Rock.ViewModels` and `Rock.Enums`, do not add classes/enums to the root namespace. Valid patterns:

&nbsp; - `\[Domain]` (e.g., `Rock.ViewModels.CMS`)

&nbsp; - `Blocks.\[Domain].\[BlockName]` (e.g., `Rock.Blocks.Core.CampusDetail`)

&nbsp; - `Controls`, `Utility`



---



\## `\[RockInternal]` Attribute



Use this attribute in three cases:



1\. \*\*Permanently internal\*\* — code never intended for plugins (set `keepInternalForever: true` if RockWeb access requires `public`).

2\. \*\*Temporarily internal\*\* — new feature with unconfirmed API, considered experimental but intended to go public eventually.

3\. \*\*Graduating to public\*\* — once confirmed stable, remove the attribute and make it `public`.



Always include the Rock version string as the first parameter: `\[RockInternal( "1.16" )]`.



---



\## Commit Messages



Commits fall into two categories based on whether they should appear in release notes:



\- \*\*`+`\*\* — Notable change; will appear in release notes.

\- \*\*`-`\*\* — Small or trivial change; will not appear in release notes.



\### Format for release note commits (`+`)



```

\+ (\[Domain]) \[Message]. (Fixes #0000)

```



\*\*Domain\*\* must be exactly one of the following (wrapped in parentheses):

`AI`, `API`, `CMS`, `Check-in`, `Communication`, `Connection`, `Core`, `CRM`, `Engagement`, `Event`, `Farm`, `Finance`, `Group`, `Lava`, `LMS`, `Mobile`, `Prayer`, `Reporting`, `Workflow`, `Other`
IMPORTANT: You may only use one of the domains listed. You may not make new ones up.


\*\*Message\*\* must begin with one of these specific words, which determines the release note classification:



| Starting Word | Classification |

|---|---|

| `Fixes` / `Fixed` | Bug Fix |

| `Improve` / `Improved` / `Updated` | Improvement |

| `Add` / `Added` | New Feature |



The message should be descriptive enough to serve as the full release note text.



If the commit resolves a tracked issue, append `(Fixes #0000)` at the end.



\### Examples



```

\+ (Core) Fixed the friendly schedule text display for single-date schedules to use a more friendly format (e.g., "Once on March 29, 2026 at 11:00 AM" instead of "Once at 3/29/2026 11:00 AM"). (Fixes #6694)



\+ (Finance) Added support for ACH refunds on the NMI gateway.



\+ (CRM) Improved the duplicate detection merge process to preserve giving records.



\- Fixed typo in variable name.



\- Removed unused using statement.

```



---



\## Locks



Avoid `lock()` in C# without first consulting the DSD. Rely on database-level unique constraints where possible to prevent duplicate inserts. Clustered/web-farm environments make in-process locking unreliable.

