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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Cms.ObsidianContentDetail;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// A single block, dropped on any page, that renders author-defined Obsidian
    /// UI in view mode and, for an authorized administrator, edits and compiles
    /// it in place.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <remarks>
    /// The block owns a single <see cref="Rock.Model.ObsidianContent"/> record,
    /// resolved by the block's own <see cref="Rock.Blocks.RockBlockType.BlockId"/>
    /// (the <c>HtmlContent</c> pattern). View mode is served only the precompiled
    /// output. The compiler (and its eval requirement) lives entirely in the
    /// administrator's edit path, in the browser.
    /// </remarks>

    [DisplayName( "Obsidian Content Detail" )]
    [Category( "CMS" )]
    [Description( "Renders author-defined Obsidian UI in place, and lets an administrator author and compile it in the browser." )]
    [IconCssClass( "ti ti-code" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "8C7E29E5-E2C5-4331-B7F7-06EF894E7316" )]
    [Rock.SystemGuid.BlockTypeGuid( "D4A5F720-493C-4DE8-B4B6-D6667D7ED2A2" )]
    public class ObsidianContentDetail : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ObsidianContentDetailInitializationBox();

            using ( var rockContext = new RockContext() )
            {
                var content = new ObsidianContentService( rockContext ).GetByBlockId( BlockId );
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
        /// Upserts the authored content for this block placement. The source is
        /// compiled in the author's browser; this action stores both the source
        /// and the compiled output.
        /// </summary>
        /// <param name="bag">The source, compiled output, and targeted Vue version.</param>
        /// <returns>An empty ok result on success; a failure result otherwise.</returns>
        [BlockAction]
        public BlockActionResult SaveContent( SaveObsidianContentRequestBag bag )
        {
            // Authoring runs as the visitor in the browser and can call any API the
            // visitor can, so editing is gated to administrators (EDIT authorization).
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to edit this content." );
            }

            if ( bag == null )
            {
                return ActionBadRequest( "No content was provided." );
            }

            using ( var rockContext = new RockContext() )
            {
                var service = new ObsidianContentService( rockContext );
                var content = service.GetOrCreateByBlockId( BlockId );

                content.Source = bag.Source;
                content.CompiledContent = bag.CompiledContent;
                content.CompiledVueVersion = bag.CompiledVueVersion;
                content.CompiledDateTime = RockDateTime.Now;

                rockContext.SaveChanges();
            }

            return ActionOk();
        }

        #endregion Block Actions
    }
}
