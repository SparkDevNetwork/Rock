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
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets the attributes that apply to every entity of a given type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the generic member of the Get{Entity}AvailableAttributes family,
    /// covering the entities that never justify a dedicated tool. It is
    /// deliberately not named GetEntityTypeAvailableAttributes, which would read
    /// as returning attributes <em>of</em> an entity type rather than <em>for</em>
    /// entities of that type.
    /// </para>
    /// <para>
    /// It has no qualifier support. Qualifiers range from none, to one optional,
    /// to several required, depending on the entity, and a single column and value
    /// pair cannot express that. A caller supplying the wrong pair would get a
    /// plausible but wrong set, which is worse than getting none. Qualified cases
    /// get dedicated tools, as defined values do.
    /// </para>
    /// </remarks>
    [Description( "Gets the attributes that apply to every entity of a given type. Returns only unqualified attributes; entities whose attributes depend on a qualifier have their own tool." )]
    [AgentPurpose( "Provides the attribute definitions available on an entity type, along with any value format instructions." )]
    [AgentToolPrerequisite( "Call ListEntityTypes to determine the entityTypeIdKey." )]
    [AgentToolGuid( "2A0EF1D6-8C10-4E9C-BCDB-0FCA3FEC0998" )]
    public AgentToolResult GetEntityAvailableAttributes( string entityTypeIdKey, string partialName = null, int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var entityType = helper.GetRequiredEntity<Rock.Model.EntityType>( entityTypeIdKey );

        if ( entityType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListEntityTypes )} function to determine the available entity types." );
        }

        // Unqualified attributes only, which is what makes this safe to answer for
        // any entity type without knowing that type's qualifier scheme.
        var attributes = AttributeCache.All( AgentRequestContext.RockContext )
            .Where( a => a.EntityTypeId == entityType.Id )
            .Where( a => a.EntityTypeQualifierColumn.IsNullOrWhiteSpace() )
            .AsQueryable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            attributes = attributes.Where( a =>
                ( a.Name != null && a.Name.Contains( partialName ) )
                || ( a.Key != null && a.Key.Contains( partialName ) ) );
        }

        var orderedAttributes = attributes
            .OrderBy( a => a.Order )
            .ThenBy( a => a.Name )
            .ThenBy( a => a.Id )
            .ToList();

        // Paging here departs from the rest of the family, which returns whole
        // sets. Those tools are each scoped to one entity, so the ceiling is
        // knowable. This one takes any entity type in the system, so its ceiling
        // is the worst case across all of them and no argument the caller supplies
        // changes that.
        //
        // A cursor is not an option and would not help. The source is an
        // in-memory collection rather than an IQueryable, so CursorPaginator
        // cannot seek over it, and there are no database round trips to save.
        var page = helper.GetPaginatedItems( orderedAttributes, pageNumber, AttributePageSize );

        // Security and visibility filtering happen inside the helper, which is the
        // same code path the entity-based AvailableAttributes tools use. Building
        // the results here instead would mean a second copy of those filters.
        var resultPage = page.WithItems( helper.GetAvailableAttributes( page.Items ) );

        return helper.GetPaginatedResult( resultPage );
    }

    #endregion

    #region Constants

    /// <summary>
    /// The page size for attribute definitions, deliberately larger than the
    /// default. An attribute definition is a handful of small fields, so 200 rows
    /// is a modest payload, while the default of 50 would put an entity type like
    /// Person at six or more round trips for what is usually a single reference
    /// lookup.
    /// </summary>
    private const int AttributePageSize = 200;

    #endregion
}
