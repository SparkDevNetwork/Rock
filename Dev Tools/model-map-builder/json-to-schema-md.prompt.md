# Prompt: Convert Model Map JSON → Rock DB schema markdown

You are a deterministic converter. You are given the JSON produced by the Rock
**Model Map Builder** (`model-map.json`) and must emit a set of markdown files
that document the Rock database schema, one per table plus several index files.
Produce **only** the files described below, with the **exact** formatting shown.
Do not summarize, editorialize, or add commentary. Two runs on the same input
must produce byte-identical output.

---

## Inputs

- `model-map.json` — the full document (structure below).
- `version` — a label like `v20`. If not given, derive it from the JSON's
  `rockVersion` as `v` + the major component (e.g. `"20.0.4"` → `v20`).

## Input JSON structure (camelCase)

```jsonc
{
  "rockVersion": "20.0.4",
  "domains": [
    {
      "domain": "CRM",                         // Rock domain (release-note casing)
      "models": [
        {
          "name": "Person",
          "tableName": "…",                    // present only when it differs from name
          "comment": { "summary": "…(HTML)…", "value": "…", "remarks": "…", "returns": "…", "example": "…" },
          "isObsolete": false,
          "obsoleteMessage": null,
          "properties": [ /* see below */ ],
          "indexes": [ { "name": "IX_Email", "isUnique": false, "isPrimaryKey": false, "columns": ["Email"] } ],
          "foreignKeys": [ { "columnName": "GivingGroupId", "referenceTableName": "Group", "referenceColumnName": "Id" } ]
          // "methods": [...]  // may be present; IGNORE for schema markdown
        }
      ]
    }
  ],
  "entityTypes": [ { "name": "Rock.Model.Person", "model": "Person", "guid": "72657ed8-…" } ],
  "systemDefinedTypes": [ { "name": "Record Status", "guid": "8522badd-…", "definedValues": [ { "guid": "618f906c-…", "value": "Active", "description": "…" } ] } ],
  "systemGroupTypes": [ { "name": "Family", "guid": "790e3215-…", "roles": [ { "name": "Adult", "guid": "2639f9a5-…" } ] } ]
}
```

### Property object

```jsonc
{
  "name": "NickName",
  "comment": { "summary": "…(HTML)…" },
  "dataType": "nvarchar",          // OMITTED when the property is not a real DB column
  "length": 50,                    // char/binary length, or numeric precision for decimal
  "scale": 2,                      // decimal scale only
  "isNullable": true,              // DB column nullability
  "isPrimaryKey": false,
  "required": false,               // from [Required]; use only as a fallback for nullability
  "isEnum": false,
  "enumValues": { "0": "Unknown", "1": "Male", "2": "Female" },   // present only when isEnum
  "isDefinedValue": false,
  "definedType": {                 // present only when isDefinedValue
    "guid": "8522badd-…",
    "name": "Record Status",
    "values": [ { "guid": "618f906c-…", "value": "Active", "description": "…" } ]  // system values only
  }
}
```

---

## Global rules

1. **Only real columns appear in table column lists.** A property is a column
   **iff it has a `dataType`.** Skip navigation/computed properties that have no
   `dataType` (they are not database columns).
2. **Type string.** Combine `dataType`, `length`, `scale`:
   - character/binary types (`char`, `varchar`, `nchar`, `nvarchar`, `binary`, `varbinary`): `"{dataType} ({length})"`, e.g. `nvarchar (50)`. If `length` is `-1`, use `"{dataType} (max)"`.
   - `decimal` / `numeric`: `"{dataType} ({length}, {scale})"` (treat missing `scale` as `0`).
   - everything else (`int`, `bit`, `datetime`, `uniqueidentifier`, …): just `"{dataType}"`.
   Keep the space before `(` exactly as shown.
3. **Nullable column** = `isNullable ? "YES" : "NO"`. If `isNullable` is absent, fall back to `required ? "NO" : "YES"`.
4. **HTML → plain text.** All `comment.*` values are HTML. For any place a
   description is emitted, strip tags to their inner text (`<code>x</code>` → `x`,
   `<a href=…>x</a>` → `x`, `<pre>`/`<p>` → surrounding space), decode entities,
   and collapse runs of whitespace to a single space. Trim.
5. **Pipe escaping.** In any table cell, replace `|` with `\|` after building the
   cell text, so descriptions can't break the markdown table.
6. **Domain folder** = the model's `domain`, lowercased with spaces → hyphens
   (`"CRM"` → `crm`, `"Check-in"` → `check-in`). Blank/absent → `other`.
7. **Determinism.** Apply every sort specified below exactly. Do not reorder
   otherwise.

---

## Output files

### 1. Per-table file → `<domain-folder>/<name>.md`

One file per model. Assemble these sections in order, omitting any that would be
empty:

````markdown
## {name}

{plain-text model description from comment.summary}

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| {col} | {type} | {YES|NO} | {notes} |
...

### Indexes

| Name | Columns | Unique |
|------|---------|--------|
| {index.name} | {index.columns joined ", "} | {YES|NO} |
...

### Enums

**{ColumnName}**

| Value | Meaning |
|-------|---------|
| {key} | {value} |
...

### DefinedValues

System-defined values shown below. Non-system values vary by installation.
Use the helper query to list all values for your environment.

**{ColumnName}** (DefinedType: {definedType.name}, Guid: {definedType.guid})

| Guid | Value | Description |
|------|-------|-------------|
| {v.guid} | {v.value} | {plain-text v.description} |
...

Helper query:
```sql
SELECT [Id], [Value], [Description], [Guid] FROM [DefinedValue]
WHERE [DefinedTypeId] = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{definedType.guid}')
ORDER BY [Order]
```
````

**Columns table** — one row per property that has a `dataType`, in the order they
appear in the JSON (already alphabetical). The **Notes** cell is the following
parts joined with ` | ` (then pipe-escaped), in this priority order, skipping any
that don't apply:

1. `PK` — if `isPrimaryKey`.
2. Foreign key — look up the column in the model's `foreignKeys` by `columnName`:
   - If the property `isDefinedValue` and has `definedType` (or the FK target is `DefinedValue`):
     `FK > DefinedValue.Id (DefinedType: "{definedType.name}", Guid: {definedType.guid})`
     (drop the `, Guid: …` part only if the guid is missing).
   - Otherwise: `FK > {referenceTableName}.{referenceColumnName}`.
   - If there is no FK entry but the property `isDefinedValue` with a `definedType`, still emit the `FK > DefinedValue.Id (DefinedType: …)` form.
3. `Enum (see below)` — if `isEnum` and `enumValues` is non-empty.
4. The plain-text column description (`comment.summary`).

**Indexes section** — only if the model has `indexes`. List the **primary-key
index first** (the one with `isPrimaryKey: true`), then the remaining indexes
sorted by `name` (ordinal). `Unique` = `isUnique ? "YES" : "NO"`.

**Enums section** — only if at least one column has `enumValues`. One block per
such column, in column order. Rows sorted by the numeric key ascending.

**DefinedValues section** — only for columns whose `definedType` has a non-empty
`values` list. Emit the intro paragraph once, then one block (heading + table +
helper query) per such column, in column order.

### 2. `INDEX.md`

```markdown
# Rock RMS {version} Schema

Generated from {model count} models.

| Table | Domain | Description | Path |
|-------|--------|-------------|------|
| {name} | {domain-folder} | {desc, truncated} | {domain-folder}/{name}.md |
```

Rows: every model, sorted by `name` (case-insensitive). `Description` is the
plain-text `comment.summary`, truncated to 120 characters with a trailing `...`
if longer, then pipe-escaped.

### 3. `COLUMNS.md`

```markdown
# Rock RMS {version} -- Column Reference

| Column | Type | Count | Tables |
|--------|------|-------|--------|
| {column} | {type} | {count} | {tables joined ", "} |
```

Build a map keyed by `(column name, type string)` over **all real columns of all
models**. `Count` = number of distinct tables. `Tables` = the table names sorted
alphabetically, comma-joined. Rows sorted by column name (case-insensitive), then
by type. A column that appears with two different type strings gets two rows.

### 4. `ENTITY-TYPES.md`

```markdown
# Rock RMS {version} -- Entity Types

Generated from {entityTypes count} entity types.

| Name | Model | Guid |
|------|-------|------|
| {name} | {model} | {guid} |
```

From `entityTypes`, sorted by `name` (case-insensitive).

### 5. `SYSTEM-GUIDS-DEFINEDTYPES.md`

```markdown
# Rock RMS {version} -- System GUIDs: Defined Types

System-defined DefinedTypes and their known values. Non-system values vary by installation.

Generated from {systemDefinedTypes count} defined types.

## {name}

**Guid:** `{guid}`

| Name | Guid | Description |
|------|------|-------------|
| {v.value} | {v.guid} | {plain-text v.description} |
```

From `systemDefinedTypes`, sorted by `name` (case-insensitive). The table's
**Name** column is each defined value's `value`. If a type has no `definedValues`,
replace the table with `*No system-defined values.*`. Put one blank line after
each type's block.

### 6. `SYSTEM-GUIDS-GROUPTYPES.md`

```markdown
# Rock RMS {version} -- System GUIDs: Group Types

System-defined GroupTypes and their known roles.

Generated from {systemGroupTypes count} group types.

## {name}

**Guid:** `{guid}`

| Role | Guid |
|------|------|
| {role.name} | {role.guid} |
```

From `systemGroupTypes`, sorted by `name` (case-insensitive). If a group type has
no `roles`, replace the table with `*No system-defined roles.*`. One blank line
after each block.

---

## Worked example (Person, abbreviated)

Input property:
```json
{ "name": "NickName", "dataType": "nvarchar", "length": 50, "isNullable": true, "isPrimaryKey": false,
  "comment": { "summary": "Gets or sets the nick name of the Person. If a nickname was not entered, the first name is used." } }
```
Row:
```
| NickName | nvarchar (50) | YES | Gets or sets the nick name of the Person. If a nickname was not entered, the first name is used. |
```

Input property (defined value):
```json
{ "name": "RecordStatusValueId", "dataType": "int", "isNullable": true, "isDefinedValue": true,
  "definedType": { "guid": "8522badd-2871-45a5-81dd-c76da07e2e7e", "name": "Record Status",
    "values": [ { "guid": "618f906c-c33d-4fa3-8aef-e58cb7b63f1e", "value": "Active", "description": "Denotes an individual that is actively participating…" } ] },
  "comment": { "summary": "Gets or sets the Id of the Record Status <code>DefinedValue</code> representing the status of this entity" } }
```
Row:
```
| RecordStatusValueId | int | YES | FK > DefinedValue.Id (DefinedType: "Record Status", Guid: 8522badd-2871-45a5-81dd-c76da07e2e7e) | Gets or sets the Id of the Record Status DefinedValue representing the status of this entity |
```
…and a matching `### DefinedValues` block for `RecordStatusValueId`.

---

## Final instruction

Emit each output file with a clear delimiter identifying its path (e.g. a heading
line `=== FILE: crm/Person.md ===` followed by the file's content), so the files
can be split apart programmatically. Produce every per-table file plus the six
index files. Nothing else.
```
