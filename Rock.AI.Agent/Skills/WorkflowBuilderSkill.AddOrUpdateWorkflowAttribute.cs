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
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;
using Rock.Model;
using Rock.Configuration;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Adds a workflow attribute or updates an existing one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Workflow attributes are the variables a workflow's actions read and write,
    /// and the fields its forms present. They are Attribute rows on the Workflow
    /// entity qualified by the workflow type, not rows of their own table, which is
    /// why they are managed here rather than through a generic attribute tool. Both
    /// the qualifier column and its value have to be set, or the attribute appears
    /// on every workflow in the system.
    /// </para>
    /// <para>
    /// Changing the field type of an attribute is refused once instances exist. The
    /// stored values were written in the old type's format, and nothing converts
    /// them. A Single Select holding "S", "M", and "L" across four hundred instances
    /// does not become valid Person values, and nothing throws. With no instances
    /// there is nothing stored to corrupt and the change is allowed.
    /// </para>
    /// </remarks>
    [Description( "Adds a workflow attribute, which is a variable the workflow's actions read and write, or updates an existing one." )]
    [AgentUsage( "key, name, and fieldTypeIdKey are required when adding. Supplying attributeIdKey updates that attribute and leaves any parameter you omit unchanged. Supply at most one of insertAfterAttributeIdKey or insertBeforeAttributeIdKey. The response returns both idKey and guid: use idKey to call this skill again, and guid when referencing this attribute from another action's settings or another attribute's defaultValue." )]
    [AgentToolPrerequisite( "Call LookupFieldTypes to determine the fieldTypeIdKey, and GetFieldType to determine the valid configurationValues keys for it." )]
    [AgentGuardrail( "The field type cannot be changed once the workflow has saved instances, because existing values were stored in the old format." )]
    [AgentToolGuid( "8C371846-1F64-4A47-AB68-416CC584C85E" )]
    public AgentToolResult AddOrUpdateWorkflowAttribute(
        [Description( "Required when editing an existing attribute." )]
        string attributeIdKey = null,
        [Description( "The workflow type, for a variable every activity can read and write. Supply this or activityTypeIdKey when adding, not both." )]
        string workflowTypeIdKey = null,
        [Description( "The activity, for a variable only that activity's actions can read and write. Supply this or workflowTypeIdKey when adding, not both." )]
        string activityTypeIdKey = null,
        [Description( "The programmatic key actions use to reference this attribute. Cannot be changed once actions reference it." )]
        string key = null,
        string name = null,
        SetOrClear<string> description = null,
        string fieldTypeIdKey = null,
        bool? isRequired = null,
        // One of the four slots stored unchanged, so it holds a guid rather than an
        // idKey. Named in the skill description; keep the two in step.
        [Description( "The value the attribute starts with. Written into Rock's stored configuration unchanged, so when this attribute's field type references another record, such as a person, defined value, connection opportunity, or system phone number, this must be that record's guid. Get it from the target entity's own Get tool, never from its idKey." )]
        SetOrClear<string> defaultValue = null,
        [Description( "The field type's qualifiers, as key and value pairs. Get the valid keys from GetFieldType." )]
        Dictionary<string, string> configurationValues = null,
        [Description( "The key of the attribute this one should follow." )]
        string insertAfterAttributeIdKey = null,
        [Description( "The key of the attribute this one should precede." )]
        string insertBeforeAttributeIdKey = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var attributeService = new AttributeService( rockContext );

        var placement = ResolveSiblingPlacement( insertAfterAttributeIdKey, insertBeforeAttributeIdKey, helper );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        Rock.Model.Attribute attribute;
        Rock.Model.WorkflowType workflowType;
        WorkflowActivityType activityType = null;
        var scope = WorkflowAttributeScope.Workflow;
        var isNew = attributeIdKey.IsNullOrWhiteSpace();

        if ( !isNew )
        {
            attribute = helper.GetRequiredEntity<Rock.Model.Attribute>( attributeIdKey );

            if ( attribute == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the workflow's attributes." );
            }

            // This resolves the owner, tells us which scope the attribute is in, and
            // proves it is a workflow or activity attribute at all. Without the check
            // any attribute in Rock could be edited here.
            var owner = GetAttributeOwner( attribute, helper, rockContext );

            if ( owner == null )
            {
                return helper.ErrorResult;
            }

            scope = owner.Scope;
            workflowType = owner.WorkflowType;
            activityType = owner.ActivityType;

            // The scope of an existing attribute is fixed. Moving one between scopes
            // would mean a new qualifier on rows that existing values already point
            // at, so it is refused rather than half done.
            if ( scope == WorkflowAttributeScope.Workflow && activityTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                return Error( $"The attribute '{attribute.Name}' belongs to the whole workflow and cannot be moved into an activity." )
                    .WithInstructions( "Add a new activity attribute and remove this one if that is what you want." );
            }

            if ( scope == WorkflowAttributeScope.Activity && workflowTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                return Error( $"The attribute '{attribute.Name}' belongs to the activity '{activityType?.Name}' and cannot be moved to the whole workflow." )
                    .WithInstructions( "Add a new workflow attribute and remove this one if that is what you want." );
            }
        }
        else
        {
            var hasWorkflowParent = workflowTypeIdKey.IsNotNullOrWhiteSpace();
            var hasActivityParent = activityTypeIdKey.IsNotNullOrWhiteSpace();

            if ( hasWorkflowParent && hasActivityParent )
            {
                return Error( $"Supply either {nameof( workflowTypeIdKey )} or {nameof( activityTypeIdKey )}, not both." )
                    .WithInstructions( "A variable belongs to the whole workflow or to one activity. Which one decides where it is stored and which actions can reach it." );
            }

            if ( !hasWorkflowParent && !hasActivityParent )
            {
                return Error( $"Either {nameof( workflowTypeIdKey )} or {nameof( activityTypeIdKey )} is required when adding an attribute." )
                    .WithInstructions( $"Call the {nameof( ListWorkflowTypes )} function to determine the available workflow types, or supply {nameof( attributeIdKey )} to update an existing attribute instead." );
            }

            if ( hasActivityParent )
            {
                scope = WorkflowAttributeScope.Activity;

                activityType = helper.GetRequiredEntity<WorkflowActivityType>( activityTypeIdKey );

                if ( activityType == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the available activities." );
                }

                workflowType = activityType.WorkflowType;
            }
            else
            {
                workflowType = helper.GetRequiredEntity<Rock.Model.WorkflowType>( workflowTypeIdKey );

                if ( workflowType == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( ListWorkflowTypes )} function to determine the available workflow types." );
                }
            }

            if ( key.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( key )} is required when adding a workflow attribute." )
                    .WithInstructions( "The key is what actions use to reference the attribute, so it has to be chosen deliberately and stays fixed afterwards." );
            }

            if ( name.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( name )} is required when adding a workflow attribute." );
            }

            if ( fieldTypeIdKey.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( fieldTypeIdKey )} is required when adding a workflow attribute." )
                    .WithInstructions( $"Call the {nameof( CoreAdministrationSkill.LookupFieldTypes )} function to determine the available field types." );
            }

            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            attribute = rockContext.Set<Rock.Model.Attribute>().Create();

            // The two scopes are stored against different entities with different
            // qualifiers, which is the whole difference between them.
            if ( scope == WorkflowAttributeScope.Activity )
            {
                attribute.EntityTypeId = EntityTypeCache.GetId<WorkflowActivity>();
                attribute.EntityTypeQualifierColumn = "ActivityTypeId";
                attribute.EntityTypeQualifierValue = activityType.Id.ToString();
            }
            else
            {
                attribute.EntityTypeId = EntityTypeCache.GetId<Rock.Model.Workflow>();
                attribute.EntityTypeQualifierColumn = "WorkflowTypeId";
                attribute.EntityTypeQualifierValue = workflowType.Id.ToString();
            }

            attributeService.Add( attribute );
        }

        if ( key.IsNotNullOrWhiteSpace() )
        {
            var scopeSiblings = scope == WorkflowAttributeScope.Activity
                ? GetActivityAttributes( activityType.Id, rockContext )
                : GetWorkflowAttributes( workflowType.Id, rockContext );

            var isKeyTaken = scopeSiblings
                .Any( a => a.Id != attribute.Id && a.Key.Equals( key, StringComparison.OrdinalIgnoreCase ) );

            if ( isKeyTaken )
            {
                var owner = scope == WorkflowAttributeScope.Activity ? $"the activity '{activityType.Name}'" : $"the workflow type '{workflowType.Name}'";

                return Error( $"Another attribute on {owner} already uses the key '{key}'." )
                    .WithInstructions( "Supply a different key, or update the existing attribute instead of adding one." );
            }

            attribute.Key = key;
        }

        if ( fieldTypeIdKey.IsNotNullOrWhiteSpace() )
        {
            var fieldType = helper.GetRequiredEntity<Rock.Model.FieldType>( fieldTypeIdKey );

            if ( fieldType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( CoreAdministrationSkill.LookupFieldTypes )} function to determine the available field types." );
            }

            var isFieldTypeChanging = !isNew && attribute.FieldTypeId != fieldType.Id;

            if ( isFieldTypeChanging )
            {
                // Checked at the moment of the write, never inferred from anything
                // the caller says.
                var instanceCount = GetWorkflowInstanceCount( workflowType.Id, rockContext );

                if ( instanceCount > 0 )
                {
                    return Error( $"The field type of '{attribute.Name}' cannot be changed because the workflow type '{workflowType.Name}' has {instanceCount} saved instance(s) holding values in the current format. Changing it would leave every one of those values unreadable." )
                        .WithInstructions( $"Add a new attribute with the field type you want and leave this one in place, or remove the existing instances first. There is no conversion." );
                }
            }

            attribute.FieldTypeId = fieldType.Id;
        }

        helper.UpdateProperty( attribute, a => a.Name, name );
        helper.UpdateProperty( attribute, a => a.Description, description );
        helper.UpdateProperty( attribute, a => a.IsRequired, isRequired );
        helper.UpdateProperty( attribute, a => a.DefaultValue, defaultValue );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( configurationValues != null )
        {
            ApplyAttributeQualifiers( attribute, configurationValues );
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( placement.IsSpecified || isNew )
        {
            // Ordered within its own scope, so activity attributes are numbered
            // independently of the workflow's.
            var siblings = attributeService.Queryable()
                .Where( a => a.EntityTypeId == attribute.EntityTypeId
                    && a.EntityTypeQualifierColumn == attribute.EntityTypeQualifierColumn
                    && a.EntityTypeQualifierValue == attribute.EntityTypeQualifierValue )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Id )
                .ToList();

            PlaceAmongSiblings( siblings, siblings.First( a => a.Id == attribute.Id ), placement, a => a.Id, ( a, order ) => a.Order = order );

            helper.SaveChangesIfNoErrors();

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }
        }

        // Read back through the cache, which the save has already refreshed, so the
        // result reflects the qualifiers as Rock resolved them rather than as they
        // were supplied.
        var savedAttribute = AttributeCache.Get( attribute.Id, rockContext );

        var scopeDescription = scope == WorkflowAttributeScope.Activity
            ? $"activity attribute on '{activityType.Name}'"
            : "workflow attribute";

        return Success( GetWorkflowAttributeResult( savedAttribute, scope ) )
            .WithInstructions( isNew ? $"The {scopeDescription} has been created." : $"The {scopeDescription} has been updated." )
            .WithHistoryContent( new KeyNameResult( attribute.Id, attribute.Guid, attribute.Name ) );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Applies field type qualifiers to an attribute, adding, updating, and removing
    /// so the stored set matches what was supplied.
    /// </summary>
    /// <remarks>
    /// Qualifiers are rows rather than a column, so they cannot simply be assigned.
    /// The supplied dictionary is treated as the complete set for the keys it names,
    /// which is what lets a caller correct a wrong qualifier without knowing how it
    /// was stored.
    /// </remarks>
    /// <param name="attribute">The attribute to configure.</param>
    /// <param name="configurationValues">The qualifier keys and values.</param>
    private static void ApplyAttributeQualifiers( Rock.Model.Attribute attribute, Dictionary<string, string> configurationValues )
    {
        foreach ( var configurationValue in configurationValues )
        {
            var qualifier = attribute.AttributeQualifiers
                .FirstOrDefault( q => q.Key.Equals( configurationValue.Key, StringComparison.OrdinalIgnoreCase ) );

            if ( qualifier == null )
            {
                attribute.AttributeQualifiers.Add( new AttributeQualifier
                {
                    Key = configurationValue.Key,
                    Value = configurationValue.Value
                } );

                continue;
            }

            qualifier.Value = configurationValue.Value;
        }
    }

    #endregion
}
