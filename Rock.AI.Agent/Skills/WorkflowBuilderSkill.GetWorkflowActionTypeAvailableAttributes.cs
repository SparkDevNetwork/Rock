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
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets the settings a workflow action component accepts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Takes the component's entity type rather than a configured action, which is
    /// what makes it work before the action exists. A caller adding an action has
    /// nothing to point at yet, and a caller editing one already has the component
    /// key from the read tools. An action's settings depend on nothing but its
    /// component, so there is no per-action variation for a second parameter to
    /// resolve.
    /// </para>
    /// <para>
    /// Returns the bare attribute shape, like every other tool in this family. A
    /// derived shape carrying the current value was considered and rejected: the
    /// helper hands back base instances that cannot be upcast, so a richer shape
    /// would mean rebuilding the results here and copying the visibility and
    /// authorization filters with them. Two copies of a security filter drift.
    /// Current values come from the read tools instead.
    /// </para>
    /// </remarks>
    [Description( "Gets the settings a workflow action component accepts, along with each setting's field type and allowed values." )]
    [AgentPurpose( "Provides the setting definitions for an action so its configuration can be supplied correctly." )]
    [AgentUsage( "Works whether or not an action of this kind exists yet, so call it before adding an action as well as before editing one. Where a setting's fieldType references another record, Workflow Attribute and Workflow Text Or Attribute among them, the value supplied for it in AddOrUpdateWorkflowActionType's settings must be that record's guid, never its idKey." )]
    [AgentToolPrerequisite( "Call LookupWorkflowActionComponents to determine the actionEntityTypeIdKey." )]
    [AgentToolGuid( "99871B11-0F69-4E0D-BCBA-446317F8B5B6" )]
    public AgentToolResult GetWorkflowActionTypeAvailableAttributes( string actionEntityTypeIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var actionEntityType = helper.GetRequiredEntity<Rock.Model.EntityType>( actionEntityTypeIdKey );

        if ( actionEntityType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( LookupWorkflowActionComponents )} function to determine the available actions." );
        }

        // An action's settings are attributes on WorkflowActionType qualified by the
        // component's entity type. A stub carrying only that qualifier is enough for
        // LoadAttributes to resolve them, which is what lets this answer before any
        // action of this kind exists, and it keeps the tool on the standard helper
        // so no bespoke result shape is needed.
        var stubActionType = new WorkflowActionType
        {
            EntityTypeId = actionEntityType.Id
        };

        stubActionType.LoadAttributes( AgentRequestContext.RockContext );

        // Active and Order are suppressed, and this is not cosmetic. Every action
        // carries them because ActionComponent overrides both, and a model that sees
        // a setting named Order will reasonably assume it controls the action's
        // position in the activity. It does not. Position is a property on the
        // action, set through AddOrUpdateWorkflowActionType. Writing the attribute
        // produces a junk value, leaves ordering untouched, and raises no error.
        var settings = helper.GetAvailableAttributes( stubActionType )
            .Where( a => !VestigialSettingKeys.Contains( a.Key ) )
            .ToList();

        if ( !settings.Any() )
        {
            return NoData()
                .WithInstructions( $"This action takes no settings. Add it with {nameof( AddOrUpdateWorkflowActionType )} and supply no settings." );
        }

        /*
            8/19/26 - CLAUDE

            The instruction below exists because silence reads as agreement. An action
            usually mixes settings that accept a workflow attribute guid with settings
            that do not, and only the former describe themselves. A caller that reads
            three settings saying a guid is a reference and two saying nothing at all
            will generalize, and store a raw attribute guid in a plain text setting,
            where it is kept verbatim and shown to the person as a guid.

            The test is deliberately the presence of valueFormat rather than a list of
            field type class names. The field types that accept a reference already
            describe themselves through GetFieldHints and the ones that do not return
            null, so the distinction is already carried in the payload. A second copy
            here would be one more thing to keep in step with them.

            It is sent unconditionally. Deciding whether a given action mixes the two
            kinds would mean inspecting the wording of each valueFormat, which is the
            fragile coupling this avoids, and the statement holds either way.

            Reason: A setting that says nothing about guids was being read as one that
            accepts them.
        */

        // No paging and no cap. One component's settings are bounded by its own
        // declaration, which is a handful of fields.
        return Success( settings )
            .WithInstructions( "A setting turns a guid into a reference to another record only when its valueFormat says it does. Where a setting has no valueFormat, the value is stored exactly as supplied, so a guid stays literal text and is never read as a reference. To use a workflow attribute's value in such a setting, supply Lava that reads the attribute by its key rather than supplying its guid, such as {{ Workflow | Attribute:'SampleKey' }} for an attribute on the workflow type or {{ Activity | Attribute:'SampleKey' }} for one on the containing activity." )
            .WithHistoryKey( $"workflow-action-settings-{actionEntityTypeIdKey}" );
    }

    #endregion
}
