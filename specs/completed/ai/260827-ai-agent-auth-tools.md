---
author: Daniel Hazelbaker
date_created: 2026-08-27
summary: >-
  Four AI agent tools for reading and writing entity authorization (Rock's
  per-entity security rules), added to CoreAdministrationSkill. Two reads list
  an entity's effective rules and its supported actions; two writes add/update
  and delete a single rule atomically. Every write follows the canonical
  Security block pattern and refreshes the authorization cache, and the write
  and delete tools carry a guardrail because there is no core safeguard against
  an administrator removing their own access.
contributors: []
---

# Agent Authorization Tools

## Summary

This spec defines four tools that let an agent read and write Rock's entity authorization, the per-entity security rules stored in the `Auth` table. They live in `CoreAdministrationSkill`, not a new skill, because authorization is cross-cutting reference-and-configuration surface that every other configuration skill may need, which is the same reason defined types and categories live there.

- `ListAuthorizationForEntity` reads the auth rules that apply to one entity, or to an entity type's default.
- `ListAuthorizationActionsForEntity` reads the actions that entity supports securing, such as View, Edit, and Administrate.
- `AddOrUpdateAuthorizationForEntity` adds or updates a single rule.
- `DeleteAuthorizationForEntity` deletes a single rule.

The goal is one general mechanism that can set authorization for *any* securable entity, driven entirely by IdKeys, following the same read and write paths as Rock's own Security dialog.

## Motivation

An agent that configures Rock will eventually need to secure what it configures: restrict a workflow to a role, grant a group View on a category, make a data view private. Today it cannot, and there is no partial substitute. Authorization is also the one configuration surface where a wrong write is dangerous in a way a wrong defined value is not, because a rule change can remove the acting person's own access and there is no core safeguard that prevents it. That combination, high value and real risk, is why it is worth specifying before it is built rather than growing it per skill.

Two facts shape the whole design and were confirmed by reading the code rather than assumed:

**There is no core lock-out protection.** `Rock.Blocks.Administration.Security` gates each action only on `IsAuthorized( ADMINISTRATE, CurrentPerson )` and will happily delete the very rule granting the current person that access; recovery relies on an inherited parent rule or a higher role. The tools must therefore carry their own guardrail.

**`GetPaginatedItems` performs no security filtering,** per the tool conventions. It does not arise here because the rule sets are small and read whole, but the write path's cache refresh is the same class of silent-failure hazard: a rule saved without refreshing the authorization cache changes nothing observable until the cache expires.

## Requirements

**Surface**

- All four tools MUST live in `CoreAdministrationSkill`.
- Every parameter MUST be an IdKey or an enum. No fully qualified class names, no raw integer Ids, no Guids in parameters.
- The two write tools (`AddOrUpdate`, `Delete`) MUST each carry `[AgentGuardrail]`, following `NoteSkill.DeleteNote`.
- A write MUST refresh the authorization cache for the affected entity and action before returning success.

**Correctness**

- A write or delete MUST validate that the targeted `Auth` row belongs to the entity named in the call, mirroring `GetValidatedAuth` in the Security block, so a caller cannot mutate an unrelated rule by passing its key.
- The target entity MUST be `ISecured`. A non-secured entity type is an error naming the problem.
- A change MUST be atomic: one `SaveChanges` for the rule, then one cache refresh. No multi-step write that can half-apply.

**Safety**

- The write and delete descriptions MUST warn against removing the acting person's own access.
- The tools MUST always refuse a change that would strip the acting person's own `ADMINISTRATE` on the entity (unless an inherited or role-based rule preserves it). There is no override parameter; a genuine self-lockout must be performed through Rock's security screen.

## Design

### Storage model, read out of the code

One `Auth` row (`Rock/Model/Core/Auth/Auth.cs`) is one rule:

| Column | Meaning |
|---|---|
| `EntityTypeId` | The entity type secured. Required. |
| `EntityId` | The specific instance. **Null means the entity type's default**, applying to all instances. |
| `Action` | `View`, `Edit`, `Administrate`, and so on. |
| `AllowOrDeny` | `"A"` or `"D"`. |
| `SpecialRole` | `None`, `AllUsers`, `AllAuthenticatedUsers`, `AllUnAuthenticatedUsers`. |
| `PersonAliasId` | Set when the rule targets a person and `SpecialRole` is `None`. |
| `GroupId` | Set when the rule targets a security role and `SpecialRole` is `None`. |
| `Order` | Lower wins; the first matching rule decides. |

A rule targets exactly one subject kind: a special role, a person, or a security role group. Rules are evaluated in `Order`; the first match settles allow or deny, and if no rule matches, the entity type's `IsAllowedByDefault` decides (true only for View and Tag).

### Tool inventory

| # | Tool | Kind | Guardrail | Guid |
|---|---|---|---|---|
| 1 | `ListAuthorizationForEntity` | List | no | `25CA6D47-0883-40C4-B222-BC0C64693C11` |
| 2 | `ListAuthorizationActionsForEntity` | List | no | `7E4933CE-3E2E-4755-B3E8-7424F0642A5A` |
| 3 | `AddOrUpdateAuthorizationForEntity` | AddOrUpdate | **yes** | `FF3C1804-8980-4043-A35C-45830EB3336F` |
| 4 | `DeleteAuthorizationForEntity` | Delete | **yes** | `AE38A4D5-2F9D-4865-A5EC-AA7719311D50` |

### Resolving an arbitrary entity

Every tool takes `entityTypeIdKey` and an optional `entityIdKey`. Resolution follows the Security block: resolve the entity type, decode `entityIdKey` to an Id, and load the instance via the entity type's service; when `entityIdKey` is omitted, target the entity type default (`Activator.CreateInstance` of the type, as `TryGetSecuredEntity` does). The resolved object must be `ISecured`; if it is not, return an error naming the entity type. This is the one place raw reflection is unavoidable, and it is confined to a private helper.

Because `entityIdKey` decodes to an Id, honor `DisablePredictableIds` the way the reporting and security code does: do not accept a raw integer where predictable Ids are disabled.

### 1. ListAuthorizationForEntity

```csharp
public AgentToolResult ListAuthorizationForEntity( string entityTypeIdKey, string entityIdKey = null, string action = null )
```

Lists the rules that apply to the entity, or to the entity-type default when `entityIdKey` is omitted. Reads with `AuthService.Get( entityTypeId, entityId )` (or `GetAuths(..., action)` when `action` is supplied), which returns rows ordered by `Order`.

**Output** per rule: `IdKey, Action, AllowOrDeny, Order, Subject`. `Subject` is one shape describing the target: `{ Kind, SpecialRole?, Person { IdKey, Name }?, Group { IdKey, Name }? }` where `Kind` is `Person`, `Group`, or `SpecialRole`. Also a top-level note when no `AllUsers` rule exists for an action, echoing the Security block's warning that unmatched people fall through to the default.

**Volume.** A handful to a few dozen rules. **Paging.** None; read whole. **History.** Compact.

**Why not paged.** The rule set for one entity is bounded and small. This is the "bounded reference set, returned whole" case.

Inherited rules from parent authorities (category, parent entity) are **read-only context**: include them flagged `IsInherited = true` with the authority they came from, following `AddParentRules`, so a caller understands the effective picture, but the write tools refuse to edit them (an inherited rule has no `Auth` row on this entity to edit).

### 2. ListAuthorizationActionsForEntity

```csharp
public AgentToolResult ListAuthorizationActionsForEntity( string entityTypeIdKey, string entityIdKey = null )
```

Returns the actions the entity supports securing, from `ISecured.SupportedActions` (a `Dictionary<string,string>` of action to description).

**Output** per action: `Action, Description, IsAllowedByDefault`. Named `Actions`, not `Verbs`: Rock's codebase calls these actions throughout (`Authorization.VIEW`, `SupportedActions`), and "verb" appears nowhere.

**Volume.** Three to a handful. **Paging.** None. **History.** Compact.

### 3. AddOrUpdateAuthorizationForEntity

`[AgentGuardrail]`

```csharp
public AgentToolResult AddOrUpdateAuthorizationForEntity(
    string entityTypeIdKey,
    string action,
    AllowOrDeny allowOrDeny,
    string entityIdKey = null,
    string authIdKey = null,
    string personIdKey = null,
    string groupIdKey = null,
    SpecialRole? specialRole = null,
    int? order = null )
```

Adds a new rule or updates an existing one. `AllowOrDeny` is a new two-value enum (`Allow`, `Deny`) mapped to the stored `"A"`/`"D"`; the raw single letters never appear in the surface. Exactly one subject must be identified: `personIdKey`, `groupIdKey`, or `specialRole`.

**Update path.** When `authIdKey` is supplied, load that `Auth`, validate it belongs to the named entity and action (mirroring `GetValidatedAuth`), and update `AllowOrDeny` and `Order`. Changing the subject of an existing rule is not allowed; delete and re-add instead.

**Add path.** Validate the action is in `SupportedActions`. Resolve the subject. De-duplicate the way `AddRole`/`AddUser` do: an identical existing rule is returned rather than duplicated. Append at `Order = max + 1` unless `order` is given.

**Cache.** After `SaveChanges`, call `Authorization.RefreshAction( entityTypeId, entityId, action )`, the narrowest refresh. This is not optional; without it the change is invisible until the cache expires.

**Self-lockout guard.** Before saving a Deny (or a change that would leave the acting person without an Allow) on `ADMINISTRATE`, check whether the acting person would still be authorized to Administrate the entity afterward, counting inherited and role rules. If not, refuse and say so. There is no override: removing your own ability to administer an entity through the agent is never allowed, and the rare case where it is genuinely intended is performed through Rock's security screen instead. Core has no such guard; this is a deliberate addition.

**Output.** The saved rule in the tool 1 shape, plus a chained instruction to re-read with `ListAuthorizationForEntity` since order interacts across rules.

### 4. DeleteAuthorizationForEntity

`[AgentGuardrail]`

```csharp
public AgentToolResult DeleteAuthorizationForEntity( string authIdKey, string entityTypeIdKey, string entityIdKey = null )
```

Deletes one rule. Load the `Auth`, validate it belongs to the named entity (the `GetValidatedAuth` ownership check), delete, `SaveChanges`, then `RefreshAction`. The same self-lockout guard applies and is likewise not overridable: a deletion that would remove the acting person's own `ADMINISTRATE` is always refused.

Requiring `entityTypeIdKey` (and `entityIdKey`) alongside `authIdKey` is the ownership check, not redundancy: it is what stops a caller deleting an unrelated rule by key.

### Result classes

Under `Agent/Rock.AI.Agent/Classes/Skills/CoreAdministrationSkill/`: `AuthorizationRuleResult` (IdKey, Action, AllowOrDeny, Order, Subject, IsInherited, InheritedFrom), `AuthorizationSubjectResult` (Kind, SpecialRole, Person, Group), `AuthorizationActionResult` (Action, Description, IsAllowedByDefault). One new enum, `AllowOrDeny { Allow, Deny }`, in `Rock.Enums` under the Security domain, or reuse an existing one if a suitable enum already exists (open question).

### Cross-cutting conventions applied

- **Admin surface.** Authorization is gated at the skill; person-level filtering is not sufficient. The write and delete tools additionally require the acting person to be authorized to `ADMINISTRATE` the target, matching the Security block gate.
- **Validate before writing.** Every reference (entity type, entity, person, group, action) is checked against its source and the error names the bad value.
- **Chain forward on errors.** A bad action chains to `ListAuthorizationActionsForEntity`; a bad entity chains to the relevant lookup.
- **Audit is automatic.** `Auth.SaveHook` writes `AuthAuditLog` rows on every change; the tools do not write audit records themselves.

## Out of Scope

- **Reordering as its own tool.** `Order` is settable on `AddOrUpdate`, which covers the need. A dedicated reorder tool like the Security block's `ReorderRule` can come later if the single-rule order proves awkward.
- **Bulk or copy operations.** No `CopyAuthorization` equivalent. One rule per call keeps each change reviewable and atomic.
- **Editing inherited rules.** Inherited rules are read-only context. To change them, the caller edits the parent authority (the category or parent entity) directly.
- **The two destructive convenience patterns.** `MakePrivate` and `AllowAllUsers` both delete all existing rules for an action first. They are not exposed; a caller composes the same result from explicit add/delete calls, where each destructive step is visible.

## Considered but Rejected

### One tool that sets the whole rule set for an action

Rejected. A single "here are the rules for View" call is atomic in the wrong unit: it makes every change a full replace, which is exactly the destructive shape (`AllowAllUsers`, `MakePrivate`) that makes self-lockout easy and review hard. One rule per call keeps each change small and reversible.

### Accepting `"A"`/`"D"` directly

Rejected. The stored form is an implementation detail. An `AllowOrDeny` enum makes the two states explicit and unmistakable, the same reasoning that keeps class-name strings out of the surface.

### No lock-out guard, matching core

Rejected. Core relies on a human noticing the Administrate tab before they save. An agent has no such instinct, and the failure is severe and easy to hit. The guard always refuses a self-lockout rather than offering an override: the legitimate case is rare enough that routing it to Rock's security screen is preferable to giving the agent any way to remove its operator's own access.

## Open Questions

1. **Does a suitable `AllowOrDeny` enum already exist** in `Rock.Enums`, or is a new one warranted? The stored value is a string today; nothing forces a new enum, but the surface reads better with one.
2. **How far should the self-lockout check reach?** Checking `IsAuthorized( ADMINISTRATE )` after a simulated change is the reliable test, but it means evaluating authorization mid-write. The alternative, a cheaper heuristic on the visible rules, can miss inherited grants. The reliable check is preferred unless it proves too expensive.
3. **Should inherited rules appear in `ListAuthorizationForEntity` by default,** or behind a flag? They are essential to understanding the effective picture but add rows the caller cannot act on. Defaulting them on, clearly flagged, is proposed.

## Related

- [260807-ai-agent-tool-conventions.md](260807-ai-agent-tool-conventions.md) — the shared conventions this spec assumes.
- [260807-ai-agent-core-administration-skill.md](completed/ai/260807-ai-agent-core-administration-skill.md) — the skill these tools are added to.
- `Rock.Blocks/Administration/Security.cs` — the canonical read/write pattern these tools mirror.
