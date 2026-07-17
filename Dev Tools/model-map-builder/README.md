# Model Map Builder

A console tool that generates a JSON "model map" of Rock's entity models and
writes it into source control. It is the file-based counterpart to the live
**Model Map** block (`Rock.Blocks/Example/ModelMap.cs`): same information
(properties, methods, XML doc comments, enums, defined values, obsolete flags,
grouped by domain), but produced as a versioned file instead of a web UI.

## What it produces

A single JSON file (default `Dev Tools/docs/model-map/model-map.json`) shaped as:

```jsonc
{
  "generatedAtUtc": "2026-07-17T21:00:22Z",
  "rockVersion": "20.0.4",
  "domains": [
    {
      "domain": "CRM",
      "models": [
        {
          "name": "Person",
          "properties": [
            { "name": "Gender", "isEnum": true, "enumValues": { "0": "Unknown", "1": "Male", "2": "Female" } },
            {
              "name": "RecordStatusValueId",
              "isDefinedValue": true,
              "definedType": {
                "guid": "8522badd-2871-45a5-81dd-c76da07e2e7e",
                "name": "Record Status",
                "values": [ { "guid": "618f906c-...", "value": "Active", "description": "..." } ]
              }
            }
          ]
          // "methods": [ ... ]  // only present when --include-methods is passed
        }
      ]
    }
  ]
}
```

Each property carries its physical database schema, queried from the live DB:
`dataType`, `length`, `scale`, `isNullable`, and `isPrimaryKey`. Each model
carries its `indexes` and `foreignKeys`. Comments are nested objects with
`summary`, `value`, `remarks`, `returns`, and `example` (empty sections
omitted).

Defined-value properties include the defined type's **name + guid** and its
**system** defined values (guid / value / description); non-system values vary
by installation and are excluded. Enum properties include their value list.
Methods are omitted unless `--include-methods` is passed.

Output is deterministically sorted (domains alphabetically with `Other` last,
models/properties alphabetically), so between runs on unchanged models the only
thing that changes is `generatedAtUtc`.

## How it works

- Reads the `RockContext` connection string from
  `RockWeb/web.ConnectionStrings.config` and stands up a headless `RockApp` so
  Rock's `DefinedTypeCache` can resolve real defined-value rows from the database.
- Discovers models by reflecting over `Rock.dll` (types that are `IEntity` or
  carry `[IncludeForModelMap]`), rather than `EntityTypeCache.All()`. The cache's
  eager type-loading throws in a headless process for component/plugin types;
  reflection is both resilient and a truer reflection of the models in the code.
- Enum values come from reflection; defined-value rows come from the database.
- Physical schema (SQL types, lengths, nullability, primary keys, indexes,
  foreign keys) is read directly from the database catalog
  (`INFORMATION_SCHEMA`, `sys.indexes`, `sys.foreign_keys`) in one pass, since no
  model-map block exposes it. See `DatabaseSchemaReader`.

Because it reads a live database, **your local Rock database must be migrated /
up to date** or the defined values (and any brand-new models) may be stale.

## Running it

Build the project, then run the executable (it works from its own output
folder; missing Rock runtime dependencies are probed from `RockWeb/bin`):

```
msbuild "Dev Tools/model-map-builder/Rock.ModelMapBuilder.csproj" -t:Restore,Build
"Dev Tools/model-map-builder/bin/Debug/net472/Rock.ModelMapBuilder.exe"
```

### Options

| Flag | Default | Description |
|---|---|---|
| `--output <path>` | `Dev Tools/docs/model-map/model-map.json` | Where to write the JSON. |
| `--rockweb <path>` | `<repo>/RockWeb` | RockWeb folder (for the connection string and XML doc fallback). |
| `--compact` | off | Minified output instead of indented. |
| `--include-methods` | off | Include each model's methods. Off by default. |

The tool returns a non-zero exit code on failure, so it can gate a build step.

## Future: GitHub Action

This is designed to later run in CI that regenerates and commits the file when
models change. That workflow will need: a Windows runner with MSBuild (Rock.dll
is .NET Framework 4.7.2), a database the tool can reach, a path filter on
`Rock/Model/**` so it only runs on model changes, and a commit-back step. When
wiring that up, the `generatedAtUtc` timestamp means the file changes every run;
gate the commit on a real content diff or accept the timestamp churn.
