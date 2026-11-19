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

### Page Parameters Must Be Bounded

A page parameter (query string or route value) that selects which data a block shows or changes must be limited by something the visitor cannot control. Valid bounds are:

- **Block configuration.** An administrator picks the allowed values in the block settings, and the block rejects any page parameter value outside that list.
- **Authorization.** The block checks that the current person is authorized to view or edit the specific record the parameter selects.

An empty or unset block setting must **never** mean "allow any value". When the allowed list is empty, the block shows nothing (or a configuration warning to administrators), not every possible record.

**Why:** Unbounded parameters let anyone change an id in the URL, walk through values, and read data the page was never meant to expose.

```csharp
// Correct: the parameter is limited to values chosen in block settings.
var allowedGroupTypeGuids = GetAttributeValue( AttributeKey.GroupTypes ).SplitDelimitedValues().AsGuidList();
var groupType = GroupTypeCache.Get( PageParameter( PageParameterKey.GroupType ), !PageCache.Layout.Site.DisablePredictableIds );

if ( groupType == null || !allowedGroupTypeGuids.Contains( groupType.Guid ) )
{
    return;
}

// Incorrect: any value in the URL is accepted when the setting is empty.
var groupTypeId = GetAttributeValue( AttributeKey.GroupType ).AsIntegerOrNull()
    ?? PageParameter( PageParameterKey.GroupType ).AsIntegerOrNull();
```

Apply the same bound inside block actions. In an Obsidian block action, `PageParameter()` returns what the client posted, so re-check it there even if the initial render already did.

---

## Creating Linked Page URLs

```csharp
var pageParams = new Dictionary<string, string>();
pageParams.Add( PageParameterKey.PersonId, Person.Id.ToString() );
pageParams.Add( PageParameterKey.GroupId, group.Id.ToString() );
var url = LinkedPageUrl( AttributeKey.AttendancePageAttribute, pageParams );
```
