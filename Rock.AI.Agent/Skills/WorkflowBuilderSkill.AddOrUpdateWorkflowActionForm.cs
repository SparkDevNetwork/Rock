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
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Adds the form shown by a user entry action, or updates an existing one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Fields are supplied as one flat list. Sections and per-field column widths
    /// are deliberately not offered; see <see cref="ReplaceFormFields"/> for why.
    /// Styling is done with each field's pre and post HTML, which both renderers
    /// honour on a form that has no sections.
    /// </para>
    /// <para>
    /// Supplying fields or buttons replaces what is there rather than merging.
    /// This is the one place in either skill where absence means deletion, and it is
    /// safe only because the tool sees the complete unit and the unit is small.
    /// Merging would need a stable identity per field, and the natural one, the
    /// attribute, can legitimately appear more than once on a form.
    /// </para>
    /// </remarks>
    [Description( "Adds the form shown by a user entry action, or updates an existing one, including its fields and buttons." )]
    [AgentUsage( "Supplying fields replaces every existing field. Supplying buttons replaces every existing button. Omit either to leave it untouched, which is how you change only the header. Use a field's preHtml and postHtml for layout and styling; there are no sections and no column widths." )]
    [AgentToolPrerequisite( "Call GetWorkflowType to determine the actionTypeIdKey and the workflow attributes the form's fields can edit." )]
    [AgentGuardrail( "Supplying fields deletes every field currently on the form. Read the form first and send back the complete set, not just the parts being changed. Refuses on a workflow type whose isFormBuilder is true; only the form is off limits there, the rest of that workflow can still be edited." )]
    [AgentToolGuid( "AEF3A669-4696-42E0-AC97-FF109CC72FE2" )]
    public AgentToolResult AddOrUpdateWorkflowActionForm(
        [Description( "The user entry action whose form this is. Always required." )]
        string actionTypeIdKey = null,
        [Description( "Markup rendered above the form's fields." )]
        SetOrClear<string> header = null,
        [Description( "Markup rendered below the form's fields." )]
        SetOrClear<string> footer = null,
        bool? allowNotes = null,
        [Description( "The key of the system communication used to notify the assigned person that the form is waiting." )]
        SetOrClear<string> notificationSystemCommunicationIdKey = null,
        [Description( "Whether the notification includes the form's buttons so the person can respond from the message." )]
        bool? includeActionsInNotification = null,
        [Description( "The buttons at the bottom of the form, in order. These are how a workflow branches. Replaces every existing button." )]
        List<WorkflowFormButtonInput> buttons = null,
        [Description( "The fields on the form, in order. Replaces every existing field." )]
        List<WorkflowFormFieldInput> fields = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        // Optional in the signature but checked here, so a caller that omits it gets
        // this message rather than a binding failure it cannot interpret.
        if ( actionTypeIdKey.IsNullOrWhiteSpace() )
        {
            return Error( $"{nameof( actionTypeIdKey )} is required." )
                .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the user entry action this form belongs to." );
        }

        var actionType = helper.GetRequiredEntity<WorkflowActionType>( actionTypeIdKey );

        if ( actionType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( GetWorkflowType )} function to determine the available actions." );
        }

        var workflowTypeId = actionType.ActivityType?.WorkflowTypeId;

        if ( !workflowTypeId.HasValue )
        {
            return Error( "That action is not attached to a workflow type." );
        }

        // Refused before anything is written. Form Builder reads a form only through
        // its sections, pulling the fields that point at each one
        // (FormBuilderDetail.cs:584). This tool writes fields with no section, so
        // rewriting one of its forms would leave the fields in the database and
        // rendering correctly, while Form Builder showed an empty form with no way
        // to get them back.
        var workflowType = WorkflowTypeCache.Get( workflowTypeId.Value, rockContext );

        if ( workflowType?.IsFormBuilder == true )
        {
            return Error( "This form was created in Form Builder and is not editable through the AI tools." )
                .WithInstructions( "Nothing was changed. Edit the form in Form Builder. The rest of this workflow can still be changed here, including its attributes, activities, and actions." );
        }

        var form = actionType.WorkflowForm;
        var isNewForm = form == null;

        if ( isNewForm )
        {
            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            form = rockContext.Set<WorkflowActionForm>().Create();

            // Rock's default single button, so a new form can be submitted
            // before any buttons are configured.
            form.Actions = "Submit^^^Your information has been submitted successfully.";

            new WorkflowActionFormService( rockContext ).Add( form );
            actionType.WorkflowForm = form;
        }

        helper.UpdateProperty( form, f => f.Header, header );
        helper.UpdateProperty( form, f => f.Footer, footer );
        helper.UpdateProperty( form, f => f.AllowNotes, allowNotes );
        helper.UpdateProperty( form, f => f.IncludeActionsInNotification, includeActionsInNotification );

        if ( notificationSystemCommunicationIdKey != null )
        {
            if ( notificationSystemCommunicationIdKey.ClearValue )
            {
                form.NotificationSystemCommunicationId = null;
            }
            else if ( notificationSystemCommunicationIdKey.Value.IsNotNullOrWhiteSpace() )
            {
                var systemCommunication = helper.GetRequiredEntity<Rock.Model.SystemCommunication>( notificationSystemCommunicationIdKey.Value );

                if ( systemCommunication == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( CoreAdministrationSkill.ListSystemCommunications )} function to determine the available templates." );
                }

                form.NotificationSystemCommunicationId = systemCommunication.Id;
            }
        }

        if ( buttons != null )
        {
            var actionsValue = BuildFormActions( buttons, workflowTypeId.Value, helper, rockContext );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            form.Actions = actionsValue;
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Saved before the fields are rebuilt, so a brand new form has the id its
        // fields need to point at.
        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( fields != null )
        {
            ReplaceFormFields( form, fields, actionType.ActivityType, helper, rockContext );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult
                    .WithInstructions( "Nothing was changed. Correct the fields and call this function again with the complete set of fields." );
            }
        }

        // Re-read so the result reflects what is stored rather than what was
        // tracked, since the fields were rebuilt through services.
        var savedActionType = new WorkflowActionTypeService( rockContext )
            .Queryable( "WorkflowForm.FormAttributes,WorkflowForm.FormSections,ActivityType" )
            .FirstOrDefault( at => at.Id == actionType.Id );

        var result = GetActionFormResult( savedActionType ?? actionType, rockContext, clipLongValues: false );

        var instructions = isNewForm ? "The form has been created." : "The form has been updated.";
        var storageWarning = GetUnstoredFormDataWarning( workflowTypeId.Value, rockContext );

        if ( storageWarning.IsNotNullOrWhiteSpace() )
        {
            instructions += " " + storageWarning;
        }

        return Success( result )
            .WithInstructions( instructions )
            .WithHistoryContent( new KeyNameResult( actionType.Id, actionType.Guid, actionType.Name ) );
    }

    #endregion

    #region Constants

    /// <summary>
    /// The entity type of <c>Rock.Workflow.Action.PersistWorkflow</c>, used to detect
    /// whether a workflow already saves itself somewhere in its own structure.
    /// </summary>
    private static readonly Guid PersistWorkflowEntityTypeGuid = new Guid( "F1A39347-6FE0-43D4-89FB-544195088ECF" );

    #endregion

    #region Helper Methods

    /// <summary>
    /// Warns when a form collects data that nothing will store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A workflow with a single form is never persisted by Rock. The submit creates a
    /// fresh workflow, replays it, applies the submitted values, and finishes, and
    /// nothing is written. Whatever an action does with the values during that pass
    /// still happens, but the values themselves are gone. Rock only persists on its
    /// own when it reaches a second interactive action inside one request, which a
    /// single-form workflow never does.
    /// </para>
    /// <para>
    /// This is the failure mode this warning exists for, and it is the reason the
    /// warning is returned rather than documented. The workflow appears to work: the
    /// form renders, submits, and completes. Only later does anyone notice there is no
    /// record of what was entered, and by then the form has been in use for weeks.
    /// </para>
    /// <para>
    /// Warn rather than refuse. A form whose actions write the values somewhere else,
    /// onto a person, a group member, or an interaction, is a legitimate design and
    /// needs no persistence. Only the caller knows which case this is.
    /// </para>
    /// </remarks>
    /// <param name="workflowTypeId">The workflow type the form belongs to.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The warning, or <c>null</c> when the workflow already persists.</returns>
    private static string GetUnstoredFormDataWarning( int workflowTypeId, RockContext rockContext )
    {
        var workflowType = new WorkflowTypeService( rockContext ).Get( workflowTypeId );

        if ( workflowType?.IsPersisted == true )
        {
            return null;
        }

        // A Workflow Persist action anywhere in the type is enough. Where it sits
        // decides which submissions get saved, which is the caller's call rather than
        // this check's.
        var persistEntityTypeId = EntityTypeCache.GetId( PersistWorkflowEntityTypeGuid );

        var hasPersistAction = persistEntityTypeId.HasValue
            && new WorkflowActionTypeService( rockContext ).Queryable()
                .Any( at => at.ActivityType.WorkflowTypeId == workflowTypeId
                    && at.EntityTypeId == persistEntityTypeId.Value );

        if ( hasPersistAction )
        {
            return null;
        }

        return "This workflow does not persist, so anything entered on this form is discarded once processing finishes. "
            + "If an action writes the values somewhere else, onto a person, a group member, or an interaction, that is fine and nothing more is needed. "
            + "If not, the submission leaves no record. Add a Workflow Persist action (Rock.Workflow.Action.PersistWorkflow) to the activity that runs after this form is submitted, "
            + "which saves the entered values without creating a row for people who open the form and never finish. "
            + "Prefer that over the workflow type's isPersisted setting, which saves every instance the moment someone lands on the form.";
    }

    /// <summary>
    /// Builds the stored representation of a form's buttons.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Buttons live in one delimited column rather than in rows, joined with a pipe
    /// and split internally by a caret. Each button's target activity is stored as a
    /// unique identifier so the form survives an export.
    /// </para>
    /// <para>
    /// Written raw rather than through <c>RockSerializableList.ToUriEncodedString</c>.
    /// That helper escapes every entry, which turns the caret separators into
    /// <c>%5E</c>, and the code that renders a form splits on a raw caret without
    /// unescaping first, in <c>WorkflowActionFormCache.GetFormActionButtons</c>. An
    /// escaped button therefore yields one field instead of four, fails that method's
    /// length check, and is dropped silently, so the form renders with no buttons at
    /// all.
    /// </para>
    /// <para>
    /// Rock has two readers for this column and only one of them unescapes, so a round
    /// trip through <c>WorkflowActionFormUserAction.FromUriEncodedString</c> looks
    /// correct while the form itself stays broken. Match what Rock's own workflow type
    /// editor writes, which is a plain pipe delimited string.
    /// </para>
    /// </remarks>
    /// <param name="buttons">The buttons the caller supplied.</param>
    /// <param name="workflowTypeId">The workflow type whose activities a button may activate.</param>
    /// <param name="helper">The helper to record errors on.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The serialized buttons.</returns>
    private static string BuildFormActions( List<WorkflowFormButtonInput> buttons, int workflowTypeId, AgentToolHelper helper, RockContext rockContext )
    {
        var activityTypes = new WorkflowActivityTypeService( rockContext ).Queryable()
            .Where( at => at.WorkflowTypeId == workflowTypeId )
            .Select( at => new { at.Id, at.Guid, at.Name } )
            .ToList();

        var buttonValues = new List<string>();

        foreach ( var button in buttons )
        {
            if ( button.Name.IsNullOrWhiteSpace() )
            {
                helper.AddError( "Every form button needs a name." );

                continue;
            }

            var buttonStyleGuid = Rock.SystemGuid.DefinedValue.BUTTON_HTML_PRIMARY;

            if ( button.ButtonStyleDefinedValueIdKey.IsNotNullOrWhiteSpace() )
            {
                var buttonStyleId = IdHasher.Instance.GetId( button.ButtonStyleDefinedValueIdKey );
                var buttonStyle = buttonStyleId.HasValue ? DefinedValueCache.Get( buttonStyleId.Value, rockContext ) : null;

                if ( buttonStyle == null )
                {
                    helper.AddError( $"The button style for '{button.Name}' was not found." );

                    continue;
                }

                buttonStyleGuid = buttonStyle.Guid.ToString();
            }

            var activateActivityGuid = string.Empty;

            if ( button.ActivateActivityTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                var activityTypeId = IdHasher.Instance.GetId( button.ActivateActivityTypeIdKey );
                var activityType = activityTypeId.HasValue ? activityTypes.FirstOrDefault( at => at.Id == activityTypeId.Value ) : null;

                if ( activityType == null )
                {
                    helper.AddError( $"The activity that '{button.Name}' activates is not part of this workflow type." );

                    continue;
                }

                activateActivityGuid = activityType.Guid.ToString();
            }

            buttonValues.Add( $"{button.Name}^{buttonStyleGuid}^{activateActivityGuid}^{button.ResponseText}" );
        }

        return buttonValues.AsDelimited( "|" );
    }

    /// <summary>
    /// Replaces a form's fields with the supplied set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Everything is validated before anything is removed, so a bad field key leaves
    /// the existing form untouched rather than deleting it and then failing to
    /// rebuild it.
    /// </para>
    /// <para>
    /// Fields are written with no section, and any sections the form already had are
    /// removed. Sections and column widths exist only in Form Builder: the workflow
    /// editor's field row offers pre and post HTML and visibility rules and has no
    /// notion of either. Authoring them here would produce a form that the editor
    /// these workflows are meant to be maintained in cannot show or change.
    /// </para>
    /// <para>
    /// Removing them is safe only because a Form Builder workflow is refused before
    /// this point. The sections that reach here belong to a workflow editor form that
    /// acquired them some other way, where nothing can display them anyway.
    /// </para>
    /// <para>
    /// Leaving the section behind is not an option either. A column size only takes
    /// effect inside a section, and the Obsidian renderer drops pre and post HTML
    /// for any field that has one, so a sectioned field cannot be styled by the one
    /// mechanism this tool does offer.
    /// </para>
    /// </remarks>
    /// <param name="form">The form to rebuild.</param>
    /// <param name="fields">The fields the caller supplied.</param>
    /// <param name="activityType">The activity the form's action belongs to, which determines the attributes its fields may edit.</param>
    /// <param name="helper">The helper to record errors on.</param>
    /// <param name="rockContext">The context to write through.</param>
    private static void ReplaceFormFields( WorkflowActionForm form, List<WorkflowFormFieldInput> fields, WorkflowActivityType activityType, AgentToolHelper helper, RockContext rockContext )
    {
        // Both scopes, because Rock's own form editor offers the workflow's
        // attributes and the containing activity's. Validating against only the
        // workflow's would reject forms the UI produces.
        var referenceableAttributes = GetReferenceableAttributes( activityType, rockContext )
            .GroupBy( a => a.Id )
            .ToDictionary( g => g.Key, g => g.First() );

        // Validate first, rules included. A form that is deleted and then not rebuilt
        // is worse than a form that was never changed.
        var fieldAttributeIds = new HashSet<int>();
        var ruleAttributeIds = new HashSet<int>();

        foreach ( var fieldInput in fields )
        {
            var attributeId = IdHasher.Instance.GetId( fieldInput.AttributeIdKey );

            if ( !attributeId.HasValue || !referenceableAttributes.ContainsKey( attributeId.Value ) )
            {
                helper.AddError( $"The field '{fieldInput.AttributeIdKey}' is not an attribute of this workflow type or of the activity this form belongs to." );

                continue;
            }

            fieldAttributeIds.Add( attributeId.Value );

            CollectRuleAttributeIds( fieldInput.VisibilityRules, ruleAttributeIds );
            BuildVisibilityRules( fieldInput.VisibilityRules, fieldInput.VisibilityRuleMatch, referenceableAttributes, $"the field '{referenceableAttributes[attributeId.Value].Name}'", helper );
        }

        if ( helper.HasErrors )
        {
            return;
        }

        var formAttributeService = new WorkflowActionFormAttributeService( rockContext );
        var formSectionService = new WorkflowActionFormSectionService( rockContext );

        formAttributeService.DeleteRange( form.FormAttributes.ToList() );
        formSectionService.DeleteRange( form.FormSections.ToList() );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return;
        }

        var fieldOrder = 0;

        foreach ( var fieldInput in OrderInputs( fields, f => f.Order ) )
        {
            var formAttribute = new WorkflowActionFormAttribute
            {
                WorkflowActionFormId = form.Id,

                // Left null on purpose. A field with no section is the only shape
                // both renderers style with pre and post HTML.
                ActionFormSectionId = null,
                AttributeId = IdHasher.Instance.GetId( fieldInput.AttributeIdKey ).Value,
                Order = fieldOrder++,
                IsVisible = fieldInput.IsVisible ?? true,
                IsRequired = fieldInput.IsRequired ?? false,
                IsReadOnly = fieldInput.IsReadOnly ?? false,
                HideLabel = fieldInput.HideLabel ?? false,
                PreHtml = fieldInput.PreHtml,
                PostHtml = fieldInput.PostHtml
            };

            // The typed accessor serializes to the JSON column, and nulls it when the
            // rule list is empty, so an unconditional field stores nothing.
            formAttribute.FieldVisibilityRules = BuildVisibilityRules(
                fieldInput.VisibilityRules,
                fieldInput.VisibilityRuleMatch,
                referenceableAttributes,
                "this field",
                helper );

            formAttributeService.Add( formAttribute );
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return;
        }

        // Reported, not refused. A rule may legitimately test an attribute that no
        // field on this form edits, because an earlier action can have set the value.
        // Far more often it means the caller pointed at the wrong attribute, and
        // nothing else would ever say so.
        var offFormRuleAttributeIds = ruleAttributeIds.Except( fieldAttributeIds ).ToList();

        if ( offFormRuleAttributeIds.Any() )
        {
            var names = offFormRuleAttributeIds
                .Where( id => referenceableAttributes.ContainsKey( id ) )
                .Select( id => referenceableAttributes[id].Name )
                .ToList();

            if ( names.Any() )
            {
                helper.AddInstructions( $"Some visibility rules test attributes that no field on this form collects: {string.Join( ", ", names )}. That works when an earlier action sets the value, but check it was not meant to be a field on the form." );
            }
        }
    }

    /// <summary>
    /// Records which attributes a set of visibility rules refers to.
    /// </summary>
    /// <remarks>
    /// Gathered during validation so the whole form can be checked at the end for
    /// rules that test something the form never asks for.
    /// </remarks>
    /// <param name="ruleInputs">The rules to inspect.</param>
    /// <param name="attributeIds">The set to add to.</param>
    private static void CollectRuleAttributeIds( List<WorkflowFormVisibilityRuleInput> ruleInputs, HashSet<int> attributeIds )
    {
        if ( ruleInputs == null )
        {
            return;
        }

        foreach ( var ruleInput in ruleInputs )
        {
            var attributeId = IdHasher.Instance.GetId( ruleInput.ComparedToAttributeIdKey );

            if ( attributeId.HasValue )
            {
                attributeIds.Add( attributeId.Value );
            }
        }
    }

    /// <summary>
    /// Orders supplied inputs, honouring an explicit order only when every item
    /// carries one.
    /// </summary>
    /// <remarks>
    /// The list position and an explicit order are two ways to say the same thing.
    /// Mixing them is where they disagree, so an explicit order is used only when it
    /// is complete; otherwise the list wins and the order values are ignored.
    /// </remarks>
    /// <typeparam name="TInput">The input type being ordered.</typeparam>
    /// <param name="inputs">The inputs as supplied.</param>
    /// <param name="getOrder">Reads an input's explicit order.</param>
    /// <returns>The inputs in the order they should be written.</returns>
    private static IEnumerable<TInput> OrderInputs<TInput>( List<TInput> inputs, System.Func<TInput, int?> getOrder )
    {
        if ( inputs.Any() && inputs.All( i => getOrder( i ).HasValue ) )
        {
            return inputs.OrderBy( i => getOrder( i ).Value );
        }

        return inputs;
    }

    #endregion
}
