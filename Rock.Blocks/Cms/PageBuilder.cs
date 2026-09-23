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
using System.Data.Entity;
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

            var moduleTypes = new ModuleTypeService( RockContext )
                .Queryable()
                .AsNoTracking()
                .OrderBy( moduleType => moduleType.Name )
                .ToList();

            return new PageBuilderInitializationBox
            {
                TargetPageUrl = targetPageUrl,
                ModuleTypes = moduleTypes
                    .Select( moduleType => new PageBuilderModuleTypeBag
                    {
                        Key = moduleType.IdKey,
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
        /// Adds a new module of the dropped type to the target page in a new Canvas block, at the position it was dropped.
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

            var moduleType = new ModuleTypeService( RockContext ).Get( bag.ModuleTypeKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( moduleType == null )
            {
                return ActionBadRequest( "The module type could not be found." );
            }

            var canvasBlockType = GetCanvasBlockType();

            if ( canvasBlockType == null )
            {
                return ActionBadRequest( "The Canvas block type has not been registered." );
            }

            // Block type attributes are otherwise created when a page first renders a block of that type, and the new block needs its Module Instance attribute now.
            BlockTypeService.VerifyBlockTypeInstanceProperties( new[] { canvasBlockType.Id } );

            var blockService = new BlockService( RockContext );
            var block = new Block
            {
                PageId = targetPage.Id,
                Zone = bag.ZoneName,
                BlockTypeId = canvasBlockType.Id,
                Name = moduleType.Name
            };

            // A new module starts with its type's default settings, since its attribute values fall back to their defaults.
            var moduleInstance = new ModuleInstance
            {
                Name = moduleType.Name,
                ModuleTypeId = moduleType.Id
            };

            RockContext.WrapTransaction( () =>
            {
                new ModuleInstanceService( RockContext ).Add( moduleInstance );
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
                block.SetAttributeValue( Canvas.AttributeKey.ModuleInstance, moduleInstance.Guid.ToString() );
                block.SaveAttributeValues( RockContext );
            } );

            // Saving a new page block does not refresh its page's cached block list.
            PageCache.Remove( targetPage.Id );

            return ActionOk( block.Id );
        }

        /// <summary>
        /// Deletes a Canvas block from the target page, along with its module instance unless that instance is shareable or displayed elsewhere.
        /// </summary>
        /// <param name="blockId">The identifier of the Canvas block to delete.</param>
        /// <returns>An empty successful result, or an error.</returns>
        [BlockAction]
        public BlockActionResult DeleteModule( int blockId )
        {
            var targetPage = GetTargetPage();

            if ( targetPage == null )
            {
                return ActionNotFound( "The target page could not be found." );
            }

            if ( !targetPage.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionUnauthorized( "You are not authorized to delete blocks from this page." );
            }

            var blockService = new BlockService( RockContext );
            var block = blockService.Get( blockId );
            var canvasBlockType = GetCanvasBlockType();

            // Only a Canvas block on the page being composed can be deleted from here.
            if ( block == null || block.PageId != targetPage.Id || canvasBlockType == null || block.BlockTypeId != canvasBlockType.Id )
            {
                return ActionNotFound( "The module could not be found." );
            }

            block.LoadAttributes( RockContext );

            var moduleInstanceGuid = block.GetAttributeValue( Canvas.AttributeKey.ModuleInstance ).AsGuidOrNull();
            var moduleInstanceService = new ModuleInstanceService( RockContext );
            var moduleInstance = moduleInstanceGuid.HasValue ? moduleInstanceService.Get( moduleInstanceGuid.Value ) : null;

            /*
                09/23/26 - JMH

                A Canvas references its module instance from a block attribute value rather than owning it,
                so the instance is only deleted when nothing else can be using it: it must not be shareable,
                and no other Canvas block may reference it. Deleting a block leaves its attribute values
                behind until the Rock Cleanup job removes them, so only values of blocks that still exist
                count as references.

                Reason: Deleting a Canvas must not remove a module instance another Canvas still displays.
            */
            var isModuleInstanceDeleted = moduleInstance != null && !moduleInstance.IsShareable;

            if ( isModuleInstanceDeleted )
            {
                var moduleInstanceAttributeId = block.Attributes[Canvas.AttributeKey.ModuleInstance].Id;
                var moduleInstanceValue = moduleInstance.Guid.ToString();
                var blockIdQuery = blockService.Queryable().Select( b => b.Id );

                isModuleInstanceDeleted = !new AttributeValueService( RockContext )
                    .Queryable()
                    .Any( av => av.AttributeId == moduleInstanceAttributeId
                        && av.EntityId != block.Id
                        && av.Value == moduleInstanceValue
                        && blockIdQuery.Contains( av.EntityId.Value ) );
            }

            RockContext.WrapTransaction( () =>
            {
                blockService.Delete( block );

                if ( isModuleInstanceDeleted )
                {
                    moduleInstanceService.Delete( moduleInstance );
                }

                RockContext.SaveChanges();
            } );

            PageCache.Remove( targetPage.Id );

            return ActionOk();
        }

        #endregion Block Actions
    }
}
