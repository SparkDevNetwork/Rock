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

using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.ForgeContentBuilderSkill;
using Rock.Cms;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ForgeContentBuilderSkill
{
    #region Tool(s)

    [Description( "Compiles and saves the authored source of a Forge Content block placement. A failed compile stores nothing and returns the compiler's errors." )]
    [AgentToolPreamble( "Compiling and saving the component source." )]
    [AgentUsage( "blockId is the block placement to write; source is the authored Vue single-file-component. The server compiles the source and either stores the result or returns the compile errors for you to fix and retry. Nothing is stored when the compile fails." )]
    [AgentUsage( "Never save a component whose data is hardcoded, mocked, or invented while presenting it as real. If the endpoints it needs could not be created, stop and report what failed instead of shipping fake numbers." )]
    [AgentToolGuid( "3E97A0C5-48D2-4F16-85B9-C1D7E63A2F40" )]
    public AgentToolResult AddOrUpdateForgeContent(
        [Description( "The id of the Forge Content block placement to write." )]
        string blockId,

        [Description( "The authored Vue single-file-component source." )]
        string source )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        if ( source.IsNullOrWhiteSpace() )
        {
            helper.AddError( "No source was provided." );
        }

        var block = helper.GetRequiredEntity<Model.Block>( blockId, checkSecurity: false );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Authored code runs in the visitor's browser as the visitor and can call
        // any API the visitor can, so writing is gated to administrators (EDIT
        // authorization), exactly as the block's SaveContent action gates it.
        var blockCache = BlockCache.Get( block.Id, rockContext );

        if ( blockCache == null || !blockCache.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            helper.AddError( "You are not authorized to edit this component." );

            return helper.ErrorResult;
        }

        // The one compile path, shared with the block's own save action. A failed
        // compile stores nothing: a saved-but-blank block with no error anywhere
        // is the exact failure mode this path exists to prevent.
        var compileResult = new ForgeContentCompiler().CompileSource( source );

        if ( compileResult.IsBrowserMissing )
        {
            /*
                8/17/2026 - CLAUDE

                Distinct from every other failure because it is transient and not
                the caller's fault. Compiling runs in the headless Chromium Rock
                also uses for PDFs, and that build is downloaded on first use
                rather than shipped. On an instance that has never generated a PDF
                it is simply not there yet.

                The compile path deliberately refuses to trigger that download: it
                is on the order of a hundred megabytes and an agent is waiting on
                a tool call. So nothing is stored, and the agent is told to retry
                rather than to fix source that was never the problem.

                Reason: A missing browser is a wait, not a compile error.
            */
            helper.AddError( "The server could not compile because its browser engine is still being provisioned. Nothing was saved. This is not a problem with your source. Tell the user the instance needs its PDF/browser engine installed, which happens automatically the first time a PDF is generated, and try AddOrUpdateForgeContent again in a few minutes." );

            return helper.ErrorResult;
        }

        if ( compileResult.IsRenderEndpointUnreachable )
        {
            // A configuration problem, not a wait and not the caller's source.
            // The compiler's message names the endpoint and the setting.
            helper.AddError( string.Join( "\n", compileResult.Errors ) + " Nothing was saved. This is not a problem with your source; tell the user their configured external render endpoint appears to be down." );

            return helper.ErrorResult;
        }

        if ( compileResult.IsBundleMissing )
        {
            // Nothing is stored, deliberately. An earlier design saved source-only
            // here and relied on the block's in-browser compile to recover it, but
            // the browser no longer compiles, so a source-only save would just be
            // a component that silently never renders.
            helper.AddError( "The compiler bundle is not deployed on this server, so the component cannot be compiled. Nothing was saved. Tell the user this Rock instance appears to be missing its Obsidian build output." );

            return helper.ErrorResult;
        }

        if ( !compileResult.IsSuccess )
        {
            var compileError = "The source failed to compile. Fix the source and call AddOrUpdateForgeContent again. Compiler errors:\n" + string.Join( "\n", compileResult.Errors );

            // TypeScript syntax is the single most common weak-model compile
            // failure, and the raw compiler error rarely names it. Detect the
            // telltales and say the fix plainly so the retry succeeds.
            if ( System.Text.RegularExpressions.Regex.IsMatch( source, "lang=\"ts\"|lang='ts'|:\\s*(string|number|boolean|any|void)\\b|\\binterface\\s+\\w+|\\bas\\s+(string|number|boolean|any)\\b" ) )
            {
                compileError += "\nThe source appears to contain TypeScript syntax. Authored components are plain JavaScript: remove lang=\"ts\" from the script tag and strip every type annotation, interface declaration, generic, and 'as' cast, then save again.";
            }

            helper.AddError( compileError );

            return helper.ErrorResult;
        }

        var content = new ForgeContentService( rockContext ).GetOrCreateByBlockId( block.Id );

        content.Source = source;
        content.CompiledContent = compileResult.CompiledContent;
        content.CompiledVueVersion = compileResult.VueVersion;
        content.CompiledDateTime = RockDateTime.Now;

        rockContext.SaveChanges();

/*
            8/28/2026 - CLAUDE

            The verify instruction exists because "compiled" was being reported
            to users as "done", and the user then spent five or six prompts
            acting as the test harness for endpoint mismatches the agent could
            have caught itself. A compile proves syntax; it proves nothing
            about the contract between the component's invoke payloads and the
            endpoints' templates. Re-saving each endpoint with testParameters
            shaped like the component's payload is the only self-serve way to
            exercise that contract today, so the instruction names it.

            Reason: Push the agent to verify the component-endpoint contract
            before declaring the build done.
        */
        return Success( new ForgeContentSaveResult
        {
            BlockIdKey = block.IdKey,
            CompiledVueVersion = compileResult.VueVersion
        } )
            .WithInstructions( "The component compiled and saved. Compiling proves syntax only; it does not prove the component works. Before telling the user it is done, verify the data contract yourself: for each endpoint this component invokes, call AddOrUpdateLavaEndpoint again with the same template plus testParameters shaped exactly like the payload the component sends, and confirm the test output is valid JSON whose property names match what the component reads (including casing). Fix mismatches now rather than letting the user discover them." )
            .WithInstructions( "Then remind the user to view the page as a normal member, not as an administrator, before trusting it: the component runs as whoever views the page." );
    }

    #endregion
}
