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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Adds a block to a page, layout, or site, or updates an existing block. Returns the block's IdKey, which is the block id the Forge Content Builder skill's AddOrUpdateForgeContent tool needs." )]
    [AgentToolPreamble( "Saving the block." )]
    [AgentUsage( "When adding, resolve the block type with ListBlockTypes and provide exactly one of pageIdKey, layoutIdKey, or siteIdKey. For AI-authored content use the 'Forge Content' block type. Returns the block IdKey to pass as blockId to the Forge Content Builder skill's AddOrUpdateForgeContent tool." )]
    [AgentUsage( "Blocks placed on a layout or site render on every page that uses the layout or site; place on a page unless the user asks otherwise." )]
    [AgentToolGuid( "05C9C108-4516-46B7-85FB-5C8FE6212CCF" )]
    public AgentToolResult AddOrUpdateBlock(
        [Description( "Required when editing an existing block. Do not provide when adding a new block." )]
        string blockIdKey = null,

        [Description( "Where the new block lives: Page, Layout, or Site. Optional; inferred from whichever of pageIdKey, layoutIdKey, or siteIdKey is provided." )]
        BlockLocation? blockLocation = null,

        [Description( "The IdKey or guid of the page to add the block to." )]
        string pageIdKey = null,

        [Description( "The IdKey or guid of the layout to add the block to. The block renders on every page using the layout." )]
        string layoutIdKey = null,

        [Description( "The IdKey or guid of the site to add the block to. The block renders on every page of the site." )]
        string siteIdKey = null,

        [Description( "The IdKey or guid of the block type to add. Required when adding; use ListBlockTypes to find it. For AI-authored content use the 'Forge Content' block type." )]
        string blockTypeIdKey = null,

        [Description( "An administrative name for the block. Defaults to the block type name when adding." )]
        SetOrClear<string> name = null,

        [Description( "The zone to place the block in. Defaults to 'Main' when adding." )]
        SetOrClear<string> zone = null,

        [Description( "The block settings to set, as attribute key and value pairs." )]
        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var isAdd = blockIdKey.IsNullOrWhiteSpace();
        Model.Block block = null;
        BlockTypeCache blockTypeCache = null;
        IEntity placementEntity = null;

        if ( name?.ClearValue == true )
        {
            helper.AddError( "The name of a block cannot be cleared." );
        }

        if ( zone?.ClearValue == true )
        {
            helper.AddError( "The zone of a block cannot be cleared." );
        }

        if ( isAdd )
        {
            if ( blockTypeIdKey.IsNullOrWhiteSpace() )
            {
                helper.AddError( $"A {nameof( blockTypeIdKey )} is required when adding a block. Use {nameof( ListBlockTypes )} to find it, or ask the user which block to add." );
            }
            else
            {
                var blockType = helper.GetRequiredEntity<Model.BlockType>( blockTypeIdKey, checkSecurity: false );

                blockTypeCache = blockType != null ? BlockTypeCache.Get( blockType.Id, rockContext ) : null;
            }

            // Exactly one placement target decides where the block lives.
            var placementKeys = new[] { pageIdKey, layoutIdKey, siteIdKey }
                .Count( k => k.IsNotNullOrWhiteSpace() );

            if ( placementKeys != 1 )
            {
                helper.AddError( $"Provide exactly one of {nameof( pageIdKey )}, {nameof( layoutIdKey )}, or {nameof( siteIdKey )} to say where the new block lives." );
            }
            else if ( blockLocation.HasValue )
            {
                // The location is optional declared intent; when provided it
                // must agree with the placement key actually passed.
                var impliedLocation = pageIdKey.IsNotNullOrWhiteSpace()
                    ? BlockLocation.Page
                    : layoutIdKey.IsNotNullOrWhiteSpace() ? BlockLocation.Layout : BlockLocation.Site;

                if ( blockLocation.Value != impliedLocation )
                {
                    helper.AddError( $"The {nameof( blockLocation )} '{blockLocation.Value}' does not match the placement key that was provided. Omit {nameof( blockLocation )} or pass the matching key." );
                }
            }
        }
        else
        {
            block = helper.GetRequiredEntity<Model.Block>( blockIdKey, checkSecurity: false );

            if ( pageIdKey.IsNotNullOrWhiteSpace() || layoutIdKey.IsNotNullOrWhiteSpace() || siteIdKey.IsNotNullOrWhiteSpace() || blockLocation.HasValue )
            {
                helper.AddError( $"An existing block cannot be moved, do not provide a {nameof( pageIdKey )}, {nameof( layoutIdKey )}, {nameof( siteIdKey )}, or {nameof( blockLocation )} when editing." );
            }

            if ( blockTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                helper.AddError( $"The block type of an existing block cannot be changed, do not provide a {nameof( blockTypeIdKey )} when editing." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( isAdd )
        {
            // Authorization is checked through the cache so inherited page and
            // site security participates.
            ISecured placementCache;

            if ( pageIdKey.IsNotNullOrWhiteSpace() )
            {
                var page = helper.GetRequiredEntity<Model.Page>( pageIdKey, checkSecurity: false );

                placementEntity = page;
                placementCache = page != null ? PageCache.Get( page.Id, rockContext ) : null;
            }
            else if ( layoutIdKey.IsNotNullOrWhiteSpace() )
            {
                var layout = helper.GetRequiredEntity<Model.Layout>( layoutIdKey, checkSecurity: false );

                placementEntity = layout;
                placementCache = layout != null ? LayoutCache.Get( layout.Id, rockContext ) : null;
            }
            else
            {
                var site = helper.GetRequiredEntity<Model.Site>( siteIdKey, checkSecurity: false );

                placementEntity = site;
                placementCache = site != null ? SiteCache.Get( site.Id, rockContext ) : null;
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( placementCache == null || !placementCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
            {
                helper.AddError( "You are not authorized to add blocks there." );

                return helper.ErrorResult;
            }

            var blockService = new BlockService( rockContext );

            block = new Model.Block
            {
                PageId = placementEntity is Model.Page ? placementEntity.Id : ( int? ) null,
                LayoutId = placementEntity is Model.Layout ? placementEntity.Id : ( int? ) null,
                SiteId = placementEntity is Model.Site ? placementEntity.Id : ( int? ) null,
                BlockTypeId = blockTypeCache.Id,
                Zone = zone?.Value.IsNotNullOrWhiteSpace() == true ? zone.Value : "Main",
                Name = name?.Value.IsNotNullOrWhiteSpace() == true ? name.Value : blockTypeCache.Name
            };

            blockService.Add( block );

            // Place the new block at the end of its zone.
            block.Order = blockService.GetMaxOrder( block );
        }
        else
        {
            var blockCache = BlockCache.Get( block.Id, rockContext );

            if ( blockCache == null || !blockCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
            {
                helper.AddError( "You are not authorized to edit that block." );

                return helper.ErrorResult;
            }

            blockTypeCache = BlockTypeCache.Get( block.BlockTypeId, rockContext );

            helper.UpdateProperty( block, b => b.Name, name );
            helper.UpdateProperty( block, b => b.Zone, zone );
        }

        helper.SetAttributeValues( block, attributeValues );

        if ( !block.IsValid )
        {
            helper.AddError( block.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? "The block could not be saved." );
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( isAdd )
        {
            // A new block inherits the authorization of what it was placed on.
            Authorization.CopyAuthorization( ( ISecured ) placementEntity, block, rockContext );
        }

        // Flush the pages the block renders on so it appears.
        if ( block.PageId.HasValue )
        {
            PageCache.FlushPage( block.PageId.Value );
        }
        else if ( block.LayoutId.HasValue )
        {
            PageCache.FlushPagesForLayout( block.LayoutId.Value );
        }
        else if ( block.SiteId.HasValue )
        {
            PageCache.FlushPagesForSite( block.SiteId.Value );
        }

        var pageCache = block.PageId.HasValue
            ? PageCache.Get( block.PageId.Value, rockContext )
            : null;

        return Success( new BlockResult
        {
            Id = block.Id,
            Guid = block.Guid,
            Name = block.Name,
            Zone = block.Zone,
            Order = block.Order,
            Location = block.PageId.HasValue
                ? "Page"
                : block.LayoutId.HasValue ? "Layout" : "Site",
            BlockType = blockTypeCache != null
                ? new BlockTypeResult
                {
                    Id = blockTypeCache.Id,
                    Guid = blockTypeCache.Guid,
                    Name = blockTypeCache.Name,
                    Category = blockTypeCache.Category
                }
                : null,
            PageUrl = pageCache != null ? GetPageUrl( pageCache ) : null
        } )
            .WithHistoryContent( new KeyNameResult
            {
                Id = block.Id,
                Name = block.Name
            } )
            .WithInstructions( $"The block has been {( isAdd ? "created" : "updated" )}." );
    }

    #endregion
}
