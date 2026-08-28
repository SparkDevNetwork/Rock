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
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the values of a single defined type.
    /// </summary>
    /// <remarks>
    /// Named for the entity rather than the relationship. The parent is a
    /// required parameter, not part of the name, matching ListGroupMembers.
    /// </remarks>
    [Description( "Lists the values of a single defined type." )]
    [AgentPurpose( "Retrieves the values a caller can choose from for a defined type." )]
    [AgentToolPrerequisite( "Call ListDefinedTypes to determine the definedTypeIdKey." )]
    [AgentToolGuid( "0351DA93-E519-48D6-BB05-21D93A9583CA" )]
    public AgentToolResult ListDefinedValues(
        string definedTypeIdKey,
        string partialValue = null,
        string categoryIdKey = null,
        bool includeInactive = false,
        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var definedType = helper.GetRequiredEntity<Rock.Model.DefinedType>( definedTypeIdKey );

        if ( definedType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListDefinedTypes )} function to determine the available defined types." );
        }

        var definedTypeCache = DefinedTypeCache.Get( definedType.Id, AgentRequestContext.RockContext );
        var isCategorized = definedTypeCache?.CategorizedValuesEnabled ?? false;

        var values = DefinedValueCache.All( AgentRequestContext.RockContext )
            .Where( dv => dv.DefinedTypeId == definedType.Id );

        // Security is conditional on the parent type. When values are secured the
        // filter runs across the whole collection here, before paging, so that a
        // page is never short and hasMoreItems is never wrong.
        if ( definedType.EnableSecurityOnValues )
        {
            values = values.Where( dv => dv.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) );
        }

        if ( !includeInactive )
        {
            values = values.Where( dv => dv.IsActive );
        }

        var query = values.AsQueryable();

        if ( partialValue.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( dv => dv.Value.ContainsIgnoreCase( partialValue )
                || dv.Description.ContainsIgnoreCase( partialValue ) );
        }

        query = helper.WhereOptionalIdKey( query, dv => dv.CategoryId, categoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var orderedValues = query
            .OrderBy( dv => dv.Order )
            .ThenBy( dv => dv.Value )
            .ThenBy( dv => dv.Id )
            .ToList();

        var page = helper.GetPaginatedItems( orderedValues, pageNumber );

        var resultPage = page.WithItems( page.Items
            .Select( dv => new DefinedValueResult
            {
                Id = dv.Id,

                // A defined value is referenced by Guid in workflow action
                // settings and attribute default values, so the list has to
                // carry it or a caller has no way to reach one.
                Guid = dv.Guid,
                Value = dv.Value,
                Description = dv.Description,

                // Only when the type uses categories. A null category on every
                // row of a type that does not use them is noise.
                Category = isCategorized && dv.CategoryId.HasValue
                    ? new KeyNameResult { Id = dv.CategoryId.Value, Guid = CategoryCache.Get( dv.CategoryId.Value )?.Guid, Name = dv.CategoryName }
                    : null,
                AttributeValues = dv.GetGridAttributeValueResults( AgentRequestContext ).ToList()
            } )
            .ToList() );

        var historyPage = page.WithItems( page.Items
            .Select( dv => new KeyNameResult { Id = dv.Id, Guid = dv.Guid, Name = dv.Value } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
