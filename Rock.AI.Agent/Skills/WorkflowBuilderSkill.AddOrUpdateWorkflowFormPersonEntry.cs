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
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Workflow.FormBuilder;

namespace Rock.AI.Agent.Skills;

internal sealed partial class WorkflowBuilderSkill
{
    #region Tool(s)

    /// <summary>
    /// Turns on the person entry block of a user entry form, or updates its settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Person entry is what makes a form collect a real person rather than plain text.
    /// Rock matches the entered details against existing records and creates one when
    /// there is no match, then writes the result into a workflow attribute so the rest
    /// of the workflow can act on an actual person.
    /// </para>
    /// <para>
    /// A separate tool from <see cref="AddOrUpdateWorkflowActionForm"/> rather than
    /// thirty more parameters on it. Person entry is configured as one unit and is off
    /// on most forms, so folding it in would bury the common case of a header, some
    /// buttons, and a few fields.
    /// </para>
    /// <para>
    /// Every parameter merges. Unlike the form tool's fields and buttons, nothing here
    /// is replace-by-absence, so a caller can change one setting without resending the
    /// rest.
    /// </para>
    /// </remarks>
    [Description( "Turns on the person entry block of a user entry form, or updates its settings. Person entry collects a real person, matching or creating the record, rather than collecting plain attribute values." )]
    [AgentPurpose( "Configures a form to collect a person record rather than loose text fields." )]
    [AgentUsage( "personAttributeIdKey is required when turning person entry on, because without it the matched person is collected and then unreachable. Every parameter merges, so send only what is changing." )]
    [AgentToolPrerequisite( "Call GetWorkflowTypeConfiguration to determine the actionTypeIdKey and the workflow attributes the person, spouse, and family can be written to." )]
    [AgentToolGuid( "3E7B1A4C-5D26-4F98-9C03-8B41D5E6720F" )]
    public AgentToolResult AddOrUpdateWorkflowFormPersonEntry(
        [Description( "The user entry action whose form this is. Always required." )]
        string actionTypeIdKey = null,
        [Description( "Whether the form collects a person at all. Everything else here does nothing while this is false." )]
        bool? allowPersonEntry = null,

        [Description( "The workflow attribute the matched or created person is written to. Must be a Person attribute. Required when turning person entry on." )]
        SetOrClear<string> personAttributeIdKey = null,
        [Description( "The workflow attribute the spouse is written to. Only meaningful when spouseOption shows the spouse fields." )]
        SetOrClear<string> spouseAttributeIdKey = null,
        [Description( "The workflow attribute the family group is written to." )]
        SetOrClear<string> familyAttributeIdKey = null,

        WorkflowActionFormPersonEntryOption? addressOption = null,
        WorkflowActionFormPersonEntryOption? birthdateOption = null,
        WorkflowActionFormPersonEntryOption? emailOption = null,
        WorkflowActionFormPersonEntryOption? ethnicityOption = null,
        WorkflowActionFormPersonEntryOption? genderOption = null,
        WorkflowActionFormPersonEntryOption? maritalStatusOption = null,
        WorkflowActionFormPersonEntryOption? mobilePhoneOption = null,
        WorkflowActionFormPersonEntryOption? raceOption = null,
        WorkflowActionFormPersonEntryOption? spouseOption = null,
        [Description( "Whether the SMS opt-in checkbox is Show or Hide. A different set of values from the other options, because opt-in cannot be required." )]
        WorkflowActionFormShowHideOption? smsOptInOption = null,
        [Description( "The label shown above the spouse fields." )]
        SetOrClear<string> spouseLabel = null,

        [Description( "Whether the block is prefilled from the signed-in person." )]
        bool? isAutofillCurrentPersonEnabled = null,
        [Description( "Whether the campus picker is shown." )]
        bool? isCampusVisible = null,
        [Description( "Whether the campus picker offers inactive campuses. Defaults to false when person entry is on and nothing has been set, which is the opposite of Rock's own default." )]
        bool? isInactiveCampusIncluded = null,
        [Description( "Whether the whole block is hidden when the person is already signed in." )]
        bool? isHiddenIfCurrentPersonKnown = null,

        [Description( "The heading shown above the person entry block." )]
        SetOrClear<string> title = null,
        [Description( "Explanatory text shown beneath the heading." )]
        SetOrClear<string> description = null,
        [Description( "Whether a horizontal rule is drawn beneath the heading." )]
        bool? isHeadingSeparatorShown = null,
        [Description( "Markup rendered above the person entry fields." )]
        SetOrClear<string> preHtml = null,
        [Description( "Markup rendered below the person entry fields." )]
        SetOrClear<string> postHtml = null,

        [Description( "The connection status given to a person this block creates. A Person Connection Status defined value." )]
        SetOrClear<string> connectionStatusDefinedValueIdKey = null,
        [Description( "The record status given to a person this block creates. A Person Record Status defined value. Defaults to Active when person entry is on and no value has been set." )]
        SetOrClear<string> recordStatusDefinedValueIdKey = null,
        [Description( "The record source recorded against a person this block creates. A Record Source Type defined value." )]
        SetOrClear<string> recordSourceDefinedValueIdKey = null,
        [Description( "The location type an entered address is saved as. A Group Location Type defined value. Defaults to Home when person entry is on and no value has been set." )]
        SetOrClear<string> addressTypeDefinedValueIdKey = null,
        [Description( "Limits the campus picker to campuses of this status. A Campus Status defined value." )]
        SetOrClear<string> campusStatusDefinedValueIdKey = null,
        [Description( "Limits the campus picker to campuses of this type. A Campus Type defined value." )]
        SetOrClear<string> campusTypeDefinedValueIdKey = null,
        [Description( "The section type controlling the block's visual treatment. A Section Type defined value." )]
        SetOrClear<string> sectionTypeDefinedValueIdKey = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        // Optional in the signature but checked here, so a caller that omits it gets
        // this message rather than a binding failure it cannot interpret.
        if ( actionTypeIdKey.IsNullOrWhiteSpace() )
        {
            return Error( $"{nameof( actionTypeIdKey )} is required." )
                .WithInstructions( $"Call the {nameof( GetWorkflowTypeConfiguration )} function to determine the user entry action this form belongs to." );
        }

        var actionType = helper.GetRequiredEntity<WorkflowActionType>( actionTypeIdKey );

        if ( actionType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( GetWorkflowTypeConfiguration )} function to determine the available actions." );
        }

        var activityType = actionType.ActivityType;
        var workflowTypeId = activityType?.WorkflowTypeId;

        if ( !workflowTypeId.HasValue )
        {
            return Error( "That action is not attached to a workflow type." );
        }

        // Same refusal as the form tool. A Form Builder template with person entry
        // enabled overrides the form's own settings entirely, so writing them here
        // would save cleanly and change nothing at run time.
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

            // Rock's default single button, so a new form can be submitted before any
            // buttons are configured.
            form.Actions = "Submit^^^Your information has been submitted successfully.";

            new WorkflowActionFormService( rockContext ).Add( form );
            actionType.WorkflowForm = form;
        }

        // Captured before the update, so the first-time defaults below can tell the
        // difference between turning person entry on and adjusting a block that was
        // already on. An existing block's settings are never overwritten.
        var isTurningPersonEntryOn = allowPersonEntry == true && !form.AllowPersonEntry;

        helper.UpdateProperty( form, f => f.AllowPersonEntry, allowPersonEntry );

        SetPersonEntryAttribute( form, f => f.PersonEntryPersonAttributeGuid, personAttributeIdKey, activityType, helper, rockContext );
        SetPersonEntryAttribute( form, f => f.PersonEntrySpouseAttributeGuid, spouseAttributeIdKey, activityType, helper, rockContext );
        SetPersonEntryAttribute( form, f => f.PersonEntryFamilyAttributeGuid, familyAttributeIdKey, activityType, helper, rockContext );

        // Each falls back to our own first-time default rather than Rock's when the
        // caller said nothing and person entry is being turned on. See
        // FirstTimeDefault for why these cannot use ApplyPersonEntryDefaults.
        helper.UpdateProperty( form, f => f.PersonEntryAddressEntryOption,
            addressOption ?? FirstTimeDefault( isTurningPersonEntryOn, WorkflowActionFormPersonEntryOption.Hidden ) );
        helper.UpdateProperty( form, f => f.PersonEntryBirthdateEntryOption,
            birthdateOption ?? FirstTimeDefault( isTurningPersonEntryOn, WorkflowActionFormPersonEntryOption.Optional ) );
        helper.UpdateProperty( form, f => f.PersonEntryEmailEntryOption,
            emailOption ?? FirstTimeDefault( isTurningPersonEntryOn, WorkflowActionFormPersonEntryOption.Optional ) );
        helper.UpdateProperty( form, f => f.PersonEntryEthnicityEntryOption, ethnicityOption );
        helper.UpdateProperty( form, f => f.PersonEntryGenderEntryOption,
            genderOption ?? FirstTimeDefault( isTurningPersonEntryOn, WorkflowActionFormPersonEntryOption.Required ) );
        helper.UpdateProperty( form, f => f.PersonEntryMaritalStatusEntryOption,
            maritalStatusOption ?? FirstTimeDefault( isTurningPersonEntryOn, WorkflowActionFormPersonEntryOption.Optional ) );
        helper.UpdateProperty( form, f => f.PersonEntryMobilePhoneEntryOption,
            mobilePhoneOption ?? FirstTimeDefault( isTurningPersonEntryOn, WorkflowActionFormPersonEntryOption.Optional ) );
        helper.UpdateProperty( form, f => f.PersonEntryRaceEntryOption, raceOption );
        helper.UpdateProperty( form, f => f.PersonEntrySpouseEntryOption,
            spouseOption ?? FirstTimeDefault( isTurningPersonEntryOn, WorkflowActionFormPersonEntryOption.Hidden ) );
        helper.UpdateProperty( form, f => f.PersonEntrySmsOptInEntryOption,
            smsOptInOption ?? FirstTimeDefault( isTurningPersonEntryOn, WorkflowActionFormShowHideOption.Show ) );
        helper.UpdateProperty( form, f => f.PersonEntrySpouseLabel, spouseLabel );

        helper.UpdateProperty( form, f => f.PersonEntryAutofillCurrentPerson, isAutofillCurrentPersonEnabled );
        helper.UpdateProperty( form, f => f.PersonEntryCampusIsVisible, isCampusVisible );
        helper.UpdateProperty( form, f => f.PersonEntryHideIfCurrentPersonKnown,
            isHiddenIfCurrentPersonKnown ?? FirstTimeDefault( isTurningPersonEntryOn, false ) );

        // Not a column. This one lives in the form's additional settings JSON, which is
        // why it is set through the settings object rather than UpdateProperty.
        if ( isInactiveCampusIncluded.HasValue )
        {
            SetIncludeInactiveCampus( form, isInactiveCampusIncluded.Value );
        }

        helper.UpdateProperty( form, f => f.PersonEntryTitle, title );
        helper.UpdateProperty( form, f => f.PersonEntryDescription, description );
        helper.UpdateProperty( form, f => f.PersonEntryShowHeadingSeparator, isHeadingSeparatorShown );
        helper.UpdateProperty( form, f => f.PersonEntryPreHtml, preHtml );
        helper.UpdateProperty( form, f => f.PersonEntryPostHtml, postHtml );

        // Each of these validates against the defined type named by the property's own
        // DefinedValue attribute, so a value from the wrong type is refused by name.
        helper.UpdateDefinedValueProperty( form, f => f.PersonEntryConnectionStatusValue, connectionStatusDefinedValueIdKey );
        helper.UpdateDefinedValueProperty( form, f => f.PersonEntryRecordStatusValue, recordStatusDefinedValueIdKey );
        helper.UpdateDefinedValueProperty( form, f => f.PersonEntryRecordSourceValue, recordSourceDefinedValueIdKey );
        helper.UpdateDefinedValueProperty( form, f => f.PersonEntryGroupLocationTypeValue, addressTypeDefinedValueIdKey );
        helper.UpdateDefinedValueProperty( form, f => f.PersonEntryCampusStatusValue, campusStatusDefinedValueIdKey );
        helper.UpdateDefinedValueProperty( form, f => f.PersonEntryCampusTypeValue, campusTypeDefinedValueIdKey );
        helper.UpdateDefinedValueProperty( form, f => f.PersonEntrySectionTypeValue, sectionTypeDefinedValueIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        ApplyPersonEntryDefaults( form, rockContext );

        // Checked against the value the form will actually hold rather than against the
        // parameter, so enabling person entry in one call and binding the attribute in
        // an earlier one is accepted.
        if ( form.AllowPersonEntry && !form.PersonEntryPersonAttributeGuid.HasValue )
        {
            return Error( "Person entry needs a person attribute to write its result to. Without one the form collects a person that nothing in the workflow can reach." )
                .WithInstructions( $"Add a Person attribute with {nameof( AddOrUpdateWorkflowAttribute )}, then call this function again with its key as personAttributeIdKey." );
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var savedActionType = new WorkflowActionTypeService( rockContext )
            .Queryable( "WorkflowForm.FormAttributes,WorkflowForm.FormSections,ActivityType" )
            .FirstOrDefault( at => at.Id == actionType.Id );

        var result = GetActionFormResult( savedActionType ?? actionType, rockContext, clipLongValues: false );

        return Success( result )
            .WithInstructions( form.AllowPersonEntry
                ? "Person entry is configured. The matched or created person is written to the bound attribute, which later actions can read."
                : "Person entry is turned off. The form collects only its own fields." )
            .WithHistoryContent( new KeyNameResult( actionType.Id, actionType.Guid, actionType.Name ) );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Fills in the settings that have a sensible default when a caller did not supply
    /// one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two settings have a defensible default. Record status has none on the model, so
    /// a block configured without it creates people with no record status at all. That
    /// is not a visible failure: the person is created, appears in searches, and only
    /// later behaves oddly wherever record status is filtered on. Active is what Rock's
    /// own UI uses and what any normal form wants.
    /// </para>
    /// <para>
    /// Address type is the same shape of problem. An address collected against no
    /// location type is stored and then belongs nowhere, so it does not appear as the
    /// person's home address. Home is the only sensible default for a form asking
    /// someone where they live.
    /// </para>
    /// <para>
    /// Campus type and status default to Physical and Open. A picker offering every
    /// campus regardless of type or status shows online and closed campuses to someone
    /// being asked where they attend, which is rarely what a form wants.
    /// </para>
    /// <para>
    /// Include-inactive-campus defaults to <c>false</c>, which is deliberately the
    /// opposite of Rock's own default of <c>true</c>. Offering a campus the church has
    /// closed is a data-entry mistake waiting to happen, and a caller that genuinely
    /// wants it can ask.
    /// </para>
    /// <para>
    /// Connection status deliberately has no default. Unlike these its list is
    /// configured per organization, so there is no value that is right everywhere and
    /// guessing would apply a wrong status silently to every person the form creates.
    /// </para>
    /// <para>
    /// All are applied only when person entry is on and only when nothing is already
    /// set, so none overwrites a deliberate choice nor quietly re-adds a value a caller
    /// just cleared in the same call.
    /// </para>
    /// </remarks>
    /// <param name="form">The form being configured.</param>
    /// <param name="rockContext">The context to read through.</param>
    private static void ApplyPersonEntryDefaults( WorkflowActionForm form, RockContext rockContext )
    {
        if ( !form.AllowPersonEntry )
        {
            return;
        }

        if ( !form.PersonEntryRecordStatusValueId.HasValue )
        {
            form.PersonEntryRecordStatusValueId = GetDefinedValueId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE, rockContext );
        }

        if ( !form.PersonEntryGroupLocationTypeValueId.HasValue )
        {
            form.PersonEntryGroupLocationTypeValueId = GetDefinedValueId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, rockContext );
        }

        if ( !form.PersonEntryCampusTypeValueId.HasValue )
        {
            form.PersonEntryCampusTypeValueId = GetDefinedValueId( Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL, rockContext );
        }

        if ( !form.PersonEntryCampusStatusValueId.HasValue )
        {
            form.PersonEntryCampusStatusValueId = GetDefinedValueId( Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN, rockContext );
        }

        // Absent is what Rock reads as true, so the default has to be written rather
        // than left unset.
        var additionalSettings = form.GetAdditionalSettingsOrNull<PersonEntryAdditionalSettings>();

        if ( additionalSettings?.IncludeInactiveCampus == null )
        {
            SetIncludeInactiveCampus( form, false );
        }
    }

    /// <summary>
    /// Returns a default only at the moment person entry is turned on.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The option settings cannot go through ApplyPersonEntryDefaults, because that
    /// fills a value only when the form holds none and these are non-nullable columns
    /// that always hold something. Rock's own default is already there, so "unset" is
    /// not a state that can be detected after the fact.
    /// </para>
    /// <para>
    /// The moment worth acting on is the transition from off to on, which is when
    /// someone is configuring person entry for the first time and has said nothing
    /// about a given field. Adjusting a block that was already on leaves every
    /// unmentioned setting exactly as it was, so a deliberate choice made earlier is
    /// never quietly reverted.
    /// </para>
    /// </remarks>
    /// <typeparam name="TValue">The setting's type.</typeparam>
    /// <param name="isTurningPersonEntryOn">Whether this call is enabling person entry.</param>
    /// <param name="value">The default to apply at that moment.</param>
    /// <returns>The default, or <c>null</c> to leave the current value alone.</returns>
    private static TValue? FirstTimeDefault<TValue>( bool isTurningPersonEntryOn, TValue value )
        where TValue : struct
    {
        return isTurningPersonEntryOn ? value : ( TValue? ) null;
    }

    /// <summary>
    /// Resolves a defined value's id from its unique identifier.
    /// </summary>
    /// <param name="definedValueGuid">The defined value's unique identifier.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The id, or <c>null</c> when the value is not present.</returns>
    private static int? GetDefinedValueId( string definedValueGuid, RockContext rockContext )
    {
        return DefinedValueCache.Get( definedValueGuid.AsGuid(), rockContext )?.Id;
    }

    /// <summary>
    /// Sets whether the person entry campus picker offers inactive campuses.
    /// </summary>
    /// <remarks>
    /// This is the one person entry setting that is not a column on the form. It lives
    /// in the form's additional settings JSON as
    /// <see cref="PersonEntryAdditionalSettings"/>, and the non-template path reads it
    /// as <c>?? true</c>, so leaving it unset means inactive campuses are offered.
    /// </remarks>
    /// <param name="form">The form being configured.</param>
    /// <param name="isIncluded">Whether inactive campuses should be offered.</param>
    private static void SetIncludeInactiveCampus( WorkflowActionForm form, bool isIncluded )
    {
        var settings = form.GetAdditionalSettings<PersonEntryAdditionalSettings>();

        settings.IncludeInactiveCampus = isIncluded;

        form.SetAdditionalSettings( settings );
    }

    /// <summary>
    /// Sets one of person entry's three attribute bindings.
    /// </summary>
    /// <remarks>
    /// These are raw Guid columns rather than foreign keys, so there is no navigation
    /// property and <c>UpdateNavigationProperty</c> cannot be used. The attribute is
    /// still validated against the same scope a form field is, the workflow's own
    /// attributes plus the containing activity's, so a binding cannot point at an
    /// attribute this form could never reach.
    /// </remarks>
    /// <param name="form">The form being configured.</param>
    /// <param name="propertyExpression">The Guid property to set.</param>
    /// <param name="parameter">The supplied attribute key, or a clear instruction.</param>
    /// <param name="activityType">The activity the form's action belongs to.</param>
    /// <param name="helper">The helper to record errors on.</param>
    /// <param name="rockContext">The context to read through.</param>
    private static void SetPersonEntryAttribute(
        WorkflowActionForm form,
        System.Linq.Expressions.Expression<System.Func<WorkflowActionForm, System.Guid?>> propertyExpression,
        SetOrClear<string> parameter,
        WorkflowActivityType activityType,
        AgentToolHelper helper,
        RockContext rockContext )
    {
        if ( parameter == null )
        {
            return;
        }

        var property = ( ( System.Linq.Expressions.MemberExpression ) propertyExpression.Body ).Member as System.Reflection.PropertyInfo;

        if ( parameter.ClearValue )
        {
            property.SetValue( form, null );

            return;
        }

        if ( parameter.Value.IsNullOrWhiteSpace() )
        {
            return;
        }

        var attributeId = IdHasher.Instance.GetId( parameter.Value );
        var attribute = attributeId.HasValue
            ? GetReferenceableAttributes( activityType, rockContext ).FirstOrDefault( a => a.Id == attributeId.Value )
            : null;

        if ( attribute == null )
        {
            helper.AddError( $"'{parameter.Value}' is not an attribute of this workflow type or of the activity this form belongs to." );

            return;
        }

        property.SetValue( form, attribute.Guid );
    }

    #endregion
}
