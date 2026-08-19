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
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Constants

    /// <summary>
    /// The processing interval given to a new workflow type when the caller does not
    /// ask for one, in seconds. One day, which Rock's own screen would show as 1440
    /// minutes.
    /// </summary>
    private const int DefaultProcessingIntervalSeconds = 86400;

    #endregion

    #region Tool(s)

    /// <summary>
    /// Adds a workflow type or updates an existing one.
    /// </summary>
    /// <remarks>
    /// This creates the shell only. Attributes, activities, and actions are added by
    /// their own tools, so building one workflow takes several calls. There is no
    /// transaction across them, which is why a failed call should be followed by
    /// reading the tree back rather than assuming what landed.
    /// </remarks>
    [Description( "Adds a new workflow type or updates an existing one. This creates the workflow shell only; its attributes, activities, and actions are added separately." )]
    [AgentUsage( "name and categoryIdKey are required when adding. Supplying workflowTypeIdKey updates that workflow type and leaves any parameter you omit unchanged." )]
    [AgentToolPrerequisite( "Call ListCategories with the WorkflowType entity type to determine the categoryIdKey." )]
    [AgentToolGuid( "DD2120CD-0FD6-45FC-8633-60FFA69B16CC" )]
    public AgentToolResult AddOrUpdateWorkflowType(
        string workflowTypeIdKey = null,
        string categoryIdKey = null,
        string name = null,
        SetOrClear<string> description = null,
        bool? isActive = null,
        [Description( "Whether every instance is saved to the database from the moment it is created. Defaults to false. Persisting every workflow is the most common performance mistake in Rock, so leave this off unless the workflow has no forms and its state must exist before any of them. For a workflow with a form, add a Workflow Persist action after the form instead, which saves the submitted values without creating a row for people who never finish." )]
        bool? isPersisted = null,
        [Description( "How much detail is written to the workflow log. Defaults to None. Verbose logging on a busy workflow produces a large amount of data." )]
        WorkflowLoggingLevel? loggingLevel = null,
        [Description( "The noun used for one instance of this workflow, such as Request." )]
        SetOrClear<string> workTerm = null,
        [Description( "The CSS class for the workflow's icon, such as 'ti ti-clipboard-text'." )]
        SetOrClear<string> iconCssClass = null,
        [Description( "The Lava template used to summarize one instance of the workflow." )]
        SetOrClear<string> summaryViewText = null,
        [Description( "The message shown when the workflow reaches a point with nothing for the person to do." )]
        SetOrClear<string> noActionMessage = null,
        [Description( "A short prefix put in front of each instance's number, such as 'REQ'. Cosmetic, but it is what people quote back to you." )]
        SetOrClear<string> workflowIdPrefix = null,
        [Description( "The URL fragment this workflow's form is reached by, when it is used as a public entry form." )]
        SetOrClear<string> slug = null,
        [Description( "The shortest time that must pass before the Process Workflows job picks up the same waiting instance again, in seconds. Rock's own screen asks for this in minutes, so multiply by 60. Defaults to 86400 (one day) on a new workflow type." )]
        SetOrClear<int> processingIntervalSeconds = null,
        [Description( "How many days of workflow log entries to keep. Omit to keep them indefinitely, which on a busy workflow is how the log becomes the largest table in the database." )]
        SetOrClear<int> logRetentionPeriod = null,
        [Description( "How many days to keep completed instances before they are removed." )]
        SetOrClear<int> completedWorkflowRetentionPeriod = null,
        [Description( "How many days an incomplete instance may live before it is removed regardless of state." )]
        SetOrClear<int> maxWorkflowAgeDays = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var workflowTypeService = new WorkflowTypeService( rockContext );

        Rock.Model.WorkflowType workflowType;
        var isNew = workflowTypeIdKey.IsNullOrWhiteSpace();

        if ( !isNew )
        {
            workflowType = helper.GetRequiredEntity<Rock.Model.WorkflowType>( workflowTypeIdKey );

            if ( workflowType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListWorkflowTypes )} function to determine the available workflow types." );
            }
        }
        else
        {
            if ( name.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( name )} is required when adding a workflow type." );
            }

            if ( categoryIdKey.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( categoryIdKey )} is required when adding a workflow type." )
                    .WithInstructions( $"Call the {nameof( CoreAdministrationSkill.ListCategories )} function with the WorkflowType entity type to determine the available categories. An uncategorized workflow type is hard for anyone to find afterwards." );
            }

            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            workflowType = rockContext.Set<Rock.Model.WorkflowType>().Create();

            workflowType.IsActive = true;
            workflowType.WorkTerm = "Work";

            /*
                8/18/26 - CLAUDE

                Seeded to one day rather than zero. This value only gates how often the
                Process Workflows job revisits a persisted instance that is waiting, so
                it costs a workflow that runs straight through nothing: that workflow
                finishes on its first pass and is never revisited. Zero meant every
                waiting instance was re-examined on every job run, which is the setting
                that hurts on a workflow type with a lot of open instances, and it was
                being applied by default to workflow types nobody had thought about.

                Rock's own workflow type screen seeds 28800 (8 hours) for the same
                reason. A day is used here instead because a caller who cared about the
                interval would have said so, and the ones who say nothing are building
                request-and-approve workflows where a daily sweep is plenty.

                Reason: A default nobody chose should be the cheap one.
            */
            workflowType.ProcessingIntervalSeconds = DefaultProcessingIntervalSeconds;

            // Both defaults are deliberate rather than inherited. Persisting
            // every workflow is the most common performance mistake in Rock, and
            // verbose logging left on after debugging is the second.
            workflowType.IsPersisted = false;
            workflowType.LoggingLevel = WorkflowLoggingLevel.None;

            workflowTypeService.Add( workflowType );
        }

        if ( categoryIdKey.IsNotNullOrWhiteSpace() )
        {
            var category = helper.GetRequiredEntity<Rock.Model.Category>( categoryIdKey );

            if ( category == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( CoreAdministrationSkill.ListCategories )} function with the WorkflowType entity type to determine the available categories." );
            }

            workflowType.CategoryId = category.Id;
        }

        helper.UpdateProperty( workflowType, wt => wt.Name, name );
        helper.UpdateProperty( workflowType, wt => wt.Description, description );
        helper.UpdateProperty( workflowType, wt => wt.IsActive, isActive );
        helper.UpdateProperty( workflowType, wt => wt.IsPersisted, isPersisted );
        helper.UpdateProperty( workflowType, wt => wt.LoggingLevel, loggingLevel );
        helper.UpdateProperty( workflowType, wt => wt.WorkTerm, workTerm );
        helper.UpdateProperty( workflowType, wt => wt.IconCssClass, iconCssClass );
        helper.UpdateProperty( workflowType, wt => wt.SummaryViewText, summaryViewText );
        helper.UpdateProperty( workflowType, wt => wt.NoActionMessage, noActionMessage );
        helper.UpdateProperty( workflowType, wt => wt.WorkflowIdPrefix, workflowIdPrefix );
        helper.UpdateProperty( workflowType, wt => wt.Slug, slug );
        helper.UpdateProperty( workflowType, wt => wt.ProcessingIntervalSeconds, processingIntervalSeconds );
        helper.UpdateProperty( workflowType, wt => wt.LogRetentionPeriod, logRetentionPeriod );
        helper.UpdateProperty( workflowType, wt => wt.CompletedWorkflowRetentionPeriod, completedWorkflowRetentionPeriod );
        helper.UpdateProperty( workflowType, wt => wt.MaxWorkflowAgeDays, maxWorkflowAgeDays );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Saving is enough to refresh the cache. WorkflowType is ICacheable, and the
        // context updates those entries as part of the save.
        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var activityTypeCount = new WorkflowActivityTypeService( rockContext ).Queryable()
            .Count( at => at.WorkflowTypeId == workflowType.Id );

        var result = new WorkflowTypeSummaryResult
        {
            Id = workflowType.Id,
            Guid = workflowType.Guid,
            Name = workflowType.Name,
            Description = workflowType.Description,
            Category = workflowType.CategoryId.HasValue
                ? GetCategoryKeyName( workflowType.CategoryId.Value, rockContext )
                : null,
            IsActive = workflowType.IsActive ?? true,
            IsPersisted = workflowType.IsPersisted,
            LoggingLevel = workflowType.LoggingLevel,
            ActivityTypeCount = activityTypeCount
        };

        var toolResult = Success( result )
            .WithInstructions( isNew
                ? $"The workflow type has been created. Add its variables with {nameof( AddOrUpdateWorkflowAttribute )}, then its activities with {nameof( AddOrUpdateWorkflowActivityType )}."
                : "The workflow type has been updated." )
            .WithHistoryContent( new KeyNameResult( workflowType.Id, workflowType.Guid, workflowType.Name ) );

        // Reported rather than rejected. A guessed icon class shipped dead in an
        // early build and raised no error, but Font Awesome classes are still valid
        // in older themes, so refusing one would break a legitimate value.
        var suppliedIconCssClass = iconCssClass?.ClearValue == false ? iconCssClass.Value : null;

        if ( suppliedIconCssClass.IsNotNullOrWhiteSpace() && !suppliedIconCssClass.StartsWith( TablerIconPrefix ) )
        {
            toolResult = toolResult
                .WithInstructions( $"The icon class '{suppliedIconCssClass}' does not start with '{TablerIconPrefix}', which is what current Rock themes use. If this was a guess rather than a class you looked up, it will render as nothing and raise no error." );
        }

        return toolResult;
    }

    #endregion
}
