---
author: Jon Edmiston
date_created: 2026-08-11
summary: >-
  Adds GetSystemPhoneNumber to the Communication skill, the partner Get for the
  existing LookupSystemPhoneNumbers. Returns one system phone number in full,
  including its unique identifier, which the lookup deliberately withholds.
  Without it there is no way to reach a system phone number's GUID through the
  agent, and no way to see the fourteen columns the lookup does not return.
contributors: []
---

# GetSystemPhoneNumber

## Summary

One new read-only tool on `CommunicationSkill`. `LookupSystemPhoneNumbers` returns a
bounded reference set with five fields and no `Guid`; this returns one number with
everything on it.

Assumes the [shared tool conventions](260807-ai-agent-tool-conventions.md) and completes
the identifier rule from
[the result GUID change](completed/ai/260807-ai-agent-result-guids.md).

## Motivation

Two gaps, and the first is structural rather than a matter of taste.

**There is no route to the GUID.** The conventions rule is that a `Lookup` returns a
compact set without `Guid`, and the way a caller reaches one is the partner `Get`.
`LookupSystemPhoneNumbers` follows the first half of that rule and there is no second
half, so the identifier is currently unreachable through the agent. That matters
because Rock stores a system phone number as a GUID in the places that reference it,
including workflow action settings, so an agent configuring a `SendSms` action cannot
supply the value it needs.

**The lookup returns a third of the record.** `SystemPhoneNumber` has fourteen columns.
The lookup returns `Name`, `Description`, `Number`, `AssignedToPerson`, and
`IsSmsEnabled`. Nothing surfaces the SMS forwarding configuration, the received-message
workflow, the notification group, the opt-in and opt-out behavior, or the attribute
values, and there is no tool that will.

## Requirements

- The tool MUST return the `Guid`, populated explicitly.
- The tool MUST return every column of `SystemPhoneNumber` that a caller can act on.
- The tool MUST enforce entity-level VIEW authorization, and MUST sanitize attribute
  values for per-attribute VIEW authorization.
- The tool MUST NOT change what `LookupSystemPhoneNumbers` returns.
- Every field added to the shared result MUST be nullable, so an unset value is omitted
  rather than serialized as a default.
- Related entities MUST be returned as key and name references, never as bare ids.

## Design

### Declaration

```csharp
[Description( "Gets one system phone number in full, including its unique identifier and its SMS configuration." )]
[AgentPurpose( "Provides the complete detail of a system phone number, including the unique identifier that other configuration stores it by." )]
[AgentToolPrerequisite( "Call LookupSystemPhoneNumbers to determine the systemPhoneNumberIdKey." )]
[AgentToolGuid( "7B4E6C15-9D2A-4F83-A0E1-3C5B8D2F41A6" )]
public AgentToolResult GetSystemPhoneNumber( string systemPhoneNumberIdKey )
```

Lives on `CommunicationSkill`, next to the lookup it partners. It is a read, so it uses
`new AgentToolHelper( AgentRequestContext, _logger )` and no write context.

### Output

```
IdKey, Guid, Name, Description, Number, IsActive, Order,
AssignedToPerson { IdKey, Guid, FullName },
IsSmsEnabled, IsSmsForwardingEnabled,
SmsReceivedWorkflowType { IdKey, Guid, Name },
SmsNotificationGroup { IdKey, Guid, Name },
MobileApplicationSite { IdKey, Guid, Name },
SuppressSmsOptInOutAutoReplies, DisableSmsOptInOutTracking,
CreatedDateTime, ModifiedDateTime,
AttributeValues[]
```

The three navigation properties come back as `KeyNameResult` rather than as ids, per
the conventions. Each is null when unset, which is the common case for all three, and a
null reference is omitted from the output entirely.

`IdKey`, `Guid`, `CreatedDateTime`, `ModifiedDateTime`, and `AttributeValues` all come
from `EntityResultBase`. There is no `CreatedByPerson` or `ModifiedByPerson`: the base
class does not carry them and no result class in the codebase adds them, so this tool
does not invent the precedent.

### One result class, extended, not a second one

`SystemPhoneNumberResult` in `Rock/AI/Agent/Classes/Entity/` gains the new fields, and
both tools return it. The lookup populates what it populates today and the new fields
simply do not appear in its output.

**This works because the agent serializer omits nulls.** Verified against
`AgentSerializerOptions.GetOptions`, which inherits `DefaultIgnoreCondition` from
`AIJsonUtilities.DefaultOptions`. Serializing the class with exactly the five fields the
lookup sets produces:

```json
{"name":"Main Line","description":"The church main number","number":"+15551234567","isSmsEnabled":true,"idKey":"VWPe9xdLyw"}
```

No `guid`, no audit fields, no empty keys. Adding ten unset properties changes that
output by nothing. A second class would buy no payload saving, and it would mean two
shapes to keep in step for one entity.

**The condition: every added field must be nullable.** Null is omitted; a default is
not. The same probe shows a non-nullable `bool` and `int` serializing as `false` and `0`
even when never assigned:

```json
{"setString":"x","trueBool":true,"plainFalseBool":false,"plainZeroInt":0}
```

So `IsActive`, `IsSmsForwardingEnabled`, `SuppressSmsOptInOutAutoReplies`,
`DisableSmsOptInOutTracking` are `bool?`, and `Order` is `int?`. Declaring any of them
as a plain value type would put a misleading `false` on every row the lookup returns.
The class already sets this precedent: `IsSmsEnabled` is `bool?`, not `bool`.

The cost is that the class no longer describes a single tool's output, so a reader
cannot tell from the class alone which tool fills which field. Each added property
carries a doc comment saying it is populated by `GetSystemPhoneNumber` only. That is
cheaper than maintaining two classes, and `LookupSystemPhoneNumbers` is the only other
consumer, so the blast radius of the change is one file.

### ProviderIdentifier is deliberately excluded

The column holds the SMS provider's own identifier for the number. It is not a secret,
but it is an integration detail that no agent-facing decision depends on, it is only
meaningful to the provider, and returning it invites a model to treat it as something
it can set or match on. Left out until something needs it.

This is the one field of the fourteen that is omitted, and it is called out here so a
future reader can tell the omission from an oversight.

### Security

Two checks, and they are separate.

1. **Entity level.** `SystemPhoneNumber` derives from `Model<T>`, so it is `ISecured`.
   `helper.GetRequiredEntity<SystemPhoneNumber>` applies the VIEW check.
2. **Attribute level.** `SystemPhoneNumberCache` is a `ModelCache`, so the entity has
   attributes. `GetAttributeValueResults` filters only on `IsPublic`; the per-attribute
   VIEW check lives in `EntityResultBase.Sanitize`, and `Success` does not sanitize.
   So the tool must call it:

   ```csharp
   if ( !result.Sanitize( AgentRequestContext ) )
   {
       return Error( "You do not have permission to view this system phone number." );
   }
   ```

   This is the same trap that produced a real defect in the Core Administration skill:
   a `Get` that returns attribute values and forgets to sanitize returns values the
   person is not authorized to see, and nothing warns.

### Reading through the cache

Resolve the entity with `helper.GetRequiredEntity` for the security check, then read
detail from `SystemPhoneNumberCache.Get( id )`, matching what the lookup already does.
The navigation properties resolve through their own caches, except
`AssignedToPersonAlias`, which the lookup already resolves via
`PersonAliasService.GetPerson`.

### History

Full content. One record of roughly a dozen small fields is not large, and the value of
having it in history is that the GUID stays available for the rest of the conversation
without a second call.

### Notes

1. **Do not add `Guid` to `LookupSystemPhoneNumbers`.** The lookup stays as it is. This
   tool existing is what makes that correct rather than a gap; see the GUID spec,
   "No exceptions, including the tempting one".
2. `IsActive` is returned here even though the lookup filters to active numbers only. A
   caller that arrives with a key from elsewhere should be told the number is inactive
   rather than left to infer it.
3. No `partialName` or filter parameters. This is a `Get` of one item.

## Out of Scope

| Item | Reason |
|---|---|
| `AddOrUpdateSystemPhoneNumber` | Provisioning a phone number is a provider-side action with billing consequences. Not something to hand an agent without a separate discussion. |
| `ProviderIdentifier` | See above. |
| Listing messages sent from a number | A different entity and a different question. |

## Decisions without precedent

### One result class serving both a Lookup and a Get

Elsewhere a summary and a detail shape are separate classes, as
`WorkflowTypeSummaryResult` and `WorkflowTypeDetailResult` are. Here they are one,
because the serializer omits nulls and the two shapes differ only by which fields are
populated. Verified rather than assumed; see "One result class, extended".

The rule this establishes: **split the class when the two shapes need different types
for the same concept, not when one simply has more fields than the other.** The workflow
split earns its place because a summary carries `ActivityTypeCount` where the detail
carries the activities themselves. Nothing like that applies here.

## Related

- [260807-ai-agent-tool-conventions.md](260807-ai-agent-tool-conventions.md) — the `Lookup` versus `Get` rule this completes.
- [260807-ai-agent-result-guids.md](completed/ai/260807-ai-agent-result-guids.md) — the identifier rule that makes the partner `Get` mandatory.
