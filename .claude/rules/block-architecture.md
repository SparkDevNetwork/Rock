---
paths:
  - "Rock.Blocks/**"
  - "Rock.JavaScript.Obsidian.Blocks/**"
  - "RockWeb/Blocks/**"
  - "Rock.ViewModels/Blocks/**"
---

# Block Architecture

Patterns for Rock RMS block development. Loaded when working in block directories.

---

## Attribute and Key Declarations

- Declare `FieldAttribute`s vertically, assigning properties (not constructor parameters).
- Define attribute keys as constants in a nested `private static class AttributeKey`.
- Define page parameter keys in a `private static class PageParameterKey`.
- Define `AttributeCategory` constants in a nested `private static class AttributeCategory` if breaking attributes into categories.
- Define Person Preference keys in a `private static class PersonPreferenceKey`.

---

## Accessing Page Parameters

```csharp
// Correct
PageParameter( PageParameterKey.Group )

// Incorrect
Request.Params["Group"]
PageParameter( "Group" )
```

**Favor simple entity name** for page parameters that accept Id, IdKey, or Guid. Retrieve using:
```csharp
var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
```

---

## Creating Linked Page URLs

```csharp
var pageParams = new Dictionary<string, string>();
pageParams.Add( PageParameterKey.PersonId, Person.Id.ToString() );
pageParams.Add( PageParameterKey.GroupId, group.Id.ToString() );
var url = LinkedPageUrl( AttributeKey.AttendancePageAttribute, pageParams );
```
