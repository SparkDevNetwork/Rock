# WebForms Blocks (`.ascx` / `.ascx.cs`, legacy)

Load when the code touches `RockWeb/Blocks/`, postback handlers, `HiddenField`, `ViewState`,
or grids.

## Client values on postback

- **Client input.** `HiddenField`, `CommandArgument`, `RowKeyId`, `RowKeyValue`, `data-*`,
  query string, `PageParameter()`, and picker values (pickers post through a `HiddenField`)
  are client input. They aren't trusted for security decisions.
- **ViewState.** Rock stores ViewState encrypted and signed with the site's `machineKey`, so
  the browser can't read or change it. It's a good place for values the server created (a
  transaction Guid, the selected person). But it isn't tied to a person, a page, or a time. A
  person can replay their own old ViewState, or post it to another page with the same controls,
  and it still works after their permissions change. A value from ViewState still needs the
  handler's authorization check.
- **Event validation covers list controls only.** ASP.NET rejects DropDownList,
  RadioButtonList, and CheckBoxList values the server didn't render. It doesn't cover pickers
  or text boxes, and it can't help if the option list was built without the security filter.
  Re-check picker values against the same filtered query that built the options.

## Handlers

- **No automatic checks before handlers.** Rock only checks block VIEW before a block loads.
  Each handler that takes an Id (Edit, Save, Delete, Cancel, Copy, grid rebind, KPI render)
  reloads the record through one shared helper that checks block EDIT (`UserCanEdit` /
  `IsUserAuthorized`) where the block needs it, plus `IsAuthorized` on the loaded record. For
  example: `GetAuthorizedDataView( service, id, Authorization.EDIT )` in DataViewDetail. Never
  reload by the raw hidden field value.
- **Copy permissions only when new.** Check EDIT on the saved record before applying posted
  values. Copy permissions from a source only when the record is new.
- **Ids belong to the page.** An Id from a grid row or hidden field must belong to what the
  block was rendered for. For example: `groupMember.GroupId == _group.Id` (GroupMemberList),
  `device.PersonAlias.PersonId == _person.Id` (PersonalDevices). When a block setting already
  holds the Id, read the setting, not a hidden field copy. A parent picked in a picker must be
  the same entity type.

## Output

- **Encoding.** `Literal`, `Label`, `RockLiteral`, `InnerHtml`, and HTML built in a
  `StringBuilder` render raw. User text needs `.EncodeHtml()`, or a Literal with
  `Mode="Encode"`. `RockBoundField` encodes by default, so `HtmlEncode="false"` on a column is
  a finding unless the value was encoded or cleaned first.
- **Script.** Values from the request, a page parameter, or a person go into
  `RegisterStartupScript` or `RegisterClientScriptBlock` only through `ToJson()` or
  `EscapeQuotes()`. Never paste them into a script string.
- **Redirects.** A redirect to `ReturnUrl` or any client URL uses `SiteCache.IsSafeRedirectUrl`
  (see the index). `Response.Redirect( Server.UrlDecode( PageParameter( "ReturnUrl" ) ) )` with
  no check is a finding.
