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
using Rock.Model;

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
    public class ElectronicSignature : ActionComponent
    {
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
    }
}