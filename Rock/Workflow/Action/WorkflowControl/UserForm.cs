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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Workflow;
using Rock.Field;
using Rock.Model;
using Rock.Net;
using Rock.Utility;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.ViewModels.Workflow;
using Rock.Web.Cache;
using Rock.Workflow.FormBuilder;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Prompts user for attribute values" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Form" )]

    [Rock.SystemGuid.EntityTypeGuid( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68" )]
    public class UserEntryForm : ActionComponent, IInteractiveAction
    {
        #region Keys

        private static class ComponentConfigurationKey
        {
            public const string HeaderHtml = "headerHtml";
            public const string FooterHtml = "footerHtml";
            public const string Sections = "sections";
            public const string Fields = "fields";
            public const string PersonEntry = "personEntry";
            public const string Buttons = "buttons";
            public const string ShowNotes = "showNotes";
        }

        private static class ComponentDataKey
        {
            public const string FieldValues = "fieldValues";
            public const string PersonEntryValues = "personEntryValues";
            public const string Button = "button";
        }

        #endregion

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var actionType = action.ActionTypeCache;

            /*
             * 2020-01-30: DL
             * Workflow Form instances created prior to v1.10.2 may hold a reference to a SystemEmail (deprecated) rather than a SystemCommunication,
             * Processing has been added here to maintain backward-compatibility, with the SystemCommunication setting being preferred if it exists.
             */
#pragma warning disable CS0618 // Type or member is obsolete
            var sendNotification = !action.LastProcessedDateTime.HasValue &&
                actionType != null &&
                actionType.WorkflowForm != null &&
                ( actionType.WorkflowForm.NotificationSystemCommunicationId.HasValue );
#pragma warning restore CS0618 // Type or member is obsolete

            if ( sendNotification )
            {
                if ( action.Activity != null && ( action.Activity.AssignedPersonAliasId.HasValue || action.Activity.AssignedGroupId.HasValue ) )
                {
                    var recipients = new List<RockMessageRecipient>();
                    var workflowMergeFields = GetMergeFields( action );

                    if ( action.Activity.AssignedPersonAliasId.HasValue )
                    {
                        var person = new PersonAliasService( rockContext ).Queryable()
                            .Where( a => a.Id == action.Activity.AssignedPersonAliasId.Value )
                            .Select( a => a.Person )
                            .FirstOrDefault();

                        if ( person != null && !string.IsNullOrWhiteSpace( person.Email ) )
                        {
                            recipients.Add( new RockEmailMessageRecipient( person, CombinePersonMergeFields( person, workflowMergeFields ) ) );
                            action.AddLogEntry( string.Format( "Form notification sent to '{0}'", person.FullName ) );
                        }
                    }

                    if ( action.Activity.AssignedGroupId.HasValue )
                    {
                        var personList = new GroupMemberService( rockContext ).GetByGroupId( action.Activity.AssignedGroupId.Value )
                            .Where( m =>
                                m.GroupMemberStatus == GroupMemberStatus.Active &&
                                m.Person.Email != "" )
                            .Select( m => m.Person )
                            .ToList();

                        foreach ( var person in personList )
                        {
                            recipients.Add( new RockEmailMessageRecipient( person, CombinePersonMergeFields( person, workflowMergeFields ) ) );
                            action.AddLogEntry( string.Format( "Form notification sent to '{0}'", person.FullName ) );
                        }
                    }

                    if ( recipients.Count > 0 )
                    {
                        // The email may need to reference activity Id, so we need to save here.
                        WorkflowService workflowService = new WorkflowService( rockContext );
                        workflowService.PersistImmediately( action );

                        // Create and send the notification email.
                        RockEmailMessage emailMessage = null;

                        if ( action.ActionTypeCache.WorkflowForm.NotificationSystemCommunicationId.HasValue )
                        {
                            var systemCommunication = new SystemCommunicationService( rockContext ).Get( action.ActionTypeCache.WorkflowForm.NotificationSystemCommunicationId.Value );

                            if ( systemCommunication != null )
                            {
                                emailMessage = new RockEmailMessage( systemCommunication );
                            }
                        }

                        if ( emailMessage != null )
                        {
                            emailMessage.SetRecipients( recipients );
                            emailMessage.CreateCommunicationRecord = false;

                            /*
                                [2024-06-20] - DJL

                                Changed the AppRoot from InternalApplicationRoot to PublicApplicationRoot.

                                Reason:
                                    To ensure that links embedded in the email are accessible to both
                                    internal and external recipients.
                             */
                            emailMessage.AppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) ?? string.Empty;

                            emailMessage.Send();
                        }
                        else
                        {
                            action.AddLogEntry( "Could not find the selected notification system communication.", true );
                        }

                    }
                    else
                    {
                        action.AddLogEntry( "Could not send form notification due to no assigned person or group member not having email address", true );
                    }
                }
                else
                {
                    action.AddLogEntry( "Could not send form notification due to no assigned person or group", true );
                }
            }

            // Always return false. Special logic for User Form will be handled in the WorkflowEntry block.
            return false;
        }

        private Dictionary<string, object> CombinePersonMergeFields( Person person, Dictionary<string, object> mergeFields )
        {
            var personFields = new Dictionary<string, object>();
            personFields.Add( "Person", person );

            foreach ( var keyVal in mergeFields )
            {
                personFields.Add( keyVal.Key, keyVal.Value );
            }

            return personFields;
        }

        #region IInteractiveAction

        /// <inheritdoc/>
        InteractiveActionResult IInteractiveAction.StartAction( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext )
        {
            return StartActionInternal( action, rockContext, requestContext );
        }

        /// <inheritdoc/>
        InteractiveActionResult IInteractiveAction.UpdateAction( WorkflowAction action, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
            return UpdateActionInternal( action, componentData, rockContext, requestContext );
        }

        #endregion

        #region Methods

        /// <inheritdoc cref="IInteractiveAction.StartAction(WorkflowAction, RockContext, RockRequestContext)"/>
        internal static InteractiveActionResult StartActionInternal( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext )
        {
            var actionId = requestContext.GetPageParameter( "ActionId" ).AsIntegerOrNull();
            var command = requestContext.GetPageParameter( "Command" );

            // If this is a link from an e-mail then we need to auto-submit
            // the form if we are able to.
            if ( actionId == action.Id && command.IsNotNullOrWhiteSpace() )
            {
                var actionData = CompleteEntryFormAction( action, command, rockContext, requestContext );

                return new InteractiveActionResult
                {
                    IsSuccess = true,
                    ProcessingType = InteractiveActionContinueMode.Continue,
                    ActionData = actionData
                };
            }

            return new InteractiveActionResult
            {
                IsSuccess = false,
                IsNotesVisible = action?.ActionTypeCache?.WorkflowForm?.AllowNotes == true,
                ProcessingType = InteractiveActionContinueMode.Stop,
                ActionData = GetEntryFormData( action, rockContext, requestContext )
            };
        }

        /// <inheritdoc cref="IInteractiveAction.UpdateAction(WorkflowAction, Dictionary{string, string}, RockContext, RockRequestContext)"/>
        internal static InteractiveActionResult UpdateActionInternal( WorkflowAction action, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
            var form = action?.ActionTypeCache?.WorkflowForm;
            var fieldValues = componentData.GetValueOrNull( ComponentDataKey.FieldValues )?.FromJsonOrNull<Dictionary<Guid, string>>();
            var personEntryValues = componentData.GetValueOrNull( ComponentDataKey.PersonEntryValues )?.FromJsonOrNull<PersonEntryValuesBag>();
            var button = componentData.GetValueOrNull( ComponentDataKey.Button );

            if ( form == null )
            {
                throw new Exception( "Invalid action state, could not find workflow or form details." );
            }

            if ( personEntryValues != null )
            {
                // The person entry processor requires its own RockContext.
                using ( var personEntryRockContext = new RockContext() )
                {
                    var processor = new WorkflowPersonEntryProcessor( action, personEntryRockContext );
                    processor.SetFormPersonEntryValues( requestContext.CurrentPerson?.Id, personEntryValues );
                }
            }

            SetFormFieldValues( action, fieldValues, rockContext );

            var actionData = CompleteEntryFormAction( action, button, rockContext, requestContext );

            return new InteractiveActionResult
            {
                IsSuccess = true,
                ProcessingType = InteractiveActionContinueMode.Continue,
                ActionData = actionData
            };
        }

        /// <summary>
        /// Gets the data to be sent to a UI component in order to display.
        /// the entry form.
        /// </summary>
        /// <param name="action">The action that represents the entry form to display.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that identifies the current request, must never be <c>null</c>.</param>
        /// <returns>An dictionary of strings that represents the form to display.</returns>
        private static InteractiveActionDataBag GetEntryFormData( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext )
        {
            var activity = action?.Activity;
            var workflow = action?.Activity?.Workflow;
            var form = action?.ActionTypeCache?.WorkflowForm;

            // Make sure all our data is valid.
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            if ( requestContext == null )
            {
                throw new ArgumentNullException( nameof( requestContext ) );
            }

            if ( activity == null || workflow == null || form == null )
            {
                throw new Exception( "Invalid action state, could not find workflow or form details." );
            }

            // Prepare the merge fields for the HTML content.
            var mergeFields = requestContext.GetCommonMergeFields();
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", activity );
            mergeFields.Add( "Workflow", workflow );

            var formBuilderTemplate = workflow.WorkflowTypeCache.FormBuilderTemplateId.HasValue
                ? WorkflowFormBuilderTemplateCache.Get( workflow.WorkflowTypeCache.FormBuilderTemplateId.Value, rockContext )
                : null;

            var headerTemplate = formBuilderTemplate != null
                ? formBuilderTemplate.FormHeader.ToStringSafe() + form.Header.ToStringSafe()
                : form.Header.ToStringSafe();
            var footerTemplate = formBuilderTemplate != null
                ? formBuilderTemplate.FormFooter.ToStringSafe() + form.Footer.ToStringSafe()
                : form.Footer.ToStringSafe();

            var fields = GetFormFields( form, action, rockContext );
            var fieldValues = GetFormFieldValues( form, action, rockContext );
            var personEntryValues = GetPersonEntryValues( action, requestContext.CurrentPerson, rockContext );
            var personEntryConfiguration = GetPersonEntryConfiguration( action, requestContext.CurrentPerson, personEntryValues?.MaritalStatusGuid, mergeFields, rockContext );

            return new InteractiveActionDataBag
            {
                ComponentUrl = requestContext.ResolveRockUrl( "~/Obsidian/Blocks/Workflow/WorkflowEntry/Actions/entryForm.obs" ),
                ComponentConfiguration = new Dictionary<string, string>
                {
                    [ComponentConfigurationKey.HeaderHtml] = headerTemplate.ResolveMergeFields( mergeFields ),
                    [ComponentConfigurationKey.FooterHtml] = footerTemplate.ResolveMergeFields( mergeFields ),
                    [ComponentConfigurationKey.Sections] = GetSections( form, rockContext ).ToCamelCaseJson( false, false ),
                    [ComponentConfigurationKey.Fields] = fields.ToCamelCaseJson( false, false ),
                    [ComponentConfigurationKey.PersonEntry] = personEntryConfiguration.ToCamelCaseJson( false, false ),
                    [ComponentConfigurationKey.Buttons] = form.GetFormActionButtons( rockContext )
                        .Select( b => GetEntryFormButton( b, rockContext ) )
                        .ToCamelCaseJson( false, false )
                },
                ComponentData = new Dictionary<string, string>
                {
                    [ComponentDataKey.FieldValues] = fieldValues.ToCamelCaseJson( false, false ),
                    [ComponentDataKey.PersonEntryValues] = personEntryValues.ToCamelCaseJson( false, false ),
                }
            };
        }

        /// <summary>
        /// Gets the person entry details to be sent to the shell.
        /// </summary>
        /// <param name="action">The action currently being processed.</param>
        /// <param name="currentPerson">The current person the form will be displayed to.</param>
        /// <param name="currentMaritalStatusGuid">The current marital status that will be selected in the UI.</param>
        /// <param name="mergeFields">The merge fields to use for Lava parsing.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>The object that will be included in the response that details the person entry part of the form.</returns>
        private static PersonEntryConfigurationBag GetPersonEntryConfiguration( WorkflowAction action, Person currentPerson, Guid? currentMaritalStatusGuid, IDictionary<string, object> mergeFields, RockContext rockContext )
        {
            var form = action.ActionTypeCache.WorkflowForm;

            if ( form == null )
            {
                return null;
            }

            var workflowType = WorkflowTypeCache.Get( action.Activity.Workflow.WorkflowTypeId, rockContext );
            var formBuilderTemplate = workflowType.FormBuilderTemplateId.HasValue
                ? WorkflowFormBuilderTemplateCache.Get( workflowType.FormBuilderTemplateId.Value )
                : null;
            var formPersonEntrySettings = form.GetFormPersonEntrySettings( formBuilderTemplate );

            if ( !form.GetAllowPersonEntry( formBuilderTemplate ) )
            {
                return null;
            }

            if ( formPersonEntrySettings.HideIfCurrentPersonKnown && currentPerson != null )
            {
                return null;
            }

            var showCampus = formPersonEntrySettings.ShowCampus;
            List<ListItemBag> campusBags = null;
            string sectionCssClass = null;

            if ( showCampus )
            {
                campusBags = CampusCache.All( rockContext )
                    .Where( c => !formPersonEntrySettings.CampusStatusValueId.HasValue || c.CampusStatusValueId == formPersonEntrySettings.CampusStatusValueId.Value )
                    .Where( c => !formPersonEntrySettings.CampusTypeValueId.HasValue || c.CampusTypeValueId == formPersonEntrySettings.CampusTypeValueId.Value )
                    .Where( c => formPersonEntrySettings.IncludeInactiveCampus || c.IsActive == true )
                    .ToListItemBagList();

                if ( campusBags.Count == 0 )
                {
                    showCampus = false;
                }
                else if ( campusBags.Count == 1 )
                {
                    // If we have a single campus, only show if there were any filters.
                    showCampus = formPersonEntrySettings.CampusStatusValueId.HasValue
                        || formPersonEntrySettings.CampusTypeValueId.HasValue;
                }
            }

            if ( form.PersonEntrySectionTypeValueId.HasValue )
            {
                var sectionTypeValue = DefinedValueCache.Get( form.PersonEntrySectionTypeValueId.Value, rockContext );
                sectionCssClass = sectionTypeValue?.GetAttributeValue( "CSSClass" );
            }

            return new PersonEntryConfigurationBag
            {
                Title = form.PersonEntryTitle,
                Description = form.PersonEntryDescription,
                ShowHeadingSeparator = form.PersonEntryShowHeadingSeparator,
                SectionCssClass = sectionCssClass,
                PreHtml = form.PersonEntryPreHtml.ResolveMergeFields( mergeFields ),
                PostHtml = form.PersonEntryPostHtml.ResolveMergeFields( mergeFields ),
                IsCampusVisible = showCampus,
                Campuses = campusBags,
                SpouseOption = formPersonEntrySettings.SpouseEntry,
                GenderOption = formPersonEntrySettings.Gender,
                EmailOption = formPersonEntrySettings.Email,
                MobilePhoneOption = formPersonEntrySettings.MobilePhone,
                IsSmsVisible = formPersonEntrySettings.SmsOptIn == WorkflowActionFormShowHideOption.Show,
                BirthDateOption = formPersonEntrySettings.Birthdate,
                AddressOption = formPersonEntrySettings.AddressTypeValueId.HasValue
                    ? formPersonEntrySettings.Address
                    : WorkflowActionFormPersonEntryOption.Hidden,
                MaritalStatusOption = formPersonEntrySettings.MaritalStatus,
                MaritalStatuses = GetMaritalStatusItems( currentMaritalStatusGuid, rockContext ),
                SpouseLabel = formPersonEntrySettings.SpouseLabel,
                RaceOption = formPersonEntrySettings.RaceEntry,
                EthnicityOption = formPersonEntrySettings.EthnicityEntry
            };
        }

        /// <summary>
        /// Gets the existing values to display in the Person Entry section of
        /// the form.
        /// </summary>
        /// <param name="action">The action that represents this form and provides all the configuration data.</param>
        /// <param name="currentPerson">The current person the form will be displayed to.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>The current values to be displayed on the person entry form.</returns>
        private static PersonEntryValuesBag GetPersonEntryValues( WorkflowAction action, Person currentPerson, RockContext rockContext )
        {
            var form = action.ActionTypeCache.WorkflowForm;
            var workflowType = WorkflowTypeCache.Get( action.Activity.Workflow.WorkflowTypeId, rockContext );
            var formBuilderTemplate = workflowType.FormBuilderTemplateId.HasValue
                ? WorkflowFormBuilderTemplateCache.Get( workflowType.FormBuilderTemplateId.Value )
                : null;
            var formSettings = form.GetFormPersonEntrySettings( formBuilderTemplate );

            if ( !form.GetAllowPersonEntry( formBuilderTemplate ) )
            {
                return null;
            }

            if ( formSettings.HideIfCurrentPersonKnown && currentPerson != null )
            {
                return null;
            }

            action.GetPersonEntryPeople( rockContext, currentPerson?.Id, out var personEntryPerson, out var personEntrySpouse );

            // Get the default address if it is supposed to show.
            AddressControlBag address = null;
            var promptForAddress = formSettings.Address != WorkflowActionFormPersonEntryOption.Hidden
                && formSettings.AddressTypeValueId.HasValue;
            if ( promptForAddress && ( personEntryPerson?.PrimaryFamilyId ).HasValue )
            {
                var addressLocationTypeValueId = formSettings.AddressTypeValueId.Value;

                var familyLocation = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.GroupId == personEntryPerson.PrimaryFamilyId.Value && a.GroupLocationTypeValueId == addressLocationTypeValueId )
                    .Select( a => a.Location )
                    .FirstOrDefault();

                if ( familyLocation != null )
                {
                    address = new AddressControlBag
                    {
                        City = familyLocation?.City,
                        Country = familyLocation?.Country,
                        State = familyLocation?.State,
                        Locality = familyLocation?.County,
                        PostalCode = familyLocation?.PostalCode,
                        Street1 = familyLocation?.Street1,
                        Street2 = familyLocation?.Street2
                    };
                }
            }

            // Get current marital status or default to Married if this will
            // be a new person.
            var maritalStatusGuid = personEntryPerson != null
                ? personEntryPerson.MaritalStatusValue?.Guid
                : Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();

            return new PersonEntryValuesBag
            {
                Address = address,
                CampusGuid = personEntryPerson?.PrimaryCampusId != null
                    ? CampusCache.Get( personEntryPerson.PrimaryCampusId.Value, rockContext )?.Guid
                    : null,
                MaritalStatusGuid = maritalStatusGuid,
                Person = GetPersonBag( formSettings, personEntryPerson, rockContext ),
                Spouse = GetPersonBag( formSettings, personEntrySpouse, rockContext )
            };
        }

        /// <summary>
        /// Gets the details of a single person that will be displayed in the
        /// person entry form. This can either be the main person or the spouse.
        /// </summary>
        /// <param name="formSettings">The settings that describe how the form is configured.</param>
        /// <param name="person">The person to be displayed on the form.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>The current values that represent <paramref name="person"/>.</returns>
        private static PersonBasicEditorBag GetPersonBag( FormPersonEntrySettings formSettings, Person person, RockContext rockContext )
        {
            if ( person == null )
            {
                return null;
            }

            var personBag = new PersonBasicEditorBag
            {
                FirstName = person.FirstName,
                NickName = person.NickName,
                LastName = person.LastName,
            };

            if ( formSettings.Email != WorkflowActionFormPersonEntryOption.Hidden )
            {
                personBag.Email = person.Email;
            }

            if ( formSettings.MobilePhone != WorkflowActionFormPersonEntryOption.Hidden )
            {
                var existingMobilePhone = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), rockContext );

                personBag.MobilePhoneCountryCode = existingMobilePhone?.CountryCode;
                personBag.MobilePhoneNumber = existingMobilePhone?.Number;
                personBag.IsMessagingEnabled = existingMobilePhone?.IsMessagingEnabled ?? false;
            }

            if ( formSettings.Birthdate != WorkflowActionFormPersonEntryOption.Hidden )
            {
                personBag.PersonBirthDate = person.BirthDate.HasValue
                    ? new DatePartsPickerValueBag
                    {
                        Year = person.BirthDate.Value.Year,
                        Month = person.BirthDate.Value.Month,
                        Day = person.BirthDate.Value.Day
                    }
                    : null;
            }

            if ( formSettings.Gender != WorkflowActionFormPersonEntryOption.Hidden )
            {
                personBag.PersonGender = person.Gender;
            }

            return personBag;
        }

        /// <summary>
        /// Gets the bags that will represent each section of the entry form.
        /// </summary>
        /// <param name="form">The configuration details of the form.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>A collection of <see cref="EntryFormSectionBag"/> objects.</returns>
        private static List<EntryFormSectionBag> GetSections( WorkflowActionFormCache form, RockContext rockContext )
        {
            return form.FormAttributes
                .Select( a => a.ActionFormSectionId )
                .Where( a => a.HasValue )
                .Distinct()
                .Select( a => WorkflowActionFormSectionCache.Get( a.Value ) )
                .Where( a => a != null )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Title )
                .Select( a => new EntryFormSectionBag
                {
                    Id = a.IdKey,
                    Title = a.Title,
                    Description = a.Description,
                    CssClass = a.SectionTypeValueId.HasValue
                        ? DefinedValueCache.Get( a.SectionTypeValueId.Value, rockContext )?.GetAttributeValue( "CSSClass" )
                        : null,
                    IsHeadingSeparatorVisible = a.ShowHeadingSeparator,
                    VisibilityRule = new FieldFilterGroupBag
                    {
                        ExpressionType = a.SectionVisibilityRules.FilterExpressionType,
                        Rules = a.SectionVisibilityRules.RuleList
                            .Select( r =>
                            {
                                var ruleAttribute = form.FormAttributes
                                    .FirstOrDefault( fa => fa.Attribute.Guid == r.ComparedToFormFieldGuid )
                                    ?.Attribute;

                                if ( ruleAttribute == null )
                                {
                                    return null;
                                }

                                return FieldVisibilityRule.GetPublicRuleBag( ruleAttribute, r.ComparisonType, r.ComparedToValue );
                            } )
                            .Where( r => r != null )
                            .ToList()
                    }
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the fields that should be displayed on the form. This handles
        /// all filtering requirements for form fields. Conditional logic is
        /// handled on the client.
        /// </summary>
        /// <param name="form">The data that describes the form to be displayed.</param>
        /// <param name="action">The action currently being processed.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>A list of bags that represent the fields to be displayed.</returns>
        private static List<EntryFormFieldBag> GetFormFields( WorkflowActionFormCache form, WorkflowAction action, RockContext rockContext )
        {
            var fields = new List<EntryFormFieldBag>();
            var activity = action.Activity;
            var workflow = activity.Workflow;

            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( !formAttribute.IsVisible )
                {
                    continue;
                }

                var attribute = AttributeCache.Get( formAttribute.AttributeId, rockContext );

                // If there is no attribute or field logic then we can't
                // properly display the field so skip it.
                if ( attribute == null || attribute.FieldType?.Field == null )
                {
                    continue;
                }

                var value = attribute.DefaultValue;

                // Get the current value from either the workflow or the activity.
                if ( workflow.AttributeValues.ContainsKey( attribute.Key ) && workflow.AttributeValues[attribute.Key] != null )
                {
                    value = workflow.AttributeValues[attribute.Key].Value;
                }
                else if ( activity.AttributeValues.ContainsKey( attribute.Key ) && activity.AttributeValues[attribute.Key] != null )
                {
                    value = activity.AttributeValues[attribute.Key].Value;
                }

                var formField = new EntryFormFieldBag
                {
                    Attribute = PublicAttributeHelper.GetPublicAttributeForEdit( attribute ),
                    IsLabelHidden = formAttribute.HideLabel,
                    IsRequired = formAttribute.IsRequired,
                    PreHtml = formAttribute.PreHtml,
                    PostHtml = formAttribute.PostHtml,
                    SectionId = formAttribute.ActionFormSectionId.HasValue
                        ? IdHasher.Instance.GetHash( formAttribute.ActionFormSectionId.Value )
                        : null,
                    ColumnSize = formAttribute.ColumnSize,
                    VisibilityRule = new FieldFilterGroupBag
                    {
                        ExpressionType = formAttribute.FieldVisibilityRules.FilterExpressionType,
                        Rules = formAttribute.FieldVisibilityRules.RuleList
                            .Select( r =>
                            {
                                var ruleAttribute = form.FormAttributes
                                    .FirstOrDefault( fa => fa.Attribute.Guid == r.ComparedToFormFieldGuid )
                                    ?.Attribute;

                                if ( ruleAttribute == null )
                                {
                                    return null;
                                }

                                return FieldVisibilityRule.GetPublicRuleBag( ruleAttribute, r.ComparisonType, r.ComparedToValue );
                            } )
                            .Where( r => r != null )
                            .ToList()
                    }
                };

                // A read only field will be displayed as plain HTML. This
                // conversion is handled by the method that gets the form
                // field values. We indicate this to the client by setting
                // the field type identifier to an empty guid (since it
                // can't be null).
                if ( formAttribute.IsReadOnly )
                {
                    formField.Attribute.FieldTypeGuid = Guid.Empty;
                    formField.Attribute.ConfigurationValues = null;
                }

                fields.Add( formField );
            }

            return fields;
        }

        /// <summary>
        /// Gets the current field values for the fields that should be displayed
        /// on the form.
        /// </summary>
        /// <param name="form">The data that describes the form to be displayed.</param>
        /// <param name="action">The action currently being processed.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>A dictionary of field values with the keys being the attribute unique identifiers.</returns>
        private static Dictionary<Guid, string> GetFormFieldValues( WorkflowActionFormCache form, WorkflowAction action, RockContext rockContext )
        {
            var fieldValues = new Dictionary<Guid, string>();
            var activity = action.Activity;
            var workflow = activity.Workflow;

            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( !formAttribute.IsVisible )
                {
                    continue;
                }

                var attribute = AttributeCache.Get( formAttribute.AttributeId, rockContext );
                var field = attribute?.FieldType?.Field;

                // If there is no attribute or field logic then we can't
                // properly display the field so skip it.
                if ( attribute == null || field == null )
                {
                    continue;
                }

                var value = attribute.DefaultValue;

                // Get the current value from either the workflow or the activity.
                if ( workflow.AttributeValues.ContainsKey( attribute.Key ) && workflow.AttributeValues[attribute.Key] != null )
                {
                    value = workflow.AttributeValues[attribute.Key].Value;
                }
                else if ( activity.AttributeValues.ContainsKey( attribute.Key ) && activity.AttributeValues[attribute.Key] != null )
                {
                    value = activity.AttributeValues[attribute.Key].Value;
                }

                // If the field is read only then we render it to HTML and
                // the client displays it as raw HTML.
                if ( formAttribute.IsReadOnly )
                {
                    var htmlValue = field.GetCondensedHtmlValue( value, attribute.ConfigurationValues );

                    fieldValues.Add( attribute.Guid, htmlValue );
                }
                else
                {
                    var publicValue = field.GetPublicEditValue( value, attribute.ConfigurationValues );

                    fieldValues.Add( attribute.Guid, publicValue );
                }
            }

            return fieldValues;
        }

        /// <summary>
        /// Sets the form values.
        /// </summary>
        /// <param name="action">The action that is processing the updates.</param>
        /// <param name="values">The form fields holding the data from the client..</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        internal static void SetFormFieldValues( WorkflowAction action, Dictionary<Guid, string> values, RockContext rockContext )
        {
            var activity = action.Activity;
            var workflow = activity.Workflow;
            var form = action.ActionTypeCache.WorkflowForm;

            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( formAttribute.IsVisible && !formAttribute.IsReadOnly )
                {
                    var attribute = AttributeCache.Get( formAttribute.AttributeId, rockContext );

                    if ( attribute == null || !values.TryGetValue( attribute.Guid, out var formFieldValue ) )
                    {
                        continue;
                    }
                    IHasAttributes item = null;

                    if ( attribute.EntityTypeId == workflow.TypeId )
                    {
                        item = workflow;
                    }
                    else if ( attribute.EntityTypeId == activity.TypeId )
                    {
                        item = activity;
                    }

                    item?.SetPublicAttributeValue( attribute.Key, formFieldValue, null, false );
                }
            }
        }

        /// <summary>
        /// Gets the items to display in the marital status picker.
        /// </summary>
        /// <param name="currentSelectionGuid">The currently selected marital status value.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> objects that represent the picker options.</returns>
        private static List<ListItemBag> GetMaritalStatusItems( Guid? currentSelectionGuid, RockContext rockContext )
        {
            var definedType = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid(), rockContext );

            return definedType.DefinedValues
                .Where( dv => dv.IsActive || dv.Guid == currentSelectionGuid )
                .OrderBy( dv => dv.Order )
                .ThenBy( dv => dv.Value )
                .ToListItemBagList();
        }

        /// <summary>
        /// Convert the item bag for a button into a representation that can
        /// be sent down to a client for processing.
        /// </summary>
        /// <param name="buttonBag">The bag that represents the button.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>An instance of <see cref="EntryFormButtonBag"/> that represents the button.</returns>
        private static EntryFormButtonBag GetEntryFormButton( ListItemBag buttonBag, RockContext rockContext )
        {
            // Get the button HTML. If actionParts has a guid at [1],
            // get the buttonHtml from the DefinedValue with that Guid.
            // Otherwise, use a default
            string buttonHtml = string.Empty;
            var buttonDefinedValue = DefinedTypeCache.Get( SystemGuid.DefinedType.BUTTON_HTML.AsGuid(), rockContext )
                .DefinedValues
                .FirstOrDefault( dv => dv.Value == buttonBag.Value );

            if ( buttonDefinedValue != null )
            {
                buttonHtml = buttonDefinedValue.GetAttributeValue( "ButtonHTML" );
            }

            if ( buttonHtml.IsNullOrWhiteSpace() )
            {
                buttonHtml = "<a href=\"{{ ButtonLink }}\" onclick=\"{{ ButtonClick }}\" class=\"btn btn-primary\" data-loading-text=\"<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}\">{{ ButtonText }}</a>";
            }

            var buttonMergeFields = new Dictionary<string, object>();
            var javascriptCancelButton = WorkflowActionFormUserAction.IsCancelButton( buttonDefinedValue?.Guid, buttonHtml );
            var buttonClickScript = javascriptCancelButton ? "__novalidate__" : "__validate__";

            buttonMergeFields.Add( "ButtonText", buttonBag.Text.EncodeHtml() );
            buttonMergeFields.Add( "ButtonClick", buttonClickScript );

            var buttonLinkScript = $"#__submit__:{buttonBag.Text.EncodeHtml()}";
            buttonMergeFields.Add( "ButtonLink", buttonLinkScript );

            buttonHtml = buttonHtml.ResolveMergeFields( buttonMergeFields );

            return new EntryFormButtonBag
            {
                Action = buttonBag.Value,
                Title = buttonBag.Text,
                Html = buttonHtml
            };
        }

        /// <summary>
        /// Completes the form action based on the action selected by the user.
        /// </summary>
        /// <param name="action">The specific action to be completed by the form action.</param>
        /// <param name="formAction">The form action.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The request context associated with this form action or <c>null</c> if there is not an active request.</param>
        /// <returns>A response message.</returns>
        internal static InteractiveActionDataBag CompleteEntryFormAction( WorkflowAction action, string formAction, RockContext rockContext, RockRequestContext requestContext )
        {
            var activity = action.Activity;
            var workflow = activity.Workflow;
            var mergeFields = requestContext.GetCommonMergeFields()
                ?? new Dictionary<string, object>();

            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", activity );
            mergeFields.Add( "Workflow", workflow );

            Guid activityTypeGuid = Guid.Empty;
            string responseText = "Your information has been submitted successfully.";

            // Get the target activity type guid and response text from the
            // submitted form action.
            foreach ( var act in action.ActionTypeCache.WorkflowForm.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var actionDetails = act.Split( new char[] { '^' } );
                if ( actionDetails.Length > 0 && actionDetails[0] == formAction )
                {
                    if ( actionDetails.Length > 2 )
                    {
                        activityTypeGuid = actionDetails[2].AsGuid();
                    }

                    if ( actionDetails.Length > 3 && !string.IsNullOrWhiteSpace( actionDetails[3] ) )
                    {
                        responseText = actionDetails[3].ResolveMergeFields( mergeFields );
                    }
                    break;
                }
            }

            action.MarkComplete();
            action.FormAction = formAction;
            action.AddLogEntry( "Form Action Selected: " + action.FormAction );

            if ( action.ActionTypeCache.IsActivityCompletedOnSuccess )
            {
                action.Activity.MarkComplete();
            }

            // Set the attribute that should contain the submitted form action.
            if ( action.ActionTypeCache.WorkflowForm.ActionAttributeGuid.HasValue )
            {
                var attribute = AttributeCache.Get( action.ActionTypeCache.WorkflowForm.ActionAttributeGuid.Value, rockContext );

                if ( attribute != null )
                {
                    IHasAttributes item = null;

                    if ( attribute.EntityTypeId == workflow.TypeId )
                    {
                        item = workflow;
                    }
                    else if ( attribute.EntityTypeId == activity.TypeId )
                    {
                        item = activity;
                    }

                    item?.SetAttributeValue( attribute.Key, formAction );
                }
            }

            // Activate the requested activity if there was one.
            if ( !activityTypeGuid.IsEmpty() )
            {
                var activityType = workflow.WorkflowTypeCache.ActivityTypes.Where( a => a.Guid.Equals( activityTypeGuid ) ).FirstOrDefault();
                if ( activityType != null )
                {
                    WorkflowActivity.Activate( activityType, workflow );
                }
            }

            var workflowTypeCache = WorkflowTypeCache.Get( workflow.WorkflowTypeId );

            SetFormBuilderCampusValue( action, workflow, workflowTypeCache, requestContext );

            if ( workflowTypeCache.IsPersisted == false && workflowTypeCache.IsFormBuilder )
            {
                /* 3/14/2022 MP
                 If this is a FormBuilder workflow, the WorkflowType probably has _workflowType.IsPersisted == false.
                 This is because we don't want to persist the workflow until they have submitted.
                 So, in the case of FormBuilder, we'll persist when they submit regardless of the _workflowType.IsPersisted setting
                */
                new WorkflowService( rockContext ).PersistImmediately( action );
            }

            // TODO: This might not be needed anymore. The only thing I can
            // find that actually looks at this value is the ProcessWorkflows
            // job - which shouldn't matter.

            // If the LastProcessedDateTime is equal to RockDateTime.Now we need to pause for a bit so the workflow will actually process here.
            // The resolution of System.DateTime.UTCNow is between .5 and 15 ms which can cause the workflow processing to not properly pick up
            // where it left off.
            // Without this you might see random failures of workflows to save automatically.
            // https://docs.microsoft.com/en-us/dotnet/api/system.datetime.utcnow?view=netframework-4.7#remarks
            while ( workflow.LastProcessedDateTime == RockDateTime.Now )
            {
                System.Threading.Thread.Sleep( 1 );
            }

            ProcessFormBuilderCommunications( action, rockContext, requestContext );

            return GetActionResponseDataBag( action, workflowTypeCache, responseText );
        }

        /// <summary>
        /// Sets the campus of the workflow based on the form builder settings
        /// that were configured.
        /// </summary>
        /// <param name="action">The action that is being processed.</param>
        /// <param name="workflow">The workflow that is being processed.</param>
        /// <param name="workflowType">The workflow type that is being processed.</param>
        /// <param name="requestContext">The currently executing request that has completed this action.</param>
        private static void SetFormBuilderCampusValue( WorkflowAction action, Model.Workflow workflow, WorkflowTypeCache workflowType, RockRequestContext requestContext )
        {
            // If the current action is not a form builder action then we
            // don't do anything.
            if ( !( action.ActionTypeCache.WorkflowAction is Rock.Workflow.Action.FormBuilder ) )
            {
                return;
            }

            var workflowCampusSetFrom = workflowType?.FormBuilderSettings?.CampusSetFrom;
            switch ( workflowCampusSetFrom )
            {
                case Rock.Workflow.FormBuilder.CampusSetFrom.CurrentPerson:
                    workflow.CampusId = requestContext.CurrentPerson?.PrimaryCampusId;

                    break;

                case Rock.Workflow.FormBuilder.CampusSetFrom.WorkflowPerson:
                    action.GetPersonEntryPeople( new RockContext(), requestContext.CurrentPerson?.Id, out var personEntryPerson, out _ );
                    if ( personEntryPerson != null )
                    {
                        workflow.CampusId = personEntryPerson.PrimaryCampusId;
                    }

                    break;

                default:
                    var campusIdFromUrl = requestContext.GetPageParameter( "Campus" ).AsIntegerOrNull();
                    var campusGuidFromUrl = requestContext.GetPageParameter( "Campus" ).AsGuidOrNull();
                    if ( campusIdFromUrl.HasValue )
                    {
                        workflow.CampusId = campusIdFromUrl;
                    }
                    else if ( campusGuidFromUrl.HasValue )
                    {
                        workflow.CampusId = CampusCache.GetId( campusGuidFromUrl.Value );
                    }

                    break;
            }
        }

        /// <summary>
        /// Gets the response data for the workflow if it is a form builder
        /// type of workflow.
        /// </summary>
        /// <param name="action">The action that is being processed.</param>
        /// <param name="workflowType">The workflow type that is being processed.</param>
        /// <param name="responseText"></param>
        /// <returns>The action data that describes the next screen.</returns>
        private static InteractiveActionDataBag GetActionResponseDataBag( WorkflowAction action, WorkflowTypeCache workflowType, string responseText )
        {
            // If the current action is not a form builder action then we
            // just return the original response text.
            if ( action.ActionTypeCache.EntityType?.Guid != new Guid( "B2A91AD5-3B41-45A6-A670-EBBF3FF626F9" ) )
            {
                return new InteractiveActionDataBag
                {
                    Message = new InteractiveMessageBag
                    {
                        Type = Enums.Workflow.InteractiveMessageType.Information,
                        Content = responseText
                    }
                };
            }

            FormCompletionActionSettings completionActionSettings;

            if ( workflowType.FormBuilderTemplate != null )
            {
                completionActionSettings = workflowType.FormBuilderTemplate.CompletionActionSettings;
            }
            else if ( workflowType.FormBuilderSettings != null )
            {
                completionActionSettings = workflowType.FormBuilderSettings.CompletionAction;
            }
            else
            {
                // Not a form builder or form builder template, so use responseText.
                completionActionSettings = null;
            }

            if ( completionActionSettings != null && completionActionSettings.Type == FormCompletionActionType.DisplayMessage )
            {
                // If the form builder has a completion action of DisplayMessage,
                // set response from that.
                var content = completionActionSettings.Message;

                return new InteractiveActionDataBag
                {
                    Message = new InteractiveMessageBag
                    {
                        Type = InteractiveMessageType.Success,
                        Content = content
                    }
                };
            }
            else if ( completionActionSettings != null && completionActionSettings.Type == FormCompletionActionType.Redirect )
            {
                // If the form builder has a completion action of Redirect,
                // set response from that.
                return new InteractiveActionDataBag
                {
                    Message = new InteractiveMessageBag
                    {
                        Type = InteractiveMessageType.Redirect,
                        Content = completionActionSettings.RedirectUrl
                    }
                };
            }
            else
            {
                return new InteractiveActionDataBag
                {
                    Message = new InteractiveMessageBag
                    {
                        Type = InteractiveMessageType.Information,
                        Content = responseText
                    }
                };
            }
        }

        #endregion

        #region Form Builder Communication Methods

        /// <summary>
        /// Processes any form builder communications that need to happen after
        /// a form has been submitted.
        /// </summary>
        /// <param name="action">The workflow aciton being processed.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="requestContext">The context describing the current request.</param>
        private static void ProcessFormBuilderCommunications( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext )
        {
            // Only send communications if this was a FormBuilder form rather
            // than any entry form.
            if ( action.ActionTypeCache.EntityType?.Guid != new Guid( "B2A91AD5-3B41-45A6-A670-EBBF3FF626F9" ) )
            {
                return;
            }

            var workflowType = action.Activity.Workflow.WorkflowTypeCache;

            // Confirmation email can come FormBuilderSettings or FormBuilderTemplate
            var confirmationEmailSettings = workflowType.FormBuilderTemplate?.ConfirmationEmailSettings
                ?? workflowType.FormBuilderSettings?.ConfirmationEmail;

            if ( confirmationEmailSettings?.Enabled == true )
            {
                SendFormBuilderConfirmationEmail( action, confirmationEmailSettings, rockContext, requestContext );
            }

            // Notification Email is only defined on FormBuilder. FormBuilderTemplate
            // doesn't have NotificationEmailSettings.
            var notificationEmailSettings = workflowType.FormBuilderSettings?.NotificationEmail;

            if ( notificationEmailSettings != null )
            {
                SendFormBuilderNotificationEmail( action, notificationEmailSettings, rockContext, requestContext );
            }
        }

        /// <summary>
        /// Sends the form builder confirmation email.
        /// </summary>
        /// <param name="action">The current action being processed.</param>
        /// <param name="confirmationEmailSettings">The confirmation email settings.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="requestContext">The context describing the current request.</param>
        private static void SendFormBuilderConfirmationEmail( WorkflowAction action, FormConfirmationEmailSettings confirmationEmailSettings, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( confirmationEmailSettings == null || confirmationEmailSettings.Enabled == false )
            {
                return;
            }

            var activity = action.Activity;
            var workflow = activity.Workflow;

            var formConfirmationEmailDestination = confirmationEmailSettings.Destination;

            var mergeFields = requestContext.GetCommonMergeFields();
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", activity );
            mergeFields.Add( "Workflow", workflow );

            // If the RecipientType indicates that we should use the Person or
            // Spouse key. We'll get the attribute from the Workflow.
            // Note: It will only be a Workflow Attribute, not an Action attribute.
            AttributeCache recipientWorkflowAttribute;
            if ( formConfirmationEmailDestination == FormConfirmationEmailDestination.Person )
            {
                recipientWorkflowAttribute = workflow.Attributes.GetValueOrNull( "Person" );
            }
            else if ( formConfirmationEmailDestination == FormConfirmationEmailDestination.Spouse )
            {
                // If the RecipientType indicates that we should use the Spouse key. We'll get the attribute from the Workflow
                recipientWorkflowAttribute = workflow.Attributes.GetValueOrNull( "Spouse" );
            }
            else
            {
                var recipientAttributeGuid = confirmationEmailSettings.RecipientAttributeGuid;
                recipientWorkflowAttribute = recipientAttributeGuid.HasValue
                    ? AttributeCache.Get( recipientAttributeGuid.Value )
                    : null;
            }

            if ( recipientWorkflowAttribute == null )
            {
                // Unable to to determine Recipient Attribute
                return;
            }

            var recipients = new List<RockMessageRecipient>();
            var recipientWorkflowAttributeValue = action.GetWorkflowAttributeValue( recipientWorkflowAttribute.Guid );

            if ( recipientWorkflowAttribute.FieldTypeId == FieldTypeCache.GetId( Rock.SystemGuid.FieldType.PERSON.AsGuid() ) )
            {
                var personAliasGuid = recipientWorkflowAttributeValue.AsGuid();

                if ( !personAliasGuid.IsEmpty() )
                {
                    var recipientPerson = new PersonAliasService( rockContext ).GetPerson( personAliasGuid );
                    if ( recipientPerson != null && !string.IsNullOrWhiteSpace( recipientPerson.Email ) )
                    {
                        recipients.Add( new RockEmailMessageRecipient( recipientPerson, mergeFields ) );
                    }
                }
            }
            else
            {
                // If this isn't a Person, assume it is an email address.
                string recipientEmailAddress = recipientWorkflowAttributeValue;
                recipients.Add( RockEmailMessageRecipient.CreateAnonymous( recipientEmailAddress, mergeFields ) );
            }

            SendFormBuilderCommunication( confirmationEmailSettings.Source, recipients, mergeFields );
        }

        /// <summary>
        /// Sends the form builder notification email.
        /// </summary>
        /// <param name="action">The workflow aciton being processed.</param>
        /// <param name="notificationEmailSettings">The notification email settings.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="requestContext">The context describing the current request.</param>
        private static void SendFormBuilderNotificationEmail( WorkflowAction action, FormNotificationEmailSettings notificationEmailSettings, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( notificationEmailSettings == null || notificationEmailSettings.Enabled == false )
            {
                return;
            }

            var activity = action.Activity;
            var workflow = activity.Workflow;
            var formNotificationEmailDestination = notificationEmailSettings.Destination;

            var mergeFields = requestContext.GetCommonMergeFields();
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", activity );
            mergeFields.Add( "Workflow", workflow );

            var recipients = new List<RockMessageRecipient>();

            if ( formNotificationEmailDestination == FormNotificationEmailDestination.EmailAddress
                && notificationEmailSettings.EmailAddress.IsNotNullOrWhiteSpace() )
            {
                var recipientEmailAddresses = notificationEmailSettings.EmailAddress.Replace( ";", "," ).Split( ',' );

                foreach ( var eachRcipient in recipientEmailAddresses )
                {
                    recipients.Add( RockEmailMessageRecipient.CreateAnonymous( eachRcipient, mergeFields ) );
                }
            }
            else if ( formNotificationEmailDestination == FormNotificationEmailDestination.SpecificIndividual
                && notificationEmailSettings.RecipientAliasId.HasValue )
            {
                var recipientPerson = new PersonAliasService( rockContext ).GetPerson( notificationEmailSettings.RecipientAliasId.Value );
                if ( recipientPerson == null )
                {
                    return;
                }

                recipients.Add( new RockEmailMessageRecipient( recipientPerson, mergeFields ) );
            }
            else if ( formNotificationEmailDestination == FormNotificationEmailDestination.CampusTopic
                && notificationEmailSettings.CampusTopicValueId.HasValue )
            {
                var workflowCampusId = workflow?.CampusId;
                if ( workflowCampusId.HasValue )
                {
                    var campusTopicEmail = new CampusTopicService( rockContext ).Queryable()
                        .Where( a => a.TopicTypeValueId == notificationEmailSettings.CampusTopicValueId.Value
                            && a.CampusId == workflowCampusId )
                        .Select( a => a.Email )
                        .FirstOrDefault();

                    if ( campusTopicEmail.IsNullOrWhiteSpace() )
                    {
                        return;
                    }

                    recipients.Add( RockEmailMessageRecipient.CreateAnonymous( campusTopicEmail, mergeFields ) );
                }
            }
            else
            {
                return;
            }

            SendFormBuilderCommunication( notificationEmailSettings.Source, recipients, mergeFields );
        }

        /// <summary>
        /// Sends a form builder communication.
        /// </summary>
        /// <param name="formEmailSourceSettings">The form email source settings.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="mergeFields">The merge fields to use when resolving lava content.</param>
        private static void SendFormBuilderCommunication( FormEmailSourceSettings formEmailSourceSettings, List<RockMessageRecipient> recipients, Dictionary<string, object> mergeFields )
        {
            if ( formEmailSourceSettings.Type == FormEmailSourceType.UseTemplate && formEmailSourceSettings.SystemCommunicationId.HasValue )
            {
                var systemCommunication = new SystemCommunicationService( new RockContext() ).Get( formEmailSourceSettings.SystemCommunicationId.Value );
                if ( systemCommunication != null )
                {
                    var emailMessage = new RockEmailMessage( systemCommunication );
                    emailMessage.SetRecipients( recipients );
                    emailMessage.Send();
                }
            }
            else if ( formEmailSourceSettings.Type == FormEmailSourceType.Custom )
            {
                string customBody;
                if ( formEmailSourceSettings.AppendOrgHeaderAndFooter )
                {
                    var globalEmailHeader = "{{ 'Global' | Attribute:'EmailHeader' }}";
                    var globalEmailFooter = "{{ 'Global' | Attribute:'EmailFooter' }}";

                    customBody = $@"
{globalEmailHeader}
{formEmailSourceSettings.Body}
{globalEmailFooter}
";
                }
                else
                {
                    customBody = formEmailSourceSettings.Body;
                }

                var emailMessage = new RockEmailMessage
                {
                    ReplyToEmail = formEmailSourceSettings.ReplyTo,
                    Subject = formEmailSourceSettings.Subject,
                    Message = customBody?.ResolveMergeFields( mergeFields )
                };

                emailMessage.SetRecipients( recipients );
                emailMessage.Send();
            }
        }

        #endregion
    }
}
