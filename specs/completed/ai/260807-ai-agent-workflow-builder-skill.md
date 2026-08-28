---
author: Jon Edmiston
date_created: 2026-08-07
summary: >-
  An AI agent skill that authors Rock workflow types. Fourteen tools that
  discover the installed workflow action components and their settings, then
  create, edit, and remove workflow types, attributes, activities, actions,
  and forms by writing entities directly. Container export and import are out
  of scope. Distinct from the existing WorkflowSkill, which executes a
  configured allow-list rather than authoring anything.
contributors: []
---

# Workflow Builder Agent Skill

## Summary

Fourteen tools that discover workflow actions and then create, edit, and remove workflow
structure by writing entities directly. Container export and import are out of scope, see
[Out of Scope](#out-of-scope).

Companion to the [Core Administration skill](260807-ai-agent-core-administration-skill.md),
which supplies the configuration metadata the workflow actions point at. Both assume the
[shared tool conventions](260807-ai-agent-tool-conventions.md) and note only their departures.
The identifier rule they both rely on is implemented by
[the result GUID change](completed/ai/260807-ai-agent-result-guids.md).

## Motivation

Building a workflow by hand means knowing which action components are installed, what settings
each one accepts, and which of a dozen reference tables a given setting expects a key from.
None of that is discoverable from the outside, and a static knowledge pack cannot supply it
because plugins install their own actions.

Scope was derived by tracing three demo builds and listing every value that had to be looked
up, guessed, or asked for by hand. Two of those gaps produced real defects: a wrong field type
class name broke an import, and a guessed icon class shipped dead. Neither raised an error.

The failure mode this skill exists to prevent is silent. An agent that cannot resolve a
reference does not fail loudly; it explains the workflow wrong, or edits the wrong thing.

## Requirements

- The skill MUST discover action components by reflecting over the installed container, so
  plugin and custom actions appear alongside core ones.
- Every write MUST validate its references against the corresponding lookup tool and return an
  error naming the bad value, rather than surfacing a foreign key error.
- A read MUST NOT drop rows from a collection. A single long value MAY be clipped, provided it
  is flagged and a companion tool returns it whole.
- Every node in a read result MUST carry its own IdKey and Guid, with no exceptions.
- A variable MUST be creatable at either scope, workflow or activity, and a read MUST report
  which scope each one is in.
- A reference to an attribute, whether from a form field or an action's criteria, MUST be
  validated against both the workflow's attributes and the containing activity's. Narrower
  rejects valid configurations; wider accepts references that silently never resolve.
- A change that would silently corrupt stored values MUST be refused while instances exist,
  and the error MUST name the instance count.
- Every delete MUST count what it is about to destroy and report it before acting.
- Deleting a workflow type MUST require an explicit confirmation flag, enforced by the tool
  rather than left to the agent.

## Design

### Skill declaration
```
[Description( "Provides the ability to author Rock workflow types: discovering actions and their settings, and creating, editing, and removing workflow structure." )]
[AgentSkillGuid( "A74514DD-9955-49D6-8DC3-A33033797B0A" )]
[EntityTypeGuid( "7A9C6D45-947B-4B32-A09E-23718F0C8A08" )]
internal sealed partial class WorkflowBuilderSkill : AgentSkillComponent
```

This is distinct from the existing `WorkflowSkill`, which **executes** a configured
allow-list of workflow types. This skill authors, and is deliberately not constrained by that
configuration. Leave a comment on both saying so, or someone will "fix" the overlap by
deleting one.

Scope was derived by tracing the three demo builds under `demo/` and listing every value that
had to be looked up, guessed, or asked for by hand. Two of those gaps produced real defects:
a wrong field type class name broke an import, and a guessed icon class shipped dead.

### Tool inventory

| # | Tool | Kind | Paging |
|---|---|---|---|
| 1 | `LookupWorkflowActionComponents` | Lookup | none |
| 2 | `GetWorkflowActionTypeAvailableAttributes` | Get | none |
| 3 | `ListWorkflowTypes` | List | page number |
| 4 | `GetWorkflowType` | Get | none |
| 5 | `GetWorkflowActionType` | Get | none |
| 6 | `AddOrUpdateWorkflowType` | Write | none |
| 7 | `AddOrUpdateWorkflowAttribute` | Write | none |
| 8 | `AddOrUpdateWorkflowActivityType` | Write | none |
| 9 | `AddOrUpdateWorkflowActionType` | Write | none |
| 10 | `AddOrUpdateWorkflowActionForm` | Write | none |
| 11 | `DeleteWorkflowAttribute` | Delete | none |
| 12 | `DeleteWorkflowActivityType` | Delete | none |
| 13 | `DeleteWorkflowActionType` | Delete | none |
| 14 | `DeleteWorkflowType` | Delete | none |

---

### 1. LookupWorkflowActionComponents

`[AgentToolGuid( "D319EB2C-F2CE-44F2-80E1-0705C6AC68DF" )]`

```csharp
public AgentToolResult LookupWorkflowActionComponents( string partialName = null, string category = null )
```

**Output.** `EntityTypeIdKey, ClassName, Name, Category, Description`.

`Name` is the `ComponentName` export metadata. `EntityTypeIdKey` is what tools 2 and 9 take.
`ClassName` is returned so the agent can recognize an action it reads elsewhere, and matches
what the knowledge pack articles name. It is never passed back.

**No `EntityTypeGuid`.** Nothing consumes it. `WorkflowActionType.EntityTypeId` is an Id, and
every tool that names a component takes `actionEntityTypeIdKey`. The knowledge pack keys its
articles on class name. A GUID with no consumer is weight on all 145 rows of a lookup that
lives permanently in history.

This is not the structural exception in the GUID spec, which keeps `Guid` on a `List` that has
no partner `Get`. That exception exists for values a caller genuinely needs and cannot reach
any other way. Here the value is not needed at all, so no partner `Get` is warranted either.

#### On the name

Two names were rejected, and both rejections matter because both will be proposed again.

**Not `LookupWorkflowActions`.** `WorkflowAction` is a real Rock table: a running instance of
a configured action. A tool named for it that returns installed components is wrong in a way
that is invisible until someone trusts the name.

**Not `LookupWorkflowActionType`.** `WorkflowActionType` is also a real table: one configured
action inside a `WorkflowActivityType`. Tool 5 is already named for it and returns exactly
that. Using it here would put two tools in one skill named for one entity while returning
unrelated things.

This tool returns neither. It reflects over `ActionContainer.Instance` and returns installed
**action components**, whose only identity is their `EntityType`.

`ActionComponent` is a real Rock type, not invented vocabulary: `ActionContainer` is declared
as `Container<ActionComponent, IComponentData>`. The conventions rule is against naming a tool
after an entity that does not exist. `ActionComponent` exists, and it is the only one of the
three names that describes what comes back.

**Volume.** 145 action components in core, plus plugins.

**Paging.** None, and no cap. The set is bounded by installed code, not by data.

**History.** `.WithHistoryKey( "workflow-actions" )`.

**Notes.**

1. Source is `ActionContainer.Instance`, reflected over installed components, so plugin and
   custom actions appear alongside core ones. This is the piece no static knowledge pack can
   provide.
2. `category` is a plain string here, not an IdKey. Action categories come from component
   metadata, not the `Category` entity.
3. An action component has no identity of its own. Its `EntityType` is the identity, which is
   why the output field is named `EntityTypeIdKey` rather than `IdKey`.

### 2. GetWorkflowActionTypeAvailableAttributes

`[AgentToolGuid( "99871B11-0F69-4E0D-BCBA-446317F8B5B6" )]`

The name is precise on both halves. The entity really is `WorkflowActionType`: an action's
settings are attributes on it, qualified by the action component's entity type. And this
really is the `Get{Entity}AvailableAttributes` pattern, because it exists to support a write,
namely the `settings` parameter on tool 9.

```csharp
public AgentToolResult GetWorkflowActionTypeAvailableAttributes( string actionEntityTypeIdKey )
```

| Input | Required | Notes |
|---|---|---|
| `actionEntityTypeIdKey` | **Yes** | The action component. Comes from tool 1's `EntityTypeIdKey`, or from `ActionEntityTypeIdKey` on any action returned by tools 4 and 5 |

**One parameter, not two.** An earlier form also accepted an existing `actionTypeIdKey`,
mirroring `GetConnectionRequestAvailableAttributes`, which takes either the entity or the
thing that determines the attribute set. That mirroring was wrong here.

For a connection request the attribute set depends on the opportunity, which is stored on the
request, so passing the request is the only way to resolve it. An action's settings depend on
**nothing but its component's entity type.** There is no per-action variation to resolve, so
the second parameter added a branch and bought nothing.

**This requires `ActionEntityTypeIdKey` on the action shape in tools 4 and 5**, which it did
not carry. Without it, an agent that read a workflow held an action's IdKey and its
`ActionClassName`, and class strings are output-only, so its only route to the settings was to
re-resolve the class through tool 1. Adding one field to the read shape removes both the
round trip and the second parameter.

**No class name parameter.** Class strings are output only across both skills; see the Core
Administration spec, "Class name strings are output, never input".

**Output.** The core `AttributeResult`, unwrapped and unextended. **No result class of its
own.** The tool body is the same one line as the other four in the family:

```csharp
return Success( helper.GetAvailableAttributes( stubActionType )
    .Where( a => a.Key != "Active" && a.Key != "Order" )
    .ToList() );
```

#### Why this returns the bare shape, and what has to change in core first

All four existing `Get{Entity}AvailableAttributes` tools end in
`return Success( helper.GetAvailableAttributes( entity ) )`. Not one wraps or extends the
result. Matching them is not cosmetic:

**A custom shape means re-implementing the security filter.** `helper.GetAvailableAttributes`
returns `ICollection<AttributeResult>` of base instances, which cannot be upcast, so any
derived shape forces this tool to build its own results. That means duplicating the
`isInternal || IsPublic` visibility filter and the `IsAuthorized( VIEW, currentPerson )`
check. Two copies of a security filter drift, and when this one drifts it leaks attribute
definitions the person is not allowed to see. That risk is not worth four convenience fields.

**But the bare shape is not sufficient today**, and the gap is in core rather than here.
`AttributeResult` carries only `Name`, `Key`, `ValueFormat`, `IsRequired`, and `IsReadOnly`.
An agent choosing a setting value cannot see the setting's description, its field type, or its
allowed values.

Every missing field is **already in the helper's own loop**, on the `AttributeCache` it is
iterating, and is discarded:

| Field | Where it already is |
|---|---|
| `Description` | `AttributeCache.Description` |
| `Order` | `AttributeCache.Order` |
| `FieldType { IdKey, Name }` | `AttributeCache.FieldType` |
| `Values`, `IsCompleteList` | `FieldTypeHints`, computed then dropped |

That last one is the sharpest. `AgentToolHelper.GetAvailableAttributes` calls
`fieldType.GetFieldHints( a.ConfigurationValues )` and reads `hints.ValueFormat` while
throwing away `hints.Values` and `hints.IsCompleteList` on the same object.

**Prerequisite: `AttributeResult` gains those fields.** Specified alongside the `Guid`
addition in `Rock/specs/260807-ai-agent-result-guids.md`, because it is the same class, the
same helper method, and the same commit. It improves all five tools rather than this one.

**Second prerequisite: `GetFieldHints()` on the field types with enumerable values.** It is
`virtual` returning `null` on the base and overridden by **4 of 173** field types, none of
them Single Select. So `Values` stays empty for exactly the settings that need it most.

Implementing it across all 173 is the Rock core project already logged against
`IFieldType.ConfigurationKeys()`. Implementing it on the roughly eight with genuinely
enumerable values, single select, multi select, boolean, defined value and their kin, is
small, bounded, and covers nearly all workflow authoring. Same shape of decision as the
hand-authored `GetFieldType` supplements in the Core Administration spec.

**Until both land, this tool cannot tell an agent a Single Select's options.** That is a real
v1 limitation and it should be stated rather than papered over by returning the raw
`ConfigurationValues` dictionary, which was the earlier proposal here. Raw qualifiers are
implementation detail, they are unbounded in size, and shipping them would make the temporary
workaround permanent.

**Volume.** Typically 3 to 15 settings per action. `SendEmail` is the largest at around 12.

**Paging.** None.

**History.** Full content. Expect one call per action type used, so this is the
highest-traffic tool in either skill.

**Requirement: must suppress the `Active` and `Order` attributes.**

Every action carries vestigial `Active` and `Order` attributes because `ActionComponent`
overrides both. A model that sees a setting named `Order` will reasonably assume it controls
the action's position in the activity. It does not. Position is `WorkflowActionType.Order`, a
property, set through tool 9. Writing the attribute produces a junk `AttributeValue`, leaves
ordering untouched, and raises no error.

**Why this is not just a call to `GetEntityAvailableAttributes`.** Action settings are
attributes on `Rock.Model.WorkflowActionType` qualified by `EntityTypeId`.

**The Core Administration tool cannot return them at all.** It deliberately has no qualifier
support and returns only attributes that apply to every entity of a type. Action settings are
qualified by definition, so this is the dedicated tool the conventions doc calls for.

Two further reasons it would stay even if qualifiers were supported:

1. The generic tool must not suppress `Active` and `Order`, since they are legitimate
   attributes in the general case. A workflow-specific wrapper can.
2. "How do I configure this action" should find a tool named for that, rather than requiring
   the model to know action settings are modeled as entity attributes.

### 3. ListWorkflowTypes

`[AgentToolGuid( "5B0804A3-F2F4-4F5E-AB3C-E0F618370BE1" )]`

```csharp
public AgentToolResult ListWorkflowTypes( string partialName = null, string categoryIdKey = null, bool includeInactive = false, int pageNumber = 1 )
```

**Output.** `IdKey, Guid, Name, Description, Category { IdKey, Guid, Name }, IsActive, IsPersisted, LoggingLevel, ActivityTypeCount`.

`LoggingLevel` earns its place in a list result. Core Rules says default it to None, and
this is where someone notices a workflow that was left on a verbose level after debugging.
`IsPersisted` is here for the same reason.

**Volume.** A large church can run 300 to 500 workflow types.

**Paging.** Page number, 50 per page. Source is `WorkflowTypeCache.All()`.

**Why not a cursor.** Deliberate, not an oversight. The source is a cache and is already
materialized, so security filtering runs across the whole collection before paging and there
are no database round trips for `CursorPaginator` to save. It needs an `IQueryable` besides.
See the conventions doc, section 5.

**History.** Compact page of `KeyNameResult`.

**Security.** Filter by `IsAuthorized( VIEW, currentPerson )` **across the whole collection
before paging**, never after. `GetPaginatedItems` does no security filtering of its own and
takes no person. Filtering a page after the fact yields short pages and a wrong
`HasMoreItems`. Filtering first is safe here only because a cache collection is already
materialized; see the conventions doc, section 5.

**Notes.** Named `List`, not `Lookup`, both because it takes filters and because it avoids a
name collision with `WorkflowSkill.LookupWorkflowTypes`.

### 4. GetWorkflowType

`[AgentToolGuid( "01D84BCE-8B18-4200-9435-3D1F5572BD92" )]`

```csharp
public AgentToolResult GetWorkflowType( string workflowTypeIdKey )
```

**Output.** The whole tree, as a readable projection rather than a container:

```
IdKey, Guid, Name, Description, Category { IdKey, Guid, Name },
IsActive, IsPersisted, LoggingLevel, WorkTerm, IconCssClass, SummaryViewText, NoActionMessage,
WorkflowIdPrefix, Slug, ProcessingIntervalSeconds,
LogRetentionPeriod, CompletedWorkflowRetentionPeriod, MaxWorkflowAgeDays,
Attributes[] {
  IdKey, Guid, Scope, Key, Name, Description, FieldType { IdKey, Name, Class }, IsRequired, Order, DefaultValue,
  ConfigurationValues { key: value }
},
ActivityTypes[] {
  IdKey, Guid, Name, Description, Order, IsActive, IsActivatedWithWorkflow,
  Attributes[] { ...the same attribute shape, Scope = Activity },
  ActionTypes[] {
    IdKey, Guid, Name, Order, ActionEntityTypeIdKey, ActionClassName, ActionName,
    IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, IsActionCompletedIfCriteriaUnmet,
    Criteria { AttributeKey, AttributeName, ComparisonType, Value },
    Settings { settingKey: value },
    Form {
      IdKey, Guid, Header, Footer, AllowNotes, AllowPersonEntry,
      NotificationSystemCommunication { IdKey, Guid, Title },
      Buttons[] { Name, ButtonStyleGuid, ButtonStyleName, ActivateActivityIdKey, ActivateActivityName, ResponseText },
      Fields[] { AttributeIdKey, AttributeKey, AttributeName, Order, IsVisible, IsRequired, IsReadOnly, HideLabel, PreHtml, PostHtml }
    }
  }
}
```

**Volume.** Measured by rendering the demo containers into this exact shape, not estimated.
The container column is Rock's `ExportedEntitiesContainer`, included only as the baseline the
demos are stored in.

| Demo | Entities | Container | This projection, minified |
|---|---|---|---|
| staff-onboarding | 32 | 21.4 KB | **5.5 KB** |
| branching-styled | 48 | 32.6 KB | **10.0 KB** |

Roughly 2,550 tokens for the larger one. Where those 10 KB go:

| Part | Bytes |
|---|---|
| `attributes[]`, 7 workflow attributes | 2,195 |
| `activityTypes[]`, 5 activities holding 9 actions | 7,332 |
| ↳ form fields | 1,632 |
| ↳ buttons | 609 |
| ↳ panel HTML in form headers and footers | 722 |
| ↳ action settings | 524 |

**Structure is the cost, not content.** Form fields alone outweigh every action setting in
the workflow. Nothing in a normally-authored workflow came close to the 500 character clip.

These figures were measured before sections were dropped, so they are now slightly
pessimistic. The shape of the conclusion does not change: per-field metadata is the largest
line item either way.

Long values dominate the worst case, not the normal one. The clip earns its place as the
ceiling on a `SendEmail` with a large HTML body, but per-field metadata is what actually
scales with workflow size.

**Paging and splitting.** Neither. Return the full tree every call.

Splitting by activity was considered and rejected. An activity's own properties are about
100 characters; all the bulk sits on the actions underneath it. A per-activity tool would
**paginate** the tree, handing back the same total in smaller batches at a cost of one call
per activity, while removing nothing expensive. Partial views also break editing: "add a step
after the second form" needs the activity order and the action order inside it, and every
write and delete tool needs IdKeys that only this tool provides.

A `detail` parameter for structure-only output was also considered and rejected. With values
truncated it would save about 3 KB, and in exchange an agent could look at an action, not see
its settings, and conclude one is unset when it is not. That is a correctness risk traded for
a saving that does not matter.

**Truncation.** Clip any single setting value, form header, or form footer past **500
characters**. Return the leading portion and set `IsTruncated` on that value. **Never
truncate structure.**

500 rather than a larger figure because the purpose is recognition, not reading. Enough to
tell which template is in a setting, not enough to review it. At 2,000, a thirty-action
workflow could still reach 60 KB, which is exactly the case worth avoiding.

**Truncation requires tool 5.** A clipped value with no way to recover it is data loss, which
the conventions doc forbids. `GetWorkflowActionType` is the companion that returns one
action's values whole, and the two ship together.

#### Every node carries a `guid`

No exceptions. Uniform, one rule, no table to consult.

Dropping `guid` from the nodes nothing appears to reference is a tempting optimization. **Do
not.** Four reasons, recorded because it will be proposed again.

1. **You cannot survey what references what.** Checking a sample workflow shows which GUIDs
   its actions happen to use. There are 145 core action components plus whatever plugins
   install, and **a setting value can hold any GUID at all.**
2. **Known references already span several node types.** `CriteriaAttributeGuid` on an action
   type, `ActionAttributeGuid` on a form, the activity GUID embedded in the buttons string,
   and attribute GUIDs inside arbitrary settings. The surface is wider than one demo shows.
3. **The failure is silent.** An agent that cannot resolve a GUID does not error. It explains
   the workflow wrong, or edits the wrong thing. That is the failure class this entire
   knowledge pack exists to prevent.
4. **The saving was 690 bytes, about 7%,** on the dimension that is not the problem. Form
   fields are 1,632 bytes and settings are 524. If payload ever genuinely hurts, the lever is
   per-field metadata, not identifiers.
5. **A uniform rule is free to implement correctly.** A selective one is a table someone must
   consult, and get right, forever.

Revisit only with a measurement across many real workflows showing that identifiers are a
material share of the payload. They are not, in anything measured so far.

#### idKey goes in parameters, guid goes in stored values

Callers were observed putting an idKey where Rock needs a guid. It fails silently: the
reference does not resolve, nothing errors, and the damage appears later as a workflow that
quietly does nothing.

The rule is two lines, and it holds without exception:

1. **Every parameter takes an idKey.** They are all suffixed `IdKey`, and the tool converts
   to whatever Rock stores. `criteriaAttributeIdKey` becomes `CriteriaAttributeGuid`, a form
   button's target activity becomes a GUID in the `Actions` string, a visibility rule's
   attribute becomes `ComparedToFormFieldGuid`. The caller never supplies a guid here.
2. **Four values are written through unchanged, and hold a guid** when the field type behind
   them references another record:

   | Slot | Tool |
   |---|---|
   | `settings[].value` | `AddOrUpdateWorkflowActionType` |
   | `criteriaValue` | `AddOrUpdateWorkflowActionType` |
   | `defaultValue` | `AddOrUpdateWorkflowAttribute` |
   | `visibilityRules[].comparedToValue` | `AddOrUpdateWorkflowActionForm` |

The skill description carries the short form of this, and each of the four slots repeats it
in its own parameter description. **The list is duplicated in five places on purpose**, since
an agent reads whichever one is in front of it. Each slot carries a code comment pointing at
the skill description, so a fifth slot does not silently escape the set.

`settings` takes `List<AttributeValueResult>`, which carries a `TextValue` the write path
ignores. Its description says so, because two value-shaped fields on one object is an obvious
place to put the literal in the wrong one. Removing `TextValue` with a dedicated input class
would be better than documenting it, and is worth doing if the confusion continues.

**Prose is a suggestion, not a constraint.** The durable fix is to validate: when a setting's
field type stores a reference and the value does not parse as a guid, reject it and name the
tool that returns one. An idKey and a guid are trivially distinguishable, so there is no
ambiguity to resolve. Not implemented yet.

#### Values that are not trimmed, deliberately

**False booleans and nulls stay.** Omitting `isVisible`, `isRequired`, `isReadOnly`, and
`hideLabel` when false would cut the fields block by roughly half, and it was rejected. An
absent key is ambiguous: the reader cannot tell whether the field is false or whether the
tool declined to send it, and getting that backwards on `isRequired` changes what the form
does.

**This is the same call as the `guid` decision and the `detail` parameter.** Three separate
proposals to shrink the payload, all rejected, all for one reason: **every one traded a
silent correctness risk for a saving in the single digits.** The payload is around 10 KB for
a mid-sized workflow. It does not need defending at that price.

If size ever becomes a real constraint, the honest lever is per-field metadata, which is the
largest line item and the one that scales with workflow size. Not identifiers, not booleans,
and not by hiding structure the agent needs to edit correctly.

#### Resolve what points inside the tree

Two things are stored as bare GUIDs. One must be resolved, the other should be.

1. **Setting keys. Required.** An action's settings are `AttributeValue` rows keyed by the
   configuration attribute's GUID. Return the setting's `Key`, not
   `BBED8A83-8BB2-4D35-BAFB-05F67DCAD112`. The agent cannot resolve this itself: those
   attributes belong to the action component and never appear in the tree.
2. **Setting values that point at a node in this tree. Recommended.** When a value resolves
   to an activity type or a workflow attribute in the same workflow, return the raw value
   **and** a resolved name alongside it. The raw value is what a write sends back; the name
   is what makes the tree readable. Since every node carries its `guid`, the agent could
   cross-reference this itself, so it is a convenience rather than a necessity.

The second costs roughly 40 bytes per resolved value, three in the demo. Cheap, and it
removes the most common cross-referencing step when reading a workflow: "which activity does
this action activate."

**History.** `.WithoutHistoryContent()` with a history key. The tree is too large for chat
history.

**Notes.**

0. **Attributes appear at two levels and carry their `Scope`.** The workflow's own are on the
   root; an activity's are on that activity. They are stored against different entities with
   different qualifiers, and the same key can exist in both at once, so merging them into one
   list would read as a duplicate and lose the distinction that decides which actions can
   reach the variable.
1. Criteria resolve from an attribute GUID to that attribute's key and name. Settings
   resolution is covered above under "Resolve what points inside the tree".
2. Resolve the form `Actions` string. It is stored as
   `Name^ButtonGuid^ActivityGuid^ResponseText` joined by `|`, per
   `WorkflowActionFormProcessor`.
3. **This tool is load-bearing.** Every write and delete tool takes an IdKey. Nothing in
   "drop the reminder email step" is an IdKey. This is the only tool that turns a phrase into
   one, so every node it returns must carry its IdKey.
4. This is the surviving piece of `design/artifact-format.md`, which is otherwise
   superseded. It also answers "explain my workflow" and drives the diff on an edit.

### 5. GetWorkflowActionType

`[AgentToolGuid( "B6EB95E5-80EA-4E77-A4DE-89BB327D382F" )]`
`[AgentToolPrerequisite( "Call GetWorkflowType first to find the action's IdKey." )]`

The companion that makes tool 4's 500-character clip safe. The two ship together: a clipped
value with no way to recover it is data loss.

```csharp
public AgentToolResult GetWorkflowActionType( string actionTypeIdKey )
```

**Output.** One action in the tool 4 action shape, **with no truncation**, plus its parent
references so the caller knows where it sits:

```
IdKey, Guid, Name, Order, ActionEntityTypeIdKey, ActionClassName, ActionName,
IsActionCompletedOnSuccess, IsActivityCompletedOnSuccess, IsActionCompletedIfCriteriaUnmet,
ActivityType { IdKey, Guid, Name },
WorkflowType { IdKey, Guid, Name },
Criteria { AttributeKey, AttributeName, ComparisonType, Value },
Settings { settingKey: value },
Form { ... }
```

**Volume.** One action. Usually under 1 KB, occasionally much larger, which is the entire
point of the tool.

**Paging.** None, and **no truncation**. If a setting holds a 40 KB Lava template, this
returns 40 KB. That is the only way to edit it correctly.

**History.** `.WithoutHistoryContent()` with a history key, for the same reason as tool 4.

**Notes.**

1. **Only call this when the full text is needed**, which in practice means editing a long
   template or diagnosing something the clipped preview cannot explain. The tool description
   should say so, or it becomes a habit and undoes the saving.
2. Resolve settings from attribute GUIDs to named settings, exactly as tool 4 does. The two
   must use the same renderer, or a value will read differently depending on which tool
   returned it.
3. Returning the parent references matters. A caller that arrives here from a search rather
   than from tool 4 otherwise has no idea which activity the action belongs to.

---

### The zero-instance exception

Two changes are rejected outright because they silently corrupt stored data: changing an
attribute's field type (tool 7) and changing an action's component (tool 9). In both cases
Rock neither converts nor clears the values left behind, and nothing throws.

**Both rejections lift when the workflow type has zero instances.** "Change that to a memo
field" is a reasonable thing to say about a workflow built five minutes ago, and a flat
refusal there is friction with no safety behind it. With no instances there is nothing stored
to corrupt.

The check is one count query against `Workflow` filtered by `WorkflowTypeId`. It must be run
at the moment of the write, not inferred from anything the caller says.

When the exception applies, the write still cleans up after itself: tool 9 removes the
`AttributeValue` rows qualified by the old component's entity type, which would otherwise sit
orphaned. Tool 7 has nothing to clean, since the values it would have corrupted do not exist.

When the exception does not apply, the error names the instance count. "This workflow has 412
instances, so changing the field type would corrupt their stored values" is actionable in a
way that "not allowed" is not.

### 6. AddOrUpdateWorkflowType

`[AgentToolGuid( "DD2120CD-0FD6-45FC-8633-60FFA69B16CC" )]`

```csharp
public AgentToolResult AddOrUpdateWorkflowType(
    string workflowTypeIdKey = null,
    string categoryIdKey = null,
    string name = null,
    SetOrClear<string> description = null,
    bool? isActive = null,
    bool? isPersisted = null,
    WorkflowLoggingLevel? loggingLevel = null,
    SetOrClear<string> workTerm = null,
    SetOrClear<string> iconCssClass = null,
    SetOrClear<string> summaryViewText = null,
    SetOrClear<string> noActionMessage = null,
    SetOrClear<string> workflowIdPrefix = null,
    SetOrClear<string> slug = null,
    SetOrClear<int> processingIntervalSeconds = null,
    SetOrClear<int> logRetentionPeriod = null,
    SetOrClear<int> completedWorkflowRetentionPeriod = null,
    SetOrClear<int> maxWorkflowAgeDays = null )
```

Omit `workflowTypeIdKey` to create. On create, `name` and `categoryIdKey` are required.

**Output.** The `ListWorkflowTypes` row shape for the saved type.

**History.** `.WithHistoryContent( new KeyNameResult { ... } )` plus
`.WithInstructions( "The workflow type has been created." / "updated." )`.

**Notes.**

1. Default `isPersisted` to `false` on create. Persisting every workflow is the single most
   common performance mistake in Rock, and the default should not encourage it.
2. Default `loggingLevel` to `None` on create.
3. `iconCssClass` should be validated against the Tabler icon prefix. A guessed icon class
   shipped dead in the first demo and raised no error. Report rather than refuse; Font
   Awesome classes are still valid in older themes.
4. **The retention values are here because nothing else surfaces them.** A workflow left with
   no `logRetentionPeriod` keeps every log entry forever, which is how `WorkflowLog` becomes
   the largest table in a church's database. `completedWorkflowRetentionPeriod` and
   `maxWorkflowAgeDays` are the same argument for the instances themselves. An agent that can
   create workflows but cannot set their retention is building the problem.
5. `processingIntervalSeconds` defaults to `0` on create, meaning process immediately, which
   is what nearly every workflow wants. It matters for workflows built around a delay.
6. `slug` and `workflowIdPrefix` are stored as supplied. Neither is checked for uniqueness
   here, because Rock does not enforce it at the database level either.

### 7. AddOrUpdateWorkflowAttribute

`[AgentToolGuid( "8C371846-1F64-4A47-AB68-416CC584C85E" )]`

```csharp
public AgentToolResult AddOrUpdateWorkflowAttribute(
    string attributeIdKey = null,
    string workflowTypeIdKey = null,
    string activityTypeIdKey = null,
    string key = null,
    string name = null,
    SetOrClear<string> description = null,
    string fieldTypeIdKey = null,
    bool? isRequired = null,
    SetOrClear<string> defaultValue = null,
    Dictionary<string, string> configurationValues = null,
    string insertAfterAttributeIdKey = null,
    string insertBeforeAttributeIdKey = null )
```

Omit `attributeIdKey` to create. On create, exactly one of `workflowTypeIdKey` or
`activityTypeIdKey` is required, along with `key`, `name`, and `fieldTypeIdKey`.

**Output.** The saved attribute in the `GetWorkflowType` attribute shape, including its
`Scope`.

**Notes.**

1. **Two scopes, one tool.** A workflow attribute is stored on the `Rock.Model.Workflow`
   entity with `EntityTypeQualifierColumn = "WorkflowTypeId"`. An activity attribute is
   stored on `Rock.Model.WorkflowActivity` with `EntityTypeQualifierColumn = "ActivityTypeId"`.
   Different entity, different qualifier, and Rock's workflow type block keeps them in
   separate state (`AttributesState` versus `ActivityAttributesState`). Set both the column
   and the value, or the attribute appears on every workflow in the system.

   One tool rather than two because everything else is identical: key uniqueness, the field
   type guard, relative positioning, and the result shape. Two tools would duplicate all of
   it, and duplicated validation drifts.

2. **Supplying both parents is refused.** They mean different storage and different reach,
   so picking one for the caller would put the variable somewhere it was not asked to go.

3. **The scope of an existing attribute cannot be changed.** Moving one would re-qualify a
   row that stored values already point at. Refuse and instruct the caller to add the new
   one and remove the old.

4. Key uniqueness is checked **within the scope**, not across both. The same key can exist
   as a workflow attribute and an activity attribute at once; that is legal and the reason
   the read result reports `Scope`.

5. Ordering is also per scope, so an activity's attributes are numbered independently of the
   workflow's.

6. **Changing `fieldTypeIdKey` on an existing attribute is destructive.** Stored values are
   in the old field type's format. A Single Select holding `"S"`, `"M"`, `"L"` across 400
   instances does not become valid Person values; Rock neither converts nor clears them, and
   nothing throws. Reject the change and instruct the caller to delete and recreate, **unless
   the workflow type has zero instances**, in which case allow it. See "The zero-instance
   exception" above. For an activity attribute the instance count is still the owning
   workflow type's, reached through the activity.

7. Ordering uses relative positioning. Supply at most one of `insertAfterAttributeIdKey` or
   `insertBeforeAttributeIdKey`. When neither is supplied, append. Renumber siblings in one
   pass after the insert.

8. `configurationValues` are the field type's qualifiers. Get the valid keys from
   `GetFieldType` in the Core Administration skill.

### On the parameter order

`attributeIdKey` comes first and every identifier is optional, which is how all seven
existing `AddOrUpdate` tools are written (`AddOrUpdateNote`, `AddOrUpdateConnectionRequest`,
`AddOrUpdateContentChannelItem`, `AddOrUpdateGroupMember`, `AddOrUpdateStep`,
`AddOrUpdatePrayerRequest`, `AddOrUpdateReminder`). Required-on-create is enforced in the
body with a readable error.

An earlier form made the parent positionally required. That had two costs. Updating an
attribute forced the caller to supply a parent it had already implied by naming the
attribute, and omitting it threw inside `InvokeAsync` during argument binding, which the MCP
endpoint surfaces as a server error rather than the message the tool would otherwise have
returned. Tools 8, 9, and 10 follow the same rule for the same reason.

### 8. AddOrUpdateWorkflowActivityType

`[AgentToolGuid( "85129AA8-724E-4BAB-BBAB-7DF9253F11DE" )]`

```csharp
public AgentToolResult AddOrUpdateWorkflowActivityType(
    string activityTypeIdKey = null,
    string workflowTypeIdKey = null,
    string name = null,
    SetOrClear<string> description = null,
    bool? isActive = null,
    bool? isActivatedWithWorkflow = null,
    string insertAfterActivityTypeIdKey = null,
    string insertBeforeActivityTypeIdKey = null )
```

Omit `activityTypeIdKey` to create. On create, `workflowTypeIdKey` and `name` are required.

**Output.** The saved activity type without its actions or attributes, in the
`GetWorkflowType` shape.

**Notes.**

1. `isActivatedWithWorkflow` defaults to `false` on create. Only the first activity in a
   workflow normally sets it true, and defaulting it true causes every activity to fire at
   once.
2. Relative positioning follows tool 7 note 7.
3. On update the parent is read from the activity, so a caller holding only the activity key
   does not have to supply it. When it is supplied anyway it is checked rather than trusted,
   because a mismatch means the caller is working from a stale read of a different workflow.
   See tool 7, "On the parameter order".
4. An activity's own attributes are added through tool 7 with `activityTypeIdKey`, not here.

### 9. AddOrUpdateWorkflowActionType

`[AgentToolGuid( "8C1147F0-7A46-4274-9A6D-668ECD052B87" )]`

```csharp
public AgentToolResult AddOrUpdateWorkflowActionType(
    string actionTypeIdKey = null,
    string activityTypeIdKey = null,
    string name = null,
    string actionEntityTypeIdKey = null,
    bool? isActionCompletedOnSuccess = null,
    bool? isActivityCompletedOnSuccess = null,
    bool? isActionCompletedIfCriteriaUnmet = null,
    string criteriaAttributeIdKey = null,
    ComparisonType? criteriaComparisonType = null,
    SetOrClear<string> criteriaValue = null,
    List<AttributeValueResult> settings = null,
    string insertAfterActionTypeIdKey = null,
    string insertBeforeActionTypeIdKey = null )
```

Omit `actionTypeIdKey` to create. On create, `activityTypeIdKey`, `name`, and
`actionEntityTypeIdKey` are required.

**Output.** The saved action type in the `GetWorkflowType` action shape, minus the form.

**Notes.**

1. `settings` uses `List<AttributeValueResult>` so
   `helper.SetAttributeValues( actionType, settings )` applies directly. Action settings
   genuinely are attribute values on the `WorkflowActionType` entity, so no new mechanism is
   needed.
2. Validate every settings `Key` against tool 2 for the given action class. Reject unknown
   keys with an error rather than writing an orphan `AttributeValue`. Reject `Active` and
   `Order` explicitly with an instruction pointing at the positioning parameters.
3. `CriteriaAttributeGuid` is a **Guid** column, not an Id. Resolve `criteriaAttributeIdKey`
   to the attribute's Guid before saving, and **check that it is in scope first**: the
   workflow's own attributes plus the containing activity's, which is what Rock's criteria
   picker offers. Without the check any attribute in Rock is accepted, including one from an
   unrelated entity, and the criteria then silently never matches.
4. **Changing `actionEntityTypeIdKey` on an existing action is destructive.** Settings are
   attribute values qualified by the old component's entity type. Point `EntityTypeId` at a
   new component and the action looks for its own attributes, finds none, and runs entirely
   on defaults while the old `AttributeValue` rows sit orphaned. Nothing throws. A `SendEmail`
   turned into `SetAttributeValue` just runs empty. Reject the change and instruct the caller
   to delete and recreate, **unless the workflow type has zero instances**, in which case
   allow it and clear the orphaned `AttributeValue` rows. See "The zero-instance exception"
   above.
5. Relative positioning follows tool 7 note 7, and the parameter order follows tool 7,
   "On the parameter order".
6. `isActionCompletedOnSuccess` defaults to `true` on create. That matches what the Rock UI
   does and what nearly every action needs.
7. **The suppressed `Order` attribute has to be filled in here.** `Order` is declared on
   `Rock.Extension.Component` with an `IntegerField` that names neither a required flag nor a
   default, and `FieldAttribute` defaults `required` to true. So every action carries a
   required attribute that is blank on creation, and `SetAttributeValues` refuses to save
   *anything* until it holds a value.

   Tool 2 hides `Order` and note 2 above rejects it as input. Those two decisions are only
   coherent if this tool supplies the value itself, from the action's own `Order` property,
   which is what `Component.Order` parses back out. Without it the caller is told a key is
   required that the same tool refuses to accept, and the action's real settings are silently
   discarded along with the failed save.

   `Active` needs no help. Its `BooleanField` declares `DefaultBooleanValue`, so it is
   `"False"` rather than blank and never trips the required check.

### 10. AddOrUpdateWorkflowActionForm

`[AgentToolGuid( "AEF3A669-4696-42E0-AC97-FF109CC72FE2" )]`

```csharp
public AgentToolResult AddOrUpdateWorkflowActionForm(
    string actionTypeIdKey = null,
    SetOrClear<string> header = null,
    SetOrClear<string> footer = null,
    bool? allowNotes = null,
    SetOrClear<string> notificationSystemCommunicationIdKey = null,
    bool? includeActionsInNotification = null,
    List<WorkflowFormButtonInput> buttons = null,
    List<WorkflowFormFieldInput> fields = null )
```

`WorkflowFormButtonInput`: `Name, ButtonStyleDefinedValueIdKey, ActivateActivityTypeIdKey, ResponseText`.

`WorkflowFormFieldInput`: `AttributeIdKey, Order, IsVisible, IsRequired, IsReadOnly, HideLabel, PreHtml, PostHtml`.

**Output.** The saved form in the `GetWorkflowType` form shape.

**Notes.**

1. **The form is edited as a whole, not field by field.** When `fields` is supplied it
   replaces the existing fields entirely. This is the one place a replace-by-absence diff is
   safe, because the tool sees the complete unit and the unit is small. It removes four
   partial-edit tools from the surface.
2. When `fields` is omitted, leave the existing fields untouched. This lets a caller change
   only the header without resending the layout.
3. Creates the `WorkflowActionForm` if the action type has none. The relationship is 1 to 1
   through `WorkflowActionType.WorkflowFormId`.
4. `Actions` is a delimited string, not a table. Serialize `buttons` as
   `Name^ButtonGuid^ActivityGuid^ResponseText` joined by `|`.
5. **Fields are written with `ActionFormSectionId` left null, and any sections the form
   already had are deleted.** Sections and column widths are out of scope for this skill; see
   "No sections and no column widths" below for the reasoning. Deleting them is not
   incidental: a field inside a section cannot be styled with pre and post HTML, because the
   Obsidian renderer drops both for sectioned fields.
6. **Refuse outright when `WorkflowType.IsFormBuilder` is true**, before writing anything.
   This is what makes note 5 safe rather than destructive; see "Form Builder forms are
   refused" below. `GetWorkflowType` returns `IsFormBuilder` so an agent can see the refusal
   coming.
7. Person entry configuration is not in this tool. See Out of Scope.
8. **A field may reference the workflow's attributes or the containing activity's.** Rock's
   workflow type block puts the activity's attributes into `HttpContext.Items` before
   rendering that activity's actions (`WorkflowTypeDetail.ascx.cs:1762`), which is how its
   field picker offers both. Validating against only the workflow's rejects forms the UI
   itself produces, so a form built by hand cannot be round-tripped through this tool.
9. `actionTypeIdKey` is always required, but it is declared optional and checked in the body
   for the reason given in tool 7, "On the parameter order": a missing positional argument
   fails during binding and surfaces as a server error rather than a usable message.

#### No sections and no column widths

Both were supported in an earlier draft of this tool and were **deliberately removed**. This
is recorded here because the capability exists in the data model, so the omission looks like
an oversight unless it is written down.

**The reason is which editor these workflows are maintained in.** Sections and column widths
are a Form Builder concept. The workflow editor's field row
(`Rock/Web/UI/Controls/Workflow Controls/WorkflowFormAttributeRow.cs`) offers pre HTML, post
HTML, and visibility rules, and has no control for either one. Authoring a form with sections
produces something a person cannot then open and change in the editor this skill's workflows
are meant to live in. A workflow an agent can build but a human cannot maintain is worse than
one with a plainer layout.

**Column widths do not work outside a section anyway.** `col-md-{columnSize}` is applied only
in the sectioned branch of the Obsidian renderer
(`entryFormSection.partial.obs:112`), and the WebForms block uses a `PlaceHolder` that emits
no wrapper at all for an unsectioned field (`WorkflowEntry.ascx.cs:1211`). Offering a setting
that silently does nothing is worse than not offering it.

**Sections and pre/post HTML are mutually exclusive in Obsidian.** The sectioned branch never
reads `preHtml` or `postHtml`; only the unsectioned branch renders them, through
`ItemsWithPreAndPostHtml` (`entryFormSection.partial.obs:22-32`). So keeping sections would
have cost the one styling mechanism this tool does offer. The WebForms block renders both,
but nests them inside the column div, where a layout-breaking element cannot do its job.

**What to use instead.** A field's `preHtml` and `postHtml`. Both renderers honour them on an
unsectioned field, and both are editable in the workflow editor.

Do not re-add either without first deciding that these workflows no longer need to be
editable in the workflow editor. That is the trade, and it is not a small one.

#### Form Builder forms are refused

Dropping sections creates a hazard that has to be closed explicitly, and this was **missed in
the first pass**, so it is recorded rather than left to be rediscovered.

Form Builder reads a form **only** through its sections. `GetFormSectionViewModels` loops the
form's sections and, for each, takes the fields whose `ActionFormSectionId` points at it
(`FormBuilderDetail.cs:584`). There is no branch for a field without a section.

So rewriting a Form Builder form through this tool would delete the sections, write the
fields unsectioned, and leave Form Builder showing **an empty form**. The fields would still
exist and would still render correctly at runtime, in both the WebForms and Obsidian entry
blocks, which is what makes it worse: nothing errors, and the damage is only visible to
whoever next opens the form to edit it.

**The rule: refuse when `WorkflowType.IsFormBuilder` is true, before writing anything.** Not
a warning. The tool cannot produce a form that Form Builder can display, so there is no
correct way for it to proceed.

**The refusal is scoped to this tool alone.** A Form Builder workflow stays fully editable
everywhere else: its properties, attributes, activities, and actions all go through the other
write tools untouched, and new activities and actions can be added to it. Only the form
itself is off limits, because the form is the only part with a layout this skill cannot
reproduce. The error says so, since an agent that reads "not editable" without the scope will
stop working on the whole workflow.

Sections are still deleted when they appear on a workflow type that is **not** a Form Builder
one, which is the case an earlier build of this skill could produce. Nothing can display
those, so flattening them loses nothing.

Reads are unaffected. `GetWorkflowType` flattens a Form Builder form's fields into section
then field order rather than dropping them, because a field missing from a read reads as a
field that is not on the form. It also returns `IsFormBuilder`, so an agent can tell that the
form it is looking at is one it must not rewrite.

### 15. AddOrUpdateWorkflowFormPersonEntry

`[AgentToolGuid( "3E7B1A4C-5D26-4F98-9C03-8B41D5E6720F" )]`

Turns on a form's person entry block, or updates its settings. Person entry is what makes a
form collect a **real person** rather than loose text: Rock matches the entered details
against existing records, creates one when there is no match, and writes the result into a
workflow attribute the rest of the workflow can act on.

**Why a separate tool.** `WorkflowActionForm` carries 29 `PersonEntry*` columns. Folding
them into tool 10 would more than triple its signature and bury the common case, which is a
header, some buttons, and a few fields. Person entry is also configured as one unit and is
off on most forms. This is the same argument that keeps tool 10 separate from tool 9.

**Parameters**, grouped as the block itself is:

| Group | Parameters |
|---|---|
| Target and switch | `actionTypeIdKey`, `allowPersonEntry` |
| Where results land | `personAttributeIdKey`, `spouseAttributeIdKey`, `familyAttributeIdKey` |
| Which fields are asked | `addressOption`, `birthdateOption`, `emailOption`, `ethnicityOption`, `genderOption`, `maritalStatusOption`, `mobilePhoneOption`, `raceOption`, `spouseOption`, `smsOptInOption`, `spouseLabel` |
| Behavior | `isAutofillCurrentPersonEnabled`, `isCampusVisible`, `isHiddenIfCurrentPersonKnown` |
| Presentation | `title`, `description`, `isHeadingSeparatorShown`, `preHtml`, `postHtml` |
| Values applied to a created person | `connectionStatusDefinedValueIdKey`, `recordStatusDefinedValueIdKey`, `recordSourceDefinedValueIdKey`, `addressTypeDefinedValueIdKey`, `campusStatusDefinedValueIdKey`, `campusTypeDefinedValueIdKey`, `sectionTypeDefinedValueIdKey` |

**Output.** The saved form in the `GetWorkflowType` form shape, including the new
`PersonEntry` block.

**Notes.**

1. **Everything merges. Nothing is replace-by-absence.** Unlike tool 10's `fields` and
   `buttons`, each setting is its own parameter, so a caller changes one without resending
   the rest. That also means a caller never has to read the block before editing it, which
   is why the read shape below is for explaining a workflow rather than for safe editing.
2. **Nine options take `WorkflowActionFormPersonEntryOption`** (Hidden, Optional, Required).
   `smsOptInOption` takes `WorkflowActionFormShowHideOption` (Hide, Show) instead, because
   an opt-in cannot be required. Keep the two enums distinct rather than flattening them;
   the model does.
3. **The three attribute bindings are raw `Guid?` columns, not foreign keys**, so
   `UpdateNavigationProperty` cannot be used and the guid is resolved by hand. They are
   still validated against the same scope a form field is, the workflow's attributes plus
   the containing activity's, so a binding cannot point at an attribute the form could never
   reach.
4. **`personAttributeIdKey` is required whenever person entry ends up on.** Without it the
   block matches or creates a person and nothing in the workflow can reference the result.
   The check runs against the value the form will hold rather than against the parameter, so
   enabling in one call and binding in an earlier one is accepted.
5. **The seven defined values go through `UpdateDefinedValueProperty`**, which validates
   against the defined type named by each property's own `[DefinedValue]` attribute. A value
   from the wrong type is refused by name for free.
6. **Record status defaults to Active.** It has no default on the model, so a block
   configured without it creates people carrying no record status at all. That fails
   silently: the person is created, appears in searches, and only misbehaves later wherever
   record status is filtered on. The default is applied only when person entry is on and the
   form holds no value, so it never overwrites a deliberate choice and never re-adds a value
   cleared in the same call.
7. **Refused on a Form Builder workflow**, same as tool 10. A Form Builder template with
   person entry enabled overrides the form's own settings entirely
   (`WorkflowActionFormCache.cs:337`), so writing them would save cleanly and change nothing
   at run time.
8. Creates the `WorkflowActionForm` if the action has none, matching tool 10.

#### The read shape

`WorkflowActionFormResult` gains a `PersonEntry` object carrying all 29 settings, with the
defined values and attribute bindings resolved to `KeyNameResult` references rather than
returned as raw identifiers.

**Populated only when `AllowPersonEntry` is true.** The serializer omits nulls, so a form
without person entry costs nothing, which is what makes it affordable to include in the
tree read as well as the single-action read. That matters because the alternative, returning
`allowPersonEntry: true` and nothing else, is omitting structure rather than clipping a
value, and an agent could not explain what such a form collects.

`preHtml` and `postHtml` clip at 500 characters in `GetWorkflowType` with a truncation flag,
and come back whole from `GetWorkflowActionType`. That is exactly how the form's header and
footer already behave.

**Form builder settings stay out of scope.** The Out of Scope row said to take them with
person entry or not at all; the decision is not at all, because this skill does not support
Form Builder workflows in the first place.

---

All four destroy instance history silently. Rock's own UI does the same, but the UI puts a
person in front of a confirm dialog first. These tools have to carry that weight themselves,
which is why each reports counts before acting.

### 11. DeleteWorkflowAttribute

`[AgentToolGuid( "A8A8FA55-A8D5-4791-991C-691E5D8279C3" )]`
`[AgentGuardrail( "This permanently deletes the workflow attribute and every stored value for it across all existing workflow instances. Confirm the attribute with the person before proceeding." )]`

```csharp
public AgentToolResult DeleteWorkflowAttribute( string attributeIdKey )
```

**Output.** Text confirmation naming the attribute and the number of `AttributeValue` rows
removed.

**Notes.**

1. `AttributeValue` cascades on `Attribute` delete (`AttributeValue.cs:286`). Every stored
   value goes with it, silently.
2. Count the affected `AttributeValue` rows and any form fields referencing the attribute
   **before** deleting, and report both in the confirmation.
3. Remove any `WorkflowActionFormAttribute` rows pointing at it first.
4. **Accepts either scope**, workflow or activity, and reports which one it removed. Both are
   resolved the same way, by reading the qualifier back to its owner, which is also what
   proves the attribute belongs to a workflow at all. Without that check the tool would
   delete any attribute in Rock.

### 12. DeleteWorkflowActivityType

`[AgentToolGuid( "02164475-C6BB-4F8A-822C-BC37FEA77F03" )]`
`[AgentGuardrail( "This permanently deletes the activity, every action inside it, and the execution history of that activity across all existing workflow instances. Deactivating the activity is the non-destructive alternative. Confirm with the person before proceeding." )]`

```csharp
public AgentToolResult DeleteWorkflowActivityType( string activityTypeIdKey )
```

**Output.** Text confirmation naming the activity, its action count, and the number of
`WorkflowActivity` instance rows removed.

**Notes.**

1. Offer deactivation first. `WorkflowActivityType.IsActive` exists, so a caller who only
   wants the activity to stop running has a reversible option. Action types have no such
   flag.
2. Delete order: each child action type through the tool 13 path, then instance
   `WorkflowActivity` rows in batches of 100, then the activity type.
3. **The activity's own attributes must be removed by hand**, and their stored values with
   them. Like workflow attributes they have no foreign key to their owner, only a qualifier,
   so no cascade reaches them and nothing else will ever clean them up. Count them and report
   the number alongside the action and instance counts.

### 13. DeleteWorkflowActionType

`[AgentToolGuid( "31D70F62-B03A-47F7-8086-1C00637847CB" )]`
`[AgentGuardrail( "This permanently deletes the action and its execution history across all existing workflow instances. Confirm with the person before proceeding." )]`

```csharp
public AgentToolResult DeleteWorkflowActionType( string actionTypeIdKey )
```

**Output.** Text confirmation naming the action and the number of `WorkflowAction` instance
rows removed.

**Notes.**

1. Delete order, copied from `RockWeb/Blocks/WorkFlow/WorkflowTypeDetail.ascx.cs:767-805`:
   the `WorkflowActionForm` and its sections and fields, then instance `WorkflowAction` rows
   in batches of 100, then the action type.
2. `WorkflowAction.ActionType` does **not** cascade (`WorkflowAction.cs:126-127`), which is
   why the instance rows must be removed by hand. Skipping that step throws a foreign key
   error rather than silently succeeding.
3. Count the instance rows before deleting and report the count. This is the only signal a
   person gets that history is about to disappear.

### 14. DeleteWorkflowType

`[AgentToolGuid( "6C0A6C0E-9E24-4C1B-B60E-2B2A2A5FA7F1" )]`
`[AgentGuardrail( "This permanently deletes the workflow type, every activity and action in it, every workflow attribute and its stored values, and every workflow instance and its entire execution history. It cannot be undone. Deactivating the workflow type is the non-destructive alternative. Confirm with the person before proceeding." )]`

```csharp
public AgentToolResult DeleteWorkflowType( string workflowTypeIdKey )
```

**Output.** Text confirmation naming the workflow type and reporting, separately: activity
count, action count, workflow attribute count, `AttributeValue` row count, and `Workflow`
instance count.

This is the largest blast radius in either skill. It is included because an agent that can
create a workflow type and cannot remove one leaves the person to clean up its mistakes by
hand, which is worse.

**Notes.**

1. **Confirmation is required and enforced by the tool, not left to the agent.**

   The signature carries `bool isConfirmed = false`. Called without it, the tool **deletes
   nothing.** It returns the counts, names the workflow type, offers deactivation, and
   instructs the caller to call again with `isConfirmed: true` once a person has agreed.

   ```csharp
   public AgentToolResult DeleteWorkflowType( string workflowTypeIdKey, bool isConfirmed = false )
   ```

   `[AgentGuardrail]` is advisory. It shapes the model's behavior and does not constrain it,
   which is fine for tools 11 through 13 where the blast radius is one attribute, one activity,
   or one action. It is not fine here, where a single call destroys a workflow type, every
   instance, and all history with no undo. The parameter makes the first call harmless, so the
   agent physically cannot delete a workflow type in one turn.

   The two calls must also be a genuine round trip. The tool description states that the
   confirming call requires human agreement obtained **between** the two calls, not the agent's
   own judgment that deletion is warranted.

   **This is the only tool in either skill with a confirmation parameter**, and that is
   deliberate. If every destructive tool had one, the pattern would become noise the model
   learns to satisfy automatically. Reserve it for the one action that cannot be undone.

2. **Offer deactivation first**, and mean it. `WorkflowType.IsActive` exists. A caller who
   wants the workflow to stop running has a fully reversible option, and for most requests
   phrased as "get rid of this" that is what they actually want. The unconfirmed call should
   say so alongside the counts.
3. **Most of the cascade is automatic, and that is the danger.** The following delete
   themselves through the database with no code:

   | Relationship | Cascade |
   |---|---|
   | `WorkflowActivityType` → `WorkflowType` | true |
   | `WorkflowActionType` → `WorkflowActivityType` | true |
   | `Workflow` → `WorkflowType` | true |
   | `WorkflowActivity` → `Workflow` | true |
   | `WorkflowAction` → `WorkflowActivity` | true |

   A single `Delete( workflowType )` therefore destroys every instance and all history with no
   further code, which is exactly what Rock's own UI does at
   `RockWeb/Blocks/WorkFlow/WorkflowTypeDetail.ascx.cs:322`. Count everything **before** the
   delete, because after `SaveChanges` there is nothing left to count.
4. **Two things do not cascade and must be handled by hand.**

   **`WorkflowActionForm`.** `WorkflowActionType.WorkflowForm` is `HasOptional` with
   `WillCascadeOnDelete( false )` (`WorkflowActionType.cs:195`). Deleting the type through
   the cascade leaves every form, and its sections and fields, orphaned in the database. Walk
   the action types and delete their forms first, following the tool 13 path.

   **Workflow and activity attributes.** `Attribute` has no foreign key to `WorkflowType` or
   to `WorkflowActivityType` at all. The workflow's are found by
   `EntityTypeQualifierColumn = "WorkflowTypeId"` on the `Rock.Model.Workflow` entity type,
   and each activity's by `"ActivityTypeId"` on `Rock.Model.WorkflowActivity`, so nothing
   removes either. Delete both, and their `AttributeValue` rows, through the tool 11 path.
   Sweep every activity, not just the workflow root, or the activities take their attributes
   into orphanhood as they cascade away.
5. **Delete order.** Forms and their sections and fields, then workflow attributes and their
   values, then the workflow type itself and let the database cascade take the rest.
6. **Authorization.** Check `IsAuthorized( Authorization.ADMINISTRATE, currentPerson )`, not
   `EDIT`. That is what the UI block requires, and this is a strictly larger action.
7. Deleting a workflow type used by a `Launch Workflow` action in a *different* workflow type
   leaves that action pointing at nothing. There is no foreign key to catch it. Report any
   such references in the confirmation if they can be found cheaply; do not block on it.

---

### Result classes

Under `Agent/Rock.AI.Agent/Classes/Skills/WorkflowBuilderSkill/`:

`WorkflowActionComponentResult`, `WorkflowTypeSummaryResult`,
`WorkflowTypeDetailResult`, `WorkflowAttributeResult`, `WorkflowActivityTypeResult`,
`WorkflowActionTypeResult`, `WorkflowActionFormResult`, `WorkflowFormButtonResult`,
`WorkflowFormFieldResult`, plus the two input classes `WorkflowFormButtonInput` and
`WorkflowFormFieldInput`.

Two supporting types sit alongside them. `WorkflowFieldTypeResult` derives from
`KeyNameResult` to carry the field type's `Class`, because a guessed field type class broke
an import in an early build and raised no error. `WorkflowAttributeScope` is the enum that
distinguishes a workflow attribute from an activity attribute; see tool 7.

All result classes derive from `EntityResultBase` where they represent an entity, so `IdKey`
and `Guid` come free. Populate `Guid` explicitly.

**Tool 2 has no result class.** It returns the core `AttributeResult` straight from
`helper.GetAvailableAttributes`, matching the other four tools in that family. See tool 2 for
why a derived shape was rejected.

`WorkflowActionComponentResult` does **not** derive from `EntityResultBase`. An action
component has no identity of its own, so the result carries `EntityTypeIdKey` rather than an
`IdKey` of its own, and no `Guid` at all.

Tools 4 and 5 share `WorkflowActionTypeResult`. Tool 5 adds the `ActivityType` and
`WorkflowType` parent references and skips the 500-character clip. **They must use one
renderer**, or the same setting will read differently depending on which tool returned it,
which is the kind of inconsistency that costs an afternoon to notice.

---

### Decisions without precedent

Everything in this spec that has **no precedent** in the existing skills, listed so it gets
reviewed deliberately rather than discovered during implementation.

Verified as precedented and therefore **not** listed below: `pageNumber` paging
(`ListAttendance`, `ListBenevolenceRequests`), enum parameters
(`AddOrUpdateConnectionRequest` takes `ConnectionState?`), `List<AttributeValueResult>` as a
parameter, refusing a call with `helper.AddError` (`ListConnectionRequests`), and
`[AgentGuardrail]` on a destructive tool (`DeleteNote`).

#### 1. Nested input objects. **Verified supported.**

Tool 10 takes `List<WorkflowFormFieldInput>`, and each field carries its own
`List<WorkflowFormVisibilityRuleInput>`. The only object parameter anywhere in the existing
skills is a flat `List<AttributeValueResult>`, so this needed checking before the tool was
designed around it. Nesting was verified to at least three levels, which is more than the
final design uses.

**It works.** C# tools are registered with `KernelFunctionFactory.CreateFromMethod`, which
derives the JSON schema from the CLR parameter types by reflection rather than from
`ParameterSchema`. The generated schema describes both the outer array and the inner
collection's object properties. The MCP server reuses the same `function.JsonSchema`, so both
consumers are covered.

`ParameterSchemaDataType`, which offers only scalars plus an `IsCollection` flag, applies to
Lava-defined tools and is not the path a C# tool takes.

The flattening fallback that was held in reserve is not needed.

**This departs from a published rule, deliberately.** The Rock developer docs say, under
[Tool Parameters](https://community.rockrms.com/developer/ai-agents/writing-custom-tools/native-tools/tool-parameters):
"Flatten parameters: Do not create POCO objects. Use top-level method arguments instead."
Tool 10 does not, and that is recorded here so it is not mistaken for an oversight.

The rule cannot be satisfied by this tool. A form has an unbounded number of fields, each
with eight properties and its own list of visibility rules. No set of scalar arguments
expresses that. The alternative is several tools that each write part of a form, which was
rejected because it leaves a half-built form visible to anyone the workflow reaches in
between.

Two things keep the departure narrow. The nesting was verified against the real
registration path rather than assumed, and `AddOrUpdateWorkflowActionType` takes
`List<AttributeValueResult>`, a shape already used by the existing skills, rather than a
new POCO of its own. Revisit if the docs grow a stated pattern for collection parameters.

#### 2. A generic `Get{Entity}AvailableAttributes`

Every existing one is entity-specific. Core Administration tool 11 takes an
`entityTypeIdKey` instead, which is what lets it serve Category, Schedule, Binary File Type,
and anything else that never justifies its own tool.


#### 3. A deeply nested read result

`GetWorkflowType` returns five levels: workflow, activities, actions, forms, and the fields
on those forms. The deepest existing result is `ConnectionRequestResult` with a flat list of
activities. The GetWorkflowType entry argues why the depth is right; it is still new.

#### 4. Truncating a value inside a result

The `IsTruncated` flag and its 500-character clip have no precedent. Existing tools return
values whole or not at all. The conventions doc now permits this only when a companion tool
can return the value untruncated, which is why tool 5 exists.

#### 5. Relative positioning on writes

`insertAfterIdKey` and `insertBeforeIdKey` have no precedent. No existing write tool manages
ordering at all, because no existing tool writes an ordered collection.

#### 6. Writes that reject a specific field change

Tools 7 and 9 refuse to change `fieldTypeIdKey` and `actionEntityTypeIdKey` on an existing record.
Existing write tools reject bad references but never a legitimate value for a field they own.
The zero-instance exception softens it, and is recorded with the two writes it governs.

#### 7. Replace-by-absence on form fields

Supplying `fields` to tool 10 replaces every field. Nothing else in either skill infers a
deletion from something being missing, and the conventions doc forbids it in general. Tool 10
note 1 argues the unit is small enough and wholly visible enough for this to be safe. **It is
still the single most dangerous thing in this spec**, and if any part of it proves wrong the
fallback is per-field tools.

#### 8. Delete tools that report before acting

`DeleteNote` simply deletes. Tools 11 through 13 count affected instance rows and referencing
form fields first and report them. That is a deliberate departure, since these deletions
destroy history silently and Rock's own UI puts a confirmation in front of them.

---

## Out of Scope

| Item | Reason |
|---|---|
| `ExportWorkflowType` | Removed from scope at the user's direction. |
| `ImportWorkflowType` | Same. Direct writes replace the container path for v1. |
| ~~Person entry form configuration~~ | **Now in scope.** Implemented as tool 15, `AddOrUpdateWorkflowFormPersonEntry`, exactly as this row anticipated. |
| Workflow type security and auth rules | Second wave. |
| **Form sections and per-field column widths** | Deliberately not supported, not merely deferred. Both are Form Builder concepts that the workflow editor cannot display or change, and these workflows are meant to stay editable there. Column widths also do nothing outside a section, and a section costs the pre/post HTML this tool styles with. Full reasoning in tool 10, "No sections and no column widths". |
| Conditional field visibility | Specced separately in [260811-ai-agent-workflow-conditional-forms.md](260811-ai-agent-workflow-conditional-forms.md). Field rules are supported; section rules are not, since there are no sections. |
| `WorkflowTrigger` | A workflow can be launched by an action or by hand, but not wired to fire on an entity change. Deliberately deferred. |
| Form builder settings (`IsFormBuilder`, `FormBuilderTemplateId`, `FormStartDateTime`, `FormEndDateTime`, `WorkflowExpireDateTime`, `IsLoginRequired`) | This row said to take them with person entry or not at all. Person entry landed as tool 15 and these did not: the skill refuses Form Builder workflows outright, so settings that only apply to one would be unreachable. Decided, not deferred. |
| `LookupSecurityRoles`, `LookupSchedules`, `LookupBinaryFileTypes` | Second wave. None of the three demos needed them. |
| Snapshot and versioning tools | Blocked on the snapshot model in `OUTLINE.md` Section 8 item 11. |

---

## Do Not Duplicate

The existing skills already cover the domain reference data that workflow actions point at.
Use them rather than adding a generic record-search tool, which would be a broader read
surface than this skill needs.

| Existing tool | Actions that need it |
|---|---|
| `NoteSkill.LookupNoteTypes` | `PersonNoteAdd`, `AddNoteToGroupMember` |
| `GroupSkill.ListGroups` | `AddPersonToGroup`, `RemovePersonFromGroup`, `PostAttendanceToGroup` |
| `GroupSkill.LookupGroupTypes` | `AddGroup` |
| `ConnectionSkill.LookupConnectionTypesAndOpportunities` | `CreateConnectionRequest` |
| `CommunicationSkill.LookupSystemPhoneNumbers` | `SendSms` |
| `FinanceSkill.LookupFinancialAccounts` | `ProcessPayment` |
| `FinanceSkill.LookupBenevolenceTypes` | `BenevolenceRequestAdd` |
| `ContentChannelSkill.LookupContentChannelTypes` | `AddContentChannelItem` |
| `StepSkill.LookupStepPrograms` | `AddStep` |
| `ReminderSkill.LookupReminderTypes` | `AddReminder` |
| `SystemUtilitySkill.LookupCampuses` | `SetWorkflowCampus` |
| `SiteSkill.LookupSites` | `CreateShortLink` |
| `PrayerSkill.LookupPrayerCategories` | prayer actions |
| `PersonSkill.SearchPerson` | any person-valued setting |
| `AttendanceSkill.LookupAreas`, `LookupLocations`, `LookupCheckInConfigurations` | check-in actions |

---

## Decided

Nothing in this spec is open except novel decision 1, the two-level nested input objects on
tool 10, which is unverified against the framework and must be settled before that tool is
built.

The zero-instance exception is decided and is recorded next to the two writes it governs.
The remaining decision is below.

### Partial writes are the agent's problem

There is no transaction across tool calls. Each write opens its own context and saves
independently. If the agent creates a workflow type, adds an activity and three actions, and
the fourth call fails, Rock holds a workflow type that is half built and, because `IsActive`
defaults true, fully launchable.

**The skill does not solve this. The agent recovers.** On a failed write it reads the current
state back with `GetWorkflowType` and continues from what is actually there, rather than from
what it believed it had written.

This is the right split. The alternatives all fail on editing rather than creation. Creating
inactive and activating last protects a new workflow, but you cannot deactivate a live
workflow for the length of a conversation without stopping real work, so a failed edit still
leaves an inconsistent workflow. A draft flag means a second copy of the whole tree and a
merge. Neither is worth building for a case the agent can read its way out of.

**What this requires of the tools**, and it is already true of all of them:

1. Every write returns the saved entity, so the agent's picture stays current without a
   re-read on the happy path.
2. Every write is independently addressable by IdKey, so recovery never depends on replaying
   the sequence in order.
3. `GetWorkflowType` returns the whole tree in one call, which is what makes reading back
   after a failure cheap. This is a concrete argument against the per-activity split rejected
   under tool 4: partial reads would make recovery a multi-call exercise exactly when the
   agent's model of the workflow is least trustworthy.

The agent's instructions should say plainly that a failed write means re-read before
continuing, and never assume a write landed because the one before it did.

## Related

- [260807-ai-agent-tool-conventions.md](260807-ai-agent-tool-conventions.md) — the shared conventions this skill assumes.
- [260807-ai-agent-core-administration-skill.md](260807-ai-agent-core-administration-skill.md) — the companion skill supplying configuration metadata.
- [260807-ai-agent-result-guids.md](completed/ai/260807-ai-agent-result-guids.md) — implements the identifier rule both skills rely on.
