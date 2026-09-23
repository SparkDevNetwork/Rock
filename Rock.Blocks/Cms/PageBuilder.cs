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
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Cms.PageBuilder;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Composes a page by dragging modules into its builder-enabled zones.
    /// </summary>

    [DisplayName( "Page Builder" )]
    [Category( "CMS" )]
    [Description( "Composes a page by dragging modules into its builder-enabled zones." )]
    [IconCssClass( "ti ti-layout" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Target Page",
        Description = "The page to compose. Only zones that opt in to the Page Builder accept modules.",
        IsRequired = true,
        DefaultValue = "83FC4394-ED09-461D-B865-68C1F5AFB34E", // Page Builder Sample
        Order = 0,
        Key = AttributeKey.TargetPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "5968C4FC-2508-4457-B1D6-E3AC6711C9F9" )]
    [Rock.SystemGuid.BlockTypeGuid( "89B56E5A-5A7D-44E6-85A0-49F101F814BE" )]
    public class PageBuilder : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string TargetPage = "TargetPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var targetPageUrl = this.GetLinkedPageUrl( AttributeKey.TargetPage );

            if ( targetPageUrl.IsNullOrWhiteSpace() )
            {
                return new PageBuilderInitializationBox
                {
                    ErrorMessage = "No target page is configured."
                };
            }

            return new PageBuilderInitializationBox
            {
                TargetPageUrl = targetPageUrl,
                ModuleTypes = PageBuilderModuleTypes.All
                    .Select( moduleType => new PageBuilderModuleTypeBag
                    {
                        Key = moduleType.Key,
                        Name = moduleType.Name,
                        IconCssClass = moduleType.IconCssClass
                    } )
                    .ToList()
            };
        }

        /// <summary>
        /// Gets the page being composed.
        /// </summary>
        /// <returns>The target page, or <c>null</c> if none is configured.</returns>
        private PageCache GetTargetPage()
        {
            var pageReference = new PageReference( GetAttributeValue( AttributeKey.TargetPage ) );

            return pageReference.PageId > 0 ? PageCache.Get( pageReference.PageId ) : null;
        }

        /// <summary>
        /// Gets the Canvas block type from its class, so its identifier is declared in only one place.
        /// </summary>
        /// <returns>The Canvas block type, or <c>null</c> if it has not been registered.</returns>
        private static BlockTypeCache GetCanvasBlockType()
        {
            var blockTypeGuid = typeof( Canvas ).GetCustomAttribute<Rock.SystemGuid.BlockTypeGuidAttribute>()?.Guid;

            return blockTypeGuid.HasValue ? BlockTypeCache.Get( blockTypeGuid.Value ) : null;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Adds a dropped module to the target page in a new Canvas block, at the position it was dropped.
        /// </summary>
        /// <param name="bag">Where the module was dropped and which module type it is.</param>
        /// <returns>The identifier of the new Canvas block, or an error.</returns>
        [BlockAction]
        public BlockActionResult AddModule( PageBuilderAddModuleBag bag )
        {
            if ( bag == null || bag.ZoneName.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "No drop location was provided." );
            }

            var targetPage = GetTargetPage();

            if ( targetPage == null )
            {
                return ActionNotFound( "The target page could not be found." );
            }

            if ( !targetPage.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionUnauthorized( "You are not authorized to add blocks to this page." );
            }

            var moduleType = PageBuilderModuleTypes.Get( bag.ModuleTypeKey );

            if ( moduleType == null )
            {
                return ActionBadRequest( "The module type could not be found." );
            }

            var canvasBlockType = GetCanvasBlockType();

            if ( canvasBlockType == null )
            {
                return ActionBadRequest( "The Canvas block type has not been registered." );
            }

            var blockService = new BlockService( RockContext );
            var block = new Block
            {
                PageId = targetPage.Id,
                Zone = bag.ZoneName,
                BlockTypeId = canvasBlockType.Id,
                Name = moduleType.Name
            };

            blockService.Add( block );
            block.Order = blockService.GetMaxOrder( block );

            RockContext.SaveChanges();

            // New blocks inherit the page's authorization rules.
            Authorization.CopyAuthorization( targetPage, block, RockContext );

            /*
                09/23/26 - JMH

                Order only positions a block among the page blocks in its zone, because Rock renders a
                zone's site and layout blocks ahead of its page blocks regardless of Order. The builder
                only offers drop positions below those, so the block it was dropped in front of is always
                a page block in this zone and the reorder can stay within them.

                Reason: A drop position maps directly onto Order among the zone's page blocks.
            */
            if ( bag.BeforeBlockId.HasValue )
            {
                var zoneBlocks = blockService.GetByPageAndZone( targetPage.Id, bag.ZoneName ).ToList();

                if ( zoneBlocks.ReorderEntity( block.Id.ToString(), bag.BeforeBlockId.Value.ToString() ) )
                {
                    RockContext.SaveChanges();
                }
            }

            block.LoadAttributes( RockContext );
            block.SetAttributeValue( Canvas.AttributeKey.ModuleType, moduleType.Key );
            block.SetAttributeValue( Canvas.AttributeKey.ModuleSettings, moduleType.GetDefaultSettings().ToJson() );
            block.SaveAttributeValues( RockContext );

            // Saving a new page block does not refresh its page's cached block list.
            PageCache.Remove( targetPage.Id );

            return ActionOk( block.Id );
        }

        #endregion Block Actions
    }
}
