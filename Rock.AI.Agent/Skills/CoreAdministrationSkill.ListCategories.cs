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
    /// Lists the categories defined for one entity type.
    /// </summary>
    [Description( "Lists the categories defined for one entity type, such as the categories a workflow type can be filed under." )]
    [AgentPurpose( "Finds the category an entity should be filed under." )]
    [AgentToolPrerequisite( "Call ListEntityTypes to determine the entityTypeIdKey." )]
    [AgentToolGuid( "8B1EFF0E-AAE0-43BF-A2DA-D1C71EADF28B" )]
    public AgentToolResult ListCategories(
        string entityTypeIdKey,
        string partialName = null,
        string parentCategoryIdKey = null,
        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var entityType = helper.GetRequiredEntity<Rock.Model.EntityType>( entityTypeIdKey );

        if ( entityType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListEntityTypes )} function to determine the available entity types." );
        }

        // Security filtering runs across the whole collection here, before
        // paging. GetPaginatedItems does no authorization and takes no person, so
        // filtering after it would yield short pages and a wrong hasMoreItems.
        var categories = CategoryCache.All( AgentRequestContext.RockContext )
            .Where( c => c.EntityTypeId == entityType.Id )
            .Where( c => c.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .AsQueryable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            categories = categories.Where( c => c.Name != null && c.Name.Contains( partialName ) );
        }

        categories = helper.WhereOptionalIdKey( categories, c => c.ParentCategoryId, parentCategoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var orderedCategories = categories
            .OrderBy( c => c.Order )
            .ThenBy( c => c.Name )
            .ThenBy( c => c.Id )
            .ToList();

        var page = helper.GetPaginatedItems( orderedCategories, pageNumber );

        var resultPage = page.WithItems( page.Items
            .Select( c => new CategoryResult
            {
                Id = c.Id,
                Name = c.Name,

                // The parent stays in the list result. Without it a flat
                // rendering of a deep tree is unreadable.
                ParentCategory = c.ParentCategory != null
                    ? new KeyNameResult { Id = c.ParentCategory.Id, Name = c.ParentCategory.Name }
                    : null
            } )
            .ToList() );

        var historyPage = page.WithItems( page.Items
            .Select( c => new KeyNameResult { Id = c.Id, Name = c.Name } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
