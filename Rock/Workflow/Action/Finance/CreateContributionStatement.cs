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
using System.IO;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Creates a contribution statement workflow action.
    /// Implements the <see cref="Rock.Workflow.ActionComponent" />
    /// </summary>
    [ActionCategory( "Finance" )]
    [Description( "Creates a contribution statement for a provided giver." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Create Contribution Statement" )]

    #region Workflow Attributes

    [WorkflowAttribute( "Giver",
        Description = "Workflow attribute that contains the person to create the contribution statement for.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.Giver,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowTextOrAttribute(
        "Start Date",
        "Attribute Value",
        Description = "The start date for the contribution statement. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.StartDateTime,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.DateTimeFieldType", "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute(
        "End Date",
        "Attribute Value",
        Description = "The end date for the contribution statement. If no date is provided the current date time will be used. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.EndDateTime,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.DateTimeFieldType", "Rock.Field.Types.TextFieldType" } )]
    [DocumentTypeField(
        "Document Type",
        Description = "The type of document to store the contribution statement in.",
        IsRequired = true,
        AllowMultiple = false,
        Order = 3,
        Key = AttributeKey.DocumentType )]
    [FinancialStatementTemplateField(
        "Statement Template",
        Description = "The contibution statement template to use for the document.",
        IsRequired = true,
        Order = 4,
        Key = AttributeKey.StatementTemplate
        )]
    [EnumField(
        "Save Statement For",
        Description = "Determines who in the giving group will get the contribution statement. This only applies to individuals who have the same giving group id as the Giver provided. Do not save will be used when you only want the document created, but do not want to save it to a person's record.",
        DefaultEnumValue = 0,
        EnumSourceType = typeof( FinancialStatementGeneratorOptions.FinancialStatementIndividualSaveOptions.FinancialStatementIndividualSaveOptionsSaveFor ),
        IsRequired = true,
        Order = 5,
        Key = AttributeKey.SaveStatementFor )]
    [TextField(
        "Document Purpose Key",
        Description = "The purpose key you provide will be what is shown to individuals to describe what period the statement is for. In most cases this should be the year. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 6,
        Key = AttributeKey.DocumentPurposeKey )]
    [BooleanField(
        "Overwrite Documents with Same Purpose Key",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Description = "Determines if statements with the purpose keys should be overwritten.",
        DefaultBooleanValue = true,
        Order = 7,
        Key = AttributeKey.OverwriteDocumentsWithSamePurposeKey )]
    [WorkflowAttribute(
        "Document",
        Description = "The workflow attribute to place the PDF document in.",
        IsRequired = false,
        Order = 8,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.BinaryFileFieldType" },
        Key = AttributeKey.DocumentAttribute )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "E2C38F27-67DD-4C64-9584-42A552660F7E")]
    public class CreateContributionStatement : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DocumentAttribute = "DocumentAttribute";
            public const string DocumentPurposeKey = "DocumentPurposeKey";
            public const string DocumentType = "DocumentType";
            public const string Giver = "Giver";
            public const string OverwriteDocumentsWithSamePurposeKey = "OverwriteDocumentsWithSamePurposeKey";
            public const string SaveStatementFor = "SaveStatementFor";
            public const string StartDateTime = "StartDateTime";
            public const string EndDateTime = "EndDateTime";
            public const string StatementTemplate = "StatementTemplate";
        }

        #endregion

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var mergeFields = GetMergeFields( action );

            // Get configuration settings.
            var giverGuid = GetAttributeValue( action, AttributeKey.Giver ).AsGuidOrNull();
            var documentTypeId = GetAttributeValue( action, AttributeKey.DocumentType, true ).AsIntegerOrNull();
            var statementTemplateGuid = GetAttributeValue( action, AttributeKey.StatementTemplate, true ).AsGuidOrNull();
            var saveStatementFor = ( FinancialStatementGeneratorOptions.FinancialStatementIndividualSaveOptions.FinancialStatementIndividualSaveOptionsSaveFor ) ( GetAttributeValue( action, AttributeKey.SaveStatementFor, true ).AsIntegerOrNull() ?? 0 );
            var overwriteDocumentsWithSamePurposeKey = GetAttributeValue( action, AttributeKey.OverwriteDocumentsWithSamePurposeKey, true ).AsBoolean();
            var documentGuid = GetAttributeValue( action, AttributeKey.DocumentAttribute ).AsGuidOrNull();

            var startDateTime = GetAttributeValue( action, AttributeKey.StartDateTime, true )
                .ResolveMergeFields( mergeFields )
                .AsDateTime();
            var endDateTime = GetAttributeValue( action, AttributeKey.EndDateTime, true )
                .ResolveMergeFields( mergeFields )
                .AsDateTime();
            var documentPurposeKey = GetAttributeValue( action, AttributeKey.DocumentPurposeKey, true )
                .ResolveMergeFields( mergeFields );

            // Get the target person.
            Person targetPerson = null;
            if ( giverGuid.HasValue )
            {
                var attributePerson = AttributeCache.Get( giverGuid.Value, rockContext );
                if ( attributePerson != null )
                {
                    var attributePersonValue = action.GetWorkflowAttributeValue( giverGuid.Value );
                    if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                    {
                        if ( attributePerson.FieldType.Class == typeof( Rock.Field.Types.PersonFieldType ).FullName )
                        {
                            var personAliasGuid = attributePersonValue.AsGuid();
                            if ( !personAliasGuid.IsEmpty() )
                            {
                                targetPerson = new PersonAliasService( rockContext ).Queryable()
                                    .Where( a => a.Guid.Equals( personAliasGuid ) )
                                    .Select( a => a.Person )
                                    .FirstOrDefault();
                            }
                        }
                        else
                        {
                            errorMessages.Add( "The attribute used to provide the giver was not of type 'Person'." );
                        }
                    }
                }
            }

            if ( targetPerson == null )
            {
                var errorMessage = "Giver must be specified.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            // Get the statement template.
            FinancialStatementTemplate financialStatementTemplate = new FinancialStatementTemplateService( rockContext ).Get( statementTemplateGuid.Value );
            if ( financialStatementTemplate == null )
            {
                var errorMessage = "StatementTemplate must be specified.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            if ( startDateTime == null )
            {
                var errorMessage = "Start Date must be specified.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            // Get the document type.
            var documentType = new DocumentTypeService( rockContext ).GetByIds( new List<int> { documentTypeId.Value } ).FirstOrDefault();

            // Create the Statement save options.
            var individualSaveOptions = new FinancialStatementGeneratorOptions.FinancialStatementIndividualSaveOptions
            {
                DocumentDescription = "",
                DocumentName = "", // Leave this blank to use the Document Type Name template.
                DocumentPurposeKey = documentPurposeKey,
                DocumentSaveFor = saveStatementFor,
                DocumentTypeId = documentType.Id,
                OverwriteDocumentsOfThisTypeWithSamePurposeKey = overwriteDocumentsWithSamePurposeKey,
                SaveStatementsForIndividuals = true
            };

            // Populate the statement options.
            FinancialStatementGeneratorOptions financialStatementGeneratorOptions = new FinancialStatementGeneratorOptions
            {
                StartDate = startDateTime,
                EndDate = endDateTime,
                FinancialStatementTemplateId = financialStatementTemplate.Id,
                IndividualSaveOptions = individualSaveOptions,
                PersonId = targetPerson.Id
            };

            // Populate the recipients, this is determined by the Save For setting.
            var financialStatementGeneratorRecipients = FinancialStatementGeneratorHelper.GetFinancialStatementGeneratorRecipients( financialStatementGeneratorOptions );

            foreach ( var recipient in financialStatementGeneratorRecipients )
            {
                // Process the file upload for the recipient.
                Stream pdfStream = null;

                try
                {
                    // Generate the statement from the specified options.
                    var financialStatementGeneratorRecipientRequest = new FinancialStatementGeneratorRecipientRequest( financialStatementGeneratorOptions )
                    {
                        FinancialStatementGeneratorRecipient = recipient
                    };

                    var result = FinancialStatementGeneratorHelper.GetStatementGeneratorRecipientResult( financialStatementGeneratorRecipientRequest, targetPerson );

                    if ( string.IsNullOrWhiteSpace( result.Html ) )
                    {
                        // Don't generate a statement if no statement HTML.
                        action.AddLogEntry( "The contribution statement could not be generated.", true );

                        // Return true here to allow the action to run 'successfully' without a document. This allows the action to be easily used when the document is optional without a bunch of action filter tests.
                        return true;
                    }

                    // Render the PDF from the HTML result.
                    using ( var pdfGenerator = new Pdf.PdfGenerator() )
                    {
                        if ( result.FooterHtmlFragment.IsNotNullOrWhiteSpace() )
                        {
                            pdfGenerator.FooterHtml = result.FooterHtmlFragment;
                        }

                        pdfStream = pdfGenerator.GetPDFDocumentFromHtml( result.Html );

                        // Upload the PDF.
                        FinancialStatementGeneratorUploadGivingStatementData uploadGivingStatementData = new FinancialStatementGeneratorUploadGivingStatementData
                        {
                            FinancialStatementGeneratorRecipient = recipient,
                            FinancialStatementIndividualSaveOptions = individualSaveOptions,
                            PDFData = pdfStream.ReadBytesToEnd()
                        };

                        // This will add the document to all of the individuals that should get it based on the settings.
                        var uploadDocumentResponse = FinancialStatementGeneratorHelper.UploadGivingStatementDocument( uploadGivingStatementData, out int? binaryFileId );

                        action.AddLogEntry( $"Uploaded contribution statements for {uploadDocumentResponse.NumberOfIndividuals} individual(s)." );

                        // Update the binary file that is set on the workflow.
                        var binaryFileGuid = new BinaryFileService( rockContext ).GetSelect( binaryFileId.Value, a => a.Guid );

                        if ( binaryFileId.HasValue && binaryFileGuid != null )
                        {
                            this.SetWorkflowAttributeValue( action, AttributeKey.DocumentAttribute, binaryFileGuid );
                        }
                        rockContext.SaveChanges();
                    }
                }
                catch ( Exception ex )
                {
                    errorMessages.Add( ex.Message );
                    action.AddLogEntry( ex.Message, true );
                    return false;
                }
                finally
                {
                    // Clean up stream.
                    if ( pdfStream != null )
                    {
                        pdfStream.Close();
                        pdfStream.Dispose();
                        pdfStream = null;
                    }
                }
            }

            return true;
        }
    }
}
