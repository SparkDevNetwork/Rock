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

using Rock.Attribute;
using Rock.Data;
using Rock.ElectronicSignature;
using Rock.Enums.Workflow;
using Rock.Model;
using Rock.Net;
using Rock.Pdf;
using Rock.ViewModels.Workflow;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Allows for electronic signing a document based on a workflow template." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Electronic Signature" )]

    [SignatureDocumentTemplateField(
        "Signature Document Template",
        Description = "The template to use for the signature document.",
        Key = AttributeKey.SignatureDocumentTemplate,
        ShowTemplatesThatHaveExternalProviders = false,
        IsRequired = false,
        Order = 1 )]

    [WorkflowTextOrAttribute( "Signature Document Template Id or Guid",
        "Signature Document Template Attribute",
        Description = "The Id or Guid of a signature document template to use for the signature document. If a Signature Document Template is specified above, this setting will be ignored.",
        Key = AttributeKey.SignatureDocumentTemplateIdOrGuid,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.SignatureDocumentTemplateFieldType" },
        IsRequired = false,
        Order = 2 )]

    [WorkflowAttribute(
        "Applies to Person",
        Description = "The attribute that represents the person that the document applies to.",
        Key = AttributeKey.AppliesToPersonAlias,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.TextFieldType" },
        Order = 3 )]

    [WorkflowAttribute(
        "Assigned To Person",
        Description = "The attribute that represents the person that will be signing the document. This is only needed if the signature will be completed via an email",
        Key = AttributeKey.AssignedToPersonAlias,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.TextFieldType" },
        Order = 4 )]

    [WorkflowAttribute(
        "Signed by Person",
        Description = "The attribute that represents the person that signed the document. If a person is logged in that person will override this value.",
        Key = AttributeKey.SignedByPersonAlias,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType", "Rock.Field.Types.TextFieldType" },
        Order = 5 )]

    [WorkflowAttribute(
        "Signature Document",
        Description = "The workflow attribute to place the PDF document in.",
        Key = AttributeKey.SignatureDocument,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.BinaryFileFieldType" },
        Order = 6 )]

    [TextField(
        "Signature Document Name",
        Description = "The name to use for the new document that is created. <span class='tip tip-lava'></span>",
        Key = AttributeKey.SignatureDocumentName,
        IsRequired = true,
        Order = 7 )]

    [Rock.SystemGuid.EntityTypeGuid( "41491689-00BD-49A1-A3CD-A59FBBD2B2F8" )]
    public class ElectronicSignature : ActionComponent, IInteractiveAction
    {
        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string SignatureDocumentTemplate = "SignatureDocumentTemplate";
            public const string SignatureDocumentTemplateIdOrGuid = "SignatureDocumentTemplateIdOrGuid";
            public const string AppliesToPersonAlias = "AppliesToPersonAlias";
            public const string AssignedToPersonAlias = "AssignedToPersonAlias";
            public const string SignedByPersonAlias = "SignedByPersonAlias";
            public const string SignatureDocument = "SignatureDocument";
            public const string SignatureDocumentName = "SignatureDocumentName";
        }

        private static class ComponentConfigurationKey
        {
            public const string SignatureType = "signatureType";

            public const string DocumentTerm = "documentTerm";

            public const string SignedByEmail = "signedByEmail";

            public const string LegalName = "legalName";

            public const string SendCopy = "sendCopy";

            public const string ShowName = "showName";

            public const string Content = "content";
        }

        private static class ComponentDataKey
        {
            public const string SignatureData = "signatureData";

            public const string SignedByName = "signedByName";

            public const string SignedByEmail = "signedByEmail";
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

            // Always return false. Special logic for e-Signature will be handled in the WorkflowEntry block
            return false;
        }

        #region IInteractiveAction

        /// <inheritdoc/>
        InteractiveActionResult IInteractiveAction.StartAction( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext )
        {
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

            return new InteractiveActionResult
            {
                IsSuccess = false,
                ProcessingType = InteractiveActionContinueMode.Stop,
                ActionData = GetSignatureFormData( action, rockContext, requestContext )
            };
        }

        /// <inheritdoc/>
        InteractiveActionResult IInteractiveAction.UpdateAction( WorkflowAction action, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
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

            if ( !CompleteDocumentSigning( action, componentData, rockContext, requestContext, out var errorMessage ) )
            {
                return new InteractiveActionResult
                {
                    IsSuccess = true,
                    ProcessingType = InteractiveActionContinueMode.Continue,
                    ActionData = new InteractiveActionDataBag
                    {
                        Message = new InteractiveMessageBag
                        {
                            Type = InteractiveMessageType.Error,
                            Content = errorMessage
                        }
                    }
                };
            }

            return new InteractiveActionResult
            {
                IsSuccess = true,
                ProcessingType = InteractiveActionContinueMode.Continue,
                ActionData = new InteractiveActionDataBag
                {
                    Message = new InteractiveMessageBag
                    {
                        Type = InteractiveMessageType.Information,
                        Content = "Your signature has been submitted successfully."
                    }
                }
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the data to be sent to a UI component in order to display.
        /// the entry form.
        /// </summary>
        /// <param name="action">The action that represents the signature form to display.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that identifies the current request, must never be <c>null</c>.</param>
        /// <returns>An dictionary of strings that represents the form to display.</returns>
        private InteractiveActionDataBag GetSignatureFormData( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext )
        {
            var template = GetSignatureDocumentTemplate( rockContext, action );

            if ( template == null )
            {
                return new InteractiveActionDataBag
                {
                    Message = new InteractiveMessageBag
                    {
                        Type = InteractiveMessageType.Error,
                        Content = "Signature document was not found, please check workflow configuration."
                    }
                };
            }

            var signedByEmail = string.Empty;
            var legalName = string.Empty;

            var signedByPersonAliasId = GetSignedByPersonAliasId( rockContext, action, requestContext.CurrentPerson?.PrimaryAliasId );
            if ( signedByPersonAliasId.HasValue )
            {
                var signedByPerson = new PersonAliasService( rockContext ).GetPerson( signedByPersonAliasId.Value );

                // Set some sane default values.
                signedByEmail = signedByPerson?.Email;
                legalName = signedByPerson?.FullName;
            }

            // If not logged-in or the Workflow hasn't specified a SignedByPerson, show the name that was typed when on the Completion step
            // TODO: escElectronicSignatureControl.ShowNameOnCompletionStepWhenInTypedSignatureMode = ( signedByPersonAliasId == null );

            var content = GetDocumentHtml( action, template, requestContext );

            return new InteractiveActionDataBag
            {
                ComponentUrl = requestContext.ResolveRockUrl( "~/Obsidian/Blocks/Workflow/WorkflowEntry/Actions/electronicSignature.obs" ),
                ComponentConfiguration = new Dictionary<string, string>
                {
                    [ComponentConfigurationKey.SignatureType] = template.SignatureType.ConvertToInt().ToString(),
                    [ComponentConfigurationKey.DocumentTerm] = template.DocumentTerm,
                    [ComponentConfigurationKey.SignedByEmail] = signedByEmail,
                    [ComponentConfigurationKey.LegalName] = legalName,
                    [ComponentConfigurationKey.SendCopy] = template.CompletionSystemCommunicationId.HasValue.ToString(),
                    [ComponentConfigurationKey.ShowName] = ( !signedByPersonAliasId.HasValue ).ToString(),
                    [ComponentConfigurationKey.Content] = content
                },
                ComponentData = new Dictionary<string, string>()
            };
        }

        /// <summary>
        /// Complete the document signing by creating the signed document and
        /// sending out any required notifications.
        /// </summary>
        /// <param name="action">The workflow action being processed.</param>
        /// <param name="data">The data from the UI component.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that identifies the current request, must never be <c>null</c>.</param>
        /// <param name="errorMessage">On return contains any error message to be displayed.</param>
        /// <returns><c>true</c> if the operation completed successfully; otherwise <c>false</c>.</returns>
        private bool CompleteDocumentSigning( WorkflowAction action, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext, out string errorMessage )
        {
            var template = GetSignatureDocumentTemplate( rockContext, action );
            if ( template == null )
            {
                errorMessage = "Unable to determine Signature Template.";
                return false;
            }

            var signatureData = data.GetValueOrNull( ComponentDataKey.SignatureData );
            var signedByName = data.GetValueOrNull( ComponentDataKey.SignedByName );
            var signedByEmail = data.GetValueOrNull( ComponentDataKey.SignedByEmail );

            if ( signedByName.IsNullOrWhiteSpace() || signedByEmail.IsNullOrWhiteSpace() )
            {
                errorMessage = "Incomplete signature data received.";
                return false;
            }

            if ( template.SignatureType == SignatureType.Drawn && signatureData.IsNullOrWhiteSpace() )
            {
                errorMessage = "Incomplete signature data received.";
                return false;
            }

            // Get the person that will be signing the document.
            var signedByPersonAliasId = GetSignedByPersonAliasId( rockContext, action, requestContext.CurrentPerson?.PrimaryAliasId );
            var signedByPerson = signedByPersonAliasId.HasValue
                ? new PersonAliasService( rockContext ).GetPerson( signedByPersonAliasId.Value )
                : null;

            // Get the person the document applies to.
            var appliesToPersonAliasId = GetAppliesToPersonAliasId( rockContext, action );

            var content = GetDocumentHtml( action, template, requestContext );
            var signatureDocumentName = GetSignatureDocumentName( action, template, requestContext );
            var assignedToPersonAliasId = GetAssignedToPersonAliasId( rockContext, action );

            // Create the document.
            var document = rockContext.Set<SignatureDocument>().Create();
            document.SignatureDocumentTemplateId = template.Id;
            document.Status = SignatureDocumentStatus.Signed;
            document.Name = signatureDocumentName;
            document.EntityTypeId = EntityTypeCache.Get<Model.Workflow>( true, rockContext ).Id;
            document.EntityId = action.Activity.Workflow.Id;
            document.SignedByPersonAliasId = signedByPersonAliasId;
            document.AssignedToPersonAliasId = assignedToPersonAliasId;
            document.AppliesToPersonAliasId = appliesToPersonAliasId;
            document.SignedDocumentText = content;
            document.LastStatusDate = RockDateTime.Now;
            document.SignedDateTime = RockDateTime.Now;
            document.SignatureData = signatureData;
            document.SignedName = signedByName;
            document.SignedByEmail = signedByEmail;
            document.SignedClientIp = requestContext.ClientInformation.IpAddress;
            document.SignedClientUserAgent = requestContext.ClientInformation.UserAgent;

            // This needs to be done before we generate the signed PDF.
            document.SignatureVerificationHash = SignatureDocumentService.CalculateSignatureVerificationHash( document );

            // Generate the PDF, if an error happened return the error message.
            var pdfFile = CreateSignedDocumentPdf( template, document, signedByPerson, rockContext, out errorMessage );

            if ( pdfFile == null )
            {
                return false;
            }

            // Save the signed document to the database.
            document.BinaryFileId = pdfFile.Id;
            new SignatureDocumentService( rockContext ).Add( document );
            rockContext.SaveChanges();

            SaveSignatureDocumentValuesToAttributes( rockContext, action, document );

            if ( template.CompletionSystemCommunication != null )
            {
                ElectronicSignatureHelper.SendSignatureCompletionCommunication( document.Id, out _ );
            }

            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Get the HTML that represents the document to be signed after any
        /// lava merging has been completed.
        /// </summary>
        /// <param name="action">The workflow action being processed.</param>
        /// <param name="template">The template representing the document to be signed.</param>
        /// <param name="requestContext">The context that identifies the current request, must never be <c>null</c>.</param>
        /// <returns>A string of HTML that should be displayed in an IFrame.</returns>
        private string GetDocumentHtml( WorkflowAction action, SignatureDocumentTemplate template, RockRequestContext requestContext )
        {
            var mergeFields = requestContext.GetCommonMergeFields();
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", action.Activity );
            mergeFields.Add( "Workflow", action.Activity.Workflow );

            return template.LavaTemplate?.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the name of the document that will be saved to the database.
        /// </summary>
        /// <param name="action">The workflow action being processed.</param>
        /// <param name="template">The template representing the document to be signed.</param>
        /// <param name="requestContext">The context that identifies the current request, must never be <c>null</c>.</param>
        /// <returns>The name to assign to <see cref="SignatureDocument.Name"/>.</returns>
        private string GetSignatureDocumentName( WorkflowAction action, SignatureDocumentTemplate template, RockRequestContext requestContext )
        {
            var mergeFields = requestContext.GetCommonMergeFields();
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", action.Activity );
            mergeFields.Add( "Workflow", action.Activity.Workflow );
            mergeFields.Add( "SignatureDocumentTemplate", template );

            var name = GetSignatureDocumentName( action, mergeFields );

            if ( name.IsNullOrWhiteSpace() )
            {
                return "Signed Document";
            }

            return name;
        }

        /// <summary>
        /// Creates a PDF document that represents the signed document and
        /// then saves it to the database in a <see cref="BinaryFile"/>.
        /// </summary>
        /// <param name="template">The template representing the document to be signed.</param>
        /// <param name="document">The document that has been populated with the signature data.</param>
        /// <param name="signedByPerson">The person that is signing the document, may be <c>null</c>.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="errorMessage">On return, contains any error message to be displayed.</param>
        /// <returns>An instance of <see cref="BinaryFile"/> that has been saved to the database or <c>null</c> on error.</returns>
        private BinaryFile CreateSignedDocumentPdf( SignatureDocumentTemplate template, SignatureDocument document, Person signedByPerson, RockContext rockContext, out string errorMessage )
        {
            var signatureInformationHtmlArgs = new GetSignatureInformationHtmlOptions
            {
                SignatureType = template.SignatureType,
                SignedName = document.SignedName,
                DrawnSignatureDataUrl = document.SignatureData,
                SignedByPerson = signedByPerson,
                SignedDateTime = document.SignedDateTime,
                SignedClientIp = document.SignedClientIp,
                SignatureVerificationHash = document.SignatureVerificationHash
            };

            // Helper takes care of generating HTML and combining SignatureDocumentHTML and signedSignatureDocumentHtml into the final Signed Document
            var signatureInformationHtml = ElectronicSignatureHelper.GetSignatureInformationHtml( signatureInformationHtmlArgs );
            var signedSignatureDocumentHtml = ElectronicSignatureHelper.GetSignedDocumentHtml( document.SignedDocumentText, signatureInformationHtml );

            // PDF Generator to BinaryFile
            BinaryFile pdfFile;
            try
            {
                using ( var pdfGenerator = new PdfGenerator() )
                {
                    var binaryFileTypeId = template.BinaryFileTypeId
                        ?? BinaryFileTypeCache.GetId( Rock.SystemGuid.BinaryFiletype.DIGITALLY_SIGNED_DOCUMENTS.AsGuid() );

                    pdfFile = pdfGenerator.GetAsBinaryFileFromHtml( binaryFileTypeId ?? 0, document.Name, signedSignatureDocumentHtml );
                }
            }
            catch ( PdfGeneratorException pdfGeneratorException )
            {
                ExceptionLogService.LogException( pdfGeneratorException );
                errorMessage = pdfGeneratorException.Message;
                return null;
            }

            pdfFile.IsTemporary = false;
            new BinaryFileService( rockContext ).Add( pdfFile );
            rockContext.SaveChanges();

            errorMessage = null;

            return pdfFile;
        }

        /// <summary>
        /// Gets the signature document template.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflowAction">The workflow action.</param>
        /// <returns>SignatureDocumentTemplate.</returns>
        public SignatureDocumentTemplate GetSignatureDocumentTemplate( RockContext rockContext, WorkflowAction workflowAction )
        {
            var templateGuid = this.GetAttributeValue( workflowAction, AttributeKey.SignatureDocumentTemplate, true ).AsGuidOrNull();
            if ( templateGuid.HasValue )
            {
                return new SignatureDocumentTemplateService( rockContext ).Get( templateGuid.Value );
            }

            var templateId = this.GetAttributeValue( workflowAction, AttributeKey.SignatureDocumentTemplateIdOrGuid, true ).AsIntegerOrNull();
            if ( templateId.HasValue )
            {
                return new SignatureDocumentTemplateService( rockContext ).Get( templateId.Value );
            }

            templateGuid = this.GetAttributeValue( workflowAction, AttributeKey.SignatureDocumentTemplateIdOrGuid, true ).AsGuidOrNull();
            if ( templateGuid.HasValue )
            {
                return new SignatureDocumentTemplateService( rockContext ).Get( templateGuid.Value );
            }

            return null;
        }

        /// <summary>
        /// Gets the PersonAliasId of the person that should be specified for <see cref="Rock.Model.SignatureDocument.SignedByPersonAliasId"></see>
        /// If <code>CurrentPersonAliasId</code> is known, use CurrentPersonAliasId. Otherwise, if the workflow specified one, use that.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflowAction">The workflow action.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <returns>Person.</returns>
        public int? GetSignedByPersonAliasId( RockContext rockContext, WorkflowAction workflowAction, int? currentPersonAliasId )
        {
            // if CurrentPersonAliasId is known, use that first.
            if ( currentPersonAliasId.HasValue )
            {
                return currentPersonAliasId.Value;
            }

            // if CurrentPerson is null, see if the SignedByPerson was set by the Workflow
            int? personAliasId = null;
            var personAliasGuid = this.GetAttributeValue( workflowAction, AttributeKey.SignedByPersonAlias, true ).AsGuidOrNull();
            if ( personAliasGuid.HasValue )
            {
                personAliasId = new PersonAliasService( rockContext ).GetId( personAliasGuid.Value );
            }

            return personAliasId;
        }

        /// <summary>
        /// Gets the PersonAliasId of the person that should be specified for  <see cref="Rock.Model.SignatureDocument.AppliesToPersonAliasId" ></see>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflowAction">The workflow action.</param>
        /// <returns>Person.</returns>
        public int? GetAppliesToPersonAliasId( RockContext rockContext, WorkflowAction workflowAction )
        {
            int? personAliasId = null;
            var personAliasGuid = this.GetAttributeValue( workflowAction, AttributeKey.AppliesToPersonAlias, true ).AsGuidOrNull();
            if ( personAliasGuid.HasValue )
            {
                personAliasId = new PersonAliasService( rockContext ).GetId( personAliasGuid.Value );
            }

            return personAliasId;
        }

        /// <summary>
        /// Gets the PersonAliasId of the person that should be specified for  <see cref="Rock.Model.SignatureDocument.AssignedToPersonAliasId" ></see>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflowAction">The workflow action.</param>
        /// <returns>Person.</returns>
        public int? GetAssignedToPersonAliasId( RockContext rockContext, WorkflowAction workflowAction )
        {
            int? personAliasId = null;
            var personAliasGuid = this.GetAttributeValue( workflowAction, AttributeKey.AssignedToPersonAlias, true ).AsGuidOrNull();
            if ( personAliasGuid.HasValue )
            {
                personAliasId = new PersonAliasService( rockContext ).GetId( personAliasGuid.Value );
            }

            return personAliasId;
        }

        /// <summary>
        /// Gets the value that should be specified for <see cref="Rock.Model.SignatureDocument.Name" ></see>
        /// </summary>
        /// <param name="workflowAction">The workflow action.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns>System.String.</returns>
        public string GetSignatureDocumentName( WorkflowAction workflowAction, Dictionary<string, object> mergeFields )
        {
            string signatureDocumentName = this.GetAttributeValue( workflowAction, AttributeKey.SignatureDocumentName, true );
            return signatureDocumentName?.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Saves the signature signature document values to the workflow attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflowAction">The workflow action.</param>
        /// <param name="signatureDocument">The signature document.</param>
        public void SaveSignatureDocumentValuesToAttributes( RockContext rockContext, WorkflowAction workflowAction, SignatureDocument signatureDocument )
        {
            var signatureTemplate = new SignatureDocumentTemplateService( rockContext ).Get( signatureDocument.SignatureDocumentTemplateId );
            this.SetWorkflowAttributeValue( workflowAction, AttributeKey.SignatureDocumentTemplate, signatureTemplate?.Guid );

            var personAliasService = new PersonAliasService( rockContext );

            Guid? appliesToPersonAliasGuid = null;
            Guid? assignedToPersonAliasGuid = null;
            Guid? signedByPersonAliasGuid = null;
            if ( signatureDocument.AppliesToPersonAliasId.HasValue )
            {
                appliesToPersonAliasGuid = personAliasService.GetGuid( signatureDocument.AppliesToPersonAliasId.Value );
            }

            if ( signatureDocument.AssignedToPersonAliasId.HasValue )
            {
                assignedToPersonAliasGuid = personAliasService.GetGuid( signatureDocument.AssignedToPersonAliasId.Value );
            }

            if ( signatureDocument.SignedByPersonAliasId.HasValue )
            {
                signedByPersonAliasGuid = personAliasService.GetGuid( signatureDocument.SignedByPersonAliasId.Value );
            }

            if ( signatureDocument.BinaryFileId.HasValue )
            {
                var binaryFileGuid = new BinaryFileService( rockContext ).GetGuid( signatureDocument.BinaryFileId.Value );
                this.SetWorkflowAttributeValue( workflowAction, AttributeKey.SignatureDocument, binaryFileGuid );
            }

            this.SetWorkflowAttributeValue( workflowAction, AttributeKey.AppliesToPersonAlias, appliesToPersonAliasGuid );
            this.SetWorkflowAttributeValue( workflowAction, AttributeKey.AssignedToPersonAlias, assignedToPersonAliasGuid );
            this.SetWorkflowAttributeValue( workflowAction, AttributeKey.SignedByPersonAlias, signedByPersonAliasGuid );

        }

        #endregion
    }
}