# Lava Documentation

Lava is Rock's templating language: a Liquid-derived markup that lets administrators write merge-field templates against Rock entities, custom data, and computed values. Two engines coexist (legacy DotLiquid and modern Fluid) during a multi-year migration; Fluid is the default since Rock 16.

If you are new, start with [lava-overview.md](lava-overview.md). Sub-topics worth their own docs (Filters, Blocks/Tags, Shortcodes, Lava Applications, Engine Migration, Caching) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Lava Domain Overview](lava-overview.md) | Engine layering, filters/blocks/shortcodes/commands, security gating, and the Fluid migration. |
| [Lava Shortcodes](shortcodes.md) | Built-in C# shortcodes vs user database-backed shortcodes, scope behavior (isolated vs shared), block vs inline form. |
| [The Fluid Migration](the-fluid-migration.md) | Why Fluid, the staged removal of DotLiquid, parity-fix track record, dual-engine support during migration. |
| [Writing Lava Blocks and Tags](writing-blocks.md) | `IRockLavaBlock` / Fluid block interfaces, security gating per block, lifecycle hooks, dual-engine compatibility. |
| [Writing Lava Filters](writing-filters.md) | Filter shape (static, side-effect-free), partial-file organization, identifier filters, dual-engine compatibility. |
