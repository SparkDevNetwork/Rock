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
        /// <param name="compiledContent">The client-compiled SystemJS module, or null to store source only.</param>
        /// <param name="compiledVueVersion">The Vue version the client compiled against. Required when <paramref name="compiledContent"/> is provided.</param>
        /// <returns>A success result, or an error describing why the content was rejected.</returns>
        [AgentToolName( "SetContentSource" )]
        [AgentToolPreamble( "Saving the Obsidian content source." )]
        [AgentUsage( "blockId is the block placement to write; source is the authored Vue single-file-component." )]
        [AgentUsage( "Provide compiledContent (and compiledVueVersion) when you have compiled the source yourself with the compiler from GetCompiler; otherwise pass source only and the block compiles on the next administrator view." )]
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
                    return Error( "compiledContent does not look like a compiled component module. Compile the source with the compiler from GetCompiler." );
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

                var service = new ObsidianContentService( rockContext );
                var content = service.GetOrCreateByBlockId( block.Id );

                content.Source = source;
                content.CompiledContent = hasCompiled ? compiledContent : null;
                content.CompiledVueVersion = hasCompiled ? compiledVueVersion : null;
                content.CompiledDateTime = hasCompiled ? ( System.DateTime? ) RockDateTime.Now : null;

                rockContext.SaveChanges();

                var result = Success( new { block.IdKey } );

                // When the client could not compile, tell it how the content becomes live so
                // it can set expectations with the user.
                if ( !hasCompiled )
                {
                    result.WithInstructions( "Source saved without compiled output. The content will render after an administrator next views the page, or once you provide compiledContent." );
                }

                return result;
            }
        }

        #endregion Tools
    }
}
