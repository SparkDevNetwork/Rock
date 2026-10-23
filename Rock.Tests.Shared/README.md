# Project Rock.Tests.Shared

# About
This project stores common data and reusable components that are shared amongst all of the Rock Tests projects.

The types of data stored in this project include:
* Constants that reference specific instances of well-known test data - for example, Guid identifiers of well-known entities that are referenced in testing.
* Base classes that provide common functionality for classes that define tests.
* Utility components that provide general functions for working with test data.

This project is referenced by the following projects:
* Rock.Tests
* Rock.Tests.Integration

# Mocked RockContext

`TestHelper.CreateScopedRockApp()` provides a mocked `RockContext` whose DbSets are backed by in-memory lists. The mock imitates a few Entity Framework behaviors so code under test sees the database the way it would in production.

## Seeded rows need an Id

Give every entity you seed a non-zero `Id`, the way a real database row always has one:

```csharp
rockContext.Set<Campus>().Add( new Campus
{
    Id = 1,
    Guid = Guid.NewGuid(),
    Name = "Main Campus"
} );
```

Queries against the mocked sets skip any entity with an `Id` of `0`, because that is how the mock recognizes a row that has been added but not yet saved. Real EF does not return pending rows from a query either, so code that adds an entity and then queries (for example, to find the next `Order` value) behaves the same as in production. Calling `SaveChanges()` assigns Ids to pending rows, after which they appear in queries.

A seeded row without an `Id` is invisible to the code under test, which usually shows up as a "not found" result or an empty list. The `MockData` factory methods assign Ids for you.

This applies to the sets created automatically by the mocked context. Sets configured explicitly with `SetupDbSet()` do not assign Ids on save, so they do not hide rows with an `Id` of `0`.

## Other EF behaviors to be aware of

* `AsNoTracking()` returns fresh shallow copies of the rows, as EF does. Changes made to those copies do not affect the seeded instances, and seeded instances you are holding in a test are not touched by code that reads through `AsNoTracking()` (cache loads use this path).
* `Remove()` and `RemoveRange()` match by reference first and then by key (properties marked `[Key]`, or `Id`), so removing a copy returned by `AsNoTracking()` removes the stored row.
* No save hooks run and no navigation properties are fixed up. If code under test reads a navigation property, set it explicitly when seeding.

