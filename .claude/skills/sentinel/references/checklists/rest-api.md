# REST API (`Rock.Rest`)

Load when the code touches `Rock.Rest/`, API controllers, `[Authenticate]`, `[Secured]`, auth
headers, JWT, or CORS.

## Attributes and callers

- **Attributes are present.** Every new or changed endpoint has the right `[Authenticate]` and
  `[Secured]` attributes. Check this directly. Don't assume it was copied correctly from a
  nearby endpoint.
- **Anonymous callers.** `[Authenticate]` only identifies the caller. It never rejects anyone,
  and a missing or bad token means anonymous. `[Secured]` checks the REST action's own
  security, not the record. Many v2 Controls endpoints have no `[Secured]` because they rely on
  security grants. An endpoint that only makes sense for a signed-in person checks first. In
  v2: `if ( RockRequestContext.CurrentPerson == null ) { return Unauthorized(); }`. In v1:
  check `GetPerson() == null`.

## Records

- **Record-level authorization.** Custom endpoints call `IsAuthorized` on the loaded record.
  v1 base GETs only check the REST action. v2 CRUD checks the record unless the caller has
  `EXECUTE_UNRESTRICTED_*`. A person Id from the query string (`personId`, `personAliasId`) is
  checked against the current person or their rights.
- **Create (POST) paths.** The v1 base `Post` authorizes new records against their parent
  (`IsAuthorizedForNewModel`). Custom POSTs and v1 overrides that skip `CheckCanEdit` don't, so
  they follow the index "Create paths" rule.
- **Target records.** Endpoints that act on a target record (tagging, badges, notes) also check
  VIEW on that target.
- **OData `$expand` (v1).** v1 endpoints with `[EnableQuery]` let callers add `$expand`, which
  returns related records the REST action's security never looked at (for example
  `Members/Person` on a Group). Don't add `[EnableQuery]` to endpoints that return entities
  with sensitive navigation properties. If you must, set `AllowedQueryOptions` or
  `MaxExpansionDepth`.

## Tokens and transport

- **JWT trust.** JWT and token checks trust only issuers, keys, and audiences an admin
  configured. Never take the issuer, the metadata URL, or the key URL from the token itself.
  With no configured provider, token sign-in is off.
- **Malformed tokens.** Token parsing treats any malformed or unknown token as anonymous and
  never throws. Watch for null lookups (auth client, JWT config) and exception types that
  aren't caught.
- **CORS** goes through `EnableCorsFromOriginAttribute` and the REST API Allowed Domains list.
  Never echo back the request's Origin, and never use `*` when credentials are allowed.
- **Errors.** Error responses don't return `ex.Message` or SQL errors to the caller. Log the
  error and return a generic message.
- **No GET changes data.** Browsers send the Rock cookie on cross-site GETs, so a link or image
  tag can trigger a GET.
