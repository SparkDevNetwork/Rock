# Security Model (inheritance, caches, grants, rules)

Load when the code touches `ISecured`, `IsAuthorized` or `ParentAuthority` overrides, `*Cache`
classes, `SecurityGrant`, security roles, the Security dialog, or categories and other
parent/child hierarchies.

## Inheritance

- **Cache classes mirror the entity.** If an entity overrides `ParentAuthority`,
  `ParentAuthorityPre`, `IsAuthorized`, or `IsAllowedByDefault`, its `{Entity}Cache` overrides
  them the same way. `ModelCache<T>.SetFromEntity()` copies `SupportedActions`, but not these
  overrides, because they are code, not data. When a diff touches either file, compare the two
  side by side, and treat an authorization check on a cache object as suspect until you have.
  Real misses: GroupCache (cea71bbb1e) and WorkflowTypeCache (83c0386122).
- **New child records** override `ParentAuthority` (on the model and the cache) to return
  their logical parent (for example an AI skill tool returns its skill).
- **Inheritance reads rules, not overrides.** The parent walk only reads the parent's `Auth`
  rows. It never runs the parent's `IsAuthorized` override, and it uses the child's
  `IsAllowedByDefault`, not the parent's. If a child should honor a parent's custom logic
  (owner shortcuts, group member role grants), the child's `IsAuthorized` calls the parent's
  `IsAuthorized` itself.
- **Call the record, not the static helper.** Call `entity.IsAuthorized( ... )`, not
  `Authorization.Authorized( entity, ... )`. The static call skips overrides such as group
  member role grants and personal tag owners.
- **Unsaved records.** Before calling `IsAuthorized` on an unsaved record, load the parent
  navigation property, not just the foreign key Id. Otherwise the check skips the parent and
  uses only the Global Default rules.

## Hierarchies

- **Parent changes.** When a save accepts a parent Id: reject a record that is its own parent
  or ancestor (call `IsValid` before `SaveChanges`, because saving doesn't run it for you),
  require EDIT on the new parent when it changes, check the parent is the same entity type, and
  ignore parent fields the edit screen doesn't show.
- **Cycles.** A self or cyclic parent breaks inheritance. On 19.6 and later, authorization
  gives up after 100 levels and falls to the default, which allows VIEW. On 18.6 and earlier it
  can recurse until the worker crashes. Other tree walks can still loop forever.

## Rules and roles

- **No rules on unsaved records.** Code that creates `Auth` rows or opens the security modal
  treats an unresolved or unsaved record as an error. It never falls back to the entity type
  (EntityId 0), which would change the default security for every record of that type. The
  Security button stays hidden until the item is saved.
- **Owner shortcuts** in `IsAuthorized` overrides (personal tags, notes) explicitly deny when
  `person` is null.
- **Group role grants beat Deny rules.** On groups, a GroupTypeRole with CanView, CanEdit,
  CanManageMembers, or CanTakeAttendance grants that action even when an `Auth` rule denies
  it. When a change touches group type roles, check that it doesn't quietly widen access.
- **Null-check roles.** `RoleCache.Get` returns null when a role is inactive or missing. Use
  `?.IsPersonInRole( ... ) == true` so that case counts as not in the role, instead of
  throwing.
- **Bulk changes flush the cache.** Code that changes security role members or `Auth` rows with
  bulk inserts, raw SQL, or `UpdateCacheSaveOptions` calls `RoleCache.FlushItem` and
  `Authorization.Clear()` (or `RefreshEntity`) afterward. Otherwise people keep old access
  until the cache expires.

## Security grants

- **Grants are bearer tokens.** A security grant isn't tied to a person. Anyone holding the
  token gets its rules until it expires (3 days by default). Issue a grant only for records the
  current person could already reach on that page, and keep its rules narrow.
- **Revocation** compares the token's created time, not its expiry, against the revocation
  cutoff, and fails closed. Walk the boolean through with a token issued before the cutoff.
  Real miss: 04871d2124.
- **Fallback action.** A grant used as a fallback checks the same action as the primary check.
  A TAG check falls back to `grant.IsAccessGranted( tag, TAG )`, not VIEW (723f8321ae).
