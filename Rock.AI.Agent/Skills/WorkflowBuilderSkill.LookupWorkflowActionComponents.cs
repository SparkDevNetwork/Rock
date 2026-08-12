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
using Rock.SystemGuid;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Looks up the workflow action components installed in Rock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Named for the component rather than for WorkflowAction or
    /// WorkflowActionType. Both of those are real Rock tables holding different
    /// things: a running action instance and a configured action inside an
    /// activity. This returns neither. It reflects over the installed
    /// ActionComponent classes, which is what a caller picks from when adding an
    /// action.
    /// </para>
    /// <para>
    /// Reflecting over the container rather than reading a static list is the
    /// point: plugin and custom actions appear alongside core ones, which no
    /// amount of authored documentation can provide.
    /// </para>
    /// </remarks>
    [Description( "Looks up the workflow action components installed in Rock, meaning the kinds of action that can be added to a workflow activity." )]
    [AgentPurpose( "Finds the action to add to a workflow activity, and the key needed to add it." )]
    [AgentUsage( "Call this before adding an action. Custom and plugin actions appear here alongside the built-in ones." )]
    [AgentToolGuid( "D319EB2C-F2CE-44F2-80E1-0705C6AC68DF" )]
    public AgentToolResult LookupWorkflowActionComponents( string partialName = null, string category = null )
    {
        var components = ActionContainer.Instance.Components
            .Select( c => c.Value.Value )
            .Where( c => c != null )
            .Select( c =>
            {
                var componentType = c.GetType();
                var entityType = EntityTypeCache.Get( componentType, createIfNotFound: false, AgentRequestContext.RockContext );

                if ( entityType == null )
                {
                    return null;
                }

                return new WorkflowActionComponentResult
                {
                    EntityTypeIdKey = entityType.Id.AsIdKey(),
                    EntityTypeGuid = entityType.Guid,
                    ClassName = componentType.FullName,
                    Name = ActionContainer.GetComponentName( componentType.FullName ),
                    Category = componentType.GetCustomAttributes( typeof( ActionCategoryAttribute ), true )
                        .OfType<ActionCategoryAttribute>()
                        .FirstOrDefault()?.CategoryName,
                    Description = componentType.GetCustomAttributes( typeof( DescriptionAttribute ), false )
                        .OfType<DescriptionAttribute>()
                        .FirstOrDefault()?.Description
                };
            } )
            .Where( c => c != null )
            .AsEnumerable();

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            components = components.Where( c =>
                ( c.Name != null && c.Name.IndexOf( partialName, System.StringComparison.OrdinalIgnoreCase ) >= 0 )
                || ( c.ClassName != null && c.ClassName.IndexOf( partialName, System.StringComparison.OrdinalIgnoreCase ) >= 0 ) );
        }

        if ( category.IsNotNullOrWhiteSpace() )
        {
            // A plain string, not an IdKey. Action categories come from component
            // metadata rather than the Category entity.
            components = components.Where( c => c.Category != null
                && c.Category.Equals( category, System.StringComparison.OrdinalIgnoreCase ) );
        }

        var results = components
            .OrderBy( c => c.Category )
            .ThenBy( c => c.Name )
            .ToList();

        if ( !results.Any() )
        {
            return NoData()
                .WithInstructions( $"No workflow action matched. Call {nameof( LookupWorkflowActionComponents )} with no filter to see every installed action." );
        }

        // No paging and no cap. The set is bounded by installed code, so it only
        // moves when a plugin is installed.
        return Success( results )
            .WithHistoryKey( "workflow-actions" );
    }

    #endregion
}
