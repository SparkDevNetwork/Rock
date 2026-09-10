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
using Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a workflow type whole: its settings, its attributes, and every activity
    /// and action beneath it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is also the recovery tool. Writes are not wrapped in a transaction
    /// across calls, so a failed sequence can leave a workflow half built. Reading
    /// the whole tree back is how a caller finds out what actually exists before
    /// continuing.
    /// </para>
    /// <para>
    /// Long setting values and form markup are clipped here. The purpose of the
    /// clipped text is recognition rather than reading, and a caller that needs a
    /// value whole asks for that one action.
    /// </para>
    /// </remarks>
    [Description( "Gets a workflow type in full, including its attributes and every activity and action. Long values are clipped." )]
    [AgentPurpose( "Provides the complete current structure of a workflow, which is what editing it requires." )]
    [AgentUsage( "Call this after a failed write to see what was actually saved before continuing. Values marked as truncated can be read whole with GetWorkflowActionType." )]
    [AgentToolPrerequisite( "Call LookupWorkflowTypes to determine the workflowTypeIdKey." )]
    [AgentToolGuid( "01D84BCE-8B18-4200-9435-3D1F5572BD92" )]
    public AgentToolResult GetWorkflowTypeConfiguration( string workflowTypeIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var workflowType = helper.GetRequiredEntity<Rock.Model.WorkflowType>( workflowTypeIdKey );

        if ( workflowType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( WorkflowSkill.LookupWorkflowTypes )} function to determine the available workflow types." );
        }

        // Everything the tree needs, pulled in one query rather than letting each
        // collection load on demand. A thirty action workflow would otherwise cost
        // a round trip per action and per form.
        var activityTypes = new WorkflowActivityTypeService( rockContext )
            .Queryable( "ActionTypes.WorkflowForm.FormAttributes,ActionTypes.WorkflowForm.FormSections" )
            .Where( at => at.WorkflowTypeId == workflowType.Id )
            .ToList();

        // Built once for the whole tree. Setting values that hold an activity or
        // attribute identifier get a readable name alongside the raw value, which
        // removes the most common cross-referencing step when reading a workflow.
        var referenceNames = GetWorkflowReferenceNames( workflowType.Id, rockContext );

        var result = new WorkflowTypeDetailResult
        {
            Id = workflowType.Id,
            Guid = workflowType.Guid,
            Name = workflowType.Name,
            Description = workflowType.Description,
            Category = GetCategoryKeyName( workflowType.CategoryId, rockContext ),
            IsActive = workflowType.IsActive ?? true,
            IsPersisted = workflowType.IsPersisted,
            IsFormBuilder = workflowType.IsFormBuilder,
            LoggingLevel = workflowType.LoggingLevel,
            WorkTerm = workflowType.WorkTerm,
            IconCssClass = workflowType.IconCssClass,
            SummaryViewText = workflowType.SummaryViewText,
            NoActionMessage = workflowType.NoActionMessage,
            WorkflowIdPrefix = workflowType.WorkflowIdPrefix,
            Slug = workflowType.Slug,
            ProcessingIntervalSeconds = workflowType.ProcessingIntervalSeconds,
            LogRetentionPeriod = workflowType.LogRetentionPeriod,
            CompletedWorkflowRetentionPeriod = workflowType.CompletedWorkflowRetentionPeriod,
            MaxWorkflowAgeDays = workflowType.MaxWorkflowAgeDays,
            Attributes = GetWorkflowAttributeResults( workflowType.Id, rockContext ),
            ActivityTypes = activityTypes
                .OrderBy( at => at.Order )
                .ThenBy( at => at.Id )
                .Select( at => new WorkflowActivityTypeResult
                {
                    Id = at.Id,
                    Guid = at.Guid,
                    Name = at.Name,
                    Description = at.Description,
                    Order = at.Order,
                    IsActive = at.IsActive ?? true,
                    IsActivatedWithWorkflow = at.IsActivatedWithWorkflow,
                    Attributes = GetActivityAttributeResults( at.Id, rockContext ),
                    ActionTypes = at.ActionTypes
                        .OrderBy( actionType => actionType.Order )
                        .ThenBy( actionType => actionType.Id )

                        // Clipped, which is what separates this from the single
                        // action read. Both go through the same renderer so a
                        // setting reads the same either way.
                        .Select( actionType => GetActionTypeResult( actionType, rockContext, clipLongValues: true, referenceNames ) )
                        .ToList()
                } )
                .ToList()
        };

        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this workflow type." );
        }

        // The tree is far too large for chat history, so it is kept out of it
        // entirely and referenced by key instead.
        return Success( result )
            .WithHistoryKey( $"workflow-type-{workflowType.IdKey}" )
            .WithoutHistoryContent();
    }

    #endregion
}
