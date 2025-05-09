# The Prime Directive

> First and foremost, follow the established patterns in the existing codebase.

Don't invent your own patterns. If you have an alternative or newer pattern, bring it up to the DSD, who will bring it to the TA/TL to review.

## General Rules

1. Do not break backward compatibility unless instructed to do so.
1. Always use `RockDateTime` instead of `DateTime`.
    - When producing DateTime values make sure to convert them to international ISO 8601 format.
        1. `RockDateTime.ToString("s");`
        1. not `RockDateTime.ToString();`
1. Do not add optional parameters that change the signature of a `public` method. This will break code compiled using the previous assembly. Instead, add the new method signature with the original one intact. Keep in mind that plugins do not get re-compiled as often as core.
1. Regarding Booleans:
    1. Boolean property field names should always ask a question that is answered by the value. For example, a `bool` property to indicate if something is active should be named `IsActive` and not just `Active`. A `bool` property to indicate if a category field should be displayed should be named `IsCategoryFieldVisible` and not `ShowCategoryField`.
    1. For Obsidian components, the default value must be `false` — and the name should reflect a default value of `false` as the typical/normal setting. Examples:
        - If a panel is normally hidden, then the name would be `IsPanelShown` with the default value of `false`.
        - If a `foo` option is normally enabled, then the name would be `IsFooDisabled` with a default value of `false`.
        - If a `foo` option is normally disabled, then the name would be `IsFooEnabled` with a default value of `false`.
        - It is also OK for a property to use Has" instead of "Is" as the prefix when it reads better.
1. All Page Parameters (query string params) should be in Pascal Case (`AccountId`).
1. When needing to persist configuration (Data Views, Field Types, Etc.) do not use delimiters but instead use JSON.
1. Use `private static readonly` for strings that may change.
1. Use `const` only for constants – for things that will never change such as our `SystemGuid`, `SystemSetting`, Etc.
1. When writing `.Where()` expressions in blocks, instead consider adding reusable expressions on the service.
1. When writing a LINQ query that filters results against a list of elements (e.g., a query like `.Where( p => personNameList.Contains( p.FirstName )` and `personNameList` is a `List<string>`) make sure you're aware that this will result in a `WHERE IN` clause. Avoid situations where the number of elements causes the SQL query to become too complex. The upper bound of the number of elements you can include is not a hard and fast rule, but if it causes the size of the query itself to exceed the batch size limit it will result in an error ("The query processor ran out of internal resources and could not produce a query plan.")
    - Batch Size Limit is 65,536 * (network packet size, which defaults to 4k)
    - If your list of elements comes from another query, consider using the query as to the basis of your `Contains()` match before calling `.ToList()` on the query. The practical effect of this is that Entity Framework will construct a SQL subquery rather than a very large `WHERE IN` clause with hard-coded elements.
    - If you cannot use the method above, you will need to find another workaround to avoid hitting the batch size limit, like breaking the query into smaller batches and reassembling the results in memory. Be sure you consider the performance ramifications of this approach carefully.
1. When defining `FieldAttribute`s for a Block (or Component, Job, Etc.), declare them vertically and assign the properties of the field, and not the constructor parameters. Also, define the attribute keys as constants in an `AttributeKey` `private static class` within the class that the field attributes are on.
1. When adding large strings to "settings" attributes create an `Attribute Strings` region and define a `private const string` to use for the long string.
1. If you are breaking `FieldAttribute`s into multiple categories, define the categories as constants in an `AttributeCategory` `private static class`.
1. When your block uses page parameters, set those in a `PageParameterKey` `private static class`.
    - Favor using the simple entity string name (such as `Person`) when a page parameter can accept an `Id`, `IdKey` or `Guid`.
    - Retrieve the entity using the entity service layer method passing in the `Site.DisablePredictableIds` setting like this: `var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );`
1. When the block uses Person Preferences, set those in a `PersonPreferenceKey` `private static class`.
1. When accessing a page parameter use:
    - `PageParameter( PageParameterKey.Group )`
    - instead of `Request.Params[ “Group” ] or PageParameter( “Group” )`
1. When creating a link to a page use the `LinkedPageUrl()` method, like this:
    ```
    var pageParams = new Dictionary<string, string>();
    pageParams.Add( PageParameterKey.PersonId, Person.Id.ToString() );
    pageParams.Add( PageParameterKey.GroupId, group.Id.ToString() );
    var url = LinkedPageUrl( AttributeKey.AttendancePageAttribute, pageParams );
    ```
1. Business logic methods that work with the database must be made on the service (e.g. `GetImpliedGroup()`. If there is a clear case for having it in the model add it there by calling the service. These must be approved by Developer Discussion.
1. Always use braces – even for single line blocks (`for`, `if`/`else`, Etc.).
1. Early Outs Ifs – Avoid `if` statement nesting and use early returns in your methods.
1. Use variables to document the intent for complicated `if` conditions. For example:
    ```
    // This:
    if ( expirationMonth.Value < 1 ||
        expirationMonth.Value > 12 ||
        expirationYear <= DateTime.MinValue.Year ||
        expirationYear >= DateTime.MaxValue.Year )
    {
       // invalid month (or year)
       continue;
    }

    // Could be rewritten as this:
    var areMonthAndYearInvalid = expirationMonth.Value < 1 ||
        expirationMonth.Value > 12 ||
        expirationYear <= DateTime.MinValue.Year||
        expirationYear >= DateTime.MaxValue.Year;

    if ( areMonthAndYearInvalid )
    {
       continue;
    }
    ```
1. In general, use `var`. This is mostly for consistency (see The Prime Directive). It is OK to use the actual type name in some cases such as when the type cannot be inferred from the declaration.
1. Only deprecate methods when agreed upon by the technical lead. When doing so, use the standard `[Obsolete()]` and the custom `[RockObsolete( "X.Y" )]` where `X.Y` means the item is obsoleted as of Rock vX.Y (e.g. `18.0`).
1. When Obsoleting a method, add the reason why as an engineering note above the method.
1. Be mindful of which classes, properties, methods, Etc. you put `public` on. There are a lot of things that need to be `public` (RockWeb needs it, another DLL needs it, or a plugin developer would need it), but if possible, DO NOT make it `public`, allowing us to avoid the worry of breaking changes. There are many cases where `internal`, `protected` or `private` is the better choice.
1. Avoid using a `Guid` in a LINQ `.Where()` (e.g., `g.GroupType.Guid == someguid`, which has to do a `JOIN`) when you can use the `Id` (namely if it's a cached item) as in `g.GroupTypeId == theThingsId`.
1. Don't declare class variables on singletons. This is not thread-safe. Rock has several.
    - Anything that inherits from Rock.Extension.Component. There are lots of these in Rock! For example:
        1. Workflow Actions
        1. DataFilter
        1. DataSelect
        1. Financial Gateways
        1. Communication Mediums
        1. Communication Transports
        1. Badges
        1. SMS Actions
        1. Storage Providers
        1. And more!
    - FieldTypes - These are also singletons.
    - Cache types – These obviously have to have class variables, so be careful that they aren't updated outside of the normal flush logic, and treat them as `readonly`.
    - If something must be a class variable, there are some ways to avoid the problem.
        1. Make it a `const` or `readonly` (set the values in the constructor).
        1. Store it in `HttpContext.Current.Items` (see https://github.com/sparkdevnetwork/rock/commit/b6989f562843165b9b87b8757600003a03ee2a22 for examples).
        1. Pass things around in parameters.
        1. See some examples in https://github.com/sparkdevnetwork/rock/commit/d82ea9420a1efbd88eea1d0af4478371acac6e12.

## Naming Conventions

### C#

1. Use Pascal casing for class names.
1. Use Pascal casing for method names.
1. Use Camel casing for variables and method parameters.
1. Use the prefix "I" with Camel casing for interfaces ( Example: IEntity ).
1. Do not use Hungarian notation to name member variables, but instead prefix them with an underscore only.
1. Use meaningful, descriptive words to name variables. Do not use abbreviations.
1. Do not use single-character variable names like `i`, `n`, `s`, Etc. Use names like `index`. (One exception is `i` for iteration loops).

### TypeScript

1. Use Pascal casing for classes, interfaces, namespaces, types, and enum names.
1. Use Camel casing for function names, variables, and function parameters.
1. Do not prefix private properties or fields with the underscore "_".
1. Use meaningful, descriptive words to name variables. Do not use abbreviations.
1. Use Camel casing for filenames.

## Method Size

The size of a method is a common way to judge its complexity. Smaller methods are more likely to be reused and have been proven to reduce errors and increase efficiency.

We want to keep things simple, which is usually short.

Keep in mind SOLID principles. A simple method should do one thing and be unit testable. Dependency injection, extension methods, and service methods are some good ways to simplify the code.

If breaking up the method makes it harder to read or debug then it is better to leave it as one long method. Two examples of this would be switch statements with lots of cases or when a lot of variables would have to be passed between the smaller methods. In the case of the large number of variables consider encapsulating them into an object or moving them to the class level. Otherwise, the large method is the lesser of two evils.

## Commenting Code

Document every method (public, private and internal) and any code block/section that is non-intuitive/non-obvious or complex.

1. The documentation should explain why the code exists (its purpose).
1. Method comments should use proper English (upper case character, end with period, Etc.)
1. Comments should exist approximately every 50-100 lines or roughly within view of a page of code.

## Method Behavior Parameters ("Options")

When building a method that takes options (parameters) that change the behavior of the method, a POCO should be used instead of individual parameters.

The reason for this is that many methods have grown over the years to include additional parameters. Because we need to maintain backwards binary compatibility that means we need to keep the old method signatures around, thus resulting in multiple methods that just call the main method. It often also leaves us with a method that has 8 or more parameters that must be passed in order to get to the one parameter you actually want.

This is cumbersome in both the method implementation as well as when calling the method.

For example, suppose you have a method called `GetCampuses()` and need to pass it the following parameters that alter it's behavior:

- `includeInactive`
- `campusType`
- `campusStatus`

If these are all passed as parameters then we might have ended up with the following method signatures defined for our method:

```
GetCampuses();
GetCampuses( bool includeInactive );
GetCampuses( bool includeInactive, CampusType campusType );
GetCampuses( bool includeInactive, CampusType campusType, CampusStatus campusStatus );
```

Instead, we will pass a POCO that defines the options: `GetCampuses( CampusQueryOptions options = null )`:

```
class CampusQueryOptions
{
    public bool IncludeInactive { get; set; }
}
```

In our implementation of `GetCampuses()`, we would check if `options` is `null` and if so assign a new (empty) `CampusQueryOptions` value to it. Ideally, this should be taken from a static field somewhere to reduce the number of object creations.

However, the point is we can now add additional options to `CampusQueryOptions` without changing any method signatures or existing code. When adding new options a default value should be set in the POCO that maintains the desired existing functionality.

For example, suppose our initial `CampusQueryOptions` POCO didn't have `IncludeInactive` and the method was always including inactive campuses. If that was the desired intention then the initial value should be set to `true`. On the other hand, if the intention was that normally the method would not include inactive campuses, then the initial value could be `false`. Said another way, is the current implementation a bug or intended? Often, this decision should be made with the DSD and/or PO.

If the new option does not need to alter existing functionality then it can be simply defined with a default value that means as much. In our example above, when we add the `campusType` and `campusStatus` values, we would alter the POCO in a way to maintain that. In this case, by using nullable values:

```
class CampusQueryOptions
{
    public bool IncludeInactive { get; set; }
    public CampusType? CampusType { get; set; } = null;
    public CampusStatus? CampusStatus { get; set; } = null;
}
```

Note: The `= null`; is not necessary, but was added above to make the point that the value is `null` by default and would therefore be ignored in the `GetCampuses()` method.

Remember to document these new `CampusType` and `CampusStatus` properties properly. A comment that says "gets or sets the campus type" is not enough. Your comment should indicate how the query will change based on the value set: "Limits the results to only campuses that match the specified CampusType. If null, then all campus types will be included."

In this particular case, a better POCO would probably be one that takes multiple campus types and statuses. It is likely that at some point we will want to set "get all campuses that are either open or pending":

```
class CampusQueryOptions
{
    public bool IncludeInactive { get; set; }
    public List<CampusType> CampusTypes { get; set; } = null;
    public List<CampusStatus> CampusStatuses { get; set; } = null;
}
```

In this case, your documentation comment would be along the lines of "Limits the results to only campuses that match one of the specified values. If null or empty, then all campus types will be included."

Remember, that this new "options POCO" only applies to behavior parameters. Meaning, if one of the parameters to your `GetCampuses()` method is `RockContext`, that should not be included in the POCO. That parameter doesn't alter the behavior of the method, simply where it gets the data from. Likewise, if you instead had something like `FilterCampuses()` that took a list of campuses, you would define it as such:

```
FilterCampuses( List<Campus> campuses, CampusQueryOptions options );
```

As you can see, we can now use `CampusQueryOptions` in both the `GetCampuses()` method as well as the `FilterCampuses()` method. Sometimes this can't be done, but whenever possible you should try to implement the specific properties and functionality in a way that can be used beyond your one specific method.

This new "options POCO" style does not apply across the board to every method you create. Remember that your method name should clearly indicate the intent. So if your method is a laser focused implementation that is not intended to be changed, you don't need to use a POCO (unless you are going to have a ton of parameters right off the bat). An example of this would be something like:

```
/// <summary>
/// This method will retrieve all campuses that are currently active,
/// open and physical.
/// </summary>
GetAllCurrentPhysicalCampuses( bool includeInactive = false );
```

Three things to note.

1. Our method name clearly states what the method is doing by use of a few terms:
    - "All" - This implies everything with no additional filtering options available.
    - "Current" - This implies the campus is Open. We could have said "Open" in the method name, but that locks us into a specific status. i.e. what if we later added a "Temporary" status?
    - "Physical" - This implies that only Type = Physical will be returned.
1. Because of how specific the method name is, a developer that comes along later should think twice about altering the method to add some new feature.
1. The XML documentation clearly states the intent of what is returned. In this case we do use the word "open" but later that might change to "open or temporary".

## POCO Location

When dealing with methods/options POCOs that involve the `Rock.Model` namespace (that is things like `GroupType` and `GroupTypeService`) the following rules apply.

Place the file that contains the POCO in Rock/Model/[Domain]/[Entity]/Options/[PocoName].cs. Example: Rock/Model/Group/GroupType/Options/GroupFilterOptions.cs

Place the class in the namespace that matches the folder structure. Example: `namespace Rock.Model.Group.GroupType.Options { }`

A similar pattern should be used for anything outside the `Rock.Model` namespace, though the exact formula has not been set in stone just yet. For now, plan on using a child namespace of `Options` to whatever namespace your method is in to store the POCO.

## Namespaces

As a general rule, don't add a new namespace without approval from the DSD or PO.

The exception to this is adding a standard "model domain" where you can see that is already the pattern. For example, if `Rock.Something.Core`, `Rock.Something.CMS`, `Rock.Something.CRM` namespaces already exist, adding `Rock.Something.Workflow` is probably fine. However, do not add `Rock.Something.CoolNewFeature` without getting that approved. Once created, it's nearly impossible to rename these so we want to be sure it's a well-thought-out name that won't conflict with possible upcoming features.

In regards to `Rock.ViewModels` and `Rock.Enums` assembly, the following rules apply and should not be deviated from without approval from PO currently.

1. Do not add any classes, interfaces, or enums to the root of the namespace.
1. Your namespace should match one of the following patterns:
    - [Domain]
    - Blocks.[Domain].[BlockName]
    - Controls
    - Utility

For example, the following namespaces would be considered valid:
- `Rock.ViewModels.CMS`
- `Rock.Blocks.Core.`CampusDetail
- `Rock.ViewModels.Utility`
- `Rock.Enums.Core`

However, the following would not be valid:
- `Rock.ViewModels.GroupFinder`
- `Rock.Enums.Thinker`

## Locks: C# and/or SQL

If you run into a situation where you think you need a lock() in code, you must first consult with the DSD. In general, our philosophy is that locks should be very rare. It's really only necessary in cases where you cannot control the environment in which your code is being run (OS, filesystem, database, Etc.).

Due to the increasing nature of clustered/web-farm Rock environments, we are looking into database level locking strategy. Once we have a convention for locking at the database level, we will put that information here. For now, most cases should involve relying on the unique constraints of the table' definitions to prevent accidental insertion of the same thing twice (for example, a system setting with the same key).

## Block Settings

All new blocks should use the ‘Vertical' format for block settings, where the Properties of the FieldAttribute is assigned (not the constructor parameters)

## RockInternal Attribute

The `[RockInternal]` attribute is used to signify internal code within Rock. This attribute requires a first parameter of a version string that represents the Rock Version it was introduced in. An optional boolean parameter of `keepInternalForever` indicates whether it should stay internal forever (e.g. WebForms code needs to use it but plugins never should). In the future, a helper tool will be run before every major release that provides a list of all "internal" items that should now be made public based on this version number.

The attribute should be used in the follow 3 cases:

1. Code is for internal use only and will always be used for internal use only.
    - Code has no intention of ever becoming available to 3rd party plugins.
    - If the property needs to be accessed by code in RockWeb, it must be public and denoted with the `[RockInternal]` attribute alongside a specific version and the `true` value for the optional `keepInternalForever` parameter.
1. Code is currently for internal use only but should become public eventually.
    - Code for a new feature but the method names and parameters are not yet confirmed.
    - Feature is considered experimental.
    - The intention is that this code will eventually become public.
1. Code is public for plugins to use.
    - Once internal-use code becomes fully public, the `[RockInternal]` attribute is removed and the accessor is switched to `public`.