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
using Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the workflow types in Rock.
    /// </summary>
    /// <remarks>
    /// Returns a summary rather than the full tree. A caller is usually looking for
    /// one workflow type by name, and returning every activity and action for every
    /// match would be a large payload for a lookup. The activity count is included
    /// so a caller can tell an empty shell from a built workflow before deciding to
    /// read it.
    /// </remarks>
    [Description( "Lists the workflow types in Rock, with the count of activities in each." )]
    [AgentPurpose( "Finds an existing workflow type and the key needed to read or edit it." )]
    [AgentToolPrerequisite( "Call ListCategories with the WorkflowType entity type to determine the categoryIdKey, if filtering by category." )]
    [AgentToolGuid( "5B0804A3-F2F4-4F5E-AB3C-E0F618370BE1" )]
    public AgentToolResult ListWorkflowTypes(
        string partialName = null,
        string categoryIdKey = null,
        bool includeInactive = false,
        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;
        var currentPerson = AgentRequestContext.CurrentPerson;

        // Security filtering runs across the whole collection before paging, so
        // that a page is never short and hasMoreItems is never wrong.
        var workflowTypes = WorkflowTypeCache.All( rockContext )
            .Where( wt => wt.IsAuthorized( Authorization.VIEW, currentPerson ) );

        if ( !includeInactive )
        {
            workflowTypes = workflowTypes.Where( wt => wt.IsActive ?? true );
        }

        var query = workflowTypes.AsQueryable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( wt =>
                ( wt.Name != null && wt.Name.Contains( partialName ) )
                || ( wt.Description != null && wt.Description.Contains( partialName ) ) );
        }

        query = helper.WhereOptionalIdKey( query, wt => wt.CategoryId, categoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var orderedTypes = query
            .OrderBy( wt => wt.Order )
            .ThenBy( wt => wt.Name )
            .ThenBy( wt => wt.Id )
            .ToList();

        if ( !orderedTypes.Any() )
        {
            return NoData()
                .WithInstructions( $"No workflow type matched. Call {nameof( ListWorkflowTypes )} with no filter to see what exists, or add one with {nameof( AddOrUpdateWorkflowType )}." );
        }

        var page = helper.GetPaginatedItems( orderedTypes, pageNumber );
        var pageIds = page.Items.Select( wt => wt.Id ).ToList();

        // One grouped query for the page rather than a count per row.
        var activityCounts = new WorkflowActivityTypeService( rockContext ).Queryable()
            .Where( at => pageIds.Contains( at.WorkflowTypeId ) )
            .GroupBy( at => at.WorkflowTypeId )
            .Select( g => new { WorkflowTypeId = g.Key, ActivityTypeCount = g.Count() } )
            .ToDictionary( g => g.WorkflowTypeId, g => g.ActivityTypeCount );

        var resultPage = page.WithItems( page.Items
            .Select( wt => new WorkflowTypeSummaryResult
            {
                Id = wt.Id,
                Guid = wt.Guid,
                Name = wt.Name,
                Description = wt.Description,
                Category = wt.CategoryId.HasValue
                    ? GetCategoryKeyName( wt.CategoryId.Value, rockContext )
                    : null,
                IsActive = wt.IsActive ?? true,
                IsPersisted = wt.IsPersisted,
                LoggingLevel = wt.LoggingLevel,
                ActivityTypeCount = activityCounts.TryGetValue( wt.Id, out var count ) ? count : 0
            } )
            .ToList() );

        var historyPage = page.WithItems( page.Items
            .Select( wt => new KeyNameResult { Id = wt.Id, Name = wt.Name } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
