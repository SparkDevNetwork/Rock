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
using System.Reflection;

using Rock.Attribute;
using Rock.Blocks.WorkFlow.FormBuilder;
using Rock.ClientService.Core.DefinedValue;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Security;
using Rock.SystemKey;
using Rock.ViewModels.Blocks.WorkFlow.FormBuilder;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Workflow.FormBuilder;

namespace Rock.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// Edits the details of a workflow Form Builder action.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Form Builder Detail" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Edits the details of a workflow Form Builder action." )]
    [IconCssClass( "fa fa-hammer" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

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

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.OBSIDIAN_FORM_BUILDER_DETAIL_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B" )]
    public class FormBuilderDetail : RockBlockType
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
            var workflowTypeId = RequestContext.GetPageParameter( PageParameterKey.WorkflowTypeId ).AsIntegerOrNull();

            // Build the basic view model information required to edit a
            // form.
            var viewModel = new FormBuilderDetailViewModel
            {
                SubmissionsPageUrl = this.GetLinkedPageUrl( AttributeKey.SubmissionsPage, RequestContext.GetPageParameters() ),
                AnalyticsPageUrl = this.GetLinkedPageUrl( AttributeKey.AnalyticsPage, RequestContext.GetPageParameters() ),
                Sources = GetOptionSources( RockContext )
            };

            // If we have a workflow type specified in the query string then
            // load the information from it to populate the form.
            if ( workflowTypeId.HasValue )
            {
                var formBuilderEntityTypeId = EntityTypeCache.Get( typeof( Rock.Workflow.Action.FormBuilder ) ).Id;
                var workflowType = new WorkflowTypeService( RockContext ).Get( workflowTypeId.Value );

                if ( workflowType != null && workflowType.IsFormBuilder && workflowType.Category.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    var actionForm = workflowType.ActivityTypes
                        .SelectMany( a => a.ActionTypes )
                        .Where( a => a.EntityTypeId == formBuilderEntityTypeId )
                        .FirstOrDefault();

                    if ( actionForm != null && actionForm.WorkflowForm != null )
                    {
                        viewModel.FormGuid = workflowType.Guid;
                        viewModel.Form = GetFormSettingsViewModel( actionForm.WorkflowForm, workflowType, RockContext );
                        viewModel.OtherAttributes = GetOtherAttributes( workflowType.Id, RockContext );
                    }
                }
            }

            return viewModel;
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
                actionForm.PersonEntryGroupLocationTypeValueId = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueId( formSettings.PersonEntry.AddressType );
                actionForm.PersonEntryAutofillCurrentPerson = formSettings.PersonEntry.AutofillCurrentPerson;
                actionForm.PersonEntryBirthdateEntryOption = formSettings.PersonEntry.Birthdate.ToPersonEntryOption();
                actionForm.PersonEntryCampusStatusValueId = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueId( formSettings.PersonEntry.CampusStatus );
                actionForm.PersonEntryCampusTypeValueId = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueId( formSettings.PersonEntry.CampusType );
                actionForm.PersonEntryConnectionStatusValueId = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueId( formSettings.PersonEntry.ConnectionStatus );
                actionForm.PersonEntryEmailEntryOption = formSettings.PersonEntry.Email.ToPersonEntryOption();
                actionForm.PersonEntryGenderEntryOption = formSettings.PersonEntry.Gender.ToPersonEntryOption();
                actionForm.PersonEntryHideIfCurrentPersonKnown = formSettings.PersonEntry.HideIfCurrentPersonKnown;
                actionForm.PersonEntryMaritalStatusEntryOption = formSettings.PersonEntry.MaritalStatus.ToPersonEntryOption();
                actionForm.PersonEntryMobilePhoneEntryOption = formSettings.PersonEntry.MobilePhone.ToPersonEntryOption();
                actionForm.PersonEntrySmsOptInEntryOption = formSettings.PersonEntry.SmsOptIn.ToShowHideOption();
                actionForm.PersonEntryRecordStatusValueId = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueId( formSettings.PersonEntry.RecordStatus );
                actionForm.PersonEntryCampusIsVisible = formSettings.PersonEntry.ShowCampus;
                actionForm.PersonEntrySpouseEntryOption = formSettings.PersonEntry.SpouseEntry.ToPersonEntryOption();
                actionForm.PersonEntrySpouseLabel = formSettings.PersonEntry.SpouseLabel;
                actionForm.PersonEntryRaceEntryOption = formSettings.PersonEntry.RaceEntry.ToPersonEntryOption();
                actionForm.PersonEntryEthnicityEntryOption = formSettings.PersonEntry.EthnicityEntry.ToPersonEntryOption();

                // Add the PersonEntry Settings in the Additional Settings.
                actionForm.SetAdditionalSettings( new PersonEntryAdditionalSettings
                {
                    IncludeInactiveCampus = formSettings.PersonEntry.IncludeInactiveCampus
                } );
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


            // If no attributes have been added yet then we need to take into account the attributes auto added when a form builder workflow is created.
            if ( nextAttributeOrder == 0 )
            {
                var defaultAttributesMaxOrder = attributeService.Queryable().Where( a => a.EntityTypeQualifierValue == workflowType.Id.ToString() && a.EntityTypeQualifierColumn == "WorkflowTypeId" ).Select( m => m.Order ).Max();
                nextAttributeOrder += defaultAttributesMaxOrder;
            }


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

                var allFormFields = formSettings.Sections.SelectMany( s => s.Fields ).ToList();

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

                    var formSection = AddOrUpdateFormSection( actionForm, workflowType, attributeService, formAttributeService, formSectionService, section, allFormFields, ref nextAttributeOrder );
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
        private static WorkflowActionFormSection AddOrUpdateFormSection( WorkflowActionForm actionForm, WorkflowType workflowType, AttributeService attributeService, WorkflowActionFormAttributeService formAttributeService, WorkflowActionFormSectionService formSectionService, FormSectionViewModel section, List<FormFieldViewModel> formFields, ref int nextAttributeOrder )
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
            formSection.SectionTypeValueId = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueId( section.Type );
            formSection.SectionVisibilityRules = section.VisibilityRule?.FromViewModel( formFields );

            // Loop through all fields that need to be either added or updated.
            for ( int fieldOrder = 0; fieldOrder < section.Fields.Count; fieldOrder++ )
            {
                var field = section.Fields[fieldOrder];

                var formField = AddOrUpdateFormField( actionForm, workflowType, attributeService, formAttributeService, formSection, field, formFields, ref nextAttributeOrder );
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
        private static WorkflowActionFormAttribute AddOrUpdateFormField( WorkflowActionForm actionForm, WorkflowType workflowType, AttributeService attributeService, WorkflowActionFormAttributeService formAttributeService, WorkflowActionFormSection formSection, FormFieldViewModel field, List<FormFieldViewModel> formFields, ref int nextAttributeOrder )
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
            var configurationValues = fieldType.Field.GetPrivateConfigurationValues( field.ConfigurationValues );

            // Update all the standard properties.
            formField.ActionFormSection = formSection;
            formField.Attribute.DefaultValue = fieldType.Field.GetPrivateEditValue( field.DefaultValue, configurationValues );
            formField.Attribute.Description = field.Description;
            formField.Attribute.IsRequired = field.IsRequired;
            formField.IsRequired = field.IsRequired;
            formField.Attribute.IsGridColumn = field.IsShowOnGrid;
            formField.Attribute.Key = field.Key;
            formField.Attribute.Name = field.Name;
            formField.ColumnSize = field.Size;
            formField.IsVisible = true;
            formField.HideLabel = field.IsHideLabel;
            formField.FieldVisibilityRules = field.VisibilityRule?.FromViewModel( formFields );

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
                var personEntryAdditionalSettings = actionForm.GetAdditionalSettings<PersonEntryAdditionalSettings>();

                form.AllowPersonEntry = true;
                form.PersonEntry = new FormPersonEntryViewModel
                {
                    Address = actionForm.PersonEntryAddressEntryOption.ToFormFieldVisibility(),
                    AddressType = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueGuid( actionForm.PersonEntryGroupLocationTypeValueId ),
                    AutofillCurrentPerson = actionForm.PersonEntryAutofillCurrentPerson,
                    Birthdate = actionForm.PersonEntryBirthdateEntryOption.ToFormFieldVisibility(),
                    CampusStatus = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueGuid( actionForm.PersonEntryCampusStatusValueId ),
                    CampusType = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueGuid( actionForm.PersonEntryCampusTypeValueId ),
                    ConnectionStatus = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueGuid( actionForm.PersonEntryConnectionStatusValueId ),
                    Email = actionForm.PersonEntryEmailEntryOption.ToFormFieldVisibility(),
                    Gender = actionForm.PersonEntryGenderEntryOption.ToFormFieldVisibility(),
                    HideIfCurrentPersonKnown = actionForm.PersonEntryHideIfCurrentPersonKnown,
                    MaritalStatus = actionForm.PersonEntryMaritalStatusEntryOption.ToFormFieldVisibility(),
                    MobilePhone = actionForm.PersonEntryMobilePhoneEntryOption.ToFormFieldVisibility(),
                    SmsOptIn = actionForm.PersonEntrySmsOptInEntryOption.ToFormFieldShowHide(),
                    RecordStatus = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueGuid( actionForm.PersonEntryRecordStatusValueId ),
                    ShowCampus = actionForm.PersonEntryCampusIsVisible,
                    IncludeInactiveCampus = personEntryAdditionalSettings?.IncludeInactiveCampus ?? true,
                    SpouseEntry = actionForm.PersonEntrySpouseEntryOption.ToFormFieldVisibility(),
                    SpouseLabel = actionForm.PersonEntrySpouseLabel,
                    RaceEntry = actionForm.PersonEntryRaceEntryOption.ToFormFieldVisibility(),
                    EthnicityEntry = actionForm.PersonEntryEthnicityEntryOption.ToFormFieldVisibility(),
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
                viewModel.Category = new ListItemBag
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
                    Type = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetDefinedValueGuid( workflowFormSection.SectionTypeValueId ),
                    VisibilityRule = workflowFormSection.SectionVisibilityRules?.ToViewModel()
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
                    var universalFieldTypeGuidAttribute = fieldType?.Field?.GetType().GetCustomAttribute<UniversalFieldTypeGuidAttribute>();

                    if ( fieldType == null || fieldType.Field == null )
                    {
                        continue;
                    }

                    sectionViewModel.Fields.Add( new FormFieldViewModel
                    {
                        ConfigurationValues = fieldType.Field.GetPublicConfigurationValues( attribute.ConfigurationValues, Field.ConfigurationValueUsage.Configure, null ),
                        EditConfigurationValues = fieldType.Field.GetPublicConfigurationValues( attribute.ConfigurationValues, Field.ConfigurationValueUsage.Edit, null ),
                        DefaultValue = fieldType.Field.GetPublicEditValue( attribute.DefaultValue, attribute.ConfigurationValues ),
                        Description = attribute.Description,
                        FieldTypeGuid = fieldType.Guid,
                        UniversalFieldTypeGuid = universalFieldTypeGuidAttribute?.Guid,
                        Guid = attribute.Guid,
                        IsHideLabel = formAttribute.HideLabel,
                        IsRequired = attribute.IsRequired,
                        IsShowOnGrid = attribute.IsGridColumn,
                        Key = attribute.Key,
                        Name = attribute.Name,
                        Size = formAttribute.ColumnSize ?? 12,
                        VisibilityRule = formAttribute.FieldVisibilityRules?.ToViewModel()
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
                    Svg = f.IconSvg,
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
                EmailTemplateOptions = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetEmailTemplateOptions( rockContext, RequestContext ),
                FormTemplateOptions = GetFormTemplateOptions( rockContext ),

                // This is a one-off because we need to get access to a custom
                // attribute on the Defined Value.
                SectionTypeOptions = DefinedTypeCache.Get( SystemGuid.DefinedType.SECTION_TYPE.AsGuid() )
                    .DefinedValues
                    .Where( v => v.IsActive )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value,
                        Category = v.GetAttributeValue( "CSSClass" )
                    } )
                    .ToList(),

                // Default section type is the "No Style" value.
                DefaultSectionType = "85CA07EE-6888-43FD-B8BF-24E4DD35C725".AsGuid()
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

            var formBuilderEntityTypeId = EntityTypeCache.Get( typeof( Rock.Workflow.Action.FormBuilder ) ).Id;
            var workflowType = new WorkflowTypeService( RockContext ).Get( formGuid );

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

            UpdateWorkflowType( formSettings, actionForm.WorkflowForm, workflowType, RockContext );

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the field filter sources that relate to the specified form fields.
        /// </summary>
        /// <param name="formFields">The form fields that need to be represented as filter sources.</param>
        /// <returns>A response that contains the list of <see cref="FieldFilterSourceBag"/> objects.</returns>
        [BlockAction]
        public BlockActionResult GetFilterSources( List<FormFieldViewModel> formFields )
        {
            var fieldFilterSources = new List<FieldFilterSourceBag>();

            foreach ( var field in formFields )
            {
                var fieldType = FieldTypeCache.Get( field.FieldTypeGuid );
                var universalFieldTypeGuidAttribute = fieldType?.Field?.GetType().GetCustomAttribute<UniversalFieldTypeGuidAttribute>();

                // If the field type or its C# component could not be found then
                // we abort with a hard error. We need it to convert data.
                if ( fieldType == null || fieldType.Field == null )
                {
                    throw new Exception( $"Field type '{field.FieldTypeGuid}' not found." );
                }

                // Convert the attribute configuration into values that can be used
                // for filtering a value.
                var privateConfigurationValues = fieldType.Field.GetPrivateConfigurationValues( field.ConfigurationValues );
                var publicConfigurationValues = fieldType.Field.GetPublicConfigurationValues( privateConfigurationValues, Field.ConfigurationValueUsage.Edit, null );

                /*
                 * Daniel Hazelbaker - 3/17/2022
                 * 
                 * This should be removed once the WorkflowEntry block has been converted
                 * into Obsidian. This filters out any form fields whose field type does
                 * not support change notification in WebForms.
                 */
                if ( !fieldType.IsWebFormChangeNotificationSupported( privateConfigurationValues ) )
                {
                    continue;
                }

                var source = new FieldFilterSourceBag
                {
                    Guid = field.Guid,
                    Type = 0,
                    Attribute = new PublicAttributeBag
                    {
                        AttributeGuid = field.Guid,
                        ConfigurationValues = publicConfigurationValues,
                        Description = field.Description,
                        FieldTypeGuid = universalFieldTypeGuidAttribute?.Guid ?? field.FieldTypeGuid,
                        Name = field.Name
                    }
                };

                fieldFilterSources.Add( source );
            }

            return ActionOk( fieldFilterSources );
        }

        /// <summary>
        /// Gets the edit configuration values for the public admin configuration
        /// values. This allows the UI to refresh the preview fields as the user
        /// is making changes to the configuration values.
        /// </summary>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="configurationValues">The admin configuration values.</param>
        /// <returns>The edit configuration values.</returns>
        [BlockAction]
        public BlockActionResult GetEditConfigurationValues( Guid fieldTypeGuid, Dictionary<string, string> configurationValues )
        {
            var fieldType = FieldTypeCache.Get( fieldTypeGuid );

            // If the field type or its C# component could not be found then
            // we abort with a hard error. We need it to convert data.
            if ( fieldType == null || fieldType.Field == null )
            {
                return ActionBadRequest( $"Field type '{fieldTypeGuid}' not found." );
            }

            var privateConfigurationValues = fieldType.Field.GetPrivateConfigurationValues( configurationValues );

            var publicEditConfigurationValues = fieldType.Field.GetPublicConfigurationValues( privateConfigurationValues, ConfigurationValueUsage.Edit, null );

            return ActionOk( publicEditConfigurationValues );
        }

        #endregion
    }
}
