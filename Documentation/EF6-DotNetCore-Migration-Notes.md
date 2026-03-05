# EF6 and .NET Core — Migration Research Notes

**Date:** 2026-03-05
**Context:** Rock RMS .NET Core migration planning
**EF6 version in scope:** 6.4.x (latest)

---

## Summary

Entity Framework 6.4 added multi-targeting support so EF6 could be *referenced* from .NET Core projects. This was Microsoft's intentional bridge to ease migrations — not a permanent destination. EF6 remains in **maintenance mode only** (security fixes, no new features), and Microsoft explicitly recommends EF Core for any new development or greenfield migration.

---

## What EF6.4 Added for .NET Core

EF6.3 (2019) was the first release to multi-target `netstandard2.1`, enabling EF6 to be consumed in a .NET Core project. EF6.4 refined this support. The goal was to allow large applications to migrate incrementally — moving the web/host layer to .NET Core while keeping existing EF6 data access code intact.

---

## What Works on .NET Core

| Capability | Notes |
|---|---|
| SQL Server (System.Data.SqlClient) | Only fully supported ADO.NET provider |
| Basic CRUD operations | Works once properly wired up |
| Existing `DbContext` subclasses | Portable if kept in a net472 library |
| Code-First migrations (running) | Migrations can be *applied* at runtime |
| Transactions and stored procedures | Functional |
| Lazy loading / eager loading | Works |

---

## What Does NOT Work on .NET Core

| Feature | Status | Notes |
|---|---|---|
| EDMX visual designer | **Broken** | No tooling support in SDK-style projects |
| `Enable-Migrations` / `Add-Migration` | **Broken** | PMC/CLI commands fail in .NET Core-targeted projects |
| Non-SQL Server providers | **Unsupported** | No third-party providers have released .NET Core versions |
| SQL Server Compact | **Dead** | No ADO.NET provider exists for .NET Core |
| `DbContext` in a native .NET Core project | **Not supported** | See architectural constraint below |
| Scaffolding / reverse engineering | **Broken** | Designer tooling is .NET Framework only |

---

## The Critical Architectural Constraint

> **You cannot place an EF6 `DbContext` directly inside a project that targets `net6.0`, `net8.0`, or any other .NET (Core) TFM.**

Microsoft's supported pattern for EF6 + ASP.NET Core requires one of two approaches:

### Option A — Target net472 in the web project (not true .NET Core)

```xml
<TargetFramework>net472</TargetFramework>
```

- Run ASP.NET Core on .NET Framework (`net472`)
- Get Kestrel, middleware pipeline, DI container
- Do **not** get the .NET runtime performance gains (RyuJIT improvements, GC, Span, etc.)
- This is what many large orgs did as a first step

### Option B — Separate net472 class library for data access (hybrid runtime)

```
Rock.sln
├── RockWeb            → targets net8.0 (ASP.NET Core)
├── Rock               → targets net472 (DbContext, services, models)
└── Rock.Migrations    → targets net472 (migrations)
```

- ASP.NET Core web layer runs on .NET 8 runtime
- All EF6 / data access code lives in a net472 library
- Two runtimes coexist in one process — adds complexity
- You get Kestrel + middleware performance but not full runtime gains

---

## Implication for Rock RMS

Rock's `RockContext` is central to virtually every operation in the platform. The key considerations for any migration path:

### If the goal is "get off WebForms / get onto Kestrel first"

- Option B above is feasible
- Keep `Rock` (core models, `RockContext`, services) as net472
- Move `RockWeb` to ASP.NET Core / net8.0
- Defer EF Core migration to a later phase
- **Risk:** Two-phase migration compounds complexity; EF6 debt remains

### If the goal is "full .NET Core migration"

- EF6 is not viable as the long-term ORM
- Must plan EF Core migration as part of or immediately after the host migration
- EF Core has significant API differences from EF6 (see below)

---

## EF6 vs. EF Core — Key Differences to Plan Around

| Area | EF6 | EF Core |
|---|---|---|
| `DbContext` registration | Manual / static | DI-native (`AddDbContext`) |
| Lazy loading | Proxy-based (default on) | Opt-in (proxies or `ILazyLoader`) |
| Migrations | `Add-Migration` (PMC) | `dotnet ef migrations add` |
| Raw SQL | `Database.SqlQuery<T>()` | `FromSqlRaw()` / `ExecuteSqlRaw()` |
| Bulk operations | Not native (EF Extensions needed) | Native in EF Core 7+ (`ExecuteUpdate`, `ExecuteDelete`) |
| JSON columns | Not supported | Native in EF Core 8+ |
| Compiled queries | Limited | First-class support |
| Owned entity types | Limited | Full support |
| Table splitting | Limited | Full support |
| `ObjectContext` API | Available | Removed |
| `Database.ExecuteSqlCommand` | Available | Renamed / API changed |
| Interceptors | Limited | Robust interceptor pipeline |

---

## Microsoft's Official Position

From the [Microsoft EF Support Policies](https://learn.microsoft.com/en-us/ef/efcore-and-ef6/support):

> EF6 is **no longer in active development**. Only critical bug fixes and security patches are accepted.
> EF Core is the recommended data access technology for new applications.

From the [ASP.NET Core + EF6 guidance](https://learn.microsoft.com/en-us/aspnet/core/data/entity-framework-6):

> The .NET Core support in EF6.4 is a **migration aid**, not a long-term solution. Applications should plan to migrate to EF Core.

---

## Recommended Migration Strategy for Rock

Given the scale and centrality of `RockContext`, a phased approach is most realistic:

### Phase 1 — Host migration (ASP.NET Core, net472 TFM)
- Move `RockWeb` to ASP.NET Core pipeline on `net472`
- Keep EF6 entirely intact, no ORM changes
- Validates routing, middleware, DI wiring before touching data layer

### Phase 2 — Runtime lift (.NET 8 TFM, hybrid)
- Re-target `RockWeb` to `net8.0`
- Isolate `Rock` (models/services) in a net472 library (Option B above)
- Validates that the split-library model is stable

### Phase 3 — EF Core migration
- Migrate `RockContext` and all entity configurations to EF Core
- Update LINQ queries that use EF6-specific APIs
- Migrate or regenerate code-first migrations
- Enable DI-native `DbContext` registration

---

## References

- [ASP.NET Core and Entity Framework 6 — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/data/entity-framework-6)
- [EF6 vs. EF Core feature comparison — Microsoft Learn](https://learn.microsoft.com/en-us/ef/efcore-and-ef6/)
- [EF Support Policies — Microsoft Learn](https://learn.microsoft.com/en-us/ef/efcore-and-ef6/support)
- [Announcing EF 6.3 Preview with .NET Core Support — .NET Blog](https://devblogs.microsoft.com/dotnet/announcing-entity-framework-6-3-preview-with-net-core-support/)
- [Cross-Platform EF6 with .NET Core 3.0 — MSDN Magazine](https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/august/data-points-cross-platform-ef6-with-net-core-3-0)
