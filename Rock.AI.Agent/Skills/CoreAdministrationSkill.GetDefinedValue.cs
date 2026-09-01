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
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single defined value in full detail.
    /// </summary>
    /// <remarks>
    /// This is where a caller obtains a defined value's Guid. Rock stores defined
    /// value references as Guids in several places, and the list tool leaves the
    /// Guid out so it is not repeated on every row of every page.
    /// </remarks>
    [Description( "Gets a single defined value in full detail, including its unique identifier." )]
    [AgentPurpose( "Retrieves one defined value, including the unique identifier needed to reference it." )]
    [AgentToolPrerequisite( "Call ListDefinedValues to determine the definedValueIdKey." )]
    [AgentToolGuid( "BF14C7EA-98DC-4FFF-8485-F9952B2F4B8B" )]
    public AgentToolResult GetDefinedValue( string definedValueIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var definedValue = helper.GetRequiredEntity<Rock.Model.DefinedValue>( definedValueIdKey );

        if ( definedValue == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListDefinedValues )} function to determine the available defined values." );
        }

        var definedType = DefinedTypeCache.Get( definedValue.DefinedTypeId, AgentRequestContext.RockContext );

        var category = definedValue.CategoryId.HasValue
            ? CategoryCache.Get( definedValue.CategoryId.Value, AgentRequestContext.RockContext )
            : null;

        definedValue.LoadAttributes( AgentRequestContext.RockContext );

        var result = new DefinedValueDetailResult
        {
            Id = definedValue.Id,
            Guid = definedValue.Guid,
            Value = definedValue.Value,
            Description = definedValue.Description,
            Order = definedValue.Order,
            IsActive = definedValue.IsActive,
            DefinedType = KeyNameResult.FromCache( definedType ),
            Category = KeyNameResult.FromCache( category ),
            AttributeValues = definedValue.GetAttributeValueResults( AgentRequestContext ).ToList()
        };

        // Required, not optional. GetAttributeValueResults filters only on
        // IsPublic; the per-attribute VIEW authorization happens here. Paged
        // results get this from GetPaginatedResult, but Success does not sanitize,
        // so a Get returning attribute values has to ask for it.
        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this defined value." );
        }

        return Success( result )
            .WithHistoryContent( new KeyNameResult( definedValue.Id, definedValue.Guid, definedValue.Value ) );
    }

    #endregion
}
