---
author: Kyle Henning
date_created: 2026-08-06
summary: >-
  Implementation plan for compiling Obsidian Content SFC source server-side
  with an in-process Jint engine, so repo-less MCP clients (Claude Chat,
  Claude Desktop) get real compile results instead of saving source that
  never renders. Replaces the planned GetCompiler tool and the hosted
  Spark compile service idea.
contributors: []
related_docs:
  - docs/cms/obsidian-content.md
---

# Implementation Plan: In-Process Jint Compile for Obsidian Content

This plan is addressed to a Claude Code session implementing it in the Rock repo. Read [260804-vibe-coding-findings.md](260804-vibe-coding-findings.md) and [260722-mcp-driven-obsidian-content-vibe-coding.md](260722-mcp-driven-obsidian-content-vibe-coding.md) first for the why.

**Every design decision is already made. Do not reopen them.** If you find something that contradicts this plan, say so and stop rather than choosing a different approach on your own.

## Summary

Today the Obsidian Content authoring loop only works when the MCP client can compile Vue single-file components itself (Claude Code with a repo checkout). A client with no repo and no JavaScript runtime (Claude Chat, Claude Desktop) can only save source, and `SetContentSource` stores it with a false promise that it will compile on next view. Nothing compiles it. The block renders blank with no error anywhere.

The fix: Rock compiles the source itself, in process, by running the existing browser compiler bundle inside a Jint JavaScript engine. One compiler, running on the church's own server, no new infrastructure, no source leaving the instance. The agent gets structured compile errors back synchronously and can iterate.

## Motivation

- Three of five MCP steps in the vibe coding flow currently depend on reading the Rock repo off disk. The compile step is the fatal one: `SetContentSource` rejects anything that is not a `System.register` module, and a repo-less client cannot produce one.
- The previously planned `GetCompiler` MCP tool only helps clients that can execute JavaScript. Claude Chat cannot.
- A Spark-hosted compile service was considered and set aside: it creates two compilers in production (browser and service) that must agree forever, plus a permanent hosting, versioning, and data-egress obligation. See Considered but Rejected.
- Compile-on-view was explicitly deferred by Kyle Henning ("hold on to the compile on view, we may come back to it"). It is out of scope here.

## Ground rules

- Follow `CLAUDE.md` and everything in `.claude/rules/`. Match surrounding code rather than introducing new patterns.
- **No em-dashes and no `--` in any file, comment, or string.**
- **Do not run any build.** No MSBuild, no `dotnet build`, no `npm run build`, no build skill. When a phase is done, offer to build and wait. The exception is Phase 0, which explicitly requires compiling and running its own spike harness; ask before running it.
- Copyright headers on every new file. C# and `.ts` use the multi-line block, `.obs` uses the one-liner. Templates are in `.claude/rules/code-conventions.md`.
- Work on the existing `feature-kh-obsidian-content` branch.
- Commit per phase using the format in `CLAUDE.md`. Domain is `CMS`.
- Everything new in `Rock.dll` is `internal` and, where visible to reflection or plugins, marked `[RockInternal( "18.0" )]`.

## Prerequisites

This sits on top of the Obsidian Content prototype already on the branch: the `ObsidianContent` entity, the `ObsidianContentDetail` block, the browser compile pipeline in `Rock.JavaScript.Obsidian.Blocks/src/Cms/obsidianContentDetail/obsidianContentCompiler.partial.ts`, and the `ObsidianVibeCodingSkill` MCP tools. Do not modify any of it except where a phase says so.

Key existing facts, verified against the branch:

| Fact | Where |
|---|---|
| The SFC compiler ships as an Obsidian lib, self-contained browser build, ~747KB | `RockWeb/Obsidian/Libs/vueCompilerSfc.js`, source `Rock.JavaScript.Obsidian/Framework/Libs/vueCompilerSfc.ts` (one line: `export * from "@vue/compiler-sfc"`) |
| Lib mode bundles every dependency into one file except `vue`, which stays external | `Rock.JavaScript.Obsidian/Build/build-tools.js` (the `lib` option, and the `target === "vue"` special case around line 292) |
| The orchestration logic (parse, compile script and template and styles, assemble `System.register`) lives in the block partial | `obsidianContentCompiler.partial.ts`, exports `loadCompilerAsync`, `compileSource`, `ObsidianContentCompileResult` |
| The version stamped into compile results comes from `import { version } from "vue"` | `obsidianContentCompiler.partial.ts:46` |
| A minimal `System.register` shim already exists as a pattern | `viewPanel.partial.obs` (`instantiateModule`, around lines 57 to 88) |
| `Rock.dll` resolves web root paths without `System.Web` via `RockApp.Current.MapPath` | `Rock/Web/ObsidianFingerprintManager.cs:99` |
| Rock targets `net472`; `Rock.csproj` uses `PackageReference` | `Rock/Rock.csproj` |
| `SetContentSource` validates compiled output with a shape regex and promises compile-on-view when source-only | `Rock/AI/Agent/ObsidianVibeCodingSkill.cs:71` (regex), `:133` (tool), `:193` (false promise), `:131` (AgentUsage referencing the unbuilt GetCompiler) |

If a line reference does not match what you find, trust the code and say so.

---

## Phase 0: The Jint spike (gate for everything else)

Prove that Jint on net472 can run the compiler bundle at all, before touching the repo.

### 0.1 Build a throwaway harness

Create a small console project OUTSIDE the repo (scratchpad or a sibling folder, never committed). Target `net472`. Reference the newest Jint release that targets `netstandard2.0` or `net462` (verify the target framework on nuget.org before pinning; do not take a version that requires `netstandard2.1`).

### 0.2 Load and exercise the compiler

1. Read `RockWeb/Obsidian/Libs/vueCompilerSfc.js` from the working tree (build it first if missing; ask before running that build).
2. The lib is a SystemJS-format module. Provide a JS prelude that shims `System.register` the same way `instantiateModule` in `viewPanel.partial.obs` does: capture the register callback, feed setters, run execute, collect exports. Handle a dependency on `"vue"` by supplying a stub module (an object with a `version` string is enough for the spike).
3. Call `parse` and `compileScript`/`compileTemplate` on two fixtures: a trivial `<template>` plus `<script setup>` component, and one that also has `<style scoped>`.
4. Time the cold path (engine create through dispose) and the compile call separately.
5. Configure the engine with a timeout constraint and a recursion limit so a hang fails fast.

### 0.3 The gate

- **Pass:** both fixtures compile, output contains a render function, no regex or syntax errors from Jint, and cold-path time is tolerable for an interactive save (single-digit seconds).
- **Fail:** stop the entire plan and report exactly what broke (the syntax construct or regex feature Jint rejected, with the error text). The fallback to ClearScript is a decision for Kyle Henning, not for this session. Do not start Phase 1 on a failed spike.

Report the measured timings either way; they go in the summary.

### Done when

The gate result and timings are reported. Nothing is committed in this phase.

### Result (executed 2026-08-06): PASS

Jint 4.15.3 (ships `lib/net462`, runs on net472; pin this version in Phase 2) ran the shipped `vueCompilerSfc.js` bundle on the first try. The bundle registers with an empty dependency array, so no `vue` stub is needed, and it already exports `version` ("3.3.10", matching the lockfile), confirming the Phase 1 version assumption.

Timings, Release build. The figures below were re-measured after Phase 1 against the real shipped bundle, with the .NET JIT warmed, which is what a long-running IIS worker actually experiences. Earlier spike numbers (bundle load of 1000 ms or more) included first-time JIT of Jint's own code in a fresh console process and overstate the steady-state cost.

Fixed cost, independent of content: **208 to 288 ms** to create the engine, load the 742KB bundle, and dispose.

Full cold path per request, create through dispose:

| Source size | Total | Output module |
|---|---|---|
| 604 chars | 523 ms | 2.3 KB |
| 2.4 KB | 761 ms | 6.1 KB |
| 9.3 KB | 1587 ms | 20.7 KB |
| 27.8 KB | 3699 ms | 59.9 KB |

**Cost scales linearly with source size above a fixed floor.** The synthetic data fits `450 ms + 120 ms per KB` closely (predicted 521 / 721 / 1512 / 3636 against measured 523 / 761 / 1587 / 3699). There is no quadratic knee.

**Validated against real authored components** (exported from a development database, which is the number to trust). Real code compiles roughly 1.7x faster than the synthetic fixtures, because those fixtures were unusually dense in `v-for` and `computed` constructs relative to ordinary markup:

| Real source | Synthetic model predicted | Actually measured |
|---|---|---|
| 14.97 KB | 2246 ms | **1307 ms** |
| 21.37 KB | 3014 ms | **1759 ms** |
| 29.89 KB | 4036 ms | **2375 ms** |

Refit on real content: **`235 ms + 72 ms per KB`**, an almost exact linear fit (predicts 1765 / 2375 against measured 1759 / 2375). Use this model, not the synthetic one.

**Sizing consequences.** Real authored components reach ~30 KB, considerably larger than early estimates assumed, so do not size limits against a 2 to 5 KB assumption. At the refit rate the 10 second timeout in Phase 2 allows roughly **135 KB** of source, and the largest observed real component consumes about 24% of that budget. The timeout is appropriately sized; revisit it only if authored components routinely exceed 50 KB.

Do not quote a single figure without its source size. In particular, the 425 ms seen in the Phase 4 test output belongs to the broken-template test, which throws at `parse` and never reaches template or style compilation, so it measures the cost of failing rather than the cost of compiling.

Both fixtures produced real render output (scoped style fixture emitted the `data-v-` scope id and compiled CSS), `new Function` works (production uses it for parse validation), and a broken fixture surfaced a clean compiler message ("Interpolation end sign was not found.").

**One incompatibility found, with a required workaround:** Jint's `Function.prototype.toString` returns `function () { [native code] }` instead of source text. The bundled `source-map-js` regenerates its own sort function via `new Function("return " + fn.toString())`, which then fails to parse. This fires only when compileScript merges source maps in the `inlineTemplate` path. The fix is to disable source map generation, which production wants anyway since maps are discarded:

```ts
compileScript( descriptor, {
    id,
    inlineTemplate: true,
    sourceMap: false,
    templateOptions: { compilerOptions: { sourceMap: false } }
} );
```

Phase 1 MUST carry these options into the shared lib's `compileSource` so both hosts run the identical code path. Everything else in the production compile flow (`parse`, `rewriteDefault`, `compileStyle`, the `new Function` validation) worked unmodified.

---

## Phase 1: Extract the shared compiler bundle

One compiler, one bundle, consumed by both the browser editor and the server engine.

### 1.1 Create the lib

New file: `Rock.JavaScript.Obsidian/Framework/Libs/obsidianContentCompiler.ts`

Move the orchestration logic out of `obsidianContentCompiler.partial.ts` into this file: the import splitter, clause parser, scope id hash, style injection builder, `buildSystemJsModule`, and `compileSource`, along with the `ObsidianContentCompileResult` type. This becomes the single implementation.

Three changes during the move:

1. **Import the compiler package directly.** `import { parse, compileScript, ... } from "@vue/compiler-sfc"` instead of the dynamic `@Obsidian/Libs/vueCompilerSfc` load. Lib mode bundles it, so this file becomes the one self-contained bundle (~750KB, same weight as today, loaded on demand only in edit mode and by the server).
2. **Take the version from the compiler package, not from `vue`.** `@vue/compiler-sfc` is published in lockstep with `vue` and exports its own `version`. Use that for `ObsidianContentCompileResult.vueVersion`, and drop the `import { version } from "vue"` entirely. This is what makes the bundle self-contained in both hosts (in Jint there is no import map to resolve `vue`). **Verify the `version` export exists in the pinned `@vue/compiler-sfc` version before relying on it. If it does not exist, stop and report; do not improvise a substitute.**

The exported surface of the lib is `compileSource(source: string): ObsidianContentCompileResult` plus the result type. Synchronous, no loader function needed (the bundle carries the compiler).

3. **Disable source map generation** in the `compileScript` call, exactly as the Phase 0 result records (`sourceMap: false` plus `templateOptions.compilerOptions.sourceMap: false`). This is mandatory for the Jint host and harmless in the browser; maps were always discarded.

### 1.2 Thin out the block partial

`obsidianContentCompiler.partial.ts` keeps its exported API (`loadCompilerAsync`, `compileSource`, `ObsidianContentCompileResult`) so `editPanel.partial.obs` and `previewPanel.partial.obs` do not change. Internally it becomes a thin wrapper: `loadCompilerAsync` dynamic-imports `@Obsidian/Libs/obsidianContentCompiler` and caches the module; `compileSource` delegates to it. Delete the moved logic.

### 1.3 Retire the old lib

`Rock.JavaScript.Obsidian/Framework/Libs/vueCompilerSfc.ts` exists only to feed the partial. Grep for any other reference to `@Obsidian/Libs/vueCompilerSfc`; if the partial was the only consumer, delete the lib source file so the 747KB bundle is not shipped twice. If anything else references it, leave it and say so in the summary.

### Done when

The new lib source exists, the partial delegates to it, the old lib is removed (or its retention explained), and `npm run lint` in the Obsidian project would have nothing new to complain about (do not run the build; a lint run is acceptable if offered and approved).

---

## Phase 2: The Rock-side compile service

### 2.1 Add Jint

Add the Jint `PackageReference` to `Rock/Rock.csproj`, pinned to the exact version the spike validated. Alphabetical placement among the existing references.

### 2.2 The service class

New file: `Rock/Cms/ObsidianContentCompiler.cs`

Internal static-free class (no class-level mutable state; Rock singleton rules apply) with one public-to-Rock entry point:

```csharp
internal class ObsidianContentCompiler
{
    /// <summary>Compiles authored SFC source into a SystemJS module using the shared compiler bundle.</summary>
    public ObsidianContentCompileResult CompileSource( string source )
}
```

Result POCO (same file or sibling): `IsSuccess`, `CompiledContent`, `VueVersion`, `Errors` (list of strings, raw compiler messages including any line information the compiler emits).

Behavior:

1. Resolve the bundle path with `RockApp.Current.MapPath( "~/Obsidian/Libs/obsidianContentCompiler.js" )`, mirroring `ObsidianFingerprintManager.cs:99`. If the file is missing, return a failed result saying the compiler bundle is not deployed; never throw for that.
2. Create a new Jint engine per call. Constrain it: execution timeout (10 seconds), recursion limit, memory limit. No engine caching, no statics; the engine is created, used once, and disposed. Add an engineering note explaining why (steady-state memory stays zero; compiles are rare, admin-initiated, and a human is waiting).
3. Evaluate the shim prelude, then the bundle, then call the exported `compileSource`. The shim mirrors `instantiateModule` in `viewPanel.partial.obs`: a minimal `System.register` capture. After Phase 1 the bundle should have an empty dependency array; the shim must still handle a non-empty one defensively.
4. The lib throws an `Error` with joined parse messages on bad source. Catch the Jint script exception, put the message text into `Errors`, return `IsSuccess = false`. Catch timeout and constraint violations separately with a distinct message ("compilation exceeded limits").
5. Validate the returned module string against the same `System.register` shape check `SetContentSource` uses before declaring success.
6. **Never execute the compiled output.** The engine compiles; it does not run the result.

Read the bundle file contents once per call (no caching of the file either; it changes on deploy and this path is cold).

### Done when

The service class exists, compiles conceptually against the patterns above, and has zero class-level state. Offer a build; do not run one.

---

## Phase 3: Wire it into the MCP tool

`Rock/AI/Agent/ObsidianVibeCodingSkill.cs`

### 3.1 SetContentSource compiles when the client did not

In `SetContentSource` (around line 133): when `compiledContent` is null or whitespace, call `ObsidianContentCompiler.CompileSource( source )` after the authorization check and before the write.

- **Success:** store `Source`, the compiled output, the returned `VueVersion`, and `CompiledDateTime = RockDateTime.Now`, exactly as the client-compiled path does.
- **Failure:** do NOT store anything. Return `Error(...)` carrying the compiler messages so the agent fixes the source and retries. This is the feedback loop the whole feature exists for; a failed compile must never result in a saved-but-blank block.
- **Bundle not deployed:** fall back to the current source-only save so a half-deployed instance does not lose the ability to save, but change the instruction text to say the content is saved but not compiled and an administrator must open the editor and save to compile it. No promise of compile-on-view.

Client-supplied `compiledContent` keeps working unchanged (shape regex and required `compiledVueVersion`), so the Claude Code path is untouched.

### 3.2 Fix the words

- Remove the `WithInstructions` text at line 193 that promises compile-on-view. On the server-compiled path it is replaced by success; on the bundle-missing path use the honest wording above.
- Update the `AgentUsage` at line 131 that tells clients to use "the compiler from GetCompiler". New guidance: pass source only; the server compiles it and returns compile errors to fix. Supplying `compiledContent` remains optional for clients that compile themselves.
- Update the class-level engineering note (the 7/22/2026 CLAUDE note) to reflect that the server now compiles via Jint, replacing the "server has no JavaScript engine" rationale. Keep the note's history; append rather than rewrite.

### Done when

A source-only `SetContentSource` call either stores compiled output or returns compiler errors, and no text anywhere in the skill promises compile-on-view or references GetCompiler.

---

## Phase 4: Tests

New test class in `Rock.Tests` (follow the project's existing folder and naming conventions for integration-style tests).

1. **Round trip:** compile a fixture SFC with `<template>`, `<script setup>`, and `<style scoped>`. Assert `IsSuccess`, output matches the `System.register` shape, output contains the style injection guard, and `VueVersion` is a non-empty semver.
2. **Error path:** compile a fixture with a deliberate template syntax error. Assert `IsSuccess` is false and `Errors` is non-empty.
3. **Limits path:** if practical without fragility, a fixture that trips the timeout; if that proves flaky, skip it and say so.

The tests need the built bundle on disk. Resolve it relative to the solution (`RockWeb/Obsidian/Libs/obsidianContentCompiler.js`) and mark the tests inconclusive when the file is absent rather than failing, since the Obsidian build may not have run in every environment. Do not check the bundle into the test project.

### Done when

The test file exists and expresses the three cases. Offer to run the tests; do not run them unprompted.

---

## Phase 5: Documentation

Update `docs/cms/obsidian-content.md`: the compile pipeline section gains the server-side path (agent saves source, Rock compiles in process via Jint, errors return to the agent), and any statement that the server cannot compile is corrected. Follow the docs skill conventions if invoked; a direct edit matching the file's existing style is also acceptable.

### Done when

The doc describes both compile paths (browser editor, server via MCP) accurately.

---

## Decisions already settled

Do not revisit these.

| Decision | Why |
|---|---|
| In-process engine, not a Spark-hosted service | One compiler instead of two that must agree forever; no hosting, version matrix, or data egress; nothing new for churches to install or notice. |
| Jint, not ClearScript, gated by the Phase 0 spike | Pure managed, no native binaries, zero deploy friction. ClearScript is the fallback and only Kyle decides to take it. |
| Engine per compile, no caching | Compiles are rare and admin-initiated; steady-state memory must be zero, especially on web farms. Seconds of load with a human waiting is acceptable. |
| One shared bundle consumed by browser and server | The extraction in Phase 1 is what makes "one compiler" true rather than aspirational. |
| Version comes from `@vue/compiler-sfc`'s own export | Removes the external `vue` import so the bundle is self-contained in a host with no import map. The packages version in lockstep. |
| Failed server compile stores nothing | A saved-but-blank block with no error is the exact bug this feature kills; never recreate it. |
| Browser editor keeps compiling client-side | Consolidating the block's own save path onto the server compiler is a future cleanup, not this plan. |
| The server never executes compiled output | Compilation only. The output runs in browsers, gated by the same authorization as today. |
| Everything internal, `[RockInternal( "18.0" )]` where visible | The API surface is unconfirmed; graduate later per the RockInternal convention. |

## Considered but Rejected

### Spark-hosted stateless compile service
Rejected. Permanent operational commitment (uptime, per-Rock-version Vue matrix, abuse handling), authored source leaving church infrastructure (disclosure and opt-in burden), and it structurally guarantees two compilers in production whose outputs must match forever.

### Hand-written `h()` render modules from the agent
Rejected as the primary path. It works (the runtime does not care where render functions come from) but gives up compile-time error checking, makes scoped styles manual, and leaves `Source` either unreadable or unverified. Kyle wants human-readable source that compiles.

### Compile-on-view in the admin's browser
Deferred, not rejected. Held by Kyle as a possible fallback for instances where the engine path fails. Out of scope here; do not build it, and do not remove anything that would make it harder to build later.

### ClearScript with V8
Held as fallback only. Real native binaries per architecture and deploy friction that Jint avoids. Becomes relevant only if the Phase 0 spike fails, and that call is Kyle's.

## Out of scope

- Compile-on-view (explicitly held).
- Switching the block's own `SaveContent` action to the server compiler.
- The other missing MCP skills from [260803-mcp-vibe-coding-skill-candidates.md](260803-mcp-vibe-coding-skill-candidates.md) (control catalog, styling tokens, diagnostics, lifecycle).
- A public or plugin-facing compile API.
- Rebasing the branch onto `develop` or regenerating the migration snapshot (known, tracked separately in the findings spec).

## Verification Steps

1. Phase 0 spike passes with reported timings.
2. Build the solution and the Obsidian projects (with approval) and confirm zero new errors.
3. In a running instance: place an Obsidian Content block, then through MCP call `SetContentSource` with source only. Confirm the response is success, the `[ObsidianContent]` row has non-null `CompiledContent` and `CompiledVueVersion`, and the page renders the component for a plain viewer.
4. Send deliberately broken source the same way. Confirm the tool returns the compiler's error text and the row is unchanged.
5. Open the block's browser editor, edit, and save. Confirm the client-compile path still works and produces equivalent output for the same source.
6. Run the new Rock.Tests class.

## Related

- [obsidian-content-sizes.sql](artifacts/260806-jint-in-process-obsidian-compile-plan/obsidian-content-sizes.sql) (diagnostic query: per-block source and compiled sizes, expansion ratio, estimated compile time, and an uncompiled-block detector)
- [260804-vibe-coding-findings.md](260804-vibe-coding-findings.md) (the gap analysis this plan answers)
- [260722-mcp-driven-obsidian-content-vibe-coding.md](260722-mcp-driven-obsidian-content-vibe-coding.md) (the MCP authoring design; its GetCompiler tool is superseded by this plan)
- [260721-obsidian-content-block-and-component-model.md](260721-obsidian-content-block-and-component-model.md) (the block and entity this builds on)
- [260803-lava-endpoint-implementation-plan.md](260803-lava-endpoint-implementation-plan.md) (format precedent for this document)

## Reporting back

For each phase, state what you changed, what you did not change and why, and anything you found that contradicts this plan. Do not report a phase complete until every file in it is done. If a phase is blocked, finish the others where dependencies allow and say plainly which one you left and why. Phase 0's gate overrides everything: a failed spike stops the plan.
