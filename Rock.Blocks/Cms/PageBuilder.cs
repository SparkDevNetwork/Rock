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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Cms.PageBuilder;
using Rock.ViewModels.Utility;
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
        Description = "The page the builder opens to. Only zones that opt in to the Page Builder accept modules.",
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

        private static class PageParameterKey
        {
            public const string Page = "Page";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var targetPage = GetTargetPage();

            if ( targetPage == null )
            {
                return new PageBuilderInitializationBox
                {
                    ErrorMessage = PageParameter( PageParameterKey.Page ).IsNotNullOrWhiteSpace()
                        ? "The page could not be found."
                        : "No target page is configured."
                };
            }

            // The target page is still sent with an error, so the top bar can switch to another page.
            var box = new PageBuilderInitializationBox
            {
                TargetPageUrl = new PageReference( targetPage.Id ).BuildUrl(),
                TargetPageGuid = targetPage.Guid,
                TargetPageName = targetPage.InternalName,
                TargetPageIntents = GetPageIntents( targetPage.Id )
            };

            if ( targetPage.Id == PageCache.Id )
            {
                box.ErrorMessage = "The Page Builder cannot build its own page.";
                return box;
            }

            if ( !targetPage.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                box.ErrorMessage = "You are not authorized to edit the modules on this page.";
                return box;
            }

            box.ModuleTypes = new ModuleTypeService( RockContext )
                .Queryable()
                .AsNoTracking()
                .OrderBy( moduleType => moduleType.Name )
                .ToList()
                .Select( moduleType => new PageBuilderModuleTypeBag
                {
                    Key = moduleType.IdKey,
                    Name = moduleType.Name,
                    IconCssClass = moduleType.IconCssClass
                } )
                .ToList();

            return box;
        }

        /// <summary>
        /// Gets the page being composed, which is the page chosen in the builder or else the Target Page setting.
        /// </summary>
        /// <returns>The target page, or <c>null</c> if the chosen page does not exist or none is configured.</returns>
        private PageCache GetTargetPage()
        {
            // Obsidian sends the page's parameters with every block action, so actions compose the same page the builder shows.
            var pageKey = PageParameter( PageParameterKey.Page );

            if ( pageKey.IsNotNullOrWhiteSpace() )
            {
                return PageCache.Get( pageKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            var pageReference = new PageReference( GetAttributeValue( AttributeKey.TargetPage ) );

            return pageReference.PageId > 0 ? PageCache.Get( pageReference.PageId ) : null;
        }

        /// <summary>
        /// Gets the interaction intents a page is tagged with.
        /// </summary>
        /// <param name="pageId">The identifier of the page.</param>
        /// <returns>The page's intents as Interaction Intent defined values.</returns>
        private List<ListItemBag> GetPageIntents( int pageId )
        {
            return new EntityIntentService( RockContext )
                .GetIntentValueIds<Page>( pageId )
                .Select( intentValueId => DefinedValueCache.Get( intentValueId )?.ToListItemBag() )
                .Where( intent => intent != null )
                .ToList();
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

        /// <summary>
        /// Gets a Canvas block on the target page, provided the current person may administrate that page.
        /// </summary>
        /// <param name="blockId">The identifier of the Canvas block.</param>
        /// <param name="blockService">The service that loads the block, so the caller can go on to change it.</param>
        /// <param name="block">The Canvas block, or <c>null</c> when it cannot be changed from here.</param>
        /// <param name="error">The result for the block action to return when the block cannot be changed from here.</param>
        /// <returns><c>true</c> if the block was found and may be changed; otherwise <c>false</c>.</returns>
        private bool TryGetCanvasBlock( int blockId, BlockService blockService, out Block block, out BlockActionResult error )
        {
            block = null;
            error = null;

            var targetPage = GetTargetPage();

            if ( targetPage == null )
            {
                error = ActionNotFound( "The target page could not be found." );
                return false;
            }

            if ( !targetPage.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                error = ActionUnauthorized( "You are not authorized to edit the modules on this page." );
                return false;
            }

            var canvasBlockType = GetCanvasBlockType();
            var candidateBlock = blockService.Get( blockId );

            // Only a Canvas block on the page being composed can be changed from here.
            if ( candidateBlock == null || candidateBlock.PageId != targetPage.Id || canvasBlockType == null || candidateBlock.BlockTypeId != canvasBlockType.Id )
            {
                error = ActionNotFound( "The module could not be found." );
                return false;
            }

            block = candidateBlock;

            return true;
        }

        /// <summary>
        /// Gets the module instance a Canvas block displays.
        /// </summary>
        /// <param name="block">The Canvas block.</param>
        /// <returns>The module instance, or <c>null</c> if the block does not reference one that exists.</returns>
        private ModuleInstance GetModuleInstance( Block block )
        {
            block.LoadAttributes( RockContext );

            var moduleInstanceGuid = block.GetAttributeValue( Canvas.AttributeKey.ModuleInstance ).AsGuidOrNull();

            return moduleInstanceGuid.HasValue ? new ModuleInstanceService( RockContext ).Get( moduleInstanceGuid.Value ) : null;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Adds a new module of the dropped type to the target page in a new Canvas block, at the position it was dropped.
        /// </summary>
        /// <param name="bag">Where the module was dropped and which module type it is.</param>
        /// <returns>The identifiers of the new Canvas block, or an error.</returns>
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

            return ActionOk( new PageBuilderAddModuleResponseBag
            {
                BlockId = block.Id,
                BlockGuid = block.Guid
            } );
        }

        /// <summary>
        /// Gets the settings of the module a Canvas block displays, for editing.
        /// </summary>
        /// <param name="blockId">The identifier of the Canvas block.</param>
        /// <returns>The module's settings and their values, or an error.</returns>
        [BlockAction]
        public BlockActionResult GetModuleSettings( int blockId )
        {
            if ( !TryGetCanvasBlock( blockId, new BlockService( RockContext ), out var block, out var error ) )
            {
                return error;
            }

            var moduleInstance = GetModuleInstance( block );

            if ( moduleInstance == null )
            {
                return ActionNotFound( "This Canvas has no module." );
            }

            moduleInstance.LoadAttributes( RockContext );

            return ActionOk( new PageBuilderModuleSettingsBag
            {
                BlockId = block.Id,
                BlockGuid = block.Guid,
                Attributes = moduleInstance.GetPublicAttributesForEdit( RequestContext.CurrentPerson ),
                AttributeValues = moduleInstance.GetPublicAttributeValuesForEdit( RequestContext.CurrentPerson )
            } );
        }

        /// <summary>
        /// Saves new values for the settings of the module a Canvas block displays.
        /// </summary>
        /// <param name="bag">The Canvas block and the new values of its module's settings.</param>
        /// <returns>An empty successful result, or an error.</returns>
        [BlockAction]
        public BlockActionResult SaveModuleSettings( PageBuilderModuleSettingsBag bag )
        {
            if ( bag?.AttributeValues == null )
            {
                return ActionBadRequest( "No module settings were provided." );
            }

            if ( !TryGetCanvasBlock( bag.BlockId, new BlockService( RockContext ), out var block, out var error ) )
            {
                return error;
            }

            var moduleInstance = GetModuleInstance( block );

            if ( moduleInstance == null )
            {
                return ActionNotFound( "This Canvas has no module." );
            }

            moduleInstance.LoadAttributes( RockContext );
            moduleInstance.SetPublicAttributeValues( bag.AttributeValues, RequestContext.CurrentPerson );
            moduleInstance.SaveAttributeValues( RockContext );

            return ActionOk();
        }

        /// <summary>
        /// Moves a Canvas block to where its handle was dropped on the target page, which can be in another builder-enabled zone.
        /// </summary>
        /// <param name="bag">The Canvas block and where it was dropped.</param>
        /// <returns>An empty successful result, or an error.</returns>
        [BlockAction]
        public BlockActionResult MoveModule( PageBuilderMoveModuleBag bag )
        {
            if ( bag == null || bag.ZoneName.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "No drop location was provided." );
            }

            var blockService = new BlockService( RockContext );

            if ( !TryGetCanvasBlock( bag.BlockId, blockService, out var block, out var error ) )
            {
                return error;
            }

            var pageId = block.PageId.Value;

            RockContext.WrapTransaction( () =>
            {
                // A block moving to another zone is saved there first, so it is among the blocks the reorder renumbers.
                if ( block.Zone != bag.ZoneName )
                {
                    block.Zone = bag.ZoneName;
                    RockContext.SaveChanges();
                }

                // As when adding a module, the builder only offers positions among the zone's page blocks.
                var zoneBlocks = blockService.GetByPageAndZone( pageId, bag.ZoneName ).ToList();

                if ( zoneBlocks.ReorderEntity( block.Id.ToString(), bag.BeforeBlockId?.ToString() ) )
                {
                    RockContext.SaveChanges();
                }
            } );

            // Moving a page block does not refresh its page's cached block list.
            PageCache.Remove( pageId );

            return ActionOk();
        }

        /// <summary>
        /// Deletes a Canvas block from the target page, along with its module instance unless that instance is shareable or displayed elsewhere.
        /// </summary>
        /// <param name="blockId">The identifier of the Canvas block to delete.</param>
        /// <returns>An empty successful result, or an error.</returns>
        [BlockAction]
        public BlockActionResult DeleteModule( int blockId )
        {
            var blockService = new BlockService( RockContext );

            if ( !TryGetCanvasBlock( blockId, blockService, out var block, out var error ) )
            {
                return error;
            }

            var pageId = block.PageId.Value;
            var moduleInstance = GetModuleInstance( block );

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
                    new ModuleInstanceService( RockContext ).Delete( moduleInstance );
                }

                RockContext.SaveChanges();
            } );

            PageCache.Remove( pageId );

            return ActionOk();
        }

        /// <summary>
        /// Replaces the interaction intents the target page is tagged with.
        /// </summary>
        /// <param name="intentValueGuids">The unique identifiers of the Interaction Intent defined values to tag the page with.</param>
        /// <returns>The page's intents once saved, or an error.</returns>
        [BlockAction]
        public BlockActionResult SavePageIntents( List<Guid> intentValueGuids )
        {
            var targetPage = GetTargetPage();

            if ( targetPage == null )
            {
                return ActionNotFound( "The target page could not be found." );
            }

            if ( !targetPage.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionUnauthorized( "You are not authorized to edit this page." );
            }

            var intentDefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.INTERACTION_INTENT.AsGuid() );
            var intentValueIds = ( intentValueGuids ?? new List<Guid>() )
                .Select( intentValueGuid => DefinedValueCache.Get( intentValueGuid ) )
                .Where( intentValue => intentValue != null && intentValue.DefinedTypeId == intentDefinedTypeId )
                .Select( intentValue => intentValue.Id )
                .ToList();

            new EntityIntentService( RockContext ).SetIntents<Page>( targetPage.Id, intentValueIds );
            RockContext.SaveChanges();

            // A cached page reads its intents only once, and they are written to the interactions of its visits.
            PageCache.Remove( targetPage.Id );

            return ActionOk( GetPageIntents( targetPage.Id ) );
        }

        #endregion Block Actions
    }
}
