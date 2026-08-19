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
using Rock.AI.Agent.Classes.Entity;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Adds an action to a workflow activity or updates an existing one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An action is one step: sending an email, setting an attribute, showing a
    /// form. Its configuration is supplied as settings, which are the attributes the
    /// chosen component declares, so every key is checked against that component
    /// before anything is written. An unrecognized key would otherwise become an
    /// orphaned value that the action never reads and nothing reports.
    /// </para>
    /// <para>
    /// Changing which component an existing action runs is refused once instances
    /// exist. Settings are stored against the old component, so pointing the action
    /// at a new one leaves it looking for attributes it does not have. It finds
    /// none, runs entirely on defaults, and throws nothing. A send email action
    /// turned into a set attribute action simply runs empty. With no instances there
    /// is nothing to strand, so the change is allowed and the stranded values are
    /// cleaned up.
    /// </para>
    /// </remarks>
    [Description( "Adds an action to a workflow activity, or updates an existing one. An action is one step, such as sending an email or setting a value." )]
    [AgentUsage( "name and actionEntityTypeIdKey are required when adding. Settings are the attributes the chosen action declares, so read them first. Supply at most one of insertAfterActionTypeIdKey or insertBeforeActionTypeIdKey." )]
    [AgentToolPrerequisite( "Call LookupWorkflowActionComponents to determine the actionEntityTypeIdKey, then GetWorkflowActionTypeAvailableAttributes to determine the settings it accepts." )]
    [AgentGuardrail( "The action component cannot be changed once the workflow has saved instances, because the stored settings belong to the original component." )]
    [AgentToolGuid( "8C1147F0-7A46-4274-9A6D-668ECD052B87" )]
    public AgentToolResult AddOrUpdateWorkflowActionType(
        [Description( "Required when editing an existing action." )]
        string actionTypeIdKey = null,
        [Description( "Required when adding a new action." )]
        string activityTypeIdKey = null,
        string name = null,
        [Description( "The key of the action component this step runs." )]
        string actionEntityTypeIdKey = null,
        bool? isActionCompletedOnSuccess = null,
        [Description( "Whether the whole activity finishes when this action succeeds." )]
        bool? isActivityCompletedOnSuccess = null,
        [Description( "Whether the action counts as complete when its criteria are not met, rather than blocking the activity." )]
        bool? isActionCompletedIfCriteriaUnmet = null,
        [Description( "The key of the workflow attribute to test before running this action. Omit for an action that always runs." )]
        string criteriaAttributeIdKey = null,
        ComparisonType? criteriaComparisonType = null,
        // One of the four slots stored unchanged, so it holds a guid rather than an
        // idKey. Named in the skill description; keep the two in step.
        [Description( "The value the criteria attribute is compared against. Written unchanged, so when that attribute's field type references another record this must be the record's guid, not its idKey." )]
        SetOrClear<string> criteriaValue = null,
        // One of the four slots stored unchanged, so a value holds a guid rather than
        // an idKey. Named in the skill description; keep the two in step.
        [Description( "The action's configuration, as setting keys and values. Each value is written into Rock's stored configuration unchanged. When a setting's field type references another record, see fieldType from GetWorkflowActionTypeAvailableAttributes, the value must be that record's guid: take it from the guid GetWorkflowType returns on every node, or from the target entity's own Get tool, never from its idKey. Put the value in value; textValue is output only and is ignored on write." )]
        List<AttributeValueResult> settings = null,
        [Description( "The key of the action this one should follow." )]
        string insertAfterActionTypeIdKey = null,
        [Description( "The key of the action this one should precede." )]
        string insertBeforeActionTypeIdKey = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var actionTypeService = new WorkflowActionTypeService( rockContext );

        var placement = ResolveSiblingPlacement( insertAfterActionTypeIdKey, insertBeforeActionTypeIdKey, helper );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        WorkflowActionType actionType;
        WorkflowActivityType activityType;
        var isNew = actionTypeIdKey.IsNullOrWhiteSpace();

        if ( !isNew )
        {
            actionType = helper.GetRequiredEntity<WorkflowActionType>( actionTypeIdKey );

            if ( actionType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the available actions." );
            }

            // The parent comes from the action on an update, so a caller holding only
            // the action key does not have to supply it. When it is supplied anyway
            // it is checked rather than trusted.
            activityType = actionType.ActivityType;

            if ( activityTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                var suppliedActivityType = helper.GetRequiredEntity<WorkflowActivityType>( activityTypeIdKey );

                if ( suppliedActivityType == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the available activities." );
                }

                if ( actionType.ActivityTypeId != suppliedActivityType.Id )
                {
                    return Error( $"The action '{actionType.Name}' does not belong to the activity '{suppliedActivityType.Name}'." )
                        .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine which actions belong to this activity." );
                }
            }
        }
        else
        {
            if ( activityTypeIdKey.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( activityTypeIdKey )} is required when adding an action." )
                    .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the available activities, or supply {nameof( actionTypeIdKey )} to update an existing action instead." );
            }

            activityType = helper.GetRequiredEntity<WorkflowActivityType>( activityTypeIdKey );

            if ( activityType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the available activities." );
            }

            if ( name.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( name )} is required when adding an action." );
            }

            if ( actionEntityTypeIdKey.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( actionEntityTypeIdKey )} is required when adding an action." )
                    .WithInstructions( $"Call the {nameof( LookupWorkflowActionComponents )} function to determine the available actions." );
            }

            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            actionType = rockContext.Set<WorkflowActionType>().Create();

            actionType.ActivityTypeId = activityType.Id;

            // Matches what the Rock UI does and what nearly every action needs.
            actionType.IsActionCompletedOnSuccess = true;

            actionTypeService.Add( actionType );
        }

        var previousActionEntityTypeId = isNew ? ( int? ) null : actionType.EntityTypeId;

        if ( actionEntityTypeIdKey.IsNotNullOrWhiteSpace() )
        {
            var actionEntityType = helper.GetRequiredEntity<Rock.Model.EntityType>( actionEntityTypeIdKey );

            if ( actionEntityType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( LookupWorkflowActionComponents )} function to determine the available actions." );
            }

            var isComponentChanging = !isNew && actionType.EntityTypeId != actionEntityType.Id;

            if ( isComponentChanging )
            {
                // Checked at the moment of the write, never inferred from anything
                // the caller says.
                var instanceCount = GetWorkflowInstanceCount( activityType.WorkflowTypeId, rockContext );

                if ( instanceCount > 0 )
                {
                    return Error( $"The action component of '{actionType.Name}' cannot be changed because this workflow type has {instanceCount} saved instance(s) configured against the current component. The action would find none of its settings and run on defaults." )
                        .WithInstructions( $"Delete this action with {nameof( DeleteWorkflowActionType )} and add the one you want instead, or remove the existing instances first." );
                }
            }

            actionType.EntityTypeId = actionEntityType.Id;
        }

        // Settings are validated against the component before anything is written,
        // so a bad key is reported rather than stored where nothing will read it.
        var settingAttributes = GetActionSettingAttributes( actionType.EntityTypeId, rockContext );

        if ( settings != null && settings.Any() )
        {
            ValidateSettingKeys( settings, settingAttributes, helper );
            NormalizeSettingValues( settings, settingAttributes, helper );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( GetWorkflowActionTypeAvailableAttributes )} function to determine the settings this action accepts." );
            }
        }

        if ( criteriaAttributeIdKey.IsNotNullOrWhiteSpace() )
        {
            var criteriaAttribute = helper.GetRequiredEntity<Rock.Model.Attribute>( criteriaAttributeIdKey );

            if ( criteriaAttribute == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the workflow's attributes." );
            }

            // Scoped deliberately. Without this any attribute in Rock would be
            // accepted, including one belonging to an unrelated entity, and the
            // criteria would silently never match.
            var isReferenceable = GetReferenceableAttributes( activityType, rockContext )
                .Any( a => a.Id == criteriaAttribute.Id );

            if ( !isReferenceable )
            {
                return Error( $"The attribute '{criteriaAttribute.Name}' is not an attribute of this workflow type or of the activity this action belongs to." )
                    .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine which attributes this action can test." );
            }

            // Stored as a unique identifier rather than an id, because an action's
            // criteria travel with the workflow when it is exported.
            actionType.CriteriaAttributeGuid = criteriaAttribute.Guid;
        }

        helper.UpdateProperty( actionType, at => at.Name, name );
        helper.UpdateProperty( actionType, at => at.IsActionCompletedOnSuccess, isActionCompletedOnSuccess );
        helper.UpdateProperty( actionType, at => at.IsActivityCompletedOnSuccess, isActivityCompletedOnSuccess );
        helper.UpdateProperty( actionType, at => at.IsActionCompletedIfCriteriaUnmet, isActionCompletedIfCriteriaUnmet );
        helper.UpdateProperty( actionType, at => at.CriteriaComparisonType, criteriaComparisonType );
        helper.UpdateProperty( actionType, at => at.CriteriaValue, criteriaValue );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Saved before the settings are applied. Settings are attribute values, and
        // an attribute value needs the action's id to attach to.
        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // The zero-instance exception allowed the component to change, so the values
        // belonging to the old one are cleared. Leaving them would put rows in the
        // database that no tool reports and no action reads.
        if ( previousActionEntityTypeId.HasValue && previousActionEntityTypeId.Value != actionType.EntityTypeId )
        {
            ClearOrphanedSettingValues( actionType.Id, previousActionEntityTypeId.Value, rockContext );
        }

        if ( settings != null && settings.Any() )
        {
            actionType.LoadAttributes( rockContext );

            SeedVestigialSettings( actionType );

            helper.SetAttributeValues( actionType, settings );
            helper.SaveChangesIfNoErrors();

            if ( helper.HasErrors )
            {
                return helper.ErrorResult
                    .WithInstructions( $"The action itself was saved but its settings were not. Call {nameof( GetWorkflowActionTypeAvailableAttributes )} to check the setting keys, then call this function again to apply them." );
            }
        }

        if ( placement.IsSpecified || isNew )
        {
            var siblings = actionTypeService.Queryable()
                .Where( at => at.ActivityTypeId == actionType.ActivityTypeId )
                .OrderBy( at => at.Order )
                .ThenBy( at => at.Id )
                .ToList();

            PlaceAmongSiblings( siblings, siblings.First( at => at.Id == actionType.Id ), placement, at => at.Id, ( at, order ) => at.Order = order );

            helper.SaveChangesIfNoErrors();

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }
        }

        // The form is a separate tool, so it is left off the result rather than
        // rendered as null detail the caller did not ask for.
        var result = GetActionTypeResult( actionType, rockContext, clipLongValues: false );

        result.Form = null;

        var toolResult = Success( result )
            .WithInstructions( isNew ? "The action has been created." : "The action has been updated." )
            .WithHistoryContent( new KeyNameResult( actionType.Id, actionType.Guid, actionType.Name ) );

        // A user entry action does nothing until its form exists, and the form is a
        // separate tool, so say so rather than leaving a dead step behind.
        var entityType = EntityTypeCache.Get( actionType.EntityTypeId, rockContext );

        if ( entityType?.Name == typeof( Rock.Workflow.Action.UserEntryForm ).FullName && !actionType.WorkflowFormId.HasValue )
        {
            toolResult = toolResult
                .WithInstructions( $"This is a user entry action and has no form yet, so it will show the person nothing. Call {nameof( AddOrUpdateWorkflowActionForm )} to build it." );
        }

        return toolResult;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Fills in the attributes every action carries but no caller can set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Order is declared on the base Component with an IntegerField attribute that
    /// names neither a required flag nor a default. Rock's FieldAttribute defaults
    /// required to true, so Order is a required attribute that is blank on a new
    /// action, and SetAttributeValues refuses to save anything at all until it holds
    /// a value.
    /// </para>
    /// <para>
    /// This tool hides Order from the available settings and rejects it as an input,
    /// on the grounds that it does not control where the action runs and writing it
    /// achieves nothing. Those two decisions are only coherent if the value is
    /// supplied here. Without this, an action's real settings cannot be saved, and
    /// the caller is told a key is required that the same tool refuses to accept.
    /// </para>
    /// <para>
    /// The value is a constant zero, matching what ActionComponent.Order returns for
    /// every action regardless of what is stored. Component.Order does parse this
    /// string, but ActionComponent overrides it to return 0 unconditionally, so on an
    /// action the stored value is never read by anything.
    /// </para>
    /// <para>
    /// Seeding the action's real position here instead would be worse. It reads as
    /// though the setting tracks position, which is the exact confusion this tool
    /// exists to prevent, and it would freeze at whatever position the action held
    /// when it was created and then look stale after any reorder.
    /// </para>
    /// <para>
    /// Active needs no help; its BooleanField declares a default, so it is never blank.
    /// </para>
    /// </remarks>
    /// <param name="actionType">The action whose attributes have been loaded.</param>
    private static void SeedVestigialSettings( WorkflowActionType actionType )
    {
        if ( actionType.Attributes == null || !actionType.Attributes.ContainsKey( "Order" ) )
        {
            return;
        }

        if ( actionType.GetAttributeValue( "Order" ).IsNullOrWhiteSpace() )
        {
            actionType.SetAttributeValue( "Order", "0" );
        }
    }

    /// <summary>
    /// Checks every supplied setting key against what the action component declares.
    /// </summary>
    /// <remarks>
    /// Active and Order are called out separately because they are the two a caller
    /// is most likely to reach for and the two that do nothing. Every action carries
    /// them because ActionComponent overrides both, so they pass a naive existence
    /// check while having no effect on the action or its position.
    /// </remarks>
    /// <param name="settings">The settings the caller supplied.</param>
    /// <param name="settingAttributes">The settings the component declares.</param>
    /// <param name="helper">The helper to record errors on.</param>
    private static void ValidateSettingKeys( List<AttributeValueResult> settings, List<AttributeCache> settingAttributes, AgentToolHelper helper )
    {
        var validKeys = new HashSet<string>( settingAttributes.Select( a => a.Key ), StringComparer.OrdinalIgnoreCase );

        foreach ( var setting in settings )
        {
            if ( setting.Key.IsNullOrWhiteSpace() )
            {
                helper.AddError( "Every setting needs a key." );

                continue;
            }

            if ( setting.Key.Equals( "Order", StringComparison.OrdinalIgnoreCase ) )
            {
                helper.AddError( "'Order' is not a usable setting. It does not control where the action runs; use insertAfterActionTypeIdKey or insertBeforeActionTypeIdKey for that." );

                continue;
            }

            if ( setting.Key.Equals( "Active", StringComparison.OrdinalIgnoreCase ) )
            {
                helper.AddError( "'Active' is not a usable setting. It is a vestigial attribute every action carries and writing it has no effect." );

                continue;
            }

            if ( !validKeys.Contains( setting.Key ) )
            {
                helper.AddError( $"'{setting.Key}' is not a setting this action accepts." );
            }
        }
    }

    /// <summary>
    /// Rewrites a setting supplied as a label into the value the setting actually
    /// stores, and refuses one that matches neither.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 8/18/26 - CLAUDE
    ///
    /// A select-backed setting stores the value side of its list, which for an
    /// enum-backed setting is a number. Writing the label instead saved cleanly and
    /// even ran correctly, because the actions parse these with ConvertToEnum, which
    /// accepts the name. What it broke was Rock's own editor: its dropdown holds the
    /// numbers, so a stored label matched no item and the setting rendered empty and
    /// reported itself as missing on a required field.
    ///
    /// Reason: A value Rock's editor cannot show is a value that gets silently lost
    /// the first time a person opens that screen.
    /// </para>
    /// <para>
    /// The label is translated rather than refused. It is the natural thing to write,
    /// Rock accepts it at run time, and the only thing wrong with it is where it
    /// lands. A value matching neither side is refused with the list, because at that
    /// point there is nothing to infer.
    /// </para>
    /// <para>
    /// Values are split on commas so a multi-select setting is checked entry by
    /// entry. An option cannot itself contain a comma, because the list these are
    /// read from is comma separated.
    /// </para>
    /// <para>
    /// Anything holding Lava is left alone. Its value is not known until the workflow
    /// runs, so there is nothing here to compare against.
    /// </para>
    /// </remarks>
    /// <param name="settings">The settings the caller supplied.</param>
    /// <param name="settingAttributes">The settings the component declares.</param>
    /// <param name="helper">The helper to record errors on.</param>
    private static void NormalizeSettingValues( List<AttributeValueResult> settings, List<AttributeCache> settingAttributes, AgentToolHelper helper )
    {
        const int MaximumReportedSettingValues = 25;

        var attributesByKey = settingAttributes
            .GroupBy( a => a.Key, StringComparer.OrdinalIgnoreCase )
            .ToDictionary( g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase );

        foreach ( var setting in settings )
        {
            if ( setting.Key.IsNullOrWhiteSpace() || setting.Value.IsNullOrWhiteSpace() )
            {
                continue;
            }

            if ( setting.Value.Contains( "{{" ) || setting.Value.Contains( "{%" ) )
            {
                continue;
            }

            if ( !attributesByKey.TryGetValue( setting.Key, out var attribute ) )
            {
                continue;
            }

            var selectableValues = GetSelectableValues( attribute );

            if ( selectableValues == null )
            {
                continue;
            }

            var normalizedParts = new List<string>();
            var unmatchedParts = new List<string>();

            foreach ( var suppliedPart in setting.Value.Split( ',' ).Select( p => p.Trim() ).Where( p => p.IsNotNullOrWhiteSpace() ) )
            {
                var match = selectableValues.FirstOrDefault( v => v.Value.Equals( suppliedPart, StringComparison.OrdinalIgnoreCase ) )
                    ?? selectableValues.FirstOrDefault( v => CollapseSpaces( v.Text ).Equals( CollapseSpaces( suppliedPart ), StringComparison.OrdinalIgnoreCase ) );

                if ( match == null )
                {
                    unmatchedParts.Add( suppliedPart );
                }
                else
                {
                    normalizedParts.Add( match.Value );
                }
            }

            if ( unmatchedParts.Any() )
            {
                // Capped because a setting whose list comes from SQL can return
                // thousands of rows, and an error naming every one of them is not an
                // error anyone can read.
                var options = string.Join( ", ", selectableValues.Take( MaximumReportedSettingValues ).Select( v => $"'{v.Value}' for {v.Text}" ) );

                if ( selectableValues.Count > MaximumReportedSettingValues )
                {
                    options += $", and {selectableValues.Count - MaximumReportedSettingValues} more";
                }

                helper.AddError( $"'{string.Join( "', '", unmatchedParts )}' is not something the '{attribute.Name}' setting accepts. It stores one of: {options}." );

                continue;
            }

            setting.Value = string.Join( ",", normalizedParts );
        }
    }

    /// <summary>
    /// Removes every space from a value so a label is matched the way a person would
    /// read it rather than the way it happens to be spaced.
    /// </summary>
    /// <param name="value">The value to collapse.</param>
    /// <returns>The value without spaces.</returns>
    private static string CollapseSpaces( string value )
    {
        return ( value ?? string.Empty ).Replace( " ", string.Empty );
    }

    /// <summary>
    /// Removes the stored settings that belonged to an action's previous component.
    /// </summary>
    /// <remarks>
    /// Only reachable through the zero-instance exception, which is the one case
    /// where an action is allowed to change component. The values are keyed to the
    /// old component's attributes, so nothing else will ever read or report them.
    /// </remarks>
    /// <param name="actionTypeId">The action whose values are being cleared.</param>
    /// <param name="previousActionEntityTypeId">The component the action used to run.</param>
    /// <param name="rockContext">The context to write through.</param>
    private static void ClearOrphanedSettingValues( int actionTypeId, int previousActionEntityTypeId, RockContext rockContext )
    {
        var previousAttributeIds = GetActionSettingAttributes( previousActionEntityTypeId, rockContext )
            .Select( a => a.Id )
            .ToList();

        if ( !previousAttributeIds.Any() )
        {
            return;
        }

        var attributeValueService = new AttributeValueService( rockContext );

        var orphanedValues = attributeValueService.Queryable()
            .Where( av => av.EntityId == actionTypeId && previousAttributeIds.Contains( av.AttributeId ) )
            .ToList();

        if ( !orphanedValues.Any() )
        {
            return;
        }

        attributeValueService.DeleteRange( orphanedValues );
        rockContext.SaveChanges();
    }

    #endregion
}
