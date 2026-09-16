---
author: Jon Edmiston
date_created: 2026-08-11
summary: >-
  Adds conditional visibility to workflow forms built through the Workflow
  Builder agent skill. A field can be shown only when another attribute holds a
  given value, which is the difference between a form that asks everything and a
  form that adapts. Extends the existing field input rather than adding a tool,
  because the rules belong to the field they govern.
contributors: []
---

# Conditional Workflow Forms

## Summary

`AddOrUpdateWorkflowActionForm` can build a form, but every field on it is always
visible. Rock supports conditional visibility and stores the rules in a column the
skill does not touch, so any form more sophisticated than "ask everything at once"
cannot be produced through the agent.

This adds visibility rules to the existing field input, and reports them in the read
result.

**Fields only.** Rock also supports rules on a whole section, and this spec covered
them in an earlier draft. Sections were then dropped from the skill entirely, so
section rules went with them. See the Workflow Builder spec, tool 10, "No sections and
no column widths".

Companion to [the Workflow Builder skill](260807-ai-agent-workflow-builder-skill.md),
whose tool 10 this extends. Assumes the
[shared tool conventions](260807-ai-agent-tool-conventions.md).

## Motivation

A form that asks every question regardless of the answers is the one thing an agent
can currently build. Real forms branch: ask for the reason only when the answer was
"no", collect shipping details only when delivery was chosen, hide the follow-up
question from anyone it does not apply to.

The gap is invisible from the outside. `GetWorkflowType` returns a form built in
Rock's UI with its rules silently dropped, so an agent reading that form concludes the
fields are unconditional. Editing it then writes that wrong conclusion back, because
supplying `fields` replaces every field. **Adding a single field to a conditional form
through the current tool destroys every rule on it.** That makes this a correctness
problem, not only a missing feature.

## Requirements

- A field MUST be able to carry visibility rules.
- A rule MUST reference the attribute it tests by IdKey, never by raw GUID or by index.
- The compared-to attribute MUST be validated against the same scope a form field is:
  the workflow's attributes plus the containing activity's.
- A read MUST return the rules on any form that has them, so an edit round trip
  preserves what it did not change.
- Rules MUST NOT be expressible as raw JSON by the caller.

## Design

### Where Rock keeps this

One column:

| Column | Table |
|---|---|
| `FieldVisibilityRulesJSON` | `WorkflowActionFormAttribute` (`WorkflowActionFormAttribute.cs:136`) |

Rock has a matching `SectionVisibilityRulesJSON` on `WorkflowActionFormSection`
(`WorkflowActionFormSection.cs:81`) holding the same serialized type. It is not used
here, because this skill does not write sections.

The model exposes a typed accessor that serializes to and from
`Rock.Field.FieldVisibilityRules`, so nothing here needs to touch JSON directly:

```csharp
// WorkflowActionFormAttribute.Logic.cs:45
return FieldVisibilityRulesJSON.FromJsonOrNull<Field.FieldVisibilityRules>()
    ?? new Field.FieldVisibilityRules();
```

The shape is small:

```csharp
public class FieldVisibilityRules
{
    public List<FieldVisibilityRule> RuleList { get; set; }
    public FilterExpressionType FilterExpressionType { get; set; } = FilterExpressionType.GroupAll;
}

public class FieldVisibilityRule
{
    public Guid? ComparedToFormFieldGuid { get; set; }
    public ComparisonType ComparisonType { get; set; }
    public string ComparedToValue { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
}
```

**`ComparedToFormFieldGuid` is the attribute's GUID in the workflow context.** The name
suggests a form field, and for registration templates it is one, but `Evaluate` falls
back to the attribute when the registration lookup misses
(`FieldVisibilityRules.cs`, inside `Evaluate`):

```csharp
var comparedToField = RegistrationTemplateFormFieldCache.Get( rule.ComparedToFormFieldGuid.Value );
var comparedToFieldAttributeId = comparedToField?.AttributeId
    ?? AttributeCache.Get( rule.ComparedToFormFieldGuid.Value )?.Id;
```

Workflow forms take the second branch. That is convenient: attribute GUIDs are already
in the read result, so the skill can resolve an IdKey to the right value with no new
lookup.

### Input shape

Two new properties on the existing field input, plus one new input class:

```csharp
// on WorkflowFormFieldInput
public List<WorkflowFormVisibilityRuleInput> VisibilityRules { get; set; }
public FilterExpressionType? VisibilityRuleMatch { get; set; }

internal class WorkflowFormVisibilityRuleInput
{
    public string ComparedToAttributeIdKey { get; set; }
    public ComparisonType ComparisonType { get; set; }
    public string ComparedToValue { get; set; }
}
```

`VisibilityRuleMatch` maps to `FilterExpressionType` and defaults to `GroupAll`, so
several rules must all pass. `GroupAny` is the common alternative. The enum also
carries `Filter`, `GroupAllFalse`, and `GroupAnyFalse`; only the two group values are
meaningful at this level, and the tool should say so rather than silently accepting the
others.

### The nesting works, and was verified deeper than it is used

`fields` → `VisibilityRules` is two levels. When this was designed the form input was
still `sections` → `Fields` → `VisibilityRules`, which was three, so three is what got
tested.

**Verified against Semantic Kernel 1.67.1.** `KernelFunctionFactory.CreateFromMethod`
describes all three levels in the generated schema, and a payload nested three deep
binds correctly through the same string coercion the MCP server applies. Dropping
sections took the shape back to two levels, so the verified headroom now exceeds what
the tool actually asks for.

No fallback shape is needed. Had it failed, the alternative was a flat list of rules on
the tool itself, each naming its target field, which is worse in every way that matters:
it separates a rule from the thing it governs, and it needs a stable field identity that
the replace-by-absence design deliberately does not have.

### Output shape

`WorkflowFormFieldResult` gains the mirror:

```
VisibilityRules[] {
  ComparedToAttributeIdKey, ComparedToAttributeKey, ComparedToAttributeName,
  ComparisonType, ComparedToValue
},
VisibilityRuleMatch
```

The attribute is resolved to its key and name as well as its IdKey, for the same reason
criteria are: a rule that reads `pQ7mZ equals Yes` is not reviewable, and the agent
should not have to cross-reference the tree to explain a form.

Both are omitted entirely when there are no rules, so an unconditional form stays as
small as it is today.

### Validation

1. **The compared-to attribute must be in scope**, meaning the workflow's own attributes
   or the containing activity's. This is the same check tool 10 already applies to a
   field's own `AttributeIdKey` and tool 9 applies to criteria, so it reuses
   `GetReferenceableAttributes`.
2. **Reject an out-of-scope attribute rather than storing it.** An unresolvable
   `ComparedToFormFieldGuid` makes `Evaluate` skip the rule, so the field is simply
   always visible and nothing reports why.
3. **Warn when a rule references an attribute that is not on the form.** It is legal,
   and it is occasionally deliberate when an earlier action set the value, but far more
   often it means the caller pointed at the wrong attribute. Warn, do not block.
4. **Reject `FilterExpressionType` values other than `GroupAll` and `GroupAny`.** The
   other three are meaningful in reporting, not here.
5. Validate everything before deleting anything, as tool 10 already does. A bad rule
   must leave the existing form untouched rather than half rebuilt.

### Forward references are allowed

A rule can name an attribute whose field appears later on the form, or on no field at
all. Rock evaluates against attribute values rather than form position, so this is not
an error, and forbidding it would break the legitimate case where a prior action set the
value. Requirement 3's warning covers the mistake without blocking the intent.

## Out of Scope

| Item | Reason |
|---|---|
| Person entry field visibility | Person entry configuration is out of scope for the skill as a whole. |
| Registration template form fields | Same columns, different table and a different skill. |
| `Filter`, `GroupAllFalse`, `GroupAnyFalse` | Not meaningful for a form field, and exposing them invites a model to pick one. |
| Rule-level GUID stability | `FieldVisibilityRule.Guid` self-assigns. Since fields are replaced wholesale, rule identity has no consumer. |
| Section visibility rules | There are no sections. See the Summary. |

## Decisions without precedent

### 1. Three-level nested input

Verified above. The skill's previous ceiling was two.

### 2. A validation that warns rather than refuses

Requirement 3 reports a probable mistake without blocking a legal configuration. Every
other check in the skill either accepts or refuses. This one cannot refuse without
breaking the prior-action case, and cannot stay silent without letting an obvious typo
through.

## Related

- [260807-ai-agent-workflow-builder-skill.md](260807-ai-agent-workflow-builder-skill.md) — tool 10 is what this extends.
- [260807-ai-agent-tool-conventions.md](260807-ai-agent-tool-conventions.md) — the shared conventions this assumes.
