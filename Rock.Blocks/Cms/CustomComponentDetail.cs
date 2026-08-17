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

using Rock.Attribute;
using Rock.Blocks;
using Rock.Cms;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Cms.CustomComponentDetail;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// A single block, dropped on any page, that renders an authored custom
    /// component in view mode and, for an authorized administrator, edits it
    /// in place.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <remarks>
    /// The block owns a single <see cref="Rock.Model.CustomComponent"/> record,
    /// resolved by the block's own <see cref="Rock.Blocks.RockBlockType.BlockId"/>
    /// (the <c>HtmlContent</c> ownership pattern). View mode is served only the
    /// precompiled output. Compilation happens exclusively on the server through
    /// <see cref="CustomComponentCompiler"/>; the browser never loads a compiler.
    /// </remarks>

    [DisplayName( "Custom Component" )]
    [Category( "CMS" )]
    [Description( "Renders an authored custom component, compiled on the server, and lets an administrator edit it in place." )]
    [IconCssClass( "ti ti-code" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "8C7E29E5-E2C5-4331-B7F7-06EF894E7316" )]
    [Rock.SystemGuid.BlockTypeGuid( "D4A5F720-493C-4DE8-B4B6-D6667D7ED2A2" )]
    public class CustomComponentDetail : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomComponentDetailInitializationBox();

            using ( var rockContext = new RockContext() )
            {
                var content = new CustomComponentService( rockContext ).GetByBlockId( BlockId );
                var isEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

                box.IsEditable = isEditable;

                // Every viewer receives the precompiled output for rendering.
                box.CompiledContent = content?.CompiledContent;

                // The clean source is only sent to authors so they can re-edit it.
                // A plain visitor never receives the source.
                if ( isEditable )
                {
                    box.Source = content?.Source;
                }
            }

            return box;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Compiles and upserts the authored component for this block placement.
        /// The source is compiled on the server; a failed compile stores nothing
        /// and returns the compiler's own error text.
        /// </summary>
        /// <param name="bag">The authored source.</param>
        /// <returns>The compiled output on success; a failure result carrying the compile error otherwise.</returns>
        [BlockAction]
        public BlockActionResult SaveContent( SaveCustomComponentRequestBag bag )
        {
            // Authored code runs in the visitor's browser as the visitor and can
            // call anything the visitor can, so editing is gated to administrators
            // (EDIT authorization).
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to edit this component." );
            }

            if ( bag == null || bag.Source.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "No source was provided." );
            }

            /*
                8/17/2026 - CLAUDE

                The server is the only compile path, shared with the agent skill's
                AddOrUpdateCustomComponent tool. Compiled output is never accepted from the
                client, so the stored source is always exactly what the stored
                module was compiled from.

                Reason: One compile path keeps stored source and output in lockstep.
            */
            var compileResult = new CustomComponentCompiler().CompileSource( bag.Source );

            if ( compileResult.IsBrowserMissing )
            {
                return ActionBadRequest( "The compile engine is still being provisioned on this server. Try again in a few minutes." );
            }

            if ( compileResult.IsBundleMissing )
            {
                return ActionBadRequest( "The compiler bundle is not deployed on this server, so the component cannot be compiled." );
            }

            if ( !compileResult.IsSuccess )
            {
                return ActionBadRequest( string.Join( "\n", compileResult.Errors ) );
            }

            using ( var rockContext = new RockContext() )
            {
                var service = new CustomComponentService( rockContext );
                var content = service.GetOrCreateByBlockId( BlockId );

                content.Source = bag.Source;
                content.CompiledContent = compileResult.CompiledContent;
                content.CompiledVueVersion = compileResult.VueVersion;
                content.CompiledDateTime = RockDateTime.Now;

                rockContext.SaveChanges();
            }

            return ActionOk( new SaveCustomComponentResponseBag
            {
                CompiledContent = compileResult.CompiledContent
            } );
        }

        #endregion Block Actions
    }
}
