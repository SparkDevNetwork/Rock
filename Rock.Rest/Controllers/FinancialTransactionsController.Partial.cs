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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using Rock;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;

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
        public HttpResponseMessage PostScanned( [FromBody]FinancialTransactionScannedCheck financialTransactionScannedCheck )
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
        public bool AlreadyScanned( [FromBody]string scannedCheckMicr )
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
        public DataSet GetContributionPersonGroupAddress( [FromBody]ContributionStatementOptions options )
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
                        List<string> errorMessages = new List<string>();
                        var personList = dataView.GetQuery( null, null, out errorMessages ).OfType<Rock.Model.Person>().Select( a => new { a.Id, a.GivingGroupId } ).ToList();
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
        public DataSet GetContributionTransactions( int groupId, [FromBody]ContributionStatementOptions options )
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
        public DataSet GetContributionTransactions( int groupId, int? personId, [FromBody]ContributionStatementOptions options )
        {
            var qry = Get().Where( a => a.TransactionDateTime >= options.StartDate );

            if ( options.EndDate.HasValue )
            {
                qry = qry.Where( a => a.TransactionDateTime < options.EndDate.Value );
            }

            var transactionTypeContribution = Rock.Web.Cache.DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
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

        //[HttpGet]
        //[System.Web.Http.Route( "api/FinancialTransactions/ChargeStep3/{GatewayId}/{TokenId}" )]
        //public FinancialTransaction ChargeStep3( int gatewayId, string tokenId )
        //{
        //    SetProxyCreation( true );
        //    var rockContext = (RockContext)Service.Context;
        //    var financialGateway = new FinancialGatewayService( rockContext ).Get( gatewayId );
        //    if ( financialGateway == null )
        //    {
        //        throw new HttpResponseException( Request.CreateErrorResponse( HttpStatusCode.NotFound, "Gateway does not exist!" ) );
        //    }

        //    var gateway = financialGateway.GetGatewayComponent();
        //    if ( gateway == null )
        //    {
        //        throw new HttpResponseException( Request.CreateErrorResponse( HttpStatusCode.NotFound, "Gateway component could not be loaded!" ) );
        //    }

        //    var paymentInfo = new PaymentInfo();
        //    paymentInfo.AdditionalParameters.Add( "token-id", tokenId );

        //    string errorMessage = string.Empty;

        //    var transaction = gateway.ChargeStep3( financialGateway, paymentInfo, out errorMessage );
        //    if ( transaction == null || !string.IsNullOrWhiteSpace( errorMessage ) )
        //    {
        //        throw new HttpResponseException( Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage ) );
        //    }

        //    return transaction;
        //}

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
            var personAliasIds = new PersonAliasService( (RockContext)this.Service.Context ).Queryable().Where( a => a.Person.GivingId == givingId ).Select( a => a.Id ).ToList();

            // get the transactions for the person or all the members in the person's giving group (Family)
            return Get().Where( t => t.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) );
        }

        /// <summary>
        ///
        /// </summary>
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
    }
}