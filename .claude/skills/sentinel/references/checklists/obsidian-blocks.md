# Obsidian Blocks (Vue 3 + C# `RockBlockType`)

Load when the code touches `Rock.Blocks/` (including `Rock/Blocks/Types/Mobile/`),
`Rock.JavaScript.Obsidian*/`, `[BlockAction]`, bags, or security grants.

## Block actions

- **Only page and block VIEW run first.** The framework checks page VIEW and block VIEW, EDIT,
  or ADMINISTRATE (`BlockActionsController.cs`), then runs the action. Each action checks the
  record and validates every parameter itself. Client-side checks don't count.
- **Actions can be called with GET.** Any block action can be called with GET and query-string
  parameters. The auth cookie is `SameSite=Lax`, so a link on another site can trigger a
  state-changing action with simple parameters.
- **Block EDIT is not record EDIT.** When the record is secured, save and delete actions check
  the record (or its parent). Use block security only when the record has none, or when the
  WebForms block used block security on purpose.
- **Admin-only actions.** Actions behind an admin-only UI, such as the settings modal from
  `IHasCustomActions`, check `BlockCache.IsAuthorized( Authorization.ADMINISTRATE, ... )`
  themselves. That includes the action that reads the settings. The framework hides the button
  but doesn't gate the action.
- **Action context comes from the client.** In a block action, `PageParameter()` returns what
  the client posted in `__context.PageParameters`, not the page URL. The framework doesn't
  check that the block sits on the page the client named. Re-check any record loaded from a
  page parameter, even if the initial render already checked it.
- **Captcha.** A public block that shows a captcha checks `RequestContext.IsCaptchaValid`
  inside the submit action. Showing the widget does nothing on its own.

## Bags

- **Bags don't include fields the person can't see.** Never serialize a whole entity. The
  attribute helpers (`GetPublicAttributesForView`, `GetPublicAttributeValuesForEdit`,
  `SetPublicAttributeValues`) filter by attribute VIEW or EDIT by default. Flag
  `enforceSecurity: false` and attribute lists built by hand, because then the list is the only
  gate. Filter it by attribute VIEW and re-check it on save.
- **`ValidPropertiesBox` is chosen by the client.** The client decides which properties are in
  it, so a field the UI shows as read-only can still be posted. Skip or re-check such fields on
  the server. `IfValidProperty` alone is not a check.
- **Posted attribute definitions** (for example group member attributes edited on Group Detail)
  go through `PublicAttributeHelper.AreAttributeEditsAllowed`, so a posted Guid can't take over
  an unrelated attribute. Pass `null` as the qualifier value when the owning record is new.
- **Related records.** Never delete, replace, or re-qualify a related record (a DataViewFilter,
  an attribute definition) based on an Id from the bag. Use the block's stored settings, or
  confirm the record is new or already belongs to this record.

## Rendering

- **`v-html` needs server-side cleaning.** `v-html` is fine for admin-authored Lava output.
  Anything else rendered with `v-html` (text a person entered, attribute values, content from
  outside services such as the Rock Store) is cleaned on the server first with
  `SanitizeHtml( strict: false )` or encoded. Obsidian has no client-side sanitizer. The
  default `strict: true` strips all markup. Match sibling blocks that show the same data.

## Security grants

- **Grants go to anyone who can view the block.** `RenewSecurityGrantToken` is a public block
  action. Treat the initial grant and every `RenewSecurityGrantToken()` override like any
  block action: add only the rules the current person has earned. Powerful rules such as
  `AssetAndFileManagerSecurityGrantRule` or `AddDefinedValueToTypeGrantRule` go in only after
  the same EDIT check the editor needs, in both places.
- **Attribute rules.** Use `AddRulesForAttributes( entity, currentPerson )`, which filters to
  attributes the person can EDIT. The `IEnumerable<AttributeCache>` overload adds rules for
  every attribute passed, so filter that list first.
