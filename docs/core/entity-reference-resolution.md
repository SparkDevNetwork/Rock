---
title: Entity Reference Resolution
last_updated: 2026-05-01
related_files:
  - Rock/Data/Service.cs
  - Rock/Data/Entity.cs
  - Rock/Utility/IdHasher.cs
  - Rock/Net/RockRequestContext.cs
  - Rock/Model/CMS/Site/Site.cs
  - .claude/rules/block-architecture.md
---

# Entity Reference Resolution

## Overview

Rock entities can be referenced by three different keys: integer `Id`, `Guid`, and `IdKey` (a non-sequential hashed string). Blocks, REST endpoints, and Lava commands accept any of the three forms in URL parameters and bag fields. The resolution machinery lives in `Rock.Data.Service<T>` and is governed by the per-site **Disable Predictable Ids** setting, which controls whether raw integer Ids are accepted as input.

## Why It Exists

Sequential integer Ids leak information. If `/Page/Group?GroupId=42` works, `/Page/Group?GroupId=43` also works, and an unauthenticated visitor can iterate the namespace to count groups, scrape entity URLs, or probe security. Public-facing sites need entity references that are non-guessable and non-enumerable. `IdKey` solves this with a non-cryptographic hash of the integer Id, configured per-site via `DisablePredictableIds`. Internal admin sites keep the simpler integer form for convenience; public sites turn integer ids off and force callers to use `IdKey` or `Guid`.

The `IdHasher` class itself is explicit on this point: "These keys are non-sequential and cannot be guessed. This is not a cryptographic one-way hash." (`Rock/Utility/IdHasher.cs:24`).

## Mental Model

There is **one resolver** and **one site setting**. Whenever a block or service method takes an entity reference from the outside world (page parameter, route value, bag field, REST input), it should run that string through the resolver and pass the site setting through as the "are integer ids allowed?" flag. The resolver figures out whether the string is an int, a Guid, or an `IdKey`, and returns the matching record (or nothing).

Think of `IdKey` as a stable obfuscation, not a security boundary. It prevents enumeration of consecutive ids, but it is not encrypted, not signed, and not authenticated. Authorization still has to happen separately. The setting just controls whether predictable integer ids are accepted as a shortcut; it does not change which entities a caller can ultimately reach once authorized.

The decoder priority is fixed: try as int (if allowed), then as Guid, then as `IdKey`. The first one that parses wins. Anything that does not parse returns no result.

## What You Need to Know

- **Always pass the site setting through.** The canonical pattern is `entityService.GetQueryableByKey( key, !PageCache.Layout.Site.DisablePredictableIds )`. Hardcoding `true` defeats the site's predictable-ids guarantee for that block; hardcoding `false` breaks internal admin pages that pass integer ids. The block architecture rule (`.claude/rules/block-architecture.md`) is unambiguous on this. Use the pass-through every time.
- **Page parameter names should be the simple entity name.** Use `Group`, `Person`, `Campus` (PascalCase), not `GroupId` / `GroupIdKey` / `GroupGuid`. The parameter accepts any of the three forms; the resolver figures out which. Mixing names creates ambiguity for callers and makes it harder to swap forms across environments.
- **`IdKey` is computed lazily.** It lives on every `Model<T>` as a `[NotMapped]` property at `Rock/Data/Entity.cs:64` and is computed via `IdHasher.Instance.GetHash( Id )` on access. Reading `entity.IdKey` in a tight loop hashes on every iteration; cache it locally if you need to.
- **`IdHasher` derives its salt from `DataEncryptionKey`.** If two Rock instances share a database but have different `DataEncryptionKey` values in `web.config`, their `IdKey` strings for the same row will not match. This matters when copying URLs between environments or testing.
- **`PageParameterAsId( name )` is the convenience helper for blocks.** Defined at `Rock/Net/RockRequestContext.cs:782`. Returns `0` (not null) on failure. Use it when you only need the int and you want the site setting honored automatically.

## Common Scenarios

**Resolve a single entity from a page parameter (the common case).** Read the parameter, call `GetQueryableByKey`, materialize, check authorization separately:

```csharp
var groupKey = PageParameter( PageParameterKey.Group );
var allowIntegerIds = !PageCache.Layout.Site.DisablePredictableIds;

var group = new GroupService( rockContext )
    .GetQueryableByKey( groupKey, allowIntegerIds )
    .FirstOrDefault();
```

If you need eager-load semantics rather than a queryable, the `Get( string key, bool allowIntegerIdentifier )` overload at `Rock/Data/Service.cs:282` is the single-call equivalent.

**Build a URL that links to an entity.** Use `entity.IdKey`, not `entity.Id`, when the link target may be on a public site that disables predictable ids. Internal-only links can use either form; the resolver accepts both.

**Pass an entity reference into Lava or a workflow attribute.** Use `IdKey` for any value that might be rendered into a public-facing URL. Use `Guid` for values that are stored long-term (unlike `IdKey`, the `Guid` of an entity does not change if `DataEncryptionKey` rotates).

## Key Architectural Decisions

**Three forms accepted, one resolver path.** Callers should not branch on which form they got. The resolver in `Rock/Data/Service.cs:224` does the dispatch. This keeps every block and endpoint identical in shape and means the site-setting toggle is the single point of control over whether integer ids are honored.

**Site-level toggle, not request-level.** `DisablePredictableIds` is a property of `Site`, not a per-request flag. The decision of whether a deployment leaks sequential ids is a deployment decision, not a per-call decision. This is also why the page parameter resolver (`PageParameterAsId`) reads the setting from the page's site automatically; callers should not be making this choice on their own.

**Hashids, not encryption.** `IdHasher` uses Hashids with a salt derived from `DataEncryptionKey` (`Rock/Utility/IdHasher.cs:50`). It is reversible by anyone with the salt and the algorithm is public. The goal is non-enumeration, not secrecy. Treating `IdKey` as a security boundary is a misuse.

**`IdKey` lives on `Model<T>`, not on a separate service call.** Putting it directly on the entity (`Rock/Data/Entity.cs:64`) means any code that has an entity has its hashed key, with no extra service round-trip. The trade-off is the lazy hash on access; in practice this is negligible compared to the database query that produced the entity.

## Technical Reference

### Service / API Surface

- **`Service<T>.GetQueryableByKey( string key, bool allowIntegerIdentifier = true )`** at `Rock/Data/Service.cs:224`. Returns an `IQueryable<T>` filtered to the single matching entity (or no match). Use when you want to chain `.Include()`, `.AsNoTracking()`, or other LINQ before materializing.
- **`Service<T>.Get( string key, bool allowIntegerIdentifier = true )`** at `Rock/Data/Service.cs:282`. Eagerly fetches the single matching entity or returns `null`. Same key-decoding behavior as `GetQueryableByKey`.
- **`Service<T>.Get( int id )`** at `Rock/Data/Service.cs:257`. Direct integer lookup, no decoding. Use only when you already have an int from a trusted source (your own database, not a request parameter).
- **`Service<T>.Get( Guid guid )`** at `Rock/Data/Service.cs:267`. Direct Guid lookup.
- **`IEntity.IdKey`** at `Rock/Data/Entity.cs:64`. `[NotMapped]` string property on every `Model<T>`. Returns `IdHasher.Instance.GetHash( Id )` or empty string if `Id` is zero.
- **`IdHasher.Instance.GetId( string hashedKey )`** at `Rock/Utility/IdHasher.cs:61`. Decodes a hashed key to its integer id, or `null` if the string is not a valid hash.
- **`IdHasher.Instance.GetHash( int id )`** at `Rock/Utility/IdHasher.cs:101`. Encodes an integer to its hashed key.
- **`IdHasher.Instance.TryGetId( string hashedKey, out int id )`** at `Rock/Utility/IdHasher.cs:80`. Try-pattern variant of `GetId`.

### Site Setting

- **`Site.DisablePredictableIds`** at `Rock/Model/CMS/Site/Site.cs:471`. `bool`, default `false`. When `true`, raw integer ids in inputs are rejected; only `IdKey` and `Guid` are honored.
- **`SiteCache.DisablePredictableIds`** at `Rock/Web/Cache/Entities/SiteCache.cs:510`. Mirror of the entity property on the cached site, used by request-context lookups to avoid hitting the database.

### Request Context Helpers

- **`RockRequestContext.PageParameterAsId( string name )`** at `Rock/Net/RockRequestContext.cs:782`. Reads a page parameter and returns its value as an int, honoring the site's `DisablePredictableIds` setting. Returns `0` on failure. Pattern internally: try int parse if allowed, else decode through `IdHasher`.
- **`RockRequestContext` context entity loading** at `Rock/Net/RockRequestContext.cs:618`. The same site-setting pass-through is applied when context entity cookies are decoded into `IEntity` instances.

### Decoder Priority

`GetQueryableByKey` and the eager `Get( string key, ... )` overload follow this fixed priority for the input string:

1. **Integer parse** (if `allowIntegerIdentifier` is `true`). On hit, returns the entity with that `Id`.
2. **Guid parse**. On hit, returns the entity with that `Guid`.
3. **`IdHasher.Instance.GetId`** (hashids decode). On hit, returns the entity with that decoded `Id`.
4. No match. Returns an empty queryable / `null`.

The first parse that succeeds wins; subsequent forms are not tried. This is why integer ids must be disabled at the site level (not at the call site) to actually be ignored: any caller that hardcodes `allowIntegerIdentifier: true` short-circuits the priority chain.

### Page Parameter Naming

Per `.claude/rules/block-architecture.md`, page parameters that accept an entity reference should be named for the simple entity (e.g. `Group`, `Person`, `Campus`), not the suffix-tagged variant (`GroupId`, `GroupIdKey`). This is because the parameter accepts any of the three forms; the suffix-tagged name lies about that. Block code reads with `PageParameter( PageParameterKey.Group )` and feeds the result into `GetQueryableByKey`.

### File Index

- `Rock/Data/Service.cs` (Service<T> base, with `Get( string )` and `GetQueryableByKey`)
- `Rock/Data/Entity.cs` (`IdKey` property on every Model<T>)
- `Rock/Utility/IdHasher.cs` (Hashids singleton)
- `Rock/Net/RockRequestContext.cs` (`PageParameterAsId`, context-entity decode)
- `Rock/Model/CMS/Site/Site.cs` (`DisablePredictableIds`)
- `Rock/Web/Cache/Entities/SiteCache.cs` (cached mirror)
- `.claude/rules/block-architecture.md` (the canonical pass-through pattern)

## Recent Impactful Changes

- **2025-11-20** ([commit `07c14ca3e0`](https://github.com/SparkDevNetwork/Rock/commit/07c14ca3e0)). Fixed an issue where `IdHasher` would throw an exception when decoding a null or empty hashed key. Callers can now pass empty strings to `GetQueryableByKey` and get a no-match result instead of an exception.
