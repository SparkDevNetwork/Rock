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
//
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.WorkflowActionType"/> entity objects
    /// </summary>
    public partial class WorkflowActionService
    {
        /// <summary>
        /// Gets the active forms.
        /// </summary>
        /// <returns></returns>
        public IQueryable<WorkflowAction> GetActiveForms()
        {
            var rockContext = this.Context as RockContext;

            return Queryable()
                .Where( a =>
                    a.ActionType.WorkflowFormId.HasValue &&
                    ( a.ActionType.ActivityType.IsActive ?? true ) &&
                    ( a.ActionType.ActivityType.WorkflowType.IsActive ?? true ) &&
                    a.ActionType.ActivityType.WorkflowType.CategoryId.HasValue &&
                    !a.CompletedDateTime.HasValue &&
                    a.Activity.ActivatedDateTime.HasValue &&
                    !a.Activity.CompletedDateTime.HasValue &&
                    a.Activity.Workflow.ActivatedDateTime.HasValue &&
                    !a.Activity.Workflow.CompletedDateTime.HasValue );
        }

        /// <summary>
        /// Gets the active forms.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public List<WorkflowAction> GetActiveForms( Person person )
        {
            // fetch all the person's alias ids to avoid a join that might be costly
            var personAliasIds = person.Aliases.Select( a => a.Id ).ToList();

            // Get all of the active activities that are assigned to the specified person
            var assignedActiveActivityIdList = new WorkflowActivityService( this.Context as RockContext ).Queryable()
                .Where( a => a.ActivatedDateTime.HasValue && !a.CompletedDateTime.HasValue )
                .Where( a =>
                 ( a.AssignedPersonAliasId.HasValue && personAliasIds.Contains( a.AssignedPersonAliasId.Value ) ) ||
                         ( a.AssignedGroupId.HasValue && a.AssignedGroup.Members.Any( m => m.PersonId == person.Id ) ) ).Select( a => a.Id ).ToList();

            // Get all of the active form actions with an activity that assigned to the specified person
            var formActionsQry = GetActiveForms().Where( a => assignedActiveActivityIdList.Contains( a.ActivityId ) );

            // Check security for the action's activity type and workflow type
            var workflowTypeIds = new Dictionary<int, bool>();
            var activityTypeIds = new Dictionary<int, bool>();

            var result = formActionsQry.Include( a => a.Activity.Workflow ).ToList();

            var assignedActivityTypes = result.Select( a => a.ActionTypeCache.ActivityType ).Distinct().ToList();

            foreach ( var assignedActivityType in assignedActivityTypes )
            {
                if ( !workflowTypeIds.ContainsKey( assignedActivityType.WorkflowTypeId ) )
                {
                    workflowTypeIds.Add( assignedActivityType.WorkflowTypeId, assignedActivityType.WorkflowType.IsAuthorized( Rock.Security.Authorization.VIEW, person ) );
                }

                if ( workflowTypeIds[assignedActivityType.WorkflowTypeId] && !activityTypeIds.ContainsKey( assignedActivityType.Id ) )
                {
                    activityTypeIds.Add( assignedActivityType.Id, assignedActivityType.IsAuthorized( Rock.Security.Authorization.VIEW, person ) );
                }
            }

            // Get just the authorized activity types
            var authorizedActivityTypeIds = activityTypeIds.Where( w => w.Value ).Select( w => w.Key ).ToList();

            // Get the actions that user is authorized to see and that 
            return result.Where( a => authorizedActivityTypeIds.Contains( a.ActionType.ActivityTypeId ) )
                .ToList();
        }
    }
}