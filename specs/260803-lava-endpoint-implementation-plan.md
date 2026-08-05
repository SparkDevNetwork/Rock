---
title: Lava Endpoint Data for Obsidian Content - Implementation Plan
date_created: 2026-08-03
source_spec: specs/260803-lava-endpoint-data-for-obsidian-content.md
audience: Claude Code, implementing in the Rock repo
---

# Implementation Plan: Lava Endpoint Data for Obsidian Content

You are implementing the design in [260803-lava-endpoint-data-for-obsidian-content.md](260803-lava-endpoint-data-for-obsidian-content.md). Read that spec first for the why. This document is the what and the where.

**Every design decision is already made. Do not reopen them.** If you believe something is wrong, say so and stop rather than choosing a different approach on your own.

## Goal

An Obsidian Content block gets its data from Lava endpoints instead of hunting for a REST API. Claude writes Lava, an MCP tool creates the endpoint, and the authored Vue component calls it like a block action.

## Ground rules

- Follow `CLAUDE.md` and everything in `.claude/rules/`. Match surrounding code rather than introducing new patterns.
- **No em-dashes and no `--` in any file, comment, or string.**
- **Do not run any build.** No MSBuild, no `dotnet build`, no `npm run build`, no build skill. When a phase is done, offer to build and wait.
- Copyright headers on every new file. C# and TypeScript use the multi-line block, `.obs` uses the one-liner. Templates are in `.claude/rules/code-conventions.md`.
- Work on the existing `feature-kh-obsidian-content` branch unless told otherwise. That branch holds the Obsidian Content block this builds on.
- Commit per phase, using the format in `CLAUDE.md`. Domain is `CMS`.

## Prerequisites

This sits on top of the Obsidian Content prototype (the `ObsidianContent` entity, the `ObsidianContentDetail` block, and the browser compile pipeline). All of it is already on the branch. You are not modifying any of it except where a phase says so.

---

## Phase 1: Let Lava endpoints return JSON

Today [LavaAppController.cs](../Rock.Rest/v2/LavaAppController.cs) hardcodes `text/html` for every response, and there is no content-type field anywhere on `LavaEndpoint`. Add one, stored in the existing settings blob so no migration is needed.

Default it to `text/html` everywhere so existing endpoints behave exactly as they do now.

### 1.1 Add the setting

`Rock/Cms/LavaEndpointAdditionalSettings.cs`

Add a `ContentType` string property defaulting to `"text/html"`, alongside the existing `EnableCrossSiteForgeryProtection`. Document it the way the existing property is documented.

### 1.2 Surface it on the cache

`Rock/Web/Cache/Entities/LavaEndpointCache.cs`

Add a `ContentType` property that reads from `EndpointAdditionalSettings`, mirroring `EnableCrossSiteForgeryProtection` at line 258 exactly. Same shape, same style.

### 1.3 Use it in the controller

`Rock.Rest/v2/LavaAppController.cs`, in `ProcessEndpoint` (around line 157)

Replace the hardcoded literal:

```csharp
responseMessage.Content = new StringContent( context.EndpointResponse.Content, Encoding.UTF8, "text/html" );
```

Use the endpoint's `ContentType`, falling back to `"text/html"` when the endpoint could not be resolved (`context.LavaEndpoint` can be null on a 404 path, so guard it).

### 1.4 Add it to the bag

`Rock.ViewModels/Blocks/Cms/LavaEndpointDetail/LavaEndpointBag.cs`

Add a `ContentType` property next to `EnableCrossSiteForgeryProtection` (around line 114).

### 1.5 Read and write it in the block

`Rock.Blocks/Cms/LavaEndpointDetail.cs`

- Around line 201, in the entity-to-bag mapping, read it the same way `EnableCrossSiteForgeryProtection` is read, defaulting to `"text/html"`.
- Around line 310, `SetAdditionalSettings` currently constructs a new settings object with only the CSRF flag. **This is a latent bug you must not repeat: constructing a fresh object discards any other setting.** Include `ContentType` in that construction so both settings persist.

### 1.6 Add the UI field

`Rock.JavaScript.Obsidian.Blocks/src/Cms/LavaEndpointDetail/editPanel.partial.obs`

Note the capital `L` in the folder name. Add an input for the content type alongside the other endpoint settings, following whatever control and layout pattern that file already uses. A text box is fine; a drop down with the common types plus a custom option is better if the file already has drop downs to copy from.

Check whether `EnableCrossSiteForgeryProtection` has a UI field in that file. If it does not, match its absence rather than adding one, and mention it in your summary.

### Done when

The content type round-trips: settings class, cache, controller, bag, block read, block write, UI. Both settings survive a save.

---

## Phase 2: Keep the response body on non-200

`Rock.Rest/v2/LavaAppController.cs`, in `MergeRequest` (around lines 301 to 308)

Current behavior throws the body away whenever the template set a non-200 status:

```csharp
if ( HttpContext.Current?.Response.StatusCode != 200 )
{
    content = $"Endpoint returned status of {HttpContext.Current?.Response.StatusCode}.";
    context.EndpointResponse.ResponseStatus = ( HttpStatusCode ) HttpContext.Current?.Response.StatusCode;
}
```

Change it to honor the status code but keep whatever the template produced. Only substitute the generic message when the template produced nothing at all, so a caller never receives an empty body with no explanation.

Add an engineering note in the format from `CLAUDE.md` explaining why: a JSON endpoint has to be able to return a structured error body alongside a non-200 status.

### Done when

`{% httpresponse status:'404' %}` plus a JSON body returns 404 with that JSON. An endpoint that sets a non-200 and emits nothing still returns the generic message.

---

## Phase 3: The `useLavaApp` framework utility

New file: `Rock.JavaScript.Obsidian/Framework/Utility/lavaApp.ts`

The `Framework/Utility` folder is auto-bundled by the Obsidian build (see the `nested` option in `Build/build-tools.js`), so **no build config change is needed**. The file becomes importable as `@Obsidian/Utility/lavaApp`.

### Contract

```ts
/** Options that change how a single endpoint call is made. */
export type LavaAppInvokeOptions = {
    /** The HTTP method. Endpoints are keyed by slug AND method, so this selects which endpoint. */
    method?: "GET" | "POST";
};

/** A bound Lava application that can invoke its endpoints by name. */
export type LavaApp = {
    invoke: <T>(endpointSlug: string, data?: Record<string, unknown>, options?: LavaAppInvokeOptions) => Promise<HttpResult<T>>;
};

export function useLavaApp(applicationSlug: string): LavaApp;
```

### Behavior

- Build the URL as `/api/v2/lava-app/1/${applicationSlug}/${endpointSlug}`. The `1/` is a version segment in the route, not a typo.
- Default the method to `POST`. For `GET`, pass `data` as query params; for `POST`, pass it as the body.
- Always send the header `X-Helix-CSRF-Protection: true`. CSRF protection defaults to **on** for every endpoint, so omitting it produces a 401.
- Delegate to `doApiCall` from `./http`, using the overload that accepts an options object with `headers`.
- Return `HttpResult<T>` unchanged, so callers use the same `isSuccess` / `data` / `errorMessage` shape as `invokeBlockAction`. Import the type from `@Obsidian/Types/Utility/http`.
- If `result.data` arrives as a string, `JSON.parse` it and return the parsed value. On a parse failure return a failed `HttpResult` with a clear `errorMessage`. This branch covers endpoints still returning `text/html`; it stays harmless after Phase 1.

Follow `.claude/rules/obsidian-conventions.md`: 4-space indent, double quotes, semicolons, Stroustrup braces, no spaces inside parens, explicit return types on function declarations.

### Done when

The file exists, is importable as `@Obsidian/Utility/lavaApp`, and lints clean under the project's ESLint config.

---

## Phase 4: The MCP tools

New file: `Rock/AI/Agent/LavaDataSkill.cs`

**Mirror [PageBuilderSkill.cs](../Rock/AI/Agent/PageBuilderSkill.cs) exactly** for class structure, attributes, authorization, and result helpers. It is the closest existing precedent: an `AgentSkillComponent` subclass whose tools create CMS objects.

Copy its authorization approach verbatim rather than inventing one. Creating a Lava endpoint must be gated at least as tightly as creating a page.

### GUIDs

Generate fresh GUIDs for the skill's `EntityTypeGuid`, its `AgentSkillGuid`, and one `AgentToolGuid` per tool. Uppercase, hyphenated. Do not reuse any GUID that appears elsewhere in the repo.

### Tools

**`CreateLavaEndpoint`**

Parameters: application slug, application name (used only when creating), endpoint slug, HTTP method, the Lava template, enabled Lava commands, security mode.

Behavior:
1. Find the `LavaApplication` by slug, or create it if absent. One application per Obsidian Content block is the intended pattern, so reuse is the common path on the second and later endpoints.
2. Create the `LavaEndpoint` under it. Set `ContentType` to `application/json` in the additional settings, since these endpoints exist to feed components.
3. **Test-execute the template** (see below) and include the result in the tool response.
4. Return the application slug, the endpoint slug, and the full callable URL.

**`GetLavaEndpoint`**

Parameters: application slug, endpoint slug. Returns the current `CodeTemplate` so the agent can iterate.

**`UpdateLavaEndpoint`**

Parameters: application slug, endpoint slug, the new template. Replaces `CodeTemplate`, test-executes, returns the result.

### Test-execute

This is the single most valuable part of the phase. It keeps the agent in the loop instead of shipping blind.

Render the template with the acting person from `AgentRequestContext`, and **explicitly pass `ExceptionHandlingStrategySpecifier.Throw` via `LavaRenderParameters`** so a broken template surfaces as a real error instead of being silently ignored. Catch that exception in the tool and return it as the error message.

Do not change the engine's global exception strategy. Set it for this render only.

Be honest in the tool's response about what the test covers: it renders with the current person and no HTTP request context, so it catches syntax errors, bad filters, and null references, but not request-specific behavior. Say so in the `AgentUsage` text so the agent does not over-trust a pass.

### Done when

An administrator can create an application and endpoint through MCP, get a compile-or-render result back, read the template, and replace it.

---

## Phase 5: Teach the pattern

`Rock/AI/Agent/ObsidianVibeCodingSkill.cs` and the new `LavaDataSkill`

Update the skill instructions and `AgentUsage` text so an agent knows the intended flow:

1. For data, create a Lava endpoint rather than searching for a REST endpoint.
2. Group a block's endpoints under one application named after the dashboard.
3. In the component, import `useLavaApp` from `@Obsidian/Utility/lavaApp` rather than hand-rolling the URL, header, and parsing.
4. Check `isSuccess` before reading `data`, and render an empty state rather than an error when the endpoint legitimately has no rows.

### Done when

The instructions describe the flow well enough that an agent with no other context follows it.

---

## Decisions already settled

Do not revisit these. Each was argued out in the spec.

| Decision | Why |
|---|---|
| Content type lives in the settings blob, not a new column | `AdditionalSettingsJson` already exists, so no migration. Rock persists configuration as JSON. |
| `useLavaApp` ships as a framework utility, not inlined boilerplate | Compiled components are frozen in the database. An import is the only channel that can fix an existing dashboard without re-authoring it. |
| One Lava application per Obsidian Content block | Security set once, `ConfigurationRigging` shared across endpoints, and endpoints cascade-delete with the application. |
| `invoke` returns `HttpResult<T>` | Same shape as `invokeBlockAction`, so the block-action framing is literal. |
| Test-execute on create, rather than richer runtime error reporting | Catches breakage while the agent can still fix it. Visitors should never see Lava error text. |
| Tools are administrator-gated | Matches the existing authoring tools. Grants nothing beyond what the Lava Endpoint Detail block already gives an admin. |

## Out of scope

- Orphaned applications when a block is deleted, and drift between a component and its endpoint. Both already exist for any Lava Application consumer and are not made worse here.
- The swallowed Lava exception in `MergeRequest`'s `try/catch`. Pre-existing, unrelated to this feature, and deliberately left alone.
- `SearchRockApis` and the other discovery tools. Separate spec.
- Anything touching the browser compile pipeline or the `ObsidianContent` entity.

## Reporting back

For each phase, state what you changed, what you did not change and why, and anything you found that contradicts this plan. If a line reference here does not match what you find, trust the code and say so.

Do not report a phase complete until every file in it is done. If a phase is blocked, finish the others and say plainly which one you left and why.
