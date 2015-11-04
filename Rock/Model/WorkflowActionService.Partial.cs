// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;

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
            // Get all of the active form actions with an activity that assigned to the current user
            var formActionsQry = GetActiveForms().Where( a =>
                        ( a.Activity.AssignedPersonAlias != null && a.Activity.AssignedPersonAlias.PersonId == person.Id ) ||
                        ( a.Activity.AssignedGroup != null && a.Activity.AssignedGroup.Members.Any( m => m.PersonId == person.Id ) )
                 );

            // Check security for the action's activity type and workflow type
            var workflowTypeIds = new Dictionary<int, bool>();
            var activityTypeIds = new Dictionary<int, bool>();
            
            var assignedActivityTypes = formActionsQry.Select(a => a.ActionType.ActivityType ).Distinct().ToList();

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
            var authorizedActivityTypeIds = activityTypeIds.Where( w => w.Value ).Select( w => w.Key).ToList();

            // Get the actions that user is authorized to see and that 
            return formActionsQry.Where( a => authorizedActivityTypeIds.Contains( a.ActionType.ActivityTypeId ) )
                .ToList();
        }
    }
}