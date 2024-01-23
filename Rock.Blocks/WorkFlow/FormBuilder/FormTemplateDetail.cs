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
using Rock.Blocks.WorkFlow.FormBuilder;
using Rock.ClientService.Core.DefinedValue;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormTemplateDetail;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// Shows the details of a FormBuilder template.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Form Template Detail" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows the details of a FormBuilder template." )]
    [IconCssClass( "fa fa-align-left" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.OBSIDIAN_FORM_TEMPLATE_DETAIL_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "A522F0A4-39D4-4047-A012-EF42F7D2759D")]
    public class FormTemplateDetail : RockBlockType
    {
        private static class PageParameterKey
        {
            public const string FormTemplateId = "FormTemplateId";
        }

        private static class AttributeKey
        {
        }

        /// <summary>
        /// The unique identifier to identify the "Person" attribute for the
        /// confirmation e-mail. This will be translated to an enum value on
        /// save.
        /// </summary>
        private static Guid RecipientPersonGuid = new Guid( "00000000-0000-0000-0000-000000000001" );

        /// <summary>
        /// The unique identifier to identify the "Spouse" attribute for the
        /// confirmation e-mail. This will be translated to an enum value on
        /// save.
        /// </summary>
        private static Guid RecipientSpouseGuid = new Guid( "00000000-0000-0000-0000-000000000002" );

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
                EmailTemplateOptions = Rock.Blocks.WorkFlow.FormBuilder.Utility.GetEmailTemplateOptions( rockContext, RequestContext ),
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
                .Select( t => new ListItemBag
                {
                    Value = t.Guid.ToString(),
                    Text = t.Name
                } )
                .ToList();

            return new TemplateDetailViewModel
            {
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
            var formConfirmationEmail = template.ConfirmationEmailSettingsJson?.FromJsonOrNull<Rock.Workflow.FormBuilder.FormConfirmationEmailSettings>();
            var confirmationEmail = formConfirmationEmail?.ToViewModel( rockContext );

            // Special logic to translate the enum values into values that can
            // be used by the recipient picker.
            if ( formConfirmationEmail.Destination == Rock.Workflow.FormBuilder.FormConfirmationEmailDestination.Person )
            {
                confirmationEmail.RecipientAttributeGuid = RecipientPersonGuid;
            }
            else if ( formConfirmationEmail.Destination == Rock.Workflow.FormBuilder.FormConfirmationEmailDestination.Spouse )
            {
                confirmationEmail.RecipientAttributeGuid = RecipientSpouseGuid;
            }

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
                ConfirmationEmail = confirmationEmail,
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
                var isNew = false;
                var templateService = new WorkflowFormBuilderTemplateService( rockContext );
                WorkflowFormBuilderTemplate formTemplate;

                // If we are provided an empty guid (not null) then we treat that
                // to mean create a new template.
                if ( guid == Guid.Empty )
                {
                    formTemplate = new WorkflowFormBuilderTemplate();
                    templateService.Add( formTemplate );
                    isNew = true;
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
                formTemplate.CompletionSettingsJson = template.CompletionAction?.FromViewModel().ToJson();

                var confirmationEmail = template.ConfirmationEmail?.FromViewModel( rockContext );

                // Special check for template logic. We don't have attributes yet
                // so check if the confirmation e-mail uses the special values to
                // indicate which attribute to be used at runtime. If we find one
                // of those special values then translate it to the enum.
                if ( confirmationEmail != null )
                {
                    if ( confirmationEmail.RecipientAttributeGuid == RecipientPersonGuid )
                    {
                        confirmationEmail.Destination = Rock.Workflow.FormBuilder.FormConfirmationEmailDestination.Person;
                        confirmationEmail.RecipientAttributeGuid = null;
                    }
                    else if ( confirmationEmail.RecipientAttributeGuid == RecipientSpouseGuid )
                    {
                        confirmationEmail.Destination = Rock.Workflow.FormBuilder.FormConfirmationEmailDestination.Spouse;
                        confirmationEmail.RecipientAttributeGuid = null;
                    }
                }

                formTemplate.ConfirmationEmailSettingsJson = confirmationEmail?.ToJson();

                rockContext.SaveChanges();

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        ["FormTemplateId"] = formTemplate.Id.ToString()
                    } ) );
                }

                // Ensure navigation properties will work now.
                formTemplate = templateService.Get( formTemplate.Id );

                return ActionOk( GetTemplateDetailViewModel( formTemplate, rockContext ) );
            }
        }

        #endregion
    }
}
