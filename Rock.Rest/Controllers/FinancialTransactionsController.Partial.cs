// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

using Rock;
using Rock.Data;
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
        /// <param name="financialTransaction">The financial transaction.</param>
        /// <param name="checkMicr">The check micr.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/PostScanned" )]
        public HttpResponseMessage PostScanned( [FromBody]FinancialTransactionScannedCheck financialTransactionScannedCheck )
        {
            FinancialTransaction financialTransaction = financialTransactionScannedCheck.FinancialTransaction;
            financialTransaction.CheckMicrEncrypted = Encryption.EncryptString( financialTransactionScannedCheck.ScannedCheckMicr );
            financialTransaction.CheckMicrHash = Encryption.GetSHA1Hash( financialTransactionScannedCheck.ScannedCheckMicr );
            return this.Post( financialTransaction );
        }

        /// <summary>
        /// Returns true if a transaction with the same routing number, accountnumber and checknumber is already in the database
        /// </summary>
        /// <param name="scannedCheckMicr">The scanned check micr in the format {RoutingNumber}_{AccountNumber}_{CheckNumber}</param>
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
        public DataSet GetContributionPersonGroupAddress( [FromBody]Rock.Net.RestParameters.ContributionStatementOptions options )
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
                result.Tables[0].TableName = "contribution_person_group_address";
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
        public DataSet GetContributionTransactions( int groupId, [FromBody]Rock.Net.RestParameters.ContributionStatementOptions options )
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
        public DataSet GetContributionTransactions( int groupId, int? personId, [FromBody]Rock.Net.RestParameters.ContributionStatementOptions options )
        {
            var qry = Get()
                .Where( a => a.TransactionDateTime >= options.StartDate )
                .Where( a => a.TransactionDateTime < ( options.EndDate ?? DateTime.MaxValue ) );

            if ( personId.HasValue )
            {
                // get transactions for a specific person
                qry = qry.Where( a => a.AuthorizedPersonAlias.PersonId == personId.Value );
            }
            else
            {
                // get transactions for all the persons in the specified group that have specified that group as their GivingGroup
                GroupMemberService groupMemberService = new GroupMemberService( (RockContext)Service.Context );
                var personIdList = groupMemberService.GetByGroupId( groupId ).Where( a => a.Person.GivingGroupId == groupId ).Select( s => s.PersonId ).ToList();

                qry = qry.Where( a => personIdList.Contains( a.AuthorizedPersonAlias.PersonId ) );
            }

            if ( options.AccountIds != null )
            {
                qry = qry.Where( a => options.AccountIds.Contains( a.TransactionDetails.FirstOrDefault().AccountId ) );
            }

            var selectQry = qry.Select( a => new
            {
                a.TransactionDateTime,
                CurrencyTypeValueName = a.FinancialPaymentDetail != null ? a.FinancialPaymentDetail.CurrencyTypeValue.Value : "",
                a.Summary,
                Details = a.TransactionDetails.Select( d => new
                {
                    d.AccountId,
                    AccountName = d.Account.Name,
                    a.Summary,
                    d.Amount
                } ).OrderBy( x => x.AccountName ),
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
                foreach ( var detail in fieldItems.Details )
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
                    fieldItems.Details.Sum(a => a.Amount),
                    detailTable
                };

                dataTable.Rows.Add( itemArray );
            }

            dataTable.EndLoadData();

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add( dataTable );

            return dataSet;
        }

    }
}
