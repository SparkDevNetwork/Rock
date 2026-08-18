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
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Deletes a block from its page, layout, or site, along with any custom component content stored against it." )]
    [AgentToolPreamble( "Deleting the block." )]
    [AgentUsage( "Deleting a block is permanent and takes its settings and any authored custom component source with it. Confirm the exact block with the user before deleting, and never delete a block the user did not name explicitly." )]
    [AgentToolGuid( "B30F66EA-0D9E-4854-BB82-A96BE7719D00" )]
    public AgentToolResult DeleteBlock(
        [Description( "The IdKey or guid of the block to delete." )]
        string blockIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var block = helper.GetRequiredEntity<Model.Block>( blockIdKey, checkSecurity: false );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Authorization is checked through the cache so inherited page and
        // site security participates.
        var blockCache = BlockCache.Get( block.Id, rockContext );

        if ( blockCache == null || !blockCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
        {
            helper.AddError( "You are not authorized to delete that block." );

            return helper.ErrorResult;
        }

        var name = block.Name;
        var pageId = block.PageId;
        var layoutId = block.LayoutId;
        var siteId = block.SiteId;

        // Any CustomComponent row cascades with the block, which is the
        // intended unwind for a scratch component.
        new BlockService( rockContext ).Delete( block );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Flush the pages the block rendered on so it disappears.
        if ( pageId.HasValue )
        {
            PageCache.FlushPage( pageId.Value );
        }
        else if ( layoutId.HasValue )
        {
            PageCache.FlushPagesForLayout( layoutId.Value );
        }
        else if ( siteId.HasValue )
        {
            PageCache.FlushPagesForSite( siteId.Value );
        }

        return Success( new BlockDeleteResult
        {
            IsDeleted = true,
            Name = name
        } );
    }

    #endregion
}
