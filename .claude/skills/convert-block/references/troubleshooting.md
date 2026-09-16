# Troubleshooting Reference

Common issues and solutions encountered during Obsidian block conversions.

---

## Block Not Found

If the block path cannot be resolved:
1. Use `Glob` to search: `RockWeb/Blocks/**/$ARGUMENTS.ascx.cs`
2. Try partial name matches if the exact name fails
3. Ask the user to confirm the category and block name

---

## Category Ambiguity

Some block names exist in multiple categories (e.g., `Detail` blocks in `Core/` vs `Crm/`). When multiple matches are found:
1. List all matches with full paths
2. Ask the user which one to convert

---

## Unsupported WebForms Patterns

These patterns require special attention during conversion. They don't block the conversion but need careful handling:

| Pattern | Issue | Approach |
|---|---|---|
| `System.Web.HttpContext` | Not available in Obsidian | Use `RequestContext` properties instead |
| `ScriptManager.RegisterStartupScript` | WebForms-only | Replace with Vue reactivity or `invokeBlockAction` |
| `UpdatePanel` / `__doPostBack` | Partial postback | Replace with block actions + reactive state |
| `ViewState` | Server-side state | Move to component `ref`/`reactive` state or block config |
| Nested `UserControl` references | `.ascx` includes | Convert to Vue partial components (`.partial.obs`) |
| `Page.IsPostBack` | WebForms lifecycle | Not needed — Vue handles initial vs. update rendering |
| `<%# Eval("...") %>` / `<%# Bind("...") %>` | Data binding | Replace with Vue template bindings |
| `Session` access | Server session state | Use block configuration, person preferences, or block actions |

If a block relies heavily on `System.Web` patterns throughout (not just a few lines), flag this to the user in Phase 2 — it may need a more significant redesign rather than a line-by-line conversion.

---

## Windows Compatibility

- Use `Glob` tool instead of `find` for file searches
- Use forward slashes in paths when passing to tools
- Git commands work the same on Windows with bash shell

---

## Common Build Errors After Conversion

| Error | Cause | Fix |
|---|---|---|
| `CS0246: type or namespace 'XBag' could not be found` | Bag namespace mismatch | Verify namespace is `Rock.ViewModels.Blocks.[Category].[BlockName]` |
| `CS0534: does not implement inherited abstract member` | Missing base class override | Check which abstract methods the base class requires |
| `TS2307: Cannot find module` | Missing `.d.ts` placeholder | Create the placeholder `.d.ts` file for the bag |
| `TS2345: Argument of type ... is not assignable` | Type mismatch in bag property | Verify the C# bag property type maps to the correct TypeScript type (e.g., `string` → `string`, `int/decimal` → `number`, `Guid` → `string`, `ListItemBag` → `ListItemBag`, `DateTime` → `string`) |

---

## Common Runtime Issues

| Issue | Cause | Fix |
|---|---|---|
| Block shows "Block not found" | `BlockTypeGuid` not registered | Run Rock.CodeGeneration or add a migration |
| Attributes don't save | Missing `entity.SaveAttributeValues()` after `SaveChanges()` | Add the call after `RockContext.SaveChanges()` |
| Grid shows no data | `GetListQueryable` returns wrong query | Verify the LINQ query and check for missing `.Include()` calls |
| Edit panel fields don't persist | Using `ref` instead of `propertyRef` | Switch to `propertyRef` and wire up `ValidPropertiesBox` |
