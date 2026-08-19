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
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the entity types registered in Rock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There is no way to filter by kind of entity type, meaning "only models" or
    /// "only field types". Rock has no classification column, so such a filter
    /// would have to be computed from cache and container membership. That work
    /// is scoped separately.
    /// </para>
    /// <para>
    /// A substring filter serves the real use case, which is resolving a named
    /// entity type for the category or attribute tools. A caller always knows the
    /// name it wants.
    /// </para>
    /// </remarks>
    [Description( "Lists the entity types registered in Rock, such as Person, Group, or Workflow. Filter by partial name to find the one you need." )]
    [AgentPurpose( "Resolves a named entity type so categories or attributes can be looked up for it." )]
    [AgentUsage( "Supply a partialName. The unfiltered set runs to well over a thousand rows." )]
    [AgentToolGuid( "7BD8DF7C-09AA-4809-8364-37D594370E99" )]
    public AgentToolResult ListEntityTypes( string partialName = null, int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        // Rows must come from the cache. Guid, FriendlyName, and IsSecured exist
        // only on the EntityType table, so no other source can populate them.
        var entityTypes = EntityTypeCache.All( AgentRequestContext.RockContext ).AsQueryable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            entityTypes = entityTypes.Where( et =>
                ( et.Name != null && et.Name.Contains( partialName ) )
                || ( et.FriendlyName != null && et.FriendlyName.Contains( partialName ) ) );
        }

        var orderedEntityTypes = entityTypes
            .OrderBy( et => et.FriendlyName )
            .ThenBy( et => et.Id )
            .Select( et => new EntityTypeResult
            {
                Id = et.Id,
                Guid = et.Guid,
                Name = et.Name,
                FriendlyName = et.FriendlyName,
                IsEntity = et.IsEntity,
                IsSecured = et.IsSecured
            } );

        var page = helper.GetPaginatedItems( orderedEntityTypes, pageNumber );

        // No history. The result is expected to be large and it is reference data
        // the agent can re-fetch. A conditional rule that stored it only for small
        // pages was considered and rejected as harder to reason about later than
        // the saving is worth.
        return helper.GetPaginatedResult( page )
            .WithoutHistoryContent();
    }

    #endregion
}
