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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.WorkflowSkill;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class WorkflowSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the workflows that match the supplied filters.
    /// </summary>
    /// <remarks>
    /// A live instance can hold a very large number of workflows, so at least one
    /// filter is required to narrow the set and the results are cursor paged.
    /// Workflows derive from Model&lt;T&gt; and are secured per row, so this uses
    /// CursorPaginator to filter by view permission while paging. Only workflows
    /// whose type the person can edit or view the list of, workflows they
    /// started, and (when they filter on themselves) workflows with an active
    /// activity assigned to them are returned. The "assigned to"
    /// filter follows the My Workflows block: a workflow matches when one of its
    /// activities is assigned to the person directly or through an active group
    /// membership.
    /// </remarks>
    [Description( "Lists the workflows that match the filters. Workflows are the running (or completed) instances of a workflow type." )]
    [AgentPurpose( "Finds existing workflows so they can be read." )]
    [AgentUsage( "At least one filter must be provided. The assignedToPersonIdKey filter returns workflows that have an activity assigned to that person, directly or through a group they belong to." )]
    [AgentToolPrerequisite( "Call LookupWorkflowTypes to determine the workflowTypeIdKey, and SearchPerson to determine any person idKey." )]
    [AgentToolGuid( "22AEE8DA-576C-43DE-8EBD-66E81EAF9208" )]
    public AgentToolResult ListWorkflows(
        string workflowTypeIdKey = null,
        string initiatedByPersonIdKey = null,
        string assignedToPersonIdKey = null,
        bool? isActive = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;
        var rockContext = AgentRequestContext.RockContext;

        // At least one narrowing filter is required so the tool cannot return
        // every workflow in the system. The lifecycle (isActive) filter is not
        // considered narrowing on its own.
        var hasAnyFilter = workflowTypeIdKey.IsNotNullOrWhiteSpace()
            || initiatedByPersonIdKey.IsNotNullOrWhiteSpace()
            || assignedToPersonIdKey.IsNotNullOrWhiteSpace()
            || startDate.HasValue
            || endDate.HasValue;

        if ( !hasAnyFilter )
        {
            helper.AddError( "At least one of workflowTypeIdKey, initiatedByPersonIdKey, assignedToPersonIdKey, startDate or endDate must be provided to limit the results returned." );
            return helper.ErrorResult;
        }

        var query = new WorkflowService( rockContext ).Queryable()
            .Include( w => w.InitiatorPersonAlias.Person )
            .Include( w => w.Activities );

        query = helper.WhereOptionalIdKey( query, w => w.WorkflowTypeId, workflowTypeIdKey );
        query = helper.WhereOptionalIdKey( query, w => w.InitiatorPersonAlias.PersonId, initiatedByPersonIdKey );
        query = helper.WhereOptionalPropertyBetween( query, w => w.ActivatedDateTime, startDate, endDate );

        // A workflow is "assigned to" a person when one of its activities is
        // assigned to them directly or through an active membership in the
        // assigned group. This mirrors the My Workflows block. The subquery is
        // left unexecuted so EF generates a single WHERE ... IN (SELECT ...)
        // rather than materializing a potentially large id list.
        var currentPersonId = currentPerson?.Id ?? 0;
        IQueryable<int> ownActiveWorkflowIdQuery = null;

        if ( assignedToPersonIdKey.IsNotNullOrWhiteSpace() )
        {
            var assignedPerson = helper.GetRequiredEntity<Rock.Model.Person>( assignedToPersonIdKey, checkSecurity: false );

            if ( assignedPerson == null )
            {
                return helper.ErrorResult
                    .WithInstructions( "Call the SearchPerson function to determine the person idKey." );
            }

            var assignedPersonId = assignedPerson.Id;

            var assignedActivityQuery = new WorkflowActivityService( rockContext ).Queryable()
                .Where( a =>
                    ( a.AssignedPersonAlias != null && a.AssignedPersonAlias.PersonId == assignedPersonId )
                    || ( a.AssignedGroup != null && a.AssignedGroup.Members.Any( m => m.PersonId == assignedPersonId && m.GroupMemberStatus != GroupMemberStatus.Inactive ) ) );

            var assignedWorkflowIdQuery = assignedActivityQuery.Select( a => a.WorkflowId );

            query = query.Where( w => assignedWorkflowIdQuery.Contains( w.Id ) );

            // When people filter on themselves they also see workflows that
            // have an active activity assigned to them, like My Workflows.
            if ( currentPerson != null && assignedPersonId == currentPersonId )
            {
                ownActiveWorkflowIdQuery = assignedActivityQuery
                    .Where( a => a.ActivatedDateTime.HasValue
                        && !a.CompletedDateTime.HasValue
                        && a.Workflow.ActivatedDateTime.HasValue
                        && !a.Workflow.CompletedDateTime.HasValue )
                    .Select( a => a.WorkflowId );
            }
        }

        // isActive true limits to activated, not-yet-completed workflows; false
        // limits to completed workflows; null returns both.
        if ( isActive.HasValue )
        {
            query = isActive.Value
                ? query.Where( w => w.ActivatedDateTime.HasValue && !w.CompletedDateTime.HasValue )
                : query.Where( w => w.CompletedDateTime.HasValue );
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // The ThenBy on Id is the unique tiebreaker the cursor needs so two
        // workflows activated at the same time cannot produce the same cursor.
        var paginator = new CursorPaginator<Rock.Model.Workflow>( currentPerson, qry => qry
            .OrderByDescending( w => w.ActivatedDateTime.HasValue )
            .ThenByDescending( w => w.ActivatedDateTime )
            .ThenBy( w => w.Id ) );

        /*
            10/16/2026 - MSE

            Workflow VIEW is the workflow type's launch right, so the paginator
            alone lets anyone who can launch a type list every instance of it.
            Reading instances needs EDIT or VIEW_LIST, like Workflow List and
            Workflow Detail. People can still see workflows they started, and
            the ones with an active activity assigned to them when they filter
            on themselves, like the My Workflows block.

            The main filter runs in SQL so the paginator, which stops after a
            fixed number of batches, does not hide the person's own workflows
            behind rows it would drop. The paginator predicate below repeats the
            check per workflow so rules on a single workflow are still applied.

            Reason: Do not list workflows the person is not allowed to view.
        */
        var listableWorkflowTypeIds = WorkflowTypeCache.All( rockContext )
            .Where( t => t.IsAuthorized( Authorization.EDIT, currentPerson )
                || t.IsAuthorized( Authorization.VIEW_LIST, currentPerson ) )
            .Select( t => t.Id )
            .ToList();

        var ownActiveWorkflowIds = new HashSet<int>();

        if ( ownActiveWorkflowIdQuery != null )
        {
            query = query.Where( w => listableWorkflowTypeIds.Contains( w.WorkflowTypeId )
                || ( w.InitiatorPersonAliasId.HasValue && w.InitiatorPersonAlias.PersonId == currentPersonId )
                || ownActiveWorkflowIdQuery.Contains( w.Id ) );

            ownActiveWorkflowIds = new HashSet<int>( ownActiveWorkflowIdQuery.Distinct().ToList() );
        }
        else
        {
            query = query.Where( w => listableWorkflowTypeIds.Contains( w.WorkflowTypeId )
                || ( w.InitiatorPersonAliasId.HasValue && w.InitiatorPersonAlias.PersonId == currentPersonId ) );
        }

        paginator.AddPredicate( w => w.IsAuthorized( Authorization.EDIT, currentPerson )
            || w.IsAuthorized( Authorization.VIEW_LIST, currentPerson )
            || ( currentPerson != null && w.InitiatorPersonAlias?.PersonId == currentPerson.Id )
            || ownActiveWorkflowIds.Contains( w.Id ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( w => new WorkflowResult
            {
                Id = w.Id,
                Guid = w.Guid,
                Name = w.Name,
                WorkflowType = KeyNameResult.FromCache( WorkflowTypeCache.Get( w.WorkflowTypeId, rockContext ) ),
                Status = w.Status,
                IsActive = w.ActivatedDateTime.HasValue && !w.CompletedDateTime.HasValue,
                ActivatedDateTime = w.ActivatedDateTime,
                CompletedDateTime = w.CompletedDateTime,
                InitiatedByPerson = PersonResult.NameOnly( w.InitiatorPersonAlias ),
                ActiveActivityNames = w.Activities
                    .Where( wa => wa.ActivatedDateTime.HasValue && !wa.CompletedDateTime.HasValue && wa.ActivityTypeCache != null && wa.ActivityTypeCache.IsActive != false )
                    .OrderBy( wa => wa.ActivityTypeCache.Order )
                    .Select( wa => wa.ActivityTypeCache.Name )
                    .ToList()
            } )
            .ToList() );

        var historyPage = cursorPage.WithItems( cursorPage.Items
            .Select( w => new KeyNameResult { Id = w.Id, Guid = w.Guid, Name = w.Name } ) );

        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
