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
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Removes a workflow attribute and every stored value for it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The stored values cascade from the attribute, so every value this variable
    /// ever held across every workflow instance goes with it, silently. That is what
    /// the counts in the result are for: they are the only report of how much data
    /// was involved.
    /// </para>
    /// <para>
    /// Form fields that edit the attribute are removed too. Action settings are not,
    /// because an action stores an attribute reference inside a setting value where
    /// nothing can find it. An action that referenced this attribute keeps a
    /// reference that no longer resolves.
    /// </para>
    /// </remarks>
    [Description( "Removes a workflow attribute, every form field that edits it, and every value stored for it across all existing workflow instances." )]
    [AgentUsage( "Report the stored value count to the person before calling this. Actions that reference the attribute inside a setting value cannot be detected and will keep a reference that no longer resolves." )]
    [AgentGuardrail( "This permanently deletes the workflow attribute and every stored value for it across all existing workflow instances. Confirm the attribute with the person before proceeding." )]
    [AgentToolPrerequisite( "Call GetWorkflowType to determine the attributeIdKey." )]
    [AgentToolGuid( "A8A8FA55-A8D5-4791-991C-691E5D8279C3" )]
    public AgentToolResult DeleteWorkflowAttribute( string attributeIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var attributeService = new AttributeService( rockContext );
        var formAttributeService = new WorkflowActionFormAttributeService( rockContext );

        var attribute = helper.GetRequiredEntity<Rock.Model.Attribute>( attributeIdKey );

        if ( attribute == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the workflow's attributes." );
        }

        // Proves this is a workflow or activity attribute before removing it, so the
        // tool cannot be used to delete arbitrary attributes elsewhere in Rock.
        var owner = GetAttributeOwner( attribute, helper, rockContext );

        if ( owner == null )
        {
            return helper.ErrorResult;
        }

        var attributeName = attribute.Name;
        var attributeKey = attribute.Key;

        // Counted before the delete. Both cascade, so afterwards there is no way to
        // report what went.
        var storedValueCount = new AttributeValueService( rockContext ).Queryable()
            .Count( av => av.AttributeId == attribute.Id );

        var formFields = formAttributeService.Queryable()
            .Where( fa => fa.AttributeId == attribute.Id )
            .ToList();

        var formFieldCount = formFields.Count;

        // Removed explicitly rather than left to the cascade, so the form is
        // consistent at every point rather than only after the attribute goes.
        if ( formFieldCount > 0 )
        {
            formAttributeService.DeleteRange( formFields );

            helper.SaveChangesIfNoErrors();

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }
        }

        if ( !attributeService.CanDelete( attribute, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        attributeService.Delete( attribute );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( new
        {
            IsDeleted = true,
            Key = attributeKey,
            Name = attributeName,
            owner.Scope,
            DeletedStoredValueCount = storedValueCount,
            DeletedFormFieldCount = formFieldCount,
            WorkflowTypeIdKey = owner.WorkflowType?.IdKey,
            ActivityTypeIdKey = owner.ActivityType?.IdKey
        } );
    }

    #endregion
}
