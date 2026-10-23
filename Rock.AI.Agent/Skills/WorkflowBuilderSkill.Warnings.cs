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
using System.Linq;

using Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Warning Helpers

    /*
        9/16/26 - CLAUDE

        Every defect reported here is one Rock accepts without complaint and never
        reports at run time: the workflow saves, opens correctly in the builder, and
        then either never runs a step or never reaches a completed state. An agent
        cannot learn any of them from behavior, because there is no failure to observe.

        They are warnings rather than errors because each shape is occasionally
        deliberate. An action that waits on criteria some other process will satisfy
        later is a documented pattern, not a mistake, so refusing the write would
        break it.

        Reason: Surface the workflow defects that Rock itself stays silent about.
    */

    /// <summary>
    /// Finds the defects in a single action that can be seen without looking at the
    /// rest of the workflow.
    /// </summary>
    /// <remarks>
    /// Split out from <see cref="GetStructuralWarnings"/> so a tool holding one action
    /// can run the same checks. Reachability is deliberately not here: it is a property
    /// of the whole tree and cannot be answered from one action.
    /// </remarks>
    /// <param name="actionType">The action to inspect.</param>
    /// <param name="activityTypeName">The owning activity's name, included in the message when the caller is reporting on a whole workflow. Omit when the action is the only thing being reported on.</param>
    /// <returns>A warning for each defect found, empty when there are none.</returns>
    private static List<string> GetActionWarnings( WorkflowActionTypeResult actionType, string activityTypeName = null )
    {
        var warnings = new List<string>();

        var location = activityTypeName.IsNullOrWhiteSpace()
            ? $"Action '{actionType.Name}'"
            : $"Action '{actionType.Name}' in '{activityTypeName}'";

        if ( actionType.Criteria != null && !actionType.IsActionCompletedIfCriteriaUnmet )
        {
            warnings.Add( $"{location} has criteria but is not completed when they go unmet. If they never match, the action stays open, so its activity never completes and neither does the workflow. Set isActionCompletedIfCriteriaUnmet unless the workflow is meant to wait for those criteria." );
        }

        if ( !actionType.IsActionCompletedOnSuccess )
        {
            warnings.Add( $"{location} is not completed on success, so it holds its activity open even after it runs." );
        }

        return warnings;
    }

    /// <summary>
    /// Finds the defects in a whole workflow: activities nothing can reach, and
    /// actions that can never complete.
    /// </summary>
    /// <remarks>
    /// Runs entirely against the assembled result rather than the database, so it
    /// costs no additional queries. Everything it needs was already gathered to build
    /// the tree.
    /// </remarks>
    /// <param name="workflowType">The assembled workflow tree.</param>
    /// <returns>A warning for each defect found, empty when there are none.</returns>
    private static List<string> GetStructuralWarnings( WorkflowTypeDetailResult workflowType )
    {
        var warnings = new List<string>();
        var activityTypes = workflowType.ActivityTypes;

        // A workflow with no activities is part way through being built rather than
        // broken. Reporting on it would fire on every read during normal construction
        // and teach the caller to ignore the whole field.
        if ( activityTypes == null || !activityTypes.Any() )
        {
            return warnings;
        }

        // Guid is nullable on the result base but is always populated for an activity
        // in this tree. Nulls are skipped rather than assumed, so a missing one can never
        // be reported as unreachable.
        var activityGuids = new HashSet<Guid>( activityTypes.Where( at => at.Guid.HasValue ).Select( at => at.Guid.Value ) );
        var activityGuidsByIdKey = activityTypes.Where( at => at.Guid.HasValue ).ToDictionary( at => at.IdKey, at => at.Guid.Value );

        var reachableGuids = new HashSet<Guid>( activityTypes
            .Where( at => at.IsActivatedWithWorkflow && at.Guid.HasValue )
            .Select( at => at.Guid.Value ) );

        foreach ( var actionType in activityTypes.SelectMany( at => at.ActionTypes ?? new List<WorkflowActionTypeResult>() ) )
        {
            foreach ( var button in actionType.Form?.Buttons ?? new List<WorkflowFormButtonResult>() )
            {
                if ( button.ActivateActivityIdKey.IsNotNullOrWhiteSpace()
                    && activityGuidsByIdKey.TryGetValue( button.ActivateActivityIdKey, out var buttonTargetGuid ) )
                {
                    reachableGuids.Add( buttonTargetGuid );
                }
            }

            /*
                Matched on the setting's value rather than on the action's class name.
                Several components activate an activity and a plugin can add another,
                so recognizing the reference is more reliable than maintaining a list
                of the components that write one. A setting naming an activity for some
                other purpose counts as a route too, which can only ever suppress a
                warning rather than invent one, and that is the safer direction.
            */
            foreach ( var setting in actionType.Settings?.Values ?? Enumerable.Empty<WorkflowActionSettingResult>() )
            {
                var settingGuid = setting.Value.AsGuidOrNull();

                if ( settingGuid.HasValue && activityGuids.Contains( settingGuid.Value ) )
                {
                    reachableGuids.Add( settingGuid.Value );
                }
            }
        }

        if ( !activityTypes.Any( at => at.IsActivatedWithWorkflow ) )
        {
            warnings.Add( "No activity is activated with the workflow, so nothing happens when it launches. Set isActivatedWithWorkflow on the activity that should start it." );
        }

        foreach ( var activityType in activityTypes )
        {
            if ( activityType.Guid.HasValue && !reachableGuids.Contains( activityType.Guid.Value ) )
            {
                warnings.Add( $"Activity '{activityType.Name}' is unreachable: it is not activated with the workflow, no form button activates it, and no action setting names it. It will never run." );

                // Its actions are not worth reporting on when the activity itself
                // never runs. Listing them would bury the one warning that matters.
                continue;
            }

            foreach ( var actionType in activityType.ActionTypes ?? new List<WorkflowActionTypeResult>() )
            {
                warnings.AddRange( GetActionWarnings( actionType, activityType.Name ) );
            }
        }

        return warnings;
    }

    #endregion Warning Helpers
}
