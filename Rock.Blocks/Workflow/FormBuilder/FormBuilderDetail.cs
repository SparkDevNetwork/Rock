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
using System.Linq;

using Rock.Attribute;
using Rock.ClientService.Core.DefinedValue;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemKey;
using Rock.ViewModel.Blocks.Workflow.FormBuilder;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;
using Rock.Workflow.FormBuilder;

namespace Rock.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// Edits the details of a workflow Form Builder action.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Form Builder Detail" )]
    [Category( "Obsidian > Workflow > FormBuilder" )]
    [Description( "Edits the details of a workflow Form Builder action." )]
    [IconCssClass( "fa fa-hammer" )]

    #region Block Attributes

    [LinkedPage( "Submissions Page",
        Description = "The page that contains the Submissions block to view submissions existing submissions for this form.",
        IsRequired = true,
        Key = AttributeKey.SubmissionsPage,
        Order = 0 )]

    [LinkedPage( "Analytics Page",
        Description = "The page that contains the Analytics block to view statistics on existing submissions for this form.",
        IsRequired = true,
        Key = AttributeKey.AnalyticsPage,
        Order = 1 )]

    #endregion

    public class FormBuilderDetail : RockObsidianBlockType
    {
        private static class PageParameterKey
        {
            public const string WorkflowTypeId = "WorkflowTypeId";
        }

        private static class AttributeKey
        {
            public const string SubmissionsPage = "SubmissionsPage";

            public const string AnalyticsPage = "AnalyticsPage";
        }

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var workflowTypeId = RequestContext.GetPageParameter( PageParameterKey.WorkflowTypeId ).AsIntegerOrNull();

                // Build the basic view model information required to edit a
                // form.
                var viewModel = new FormBuilderDetailViewModel
                {
                    SubmissionsPageUrl = this.GetLinkedPageUrl( AttributeKey.SubmissionsPage, RequestContext.GetPageParameters() ),
                    AnalyticsPageUrl = this.GetLinkedPageUrl( AttributeKey.AnalyticsPage, RequestContext.GetPageParameters() ),
                    Sources = GetOptionSources( rockContext )
                };

                // If we have a workflow type specified in the query string then
                // load the information from it to populate the form.
                if ( workflowTypeId.HasValue )
                {
                    var formBuilderEntityTypeId = EntityTypeCache.Get( typeof( Rock.Workflow.Action.FormBuilder ) ).Id;
                    var workflowType = new WorkflowTypeService( rockContext ).Get( workflowTypeId.Value );

                    if ( workflowType != null && workflowType.IsFormBuilder )
                    {
                        var actionForm = workflowType.ActivityTypes
                            .SelectMany( a => a.ActionTypes )
                            .Where( a => a.EntityTypeId == formBuilderEntityTypeId )
                            .FirstOrDefault();

                        if ( actionForm != null && actionForm.WorkflowForm != null )
                        {
                            viewModel.FormGuid = workflowType.Guid;
                            viewModel.Form = GetFormSettingsViewModel( actionForm.WorkflowForm, workflowType, rockContext );
                            viewModel.OtherAttributes = GetOtherAttributes( workflowType.Id, rockContext );
                        }
                    }
                }

                return viewModel;
            }
        }

        /// <summary>
        /// Updates a workflow type with values from the view model.
        /// </summary>
        /// <param name="formSettings">The view model that contains the settings to be updated.</param>
        /// <param name="actionForm">The <see cref="WorkflowActionForm"/> to be updated.</param>
        /// <param name="workflowType">The <see cref="WorkflowType"/> to be updated.</param>
        /// <param name="rockContext">The database context to use for data lookups.</param>
        private static void UpdateWorkflowType( FormSettingsViewModel formSettings, WorkflowActionForm actionForm, WorkflowType workflowType, RockContext rockContext )
        {
            actionForm.Header = formSettings.HeaderContent;
            actionForm.Footer = formSettings.FooterContent;

            actionForm.AllowPersonEntry = formSettings.AllowPersonEntry;
            if ( formSettings.AllowPersonEntry )
            {
                actionForm.PersonEntryAddressEntryOption = formSettings.PersonEntry.Address.ToPersonEntryOption();
                actionForm.PersonEntryGroupLocationTypeValueId = Utility.GetDefinedValueId( formSettings.PersonEntry.AddressType );
                actionForm.PersonEntryAutofillCurrentPerson = formSettings.PersonEntry.AutofillCurrentPerson;
                actionForm.PersonEntryBirthdateEntryOption = formSettings.PersonEntry.Birthdate.ToPersonEntryOption();
                actionForm.PersonEntryCampusStatusValueId = Utility.GetDefinedValueId( formSettings.PersonEntry.CampusStatus );
                actionForm.PersonEntryCampusTypeValueId = Utility.GetDefinedValueId( formSettings.PersonEntry.CampusType );
                actionForm.PersonEntryConnectionStatusValueId = Utility.GetDefinedValueId( formSettings.PersonEntry.ConnectionStatus );
                actionForm.PersonEntryEmailEntryOption = formSettings.PersonEntry.Email.ToPersonEntryOption();
                actionForm.PersonEntryGenderEntryOption = formSettings.PersonEntry.Gender.ToPersonEntryOption();
                actionForm.PersonEntryHideIfCurrentPersonKnown = formSettings.PersonEntry.HideIfCurrentPersonKnown;
                actionForm.PersonEntryMaritalStatusEntryOption = formSettings.PersonEntry.MaritalStatus.ToPersonEntryOption();
                actionForm.PersonEntryMobilePhoneEntryOption = formSettings.PersonEntry.MobilePhone.ToPersonEntryOption();
                actionForm.PersonEntryRecordStatusValueId = Utility.GetDefinedValueId( formSettings.PersonEntry.RecordStatus );
                actionForm.PersonEntryCampusIsVisible = formSettings.PersonEntry.ShowCampus;
                actionForm.PersonEntrySpouseEntryOption = formSettings.PersonEntry.SpouseEntry.ToPersonEntryOption();
                actionForm.PersonEntrySpouseLabel = formSettings.PersonEntry.SpouseLabel;
            }

            UpdateFormSections( formSettings, actionForm, workflowType, rockContext );

            // Update the workflow type properties.
            workflowType.Name = formSettings.General.Name.Trim();
            workflowType.Description = formSettings.General.Description.Trim();
            workflowType.FormStartDateTime = formSettings.General.EntryStarts?.DateTime;
            workflowType.FormEndDateTime = formSettings.General.EntryEnds?.DateTime;
            workflowType.IsLoginRequired = formSettings.General.IsLoginRequired;
            workflowType.CategoryId = CategoryCache.GetId( formSettings.General.Category.Value.AsGuid() ).Value;

            // Update the workflow type form template.
            if ( formSettings.General.Template.HasValue )
            {
                workflowType.FormBuilderTemplateId = new WorkflowFormBuilderTemplateService( rockContext )
                    .GetId( formSettings.General.Template.Value );
            }
            else
            {
                workflowType.FormBuilderTemplateId = null;
            }

            // Update the settings stored in the JSON blob.
            var settings = workflowType.FormBuilderSettingsJson.FromJsonOrNull<Rock.Workflow.FormBuilder.FormSettings>() ?? new Rock.Workflow.FormBuilder.FormSettings();

            settings.CompletionAction = formSettings.Completion.FromViewModel();
            settings.ConfirmationEmail = formSettings.ConfirmationEmail.FromViewModel( rockContext );
            settings.NotificationEmail = formSettings.NotificationEmail.FromViewModel( rockContext );
            settings.CampusSetFrom = formSettings.CampusSetFrom?.FromViewModel();

            workflowType.FormBuilderSettingsJson = settings.ToJson();
        }

        /// <summary>
        /// Handles logic for updating all the form sections as well as attributes.
        /// This will create any new items and delete any removed items as well.
        /// </summary>
        /// <param name="formSettings">The form settings that contain all the configuration information.</param>
        /// <param name="actionForm">The <see cref="WorkflowActionForm"/> entity being updated.</param>
        /// <param name="workflowType">The <see cref="WorkflowType"/> that is being updated.</param>
        /// <param name="rockContext">The database context to operate in.</param>
        private static void UpdateFormSections( FormSettingsViewModel formSettings, WorkflowActionForm actionForm, WorkflowType workflowType, RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            var formAttributeService = new WorkflowActionFormAttributeService( rockContext );
            var formSectionService = new WorkflowActionFormSectionService( rockContext );

            var nextAttributeOrder = actionForm.FormAttributes != null && actionForm.FormAttributes.Any()
                ? actionForm.FormAttributes.Select( a => a.Order ).Max() + 1
                : 0;

            if ( formSettings.Sections != null )
            {
                // Get all the section identifiers that are sticking around.
                var allValidSectionGuids = formSettings.Sections
                    .Select( s => s.Guid )
                    .ToList();

                // Get all the attribute identifiers that are sticking around.
                var allValidAttributeGuids = formSettings.Sections
                    .SelectMany( s => s.Fields )
                    .Select( f => f.Guid )
                    .ToList();

                // Find all sections that no longer exist in this form.
                var sectionsToDelete = actionForm.FormSections
                    .Where( s => !allValidSectionGuids.Contains( s.Guid ) )
                    .ToList();

                // Find all form attributes that no longer exist in this form.
                var formAttributesToDelete = actionForm.FormAttributes
                    .Where( a => !allValidAttributeGuids.Contains( a.Attribute.Guid ) )
                    .ToList();

                // Delete all sections that no longer exist in this form.
                sectionsToDelete.ForEach( s =>
                {
                    formSectionService.Delete( s );
                } );

                // Delete all form attributes that no longer exist in this form.
                formAttributesToDelete.ForEach( a =>
                {
                    if ( a.Attribute.IsSystem )
                    {
                        attributeService.Delete( a.Attribute );
                    }
                    formAttributeService.Delete( a );
                } );

                // Loop through all sections that need to be either added or updated.
                for ( int sectionOrder = 0; sectionOrder < formSettings.Sections.Count; sectionOrder++ )
                {
                    var section = formSettings.Sections[sectionOrder];

                    var formSection = AddOrUpdateFormSection( actionForm, workflowType, attributeService, formAttributeService, formSectionService, section, ref nextAttributeOrder );
                    formSection.Order = sectionOrder;
                }
            }
            else
            {
                // Remove all form attributes and sections.
                var nonUserAttributes = actionForm.FormAttributes
                    .Select( a => a.Attribute )
                    .Where( a => a.IsSystem )
                    .ToList();

                attributeService.DeleteRange( nonUserAttributes );
                formAttributeService.DeleteRange( actionForm.FormAttributes );
                formSectionService.DeleteRange( actionForm.FormSections );
            }
        }

        /// <summary>
        /// Adds a new form section or updates an existing form section in the
        /// <paramref name="actionForm"/>. This also handles the creation of
        /// any new form attributes as needed.
        /// </summary>
        /// <param name="actionForm">The <see cref="WorkflowActionForm"/> being updated.</param>
        /// <param name="workflowType">The <see cref="WorkflowType"/> being updated.</param>
        /// <param name="attributeService">The database service that provides access to the attributes.</param>
        /// <param name="formAttributeService">The database service that provides access to the form attributes.</param>
        /// <param name="formSectionService">The database service that provides access to the form sections.</param>
        /// <param name="section">The section view model that will be used as the source of information.</param>
        /// <param name="nextAttributeOrder">The next attribute order number to use when creating a new attribute.</param>
        /// <returns>The <see cref="WorkflowActionFormSection"/> entity that was either updated or created.</returns>
        private static WorkflowActionFormSection AddOrUpdateFormSection( WorkflowActionForm actionForm, WorkflowType workflowType, AttributeService attributeService, WorkflowActionFormAttributeService formAttributeService, WorkflowActionFormSectionService formSectionService, FormSectionViewModel section, ref int nextAttributeOrder )
        {
            var formSection = actionForm.FormSections.FirstOrDefault( s => s.Guid == section.Guid );

            // If the section was not found then create a new one and add it
            // to the form.
            if ( formSection == null )
            {
                formSection = new WorkflowActionFormSection
                {
                    Guid = section.Guid
                };
                actionForm.FormSections.Add( formSection );
                formSectionService.Add( formSection );
            }

            // Update the standard section properties from the view model.
            formSection.Description = section.Description;
            formSection.Title = section.Title;
            formSection.ShowHeadingSeparator = section.ShowHeadingSeparator;
            formSection.SectionTypeValueId = Utility.GetDefinedValueId( section.Type );

            // Loop through all fields that need to be either added or updated.
            for ( int fieldOrder = 0; fieldOrder < section.Fields.Count; fieldOrder++ )
            {
                var field = section.Fields[fieldOrder];

                var formField = AddOrUpdateFormField( actionForm, workflowType, attributeService, formAttributeService, formSection, field, ref nextAttributeOrder );
                formField.Order = fieldOrder;
            }

            return formSection;
        }

        /// <summary>
        /// Adds a new form field or updates an existing form field in the form.
        /// This will also handle creating the attribute if a new form field must
        /// be created.
        /// </summary>
        /// <param name="actionForm">The <see cref="WorkflowActionForm"/> being updated.</param>
        /// <param name="workflowType">The <see cref="WorkflowType"/> being updated.</param>
        /// <param name="attributeService">The database service that provides access to attributes.</param>
        /// <param name="formAttributeService">The database service that provides access to form attributes.</param>
        /// <param name="formSection">The <see cref="WorkflowActionFormSection"/> being updated.</param>
        /// <param name="field">The field view model that contains the source information.</param>
        /// <param name="nextAttributeOrder">The next attribute Order value to use when adding a new attribute.</param>
        /// <returns>The <see cref="WorkflowActionFormAttribute"/> that was either created or updated.</returns>
        private static WorkflowActionFormAttribute AddOrUpdateFormField( WorkflowActionForm actionForm, WorkflowType workflowType, AttributeService attributeService, WorkflowActionFormAttributeService formAttributeService, WorkflowActionFormSection formSection, FormFieldViewModel field, ref int nextAttributeOrder )
        {
            var fieldType = FieldTypeCache.Get( field.FieldTypeGuid );

            // If the field type or its C# component could not be found then
            // we abort with a hard error. We need it to convert data.
            if ( fieldType == null || fieldType.Field == null )
            {
                throw new Exception( $"Field type '{field.FieldTypeGuid}' not found." );
            }

            var formField = actionForm.FormAttributes.FirstOrDefault( a => a.Attribute.Guid == field.Guid );

            // If the form field was not found then create a new attribute and
            // new form field.
            if ( formField == null )
            {
                var attribute = new Rock.Model.Attribute
                {
                    Guid = field.Guid,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Workflow>().Id,
                    EntityTypeQualifierColumn = nameof( Rock.Model.Workflow.WorkflowTypeId ),
                    EntityTypeQualifierValue = workflowType.Id.ToString(),
                    FieldTypeId = fieldType.Id,
                    IsSystem = true,
                    Order = nextAttributeOrder++
                };

                formField = new WorkflowActionFormAttribute
                {
                    Attribute = attribute
                };

                actionForm.FormAttributes.Add( formField );
                attributeService.Add( attribute );
                formAttributeService.Add( formField );
            }

            // Convert the attribute configuration into values that can be stored
            // in the database.
            var configurationValues = fieldType.Field.GetPrivateConfigurationOptions( field.ConfigurationValues );

            // Update all the standard properties.
            formField.ActionFormSection = formSection;
            formField.Attribute.DefaultValue = fieldType.Field.GetPrivateEditValue( field.DefaultValue, configurationValues );
            formField.Attribute.Description = field.Description;
            formField.Attribute.IsRequired = field.IsRequired;
            formField.Attribute.IsGridColumn = field.IsShowOnGrid;
            formField.Attribute.Key = field.Key;
            formField.Attribute.Name = field.Name;
            formField.ColumnSize = field.Size;
            formField.HideLabel = field.IsHideLabel;

            // Add or update the attribute qualifiers. Do not delete any old ones.
            foreach ( var kvp in configurationValues )
            {
                var qualifier = formField.Attribute.AttributeQualifiers.FirstOrDefault( q => q.Key == kvp.Key );

                if ( qualifier == null )
                {
                    formField.Attribute.AttributeQualifiers.Add( new AttributeQualifier
                    {
                        IsSystem = false,
                        Key = kvp.Key,
                        Value = kvp.Value ?? string.Empty
                    } );
                }
                else
                {
                    qualifier.Value = kvp.Value ?? string.Empty;
                }
            }

            return formField;
        }

        /// <summary>
        /// Gets the view model that represents a workflow form builder form in
        /// its entirety.
        /// </summary>
        /// <param name="actionForm">The <see cref="WorkflowActionForm"/> to be represented by the view model.</param>
        /// <param name="workflowType">The <see cref="WorkflowType"/> to be represented by the view model.</param>
        /// <param name="rockContext">The database context used for data lookup.</param>
        /// <returns>A view model that represents the form.</returns>
        private static FormSettingsViewModel GetFormSettingsViewModel( WorkflowActionForm actionForm, WorkflowType workflowType, RockContext rockContext )
        {
            var settings = workflowType.FormBuilderSettingsJson.FromJsonOrNull<Rock.Workflow.FormBuilder.FormSettings>();

            var form = new FormSettingsViewModel
            {
                HeaderContent = actionForm.Header,
                FooterContent = actionForm.Footer,
                General = GetFormGeneralViewModel( workflowType ),
                Sections = GetFormSectionViewModels( actionForm ),
                ConfirmationEmail = settings?.ConfirmationEmail.ToViewModel( rockContext ),
                NotificationEmail = settings?.NotificationEmail.ToViewModel( rockContext ),
                Completion = settings?.CompletionAction.ToViewModel(),
                CampusSetFrom = settings?.CampusSetFrom?.ToViewModel()
            };

            // Build the person entry settings.
            if ( actionForm.AllowPersonEntry )
            {
                form.AllowPersonEntry = true;
                form.PersonEntry = new FormPersonEntryViewModel
                {
                    Address = actionForm.PersonEntryAddressEntryOption.ToFormFieldVisibility(),
                    AddressType = Utility.GetDefinedValueGuid( actionForm.PersonEntryGroupLocationTypeValueId ),
                    AutofillCurrentPerson = actionForm.PersonEntryAutofillCurrentPerson,
                    Birthdate = actionForm.PersonEntryBirthdateEntryOption.ToFormFieldVisibility(),
                    CampusStatus = Utility.GetDefinedValueGuid( actionForm.PersonEntryCampusStatusValueId ),
                    CampusType = Utility.GetDefinedValueGuid( actionForm.PersonEntryCampusTypeValueId ),
                    ConnectionStatus = Utility.GetDefinedValueGuid( actionForm.PersonEntryConnectionStatusValueId ),
                    Email = actionForm.PersonEntryEmailEntryOption.ToFormFieldVisibility(),
                    Gender = actionForm.PersonEntryGenderEntryOption.ToFormFieldVisibility(),
                    HideIfCurrentPersonKnown = actionForm.PersonEntryHideIfCurrentPersonKnown,
                    MaritalStatus = actionForm.PersonEntryMaritalStatusEntryOption.ToFormFieldVisibility(),
                    MobilePhone = actionForm.PersonEntryMobilePhoneEntryOption.ToFormFieldVisibility(),
                    RecordStatus = Utility.GetDefinedValueGuid( actionForm.PersonEntryRecordStatusValueId ),
                    ShowCampus = actionForm.PersonEntryCampusIsVisible,
                    SpouseEntry = actionForm.PersonEntrySpouseEntryOption.ToFormFieldVisibility(),
                    SpouseLabel = actionForm.PersonEntrySpouseLabel
                };
            }

            return form;
        }

        /// <summary>
        /// Gets the view model that represents the general settings of a form.
        /// These are roughly equivalent to what you would be editing on a
        /// standard detail page.
        /// </summary>
        /// <param name="workflowType">The <see cref="WorkflowType"/> that will be represented.</param>
        /// <returns>A view model that represents the general settings of the form.</returns>
        private static FormGeneralViewModel GetFormGeneralViewModel( WorkflowType workflowType )
        {
            var viewModel = new FormGeneralViewModel
            {
                Description = workflowType.Description,
                EntryEnds = workflowType.FormEndDateTime,
                EntryStarts = workflowType.FormStartDateTime,
                IsLoginRequired = workflowType.IsLoginRequired,
                Name = workflowType.Name,
                Template = workflowType.FormBuilderTemplate?.Guid
            };

            if ( workflowType.Category != null )
            {
                viewModel.Category = new ListItemViewModel
                {
                    Value = workflowType.Category.Guid.ToString(),
                    Text = workflowType.Category.Name
                };
            }

            return viewModel;
        }

        /// <summary>
        /// Gets the list of view models that represent all the sections and fields
        /// of a form.
        /// </summary>
        /// <param name="actionForm">The <see cref="WorkflowActionForm"/> to be represented.</param>
        /// <returns>A collection of view models that represent that sections that will be edited.</returns>
        private static List<FormSectionViewModel> GetFormSectionViewModels( WorkflowActionForm actionForm )
        {
            var sectionViewModels = new List<FormSectionViewModel>();

            foreach ( var workflowFormSection in actionForm.FormSections.OrderBy( s => s.Order ) )
            {
                // Create the basic section view model.
                var sectionViewModel = new FormSectionViewModel
                {
                    Guid = workflowFormSection.Guid,
                    Description = workflowFormSection.Description,
                    Fields = new List<FormFieldViewModel>(),
                    ShowHeadingSeparator = workflowFormSection.ShowHeadingSeparator,
                    Title = workflowFormSection.Title,
                    Type = Utility.GetDefinedValueGuid( workflowFormSection.SectionTypeValueId )
                };

                // Get all the form attributes for this section.
                var formAttributes = actionForm.FormAttributes
                    .Where( a => a.ActionFormSectionId == workflowFormSection.Id )
                    .ToList();

                // Loop through each form attribute and make a view model out of it.
                foreach ( var formAttribute in formAttributes.OrderBy( a => a.Order ) )
                {
                    var attribute = AttributeCache.Get( formAttribute.AttributeId );
                    var fieldType = FieldTypeCache.Get( attribute.FieldTypeId );

                    if ( fieldType == null || fieldType.Field == null )
                    {
                        continue;
                    }

                    sectionViewModel.Fields.Add( new FormFieldViewModel
                    {
                        ConfigurationValues = fieldType.Field.GetPublicConfigurationValues( attribute.ConfigurationValues ),
                        DefaultValue = fieldType.Field.GetPublicEditValue( attribute.DefaultValue, attribute.ConfigurationValues ),
                        Description = attribute.Description,
                        FieldTypeGuid = fieldType.Guid,
                        Guid = attribute.Guid,
                        IsHideLabel = formAttribute.HideLabel,
                        IsRequired = attribute.IsRequired,
                        IsShowOnGrid = attribute.IsGridColumn,
                        Key = attribute.Key,
                        Name = attribute.Name,
                        Size = formAttribute.ColumnSize ?? 12
                    } );
                }

                sectionViewModels.Add( sectionViewModel );
            }

            return sectionViewModels;
        }

        /// <summary>
        /// Gets all the field types that can be used while building the form.
        /// This represents the list of field types that can be dragged from
        /// the left panel onto the main form sections.
        /// </summary>
        /// <returns>The list of field type view models.</returns>
        private static List<FormFieldTypeViewModel> GetAvailableFieldTypes()
        {
            return FieldTypeCache.All()
                .Where( f => f.Platform.HasFlag( Rock.Utility.RockPlatform.Obsidian )
                    && ( f.Usage.HasFlag( Field.FieldTypeUsage.Common ) || f.Usage.HasFlag( Field.FieldTypeUsage.Advanced ) ) )
                .Select( f => new FormFieldTypeViewModel
                {
                    Guid = f.Guid,
                    Text = f.Name,
                    Icon = f.IconCssClass,
                    IsCommon = f.Usage.HasFlag( Field.FieldTypeUsage.Common )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets a list of other attributes that can be used in the form.
        /// </summary>
        /// <param name="workflowTypeId">The identifier of the workflow type used to load user attributes.</param>
        /// <param name="rockContext">The database context to execute queries in.</param>
        /// <returns>A list of <see cref="FormOtherAttributeViewModel"/> view models that represent the attributes.</returns>
        private static List<FormOtherAttributeViewModel> GetOtherAttributes( int workflowTypeId, RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            var workflowEntityTypeId = EntityTypeCache.GetId<Rock.Model.Workflow>();

            return attributeService.Queryable()
                .Where( a => a.EntityTypeId == workflowEntityTypeId
                    && a.EntityTypeQualifierColumn == nameof( Rock.Model.Workflow.WorkflowTypeId )
                    && a.EntityTypeQualifierValue == workflowTypeId.ToString()
                    && (
                        !a.IsSystem
                        || a.Key == WorkflowFormBuilderKey.Person
                        || a.Key == WorkflowFormBuilderKey.Spouse
                        || a.Key == WorkflowFormBuilderKey.Family
                       )
                   )
                .Select( a => new FormOtherAttributeViewModel
                {
                    Guid = a.Guid,
                    FieldTypeGuid = a.FieldType.Guid,
                    Name = a.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets all the options sources and lists of things the individual can
        /// select from when building the form.
        /// </summary>
        /// <param name="rockContext">The database context to use for data lookups.</param>
        /// <returns>A view model that represents all the options.</returns>
        private FormValueSourcesViewModel GetOptionSources( RockContext rockContext )
        {
            var definedValueClientService = new DefinedValueClientService( rockContext, RequestContext.CurrentPerson );

            return new FormValueSourcesViewModel
            {
                FieldTypes = GetAvailableFieldTypes(),
                CampusStatusOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.CAMPUS_STATUS.AsGuid() ),
                CampusTopicOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.TOPIC_TYPE.AsGuid() ),
                CampusTypeOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid() ),
                AddressTypeOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() ),
                ConnectionStatusOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ),
                RecordStatusOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid() ),
                EmailTemplateOptions = Utility.GetEmailTemplateOptions( rockContext, RequestContext ),
                FormTemplateOptions = GetFormTemplateOptions( rockContext ),

                SectionTypeOptions = new List<ListItemViewModel>()
                {
                    new ListItemViewModel
                    {
                        Value = Guid.NewGuid().ToString(),
                        Text = "Plain",
                        Category = ""
                    },
                    new ListItemViewModel
                    {
                        Value = Guid.NewGuid().ToString(),
                        Text = "Well",
                        Category = "well"
                    }
                }
            };
        }

        /// <summary>
        /// Get the form template list item options that can be selected in the
        /// settings UI.
        /// </summary>
        /// <param name="rockContext">The database context used for data queries.</param>
        /// <returns>A list of <see cref="FormTemplateListItemViewModel"/> objects that represent the available templates.</returns>
        private List<FormTemplateListItemViewModel> GetFormTemplateOptions( RockContext rockContext )
        {
            return new WorkflowFormBuilderTemplateService( rockContext ).Queryable()
                .Where( t => t.IsActive )
                .ToList()
                .Where( t => t.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .Select( t => new FormTemplateListItemViewModel
                {
                    Value = t.Guid.ToString(),
                    Text = t.Name,
                    FormHeader = t.FormHeader,
                    FormFooter = t.FormFooter,
                    IsCompletionActionConfigured = t.CompletionSettingsJson != null,
                    IsConfirmationEmailConfigured = t.ConfirmationEmailSettingsJson?.FromJsonOrNull<FormConfirmationEmailSettings>()?.Enabled ?? false,
                    IsLoginRequiredConfigured = t.IsLoginRequired,
                    IsPersonEntryConfigured = t.AllowPersonEntry
                } )
                .ToList();
        }

        #endregion

        #region Block Action

        /// <summary>
        /// Saves the changes made to the specified workflow type (form).
        /// </summary>
        /// <param name="formGuid">The unique identifier of the form to be updated.</param>
        /// <param name="formSettings">The view model representation of the form data.</param>
        /// <returns>Action result that specifies if the save was successful.</returns>
        [BlockAction]
        public BlockActionResult SaveForm( Guid formGuid, FormSettingsViewModel formSettings )
        {
            if ( formGuid == Guid.Empty || formSettings == null )
            {
                return ActionBadRequest( "Invalid parameters provided." );
            }

            using ( var rockContext = new RockContext() )
            {
                var formBuilderEntityTypeId = EntityTypeCache.Get( typeof( Rock.Workflow.Action.FormBuilder ) ).Id;
                var workflowType = new WorkflowTypeService( rockContext ).Get( formGuid );

                if ( workflowType == null || !workflowType.IsFormBuilder )
                {
                    return ActionBadRequest( "Specified workflow type is not a form builder." );
                }

                // Find the action type that represents the form.
                var actionForm = workflowType.ActivityTypes
                    .SelectMany( a => a.ActionTypes )
                    .Where( a => a.EntityTypeId == formBuilderEntityTypeId )
                    .FirstOrDefault();

                if ( actionForm == null || actionForm.WorkflowForm == null )
                {
                    return ActionBadRequest( "Specified workflow is not properly configured for use in form builder." );
                }

                UpdateWorkflowType( formSettings, actionForm.WorkflowForm, workflowType, rockContext );

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}
