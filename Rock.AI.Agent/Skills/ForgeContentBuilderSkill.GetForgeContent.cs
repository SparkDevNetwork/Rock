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
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ForgeContentBuilderSkill
{
    #region Tool(s)

    [Description( "Reads the current authored source of a Forge Content block placement so it can be iterated on." )]
    [AgentToolPreamble( "Reading the current component source." )]
    [AgentUsage( "blockId is the id of the Forge Content block placement to read." )]
    [AgentToolGuid( "D24F8B61-9C07-4E5A-B173-60A4F2C8E9D3" )]
    public AgentToolResult GetForgeContent(
        [Description( "The id of the Forge Content block placement to read." )]
        string blockId )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var block = helper.GetRequiredEntity<Model.Block>( blockId, checkSecurity: false );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Source is only exposed to editors; a plain viewer never receives it,
        // matching the block's own view-mode behavior. Block security is checked
        // through the cache so inherited page and site security participates.
        var blockCache = BlockCache.Get( block.Id, rockContext );

        if ( blockCache == null || !blockCache.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            helper.AddError( "You are not authorized to read this component." );

            return helper.ErrorResult;
        }

        var content = new ForgeContentService( rockContext ).GetByBlockId( block.Id );

        if ( content == null || content.Source.IsNullOrWhiteSpace() )
        {
            return NoData();
        }

        return Success( new ForgeContentResult
        {
            Source = content.Source,
            CompiledVueVersion = content.CompiledVueVersion
        } );
    }

    #endregion
}
