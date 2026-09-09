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
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets the attributes that can be set on a block.
    /// </summary>
    /// <remarks>
    /// A block's attributes are qualified by its block type, so the set can be
    /// resolved either from an existing block or, before the block is created,
    /// from the block type alone.
    /// </remarks>
    [Description( "Gets the attributes (block settings) that can be set when adding or updating a block. These are specific to the block type." )]
    [AgentPurpose( "Determines which attribute values (block settings) AddOrUpdateBlock accepts." )]
    [AgentUsage( "To inspect an existing block's settings, pass blockIdKey. To see what a not-yet-created block will accept, omit blockIdKey and pass blockTypeIdKey instead." )]
    [AgentToolPrerequisite( "Call ListBlocks to determine the blockIdKey, or ListBlockTypes to determine the blockTypeIdKey when the block does not exist yet." )]
    [AgentToolGuid( "D29258E4-BDC2-40FE-B4F2-EC3DC745413C" )]
    public AgentToolResult GetBlockAvailableAttributes(
        [Description( "The IdKey or guid of an existing block. Omit when inspecting a block type before the block is created." )]
        string blockIdKey = null,
        [Description( "The IdKey or guid of the block type. Used when blockIdKey is not provided to resolve the attributes a new block of that type will accept." )]
        string blockTypeIdKey = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        Model.Block block;

        if ( blockIdKey.IsNotNullOrWhiteSpace() )
        {
            block = helper.GetRequiredEntity<Model.Block>( blockIdKey, checkSecurity: true );

            if ( block == null )
            {
                return helper.ErrorResult
                    .WithInstructions( "Call the ListBlocks function to determine the available blocks." );
            }
        }
        else
        {
            var blockType = helper.GetRequiredEntity<Model.BlockType>( blockTypeIdKey, checkSecurity: false );

            if ( blockType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Provide a blockIdKey for an existing block, or call the {nameof( ListBlockTypes )} function to determine the blockTypeIdKey." );
            }

            // A block's attributes are qualified by its block type, so a stub
            // block with only the block type set resolves the same attributes a
            // newly created block of that type would have.
            block = new Model.Block
            {
                BlockTypeId = blockType.Id
            };
        }

        block.LoadAttributes( AgentRequestContext.RockContext );

        return Success( helper.GetAvailableAttributes( block ) );
    }

    #endregion
}
