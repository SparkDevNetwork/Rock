# Detail Block Patterns

Patterns specific to Detail block conversions. Load this file when the block is classified as **Detail**.

---

## Entity Attributes

Attributes must be explicitly loaded before reading or writing.

**For view:**
```csharp
entity.LoadAttributes( RockContext );
bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );
```

**For edit:**
```csharp
entity.LoadAttributes( RockContext );
bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );
```

**Saving (inside `UpdateEntityFromBox`):**
```csharp
box.IfValidProperty( nameof( box.Bag.AttributeValues ),
    () =>
    {
        entity.LoadAttributes( RockContext );
        entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
    } );
```
After `SaveChanges()`: `entity.SaveAttributeValues( RockContext );`

The bag must declare:
```csharp
public Dictionary<string, string> AttributeValues { get; set; }
public Dictionary<string, PublicAttributeBag> Attributes { get; set; }
```

---

## ValidPropertiesBox

Required pattern for all detail block `Save` actions. Wraps an entity bag and tracks which properties were actually changed on the client.

In `UpdateEntityFromBox`:
```csharp
protected override void UpdateEntityFromBox( SomeEntity entity, ValidPropertiesBox<SomeEntityBag> box )
{
    if ( box.ValidProperties == null )
    {
        return;
    }

    box.IfValidProperty( nameof( box.Bag.Name ),
        () => entity.Name = box.Bag.Name );

    box.IfValidProperty( nameof( box.Bag.IsActive ),
        () => entity.IsActive = box.Bag.IsActive );
}
```

In `editPanel.partial.obs`, every editable field is declared with `propertyRef` (not plain `ref`) and emits a new `ValidPropertiesBox` when any value changes. **Reference:** any detail block's `editPanel.partial.obs`.

---

## IBreadCrumbBlock

Implement on detail blocks that contribute a breadcrumb. Uses `new RockContext()` (one of the few legitimate exceptions):

```csharp
public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
{
    var key = pageReference.GetPageParameter( PageParameterKey.EntityId );
    var name = new SomeEntityService( RockContext ).GetSelect( key, e => e.Name );

    var pageParameters = new Dictionary<string, string>();
    if ( name != null )
    {
        pageParameters.Add( PageParameterKey.EntityId, key );
    }

    var breadCrumb = new BreadCrumbLink( name ?? "New Entity",
        new PageReference( pageReference.PageId, 0, pageParameters ) );

    return new BreadCrumbResult { BreadCrumbs = new List<IBreadCrumb> { breadCrumb } };
}
```

---

## SecurityGrantToken

Required for detail blocks where the frontend needs security-checked operations (e.g., attribute values):

```csharp
private string GetSecurityGrantToken( SomeEntity entity )
{
    var securityGrant = new Rock.Security.SecurityGrant();
    securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );
    return securityGrant.ToToken();
}

protected override string RenewSecurityGrantToken()
{
    var entity = GetInitialEntity();
    if ( entity == null ) { return null; }
    entity.LoadAttributes( RockContext );
    return GetSecurityGrantToken( entity );
}
```

---

## IsSystem Guard

```csharp
// In Delete
if ( entity.IsSystem )
{
    return ActionBadRequest( $"Cannot delete a system {SomeEntity.FriendlyTypeName}." );
}

// In UpdateEntityFromBox — restrict structural edits
if ( !entity.IsSystem )
{
    box.IfValidProperty( nameof( box.Bag.Name ), () => entity.Name = box.Bag.Name );
}
```

In the edit panel, show an informational `<NotificationBox>` and disable structural fields when `isSystem` is true.

---

## UI Layout — ContentSection and ContentStack

### Nesting hierarchy (strictly top-down)
```
<ContentSectionContainer>   ← manages sidebar nav, ordering
  <ContentSection>          ← collapsible named section
    <ContentStack>          ← labeled subsection within a section
      <!-- form controls -->
    </ContentStack>
  </ContentSection>
</ContentSectionContainer>
```

### ContentSection props
| Prop | Notes |
|---|---|
| `title` | Section heading. Omit for the first (light) section. |
| `icon` | Icon CSS class (e.g., `"ti ti-settings"`). |
| `light` | Always use on the **first section** (basic required fields). |
| `disableCollapse` | Use for sections with required actions. |
| `isCollapsed` | Start collapsed. Useful for `v-for` items. |

### ContentStack props
| Prop | Notes |
|---|---|
| `title` | Subsection label. |
| `description` | Short contextual description. |
| `headerLocation` | `"left"` produces two-column label/content layout (default, no prop needed). Use `"top"` when the stack contains a grid or many items. |

### Consistency rule
> If any `ContentSection` in a block uses `ContentStack` inside it, every `ContentSection` in that block must also wrap its content in a `ContentStack`.

### `headerLocation` guidance
The default value is `"left"` (no prop needed) — the stack title and description appear to the left, creating a two-column layout. Use `headerLocation="top"` when the stack contains a grid or many items.

**Reference:** `Rock.JavaScript.Obsidian.Blocks/src/Group/GroupTypeDetail/groupTypeAttributes.partial.obs` (top), `Rock.JavaScript.Obsidian.Blocks/src/Engagement/StepProgramDetail/editPanel.partial.obs` (default/omitted).

### Edit panel rules (when using ContentSectionContainer)
- First section is always `light` with no `title` — holds essential fields (name, active status).
- Every subsequent section has a `title` and an `icon`.
- `AttributeValuesContainer` goes inside its own dedicated `ContentSection`.

### View panel rule
- View panel uses `<ValueDetailList>` only — never ContentSection/ContentStack.

---

## Detail Block Frontend

The parent `.obs` file uses `useEntityDetailBlock`, `DetailBlockBox`, `ValidPropertiesBox`, and `DetailPanelMode`. It delegates rendering to `viewPanel.partial.obs` (read-only) and `editPanel.partial.obs` (editable form).

### Edit panel root element

Choose the root element based on the block's complexity:

- **`<fieldset>`** — Default for most detail blocks. Use when the block has a single logical group of fields (Name, Description, a few pickers, and an `AttributeValuesContainer`). This is the dominant pattern (~85% of blocks).
- **`<ContentSectionContainer>`** — Use when the block has **multiple distinct sections** that benefit from collapsible headings and sidebar navigation (e.g., a block with General, Attributes, Workflows, and Advanced Settings sections).

**When in doubt, use `<fieldset>`.** Only escalate to `ContentSectionContainer` if the WebForms block has clearly separate panels/tabs or the field count warrants sectioning.

### Key rules
- `viewPanel.partial.obs` uses `<ValueDetailList>` — do **not** use `ContentSection` / `ContentStack` in view mode.
- Every editable field uses `propertyRef`, not plain `ref`.
- Emit a new `ValidPropertiesBox` in a `watch([...propRefs])` watcher.

**Reference (fieldset):** `Rock.JavaScript.Obsidian.Blocks/src/Cms/LayoutDetail/editPanel.partial.obs`
**Reference (ContentSectionContainer):** `Rock.JavaScript.Obsidian.Blocks/src/Engagement/ConnectionTypeDetail/editPanel.partial.obs`
