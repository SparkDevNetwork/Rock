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
using Rock.AI.Agent.Classes.Skills.CustomComponentSkill;
using Rock.Cms;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CustomComponentSkill
{
    #region Tool(s)

    [Description( "Compiles and saves the authored source of a Custom Component block placement. A failed compile stores nothing and returns the compiler's errors." )]
    [AgentToolPreamble( "Compiling and saving the component source." )]
    [AgentUsage( "blockId is the block placement to write; source is the authored Vue single-file-component. The server compiles the source and either stores the result or returns the compile errors for you to fix and retry. Nothing is stored when the compile fails." )]
    [AgentToolGuid( "26FFEE94-4868-4DEC-BE40-68FBE30DAEB8" )]
    public AgentToolResult AddOrUpdateCustomComponent(
        [Description( "The id of the Custom Component block placement to write." )]
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
        var compileResult = new CustomComponentCompiler().CompileSource( source );

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
            helper.AddError( "The server could not compile because its browser engine is still being provisioned. Nothing was saved. This is not a problem with your source. Tell the user the instance needs its PDF/browser engine installed, which happens automatically the first time a PDF is generated, and try AddOrUpdateCustomComponent again in a few minutes." );

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
            helper.AddError( "The source failed to compile. Fix the source and call AddOrUpdateCustomComponent again. Compiler errors:\n" + string.Join( "\n", compileResult.Errors ) );

            return helper.ErrorResult;
        }

        var content = new CustomComponentService( rockContext ).GetOrCreateByBlockId( block.Id );

        content.Source = source;
        content.CompiledContent = compileResult.CompiledContent;
        content.CompiledVueVersion = compileResult.VueVersion;
        content.CompiledDateTime = RockDateTime.Now;

        rockContext.SaveChanges();

        return Success( new CustomComponentSaveResult
        {
            BlockIdKey = block.IdKey,
            CompiledVueVersion = compileResult.VueVersion
        } )
            .WithInstructions( "The component compiled and saved. Remind the user to view the page as a normal member, not as an administrator, before trusting it: the component runs as whoever views the page." );
    }

    #endregion
}
