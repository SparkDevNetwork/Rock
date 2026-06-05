---
spec: specs/260511-person-session-as-session-authority.md
date_created: 2026-05-20
status: draft
summary: >-
  Multi-phase implementation plan for the PersonSession spec. Each phase is a
  self-contained, commit-sized unit of work. The final phase enumerates the
  end-to-end manual test scenarios a human must validate before sign-off.
---

# PersonSession Implementation Plan

This is a hand-off document for the implementation agent. It is paired with `specs/260511-person-session-as-session-authority.md` (the "spec"). The plan does **not** restate design decisions; it sequences the spec into commit-sized phases and lists the verification each phase must pass.

When a phase references the spec, follow the spec exactly. When the spec is silent or ambiguous, ask before guessing.

---

## Guardrails

Apply to every phase.

- **Follow the spec, not your instincts.** Every architectural decision is settled in the spec — including ones recorded in "Considered but Rejected." Do not re-litigate. If something looks wrong, stop and ask.
- **Follow established Rock patterns** (CLAUDE.md "Prime Directive"). When the spec is silent on a code-level detail, mirror the closest existing precedent in the codebase rather than inventing one.
- **No public-signature breaks.** New behavior is added via new methods or new overloads. Existing public methods keep their signatures (CLAUDE.md "Critical Rules").
- **Copyright headers on every new file** (.claude/rules/code-conventions.md).
- **Tests live in the same phase as the code they cover.** Pick the cheapest flavor (plain unit > mocked-database > full integration) per the spec's "Test classification" subsection. Tag flavor on each test bullet when it is not obvious.
- **"Full integration" means the database, not the HTTP pipeline.** Rock.Tests can spin up a real MSSQL via Docker for `PreSave`/`PostSave` hooks, SQL upserts, and transactional behavior. It CANNOT host the WebForms pipeline — there is no `iisexpress.exe` harness pointed at a known-state test database, and building one is out of scope. Any behavior that lives in `Global.asax.cs` (or otherwise depends on the WebForms request lifecycle running for real) is validated by (a) unit/mocked-database tests against the underlying service method in `Rock.dll` and (b) the Phase 17 manual checklist. Phases that wire a Global.asax hook are intentionally thin shims (one or two service calls plus a principal write) so this trade-off is safe — but if a phase grows nontrivial logic in the shim, surface it back to the user rather than expanding tests.
- **`System.Web` is the enemy, with one explicit exception.** `PersonSession`, the enums, and all `PersonSessionService` methods except `UpgradeLegacyCookieForRequest` must not reference `System.Web`; HTTP / cookie work routes through `RockRequestContext`. The single accepted deviation is `UpgradeLegacyCookieForRequest` (Phase 6), which reads the `FormsIdentity` from `HttpContext.Current.User` directly because `RockRequestContext` does not expose a .NET `IIdentity` and growing that surface for short-lived bridge code is not justified. The whole body of that one method is wrapped in `#if WEBFORMS` with a `#else return null;` branch; `System.Web` types are fully qualified inline (`System.Web.HttpContext.Current`, `System.Web.Security.FormsIdentity`, etc.) — do NOT add `using System.Web;` at the file level. See Phase 6.
- **Per-phase scope hygiene.** Do exactly what the phase says. Resist the urge to fold in work from a later phase, even if it looks small. Each phase is sized to be reviewable and revertable on its own.
- **Build at the end of every phase.** A phase that leaves the solution unbuildable is incomplete. Use the `build` skill.
- **No surprise migrations.** A phase that needs an EF migration says so explicitly. Do not introduce migrations outside the phases that call for them.

---

## Phase 1 — Data model foundation

### Goal
Stand up the new entity, enums, and database structures. No behavior change yet; nothing reads or writes these yet outside their own invariant tests.

### Deliverables
- **Enums** — file path `Rock.Enums/Security/EnumName.cs`, namespace `Rock.Enums.Security`. Do NOT apply the `[EnumDomain]` attribute; that attribute is a legacy artifact from enums moved from a previous assembly and is not used on net-new enums.
  - `AuthenticationStrength` (`NotAuthenticated`, `Authenticated`, `Elevated`, `MultiFactor`).
  - `AuthenticationRequirement` (`Elevated`, `MultiFactor`).
  - `PersonSessionCreationSource` (`Unknown`, `Component`, `Impersonation`, `UserToken`, `ApiKey`, `Legacy`).
- **Entity** (`Rock/Model/Security/PersonSession/PersonSession.cs`):
  - Inherits `Rock.Data.Model<PersonSession>`, implements `IHasAdditionalSettings`.
  - All columns per the entity table in spec ("Design / Entity: `PersonSession`"). Note `InteractionDeviceTypeId` (FK, nullable, `WillCascadeOnDelete( false )`), NOT a raw `UserAgent` column.
  - `InactiveDateTime` is a private-set property; populated in `PreSave` when `IsActive` flips false. `IsActive` is true by default.
  - `[RockDomain( "Security" )]` on the entity class. Subsequent files for this model (Service, Options POCOs, view models, etc.) follow the same `Security` domain convention.
- **Configuration** (`Rock/Model/Security/PersonSession/PersonSessionConfiguration.cs`): FK definitions per the spec's cascade table; `Has(...)/WithMany(...)/HasForeignKey(...)/WillCascadeOnDelete( false )` for all FKs; `ON DELETE SET NULL` semantics for `UserLoginId`.
- **SystemGuid** (`Rock/SystemGuid/EntityType.cs` and any related places that catalog new entity types).
- **`InteractionSession.PersonSessionId`** column added (int FK, nullable, no cascade). Do NOT yet update the SQL upsert at `Rock/Model/Core/Interaction/InteractionService.cs:583` — that is Phase 9.
- **EF migration** (`Rock.Migrations/Migrations/`): one migration creates `PersonSession`, its indexes (at minimum `PersonAliasId`, `UserLoginId`, `IsActive`, `LastActivityDateTime`), and adds `InteractionSession.PersonSessionId`.

### Tests
- Plain unit: `IsActive` defaults to `true` on construction.
- Full integration (`PreSave` hook required): setting `IsActive = false` via the service path stamps `InactiveDateTime`; direct caller writes to `InactiveDateTime` are not possible (private setter, compile-time enforced).

### Verification
- `build` clean.
- Migration applies on a fresh database and on a database with seed data without errors.

### Spec references
- "Design / Entity: `PersonSession`"
- "Design / Enums"
- ".claude/rules/data-model.md" (cascade conventions, standard columns)

---

## Phase 2 — Service skeleton, strength evaluation, impersonation query helpers

### Goal
Establish `PersonSessionService` and the read-only helpers that downstream phases will lean on. No session creation, no cookie work.

### Deliverables
- **`Rock/Model/Security/PersonSession/PersonSessionService.cs`** with:
  - `private const int ElevatedWindowMinutes = 30;`
  - `private const int MultiFactorWindowMinutes = 60;`
  - `public static DateTime GetElevatedAuthenticationThreshold()` and `public static DateTime GetMultiFactorAuthenticationThreshold()` — both `static`. The entity's `GetAuthenticationStrength()` calls them without holding a service instance; EF callers compose `Where` clauses against them without allocating one either. Return threshold `DateTime`, not the raw int; see spec for rationale.
  - `public PersonSession GetImpersonatorSession( PersonSession session )` — reads the restore Guid from `AdditionalSettings` and loads the impersonator's prior `PersonSession` by Guid. Stays on the service because the lookup needs a `RockContext`.
  - `internal ImpersonationProcessResult` POCO with `IsRedirectRequired` and `Session` properties (init-only).
- **`PersonSession` entity methods:**
  - `public bool IsImpersonated()` — pure check of `CreationSource` (returns true for `Impersonation` or `UserToken`). Lives on the entity rather than the service because it needs no database collaborator; the fluent `session.IsImpersonated()` shape is also what every Pattern A call site wants.
  - `public AuthenticationStrength GetAuthenticationStrength()` — computes against the service's threshold methods. Strongest applicable wins.
- **Additional-settings POCOs** — `internal` POCOs in namespace `Rock.Security` (file path `Rock/Security/*.cs`). Per Rock's standard `IHasAdditionalSettings` convention, the JSON key for each POCO defaults to the type's full namespace+type name; do NOT configure an explicit key. Read and written via the existing `GetAdditionalSettings<T>()` / `GetAdditionalSettingsOrNull<T>()` / `SetAdditionalSettings<T>(...)` extension methods. Callers MUST NOT touch `AdditionalSettingsJson` directly anywhere in the codebase.
  - `PersonSessionAdminImpersonationSettings` — present on sessions with `CreationSource = Impersonation`. Properties:
    - `Guid ImpersonatorPersonSessionGuid` — the impersonator's prior `PersonSession.Guid`, the value `EndImpersonationAndRestore` reads to revert the session.
    - `Guid ImpersonatorInteractionSessionGuid` — the impersonator's prior `InteractionSession.Guid`, so `EndImpersonationAndRestore` can re-attach the admin's pre-impersonation activity trail. (The new `InteractionSession` created at impersonation start remains in the database as a historical row but is no longer the "current" session for the admin's browser after restore.)
    - Future companion resume state (a target-URL override, a started-at timestamp, etc.) can be added as additional properties without a schema change.
  - `PersonSessionUserTokenSettings` — present on sessions with `CreationSource = UserToken`. Properties: `Guid OriginatingPersonTokenGuid` (the source `PersonToken.Guid`, the value the per-request page-scope re-validation reads against the source `PersonToken` row). Required, not optional — `UserToken` sessions cannot function without this reference.
  - Both POCOs end in `Settings` so the type-name-as-key shape reads naturally in the persisted JSON. Use `internal` (not just `[RockInternal]`) so the compiler prevents plugin code from depending on these shapes — these are storage shapes for core, not part of the public API.

### Tests
- Plain unit: `GetAuthenticationStrength` for `IsActive = false` → `NotAuthenticated`. (The "null session → `NotAuthenticated`" case is not testable as an instance method; it lives at the caller layer via `RockRequestContext.PersonSession?.GetAuthenticationStrength() ?? NotAuthenticated` and is exercised through `MeetsRequirement` tests in Phase 7.)
- Plain unit: `Authenticated`, `Elevated`, `MultiFactor` mappings as enumerated in spec "Test Plan / Strength mapping."
- Plain unit (against the entity, no `RockContext` needed): `PersonSession.IsImpersonated()` returns false for `Component`, `Unknown`, `Legacy`, `ApiKey`; returns true for `Impersonation`, `UserToken`. Covers the spec "Test Plan / Impersonation: query helpers" assertions.
- Mocked-database (needs a `RockContext` to look up the restore reference): `PersonSessionService.GetImpersonatorSession` returns the prior session for a valid restore reference; null for `UserToken`, `Component`, etc.

### Verification
- `build` clean.
- New unit tests green.

### Spec references
- "Design / Method: `GetAuthenticationStrength()`"
- "Design / Service: `PersonSessionService`"
- "Design / Result types / `ImpersonationProcessResult`"

---

## Phase 3 — Session creation methods

### Goal
Implement every code path that builds a `PersonSession` row, including the central `PopulateNewSession` helper. No cookie work; no auth pipeline integration yet.

### Deliverables
- **Private `PopulateNewSession`** helper owns shared invariants (`IsActive = true`, audit columns, `IssuedDateTime` defaulting to `RockDateTime.Now`, `IsPersistent` default, etc.). Every Start/FindOrCreate method delegates to it.
- **Populate-but-don't-save methods** (caller commits): `StartComponentSession`, `StartImpersonationSession`, `StartUserTokenSession`. Signatures per spec "Design / Central creation path."
- **Find-or-create methods** (save when creating): `FindOrCreateApiKeySession`, `FindOrCreateLegacyUpgradeSession`. Each uses the upsert-with-unique-key pattern (mirror the precedent at `Rock/Model/Core/Interaction/InteractionService.cs:583`).
- **`EndImpersonationAndRestore`** (`internal`): admin-impersonation only; returns the impersonator session on success; null if restore reference is dangling (and marks current inactive); throws on non-`Impersonation` `CreationSource`.
- **`StartImpersonationSession`** must:
  - Copy `LastStepUpAuthenticationDateTime` and `LastMultiFactorAuthenticationDateTime` from the impersonator's prior session. (Null source values stay null on the new session — no "stamp to now" fallback.)
  - Stamp `PersonSessionAdminImpersonationSettings { ImpersonatorPersonSessionGuid = impersonatorSession.Guid, ImpersonatorInteractionSessionGuid = impersonatorInteractionSession.Guid }` onto the new session via `SetAdditionalSettings<T>(...)` before returning it. Both Guids are required; the caller is responsible for supplying them.
  - Signature picks up an additional `InteractionSession` (or its Guid) parameter so the caller passes in the impersonator's prior `InteractionSession` explicitly. Do not look it up internally — the caller (Phase 13) already has the value from `RockRequestContext`.
- **`StartUserTokenSession`** must:
  - Leave `LastStepUpAuthenticationDateTime` and `LastMultiFactorAuthenticationDateTime` **null**. This is the explicit divergence from today's `ProcessImpersonation`; do not stamp them.
  - Stamp `PersonSessionUserTokenSettings { OriginatingPersonTokenGuid = token.Guid }` onto the new session via `SetAdditionalSettings<T>(...)` before returning it.
- **`EndImpersonationAndRestore`** reads the impersonator's prior session and InteractionSession Guids via `session.GetAdditionalSettings<PersonSessionAdminImpersonationSettings>()` (or the equivalent existing Rock extension), looks up the `PersonSession` row, and re-attaches the prior `InteractionSession` so the admin's pre-impersonation activity trail resumes. Either Guid going dangling is treated the same way (return null, mark current inactive). It never reads `AdditionalSettingsJson` directly.
- **`GetImpersonatorSession`** uses the same `GetAdditionalSettings<PersonSessionAdminImpersonationSettings>` pattern.
- **InteractionDeviceType resolution.** On every `PopulateNewSession` call, resolve the request's User-Agent string (when available via `RockRequestContext` — do NOT touch `HttpContext` directly) to an `InteractionDeviceType` row using the standard find-or-create pattern that `InteractionService` already uses. Stamp `PersonSession.InteractionDeviceTypeId` from the result. Null UA leaves the FK null without error.
- **`PostSave` event hook.** Set up the seam where downstream events (new-device email, audit logging, anomaly detection) will fire from. This phase does NOT wire any concrete subscribers — just exposes the hook so later work can plug in without changing creation methods.

### Tests
- Full integration (`PreSave`/`PostSave` hooks): the `InactiveDateTime` invariant from Phase 1 is re-verified now that callers can go through `StartComponentSession` end-to-end.
- Full integration (upsert race): concurrent `FindOrCreateApiKeySession` for the same `UserLogin` results in exactly one `PersonSession` row.
- Mocked-database: admin-impersonation creation copies both recency timestamps from the impersonator's prior session.
- Mocked-database: admin-impersonation creation with null impersonator recency leaves the new session's recency null (no stamp-to-now).
- Mocked-database: `StartUserTokenSession` leaves both recency timestamps null.
- Mocked-database: `StartImpersonationSession` stamps `PersonSessionAdminImpersonationSettings` on the new session; round-tripping through `GetAdditionalSettings<PersonSessionAdminImpersonationSettings>` returns the impersonator's prior `PersonSession.Guid` AND prior `InteractionSession.Guid`.
- Mocked-database: `StartUserTokenSession` stamps `PersonSessionUserTokenSettings` on the new session; round-trip returns the originating `PersonToken.Guid`.
- Mocked-database: a session built by `StartComponentSession`, `StartImpersonationSession`, or `FindOrCreateApiKeySession` has the **opposite** POCO absent (e.g. a `Component` session has no `PersonSessionAdminImpersonationSettings`, no `PersonSessionUserTokenSettings`). Prevents accidental cross-pollination through `PopulateNewSession`.
- Mocked-database: `EndImpersonationAndRestore` returns null AND marks current inactive when restore reference is dangling.
- Mocked-database: `EndImpersonationAndRestore` throws on `UserToken` or `Component` `CreationSource`.
- Mocked-database: brand-new UA string creates exactly one `InteractionDeviceType` row under concurrent first-request creates.
- Mocked-database: existing UA reuses the existing `InteractionDeviceType` row.

### Verification
- `build` clean.
- New tests green.

### Spec references
- "Design / Central creation path"
- "Design / Entity: `PersonSession`" (for the `InteractionDeviceTypeId` semantics)

---

## Phase 4 — Cookie container

### Goal
Implement the encrypt-then-MAC cookie value, the internal decoder, the write seam (`SetAuthCookie`), and the read-side lifecycle seam (`ResolveSessionForRequest`) that the auth pipeline calls. The cookie can round-trip end-to-end after this phase. Phase 5 then wires it into `Global.asax.cs` as a thin shim.

### Deliverables
- **Plaintext payload** — minified JSON `{"v":1,"sid":"<guid>","iat":"<ISO 8601>"}`. Define a payload-version constant.
  - **Serialize with `System.Text.Json.JsonSerializer` directly against a small `internal` typed model.** Do NOT use Rock's `.ToJson()` / `.FromJsonOrNull<T>()` extension methods (those use Newtonsoft.Json, which carries assembly-version coupling, default serializer settings, and allocation overhead this payload does not need). The cookie payload is a black box owned end-to-end by `PersonSessionService`; nothing external touches it, so going direct to System.Text.Json is safe and is the intentional choice here.
- **`PersonSessionService.GetCookieValue( PersonSession )`** — pure encoder: builds the plaintext model, serializes via `System.Text.Json`, encrypts via `Rock.Security.Encryption.EncryptString`, returns the base64 string. MUST NOT touch `HttpContext`. No reissue decisions.
- **`PersonSessionService.SetAuthCookie( PersonSession, RockRequestContext )`** — pure write: calls `GetCookieValue`, writes the cookie through `RockRequestContext`. Browser-side `Expires` computed via the `MIN( PersonSession.ExpiresDateTime ?? MaxValue, RockDateTime.Now.Add( FormsAuthentication.Timeout ) )` formula for persistent sessions; no `Expires` for non-persistent. Does NOT decide whether to reissue — callers (typically `ResolveSessionForRequest`) make that decision.
- **`TryDecodeCookie( string cookieValue, out CookiePayload payload, out DecodeMetadata metadata )`** (internal) — performs the base64 + `"V2"` footer check, calls `Rock.Security.Encryption.DecryptString`, deserializes JSON via `System.Text.Json.JsonSerializer.Deserialize`, returns the parsed session Guid + iat + decryption metadata (which `DataEncryptionKey` / `OldDataEncryptionKey{n}` decrypted it, payload version). Returns `false` for non-new-format cookies, tamper, or any decode failure. `CookiePayload` and `DecodeMetadata` are `internal` POCOs in `Rock.Security` so the rest of the service can pass the values around without touching `HttpContext`.
- **`PersonSessionService.ResolveSessionForRequest( RockRequestContext context )`** — `public`, `[RockInternal( "20.0" )]`. The single read-side seam for cookie validation; the auth pipeline (Phase 5) calls this and nothing else. Steps:
  1. Read the `.ROCK` cookie value from `RockRequestContext`. If absent, return null.
  2. `TryDecodeCookie`. If it returns false (non-new-format, tampered, or otherwise undecodable), return null. (Legacy-format cookies are intentionally left alone for Phase 6.)
  3. Load the `PersonSession` by `payload.Sid`. If not found, `IsActive == false`, or `ExpiresDateTime` past, expire the cookie via `RockRequestContext` and return null.
  4. Kill-switch: read the `RejectAuthenticationCookiesIssuedBefore` setting from `Rock/Security/SecuritySettings.cs:123`. If `PersonSession.IssuedDateTime` precedes it, mark the session inactive (`PreSave` stamps `InactiveDateTime`), expire the cookie via `RockRequestContext`, return null.
  5. Reissue triggers (any one fires reissue):
     - Half-life check (`now - payload.Iat >= FormsAuthentication.Timeout / 2`).
     - `metadata.DecryptedWithOldKey == true` (cookie decrypted via an `OldDataEncryptionKey{n}` rather than the current `DataEncryptionKey`).
     - `payload.Version` older than the current payload-version constant.
     - On any fire, call `SetAuthCookie( session, context )`. Reissue MUST NOT change `PersonSession.IssuedDateTime`.
  6. Return the resolved `PersonSession`.
- **Deprecate** `Authorization.GetSimpleAuthCookie` as a thin wrapper around `GetCookieValue` (kept usable during the dual-reader window; full removal in a later phase). Mark `[Obsolete]` `[RockObsolete( "20.0" )]` with a message pointing at the new method.

### Tests
- Plain unit (`Encryption.EncryptString` / `DecryptString` + `System.Text.Json` round-trip): a payload with `iat = T` and `v = current` round-trips back to the same values.
- Plain unit: tampered cookie → `TryDecodeCookie` returns false (tag mismatch from `Encryption.DecryptString`).
- Plain unit: payload serialized via `System.Text.Json` does NOT include Newtonsoft-specific artifacts (e.g. no `$type` discriminator, no PascalCase override unless declared). Lock down the wire format so a future Newtonsoft reintroduction would visibly break the test.
- Mocked-database: cookie whose `iat` is younger than half-life → `ResolveSessionForRequest` returns the session AND no `Set-Cookie` on response.
- Mocked-database: cookie whose `iat` is older than half-life → `ResolveSessionForRequest` returns the session AND a fresh `Set-Cookie` with new `iat`, same `sid`, refreshed `Expires`.
- Mocked-database: cookie decrypted via `OldDataEncryptionKey{1}` → reissue regardless of age.
- Mocked-database: cookie with older payload `v` → reissue regardless of age.
- Plain unit: reissue does NOT mutate `PersonSession.IssuedDateTime`.
- Mocked-database: non-persistent session emits cookie without `Expires` attribute; reissue logic is a no-op.
- Mocked-database: `ResolveSessionForRequest` for a `PersonSession.IsActive == false` returns null and expires the cookie.
- Mocked-database: `ResolveSessionForRequest` for a session past `ExpiresDateTime` returns null and expires the cookie.
- Mocked-database: kill-switch fire (session `IssuedDateTime` precedes threshold) marks session inactive, expires cookie, returns null.
- Mocked-database: kill-switch fire on a session whose cookie was recently reissued still rejects (the comparison uses `PersonSession.IssuedDateTime`, not the cookie's `iat`).
- Mocked-database: `ResolveSessionForRequest` against a legacy-format cookie value returns null and leaves the cookie untouched on the response (no `Set-Cookie`, no expiry). Phase 6's `PostAuthenticateRequest` hook owns legacy.
- Mocked-database: `ResolveSessionForRequest` against an absent cookie returns null with no response mutation.

### Verification
- `build` clean.
- New tests green.

### Spec references
- "Design / New cookie format"
- "Design / Cookie reissue"
- "Design / Cookie container"

---

## Phase 5 — Auth pipeline integration (new format)

### Goal
Authenticate requests carrying a new-format cookie. Legacy cookies still flow through `FormsAuthenticationModule` (Phase 6 upgrades them). Both formats work side by side at the end of this phase.

### Deliverables
- **`Application_BeginRequest` hook** in `RockWeb/App_Code/Global.asax.cs`. The hook is a thin shim that is **strictly additive** to the pre-Phase-5 body: keep every existing call in its existing position, then append the new resolution path at the end. Final ordering:
  1. **Existing:** `Context.AddOrReplaceItem( "Request_Start_Time", ... )`.
  2. **Existing:** the `RejectAuthenticationCookiesIssuedBefore` block at `Global.asax.cs:582-604`, in place and unchanged. Runs unconditionally at the top so a bad legacy cookie is removed from `Request.Cookies` before any downstream code reads the request. For a new-format cookie its `FormsAuthentication.Decrypt` call is a harmless no-op (the new format is not a `FormsAuthenticationTicket` and the result is dropped). Add the sunset comment below to the existing block, but do NOT move it inside any conditional branch.
  3. **Existing:** `WebRequestHelper.SetThreadCultureFromRequest( ... )`.
  4. **Existing (already added in the prep commit):** `RockRequestContext.AttachToCurrentRequest( Context )`. The returned context is captured for the next step.
  5. **New in Phase 5:** call `new PersonSessionService( rockContext ).ResolveSessionForRequest( rockRequestContext )` inside a `using RockContext` block.
  6. **New in Phase 5:** on a non-null return AND `session.UserLogin != null`, set `HttpContext.User = new GenericPrincipal( new GenericIdentity( session.UserLogin.UserName ), null )` and call `rockRequestContext.SetCurrentUser( session.UserLogin )`. `FormsAuthenticationModule.OnEnter` then short-circuits at its `Context.User != null && IsAuthenticated` guard.
  7. **New in Phase 5:** wrap step 5 + step 6 in a defensive `try/catch` that logs and swallows so an infrastructure-level failure cannot 500 the whole pipeline. `ResolveSessionForRequest` already handles tampered / invalid cookies internally by returning null, so the catch should be reached rarely in practice.

  Sessions with `UserLogin == null` (future `Impersonation` and `UserToken` flows) are NOT reachable at the time Phase 5 lands and are intentionally left without a principal write by this shim; later phases define the principal shape for those cases.
- **Retain the existing `RejectAuthenticationCookiesIssuedBefore` block at `Global.asax.cs:582-604`** in place at the top of the handler (NOT inside a null-return branch — see ordering above). The block keeps its current behavior (compare the legacy `FormsAuthenticationTicket.IssueDate` against the threshold; expire the cookie on violation). Add this comment to the block:
  ```csharp
  // Legacy cookie kill-switch.
  //
  // Preserved at the top of the handler to match the original pre-Phase-5
  // ordering: a bad legacy cookie is removed from Request.Cookies BEFORE
  // any downstream code (thread culture, RockRequestContext snapshot,
  // ResolveSessionForRequest) reads the request, so the rest of the
  // pipeline sees a clean state.
  //
  // Remove this block when legacy cookie support is sunset alongside
  // FindOrCreateLegacyUpgradeSession (currently targeted around Rock v23).
  // New-format cookies are kill-switched inside
  // PersonSessionService.ResolveSessionForRequest below against
  // PersonSession.IssuedDateTime; for those, the Decrypt call here is a
  // harmless no-op (the new format is not a FormsAuthenticationTicket and
  // the Decrypt result is dropped).
  ```
- **All decode, validation, kill-switch, and reissue logic for new-format cookies lives in `ResolveSessionForRequest` (Phase 4).** The BeginRequest handler does NOT duplicate any of it. The setting source (`Rock/Security/SecuritySettings.cs:123`) is unchanged; both kill switches read the same value.
- **`RockRequestContext.SetCurrentUser( UserLogin )`** — new `public` method, marked `[RockInternal( "20.0", true )]`, in the existing `Request Lifecycle` region next to `AttachToCurrentRequest` / `DetachFromCurrentRequest`. Exists because the `CurrentUser` setter is `internal` and `RockWeb`'s assembly name is generated at runtime, so `[InternalsVisibleTo( "RockWeb" )]` is not viable. In-assembly callers (`RockPage`, `ServiceScopeHandler`) keep assigning the property directly; only the BeginRequest shim needs this new entry point.

### Tests
**No automated tests in this phase.** The `Application_BeginRequest` body is a thin shim (one `RockRequestContext` construction, one `ResolveSessionForRequest` call, one conditional principal write) and the WebForms request lifecycle cannot run inside the Rock.Tests harness — there is no `iisexpress.exe` test host pointed at a known-state database (see Guardrails). Coverage strategy:

- **Unit-level coverage** lives in Phase 4 (`ResolveSessionForRequest` exercises decode, validate, kill-switch, reissue against a mocked database and synthetic `RockRequestContext`). Every observable behavior the BeginRequest shim depends on is covered there.
- **Pipeline correctness** is validated by the Phase 17 manual checklist (login, kill-switch, tampered-cookie behavior, anonymous request, reissue at half-life) plus code review of the shim.
- **If the shim grows logic** beyond construct-context / call-service / set-principal, stop and surface it — that is a signal to either move logic into the service or to invest in a real HTTP harness, and the call belongs to the user, not the implementation agent.

### Verification
- `build` clean.
- Phase 4's `ResolveSessionForRequest` unit/mocked-database tests still pass (regression check; the shim must not have inadvertently changed any observable).
- Code review of the shim against this phase's deliverables — the body should be visibly tiny (read context, call service, set principal). If it isn't, see the test-coverage note above.

### Spec references
- "Design / Cookie container / Auth pipeline integration during the dual-reader window"
- "Touch-points to update / `RejectAuthenticationCookiesIssuedBefore`"

---

## Phase 6 — Legacy cookie upgrade

### Goal
Transparently upgrade legacy `FormsAuthenticationTicket` cookies to new-format `PersonSession` rows. Existing users do not get forced to re-log in.

### Deliverables
- **`PersonSessionService.UpgradeLegacyCookieForRequest( RockRequestContext context )`** — `public`, `[RockInternal( "20.0" )]`, marked `[Obsolete]` `[RockObsolete( "20.0" )]` from day one (bridge code with a v23 target removal). Thin shim that pulls the legacy ticket off `HttpContext` and delegates to the testable helper below. Body shape:
  ```csharp
  #if WEBFORMS
      var formsIdentity = System.Web.HttpContext.Current?.User?.Identity as System.Web.Security.FormsIdentity;
      if ( formsIdentity?.Ticket == null )
      {
          return null;
      }
      return UpgradeLegacyTicket( formsIdentity.Ticket, context );
  #else
      return null;
  #endif
  ```
  Notes:
  - Wrap the entire body in `#if WEBFORMS` ... `#else return null; #endif`. .NET Core has no `FormsAuthenticationTicket` to upgrade from.
  - **Fully qualify** every `System.Web` type inline (`System.Web.HttpContext.Current`, `System.Web.Security.FormsIdentity`). Do NOT add `using System.Web;` or `using System.Web.Security;` to the file — the System.Web exposure is intentionally contained to these two methods' bodies so the rest of the file stays clean. The Guardrails section calls this the one accepted deviation from "no System.Web in PersonSessionService."
- **`PersonSessionService.UpgradeLegacyTicket( System.Web.Security.FormsAuthenticationTicket ticket, RockRequestContext context )`** — `internal` helper that owns the real upgrade logic. Marked `[Obsolete]` `[RockObsolete( "20.0" )]` alongside its caller. Also wrapped in `#if WEBFORMS` (because `FormsAuthenticationTicket` itself is a System.Web type and the parameter type can only exist on the WebForms build). Steps:
  1. Parse the ticket's `UserData` JSON. If `IsImpersonated == true`, expire the cookie via `RockRequestContext`, return null. Do NOT create a `PersonSession`. (Impersonation cookies were always short-lived; silently upgrading them would extend impersonation past its intended lifetime.)
  2. If `ticket.IsPersistent == false`, expire the cookie via `RockRequestContext`, return null. Do NOT create a `PersonSession`. (The user unchecked "remember me" at login; the legacy cookie was a transient session cookie. Every `PersonSession` created here is stamped `IsPersistent = true`, so silently upgrading a transient ticket would promote it to a long-lived session and contradict the user's original choice. The recipient re-authenticates on the new format with whatever persistence they prefer at that point.)
  3. Resolve `UserLoginId` from the ticket's `Name`. If the `UserLogin` no longer exists, expire the cookie and return null.
  4. Call `FindOrCreateLegacyUpgradeSession( userLoginId, ticket.IssueDate )`. The composite key `(UserLoginId, IssuedDateTime, CreationSource = Legacy)` guarantees repeated legacy-cookie presentations resolve to the same row.
  5. Call `SetAuthCookie( upgradedSession, context )` to emit the new-format cookie. (The legacy upgrade always emits a fresh cookie; the Phase 4 reissue trigger logic is bypassed because the source cookie is not new-format.)
  6. Return the upgraded `PersonSession`.
- **Why the helper split?** The `FormsAuthenticationTicket` constructor is a plain type — tests can synthesize a ticket directly without booting `HttpContext`. Keeping the System.Web read inside the public shim and the real logic inside the helper means the existing Phase 6 mocked-database tests stay valid and runnable; only the trivial shim is untested (same trade-off Phase 5 made).
- **`Application_PostAuthenticateRequest` hook** in `Global.asax.cs` — thin shim:
  1. Resolve the current `RockRequestContext`.
  2. Call `personSessionService.UpgradeLegacyCookieForRequest( rockRequestContext )`.
  3. On non-null return, replace `Context.User` with an authenticated principal backed by the upgraded session's `PersonAlias`.
  4. On null return, do nothing — `FormsAuthenticationModule`'s `FormsIdentity` (if any) flows through unchanged, and downstream handlers see the request as forms-authenticated under the legacy ticket until the next request when the new cookie takes over. (This is the same observable behavior as today for the first post-upgrade request.)
- **Any additional internal helpers introduced for the legacy decode path** (e.g. helper methods for parsing the `FormsAuthenticationTicket.UserData` JSON shape) ship `internal` and `[Obsolete]` `[RockObsolete( "20.0" )]`, also under `#if WEBFORMS`. Engineering note on each explains the bridge purpose and the v23 target removal.

### Tests
All tests target `UpgradeLegacyTicket` directly (synthesize a `FormsAuthenticationTicket` in the test, no `HttpContext` needed). The public `UpgradeLegacyCookieForRequest` shim itself is untested for the same reason as Phase 5's BeginRequest shim. Each test in this section is `#if WEBFORMS`-only and skipped on .NET Core builds.

- Mocked-database: legacy ticket with `IsImpersonated = false` and `IsPersistent = true` upgrades to a new `PersonSession` (`CreationSource = Legacy`, `IssuedDateTime = ticket.IssueDate`).
- Mocked-database: second call with the same legacy ticket resolves to the existing row (no duplicate).
- Mocked-database: legacy ticket with `IsImpersonated = true` is dropped, no `PersonSession` is created, the cookie is expired via `RockRequestContext`, helper returns null.
- Mocked-database: legacy ticket with `IsPersistent = false` is dropped, no `PersonSession` is created, the cookie is expired via `RockRequestContext`, helper returns null. (Preserves the user's "remember me" off intent rather than silently promoting a transient ticket to a long-lived session.)
- Mocked-database: legacy ticket whose `Name` does not match any active `UserLogin` is dropped (cookie expired, helper returns null).
- Mocked-database: upgraded session reports `Authenticated` strength (recency timestamps null after upgrade).
- Mocked-database: kill-switch fires correctly on an upgraded session whose ticket `IssueDate` precedes the threshold.

### Verification
- `build` clean.

### Spec references
- "Design / Cookie upgrade path"
- "Design / Deprecations and removals / Legacy cookie upgrade seam"

---

## Phase 7 — `RockRequestContext` integration + Login block wiring

### Goal
Make the current session available to the rest of the platform. Wire the Obsidian Login block (and therefore Auth0 / external providers) to the new creation path.

### Deliverables
- **`RockRequestContext.PersonSession`** (nullable) — resolved once at request entry, cached for the duration of the request. Anonymous and bearer-token requests legitimately return null; consumers handle null.
- **`HttpContext.Items` fallback** for the rare call site not running inside `RockRequestContext` — same request lifetime.
- **`RockRequestContext.MeetsRequirement( AuthenticationRequirement )`** — `Elevated` is satisfied by `Elevated` or `MultiFactor` strength; `MultiFactor` only by `MultiFactor`.
- **Obsidian Login block** (`Rock.Blocks/Security/Login.cs:718-734`) — replace `Authorization.SetAuthCookie` call with `StartComponentSession` + add to rock context + save + `SetAuthCookie`. The Auth0 and other `IExternalRedirectAuthentication` flows route through this block and are covered automatically (no Auth0-specific change).
- **MFA recency stamping at login.** When the Login block authenticates via a component whose `AuthenticationComponent.IsConfiguredForTwoFactorAuthentication()` returns true, stamp `LastMultiFactorAuthenticationDateTime = RockDateTime.Now` on the new session. Always stamp `LastStepUpAuthenticationDateTime`.

### Tests
- Plain unit: `MeetsRequirement(Elevated)` is true for `Elevated` and `MultiFactor` strength; false otherwise.
- Plain unit: `MeetsRequirement(MultiFactor)` is true only for `MultiFactor` strength.
- Mocked-database: a request without a cookie has `RockRequestContext.PersonSession == null`.
- Mocked-database: a request with a valid cookie exposes the resolved `PersonSession` on context.
- Mocked-database: login via the Obsidian Login block produces a `Component` `PersonSession` with `LastStepUpAuthenticationDateTime = Now`.
- Mocked-database: login via an MFA-configured component also stamps `LastMultiFactorAuthenticationDateTime = Now`.

### Verification
- `build` clean.

### Spec references
- "Design / `RockRequestContext` integration"
- "Design / MFA detection"
- "Pre-Implementation Research / Other auth flows / Auth0 plugin auth flow"

---

## Phase 8 — Activity tracking bus task

### Goal
Replace `UpdateUserLastActivity` with `UpdatePersonSessionLastActivity`. Throttled writes; the old task is deprecated but kept for plugins during the transition.

### Deliverables
- **New bus task `UpdatePersonSessionLastActivity`** — updates `PersonSession.LastActivityDateTime`. Throttled to once per ~5 minutes per session.
- **Page request handler.** Wire the page-request path that today fires `UpdateUserLastActivity` to fire the new task against the current `PersonSession.Id`.
- **`AuthenticateAttribute` (API-key path)** — fire the new task after resolving the API-key session (full API-key wiring is Phase 10; here we just wire the activity hook so the new task has a clean home).
- **SignalR explicitly does NOT fire the new task** (long-lived hub connections must not generate excessive activity writes).
- **`UpdateUserLastActivity` bus task** marked `[Obsolete]` `[RockObsolete( "20.0" )]`. The `IsOnline` property on the task is deprecated alongside. Writers are NOT yet removed; Phase 15 handles that.

### Tests
- Mocked-database: bus task within throttle window is a no-op.
- Mocked-database: bus task past throttle window advances `LastActivityDateTime`.

### Verification
- `build` clean.

### Spec references
- "Design / Entity: `PersonSession`" (LastActivityDateTime semantics)
- "Design / SignalR real-time hubs"
- "Design / Deprecations and removals"

---

## Phase 9 — `InteractionSession` adoption

### Goal
Stamp / adopt `InteractionSession.PersonSessionId` correctly across login, logout, impersonation, and legacy upgrade events.

### Deliverables
- **SQL upsert update** at `Rock/Model/Core/Interaction/InteractionService.cs:583` — extend the existing insert-only upsert to also UPDATE `PersonSessionId` on an existing row keyed by `RockSessionId`. The unique key on `RockSessionId` continues to mediate the race; the new column rides along.
- **Stamp at creation.** When `InteractionSession` is created and a `PersonSession` already exists on the current request, insert with `PersonSessionId` populated.
- **Adopt by update at login.** Login flow updates the existing `InteractionSession` row for the current browser session to point at the new `PersonSession`. Mid-session login should retroactively attach the pre-auth journey.
- **Adopt by update at legacy upgrade.** Same path, triggered by `FindOrCreateLegacyUpgradeSession` (Phase 6).
- **Restore prior on admin-impersonation end.** When `EndImpersonationAndRestore` runs (Phase 3 / Phase 13), it reads `PersonSessionAdminImpersonationSettings.ImpersonatorInteractionSessionGuid` and re-attaches that historical `InteractionSession` as the current row for the admin's browser session. The mechanism is symmetric with the existing `RockSessionId`-keyed lookup: the browser-session identifier is re-pointed at the prior row, the impersonation-period row remains in the database but is no longer "current," and subsequent `InteractionSession` writes resume against the restored row. (If `RockSessionId` is managed via a cookie or comparable identifier rather than ASP.NET Session, restoring it is the implementation seam here; the spec deliberately avoids ASP.NET Session for this state.)
- **Reset on auth events** — refer to the spec's truth table for which event creates vs. adopts vs. reuses vs. restores `InteractionSession`.

### Tests
The behavior in this phase splits across two seams: (1) the **SQL upsert** at `InteractionService.cs:583` that handles concurrent INSERT/UPDATE for `InteractionSession` rows keyed by `RockSessionId`, and (2) the **auth-event handlers** (login, logout, impersonation start/end, legacy upgrade) that decide whether to keep or regenerate the browser-session identifier (`RockSessionId` or the comparable seam, see Deliverables) before the next interaction-tracking call. Mocked `RockContext` intercepts LINQ + `SaveChanges` but does NOT execute raw SQL, so any test whose assertion depends on the upsert running has to be Full integration. Auth-event-side decisions don't touch the upsert and can be mocked-db.

**Full integration (real SQL needed for the upsert):**
- SQL upsert race: two concurrent inserts for the same brand-new `RockSessionId` produce exactly one `InteractionSession` row; `PersonSessionId` reflects the authenticated request whether it arrived first or second.
- SQL upsert UPDATE path: an existing `InteractionSession` row with `PersonSessionId = null` and a known `RockSessionId` gets `PersonSessionId` set when an authenticated request presents that same `RockSessionId` (covers both "login adopts existing" and "legacy cookie upgrade adopts existing").
- SQL upsert INSERT path: a brand-new `RockSessionId` presented by an authenticated request inserts a new row with `PersonSessionId` already populated (covers "stamp at creation").
- SQL upsert UPDATE scope: the UPDATE only affects the row with the matching `RockSessionId`; other `InteractionSession` rows in the table are untouched. (Regression guard against an UPDATE that forgets its `WHERE` clause.)

**Mocked-database (auth-event handlers, no SQL upsert exercised):**
- Login when not already authenticated does NOT regenerate the browser-session identifier (so the next interaction-tracking call presents the existing `RockSessionId` and the SQL upsert's UPDATE path fires — verified by the Full integration test above).
- Login as a different person regenerates the browser-session identifier (so the next interaction-tracking call presents a fresh `RockSessionId` and the SQL upsert's INSERT path fires).
- Logout regenerates the browser-session identifier.
- Admin-impersonation start regenerates the browser-session identifier; the new value is independent of the impersonator's prior value.
- Legacy cookie upgrade does NOT regenerate the browser-session identifier (so the existing `InteractionSession` is adopted, mirroring the unauthenticated-→-login flow).
- Admin-impersonation end via `EndImpersonationAndRestore` re-points the browser-session identifier to the value stored in `PersonSessionAdminImpersonationSettings.ImpersonatorInteractionSessionGuid`. The impersonation-period `InteractionSession` row remains queryable in the database afterward (not deleted, not modified — this is a plain LINQ assertion against the mocked context).
- `EndImpersonationAndRestore` when the prior `InteractionSession` row referenced by `ImpersonatorInteractionSessionGuid` has been deleted: same dangling-reference handling as the `PersonSession` case — return null, mark current inactive, do not silently continue.

### Verification
- `build` clean.

### Spec references
- "Design / Interaction with `InteractionSession` and ASP.NET Session"

---

## Phase 10 — API key request integration

### Goal
API-key requests participate in `PersonSession` via long-lived `ApiKey`-source sessions. JWT and OAuth bearer tokens explicitly do NOT.

### Deliverables
- **`Rock.Rest/Filters/AuthenticateAttribute.cs`** — after resolving a `UserLogin` by `ApiKey` (via `Authorization-Token` header or `?apikey=` query parameter), call `PersonSessionService.FindOrCreateApiKeySession( userLogin )`. Attach the session to the current request.
- **JWT (`HeaderTokens.JWT`) and ASOS bearer paths**: leave alone. Add an explicit code comment on each branch stating the intentional non-participation, citing the spec's "API key requests" subsection.
- **OIDC password-grant flow** (`Rock.Oidc/Authorization/AuthorizationProvider.cs:120-182`) similarly leave alone with a code comment.
- **`UserLogin.LastLoginDateTime`** — preserved as-is (set on credential entry / impersonation). No change.

### Tests
- Full integration (upsert race): concurrent first-API-key requests for the same `UserLogin` produce exactly one `ApiKey` `PersonSession`.
- Mocked-database: second API-key request reuses the existing session.
- Mocked-database: an API-key request whose `UserLogin` was deleted authenticates as unauthenticated; the orphan is not resurrected.
- Mocked-database: JWT request creates no `PersonSession`.
- Mocked-database: ASOS bearer request creates no `PersonSession`.

### Verification
- `build` clean.

### Spec references
- "Design / API key requests"

---

## Phase 11 — Mobile and TV device authentication

### Goal
Mobile and TV logins produce full persistent `Component` sessions. Subsequent requests with `Cookie: .ROCK=...` flow through the same auth pipeline as browsers (no special branch).

### Deliverables
- **Mobile login block** (`Rock/Mobile/MobileHelper.cs:206` and related): switch to `StartComponentSession( ..., isPersistent: true, mfaRecency: ... )` + save + `GetCookieValue( newSession )`. Return the opaque cookie value in the response body. Do NOT call `SetAuthCookie` (the device manages cookie storage manually).
- **TV login block** (`Rock/Tv/TvHelper.cs:193` and related): same pattern.
- **Device-token-refresh case**: same-person re-login reuses the existing active `PersonSession`; `GetCookieValue` is called against it to produce a fresh opaque value.
- **`Authorization.GetSimpleAuthCookie`** (`Rock/Security/Authorization.cs:853`) — make it a thin wrapper around `GetCookieValue` during the dual-reader window, marked `[Obsolete]` `[RockObsolete( "20.0" )]`.
- Confirm the existing `AuthenticateAttribute.cs:215-219` short-circuit still applies (principal already set by the cookie validation path from Phase 5).

### Tests
- Mocked-database: mobile login creates `Component` `PersonSession` with `IsPersistent = true`.
- Mocked-database: TV login creates `Component` `PersonSession` with `IsPersistent = true`.
- Plain unit: `GetCookieValue` returns a non-empty opaque string with no `HttpContext` access (use a test harness without `HttpContext.Current`).
- Mocked-database: device-token-refresh for the same person reuses the existing session.
- Mocked-database: mobile login as a different person on a device that already had a session creates a new session AND marks the prior session inactive.
- *Note:* the "cookie value sent back as `Cookie: .ROCK=...` resolves correctly" assertion is NOT a Phase 11 test. It is already covered by Phase 4 (`ResolveSessionForRequest` accepts the opaque cookie string regardless of who issued it; mobile/TV cookies are byte-identical to browser cookies after `GetCookieValue` returns) and Phase 17 manual testing (real device → real server). Adding a duplicate here would either be redundant with Phase 4 or would require the HTTP pipeline harness called out in Guardrails.

### Verification
- `build` clean.

### Spec references
- "Design / Mobile and TV device authentication"

---

## Phase 12 — User-token impersonation (`ProcessImpersonationToken`)

### Goal
Single Pattern B seam for every code path that inspects `rckipid` on an incoming request. The Person Bio block (admin impersonation) is NOT a Pattern B caller after Phase 13; this phase handles user-token (email link) flows.

### Deliverables
- **`PersonSessionService.ProcessImpersonationToken( string rckipidToken )`** — internal. Implements the five-rule matrix in the spec. Returns `ImpersonationProcessResult`. Sets `redirectRequired = true` for every rule including the invalid-token failure case.
- **`PersonToken.TimesUsed` increment** only when `rckipid` is present in the query string AND differs from the token referenced by the current session (per spec).
- **Page-scope re-validation hook**: on every request while a `UserToken` session is active, re-validate the referenced `PersonToken`'s page-scope, expiration, and revocation. Fail closed (mark session inactive on expiry/revocation; not-authorized on page-scope miss).
- **Pattern B callers migrate**:
  - `Rock/Web/UI/RockPage.cs:2111` (`ProcessImpersonation`) — single highest-priority migration target. Convert to call `ProcessImpersonationToken`.
  - `Rock.Rest/ApiControllerBase.cs:103` — `rckipid=` prefix parsing moves to `ProcessImpersonationToken`.
  - `Rock/Web/HttpModules/RockGateway.cs:499` — same.
  - `UserLogin.WebForms.cs` helpers that parse `"rckipid=" + token` out of cookies — same.

### Tests
The full matrix from spec "Test Plan / Impersonation: `ProcessImpersonationToken` matrix" — 11 rows. All mocked-database. Each row asserts: resulting `PersonSession` state, redirect-required flag, and (where applicable) `PersonToken.TimesUsed` advancement or non-advancement.

Plus:
- Mocked-database: per-request page-scope re-validation fails when a `UserToken` session navigates to a page outside the token's scope; user receives not-authorized.
- Mocked-database: per-request re-validation marks session inactive when the source `PersonToken` is revoked between requests.

### Verification
- `build` clean.

### Spec references
- "Design / Impersonation: two distinct cases / Pattern A vs Pattern B (callers)"
- "Test Plan / Impersonation: `ProcessImpersonationToken` matrix"

---

## Phase 13 — Admin impersonation rewrite

### Goal
Person Bio block hands off impersonation server-side via the new `PersonSession` row plus reissued cookie. No `PersonToken` is involved.

### Deliverables
- **`PersonSessionService.ImpersonatePerson( RockRequestContext context, int targetPersonAliasId )`** — `internal static`, returns `void`. The full server-side orchestration of an admin-impersonation handoff. Steps:
  1. Read the admin's current `PersonSession` AND `InteractionSession` from `RockRequestContext` (both are needed for restore). If the request has no active `PersonSession` (e.g., the admin's session expired between rendering the Impersonate button and clicking it), **throw `InvalidOperationException`** — the operation is not valid in the current request state. The caller (the Bio block) is responsible for the rendering check that prevents this from happening in normal flow; the throw is a defensive backstop for edge cases.
  2. Call `PersonSessionService.StartImpersonationSession( targetPersonAliasId, impersonatorSession, impersonatorInteractionSession )` (Phase 3). The Phase 3 method stamps `PersonSessionAdminImpersonationSettings` with both Guids internally; this orchestrator does NOT need to touch `SetAdditionalSettings`.
  3. Add the new session to a fresh `RockContext` (owned by this method, in a `using` block), save.
  4. Call `PersonSessionService.SetAuthCookie( newSession, context )` to write the new-format `.ROCK` cookie via `RockRequestContext`.
  5. Write a `HistoryLogin` audit row for the impersonation start (preserved from today's flow). Use the same audit fields the existing Bio block writes today; the only change is *where* the write lives.
  - The redirect to the configured target URL is NOT in this method. Redirect is the block's job (see next bullet) because the target URL is a block setting; the service shouldn't know about it. With the redirect in the block and no `rckipid` appending anywhere in this flow, the "no token in URL" property holds by construction.
  - **Why this lives on `PersonSessionService` and not on the block:** Bio.ascx.cs is a WebForms code-behind compiled at runtime; its event handlers are not reachable from Rock.Tests for the same reason `Application_BeginRequest` and `PostAuthenticateRequest` aren't (see Guardrails). Moving the orchestration into a service method makes the actual logic mocked-database testable, leaving only a trivial untested shim in the block — the same factoring Phase 5 and Phase 6 use.
- **`RockWeb/Blocks/Crm/PersonDetail/Bio.ascx.cs`** — the impersonate button click handler is a thin shim:
  1. Call `PersonSessionService.ImpersonatePerson( rockRequestContext, targetPersonAliasId )`.
  2. Redirect to the configured target URL (block setting). The URL is whatever the admin or installer set; this code appends nothing to it, so `rckipid` cannot appear unless the configured URL already contains it (operator error, not a code bug).
  3. If `ImpersonatePerson` throws `InvalidOperationException`, fall through to the page-level error handling that already exists for unauthenticated requests (the existing pattern Rock uses for "your session expired" — surface that, do not crash).
- **Remove the `PersonToken` write** from the entire admin-impersonation flow. Confirm via grep that no remaining call site in Bio.ascx.cs creates a `PersonToken`.
- **Wire `EndImpersonationAndRestore`** as the action for the "stop impersonating" path (find the existing UI affordance for this). Restoring re-attaches both the admin's prior `PersonSession` AND prior `InteractionSession`; the implementation reads both Guids out of `PersonSessionAdminImpersonationSettings`.

### Tests
All tests target `PersonSessionService.ImpersonatePerson` directly with a mocked `RockRequestContext`. The block's shim is untested (same trade-off as Phase 5/6); the "no `rckipid` in URL" property is guaranteed by construction (no code path appends it) and observed manually in Phase 17.

- Mocked-database: `ImpersonatePerson` on a request with an active admin `PersonSession` creates exactly one new `PersonSession` (`CreationSource = Impersonation`) and writes NO `PersonToken` row.
- Mocked-database: `ImpersonatePerson` writes a `HistoryLogin` audit row with the same fields the existing Bio flow writes today.
- Mocked-database: `ImpersonatePerson` calls `SetAuthCookie` through `RockRequestContext` (verify via the mocked context's recorded cookie writes).
- Mocked-database: `ImpersonatePerson` throws `InvalidOperationException` when the request has no active `PersonSession`. No `PersonSession`, no `HistoryLogin`, and no cookie write occur in this case.
- Mocked-database: `PersonToken.TimesUsed` is unchanged across the entire `ImpersonatePerson` call (covers the "no PersonToken involvement" guarantee against accidental future re-coupling).
- Mocked-database: ending impersonation via `EndImpersonationAndRestore` marks the impersonation session inactive and returns the impersonator's prior session.
- Mocked-database: ending impersonation re-attaches the prior `InteractionSession` (the one whose Guid was stamped into `PersonSessionAdminImpersonationSettings.ImpersonatorInteractionSessionGuid` at impersonation start); the `InteractionSession` created at impersonation start remains in the database but is no longer the "current" row for the admin's browser session.
- Mocked-database: dangling `ImpersonatorInteractionSessionGuid` (target row deleted between start and end) is treated the same way as a dangling `ImpersonatorPersonSessionGuid` — `EndImpersonationAndRestore` returns null and marks current inactive.

### Verification
- `build` clean.

### Spec references
- "Design / Impersonation: two distinct cases / Admin impersonation"
- "Considered but Rejected / Write a `PersonToken` row for admin impersonation"

---

## Phase 14 — Pattern A migration + reader migrations

### Goal
Every reader of the deprecated `UserLogin.*` properties or the legacy auth ticket payload moves to `PersonSession` / `RockRequestContext`. After this phase, no reader depends on the old shape.

### Deliverables
- **Pattern A callers** — convert each call site that asks "is this an impersonated session?" to read `IsImpersonated()` against the current `PersonSession` via `RockRequestContext`:
  - `Rock/Web/UI/RockPage.cs:2076`
  - `Rock/Model/CRM/UserLogin/UserLogin.WebForms.cs:101`
  - any additional callers surfaced during the sweep.
- **`UserLogin.IsAuthenticated`, `UserLogin.IsTwoFactorAuthenticated` consumers** — convert each consumer to check current session strength via `RockRequestContext.MeetsRequirement()`:
  - `Rock.Blocks/Security/ChangePassword.cs:132,228`
  - `RockWeb/Blocks/Security/Authorize.ascx.cs`
  - `Rock/Web/UI/RockPage.cs:941`
  - any additional callers surfaced during the sweep.
- **Active Users block** — read from `PersonSession.LastActivityDateTime` and `IsActive`, not from `UserLogin.LastActivityDateTime` / `IsOnLine`.
- **Data Automation job** — reactivate people based on `PersonSession.LastActivityDateTime`.
- **`Rock.Rest/Controllers/AuthController.cs:43-58`** — no behavior change, but add the engineering note required by the spec stating that this endpoint stamps MFA recency without verifying a second factor and that the security concern is deferred to the v2 REST conversion.

### Tests
- Mocked-database: Active Users block returns users with recent `PersonSession` activity.
- Mocked-database: Data Automation job uses `PersonSession.LastActivityDateTime` (verify by seeding a person with stale `UserLogin.LastActivityDateTime` but recent `PersonSession.LastActivityDateTime` and confirming they are treated as active).
- Mocked-database: `ChangePassword.cs` correctly enforces step-up via `MeetsRequirement` when the page requires it.

### Verification
- `build` clean (no new warnings yet, because Phase 15 hasn't marked the legacy properties obsolete).

### Spec references
- "Design / Impersonation: two distinct cases / Pattern A vs Pattern B (callers)"
- "Touch-points to update"

---

## Phase 15 — Deprecations and writer removals

### Goal
Mark deprecated public surface `[Obsolete]` `[RockObsolete( "20.0" )]`. Remove every writer of properties that the new model no longer needs. After this phase, the new model is the only authority.

### Deliverables
- **`UserLogin.LastActivityDateTime`** — `[Obsolete]` `[RockObsolete( "20.0" )]`. All writers removed.
- **`UserLogin.IsOnLine`** — `[Obsolete]` `[RockObsolete( "20.0" )]`. ALL writers removed wholesale (code that has to go: `MarkOnlineUsersOffline()` at app startup and shutdown in `Global.asax.cs:203,782,834`; the `Session_End` handler's offline-flag write at `Global.asax.cs:547-568`; every `UpdateUserLastActivity.Message.Send( ..., IsOnline = false )` call from logout paths — `Rock.Blocks/Security/Logout.cs:109`, `LoginStatus.cs:332`, `ConfirmAccount.cs:391`, `Rock/Web/UI/RockPage.cs:843`, `Rock/Model/CRM/UserLogin/UserLoginService.WebForms.cs:78`).
- **`UserLogin.IsAuthenticated`, `UserLogin.IsTwoFactorAuthenticated`** — `[Obsolete]` `[RockObsolete( "20.0" )]`. Both keep their signatures but always return `false`. Lava templates that referenced these properties get `false` silently — intentional, so template authors notice and migrate.
- **`UserLastActivityTransaction`** — remove (already deprecated in v13).
- **`UpdateUserLastActivity` bus task** — `[Obsolete]` `[RockObsolete( "20.0" )]`. The `IsOnline` property on the task is also deprecated.
- **`Authorization.SetAuthCookie` (and overloads), `Authorization.GetAuthCookie`, `Authorization.GetSimpleAuthCookie`, `Authorization.SignOut`** — `[Obsolete]` `[RockObsolete( "20.0" )]`. Each `[Obsolete]` message names its replacement.
- **`FormsAuthentication.SignOut` callers** — route through `PersonSessionService` (which marks the current `PersonSession` inactive and clears the `.ROCK` cookie via `RockRequestContext`).
- **Sweep for additional helpers in the same family.** Use the compiler-warning surface to find every internal Rock caller of the obsoleted helpers; convert each to the new API. Apply the same `[Obsolete]` treatment to any additional legacy login/logout helpers discovered.

### Tests
- Mocked-database: `UserLogin.IsAuthenticated` always returns false (compile-time consumers tested in Phase 14 already exercise the migrated paths; this is a regression guard).
- Mocked-database: `UserLogin.IsTwoFactorAuthenticated` always returns false.
- Static check (not a runtime test): `MarkOnlineUsersOffline()` no longer exists or has zero call sites — grep the codebase to confirm. (Booting `Global.asax` startup inside Rock.Tests is not feasible; see Guardrails. The cleanup is verified by absence at compile time and by the "App pool recycle does NOT mark all users offline" item in the Phase 17 manual checklist.)
- Mocked-database: a logout request marks the current `PersonSession` inactive AND clears the `.ROCK` cookie AND does NOT send `UpdateUserLastActivity( IsOnline = false )`.

### Verification
- `build` clean (warnings about obsoletes are expected and are exactly the point).

### Spec references
- "Design / Deprecations and removals"
- "Design / Cookie container / `FormsAuthentication.SignOut` callers"

---

## Phase 16 — Final touch-points and cleanup

### Goal
Sweep up remaining cross-cutting wiring and finalize the change.

### Deliverables
- **Rock Cleanup job** — mark `PersonSession` rows inactive once `ExpiresDateTime` passes. Do NOT delete inactive rows (history is preserved).
- **Sweep for missed `IsImpersonated` consumers** — anyone reading the legacy `UserData.IsImpersonated` from a `FormsAuthenticationTicket` must be migrated. The spec's touch-points list is not exhaustive; assume more will surface.
- **`FormsAuthentication.RedirectToLoginPage` callers** verified to still work (`RockPage.cs:954`, `RockWeb/Blocks/Fundraising/FundraisingParticipant.ascx.cs:877`, `RockWeb/Blocks/Fundraising/FundraisingOpportunityView.ascx.cs:555`, `RockWeb/Blocks/CheckIn/AttendanceSelfEntry.ascx.cs:496`). These are generic fallback redirects; no spec impact, but confirm they still trigger correctly under the new pipeline.
- **Final dual-reader window sanity check.** Run a fresh-database scenario and a populated-database scenario; confirm legacy cookies upgrade transparently and new logins produce new-format cookies.

### Verification
- `build` clean.
- `check` skill clean (build + tests + diff review).

### Spec references
- "Touch-points to update"
- "Pre-Implementation Research / Other auth flows"

---

## Phase 17 — Manual testing

This phase is for a human, not the implementation agent. After phases 1-16 land and automated tests are green, the scenarios below must be exercised end-to-end against a running Rock instance (ideally one with seed data so legacy-cookie scenarios can be reproduced). Tick each one off; surface any failure as a follow-up issue against the implementation agent before the spec moves to `specs/completed/`.

### Web login and logout

- [ ] Standard login with username and password lands the user on the post-login page; the `.ROCK` cookie is new-format; a `PersonSession` row exists with `CreationSource = Component`.
- [ ] "Remember me" checked → cookie persists across browser restart; same `PersonSession` is reused on first request post-restart.
- [ ] "Remember me" unchecked → cookie has no `Expires` attribute; browser close clears it; next session is anonymous until login.
- [ ] Standard logout marks the current `PersonSession` inactive, clears the `.ROCK` cookie, and does NOT log the user out of other devices.
- [ ] Logging in as the same person while already authenticated reuses the existing session (does NOT create a duplicate row).
- [ ] Logging in as a different person while authenticated marks the prior session inactive and creates a new one.

### Step-up and MFA

- [ ] Visit a page that requires MFA: if no recent MFA, the user is prompted for both primary credential and second factor (concurrent). After successful submit, `LastMultiFactorAuthenticationDateTime` advances.
- [ ] MFA window honored: revisiting the MFA-required page within `MultiFactorWindowMinutes` does NOT re-prompt.
- [ ] MFA window expires: after the window, revisiting re-prompts.
- [ ] Step-up window honored / expires similarly for pages that require `Elevated`.
- [ ] Browsing between MFA-required pages does NOT advance the MFA timestamp (only actual MFA entry does).

### Admin impersonation

- [ ] Click "Impersonate" on the Person Bio block. The redirect URL contains NO `rckipid` query parameter.
- [ ] After redirect, the current user is the impersonated person, not the admin.
- [ ] No `PersonToken` row was created during this flow (SQL check on the `PersonToken` table immediately before and after).
- [ ] A `HistoryLogin` row is written capturing the impersonation start.
- [ ] The admin's MFA recency is preserved on the impersonation session (verify by visiting an MFA-required page as the impersonated user — the admin's MFA stamping should grant access without re-prompting, assuming the admin was recently MFA-authenticated).
- [ ] "Stop impersonating" returns the admin to their original session. The impersonation session is marked inactive; the impersonator's prior session is the current session again.
- [ ] "Stop impersonating" also re-attaches the admin's original `InteractionSession` so the admin's pre-impersonation activity trail picks up where it left off (verify by inspecting the `InteractionSession` rows tied to the admin's browser session — the row from before impersonation should be the "current" one after restore, with the impersonation-period row preserved as historical).
- [ ] If the impersonator's prior session is deleted or itself inactive while impersonation is in progress, "stop impersonating" gracefully fails closed (current session inactive, user becomes anonymous) — verify by manually marking the impersonator's session inactive in SQL and then clicking stop.
- [ ] Two admins impersonating the same target person at the same time work independently (two separate impersonation sessions).

### User-token (`rckipid`) email links

- [ ] Click an email link with a valid `rckipid`. The user lands on the target page authenticated as the token's target person; the URL no longer carries `rckipid`.
- [ ] `PersonToken.TimesUsed` advances to 1 on the first click.
- [ ] Navigating to another page in the same browsing session does NOT advance `TimesUsed` (the token is "consumed" once per session, not per page).
- [ ] Re-clicking the same email link in another tab while the session is active does NOT advance `TimesUsed` (rule 2 of the matrix).
- [ ] Clicking the link from an already-logged-in `Component` session for the SAME person leaves the session unchanged but strips `rckipid` from the URL (rule 4).
- [ ] Clicking the link from a `Component` session for a DIFFERENT person marks the current session inactive and creates a new `UserToken` session (rule 5).
- [ ] Clicking the link while in an admin-impersonation session abandons impersonation and creates a `UserToken` session (rule 3).
- [ ] Clicking an expired / revoked / over-`UsageLimit` token marks the current session inactive; the resulting page load is anonymous (rule 1).
- [ ] Clicking a link to an MFA-required page prompts for MFA (NEW behavior; was previously bypassed). Without MFA, access is denied.
- [ ] Clicking a link to a page outside the token's page scope returns a not-authorized error.
- [ ] After navigating to a page outside the token's scope (no `rckipid`), per-request page-scope re-validation still fails the user (not-authorized).

### Legacy cookie upgrade

- [ ] A user with a legacy `.ROCK` cookie from before the deployment makes a request: the cookie is silently upgraded; a `PersonSession` is created with `CreationSource = Legacy` and `IssuedDateTime = ticket.IssueDate`; the cookie on the response is new-format.
- [ ] The user is NOT forced to re-log in.
- [ ] A second request with the same legacy cookie (e.g. from a client that does not honor `Set-Cookie`) hits the existing `PersonSession` row — no duplicate is created.
- [ ] A legacy cookie with `IsImpersonated = true` is dropped on first request; the request is anonymous; no `PersonSession` is created.
- [ ] The kill switch (`RejectAuthenticationCookiesIssuedBefore`) correctly marks an upgraded session inactive if its `IssuedDateTime` precedes the threshold.
- [ ] An upgraded session reports `Authenticated` strength (NOT `Elevated` or `MultiFactor`) until the user next authenticates.

### Cookie reissue

- [ ] A long-lived persistent session reissues the cookie at half-life (default 15 days). Verify by force-aging the `iat` (e.g. via test seed) and confirming a `Set-Cookie` header appears.
- [ ] Rotating `DataEncryptionKey` (`OldDataEncryptionKey1` set to the previous value) causes the next request to reissue the cookie with the new key, regardless of `iat` age.
- [ ] Reissue does NOT change `PersonSession.IssuedDateTime` (kill-switch correctness preserved).

### API key requests

- [ ] First request with a newly issued API key creates an `ApiKey` `PersonSession`.
- [ ] Subsequent API-key requests reuse the existing session.
- [ ] Activity bus task advances `LastActivityDateTime` (throttled).
- [ ] Deleting the `UserLogin` (revoking the key) nulls the session's `UserLoginId` via `ON DELETE SET NULL`; the historical row is preserved.
- [ ] Future requests with the deleted key authenticate as anonymous; the orphaned session is not resurrected.
- [ ] JWT requests do NOT create `PersonSession` rows (verify by issuing JWT against an unused user and counting rows).
- [ ] OAuth bearer requests do NOT create `PersonSession` rows.

### `InteractionSession` linkage

- [ ] Anonymous user arrives → `InteractionSession` row created with `PersonSessionId = null`.
- [ ] Anonymous user logs in → the existing `InteractionSession` row is updated in place to set `PersonSessionId` to the new `PersonSession.Id` (no duplicate `InteractionSession` row).
- [ ] Already-authenticated user (persistent cookie) arrives → first interaction creates an `InteractionSession` with `PersonSessionId` set at insert.
- [ ] User logs out → next request creates a new `InteractionSession` with `PersonSessionId = null`.
- [ ] User logs in as a different person → new `InteractionSession` row with the new `PersonSessionId`.
- [ ] Legacy cookie upgrade with an existing `InteractionSession` → that row is updated to point at the upgraded `Legacy` `PersonSession` (no duplicate).
- [ ] Legacy cookie upgrade with no existing `InteractionSession` → the first subsequent `InteractionSession` is stamped with the upgraded `PersonSession.Id` at insert.

### Rock Mobile

- [ ] Mobile login produces a `Component` `PersonSession` with `IsPersistent = true`; the cookie value is returned in the response body.
- [ ] Mobile API requests carrying `Cookie: .ROCK=...` resolve to the `PersonSession`, advance activity, and behave equivalently to browser requests.
- [ ] `GetLaunchPacket` returns a fresh `CurrentPerson.AuthToken` that the client stores on next launch; the value resolves to the same `PersonSession`.
- [ ] Pre-deployment Mobile client carrying a legacy cookie upgrades correctly via the composite-key lookup; the user is not signed out.
- [ ] Mobile re-login as a different person creates a new session and marks the prior session inactive.

### Rock TV

- [ ] TV login produces a `Component` `PersonSession` with `IsPersistent = true`.
- [ ] TV requests carrying the cookie resolve correctly.
- [ ] Pre-deployment TV client carrying a legacy cookie indefinitely sends the legacy cookie; the composite-key lookup keeps it resolving to the same `PersonSession` until the user re-authenticates or the legacy reader is sunset.

### Auth0 / external `IExternalRedirectAuthentication` providers

- [ ] OAuth redirect callback flows through the Obsidian Login block and produces a `Component` `PersonSession` with `AuthenticationComponentId` set to the Auth0 component. No Auth0-specific code was needed.
- [ ] Step-up and MFA recency stamping behave correctly for Auth0-authenticated sessions.

### SignalR / real-time hubs

- [ ] SignalR hub connection from an authenticated browser exposes the current `PersonSession` to hub actions.
- [ ] SignalR hub connection from an anonymous browser proceeds anonymously; hub actions see no current person.
- [ ] Long-lived SignalR connections do NOT trigger excessive `UpdatePersonSessionLastActivity` writes (the bus task is intentionally not fired for hub traffic).

### Cross-cutting

- [ ] Web farm: a cookie issued by node A validates correctly on node B (the `DataEncryptionKey` is shared).
- [ ] Active Users block lists users with recent `PersonSession.LastActivityDateTime`; matches expectations across multiple devices for the same person (each device = separate session row).
- [ ] App pool recycle does NOT mark all users offline (no more `MarkOnlineUsersOffline`).
- [ ] Data Automation job correctly reactivates people based on `PersonSession` activity (verify with a person whose `UserLogin.LastActivityDateTime` is stale but `PersonSession.LastActivityDateTime` is recent — they should be treated as active).
- [ ] Person merge: existing `PersonSession` rows are left pointing at their original `PersonAlias` (no fix-up to merge target).
- [ ] Rock Cleanup job marks expired `PersonSession` rows inactive but does NOT delete them. Historical rows remain queryable.
- [ ] `Authorization.SignOut()` (and any caller routed through `PersonSessionService`) invalidates only the current session, leaving other active sessions for the same person intact.
- [ ] Generic fallback redirects (`FormsAuthentication.RedirectToLoginPage` callers in RockPage, Fundraising, AttendanceSelfEntry) still work when no login page is configured.

### Regression: previously broken pages

- [ ] ChangePassword (Obsidian): correctly enforces step-up via `MeetsRequirement`.
- [ ] Authorize.ascx (WebForms): correctly enforces step-up via the new properties / `MeetsRequirement`.
- [ ] `RockPage.cs:941` MFA-required page enforcement: the deprecated `UserLogin.IsTwoFactorAuthenticated` property returns false; the new path enforces via `RockRequestContext.MeetsRequirement(MultiFactor)`.

### Kill-switch and recovery

- [ ] Setting `RejectAuthenticationCookiesIssuedBefore` to `RockDateTime.Now` marks every active `PersonSession` issued before the threshold inactive on first request and expires their cookies. Users are forced to re-log in.
- [ ] After kill-switch fires, fresh logins produce new `PersonSession` rows with `IssuedDateTime > threshold` that are not rejected.
- [ ] Legacy-cookie kill-switch path: a user presenting a pre-deployment legacy `FormsAuthenticationTicket` whose `IssueDate` precedes the kill-switch threshold is rejected on the *current* request — the cookie is expired and the request is anonymous. (Verify by setting the threshold to a recent value, then making a request with a legacy cookie issued before it; the user should be forced to re-log in without ever briefly authenticating. This exercises the dual-reader bridge block in Phase 5's BeginRequest hook.)

---

## Appendix A — Touch-point quick reference

For each file the implementation will modify, the phase that owns the change. Use this when reviewing a partial PR or when triaging a missed touch-point.

| File | Phase |
|---|---|
| `Rock/Model/Security/PersonSession/PersonSession.cs` (new) | 1 |
| `Rock/Model/Security/PersonSession/PersonSessionConfiguration.cs` (new) | 1 |
| `Rock/Model/Security/PersonSession/PersonSessionService.cs` (new) | 2-3, plus `ResolveSessionForRequest`/`UpgradeLegacyCookieForRequest` in 4-6, plus `ImpersonatePerson` in 13 |
| `Rock.Enums/Security/AuthenticationStrength.cs` (new) | 1 |
| `Rock.Enums/Security/AuthenticationRequirement.cs` (new) | 1 |
| `Rock.Enums/Security/PersonSessionCreationSource.cs` (new) | 1 |
| `Rock/Security/PersonSessionAdminImpersonationSettings.cs` (new) | 2 |
| `Rock/Security/PersonSessionUserTokenSettings.cs` (new) | 2 |
| `Rock.Migrations/Migrations/*PersonSession*.cs` (new) | 1 |
| `Rock/Model/Core/Interaction/InteractionSession.cs` | 1 (column), 9 (adoption) |
| `Rock/Model/Core/Interaction/InteractionService.cs:583` (SQL upsert) | 9 |
| `RockWeb/App_Code/Global.asax.cs:582-604` (BeginRequest shim → `ResolveSessionForRequest`; existing kill-switch block retained in place at the top of the handler, with a sunset comment marking it for retirement alongside `FindOrCreateLegacyUpgradeSession`) | 5 |
| `RockWeb/App_Code/Global.asax.cs` (PostAuthenticateRequest shim → `UpgradeLegacyCookieForRequest`) | 6 |
| `RockWeb/App_Code/Global.asax.cs:203,547-568,782,834` (online flag removals) | 15 |
| `Rock/Net/RockRequestContext.cs` (PersonSession + MeetsRequirement) | 7 |
| `Rock.Blocks/Security/Login.cs:718-734` | 7 |
| `Rock.Blocks/Security/ChangePassword.cs:132,228` | 14 |
| `Rock.Blocks/Security/Logout.cs:109` | 15 |
| `Rock.Blocks/Security/LoginStatus.cs:332` | 15 |
| `Rock.Blocks/Security/ConfirmAccount.cs:391` | 15 |
| `RockWeb/Blocks/Security/Authorize.ascx.cs` | 14 |
| `Rock/Web/UI/RockPage.cs:843` (online flag) | 15 |
| `Rock/Web/UI/RockPage.cs:941` (MFA gate) | 14 |
| `Rock/Web/UI/RockPage.cs:954` (fallback redirect, verify only) | 16 |
| `Rock/Web/UI/RockPage.cs:2076` (IsImpersonated read, Pattern A) | 14 |
| `Rock/Web/UI/RockPage.cs:2111` (ProcessImpersonation, Pattern B) | 12 |
| `Rock.Rest/Filters/AuthenticateAttribute.cs` | 10 |
| `Rock.Rest/ApiControllerBase.cs:103` | 12 |
| `Rock/Web/HttpModules/RockGateway.cs:499` | 12 |
| `Rock/Model/CRM/UserLogin/UserLogin.cs` (Obsolete markers) | 15 |
| `Rock/Model/CRM/UserLogin/UserLogin.WebForms.cs:101` (Pattern A) | 14 |
| `Rock/Model/CRM/UserLogin/UserLoginService.WebForms.cs:78` (online flag) | 15 |
| `Rock/Mobile/MobileHelper.cs:206` | 11 |
| `Rock/Tv/TvHelper.cs:193` | 11 |
| `RockWeb/Blocks/Crm/PersonDetail/Bio.ascx.cs` (impersonate-button shim → `PersonSessionService.ImpersonatePerson`, then `Response.Redirect` to configured target URL) | 13 |
| `Rock/Security/Authorization.cs:812,823,853,927` (Obsolete markers) | 15 |
| `Rock/Security/SecuritySettings.cs:123` (kill-switch read) | 5 |
| `Rock.Rest/Controllers/AuthController.cs:43-58` (engineering note) | 14 |
| Active Users block | 14 |
| Data Automation job | 14 |
| Rock Cleanup job | 16 |

This list is **not exhaustive**. Treat it as a starter — the obsolete markers will surface additional callers during build, and the implementer is expected to follow up on each.
