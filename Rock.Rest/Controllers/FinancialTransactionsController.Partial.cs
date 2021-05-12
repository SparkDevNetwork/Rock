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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;

using Rock;
using Rock.BulkExport;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class FinancialTransactionsController
    {
        /// <summary>
        /// Posts the scanned.
        /// </summary>
        /// <param name="financialTransactionScannedCheck">The financial transaction scanned check.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/PostScanned" )]
        public HttpResponseMessage PostScanned( [FromBody] FinancialTransactionScannedCheck financialTransactionScannedCheck )
        {
            FinancialTransaction financialTransaction = financialTransactionScannedCheck.FinancialTransaction;
            financialTransaction.CheckMicrEncrypted = Encryption.EncryptString( financialTransactionScannedCheck.ScannedCheckMicrData );

            // note: BadMicr scans don't get checked for duplicates, but just in case, make sure that CheckMicrHash isn't set if this has a bad MICR read
            if ( financialTransaction.MICRStatus != MICRStatus.Fail )
            {
                financialTransaction.CheckMicrHash = Encryption.GetSHA1Hash( financialTransactionScannedCheck.ScannedCheckMicrData );
            }

            financialTransaction.CheckMicrParts = Encryption.EncryptString( financialTransactionScannedCheck.ScannedCheckMicrParts );
            return this.Post( financialTransaction );
        }

        /// <summary>
        /// Process and charge a payment.
        /// </summary>
        /// <param name="automatedPaymentArgs"></param>
        /// <param name="enableDuplicateChecking">If false, the payment will be charged even if there is a similar transaction for the same person within a short time period.</param>
        /// <param name="enableScheduleAdherenceProtection">If false and a schedule is indicated in the args, the payment will be charged even if the schedule has already been processed accoring to it's frequency.</param>
        /// <returns>The ID of the new transaction</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/Process" )]
        public virtual HttpResponseMessage ProcessPayment( [FromBody] AutomatedPaymentArgs automatedPaymentArgs, [FromUri] bool enableDuplicateChecking = true, [FromUri] bool enableScheduleAdherenceProtection = true )
        {
            var errorMessage = string.Empty;

            var rockContext = Service.Context as RockContext;
            var automatedPaymentProcessor = new AutomatedPaymentProcessor( GetPersonAliasId( rockContext ), automatedPaymentArgs, rockContext, enableDuplicateChecking, enableScheduleAdherenceProtection );

            if ( !automatedPaymentProcessor.AreArgsValid( out errorMessage ) )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            if ( automatedPaymentProcessor.IsRepeatCharge( out errorMessage ) ||
                !automatedPaymentProcessor.IsAccordingToSchedule( out errorMessage ) )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.Conflict, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            var transaction = automatedPaymentProcessor.ProcessCharge( out errorMessage );
            var gatewayException = automatedPaymentProcessor.GetMostRecentException();

            if ( !string.IsNullOrEmpty( errorMessage ) )
            {
                if ( gatewayException != null )
                {
                    throw gatewayException;
                }

                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.InternalServerError, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            if ( transaction == null )
            {
                if ( gatewayException != null )
                {
                    throw gatewayException;
                }

                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.InternalServerError, "No transaction was created" );
                throw new HttpResponseException( errorResponse );
            }

            var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created, transaction.Id );
            return response;
        }

        /// <summary>
        /// Process the Refund.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/Refund/{transactionId}" )]
        public System.Net.Http.HttpResponseMessage Refund( int transactionId )
        {
            SetProxyCreation( true );

            var transaction = this.Get( transactionId );
            string errorMessage = string.Empty;

            var transactionService = Service as FinancialTransactionService;

            var refundTransaction = transactionService.ProcessRefund( transaction, out errorMessage );
            if ( refundTransaction != null )
            {
                Service.Context.SaveChanges();
                return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, refundTransaction.Id );
            }
            else
            {
                var response = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( response );
            }
        }

        /// <summary>
        /// Posts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override HttpResponseMessage Post( FinancialTransaction value )
        {
            if ( !value.FinancialPaymentDetailId.HasValue )
            {
                //// manually enforce that FinancialPaymentDetailId has a value so that Pre-V4 check
                //// scanners (that don't know about the new FinancialPaymentDetailId) can't post
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "FinancialPaymentDetailId cannot be null" );
            }

            return base.Post( value );
        }

        /// <summary>
        /// Returns true if a transaction with the same MICR track data is already in the database
        /// </summary>
        /// <param name="scannedCheckMicr">The scanned check micr track data</param>
        /// <returns></returns>
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/AlreadyScanned" )]
        public bool AlreadyScanned( [FromBody] string scannedCheckMicr )
        {
            // NOTE: scannedCheckMicr param is [FromBody] so that it will be encrypted when using SSL
            string checkMicrHash = Encryption.GetSHA1Hash( scannedCheckMicr );
            return this.Get().Any( a => a.CheckMicrHash == checkMicrHash );
        }

        /// <summary>
        /// Gets the contribution person group address.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetContributionPersonGroupAddress" )]
        [Obsolete( "Became obsolete in 1.7.0. Marked obsolete in 1.12.4. Use ~/api/FinancialGivingStatement/ endpoints instead" )]
        [RockObsolete( "1.12.4" )]
        public DataSet GetContributionPersonGroupAddress( [FromBody] ContributionStatementOptions options )
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add( "startDate", options.StartDate );
            if ( options.EndDate.HasValue )
            {
                parameters.Add( "endDate", options.EndDate.Value );
            }
            else
            {
                parameters.Add( "endDate", DateTime.MaxValue );
            }

            if ( options.AccountIds != null )
            {
                parameters.Add( "accountIds", options.AccountIds.AsDelimited( "," ) );
            }
            else
            {
                parameters.Add( "accountIds", DBNull.Value );
            }

            if ( options.PersonId.HasValue )
            {
                parameters.Add( "personId", options.PersonId );
            }
            else
            {
                parameters.Add( "personId", DBNull.Value );
            }

            if ( options.IncludeIndividualsWithNoAddress )
            {
                parameters.Add( "includeIndividualsWithNoAddress", options.IncludeIndividualsWithNoAddress );
            }
            else
            {
                parameters.Add( "includeIndividualsWithNoAddress", false );
            }

            parameters.Add( "orderByPostalCode", options.OrderByPostalCode );
            var result = DbService.GetDataSet( "spFinance_ContributionStatementQuery", System.Data.CommandType.StoredProcedure, parameters );

            if ( result.Tables.Count > 0 )
            {
                var dataTable = result.Tables[0];
                dataTable.TableName = "contribution_person_group_address";

                if ( options.DataViewId.HasValue )
                {
                    var dataView = new DataViewService( new RockContext() ).Get( options.DataViewId.Value );
                    if ( dataView != null )
                    {
                        var dataViewGetQueryArgs = new DataViewGetQueryArgs();
                        var personList = dataView.GetQuery( dataViewGetQueryArgs ).OfType<Rock.Model.Person>().Select( a => new { a.Id, a.GivingGroupId } ).ToList();
                        HashSet<int> personIds = new HashSet<int>( personList.Select( a => a.Id ) );
                        HashSet<int> groupsIds = new HashSet<int>( personList.Where( a => a.GivingGroupId.HasValue ).Select( a => a.GivingGroupId.Value ).Distinct() );

                        foreach ( var row in dataTable.Rows.OfType<DataRow>().ToList() )
                        {
                            var personId = row["PersonId"];
                            var groupId = row["GroupId"];
                            if ( personId != null && personId is int )
                            {
                                if ( !personIds.Contains( ( int ) personId ) )
                                {
                                    dataTable.Rows.Remove( row );
                                }
                            }
                            else if ( groupId != null && groupId is int )
                            {
                                if ( !groupsIds.Contains( ( int ) groupId ) )
                                {
                                    dataTable.Rows.Remove( row );
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the contribution transactions.
        /// </summary>
        /// <param name="groupId">The group unique identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetContributionTransactions/{groupId}" )]
        [Obsolete( "Became obsolete in 1.7.0. Marked obsolete in 1.12.4. Use ~/api/FinancialGivingStatement/ endpoints instead" )]
        [RockObsolete( "1.12.4" )]
        public DataSet GetContributionTransactions( int groupId, [FromBody] ContributionStatementOptions options )
        {
            return GetContributionTransactions( groupId, null, options );
        }

        /// <summary>
        /// Gets the contribution transactions.
        /// </summary>
        /// <param name="personId">The person unique identifier.</param>
        /// <param name="groupId">The group unique identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetContributionTransactions/{groupId}/{personId}" )]
        [Obsolete( "Became obsolete in 1.7.0. Marked obsolete in 1.12.4. Use ~/api/FinancialGivingStatement/ endpoints instead" )]
        [RockObsolete( "1.12.4" )]
        public DataSet GetContributionTransactions( int groupId, int? personId, [FromBody] ContributionStatementOptions options )
        {
            var qry = Get().Where( a => a.TransactionDateTime >= options.StartDate );

            if ( options.EndDate.HasValue )
            {
                qry = qry.Where( a => a.TransactionDateTime < options.EndDate.Value );
            }

            var transactionTypeContribution = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            if ( transactionTypeContribution != null )
            {
                int transactionTypeContributionId = transactionTypeContribution.Id;
                qry = qry.Where( a => a.TransactionTypeValueId == transactionTypeContributionId );
            }

            if ( personId.HasValue )
            {
                // get transactions for a specific person
                qry = qry.Where( a => a.AuthorizedPersonAlias.PersonId == personId.Value );
            }
            else
            {
                // get transactions for all the persons in the specified group that have specified that group as their GivingGroup
                GroupMemberService groupMemberService = new GroupMemberService( ( RockContext ) Service.Context );
                var personIdList = groupMemberService.GetByGroupId( groupId ).Where( a => a.Person.GivingGroupId == groupId ).Select( s => s.PersonId ).ToList();

                qry = qry.Where( a => personIdList.Contains( a.AuthorizedPersonAlias.PersonId ) );
            }

            if ( options.AccountIds != null )
            {
                qry = qry.Where( a => a.TransactionDetails.Any( x => options.AccountIds.Contains( x.AccountId ) ) );
            }

            var selectQry = qry.Select( a => new
            {
                a.TransactionDateTime,
                CurrencyTypeValueName = a.FinancialPaymentDetail != null ? a.FinancialPaymentDetail.CurrencyTypeValue.Value : string.Empty,
                a.Summary,
                Details = a.TransactionDetails.Select( d => new
                {
                    d.AccountId,
                    AccountName = d.Account.PublicName,
                    d.Summary,
                    d.Amount,
                    AccountOrder = d.Account.Order
                } ).OrderBy( x => x.AccountOrder ),
            } ).OrderBy( a => a.TransactionDateTime );

            DataTable dataTable = new DataTable( "contribution_transactions" );
            dataTable.Columns.Add( "TransactionDateTime", typeof( DateTime ) );
            dataTable.Columns.Add( "CurrencyTypeValueName" );
            dataTable.Columns.Add( "Summary" );
            dataTable.Columns.Add( "Amount", typeof( decimal ) );
            dataTable.Columns.Add( "Details", typeof( DataTable ) );

            var list = selectQry.ToList();

            dataTable.BeginLoadData();
            foreach ( var fieldItems in list )
            {
                DataTable detailTable = new DataTable( "transaction_details" );
                detailTable.Columns.Add( "AccountId", typeof( int ) );
                detailTable.Columns.Add( "AccountName" );
                detailTable.Columns.Add( "Summary" );
                detailTable.Columns.Add( "Amount", typeof( decimal ) );
                var transactionDetails = fieldItems.Details.ToList();

                // remove any Accounts that were not included (in case there was a mix of included and not included accounts in the transaction)
                if ( options.AccountIds != null )
                {
                    transactionDetails = transactionDetails.Where( a => options.AccountIds.Contains( a.AccountId ) ).ToList();
                }

                foreach ( var detail in transactionDetails )
                {
                    var detailArray = new object[] {
                        detail.AccountId,
                        detail.AccountName,
                        detail.Summary,
                        detail.Amount
                    };

                    detailTable.Rows.Add( detailArray );
                }

                var itemArray = new object[] {
                    fieldItems.TransactionDateTime,
                    fieldItems.CurrencyTypeValueName,
                    fieldItems.Summary,
                    transactionDetails.Sum(a => a.Amount),
                    detailTable
                };

                dataTable.Rows.Add( itemArray );
            }

            dataTable.EndLoadData();

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add( dataTable );

            return dataSet;
        }

        /// <summary>
        /// Gets transactions by people with the supplied givingId.
        /// </summary>
        /// <param name="givingId">The giving ID.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [EnableQuery]
        [System.Web.Http.Route( "api/FinancialTransactions/GetByGivingId/{givingId}" )]
        public IQueryable<FinancialTransaction> GetByGivingId( string givingId )
        {
            if ( string.IsNullOrWhiteSpace( givingId ) || !( givingId.StartsWith( "P" ) || givingId.StartsWith( "G" ) ) )
            {
                var response = new HttpResponseMessage( HttpStatusCode.BadRequest );
                response.Content = new StringContent( "The supplied givingId is not valid" );
                throw new HttpResponseException( response );
            }

            // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
            var personAliasIds = new PersonAliasService( ( RockContext ) this.Service.Context ).Queryable().Where( a => a.Person.GivingId == givingId ).Select( a => a.Id );

            // get the transactions for the person or all the members in the person's giving group (Family)
            return Get().Where( t => t.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) );
        }

        /// <summary>
        /// Exports Financial Transaction Records
        /// </summary>
        /// <param name="page">The page being requested (where first page is 1).</param>
        /// <param name="pageSize">The number of records to provide per page. NOTE: This is limited to the 'API Max Items Per Page' global attribute.</param>
        /// <param name="sortBy">Optional field to sort by. This must be a mapped property on the Person model.</param>
        /// <param name="sortDirection">The sort direction (1 = Ascending, 0 = Descending). Default is 1 (Ascending).</param>
        /// <param name="dataViewId">The optional data view to use for filtering.</param>
        /// <param name="modifiedSince">The optional date/time to filter to only get newly updated items.</param>
        /// <param name="startDateTime">Optional filter to limit to transactions with a transaction date/time greater than or equal to startDateTime</param>
        /// <param name="endDateTime">Optional filter to limit to transactions with a transaction date/time less than endDateTime</param>
        /// <param name="attributeKeys">Optional comma-delimited list of attribute keys for the attribute values that should be included with each exported record, or specify 'all' to include all attributes.</param>
        /// <param name="attributeReturnType">Raw/Formatted (default is Raw)</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/FinancialTransactions/Export" )]
        public FinancialTransactionsExport Export(
            int page = 1,
            int pageSize = 1000,
            string sortBy = null,
            System.Web.UI.WebControls.SortDirection sortDirection = System.Web.UI.WebControls.SortDirection.Ascending,
            int? dataViewId = null,
            DateTime? modifiedSince = null,
            DateTime? startDateTime = null,
            DateTime? endDateTime = null,
            string attributeKeys = null,
            AttributeReturnType attributeReturnType = AttributeReturnType.Raw
            )
        {
            // limit to 'API Max Items Per Page' global attribute
            int maxPageSize = GlobalAttributesCache.Get().GetValue( "core_ExportAPIsMaxItemsPerPage" ).AsIntegerOrNull() ?? 1000;
            var actualPageSize = Math.Min( pageSize, maxPageSize );

            FinancialTransactionExportOptions exportOptions = new FinancialTransactionExportOptions
            {
                SortBy = sortBy,
                SortDirection = sortDirection,
                DataViewId = dataViewId,
                ModifiedSince = modifiedSince,
                AttributeList = AttributesExport.GetAttributesFromAttributeKeys<FinancialTransaction>( attributeKeys ),
                AttributeReturnType = attributeReturnType,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            var rockContext = new RockContext();
            var financialTransactionService = new FinancialTransactionService( rockContext );
            return financialTransactionService.GetFinancialTransactionExport( page, actualPageSize, exportOptions );
        }

        /// <summary>
        /// Gets the giving history for the person (and giving group) during the year specified.
        /// </summary>
        /// <param name="year">Defaults to the current year.</param>
        /// <param name="includeGivingGroup">Should transactions belonging to anyone in the person's giving group be included</param>
        /// <param name="transactionTypeGuid">The guid of the defined value of the transaction type to include. If omitted, all transaction types will be included</param>
        /// <param name="excludedStatus">Transactions of this status will be excluded. If omitted, all transaction statuses will be included</param>
        /// <param name="excludedSourceTypeGuid">The unique identifier of a <see cref="FinancialTransaction.SourceTypeValue" /> to exclude from the results</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/FinancialTransactions/GivingHistory" )]
        public virtual List<Gift> GetGivingHistoryForTheCurrentPerson( [FromUri] int? year = null, [FromUri] bool includeGivingGroup = true, [FromUri] Guid? transactionTypeGuid = null,
            [FromUri] string excludedStatus = null, [FromUri] Guid? excludedSourceTypeGuid = null )
        {
            var personAliasId = GetPersonAliasId( Service.Context as RockContext );

            if ( !personAliasId.HasValue )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The person alias id for the current user did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            return GetGivingHistory( personAliasId.Value, year, includeGivingGroup, transactionTypeGuid, excludedStatus, excludedSourceTypeGuid );
        }

        /// <summary>
        /// Gets the giving history for the person (and giving group) during the year specified.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="year">Defaults to the current year.</param>
        /// <param name="includeGivingGroup">Should transactions belonging to anyone in the person's giving group be included</param>
        /// <param name="transactionTypeGuid">The guid of the defined value of the transaction type to include. If omitted, all transaction types will be included</param>
        /// <param name="excludedStatus">Transactions of this status will be excluded. If omitted, all transaction statuses will be included</param>
        /// <param name="excludedSourceTypeGuid">The unique identifier of a <see cref="FinancialTransaction.SourceTypeValue"/> to exclude from the results</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/FinancialTransactions/GivingHistory/{personAliasId}" )]
        public virtual List<Gift> GetGivingHistory( int personAliasId,
            [FromUri] int? year = null, [FromUri] bool includeGivingGroup = true, [FromUri] Guid? transactionTypeGuid = null,
            [FromUri] string excludedStatus = null, [FromUri] Guid? excludedSourceTypeGuid = null )
        {
            // Get all of the query filters ready
            var yearFilter = year ?? RockDateTime.Now.Year;
            var transactionTypeValue = transactionTypeGuid.HasValue ? DefinedValueCache.Get( transactionTypeGuid.Value ) : null;

            // Validate the filters
            if ( transactionTypeGuid.HasValue && transactionTypeValue == null )
            {
                var errorMessage = $"A transaction type defined value was expected for guid '{transactionTypeGuid.Value}', but was not found";
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            // Query for the transactions
            var rockContext = new RockContext();
            var transactionService = new FinancialTransactionService( rockContext );

            var query = transactionService.Queryable( "FinancialPaymentDetail, TransactionDetails.Account" ).AsNoTracking().Where( t =>
                t.AuthorizedPersonAliasId.HasValue &&
                t.TransactionDateTime.HasValue &&
                t.TransactionDateTime.Value.Year == yearFilter );

            // If we include the giving group, then query for everyone in it, otherwise just the id sent
            if ( includeGivingGroup )
            {
                query = query.Where( t =>
                    t.AuthorizedPersonAliasId == personAliasId ||
                    t.AuthorizedPersonAlias.Person.GivingGroup.Members.Any( m => m.Person.Aliases.Any( a => a.Id == personAliasId ) ) );
            }
            else
            {
                query = query.Where( t => t.AuthorizedPersonAliasId == personAliasId );
            }

            // If there is an excluded status param, then exclude those statuses from the results
            if ( excludedStatus != null )
            {
                query = query.Where( t => t.Status != excludedStatus );
            }

            // If there is an excluded source type param, then exclude that source from the results
            if ( excludedSourceTypeGuid.HasValue )
            {
                query = query.Where( t => t.SourceTypeValue == null || t.SourceTypeValue.Guid != excludedSourceTypeGuid.Value );
            }

            // If there is a transaction type then only include transactions of that type
            if ( transactionTypeValue != null )
            {
                query = query.Where( t => t.TransactionTypeValueId == transactionTypeValue.Id );
            }

            // Enumerate the transactions. Expiration month and year cannot be copied directly in LINQ queries without first enumerating.
            var transactions = query.OrderByDescending( g => g.TransactionDateTime ).ToList();

            // Generate return objects from the transactions
            var gifts = transactions.Select( t => new Gift
            {
                Id = t.Id,
                Guid = t.Guid,
                AuthorizedPersonAliasId = t.AuthorizedPersonAliasId.Value,
                ForeignKey = t.ForeignKey,
                ScheduledTransactionId = t.ScheduledTransactionId,
                TransactionDateTime = t.TransactionDateTime.Value,
                Summary = t.Summary,
                TransactionCode = t.TransactionCode,
                TransactionTypeValueId = t.TransactionTypeValueId,
                Status = t.Status,
                TotalAmount = t.TotalAmount,
                FinancialPaymentDetail = t.FinancialPaymentDetail == null ? null : new GiftPaymentDetail
                {
                    Id = t.FinancialPaymentDetail.Id,
                    AccountNumberMasked = t.FinancialPaymentDetail.AccountNumberMasked,
                    ForeignKey = t.FinancialPaymentDetail.ForeignKey,
                    CreditCardTypeValueId = t.FinancialPaymentDetail.CreditCardTypeValueId,
                    CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                    ExpirationMonth = t.FinancialPaymentDetail.ExpirationMonth,
                    ExpirationYear = t.FinancialPaymentDetail.ExpirationYear
                },
                TransactionDetails = t.TransactionDetails?.Select( d => new GiftDetail
                {
                    Amount = d.Amount,
                    Account = d.Account == null ? null : new GiftAccount
                    {
                        Id = d.Account.Id,
                        PublicName = d.Account.PublicName,
                        ParentAccountId = d.Account.ParentAccountId,
                        CampusId = d.Account.CampusId
                    }
                } ).ToList()
            } ).ToList();

            return gifts;
        }

        #region helper classes

        /// <summary>
        ///
        /// </summary>
        [Obsolete( "Became obsolete in 1.7.0. Marked obsolete in 1.12.4. Use ~/api/FinancialGivingStatement/ endpoints instead" )]
        [RockObsolete( "1.12.4" )]
        public class ContributionStatementOptions
        {
            /// <summary>
            /// Gets or sets the start date.
            /// </summary>
            /// <value>
            /// The start date.
            /// </value>
            public DateTime StartDate { get; set; }

            /// <summary>
            /// Gets or sets the end date.
            /// </summary>
            /// <value>
            /// The end date.
            /// </value>
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// Gets or sets the account ids.
            /// </summary>
            /// <value>
            /// The account ids.
            /// </value>
            public List<int> AccountIds { get; set; }

            /// <summary>
            /// Gets or sets the person id.
            /// </summary>
            /// <value>
            /// The person id.
            /// </value>
            public int? PersonId { get; set; }

            /// <summary>
            /// Gets or sets the Person DataViewId to filter the statements to
            /// </summary>
            /// <value>
            /// The data view identifier.
            /// </value>
            public int? DataViewId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [include individuals with no address].
            /// </summary>
            /// <value>
            /// <c>true</c> if [include individuals with no address]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeIndividualsWithNoAddress { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [order by postal code].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [order by postal code]; otherwise, <c>false</c>.
            /// </value>
            public bool OrderByPostalCode { get; set; }
        }

        /// <summary>
        /// A response object for the giving history endpoint. Closely resembles a <see cref="Rock.Model.FinancialTransaction"/>
        /// </summary>
        public class Gift
        {
            /// <summary>
            /// Gets or sets the identifier of the <see cref="Rock.Model.FinancialTransaction"/>.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the authorized person alias identifier.
            /// </summary>
            /// <value>
            /// The authorized person alias identifier.
            /// </value>
            public int AuthorizedPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier of the <see cref="Rock.Model.FinancialTransaction"/>.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the foreign key. This may be the Stripe customer identifier.
            /// </summary>
            /// <value>
            /// The foreign key.
            /// </value>
            public string ForeignKey { get; set; }

            /// <summary>
            /// Gets or sets date and time that the transaction occurred. This is the local server time.
            /// </summary>
            /// <value>
            /// A <see cref="DateTime"/> representing the time that the transaction occurred. This is the local server time.
            /// </value>
            public DateTime TransactionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the ScheduledTransactionId of the <see cref="Rock.Model.FinancialScheduledTransaction" /> that triggered
            /// this transaction. If this was an ad-hoc/on demand transaction, this property will be null.
            /// </summary>
            /// <value>
            /// A <see cref="int"/> representing the ScheduledTransactionId of the <see cref="Rock.Model.FinancialScheduledTransaction"/>
            /// </value>
            public int? ScheduledTransactionId { get; set; }

            /// <summary>
            /// For Credit Card transactions, this is the response code that the gateway returns. 
            /// For Scanned Checks, this is the check number.
            /// </summary>
            /// <value>
            /// A <see cref="string"/> representing the transaction code of the transaction.
            /// </value>
            public string TransactionCode { get; set; }

            /// <summary>
            /// Gets or sets a summary of the transaction.
            /// </summary>
            /// <value>
            /// A <see cref="string"/> representing a summary of the transaction.
            /// </value>
            public string Summary { get; set; }

            /// <summary>
            /// Gets or sets the total amount.
            /// </summary>
            /// <value>
            /// The total amount.
            /// </value>
            public decimal TotalAmount { get; set; }

            /// <summary>
            /// Gets or sets the transaction type identifier.
            /// </summary>
            /// <value>
            /// The transaction type identifier.
            /// </value>
            public int TransactionTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the status.
            /// </summary>
            /// <value>
            /// The status.
            /// </value>
            public string Status { get; set; }

            /// <summary>
            /// Gets or sets the financial payment detail.
            /// </summary>
            /// <value>
            /// The financial payment detail.
            /// </value>
            public GiftPaymentDetail FinancialPaymentDetail { get; set; }

            /// <summary>
            /// Gets or sets the transaction details.
            /// </summary>
            /// <value>
            /// The transaction details.
            /// </value>
            public List<GiftDetail> TransactionDetails { get; set; }
        }

        /// <summary>
        /// A response object for the giving history endpoint. Closely resembles a <see cref="Rock.Model.FinancialPaymentDetail"/>
        /// </summary>
        public class GiftPaymentDetail
        {
            /// <summary>
            /// Gets or sets the identifier of the <see cref="Rock.Model.FinancialPaymentDetail"/>.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the Masked Account Number (Last 4 of Account Number prefixed with 12 *'s)
            /// </summary>
            /// <value>
            /// The account number masked.
            /// </value>
            public string AccountNumberMasked { get; set; }

            /// <summary>
            /// Gets or sets the foreign key. This may be the Stripe customer identifier.
            /// </summary>
            /// <value>
            /// The foreign key.
            /// </value>
            public string ForeignKey { get; set; }

            /// <summary>
            /// Gets the expiration month by decrypting ExpirationMonthEncrypted
            /// </summary>
            /// <value>
            /// The expiration month.
            /// </value>
            public int? ExpirationMonth { get; set; }

            /// <summary>
            /// Gets the expiration year by decrypting ExpirationYearEncrypted
            /// </summary>
            /// <value>
            /// The expiration year.
            /// </value>
            public int? ExpirationYear { get; set; }

            /// <summary>
            /// Gets or sets the DefinedValueId of the credit card type <see cref="Rock.Model.DefinedValue"/> indicating the credit card brand/type that was used
            /// to make this transaction. This value will be null for transactions that were not made by credit card.
            /// </summary>
            /// <value>
            /// A <see cref="int"/> representing the DefinedValueId of the credit card type <see cref="Rock.Model.DefinedValue"/> that was used to make this transaction.
            /// This value will be null for transactions that were not made by credit card.
            /// </value>
            public int? CreditCardTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the DefinedValueId of the currency type <see cref="Rock.Model.DefinedValue"/> indicating the currency that the
            /// transaction was made in.
            /// </summary>
            /// <value>
            /// A <see cref="int" /> representing the DefinedValueId of the CurrencyType <see cref="Rock.Model.DefinedValue" /> for this transaction.
            /// </value>
            public int? CurrencyTypeValueId { get; set; }
        }

        /// <summary>
        /// A response object for the gift endpoint. Closely resembles a <see cref="Rock.Model.FinancialTransactionDetail"/>
        /// </summary>
        public class GiftDetail
        {
            /// <summary>
            /// Gets or sets the total amount of the transaction detail. This total amount includes any associated fees.
            /// </summary>
            /// <value>
            /// A <see cref="decimal"/> representing the total amount of the transaction detail.
            /// </value>
            public decimal Amount { get; set; }

            /// <summary>
            /// Gets or sets the account.
            /// </summary>
            /// <value>
            /// The account.
            /// </value>
            public GiftAccount Account { get; set; }
        }

        /// <summary>
        /// A response object for the gift endpoint. Closely resembles a <see cref="Rock.Model.FinancialAccount"/>
        /// </summary>
        public class GiftAccount
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the public name of the account.
            /// </summary>
            /// <value>
            /// The name of the public.
            /// </value>
            public string PublicName { get; set; }

            /// <summary>
            /// Gets or sets the FinancialAccountId of the parent FinancialAccount to this FinancialAccount. If this
            /// FinancialAccount does not have a parent, this property will be null.
            /// </summary>
            /// <value>
            /// A <see cref="int"/> representing the FinancialAccountId of the parent FinancialAccount to this FinancialAccount. 
            /// This property will be null if the FinancialAccount does not have a parent.
            /// </value>
            public int? ParentAccountId { get; set; }

            /// <summary>
            /// Gets or sets the CampusId of the <see cref="Rock.Model.Campus"/> that this FinancialAccount is associated with. If this FinancialAccount is not
            /// associated with a <see cref="Rock.Model.Campus"/> this property will be null.
            /// </summary>
            /// <value>
            /// A <see cref="int"/> representing the CampusId of the <see cref="Rock.Model.Campus"/> that the FinancialAccount is associated with.
            /// </value>
            public int? CampusId { get; set; }
        }

        #endregion
    }
}