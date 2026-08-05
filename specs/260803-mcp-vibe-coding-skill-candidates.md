---
title: MCP Skill Candidates for Obsidian Content Vibe Coding
date_created: 2026-08-03
status: draft, for discussion
related:
  - specs/260803-lava-endpoint-data-for-obsidian-content.md
  - specs/260803-lava-endpoint-implementation-plan.md
  - docs/cms/obsidian-content.md
related_files:
  - Rock/AI/Agent/ObsidianVibeCodingSkill.cs
  - Rock/AI/Agent/LavaDataSkill.cs
  - Rock/AI/Agent/PageBuilderSkill.cs
---

# MCP Skill Candidates for Obsidian Content Vibe Coding

## Why this list exists

Two problems drive it.

**The agent has to know what to build with.** Which controls exist, how to style them, what the common shapes are. Today that knowledge is discovered by reading the Rock repo off disk.

**The agent usually will not have the repo.** A church runs a specific version of Rock. An agent working against that instance from Claude Desktop has no checkout to read. The MCP is the only channel that can tell it what that particular instance contains.

Everything below is grounded in a real session: building a Connection Request Dashboard end to end through `PageBuilderSkill`, `LavaDataSkill`, and `ObsidianVibeCodingSkill`. Where an item says it cost time, it cost time in that session.

## Context that shapes the list

`LavaDataSkill` now pushes entity commands and gates raw SQL behind an explicit justification. That is the right call, but it creates an obligation: if SQL is hard, the entity command path has to be genuinely well supported, or agents will stall or burn turns arguing with the guardrail. `LavaEndpointPatterns` below exists specifically to pay that debt.

## At a glance

| Skill | Group | Answers | Status |
|---|---|---|---|
| ObsidianControls | Authoring | Which control, what props | Named as a gap already |
| ObsidianGrid | Authoring | Grid data contract and columns | New |
| ObsidianCharts | Authoring | Series shapes and default palette | New |
| RockStyling | Authoring | Which tokens and classes to use | New |
| ObsidianContentPatterns | Authoring | The authoring contract and skeletons | New |
| LavaEndpointPatterns | Authoring | JSON from entity commands, no SQL | New |
| RockVersion | Instance | What this instance supports | New |
| RockEntityModel | Instance | Schema, enums, cascades, friendly names | New |
| RockLavaReference | Instance | Registered commands and enablement tokens | New |
| RockRestApi | Instance | Routes, verbs, bag shapes | Named as a gap already |
| ObsidianCompiler | Instance | The assembler and pinned Vue version | Named as a gap already |
| ObsidianContentDiagnostics | Feedback | Why is the block blank | New |
| LavaAppLifecycle | Feedback | List and delete what I created | New |

---

## Group 1: What to build with

### ObsidianControls

`ListObsidianControls(query)` and `GetObsidianControl(name)`.

The useful payload is not the file. It is the `defineProps` shape, the emits, the slots, the derived import path, and one usage snippet.

Two additions beyond what the gap doc describes:

- Return the alias map, so the agent knows what is legally importable at runtime. `@Obsidian/ViewModels/*` is not importable, and an agent copying a repo block will reach for it.
- A public versus internal marker. There is currently no signal at all, so an agent can pick a control that exists, compiles, renders, and was never meant for general use.

### ObsidianGrid

Worth separating from controls. The API is spread across `grid.ts`, `grid.partial.obs`, and more than twenty column files, and assembling it took several turns.

Should return:

- The `{ rows: [...] }` data contract and `keyField` semantics, including that the key is what gets handed to column click handlers.
- Per-column distinctive props rather than the full prop list. `LabelColumn` has `classSource`, `colorSource`, `textSource`. `DeleteColumn` confirms by default unless `disableConfirmation` is set. `ButtonColumn` takes `iconClass`, `tooltip`, `visible`, `disabled`.

### ObsidianCharts

`BarSeries`, `PieSeries`, `LineSeries` shapes and the labels-plus-series pairing.

The critical fact to encode: omitting `color` gets Rock's categorical palette automatically. That is simultaneously the correct styling answer and the thing an agent will get wrong by inventing hex values.

### RockStyling

The styles-v2 token catalog with semantics attached, not just names:

- The interface ramp, softest through strongest, and what each step is for.
- Spacing, rounded, font size, font weight tokens.
- Semantic color pairs.
- Which utility classes Rock actually ships.

Every token used in the dashboard was verified by grepping `RockWeb/Styles/styles-v2/core.css`. An agent without the repo is guessing at names.

Pair the catalog with a hard rule: hex values are never acceptable, because churches reconfigure the variables and hardcoded colors survive the reconfiguration.

### ObsidianContentPatterns

The authoring contract, as a skill rather than tribal knowledge:

- Plain JavaScript only. No `lang="ts"`, no type annotations.
- No `@Obsidian/ViewModels/*` imports.
- Top-level import statements only, because the compiler extracts them with a regex rather than a parser.
- Block ordering and script region ordering.

Then two or three canonical skeletons: a dashboard with charts and a grid, a form that posts, a list with a detail modal.

Probably the highest leverage item in this group, because it prevents whole classes of compile failure rather than diagnosing them afterward.

### LavaEndpointPatterns

The skill the SQL guardrail creates demand for.

- Returning JSON from entity commands.
- Aggregating without SQL, using `assign` and `increment`, or returning rows and aggregating in the component.
- `RawBody | FromJSON` for input. Worth preferring over the `Body` merge field, which is only populated when the request content type matches exactly.
- `ToJSON` for output, so a person's name containing an apostrophe cannot break the payload.
- The convention of reporting failure in the body with an `isSuccess` flag rather than in the status code.

---

## Group 2: The instance as source of truth

### RockVersion

Small and foundational. Everything else should key off it.

`modifyentity` and `deleteentity` are v18 and later. Without a version check, an agent confidently hands v18 syntax to a v16 church and the commands silently return nothing.

### RockEntityModel

Properties, types, nullability, foreign keys, enum values, read from the running instance.

Two things a published doc set structurally cannot provide:

- Custom and plugin entities on that specific instance.
- The entity's friendly name, which is what you need to construct the command name `modifyconnectionrequest`.

Include cascade behavior on child relationships. `ConnectionRequestActivity` does not cascade, which meant a delete endpoint written the obvious way would have failed on live data for any request that had activities.

### RockLavaReference

Commands, filters, tags and shortcodes actually registered on that instance, each with its enablement token.

This closes a precise, reproducible gap. The knowledge base article for Delete Entity documents the syntax and states that the command "runs only when it is enabled," but never says the token is `RockEntityDelete`. Finding that required grepping `RequiredPermissionKey` in the repo. An instance can read that property off its own registered blocks at runtime, and it picks up plugin-provided filters for free.

### RockRestApi

Route, verb, request and response bag shapes, required permission.

Lower priority now that `LavaDataSkill` exists, since the intended path is to write an endpoint rather than find one. Still needed for anything a component cannot reach through Lava.

### ObsidianCompiler

The assembler as runnable JavaScript, the pinned Vue version, and the alias map alongside it.

Without this a repo-less client cannot compile at all, and `SetContentSource` already instructs clients to use it. This is the single item that gates whether the flow works outside Claude Code.

---

## Group 3: The feedback loop

### ObsidianContentDiagnostics

`GetBlockRenderErrors(blockId)` and `InvokeEndpointAsCurrentPerson(applicationSlug, endpointSlug, body)`.

Almost every failure in the session presented as a blank block or an opaque 500, and each one took many round trips to localize. The second tool is nearly free given `TestExecute` already exists, but running with a real request context would catch exactly what `TestExecute` openly admits it cannot.

### LavaAppLifecycle

List and delete endpoints and applications, and list Obsidian Content blocks.

A mis-created endpoint could not be removed, which is what forced a detour into the admin UI. Agents iterate, so they need cleanup.

---

## Suggested build order

Build three first:

1. **ObsidianContentPatterns.** Prevents the most failures.
2. **RockLavaReference.** Closes a gap the knowledge base structurally cannot.
3. **ObsidianContentDiagnostics.** Turns a twenty-turn debug into a two-turn one.

**ObsidianCompiler** moves to the top the moment Claude Desktop matters, because nothing else in the flow works without it.

## Open question to settle first

Every skill in Group 1 assumes it can read control source, styles, and compiler code off the instance's disk. That holds for a development checkout. It is false for a deployed instance that ships only compiled output, which is the exact case Group 2 exists to serve.

So Group 1 as described works in precisely the environment that needs it least.

A manifest generated at build time and shipped with Rock is probably the real answer: control props, chart series shapes, grid column APIs, and the style token catalog are all knowable at compile time and none of them change per instance. Worth deciding this before writing three skills that assume source is present.
