---
author: Jason Hendee
date_created: 2026-09-08
summary: >-
  The Bulk Update block could not accept a value for a Matrix attribute. The
  form for adding a matrix item rendered with no fields, so the only thing it
  could save was an empty item. The block seeded every attribute editor with an
  empty string, but the Matrix editor carries its column definitions inside the
  value rather than in the attribute configuration, so an empty string left it
  with nothing to render. The fix supplies a real starting value for Matrix
  attributes only, leaving every other field type exactly as it was.
contributors: []
---

# Bulk Update Matrix Attribute Cannot Be Populated

## Summary

A Matrix attribute configured on Person, Group Member, or Step could not be given a value through the Bulk Update block. Toggling the attribute on showed an empty grid, and the form for adding an item came up with no fields to complete, so clicking Add stored a blank item. The cause is that the Matrix field type ships its item column definitions inside the attribute value, while every other field type takes them from the attribute configuration. Bulk Update started all of its editors at an empty string, which is fine for every other field type and fatal for this one. The fix supplies a real starting value for Matrix attributes only, so no other field type changes behavior. It is implemented on `release-20.0`.

## Problem Statement

Bulk Update presents an attribute editor per attribute so an operator can push one value onto many people at once. Unlike a detail block, it has no existing entity to read a value from, so it started every editor at an empty string. The Matrix editor derives its own schema from that value, so it received no columns, rendered no inputs, and produced items with no data. The result is that a Matrix attribute is visible in the block but cannot be filled in.

## Reproduction

Affects Rock v20.0.x, where the Obsidian Bulk Update block first shipped. The WebForms block was not affected.

1. Create an Attribute Matrix Template with at least one item attribute.
2. Add a Person attribute using the Matrix field type, pointed at that template, in a category selected by the block's Attribute Categories setting.
3. Open a data view, select several people, and choose Bulk Update.
4. Toggle the Matrix attribute on and click the add button on its grid.
5. The item form appears with no fields to complete. Clicking Add inserts a blank row.

## Root Cause

`MatrixFieldType.GetPublicEditValue` at `Rock/Field/Types/MatrixFieldType.cs:146` returns a JSON payload that carries both the matrix items and the editor's schema: the item attribute definitions, their default values, and the template's row limits. The client editor takes its schema from that payload and nowhere else.

The Obsidian Matrix edit component at `Rock.JavaScript.Obsidian/Framework/FieldTypes/matrixFieldComponents.ts:41` parses the incoming value and falls back to an empty schema when parsing fails. An empty string fails to parse, so the schema is empty. The add and edit forms at `Rock.JavaScript.Obsidian/Framework/Controls/Internal/attributeMatrixEditor.obs:15` then render an attribute values container with no attributes, which produces a form with no inputs.

Bulk Update never asked the server for an edit value. It shipped only the attribute definitions and seeded each value with an empty string. Detail blocks avoid this by calling `GetPublicAttributeValuesForEdit`, which runs every attribute through the field type's edit conversion even when the stored value is blank, so the Matrix payload arrives fully populated. The legacy WebForms block avoided it a different way: `AttributeCache.AddControl` built an `AttributeMatrixEditor` server control that loaded the template itself, so no seed value was needed.

## Affected Code Paths

Primary, where the fix lands:

- `Rock.Blocks/Crm/BulkUpdate.cs` - builds the attribute lists for all three sections, and now also the starting value for any Matrix attribute among them, in `GetMatrixAttributeValues` at line 725. The field type it checks against is held at line 119.
- `Rock.JavaScript.Obsidian.Blocks/src/Crm/bulkUpdate.obs` - applies those values when building its five attribute editors, starting at line 597.
- `Rock.JavaScript.Obsidian.Blocks/src/Crm/BulkUpdate/utils.partial.ts` - shared helpers for building the editor items and the change-summary lines.
- `Rock.JavaScript.Obsidian.Blocks/src/Crm/BulkUpdate/matrixChangeTable.partial.obs` - renders a Matrix attribute's rows on the confirmation screen using the editor's own cell components.
- `Rock.JavaScript.Obsidian.Blocks/src/Crm/BulkUpdate/optionalControl.partial.obs` - wraps every optional item in the block, and now gates the form state so an unselected item cannot report validation errors.
- `Rock.JavaScript.Obsidian.Blocks/src/Crm/BulkUpdate/types.partial.ts` - return type of the two attribute block actions.
- `Rock.ViewModels/Blocks/Crm/BulkUpdate/BulkUpdateAttributeCategoryBag.cs` and its generated `bulkUpdateAttributeCategoryBag.d.ts` - new `MatrixAttributeValues` property.
- `Rock.ViewModels/Blocks/Crm/BulkUpdate/BulkUpdateAttributesBag.cs` and its generated `bulkUpdateAttributesBag.d.ts` - new return shape for the group member and step attribute block actions, which previously returned a bare attribute list with nowhere to put values.

Secondary, unchanged but relevant to verification:

- `Rock/Crm/BulkUpdate/BulkUpdateProcessor.cs:1478` - `ApplyAttributeValues` converts each submitted value to its stored form once per entity, so every selected person receives their own Attribute Matrix rather than a shared one.
- No framework or field type code was modified. The Matrix field type, the matrix editor control, `PublicAttributeHelper`, and `AttributeValuesContainer` are all untouched.

## Workarounds

User-side, until the fix ships: edit the Matrix attribute on each person individually from their profile. There is no way to set it in bulk.

## Proposed Fix

Implemented as described.

1. For each Matrix attribute, and only a Matrix attribute, the block converts a blank stored value through `PublicAttributeHelper.GetPublicValueForEdit`. That yields the payload carrying the template's columns, which is what the editor needs. The field type check lives in `GetMatrixAttributeValues` at `Rock.Blocks/Crm/BulkUpdate.cs:725`, with an engineering note recording why it is scoped that way.
2. Those starting values travel to the browser alongside the attribute definitions. The person attribute category bag gained a `MatrixAttributeValues` dictionary, and the group member and step block actions now return a bag holding both the definitions and that dictionary instead of a bare list. It is named for Matrix rather than paired generically with `Attributes` so that nothing else gets added to it by mistake; on most configurations it is empty.
3. The block applies those values when building its editors. Every non-Matrix attribute still starts at an empty string and reaches the server exactly as it did before.

Supplying a real value surfaced a second defect, because the Matrix editor derives form validation from that value. The template's row minimum arrived with it, and the editor turns a row minimum into a rule on the enclosing form plus a warning shown on load. So opening the block showed "At least 1 item is required." and the bulk update could not be submitted, even with the attribute unselected. The same mechanism blocks on a required attribute of any field type, because `RockField` turns `IsRequired` into a required rule and the Matrix editor reads that as a minimum of one row, so this was a latent defect for ordinary attributes too, not a Matrix problem.

The block had no way to scope validation to the attributes actually selected. The WebForms block scoped it by disabling the controls of unselected items, so their validators never ran. Obsidian keeps every field mounted with its own rules, and a field reports its error into the form state it injects.

- **`OptionalControl` now provides a gated form state.** Errors from fields inside it pass through to the real form only while the item is active. Because a field reports only when its value or its rules change, and toggling the item is neither, both directions are replayed from the wrapper: switching an item off clears what the form is holding for its fields, and switching it on restores it. Nothing is removed from validation and no rule is weakened. The gate covers every optional item in the block, not just attributes, since an unselected field of any kind should not be able to block the run.
- **The opt-in editor reproduces the WebForms required split.** The WebForms block built person attribute controls with required hardcoded off, and group member and step attribute controls with the attribute's real setting. That is why a person attribute could be cleared by selecting it and leaving it blank. The opt-in editor now takes an `isRequiredEnforced` flag, off by default for the person categories and on for the group member and step sections, so each path keeps the behavior it had. The add paths were already unaffected, since they use `AttributeValuesContainer` and always honored required, matching WebForms.

One further supporting change is part of the same fix:

- **The confirmation screen renders the matrix as a table.** The Matrix field type's client `getTextValue` yields no text for an edit payload, and the block's formatter falls back to the raw value when there is no text, so the confirmation screen printed the entire JSON payload. A Matrix attribute now contributes a table segment instead, rendered by `matrixChangeTable.partial.obs`: columns from the payload's item attributes in the editor's order, rows in the editor's order, and each cell handed to the same condensed component the editor's own grid uses. That last part is what makes complex columns read correctly, since a cell value is the field type's structured view representation rather than display text, so a defined value or a person would otherwise print as raw data. A matrix with no items reads as a clear, and an unreadable value reads as a clear too, which is what the server does with one when saving. The item count remains only as a fallback. Every part of this is reached through the Matrix field type check, so no other field type's summary output moves.

  The WebForms block produced a comparable table by resolving the matrix template's own formatting Lava, which it could do because it saved the matrix and its items as they were entered. Nothing is saved here until the run is confirmed, so the table is built in the browser from the payload. The visible difference is that a customized formatting template is not honored; the shipped default is a table of the same shape.

## Fix Risks

Scoping the change to the Matrix field type removes most risks. Non-Matrix attributes are given no starting value, are compared against nothing, and are formatted no differently, so their submitted payload is byte for byte what it was before. What remains:

- **A Matrix attribute with a configured Default Value.** On the group member and step add paths the editor's starting value is submitted even when nobody touched it, and it converts back to an empty stored value. A newly created group member or step would therefore have that default cleared rather than kept. A default value on a Matrix attribute is close to meaningless in practice, so this is documented rather than guarded. It cannot reach any other field type.
- **Public signature change.** The two attribute block actions changed return type. They are public methods invoked by name from this block's own component, so the practical risk is limited to a plugin that subclasses the block or calls them directly.
- **Extra work at block load.** The conversion runs once per Matrix attribute when the block renders, and issues a couple of queries each. Nothing runs for other field types, and nothing runs per selected person.
- **The gated form state is a wrapper around a framework contract.** It depends on `FormState` staying a two-member shape and on fields reporting through injection. Both are stable today and the wrapper is small, but a change to how `RockForm` collects errors would need a look here.
- **A template row minimum still blocks a selected Matrix attribute, so such an attribute cannot be cleared.** This is not new. The WebForms editor enabled its row-count validator whenever the effective minimum exceeded zero, so it blocked the same case with the message "At least 1 row is required." A Matrix attribute can only be cleared through this block when its template asks for no minimum rows, which was equally true before the conversion.

## Test Plan

**Environment prerequisites:** an Attribute Matrix Template with at least two item attributes of differing field types (Text and Defined Value work well), a Person attribute of type Matrix in a category the block is configured to show, a required Person text attribute, a Group Member attribute of type Matrix with Show on Bulk checked, a Step attribute of type Matrix with Show on Bulk checked, and a data view returning at least three people.

Note that the three sections gate their attributes differently. Person attributes come from the block's Attribute Categories setting and ignore Show on Bulk entirely. Group member and step attributes are filtered by that flag.

### T1. Person Matrix attribute (the reported case)

- [x] Toggling the attribute on shows a grid whose column headers match the template's item attributes.
- [x] The add-item form shows one input per template item attribute. This is the reported defect.
- [x] Entering values and clicking Add produces a populated row, not a blank one.
- [x] A second item can be added and both rows show their values.
- [x] Clicking an existing row opens the edit form pre-filled, and saving updates the row.
- [x] Deleting a row removes it.
- [x] Dragging to reorder two rows sticks, and the order survives submission.
- [x] After submitting, every selected person shows the matrix with the entered items.
- [x] Each person has their own matrix: changing one person's items afterward leaves the others unchanged.
- [x] A person who already had a value for that attribute receives the new one.
- [x] Toggling the attribute on with zero items clears the value on the selected people.
- [x] Leaving the attribute toggled off writes nothing to anyone.
- [x] **Rollup: T1 complete.**

### T2. Validation scoping

The riskiest part of the fix, because it touches every optional item in the block rather than only attributes.

- [x] Nothing warns or blocks on block load. This is the reported regression.
- [x] A required text attribute left unselected does not block submission. This case was broken before the fix as well, and it proves the scoping works beyond Matrix.
- [x] A required Person attribute, selected and left blank, submits and clears the value. Person attributes are not enforced as required, matching the WebForms block.
- [x] A required Group Member attribute, selected on the update path, still blocks until it is filled. Required is enforced there, also matching the WebForms block.
- [x] Unselecting an attribute whose value is invalid and then reselecting it brings the error back. This is the one failure mode the design could reintroduce.
- [x] **Rollup: T2 complete.**

### T3. The other two attribute sections

Each reaches the server through a block action whose return type changed, so each needs one pass.

- [x] A Group Member Matrix attribute, selected on the update path, applies to existing members.
- [x] A Step Matrix attribute on the add path applies to newly created steps.
- [x] Switching to a different group or step type resets the editors, with no stale matrix state.
- [x] **Rollup: T3 complete.**

### T4. No collateral damage

- [x] A Person text attribute and a Person defined value attribute both still apply, and both read as friendly text on the confirmation screen.
- [x] A non-Matrix attribute on the Group Member add path still applies, with required still enforced on that path.
- [x] No raw payload data appears anywhere on the confirmation screen.
- [x] A Matrix attribute's confirmation table matches the grid on the entry screen: same columns in the same order, same rows in the same order, and complex columns such as a defined value reading as their label rather than raw data.
- [x] **Rollup: T4 complete.**

### Deliberately not covered

Trimmed for time, and listed here so none of it is mistaken for having passed: template row minimum and maximum handling, which was checked by hand and matches the WebForms block; a template with no item attributes, or one deleted after a value was saved; attribute default values on the add paths; the authorization fences for attributes, groups and step types; a large batch; block load timing; and an attribute configured in two selected categories.
## Considered but Rejected

### Supply a starting value to every attribute, not just Matrix
Rejected, and this was the shape of the first draft of this fix. Running every attribute through the conversion is more uniform and would cover any other field type with the same problem, but it changes what gets submitted for field types whose conversion does not return an empty string for a blank input. Phone Number returns a payload carrying the default country code, and Value Filter returns a serialized empty filter expression whose round trip is not empty, so an attribute toggled on and left alone would be written rather than cleared. Covering that would have meant either enumerating field types in the test plan or adding change detection on both the add and update paths. Scoping to Matrix leaves every other field type provably untouched and needs neither.

### Summarize matrix values in the field type's own getTextValue
Rejected for this branch. Changing `MatrixFieldType.getTextValue` in the framework would fix the raw JSON display for every consumer at once, but it is shared code on a hotfix branch and the same method feeds grid columns and condensed HTML output. Worth revisiting on `develop`.

### Move the matrix schema into the field type's public configuration values
Rejected as out of scope, though it is the architecturally correct fix. If `GetPublicConfigurationValues` exposed the template's item attributes and row limits, the editor would no longer depend on the value at all, and every block would work without a seed value. That is a field type contract change with a much wider blast radius than this issue warrants, and it needs a review of anything reading the schema out of the value today.

### Reuse an existing bag for the two block actions
Rejected. `AttributeMatrixEditorNormalizeEditValueOptionsBag` has exactly the right shape but is a request bag for a specific control REST endpoint, and `EntityBagBase` is an abstract entity bag carrying an `IdKey`. Neither fits a block action result.

### Name the new dictionary `AttributeValues` to match the usual bag pairing
Rejected. `Attributes` alongside `AttributeValues` is the established pairing in `EntityBagBase` and dozens of bags, and in all of them the second means the values for all of the first. Here it holds Matrix keys and nothing else, so the familiar name would invite a later contributor to add other starting values to it, which is how the Phone Number and Value Filter problem described above would return. `MatrixAttributeValues` makes the constraint structural instead of leaving it to a doc comment. `InitialAttributeValues` was considered as a middle option and rejected for the same reason: it still reads as general. If the scope ever genuinely widens, renaming is part of that deliberate change rather than something that can slide in unnoticed.

### Fix only the person attribute path
Rejected. The group member and step paths start their editors the same way and fail identically, so fixing one would leave two known-broken surfaces behind.

## Out of Scope

- The same blank-value pattern appears in other blocks that build a new record, including Step Bulk Entry, Prayer Request Entry, the Group Placement add-group modal, and Registration Entry. A Matrix attribute on any of those would fail the same way. Each block builds its own editor values, so this fix does not reach them. Worth a separate ticket, ideally alongside the field type configuration change described above.
- Phone Number and Value Filter both return a non-blank value from the conversion this fix uses for Matrix. Nothing here depends on that, and neither is changed, but it is the reason the fix is scoped the way it is and may matter to a future change in this area.
- Bulk Update replaces an existing matrix value with a newly created Attribute Matrix rather than updating the existing one, which can leave orphaned rows behind. Pre-existing behavior of the field type's save path, not introduced or addressed here.
- Two leftover debug logging calls in the Attribute Matrix Template Detail block, noticed while checking how other blocks compare an attribute's field type. Unrelated to this issue.

## Related

- [GitHub issue #7024](https://github.com/SparkDevNetwork/Rock/issues/7024) - the original report, treated as canonical for reproduction steps.
- [Asana DEV-15480](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1218158021210542) - synchronized with the GitHub issue, referenced for priority and version targeting only.
- `d27d9ed5f7` - the Obsidian conversion of the Bulk Update block, where the blank-value pattern originated.
- `eb0374abf9` - a prior fix to the same block on this branch, whose commit message shape this fix follows.
