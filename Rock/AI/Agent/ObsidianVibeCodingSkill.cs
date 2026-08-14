// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.ComponentModel;
using System.Text.RegularExpressions;

using Rock.AI.Agent.Annotations;
using Rock.Cms;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.AI.Agent
{
    /*
        7/22/2026 - CLAUDE

        First concrete code-based agent skill in core. It exposes the Obsidian
        Content authoring loop as MCP tools so an external AI client can read and
        write the Vue source of an ObsidianContentDetail block placement. It mirrors
        the block's own SaveContent action (upsert by BlockId, stamp CompiledDateTime,
        gate on EDIT) rather than inventing a second write path.

        Compiled output is validated WITHOUT executing it: the server has no JavaScript
        engine (by design, see spec 260722), so it can only confirm the payload looks
        like a System.register module and carries a Vue version. The client is
        responsible for producing a module that compiles; a bad module is recovered by
        the block's compile-on-view fallback.

        Reason: MCP-driven authoring of Obsidian Content, reusing the block's save path.

        8/6/2026 - CLAUDE

        The "no JavaScript engine" rationale above is superseded: the server now
        compiles source-only saves itself through ObsidianContentCompiler, which runs
        the same compiler bundle as the block's editor inside a Jint engine (see spec
        260806). A failed compile stores nothing and returns the compiler's errors so
        the agent can fix the source, because a saved-but-blank block with no error
        was the exact failure this feature exists to kill. Client-supplied compiled
        output still works unchanged, and the shape validation on it remains
        non-executing. Compile-on-view still does not exist; the only fallback path
        (compiler bundle not deployed) says so honestly instead of promising it.

        Reason: Server-side compile so repo-less MCP clients get a real feedback loop.

        8/6/2026 - CLAUDE

        Control discovery is delegated to the Rock knowledge base (knowledge.rockrms.com)
        rather than reimplemented here. That service already indexes every Framework
        Controls .obs file with a semantic description, a role classification, per-release
        version scoping, and a raw-source URL, which is the search-then-fetch shape an
        agent needs and is expensive to reproduce.

        This is instruction-level composition across two MCP servers, not an integration:
        Rock cannot see or verify the knowledge base's tools, so the usages below name
        tools that only resolve when the client has both servers connected. The fallback
        is stated explicitly in the usages so an agent without it says so rather than
        inventing control APIs from the control's name, which is the failure mode that
        produces components that compile and then render wrong.

        GetRockVersion exists to make those lookups version-correct. The knowledge base
        is scoped per Rock release, so an unscoped query silently answers for whatever
        release that service considers current, which is the wrong answer for any church
        not on it.

        Reason: Reuse the knowledge base for control discovery instead of rebuilding it.
    */

    /// <summary>
    /// Agent skill that lets an authorized administrator author the source of an
    /// <see cref="Model.ObsidianContent"/> block placement through the agent, rather
    /// than through the block's in-browser editor.
    /// </summary>
    [Description( "Author and edit the Vue source rendered by an Obsidian Content block placement." )]
    [AgentSkillName( "ObsidianVibeCoding" )]
    [AgentPurpose( "Author and edit the Vue source rendered by an Obsidian Content block placement." )]
    [AgentUsage( "Use to read or replace the authored source of an Obsidian Content block the user is building. The block must already exist on a page; identify it by its block id." )]
    [AgentUsage( "When the component needs data, create a Lava endpoint with the LavaData skill's CreateLavaEndpoint tool. Do not search Rock for an existing REST endpoint: writing Lava lets you return exactly the shape the component renders, with permissions decided when the endpoint is created." )]
    [AgentUsage( "Group all of one block's endpoints under a single Lava application named after the dashboard, by passing the same applicationSlug to every CreateLavaEndpoint call. Security and configuration rigging are then set once for the whole block." )]
    [AgentUsage( "In the component, import { useLavaApp } from '@Obsidian/Utility/lavaApp', bind the application once with useLavaApp('application-slug'), then call lavaApp.invoke('endpoint-slug'). Never hand-roll the endpoint URL, the CSRF header, or the JSON parsing: the helper is a framework import so a fix there reaches components that are already compiled and stored." )]
    [AgentUsage( "invoke returns the same shape as invokeBlockAction. Check isSuccess before reading data, show errorMessage when it fails, and render an empty state rather than an error when the call succeeds but legitimately has no rows." )]
    [AgentUsage( "Before writing a component, find the controls you need with the Rock knowledge base's search_code tool, passing source_type 'obs'. Search by concept, for example 'person picker' or 'grid with columns', rather than by a guessed filename." )]
    [AgentUsage( "Read a control's real API by fetching the file_url returned with each search result. The defineProps block is the authoritative list of props, their types, and their defaults, and the JSDoc comments above them explain what each one does. Never infer a control's props from its name or from a different control." )]
    [AgentUsage( "Call GetRockVersion first and pass that version to every knowledge base lookup. The knowledge base is scoped per Rock release, so an unscoped query answers for a release this instance may not be running. If a prop you found does not exist when the source fails to compile, suspect a version mismatch before anything else." )]
    [AgentUsage( "Controls under Framework/Controls/Internal/ are internal to Rock and are not meant for authored content. Prefer a top-level control, and if only an Internal one fits, tell the user before you use it." )]
    [AgentUsage( "If the knowledge base is not available to you, say so and ask the user how to proceed. Do not guess a control's props, and do not fall back to writing plain HTML in place of a Rock control without telling the user that is what you are doing." )]
    [Rock.SystemGuid.EntityTypeGuid( "4C833FA4-A7EF-4D49-9549-B24CBB629A73" )]
    [Rock.SystemGuid.AgentSkillGuid( "647770A9-F3D7-4924-B046-5C9C43959ECB" )]
    internal class ObsidianVibeCodingSkill : AgentSkillComponent
    {
        #region Constants

        /// <summary>
        /// A conservative structural check that the compiled payload is the SystemJS
        /// module shape the block's view path expects. This does not execute the
        /// module; it only confirms it is not free-form text or a mangled string.
        /// </summary>
        private static readonly Regex SystemRegisterShape = new Regex( @"^\s*System\.register\s*\(\s*\[", RegexOptions.Compiled );

        #endregion Constants

        #region Tools

        /// <summary>
        /// Reports the Rock version this instance is running, so an agent can scope
        /// external control and API lookups to the release actually deployed here.
        /// </summary>
        /// <returns>The semantic version number of this Rock instance.</returns>
        [AgentToolName( "GetRockVersion" )]
        [AgentToolPreamble( "Checking the Rock version." )]
        [AgentUsage( "Call this before looking up any control, filter, or API in the Rock knowledge base, and pass the returned version to that lookup. Control APIs change between releases, so an unscoped lookup can describe props this instance does not have." )]
        [AgentUsage( "This is the version of the Rock instance you are connected to. It is not the newest Rock release, and it is not the version any documentation defaults to." )]
        [Rock.SystemGuid.AgentToolGuid( "3E7A1C42-8B95-4D06-A1F3-2C64D9B7E508" )]
        public AgentToolResult GetRockVersion()
        {
            // No authorization gate: the version is already visible to anonymous
            // visitors in page markup and asset fingerprints, so this exposes nothing
            // that is not public, and every control lookup depends on it.
            return Success( new
            {
                Version = Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber(),
                FullVersion = Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber()
            } );
        }

        /// <summary>
        /// Reads the current authored source for a block placement so the agent can
        /// iterate on it.
        /// </summary>
        /// <param name="blockId">The id of the Obsidian Content block placement.</param>
        /// <returns>The authored source, or a NoData result when nothing is authored yet.</returns>
        [AgentToolName( "GetContentSource" )]
        [AgentToolPreamble( "Reading the current Obsidian content source." )]
        [AgentUsage( "blockId is the id of the Obsidian Content block placement to read." )]
        [Rock.SystemGuid.AgentToolGuid( "7D3A8200-3A90-44CC-9E30-B600383E835F" )]
        public AgentToolResult GetContentSource( string blockId )
        {
            using ( var rockContext = new RockContext() )
            {
                var block = new BlockService( rockContext ).Get( blockId, allowIntegerIdentifier: false );

                if ( block == null )
                {
                    return Error( "The block was not found." );
                }

                // Source is only exposed to editors; a plain viewer never receives it,
                // matching the block's own view-mode behavior.
                var blockCache = BlockCache.Get( block.Id );

                if ( blockCache == null || !blockCache.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
                {
                    return Error( "You are not authorized to read this content." );
                }

                var content = new ObsidianContentService( rockContext ).GetByBlockId( block.Id );

                if ( content == null || content.Source.IsNullOrWhiteSpace() )
                {
                    return NoData();
                }

                return Success( new { content.Source, content.CompiledVueVersion } );
            }
        }

        /// <summary>
        /// Upserts the authored source (and, optionally, the client-compiled output)
        /// for a block placement. This is the agent-facing equivalent of the block's
        /// SaveContent action.
        /// </summary>
        /// <param name="blockId">The id of the Obsidian Content block placement.</param>
        /// <param name="source">The authored Vue source.</param>
        /// <param name="compiledContent">The client-compiled SystemJS module, or null to have the server compile the source.</param>
        /// <param name="compiledVueVersion">The Vue version the client compiled against. Required when <paramref name="compiledContent"/> is provided.</param>
        /// <returns>A success result, or an error describing why the content was rejected.</returns>
        [AgentToolName( "SetContentSource" )]
        [AgentToolPreamble( "Saving the Obsidian content source." )]
        [AgentUsage( "blockId is the block placement to write; source is the authored Vue single-file-component." )]
        [AgentUsage( "Pass source only: the server compiles it and either stores the result or returns the compile errors for you to fix and retry. Nothing is stored when the compile fails. Provide compiledContent (and compiledVueVersion) only if you compiled the source yourself; most clients should not." )]
        [Rock.SystemGuid.AgentToolGuid( "26FFEE94-4868-4DEC-BE40-68FBE30DAEB8" )]
        public AgentToolResult SetContentSource( string blockId, string source, string compiledContent = null, string compiledVueVersion = null )
        {
            if ( source.IsNullOrWhiteSpace() )
            {
                return Error( "No source was provided." );
            }

            // Non-executing validation of any supplied compiled output. The server has no
            // JavaScript engine, so it cannot confirm the module compiles or runs; it only
            // confirms the shape and that a version is present.
            var hasCompiled = compiledContent.IsNotNullOrWhiteSpace();

            if ( hasCompiled )
            {
                if ( compiledVueVersion.IsNullOrWhiteSpace() )
                {
                    return Error( "compiledVueVersion is required when compiledContent is provided." );
                }

                if ( !SystemRegisterShape.IsMatch( compiledContent ) )
                {
                    return Error( "compiledContent does not look like a compiled component module. Pass source only and let the server compile it." );
                }
            }

            using ( var rockContext = new RockContext() )
            {
                var block = new BlockService( rockContext ).Get( blockId, allowIntegerIdentifier: false );

                if ( block == null )
                {
                    return Error( "The block was not found." );
                }

                // Authoring runs as the visitor in the browser and can call any API the
                // visitor can, so writing is gated to administrators (EDIT authorization),
                // exactly as the block's SaveContent action gates it.
                var blockCache = BlockCache.Get( block.Id );

                if ( blockCache == null || !blockCache.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
                {
                    return Error( "You are not authorized to edit this content." );
                }

                // When the client did not compile the source itself, compile it here so
                // the agent gets real compile errors back while it can still fix them.
                // A failed compile stores nothing: a saved-but-blank block with no error
                // anywhere is the exact failure mode this path exists to prevent.
                var isServerCompilerUnavailable = false;

                if ( !hasCompiled )
                {
                    var compileResult = new ObsidianContentCompiler().CompileSource( source );

                    if ( compileResult.IsSuccess )
                    {
                        compiledContent = compileResult.CompiledContent;
                        compiledVueVersion = compileResult.VueVersion;
                        hasCompiled = true;
                    }
                    else if ( compileResult.IsBundleMissing )
                    {
                        // A half-deployed instance should not lose the ability to save
                        // source, but the caller must hear the honest consequence below.
                        isServerCompilerUnavailable = true;
                    }
                    else if ( compileResult.IsBrowserMissing )
                    {
                        /*
                            8/14/2026 - CLAUDE

                            Distinct from every other failure because it is transient and
                            not the caller's fault. Compiling runs in the headless Chromium
                            Rock also uses for PDFs, and that build is downloaded on first
                            use rather than shipped. On an instance that has never generated
                            a PDF it is simply not there yet.

                            The compile path deliberately refuses to trigger that download:
                            it is on the order of a hundred megabytes and an agent is waiting
                            on a tool call. So nothing is stored, and the agent is told to
                            retry rather than to fix source that was never the problem.

                            Reason: A missing browser is a wait, not a compile error.
                        */
                        return Error( "The server could not compile because its browser engine is still being provisioned. Nothing was saved. This is not a problem with your source. Tell the user the instance needs its PDF/browser engine installed, which happens automatically the first time a PDF is generated, and try SetContentSource again in a few minutes." );
                    }
                    else
                    {
                        /*
                            8/13/2026 - KH (captured by CLAUDE)

                            IDEA, NOT BUILT: when the refusal comes from the complexity
                            guard rather than from a real compile error, tell the agent it
                            can keep the component as-is by saving it through the block's
                            browser editor, which compiles in the visitor's V8 instead of
                            Jint and has far more stack to work with.

                            This looks workable and mostly reuses machinery that exists.
                            One thing has to change first: today a failed compile stores
                            NOTHING, so there would be no draft for anyone to open and
                            save. The guard path would have to store Source without
                            CompiledContent, which is exactly what the bundle-missing
                            fallback below already does, including the honest warning that
                            the block will not render until an administrator saves it in
                            the editor.

                            Worth settling before building:

                            - It softens the guard from a wall into a routing decision:
                              server compile for ordinary components, human-in-the-loop
                              browser compile for exceptional ones. That is arguably the
                              better product behavior, but it does reintroduce the
                              saved-but-blank state this feature exists to prevent, so the
                              message has to be unmissable.
                            - Untested whether the browser actually survives sources this
                              deep. V8 has far more headroom than Jint, and a failure there
                              costs a tab rather than the worker process, but nobody has
                              confirmed where its limit sits.
                            - Only offer it for a guard refusal. A genuine compile error
                              means broken source, and sending that to the editor just
                              moves the same error somewhere less visible.

                            Reason: Capturing an escape hatch for legitimately complex
                            components instead of refusing them outright.
                        */
                        return Error( "The source failed to compile. Fix the source and call SetContentSource again. Compiler errors:\n" + string.Join( "\n", compileResult.Errors ) );
                    }
                }

                var service = new ObsidianContentService( rockContext );
                var content = service.GetOrCreateByBlockId( block.Id );

                content.Source = source;
                content.CompiledContent = hasCompiled ? compiledContent : null;
                content.CompiledVueVersion = hasCompiled ? compiledVueVersion : null;
                content.CompiledDateTime = hasCompiled ? ( System.DateTime? ) RockDateTime.Now : null;

                rockContext.SaveChanges();

                var result = Success( new { block.IdKey } );

                // Only the bundle-missing fallback stores uncompiled source; be honest
                // about what that means instead of promising a compile that never runs.
                if ( isServerCompilerUnavailable )
                {
                    result.WithInstructions( "Source saved without compiled output because this server's compiler bundle is not deployed. The content will not render until an administrator opens the block's editor and saves it there, which compiles it in the browser. Tell the user this plainly." );
                }

                return result;
            }
        }

        #endregion Tools
    }
}
