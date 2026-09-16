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
    /// Gets the configuration of a single defined type.
    /// </summary>
    /// <remarks>
    /// This returns neither the type's values nor the attribute definitions that
    /// apply to them. Values are the common path and are large enough to page, so
    /// they have their own tool; attribute definitions are a different shape from
    /// attribute values and have theirs.
    /// </remarks>
    [Description( "Gets the configuration of a single defined type. This does not include the type's values." )]
    [AgentPurpose( "Retrieves the settings of one defined type." )]
    [AgentToolPrerequisite( "Call ListDefinedTypes to determine the definedTypeIdKey." )]
    [AgentToolGuid( "366B42BD-9D92-4042-8B20-04EE6B0142C7" )]
    public AgentToolResult GetDefinedType( string definedTypeIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        // Read from the model rather than the cache. HelpText and
        // EnableSecurityOnValues exist only on the DefinedType table, and this
        // returns a single row so there is nothing to save by caching.
        var definedType = helper.GetRequiredEntity<Rock.Model.DefinedType>( definedTypeIdKey );

        if ( definedType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListDefinedTypes )} function to determine the available defined types." );
        }

        var category = definedType.CategoryId.HasValue
            ? CategoryCache.Get( definedType.CategoryId.Value, AgentRequestContext.RockContext )
            : null;

        var result = new DefinedTypeDetailResult
        {
            Id = definedType.Id,
            Guid = definedType.Guid,
            Name = definedType.Name,
            Description = definedType.Description,
            HelpText = definedType.HelpText,
            Category = KeyNameResult.FromCache( category ),
            IsSystem = definedType.IsSystem,
            IsActive = definedType.IsActive,
            CategorizedValuesEnabled = definedType.CategorizedValuesEnabled ?? false,

            // Returned because it changes how ListDefinedValues behaves. Without
            // it a caller cannot tell a short value list from a security filtered
            // one.
            EnableSecurityOnValues = definedType.EnableSecurityOnValues,

            ValueCount = new DefinedValueService( AgentRequestContext.RockContext )
                .Queryable()
                .Count( dv => dv.DefinedTypeId == definedType.Id )
        };

        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this defined type." );
        }

        return Success( result )
            .WithInstructions( $"Call the {nameof( ListDefinedValues )} function to retrieve this type's values." )
            .WithHistoryContent( new KeyNameResult( definedType.Id, definedType.Guid, definedType.Name ) );
    }

    #endregion
}
