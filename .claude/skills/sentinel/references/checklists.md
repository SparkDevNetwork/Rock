# Sentinel Checklists

This file is the index. Read all of it on every review. It holds the checks that apply to all
code, then a table of area checklists. Read an area checklist only when the code under review
matches its "Load when" column. Don't force a checklist onto code it doesn't fit.

Most recent Rock security fixes share one root cause: a value the browser sent back was
treated as a server-side fact. Hidden fields, button arguments, dropdown values, page
parameters, and Ids inside bags are all client input, even when the server put them on the
page.

## Framework facts

These are true of Rock today. Use them to decide whether a check is needed.

- **No automatic record check.** Block actions only require page VIEW and block VIEW, EDIT, or
  ADMINISTRATE (`BlockActionsController.cs`). WebForms only requires block VIEW before a block
  loads (`RockPage.cs`). Nothing checks the record, the person's ownership, or block EDIT
  before an action or postback handler runs.
- **VIEW is allowed by default.** When no rule matches, `Model.IsAllowedByDefault` allows VIEW
  and TAG for everyone, including anonymous visitors. EDIT and ADMINISTRATE default to deny. A
  VIEW check alone passes for anyone on a record with no rules.
- **Parent security is inherited automatically.** `IsAuthorized` falls back to the parent
  through `ParentAuthority`. It reads only the parent's `Auth` rows, not the parent's
  `IsAuthorized` override.
- **Plain integer Ids are accepted by default.** `GetInitialEntity` and
  `Service.Get( key, allowIntegerIdentifier )` accept plain Ids unless the site turns on
  "Disable Predictable Ids" (off by default).

## Always check (every review)

- **Authorization on the record.** Any Id, IdKey, or Guid from the client is checked with
  `IsAuthorized( Authorization.<ACTION>, currentPerson )` on the loaded record before it is
  returned or changed. Call it on the record itself; it already falls back to the parent.
  Checking that the record exists, or that the person is signed in, is not enough. Report a
  miss as "Person does not have authorization on the entity."
- **An IdKey is not authorization.** IdKeys make Ids hard to guess. Anyone who has one can
  still send it. The same goes for Guids.
- **New code takes IdKeys, not plain Ids.** A new block, action, or endpoint loads records with
  `Get( key, false )` (IdKey or Guid only), so plain integer Ids are never accepted. Existing
  code that already accepts plain Ids keeps doing so for backward compatibility, using
  `Get( key, !PageCache.Layout.Site.DisablePredictableIds )`. Flag a new path that accepts
  plain Ids, and flag a change that removes plain Id support from an existing one.
- **The record belongs to the page's scope.** After loading a record from a client key,
  confirm it is one the block would actually show: its parent, type, template, site, or
  context entity matches (`definedValue.DefinedTypeId == _definedType.Id`,
  `occurrence.GroupId == group.Id`). Prefer reusing the list's own `GetListQueryable` over a
  hand-written check. Child records such as DefinedValue, SmsAction, PageRoute, and SiteDomain
  usually have no rules of their own and inherit from their parent. A check on the page's
  parent passes while a tampered key reaches a sibling under a different parent.
- **Posted choices must be among the options offered.** For every dropdown, picker, checkbox,
  or `ListItemBag` value (saved account, campus, schedule, SMS number, layout, program,
  business), rebuild the option list on the server with the same function and filters that
  built it, and reject anything not in it. The record existing, or the person being able to
  view it, is not enough.
- **Server-owned values don't round-trip through the client.** Values the server created or the
  person can't edit (transaction Guids, attendance Ids, recipient phone numbers or emails,
  filter Ids) are kept on the server or looked up again. Fields the person can't edit get a
  server value, never the posted one.
- **Redirects go only to allowed hosts.** A `ReturnUrl` or any other redirect target from the
  client is checked with `SiteCache.IsSafeRedirectUrl( url, requestUri )`, which honors the
  site's Allowed Redirect Domains. `RedirectUrlContainsXss` alone is not enough, because it
  allows any outside host.
- **Impersonation tokens are created on purpose only.** Reading `Person.ImpersonationParameter`,
  `EncryptedKey`, or `UrlEncodedKey`, or the `PersonTokenCreate` Lava filter, creates a new
  sign-in token for that person. Code that serializes a whole Person (bags, JSON, Lava debug,
  exports) skips these. Code that creates a token on purpose sets a short expiry and a usage
  limit and sends it only to that person.
- **Security fails closed.**
  - Options or flags that control security are on by default and opt out
    (`IsSecurityDisabled`), so a new caller can't skip security by forgetting a flag.
  - Security filters on `IQueryable` are assigned back (`q = q.Where( ... )`). A bare
    `query.Where( ... );` filters nothing.
  - Parsing a credential, token, cookie, or header treats malformed input as anonymous. It
    never throws. Watch for null lookups and exception types that aren't caught.
- **Errors don't leak data.** Personal or financial data, SQL, and file paths stay out of logs
  and exception messages. An unhandled exception in an Obsidian block action sends its
  innermost `Exception.Message` to the browser for any caller, so don't throw messages that
  include record data.
- **Secrets.** Secrets, API keys, and connection strings are not in code, migrations, or
  committed config. A real `machineKey` is never committed, and the one in the repo's
  `web.config` is never reused. Stored secrets use `EncryptedTextField` or
  `Encryption.EncryptString`. Secrets are never sent to the browser in bags or URLs.
- **Tokens and crypto.** Tokens, codes, and keys come from `RandomNumberGenerator` (for example
  `Encryption.GenerateEncryptionKey`), never `System.Random`. No MD5 or SHA1 for security, no
  hardcoded keys, no reused IVs. Use `Encryption.EncryptString` / `DecryptString` rather than
  new crypto code.
- **Deserialization.** Untrusted input is never deserialized with `TypeNameHandling` other than
  `None`, `BinaryFormatter`, `LosFormatter` or `ObjectStateFormatter` without
  `MachineKey.Unprotect`, or `JavaScriptSerializer` with a type resolver.
- **Packages.** New NuGet or npm packages have no known vulnerabilities and come from known
  sources.

## Entry-point checks (whenever the review reaches an entry point)

- **Every gate is repeated in the action behind it.** When the initial load, `ShowDetail`, or
  the UI hides a button, shows read-only mode, limits a modal to admins, or shows "not
  authorized," the matching block action or postback handler enforces the same condition. This
  includes Edit, Save, Delete, Cancel, Copy, and grid rebind handlers, block settings such as
  `ShowDeleteButton`, and read-only actions. Put the condition in one shared helper. Copy or
  new-from-template needs VIEW on the source.
- **The same operation from more than one entry point** (block action, REST endpoint, Lava
  command, workflow action, job, AI agent skill tool) checks the same authorization at every
  one. A check in the block that the REST endpoint is missing is a finding.
- **Collections are filtered per item.** Lists, picker options, filter choices, and aggregate
  counts or totals sent to the client leave out items the person can't VIEW. Checking the page
  or the parent is not enough. An action that takes one of those items' Ids re-applies the
  same filter helper.
- **Create paths are authorized.** Authorization runs when no existing record is found. A null
  reload must not skip the check. Evaluate a new record against the parent named by its posted
  foreign keys (a new GroupMember against its Group).
- **Service methods** that load or change records either take the current person and check
  authorization, or every caller checks it. Code trusted "because the block already checked"
  needs a caller-by-caller look.
- **Custom security actions.** Entities add custom actions by overriding `SupportedActions` on
  the model and its cache. `[SecurityAction]` only adds actions to block types and REST
  controllers. A block feature gated on a custom action (for example `VIEW_ALL` or `APPROVE`)
  checks `BlockCache.IsAuthorized( "<Action>", person )` inside the action too, not just on
  load.
- **Data leaves only through reviewed shapes.** Return a bag, not a `Rock.Model` entity. An
  entity serializes every property and loaded navigation property. Lava merge objects in
  emails, exports, and logs get the same review.
- **Jobs and workflow actions** run without a signed-in person, so `IsAuthorized` with a null
  person still allows VIEW by default. Values they read from forms or webhooks are client
  input. Actions that create files or records set the same parent links as the interactive
  path.
- **Webhooks** are anonymous. They verify the provider's signature, and a signature check that
  is off by default is noted.
- **RealTime** has no hub-level security. Every `AddToChannelAsync` call checks VIEW on the
  record behind the channel.
- **AI agent skill tools** (`Rock.AI.Agent/Skills/`) check the record with the request's
  current person, the same way the matching block does.
- **Public entry points first.** List what anonymous visitors can reach and review it first:
  blocks on pages where All Users can VIEW, REST actions granted to All Users, `.ashx` handlers
  and `RockWeb/Webhooks/`, RealTime topics, and Lava Application endpoints.
- **Fresh install.** With no rules, VIEW and TAG are allowed to everyone, including anonymous
  visitors. New data-holding entities ship explicit rules (see `migrations.md`).

## Places to look

Quick searches that found most recent fixes. A hit isn't a finding by itself; trace it.

- `hf[A-Z]\w*\.Value`, `CommandArgument`, `RowKeyId`, `RowKeyValue`, or `DataKeys[` read
  inside a click or postback handler.
- `.Get( key, !PageCache.Layout.Site.DisablePredictableIds )` or `Service.Get( id )` followed
  only by a null check.
- A `TryGetEntityForEditAction` override with no `IsAuthorized` call.
- `Delete( string key )` or `Save` in a `*List.cs` block that loads by key without
  `GetListQueryable`.
- `GetCustomSettings` or `SaveCustomSettings` without an ADMINISTRATE check.
- `DefinedValueCache.Get(` or `DefinedValueService` lookups with no `DefinedTypeId` compare.
- A bare `query.Where( ... );` on its own line.
- `enforceSecurity: false`, `securityenabled:'false'`, or any security flag that defaults off.
- An `override string RenewSecurityGrantToken` that adds rules without a permission check.
- A new `ParentAuthority` or `IsAuthorized` override without a matching change in the cache
  class.
- `Response.Redirect` or `NavigateToPage` fed by `ReturnUrl` without `IsSafeRedirectUrl`.
- `Path.Combine`, `MapPath`, `ZipArchive`, or `ExtractToDirectory` on a posted value.
- `AddToChannelAsync(`.

## Area checklists

Paths are relative to `.claude/skills/sentinel/references/checklists/`.

| File | Load when the code touches |
|---|---|
| `obsidian-blocks.md` | `Rock.Blocks/` (including `Rock/Blocks/Types/Mobile/`), `Rock.JavaScript.Obsidian*/`, `[BlockAction]`, bags, security grants |
| `webforms-blocks.md` | `RockWeb/Blocks/` `.ascx` / `.ascx.cs`, postback handlers, `HiddenField`, `ViewState`, grids |
| `rest-api.md` | `Rock.Rest/`, API controllers, `[Authenticate]`, `[Secured]`, auth headers, JWT, CORS |
| `lava-and-sql.md` | Lava templates, Lava commands, filters, or shortcodes, `ResolveMergeFields`, SQL, the `sql` command, RunSQL |
| `migrations.md` | `Rock.Migrations/`, `Rock/Plugin/HotFixes/`, data migrations, `Auth` rows |
| `security-model.md` | `ISecured`, `IsAuthorized` or `ParentAuthority` overrides, `*Cache` classes, `SecurityGrant`, security roles, the Security dialog, categories or other parent/child hierarchies |
| `self-service-and-finance.md` | Public or self-service blocks, person matching, person tokens, giving, saved accounts, scheduled transactions, pledges, assessments, reminders |
| `files-and-paths.md` | `BinaryFile`, uploads, downloads, file handlers, asset manager, file paths, zip or archive handling |

When Rock gains a new kind of entry point, add a file here and a row to this table.
