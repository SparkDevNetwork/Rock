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
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the defined types configured in Rock.
    /// </summary>
    /// <remarks>
    /// This is a List rather than a Lookup because churches create defined types
    /// freely, so the set grows with data and has to page.
    /// </remarks>
    [Description( "Lists the defined types configured in Rock. A defined type is a named set of values, such as Marital Status or Connection Status." )]
    [AgentPurpose( "Finds a defined type so its values can be retrieved." )]
    [AgentUsage( "Call this first when a value needs to come from a fixed set, then list the type's values." )]
    [AgentToolGuid( "53DDA7C1-00A5-4531-8A5D-07FBC6721798" )]
    public AgentToolResult ListDefinedTypes( string partialName = null, string categoryIdKey = null, int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        // Read from cache rather than the database. Every field returned lives on
        // the cache, and a materialized collection can be security filtered in
        // full before paging.
        var definedTypes = DefinedTypeCache.All( AgentRequestContext.RockContext )
            .Where( dt => dt.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .AsQueryable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            definedTypes = definedTypes.Where( dt => dt.Name != null && dt.Name.Contains( partialName ) );
        }

        definedTypes = helper.WhereOptionalIdKey( definedTypes, dt => dt.CategoryId, categoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var orderedTypes = definedTypes
            .OrderBy( dt => dt.Name )
            .ThenBy( dt => dt.Id )
            .Select( dt => new DefinedTypeResult
            {
                Id = dt.Id,
                Guid = dt.Guid,
                Name = dt.Name,
                Category = dt.Category != null
                    ? new KeyNameResult { Id = dt.Category.Id, Guid = dt.Category.Guid, Name = dt.Category.Name }
                    : null
            } );

        var page = helper.GetPaginatedItems( orderedTypes, pageNumber );

        // No history. Even trimmed to two fields this runs to a few hundred rows
        // on a live instance, which is more than chat history should carry for
        // reference data the agent can re-fetch on demand.
        return helper.GetPaginatedResult( page )
            .WithoutHistoryContent();
    }

    #endregion
}
