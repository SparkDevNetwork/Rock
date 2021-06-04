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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.StatementGenerator.Rest
{
    /// <summary>
    /// NOTE: WebApi doesn't support Controllers with the Same Name, even if they have different NameSpaces, so can't call this FinancialTransactionsController
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    public class FinancialGivingStatementController : Rock.Rest.ApiControllerBase
    {
        #region REST Endpoints

        /// <summary>
        /// Gets the statement generator recipients. This will be sorted based on the StatementGeneratorOptions
        /// </summary>
        /// <param name="financialStatementGeneratorOptions">The financial statement generator options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialGivingStatement/GetFinancialStatementGeneratorRecipients" )]
        public List<FinancialStatementGeneratorRecipient> GetFinancialStatementGeneratorRecipients( [FromBody] Rock.Financial.FinancialStatementGeneratorOptions financialStatementGeneratorOptions )
        {
            return FinancialStatementGeneratorHelper.GetFinancialStatementGeneratorRecipients( financialStatementGeneratorOptions );
        }

        /// <summary>
        /// Gets the statement generator recipient result for a specific person and associated group (family) with the specified address (locationGuid)
        /// NOTE: If a person is in multiple families, call this for each of the families so that the statement will go to the address of each family
        /// </summary>
        /// <param name="financialStatementGeneratorRecipientRequest">The financial statement generator recipient request.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialGivingStatement/GetStatementGeneratorRecipientResult" )]
        public FinancialStatementGeneratorRecipientResult GetStatementGeneratorRecipientResult( [FromBody] Rock.Financial.FinancialStatementGeneratorRecipientRequest financialStatementGeneratorRecipientRequest )
        {
            return FinancialStatementGeneratorHelper.GetStatementGeneratorRecipientResult( financialStatementGeneratorRecipientRequest, this.GetPerson() );
        }

        /// <summary>
        /// Uploads the giving statement document, and returns the <see cref="Rock.Model.Document"/> Id
        /// </summary>
        /// <param name="uploadGivingStatementData">The upload giving statement data.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialGivingStatement/UploadGivingStatementDocument" )]
        public FinancialStatementGeneratorUploadGivingStatementResult UploadGivingStatementDocument( [FromBody] FinancialStatementGeneratorUploadGivingStatementData uploadGivingStatementData )
        {
            var rockContext = new RockContext();

            var saveOptions = uploadGivingStatementData?.FinancialStatementIndividualSaveOptions;

            if ( saveOptions == null )
            {
                throw new FinancialGivingStatementArgumentException( "FinancialStatementIndividualSaveOptions must be specified" );
            }

            if ( !saveOptions.SaveStatementsForIndividuals )
            {
                throw new FinancialGivingStatementArgumentException( "FinancialStatementIndividualSaveOptions.SaveStatementsForIndividuals is not enabled." );
            }

            var documentTypeId = saveOptions.DocumentTypeId;

            if ( !documentTypeId.HasValue )
            {
                throw new FinancialGivingStatementArgumentException( "Document Type must be specified" );
            }

            var documentType = new DocumentTypeService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( dt => dt.Id == documentTypeId.Value )
                    .Select( a => new
                    {
                        BinaryFileTypeId = ( int? ) a.BinaryFileTypeId
                    } )
                    .FirstOrDefault();

            if ( documentType == null )
            {
                throw new FinancialGivingStatementArgumentException( "DocumentType must be specified" );
            }

            if ( documentType.BinaryFileTypeId == null )
            {
                throw new FinancialGivingStatementArgumentException( "DocumentType.BinaryFileType must be specified" );
            }

            var pdfData = uploadGivingStatementData.PDFData;

            string fileName = saveOptions.DocumentName + ".pdf";

            var financialStatementGeneratorRecipient = uploadGivingStatementData.FinancialStatementGeneratorRecipient;
            if ( financialStatementGeneratorRecipient == null )
            {
                throw new FinancialGivingStatementArgumentException( "FinancialStatementGeneratorRecipient must be specified" );
            }

            var documentName = saveOptions.DocumentName;

            List<int> documentPersonIds;
            if ( financialStatementGeneratorRecipient.PersonId.HasValue )
            {
                // If we are saving for a person that gives an individual, just give document to that person (ignore the FinancialStatementIndividualSaveOptionsSaveFor option)
                // only upload the document to the individual person
                documentPersonIds = new List<int>();
                documentPersonIds.Add( financialStatementGeneratorRecipient.PersonId.Value );
            }
            else
            {
                var groupId = financialStatementGeneratorRecipient.GroupId;
                var givingFamilyMembersQuery = new GroupMemberService( rockContext ).GetByGroupId( groupId, false );

                // limit to family members within the same giving group
                givingFamilyMembersQuery = givingFamilyMembersQuery.Where( a => a.Person.GivingGroupId.HasValue && a.Person.GivingGroupId == groupId );

                if ( saveOptions.DocumentSaveFor == FinancialStatementGeneratorOptions.FinancialStatementIndividualSaveOptions.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveAdultsInGivingGroup )
                {
                    documentPersonIds = givingFamilyMembersQuery
                        .Where( a => a.Person.AgeClassification == AgeClassification.Adult ).Select( a => a.PersonId ).ToList();
                }
                else if ( saveOptions.DocumentSaveFor == FinancialStatementGeneratorOptions.FinancialStatementIndividualSaveOptions.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveFamilyMembersInGivingGroup )
                {
                    documentPersonIds = givingFamilyMembersQuery
                        .Select( a => a.PersonId ).ToList();
                }
                else if ( saveOptions.DocumentSaveFor == FinancialStatementGeneratorOptions.FinancialStatementIndividualSaveOptions.FinancialStatementIndividualSaveOptionsSaveFor.PrimaryGiver )
                {
                    // Set document for PrimaryGiver (aka Head of Household).
                    // Note that HeadOfHouseHold would calculated based on family members within the same giving group
                    var headOfHouseHoldPersonId = givingFamilyMembersQuery.GetHeadOfHousehold( s => ( int? ) s.PersonId );
                    documentPersonIds = new List<int>();
                    if ( headOfHouseHoldPersonId.HasValue )
                    {
                        documentPersonIds.Add( headOfHouseHoldPersonId.Value );
                    }
                }
                else
                {
                    // shouldn't happen
                    documentPersonIds = new List<int>();
                }
            }

            var today = RockDateTime.Today;
            var tomorrow = today.AddDays( 1 );

            foreach ( var documentPersonId in documentPersonIds )
            {
                // Create the document, linking the entity and binary file.
                if ( saveOptions.OverwriteDocumentsOfThisTypeCreatedOnSameDate == true )
                {
                    using ( var deleteDocContext = new RockContext() )
                    {
                        var deleteDocumentService = new DocumentService( deleteDocContext );

                        // See if there is an existing one.
                        // Note include BinaryFile in the Get since we'll have to mark it temporary if it exists.
                        var existingDocument = deleteDocumentService.Queryable().Where(
                            a => a.DocumentTypeId == documentTypeId.Value
                            && a.EntityId == documentPersonId
                            && a.CreatedDateTime.HasValue
                            && a.CreatedDateTime >= today && a.CreatedDateTime < tomorrow )
                            .Include( a => a.BinaryFile ).FirstOrDefault();

                        // NOTE: Delete vs update since we normally don't change the contents of documents/binary files once they've been created
                        if ( existingDocument != null )
                        {
                            deleteDocumentService.Delete( existingDocument );
                            deleteDocContext.SaveChanges();
                        }
                    }
                }

                // Create the binary file.
                var binaryFile = new BinaryFile
                {
                    BinaryFileTypeId = documentType.BinaryFileTypeId,
                    MimeType = "application/pdf",
                    FileName = fileName,
                    FileSize = pdfData.Length,
                    IsTemporary = false,
                    ContentStream = new MemoryStream( pdfData )
                };

                new BinaryFileService( rockContext ).Add( binaryFile );
                rockContext.SaveChanges();

                Document document = new Document
                {
                    DocumentTypeId = documentTypeId.Value,
                    EntityId = documentPersonId,
                    PurposeKey = saveOptions.DocumentPurposeKey,
                    Name = saveOptions.DocumentName,
                    Description = saveOptions.DocumentDescription
                };

                document.SetBinaryFile( binaryFile.Id, rockContext );

                var documentService = new DocumentService( rockContext );

                documentService.Add( document );
            }

            rockContext.SaveChanges();

            return new FinancialStatementGeneratorUploadGivingStatementResult
            {
                NumberOfIndividuals = documentPersonIds.Count
            };
        }

        /// <summary>
        /// Render and return a giving statement for the specified person.
        /// </summary>
        /// <param name="personId">The person that made the contributions. That person's entire
        /// giving group is included, which is typically the family.</param>
        /// <param name="year">The contribution calendar year. ie 2019.  If not specified, the
        /// current year is assumed.</param>
        /// <param name="templateDefinedValueId">[Obsolete] The defined value ID that represents the statement
        /// lava. This defined value should be a part of the Statement Generator Lava Template defined
        /// type. If no ID is specified, then the default defined value for the Statement Generator Lava
        /// Template defined type is assumed.</param>
        /// <param name="financialStatementTemplateId"></param>
        /// <param name="hideRefundedTransactions">if set to <c>true</c> transactions that have any
        /// refunds will be hidden.</param>
        /// <returns>
        /// The rendered giving statement
        /// </returns>
        [System.Web.Http.Route( "api/GivingStatement/{personId}" )]
        [System.Web.Http.Route( "api/FinancialGivingStatement/{personId}" )]
        [HttpGet]
        [Authenticate, Secured]
        public HttpResponseMessage RenderGivingStatement(
            int personId,
            [FromUri] int? year = null,
            [FromUri] int? templateDefinedValueId = null,
            [FromUri] int? financialStatementTemplateId = null,
            [FromUri] bool hideRefundedTransactions = true )
        {
            if ( templateDefinedValueId.HasValue )
            {
                // if they specified templateDefinedValueId, they are wanting the obsolete version of api/GivingStatement. So call the obsolete version of it
#pragma warning disable CS0618

                var legacyHtml = StatementGeneratorFinancialTransactionsController.GetGivingStatementHTML( personId, year, templateDefinedValueId, hideRefundedTransactions, this.GetPerson() );

#pragma warning restore CS0618

                // Render the statement as HTML and send back to the user
                var legacyResponse = new HttpResponseMessage();
                legacyResponse.Content = new StringContent( legacyHtml );
                legacyResponse.Content.Headers.ContentType = new MediaTypeHeaderValue( "text/html" );
                return legacyResponse;
            }

            // Assume the current year if no year is specified
            var currentYear = RockDateTime.Now.Year;
            year = year ?? currentYear;
            var isCurrentYear = year == currentYear;
            var startDate = new DateTime( year.Value, 1, 1 );
            var endDate = isCurrentYear ? RockDateTime.Now : new DateTime( year.Value + 1, 1, 1 );

            // Declare the necessary services
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            // Get the family ID
            var person = personService.Get( personId );
            if ( person == null )
            {
                throw new FinancialGivingStatementException( string.Format( "The person with ID {0} could not be found", personId ) );
            }

            if ( !person.PrimaryFamilyId.HasValue )
            {
                throw new FinancialGivingStatementException( string.Format( "The person with ID {0} does not have a primary family ID", personId ) );
            }

            // Build the options for the generator
            var options = new FinancialStatementGeneratorOptions
            {
                EndDate = endDate,
                FinancialStatementTemplateId = financialStatementTemplateId,
                StartDate = startDate,
            };

            var financialStatementGeneratorRecipientRequest = new FinancialStatementGeneratorRecipientRequest( options )
            {
                FinancialStatementGeneratorRecipient = new FinancialStatementGeneratorRecipient { GroupId = person.PrimaryFamilyId.Value, PersonId = person.Id }
            };

            // Get the generator result
            FinancialStatementGeneratorRecipientResult result = FinancialStatementGeneratorHelper.GetStatementGeneratorRecipientResult( financialStatementGeneratorRecipientRequest, this.GetPerson() );

            // Render the statement as HTML and send back to the user
            var response = new HttpResponseMessage();
            response.Content = new StringContent( result.Html );
            response.Content.Headers.ContentType = new MediaTypeHeaderValue( "text/html" );
            return response;
        }

        #endregion REST Endpoints
    }
}
