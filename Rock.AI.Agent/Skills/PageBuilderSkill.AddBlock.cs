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

using System;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.PageBuilderSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PageBuilderSkill
{
    #region Tool(s)

    [Description( "Adds a block to a page in the specified zone. Returns the new block's IdKey, which is the block id the CustomComponent skill's SetComponentSource tool needs." )]
    [AgentToolPreamble( "Adding the block to the page." )]
    [AgentUsage( "blockType is the block type name or guid; ask the user which block if not specified. For vibe-coded content use the 'Custom Component' block type. Returns the block IdKey to pass as blockId to the CustomComponent skill's SetComponentSource tool." )]
    [AgentToolGuid( "05C9C108-4516-46B7-85FB-5C8FE6212CCF" )]
    public AgentToolResult AddBlock(
        [Description( "The IdKey or guid of the page to add the block to." )]
        string page,

        [Description( "The block type name or guid to add. For vibe-coded content use 'Custom Component'." )]
        string blockType,

        [Description( "The zone to place the block in. Defaults to 'Main'." )]
        string zone = null,

        [Description( "An optional name for the block. Defaults to the block type name." )]
        string name = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        if ( blockType.IsNullOrWhiteSpace() )
        {
            helper.AddError( "A block type is required. Ask the user which block to add." );
        }

        var targetPage = helper.GetRequiredEntity<Model.Page>( page, checkSecurity: false );

        BlockTypeCache blockTypeCache = null;

        if ( blockType.IsNotNullOrWhiteSpace() )
        {
            // Resolve the block type by guid first, then by name.
            blockTypeCache = BlockTypeCache.Get( blockType.AsGuid(), rockContext )
                ?? BlockTypeCache.All( rockContext ).FirstOrDefault( bt => bt.Name.Equals( blockType, StringComparison.OrdinalIgnoreCase ) );

            if ( blockTypeCache == null )
            {
                /*
                    8/17/2026 - CLAUDE

                    The name match is exact, so a near miss ("Component" for
                    "Custom Component") used to fail with nothing to go on, and
                    the agent's only recovery was guessing again. Suggesting the
                    closest names turns the retry into a selection.

                    Reason: An exact-match failure gave the agent no path to the right name.
                */
                var suggestions = BlockTypeCache.All( rockContext )
                    .Where( bt => bt.Name != null && bt.Name.IndexOf( blockType, StringComparison.OrdinalIgnoreCase ) >= 0 )
                    .OrderBy( bt => bt.Name.Length )
                    .Take( 5 )
                    .Select( bt => bt.Name )
                    .ToList();

                helper.AddError( suggestions.Any()
                    ? $"No block type is named exactly '{blockType}'. Close matches: {string.Join( ", ", suggestions )}. Call again with one of these exact names."
                    : $"No block type matched '{blockType}'." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Authorization is checked through the cache so inherited page and
        // site security participates.
        var pageCache = PageCache.Get( targetPage.Id, rockContext );

        if ( pageCache == null || !pageCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
        {
            helper.AddError( "You are not authorized to add blocks to that page." );

            return helper.ErrorResult;
        }

        var blockService = new BlockService( rockContext );

        var block = new Model.Block
        {
            PageId = targetPage.Id,
            Zone = zone.IsNullOrWhiteSpace() ? "Main" : zone,
            BlockTypeId = blockTypeCache.Id,
            Name = name.IsNullOrWhiteSpace() ? blockTypeCache.Name : name
        };

        blockService.Add( block );

        // Place the new block at the end of its zone.
        block.Order = blockService.GetMaxOrder( block );

        rockContext.SaveChanges();

        // A new block inherits the page's authorization rules.
        Authorization.CopyAuthorization( targetPage, block, rockContext );

        // Flush the page so the new block renders.
        PageCache.Remove( targetPage.Id );

        return Success( new AddBlockResult
        {
            Id = block.Id,
            Guid = block.Guid,
            Zone = block.Zone,
            PageUrl = $"/page/{targetPage.Id}"
        } );
    }

    #endregion
}
