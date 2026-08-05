---
title: Obsidian Content Vibe Coding - Latest Findings
author: Kyle Henning
date_created: 2026-08-04
status: summary for presentation
related:
  - specs/260721-obsidian-content-block-and-component-model.md
  - specs/260722-mcp-driven-obsidian-content-vibe-coding.md
  - specs/260803-lava-endpoint-data-for-obsidian-content.md
  - specs/260803-lava-endpoint-implementation-plan.md
  - specs/260803-mcp-vibe-coding-skill-candidates.md
  - docs/cms/obsidian-content.md
---

# Obsidian Content Vibe Coding: Latest Findings

Branch: `feature-kh-obsidian-content`. Nothing here is merged.

## The one-sentence version

An admin can now ask Claude for a dashboard and get a working one: Claude places the block, writes a Lava endpoint for the data, writes the Vue component, compiles it, and saves it. The flow works end to end, but only from Claude Code, because three of its steps still depend on reading the Rock repo off disk.

## What shipped since last time

The hardest unbuilt step in the flow was API discovery: find an existing REST endpoint that happens to return what the component needs. That step is now replaced rather than solved. Claude writes a Lava endpoint that returns exactly the shape the component renders.

Five pieces, all on the branch and uncommitted:

| Piece | What it does |
|---|---|
| `ContentType` on Lava endpoints | Endpoints can return `application/json`. Lives in the existing settings blob, so no migration. Defaults to `text/html`, so every existing endpoint is unchanged. |
| Non-200 responses keep their body | The controller used to discard the body and substitute a sentence. A JSON endpoint can now return a structured error alongside its status code. |
| `useLavaApp` framework utility | `Framework/Utility/lavaApp.ts`. Binds the application once, then takes endpoint names. Handles the version segment, the CSRF header, and the JSON parsing. |
| `LavaDataSkill` MCP tools | `CreateLavaEndpoint`, `GetLavaEndpoint`, `UpdateLavaEndpoint`. Administrator gated, mirroring `PageBuilderSkill`. |
| Skill instructions | Teach the agent to write an endpoint instead of hunting for REST, group a block's endpoints under one application, and import the helper rather than hand-rolling the call. |

Two decisions worth calling out because they shaped everything else:

**The helper ships as framework code, not as boilerplate the agent writes into each component.** Compiled components are frozen in the database. An import resolves against whatever the framework ships at render time, so fixing the route or the header name fixes every dashboard already built. Inlined boilerplate bakes the mistake in permanently, and each dashboard has to be re-authored by hand.

**Every write test-executes the template and returns the result.** The agent finds out the Lava is broken while it can still fix it, instead of a visitor seeing Lava error text later. The tool response is explicit that the test renders with the current person and no HTTP request context, so a pass is not a guarantee.

## The headline finding

Everything below came out of one real session: building a Connection Request Dashboard end to end through `PageBuilderSkill`, `LavaDataSkill`, and `ObsidianVibeCodingSkill`.

Three of the five MCP steps are unbuilt, and Claude works around all three the same way: by reading the Rock repo off disk.

| Step | Tool | Status |
|---|---|---|
| Place the block | `FindPage`, `CreatePage`, `AddBlock` | Works |
| Find controls | `ListObsidianControls`, `GetObsidianControl` | Not built. Reads the repo. |
| Find data | Now solved by `LavaDataSkill` | Works |
| Get the compiler | `GetCompiler` | Not built. Reads the repo. |
| Save | `SetContentSource`, `GetContentSource` | Works |

A church runs a specific version of Rock. An agent working against that instance from Claude Desktop has no checkout to read. So the flow that works beautifully in front of a developer does not work at all in front of the person it is actually for.

`GetCompiler` is the gate. Without the assembler and the pinned Vue version, a repo-less client cannot compile anything, and `SetContentSource` already tells clients to use a tool that does not exist.

## What the session actually cost

Concrete, reproducible friction, not speculation:

- **Grid took several turns to assemble.** The API is spread across `grid.ts`, `grid.partial.obs`, and more than twenty column files. Per-column props are the part that matters and the part that is hardest to find.
- **Every style token was verified by grepping `styles-v2/core.css`.** An agent without the repo is guessing at token names, and guessing means hardcoded hex values, which survive a church reconfiguring its variables.
- **One Lava enablement token was not documented anywhere.** The knowledge base article for Delete Entity gives the syntax and says the command runs only when enabled, but never names the token. Finding `RockEntityDelete` required grepping `RequiredPermissionKey` in the repo. An instance can read that off its own registered blocks at runtime.
- **A cascade rule almost shipped a broken delete.** `ConnectionRequestActivity` does not cascade, so a delete endpoint written the obvious way would have failed on live data for any request that had activities.
- **Almost every failure presented as a blank block or an opaque 500,** and each one took many round trips to localize.
- **A mis-created endpoint could not be removed through MCP,** which forced a detour into the admin UI. Agents iterate, so they need cleanup tools.

There is also a self-inflicted obligation. `LavaDataSkill` pushes entity commands and gates raw SQL behind an explicit justification. That is the right call, but it means the entity command path has to be genuinely well supported, or agents will stall arguing with the guardrail.

## What to build next

Thirteen candidate MCP skills, in three groups. Full detail in [the candidates spec](260803-mcp-vibe-coding-skill-candidates.md).

**Group 1, what to build with:** `ObsidianControls`, `ObsidianGrid`, `ObsidianCharts`, `RockStyling`, `ObsidianContentPatterns`, `LavaEndpointPatterns`.

**Group 2, the instance as source of truth:** `RockVersion`, `RockEntityModel`, `RockLavaReference`, `RockRestApi`, `ObsidianCompiler`.

**Group 3, the feedback loop:** `ObsidianContentDiagnostics`, `LavaAppLifecycle`.

Recommended order:

1. **ObsidianContentPatterns.** Prevents whole classes of compile failure instead of diagnosing them afterward.
2. **RockLavaReference.** Closes a gap the published knowledge base structurally cannot close.
3. **ObsidianContentDiagnostics.** Turns a twenty-turn debug into a two-turn one.

`ObsidianCompiler` moves to the top the moment Claude Desktop matters, because nothing else in the flow works without it.

## The open question to settle first

Every skill in Group 1 assumes it can read control source, styles, and compiler code off the instance's disk. That holds for a development checkout. It is false for a deployed instance that ships only compiled output, which is the exact case Group 2 exists to serve.

So Group 1 as designed works in precisely the environment that needs it least.

A manifest generated at build time and shipped with Rock is probably the real answer. Control props, chart series shapes, grid column APIs, and the style token catalog are all knowable at compile time, and none of them change per instance. Worth deciding before writing three skills that assume source is present.

## Known gaps and risks

1. **Compile-on-view does not exist,** but `SetContentSource` says it does. A source-only save stores the row, renders nothing, and raises no error anywhere.
2. **No Vue version check** on save or on render. The stored version string is accepted as given.
3. **No public versus internal signal on controls.** An agent can pick a control that exists, compiles, renders, and was never meant for general use.
4. **Authored code runs as the visitor, not as the author.** A dashboard that works while an admin tests it can silently show nothing to a normal member. Anything built this way needs one pass viewed as a non-admin.
5. **`useInvokeBlockAction()` resolves inside the authored component** and points at the host block's `SaveContent`. The server re-checks permissions, so it is not an escalation, but it is not an intended surface either.
6. **The editor's preview note is wrong.** It claims API calls will not work in the preview. They do. The preview isolates crashes and DOM changes, not the login session.
7. **The branch is 124 commits behind `develop`,** and the migration's EF snapshot needs regenerating.
8. **Nothing here has been built or committed yet.** The five phases are written and were exercised live, but no compile has been run against them.

## Decisions I want

1. Build-time manifest, or instance-disk reads for Group 1? This blocks three skills.
2. Is Claude Desktop a target for this release? If yes, `ObsidianCompiler` jumps the queue.
3. Does compile-on-view get built, or does `SetContentSource` stop promising it?
