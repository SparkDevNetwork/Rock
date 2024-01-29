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
    [Rock.SystemGuid.RestControllerGuid( "0E5FF2B5-1501-4645-BAFD-0B0CC03C3B42")]
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
            return FinancialStatementGeneratorHelper.UploadGivingStatementDocument( uploadGivingStatementData );
        }

        /// <summary>
        /// Render and return a giving statement for the specified person. If the person
        /// uses combined giving, the statement will be for the person's giving group.
        /// </summary>
        /// <param name="personId">The person that the statement is for. If the person
        /// uses combined giving, the statement will be for the person's giving group,
        /// which is typically the family.</param>
        /// <param name="year">The contribution calendar year. ie 2019.  If not specified, the
        /// current year is assumed.</param>
        /// <param name="templateDefinedValueId">[Obsolete] The defined value ID that represents the statement
        /// lava. This defined value should be a part of the Statement Generator Lava Template defined
        /// type. If no ID is specified, then the default defined value for the Statement Generator Lava
        /// Template defined type is assumed.</param>
        /// <param name="financialStatementTemplateId">The Statement Template to use. This is required (unless the obsolete templateDefinedValueId is specified).</param>
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

            var financialStatementGeneratorRecipientRequest = new FinancialStatementGeneratorRecipientRequest( options );
            if ( person.GivingGroupId.HasValue )
            {
                // If person has a GivingGroupId get the combined statement for the GivingGroup
                financialStatementGeneratorRecipientRequest.FinancialStatementGeneratorRecipient = new FinancialStatementGeneratorRecipient
                {
                    GroupId = person.GivingGroupId.Value,
                    PersonId = null
                };
            }
            else
            {
                // If person gives individually ( GivingGroupId is null) get the individual statement for the person
                // and specify Group as the Primary Family so we know which Family to use for the address.
                financialStatementGeneratorRecipientRequest.FinancialStatementGeneratorRecipient = new FinancialStatementGeneratorRecipient
                {
                    GroupId = person.PrimaryFamilyId.Value,
                    PersonId = person.Id
                };
            }

            // Set the Location ID so the recipient's mailing address is included on the statement.
            financialStatementGeneratorRecipientRequest.FinancialStatementGeneratorRecipient.LocationId = person.GetMailingLocation()?.Id;

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
