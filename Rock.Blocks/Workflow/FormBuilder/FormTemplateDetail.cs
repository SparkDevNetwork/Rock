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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.ClientService.Core.DefinedValue;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModel.Blocks.Workflow.FormBuilder.FormTemplateDetail;
using Rock.ViewModel.NonEntities;

namespace Rock.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// Shows the details of a FormBuilder template.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Form Template Detail" )]
    [Category( "Workflow > FormBuilder" )]
    [Description( "Shows the details of a FormBuilder template." )]
    [IconCssClass( "fa fa-align-left" )]

    #region Block Attributes

    #endregion

    public class FormTemplateDetail : RockObsidianBlockType
    {
        private static class PageParameterKey
        {
            public const string FormTemplateId = "FormTemplateId";
        }

        private static class AttributeKey
        {
        }

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateId = RequestContext.GetPageParameter( PageParameterKey.FormTemplateId ).AsIntegerOrNull();

                // Build the basic view model information required to edit a
                // form template.
                var viewModel = new FormTemplateDetailConfiguration
                {
                    Sources = GetOptionSources( rockContext ),
                    ParentUrl = this.GetParentPageUrl()
                };

                // If we have a template specified in the query string then
                // load the information from it to populate the form.
                if ( templateId.HasValue && templateId.Value != 0 )
                {
                    var template = new WorkflowFormBuilderTemplateService( rockContext ).Get( templateId.Value );

                    if ( template != null && template.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        viewModel.TemplateGuid = template.Guid;
                        viewModel.Template = GetTemplateDetailViewModel( template, rockContext );
                        viewModel.IsEditable = template.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
                    }
                    else
                    {
                        // null means "not found or some other error prevented viewing".
                        viewModel.TemplateGuid = null;
                    }
                }
                else
                {
                    // Guid.Empty means "create a new template".
                    viewModel.TemplateGuid = Guid.Empty;
                    viewModel.IsEditable = true;
                }

                return viewModel;
            }
        }

        /// <summary>
        /// Gets all the options sources and lists of things the individual can
        /// select from when building the form.
        /// </summary>
        /// <param name="rockContext">The database context to use for data lookups.</param>
        /// <returns>A view model that represents all the options.</returns>
        private ValueSourcesViewModel GetOptionSources( RockContext rockContext )
        {
            var definedValueClientService = new DefinedValueClientService( rockContext, RequestContext.CurrentPerson );

            return new ValueSourcesViewModel
            {
                CampusStatusOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.CAMPUS_STATUS.AsGuid() ),
                CampusTypeOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid() ),
                AddressTypeOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() ),
                ConnectionStatusOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ),
                RecordStatusOptions = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid() ),
                EmailTemplateOptions = Utility.GetEmailTemplateOptions( rockContext, RequestContext ),
            };
        }

        /// <summary>
        /// Gets the read-only details about the template that will be used to
        /// display the template to the individual before editing.
        /// </summary>
        /// <param name="template">The form template to be represented by the view model.</param>
        /// <param name="rockContext">The database context to use for data queries.</param>
        /// <returns>A <see cref="TemplateDetailViewModel"/> that represents the form template.</returns>
        private TemplateDetailViewModel GetTemplateDetailViewModel( WorkflowFormBuilderTemplate template, RockContext rockContext )
        {
            // Find all workflow types that reference this form template. Do not
            // perform any security checking since we are only showing the name.
            var usedBy = new WorkflowTypeService( rockContext ).Queryable()
                .Where( t => t.FormBuilderTemplateId.HasValue && t.FormBuilderTemplateId == template.Id )
                .Select( t => new ListItemViewModel
                {
                    Value = t.Guid.ToString(),
                    Text = t.Name
                } )
                .ToList();

            return new TemplateDetailViewModel
            {
                AuditDetails = template.GetAuditDetailViewModel(),
                Description = template.Description,
                IsActive = template.IsActive,
                Name = template.Name,
                UsedBy = usedBy
            };
        }

        /// <summary>
        /// Gets the details that allow editing of a form template. This returns
        /// data that might be considered sensitive and should only be returned
        /// when the individual has edit permissions to the template.
        /// </summary>
        /// <param name="template">The template that will be edited.</param>
        /// <param name="rockContext">The database context to use for data queries.</param>
        /// <returns>A <see cref="TemplateEditDetailViewModel"/> view model that represents the form template.</returns>
        private TemplateEditDetailViewModel GetTemplateEditViewModel( WorkflowFormBuilderTemplate template, RockContext rockContext )
        {
            return new TemplateEditDetailViewModel
            {
                Name = template.Name,
                Description = template.Description,
                IsActive = template.IsActive,
                IsLoginRequired = template.IsLoginRequired,
                FormHeader = template.FormHeader,
                FormFooter = template.FormFooter,
                AllowPersonEntry = template.AllowPersonEntry,
                PersonEntry = template.PersonEntrySettingsJson?.FromJsonOrNull<Rock.Workflow.FormBuilder.FormPersonEntrySettings>().ToViewModel(),
                ConfirmationEmail = template.ConfirmationEmailSettingsJson?.FromJsonOrNull<Rock.Workflow.FormBuilder.FormConfirmationEmailSettings>().ToViewModel( rockContext ),
                CompletionAction = template.CompletionSettingsJson?.FromJsonOrNull<Rock.Workflow.FormBuilder.FormCompletionActionSettings>().ToViewModel()
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Begins an edit operation of an existing form template by generating
        /// a view model representation of the template that contains all the
        /// properties that can be updated.
        /// </summary>
        /// <param name="guid">The unique identifier of the template to be edited.</param>
        /// <returns>A <see cref="TemplateEditDetailViewModel">view model</see> representation of the template.</returns>
        [BlockAction]
        public BlockActionResult StartEdit( Guid guid )
        {
            using ( var rockContext = new RockContext() )
            {
                var template = new WorkflowFormBuilderTemplateService( rockContext ).Get( guid );

                // Verify the template exists and the individual has the correct
                // permissions to make the edits.
                if ( template == null || !template.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( "Invalid template." );
                }

                return ActionOk( GetTemplateEditViewModel( template, rockContext ) );
            }
        }

        /// <summary>
        /// Saves the changes to the template into the database and return a
        /// new view model representation of the template.
        /// </summary>
        /// <param name="guid">The unique identifier of the existing template or an all zero Guid to create a new template.</param>
        /// <param name="template">The details about the template to be created/updated.</param>
        /// <returns>A new view model representation of the template or an error response.</returns>
        [BlockAction]
        public BlockActionResult SaveTemplate( Guid guid, TemplateEditDetailViewModel template )
        {
            using ( var rockContext = new RockContext() )
            {
                var templateService = new WorkflowFormBuilderTemplateService( rockContext );
                WorkflowFormBuilderTemplate formTemplate;

                // If we are provided an empty guid (not null) then we treat that
                // to mean create a new template.
                if ( guid == Guid.Empty )
                {
                    formTemplate = new WorkflowFormBuilderTemplate();
                    templateService.Add( formTemplate );
                }
                else
                {
                    formTemplate = templateService.Get( guid );
                }

                // Perform a check to see if we now have a valid template object
                // and that the user has permission to edit/create it.
                if ( formTemplate == null || !formTemplate.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( "Invalid template." );
                }

                // Store all the template details.
                formTemplate.Name = template.Name.Trim();
                formTemplate.Description = template.Description?.Trim();
                formTemplate.IsActive = template.IsActive;
                formTemplate.IsLoginRequired = template.IsLoginRequired;
                formTemplate.FormHeader = template.FormHeader?.Trim();
                formTemplate.FormFooter = template.FormFooter?.Trim();
                formTemplate.AllowPersonEntry = template.AllowPersonEntry;
                formTemplate.PersonEntrySettingsJson = template.PersonEntry?.FromViewModel().ToJson();
                formTemplate.ConfirmationEmailSettingsJson = template.ConfirmationEmail?.FromViewModel( rockContext ).ToJson();
                formTemplate.CompletionSettingsJson = template.CompletionAction?.FromViewModel().ToJson();

                rockContext.SaveChanges();

                // Ensure navigation properties will work now.
                formTemplate = templateService.Get( formTemplate.Id );

                return ActionOk( GetTemplateDetailViewModel( formTemplate, rockContext ) );
            }
        }

        #endregion
    }
}
