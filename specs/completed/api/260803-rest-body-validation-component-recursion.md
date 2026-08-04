---
author: Jason Hendee
date_created: 2026-08-03
summary: >-
  POST/PUT to seven entity models over the REST API takes 75 to 160 seconds
  because RockBodyModelValidator recursively walks the request graph into
  read-only properties that resolve live component and cache singletons, and
  from there into .NET reflection objects that hash entire assembly images. The
  work runs before authorization, so an unauthenticated caller can pin a worker
  thread at 100% CPU for over a minute per request. The fix replaces the
  validator's exact-name bypass list with category checks that skip
  server-resolved objects (Component subclasses, IEntityCache implementations)
  and reflection metadata, since none of those can carry request-body input.
contributors: []
---

# REST Body Validation Recurses Into Component and Cache Singletons

## Summary

Rock replaces the ASP.NET Web API body validator with `RockBodyModelValidator`, which recursively validates every populated public property of a `[FromBody]` entity graph. Several entity models expose read-only helper properties that resolve a live `Rock.Extension.Component` singleton or a `Rock.Web.Cache` object. The validator descends into those, and from there into .NET Framework Code Access Security evidence objects that compute SHA-1 and MD5 digests of whole assembly images, repeatedly.

The result is that POST/PUT on seven models takes 75 to 160 seconds per request when the body carries an id that resolves to a live component or cache, versus roughly 0.2 seconds when it does not. The database insert itself is under a millisecond; effectively all of the time is CPU spent before the database is touched. Because binding and validation run ahead of the authorization filter, an unauthenticated request incurs the same cost before it is rejected with a 401.

The fix keeps the change at the single choke point Rock already owns, `RockBodyModelValidator.ShouldValidateType`, and replaces its one-entry exact-`FullName` bypass list with category checks: skip anything assignable to `Rock.Extension.Component`, anything assignable to `Rock.Web.Cache.IEntityCache`, and .NET reflection metadata types. These objects are always resolved server-side and can never be populated from a request body, so skipping them cannot suppress a legitimate validation error.

## Motivation

This is a class of bug, not a single bad property. A live singleton resolved from an `EntityTypeId` is not user input, yet the validator treats it as another node to walk. The same shape already caused a stack-overflow crash on `POST /api/Groups` (issue #5259, fixed February 2024) by recursing through `GroupTypeCache` until the stack overflowed. That fix added a single-type bypass for `GroupTypeCache`. The present issue is the same underlying problem one notch below the crash threshold: instead of overflowing, the walk completes, but only after burning a minute or more of CPU.

Two properties of the situation raise the priority:

- **Pre-authorization cost.** The expensive work happens during model binding, which Web API runs before action filters. `SecuredAttribute` is an action filter, so the 401 is returned only after the walk finishes. An unauthenticated `PUT /api/WorkflowActionTypes/0` with a real component id returns 401 after ~75 seconds; with a bogus id it returns in ~0.3 seconds. No login is required to hold a worker thread at full CPU for over a minute, in parallel.
- **Breadth.** A timed `PUT /api/{Model}/0` probe across all IEntity models found seven affected, and the two direct cases fan out through caches so that a single body reaches every activity type, action type, and component beneath it. Two of the seven exceeded 280 seconds before the client gave up.

## Problem Statement

`RockBodyModelValidator` recursively validates the full object graph of a body-bound entity, including read-only computed properties that resolve live `Component` singletons and cache objects. Walking those objects reaches .NET reflection and CAS evidence types whose validation triggers repeated assembly-image hashing. For the affected models this turns a sub-millisecond operation into a 75-to-160-second CPU burn that runs before the request is authorized.

## Reproduction

No credentials and no data setup are required. `PUT .../{controller}/0` fails the id lookup and returns 404 (or 401 if unauthenticated) only after binding and validation have run, so it times the validator without writing anything.

Control, a non-existent `EntityTypeId`, returns in roughly 0.2 seconds:

```bash
time curl -sS -o /dev/null -X PUT 'https://<rock>/api/WorkflowActionTypes/0' \
  -H 'Content-Type: application/json' \
  -d '{"EntityTypeId":999999,"ActivityTypeId":1,"Name":"probe"}'
```

Real component, `Rock.Workflow.Action.RunLava`, takes 75 to 160 seconds:

```bash
time curl -sS -o /dev/null -X PUT 'https://<rock>/api/WorkflowActionTypes/0' \
  -H 'Content-Type: application/json' \
  -d '{"EntityTypeId":434,"ActivityTypeId":1,"Name":"probe"}'
```

Both return the same status code; only the timing differs. The RunLava `EntityTypeId` varies per install; look it up with `GET /api/EntityTypes?$filter=startswith(Name,'Rock.Workflow.Action')`. Substituting `/api/BinaryFiles/0` with `"StorageEntityTypeId": 51` (`Rock.Storage.Provider.Database`) reproduces it on the direct `BinaryFile` route.

## Root Cause

`WebApiConfig.cs:67` replaces the framework `IBodyModelValidator` with `RockBodyModelValidator`, which derives from `DefaultBodyModelValidator` and recursively walks every populated public property of the body-bound graph. `RockActionValueBinder` routes v2 body binding through `RockFormatterParameterBinding` using the same validator instance (`Rock.Rest/Utility/RockActionValueBinder.cs:40`), so both v1 and v2, and both POST and PUT, share this behavior.

The walk reaches a live component through a read-only entity property. For `WorkflowActionType`, `WorkflowAction` resolves an `ActionComponent` out of `ActionContainer` (`Rock/Model/Workflow/WorkflowActionType/WorkflowActionType.Logic.cs:37`):

```csharp
public virtual ActionComponent WorkflowAction
{
    get
    {
        return GetWorkflowAction( this.EntityTypeId );
    }
}
```

For `BinaryFile`, the `StorageEntityTypeId` setter assigns `StorageProvider` from `ProviderContainer` as a side effect (`Rock/Model/Core/BinaryFile/BinaryFile.cs:123`), and `StorageProvider` is a read-only `ProviderComponent` (`Rock/Model/Core/BinaryFile/BinaryFile.Logic.cs:39`).

Once inside a `Component`, the walk continues through `Component.EntityType` (`Rock/Extension/Component.cs:322`) to `EntityTypeCache.Properties`, a `Dictionary<string, PropertyInfo>` (`Rock/Web/Cache/Entities/EntityTypeCache.cs:143`). From a `PropertyInfo` the walk reaches `Module`, then `Assembly`, then the assembly's `Evidence`, then `System.Security.Policy.Hash`, whose `SHA1` and `MD5` getters hash the entire assembly image. That hashing is where the time is spent. Cost tracks assembly size and how many assemblies the walk reaches, not install size or data volume.

The five remaining models reach the same expense through the cache chain rather than a direct component property. `Workflow.WorkflowTypeCache` (`Rock/Model/Workflow/Workflow/Workflow.Logic.cs:47`) leads to `WorkflowTypeCache.ActivityTypes` (`Rock/Web/Cache/Entities/WorkflowTypeCache.cs:268`) to `WorkflowActivityTypeCache.ActionTypes` (`Rock/Web/Cache/Entities/WorkflowActivityTypeCache.cs:115`) to `WorkflowActionTypeCache.WorkflowAction` (`Rock/Web/Cache/Entities/WorkflowActionTypeCache.cs:156`), which resolves the same `ActionComponent`. Separately, `WorkflowActionTypeCache.EntityType` (`Rock/Web/Cache/Entities/WorkflowActionTypeCache.cs:148`) reaches `EntityTypeCache.Properties` without passing through any component at all. Because a single body fans out to every activity type, action type, and component beneath it, the cache-chain models are worse than the two direct cases.

### Affected models

| Model | Computed property | Route to the expensive work |
|----|----|----|
| WorkflowActionType | `WorkflowAction` | direct, `ActionComponent` |
| BinaryFile | `StorageProvider` | direct, `ProviderComponent` |
| Workflow | `WorkflowTypeCache` | cache chain |
| WorkflowActivity | `ActivityTypeCache` | cache chain |
| WorkflowAction | `ActionTypeCache` | cache chain |
| ConnectionWorkflow | `WorkflowTypeCache` | cache chain |
| BenevolenceWorkflow | `WorkflowTypeCache` | cache chain |

### Why authorization does not protect the endpoint

`SecuredAttribute` is an `ActionFilterAttribute` (`Rock.Rest/Filters/SecuredAttribute.cs:44`). Web API executes model binding, including body validation, before action filters run, so the validator has already completed its walk by the time the security check would reject the request. The expensive work is therefore reachable without authentication.

### Why this is not a regression

No single commit introduced this. The recursive walk is the stock `DefaultBodyModelValidator` behavior that has been present since the v1 REST API existed. The component-reaching properties predate the current validator by years (the `WorkflowActionType.WorkflowAction` getter dates to 2012; `BinaryFile` storage-provider resolution to 2015). `RockBodyModelValidator` itself was introduced in February 2024 for issue #5259 and added the bypass mechanism, but it did not introduce the walk. This issue is a second instance of the same class the #5259 fix addressed for one type.

## Affected Code Paths

Primary (the fix lands here):

- `Rock.Rest/Validation/RockBodyModelValidator.cs` — the `ShouldValidateType` override and its bypass set.

Registration and binding (context, not changed):

- `Rock.Rest/App_Start/WebApiConfig.cs:67` — installs `RockBodyModelValidator` as the `IBodyModelValidator`.
- `Rock.Rest/Utility/RockActionValueBinder.cs:40` — routes v2 body binding through the same validator, so the fix covers v2 as well as v1.
- `Rock.Rest/Filters/ValidateAttribute.cs:81` — turns remaining `ModelState` errors into a 400. This defines the behavior the fix must preserve: any legitimate validation error must still surface here.

Reached during the walk (why the categories are chosen, not changed):

- `Rock/Extension/Component.cs:322` — `Component.EntityType`, the bridge from a component into cache and reflection.
- `Rock/Web/Cache/Entities/EntityTypeCache.cs:143` — `Properties`, the `Dictionary<string, PropertyInfo>` that exposes reflection objects to the walk.
- `Rock/Web/Cache/EntityCache.cs:41` and `Rock/Web/Cache/IEntityCache.cs:26` — every entity cache implements `IEntityCache`, which is what the cache-category check keys on.

## Proposed Fix

Replace the exact-`FullName` bypass list in `RockBodyModelValidator.ShouldValidateType` with category checks. The validator exists to find DataAnnotations violations in request input, so it should never descend into an object that cannot have come from the request body. Three categories cover that:

1. **`Rock.Extension.Component` subclasses.** Severs the two direct routes (`WorkflowActionType.WorkflowAction`, `BinaryFile.StorageProvider`).
2. **`Rock.Web.Cache.IEntityCache` implementations.** Severs the five cache-chain models at the first node, so the per-request fan-out across every activity type and action type never begins, and closes the `WorkflowActionTypeCache.EntityType` route into `EntityTypeCache.Properties` that a component-only bypass would leave open. This category subsumes the existing `GroupTypeCache` entry, so the hand-maintained name list is removed rather than extended, and the roughly twenty other caches that expose an `EntityTypeCache` property stop being latent instances of the same bug.
3. **.NET reflection metadata types** (`System.Reflection.MemberInfo`, which covers `Type` and `PropertyInfo`, plus `Module` and `Assembly`). Defense in depth. The measurable cost is assembly hashing reached through reflection objects, so blocking these makes the expense unreachable even by a future property that reaches reflection by a path not anticipated here.

The checks are inline `IsAssignableFrom` calls, which keeps the override in the same simple shape it has today. No per-type memoization is warranted: `ShouldValidateType` sees only a few dozen distinct types per request graph, and five `IsAssignableFrom` calls per type are nanosecond-scale next to the property reflection and DataAnnotations work that validation performs on each node.

Sketch:

```csharp
public class RockBodyModelValidator : DefaultBodyModelValidator
{
    /// <inheritdoc/>
    public override bool ShouldValidateType( Type type )
    {
        // These are resolved server-side and never bound from the request
        // body, so validating them yields no meaningful ModelState errors
        // and walking them is unboundedly expensive.
        if ( typeof( Rock.Extension.Component ).IsAssignableFrom( type )
            || typeof( Rock.Web.Cache.IEntityCache ).IsAssignableFrom( type )
            || typeof( System.Reflection.MemberInfo ).IsAssignableFrom( type )
            || typeof( System.Reflection.Module ).IsAssignableFrom( type )
            || typeof( System.Reflection.Assembly ).IsAssignableFrom( type ) )
        {
            return false;
        }

        return base.ShouldValidateType( type );
    }
}
```

## Fix Risks

- **No change to 400 behavior.** `ValidateAttribute` converts `ModelState` errors into 400s, so the risk to guard is suppressing a legitimate error. Every property this skips is either getter-only (`WorkflowAction`, `WorkflowTypeCache`, `EntityType`) or not `[DataMember]` (`StorageProvider`), so JSON deserialization can never place request data in them. Any error they produced would come from validating server state, which is the false-positive class the #5259 fix already established as safe to skip. The `GroupTypeCache` entry present today concedes this principle for caches; the fix generalizes it.
- **Getters still execute once.** The bypass prevents descent into a returned object, not the invocation of the getter that returns it (metadata is read before `ShouldValidateType` is consulted). The affected getters are cheap cache lookups, so this has no material cost. The expense was always in the descent, not the getter.
- **Authentication and authorization are unchanged.** `SecuredAttribute` still runs and returns the same 401 and 403 responses. Once validation costs milliseconds, the pre-authorization CPU lever is gone; the fix does not need to reorder binding and authorization to close it.
- **No added state.** The override holds no fields and no static state, so it introduces no threading or lifetime concerns beyond those the base validator already has.

## Verification Steps

1. **Predicate unit tests.** `ShouldValidateType` returns false for a concrete `ActionComponent`, a concrete `ProviderComponent`, an `EntityTypeCache`, a `WorkflowTypeCache`, and `typeof(PropertyInfo)`; returns true for `typeof(Group)` and other plain entities.
2. **Direct routes.** `PUT /api/WorkflowActionTypes/0` with `EntityTypeId` set to RunLava, and `PUT /api/BinaryFiles/0` with `StorageEntityTypeId` 51, both complete in well under a second and return the same status code as before.
3. **Cache-chain route.** `PUT /api/Workflows/0` with a real `WorkflowTypeId` completes in well under a second.
4. **Full-surface regression.** Rerun the timed `PUT /api/{Model}/0` probe across all IEntity models and confirm every model responds in milliseconds, verifying the whole route surface rather than only the seven measured here.
5. **#5259 regression.** `POST /api/Groups` with a valid body still succeeds and does not crash.
6. **Genuine validation still fires.** A body that violates a DataAnnotations rule on a real bound property (for example an over-max-length `Name`) still returns 400 through `ValidateAttribute`.
7. **v1 and v2 parity.** Confirm both `/api/` and `/api/v2/` body-bound POST and PUT behave identically after the change.

## Out of Scope

- **Reordering authorization ahead of model binding.** `SecuredAttribute` is a public `ActionFilterAttribute`; converting it to an authorization filter is a breaking change for any plugin that subclasses it, and it is unnecessary once validation is cheap. This is a separate, larger change.
- **Changing the model properties themselves.** The side-effectful `StorageEntityTypeId` setter and the container-scanning getters are `[LavaVisible]` public API with plugin and Lava exposure. Fixing the validator addresses the whole class at once; reworking individual properties does not and carries more compatibility risk.
- **Rewriting the validator's traversal.** `DefaultBodyModelValidator` exposes only `Validate` and `ShouldValidateType` as extension points; depth caps or per-property skipping would mean forking the framework class. Unnecessary once the categories are blocked, though a durable follow-up could revisit it.

## Considered but Rejected

### Bypass only `Rock.Extension.Component` subclasses
A component-category bypass fixes the two direct models but not the five cache-chain models, which reach the same assembly-hashing work through `IEntityCache` objects without passing through a `Component`. `WorkflowActionTypeCache.EntityType` also reaches `EntityTypeCache.Properties` independently of any component. Adding the `IEntityCache` category is what covers all seven models and removes the need to audit the transitive property graph of the cache classes. Retained the component category as one of the three; rejected it as the whole fix.

### Keep an exact-name bypass list and add the seven types by name
An exact-`FullName` set cannot match subclasses and must be re-audited whenever a model gains a component- or cache-typed property. The category checks are complete by construction for the reflection-hashing expense and remove the maintenance burden, so the name list is replaced rather than grown.

### Move authorization before model binding
This would stop unauthenticated callers from triggering the cost, but it is a breaking change to a public filter type and does not help authenticated callers, who would still wait 75-plus seconds. Making validation cheap addresses both audiences and both threat models. Left as out of scope.

### Cap recursion depth or skip reflection members inside a forked validator
A durable long-term hardening, but it requires reimplementing framework traversal that is not exposed for extension. The category bypass stops the problem now with a change confined to the one method Rock already overrides; a deeper traversal rewrite can be revisited separately.

## Related

- Prior instance of the same class: issue #5259, `POST /api/Groups` stack overflow, fixed February 2024, which introduced `RockBodyModelValidator` and the original `GroupTypeCache` bypass.
- Validator registration: `Rock.Rest/App_Start/WebApiConfig.cs:67`.
- v2 binding path that shares the validator: `Rock.Rest/Utility/RockActionValueBinder.cs:40` and `Rock.Rest/Utility/RockFormatterParameterBinding.cs`.
- `ModelState`-to-400 translation the fix must preserve: `Rock.Rest/Filters/ValidateAttribute.cs:81`.
