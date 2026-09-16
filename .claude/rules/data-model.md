# Rock Data Model

Rock's data layer conventions: entities, columns, foreign keys, GUIDs, and data access patterns. Always loaded.

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
