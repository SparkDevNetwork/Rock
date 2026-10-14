# Rock Data Model

Rock's data layer conventions: entities, columns, foreign keys, GUIDs, entity authorization, and data access patterns. Always loaded.

---

## Standard Model<T> Columns

Every entity inheriting `Model<T>` automatically has these columns (do NOT add them manually):
- `Id` (int, PK, identity)
- `Guid` (uniqueidentifier, unique index)
- `CreatedDateTime`, `ModifiedDateTime` (datetime, nullable)
- `CreatedByPersonAliasId`, `ModifiedByPersonAliasId` (int, nullable FK to PersonAlias)
- `ForeignId` (int, nullable), `ForeignGuid` (uniqueidentifier, nullable), `ForeignKey` (nvarchar(100), nullable)

Entities inheriting `Entity<T>` get only `Id`, `Guid`, and the Foreign* columns — no audit columns.

---

## PersonAlias vs Person

Audit columns (`CreatedByPersonAliasId`, `ModifiedByPersonAliasId`) reference the `[PersonAlias]` table, **NOT** `[Person]`. This is true for:
- `Model<T>` base class audit columns (automatic)
- Custom FK columns that reference a person (always use `PersonAliasId`, not `PersonId`)

---

## Foreign Key Cascade Conventions

**Default: `WillCascadeOnDelete( false )` / `cascadeDelete: false`** unless there's a clear ownership relationship.

| FK Target | Cascade? | Notes |
|---|---|---|
| PersonAlias (audit columns) | **Never** | Handled by `Model<T>` base — no cascade |
| PersonAlias (custom FK) | **Never** | Deleting a person shouldn't cascade to referencing entities |
| Campus | **No** — use SET NULL | `ON DELETE SET NULL` in migration SQL |
| DefinedValue | **Never** | DefinedValues are shared references |
| Parent-child ownership | **Yes** (rare) | Only when child has no meaning without parent |
| Everything else | **No** | Default to false |

---

## GUID Format

Rock GUIDs must be:
- **Uppercase** (A-F, not a-f) — SystemGuid constants are always uppercase
- **Hyphenated**: `XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`
- 36 characters including hyphens

Note: Some newer entity declarations use lowercase inline GUIDs. When adding new GUIDs, use uppercase to match the SystemGuid convention.

---

## RockContext Usage

- Do **not** dispose `RockContext` prematurely — it kills lazy loading for any entities retrieved from that context.
- Do **not** create a new `RockContext` per iteration in loops. Instead, query all needed data into a list or dictionary before the loop.

---

## Entity Authorization

An entity supports security when it implements `ISecured`. Every `Model<T>` and its `*Cache` class does. A plain `Entity<T>` does not.

**Rule:** Before code returns, changes, or deletes a secured entity for a person, check that person's rights on that record.

```csharp
var group = new GroupService( rockContext ).Get( groupIdKey, false );

if ( group == null )
{
    return ActionNotFound();
}

if ( !group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
{
    return ActionForbidden( "You are not authorized to edit this group." );
}
```

**Which action to check:**

| What the code does | Check |
|---|---|
| Returns or shows the record | `Authorization.VIEW` |
| Creates, changes, or deletes the record | `Authorization.EDIT` |
| Changes the record's security | `Authorization.ADMINISTRATE` |
| A feature gated by a custom action (for example `APPROVE`) | That custom action |

**What does not count as a check:**

- **Block security.** `BlockCache.IsAuthorized()` checks the block, not the record. Use both when the feature needs both.
- **An IdKey or Guid.** These are hard to guess, but anyone who has one can still send it.
- **The record exists, or the person is signed in.**
- **A check in the UI only.** If the UI hides a button, the action behind it must repeat the same check.

**Things that are easy to miss:**

- **VIEW is allowed by default.** When no rule matches, `IsAllowedByDefault` allows VIEW for everyone, including anonymous visitors. So a VIEW check alone does not limit a record that has no rules.
- **Call it on the record itself.** `IsAuthorized` already falls back to the parent through `ParentAuthority`. Don't check only the parent.
- **Create paths need a check too.** When there is no existing record, check the parent named by the posted foreign keys. For example, check a new `GroupMember` against its `Group`.
- **Filter lists per item.** Lists, picker options, and totals sent to the client leave out records the person can't VIEW.
- **Every entry point checks.** A block action, REST endpoint, Lava command, workflow action, job, and AI agent skill tool that do the same thing all check the same rights.
- **Jobs and workflow actions** often run with no current person. With a `null` person, VIEW is still allowed by default.

If you aren't sure which action applies, or whether the block or the record is the right thing to check, ask before you write the code.

---

## LINQ and Database Queries

- Add reusable `.Where()` expressions to the service layer rather than duplicating them in blocks.
- Be cautious with `.Where( p => list.Contains( p.Field ) )` — this produces a `WHERE IN` clause. Large lists can exceed the batch size limit (~65,536 x network packet size, default 4KB).
  - **Preferred:** Use an unexecuted `IQueryable` as the basis for `Contains()` so EF generates a subquery instead.
  - **Fallback:** Break the query into smaller batches and reassemble in memory. Weigh performance carefully.
- Avoid `Guid` in LINQ joins when an `Id` from a cached item is available.

---

## Block Type Methods — Safety Critical

Two method families exist for different block types. **Using the wrong one can cause data loss.**

| Block Type | Method | Example |
|---|---|---|
| **Obsidian / Mobile** (`Rock.Blocks.*`) | `AddOrUpdateEntityBlockType()` | Entity-based, no path |
| **WebForms** (`~/Blocks/*.ascx`) | `UpdateBlockType()` or `UpdateBlockTypeByGuid()` | Path-based |

**DANGER:** `UpdateBlockTypeByGuid()` contains `DELETE FROM [BlockType] WHERE [Path] = '{path}'`. Entity-based block types (Obsidian/Mobile) have an empty `Path`, so this DELETE can wipe out ALL entity-based block types. This caused data loss in production.
